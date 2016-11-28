//***************************************************
//
//  Author: Ben Hopkins
//  Copyright (C) 2016 kode80 LLC, 
//  all rights reserved
// 
//  Free to use for non-commercial purposes, 
//  see full license in project root:
//  kode80CloudsNonCommercialLicense.html
//  
//  Commercial licenses available for purchase from:
//  http://kode80.com/
//
//***************************************************

using UnityEngine;
using UnityEngine.Rendering;

namespace kode80.Clouds
{
    [ExecuteInEditMode]
    public class kode80CloudShadows : MonoBehaviour
    {
        public kode80Clouds clouds;

        private Light _light;
        private CommandBuffer _commandBuffer;
        private Material _material;
        
        void ShadowsPreCull( Camera camera)
        {
            UpdateShadows();
        }

        public void UpdateShadows()
        {
            if( clouds == null || _light == null) { return; }

            _material.SetTexture(Uniforms._CloudCoverage, clouds.cloudCoverage);
            _material.SetMatrix(Uniforms._InvCamera, clouds.targetCamera.cameraToWorldMatrix);
            _material.SetMatrix(Uniforms._InvProjection, clouds.targetCamera.projectionMatrix.inverse);
            _material.SetVector(Uniforms._Offset, Vector3.zero);
            _material.SetFloat(Uniforms._CoverageScale, 1.0f / clouds.cloudsSharedProperties.maxDistance);
            _material.SetVector(Uniforms._CoverageOffset, clouds.coverageOffset);
            _material.SetVector(Uniforms._LightDirection, clouds.sunLight.transform.forward);
            _material.SetFloat(Uniforms._ShadowStrength, _light.shadowStrength);
            clouds.cloudsSharedProperties.ApplyToMaterial(_material);
        }

        void OnEnable()
        {
            _light = GetComponent<Light>();
            if( _light == null && _light.type != LightType.Directional)
            {
                Debug.LogWarning("kode80CloudShadows component must be on a directional light");
                enabled = false;
                return;
            }

            if( _material == null)
            {
                _material = new Material(Shader.Find("Hidden/kode80/CloudShadowPass"));
                _material.hideFlags = HideFlags.HideAndDontSave;
            }

            if( _commandBuffer == null)
            {
                _commandBuffer = new CommandBuffer();
                _commandBuffer.name = "kode80 Clouds Shadow Pass";
                _commandBuffer.Blit( BuiltinRenderTextureType.None, BuiltinRenderTextureType.CurrentActive, _material);
                _light.AddCommandBuffer( LightEvent.AfterScreenspaceMask, _commandBuffer);
            }

            Camera.onPreCull += ShadowsPreCull;
        }

        void OnDisable()
        {
            Camera.onPreCull -= ShadowsPreCull;

            if ( _commandBuffer != null)
            {
                _light.RemoveCommandBuffer( LightEvent.AfterScreenspaceMask, _commandBuffer);
                _commandBuffer = null;
            }

            if( _material != null)
            {
                DestroyImmediate(_material);
                _material = null;
            }
        }
    }
}
