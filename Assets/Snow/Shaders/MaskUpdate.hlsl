// SPDX-License-Identifier: MIT
// SPDX-FileCopyrightText: © 2021 Tyler Dodds

#ifndef SNOW_MASK_UPDATE_INCLUDED
#define SNOW_MASK_UPDATE_INCLUDED

sampler2D   _DepthTex;

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

float GetFractionalMaskBetweenLayerHeights_FromDepth(float depth01, float2 uv, sampler2D topLayerHeightTex, sampler2D bottomLayerHeightTex, float4 layerFractionalRanges)
{
	float topHeight = tex2D(topLayerHeightTex, uv).r  * (layerFractionalRanges.w - layerFractionalRanges.z) + layerFractionalRanges.z;
	float bottomHeight = tex2D(bottomLayerHeightTex, uv).r * (layerFractionalRanges.y - layerFractionalRanges.x) + layerFractionalRanges.x;
#ifdef _MASK_SMOOTHSTEP_ON
	float fractionBetweenBottomAndTop = smoothstep(bottomHeight, topHeight, depth01);
#else				
	float fractionBetweenBottomAndTop = saturate((depth01 - bottomHeight) / (topHeight - bottomHeight));
#endif
	return fractionBetweenBottomAndTop;
}

float GetFractionalMaskBetweenLayerHeights(float2 uv, sampler2D topLayerHeightTex, sampler2D bottomLayerHeightTex, float4 layerFractionalRanges)
{
	float depth01 = GetDepth01(uv);
	return GetFractionalMaskBetweenLayerHeights_FromDepth(depth01, uv, topLayerHeightTex, bottomLayerHeightTex, layerFractionalRanges);
}

float2 GetTopTwoLayerFractions_FromDepth(float depth01, float2 uv, sampler2D topLayerHeightTex, sampler2D bottomLayerHeightTex, float4 layerFractionalRanges, float similarLayerTransitionDistance, float similarLayerTransitionPower)
{
	float fractionBetweenBottomAndTop = GetFractionalMaskBetweenLayerHeights_FromDepth(depth01, uv, topLayerHeightTex, bottomLayerHeightTex, layerFractionalRanges);
	float fractionOfLayer2 = 1 - pow(saturate(1 - fractionBetweenBottomAndTop / similarLayerTransitionDistance), similarLayerTransitionPower);
	return float2(fractionOfLayer2, fractionBetweenBottomAndTop);
}

float2 GetTopTwoLayerFractions(float2 uv, sampler2D topLayerHeightTex, sampler2D bottomLayerHeightTex, float4 layerFractionalRanges, float similarLayerTransitionDistance, float similarLayerTransitionPower)
{
	float depth01 = GetDepth01(uv);
	return GetTopTwoLayerFractions_FromDepth(depth01, uv, topLayerHeightTex, bottomLayerHeightTex, layerFractionalRanges, similarLayerTransitionDistance, similarLayerTransitionPower);
}

float4 FragmentMaskUpdateGreen(float2 uv)
{
	float depth01 = GetDepth01(uv);
	return float4(1, depth01, 0, 1);
}

float4 FragmentMaskInterpolateUpdateGreen(float2 uv, sampler2D topLayerHeightTex, sampler2D bottomLayerHeightTex, float4 layerFractionalRanges)
{
	float fractionBetweenBottomAndTop = GetFractionalMaskBetweenLayerHeights(uv, topLayerHeightTex, bottomLayerHeightTex, layerFractionalRanges);
	return float4(1, fractionBetweenBottomAndTop, 0, 1);
}

float4 FragmentMaskInterpolateUpdate_BetweenLayersOneAndTwo(float2 uv, sampler2D topLayerHeightTex, sampler2D bottomLayerHeightTex, float4 layerFractionalRanges, float similarLayerTransitionDistance, float similarLayerTransitionPower)
{
	float2 topTwoLayerHeights = GetTopTwoLayerFractions(uv, topLayerHeightTex, bottomLayerHeightTex, layerFractionalRanges, similarLayerTransitionDistance, similarLayerTransitionPower);
	return float4(1, topTwoLayerHeights.x, topTwoLayerHeights.y, 1);
}

float4 FragmentMaskInterpolateFromHeightGreen(float2 uv, sampler2D maskHeightTex, sampler2D topLayerHeightTex, sampler2D bottomLayerHeightTex, float4 layerFractionalRanges)
{
	float depth01 = saturate(tex2D(maskHeightTex, uv).r);
	float fractionBetweenBottomAndTop = GetFractionalMaskBetweenLayerHeights_FromDepth(depth01, uv, topLayerHeightTex, bottomLayerHeightTex, layerFractionalRanges);
	return float4(1, fractionBetweenBottomAndTop, 0, 1);
}

float4 FragmentMaskInterpolateFromHeight_BetweenLayersOneAndTwo(float2 uv, sampler2D maskHeightTex, sampler2D topLayerHeightTex, sampler2D bottomLayerHeightTex, float4 layerFractionalRanges, float similarLayerTransitionDistance, float similarLayerTransitionPower)
{
	float depth01 = saturate(tex2D(maskHeightTex, uv).r);
	float2 topTwoLayerHeights = GetTopTwoLayerFractions_FromDepth(depth01, uv, topLayerHeightTex, bottomLayerHeightTex, layerFractionalRanges, similarLayerTransitionDistance, similarLayerTransitionPower);
	return float4(1, topTwoLayerHeights.x, topTwoLayerHeights.y, 1);
}

#endif