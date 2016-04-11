using UnityEngine;
using UnityEditor;
using System.Collections;

namespace kode80.Editor
{
	public class GUIVertical : GUIBaseContainer 
	{
		public GUILayoutOption[] layoutOptions;
		public GUIStyle style;

		public GUIVertical( GUIStyle style, params GUILayoutOption[] options)
		{
			layoutOptions = options;
			this.style = style;
		}

		public GUIVertical( params GUILayoutOption[] options)
		{
			layoutOptions = options;
			this.style = new GUIStyle();
		}

		protected override void BeginContainerOnGUI()
		{
			EditorGUILayout.BeginVertical( style, layoutOptions);
		}
		
		protected override void EndContainerOnGUI()
		{
			EditorGUILayout.EndVertical();
		}
	}
}