// SPDX-License-Identifier: MIT
// SPDX-FileCopyrightText: © 2021 Tyler Dodds

using UnityEngine;

namespace SnowRendering
{
    [ExecuteInEditMode]
    public class CustomRenderTextureUpdater : MonoBehaviour
    {
        [SerializeField]
        CustomRenderTexture _customRenderTexture;

        [SerializeField]
        private bool _initializeOnStart = true;
        
        void Start()
        {
            if(_initializeOnStart)
            {
                TryInitializeCustomRenderTexture();
            }
        }

        private void TryInitializeCustomRenderTexture()
        {
            if(_customRenderTexture != null)
            {
                _customRenderTexture.Initialize();
            }
        }
    }
}