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
using System.Collections;
using System;
using System.IO;

using kode80.GUIWrapper;
using kode80.Utils;
using kode80.Localization;


namespace kode80.Clouds
{
	public class CloudsEditor : EditorWindow
	{
		[SerializeField]
		private string _tempTexturePath;

		private FullScreenQuad _fullScreenQuad;
		private kode80Clouds _clouds;
		
		private Camera _cloudsCamera;
		private MonoBehaviour[] _cameraComponents;
		private Camera[] _cameras;
		private string[] _cameraNames;
		private Vector3 _cameraMovement;
		private Vector2 _mouseDelta;
		private Vector2 _lastMousePosition;
		private float _lastFrameTime;
		private int _cloudsCatchupFrames;
		private bool _dragBeganInRenderedView;
		private int _cameraIndex;
		private Rect _cameraRect;
		private int _mode;
		private bool _mouseDown;
		private bool _shiftDown;
		private bool _altDown;
		private bool _controlDown;
		private Vector2 _mousePositionAtControlDown;
		private float _cursorOriginalRadius;
		private Vector2 _scrollViewPosition;
		private bool _showSunInspector;
		private bool _showInspector;
		private bool _continuousUpdate;

		private TexturePainter _painter;
		private RenderTexture _coverage;

		private Texture _originalCoverage;

		private RenderTexture _renderTexture;

		private Clouds.EditorState _editorState;

		private Texture2D _testButton;


		[MenuItem( "Window/kode80/Clouds/Coverage Editor")]
		public static void Init()
		{
			CloudsEditor win = EditorWindow.GetWindow( typeof( CloudsEditor)) as CloudsEditor;
			win.titleContent = new GUIContent( "Coverage Editor");
			win.Show();
		}

		[MenuItem("Window/kode80/Clouds/Coverage Editor", true)]
		static bool ValidateMenu()
		{
			return GameObject.FindObjectOfType<kode80Clouds>() != null;
		}

		void Update()
		{
			if( EditorApplication.isPlayingOrWillChangePlaymode && EditorApplication.isPlaying == false)
			{
				// Unity's ordering of Enable/Disable & various isPlaying flags makes
				// reliably setting up/tearing down/saving etc. very convoluted when
				// user switches between Play/Edit. So for now just close window...
				// ...there *must* be a better way.
				Close();
				return;
			}

			RefreshRenderTexture();

			float fps = 1.0f / 30.0f;
			
			if( _cameraMovement.x != 0.0f || _cameraMovement.y != 0.0f || _cameraMovement.z != 0.0f ||
			    _mouseDelta.x != 0.0f || _mouseDelta.y != 0.0f)
			{
				SetNeedsUpdate();
			}
			
			if( _lastFrameTime <= Time.realtimeSinceStartup - fps)
			{
				float timeDelta = Time.realtimeSinceStartup - _lastFrameTime;

				if( _continuousUpdate || _cloudsCatchupFrames > 0)
				{
					_clouds.UpdateAnimatedProperties();

					if( _mode == 0)
					{
						float speed = 100.0f;
						_cloudsCamera.transform.position += _cameraMovement.x * _cloudsCamera.transform.right * timeDelta * speed;
						_cloudsCamera.transform.position += _cameraMovement.y * _cloudsCamera.transform.up * timeDelta * speed;
						_cloudsCamera.transform.position += _cameraMovement.z * _cloudsCamera.transform.forward * timeDelta * speed; 
						
						_cloudsCamera.transform.Rotate( _cloudsCamera.transform.right, _mouseDelta.y, Space.World);
						_cloudsCamera.transform.Rotate( Vector3.up, _mouseDelta.x, Space.World);
					}
					else if( _mode != 0)
					{
						if( _mouseDown)
						{
							Vector2 brushPoint = WindowToCoverage( _lastMousePosition);

							bool drawCoverage = _mode == 1;
							bool drawType = _mode == 2;
							float coverageOpacity = drawCoverage ? _editorState.cursorOpacity : 0.0f;
							float typeOpacity = drawType ? _editorState.cursorOpacity : 0.0f;
							float dir = _shiftDown ? -1.0f : 1.0f;
							dir = _editorState.cursorBlendValues ? dir : 1.0f;

							_painter.Render( brushPoint, 
							                 _editorState.cursorRadius, 
							                 coverageOpacity * dir, 
											 typeOpacity * dir, 
											 drawCoverage, drawType, 
											 _editorState.cursorBlendValues,
							                 _coverage, 
							                 _editorState.brushTexture);
							_editorState.MarkTempCoverageDirty();
						}
						else if( _controlDown)
						{
							float xDelta = _lastMousePosition.x - _mousePositionAtControlDown.x;
							float radiusDelta = xDelta / 200.0f;
							float radiusRange = 0.0f;

							if( xDelta > 0.0f) 
							{ 
								radiusRange = (EditorState.MaxCursorRadius - _cursorOriginalRadius) * Mathf.Min( 1.0f, radiusDelta);
							}
							else
							{ 
								radiusRange = _cursorOriginalRadius * Mathf.Max( -1.0f, radiusDelta);
							}

							_editorState.cursorRadius = _cursorOriginalRadius + radiusRange;
							_guiBrushSize.value = (int)_editorState.cursorRadius * 2;
						}
					}

					_mouseDelta = Vector2.zero;

					_cloudsCatchupFrames--;

					if( _clouds)
					{
						_clouds.SetCamera( _cloudsCamera);
						_clouds.RenderClouds();
						UpdateFullScreenQuadMaterial();
					}

					
					
					_fullScreenQuad.material.SetTexture( "_MainTex", _clouds.currentFrame);
					_cloudsCamera.Render();

					Repaint();
				}
				
				_lastFrameTime = Time.realtimeSinceStartup;
			}
		}

