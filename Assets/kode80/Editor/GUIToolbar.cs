using UnityEngine;
using UnityEditor;
using System.Collections;

namespace kode80.Editor
{
	public class GUIToolbar : GUIBase 
	{
		public int selected;
		public GUIContent[] itemContent;
		
		public GUIToolbar( GUIContent[] itemContent, OnGUIAction action=null)
		{
			this.itemContent = itemContent;
			if( action != null)
			{
				onGUIAction += action;
			}
		}
		
		protected override void CustomOnGUI ()
		{
			int newSelected = GUILayout.Toolbar( selected, itemContent); 
			if( newSelected != selected)
			{
				selected = newSelected;
				CallGUIAction();
			}
		}
	}
}
