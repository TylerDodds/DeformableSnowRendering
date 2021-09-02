// SPDX-License-Identifier: MIT
// SPDX-FileCopyrightText: © 2021 Tyler Dodds

Shader "Hidden/Snow/Depth Rectification"
{
	Properties
	{
		_DepthTex("Depth Texture", 2D) = "white" {}
	}

	HLSLINCLUDE

	#pragma vertex Vert

	#pragma target 4.5
	#pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
	#pragma enable_d3d11_debug_symbols

	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

	#pragma enable_d3d11_debug_symbols

	sampler2D   _DepthTex;
	float4 _TextureSize;

	float2 GetSampleUVs(Varyings varyings)
	{
		PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _TextureSize.zw);
		return posInput.positionNDC.xy;//Probably don't need to multiply by _RTHandleScale.xy, because texture size is independent of camera
	}

	float GetDepth01(float2 localTexcoord)
	{
		//Need to mirror horizontal coordinate based on camera upwards view.
		float2 sampleUv = float2(1 - localTexcoord.x, localTexcoord.y);
		float depthTexValue = tex2D(_DepthTex, sampleUv).r;
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

	ENDHLSL

	SubShader
	{
	   Pass
	   {
			Name "Height From Depth"

			ZWrite Off
			ZTest Always
			Blend Off
			Cull Off

			HLSLPROGRAM
			#pragma fragment frag

			float frag(Varyings varyings) : COLOR
			{
				float2 uv = GetSampleUVs(varyings);
				float depth01 = GetDepth01(uv);
				return depth01;
			}

			ENDHLSL
		}
	}
}