		private Vector2 WindowMousePositionToNormalizedViewPosition( Vector2 mousePosition)
		{
			mousePosition.x -= _cameraRect.x;
			mousePosition.y -= _cameraRect.y;

			return new Vector2( mousePosition.x / _cameraRect.width, 1.0f - mousePosition.y / _cameraRect.height);
		}

		private Vector3 WindowToWorld( Vector2 position)
		{
			Vector2 normalizedPoint = WindowMousePositionToNormalizedViewPosition( position);
			return  _clouds.cloudsSharedProperties.NormalizedPointToAtmosphere( normalizedPoint, _cloudsCamera);
		}

		private Vector2 WindowToCoverage( Vector2 position, bool includeOffset=true)
		{
			Vector3 world = WindowToWorld( position);
			world /= _clouds.cloudsSharedProperties.maxDistance;
			world.x = world.x * 0.5f + 0.5f;
			world.z = world.z * 0.5f + 0.5f;

			if( includeOffset)
			{
				world.x += _clouds.coverageOffsetX;
				world.z += _clouds.coverageOffsetY;
				world.x -= Mathf.Floor( world.x);
				world.z -= Mathf.Floor( world.z);
			}

			return new Vector2( world.x, world.z);
		}
		
		private void UpdateFullScreenQuadMaterial()
		{
			Vector3 cursor = Vector3.zero;

			if( _mode != 0 && _controlDown == false)
			{
				cursor = WindowToWorld( _lastMousePosition);
				_fullScreenQuad.material.SetVector( "_Cursor", cursor);
			}

			float pixelToCloudSpace = _clouds.cloudsSharedProperties.maxDistance / _coverage.width;

			_fullScreenQuad.material.SetTexture( "_Coverage", _coverage);
			_fullScreenQuad.material.SetFloat( "_DrawCoverage", 0.0f);
			_fullScreenQuad.material.SetFloat( "_DrawType", 0.0f);
			_fullScreenQuad.material.SetFloat( "_DrawCursor", _mode != 0 ? 1.0f : 0.0f);
			_fullScreenQuad.material.SetFloat( "_CursorRadius", _editorState.cursorRadius * pixelToCloudSpace);
			_fullScreenQuad.material.SetInt( "_IsGamma", QualitySettings.activeColorSpace == ColorSpace.Gamma ? 1 : 0);
			_clouds.cloudsSharedProperties.ApplyToMaterial( _fullScreenQuad.material);
		}

		void OnFocus()
		{
		}

		void OnLostFocus()
		{
		}

		private Clouds.EditorState FindOrCreateEditorState()
		{
			Clouds.EditorState state = GameObject.FindObjectOfType<Clouds.EditorState>();
			if( state == null)
			{
				GameObject stateGO = new GameObject( "CloudsEditorState");
				stateGO.tag = "EditorOnly";
				stateGO.hideFlags = HideFlags.HideInHierarchy;
				state = stateGO.AddComponent<Clouds.EditorState>();
			}

			return state;
		}

