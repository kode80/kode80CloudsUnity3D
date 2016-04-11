using UnityEngine;
using UnityEditor;
using System.Collections;

namespace kode80.Editor
{
	public class GUIVector3Field : GUIBase 
	{
		public Vector3 vector;

		private GUIContent _content;
		public GUIContent content { get { return _content; } }
		
		public GUIVector3Field( GUIContent content, OnGUIAction action=null)
		{
			_content = content;
			if( action != null)
			{
				onGUIAction += action;
			}
		}
		
		protected override void CustomOnGUI ()
		{
			Vector3 newVector = EditorGUILayout.Vector3Field( _content, vector);
			if( newVector != vector)
			{
				vector = newVector;
				CallGUIAction();
			}
		}
	}
}
