// SPDX-License-Identifier: MIT
// SPDX-FileCopyrightText: © 2021 Tyler Dodds

Shader "Snow/Normal Update From Height Texture Fullscreen"
{
	Properties
	{
		_HeightTex("Mask Height Texture", 2D) = "white" {}
		_InitialHeightTex("Initial Height Texture", 2D) = "white" {}
		_InitialNormalTex("Initial Normal Texture", 2D) = "white" {}
		_InitialNormalStrength("Initial Normal Strength", Range(0, 5)) = 1
		_Scale("Texture Relative Scale XYZ", Vector) = (1, 1, 1, 0)
		_HeightFractionSmoothing ("Height Fraction Smoothing Range", Range(0, 0.5)) = 0.1
	}

		SubShader
	{
	   Lighting Off
	   Blend One Zero

		HLSLINCLUDE
		#pragma vertex Vert

		#pragma target 4.5
		#pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
		#pragma enable_d3d11_debug_symbols

		#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

		#include "NormalUpdate.hlsl"

		float4 _TextureSize; // We need the texture size because we have a non fullscreen render target (blur buffers are downsampled in half res)

		float2 GetSampleUVs(Varyings varyings)
		{
			PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _TextureSize.zw);
			return posInput.positionNDC.xy;//Probably don't need to multiply by _RTHandleScale.xy, because texture size is independent of camera
		}

		ENDHLSL

	   Pass
	   {
		   Name "NormalUpdate"

		   HLSLPROGRAM
		   #pragma fragment frag

			sampler2D   _InitialNormalTex;
			float _InitialNormalStrength;

			float2 frag(Varyings varyings) : SV_Target
		   {
			   return FragmentNormalUpdate(GetSampleUVs(varyings), _InitialNormalTex, _InitialNormalStrength);
		   }

			ENDHLSL
		}

		Pass
	   {
		   Name "DetailNormalUpdate"

		   HLSLPROGRAM
		   #pragma fragment frag

			float4 frag(Varyings varyings) : SV_Target
			{
				return FragmentDetailNormalUpdate(GetSampleUVs(varyings));
			}

			ENDHLSL
		}

		Pass
		{
			Name "DetailNormalUpdateFromBlurred"

			HLSLPROGRAM
			#pragma fragment frag

			float4 frag(Varyings varyings) : SV_Target
			{
				return FragmentDetailNormalUpdateFromBlurred(GetSampleUVs(varyings));
			}

			ENDHLSL
		}
	}
}