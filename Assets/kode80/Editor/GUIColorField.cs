using UnityEngine;
using UnityEditor;
using System.Collections;

namespace kode80.Editor
{
	public class GUIColorField : GUIBase 
	{
		public Color color;
		
		private GUIContent _content;
		public GUIContent content { get { return _content; } }
		
		public GUIColorField( GUIContent content, OnGUIAction action=null)
		{
			_content = content;
			if( action != null)
			{
				onGUIAction += action;
			}
		}
		
		protected override void CustomOnGUI ()
		{
			Color newColor = EditorGUILayout.ColorField( _content, color);
			if( newColor != color)
			{
				color = newColor;
				CallGUIAction();
			}
		}
	}
}
