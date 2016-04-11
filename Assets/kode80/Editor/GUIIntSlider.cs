using UnityEngine;
using UnityEditor;
using System.Collections;

namespace kode80.Editor
{
	public class GUIIntSlider : GUIBase 
	{
		public int value;
		public int minValue;
		public int maxValue;
		
		private GUIContent _content;
		public GUIContent content { get { return _content; } }
		
		public GUIIntSlider( GUIContent content, int value=0, int minValue=0, int maxValue=100, OnGUIAction action=null)
		{
			this.value = value;
			this.minValue = minValue;
			this.maxValue = maxValue;
			
			_content = content;
			if( action != null)
			{
				onGUIAction += action;
			}
		}
		
		protected override void CustomOnGUI ()
		{
			int newValue = EditorGUILayout.IntSlider( _content, value, minValue, maxValue);
			if( newValue != value)
			{
				value = newValue;
				CallGUIAction();
			}
		}
	}
}
