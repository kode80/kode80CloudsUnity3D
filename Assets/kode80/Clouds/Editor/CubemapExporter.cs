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
using UnityEditor;
using System;
using System.Collections;
using System.IO;


namespace kode80.Clouds
{
	public class CubemapExporter 
	{
		private kode80Clouds _clouds;
		private FullScreenQuad _fullScreenQuad;
		private Camera _camera;

		public void ExportCubemap( kode80Clouds clouds, FullScreenQuad fullScreenQuad, Camera targetCamera)
		{
			_clouds = clouds;

			kode80Clouds.SubPixelSize oldSubPixelSize = _clouds.subPixelSize;
			int oldDownsample = _clouds.downsample;

			_clouds.subPixelSize = kode80Clouds.SubPixelSize.Sub1x1;
			_clouds.downsample = 1;
			_clouds.UpdateSharedFromPublicProperties();

            GameObject cmGO = GameObject.Instantiate(targetCamera.gameObject);
            cmGO.name = "CubeMapCam";
            cmGO.hideFlags = HideFlags.HideAndDontSave;
            _camera = cmGO.GetComponent<Camera>();
            _camera.targetTexture = null;
			_fullScreenQuad = cmGO.GetComponentInChildren<FullScreenQuad>();
			_fullScreenQuad.targetCamera = _camera;

			fullScreenQuad.gameObject.SetActive(false);

			Cubemap cm = new Cubemap( 2048, TextureFormat.RGB24, true);
			Camera.onPreCull += OnPreCull;
			_camera.RenderToCubemap( cm);
			Camera.onPreCull -= OnPreCull;
			UnityEngine.Object.DestroyImmediate( cmGO);

            SaveCubeMap( cm, "Assets/kode80Clouds.cubemap");
            //SaveCubeMapFaces(cm, "Assets/kode80Clouds");
			
			fullScreenQuad.gameObject.SetActive(true);

			_clouds.subPixelSize = oldSubPixelSize;
			_clouds.downsample = oldDownsample;
			_clouds.UpdateSharedFromPublicProperties();

			_camera = null;
			_clouds = null;
			_fullScreenQuad = null;
		}

		private void OnPreCull( Camera theCamera)
		{
			if( theCamera == _camera)
			{
				_fullScreenQuad.transform.localRotation = Quaternion.identity;
				_fullScreenQuad.transform.localPosition = Vector3.zero;
				_fullScreenQuad.GenerateMesh( theCamera, theCamera.farClipPlane - 10.0f);

				var rotation = Quaternion.LookRotation(
					-theCamera.cameraToWorldMatrix.MultiplyVector( Vector3.forward),
					theCamera.cameraToWorldMatrix.MultiplyVector( Vector3.up));
				_fullScreenQuad.transform.rotation = rotation;
				_fullScreenQuad.transform.localScale = new Vector3( -1, 1, 1);

				_clouds.SetCamera( _camera);
				_clouds.RenderClouds();

				_fullScreenQuad.material.SetFloat( "_DrawCoverage", 0.0f);
				_fullScreenQuad.material.SetFloat( "_DrawCursor", 0.0f);
				_fullScreenQuad.material.SetTexture( "_MainTex", _clouds.currentFrame);
				_clouds.cloudsSharedProperties.ApplyToMaterial( _fullScreenQuad.material);
			}
		}

        private void SaveCubeMap( Cubemap cubemap, string path)
        {
            string cubemapPath = AssetDatabase.GenerateUniqueAssetPath(path);
            AssetDatabase.CreateAsset(cubemap, cubemapPath);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

		private void SaveCubeMapFaces( Cubemap cubemap, string path)
		{
			Texture2D tex = new Texture2D( cubemap.width, cubemap.height, TextureFormat.RGB24, false);
			
			SaveCubeMapFace( cubemap, CubemapFace.NegativeX, tex, path);
			SaveCubeMapFace( cubemap, CubemapFace.PositiveX, tex, path);
			SaveCubeMapFace( cubemap, CubemapFace.NegativeY, tex, path);
			SaveCubeMapFace( cubemap, CubemapFace.PositiveY, tex, path);
			SaveCubeMapFace( cubemap, CubemapFace.NegativeZ, tex, path);
			SaveCubeMapFace( cubemap, CubemapFace.PositiveZ, tex, path);

			UnityEngine.Object.DestroyImmediate( tex);

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
		}
		
		private void SaveCubeMapFace( Cubemap cubemap, CubemapFace face, Texture2D tex, string path)
		{
            path = AssetDatabase.GenerateUniqueAssetPath(path + "_" + face.ToString() + ".png");
            string dataPath = Application.dataPath.Substring(0, Application.dataPath.Length - "/Assets".Length);
            path = Path.Combine(dataPath, path);

            int w = cubemap.width;
			int h = cubemap.height;
			
			Color[] pixels = cubemap.GetPixels( face);
			Color[] pixelsFlipped = new Color[ w * h];
			
			
			for( int i=0; i<h; i++)
			{
				Array.Copy( pixels, i * w, pixelsFlipped, (h - 1 - i) * w, w);
			}
			
			tex.SetPixels( pixelsFlipped);
			byte[] bytes = tex.EncodeToPNG();      
			File.WriteAllBytes( path, bytes);   
		}
	}
}