		private FullScreenQuad CreateFullScreenQuad()
		{
			GameObject quadGO = EditorUtility.CreateGameObjectWithHideFlags( "CloudsEditorQuad", 
			                                                                HideFlags.HideAndDontSave, 
			                                                                typeof(FullScreenQuad));
			quadGO.tag = "EditorOnly";
			
			Material material = new Material( Shader.Find( "Hidden/kode80/CloudBlenderEditor"));
			material.hideFlags = HideFlags.HideAndDontSave;
			FullScreenQuad quad = quadGO.GetComponent<FullScreenQuad>();
			quad.material = material;

			return quad;
		}

		private Camera CreateCloudsCamera()
		{
			GameObject cameraGO;

			Camera cloudsCamera = _clouds.targetCamera ?? Camera.main;

			if( cloudsCamera != null)
			{
				cameraGO = Instantiate( cloudsCamera.gameObject);
				cameraGO.name = "CloudsEditorCamera";
				cameraGO.hideFlags = HideFlags.HideAndDontSave;

				_cameraComponents = cameraGO.GetComponents<MonoBehaviour>();
				_editorState.cameraComponentStates.ApplyStates( _cameraComponents);

                AudioListener listener = cameraGO.GetComponent<AudioListener>();
                if( listener != null)
                {
                    DestroyImmediate(listener);
                }
			}
			else
			{
				cameraGO = EditorUtility.CreateGameObjectWithHideFlags( "CloudsEditorCamera", 
				                                                       HideFlags.HideAndDontSave, 
				                                                       typeof(Camera));
			}
			cameraGO.tag = "EditorOnly";

			Camera cam = cameraGO.GetComponent<Camera>();
			cam.hideFlags = HideFlags.HideAndDontSave;
			cam.renderingPath = RenderingPath.DeferredShading;
			cam.enabled = false;
			cam.cameraType = CameraType.SceneView;

			return cam;
		}

		void OnEnable()
		{
			minSize = new Vector2( 350.0f, 150.0f);
			_testButton = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/kode80/Clouds/Editor/gui/button_camera.png");


			_editorState = FindOrCreateEditorState();
			bool isWindowOpening = _editorState.EditorWindowEnabled();

			_clouds = GameObject.FindObjectOfType<kode80Clouds>();

			_fullScreenQuad = CreateFullScreenQuad();
			_cloudsCamera = CreateCloudsCamera();
            _fullScreenQuad.targetCamera = _cloudsCamera;

			_clouds.SetCamera( _cloudsCamera);
			_clouds.onValidateDelegate += OnCloudsValidate;

			CreateGUI();

			_painter = CreateInstance<TexturePainter>();
			_painter.hideFlags = HideFlags.HideAndDontSave;

			_coverage = _editorState.tempCoverage;
			if( isWindowOpening)
			{
				if( _clouds.cloudCoverage != null)
				{
					_editorState.CopyCoverageAssetToTemp( _clouds.cloudCoverage as Texture2D);
				}
				else
				{
					ClearCoverage();
				}
			}

			_originalCoverage = _clouds.cloudCoverage;
			_clouds.cloudCoverage = _coverage;

			UpdateFullScreenQuadMaterial();

			wantsMouseMove = true;
			SetNeedsUpdate();	
			_lastFrameTime = Time.realtimeSinceStartup;
			RefreshCamerasArray();
			RefreshRenderTexture();
			
			EditorApplication.modifierKeysChanged += OnModifierKeysChanged;
		}

		void ClearCoverage()
		{
			_editorState.ClearTempCoverage();
			SetNeedsUpdate();
		}

		void RefreshRenderTexture()
		{
			int w = (int)_cameraRect.width;
			int h = (int)_cameraRect.height;

			if( _renderTexture == null || _renderTexture.width != w || _renderTexture.height != h)
			{
				if( _renderTexture)
				{
					_renderTexture.Release();
				}
				_renderTexture = new RenderTexture( w, h, 16, RenderTextureFormat.Default);
				_renderTexture.hideFlags = HideFlags.HideAndDontSave;
				_cloudsCamera.targetTexture = _renderTexture;

				SetNeedsUpdate();
			}
		}
		
