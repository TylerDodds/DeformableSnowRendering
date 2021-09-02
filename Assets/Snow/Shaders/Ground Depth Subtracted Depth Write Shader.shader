// SPDX-License-Identifier: MIT
// SPDX-FileCopyrightText: © 2021 Tyler Dodds

Shader "Hidden/Snow/Ground Depth Subtracted Depth Write"
{
    Properties
    {
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

            sampler2D _GroundDepthTex;
			float4 _Ranges;

			float frag (v2f i, fixed facing : VFACE) : SV_Depth
            {
				float depth01 = i.clipSpace.z;
				float2 uv = (i.clipSpace.xy + 1) * 0.5;
				//No need to mirror horizontal coordinate based on camera upwards view, since ground depth is rendered from the same viewpoint.
				if (_ProjectionParams.x < 0)
					uv.y = 1 - uv.y;//See https://docs.unity3d.com/Manual/SL-PlatformDifferences.html
				float groundDepthSample = SAMPLE_DEPTH_TEXTURE(_GroundDepthTex, uv).r;
				#if defined(UNITY_REVERSED_Z)
				#if UNITY_REVERSED_Z == 1
				depth01 = 1.0 - depth01;
				groundDepthSample = 1.0 - groundDepthSample;
				#endif
				#endif
				float subtracted = depth01 - groundDepthSample;
				float subtractedRemapped = subtracted * _Ranges.y;
				clip(subtractedRemapped + (_Ranges.y - _Ranges.x));
				float subtracted01 = saturate(subtractedRemapped / (_Ranges.y - _Ranges.x));
				#if defined(UNITY_REVERSED_Z)
				#if UNITY_REVERSED_Z == 1
				subtracted01 = 1.0 - subtracted01;
				#endif
				#endif
				return subtracted01;
            }
            ENDCG
        }
    }
}
