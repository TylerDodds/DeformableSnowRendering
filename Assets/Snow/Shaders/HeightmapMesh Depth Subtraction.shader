// SPDX-License-Identifier: MIT
// SPDX-FileCopyrightText: © 2021 Tyler Dodds

Shader "Snow/Heightmap Mesh Depth Subtraction"
{
	Properties
	{
		_DepthTex("Depth Texture", 2D) = "white" {}
		_MeshHeightmap("Mesh Heightmap", 2D) = "white" {}
		_Ranges("Mesh Heightmap Range (x), Capture Range (y)", Vector) = (1, 1.1, 0, 0)
	}

		SubShader
		{
		   Lighting Off
		   Blend One Zero

			CGINCLUDE

			sampler2D _DepthTex;

			float GetDepth01(float2 depthUv)
			{
				float depthTexValue = SAMPLE_DEPTH_TEXTURE(_DepthTex, depthUv);
				//NB linear01depth will only work for perspective cameras. See https://forum.unity.com/threads/getting-scene-depth-z-buffer-of-the-orthographic-camera.601825/#post-4966334 and https://fatdogsp.github.io/2020/02/25/Orthographic-SSR-Water/ for some related discussion.
				//Since orthographic distance is already encoded linearly, we only need to worry about UNITY_REVERSED_Z
				float depth01 = depthTexValue;
				#if defined(UNITY_REVERSED_Z)
					#if UNITY_REVERSED_Z == 1
						depth01 = 1.0f - depthTexValue;
					#endif
				#endif
				return depth01;
			}

			ENDCG

		   Pass
		   {
			   Name "Depth Subtraction"
			   CGPROGRAM
			   #include "UnityCustomRenderTexture.cginc"
			   #pragma vertex CustomRenderTextureVertexShader
			   #pragma fragment frag
			   #pragma target 3.0

			   sampler2D _MeshHeightmap;
				float4 _Ranges;

			   float frag(v2f_customrendertexture IN) : COLOR
			   {
					float2 uv = IN.localTexcoord;
					float depth01 = GetDepth01(uv);
					//Need to mirror horizontal coordinate based on camera upwards view. We'll do this for heightmap lookup, so the result will be flipped like the incoming depth texture.
					float heightmapSample = tex2Dlod(_MeshHeightmap, float4(1 - uv.x, uv.y, 0, 0));
					float depthRemapped = _Ranges.y * depth01;
					float heightmapRemapped = _Ranges.x * heightmapSample;
					float subtractedRemapped = depthRemapped - heightmapRemapped;
					float subtracted01 = saturate(subtractedRemapped / (_Ranges.y - _Ranges.x));
					#if defined(UNITY_REVERSED_Z)
					#if UNITY_REVERSED_Z == 1
					subtracted01 = 1.0f - subtracted01;
					#endif
					#endif
					return subtracted01;
				}
			   ENDCG
			}
	}
}
