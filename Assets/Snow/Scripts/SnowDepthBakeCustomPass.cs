// SPDX-License-Identifier: MIT
// SPDX-FileCopyrightText: © 2021 Tyler Dodds

using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace SnowRendering
{
    class SnowDepthBakeCustomPass : CustomPass
    {
        [Header("Setup")]

        [SerializeField]
        private GameObject _snowGroundObjectsRoot = null;

        [SerializeField]
        private Material _templateSnowMaterial = null;

        [Tooltip("The horizontal scale (x, z) in meters covered by the template material.")]
        [SerializeField]
        private Vector2 _templateSnowMaterialScale = Vector2.one;

        [SerializeField]
        private bool _useTemplateMaterialInitialTextureScaleOffets = false;

        [SerializeField]
        private int _textureEdgeSize = 2048;

        [Range(0, 1)]
        [SerializeField]
        private float _startingHeightFraction = 0.7f;
        [Range(0, 1)]
        [SerializeField]
        private float _startingHeightOffset = 0;
        [SerializeField]
        private Texture2D _startingHeightTexture = null;

        [SerializeField]
        [Tooltip("This seems to be necessary to correctly generate detail normal maps from generated heightmaps, even if this doesn't match what we'd expect with authored detail normal maps.")]
        private bool _invertDetailNormalY = true;


        [Header("Initialization")]

        [SerializeField]
        private LayerMask _groundLayerMask = 512;//2^9 for 9th layer

        [SerializeField]
        private uint _edgeFalloffDownsampleTimes = 1;

        [SerializeField]
        private float _falloffScale = 10;
        [SerializeField]
        private float _falloffOffset = 0.2f;
        [SerializeField]
        private float _falloffLaplacianBlurRadius = 16;
        [SerializeField]
        private float _edgeFalloffRadius = 8;


        [Header("Setup & Execution")]

        [SerializeField]
        private bool _falloffAtEdges = true;

        [SerializeField]
        private bool _useSmoothedNormalGeneration = true;

        [SerializeField]
        private ComputeShader _displacementComputeShader;

        [SerializeField]
        private bool _simulateVelocity = true;

        //Store values used during Setup, in case user changes them during executing, eg. in the Editor
        private bool _falloffAtEdgesValue = true;
        private bool _useSmoothedNormalGenerationValue = true;
        private ComputeShader _displacementComputeShaderValue = null;
        private bool _simulateVelocityValue = true;


        [Header("Execution")]

        [SerializeField]
        private bool _render = true;

        [SerializeField]
        private LayerMask _layerMask = 1;//2^0 for 0th layer

        [Range(0, 8)]
        [SerializeField]
        private float _blurRadius = 4;

        [Range(0, 10)]
        [SerializeField]
        private int _numExtraDisplacementPasses = 0;

        [Range(0, 5)]
        [SerializeField]
        private float _speedScale = 1f;
        [Range(0, 5)]
        [SerializeField]
        private float _speedOffset = 0f;
        [SerializeField]
        [Range(0.2f, 5f)]
        [Tooltip("Power of gradient magnitude to use as horizontal normal strength in displacement calculation.")]
        private float _gradientFractionalPower = 0.5f;
        [SerializeField]
        [Range(0f, 5f)]
        [Tooltip("Scale of gradient magnitude to use as horizontal normal strength in displacement calculation.")]
        private float _gradientScale = 1.5f;
        [SerializeField]
        [Range(0f, 1f)]
        private float _motionJitterScale = 1f;

        [SerializeField]
        private bool _useHeightSpreadPass = true;
        [SerializeField]
        [Range(0.1f, 5f)]
        private float _heightSpreadHalfLife = 1f;
        [SerializeField]
        [Range(0.1f, 5f)]
        private float _heightDecayHalfLife = 0.5f;

        [SerializeField]
        private bool _approximateSweptArea = true;
        [SerializeField]
        [Range(0.1f, 5f)]
        private float _heightBlockHalfLife = 0.5f;

        [SerializeField]
        [Range(0, 5)]
        private float _heightDecayMagnitude = 2.2f;
        [SerializeField]
        [Range(0, 1)]
        private float _heightDecayReference = 1.0f;
        [SerializeField]
        [Range(-2, 2)]
        private float _heightDecayPowerLow = -0.5f;
        [SerializeField]
        [Range(-2, 2)]
        private float _heightDecayPowerHigh = 0.0f;

        [SerializeField]
        [Range(0, 5)]
        private float _velocityDecayMagnitude = 0.5f;
        [SerializeField]
        [Range(0, 10)]
        private float _velocityDecayReference = 1.2f;
        [SerializeField]
        [Range(-2, 2)]
        private float _velocityDecayPowerLow = -1.0f;
        [SerializeField]
        [Range(-2, 2)]
        private float _velocityDecayPowerHigh = 1.2f;

        // Trick to always include these shaders in build
        [SerializeField, HideInInspector]
        private Shader _blurShader;
        [SerializeField, HideInInspector]
        private Shader _groundDepthShader;
        [SerializeField, HideInInspector]
        private Shader _maskShaderCRT;
        [SerializeField, HideInInspector]
        private Shader _maskShaderFullscreen;
        [SerializeField, HideInInspector]
        private Shader _detailNormalShaderCRT;
        [SerializeField, HideInInspector]
        private Shader _detailNormalShaderFullscreen;
        [SerializeField, HideInInspector]
        private Shader _dilationShader;
        [SerializeField, HideInInspector]
        private Shader _indicatorFunctionShader;
        [SerializeField, HideInInspector]
        private Shader _gradientShader;
        [SerializeField, HideInInspector]
        private Shader _scharrShader;
        [SerializeField, HideInInspector]
        private Shader _tintedLookupShader;
        [SerializeField, HideInInspector]
        private Shader _depthRectificationShader;
        [SerializeField, HideInInspector]
        private Shader _uvEdgesShader;
        [SerializeField, HideInInspector]
        private Shader _heightEncodeDecodeShader;

        private Camera _bakingCamera = null;
        private HDCamera _bakingHDCamera = null;
        private ShaderTagId[] _depthShaderTags;
        private ShaderTagId[] _motionVectorsShaderTags;

        private Bounds _renderersBounds = new Bounds();
        private Vector2 _templateSnowRange;
        private int _templateLayerCount;
        private Vector2Int _textureSize;
        private float _originalBoundsHeight;
        private float _boundsAndSnowHeight;

        private bool _performedGroundBake = false;
        private bool _performedHeightEncoding = false;

        private RTHandle _blurBuffer;//Temporary buffer for performing blurring
        private RTHandle _dilationBuffer;//Temporary buffer for performing dilation/erosion
        private RTHandle _depthBakeBuffer;//Temporary buffer for baking of depth each frame

        private RTHandle _depthCaptureBuffer;//Buffer to hold final depth results each frame to use in later update via RenderTexture or ComputeShader
        private RTHandle _motionVectorsBuffer;//Temporary buffer for baking of motion vectors each frame to use in later update via ComputeShader
        private RTHandle _gradientBuffer;//Buffer to hold final horizontal gradient results each frame to use in later update via ComputeShader

        private RenderTexture _edgeFalloffTexture;//This a permanent RenderTexture to hold the baked edge falloff mask height texture over time
        private RenderTexture _groundBakeTexture;//This a permanent RenderTexture to hold the baked ground height over time

        private RenderTexture _maskHeightRenderTexture;//This is a permanent RenderTexture that gets updated with current height over time
        private RenderTexture _layerMaskRenderTexture;
        private RenderTexture _detailNormalRenderTexture;

        private ComputeBuffer _heightComputeBufferEncoded;
        private ComputeBuffer _velocityComputeBufferEncoded;
        private ComputeBuffer _velocityComputeBufferFloat;

        private Material _detailNormalMaterial;
        private Material _maskMaterial;
        private Material _blurMaterial;
        private Material _groundDepthMaterial;
        private Material _litMaterial;
        private int _litMaterialDepthOnlyPassIndex;
        private Material _dilationMaterial;
        private Material _indicatorMaterial;
        private Material _gradientMaterial;
        private Material _scharrMaterial;
        private Material _tintedLookupMaterial;
        private Material _depthRectificationMaterial;
        private Material _uvEdgesMaterial;
        private Material _heightEncodeDecodeMaterial;
        private MaterialPropertyBlock _snowMaterialProperties;
        private MaterialPropertyBlock _hBlurProperties;
        private MaterialPropertyBlock _vBlurProperties;
        private MaterialPropertyBlock _hDilationProperties;
        private MaterialPropertyBlock _vDilationProperties;
        private MaterialPropertyBlock _indicatorProperties;
        private MaterialPropertyBlock _gradientProperties;
        private MaterialPropertyBlock _scharrProperties;
        private MaterialPropertyBlock _tintedLookupProperties;
        private MaterialPropertyBlock _depthRectificationProperties;
        private MaterialPropertyBlock _uvEdgesProperties;
        private MaterialPropertyBlock _outputRenderTextureProperties;
        private MaterialPropertyBlock _heightEncodeDecodeProperties;
        private MaterialPropertyBlock _sweepAreaProperties;

        private ComputeShaderKernelId _displacementKernelIndex;
        private ComputeShaderKernelId _minimumKernelIndex;
        private ComputeShaderKernelId _minimumFalloffKernelIndex;
        private ComputeShaderKernelId _heightAboveOneSpreadKernelIndex;
        private ComputeShaderKernelId _velocityClearKernelIndex;
        private ComputeShaderKernelId _velocityUpdateKernelIndex;
        private ComputeShaderKernelId _velocityDecodeKernelIndex;
        private ComputeShaderKernelId _heightEncodeKernelIndex;
        private ComputeShaderKernelId _sweepAreaKernelIndex;
        private ShaderPassId _hBlurPassId;
        private ShaderPassId _vBlurPassId;
        private ShaderPassId _depthRectificationPassId;
        private ShaderPassId _hErosionPassId;
        private ShaderPassId _vErosionPassId;
        private ShaderPassId _hDilationPassId;
        private ShaderPassId _vDilationPassId;
        private ShaderPassId _hDilationBelowEpsPassId;
        private ShaderPassId _vDilationBelowEpsPassId;
        private ShaderPassId _gradientWithMultiplierPassId;
        private ShaderPassId _indicatorFunctionDepthPassId;
        private ShaderPassId _gradientLengthOffsetMagnitudeSaturatedPassId;
        private ShaderPassId _laplacianPassId;
        private ShaderPassId _edgeDistanceMinPassId;
        private ShaderPassId _tintedLookupFloatPassId;
        private ShaderPassId _tintedLookupMaxFloatPassId;
        private ShaderPassId _remapBottomToUnitRangePassId;
        private ShaderPassId _lookupPowerOffsetFloatPassId;
        private ShaderPassId _signMappedToUnitRangePassId;
        private ShaderPassId _scharrBlurHorizontalVerticalPassId;
        private ShaderPassId _detailNormalUpdatePassId;
        private ShaderPassId _detailNormalUpdateFromBlurredPassId;
        private ShaderPassId _maskHeightUpdatePassId;
        private ShaderPassId _maskInterpolatedHeightGreenChannelPassId;
        private ShaderPassId _maskInterpolatedHeightLayerOneTwoFractionsPassId;
        private ShaderPassId _maskInterpolatedHeightGreenChannelFromHeightmapPassId;
        private ShaderPassId _maskInterpolatedHeightLayerOneTwoFractionsFromHeightmapPassId;
        private ShaderPassId _heightDecodePassId;

        private ShaderPassId LayerMaskShaderPassId => _templateLayerCount == 3 ? (UpdateLayerMaskFromFinalMaskHeight ? _maskInterpolatedHeightGreenChannelFromHeightmapPassId : _maskInterpolatedHeightGreenChannelPassId) : (UpdateLayerMaskFromFinalMaskHeight ? _maskInterpolatedHeightLayerOneTwoFractionsFromHeightmapPassId : _maskInterpolatedHeightLayerOneTwoFractionsPassId);//Note that _templateLayerCount is either 3 or 4
        private ShaderPassId DetailNormalShaderPassId => _useSmoothedNormalGenerationValue ? _detailNormalUpdateFromBlurredPassId : _detailNormalUpdatePassId;

        private LowDiscrepancySequence _lowDiscrepancySequence;

        List<MeshRenderer> _snowMeshRenderers;
        List<MeshRenderer> _tempMeshRendererComponents;
        List<Material> _tempMaterials;
        List<Vector3> _tempVertices;
        List<Vector2> _tempUVs;

        private bool UpdateMaskHeightFromComputeShader => _displacementComputeShaderValue != null;
        private bool UpdateLayerMaskFromFinalMaskHeight => UpdateMaskHeightFromComputeShader;
        private bool CaptureMotionVectors => UpdateMaskHeightFromComputeShader;
        private bool SimulateVelocity => _simulateVelocityValue && UpdateMaskHeightFromComputeShader;
        private bool ApproximateSweptArea => UpdateMaskHeightFromComputeShader && CaptureMotionVectors && _approximateSweptArea;

        static class ShaderID
        {
            public static readonly int _RTHandleScale = Shader.PropertyToID("_RTHandleScale");//Note that since we allocate fixed-size RTHandles, they will have that fixed currentRenderTargetSize, even if the currentViewportSize is smaller (because main camera is rendering to smaller screen size) and therefore rtHandleScale is less than one. Therefore we can use CustomPassCommon.hlsl-type updating with _TextureSize of the target texture.
            public static readonly int _Radius = Shader.PropertyToID("_Radius");
            public static readonly int _Source = Shader.PropertyToID("_Source");
            public static readonly int _TextureSize = Shader.PropertyToID("_TextureSize");
            public static readonly int _LayerCount = Shader.PropertyToID("_LayerCount");
            public static readonly int _DepthTex = Shader.PropertyToID("_DepthTex");
            public static readonly int _GroundDepthTex = Shader.PropertyToID("_GroundDepthTex");
            public static readonly int _Ranges = Shader.PropertyToID("_Ranges");
            public static readonly int _Epsilon = Shader.PropertyToID("_Epsilon");
            public static readonly int _Multiplier = Shader.PropertyToID("_Multiplier");
            public static readonly int _Offset = Shader.PropertyToID("_Offset");
            public static readonly int _UseHeightBasedBlend = Shader.PropertyToID("_UseHeightBasedBlend");
            public static readonly int unity_MatrixVP = Shader.PropertyToID("unity_MatrixVP");
            public static class Filtering
            {
                public static readonly int _ScaleXOffsetXScaleYOffsetY = Shader.PropertyToID("_ScaleXOffsetXScaleYOffsetY");
                public static readonly int _RemapSource = Shader.PropertyToID("_RemapSource");
            }
            public static class HeightRange
            {
                public static readonly int Parameterization0 = Shader.PropertyToID("_HeightMapParametrization0");
                public static readonly int Parameterization1 = Shader.PropertyToID("_HeightMapParametrization1");
                public static readonly int Parameterization2 = Shader.PropertyToID("_HeightMapParametrization2");
                public static readonly int Parameterization3 = Shader.PropertyToID("_HeightMapParametrization3");
                public static readonly int Center0 = Shader.PropertyToID("_HeightCenter0");
                public static readonly int Center1 = Shader.PropertyToID("_HeightCenter1");
                public static readonly int Center2 = Shader.PropertyToID("_HeightCenter2");
                public static readonly int Center3 = Shader.PropertyToID("_HeightCenter3");
                public static readonly int Amplitude0 = Shader.PropertyToID("_HeightAmplitude0");
                public static readonly int Amplitude1 = Shader.PropertyToID("_HeightAmplitude1");
                public static readonly int Amplitude2 = Shader.PropertyToID("_HeightAmplitude2");
                public static readonly int Amplitude3 = Shader.PropertyToID("_HeightAmplitude3");
                public static readonly int Min0 = Shader.PropertyToID("_HeightMin0");
                public static readonly int Min1 = Shader.PropertyToID("_HeightMin1");
                public static readonly int Min2 = Shader.PropertyToID("_HeightMin2");
                public static readonly int Min3 = Shader.PropertyToID("_HeightMin3");
                public static readonly int Max0 = Shader.PropertyToID("_HeightMax0");
                public static readonly int Max1 = Shader.PropertyToID("_HeightMax1");
                public static readonly int Max2 = Shader.PropertyToID("_HeightMax2");
                public static readonly int Max3 = Shader.PropertyToID("_HeightMax3");
                public static readonly int Offset0 = Shader.PropertyToID("_HeightOffset0");
                public static readonly int Offset1 = Shader.PropertyToID("_HeightOffset1");
                public static readonly int Offset2 = Shader.PropertyToID("_HeightOffset2");
                public static readonly int Offset3 = Shader.PropertyToID("_HeightOffset3");
            }
            public static class HeightMap
            {
                public static readonly int _HeightMap0 = Shader.PropertyToID("_HeightMap0");
                public static readonly int _HeightMap1 = Shader.PropertyToID("_HeightMap1");
                public static readonly int _HeightMap2 = Shader.PropertyToID("_HeightMap2");
                public static readonly int _HeightMap3 = Shader.PropertyToID("_HeightMap3");
                public static int FromLayerCount(int layerCount)
                {
                    switch(layerCount)
                    {
                        case 4:
                            return _HeightMap3;
                        case 3:
                            return _HeightMap2;
                        case 2:
                            return _HeightMap1;
                        default:
                            return _HeightMap0;
                    }
                }
            }
            public static class NormalMap
            {
                public static readonly int _NormalMap0 = Shader.PropertyToID("_NormalMap0");
                public static readonly int _NormalMap1 = Shader.PropertyToID("_NormalMap1");
                public static readonly int _NormalMap2 = Shader.PropertyToID("_NormalMap2");
                public static readonly int _NormalMap3 = Shader.PropertyToID("_NormalMap3");
                public static int FromLayerCount(int layerCount)
                {
                    switch (layerCount)
                    {
                        case 4:
                            return _NormalMap3;
                        case 3:
                            return _NormalMap2;
                        case 2:
                            return _NormalMap1;
                        default:
                            return _NormalMap0;
                    }
                }
            }
            public static class NormalScale
            {
                public static readonly int _NormalScale0 = Shader.PropertyToID("_NormalScale0");
                public static readonly int _NormalScale1 = Shader.PropertyToID("_NormalScale1");
                public static readonly int _NormalScale2 = Shader.PropertyToID("_NormalScale2");
                public static readonly int _NormalScale3 = Shader.PropertyToID("_NormalScale3");
                public static int FromLayerCount(int layerCount)
                {
                    switch (layerCount)
                    {
                        case 4:
                            return _NormalScale3;
                        case 3:
                            return _NormalScale2;
                        case 2:
                            return _NormalScale1;
                        default:
                            return _NormalScale0;
                    }
                }
            }
            public static class Mask
            {
                public static readonly int _TopLayerHeightTex = Shader.PropertyToID("_TopLayerHeightTex");
                public static readonly int _BottomLayerHeightTex = Shader.PropertyToID("_BottomLayerHeightTex");
                public static readonly int _LayerFractionalRanges = Shader.PropertyToID("_LayerFractionalRanges");
            }
            public static class Detail
            {
                public static readonly int _InitialHeightTex = Shader.PropertyToID("_InitialHeightTex");
                public static readonly int _InitialNormalTex = Shader.PropertyToID("_InitialNormalTex");
                public static readonly int _InitialNormalStrength = Shader.PropertyToID("_InitialNormalStrength");
                public static readonly int _Scale = Shader.PropertyToID("_Scale");
                public static readonly int _HeightTex = Shader.PropertyToID("_HeightTex");
                public static readonly int _MaskHeightTex = Shader.PropertyToID("_MaskHeightTex");
            }
            public static class Compute
            {
                public static readonly int ReversedZAmount = Shader.PropertyToID("ReversedZAmount");
                public static readonly int TextureSize = Shader.PropertyToID("TextureSize");
                public static readonly int TexelsPerMeter = Shader.PropertyToID("TexelsPerMeter");

                public static readonly int MaskHeightTexture = Shader.PropertyToID("MaskHeightTexture");
                public static readonly int CurrentHeightTexture = Shader.PropertyToID("CurrentHeightTexture");
                public static readonly int CameraMotionVectorsTexture = Shader.PropertyToID("CameraMotionVectorsTexture");
                public static readonly int HorizontalGradientTexture = Shader.PropertyToID("HorizontalGradientTexture");
                public static readonly int HeightComputeBufferEncodedReadWrite = Shader.PropertyToID("HeightComputeBufferEncodedReadWrite");
                public static readonly int FalloffMaskTexture = Shader.PropertyToID("FalloffMaskTexture");

                public static readonly int CurrentHeightTextureRW = Shader.PropertyToID("CurrentHeightTextureRW");
                public static readonly int CameraMotionVectorsTextureRW = Shader.PropertyToID("CameraMotionVectorsTextureRW");

                public static readonly int VelocityComputeBufferEncodedReadWrite = Shader.PropertyToID("VelocityComputeBufferEncodedReadWrite");
                public static readonly int VelocityComputeBufferFloatRead = Shader.PropertyToID("VelocityComputeBufferFloatRead");
                public static readonly int VelocityComputeBufferEncodedRead = Shader.PropertyToID("VelocityComputeBufferEncodedRead");
                public static readonly int VelocityComputeBufferFloatReadWrite = Shader.PropertyToID("VelocityComputeBufferFloatReadWrite");

                public static readonly int FrameTime = Shader.PropertyToID("FrameTime");
                public static readonly int SpeedScaleOffset_GradientFractionalPowerScale = Shader.PropertyToID("SpeedScaleOffset_GradientFractionalPowerScale");
                public static readonly int Jitter = Shader.PropertyToID("Jitter");
                public static readonly int HalfLife_HeightSpread_Decay_Block = Shader.PropertyToID("HalfLife_HeightSpread_Decay_Block");
                public static readonly int HeightDecay_Reference_Magnitude_PowerLowHigh = Shader.PropertyToID("HeightDecay_Reference_Magnitude_PowerLowHigh");
                public static readonly int VelocityDecay_Reference_Magnitude_PowerLowHigh = Shader.PropertyToID("VelocityDecay_Reference_Magnitude_PowerLowHigh");
            }
        }

        static class PassNames
        {
            public static readonly string HorizontalBlur = "Horizontal Blur";
            public static readonly string VerticalBlur = "Vertical Blur";
            public static readonly string DepthRectificationHeight = "Height From Depth";
            public static readonly string HorizontalErosion = "Horizontal Erosion";
            public static readonly string VerticalErosion = "Vertical Erosion";
            public static readonly string HorizontalDilation = "Horizontal Dilation";
            public static readonly string VerticalDilation = "Vertical Dilation";
            public static readonly string HorizontalDilationBelowEpsilon = "Horizontal Dilation If Below Epsilon";
            public static readonly string VerticalDilationBelowEpsilon = "Vertical Dilation If Below Epsilon";
            public static readonly string GradientWithMultiplier = "Gradient 2D With Multiplier";
            public static readonly string IndicatorFunctionOfDepth = "Indicator Function of Depth";
            public static readonly string GradientLengthOffsetMagnitudeSaturated = "Length of Gradient 2D, With Scale And Offset, Saturated";
            public static readonly string Laplacian = "Laplacian";
            public static readonly string EdgeDistanceMin = "Edge Distance (Min Blend)";
            public static readonly string TintedLookupFloat = "Tinted Lookup (float)";
            public static readonly string TintedLookupMaxFloat = "Tinted Lookup Max (float)";
            public static readonly string RemapBottomToUnitRange = "Remap [Bottom, 1] to [0, 1] (float)";
            public static readonly string LookupPowerOffsetFloat = "Power and Offset (float)";
            public static readonly string SignMappedToUnitRange = "Sign Mapped 0 To 1";
            public static readonly string ScharrBlurHorizontalVertical = "Scharr Blur Horizontal & Vertical";
            public static readonly string DetailNormalUpdate = "DetailNormalUpdate";
            public static readonly string DetailNormalUpdateFromBlurred = "DetailNormalUpdateFromBlurred";
            public static readonly string MaskHeightUpdate = "HeightUpdate";
            public static readonly string MaskInterpolatedHeightGreenChannel = "MaskInterpolateUpdateGreen";
            public static readonly string MaskInterpolatedHeightLayerOneTwoFractions = "MaskInterpolateUpdate_BetweenLayersOneAndTwo";
            public static readonly string MaskInterpolatedHeightGreenChannelFromHeightmap = "MaskInterpolateFromHeight_Green";
            public static readonly string MaskInterpolatedHeightLayerOneTwoFractionsFromHeightmap = "MaskInterpolateFromHeight_BetweenLayersOneAndTwo";
            public static readonly string HeightDecode = "Height Decode";
        }

        static class KernelNames
        {
            public static readonly string SimpleDisplacement = "SimpleDisplacement";
            public static readonly string HeightMinimum = "HeightMinimum";
            public static readonly string HeightMinimumWithFalloffMask = "HeightMinimumWithFalloffMask";
            public static readonly string HeightAboveOneSpread = "HeightAboveOneSpread";
            public static readonly string VelocityClear = "VelocityClear";
            public static readonly string VelocityUpdateSimple = "VelocityUpdateSimple";
            public static readonly string VelocityDecode = "VelocityDecode";
            public static readonly string HeightEncode = "HeightEncode";
            public static readonly string SweepArea = "SweepArea";
        }

        static class ShaderNames
        {
            public static readonly string LitHDRP = "HDRP/Lit";
            public static readonly string MaskUpdate = "Snow/Layer Mask Update From Depth Texture Fullscreen";
            public static readonly string DetailNormal = "Snow/Normal Update From Height Texture Fullscreen";
            public static readonly string Blur = "Hidden/FullScreen/TextureBlur";
            public static readonly string GroundDepthSubtracted = "Hidden/Snow/Ground Depth Subtracted Depth Write";
            public static readonly string ScharrLaplacian = "Hidden/FullScreen/ScharrLaplacianFilter";
            public static readonly string DepthRectification = "Hidden/Snow/Depth Rectification";
            public static readonly string UVEdges = "Hidden/FullScreen/UVEdges";
            public static readonly string MagnitudeDilation = "Hidden/FullScreen/MagnitudeDilation";
            public static readonly string TintedLookup = "Hidden/FullScreen/TintedLookup";
            public static readonly string Gradient2D = "Hidden/FullScreen/Gradient2D";
            public static readonly string IndicatorFunction = "Hidden/FullScreen/IndicatorFunction";
            public static readonly string HeightEncodeDecode = "Hidden/Snow/HeightEncodeDecode";
        }

        static class ShaderKeywords
        {
            public static readonly string SimulateVelocity = "SIMULATE_VELOCITY";
        }

        private string GetTopLayerNameWithPrefix(string prefix) => prefix + (_templateLayerCount - 1).ToString();

        private ProfilingSampler _depthBlurSampler;
        private ProfilingSampler _dilationSampler;
        private ProfilingSampler _gradientSampler;

        private static bool IsHeightBasedBlend(Material material)
        {
            return material.HasProperty(ShaderID._UseHeightBasedBlend) && material.GetInt(ShaderID._UseHeightBasedBlend) > 0;
        }

        private bool IsSnowMeshRenderer(MeshRenderer meshRenderer, List<Material> outMaterials)
        {
            bool heightBasedBlend = IsHeightBasedBlend(_templateSnowMaterial);

            meshRenderer.GetSharedMaterials(outMaterials);
            bool isSnow = ((1 << meshRenderer.gameObject.layer) & _groundLayerMask) != 0;//Test layer against ground LayerMask
            if (isSnow)
            {
                isSnow = false;
                string[] templateKeywords = _templateSnowMaterial.shaderKeywords;
                foreach (Material material in outMaterials)
                {
                    if (material.name.ToLowerInvariant().Contains("snow") && material.shader == _templateSnowMaterial.shader && material.shaderKeywords.SequenceEqual(templateKeywords))//NB For now, require same material and keyword set.
                    {
                        isSnow = true;

                        if (heightBasedBlend)//Log warning if main layer could be above bottom snow layer
                        {
                            float materialMainRangeTop = GetHeightRange(material, 0).y;
                            float templateSnowLayerBottom1 = GetHeightRange(_templateSnowMaterial, 1).x;
                            if (materialMainRangeTop > templateSnowLayerBottom1)
                            {
                                Debug.LogWarningFormat("Snow renderer {0}'s material {1} main layer reaches to {2}, which is higher than the bottom of the first snow layer at {3}.", meshRenderer.name, material.name, materialMainRangeTop, templateSnowLayerBottom1);
                            }
                        }
                    }
                }
            }
            if (isSnow)
            {
                const float uvDelta = 0.01f;
                const float uvDeltaSquared = uvDelta * uvDelta;
                const int uvChannel = 0;
                //Ensure objects have world-space-aligned linear UVs
                Bounds bounds = meshRenderer.bounds;
                Vector2 horizontalBoundsMin = new Vector2(bounds.min.x, bounds.min.z);
                Vector2 horizontalBoundsSize = new Vector2(bounds.size.x, bounds.size.z);
                Transform transform = meshRenderer.transform;
                MeshFilter meshFilter = meshRenderer.GetComponent<MeshFilter>();
                Mesh mesh = meshFilter.sharedMesh;
                mesh.GetVertices(_tempVertices);
                mesh.GetUVs(uvChannel, _tempUVs);
                for (int vertexIndex = 0; vertexIndex < mesh.vertexCount; vertexIndex++)
                {
                    Vector3 vertex = _tempVertices[vertexIndex];
                    Vector2 uv = _tempUVs[vertexIndex];
                    Vector3 vertexWorld = transform.TransformPoint(vertex);
                    Vector2 vertexHorizontalFraction = (new Vector2(vertexWorld.x, vertexWorld.z) - horizontalBoundsMin) / horizontalBoundsSize;
                    Vector2 uvDifference = vertexHorizontalFraction - uv;
                    if (uvDifference.sqrMagnitude > uvDeltaSquared)
                    {
                        isSnow = false;
                        Debug.LogWarningFormat("Mesh Renderer {0} is set up to be a snow renderer, but does not have linear world-space horizontal UVs in channel {1}", meshRenderer.name, uvChannel);
                    }
                }
            }
            return isSnow;
        }

        private Vector2 GetHeightRange(Material material, int parameterizationId, int centerId, int amplitudeId, int minId, int maxId, int offsetId)
        {
            Vector2 rangeCm = Vector2.zero;
            if (material.GetFloat(parameterizationId) > 0)
            {
                float centerValue = material.GetFloat(centerId);
                float amplitude = material.GetFloat(amplitudeId);
                float max = (1f - centerValue) * amplitude;
                float min = (0f - centerValue) * amplitude;
                rangeCm = new Vector2(min, max);
            }
            else
            {
                rangeCm = new Vector2(material.GetFloat(minId), material.GetFloat(maxId));
            }

            Vector2 rangeOffsetCm = rangeCm + material.GetFloat(offsetId) * Vector2.one;
            return rangeOffsetCm * 0.01f;
        }

        private Vector2 GetExpandedRange(Vector2 rangeA, Vector2 rangeB)
        {
            return new Vector2(Mathf.Min(rangeA.x, rangeB.x), Mathf.Max(rangeA.y, rangeB.y));
        }

        private Vector2 GetHeightRange(Material material, int layerIndex)
        {
            Vector2 range = Vector2.zero;
            try
            {
                if (layerIndex == 0)
                {
                    range = GetHeightRange(material, ShaderID.HeightRange.Parameterization0, ShaderID.HeightRange.Center0, ShaderID.HeightRange.Amplitude0, ShaderID.HeightRange.Min0, ShaderID.HeightRange.Max0, ShaderID.HeightRange.Offset0);
                }
                if (layerIndex == 1)
                {
                    range = GetHeightRange(material, ShaderID.HeightRange.Parameterization1, ShaderID.HeightRange.Center1, ShaderID.HeightRange.Amplitude1, ShaderID.HeightRange.Min1, ShaderID.HeightRange.Max1, ShaderID.HeightRange.Offset1);
                }
                if (layerIndex == 2)
                {
                    range = GetHeightRange(material, ShaderID.HeightRange.Parameterization2, ShaderID.HeightRange.Center2, ShaderID.HeightRange.Amplitude2, ShaderID.HeightRange.Min2, ShaderID.HeightRange.Max2, ShaderID.HeightRange.Offset2);
                }
                if (layerIndex == 3)
                {
                    range = GetHeightRange(material, ShaderID.HeightRange.Parameterization3, ShaderID.HeightRange.Center3, ShaderID.HeightRange.Amplitude3, ShaderID.HeightRange.Min3, ShaderID.HeightRange.Max3, ShaderID.HeightRange.Offset3);
                }
            }
            catch
            {
                range = Vector2.zero;
            }
            return range;
        }

        private Vector2 GetHeightRange(Material material, out int layerCount)
        {
            Vector2 range = Vector2.zero;
            layerCount = 0;
            try
            {
                layerCount = (int)material.GetFloat(ShaderID._LayerCount);
                if (layerCount > 0)
                {
                    range = GetExpandedRange(range, GetHeightRange(material, 0));
                }
                if (layerCount > 1)
                {
                    range = GetExpandedRange(range, GetHeightRange(material, 1));
                }
                if (layerCount > 2)
                {
                    range = GetExpandedRange(range, GetHeightRange(material, 2));
                }
                if (layerCount > 3)
                {
                    range = GetExpandedRange(range, GetHeightRange(material, 3));
                }
            }
            catch
            {
                range = Vector2.zero;
            }
            return range;
        }

        void GetLayerMaskOffsetAndScaling_AxisAligned(Bounds bounds, out Vector2 offset, out Vector2 scaling)
        {
            Vector2 fullMin = new Vector2(_renderersBounds.min.x, _renderersBounds.min.z);
            Vector2 fullMax = new Vector2(_renderersBounds.max.x, _renderersBounds.max.z);
            Vector2 boundsMin = new Vector2(bounds.min.x, bounds.min.z);
            Vector2 boundsMax = new Vector2(bounds.max.x, bounds.max.z);
            Vector2 fullSize = fullMax - fullMin;
            Vector2 minFrac = (boundsMin - fullMin) / fullSize;
            Vector2 maxFrac = (boundsMax - fullMin) / fullSize;
            Vector2 fracSize = maxFrac - minFrac;

            scaling = fracSize;
            offset = minFrac;
        }

        void GetMaterialScalingOffset_AxisAligned(Bounds bounds, Vector2 initialScaling, Vector2 initialOffset, bool offsetFromRenderersBounds, out Vector2 scaling, out Vector2 offset)
        {
            //If we have a template scale(object size) of R, and the original scaling and offset of the texture is given by j, k, then the texture starts at 0 meters as u0 = k, and ends at R meters as u1 = k + j.
            //Then at the renderer's min and max bounds points a, b (in meters), we'll calculate what the u coordinate would be at these points, which is the interpolating between u0 and u1:
            // u(x) = u0 + x (u1 - u0) / R = k + x j / R, so ua = k + a j / R, and similarly ub = k + b j / R.
            //Note that a and b may be measured from any origin. This may be the world origin, in which case a and b are just the world - space values, but we may also wish to measure relative to the collective bounds' minimum point.
            //If the renderer's texture's unknown final scale and offset are given by S and D, then by definition the u value at a is D, and the u value at b is D + S, assuming its uvs are uniformly distributed in [0,1].
            //Equating these, we have k + aj/R = D, k + bj/R = D + S; substituting D, (b-a)(j/R) = S, and then D = (k + aj / R).

            Vector2 boundsMin = new Vector2(bounds.min.x, bounds.min.z);
            Vector2 boundsMax = new Vector2(bounds.max.x, bounds.max.z);
            Vector2 boundsSize = new Vector2(bounds.size.x, bounds.size.z);

            Vector2 fullMin = new Vector2(_renderersBounds.min.x, _renderersBounds.min.z);
            Vector2 offsetPoint = offsetFromRenderersBounds ? fullMin : Vector2.zero;
            Vector2 minOffset = boundsMin - offsetPoint;

            Vector2 materialScale = _templateSnowMaterialScale == Vector2.zero ? Vector2.one : _templateSnowMaterialScale;//Use default scale of 1 if not set to avoid division by zero.
            scaling = (boundsSize * initialScaling / materialScale);
            offset = (initialOffset + minOffset * initialScaling / materialScale);
        }

        void SetTextureScalingOffsetRelativeToTemplateAndBounds(Bounds bounds, TextureId trextureId)
        {
            Vector2 initialScale = Vector2.one;
            Vector2 initialOffset = Vector2.zero;
            if (_useTemplateMaterialInitialTextureScaleOffets)
            {
                initialScale = _templateSnowMaterial.GetTextureScale(trextureId.Id);
                initialOffset = _templateSnowMaterial.GetTextureOffset(trextureId.Id);
            }
            GetMaterialScalingOffset_AxisAligned(bounds, initialScale, initialOffset, true, out Vector2 scaling, out Vector2 offset);
            _snowMaterialProperties.SetVector(trextureId.Id_ST, new Vector4(scaling.x, scaling.y, offset.x, offset.y));
        }

        private void ValidateSnowLayers()
        {
            if (GetHeightRange(_templateSnowMaterial, 2) != GetHeightRange(_templateSnowMaterial, 1))
            {
                Debug.LogWarning("Middle snow layer should be same height range as bottom one when using three snow layers.");
            }

            if (!SnowMaterialUtil.ValidateSnowLayersProperties(_templateSnowMaterial, _templateLayerCount))
            {
                Debug.LogWarning("Snow layer properties (as in HDRP/LayeredLitTessellation material) do not all exist.");
            }

            if (_templateLayerCount > 3)
            {
                if (!SnowMaterialUtil.ValidateLayers2And3Equal(_templateSnowMaterial))
                {
                    Debug.LogWarning("Middle snow layer properties (except height range) should be equivalent to the top one when using three snow layers.");
                }
            }
        }

        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in an performance manner.
        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            //Since we need to modify Renderers material properties, we should only setup and execute this CustomPass during play mode, not edit mode.
            if (!Application.isPlaying)
            {
                return;
            }

            _depthShaderTags = new ShaderTagId[2]
            {
            new ShaderTagId("DepthOnly"),
            new ShaderTagId("DepthForwardOnly"),
            };

            _motionVectorsShaderTags = new ShaderTagId[1]
            {
            new ShaderTagId("MotionVectors"),
            };

            _falloffAtEdgesValue = _falloffAtEdges;
            _useSmoothedNormalGenerationValue = _useSmoothedNormalGeneration;
            _displacementComputeShaderValue = _displacementComputeShader;
            _simulateVelocityValue = _simulateVelocity;

            _depthBlurSampler = new ProfilingSampler("Depth Blur");
            _dilationSampler = new ProfilingSampler("Motion Dilation");
            _gradientSampler = new ProfilingSampler("Indicator Gradient");

            _hBlurPassId = new ShaderPassId(PassNames.HorizontalBlur, _blurMaterial, 0);
            _vBlurPassId = new ShaderPassId(PassNames.VerticalBlur, _blurMaterial, 1);
            _depthRectificationPassId = new ShaderPassId(PassNames.DepthRectificationHeight, _depthRectificationMaterial, 0);
            _hErosionPassId = new ShaderPassId(PassNames.HorizontalErosion, _dilationMaterial, 4);
            _vErosionPassId = new ShaderPassId(PassNames.VerticalErosion, _dilationMaterial, 5);
            _hDilationPassId = new ShaderPassId(PassNames.HorizontalDilation, _dilationMaterial, 0);
            _vDilationPassId = new ShaderPassId(PassNames.VerticalDilation, _dilationMaterial, 1);
            _hDilationBelowEpsPassId = new ShaderPassId(PassNames.HorizontalDilationBelowEpsilon, _dilationMaterial, 2);
            _vDilationBelowEpsPassId = new ShaderPassId(PassNames.VerticalDilationBelowEpsilon, _dilationMaterial, 3);
            _gradientWithMultiplierPassId = new ShaderPassId(PassNames.GradientWithMultiplier, _gradientMaterial, 1);
            _indicatorFunctionDepthPassId = new ShaderPassId(PassNames.IndicatorFunctionOfDepth, _indicatorMaterial, 1);
            _gradientLengthOffsetMagnitudeSaturatedPassId = new ShaderPassId(PassNames.GradientLengthOffsetMagnitudeSaturated, _scharrMaterial, 4);
            _laplacianPassId = new ShaderPassId(PassNames.Laplacian, _scharrMaterial, 6);
            _edgeDistanceMinPassId = new ShaderPassId(PassNames.EdgeDistanceMin, _uvEdgesMaterial, 1);
            _tintedLookupFloatPassId = new ShaderPassId(PassNames.TintedLookupFloat, _tintedLookupMaterial, 0);
            _tintedLookupMaxFloatPassId = new ShaderPassId(PassNames.TintedLookupMaxFloat, _tintedLookupMaterial, 5);
            _remapBottomToUnitRangePassId = new ShaderPassId(PassNames.RemapBottomToUnitRange, _tintedLookupMaterial, 2);
            _lookupPowerOffsetFloatPassId = new ShaderPassId(PassNames.LookupPowerOffsetFloat, _tintedLookupMaterial, 3);
            _signMappedToUnitRangePassId = new ShaderPassId(PassNames.SignMappedToUnitRange, _tintedLookupMaterial, 4);
            _scharrBlurHorizontalVerticalPassId = new ShaderPassId(PassNames.ScharrBlurHorizontalVertical, _scharrMaterial, 5);
            _detailNormalUpdatePassId = new ShaderPassId(PassNames.DetailNormalUpdate, _detailNormalMaterial, 1);
            _detailNormalUpdateFromBlurredPassId = new ShaderPassId(PassNames.DetailNormalUpdateFromBlurred, _detailNormalMaterial, 2);
            _maskHeightUpdatePassId = new ShaderPassId(PassNames.MaskHeightUpdate, _maskMaterial, 1);
            _maskInterpolatedHeightGreenChannelPassId = new ShaderPassId(PassNames.MaskInterpolatedHeightGreenChannel, _maskMaterial, 2);
            _maskInterpolatedHeightLayerOneTwoFractionsPassId = new ShaderPassId(PassNames.MaskInterpolatedHeightLayerOneTwoFractions, _maskMaterial, 3);
            _maskInterpolatedHeightGreenChannelFromHeightmapPassId = new ShaderPassId(PassNames.MaskInterpolatedHeightGreenChannelFromHeightmap, _maskMaterial, 4);
            _maskInterpolatedHeightLayerOneTwoFractionsFromHeightmapPassId = new ShaderPassId(PassNames.MaskInterpolatedHeightLayerOneTwoFractionsFromHeightmap, _maskMaterial, 5);
            _heightDecodePassId = new ShaderPassId(PassNames.HeightDecode, _heightEncodeDecodeMaterial, 0);

            if (_snowGroundObjectsRoot == null || _templateSnowMaterial == null)
            {
                return;
            }

            _templateSnowRange = GetHeightRange(_templateSnowMaterial, out _templateLayerCount);
            if (_templateLayerCount < 3)
            {
                return;
            }

            ValidateSnowLayers();

            if (_snowMeshRenderers == null)
            {
                _snowMeshRenderers = new List<MeshRenderer>();
            }
            _snowMeshRenderers.Clear();
            if (_tempMeshRendererComponents == null)
            {
                _tempMeshRendererComponents = new List<MeshRenderer>();
            }
            _tempMeshRendererComponents.Clear();
            if (_tempMaterials == null)
            {
                _tempMaterials = new List<Material>();
            }
            _tempMaterials.Clear();
            if (_tempVertices == null)
            {
                _tempVertices = new List<Vector3>();
            }
            _tempVertices.Clear();
            if (_tempUVs == null)
            {
                _tempUVs = new List<Vector2>();
            }
            _tempUVs.Clear();

            Quaternion startRootLocalRotation = _snowGroundObjectsRoot.transform.localRotation;
            _snowGroundObjectsRoot.transform.localRotation = Quaternion.identity;

            _renderersBounds = new Bounds(Vector3.zero, Vector3.zero);
            _snowGroundObjectsRoot.GetComponentsInChildren(_tempMeshRendererComponents);
            foreach (MeshRenderer meshRenderer in _tempMeshRendererComponents)
            {
                if (IsSnowMeshRenderer(meshRenderer, _tempMaterials))
                {
                    _snowMeshRenderers.Add(meshRenderer);

                    if (_renderersBounds.size == Vector3.zero)
                    {
                        _renderersBounds = meshRenderer.bounds;
                    }
                    else
                    {
                        _renderersBounds.Encapsulate(meshRenderer.bounds);
                    }
                }
            }

            if (_snowMeshRenderers.Count == 0)
            {
                return;
            }

            Vector3 boundsSize3D = _renderersBounds.size;
            Vector2 boundsSize2D = new Vector2(boundsSize3D.x, boundsSize3D.z);
            float boundsHorizontalSizeMax = Mathf.Max(boundsSize2D.x, boundsSize2D.y);
            _textureSize = new Vector2Int(Mathf.RoundToInt(boundsSize2D.x * _textureEdgeSize / boundsHorizontalSizeMax), Mathf.RoundToInt(boundsSize2D.y * _textureEdgeSize / boundsHorizontalSizeMax));

            _originalBoundsHeight = boundsSize3D.y;
            _boundsAndSnowHeight = _templateSnowRange.y + _originalBoundsHeight;

            _depthCaptureBuffer = RTHandles.Alloc(_textureSize.x, _textureSize.y, colorFormat: GraphicsFormat.R16_UNorm, depthBufferBits: DepthBits.None, name: "Depth Capture", enableRandomWrite: ApproximateSweptArea);

            GameObject bakingCameraGO = new GameObject("Snow Baking Camera");
            bakingCameraGO.hideFlags = HideFlags.HideAndDontSave;
            _bakingCamera = bakingCameraGO.AddComponent<Camera>();
            _bakingHDCamera = HDCamera.GetOrCreate(_bakingCamera);
            _bakingCamera.enabled = false;
            _bakingCamera.orthographic = true;
            _bakingCamera.aspect = boundsSize2D.x / boundsSize2D.y;
            _bakingCamera.orthographicSize = 0.5f * boundsSize2D.y;//NB This works with aspects ratios less than or greater than 1
            _bakingCamera.transform.eulerAngles = new Vector3(-90f, 180f, 0);
            _bakingCamera.transform.position = new Vector3(_renderersBounds.center.x, _renderersBounds.min.y, _renderersBounds.center.z);
            _bakingCamera.nearClipPlane = 0f;
            _bakingCamera.farClipPlane = _boundsAndSnowHeight;
            _bakingCamera.transform.parent = _snowGroundObjectsRoot.transform;
            _bakingCamera.depthTextureMode |= DepthTextureMode.Depth | DepthTextureMode.MotionVectors;

            if (_maskShaderFullscreen == null)
            {
                _maskShaderFullscreen = Shader.Find(ShaderNames.MaskUpdate);
            }
            _maskMaterial = CoreUtils.CreateEngineMaterial(_maskShaderFullscreen);

            if (_detailNormalShaderFullscreen == null)
            {
                _detailNormalShaderFullscreen = Shader.Find(ShaderNames.DetailNormal);
            }
            _detailNormalMaterial = CoreUtils.CreateEngineMaterial(_detailNormalShaderFullscreen);

            Texture topLayerHeightMap = _templateSnowMaterial.GetTexture(ShaderID.HeightMap.FromLayerCount(_templateLayerCount));
            _maskMaterial.SetTexture(ShaderID._DepthTex, _depthCaptureBuffer);
            _maskMaterial.SetTexture(ShaderID.Mask._TopLayerHeightTex, topLayerHeightMap);
            _maskMaterial.SetTexture(ShaderID.Mask._BottomLayerHeightTex, _templateSnowMaterial.GetTexture(ShaderID.HeightMap._HeightMap1));
            Vector2 templateBottomRange = GetHeightRange(_templateSnowMaterial, 1);
            Vector2 templateTopRange = GetHeightRange(_templateSnowMaterial, _templateLayerCount - 1);
            Vector2 templateTotalRange = GetExpandedRange(Vector2.zero, GetExpandedRange(templateBottomRange, templateTopRange));
            float getFractionalRange(float input) => (input - templateTotalRange.x) / (templateTotalRange.y - templateTotalRange.x);
            Vector4 bottomTopFractionalRanges = new Vector4(getFractionalRange(templateBottomRange.x), getFractionalRange(templateBottomRange.y), getFractionalRange(templateTopRange.x), getFractionalRange(templateTopRange.y));
            _maskMaterial.SetVector(ShaderID.Mask._LayerFractionalRanges, bottomTopFractionalRanges);

            _detailNormalMaterial.SetTexture(ShaderID.Detail._InitialHeightTex, topLayerHeightMap);
            _detailNormalMaterial.SetTexture(ShaderID.Detail._InitialNormalTex, _templateSnowMaterial.GetTexture(ShaderID.NormalMap.FromLayerCount(_templateLayerCount)));
            _detailNormalMaterial.SetFloat(ShaderID.Detail._InitialNormalStrength, _templateSnowMaterial.GetFloat(ShaderID.NormalScale.FromLayerCount(_templateLayerCount)));
            _detailNormalMaterial.SetVector(ShaderID.Detail._Scale, new Vector4(boundsSize2D.x, boundsSize2D.y, _templateSnowRange.y - _templateSnowRange.x, _invertDetailNormalY ? -1f : 1f));

            if (_blurShader == null)
            {
                _blurShader = Shader.Find(ShaderNames.Blur);
            }
            _blurMaterial = CoreUtils.CreateEngineMaterial(_blurShader);
            _hBlurProperties = new MaterialPropertyBlock();
            _vBlurProperties = new MaterialPropertyBlock();

            _depthBakeBuffer = RTHandles.Alloc(_textureSize.x, _textureSize.y, colorFormat: GraphicsFormat.None, depthBufferBits: DepthBits.Depth16, dimension: TextureDimension.Tex2D);
            _blurBuffer = RTHandles.Alloc(_textureSize.x, _textureSize.y, colorFormat: GraphicsFormat.R16_UNorm, dimension: TextureDimension.Tex2D);

            RenderTextureDescriptor groundBakeDescriptor = new RenderTextureDescriptor(_textureSize.x, _textureSize.y, RenderTextureFormat.Depth, 16);
            _groundBakeTexture = new RenderTexture(groundBakeDescriptor);
            if (_groundDepthShader == null)
            {
                _groundDepthShader = Shader.Find(ShaderNames.GroundDepthSubtracted);
            }
            _groundDepthMaterial = CoreUtils.CreateEngineMaterial(_groundDepthShader);
            _groundDepthMaterial.SetTexture(ShaderID._GroundDepthTex, _groundBakeTexture);
            _groundDepthMaterial.SetVector(ShaderID._Ranges, new Vector4(_originalBoundsHeight, _boundsAndSnowHeight, 0f, 0f));

            _litMaterial = CoreUtils.CreateEngineMaterial(Shader.Find(ShaderNames.LitHDRP));
            _litMaterialDepthOnlyPassIndex = _litMaterial.FindPass(_depthShaderTags[0].name);

            if (_falloffAtEdgesValue || _useSmoothedNormalGenerationValue)
            {
                if (_scharrShader == null)
                {
                    _scharrShader = Shader.Find(ShaderNames.ScharrLaplacian);
                }
                _scharrMaterial = CoreUtils.CreateEngineMaterial(_scharrShader);
                _scharrProperties = new MaterialPropertyBlock();
            }

            if (_falloffAtEdgesValue)
            {
                int downsampleFactor = Mathf.RoundToInt(Mathf.Pow(2, _edgeFalloffDownsampleTimes));
                _edgeFalloffTexture = new RenderTexture(new RenderTextureDescriptor(_textureSize.x / downsampleFactor, _textureSize.y / downsampleFactor, GraphicsFormat.R8_UNorm, 0))
                {
                    name = "Edge Falloff"
                };

                if (_depthRectificationShader == null)
                {
                    _depthRectificationShader = Shader.Find(ShaderNames.DepthRectification);
                }
                _depthRectificationMaterial = CoreUtils.CreateEngineMaterial(_depthRectificationShader);
                _depthRectificationProperties = new MaterialPropertyBlock();

                if (_uvEdgesShader == null)
                {
                    _uvEdgesShader = Shader.Find(ShaderNames.UVEdges);
                }
                _uvEdgesMaterial = CoreUtils.CreateEngineMaterial(_uvEdgesShader);
                _uvEdgesProperties = new MaterialPropertyBlock();
            }

            if (CaptureMotionVectors || _falloffAtEdgesValue)
            {
                if (_dilationShader == null)
                {
                    _dilationShader = Shader.Find(ShaderNames.MagnitudeDilation);
                }
                _dilationMaterial = CoreUtils.CreateEngineMaterial(_dilationShader);
                _dilationBuffer = RTHandles.Alloc(_textureSize.x, _textureSize.y, colorFormat: GraphicsFormat.R16G16B16A16_SFloat, dimension: TextureDimension.Tex2D);
                _hDilationProperties = new MaterialPropertyBlock();
                _vDilationProperties = new MaterialPropertyBlock();

                if (_tintedLookupShader == null)
                {
                    _tintedLookupShader = Shader.Find(ShaderNames.TintedLookup);
                }
                _tintedLookupMaterial = CoreUtils.CreateEngineMaterial(_tintedLookupShader);
                _tintedLookupProperties = new MaterialPropertyBlock();
            }

            if (CaptureMotionVectors || _useSmoothedNormalGenerationValue)
            {
                if (_gradientShader == null)
                {
                    _gradientShader = Shader.Find(ShaderNames.Gradient2D);
                }
                _gradientMaterial = CoreUtils.CreateEngineMaterial(_gradientShader);
                _gradientBuffer = RTHandles.Alloc(_textureSize.x, _textureSize.y, colorFormat: GraphicsFormat.R16G16_SFloat, name: "Horizontal Gradient");
                _gradientProperties = new MaterialPropertyBlock();
            }

            if (CaptureMotionVectors)
            {
                _motionVectorsBuffer = RTHandles.Alloc(_textureSize.x, _textureSize.y, colorFormat: GraphicsFormat.R16G16B16A16_SFloat, depthBufferBits: DepthBits.None, dimension: TextureDimension.Tex2D, name: "Motion Vectors", enableRandomWrite: ApproximateSweptArea);

                //NB We won't handle change in targetTexture size here. In case we need to, we should Cleanup() and Setup() the pass from a higher architectural level.

                if (_indicatorFunctionShader == null)
                {
                    _indicatorFunctionShader = Shader.Find(ShaderNames.IndicatorFunction);
                }
                _indicatorMaterial = CoreUtils.CreateEngineMaterial(_indicatorFunctionShader);
                _indicatorProperties = new MaterialPropertyBlock();


                _lowDiscrepancySequence = new LowDiscrepancySequence(30, 3, 5);
            }

            Texture initialMaskHeightTexture = _startingHeightTexture == null ? topLayerHeightMap : _startingHeightTexture;

            Color _detailNormalNeutral = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            RenderTextureDescriptor maskHeightDescriptor = new RenderTextureDescriptor(_textureSize.x, _textureSize.y, RenderTextureFormat.RFloat)//Need R32 format if we wish to be able to load from this as RWTexture in Compute Shader. See https://docs.microsoft.com/en-us/previous-versions//ff471325(v=vs.85)#uav-typed-load
            {
                enableRandomWrite = UpdateMaskHeightFromComputeShader,
            };
            _maskHeightRenderTexture = new RenderTexture(maskHeightDescriptor)
            {
                name = "Mask Height RT"
            };

            if (!CaptureMotionVectors || initialMaskHeightTexture == null)
            {
                CoreUtils.SetRenderTarget(cmd, _maskHeightRenderTexture, clearFlag: ClearFlag.Color, clearColor: Color.white * _startingHeightFraction);
            }
            else
            {
                CoreUtils.SetRenderTarget(cmd, _maskHeightRenderTexture, clearFlag: ClearFlag.All);
                _tintedLookupProperties.Clear();
                _tintedLookupProperties.SetTexture(ShaderID._Source, initialMaskHeightTexture);
                SetTextureSize(_tintedLookupProperties, _maskHeightRenderTexture);
                _tintedLookupProperties.SetFloat(ShaderID._Multiplier, _startingHeightFraction * (bottomTopFractionalRanges.w - bottomTopFractionalRanges.z));//Multiplier is relative to top layer fractional height range
                _tintedLookupProperties.SetFloat(ShaderID._Offset, _startingHeightOffset + bottomTopFractionalRanges.z);//Default offset is relative to top layer minimum fractional height
                CoreUtils.DrawFullScreen(cmd, _tintedLookupMaterial, _tintedLookupProperties, shaderPassId: _tintedLookupFloatPassId);
            }
            if (_useSmoothedNormalGenerationValue)
            {
                _detailNormalMaterial.SetTexture(ShaderID.Detail._HeightTex, _gradientBuffer);
            }
            else
            {
                _detailNormalMaterial.SetTexture(ShaderID.Detail._HeightTex, _maskHeightRenderTexture);
            }
            if (UpdateLayerMaskFromFinalMaskHeight)
            {
                _maskMaterial.SetTexture(ShaderID.Detail._MaskHeightTex, _maskHeightRenderTexture);
            }

            RenderTextureDescriptor layerMaskDescriptor = new RenderTextureDescriptor(_textureSize.x, _textureSize.y, RenderTextureFormat.ARGB32);
            _layerMaskRenderTexture = new RenderTexture(layerMaskDescriptor)
            {
                name = "Layer Mask RT"
            };
            CoreUtils.SetRenderTarget(cmd, _layerMaskRenderTexture, clearFlag: ClearFlag.Color, clearColor: Color.white);

            RenderTextureDescriptor detailNormalDescrptor = new RenderTextureDescriptor(_textureSize.x, _textureSize.y, RenderTextureFormat.ARGB32);
            _detailNormalRenderTexture = new RenderTexture(detailNormalDescrptor)
            {
                name = "Detail Normal RT"
            };
            CoreUtils.SetRenderTarget(cmd, _detailNormalRenderTexture, clearFlag: ClearFlag.Color, clearColor: _detailNormalNeutral);

            _outputRenderTextureProperties = new MaterialPropertyBlock();

            if (UpdateMaskHeightFromComputeShader)
            {
                _heightComputeBufferEncoded = new ComputeBuffer(_textureSize.x * _textureSize.y, sizeof(int));

                _displacementKernelIndex = new ComputeShaderKernelId(KernelNames.SimpleDisplacement, _displacementComputeShaderValue, 0);
                _minimumKernelIndex = new ComputeShaderKernelId(KernelNames.HeightMinimum, _displacementComputeShaderValue, 1);
                _minimumFalloffKernelIndex = new ComputeShaderKernelId(KernelNames.HeightMinimumWithFalloffMask, _displacementComputeShaderValue, 2);
                _heightAboveOneSpreadKernelIndex = new ComputeShaderKernelId(KernelNames.HeightAboveOneSpread, _displacementComputeShaderValue, 3);
                if (SimulateVelocity)
                {
                    _velocityClearKernelIndex = new ComputeShaderKernelId(KernelNames.VelocityClear, _displacementComputeShaderValue, 4);
                    _velocityUpdateKernelIndex = new ComputeShaderKernelId(KernelNames.VelocityUpdateSimple, _displacementComputeShaderValue, 5);
                    _velocityDecodeKernelIndex = new ComputeShaderKernelId(KernelNames.VelocityDecode, _displacementComputeShaderValue, 6);
                }
                _heightEncodeKernelIndex = new ComputeShaderKernelId(KernelNames.HeightEncode, _displacementComputeShaderValue, 7);
                _sweepAreaKernelIndex = new ComputeShaderKernelId(KernelNames.SweepArea, _displacementComputeShaderValue, 8);

                _displacementComputeShaderValue.SetFloat(ShaderID.Compute.ReversedZAmount, SystemInfo.usesReversedZBuffer ? 1f : 0f);
                _displacementComputeShaderValue.SetVector(ShaderID.Compute.TextureSize, GetTextureSizeVector(_textureSize));
                _displacementComputeShaderValue.SetFloat(ShaderID.Compute.TexelsPerMeter, _textureEdgeSize / boundsHorizontalSizeMax);

                if (_displacementKernelIndex >= 0)
                {
                    _displacementComputeShaderValue.SetTexture(_displacementKernelIndex, ShaderID.Compute.MaskHeightTexture, _maskHeightRenderTexture);
                    _displacementComputeShaderValue.SetTexture(_displacementKernelIndex, ShaderID.Compute.CurrentHeightTexture, _depthCaptureBuffer);
                    _displacementComputeShaderValue.SetTexture(_displacementKernelIndex, ShaderID.Compute.CameraMotionVectorsTexture, _motionVectorsBuffer);
                    _displacementComputeShaderValue.SetTexture(_displacementKernelIndex, ShaderID.Compute.HorizontalGradientTexture, _gradientBuffer);
                    _displacementComputeShaderValue.SetBuffer(_displacementKernelIndex, ShaderID.Compute.HeightComputeBufferEncodedReadWrite, _heightComputeBufferEncoded);
                }
                if (_minimumKernelIndex >= 0)
                {
                    _displacementComputeShaderValue.SetTexture(_minimumKernelIndex, ShaderID.Compute.CurrentHeightTexture, _depthCaptureBuffer);
                    _displacementComputeShaderValue.SetBuffer(_minimumKernelIndex, ShaderID.Compute.HeightComputeBufferEncodedReadWrite, _heightComputeBufferEncoded);
                }
                if (_minimumFalloffKernelIndex >= 0 && _edgeFalloffTexture != null)
                {
                    _displacementComputeShaderValue.SetTexture(_minimumFalloffKernelIndex, ShaderID.Compute.CurrentHeightTexture, _depthCaptureBuffer);
                    _displacementComputeShaderValue.SetTexture(_minimumFalloffKernelIndex, ShaderID.Compute.FalloffMaskTexture, _edgeFalloffTexture);
                    _displacementComputeShaderValue.SetBuffer(_minimumFalloffKernelIndex, ShaderID.Compute.HeightComputeBufferEncodedReadWrite, _heightComputeBufferEncoded);
                }
                if (_heightAboveOneSpreadKernelIndex >= 0)
                {
                    _displacementComputeShaderValue.SetBuffer(_heightAboveOneSpreadKernelIndex, ShaderID.Compute.HeightComputeBufferEncodedReadWrite, _heightComputeBufferEncoded);
                    _displacementComputeShaderValue.SetTexture(_heightAboveOneSpreadKernelIndex, ShaderID.Compute.MaskHeightTexture, _maskHeightRenderTexture);
                }
                if (_heightEncodeKernelIndex >= 0)
                {
                    _displacementComputeShaderValue.SetTexture(_heightEncodeKernelIndex, ShaderID.Compute.MaskHeightTexture, _maskHeightRenderTexture);
                    _displacementComputeShaderValue.SetBuffer(_heightEncodeKernelIndex, ShaderID.Compute.HeightComputeBufferEncodedReadWrite, _heightComputeBufferEncoded);
                }


                if (_heightEncodeDecodeShader == null)
                {
                    _heightEncodeDecodeShader = Shader.Find(ShaderNames.HeightEncodeDecode);
                }
                _heightEncodeDecodeMaterial = new Material(_heightEncodeDecodeShader);
                _heightEncodeDecodeMaterial.SetBuffer(ShaderID.Compute.HeightComputeBufferEncodedReadWrite, _heightComputeBufferEncoded);
                _heightEncodeDecodeProperties = new MaterialPropertyBlock();
            }
            if(ApproximateSweptArea)
            {
                if (_sweepAreaKernelIndex >= 0)
                {
                    _displacementComputeShaderValue.SetTexture(_sweepAreaKernelIndex, ShaderID.Compute.CurrentHeightTextureRW, _depthCaptureBuffer);
                    _displacementComputeShaderValue.SetTexture(_sweepAreaKernelIndex, ShaderID.Compute.CameraMotionVectorsTextureRW, _motionVectorsBuffer);
                    _displacementComputeShaderValue.SetTexture(_sweepAreaKernelIndex, ShaderID.Compute.CameraMotionVectorsTexture, _dilationBuffer);//Cannot load from float4 RWTexture2D, only write, so we will load from here.
                }
            }

            if (SimulateVelocity)
            {
                _displacementComputeShaderValue.EnableKeyword(ShaderKeywords.SimulateVelocity);

                const int velocityComputeBufferIntStride = sizeof(int) * 3;//NB This should match the size of the VelocityDataEncoded struct.
                const int velocityComputeBufferFloatStride = sizeof(float) * 3;//NB This should match the size of the VelocityData struct.
                int velocityComputeBufferSize = _textureSize.x * _textureSize.y;
                _velocityComputeBufferEncoded = new ComputeBuffer(velocityComputeBufferSize, velocityComputeBufferIntStride);
                _velocityComputeBufferFloat = new ComputeBuffer(velocityComputeBufferSize, velocityComputeBufferFloatStride);
                if (_displacementKernelIndex >= 0)
                {
                    _displacementComputeShaderValue.SetBuffer(_displacementKernelIndex, ShaderID.Compute.VelocityComputeBufferEncodedReadWrite, _velocityComputeBufferEncoded);
                }
                if (_minimumKernelIndex >= 0)
                {
                    _displacementComputeShaderValue.SetBuffer(_minimumKernelIndex, ShaderID.Compute.VelocityComputeBufferEncodedReadWrite, _velocityComputeBufferEncoded);
                }
                if (_minimumFalloffKernelIndex >= 0 && _edgeFalloffTexture != null)
                {
                    _displacementComputeShaderValue.SetBuffer(_minimumFalloffKernelIndex, ShaderID.Compute.VelocityComputeBufferEncodedReadWrite, _velocityComputeBufferEncoded);
                }
                if(_heightAboveOneSpreadKernelIndex >= 0)
                {
                    _displacementComputeShaderValue.SetBuffer(_heightAboveOneSpreadKernelIndex, ShaderID.Compute.VelocityComputeBufferEncodedReadWrite, _velocityComputeBufferEncoded);
                }
                if (_velocityUpdateKernelIndex >= 0)
                {
                    _displacementComputeShaderValue.SetTexture(_velocityUpdateKernelIndex, ShaderID.Compute.MaskHeightTexture, _maskHeightRenderTexture);
                    _displacementComputeShaderValue.SetBuffer(_velocityUpdateKernelIndex, ShaderID.Compute.VelocityComputeBufferFloatRead, _velocityComputeBufferFloat);
                    _displacementComputeShaderValue.SetBuffer(_velocityUpdateKernelIndex, ShaderID.Compute.VelocityComputeBufferEncodedReadWrite, _velocityComputeBufferEncoded);
                    _displacementComputeShaderValue.SetBuffer(_velocityUpdateKernelIndex, ShaderID.Compute.HeightComputeBufferEncodedReadWrite, _heightComputeBufferEncoded);
                }
                if (_velocityClearKernelIndex >= 0)
                {
                    _displacementComputeShaderValue.SetBuffer(_velocityClearKernelIndex, ShaderID.Compute.VelocityComputeBufferEncodedReadWrite, _velocityComputeBufferEncoded);
                }
                if (_velocityDecodeKernelIndex >= 0)
                {
                    _displacementComputeShaderValue.SetBuffer(_velocityDecodeKernelIndex, ShaderID.Compute.VelocityComputeBufferEncodedRead, _velocityComputeBufferEncoded);
                    _displacementComputeShaderValue.SetBuffer(_velocityDecodeKernelIndex, ShaderID.Compute.VelocityComputeBufferFloatReadWrite, _velocityComputeBufferFloat);
                }
            }
            else if (_displacementComputeShaderValue != null)
            {
                _displacementComputeShaderValue.DisableKeyword(ShaderKeywords.SimulateVelocity);
            }

            _snowMaterialProperties = new MaterialPropertyBlock();
            SnowMaterialUtil.CopySnowLayersProperties(_templateSnowMaterial, _templateLayerCount, _snowMaterialProperties);
            _snowMaterialProperties.SetTexture(SnowTextureId._LayerMaskMap, _layerMaskRenderTexture);
            _snowMaterialProperties.SetTexture(SnowTextureId._DetailMap1, _detailNormalRenderTexture);
            _snowMaterialProperties.SetTexture(SnowTextureId._DetailMap2, _detailNormalRenderTexture);
            if (_templateLayerCount == 4)
            {
                _snowMaterialProperties.SetTexture(SnowTextureId._DetailMap3, _detailNormalRenderTexture);
            }
            foreach (MeshRenderer snowRenderer in _snowMeshRenderers)
            {
                SetRendererSnowLayersTextureScaleOffsets(snowRenderer);
            }

            _snowGroundObjectsRoot.transform.localRotation = startRootLocalRotation;
            _bakingCamera.transform.parent = null;

            _performedGroundBake = false;
            _performedHeightEncoding = false;
        }

        private void SetRendererSnowLayersTextureScaleOffsets(MeshRenderer snowRenderer)
        {
            Bounds snowRendererBounds = snowRenderer.bounds;
            GetLayerMaskOffsetAndScaling_AxisAligned(snowRendererBounds, out Vector2 layerMaskOffset, out Vector2 layerMaskScaling);
            Vector4 layerMaskScalingAndOffset = new Vector4(layerMaskScaling.x, layerMaskScaling.y, layerMaskOffset.x, layerMaskOffset.y);
            _snowMaterialProperties.SetVector(SnowTextureId._LayerMaskMap.Id_ST, layerMaskScalingAndOffset);
            _snowMaterialProperties.SetVector(SnowTextureId._DetailMap1.Id_ST, layerMaskScalingAndOffset);
            _snowMaterialProperties.SetVector(SnowTextureId._DetailMap2.Id_ST, layerMaskScalingAndOffset);

            SetTextureScalingOffsetRelativeToTemplateAndBounds(snowRendererBounds, SnowTextureId._BaseColorMap1);
            SetTextureScalingOffsetRelativeToTemplateAndBounds(snowRendererBounds, SnowTextureId._MaskMap1);
            SetTextureScalingOffsetRelativeToTemplateAndBounds(snowRendererBounds, SnowTextureId._NormalMap1);
            SetTextureScalingOffsetRelativeToTemplateAndBounds(snowRendererBounds, SnowTextureId._NormalMapOS1);
            SetTextureScalingOffsetRelativeToTemplateAndBounds(snowRendererBounds, SnowTextureId._BentNormalMap1);
            SetTextureScalingOffsetRelativeToTemplateAndBounds(snowRendererBounds, SnowTextureId._BentNormalMapOS1);
            SetTextureScalingOffsetRelativeToTemplateAndBounds(snowRendererBounds, SnowTextureId._HeightMap1);
            SetTextureScalingOffsetRelativeToTemplateAndBounds(snowRendererBounds, SnowTextureId._SubsurfaceMaskMap1);
            SetTextureScalingOffsetRelativeToTemplateAndBounds(snowRendererBounds, SnowTextureId._ThicknessMap1);
            SetTextureScalingOffsetRelativeToTemplateAndBounds(snowRendererBounds, SnowTextureId._BaseColorMap2);
            SetTextureScalingOffsetRelativeToTemplateAndBounds(snowRendererBounds, SnowTextureId._MaskMap2);
            SetTextureScalingOffsetRelativeToTemplateAndBounds(snowRendererBounds, SnowTextureId._NormalMap2);
            SetTextureScalingOffsetRelativeToTemplateAndBounds(snowRendererBounds, SnowTextureId._NormalMapOS2);
            SetTextureScalingOffsetRelativeToTemplateAndBounds(snowRendererBounds, SnowTextureId._BentNormalMap2);
            SetTextureScalingOffsetRelativeToTemplateAndBounds(snowRendererBounds, SnowTextureId._BentNormalMapOS2);
            SetTextureScalingOffsetRelativeToTemplateAndBounds(snowRendererBounds, SnowTextureId._HeightMap2);
            SetTextureScalingOffsetRelativeToTemplateAndBounds(snowRendererBounds, SnowTextureId._SubsurfaceMaskMap2);
            SetTextureScalingOffsetRelativeToTemplateAndBounds(snowRendererBounds, SnowTextureId._ThicknessMap2);
            if (_templateLayerCount == 4)
            {
                _snowMaterialProperties.SetVector(SnowTextureId._DetailMap3.Id_ST, layerMaskScalingAndOffset);

                SetTextureScalingOffsetRelativeToTemplateAndBounds(snowRendererBounds, SnowTextureId._BaseColorMap3);
                SetTextureScalingOffsetRelativeToTemplateAndBounds(snowRendererBounds, SnowTextureId._MaskMap3);
                SetTextureScalingOffsetRelativeToTemplateAndBounds(snowRendererBounds, SnowTextureId._NormalMap3);
                SetTextureScalingOffsetRelativeToTemplateAndBounds(snowRendererBounds, SnowTextureId._NormalMapOS3);
                SetTextureScalingOffsetRelativeToTemplateAndBounds(snowRendererBounds, SnowTextureId._BentNormalMap3);
                SetTextureScalingOffsetRelativeToTemplateAndBounds(snowRendererBounds, SnowTextureId._BentNormalMapOS3);
                SetTextureScalingOffsetRelativeToTemplateAndBounds(snowRendererBounds, SnowTextureId._HeightMap3);
                SetTextureScalingOffsetRelativeToTemplateAndBounds(snowRendererBounds, SnowTextureId._SubsurfaceMaskMap3);
                SetTextureScalingOffsetRelativeToTemplateAndBounds(snowRendererBounds, SnowTextureId._ThicknessMap3);
            }
            snowRenderer.SetPropertyBlock(_snowMaterialProperties);
        }

        private float GetBlurredGradientMaximum(float blurRadiusBase)
        {
            //Given the coefficients in "Full Texture Blur.shader", the maximum possible gradient (after blur) is on either side of a step function. Here, R is _Radius in FullTextureBlur.
            //For the px at the high edge of the step, the taps are included with prefix of saturate(1-D), where D is the number of pixels away from center.
            //Two px away, the taps are included with prefix of saturate(-D-1), where D is the number of pixels away from center.
            //So the difference calculated by the gradient is 0.27343750/(2) = 0.13671875, since we'll choose to calculate the gradient in units of px.
            float positiveValue = 0f, negativeValue = 0f;
            float[] coefficients = new float[] { 0.27343750f, 0.21875000f, 0.10937500f, 0.03125000f, 0.00390625f };
            for (int offsetIndex = -4; offsetIndex <= 4; offsetIndex++)
            {
                float coefficient = coefficients[Mathf.Abs(offsetIndex)];
                float offset = offsetIndex * blurRadiusBase;
                positiveValue += coefficient * Mathf.Clamp01(1 - offset);
                negativeValue += coefficient * Mathf.Clamp01(-offset - 1);
            }
            return (positiveValue - negativeValue) / 2;
        }

        void SetTextureSize(MaterialPropertyBlock block, Texture target)
        {
            Vector4 textureSizeVector = GetTextureSizeVector(target);
            block.SetVector(ShaderID._TextureSize, textureSizeVector);
        }

        private static Vector4 GetTextureSizeVector(Texture target)
        {
            Vector2Int scaledViewportSize = new Vector2Int(target.width, target.height);
            return GetTextureSizeVector(scaledViewportSize);
        }

        private static Vector4 GetTextureSizeVector(Vector2Int size)
        {
            return new Vector4(size.x, size.y, 1.0f / size.x, 1.0f / size.y);
        }

        protected virtual bool ValidateBakingHDCameraForExecute(HDCamera bakingHDCamera)
        {
            //Validate _bakingHDCamera as needed.
            return true;
        }

        private bool DispatchComputeDisplacement(CustomPassContext ctx, int kernelIndex)
        {
            bool dispatched = false;
            if (kernelIndex >= 0)
            {
                _displacementComputeShaderValue.GetKernelThreadGroupSizes(kernelIndex, out uint xThreadSize, out uint yThreadSize, out uint zThreadSize);
                ctx.cmd.DispatchCompute(_displacementComputeShaderValue, kernelIndex, Mathf.CeilToInt(_textureSize.x / (float)xThreadSize), Mathf.CeilToInt(_textureSize.y / (float)yThreadSize), 1);
                dispatched = true;
            }
            return dispatched;
        }

        protected override void Execute(CustomPassContext ctx)
        {
            if (_depthCaptureBuffer == null || _bakingCamera == null || !_render || ctx.hdCamera == _bakingHDCamera || !ValidateBakingHDCameraForExecute(_bakingHDCamera))
            {
                return;
            }

            if(!Application.isPlaying)
            {
                return;
            }

            float blurRadiusBase = _blurRadius / 4.0f;//Divide by four, since we have four taps on either side
                                                      //Note that CustomPassUtils.GaussianBlur is an option in HDRP 10, although we'd need to wrap RenderTextures in RTHandles. Since our own implementation already exists and we define the exact blur kernel we're using, we'll continue using it.

            _bakingCamera.TryGetCullingParameters(out ScriptableCullingParameters cullingParams);
            cullingParams.cullingOptions = CullingOptions.ShadowCasters;
            cullingParams.cullingMask = (uint)_layerMask.value;
            CullingResults cullingResult = ctx.renderContext.Cull(ref cullingParams);

            var heightRendererListDesc = new RendererListDesc(_depthShaderTags, cullingResult, _bakingCamera)
            {
                rendererConfiguration = PerObjectData.MotionVectors,
                renderQueueRange = RenderQueueRange.all,
                sortingCriteria = SortingCriteria.BackToFront,
                excludeObjectMotionVectors = false,
                stateBlock = new RenderStateBlock(RenderStateMask.Raster) { rasterState = new RasterState(depthClip: false) },
                overrideMaterial = _groundDepthMaterial,
            };

            var p = GL.GetGPUProjectionMatrix(_bakingCamera.projectionMatrix, true);
            Matrix4x4 scaleMatrix = Matrix4x4.identity;
            scaleMatrix.m22 = -1.0f;
            var v = scaleMatrix * _bakingCamera.transform.localToWorldMatrix.inverse;
            var vp = p * v;
            ctx.cmd.SetGlobalMatrix(ShaderID.unity_MatrixVP, vp);//Our "Regular" (non-HDRP) depth write shader needs this matrix name definition for the vertex shader. The other matrices are overwritten by the HDRP monolithic camera command buffer, so we don't need those.

            _bakingCamera.TryGetCullingParameters(out ScriptableCullingParameters cullingParamsGround);
            cullingParamsGround.cullingOptions = CullingOptions.ShadowCasters;
            cullingParamsGround.cullingMask = (uint)_groundLayerMask.value;
            CullingResults cullingResultGround = ctx.renderContext.Cull(ref cullingParamsGround);

            RenderStateBlock groundRendererStateBlock = new RenderStateBlock(RenderStateMask.Raster | RenderStateMask.Depth) { rasterState = new RasterState(cullingMode: CullMode.Off), depthState = new DepthState(compareFunction: CompareFunction.GreaterEqual) };
            RendererListDesc groundRendererListDesc = new RendererListDesc(_depthShaderTags, cullingResultGround, _bakingCamera)
            {
                rendererConfiguration = PerObjectData.None,
                renderQueueRange = RenderQueueRange.all,
                sortingCriteria = SortingCriteria.BackToFront,
                excludeObjectMotionVectors = false,
                stateBlock = groundRendererStateBlock,
                overrideMaterial = _litMaterial,
                overrideMaterialPassIndex = _litMaterialDepthOnlyPassIndex,
            };

            if (!_performedGroundBake)
            {
                //Update the permanent RenderTexture only when needed
                CoreUtils.SetRenderTarget(ctx.cmd, _groundBakeTexture, ClearFlag.Depth);
                ctx.cmd.ClearRenderTarget(true, false, Color.white, 0f);
                using (new HDRenderPipeline.OverrideCameraRendering(ctx.cmd, _bakingCamera, false))
                {
                    CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, RendererList.Create(groundRendererListDesc));
                }
                _performedGroundBake = true;

                if (_falloffAtEdgesValue)
                {
                    float falloffScaleFinal = -_falloffScale;
                    float falloffOffsetFinal = 1 + _falloffOffset;
                    bool blurBeforeScharr = false;//Note that performing blur before the Scharr filter tends to lead to falloff that consistently doesn't reach zero height.
                    float laplacianBlurRadiusBase = _falloffLaplacianBlurRadius / 4.0f;

                    //Temporarily use _depthCaptureTexture (not yet rendered to in this Execute function) to store depth rectification results (handle flipped x, and possible reversed depth).
                    //Use _blurBuffer as another temporary buffer of the appropriate format.
                    _depthRectificationProperties.SetTexture(ShaderID._DepthTex, _groundBakeTexture);
                    SetTextureSize(_depthRectificationProperties, _depthCaptureBuffer);
                    CoreUtils.DrawFullScreen(ctx.cmd, _depthRectificationMaterial, _depthCaptureBuffer, _depthRectificationProperties, shaderPassId: _depthRectificationPassId);

                    uint numberOfTempRTHandles = System.Math.Max(1, _edgeFalloffDownsampleTimes) + 1;
                    RTHandle[] tempRTHandles = new RTHandle[numberOfTempRTHandles];

                    int downsampleFactor = 1;
                    for (int rtHandleIndex = 0; rtHandleIndex < numberOfTempRTHandles; rtHandleIndex++)
                    {
                        downsampleFactor = Mathf.RoundToInt(Mathf.Pow(2, System.Math.Min(rtHandleIndex + 1, _edgeFalloffDownsampleTimes)));
                        tempRTHandles[rtHandleIndex] = RTHandles.Alloc(_textureSize.x / downsampleFactor, _textureSize.y / downsampleFactor, colorFormat: GraphicsFormat.R16_UNorm, dimension: TextureDimension.Tex2D);
                        ctx.cmd.Blit(rtHandleIndex == 0 ? _depthCaptureBuffer : tempRTHandles[rtHandleIndex - 1], tempRTHandles[rtHandleIndex]);
                    }
                    RTHandle downsampledABuffer = tempRTHandles[numberOfTempRTHandles - 2];
                    RTHandle downsampledBBuffer = tempRTHandles[numberOfTempRTHandles - 1];
                    RTHandle laplacianBuffer = RTHandles.Alloc(_textureSize.x / downsampleFactor, _textureSize.y / downsampleFactor, colorFormat: GraphicsFormat.R16_SFloat, dimension: TextureDimension.Tex2D);

                    //Blur into downsampledB
                    if (blurBeforeScharr)
                    {
                        _hBlurProperties.Clear();
                        _hBlurProperties.SetFloat(ShaderID._Radius, blurRadiusBase);
                        _hBlurProperties.SetTexture(ShaderID._Source, downsampledBBuffer);
                        SetTextureSize(_hBlurProperties, _edgeFalloffTexture);
                        CoreUtils.DrawFullScreen(ctx.cmd, _blurMaterial, _edgeFalloffTexture, _hBlurProperties, shaderPassId: _hBlurPassId);
                        _vBlurProperties.Clear();
                        _vBlurProperties.SetFloat(ShaderID._Radius, blurRadiusBase);
                        _vBlurProperties.SetTexture(ShaderID._Source, _edgeFalloffTexture);
                        SetTextureSize(_vBlurProperties, downsampledBBuffer);
                        CoreUtils.DrawFullScreen(ctx.cmd, _blurMaterial, downsampledBBuffer, _vBlurProperties, shaderPassId: _vBlurPassId);
                    }

                    //Find gradient length with scale, offset in _edgeFalloffTexture
                    _scharrProperties.Clear();
                    _scharrProperties.SetTexture(ShaderID._Source, downsampledBBuffer);
                    _scharrProperties.SetVector(ShaderID.Filtering._ScaleXOffsetXScaleYOffsetY, new Vector4(falloffScaleFinal, falloffOffsetFinal, falloffScaleFinal, falloffOffsetFinal));
                    SetTextureSize(_scharrProperties, _edgeFalloffTexture);
                    CoreUtils.DrawFullScreen(ctx.cmd, _scharrMaterial, _edgeFalloffTexture, _scharrProperties, shaderPassId: _gradientLengthOffsetMagnitudeSaturatedPassId);

                    //Blur for Laplacian into downsampledB
                    ctx.cmd.Blit(downsampledABuffer, downsampledBBuffer);
                    float totalAmountOfBlur = 0;
                    while (totalAmountOfBlur < laplacianBlurRadiusBase)
                    {
                        float blurAmount = Mathf.Min(1, laplacianBlurRadiusBase - totalAmountOfBlur);
                        _hBlurProperties.Clear();
                        _hBlurProperties.SetFloat(ShaderID._Radius, blurAmount);
                        _hBlurProperties.SetTexture(ShaderID._Source, downsampledBBuffer);
                        SetTextureSize(_hBlurProperties, downsampledABuffer);
                        CoreUtils.DrawFullScreen(ctx.cmd, _blurMaterial, downsampledABuffer, _hBlurProperties, shaderPassId: _hBlurPassId);
                        _vBlurProperties.Clear();
                        _vBlurProperties.SetFloat(ShaderID._Radius, blurAmount);
                        _vBlurProperties.SetTexture(ShaderID._Source, downsampledABuffer);
                        SetTextureSize(_vBlurProperties, downsampledBBuffer);
                        CoreUtils.DrawFullScreen(ctx.cmd, _blurMaterial, downsampledBBuffer, _vBlurProperties, shaderPassId: _vBlurPassId);
                        totalAmountOfBlur += blurAmount;
                    }

                    //Get Laplacian.
                    _scharrProperties.Clear();
                    _scharrProperties.SetTexture(ShaderID._Source, downsampledBBuffer);
                    SetTextureSize(_scharrProperties, laplacianBuffer);
                    CoreUtils.DrawFullScreen(ctx.cmd, _scharrMaterial, laplacianBuffer, _scharrProperties, shaderPassId: _laplacianPassId);

                    //Blur _edgeFalloffTexture gradient results
                    _hBlurProperties.Clear();
                    _hBlurProperties.SetFloat(ShaderID._Radius, blurRadiusBase);
                    _hBlurProperties.SetTexture(ShaderID._Source, _edgeFalloffTexture);
                    SetTextureSize(_hBlurProperties, downsampledABuffer);
                    CoreUtils.DrawFullScreen(ctx.cmd, _blurMaterial, downsampledABuffer, _hBlurProperties, _hBlurPassId);
                    _vBlurProperties.Clear();
                    _vBlurProperties.SetFloat(ShaderID._Radius, blurRadiusBase);
                    _vBlurProperties.SetTexture(ShaderID._Source, downsampledABuffer);
                    SetTextureSize(_vBlurProperties, _edgeFalloffTexture);
                    CoreUtils.DrawFullScreen(ctx.cmd, _blurMaterial, _edgeFalloffTexture, _vBlurProperties, _vBlurPassId);

                    //Locally perform erosion to get locally minimum value, then remap from [local_minimum, 1] to [0, 1] at each pixel, into downsampledB.
                    ctx.cmd.Blit(_edgeFalloffTexture, downsampledBBuffer);
                    int numDilations = Mathf.CeilToInt(_blurRadius);
                    for (int dilationIndex = 0; dilationIndex < numDilations; dilationIndex++)
                    {
                        _hDilationProperties.Clear();
                        _hDilationProperties.SetTexture(ShaderID._Source, downsampledBBuffer);
                        SetTextureSize(_hDilationProperties, downsampledABuffer);
                        CoreUtils.DrawFullScreen(ctx.cmd, _dilationMaterial, downsampledABuffer, _hDilationProperties, shaderPassId: _hErosionPassId);
                        _vDilationProperties.Clear();
                        _vDilationProperties.SetTexture(ShaderID._Source, downsampledABuffer);
                        SetTextureSize(_vDilationProperties, downsampledBBuffer);
                        CoreUtils.DrawFullScreen(ctx.cmd, _dilationMaterial, downsampledBBuffer, _vDilationProperties, shaderPassId: _vErosionPassId);
                    }

                    //Remap blurred gradient length texture using its local minimum values (obtained from erosion)
                    ctx.cmd.Blit(_edgeFalloffTexture, downsampledABuffer);
                    _tintedLookupProperties.Clear();
                    _tintedLookupProperties.SetFloat(ShaderID._Multiplier, 1);
                    _tintedLookupProperties.SetFloat(ShaderID._Offset, 0);
                    _tintedLookupProperties.SetTexture(ShaderID.Filtering._RemapSource, downsampledBBuffer);
                    _tintedLookupProperties.SetTexture(ShaderID._Source, downsampledABuffer);
                    SetTextureSize(_tintedLookupProperties, _edgeFalloffTexture);
                    CoreUtils.DrawFullScreen(ctx.cmd, _tintedLookupMaterial, _edgeFalloffTexture, _tintedLookupProperties, shaderPassId: _remapBottomToUnitRangePassId);

                    //Take final power of edge falloff texture
                    ctx.cmd.Blit(_edgeFalloffTexture, downsampledABuffer);
                    _tintedLookupProperties.SetFloat(ShaderID._Multiplier, 0.5f);
                    CoreUtils.DrawFullScreen(ctx.cmd, _tintedLookupMaterial, _edgeFalloffTexture, _tintedLookupProperties, shaderPassId: _lookupPowerOffsetFloatPassId);

                    //Assign sign of laplacian, mapped from [-1, 1] to [0, 1] to downsampledB
                    _tintedLookupProperties.Clear();
                    _tintedLookupProperties.SetTexture(ShaderID._Source, laplacianBuffer);
                    SetTextureSize(_tintedLookupProperties, downsampledBBuffer);
                    CoreUtils.DrawFullScreen(ctx.cmd, _tintedLookupMaterial, downsampledBBuffer, _tintedLookupProperties, shaderPassId: _signMappedToUnitRangePassId);

                    //Erode sign of laplacian by 1 px
                    numDilations = 1;
                    for (int dilationIndex = 0; dilationIndex < numDilations; dilationIndex++)
                    {
                        _hDilationProperties.Clear();
                        _hDilationProperties.SetTexture(ShaderID._Source, downsampledBBuffer);
                        SetTextureSize(_hDilationProperties, downsampledABuffer);
                        CoreUtils.DrawFullScreen(ctx.cmd, _dilationMaterial, downsampledABuffer, _hDilationProperties, shaderPassId: _hErosionPassId);
                        _vDilationProperties.Clear();
                        _vDilationProperties.SetTexture(ShaderID._Source, downsampledABuffer);
                        SetTextureSize(_vDilationProperties, downsampledBBuffer);
                        CoreUtils.DrawFullScreen(ctx.cmd, _dilationMaterial, downsampledBBuffer, _vDilationProperties, shaderPassId: _vErosionPassId);
                    }

                    //Use Max BlendOp to combine remapped sign of laplacian with edge falloff texture
                    _tintedLookupProperties.Clear();
                    _tintedLookupProperties.SetFloat(ShaderID._Multiplier, 1);
                    _tintedLookupProperties.SetFloat(ShaderID._Offset, 0);
                    _tintedLookupProperties.SetTexture(ShaderID._Source, downsampledBBuffer);
                    SetTextureSize(_tintedLookupProperties, _edgeFalloffTexture);
                    CoreUtils.DrawFullScreen(ctx.cmd, _tintedLookupMaterial, _edgeFalloffTexture, _tintedLookupProperties, shaderPassId: _tintedLookupMaxFloatPassId);

                    //Take minimum of edge falloff texture with uv edge falloff
                    _uvEdgesProperties.Clear();
                    _uvEdgesProperties.SetFloat(ShaderID._Multiplier, 1f / _edgeFalloffRadius);
                    SetTextureSize(_uvEdgesProperties, _edgeFalloffTexture);
                    CoreUtils.DrawFullScreen(ctx.cmd, _uvEdgesMaterial, _edgeFalloffTexture, _uvEdgesProperties, shaderPassId: _edgeDistanceMinPassId);

                    for (int tempHandleIndex = 0; tempHandleIndex < tempRTHandles.Length; tempHandleIndex++)
                    {
                        tempRTHandles[tempHandleIndex].Release();
                    }
                }
            }

            CoreUtils.SetRenderTarget(ctx.cmd, _depthBakeBuffer, ClearFlag.Depth);
            using (new HDRenderPipeline.OverrideCameraRendering(ctx.cmd, _bakingCamera, false))
            {
                CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, RendererList.Create(heightRendererListDesc));
            }

            if (CaptureMotionVectors)
            {
                var motionVectorRendererListDesc = new RendererListDesc(_motionVectorsShaderTags, cullingResult, _bakingCamera)
                {
                    rendererConfiguration = PerObjectData.MotionVectors,
                    renderQueueRange = RenderQueueRange.all,
                    sortingCriteria = SortingCriteria.BackToFront,
                    excludeObjectMotionVectors = false,
                    stateBlock = new RenderStateBlock(RenderStateMask.Raster) { rasterState = new RasterState(depthClip: false) },
                };

                CoreUtils.SetRenderTarget(ctx.cmd, _motionVectorsBuffer, ClearFlag.All);
                RendererList motionVectorsRendererList = RendererList.Create(motionVectorRendererListDesc);
                using (new HDRenderPipeline.OverrideCameraRendering(ctx.cmd, _bakingCamera, false))
                {
                    CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, motionVectorsRendererList);
                }

                using (new ProfilingScope(ctx.cmd, _dilationSampler))
                {
                    const bool dilateOnlyIfBelowEpsilon = true;
                    const float _dilationEpsilon = 1e-6f;
                    int numDilations = Mathf.CeilToInt(_blurRadius);
                    for (int dilationIndex = 0; dilationIndex < numDilations; dilationIndex++)
                    {
                        _hDilationProperties.SetTexture(ShaderID._Source, _motionVectorsBuffer);
                        _hDilationProperties.SetFloat(ShaderID._Epsilon, _dilationEpsilon);
                        SetTextureSize(_hDilationProperties, _dilationBuffer);
                        int dilationPassId = dilateOnlyIfBelowEpsilon ? _hDilationBelowEpsPassId : _hDilationPassId;
                        CoreUtils.DrawFullScreen(ctx.cmd, _dilationMaterial, _dilationBuffer, _hDilationProperties, shaderPassId: dilationPassId);
                        _vDilationProperties.SetTexture(ShaderID._Source, _dilationBuffer);
                        _vDilationProperties.SetFloat(ShaderID._Epsilon, _dilationEpsilon);
                        SetTextureSize(_vDilationProperties, _motionVectorsBuffer);
                        dilationPassId = dilateOnlyIfBelowEpsilon ? _vDilationBelowEpsPassId : _vDilationPassId;
                        CoreUtils.DrawFullScreen(ctx.cmd, _dilationMaterial, _motionVectorsBuffer, _vDilationProperties, shaderPassId: dilationPassId);
                    }
                }

                using (new ProfilingScope(ctx.cmd, _gradientSampler))
                {
                    //Temporarily use _depthCaptureTexture (not yet rendered to in this Execute function) to store indicator results.
                    //We'll use _blurBuffer temporarily to handle the blurring
                    SetTextureSize(_indicatorProperties, _depthCaptureBuffer);
                    _indicatorProperties.SetTexture(ShaderID._Source, _depthBakeBuffer);
                    CoreUtils.DrawFullScreen(ctx.cmd, _indicatorMaterial, _depthCaptureBuffer, _indicatorProperties, shaderPassId: _indicatorFunctionDepthPassId);//Get indicator function from baked depth buffer
                    _hBlurProperties.Clear();
                    _hBlurProperties.SetFloat(ShaderID._Radius, blurRadiusBase);
                    _hBlurProperties.SetTexture(ShaderID._Source, _depthCaptureBuffer);
                    SetTextureSize(_hBlurProperties, _blurBuffer);
                    CoreUtils.DrawFullScreen(ctx.cmd, _blurMaterial, _blurBuffer, _hBlurProperties, shaderPassId: _hBlurPassId);
                    _vBlurProperties.Clear();
                    _vBlurProperties.SetFloat(ShaderID._Radius, blurRadiusBase);
                    _vBlurProperties.SetTexture(ShaderID._Source, _blurBuffer);
                    SetTextureSize(_vBlurProperties, _depthCaptureBuffer);
                    CoreUtils.DrawFullScreen(ctx.cmd, _blurMaterial, _depthCaptureBuffer, _vBlurProperties, shaderPassId: _vBlurPassId);

                    float blurredGradientMaximum = GetBlurredGradientMaximum(blurRadiusBase);
                    _gradientProperties.SetFloat(ShaderID._Multiplier, -1f / blurredGradientMaximum);
                    _gradientProperties.SetTexture(ShaderID._Source, _depthCaptureBuffer);
                    SetTextureSize(_gradientProperties, _gradientBuffer);
                    CoreUtils.DrawFullScreen(ctx.cmd, _gradientMaterial, _gradientBuffer, _gradientProperties, shaderPassId: _gradientWithMultiplierPassId);
                }
            }

            //For information on use of RTHandles system, see: https://docs.unity3d.com/Packages/com.unity.render-pipelines.core@10.2/manual/rthandle-system-using.html

            using (new ProfilingScope(ctx.cmd, _depthBlurSampler))
            {
                _hBlurProperties.Clear();
                _hBlurProperties.SetFloat(ShaderID._Radius, blurRadiusBase);
                _hBlurProperties.SetTexture(ShaderID._Source, _depthBakeBuffer);
                SetTextureSize(_hBlurProperties, _blurBuffer);
                CoreUtils.DrawFullScreen(ctx.cmd, _blurMaterial, _blurBuffer, _hBlurProperties, shaderPassId: _hBlurPassId);
                // Copy back the result in the target render texture while doing a vertical blur
                _vBlurProperties.Clear();
                _vBlurProperties.SetFloat(ShaderID._Radius, blurRadiusBase);
                _vBlurProperties.SetTexture(ShaderID._Source, _blurBuffer);
                SetTextureSize(_vBlurProperties, _depthCaptureBuffer);
                CoreUtils.DrawFullScreen(ctx.cmd, _blurMaterial, _depthCaptureBuffer, _vBlurProperties, shaderPassId: _vBlurPassId);
            }

            if (ApproximateSweptArea)
            {
                ctx.cmd.Blit(_motionVectorsBuffer, _dilationBuffer);
                if (_sweepAreaKernelIndex >= 0)
                {
                    DispatchComputeDisplacement(ctx, _sweepAreaKernelIndex);
                }
            }

            if (UpdateMaskHeightFromComputeShader)
            {
                _heightEncodeDecodeProperties.Clear();
                SetTextureSize(_heightEncodeDecodeProperties, _maskHeightRenderTexture);

                _displacementComputeShaderValue.SetFloat(ShaderID.Compute.FrameTime, Time.deltaTime);
                _displacementComputeShaderValue.SetVector(ShaderID.Compute.SpeedScaleOffset_GradientFractionalPowerScale, new Vector4(_speedScale, _speedOffset + _blurRadius, _gradientFractionalPower, _gradientScale));//NB Minimum offset based on the blur radius in px
                Vector2 unitSquareJitter = _lowDiscrepancySequence.GetNextValues2();
                Vector2 scaledJitter = (unitSquareJitter - 0.5f * Vector2.one) * _motionJitterScale;
                _displacementComputeShaderValue.SetVector(ShaderID.Compute.Jitter, scaledJitter);
                _displacementComputeShaderValue.SetVector(ShaderID.Compute.HalfLife_HeightSpread_Decay_Block, new Vector4(_heightSpreadHalfLife, _heightDecayHalfLife, _heightBlockHalfLife, 0f));
                _displacementComputeShaderValue.SetVector(ShaderID.Compute.HeightDecay_Reference_Magnitude_PowerLowHigh, new Vector4(_heightDecayReference, _heightDecayMagnitude, _heightDecayPowerLow, _heightDecayPowerHigh));
                _displacementComputeShaderValue.SetVector(ShaderID.Compute.VelocityDecay_Reference_Magnitude_PowerLowHigh, new Vector4(_velocityDecayReference, _velocityDecayMagnitude, _velocityDecayPowerLow, _velocityDecayPowerHigh));

                if (!_performedHeightEncoding)
                {
                    _performedHeightEncoding = DispatchComputeDisplacement(ctx, _heightEncodeKernelIndex);
                }

                if (SimulateVelocity)
                {
                    DispatchComputeDisplacement(ctx, _velocityDecodeKernelIndex);
                    DispatchComputeDisplacement(ctx, _velocityClearKernelIndex);
                    DispatchComputeDisplacement(ctx, _velocityUpdateKernelIndex);
                }

                for (int displacementIndex = 0; displacementIndex < 1 + _numExtraDisplacementPasses; displacementIndex++)
                {
                    CoreUtils.DrawFullScreen(ctx.cmd, _heightEncodeDecodeMaterial, _maskHeightRenderTexture, _heightEncodeDecodeProperties, shaderPassId: _heightDecodePassId);
                    DispatchComputeDisplacement(ctx, _displacementKernelIndex);
                }

                if (_useHeightSpreadPass)
                {
                    CoreUtils.DrawFullScreen(ctx.cmd, _heightEncodeDecodeMaterial, _maskHeightRenderTexture, _heightEncodeDecodeProperties, shaderPassId: _heightDecodePassId);
                    DispatchComputeDisplacement(ctx, _heightAboveOneSpreadKernelIndex);
                }
                if (_falloffAtEdgesValue && _edgeFalloffTexture != null)
                {
                    DispatchComputeDisplacement(ctx, _minimumFalloffKernelIndex);
                }
                else
                {
                    DispatchComputeDisplacement(ctx, _minimumKernelIndex);
                }
                CoreUtils.DrawFullScreen(ctx.cmd, _heightEncodeDecodeMaterial, _maskHeightRenderTexture, _heightEncodeDecodeProperties, shaderPassId: _heightDecodePassId);
            }

            if (_useSmoothedNormalGenerationValue)
            {
                _scharrProperties.Clear();
                _scharrProperties.SetTexture(ShaderID._Source, _maskHeightRenderTexture);
                SetTextureSize(_scharrProperties, _gradientBuffer);
                CoreUtils.DrawFullScreen(ctx.cmd, _scharrMaterial, _gradientBuffer, _scharrProperties, shaderPassId: _scharrBlurHorizontalVerticalPassId);
            }

            Vector4[] crtCenters = new Vector4[] { new Vector4(0.5f, 0.5f, 0.5f, 0f) };
            Vector4[] crtSizesRotations = new Vector4[] { new Vector4(1f, 1f, 1f, 0f) };
            if (!UpdateMaskHeightFromComputeShader)
            {
                _outputRenderTextureProperties.Clear();
                SetTextureSize(_outputRenderTextureProperties, _maskHeightRenderTexture);
                CoreUtils.DrawFullScreen(ctx.cmd, _maskMaterial, _maskHeightRenderTexture, _outputRenderTextureProperties, shaderPassId: _maskHeightUpdatePassId);
            }
            _outputRenderTextureProperties.Clear();
            SetTextureSize(_outputRenderTextureProperties, _layerMaskRenderTexture);
            CoreUtils.DrawFullScreen(ctx.cmd, _maskMaterial, _layerMaskRenderTexture, _outputRenderTextureProperties, shaderPassId: LayerMaskShaderPassId);
            _outputRenderTextureProperties.Clear();
            SetTextureSize(_outputRenderTextureProperties, _detailNormalRenderTexture);
            CoreUtils.DrawFullScreen(ctx.cmd, _detailNormalMaterial, _detailNormalRenderTexture, _outputRenderTextureProperties, shaderPassId: DetailNormalShaderPassId);
        }

        // release all resources
        protected override void Cleanup()
        {
            if (_blurMaterial != null)
            {
                CoreUtils.Destroy(_blurMaterial);
            }
            if (_groundDepthMaterial != null)
            {
                CoreUtils.Destroy(_groundDepthMaterial);
            }
            if (_litMaterial != null)
            {
                CoreUtils.Destroy(_litMaterial);
            }
            if (_maskMaterial != null)
            {
                CoreUtils.Destroy(_maskMaterial);
            }
            if (_detailNormalMaterial != null)
            {
                CoreUtils.Destroy(_detailNormalMaterial);
            }
            if (_dilationMaterial != null)
            {
                CoreUtils.Destroy(_dilationMaterial);
            }
            if (_indicatorMaterial != null)
            {
                CoreUtils.Destroy(_indicatorMaterial);
            }
            if (_gradientMaterial != null)
            {
                CoreUtils.Destroy(_gradientMaterial);
            }
            if (_tintedLookupMaterial != null)
            {
                CoreUtils.Destroy(_tintedLookupMaterial);
            }
            if (_scharrMaterial != null)
            {
                CoreUtils.Destroy(_scharrMaterial);
            }
            if (_depthRectificationMaterial != null)
            {
                CoreUtils.Destroy(_depthRectificationMaterial);
            }
            if (_uvEdgesMaterial != null)
            {
                CoreUtils.Destroy(_uvEdgesMaterial);
            }
            if (_heightEncodeDecodeMaterial != null)
            {
                CoreUtils.Destroy(_heightEncodeDecodeMaterial);
            }

            if (_blurBuffer != null)
            {
                _blurBuffer.Release();
            }
            if (_depthBakeBuffer != null)
            {
                _depthBakeBuffer.Release();
            }
            if (_groundBakeTexture != null)
            {
                _groundBakeTexture.Release();
            }
            if (_depthCaptureBuffer != null)
            {
                _depthCaptureBuffer.Release();
            }
            if (_motionVectorsBuffer != null)
            {
                _motionVectorsBuffer.Release();
            }
            if (_dilationBuffer != null)
            {
                _dilationBuffer.Release();
            }
            if (_gradientBuffer != null)
            {
                _gradientBuffer.Release();
            }

            if (_maskHeightRenderTexture != null)
            {
                _maskHeightRenderTexture.Release();
            }
            if (_edgeFalloffTexture != null)
            {
                _edgeFalloffTexture.Release();
            }
            if (_layerMaskRenderTexture != null)
            {
                _layerMaskRenderTexture.Release();
            }
            if (_detailNormalRenderTexture != null)
            {
                _detailNormalRenderTexture.Release();
            }

            if (_velocityComputeBufferEncoded != null)
            {
                _velocityComputeBufferEncoded.Release();
            }
            if (_velocityComputeBufferFloat != null)
            {
                _velocityComputeBufferFloat.Release();
            }
            if (_heightComputeBufferEncoded != null)
            {
                _heightComputeBufferEncoded.Release();
            }

            if (_bakingCamera!= null && _bakingCamera.gameObject != null)
            {
                GameObject.DestroyImmediate(_bakingCamera.gameObject);
            }
        }
    }
}