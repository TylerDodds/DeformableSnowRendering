// SPDX-License-Identifier: MIT
// SPDX-FileCopyrightText: © 2021 Tyler Dodds

Shader "Hidden/FullScreen/TintedLookup"
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
	float _Offset;
	float4 _Tint;
	float4 _TintOffset;
	sampler2D _RemapSource;

    #pragma enable_d3d11_debug_symbols

	//NB Note that we don't need to clamp UVs, since we're dealing with full-sized RenderTextures, so we won't bleed from larger-sized RenderTextures that the RenderHandle is bound to.
    float2 GetSampleUVs(Varyings varyings)
    {
        PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _TextureSize.zw);
		return posInput.positionNDC.xy;//Probably don't need to multiply by _RTHandleScale.xy, because texture size is independent of camera
    }

	float TintedLookup1(Varyings varyings) : SV_Target
	{
		float2 uv = GetSampleUVs(varyings);
		float curr = tex2D(_Source, uv).r;
		return _Offset + curr * _Multiplier;
	}

    float4 TintedLookup4(Varyings varyings) : SV_Target
    {
		float2 uv = GetSampleUVs(varyings);
		float4 curr = tex2D(_Source, uv);
		return _TintOffset + curr * _Tint;
    }

	float RemapBottom1To01(Varyings varyings) : SV_Target
	{
		float2 uv = GetSampleUVs(varyings);
		float curr = tex2D(_Source, uv).r;
		float currRemap = tex2D(_RemapSource, uv).r;
		float multiplier = currRemap >= 1 ? 1 : 1 / (1 - currRemap);
		float offset = 1 - multiplier;
		return curr * multiplier + offset;
	}

	float PowerandOffset(Varyings varyings) : SV_Target
	{
		float2 uv = GetSampleUVs(varyings);
		float curr = tex2D(_Source, uv).r;
		return pow(curr, _Multiplier) + _Offset;
	}

	float SignMapped0To1(Varyings varyings) : SV_Target
	{
		float2 uv = GetSampleUVs(varyings);
		float curr = tex2D(_Source, uv).r;
		return curr > 0 ? 1 : (curr < 0 ? 0 : 0.5);
	}

    ENDHLSL

    SubShader
    {
		Pass
		{
			Name "Tinted Lookup (float)"

			ZWrite Off
			ZTest Always
			Blend Off
			Cull Off

			HLSLPROGRAM
				#pragma fragment TintedLookup1
			ENDHLSL
		}

        Pass
        {
            Name "Tinted Lookup (float4)"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment TintedLookup4
            ENDHLSL
        }

		Pass
		{
			Name "Remap [Bottom, 1] to [0, 1] (float)"

			ZWrite Off
			ZTest Always
			Blend Off
			Cull Off

			HLSLPROGRAM
				#pragma fragment RemapBottom1To01
			ENDHLSL
		}

		Pass
		{
			Name "Power and Offset (float)"

			ZWrite Off
			ZTest Always
			Blend Off
			Cull Off

			HLSLPROGRAM
				#pragma fragment PowerandOffset
			ENDHLSL
		}

		Pass
		{
			Name "Sign Mapped 0 To 1"

			ZWrite Off
			ZTest Always
			Blend Off
			Cull Off

			HLSLPROGRAM
				#pragma fragment SignMapped0To1
			ENDHLSL
		}

		Pass
		{
			Name "Tinted Lookup Max (float)"

			ZWrite Off
			ZTest Always
			Blend One One

			BlendOp Max
			Cull Off

			HLSLPROGRAM
				#pragma fragment TintedLookup1
			ENDHLSL
		}
    }
    Fallback Off
}