		void OnDisable()
		{
			if( _testButton)
			{
				Resources.UnloadAsset( _testButton);
				_testButton = null;
			}

			_editorState.EditorWindowDisabled();
			EditorApplication.modifierKeysChanged -= OnModifierKeysChanged;
			_coverage = null;

			if( _cloudsCamera)
			{
				DestroyImmediate( _cloudsCamera.gameObject);
				_cloudsCamera = null;
			}

			if( _renderTexture)
			{
				DestroyImmediate( _renderTexture);
				_renderTexture = null;
			}

			if( _fullScreenQuad)
			{
				DestroyImmediate( _fullScreenQuad.gameObject);
				_fullScreenQuad = null;
			}

			if( _painter)
			{
				DestroyImmediate( _painter);
				_painter = null;
			}

			if( _clouds)
			{
				_clouds.onValidateDelegate -= OnCloudsValidate;
				_clouds.cloudCoverage = _originalCoverage;
				_clouds = null;
			}
		}

		void OnDestroy()
		{
		}

		void OnCloudsValidate()
		{
			SetNeedsUpdate();
		}

		void OnModifierKeysChanged()
		{
			// OnGUI modifier key up/down events don't work reliably (at all?) on OSX
			// The modifierKeysChanged delegate *does* work reliably but provides no
			// way of detecting which modifiers changed (Event.current is always null)
			// Calling Repaint() here forces an OnGUI call where we can use Event.current
			// to manually implement modifier up/down.  *sigh*
			Repaint();
		}

		void OnHierarchyChange()
		{
			RefreshCamerasArray();
			Repaint();
		}

		string PresentNewCoverageDialog()
		{
			return EditorUtility.SaveFilePanelInProject( "New Coverage Map",
			                                             "NewCoverage",
			                                             "png",
			                                             "Save a new coverage map texture.");
		}

		bool CanWriteNewCoverageTo( string path)
		{
			if( File.Exists( path))
			{
				return EditorUtility.DisplayDialog( "File already exists",
				       		                        "The file already exists, are you sure you want to overwrite?",
				                                    "Yes",
				                                    "No");
			}

			return true;
		}

		private GUIVertical _gui;
		private GUIVertical _guiPropertiesPanel;
		private GUIVertical _guiScenePlaceholder;
		private GUIToolbar _guiToolbar;
		private GUIToggle _guiBrushBlendValues;
		private GUIIntSlider _guiBrushSize;
		private GUISlider _guiBrushOpacity;
		private GUITextureField _guiBrushTexture;
		private GUIVector3Field _guiSunRotation;
		private GUIColorField _guiSunColor;
		private GUIFoldout _guiCameraFoldout;

		void TogglePropertiesPanel( GUIBase sender)
		{
			_guiPropertiesPanel.isHidden = !_guiPropertiesPanel.isHidden;
		}

		GUIFoldout CreateHelpFoldout()
		{
			GUIFoldout foldout = new GUIFoldout( Localize.GUI( "Help"));
			GUIHorizontal indentLayout = foldout.Add( new GUIHorizontal()) as GUIHorizontal;
			indentLayout.Add( new GUISpace());
			GUIVertical layout = indentLayout.Add( new GUIVertical()) as GUIVertical;
			layout.Add( new GUILabel( Localize.GUI( "Alt/Alt+Shift: cycle editor modes")));
			layout.Add( new GUILabel( Localize.GUI( "Camera Mode:\nA/S/W/D to move\nLMB to look")));
			layout.Add( new GUILabel( Localize.GUI( "Coverage Paint Mode:\nLMB to increase coverage\nLMB+Shift to decrease coverage")));
			layout.Add( new GUILabel( Localize.GUI( "Cloud Type Paint Mode:\nLMB to increase cloud type\nLMB+Shift to decrease cloud type")));
			layout.Add( new GUILabel( Localize.GUI( "Brush Properties:\nBrush size, opacity & texture can be altered\nfrom the Brush Properties foldout.\nHolding Control while moving the mouse\nin either paint mode adjusts brush size.")));
			layout.Add( new GUIButton( Localize.GUI( "Display full documentation")));
			return foldout;
		}

