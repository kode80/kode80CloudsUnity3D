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
	public class RenderSettings : ScriptableObject
	{
		[HeaderAttribute("Sun")]
		public Vector3 sunDirection;
		public Color sunColor;

		[HeaderAttribute("Lighting")]
		public Color cloudBaseColor;
		public Color cloudTopColor;

//		[MenuItem("Assets/Create/kode80/Clouds/RenderSettings")]
//		public static void CreateAsset()
//		{
//			ScriptableObjectUtility.CreateAsset<Clouds.RenderSettings>( "CloudsRenderSettings");
//		}
	}
}
