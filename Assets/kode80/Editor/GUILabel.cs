using UnityEngine;
using UnityEditor;
using System.Collections;

namespace kode80.Editor
{
	public class GUILabel : GUIBase 
	{
		private GUIContent _content;
		public GUIContent content { get { return _content; } }
		
		public GUILabel( GUIContent content)
		{
			_content = content;
		}
		
		protected override void CustomOnGUI ()
		{
			GUILayout.Label( _content);
		}
	}
}
