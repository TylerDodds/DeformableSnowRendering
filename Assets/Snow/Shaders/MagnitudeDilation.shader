// SPDX-License-Identifier: MIT
// SPDX-FileCopyrightText: © 2021 Tyler Dodds

Shader "Hidden/FullScreen/MagnitudeDilation"
{
    HLSLINCLUDE

    #pragma vertex Vert

    #pragma target 4.5
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
    #pragma enable_d3d11_debug_symbols

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

    sampler2D _Source;
    float4 _TextureSize;
	float _Epsilon;

    #pragma enable_d3d11_debug_symbols

	//NB Note that we don't need to clamp UVs, since we're dealing with full-sized RenderTextures, so we won't bleed from larger-sized RenderTextures that the RenderHandle is bound to.
    float2 GetSampleUVs(Varyings varyings)
    {
        PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _TextureSize.zw);
		return posInput.positionNDC.xy;//Probably don't need to multiply by _RTHandleScale.xy, because texture size is independent of camera
    }

	float3 GetMaximumWithNeighbours(Varyings varyings, float2 offsetFraction)
	{
		float2 offset = _TextureSize.zw * offsetFraction;
		float2 uv = GetSampleUVs(varyings);

		float3 prev = tex2D(_Source, uv - offset).rgb;
		float3 curr = tex2D(_Source, uv).rgb;
		float3 next = tex2D(_Source, uv + offset).rgb;

		float prevLenSq = dot(prev, prev);
		float currLenSq = dot(curr, curr);
		float nextLenSq = dot(next, next);

		bool nextLargerCurr = nextLenSq > currLenSq;
		bool nextLargerPrev = nextLenSq > prevLenSq;
		bool currLargerPrev = currLenSq > prevLenSq;
		return nextLargerCurr ? (nextLargerPrev ? next : prev) : (currLargerPrev ? curr : prev);
	}

	float3 GetMaximumWithNeighboursIfBelowEpsilon(Varyings varyings, float2 offsetFraction)
	{
		float2 offset = _TextureSize.zw * offsetFraction;
		float2 uv = GetSampleUVs(varyings);

		float3 prev = tex2D(_Source, uv - offset).rgb;
		float3 curr = tex2D(_Source, uv).rgb;
		float3 next = tex2D(_Source, uv + offset).rgb;

		float prevLenSq = dot(prev, prev);
		float currLenSq = dot(curr, curr);
		float nextLenSq = dot(next, next);

		bool nextLargerPrev = nextLenSq > prevLenSq;
		bool currLargerEpsilon = currLenSq > _Epsilon;
		return currLargerEpsilon ? curr : (nextLargerPrev ? next : prev);
	}

	float3 GetMinimumWithNeighbours(Varyings varyings, float2 offsetFraction)
	{
		float2 offset = _TextureSize.zw * offsetFraction;
		float2 uv = GetSampleUVs(varyings);

		float3 prev = tex2D(_Source, uv - offset).rgb;
		float3 curr = tex2D(_Source, uv).rgb;
		float3 next = tex2D(_Source, uv + offset).rgb;

		float prevLenSq = dot(prev, prev);
		float currLenSq = dot(curr, curr);
		float nextLenSq = dot(next, next);

		bool nextSmallerCurr = nextLenSq < currLenSq;
		bool nextSmallerPrev = nextLenSq < prevLenSq;
		bool currSmallerPrev = currLenSq < prevLenSq;
		return nextSmallerCurr ? (nextSmallerPrev ? next : prev) : (currSmallerPrev ? curr : prev);
	}

    float4 HorizontalDilation(Varyings varyings) : SV_Target
    {
		return float4(GetMaximumWithNeighbours(varyings, float2(1,0)), 0);
    }

	float4 VerticalDilation(Varyings varyings) : SV_Target
    {
		return float4(GetMaximumWithNeighbours(varyings, float2(0,1)), 0);
    }

	float4 HorizontalDilationIfBelowEpsilon(Varyings varyings) : SV_Target
	{
		return float4(GetMaximumWithNeighboursIfBelowEpsilon(varyings, float2(1,0)), 0);
	}

	float4 VerticalDilationIfBelowEpsilon(Varyings varyings) : SV_Target
	{
		return float4(GetMaximumWithNeighboursIfBelowEpsilon(varyings, float2(0,1)), 0);
	}

	float4 HorizontalErosion(Varyings varyings) : SV_Target
	{
		return float4(GetMinimumWithNeighbours(varyings, float2(1,0)), 0);
	}

		float4 VerticalErosion(Varyings varyings) : SV_Target
	{
		return float4(GetMinimumWithNeighbours(varyings, float2(0,1)), 0);
	}

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "Horizontal Dilation"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment HorizontalDilation
            ENDHLSL
        }

        Pass
        {
            Name "Vertical Dilation"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment VerticalDilation
            ENDHLSL
        }

		Pass
		{
			Name "Horizontal Dilation If Below Epsilon"

			ZWrite Off
			ZTest Always
			Blend Off
			Cull Off

			HLSLPROGRAM
				#pragma fragment HorizontalDilationIfBelowEpsilon
			ENDHLSL
		}

		Pass
		{
			Name "Vertical Dilation If Below Epsilon"

			ZWrite Off
			ZTest Always
			Blend Off
			Cull Off

			HLSLPROGRAM
				#pragma fragment VerticalDilationIfBelowEpsilon
			ENDHLSL
		}

		Pass
		{
			Name "Horizontal Erosion"

			ZWrite Off
			ZTest Always
			Blend Off
			Cull Off

			HLSLPROGRAM
				#pragma fragment HorizontalErosion
			ENDHLSL
		}

			Pass
		{
			Name "Vertical Erosion"

			ZWrite Off
			ZTest Always
			Blend Off
			Cull Off

			HLSLPROGRAM
				#pragma fragment VerticalErosion
			ENDHLSL
		}
    }
    Fallback Off
}
