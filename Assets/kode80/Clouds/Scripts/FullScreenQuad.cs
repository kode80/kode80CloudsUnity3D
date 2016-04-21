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

namespace kode80.Clouds
{
	[ExecuteInEditMode]
	public class FullScreenQuad : MonoBehaviour 
	{
		public Material material;
		public bool renderWhenPlaying;
        public Camera targetCamera;
		private Mesh _mesh;
        private MeshFilter _filter;
        private MeshRenderer _renderer;

		void Awake()
		{
			_mesh = new Mesh();
			_mesh.hideFlags = HideFlags.HideAndDontSave;
            Vector3 v0 = new Vector3(-1.0f, -1.0f, 1.0f);
            Vector3 v1 = new Vector3(1.0f, -1.0f, 1.0f);
            Vector3 v2 = new Vector3(1.0f, 1.0f, 1.0f);
            Vector3 v3 = new Vector3(-1.0f, 1.0f, 1.0f);

            Vector2 uv0 = new Vector2(0.0f, 0.0f);
            Vector2 uv1 = new Vector2(1.0f, 0.0f);
            Vector2 uv2 = new Vector2(1.0f, 1.0f);
            Vector2 uv3 = new Vector2(0.0f, 1.0f);

            _mesh.vertices = new Vector3[] { v0, v1, v2, v3 };
            _mesh.uv = new Vector2[] { uv0, uv1, uv2, uv3 };
            _mesh.triangles = new int[] { 0, 1, 2,
                                         2, 3, 0};
            _mesh.bounds = new Bounds(Vector3.zero, new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));

            _renderer = gameObject.AddComponent<MeshRenderer>();
            _renderer.hideFlags = HideFlags.HideAndDontSave;

            _filter = gameObject.AddComponent<MeshFilter>();
            _filter.hideFlags = HideFlags.HideAndDontSave;
            _filter.sharedMesh = _mesh;
		}
        
		void Start () {
        }

        void OnEnable()
        {
            Camera.onPreCull += QuadPreCull;
        }

        void OnDisable()
        {
            Camera.onPreCull -= QuadPreCull;
        }

        void LateUpdate()
        {
            _renderer.enabled = true;
            _renderer.sharedMaterial = material;
        }
        
        void QuadPreCull( Camera camera)
        {
            Camera target = targetCamera ?? Camera.main;
            _renderer.enabled = camera == target;
        }
	}
}