		void CreateGUI()
		{
			int padding = 4;
			GUIStyle style = new GUIStyle();
			style.padding = new RectOffset( padding, padding, padding, padding);

			_gui = new GUIVertical();

			GUIHorizontal toolLayout = new GUIHorizontal( style);
			toolLayout.Add( new GUIButton( Localize.GUI( null, "Toggle properties panel", 
			                                            "Assets/kode80/Clouds/Editor/gui/button_properties.png"), 
			                               TogglePropertiesPanel));
			toolLayout.Add( new GUISpace());
			toolLayout.Add( new GUIButton( Localize.GUI( null, "Create a new coverage map", 
			                                            "Assets/kode80/Clouds/Editor/gui/button_new.png"), 
			                               NewCoverageMapAction));
			toolLayout.Add( new GUIButton( Localize.GUI( null, "Save the current coverage map", 
			                                            "Assets/kode80/Clouds/Editor/gui/button_save.png"),
			                              SaveCoverageMapAction));
			toolLayout.Add( new GUIButton( Localize.GUI( null, "Save the current coverage map as a new file", 
			                                            "Assets/kode80/Clouds/Editor/gui/button_saveas.png"),
			                              SaveCoverageMapAsAction));
			toolLayout.Add( new GUISpace());
			toolLayout.Add( new GUIButton( Localize.GUI( null, "Export cubemap from current camera", 
			                                            "Assets/kode80/Clouds/Editor/gui/button_cubemap.png"),
			                               ExportCubemapAction));

			toolLayout.Add( new GUISpace( true));
			GUIContent[] toolbarContent = new GUIContent[] {
				Localize.GUI( null, null, "Assets/kode80/Clouds/Editor/gui/button_camera.png"),
				Localize.GUI( null, null, "Assets/kode80/Clouds/Editor/gui/button_coverage.png"),
				Localize.GUI( null, null, "Assets/kode80/Clouds/Editor/gui/button_type.png")
			};
			_guiToolbar = toolLayout.Add( new GUIToolbar( toolbarContent, ChangeModeAction)) as GUIToolbar;
			toolLayout.Add( new GUISpace( true));
			toolLayout.Add( new GUIButton( Localize.GUI( null, "Clear the current coverage map", 
			                                            "Assets/kode80/Clouds/Editor/gui/button_clearmap.png"),
			                              ClearCoverageMapAction));

			GUIFoldout helpFoldout = CreateHelpFoldout();

			GUIFoldout editorFoldout = new GUIFoldout( Localize.GUI ( "Editor Properties"));
			editorFoldout.Add( new GUIToggle( Localize.GUI( "Continuous Update", 
			                                                "If disabled, the editor will only render on changes"), 
			                                  ContinuousUpdateToggleAction));

			if( _cameraComponents.Length > 0)
			{
				_guiCameraFoldout = new GUIFoldout( Localize.GUI( "Editor Camera"));
				foreach( MonoBehaviour component in _cameraComponents)
				{
					GUIToggle toggle = new GUIToggle( new GUIContent( component.GetType().Name), CameraComponentToggled);
					toggle.isToggled = component.enabled;
					_guiCameraFoldout.Add( toggle);
				}
			}

			GUIFoldout brushFoldout = new GUIFoldout( Localize.GUI( "Brush Properties"));
			_guiBrushBlendValues = brushFoldout.Add( new GUIToggle( Localize.GUI( "Blend Values", "Blend values when painting or set to a specific value"), 
																	UpdateBrushPropertiesAction)) as GUIToggle;
			_guiBrushOpacity = brushFoldout.Add( new GUISlider( Localize.GUI( "Opacity", "Brush opacity"), 
			                                                    0.2f, 0.0f, 1.0f, 
			                                                    UpdateBrushPropertiesAction)) as GUISlider;
			_guiBrushSize = brushFoldout.Add( new GUIIntSlider( Localize.GUI( "Size", "Brush size"), 2, 2, 
			                                                    (int)EditorState.MaxCursorRadius, 
			                                                    UpdateBrushPropertiesAction)) as GUIIntSlider;
			_guiBrushTexture = brushFoldout.Add( new GUITextureField( Localize.GUI( "Brush", "Brush texture"), 
			                                                          UpdateBrushPropertiesAction)) as GUITextureField;

			GUIFoldout sunFoldout = new GUIFoldout( Localize.GUI( "Sun Properties"));
			_guiSunRotation = sunFoldout.Add( new GUIVector3Field( Localize.GUI( "Rotation", "Sun's rotation"), 
			                                                       UpdateSunPropertiesAction)) as GUIVector3Field;
			_guiSunColor = sunFoldout.Add( new GUIColorField( Localize.GUI( "Color", "Sun's color"), 
			                                                  UpdateSunPropertiesAction)) as GUIColorField;

			GUIFoldout cloudsFoldout = new GUIFoldout( Localize.GUI( "Clouds Properties"));

			GUIHorizontal subLayout = new GUIHorizontal();
			subLayout.Add( new GUISpace());
			subLayout.Add( new GUIButton( Localize.GUI( "Load Settings", 
			                                            "Load key render settings from asset"), 
			                              LoadRenderSettingsAction));
			subLayout.Add( new GUIButton( Localize.GUI( "Save Settings", 
			                                            "Save key render settings to asset"), 
			                              SaveRenderSettingsAction));

			cloudsFoldout.Add( subLayout);
			cloudsFoldout.Add( new GUISpace());
			cloudsFoldout.Add( new GUIDefaultInspector( _clouds));

			GUIScrollView scrollView = new GUIScrollView();
			scrollView.Add( helpFoldout);
			scrollView.Add( editorFoldout);
			if( _cameraComponents.Length > 0) { scrollView.Add( _guiCameraFoldout); }
			scrollView.Add( brushFoldout);
			scrollView.Add( sunFoldout);
			scrollView.Add( cloudsFoldout);

			_guiPropertiesPanel = new GUIVertical( GUILayout.MaxWidth(320.0f));
			_guiPropertiesPanel.Add( scrollView);

			_guiScenePlaceholder = new GUIVertical( GUILayout.ExpandWidth( true), GUILayout.ExpandHeight( true));
			_guiScenePlaceholder.shouldStoreLastRect = true;

			GUIHorizontal lowerLayout = new GUIHorizontal();
			lowerLayout.Add( _guiPropertiesPanel);
			lowerLayout.Add( _guiScenePlaceholder);

			_gui.Add( toolLayout);
			_gui.Add( lowerLayout);

			// Update properties
			_guiBrushBlendValues.isToggled = _editorState.cursorBlendValues;
			_guiBrushOpacity.value = _editorState.cursorOpacity;
			_guiBrushSize.value = (int)_editorState.cursorRadius * 2;
			_guiBrushTexture.texture = _editorState.brushTexture;
			_guiSunColor.color = _clouds.sunLight.color;
			_guiSunRotation.vector = _clouds.sunLight.transform.eulerAngles;
		}

