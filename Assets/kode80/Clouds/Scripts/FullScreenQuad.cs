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
using UnityEngine.VR;

namespace kode80.Clouds
{
	[ExecuteInEditMode]
	public class FullScreenQuad : MonoBehaviour 
	{
		public float horizontalScale = 1.0f;
		public float verticalScale = 1.0f;
		public Camera targetCamera;
		public Material material;
		private MeshFilter _meshFilter;
		private MeshRenderer _meshRenderer;


		void Start ()
		{
			targetCamera = targetCamera == null ? Camera.main : targetCamera;
			_meshFilter = gameObject.AddComponent<MeshFilter>();
			_meshRenderer = gameObject.AddComponent<MeshRenderer>();

			GenerateMesh();
		}

		void Update()
		{
			GenerateMesh();
		}

		void GenerateMesh()
		{
			float width = VRSettings.enabled ? VRSettings.eyeTextureWidth : targetCamera.pixelWidth;
			float height = VRSettings.enabled ? VRSettings.eyeTextureHeight : targetCamera.pixelHeight;

			float aspect = width / height;
			float fov = Camera.main.fieldOfView * Mathf.Deg2Rad;
			float h = (transform.localPosition.z * 2.0f * Mathf.Tan(fov / 2.0f)) / 2.0f;
			float w = h * aspect;

			w *= horizontalScale;
			h *= verticalScale;

			float z = 0.0f;
			Vector3 v0 = new Vector3(-w, -h, z);
			Vector3 v1 = new Vector3(w, -h, z);
			Vector3 v2 = new Vector3(w, h, z);
			Vector3 v3 = new Vector3(-w, h, z);

			Vector2 uv0 = new Vector2(0.0f, 0.0f);
			Vector2 uv1 = new Vector2(1.0f, 0.0f);
			Vector2 uv2 = new Vector2(1.0f, 1.0f);
			Vector2 uv3 = new Vector2(0.0f, 1.0f);

			Mesh mesh = new Mesh();
			mesh.vertices = new Vector3[] { v0, v1, v2, v3 };
			mesh.uv = new Vector2[] { uv0, uv1, uv2, uv3 };
			mesh.triangles = new int[] { 0, 1, 2,
				2, 3, 0};
			mesh.RecalculateBounds();

			_meshFilter.mesh = mesh;
			_meshRenderer.sharedMaterial = material;
		}
	}
}