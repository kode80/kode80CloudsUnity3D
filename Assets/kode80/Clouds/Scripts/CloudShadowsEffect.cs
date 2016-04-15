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
using System.Collections;

namespace kode80.Clouds
{
	[ExecuteInEditMode]
	public class CloudShadowsEffect : MonoBehaviour 
	{
		public kode80Clouds clouds;
		public Camera targetCamera;

        private Material _material;

		
		void OnEnable()
		{
			CreateMaterialsIfNeeded();
		}
		
		void OnDisable()
		{
			DestroyMaterials();
		}

		void Reset () 
		{
			CreateMaterialsIfNeeded();
		}

		void Start () 
		{
			CreateMaterialsIfNeeded();
            clouds = GameObject.FindObjectOfType<kode80Clouds>();
			targetCamera.depthTextureMode = DepthTextureMode.Depth;
		}
		
		// Update is called once per frame
		void Update () {
		
		}

		[ImageEffectOpaque]
		public void OnRenderImage( RenderTexture src, RenderTexture dst)
		{
            if( clouds == null) {
                Graphics.Blit(src, dst);
                return;
            }

			CreateMaterialsIfNeeded();

			_material.SetTexture( "_CloudCoverage", clouds.cloudCoverage);
			_material.SetMatrix( "_InvCamera", targetCamera.cameraToWorldMatrix);
			_material.SetMatrix( "_InvProjection", targetCamera.projectionMatrix.inverse);
			_material.SetVector("_Offset", Vector3.zero);
			_material.SetFloat("_CoverageScale", 1.0f / clouds.cloudsSharedProperties.maxDistance);
			_material.SetVector("_CoverageOffset", clouds.coverageOffset);
			_material.SetVector( "_LightDirection", clouds.sunLight.transform.forward);
			clouds.cloudsSharedProperties.ApplyToMaterial( _material);

            Graphics.Blit( src, dst, _material);
        }

        private void CreateMaterialsIfNeeded()
		{
			if( _material == null)
			{
				_material = new Material( Shader.Find( "hidden/kode80/CloudShadowPass"));
				_material.hideFlags = HideFlags.HideAndDontSave;
			}
		}

		private void DestroyMaterials()
		{
			DestroyImmediate( _material);
			_material = null;
		}
	}
}