Shader "Hidden/FullScreen/Gradient2D"
{
    HLSLINCLUDE

    #pragma vertex Vert

    #pragma target 4.5
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
    #pragma enable_d3d11_debug_symbols

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

    sampler2D _Source;
    float4 _TextureSize;
	float _Multiplier;

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
		float prevX = tex2D(_Source, uv - offsetX).r;
		float nextX = tex2D(_Source, uv + offsetX).r;

		float2 offsetY = float2(0, _TextureSize.w);
		float prevY = tex2D(_Source, uv - offsetY).r;
		float nextY = tex2D(_Source, uv + offsetY).r;

		float gradX = (nextX - prevX) / (2);
		float gradY = (nextY - prevY) / (2);

		return float2(gradX, gradY);
	}

	float GetGradientDirection(Varyings varyings, float2 offset)
	{
		float2 uv = GetSampleUVs(varyings);

		float prev = tex2D(_Source, uv - offset).r;
		float next = tex2D(_Source, uv + offset).r;

		float grad = (next - prev) / (2);
		return grad;
	}

    float2 Gradient2D(Varyings varyings) : SV_Target
    {
		return GetGradient2D(varyings);
    }

	float2 Gradient2DWithMultiplier(Varyings varyings) : SV_Target
	{
		return GetGradient2D(varyings) * _Multiplier;
	}

	float GradientHorizontal(Varyings varyings) : SV_Target
	{
		return GetGradientDirection(varyings, float2(_TextureSize.z, 0));
	}

	float GradientVertical(Varyings varyings) : SV_Target
	{
		return GetGradientDirection(varyings, float2(0, _TextureSize.w));
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
			Name "Gradient 2D With Multiplier"

			ZWrite Off
			ZTest Always
			Blend Off
			Cull Off

			HLSLPROGRAM
				#pragma fragment Gradient2DWithMultiplier
			ENDHLSL
		}

		Pass
		{
			Name "Gradient Horizontal"

			ZWrite Off
			ZTest Always
			Blend Off
			Cull Off

			HLSLPROGRAM
				#pragma fragment GradientHorizontal
			ENDHLSL
		}

		Pass
		{
			Name "Gradient Vertical"

			ZWrite Off
			ZTest Always
			Blend Off
			Cull Off

			HLSLPROGRAM
				#pragma fragment GradientVertical
			ENDHLSL
		}
    }
    Fallback Off
}
