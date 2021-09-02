// SPDX-License-Identifier: MIT
// SPDX-FileCopyrightText: © 2021 Tyler Dodds

using UnityEngine;

namespace SnowRendering
{
    static class SnowMaterialUtil
    {
        internal static void CopySnowLayersProperties(Material snowTemplate, int numLayers, MaterialPropertyBlock materialPropertyBlock)
        {
            void SetColor(int nameId) => materialPropertyBlock.SetColor(nameId, snowTemplate.GetColor(nameId));
            void SetTexture(int nameId)
            {
                Texture snowTexture = snowTemplate.GetTexture(nameId);
                if (snowTexture != null)
                {
                    materialPropertyBlock.SetTexture(nameId, snowTexture);
                }
            }
            void SetFloat(int nameId) => materialPropertyBlock.SetFloat(nameId, snowTemplate.GetFloat(nameId));
            void SetInt(int nameId) => materialPropertyBlock.SetInt(nameId, snowTemplate.GetInt(nameId));
            void SetVector(int nameId) => materialPropertyBlock.SetVector(nameId, snowTemplate.GetVector(nameId));
            Color detailsMappingMask = new Color(1, 0, 0, 0);

            if (numLayers > 1)
            {
                SetColor(SnowShaderId._BaseColor1);
                SetTexture(SnowTextureId._BaseColorMap1);
                SetFloat(SnowShaderId._Metallic1);
                SetFloat(SnowShaderId._Smoothness1);
                SetFloat(SnowShaderId._SmoothnessRemapMin1);
                SetFloat(SnowShaderId._SmoothnessRemapMax1);
                SetFloat(SnowShaderId._AORemapMin1);
                SetFloat(SnowShaderId._AORemapMax1);
                SetTexture(SnowTextureId._MaskMap1);
                SetTexture(SnowTextureId._NormalMap1);
                SetTexture(SnowTextureId._NormalMapOS1);
                SetFloat(SnowShaderId._NormalScale1);
                SetTexture(SnowTextureId._BentNormalMap1);
                SetTexture(SnowTextureId._BentNormalMapOS1);
                SetTexture(SnowTextureId._HeightMap1);
                SetFloat(SnowShaderId._HeightAmplitude1);
                SetFloat(SnowShaderId._HeightCenter1);
                SetInt(SnowShaderId._HeightMapParametrization1);
                SetFloat(SnowShaderId._HeightOffset1);
                SetFloat(SnowShaderId._HeightMin1);
                SetFloat(SnowShaderId._HeightMax1);
                SetFloat(SnowShaderId._HeightTessAmplitude1);
                SetFloat(SnowShaderId._HeightTessCenter1);
                SetFloat(SnowShaderId._HeightPoMAmplitude1);
                //SetTexture(SnowShaderId._DetailMap1);//Set in CustomPass
                SetFloat(SnowShaderId._DetailAlbedoScale1);
                SetFloat(SnowShaderId._DetailNormalScale1);
                SetFloat(SnowShaderId._DetailSmoothnessScale1);
                SetFloat(SnowShaderId._NormalMapSpace1);
                SetInt(SnowShaderId._DiffusionProfile1);
                SetVector(SnowShaderId._DiffusionProfileAsset1);
                SetFloat(SnowShaderId._DiffusionProfileHash1);
                SetFloat(SnowShaderId._SubsurfaceMask1);
                SetTexture(SnowTextureId._SubsurfaceMaskMap1);
                SetFloat(SnowShaderId._Thickness1);
                SetTexture(SnowTextureId._ThicknessMap1);
                SetVector(SnowShaderId._ThicknessRemap1);
                SetFloat(SnowShaderId._InheritBaseNormal1);
                SetFloat(SnowShaderId._InheritBaseHeight1);
                SetFloat(SnowShaderId._InheritBaseColor1);
                SetFloat(SnowShaderId._OpacityAsDensity1);

                materialPropertyBlock.SetInt(SnowShaderId._LinkDetailsWithBase1, 0);
                materialPropertyBlock.SetInt(SnowShaderId._UVDetail1, 0);
                materialPropertyBlock.SetColor(SnowShaderId._UVDetailsMappingMask1, detailsMappingMask);
                materialPropertyBlock.SetFloat(SnowShaderId._DetailAlbedoScale1, 0);
                materialPropertyBlock.SetFloat(SnowShaderId._DetailNormalScale1, 1);
                materialPropertyBlock.SetFloat(SnowShaderId._DetailSmoothnessScale1, 0);
            }

            if (numLayers > 2)
            {
                SetColor(SnowShaderId._BaseColor2);
                SetTexture(SnowTextureId._BaseColorMap2);
                SetFloat(SnowShaderId._Metallic2);
                SetFloat(SnowShaderId._Smoothness2);
                SetFloat(SnowShaderId._SmoothnessRemapMin2);
                SetFloat(SnowShaderId._SmoothnessRemapMax2);
                SetFloat(SnowShaderId._AORemapMin2);
                SetFloat(SnowShaderId._AORemapMax2);
                SetTexture(SnowTextureId._MaskMap2);
                SetTexture(SnowTextureId._NormalMap2);
                SetTexture(SnowTextureId._NormalMapOS2);
                SetFloat(SnowShaderId._NormalScale2);
                SetTexture(SnowTextureId._BentNormalMap2);
                SetTexture(SnowTextureId._BentNormalMapOS2);
                SetTexture(SnowTextureId._HeightMap2);
                SetFloat(SnowShaderId._HeightAmplitude2);
                SetFloat(SnowShaderId._HeightCenter2);
                SetInt(SnowShaderId._HeightMapParametrization2);
                SetFloat(SnowShaderId._HeightOffset2);
                SetFloat(SnowShaderId._HeightMin2);
                SetFloat(SnowShaderId._HeightMax2);
                SetFloat(SnowShaderId._HeightTessAmplitude2);
                SetFloat(SnowShaderId._HeightTessCenter2);
                SetFloat(SnowShaderId._HeightPoMAmplitude2);
                //SetTexture(SnowShaderId._DetailMap2);//Set in CustomPass
                SetFloat(SnowShaderId._DetailAlbedoScale2);
                SetFloat(SnowShaderId._DetailNormalScale2);
                SetFloat(SnowShaderId._DetailSmoothnessScale2);
                SetFloat(SnowShaderId._NormalMapSpace2);
                SetInt(SnowShaderId._DiffusionProfile2);
                SetVector(SnowShaderId._DiffusionProfileAsset2);
                SetFloat(SnowShaderId._DiffusionProfileHash2);
                SetFloat(SnowShaderId._SubsurfaceMask2);
                SetTexture(SnowTextureId._SubsurfaceMaskMap2);
                SetFloat(SnowShaderId._Thickness2);
                SetTexture(SnowTextureId._ThicknessMap2);
                SetVector(SnowShaderId._ThicknessRemap2);
                SetFloat(SnowShaderId._InheritBaseNormal2);
                SetFloat(SnowShaderId._InheritBaseHeight2);
                SetFloat(SnowShaderId._InheritBaseColor2);
                SetFloat(SnowShaderId._OpacityAsDensity2);

                materialPropertyBlock.SetInt(SnowShaderId._LinkDetailsWithBase2, 0);
                materialPropertyBlock.SetInt(SnowShaderId._UVDetail2, 0);
                materialPropertyBlock.SetColor(SnowShaderId._UVDetailsMappingMask2, detailsMappingMask);
                materialPropertyBlock.SetFloat(SnowShaderId._DetailAlbedoScale2, 0);
                materialPropertyBlock.SetFloat(SnowShaderId._DetailNormalScale2, 1);
                materialPropertyBlock.SetFloat(SnowShaderId._DetailSmoothnessScale2, 0);
            }

            if (numLayers > 3)
            {
                SetColor(SnowShaderId._BaseColor3);
                SetTexture(SnowTextureId._BaseColorMap3);
                SetFloat(SnowShaderId._Metallic3);
                SetFloat(SnowShaderId._Smoothness3);
                SetFloat(SnowShaderId._SmoothnessRemapMin3);
                SetFloat(SnowShaderId._SmoothnessRemapMax3);
                SetFloat(SnowShaderId._AORemapMin3);
                SetFloat(SnowShaderId._AORemapMax3);
                SetTexture(SnowTextureId._MaskMap3);
                SetTexture(SnowTextureId._NormalMap3);
                SetTexture(SnowTextureId._NormalMapOS3);
                SetFloat(SnowShaderId._NormalScale3);
                SetTexture(SnowTextureId._BentNormalMap3);
                SetTexture(SnowTextureId._BentNormalMapOS3);
                SetTexture(SnowTextureId._HeightMap3);
                SetFloat(SnowShaderId._HeightAmplitude3);
                SetFloat(SnowShaderId._HeightCenter3);
                SetInt(SnowShaderId._HeightMapParametrization3);
                SetFloat(SnowShaderId._HeightOffset3);
                SetFloat(SnowShaderId._HeightMin3);
                SetFloat(SnowShaderId._HeightMax3);
                SetFloat(SnowShaderId._HeightTessAmplitude3);
                SetFloat(SnowShaderId._HeightTessCenter3);
                SetFloat(SnowShaderId._HeightPoMAmplitude3);
                //SetTexture(SnowShaderId._DetailMap3);//Set in CustomPass
                SetFloat(SnowShaderId._DetailAlbedoScale3);
                SetFloat(SnowShaderId._DetailNormalScale3);
                SetFloat(SnowShaderId._DetailSmoothnessScale3);
                SetFloat(SnowShaderId._NormalMapSpace3);
                SetInt(SnowShaderId._DiffusionProfile3);
                SetVector(SnowShaderId._DiffusionProfileAsset3);
                SetFloat(SnowShaderId._DiffusionProfileHash3);
                SetFloat(SnowShaderId._SubsurfaceMask3);
                SetTexture(SnowTextureId._SubsurfaceMaskMap3);
                SetFloat(SnowShaderId._Thickness3);
                SetTexture(SnowTextureId._ThicknessMap3);
                SetVector(SnowShaderId._ThicknessRemap3);
                SetFloat(SnowShaderId._InheritBaseNormal3);
                SetFloat(SnowShaderId._InheritBaseHeight3);
                SetFloat(SnowShaderId._InheritBaseColor3);
                SetFloat(SnowShaderId._OpacityAsDensity3);

                materialPropertyBlock.SetInt(SnowShaderId._LinkDetailsWithBase2, 0);
                materialPropertyBlock.SetInt(SnowShaderId._UVDetail2, 0);
                materialPropertyBlock.SetColor(SnowShaderId._UVDetailsMappingMask2, detailsMappingMask);
                materialPropertyBlock.SetFloat(SnowShaderId._DetailAlbedoScale2, 0);
                materialPropertyBlock.SetFloat(SnowShaderId._DetailNormalScale2, 1);
                materialPropertyBlock.SetFloat(SnowShaderId._DetailSmoothnessScale2, 0);
            }

            //SetTexture("_LayerMaskMap");//Set in CustomPass
            //SetTexture("_LayerInfluenceMaskMap");//Set in Main Layer

            //Note that some of these are Toggles and may not be strictly necessary to set, since we're already enforcing the same shader and keyword set as the template material.
            SetFloat(SnowShaderId._UseHeightBasedBlend);
            SetFloat(SnowShaderId._HeightTransition);
            SetFloat(SnowShaderId._UseDensityMode);
            SetFloat(SnowShaderId._UseMainLayerInfluence);
            SetFloat(SnowShaderId._LayerCount);
            SetFloat(SnowShaderId._VertexColorMode);
            SetFloat(SnowShaderId._ObjectScaleAffectTile);
            SetFloat(SnowShaderId._UVBlendMask);
            SetColor(SnowShaderId._UVMappingMaskBlendMask);
            SetFloat(SnowShaderId._TexWorldScaleBlendMask);

        }

