using UnityEngine;
using UnityEditor;
using System.Collections;

namespace kode80.Editor
{
	public class GUIHorizontal : GUIBaseContainer 
	{
		public GUILayoutOption[] layoutOptions;
		public GUIStyle style;

		public GUIHorizontal( GUIStyle style, params GUILayoutOption[] options)
		{
			layoutOptions = options;
			this.style = style;
		}
		
		public GUIHorizontal( params GUILayoutOption[] options)
		{
			layoutOptions = options;
			this.style = new GUIStyle();
		}
		protected override void BeginContainerOnGUI()
		{
			EditorGUILayout.BeginHorizontal( style, layoutOptions);
		}
		
		protected override void EndContainerOnGUI()
		{
			EditorGUILayout.EndHorizontal();
		}
	}
}
