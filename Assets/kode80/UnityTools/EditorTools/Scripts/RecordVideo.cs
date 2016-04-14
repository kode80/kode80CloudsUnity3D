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
using System.IO;
using System.Collections;

namespace kode80.EditorTools
{
	// The RecordVideo component is used by RecordVideoWindow
	// to record frames during play mode and should't be added
	// manually by the user, so hide it from the component menu.
	[AddComponentMenu("")]
	public class RecordVideo : MonoBehaviour 
	{
		public int captureFramerate = 60;
		public int superSize = 1;
		public string folderPath;

		private bool _isRecording = false;
		public bool isRecording { get { return _isRecording; } }

		private int _sceneNumber = 0;
		public int sceneNumber { get { return _sceneNumber; } }

		private int _frameNumber = 0;

		void Start () 
		{
			Time.captureFramerate = captureFramerate;
		}

		void Update () 
		{
			if( _isRecording)
			{
				string path = string.Format("{0}/Scene{1:D03}Frame{2:D08}.png", folderPath, _sceneNumber, _frameNumber );
				Application.CaptureScreenshot( path, superSize);
				_frameNumber++;
			}
		}

		public bool StartRecording()
		{
			if( _isRecording == false && Directory.Exists( folderPath))
			{
				_isRecording = true;
			}

			return _isRecording;
		}

		public void StopRecording()
		{
			if( _isRecording == true)
			{
				_isRecording = false;
				_sceneNumber++;
				_frameNumber = 0;
			}
		}
	}
}
