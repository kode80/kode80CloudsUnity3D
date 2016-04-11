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
using System.Collections;

namespace kode80.Clouds
{
	[System.Serializable]
	public class TimeOfDayKeyFrame
	{
		public float angle;
		public float sunSize;
		public float atmosphereThickness;
		public Color fogColor;
		public Color sunColor;
		public Color bottomColor;
		public Color topColor;
	}
}	