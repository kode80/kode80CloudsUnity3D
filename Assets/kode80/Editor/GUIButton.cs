using UnityEngine;
using UnityEditor;
using System.Collections;

namespace kode80.Editor
{
	public class GUIButton : GUIBase 
	{
		private GUIContent _content;
		public GUIContent content { get { return _content; } }

		public GUIButton( GUIContent content, OnGUIAction action=null)
		{
			_content = content;
			if( action != null)
			{
				onGUIAction += action;
			}
		}

		protected override void CustomOnGUI ()
		{
			if( GUILayout.Button( content))
			{
				CallGUIAction();
			}
		}
	}
}
