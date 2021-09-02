// SPDX-License-Identifier: MIT
// SPDX-FileCopyrightText: © 2021 Tyler Dodds

Shader "Hidden/FullScreen/ScharrLaplacianFilter"
{
    HLSLINCLUDE

    #pragma vertex Vert

    #pragma target 4.5
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
    #pragma enable_d3d11_debug_symbols

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

    sampler2D _Source;
    float4 _TextureSize;
	float4 _ScaleXOffsetXScaleYOffsetY;

    #pragma enable_d3d11_debug_symbols

	//NB Note that we don't need to clamp UVs, since we're dealing with full-sized RenderTextures, so we won't bleed from larger-sized RenderTextures that the RenderHandle is bound to.
    float2 GetSampleUVs(Varyings varyings)
    {
        PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _TextureSize.zw);
		return posInput.positionNDC.xy;//Probably don't need to multiply by _RTHandleScale.xy, because texture size is independent of camera
    }

	float2 GetGradient2D(Varyings varyings)
	{
		float2 uv = GetSampleUVs(varyings);
		float2 offsetX = float2(_TextureSize.z, 0);
		float2 offsetY = float2(0, _TextureSize.w);
		const float factorA = 3. / 32.;
		const float factorB = 10. / 32.;

		float valueNN = tex2D(_Source, uv - offsetX - offsetY).r * factorA;
		float valueNP = tex2D(_Source, uv - offsetX + offsetY).r * factorA;

		float valuePN = tex2D(_Source, uv + offsetX - offsetY).r * factorA;
		float valuePP = tex2D(_Source, uv + offsetX + offsetY).r * factorA;

		float valueNS = tex2D(_Source, uv - offsetX).r * factorB;
		float valuePS = tex2D(_Source, uv + offsetX).r * factorB;

		float valueSN = tex2D(_Source, uv - offsetY).r * factorB;
		float valueSP = tex2D(_Source, uv + offsetY).r * factorB;

		float gradientX = valuePP + valuePS + valuePN - valueNP - valueNS - valueNN;
		float gradientY = valueNP + valueSP + valuePP - valueNN - valueSN - valuePN;

		return float2(gradientX, gradientY);
	}

	float2 GetBlurHorizontalVertical(Varyings varyings)
	{
		float2 uv = GetSampleUVs(varyings);
		float2 offsetX = float2(_TextureSize.z, 0);
		float2 offsetY = float2(0, _TextureSize.w);
		const float factorA = 10. / 16;
		const float factorB = 3. / 16;

		float value = tex2D(_Source, uv).r * factorA;

		float valueNS = tex2D(_Source, uv - offsetX).r * factorB;
		float valuePS = tex2D(_Source, uv + offsetX).r * factorB;

		float valueSN = tex2D(_Source, uv - offsetY).r * factorB;
		float valueSP = tex2D(_Source, uv + offsetY).r * factorB;

		float blurX = value + valueNS + valuePS;
		float blurY = value + valueSN + valueSP;

		return float2(blurX, blurY);
	}

	//See https://en.wikipedia.org/wiki/Discrete_Laplace_operator
	float GetLaplacian2D(Varyings varyings, float diagFactor, float axisFactor, float centerFactor)
	{
		float2 uv = GetSampleUVs(varyings);
		float2 offsetX = float2(_TextureSize.z, 0);
		float2 offsetY = float2(0, _TextureSize.w);

		float valueCenter = tex2D(_Source, uv).r * centerFactor;

		float valueNN = tex2D(_Source, uv - offsetX - offsetY).r * diagFactor;
		float valueNP = tex2D(_Source, uv - offsetX + offsetY).r * diagFactor;

		float valuePN = tex2D(_Source, uv + offsetX - offsetY).r * diagFactor;
		float valuePP = tex2D(_Source, uv + offsetX + offsetY).r * diagFactor;

		float valueNS = tex2D(_Source, uv - offsetX).r * axisFactor;
		float valuePS = tex2D(_Source, uv + offsetX).r * axisFactor;

		float valueSN = tex2D(_Source, uv - offsetY).r * axisFactor;
		float valueSP = tex2D(_Source, uv + offsetY).r * axisFactor;

		return valueCenter + valueNN + valueNP + valuePN + valuePP + valueNS + valuePS + valueSN + valueSP;
	}

	float GetLaplacian2D_FivePoint(Varyings varyings)
	{
		return GetLaplacian2D(varyings, 0, 1, -4);
	}

	float GetLaplacian2D_NinePoint(Varyings varyings)
	{
		return GetLaplacian2D(varyings, 0.25, 0.5, -3);
	}

	float GetLaplacian2D_EquidistantDiagonalAssumption(Varyings varyings)
	{
		return GetLaplacian2D(varyings, 1, 1, -8);
	}

	float Laplacian2D(Varyings varyings) : SV_Target
	{
		return GetLaplacian2D_NinePoint(varyings);
	}

    float2 Gradient2D(Varyings varyings) : SV_Target
    {
		return GetGradient2D(varyings);
    }

	float2 Gradient2DWithScaleAndOffset(Varyings varyings) : SV_Target
	{
		float2 grad = GetGradient2D(varyings);
		float2 gradShifted = float2(grad.x * _ScaleXOffsetXScaleYOffsetY.x + _ScaleXOffsetXScaleYOffsetY.y, grad.y * _ScaleXOffsetXScaleYOffsetY.z + _ScaleXOffsetXScaleYOffsetY.w);
		return gradShifted;
	}

	float2 Gradient2DWithScaleAndOffsetSaturated(Varyings varyings) : SV_Target
	{
		float2 gradShifted = Gradient2DWithScaleAndOffset(varyings);
		return saturate(gradShifted);
	}

	float2 Gradient2DWithScaleAndOffsetLengthSaturated(Varyings varyings) : SV_Target
	{
		float2 gradShifted = Gradient2DWithScaleAndOffset(varyings);
		return saturate(length(gradShifted));
	}

	float2 Gradient2DLengthWithScaleAndOffsetSaturated(Varyings varyings) : SV_Target
	{
		float2 grad = GetGradient2D(varyings);
		float gradLength = length(grad);
		float2 lengthShifted = gradLength * (_ScaleXOffsetXScaleYOffsetY.x + _ScaleXOffsetXScaleYOffsetY.z) * 0.5 + (_ScaleXOffsetXScaleYOffsetY.y + _ScaleXOffsetXScaleYOffsetY.w) * 0.5;
		return saturate(lengthShifted);
	}

	float2 ScharrBlurHorizontalVertical(Varyings varyings) : SV_Target
	{
		return GetBlurHorizontalVertical(varyings);
	}

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "Gradient 2D"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment Gradient2D
            ENDHLSL
        }

		Pass
		{
			Name "Gradient 2D With Scale And Offset"

			ZWrite Off
			ZTest Always
			Blend Off
			Cull Off

			HLSLPROGRAM
				#pragma fragment Gradient2DWithScaleAndOffset
			ENDHLSL
		}

		Pass
		{
			Name "Gradient 2D With Scale And Offset, Saturated"

			ZWrite Off
			ZTest Always
			Blend Off
			Cull Off

			HLSLPROGRAM
				#pragma fragment Gradient2DWithScaleAndOffsetSaturated
			ENDHLSL
		}

		Pass
		{
			Name "Length of Gradient 2D With Scale And Offset, Saturated"

			ZWrite Off
			ZTest Always
			Blend Off
			Cull Off

			HLSLPROGRAM
				#pragma fragment Gradient2DWithScaleAndOffsetLengthSaturated
			ENDHLSL
		}

		Pass
		{
			Name "Length of Gradient 2D, With Scale And Offset, Saturated"

			ZWrite Off
			ZTest Always
			Blend Off
			Cull Off

			HLSLPROGRAM
				#pragma fragment Gradient2DLengthWithScaleAndOffsetSaturated
			ENDHLSL
		}

		Pass
		{
			Name "Scharr Blur Horizontal & Vertical"

			ZWrite Off
			ZTest Always
			Blend Off
			Cull Off

			HLSLPROGRAM
				#pragma fragment ScharrBlurHorizontalVertical
			ENDHLSL
		}

		Pass
		{
			Name "Laplacian"

			ZWrite Off
			ZTest Always
			Blend Off
			Cull Off

			HLSLPROGRAM
				#pragma fragment Laplacian2D
			ENDHLSL
		}

    }
    Fallback Off
}
