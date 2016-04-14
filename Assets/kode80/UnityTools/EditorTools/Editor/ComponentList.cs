//  Copyright (c) 2016, Ben Hopkins (kode80)
//  All rights reserved.
//  
//  Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
//  
//  1. Redistributions of source code must retain the above copyright notice, 
//     this list of conditions and the following disclaimer.
//  
//  2. Redistributions in binary form must reproduce the above copyright notice, 
//     this list of conditions and the following disclaimer in the documentation 
//     and/or other materials provided with the distribution.
//  
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
//  EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
//  MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL 
//  THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
//  SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT 
//  OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
//  HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
//  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
//  EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections;
using kode80.GUIWrapper;

namespace kode80.EditorTools
{
	public class ComponentList : EditorWindow 
	{
		private GUIVertical _gui;

		[MenuItem( "Window/kode80/Editor Tools/Component List")]
		public static void Init()
		{
			ComponentList win = EditorWindow.GetWindow( typeof( ComponentList)) as ComponentList;
			win.titleContent = new GUIContent( "Component List");
			win.Show();
		}

		public void RefreshList( GameObject gameObject)
		{
			_gui = new GUIVertical();

			if( gameObject == null)
			{
				_gui.Add( new GUILabel( new GUIContent( "Select a GameObject to edit it's component list.")));
			}
			else
			{
				_gui.Add( new GUIButton( new GUIContent( "Highlight " + gameObject.name), HighlightSelectedGameObjectClicked));

				GUIScrollView scrollView = new GUIScrollView();
				_gui.Add( scrollView);

				Component[] components = gameObject.GetComponents<Component>();
				int index = 0;
				int maxIndex = Math.Max( 0, components.Length - 1);
				foreach( Component component in components)
				{
					GUIContent componentName = new GUIContent( component.GetType().Name);
					GUIDelayedIntField field = new GUIDelayedIntField( componentName, 
																	   index++, 
																	   1,
																	   maxIndex,
																	   ComponentIndexChanged);
					
					// Transform is always first component & can't be reordered
					field.isEnabled = index > 1;

					scrollView.Add( field);
				}
			}

			Repaint();
		}

		#region GUI Actions

		void HighlightSelectedGameObjectClicked( GUIBase sender)
		{
			EditorGUIUtility.PingObject( SelectedGameObject());
		}

		void ComponentIndexChanged( GUIBase sender)
		{
			GUIDelayedIntField field = sender as GUIDelayedIntField;
			GameObject gameObject = SelectedGameObject();

			ReorderComponent( gameObject, field.previousValue, field.value);
			RefreshList( gameObject);
		}

		#endregion

		void ReorderComponent( GameObject gameObject, int index, int newIndex)
		{
			Component component = GetComponent( gameObject, index);
			if( component != null)
			{
				if( newIndex < index)
				{
					while( UnityEditorInternal.ComponentUtility.MoveComponentUp( component) && --index != newIndex) {}
				}
				else if( newIndex > index)
				{
					while( UnityEditorInternal.ComponentUtility.MoveComponentDown( component) && ++index != newIndex) {}
				}
			}
		}

		GameObject SelectedGameObject()
		{
			UnityEngine.Object[] gameObjects = Selection.GetFiltered( typeof( GameObject), 
																	  SelectionMode.Editable | SelectionMode.TopLevel);
			return gameObjects.Length > 0 ? gameObjects[0] as GameObject : null;
		}

		Component GetComponent( GameObject gameObject, int index)
		{
			if( gameObject != null)
			{
				Component[] components = gameObject.GetComponents<Component>();

				if( index >= 0 && index < components.Length)
				{
					return components[ index];
				}
			}

			return null;
		}

		void OnHierarchyChange()
		{
			RefreshList( SelectedGameObject());
		}

		void OnSelectionChange()
		{
			RefreshList( SelectedGameObject());
		}

		void OnEnable()
		{
			RefreshList( SelectedGameObject());
		}

		void OnDisable()
		{
			_gui = null;
		}

		void OnGUI()
		{
			if( _gui != null)
			{
				_gui.OnGUI();
			}
		}
	}
}
