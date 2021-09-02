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
//Blur, capturing of ground depth.

using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace SnowRendering
{
    class CameraDepthBakeCustomPass : CustomPass
    {
        public Camera bakingCamera = null;
        public RenderTexture targetTexture = null;
        public bool render = true;
        ShaderTagId[] shaderTags;

        [SerializeField]
        LayerMask _layerMask = 1;//2^0 for 0th layer

        [SerializeField]
        Material _depthWriteMaterial;

        [SerializeField]
        bool _captureGroundHeight = false;

        [SerializeField]
        LayerMask _groundLayerMask = 512;//2^9 for 9th layer

        [Range(0, 8)]
        [SerializeField]
        float _blurRadius = 4;

        // Trick to always include these shaders in build
        [SerializeField, HideInInspector]
        Shader blurShader;
        [SerializeField, HideInInspector]
        Shader groundDepthShader;

        RTHandle blurBuffer;
        RTHandle depthBakeBuffer;
        RTHandle groundBakeBuffer;
        Material blurMaterial;
        Material groundDepthMaterial;
        Material litMaterial;
        int litMaterialDepthOnlyPassIndex;

        private static readonly string _groundDepthTextureName = "_GroundDepthTex";
        private static readonly string _rangesVectorName = "_Ranges";

        static class ShaderID
        {
            public static readonly int _Radius = Shader.PropertyToID("_Radius");
            public static readonly int _Source = Shader.PropertyToID("_Source");
            public static readonly int _TextureSize = Shader.PropertyToID("_TextureSize");
        }

        private ProfilingSampler _horizontalBlurSampler;
        private ProfilingSampler _verticalBlurSampler;

        private ShaderPassId _hBlurPassId;
        private ShaderPassId _vBlurPassId;

        private int GetShaderPassId(string passName, Material material, ref int passId, int defaultPassId)
        {
            if (passId < 0)
            {
                passId = material.FindPass(passName);
                if (passId < 0)
                {
                    passId = defaultPassId;
                }
            }
            return passId;
        }

        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in an performance manner.
        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            if (targetTexture == null)
            {
                return;
            }

            _hBlurPassId = new ShaderPassId("Horizontal Blur", blurMaterial, 0);
            _vBlurPassId = new ShaderPassId("Vertical Blur", blurMaterial, 1);

            _horizontalBlurSampler = new ProfilingSampler("H Blur");
            _verticalBlurSampler = new ProfilingSampler("V Blur");

            shaderTags = new ShaderTagId[2]
            {
            new ShaderTagId("DepthOnly"),
            new ShaderTagId("DepthForwardOnly"),
            };

            if (blurBuffer == null)
            {
                blurShader = Shader.Find("Hidden/FullScreen/TextureBlur");
            }
            blurMaterial = CoreUtils.CreateEngineMaterial(blurShader);

            depthBakeBuffer = RTHandles.Alloc(targetTexture.width, targetTexture.height, colorFormat: GraphicsFormat.None, depthBufferBits: DepthBits.Depth16, dimension: TextureDimension.Tex2D, name: "Object Depth");
            blurBuffer = RTHandles.Alloc(targetTexture.width, targetTexture.height, colorFormat: GraphicsFormat.R16_UNorm, dimension: TextureDimension.Tex2D);

            if (_captureGroundHeight)
            {
                groundBakeBuffer = RTHandles.Alloc(targetTexture.width, targetTexture.height, colorFormat: GraphicsFormat.None, depthBufferBits: DepthBits.Depth16, dimension: TextureDimension.Tex2D, name: "Ground Depth");
                groundDepthShader = Shader.Find("Hidden/Snow/Ground Depth Subtracted Depth Write");
                groundDepthMaterial = CoreUtils.CreateEngineMaterial(groundDepthShader);
                groundDepthMaterial.SetTexture(_groundDepthTextureName, groundBakeBuffer);

                litMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("HDRP/Lit"));
                litMaterialDepthOnlyPassIndex = litMaterial.FindPass("DepthOnly");
            }

            //NB We won't handle change in targetTexture size here. In case we need to, we should Cleanup() and Setup() the pass from a higher architectural level.

            //See these notes for trying to write to a depth-format RenderTexture: https://forum.unity.com/threads/is-it-possible-to-write-into-a-depth-render-texture-using-setrendertarget.535567/
            //In short, it seems to be a bit of a black box, and the source of a lot of frustration. Therefore we'll just accept that our output will be a non-depth type, which means we'll unfortunately need two temporary textures (one for depth capture, one for first blur direction) to handle effect.
        }

        void SetTextureSize(CommandBuffer cmd, MaterialPropertyBlock block, RenderTexture target)
        {
            Vector2Int scaledViewportSize = new Vector2Int(target.width, target.height);
            block.SetVector(ShaderID._TextureSize, new Vector4(scaledViewportSize.x, scaledViewportSize.y, 1.0f / scaledViewportSize.x, 1.0f / scaledViewportSize.y));
        }

        protected override void Execute(CustomPassContext ctx)
        {
            if (targetTexture == null || bakingCamera == null || !render || ctx.hdCamera.camera == bakingCamera)
            {
                return;
            }

            RTHandles.SetReferenceSize(targetTexture.width, targetTexture.height, MSAASamples.None);

            bakingCamera.TryGetCullingParameters(out ScriptableCullingParameters cullingParams);
            cullingParams.cullingOptions = CullingOptions.ShadowCasters;
            cullingParams.cullingMask = (uint)_layerMask.value;
            CullingResults cullingResult = ctx.renderContext.Cull(ref cullingParams);

            RenderStateBlock heightRenderStateBlock = new RenderStateBlock(RenderStateMask.Raster) { rasterState = new RasterState(depthClip: false) };
            var result = new RendererListDesc(shaderTags, cullingResult, bakingCamera)
            {
                rendererConfiguration = PerObjectData.None,
                renderQueueRange = RenderQueueRange.all,
                sortingCriteria = SortingCriteria.BackToFront,
                excludeObjectMotionVectors = false,
                stateBlock = heightRenderStateBlock,
                layerMask = _layerMask,
                overrideMaterial = _depthWriteMaterial,
            };

            var p = GL.GetGPUProjectionMatrix(bakingCamera.projectionMatrix, true);
            Matrix4x4 scaleMatrix = Matrix4x4.identity;
            scaleMatrix.m22 = -1.0f;
            var v = scaleMatrix * bakingCamera.transform.localToWorldMatrix.inverse;
            var vp = p * v;
            ctx.cmd.SetGlobalMatrix("unity_MatrixVP", vp);//Our "Regular" (non-HDRP) depth write shader needs this matrix name definition for the vertex shader. The other matrices are overwritten by the HDRP monolithic camera command buffer, so we don't need those.

            if (_captureGroundHeight && _depthWriteMaterial != null && _depthWriteMaterial.HasProperty(_rangesVectorName))
            {
                bakingCamera.TryGetCullingParameters(out ScriptableCullingParameters cullingParamsGround);
                cullingParamsGround.cullingOptions = CullingOptions.ShadowCasters;
                cullingParamsGround.cullingMask = (uint)_groundLayerMask.value;
                CullingResults cullingResultGround = ctx.renderContext.Cull(ref cullingParamsGround);

                RenderStateBlock groundRendererStateBlock = new RenderStateBlock(RenderStateMask.Raster | RenderStateMask.Depth) { rasterState = new RasterState(cullingMode: CullMode.Off), depthState = new DepthState(compareFunction: CompareFunction.GreaterEqual) };
                RendererListDesc resultGround = new RendererListDesc(shaderTags, cullingResultGround, bakingCamera)
                {
                    rendererConfiguration = PerObjectData.None,
                    renderQueueRange = RenderQueueRange.all,
                    sortingCriteria = SortingCriteria.BackToFront,
                    excludeObjectMotionVectors = false,
                    stateBlock = groundRendererStateBlock,
                    layerMask = _groundLayerMask,
                    overrideMaterial = litMaterial,
                    overrideMaterialPassIndex = litMaterialDepthOnlyPassIndex,//Need to specify the DepthOnly pass index as well
                };

                CoreUtils.SetRenderTarget(ctx.cmd, groundBakeBuffer, ClearFlag.None);
                ctx.cmd.ClearRenderTarget(true, false, Color.white, 0f);
                using (new HDRenderPipeline.OverrideCameraRendering(ctx.cmd, bakingCamera, false))
                {
                    CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, RendererList.Create(resultGround));
                }

                groundDepthMaterial.SetVector(_rangesVectorName, _depthWriteMaterial.GetVector(_rangesVectorName));
                result.overrideMaterial = groundDepthMaterial;
            }

            CoreUtils.SetRenderTarget(ctx.cmd, depthBakeBuffer, ClearFlag.Depth);
            using (new HDRenderPipeline.OverrideCameraRendering(ctx.cmd, bakingCamera, false))
            {
                CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, RendererList.Create(result));
            }


            //For information on use of RTHandles system, see: https://docs.unity3d.com/Packages/com.unity.render-pipelines.core@10.2/manual/rthandle-system-using.html

            // Horizontal Blur
            using (new ProfilingScope(ctx.cmd, _horizontalBlurSampler))
            {
                var hBlurProperties = new MaterialPropertyBlock();
                hBlurProperties.SetFloat(ShaderID._Radius, _blurRadius / 4.0f);
                hBlurProperties.SetTexture(ShaderID._Source, depthBakeBuffer);
                SetTextureSize(ctx.cmd, hBlurProperties, blurBuffer);
                CoreUtils.DrawFullScreen(ctx.cmd, blurMaterial, blurBuffer, hBlurProperties, shaderPassId: _hBlurPassId);
            }

            // Copy back the result in the target render texture while doing a vertical blur
            using (new ProfilingScope(ctx.cmd, _verticalBlurSampler))
            {
                var vBlurProperties = new MaterialPropertyBlock();
                vBlurProperties.SetFloat(ShaderID._Radius, _blurRadius / 4.0f);
                vBlurProperties.SetTexture(ShaderID._Source, blurBuffer);
                SetTextureSize(ctx.cmd, vBlurProperties, targetTexture);
                CoreUtils.DrawFullScreen(ctx.cmd, blurMaterial, targetTexture, vBlurProperties, shaderPassId: _vBlurPassId);
            }
        }

        // release all resources
        protected override void Cleanup()
        {
            if (blurMaterial != null)
            {
                CoreUtils.Destroy(blurMaterial);
            }
            if (blurBuffer != null)
            {
                blurBuffer.Release();
            }
            if (depthBakeBuffer != null)
            {
                depthBakeBuffer.Release();
            }
            if (_captureGroundHeight)
            {
                if (groundDepthMaterial != null)
                {
                    CoreUtils.Destroy(groundDepthMaterial);
                }
                if (litMaterial != null)
                {
                    CoreUtils.Destroy(litMaterial);
                }
                if (groundBakeBuffer != null)
                {
                    groundBakeBuffer.Release();
                }
            }
        }
    }
}