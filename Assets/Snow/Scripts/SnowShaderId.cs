// SPDX-License-Identifier: MIT
// SPDX-FileCopyrightText: © 2021 Tyler Dodds

using UnityEngine;

namespace SnowRendering
{
    class TextureId
    {
        public int Id { get; private set; }
        public int Id_ST { get; private set; }

        public TextureId(string textureName)
        {
            Id = Shader.PropertyToID(textureName);
            Id_ST = Shader.PropertyToID(textureName + "_ST");
        }

        public static implicit operator int(TextureId d) => d.Id;
    }

    static class SnowTextureId
    {
        public static readonly TextureId _LayerMaskMap = new TextureId("_LayerMaskMap");

        public static readonly TextureId _DetailMap0 = new TextureId("_DetailMap0");
        public static readonly TextureId _DetailMap1 = new TextureId("_DetailMap1");
        public static readonly TextureId _DetailMap2 = new TextureId("_DetailMap2");
        public static readonly TextureId _DetailMap3 = new TextureId("_DetailMap3");

        public static readonly TextureId _BaseColorMap1 = new TextureId("_BaseColorMap1");
        public static readonly TextureId _BaseColorMap2 = new TextureId("_BaseColorMap2");
        public static readonly TextureId _BaseColorMap3 = new TextureId("_BaseColorMap3");

        public static readonly TextureId _MaskMap1 = new TextureId("_MaskMap1");
        public static readonly TextureId _MaskMap2 = new TextureId("_MaskMap2");
        public static readonly TextureId _MaskMap3 = new TextureId("_MaskMap3");

        public static readonly TextureId _NormalMap1 = new TextureId("_NormalMap1");
        public static readonly TextureId _NormalMap2 = new TextureId("_NormalMap2");
        public static readonly TextureId _NormalMap3 = new TextureId("_NormalMap3");

        public static readonly TextureId _NormalMapOS1 = new TextureId("_NormalMapOS1");
        public static readonly TextureId _NormalMapOS2 = new TextureId("_NormalMapOS2");
        public static readonly TextureId _NormalMapOS3 = new TextureId("_NormalMapOS3");

        public static readonly TextureId _BentNormalMap1 = new TextureId("_BentNormalMap1");
        public static readonly TextureId _BentNormalMap2 = new TextureId("_BentNormalMap2");
        public static readonly TextureId _BentNormalMap3 = new TextureId("_BentNormalMap3");

        public static readonly TextureId _BentNormalMapOS1 = new TextureId("_BentNormalMapOS1");
        public static readonly TextureId _BentNormalMapOS2 = new TextureId("_BentNormalMapOS2");
        public static readonly TextureId _BentNormalMapOS3 = new TextureId("_BentNormalMapOS3");

        public static readonly TextureId _HeightMap1 = new TextureId("_HeightMap1");
        public static readonly TextureId _HeightMap2 = new TextureId("_HeightMap2");
        public static readonly TextureId _HeightMap3 = new TextureId("_HeightMap3");

        public static readonly TextureId _SubsurfaceMaskMap1 = new TextureId("_SubsurfaceMaskMap1");
        public static readonly TextureId _SubsurfaceMaskMap2 = new TextureId("_SubsurfaceMaskMap2");
        public static readonly TextureId _SubsurfaceMaskMap3 = new TextureId("_SubsurfaceMaskMap3");

        public static readonly TextureId _ThicknessMap1 = new TextureId("_ThicknessMap1");
        public static readonly TextureId _ThicknessMap2 = new TextureId("_ThicknessMap2");
        public static readonly TextureId _ThicknessMap3 = new TextureId("_ThicknessMap3");
    }

