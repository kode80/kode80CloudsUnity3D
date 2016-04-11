using UnityEngine;
using UnityEditor;
using System.Collections;

namespace kode80.Editor
{
	public class GUITextureField : GUIBase 
	{
		public Texture2D texture;

		private GUIContent _content;
		public GUIContent content { get { return _content; } }
		
		public GUITextureField( GUIContent content, OnGUIAction action=null)
		{
			_content = content;
			if( action != null)
			{
				onGUIAction += action;
			}
		}
		
		protected override void CustomOnGUI ()
		{
			Texture2D newTexture = EditorGUILayout.ObjectField( _content, texture, typeof( Texture2D), false) as Texture2D;
			if( newTexture != texture)
			{
				texture = newTexture;
				CallGUIAction();
			}
		}
	}
}
