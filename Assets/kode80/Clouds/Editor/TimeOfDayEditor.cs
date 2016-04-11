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
using UnityEditor;
using System.Collections;
using kode80.Clouds;

[CustomEditor( typeof(TimeOfDay))]
public class TimeOfDayEditor : Editor 
{
	public override void OnInspectorGUI()
	{
		TimeOfDay timeOfDay = (TimeOfDay)target;

		DrawDefaultInspector();
		if( GUILayout.Button( "Sort Key Frames"))
		{
			timeOfDay.SortKeyFrames();
		}
	}
}