    static class SnowShaderId
    {
        public static readonly int _BaseColor1 = Shader.PropertyToID("_BaseColor1");
        public static readonly int _Metallic1 = Shader.PropertyToID("_Metallic1");
        public static readonly int _Smoothness1 = Shader.PropertyToID("_Smoothness1");
        public static readonly int _SmoothnessRemapMin1 = Shader.PropertyToID("_SmoothnessRemapMin1");
        public static readonly int _SmoothnessRemapMax1 = Shader.PropertyToID("_SmoothnessRemapMax1");
        public static readonly int _AORemapMin1 = Shader.PropertyToID("_AORemapMin1");
        public static readonly int _AORemapMax1 = Shader.PropertyToID("_AORemapMax1");
        public static readonly int _NormalScale1 = Shader.PropertyToID("_NormalScale1");
        public static readonly int _HeightAmplitude1 = Shader.PropertyToID("_HeightAmplitude1");
        public static readonly int _HeightCenter1 = Shader.PropertyToID("_HeightCenter1");
        public static readonly int _HeightMapParametrization1 = Shader.PropertyToID("_HeightMapParametrization1");
        public static readonly int _HeightOffset1 = Shader.PropertyToID("_HeightOffset1");
        public static readonly int _HeightMin1 = Shader.PropertyToID("_HeightMin1");
        public static readonly int _HeightMax1 = Shader.PropertyToID("_HeightMax1");
        public static readonly int _HeightTessAmplitude1 = Shader.PropertyToID("_HeightTessAmplitude1");
        public static readonly int _HeightTessCenter1 = Shader.PropertyToID("_HeightTessCenter1");
        public static readonly int _HeightPoMAmplitude1 = Shader.PropertyToID("_HeightPoMAmplitude1");
        public static readonly int _DetailAlbedoScale1 = Shader.PropertyToID("_DetailAlbedoScale1");
        public static readonly int _DetailNormalScale1 = Shader.PropertyToID("_DetailNormalScale1");
        public static readonly int _DetailSmoothnessScale1 = Shader.PropertyToID("_DetailSmoothnessScale1");
        public static readonly int _NormalMapSpace1 = Shader.PropertyToID("_NormalMapSpace1");
        public static readonly int _DiffusionProfile1 = Shader.PropertyToID("_DiffusionProfile1");
        public static readonly int _DiffusionProfileAsset1 = Shader.PropertyToID("_DiffusionProfileAsset1");
        public static readonly int _DiffusionProfileHash1 = Shader.PropertyToID("_DiffusionProfileHash1");
        public static readonly int _SubsurfaceMask1 = Shader.PropertyToID("_SubsurfaceMask1");
        public static readonly int _Thickness1 = Shader.PropertyToID("_Thickness1");
        public static readonly int _ThicknessRemap1 = Shader.PropertyToID("_ThicknessRemap1");
        public static readonly int _InheritBaseNormal1 = Shader.PropertyToID("_InheritBaseNormal1");
        public static readonly int _InheritBaseHeight1 = Shader.PropertyToID("_InheritBaseHeight1");
        public static readonly int _InheritBaseColor1 = Shader.PropertyToID("_InheritBaseColor1");
        public static readonly int _OpacityAsDensity1 = Shader.PropertyToID("_OpacityAsDensity1");
        public static readonly int _LinkDetailsWithBase1 = Shader.PropertyToID("_LinkDetailsWithBase1");
        public static readonly int _UVDetail1 = Shader.PropertyToID("_UVDetail1");
        public static readonly int _UVDetailsMappingMask1 = Shader.PropertyToID("_UVDetailsMappingMask1");