		void ContinuousUpdateToggleAction( GUIBase sender)
		{
			GUIToggle toggle = sender as GUIToggle;
			_continuousUpdate = toggle.isToggled;
			SetNeedsUpdate();
		}

		void NewCoverageMapAction( GUIBase sender)
		{
			string newPath = PresentNewCoverageDialog();
			if( CanWriteNewCoverageTo( newPath))
			{
				ClearCoverage();
				_editorState.MarkTempCoverageDirty();
				_editorState.coveragePath = newPath;
				_editorState.SaveTempCoverage();
				
				_originalCoverage = AssetDatabase.LoadAssetAtPath<Texture2D>( newPath);
			}
		}

		void CameraComponentToggled( GUIBase sender)
		{
			GUIToggle toggle = sender as GUIToggle;

			MonoBehaviour component = Array.Find<MonoBehaviour>( _cameraComponents, x => x.GetType().Name == toggle.content.text);
			component.enabled = !component.enabled;
			toggle.isToggled = component.enabled;
			SetNeedsUpdate();

			_editorState.cameraComponentStates.StoreStates( _cameraComponents);
			_editorState.MarkSceneDirty();
		}

		void SaveCoverageMapAction( GUIBase sender)
		{
			_editorState.SaveTempCoverage();
		}

		void SaveCoverageMapAsAction( GUIBase sender)
		{
			EditorUtility.SaveFilePanelInProject( "Save Coverage As", "NewCoverage", "png", "Save the coverage as a new texture asset");
		}

