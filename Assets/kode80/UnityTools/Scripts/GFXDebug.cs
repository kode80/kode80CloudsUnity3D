using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace kode80.Utils
{
	public class EndOfFrameShim : MonoBehaviour
	{
		void Start() {}

		void OnEnable() {}

	}

	public class GFXDebug
	{
		private static GFXDebug _instance = null;
		public static GFXDebug instance { 
			get { 
				if( _instance == null) {
					_instance = new GFXDebug();
				}
				return _instance;
			} 
		}

		private EndOfFrameShim _eofShim;
		private RenderTexture _renderTexture;
		private bool _drawQueued = false;
		private Material _material;

		private GFXDebug()
		{
			Camera.onPostRender += OnPostRender;
			_material = new Material( Shader.Find("Hidden/kode80/UnlitAlphaMultiply"));
		}

		~GFXDebug()
		{
			Camera.onPostRender -= OnPostRender;
		}

		public static void DrawTexture( Camera targetCamera, Texture texture) {
			GFXDebug.instance.InternalDrawTexture( targetCamera, texture);
		}

		private void InternalDrawTexture( Camera targetCamera, Texture texture)
		{
			RenderTexture targetTexture = GetTemporaryRenderTexture();
			_material.SetFloat( "_AlphaMultiplier", 1.0f);
			Graphics.Blit( texture, targetTexture, _material);
		}

		private void OnPostRender( Camera camera)
		{
			if( _renderTexture != null && _drawQueued == false)
			{
				_drawQueued = true;
				GetEndOfFrameShim().StartCoroutine( DrawToScreen());
			}
		}

		private IEnumerator DrawToScreen()
		{
			yield return new WaitForEndOfFrame();
			GL.PushMatrix();
			GL.LoadPixelMatrix( 0, _renderTexture.width, _renderTexture.height, 0);
			_material.SetFloat( "_AlphaMultiplier", 0.5f);
			Graphics.DrawTexture( new Rect( 0, 0, _renderTexture.width, _renderTexture.height), _renderTexture, _material);
			GL.PopMatrix();

			RenderTexture.ReleaseTemporary( _renderTexture);
			_renderTexture = null;
			_drawQueued = false;
		}

		private RenderTexture GetTemporaryRenderTexture()
		{
			if( _renderTexture == null) 
			{
				_renderTexture = RenderTexture.GetTemporary( Camera.main.pixelWidth, Camera.main.pixelHeight, 16);
				_renderTexture.filterMode = FilterMode.Point;

				RenderTexture oldActive = RenderTexture.active;
				RenderTexture.active = _renderTexture;
				GL.Clear( true, true, Color.black);
				RenderTexture.active = oldActive;
			}

			return _renderTexture;
		}

		private EndOfFrameShim GetEndOfFrameShim()
		{
			if( _eofShim == null) {
				_eofShim = new GameObject().AddComponent<EndOfFrameShim>();
				_eofShim.gameObject.hideFlags = HideFlags.HideAndDontSave;
			}

			return _eofShim;
		}
	}
}
