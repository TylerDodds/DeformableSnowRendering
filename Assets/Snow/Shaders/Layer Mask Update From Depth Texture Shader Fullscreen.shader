// SPDX-License-Identifier: MIT
// SPDX-FileCopyrightText: © 2021 Tyler Dodds

Shader "Snow/Layer Mask Update From Depth Texture Fullscreen"
{
	Properties
	{
		_DepthTex("Depth Texture", 2D) = "white" {}
		_TopLayerHeightTex("Top Layer Height Map", 2D) = "white" {}
		_BottomLayerHeightTex("Bottom Layer Height Map", 2D) = "white" {}
		_LayerFractionalRanges("Bottom Frac. HeightRange Min (x) Max (y) and Upper", Vector) = (0, 1, 0, 1)
		_SimilarLayerTransitionDistance("Similar Layer Transition Distance", Range(0, 1)) = 0.25
		_SimilarLayerTransitionPower("Similar Layer Transition Power", Range(0, 5)) = 2
		[Toggle] _MASK_SMOOTHSTEP("Use smoothstep transition in calculating fractional mask height", Float) = 0
	}

	SubShader
	{
	   Lighting Off

		HLSLINCLUDE

		#pragma vertex Vert

		#pragma target 4.5
		#pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
		#pragma enable_d3d11_debug_symbols

		#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"
		#include "MaskUpdate.hlsl"

		float4 _TextureSize; // We need the texture size because we have a non fullscreen render target (blur buffers are downsampled in half res)

		float2 GetSampleUVs(Varyings varyings)
		{
			PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _TextureSize.zw);
			return posInput.positionNDC.xy;//Probably don't need to multiply by _RTHandleScale.xy, because texture size is independent of camera
		}


		ENDHLSL

	   Pass
	   {
		   Name "MaskUpdateGreen"
			Blend One One
			BlendOp Min

			HLSLPROGRAM

			#pragma fragment frag

		   float4 frag(Varyings varyings) : SV_Target
		   {
				return FragmentMaskUpdateGreen(GetSampleUVs(varyings));
		   }
			ENDHLSL
		}

		Pass
	   {
		   Name "HeightUpdate"
			Blend One One
			BlendOp Min

		   HLSLPROGRAM
		   #pragma fragment frag

		   float frag(Varyings varyings) : SV_Target
		   {
			    return GetDepth01(GetSampleUVs(varyings));
		   }
		  ENDHLSL
	   }

		Pass
		  {
			  Name "MaskInterpolateUpdateGreen"
				Blend One One
				BlendOp Min
			  HLSLPROGRAM
			  #pragma fragment frag
			#pragma shader_feature_local _MASK_SMOOTHSTEP_ON


			  sampler2D   _TopLayerHeightTex;
			  sampler2D   _BottomLayerHeightTex;
			  float4 _LayerFractionalRanges;


			  float4 frag(Varyings varyings) : SV_Target
			  {
					return FragmentMaskInterpolateUpdateGreen(GetSampleUVs(varyings), _TopLayerHeightTex, _BottomLayerHeightTex, _LayerFractionalRanges);
			  }
			  ENDHLSL
		  }

			Pass
			{
				Name "MaskInterpolateUpdate_BetweenLayersOneAndTwo"
				Blend One One
				BlendOp Min
				HLSLPROGRAM
				#pragma fragment frag

				sampler2D   _TopLayerHeightTex;
				sampler2D   _BottomLayerHeightTex;
				float4 _LayerFractionalRanges;
				float _SimilarLayerTransitionDistance;
				float _SimilarLayerTransitionPower;
				#pragma shader_feature_local _MASK_SMOOTHSTEP_ON


				float4 frag(Varyings varyings) : SV_Target
				{
					return FragmentMaskInterpolateUpdate_BetweenLayersOneAndTwo(GetSampleUVs(varyings), _TopLayerHeightTex, _BottomLayerHeightTex, _LayerFractionalRanges, _SimilarLayerTransitionDistance, _SimilarLayerTransitionPower);
				}
				ENDHLSL
			}

			Pass
			{
				Name "MaskInterpolateFromHeight_Green"
				HLSLPROGRAM
				#pragma fragment frag
				#pragma shader_feature_local _MASK_SMOOTHSTEP_ON


				sampler2D   _TopLayerHeightTex;
				sampler2D   _BottomLayerHeightTex;
				float4 _LayerFractionalRanges;
				sampler2D   _MaskHeightTex;


				float4 frag(Varyings varyings) : SV_Target
				{
					return FragmentMaskInterpolateFromHeightGreen(GetSampleUVs(varyings), _MaskHeightTex, _TopLayerHeightTex, _BottomLayerHeightTex, _LayerFractionalRanges);
				}
				ENDHLSL
			}

			Pass
			{
				Name "MaskInterpolateFromHeight_BetweenLayersOneAndTwo"
				HLSLPROGRAM
				#pragma fragment frag

				sampler2D   _TopLayerHeightTex;
				sampler2D   _BottomLayerHeightTex;
				float4 _LayerFractionalRanges;
				float _SimilarLayerTransitionDistance;
				float _SimilarLayerTransitionPower;
				sampler2D   _MaskHeightTex;
			#pragma shader_feature_local _MASK_SMOOTHSTEP_ON


				float4 frag(Varyings varyings) : SV_Target
				{
					return FragmentMaskInterpolateFromHeight_BetweenLayersOneAndTwo(GetSampleUVs(varyings), _MaskHeightTex, _TopLayerHeightTex, _BottomLayerHeightTex, _LayerFractionalRanges, _SimilarLayerTransitionDistance, _SimilarLayerTransitionPower);
				}
				ENDHLSL
			}
	}
}