		void ExportCubemapAction( GUIBase sender)
		{
			CubemapExporter exporter = new CubemapExporter();
			exporter.ExportCubemap( _clouds, _fullScreenQuad, _cloudsCamera);
		}

		void ClearCoverageMapAction( GUIBase sender)
		{
			bool canClear = true;

			if( _editorState.tempCoverageNeedsSaving)
			{
				canClear = EditorUtility.DisplayDialog( "Unsaved changes",
				                                        "You currently have unsaved changes, are you sure you want to clear the coverage map?",
				                                        "Yes",
				                                        "No");
			}

			if( canClear)
			{
				ClearCoverage();
			}
		}

		void ChangeModeAction( GUIBase sender)
		{
			GUIToolbar toolbar = sender as GUIToolbar;
			_mode = toolbar.selected;
			SetNeedsUpdate();
		}

		void UpdateSunPropertiesAction( GUIBase sender)
		{
			_clouds.sunLight.color = _guiSunColor.color;
			_clouds.sunLight.transform.eulerAngles = _guiSunRotation.vector;

			SetNeedsUpdate();
		}

		void UpdateBrushPropertiesAction( GUIBase sender)
		{
			_editorState.cursorBlendValues = _guiBrushBlendValues.isToggled;
			_editorState.cursorRadius = _guiBrushSize.value / 2;
			_editorState.cursorOpacity = _guiBrushOpacity.value;
			_editorState.brushTexture = _guiBrushTexture.texture;

			SetNeedsUpdate();
		}

		void LoadRenderSettingsAction( GUIBase sender)
		{
			string path = EditorUtility.OpenFilePanel( "Load Render Settings", "Assets", "asset");
			path = path.Length != 0 ? UnityFileUtility.AssetRelativePath( path) : null;
			
			if( path != null)
			{
				Clouds.RenderSettings settings = AssetDatabase.LoadAssetAtPath<Clouds.RenderSettings>( path);

				if( settings == null)
				{
					EditorUtility.DisplayDialog( Localize.STR( "Error"),
					                             Localize.STR( "Couldn't load Clouds.RenderSettings asset"),
					                             Localize.STR ( "Ok"));
				}
				else
				{
					_clouds.CopyRenderSettingsToProperties( settings);
					_guiSunColor.color = _clouds.sunLight.color;
					_guiSunRotation.vector = _clouds.sunLight.transform.eulerAngles;
				}
			}
		}

		void SaveRenderSettingsAction( GUIBase sender)
		{
			string path = EditorUtility.SaveFilePanel( "Save Render Settings", "Assets", "New CloudsRenderSettings", "asset");
			path = path.Length != 0 ? UnityFileUtility.AssetRelativePath( path) : null;

			if( path != null)
			{
				Clouds.RenderSettings settings = ScriptableObject.CreateInstance<Clouds.RenderSettings>();
				_clouds.CopyPropertiesToRenderSettings( settings);
				AssetDatabase.CreateAsset( settings, path);
				AssetDatabase.Refresh();
			}
		}

