// SPDX-License-Identifier: MIT
// SPDX-FileCopyrightText: © 2021 Tyler Dodds

#ifndef SNOW_LINE_RASTERIZING_INCLUDED
#define SNOW_LINE_RASTERIZING_INCLUDED

void RasterizeLine(int x0, int y0, int x1, int y1, float refRawDepth, float refHeight, float4 refRawMotionVectors)
{
	const float motionEpsilon = 0.000000000001;

	int deltaX = abs(x1 - x0);
	int shiftX = x0 < x1 ? 1 : -1;
	int deltaY = -abs(y1 - y0);
	int shiftY = y0 < y1 ? 1 : -1;
	int err = deltaX + deltaY;
	while (true)
	{
		//NB If line rasterization is needed elsewhere, this function at (x0, y0) could be generalized.
		uint2 id = uint2(x0, y0);
		uint2 horizontalReversedId = uint2(TextureSize.x - 1 - id.x, id.y);
		float depth01 = CurrentHeightTextureRW[horizontalReversedId];
		float height = lerp(depth01, 1.0 - depth01, ReversedZAmount);
		//NB In some cases, we may need to properly handle atomic operations here, using eg. InterlockedMin with int-encoded height and velocity (as for displacement and velocity simulation kernels).
		if (refHeight < height)
		{
			CurrentHeightTextureRW[horizontalReversedId] = refRawDepth;
			
			float2 uv = GetUv(id);
			float2 horizontalReversedUvs = float2(1 - uv.x, uv.y);
			float4 rawMotionVectors = CameraMotionVectorsTexture.SampleLevel(linearClampSampler, horizontalReversedUvs, 0);
			if (length(rawMotionVectors.xy) < motionEpsilon)//Assign motion vectors if there was no motion here previously
			{
				CameraMotionVectorsTextureRW[horizontalReversedId] = refRawMotionVectors;
			}
		}

		//Continue rasterization
		if (x0 == x1 && y0 == y1)
		{
			break;
		}
		int e2 = 2 * err;
		if (e2 >= deltaY)
		{
			err += deltaY;
			x0 += shiftX;
		}
		if (e2 <= deltaX) 
		{
			err += deltaX;
			y0 += shiftY;
		}
	}
}

#endif