        internal static bool ValidateSnowLayersProperties(Material snowTemplate, int numLayers)
        {
            bool HasColor(int nameId)
            {
                bool hasProperty = snowTemplate.HasProperty(nameId);
                try
                {
                    snowTemplate.GetColor(nameId);
                }
                catch
                {
                    hasProperty = false;
                }
                return hasProperty;
            }
            bool HasTexture(int nameId)
            {
                bool hasProperty = snowTemplate.HasProperty(nameId);
                try
                {
                    snowTemplate.GetTexture(nameId);
                }
                catch
                {
                    hasProperty = false;
                }
                return hasProperty;
            }
            bool HasFloat(int nameId)
            {
                bool hasProperty = snowTemplate.HasProperty(nameId);
                try
                {
                    snowTemplate.GetFloat(nameId);
                }
                catch
                {
                    hasProperty = false;
                }
                return hasProperty;
            }
            bool HasInt(int nameId)
            {
                bool hasProperty = snowTemplate.HasProperty(nameId);
                try
                {
                    snowTemplate.GetInt(nameId);
                }
                catch
                {
                    hasProperty = false;
                }
                return hasProperty;
            }
            bool HasVector(int nameId)
            {
                bool hasProperty = snowTemplate.HasProperty(nameId);
                try
                {
                    snowTemplate.GetVector(nameId);
                }
                catch
                {
                    hasProperty = false;
                }
                return hasProperty;
            }

            bool validated = true;

            if (numLayers > 1)
            {
                validated &= HasColor(SnowShaderId._BaseColor1);
                validated &= HasTexture(SnowTextureId._BaseColorMap1);
                validated &= HasFloat(SnowShaderId._Metallic1);
                validated &= HasFloat(SnowShaderId._Smoothness1);
                validated &= HasFloat(SnowShaderId._SmoothnessRemapMin1);
                validated &= HasFloat(SnowShaderId._SmoothnessRemapMax1);
                validated &= HasFloat(SnowShaderId._AORemapMin1);
                validated &= HasFloat(SnowShaderId._AORemapMax1);
                validated &= HasTexture(SnowTextureId._MaskMap1);
                validated &= HasTexture(SnowTextureId._NormalMap1);
                validated &= HasTexture(SnowTextureId._NormalMapOS1);
                validated &= HasFloat(SnowShaderId._NormalScale1);
                validated &= HasTexture(SnowTextureId._BentNormalMap1);
                validated &= HasTexture(SnowTextureId._BentNormalMapOS1);
                validated &= HasTexture(SnowTextureId._HeightMap1);
                validated &= HasFloat(SnowShaderId._HeightAmplitude1);
                validated &= HasFloat(SnowShaderId._HeightCenter1);
                validated &= HasInt(SnowShaderId._HeightMapParametrization1);
                validated &= HasFloat(SnowShaderId._HeightOffset1);
                validated &= HasFloat(SnowShaderId._HeightMin1);
                validated &= HasFloat(SnowShaderId._HeightMax1);
                validated &= HasFloat(SnowShaderId._HeightTessAmplitude1);
                validated &= HasFloat(SnowShaderId._HeightTessCenter1);
                validated &= HasFloat(SnowShaderId._HeightPoMAmplitude1);
                validated &= HasTexture(SnowTextureId._DetailMap1);
                validated &= HasFloat(SnowShaderId._DetailAlbedoScale1);
                validated &= HasFloat(SnowShaderId._DetailNormalScale1);
                validated &= HasFloat(SnowShaderId._DetailSmoothnessScale1);
                validated &= HasFloat(SnowShaderId._NormalMapSpace1);
                validated &= HasInt(SnowShaderId._DiffusionProfile1);
                validated &= HasVector(SnowShaderId._DiffusionProfileAsset1);
                validated &= HasFloat(SnowShaderId._DiffusionProfileHash1);
                validated &= HasFloat(SnowShaderId._SubsurfaceMask1);
                validated &= HasTexture(SnowTextureId._SubsurfaceMaskMap1);
                validated &= HasFloat(SnowShaderId._Thickness1);
                validated &= HasTexture(SnowTextureId._ThicknessMap1);
                validated &= HasVector(SnowShaderId._ThicknessRemap1);
                validated &= HasFloat(SnowShaderId._InheritBaseNormal1);
                validated &= HasFloat(SnowShaderId._InheritBaseHeight1);
                validated &= HasFloat(SnowShaderId._InheritBaseColor1);
                validated &= HasFloat(SnowShaderId._OpacityAsDensity1);

                validated &= snowTemplate.IsKeywordEnabled(SnowShaderKeywords._DETAIL_MAP1);
            }

            if (numLayers > 2)
            {
                validated &= HasColor(SnowShaderId._BaseColor2);
                validated &= HasTexture(SnowTextureId._BaseColorMap2);
                validated &= HasFloat(SnowShaderId._Metallic2);
                validated &= HasFloat(SnowShaderId._Smoothness2);
                validated &= HasFloat(SnowShaderId._SmoothnessRemapMin2);
                validated &= HasFloat(SnowShaderId._SmoothnessRemapMax2);
                validated &= HasFloat(SnowShaderId._AORemapMin2);
                validated &= HasFloat(SnowShaderId._AORemapMax2);
                validated &= HasTexture(SnowTextureId._MaskMap2);
                validated &= HasTexture(SnowTextureId._NormalMap2);
                validated &= HasTexture(SnowTextureId._NormalMapOS2);
                validated &= HasFloat(SnowShaderId._NormalScale2);
                validated &= HasTexture(SnowTextureId._BentNormalMap2);
                validated &= HasTexture(SnowTextureId._BentNormalMapOS2);
                validated &= HasTexture(SnowTextureId._HeightMap2);
                validated &= HasFloat(SnowShaderId._HeightAmplitude2);
                validated &= HasFloat(SnowShaderId._HeightCenter2);
                validated &= HasInt(SnowShaderId._HeightMapParametrization2);
                validated &= HasFloat(SnowShaderId._HeightOffset2);
                validated &= HasFloat(SnowShaderId._HeightMin2);
                validated &= HasFloat(SnowShaderId._HeightMax2);
                validated &= HasFloat(SnowShaderId._HeightTessAmplitude2);
                validated &= HasFloat(SnowShaderId._HeightTessCenter2);
                validated &= HasFloat(SnowShaderId._HeightPoMAmplitude2);
                validated &= HasTexture(SnowTextureId._DetailMap2);
                validated &= HasFloat(SnowShaderId._DetailAlbedoScale2);
                validated &= HasFloat(SnowShaderId._DetailNormalScale2);
                validated &= HasFloat(SnowShaderId._DetailSmoothnessScale2);
                validated &= HasFloat(SnowShaderId._NormalMapSpace2);
                validated &= HasInt(SnowShaderId._DiffusionProfile2);
                validated &= HasVector(SnowShaderId._DiffusionProfileAsset2);
                validated &= HasFloat(SnowShaderId._DiffusionProfileHash2);
                validated &= HasFloat(SnowShaderId._SubsurfaceMask2);
                validated &= HasTexture(SnowTextureId._SubsurfaceMaskMap2);
                validated &= HasFloat(SnowShaderId._Thickness2);
                validated &= HasTexture(SnowTextureId._ThicknessMap2);
                validated &= HasVector(SnowShaderId._ThicknessRemap2);
                validated &= HasFloat(SnowShaderId._InheritBaseNormal2);
                validated &= HasFloat(SnowShaderId._InheritBaseHeight2);
                validated &= HasFloat(SnowShaderId._InheritBaseColor2);
                validated &= HasFloat(SnowShaderId._OpacityAsDensity2);

                validated &= snowTemplate.IsKeywordEnabled(SnowShaderKeywords._DETAIL_MAP2);
            }

            if (numLayers > 3)
            {
                validated &= HasColor(SnowShaderId._BaseColor3);
                validated &= HasTexture(SnowTextureId._BaseColorMap3);
                validated &= HasFloat(SnowShaderId._Metallic3);
                validated &= HasFloat(SnowShaderId._Smoothness3);
                validated &= HasFloat(SnowShaderId._SmoothnessRemapMin3);
                validated &= HasFloat(SnowShaderId._SmoothnessRemapMax3);
                validated &= HasFloat(SnowShaderId._AORemapMin3);
                validated &= HasFloat(SnowShaderId._AORemapMax3);
                validated &= HasTexture(SnowTextureId._MaskMap3);
                validated &= HasTexture(SnowTextureId._NormalMap3);
                validated &= HasTexture(SnowTextureId._NormalMapOS3);
                validated &= HasFloat(SnowShaderId._NormalScale3);
                validated &= HasTexture(SnowTextureId._BentNormalMap3);
                validated &= HasTexture(SnowTextureId._BentNormalMapOS3);
                validated &= HasTexture(SnowTextureId._HeightMap3);
                validated &= HasFloat(SnowShaderId._HeightAmplitude3);
                validated &= HasFloat(SnowShaderId._HeightCenter3);
                validated &= HasInt(SnowShaderId._HeightMapParametrization3);
                validated &= HasFloat(SnowShaderId._HeightOffset3);
                validated &= HasFloat(SnowShaderId._HeightMin3);
                validated &= HasFloat(SnowShaderId._HeightMax3);
                validated &= HasFloat(SnowShaderId._HeightTessAmplitude3);
                validated &= HasFloat(SnowShaderId._HeightTessCenter3);
                validated &= HasFloat(SnowShaderId._HeightPoMAmplitude3);
                validated &= HasTexture(SnowTextureId._DetailMap3);
                validated &= HasFloat(SnowShaderId._DetailAlbedoScale3);
                validated &= HasFloat(SnowShaderId._DetailNormalScale3);
                validated &= HasFloat(SnowShaderId._DetailSmoothnessScale3);
                validated &= HasFloat(SnowShaderId._NormalMapSpace3);
                validated &= HasInt(SnowShaderId._DiffusionProfile3);
                validated &= HasVector(SnowShaderId._DiffusionProfileAsset3);
                validated &= HasFloat(SnowShaderId._DiffusionProfileHash3);
                validated &= HasFloat(SnowShaderId._SubsurfaceMask3);
                validated &= HasTexture(SnowTextureId._SubsurfaceMaskMap3);
                validated &= HasFloat(SnowShaderId._Thickness3);
                validated &= HasTexture(SnowTextureId._ThicknessMap3);
                validated &= HasVector(SnowShaderId._ThicknessRemap3);
                validated &= HasFloat(SnowShaderId._InheritBaseNormal3);
                validated &= HasFloat(SnowShaderId._InheritBaseHeight3);
                validated &= HasFloat(SnowShaderId._InheritBaseColor3);
                validated &= HasFloat(SnowShaderId._OpacityAsDensity3);

                validated &= snowTemplate.IsKeywordEnabled(SnowShaderKeywords._DETAIL_MAP3);
            }

            validated &= HasTexture(SnowTextureId._LayerMaskMap);
            validated &= HasTexture(SnowShaderId._LayerInfluenceMaskMap);
            validated &= HasFloat(SnowShaderId._UseHeightBasedBlend);
            validated &= HasFloat(SnowShaderId._HeightTransition);
            validated &= HasFloat(SnowShaderId._UseDensityMode);
            validated &= HasFloat(SnowShaderId._UseMainLayerInfluence);
            validated &= HasFloat(SnowShaderId._LayerCount);
            validated &= HasFloat(SnowShaderId._VertexColorMode);
            validated &= HasFloat(SnowShaderId._ObjectScaleAffectTile);
            validated &= HasFloat(SnowShaderId._UVBlendMask);
            validated &= HasColor(SnowShaderId._UVMappingMaskBlendMask);
            validated &= HasFloat(SnowShaderId._TexWorldScaleBlendMask);

            return validated;
        }

