using UnityEngine;
using UnityEditor;
using System.Collections;

namespace kode80.Editor
{
	public class GUIToggle : GUIBase 
	{
		public bool isToggled;

		private GUIContent _content;
		public GUIContent content { get { return _content; } }
		
		public GUIToggle( GUIContent content, OnGUIAction action=null)
		{
			_content = content;
			if( action != null)
			{
				onGUIAction += action;
			}
		}
		
		protected override void CustomOnGUI ()
		{
			bool newIsToggled = EditorGUILayout.Toggle( _content, isToggled);
			if( newIsToggled != isToggled)
			{
				isToggled = newIsToggled;
				CallGUIAction();
			}
		}
	}
}
