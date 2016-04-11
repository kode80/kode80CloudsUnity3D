using UnityEngine;
using UnityEditor;
using System.Collections;

namespace kode80.Editor
{
	public class GUISlider : GUIBase 
	{
		public float value;
		public float minValue;
		public float maxValue;
		
		private GUIContent _content;
		public GUIContent content { get { return _content; } }
		
		public GUISlider( GUIContent content, float value=0.0f, float minValue=0.0f, float maxValue=1.0f, OnGUIAction action=null)
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
			float newValue = EditorGUILayout.Slider( _content, value, minValue, maxValue);
			if( newValue != value)
			{
				value = newValue;
				CallGUIAction();
			}
		}
	}
}