        internal static bool ValidateLayers2And3Equal(Material material)
        {
            bool validated = true;
            bool ValidateColor(int nameIdA, int nameIdB) => material.GetColor(nameIdA) == material.GetColor(nameIdB);
            bool ValidateTexture(int nameIdA, int nameIdB) => (material.GetTexture(nameIdA) == material.GetTexture(nameIdB)) && (material.GetTextureScale(nameIdA) == material.GetTextureScale(nameIdB)) && (material.GetTextureOffset(nameIdA) == material.GetTextureOffset(nameIdB));
            bool ValidateFloat(int nameIdA, int nameIdB) => material.GetFloat(nameIdA) == material.GetFloat(nameIdB);
            bool ValidateInt(int nameIdA, int nameIdB) => material.GetInt(nameIdA) == material.GetInt(nameIdB);
            bool ValidateVector(int nameIdA, int nameIdB) => material.GetVector(nameIdA) == material.GetVector(nameIdB);

            validated &= ValidateColor(SnowShaderId._BaseColor2, SnowShaderId._BaseColor3);
            validated &= ValidateTexture(SnowTextureId._BaseColorMap2, SnowTextureId._BaseColorMap3);
            validated &= ValidateFloat(SnowShaderId._Metallic2, SnowShaderId._Metallic3);
            validated &= ValidateFloat(SnowShaderId._Smoothness2, SnowShaderId._Smoothness3);
            validated &= ValidateFloat(SnowShaderId._SmoothnessRemapMin2, SnowShaderId._SmoothnessRemapMin3);
            validated &= ValidateFloat(SnowShaderId._SmoothnessRemapMax2, SnowShaderId._SmoothnessRemapMax3);
            validated &= ValidateFloat(SnowShaderId._AORemapMin2, SnowShaderId._AORemapMin3);
            validated &= ValidateFloat(SnowShaderId._AORemapMax2, SnowShaderId._AORemapMax3);
            validated &= ValidateTexture(SnowTextureId._MaskMap2, SnowTextureId._MaskMap3);
            validated &= ValidateTexture(SnowTextureId._NormalMap2, SnowTextureId._NormalMap3);
            validated &= ValidateTexture(SnowTextureId._NormalMapOS2, SnowTextureId._NormalMapOS3);
            //validated &= ValidateFloat(SnowShaderId._NormalScale2, SnowShaderId._NormalScale3);//No need for these to be the same, since height ranges will be different, so too will the normal map strength
            validated &= ValidateTexture(SnowTextureId._BentNormalMap2, SnowTextureId._BentNormalMap3);
            validated &= ValidateTexture(SnowTextureId._BentNormalMapOS2, SnowTextureId._BentNormalMapOS3);
            validated &= ValidateTexture(SnowTextureId._HeightMap2, SnowTextureId._HeightMap3);
            //NB No need to validate height ranges, as they are expected to be different
            //validated &= ValidateFloat(SnowShaderId._HeightAmplitude3);
            //validated &= ValidateFloat(SnowShaderId._HeightCenter3);
            //validated &= ValidateInt(SnowShaderId._HeightMapParametrization3);
            //validated &= ValidateFloat(SnowShaderId._HeightOffValidate3);
            //validated &= ValidateFloat(SnowShaderId._HeightMin3);
            //validated &= ValidateFloat(SnowShaderId._HeightMax3);
            //validated &= ValidateFloat(SnowShaderId._HeightTessAmplitude3);
            //validated &= ValidateFloat(SnowShaderId._HeightTessCenter3);
            //validated &= ValidateFloat(SnowShaderId._HeightPoMAmplitude3);
            //validated &= ValidateTexture(SnowShaderId._DetailMap3);//Unused
            validated &= ValidateFloat(SnowShaderId._DetailAlbedoScale2, SnowShaderId._DetailAlbedoScale3);
            validated &= ValidateFloat(SnowShaderId._DetailNormalScale2, SnowShaderId._DetailNormalScale3);
            validated &= ValidateFloat(SnowShaderId._DetailSmoothnessScale2, SnowShaderId._DetailSmoothnessScale3);
            validated &= ValidateFloat(SnowShaderId._NormalMapSpace2, SnowShaderId._NormalMapSpace3);
            validated &= ValidateInt(SnowShaderId._DiffusionProfile2, SnowShaderId._DiffusionProfile3);
            validated &= ValidateVector(SnowShaderId._DiffusionProfileAsset2, SnowShaderId._DiffusionProfileAsset3);
            validated &= ValidateFloat(SnowShaderId._DiffusionProfileHash2, SnowShaderId._DiffusionProfileHash3);
            validated &= ValidateFloat(SnowShaderId._SubsurfaceMask2, SnowShaderId._SubsurfaceMask3);
            validated &= ValidateTexture(SnowTextureId._SubsurfaceMaskMap2, SnowTextureId._SubsurfaceMaskMap3);
            validated &= ValidateFloat(SnowShaderId._Thickness2, SnowShaderId._Thickness3);
            validated &= ValidateTexture(SnowTextureId._ThicknessMap2, SnowTextureId._ThicknessMap3);
            validated &= ValidateVector(SnowShaderId._ThicknessRemap2, SnowShaderId._ThicknessRemap3);
            validated &= ValidateFloat(SnowShaderId._InheritBaseNormal2, SnowShaderId._InheritBaseNormal3);
            validated &= ValidateFloat(SnowShaderId._InheritBaseHeight2, SnowShaderId._InheritBaseHeight3);
            validated &= ValidateFloat(SnowShaderId._InheritBaseColor2, SnowShaderId._InheritBaseColor3);
            validated &= ValidateFloat(SnowShaderId._OpacityAsDensity2, SnowShaderId._OpacityAsDensity3);

            return validated;
        }

    }
}