        public static readonly int _BaseColor2 = Shader.PropertyToID("_BaseColor2");
        public static readonly int _Metallic2 = Shader.PropertyToID("_Metallic2");
        public static readonly int _Smoothness2 = Shader.PropertyToID("_Smoothness2");
        public static readonly int _SmoothnessRemapMin2 = Shader.PropertyToID("_SmoothnessRemapMin2");
        public static readonly int _SmoothnessRemapMax2 = Shader.PropertyToID("_SmoothnessRemapMax2");
        public static readonly int _AORemapMin2 = Shader.PropertyToID("_AORemapMin2");
        public static readonly int _AORemapMax2 = Shader.PropertyToID("_AORemapMax2");
        public static readonly int _NormalScale2 = Shader.PropertyToID("_NormalScale2");
        public static readonly int _HeightAmplitude2 = Shader.PropertyToID("_HeightAmplitude2");
        public static readonly int _HeightCenter2 = Shader.PropertyToID("_HeightCenter2");
        public static readonly int _HeightMapParametrization2 = Shader.PropertyToID("_HeightMapParametrization2");
        public static readonly int _HeightOffset2 = Shader.PropertyToID("_HeightOffset2");
        public static readonly int _HeightMin2 = Shader.PropertyToID("_HeightMin2");
        public static readonly int _HeightMax2 = Shader.PropertyToID("_HeightMax2");
        public static readonly int _HeightTessAmplitude2 = Shader.PropertyToID("_HeightTessAmplitude2");
        public static readonly int _HeightTessCenter2 = Shader.PropertyToID("_HeightTessCenter2");
        public static readonly int _HeightPoMAmplitude2 = Shader.PropertyToID("_HeightPoMAmplitude2");
        public static readonly int _DetailAlbedoScale2 = Shader.PropertyToID("_DetailAlbedoScale2");
        public static readonly int _DetailNormalScale2 = Shader.PropertyToID("_DetailNormalScale2");
        public static readonly int _DetailSmoothnessScale2 = Shader.PropertyToID("_DetailSmoothnessScale2");
        public static readonly int _NormalMapSpace2 = Shader.PropertyToID("_NormalMapSpace2");
        public static readonly int _DiffusionProfile2 = Shader.PropertyToID("_DiffusionProfile2");
        public static readonly int _DiffusionProfileAsset2 = Shader.PropertyToID("_DiffusionProfileAsset2");
        public static readonly int _DiffusionProfileHash2 = Shader.PropertyToID("_DiffusionProfileHash2");
        public static readonly int _SubsurfaceMask2 = Shader.PropertyToID("_SubsurfaceMask2");
        public static readonly int _Thickness2 = Shader.PropertyToID("_Thickness2");
        public static readonly int _ThicknessRemap2 = Shader.PropertyToID("_ThicknessRemap2");
        public static readonly int _InheritBaseNormal2 = Shader.PropertyToID("_InheritBaseNormal2");
        public static readonly int _InheritBaseHeight2 = Shader.PropertyToID("_InheritBaseHeight2");
        public static readonly int _InheritBaseColor2 = Shader.PropertyToID("_InheritBaseColor2");
        public static readonly int _OpacityAsDensity2 = Shader.PropertyToID("_OpacityAsDensity2");
        public static readonly int _LinkDetailsWithBase2 = Shader.PropertyToID("_LinkDetailsWithBase2");
        public static readonly int _UVDetail2 = Shader.PropertyToID("_UVDetail2");
        public static readonly int _UVDetailsMappingMask2 = Shader.PropertyToID("_UVDetailsMappingMask2");

