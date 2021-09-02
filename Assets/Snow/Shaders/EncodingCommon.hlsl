// SPDX-License-Identifier: MIT
// SPDX-FileCopyrightText: © 2021 Tyler Dodds

#ifndef SNOW_ENCODING_COMMON_INCLUDED
#define SNOW_ENCODING_COMMON_INCLUDED

#define FloatToIntEncodingMultiplier 500000.0
#define FloatToUIntEncodingMultiplier 1000000.0

int EncodeFloatAsInt(float f)
{
	return (int)(f * FloatToIntEncodingMultiplier);
}

float DecodeIntToFloat(int i)
{
	return ((float)(i)) / FloatToIntEncodingMultiplier;
}

uint EncodeFloatAsUInt(float f)
{
	return (uint)(f * FloatToUIntEncodingMultiplier);
}

float DecodeUIntToFloat(uint i)
{
	return ((float)(i)) / FloatToUIntEncodingMultiplier;
}


int EncodeHeight(float height)
{
	return EncodeFloatAsInt(height);
}


float DecodeHeight(int height)
{
	return DecodeIntToFloat(height);
}

#endif