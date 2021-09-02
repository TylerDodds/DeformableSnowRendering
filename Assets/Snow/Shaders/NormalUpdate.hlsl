// SPDX-License-Identifier: MIT
// SPDX-FileCopyrightText: © 2021 Tyler Dodds

#ifndef SNOW_NORMAL_UPDATE_INCLUDED
#define SNOW_NORMAL_UPDATE_INCLUDED

sampler2D   _HeightTex;
float4 _HeightTex_TexelSize;
sampler2D   _InitialHeightTex;
float4 _Scale;//NB Scale w component is the yDirectionDepthDifferenceMultiplier
float _HeightFractionSmoothing;

float3 NormalStrength(float3 normalVector, float normalStrength)
{
	return normalize(float3(normalVector.rg * normalStrength, lerp(1, normalVector.b, saturate(normalStrength))));
}

float3 NormalBlend_Reoriented(float3 A, float3 B)
{
	float3 t = A.xyz + float3(0.0, 0.0, 1.0);
	float3 u = B.xyz * float3(-1.0, -1.0, 1.0);
	return (t / t.z) * dot(t, u) - u;
}

float3 getNormalFromHeightmapDerivativeSamples(float3 texelSize, float depthUPlus, float depthUMinus, float depthVPlus, float depthVMinus)
{
	float yDirectionDepthDifferenceMultiplier = _Scale.w;
	float3 uTangent = float3 (2 * texelSize.xz * _Scale.x, (depthUPlus - depthUMinus) * _Scale.z);
	float3 vTangent = float3 (2 * texelSize.zy * _Scale.y, yDirectionDepthDifferenceMultiplier * (depthVPlus - depthVMinus) * _Scale.z);

	float3 normalFromHeightMask = normalize(cross(uTangent, vTangent));
	return normalFromHeightMask;
}

float3 getNormalFromHeightMask(float2 uv)
{
	float3 texelSize = float3 (_HeightTex_TexelSize.xy, 0);
	float depthUPlus = tex2D(_HeightTex, uv + texelSize.xz).r;
	float depthUMinus = tex2D(_HeightTex, uv - texelSize.xz).r;
	float depthVPlus = tex2D(_HeightTex, uv + texelSize.zy).r;
	float depthVMinus = tex2D(_HeightTex, uv - texelSize.zy).r;

	return getNormalFromHeightmapDerivativeSamples(texelSize, depthUPlus, depthUMinus, depthVPlus, depthVMinus);
}

float3 getNormalFromHeightMaskBlurred(float2 uv)
{
	//We expect horizontally blurred height map in R channel, vertically blurred in G channel
	float3 texelSize = float3 (_HeightTex_TexelSize.xy, 0);
	float depthUPlus = tex2D(_HeightTex, uv + texelSize.xz).g;
	float depthUMinus = tex2D(_HeightTex, uv - texelSize.xz).g;
	float depthVPlus = tex2D(_HeightTex, uv + texelSize.zy).r;
	float depthVMinus = tex2D(_HeightTex, uv - texelSize.zy).r;

	return getNormalFromHeightmapDerivativeSamples(texelSize, depthUPlus, depthUMinus, depthVPlus, depthVMinus);
}

float getFractionOfInitial(float2 uv)
{
	float maskedHeight = tex2D(_HeightTex, uv).r;
	float startingHeight = tex2D(_InitialHeightTex, uv).r;
	float fractionOfInitial = smoothstep(startingHeight - _HeightFractionSmoothing, startingHeight + _HeightFractionSmoothing, maskedHeight);
	return fractionOfInitial;
}

float3 combineBlendedInitialNormals(float2 uv, float3 blendedNormal, float3 startingNormal)
{
	float fractionOfInitial = getFractionOfInitial(uv);
	float3 finalNormal = lerp(blendedNormal, startingNormal, fractionOfInitial);
	return finalNormal;
}

float3 getBlendedNormal(float2 uv, float3 startingNormal)
{
	float3 normalFromHeightMask = getNormalFromHeightMask(uv);
	float3 blendedNormal = NormalBlend_Reoriented(startingNormal, normalFromHeightMask);
	return combineBlendedInitialNormals(uv, blendedNormal, startingNormal);
}

float3 getBlendedDetailNormal(float2 uv)
{
	float3 blendedNormal = getNormalFromHeightMask(uv);
	return combineBlendedInitialNormals(uv, blendedNormal, float3(0, 0, 1));
}

float3 getBlendedDetailNormalFromBlurred(float2 uv)
{
	float3 blendedNormal = getNormalFromHeightMaskBlurred(uv);
	return combineBlendedInitialNormals(uv, blendedNormal, float3(0, 0, 1));
}

float2 FragmentNormalUpdate(float2 uv, sampler2D initialNormalTex, float initialNormalStrength)
{
	float4 startingNormalSampled = tex2D(initialNormalTex, uv);
	float3 startingNormal = NormalStrength(UnpackNormal(startingNormalSampled), initialNormalStrength);//UnpackNormal seems identical to UnpackNormalmapRGorAG used in ShaderGraph, for example.
	float3 finalNormal = getBlendedNormal(uv, startingNormal);
	//NB To pack this back into a CustomRenderTexture that can be used as a normal map, we need to rescale from [-1,1] range to [0,1] range. Only RG unsigned formats work properly as normal maps, so we only need to assign these channels.
	return finalNormal.xy * 0.5 + 0.5;
}

float4 FragmentDetailNormalUpdate(float2 uv)
{
	float3 finalNormal = getBlendedDetailNormal(uv);
	float3 finalNormalRemapped = finalNormal * 0.5 + 0.5;
	return float4(0.5, finalNormalRemapped.y, 0.5, finalNormalRemapped.x);
}

float4 FragmentDetailNormalUpdateFromBlurred(float2 uv)
{
	float3 finalNormal = getBlendedDetailNormalFromBlurred(uv);
	float3 finalNormalRemapped = finalNormal * 0.5 + 0.5;
	return float4(0.5, finalNormalRemapped.y, 0.5, finalNormalRemapped.x);
}

#endif