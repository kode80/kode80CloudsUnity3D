using UnityEngine;
using UnityEditor;
using System.Collections;

namespace kode80.Editor
{
	public class GUIFoldout : GUIBaseContainer 
	{
		public bool isOpen;

		private GUIContent _content;
		public GUIContent content { get { return _content; } }

		protected override bool areChildrenHidden { get { return isOpen == false; } }
		
		public GUIFoldout( GUIContent content)
		{
			_content = content;
		}
		
		protected override void BeginContainerOnGUI()
		{
			isOpen = EditorGUILayout.Foldout( isOpen, _content);
			if( isOpen)
			{
				EditorGUILayout.BeginVertical();
				EditorGUI.indentLevel++;
			}
		}
		
		protected override void EndContainerOnGUI()
		{
			if( isOpen)
			{
				EditorGUI.indentLevel--;
				EditorGUILayout.EndHorizontal();
			}
		}
	}
}