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
using System;
using System.IO;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
	#if UNITY_5_3
	using UnityEditor.SceneManagement;
	#endif
#endif


namespace kode80.Clouds
{
	public class EditorState : MonoBehaviour
	{
		
#if UNITY_EDITOR

		public BehaviorStateStore cameraComponentStates;

		public const float MaxCursorRadius = 256.0f;//128.0f;//50.0f;

		public bool cursorBlendValues = false;
		public Texture2D brushTexture;

		[SerializeField]
		public float _cursorOpacity = 0.1f;
		public float cursorOpacity {
			get { return _cursorOpacity; }
			set {
				if( _cursorOpacity != value)
				{
					_cursorOpacity = value;
					MarkSceneDirty();
				}
			}
		}

		[SerializeField]
		public float _cursorRadius = 5.0f;
		public float cursorRadius {
			get { return _cursorRadius; }
			set {
				if( _cursorRadius != value)
				{
					_cursorRadius = value;
					MarkSceneDirty();
				}
			}
		}

		// Work around for Unity's *stupid* EditorWindow handling
		// so we can reliably detect if the editor window has closed
		// (and the user can be alerted to any unsaved coverage changes)
		private int _windowRefCount = 0;

		public string coveragePath;
		private RenderTexture _tempCoverage;
		private bool _tempCoverageDirty;
		public bool tempCoverageNeedsSaving { get { return _tempCoverageDirty; } }
		public RenderTexture tempCoverage { get { return _tempCoverage; } }


		#region Public 

		void Reset()
		{
			if( cameraComponentStates == null)
			{
				cameraComponentStates = new BehaviorStateStore();
			}
		}

		public void MarkSceneDirty()
		{
		#if UNITY_5_3
			EditorSceneManager.MarkSceneDirty( EditorSceneManager.GetActiveScene());
		#else
			EditorApplication.MarkSceneDirty();
		#endif
		}

		public bool EditorWindowEnabled()
		{
			_windowRefCount++;
			bool isWindowOpening = _windowRefCount == 1;

			if( isWindowOpening) 
			{
				CreateTempCoverage();
			}

			return isWindowOpening;
		}

		public bool EditorWindowDisabled()
		{
			_windowRefCount--;
			bool isWindowClosing = _windowRefCount == 0;

			if( isWindowClosing) 
			{ 
				if( _tempCoverageDirty)
				{
					bool shouldSave = EditorUtility.DisplayDialog( "Unsaved changes",
					                                               "Do you want to save the coverage map? Unsaved changes will be lost",
					                                               "Yes",
					                                               "No");
					if( shouldSave)
					{
						SaveTempCoverage();
					}
				}

				DestroyTempCoverage();
			}

			return isWindowClosing;
		}

		public void ExecuteGUIForBrushTexture()
		{
			Texture2D newTexture = EditorGUILayout.ObjectField( "Brush", brushTexture, typeof( Texture2D), false) as Texture2D;
			if( newTexture != brushTexture)
			{
				brushTexture = newTexture;
				MarkSceneDirty();
			}
		}

		public bool ExecuteGUISliderForCursorOpacity( float width)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label( "Opacity");
			float newOpacity = EditorGUILayout.Slider( cursorOpacity, 0.02f, 0.5f, GUILayout.Width(width));
			bool changed = newOpacity != cursorOpacity;
			cursorOpacity = newOpacity;
			GUILayout.EndHorizontal();

			return changed;
		}
		
		public bool ExecuteGUISliderForCursorRadius( float width)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label( "Size");
			float newSize = EditorGUILayout.IntSlider( (int)(cursorRadius * 2.0f), 2, (int)MaxCursorRadius, GUILayout.Width(width));
			bool changed = newSize != (cursorRadius * 2);
			cursorRadius = newSize / 2.0f;
			GUILayout.EndHorizontal();

			return changed;
		}

		public void MarkTempCoverageDirty()
		{
			_tempCoverageDirty = true;
		}

		public void ClearTempCoverage()
		{
			RenderTexture previous = RenderTexture.active;
			RenderTexture.active = _tempCoverage;
			GL.Clear( true, true, Color.black);
			RenderTexture.active = previous;

			MarkTempCoverageDirty();
		}

		public void CopyCoverageAssetToTemp( Texture2D asset)
		{
			string path = AssetDatabase.GetAssetPath( asset);
			if( path == null)
			{
				throw new ArgumentException( "Tried copying coverage to temp but asset doesn't exist");
			}

			RenderTexture previousRT = RenderTexture.active;
			Graphics.Blit( asset, _tempCoverage);
			RenderTexture.active = previousRT;
			coveragePath = path;
			_tempCoverageDirty = false;
		}
		
		public void SaveTempCoverage()
		{
			if( coveragePath != null && _tempCoverageDirty)
			{
				SaveTempCoverageTo( coveragePath);
			}
		}

		#endregion

		#region Private

		private void CreateTempCoverage()
		{
			DestroyTempCoverage();
			
			_tempCoverage = new RenderTexture( 512, 512, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
			_tempCoverage.hideFlags = HideFlags.HideAndDontSave;
			_tempCoverage.wrapMode = TextureWrapMode.Repeat;
			_tempCoverage.useMipMap = true;

			RenderTexture previousRT = RenderTexture.active;
			Graphics.Blit( Texture2D.blackTexture, _tempCoverage);
			RenderTexture.active = previousRT;
		}

		private void SaveTempCoverageTo( string path)
		{
			if( _tempCoverage == null)
			{
				throw new NullReferenceException( "Attempted to save null temp coverage");
			}

			RenderTexture oldActive = RenderTexture.active;
			Texture2D exported = new Texture2D( _tempCoverage.width, _tempCoverage.height, TextureFormat.ARGB32, false);
			RenderTexture.active = _tempCoverage;
			exported.ReadPixels( new Rect( 0.0f, 0.0f, _tempCoverage.width, _tempCoverage.height), 0, 0);
			RenderTexture.active = oldActive;

			byte[] bytes = exported.EncodeToPNG();
			DestroyImmediate( exported);

			if( bytes != null)
			{
				File.WriteAllBytes( path, bytes);
				_tempCoverageDirty = false;
                AssetDatabase.Refresh();
                
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                importer.textureType = TextureImporterType.Advanced;
                importer.generateMipsInLinearSpace = true;
                importer.linearTexture = true;
                importer.textureFormat = TextureImporterFormat.AutomaticTruecolor;
                importer.SaveAndReimport();

				AssetDatabase.Refresh();
			}
		}

		private void DestroyTempCoverage()
		{
			if( _tempCoverage != null)
			{
				DestroyImmediate( _tempCoverage);
				_tempCoverage = null;
			}

			coveragePath = null;
		}

		#endregion

#endif

	}
}