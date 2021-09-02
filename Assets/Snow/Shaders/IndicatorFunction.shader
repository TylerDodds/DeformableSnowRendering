// SPDX-License-Identifier: MIT
// SPDX-FileCopyrightText: © 2021 Tyler Dodds

Shader "Hidden/FullScreen/IndicatorFunction"
{
    HLSLINCLUDE

    #pragma vertex Vert

    #pragma target 4.5
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
    #pragma enable_d3d11_debug_symbols

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

    sampler2D _Source;
    float4 _TextureSize;

    #pragma enable_d3d11_debug_symbols

	//NB Note that we don't need to clamp UVs, since we're dealing with full-sized RenderTextures, so we won't bleed from larger-sized RenderTextures that the RenderHandle is bound to.
    float2 GetSampleUVs(Varyings varyings)
    {
        PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _TextureSize.zw);
		return posInput.positionNDC.xy;//Probably don't need to multiply by _RTHandleScale.xy, because texture size is independent of camera
    }

	float GetDepth01(float2 localTexcoord)
	{
		float2 sampleUv = float2(1 - localTexcoord.x, localTexcoord.y);
		float depthTexValue = tex2D(_Source, sampleUv).r;
		float depth01 = depthTexValue;
		#if defined(UNITY_REVERSED_Z)
		#if UNITY_REVERSED_Z == 1
				depth01 = 1.0f - depthTexValue;
		#endif
		#endif
		return depth01;
	}

	float IndicatorFunction(Varyings varyings) : SV_Target
    {
		float2 uv = GetSampleUVs(varyings);
		float4 curr = tex2D(_Source, uv).rgba;
		float currLenSq = dot(curr, curr);
		return step(1e-6, currLenSq);
    }

	float IndicatorFunctionDepth(Varyings varyings) : SV_Target
	{
		float2 uv = GetSampleUVs(varyings);
		float depth = GetDepth01(uv);
		return step(depth, 1 - 1e-6);
	}

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "Indicator Function"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment IndicatorFunction
            ENDHLSL
        }

		Pass
		{
			Name "Indicator Function of Depth"

			ZWrite Off
			ZTest Always
			Blend Off
			Cull Off

			HLSLPROGRAM
				#pragma fragment IndicatorFunctionDepth
			ENDHLSL
		}
    }
    Fallback Off
}
