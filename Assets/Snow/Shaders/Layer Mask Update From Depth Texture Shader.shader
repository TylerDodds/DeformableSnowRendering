// SPDX-License-Identifier: MIT
// SPDX-FileCopyrightText: © 2021 Tyler Dodds

Shader "Snow/Layer Mask Update From Depth Texture CRT"
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

		CGINCLUDE

		#include "UnityCustomRenderTexture.cginc"
		#include "MaskUpdate.hlsl"
		
		ENDCG

	   Pass
	   {
		   Name "MaskUpdateGreen"
			Blend One One
			BlendOp Min
		   CGPROGRAM
		   #pragma vertex CustomRenderTextureVertexShader
		   #pragma fragment frag
		   #pragma target 3.0

		   float4 frag(v2f_customrendertexture IN) : COLOR
		   {
				return FragmentMaskUpdateGreen(IN.localTexcoord);
		   }
		   ENDCG
		}

		Pass
	   {
		   Name "HeightUpdate"
			Blend One One
			BlendOp Min
		   CGPROGRAM
		   #pragma vertex CustomRenderTextureVertexShader
		   #pragma fragment frag
		   #pragma target 3.0

		   float frag(v2f_customrendertexture IN) : COLOR
		   {
			    return GetDepth01(IN.localTexcoord);
		   }
		  ENDCG
	   }

		Pass
		  {
			  Name "MaskInterpolateUpdateGreen"
				Blend One One
				BlendOp Min
			  CGPROGRAM
			  #pragma vertex CustomRenderTextureVertexShader
			  #pragma fragment frag
			  #pragma target 3.0
			#pragma shader_feature_local _MASK_SMOOTHSTEP_ON


			  sampler2D   _TopLayerHeightTex;
			  sampler2D   _BottomLayerHeightTex;
			  float4 _LayerFractionalRanges;


			  float4 frag(v2f_customrendertexture IN) : COLOR
			  {
					return FragmentMaskInterpolateUpdateGreen(IN.localTexcoord, _TopLayerHeightTex, _BottomLayerHeightTex, _LayerFractionalRanges);
			  }
			  ENDCG
		  }

			Pass
			{
				Name "MaskInterpolateUpdate_BetweenLayersOneAndTwo"
				Blend One One
				BlendOp Min
				CGPROGRAM
				#pragma vertex CustomRenderTextureVertexShader
				#pragma fragment frag
				#pragma target 3.0

				sampler2D   _TopLayerHeightTex;
				sampler2D   _BottomLayerHeightTex;
				float4 _LayerFractionalRanges;
				float _SimilarLayerTransitionDistance;
				float _SimilarLayerTransitionPower;
				#pragma shader_feature_local _MASK_SMOOTHSTEP_ON


				float4 frag(v2f_customrendertexture IN) : COLOR
				{
					return FragmentMaskInterpolateUpdate_BetweenLayersOneAndTwo(IN.localTexcoord, _TopLayerHeightTex, _BottomLayerHeightTex, _LayerFractionalRanges, _SimilarLayerTransitionDistance, _SimilarLayerTransitionPower);
				}
				ENDCG
			}

			Pass
			{
				Name "MaskInterpolateFromHeight_Green"
				CGPROGRAM
				#pragma vertex CustomRenderTextureVertexShader
				#pragma fragment frag
				#pragma target 3.0
				#pragma shader_feature_local _MASK_SMOOTHSTEP_ON


				sampler2D   _TopLayerHeightTex;
				sampler2D   _BottomLayerHeightTex;
				float4 _LayerFractionalRanges;
				sampler2D   _MaskHeightTex;


				float4 frag(v2f_customrendertexture IN) : COLOR
				{
					return FragmentMaskInterpolateFromHeightGreen(IN.localTexcoord, _MaskHeightTex, _TopLayerHeightTex, _BottomLayerHeightTex, _LayerFractionalRanges);
				}
				ENDCG
			}

			Pass
			{
				Name "MaskInterpolateFromHeight_BetweenLayersOneAndTwo"
				CGPROGRAM
				#pragma vertex CustomRenderTextureVertexShader
				#pragma fragment frag
				#pragma target 3.0

				sampler2D   _TopLayerHeightTex;
				sampler2D   _BottomLayerHeightTex;
				float4 _LayerFractionalRanges;
				float _SimilarLayerTransitionDistance;
				float _SimilarLayerTransitionPower;
				sampler2D   _MaskHeightTex;
			#pragma shader_feature_local _MASK_SMOOTHSTEP_ON


				float4 frag(v2f_customrendertexture IN) : COLOR
				{
					return FragmentMaskInterpolateFromHeight_BetweenLayersOneAndTwo(IN.localTexcoord, _MaskHeightTex, _TopLayerHeightTex, _BottomLayerHeightTex, _LayerFractionalRanges, _SimilarLayerTransitionDistance, _SimilarLayerTransitionPower);
				}
				ENDCG
			}
	}
}
