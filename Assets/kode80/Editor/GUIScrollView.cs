using UnityEngine;
using UnityEditor;
using System.Collections;

namespace kode80.Editor
{
	public class GUIScrollView : GUIBaseContainer 
	{
		private Vector2 _scrollPosition;

		protected override void BeginContainerOnGUI()
		{
			_scrollPosition = EditorGUILayout.BeginScrollView( _scrollPosition);
		}
		
		protected override void EndContainerOnGUI()
		{
			EditorGUILayout.EndScrollView();
		}
	}
}