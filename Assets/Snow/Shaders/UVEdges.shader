// SPDX-License-Identifier: MIT
// SPDX-FileCopyrightText: © 2021 Tyler Dodds

Shader "Hidden/FullScreen/UVEdges"
{
    HLSLINCLUDE

    #pragma vertex Vert

    #pragma target 4.5
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
    #pragma enable_d3d11_debug_symbols

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

    float4 _TextureSize;
	float _Multiplier;

    #pragma enable_d3d11_debug_symbols

	//NB Note that we don't need to clamp UVs, since we're dealing with full-sized RenderTextures, so we won't bleed from larger-sized RenderTextures that the RenderHandle is bound to.
    float2 GetSampleUVs(Varyings varyings)
    {
        PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _TextureSize.zw);
		return posInput.positionNDC.xy;//Probably don't need to multiply by _RTHandleScale.xy, because texture size is independent of camera
    }

	float2 EdgeDistancesPx(Varyings varyings)
	{
		float2 uv = GetSampleUVs(varyings);
		float2 px = uv * _TextureSize.xy;
		float2 edgeDistancePx = min(px, _TextureSize.xy - px);
		return edgeDistancePx;
	}

    float EdgeDistancePx(Varyings varings) : SV_Target
	{
		float2 edgeDistances = EdgeDistancesPx(varings);
		return min(edgeDistances.x, edgeDistances.y) * _Multiplier;
	}

    ENDHLSL

    SubShader
    {
		Pass
		{
			Name "Edge Distance"

			ZWrite Off
			ZTest Always
			Blend Off
			Cull Off

			HLSLPROGRAM
				#pragma fragment EdgeDistancePx
			ENDHLSL
		}

		Pass
		{
			Name "Edge Distance (Min Blend)"

			ZWrite Off
			ZTest Always
			Blend One One
			BlendOp Min
			Cull Off

			HLSLPROGRAM
				#pragma fragment EdgeDistancePx
			ENDHLSL
		}
    }
    Fallback Off
}
