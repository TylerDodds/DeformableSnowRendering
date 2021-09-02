// SPDX-License-Identifier: MIT
// SPDX-FileCopyrightText: © 2021 Tyler Dodds

Shader "Snow/Heightmap Subtracted Depth Write"
{
    Properties
    {
        _HeightmapTex ("Heightmap Texture", 2D) = "white" {}
		_Ranges("Mesh Heightmap Range (x), Capture Range (y)", Vector) = (1, 1.1, 0, 0)
    }
    SubShader
    {

        Pass
        {
			Name "Subtracted Depth Write"

			Tags { "LightMode" = "DepthForwardOnly"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float4 clipSpace : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.clipSpace = o.vertex;
                return o;
            }

            sampler2D _HeightmapTex;
			float4 _Ranges;

			float frag (v2f i) : SV_Depth
            {
				float depth01 = i.clipSpace.z;
				#if defined(UNITY_REVERSED_Z)
				#if UNITY_REVERSED_Z == 1
				depth01 = 1.0f - depth01;
				#endif
				#endif
				float2 uv = (i.clipSpace.xy + 1) * 0.5;
				uv.x = 1 - uv.x;//Need to mirror horizontal coordinate based on camera upwards view. We'll do this for heightmap lookup, so the result will be flipped like the incoming depth texture.
				if (_ProjectionParams.x < 0)
					uv.y = 1 - uv.y;//See https://docs.unity3d.com/Manual/SL-PlatformDifferences.html
				float heightmapSample = tex2Dlod(_HeightmapTex, float4(uv, 0, 0)).r;
				float depthRemapped = _Ranges.y * depth01;
				float heightmapRemapped = _Ranges.x * heightmapSample;
				float subtractedRemapped = depthRemapped - heightmapRemapped;
				clip(subtractedRemapped + (_Ranges.y - _Ranges.x));
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
