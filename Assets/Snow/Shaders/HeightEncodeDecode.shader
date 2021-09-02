// SPDX-License-Identifier: MIT
// SPDX-FileCopyrightText: © 2021 Tyler Dodds

Shader "Hidden/Snow/HeightEncodeDecode"
{
    HLSLINCLUDE

    #pragma vertex Vert

    #pragma target 4.5
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
    #pragma enable_d3d11_debug_symbols

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"
	#include "EncodingCommon.hlsl"

	Buffer<int> HeightComputeBufferEncodedReadWrite;
    float4 _TextureSize;

    #pragma enable_d3d11_debug_symbols

	uint GetBufferIndex(uint2 id)
	{
		return id.x + _TextureSize.x * id.y;
	}

	float HeightDecode(Varyings varyings) : SV_Target
	{
		PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _TextureSize.zw);
		uint2 indices = posInput.positionSS;
		uint index = GetBufferIndex(indices);
		int encoded = HeightComputeBufferEncodedReadWrite[index];
		float decoded = DecodeHeight(encoded);
		return decoded;
	}

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "Height Decode"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment HeightDecode
            ENDHLSL
        }
    }
    Fallback Off
}
