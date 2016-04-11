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
	public class CrepuscularRays : MonoBehaviour 
	{
		public kode80Clouds clouds;
        public float sampleCount = 20.0f;
        [Range( 0.0f, 1.0f)]
        public float density = 0.813f;
        [Range(0.0f, 1.0f)]
        public float decay = 1.0f;
        [Range(0.0f, 1.0f)]
        public float weight = 1.0f;
        public float exposure = 3.0f;

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
		}
		
		// Update is called once per frame
		void Update () {
		
		}

		public void OnRenderImage( RenderTexture src, RenderTexture dst)
		{
            if( clouds == null) {
                Graphics.Blit(src, dst);
                return;
            }

			CreateMaterialsIfNeeded();

			Vector3 sunScreenSpace = clouds.targetCamera.WorldToScreenPoint( clouds.sunLight.transform.forward * -100000.0f);
			sunScreenSpace.x /= clouds.targetCamera.pixelWidth;
			sunScreenSpace.y /= clouds.targetCamera.pixelHeight;

			_material.SetTexture( "_Clouds", clouds.currentFrame);
            _material.SetVector("_SunScreenSpace", sunScreenSpace);
            _material.SetFloat("_SampleCount", sampleCount);
            _material.SetFloat("_Density", density);
            _material.SetFloat("_Decay", decay);
            _material.SetFloat("_Weight", weight);
            _material.SetFloat("_Exposure", exposure);

            Graphics.Blit( src, dst, _material);
        }

        private void CreateMaterialsIfNeeded()
		{
			if( _material == null)
			{
				_material = new Material( Shader.Find( "Hidden/kode80/CloudsCrepuscularRays"));
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