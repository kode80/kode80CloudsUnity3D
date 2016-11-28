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
		private Mesh _mesh;
		private readonly Vector3[] _vertices = new Vector3[4];
		private readonly Vector2[] _uvs = new [] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };
		private readonly int[] _triangles = new [] { 0, 1, 2, 2, 3, 0};

		void Awake()
		{
			targetCamera = targetCamera == null ? Camera.main : targetCamera;
			_meshFilter = GetOrAddComponent<MeshFilter>(gameObject);
			_meshRenderer = GetOrAddComponent<MeshRenderer>(gameObject);

			if(VRSettings.enabled && VRSettings.loadedDeviceName == "OpenVR")
			{
				horizontalScale = 1.059739f;
				verticalScale = 0.9982163f;
			}

			_mesh = new Mesh();
			_mesh.name = "Cloud Quad";
			_mesh.MarkDynamic();
			_mesh.vertices = _vertices;
			_mesh.uv = _uvs;
			_mesh.triangles = _triangles;

			GenerateMesh(targetCamera);
		}

		void Update()
		{
			GenerateMesh(targetCamera);
		}

		public void GenerateMesh( Camera targetCamera, float localZOffset = 0.0f)
		{
			bool isVR = VRSettings.enabled && targetCamera.stereoTargetEye != StereoTargetEyeMask.None;
			float width = isVR ? VRSettings.eyeTextureWidth : targetCamera.pixelWidth;
			float height = isVR ? VRSettings.eyeTextureHeight : targetCamera.pixelHeight;

			float aspect = width / height;
			float fov = targetCamera.fieldOfView * Mathf.Deg2Rad;
			float localZ = transform.localPosition.z + localZOffset;
			float h = (localZ * 2.0f * Mathf.Tan(fov / 2.0f)) / 2.0f;
			float w = h * aspect;

			w *= horizontalScale;
			h *= verticalScale;

			float z = localZOffset;
			_vertices[0] = new Vector3(-w, -h, z);
			_vertices[1] = new Vector3(w, -h, z);
			_vertices[2] = new Vector3(w, h, z);
			_vertices[3] = new Vector3(-w, h, z);

			_mesh.vertices = _vertices;
			_mesh.RecalculateBounds();
			_mesh.UploadMeshData(false);

			_meshFilter.sharedMesh = _mesh;
			_meshRenderer.sharedMaterial = material;
		}

		private T GetOrAddComponent<T>( GameObject gameObject) where T : Component
		{
			var component = gameObject.GetComponent<T>();

			if( component == null) {
				component = gameObject.AddComponent<T>();
			}

			return component;
		}
	}
}