		void OnGUI()
		{
			if( _gui == null)
			{
				return;
			}

			_gui.OnGUI();
			_cameraRect = _guiScenePlaceholder.lastRect;

			Event e = Event.current;
			_shiftDown = e.shift;
			
			if( e.alt != _altDown)
			{
				_altDown = e.alt;
				if( _altDown && e.shift)
				{
					_mode--;
					if( _mode < 0) { _mode = 2; }
				}
				else if( _altDown)
				{
					_mode++;
					if( _mode > 2) { _mode = 0; }
				}

				_guiToolbar.selected = _mode;
			}
			
			if( e.control != _controlDown)
			{
				_controlDown = e.control;
				if( _controlDown)
				{
					_mousePositionAtControlDown = e.mousePosition;
					_cursorOriginalRadius = _editorState.cursorRadius;
				}
			}

			if( e.rawType == EventType.MouseDown)
			{
				_dragBeganInRenderedView = IsMouseInRenderedView();

				if( _dragBeganInRenderedView)
				{
					_mouseDown = true;
					Focus();
					SetNeedsUpdate();
					e.Use();
				}
			}
			else if( e.rawType == EventType.MouseUp)
			{
				_mouseDown = false;

				if( _dragBeganInRenderedView)
				{
					_dragBeganInRenderedView = false;
					e.Use();
				}
			}
			else if( e.rawType == EventType.MouseDrag && _dragBeganInRenderedView)
			{
				_lastMousePosition = e.mousePosition;

				float s = 0.2f;
				_mouseDelta.x += e.delta.x * s;
				_mouseDelta.y += e.delta.y * s;
				e.Use();
			}
			else if( e.rawType == EventType.MouseMove && _mode != 0)
			{
				_lastMousePosition = e.mousePosition;
				SetNeedsUpdate();
				e.Use();
			}
			else if( e.rawType == EventType.KeyDown)
			{
				HandleKeyEvent( e, true);
			}
			else if( e.rawType == EventType.KeyUp)
			{
				HandleKeyEvent( e, false);
			}
			else if( e.rawType == EventType.Repaint)
			{
				EditorGUI.DrawPreviewTexture( _cameraRect, _renderTexture);

				if( _coverage)
				{
					Rect coverageRect = new Rect( 10.0f, 10.0f, 128.0f, 128.0f);
					coverageRect.x += _cameraRect.x;
					coverageRect.y = _cameraRect.yMax - 10.0f - coverageRect.height;

					Rect coverageUV = new Rect( _clouds.coverageOffsetX,
					                            _clouds.coverageOffsetY,
					                           	1.0f, 1.0f);
					//EditorGUI.DrawPreviewTexture( coverageRect, _coverage);
					GUI.DrawTextureWithTexCoords( coverageRect, _coverage, coverageUV);
					
					Vector2 cursorPosition = WindowToCoverage( _lastMousePosition, false);
					cursorPosition.y = 1.0f - cursorPosition.y;
					cursorPosition *= coverageRect.width;
					cursorPosition.x += coverageRect.x;
					cursorPosition.y += coverageRect.y;

					float size = Mathf.Max( 2.0f, _editorState.cursorRadius / 2.0f);

					Handles.BeginGUI();
					Handles.CircleCap( 0, new Vector2( cursorPosition.x, cursorPosition.y), Quaternion.identity, size);
					Handles.CircleCap( 0, coverageRect.center, Quaternion.identity, coverageRect.width * 0.5f);
					Handles.EndGUI();
				}
			}
		}

		private void RefreshCamerasArray()
		{
			Camera[] sceneCameras = GameObject.FindObjectsOfType(typeof(Camera)) as Camera[];
			int count = sceneCameras.Length + 1;
				
			_cameras = new Camera[ count];
			_cameraNames = new string[ count];
			
			_cameras[ 0] = _cloudsCamera;
			_cameraNames[ 0] = _cloudsCamera.name;
			
			for( int i=1; i<count; i++)
			{
				Camera cam = sceneCameras[ i - 1];
				_cameras[ i] = cam;
				_cameraNames[ i] = cam.name;
			}
		}

		private void SetNeedsUpdate()
		{
			int pixelSize = _clouds.cloudsSharedProperties.subPixelSize;
			_cloudsCatchupFrames = pixelSize * pixelSize + 1;
			Repaint();
		}

		private bool IsMouseInRenderedView()
		{
			Rect rect = _cameraRect;
			return rect.Contains( Event.current.mousePosition);
		}
		
		private void HandleKeyEvent( Event e, bool isDown)
		{
			bool used = false;
			
			if( e.keyCode == KeyCode.A) 
			{ 
				_cameraMovement.x = isDown ? -1.0f : 0.0f;
				used = true; 
			}
			else if( e.keyCode == KeyCode.D) 
			{ 
				_cameraMovement.x = isDown ? 1.0f : 0.0f; 
				used = true;
			}
			
			if( e.keyCode == KeyCode.Q) 
			{ 
				_cameraMovement.y = isDown ? -1.0f : 0.0f;
				used = true; 
			}
			else if( e.keyCode == KeyCode.E) 
			{ 
				_cameraMovement.y = isDown ? 1.0f : 0.0f;
				used = true; 
			}
			
			if( e.keyCode == KeyCode.S) 
			{ 
				_cameraMovement.z = isDown ? -1.0f : 0.0f;
				used = true;
			}
			else if( e.keyCode == KeyCode.W) 
			{ 
				_cameraMovement.z = isDown ? 1.0f : 0.0f;
				used = true; 
			}
			
			if( used)
			{
				e.Use();
			}
		}
	}
}	