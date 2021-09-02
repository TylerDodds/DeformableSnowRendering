//MIT License

//Copyright(c) 2019 Antoine Lelievre

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

//Modifications Copyright(c) 2021 Tyler Dodds

Shader "Hidden/FullScreen/TextureBlur"
{
    HLSLINCLUDE

    #pragma vertex Vert

    #pragma target 4.5
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
    #pragma enable_d3d11_debug_symbols

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

    // The PositionInputs struct allow you to retrieve a lot of useful information for your fullScreenShader:
    // struct PositionInputs
    // {
    //     float3 positionWS;  // World space position (could be camera-relative)
    //     float2 positionNDC; // Normalized screen coordinates within the viewport    : [0, 1) (with the half-pixel offset)
    //     uint2  positionSS;  // Screen space pixel coordinates                       : [0, NumPixels)
    //     uint2  tileCoord;   // Screen tile coordinates                              : [0, NumTiles)
    //     float  deviceDepth; // Depth from the depth buffer                          : [0, 1] (typically reversed)
    //     float  linearDepth; // View space Z coordinate                              : [Near, Far]
    // };

    // To sample custom buffers, you have access to these functions:
    // But be careful, on most platforms you can't sample to the bound color buffer. It means that you
    // can't use the SampleCustomColor when the pass color buffer is set to custom (and same for camera the buffer).
    // float3 SampleCustomColor(float2 uv);
    // float3 LoadCustomColor(uint2 pixelCoords);
    // float LoadCustomDepth(uint2 pixelCoords);
    // float SampleCustomDepth(float2 uv);

    // There are also a lot of utility function you can use inside Common.hlsl and Color.hlsl,
    // you can check them out in the source code of the core SRP package.

    sampler2D _Source;
    float _Radius;
    float4 _TextureSize; // We need the texture size because we have a non fullscreen render target (blur buffers are downsampled in half res)

    #pragma enable_d3d11_debug_symbols

    float BlurPixels(float taps[9])
    {
        return 0.27343750 * (taps[4]          )
             + 0.21875000 * (taps[3] + taps[5])
             + 0.10937500 * (taps[2] + taps[6])
             + 0.03125000 * (taps[1] + taps[7])
             + 0.00390625 * (taps[0] + taps[8]);
    }

	//NB Note that we don't need to clamp UVs, since we're dealing with full-sized RenderTextures, so we won't bleed from larger-sized RenderTextures that the RenderHandle is bound to.
    float2 GetSampleUVs(Varyings varyings)
    {
        PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _TextureSize.zw);
		return posInput.positionNDC.xy;//Probably don't need to multiply by _RTHandleScale.xy, because texture size is independent of camera
    }

    float HorizontalBlur(Varyings varyings) : SV_Target
    {
        float2 texcoord = GetSampleUVs(varyings);

        float2 offset = _TextureSize.zw * _Radius;
        float taps[9];
        for (int i = -4; i <= 4; i++)
        {
            float2 uv = (texcoord + float2(i, 0) * offset);
            taps[i + 4] = tex2D(_Source, uv).r;
        }

        return BlurPixels(taps);
    }

    float VerticalBlur(Varyings varyings) : SV_Target
    {
        float2 texcoord = GetSampleUVs(varyings);

        float2 offset = _TextureSize.zw * _Radius;
        float taps[9];
        for (int i = -4; i <= 4; i++)
        {
            float2 uv = (texcoord + float2(0, i) * offset);
            taps[i + 4] = tex2D(_Source, uv).r;
        }

        return BlurPixels(taps);
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "Horizontal Blur"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment HorizontalBlur
            ENDHLSL
        }

        Pass
        {
            Name "Vertical Blur"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment VerticalBlur
            ENDHLSL
        }
    }
    Fallback Off
}
