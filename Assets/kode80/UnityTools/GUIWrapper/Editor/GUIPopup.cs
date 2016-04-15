//  Copyright (c) 2016, Ben Hopkins (kode80)
//  All rights reserved.
//  
//  Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
//  
//  1. Redistributions of source code must retain the above copyright notice, 
//     this list of conditions and the following disclaimer.
//  
//  2. Redistributions in binary form must reproduce the above copyright notice, 
//     this list of conditions and the following disclaimer in the documentation 
//     and/or other materials provided with the distribution.
//  
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
//  EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
//  MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL 
//  THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
//  SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT 
//  OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
//  HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
//  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
//  EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace kode80.GUIWrapper
{
	public class GUIPopup : GUIBase 
	{
		public int selectedIndex;

		private GUIContent[] _displayedOptions;
		public GUIContent[] displayedOptions {
			get { return _displayedOptions; }
			set {
				bool valid = value != null && value.Length > 0;

				if( valid) { _displayedOptions = value; }
				else { _displayedOptions = new GUIContent[1] {new GUIContent( "None")}; }

				selectedIndex = 0;
			}
		}

		private GUIContent _content;
		public GUIContent content { get { return _content; } }

		public GUIPopup( GUIContent content, GUIContent[] displayedOptions, int selectedIndex=0, OnGUIAction action=null)
		{
			this.displayedOptions = displayedOptions;
			this.selectedIndex = selectedIndex;
			_content = content;

			if( action != null)
			{
				onGUIAction += action;
			}
		}

		protected override void CustomOnGUI ()
		{
			int newIndex = EditorGUILayout.Popup( _content, selectedIndex, displayedOptions);
			if( newIndex != selectedIndex)
			{
				selectedIndex = newIndex;
				CallGUIAction();
			}
		}
	}
}
