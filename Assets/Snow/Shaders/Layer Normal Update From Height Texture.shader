// SPDX-License-Identifier: MIT
// SPDX-FileCopyrightText: © 2021 Tyler Dodds

Shader "Snow/Normal Update From Height Texture CRT"
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

		CGINCLUDE
		#include "UnityCustomRenderTexture.cginc"

		#include "NormalUpdate.hlsl"

		ENDCG

	   Pass
	   {
		   Name "NormalUpdate"

		   CGPROGRAM
		   #pragma vertex CustomRenderTextureVertexShader
		   #pragma fragment frag
		   #pragma target 3.0

			sampler2D   _InitialNormalTex;
			float _InitialNormalStrength;

			float2 frag(v2f_customrendertexture IN) : COLOR
		   {
			   return FragmentNormalUpdate(IN.localTexcoord, _InitialNormalTex, _InitialNormalStrength);
		   }

		ENDCG
		}

		Pass
	   {
		   Name "DetailNormalUpdate"

		   CGPROGRAM
		   #pragma vertex CustomRenderTextureVertexShader
		   #pragma fragment frag
		   #pragma target 3.0

			float4 frag(v2f_customrendertexture IN) : COLOR
			{
				return FragmentDetailNormalUpdate(IN.localTexcoord);
			}

			ENDCG
		}

		Pass
		{
			Name "DetailNormalUpdateFromBlurred"

			CGPROGRAM
			#pragma vertex CustomRenderTextureVertexShader
			#pragma fragment frag
			#pragma target 3.0

			float4 frag(v2f_customrendertexture IN) : COLOR
			{
				return FragmentDetailNormalUpdateFromBlurred(IN.localTexcoord);
			}

			ENDCG
		}
	}
}