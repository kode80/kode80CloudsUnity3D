using UnityEngine;
using System.Collections;

namespace kode80.Editor
{
	public class GUIBase 
	{
		public delegate void OnGUIAction( GUIBase sender);
		public OnGUIAction onGUIAction;

		public bool isHidden;
		public bool isEnabled;
		public bool shouldStoreLastRect;

		private Rect _lastRect;
		public Rect lastRect { get { return _lastRect; } }

		public GUIBase()
		{
			isEnabled = true;
		}

		public void OnGUI()
		{
			if( isHidden == false)
			{
				GUI.enabled = isEnabled;
				CustomOnGUI();
				GUI.enabled = true;

				if( shouldStoreLastRect && Event.current.type == EventType.Repaint)
				{
					_lastRect = GUILayoutUtility.GetLastRect();
				}
			}
		}

		protected virtual void CustomOnGUI()
		{
			// Subclasses override this to implement OnGUI
		}

		protected void CallGUIAction()
		{
			if( onGUIAction != null)
			{
				onGUIAction( this);
			}
		}
	}
}