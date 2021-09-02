// SPDX-License-Identifier: MIT
// SPDX-FileCopyrightText: © 2021 Tyler Dodds

using UnityEngine;

namespace SnowRendering
{

    class ShaderPassId
    {
        private readonly string _passName;
        private readonly Material _material;
        private readonly int _defaultPassId;

        private bool _resolved = false;
        private int _passId = -1;

        public ShaderPassId(string passName, Material material, int defaultPassId)
        {
            _passName = passName;
            _material = material;
            _defaultPassId = defaultPassId;
        }

        public int GetPassId()
        {
            if (!_resolved)
            {
                if (_material != null)
                {
                    _passId = _material.FindPass(_passName);
                    if (_passId < 0)
                    {
                        _passId = _defaultPassId;
                        Debug.LogWarningFormat("Could not find pass name {0} in material {1}; using default {2} instead.", _passName, _material.name, _defaultPassId);
                    }
                    _resolved = true;
                    return _passId;
                }
                else
                {
                    return _defaultPassId;
                }
            }
            else
            {
                return _passId;
            }
        }

        public static implicit operator int(ShaderPassId shaderPassId) => shaderPassId.GetPassId();
    }

    class ComputeShaderKernelId
    {
        private readonly string _kernelName;
        private readonly ComputeShader _computeShader;
        private readonly int _defaultKernelId;

        private bool _resolved = false;
        private int _kernelId = -1;

        public ComputeShaderKernelId(string kernelName, ComputeShader computeShader, int defaultKernelId)
        {
            _kernelName = kernelName;
            _computeShader = computeShader;
            _defaultKernelId = defaultKernelId;
        }

        public int GetKernelId()
        {
            if (!_resolved)
            {
                if (_computeShader != null)
                {
                    HasKernel = _computeShader.HasKernel(_kernelName);
                    if (HasKernel)
                    {
                        _kernelId = _computeShader.FindKernel(_kernelName);
                    }
                    else
                    {
                        _kernelId = _defaultKernelId;
                        Debug.LogWarningFormat("Could not find kernel name {0} in compute shader {1}; using default {2} instead.", _kernelName, _computeShader.name, _defaultKernelId);
                    }
                    _resolved = true;
                    return _kernelId;
                }
                else
                {
                    return _defaultKernelId;
                }
            }
            else
            {
                return _kernelId;
            }
        }

        public bool HasKernel { get; private set; } = false;

        public static implicit operator int(ComputeShaderKernelId computeShaderKernelId) => computeShaderKernelId.GetKernelId();
    }
}