        public static readonly int _BaseColor3 = Shader.PropertyToID("_BaseColor3");
        public static readonly int _Metallic3 = Shader.PropertyToID("_Metallic3");
        public static readonly int _Smoothness3 = Shader.PropertyToID("_Smoothness3");
        public static readonly int _SmoothnessRemapMin3 = Shader.PropertyToID("_SmoothnessRemapMin3");
        public static readonly int _SmoothnessRemapMax3 = Shader.PropertyToID("_SmoothnessRemapMax3");
        public static readonly int _AORemapMin3 = Shader.PropertyToID("_AORemapMin3");
        public static readonly int _AORemapMax3 = Shader.PropertyToID("_AORemapMax3");
        public static readonly int _NormalScale3 = Shader.PropertyToID("_NormalScale3");
        public static readonly int _HeightAmplitude3 = Shader.PropertyToID("_HeightAmplitude3");
        public static readonly int _HeightCenter3 = Shader.PropertyToID("_HeightCenter3");
        public static readonly int _HeightMapParametrization3 = Shader.PropertyToID("_HeightMapParametrization3");
        public static readonly int _HeightOffset3 = Shader.PropertyToID("_HeightOffset3");
        public static readonly int _HeightMin3 = Shader.PropertyToID("_HeightMin3");
        public static readonly int _HeightMax3 = Shader.PropertyToID("_HeightMax3");
        public static readonly int _HeightTessAmplitude3 = Shader.PropertyToID("_HeightTessAmplitude3");
        public static readonly int _HeightTessCenter3 = Shader.PropertyToID("_HeightTessCenter3");
        public static readonly int _HeightPoMAmplitude3 = Shader.PropertyToID("_HeightPoMAmplitude3");
        public static readonly int _DetailAlbedoScale3 = Shader.PropertyToID("_DetailAlbedoScale3");
        public static readonly int _DetailNormalScale3 = Shader.PropertyToID("_DetailNormalScale3");
        public static readonly int _DetailSmoothnessScale3 = Shader.PropertyToID("_DetailSmoothnessScale3");
        public static readonly int _NormalMapSpace3 = Shader.PropertyToID("_NormalMapSpace3");
        public static readonly int _DiffusionProfile3 = Shader.PropertyToID("_DiffusionProfile3");
        public static readonly int _DiffusionProfileAsset3 = Shader.PropertyToID("_DiffusionProfileAsset3");
        public static readonly int _DiffusionProfileHash3 = Shader.PropertyToID("_DiffusionProfileHash3");
        public static readonly int _SubsurfaceMask3 = Shader.PropertyToID("_SubsurfaceMask3");
        public static readonly int _Thickness3 = Shader.PropertyToID("_Thickness3");
        public static readonly int _ThicknessRemap3 = Shader.PropertyToID("_ThicknessRemap3");
        public static readonly int _InheritBaseNormal3 = Shader.PropertyToID("_InheritBaseNormal3");
        public static readonly int _InheritBaseHeight3 = Shader.PropertyToID("_InheritBaseHeight3");
        public static readonly int _InheritBaseColor3 = Shader.PropertyToID("_InheritBaseColor3");
        public static readonly int _OpacityAsDensity3 = Shader.PropertyToID("_OpacityAsDensity3");
        public static readonly int _LinkDetailsWithBase3 = Shader.PropertyToID("_LinkDetailsWithBase3");
        public static readonly int _UVDetail3 = Shader.PropertyToID("_UVDetail3");
        public static readonly int _UVDetailsMappingMask3 = Shader.PropertyToID("_UVDetailsMappingMask3");

        public static readonly int _UseHeightBasedBlend = Shader.PropertyToID("_UseHeightBasedBlend");
        public static readonly int _HeightTransition = Shader.PropertyToID("_HeightTransition");
        public static readonly int _UseDensityMode = Shader.PropertyToID("_UseDensityMode");
        public static readonly int _UseMainLayerInfluence = Shader.PropertyToID("_UseMainLayerInfluence");
        public static readonly int _LayerCount = Shader.PropertyToID("_LayerCount");
        public static readonly int _VertexColorMode = Shader.PropertyToID("_VertexColorMode");
        public static readonly int _ObjectScaleAffectTile = Shader.PropertyToID("_ObjectScaleAffectTile");
        public static readonly int _UVBlendMask = Shader.PropertyToID("_UVBlendMask");
        public static readonly int _UVMappingMaskBlendMask = Shader.PropertyToID("_UVMappingMaskBlendMask");
        public static readonly int _TexWorldScaleBlendMask = Shader.PropertyToID("_TexWorldScaleBlendMask");
        public static readonly int _LayerInfluenceMaskMap = Shader.PropertyToID("_LayerInfluenceMaskMap");

    }

    static class SnowShaderKeywords
    {
        public static readonly string _DETAIL_MAP1 = "_DETAIL_MAP1";
        public static readonly string _DETAIL_MAP2 = "_DETAIL_MAP2";
        public static readonly string _DETAIL_MAP3 = "_DETAIL_MAP3";
    }
}