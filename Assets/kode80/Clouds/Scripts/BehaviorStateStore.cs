//***************************************************
//
//  Author: Ben Hopkins
//  Copyright (C) 2016 kode80 LLC, 
//  all rights reserved
// 
//  Free to use for non-commercial purposes, 
//  see full license in project root:
//  kode80CloudsNonCommercialLicense.html
//  
//  Commercial licenses available for purchase from:
//  http://kode80.com/
//
//***************************************************

using UnityEngine;
using System;
using System.Collections;
using System.Linq;

namespace kode80.Clouds
{
	[Serializable]
	public class BehaviorStateStore
	{
		[SerializeField]
		private string[] _names;
		[SerializeField]
		private bool[] _enabledStates;

		public void StoreStates( MonoBehaviour[] behaviors)
		{
			_names = behaviors.Select( x => x.GetType().Name).ToArray();
			_enabledStates = behaviors.Select( x => x.enabled).ToArray();
		}

		public void ApplyStates( MonoBehaviour[] behaviors)
		{
			if( _names == null || _enabledStates == null) { return; }

			int notFound = _names.GetLowerBound(0) - 1;

			foreach( MonoBehaviour behavior in behaviors)
			{
				int i = Array.IndexOf( _names, behavior.GetType().Name);
				if( i > notFound)
				{
					behavior.enabled = _enabledStates[ i];
				}
			}
		}
	}
}
