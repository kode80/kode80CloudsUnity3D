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

namespace kode80.Clouds
{
	[ExecuteInEditMode]
	public class TimeOfDay : MonoBehaviour 
	{
		public Material proceduralSkybox;
		public kode80Clouds clouds;
		public Light sun;

		public TimeOfDayKeyFrame[] keyFrames;

		void Awake()
		{
			SortKeyFrames();
		}

		void Start () 
		{
			SortKeyFrames();
		}

		void Update () 
		{
			if( keyFrames.Length < 1)
			{
				return;
			}

			float angle = sun.transform.localEulerAngles.x;
			TimeOfDayKeyFrame keyFrameA = KeyFrameBelowAngle( angle);
			TimeOfDayKeyFrame keyFrameB = KeyFrameAboveAngle( angle);
			float delta = keyFrameB.angle - keyFrameA.angle;
			float alpha = delta == 0.0f ? 0.5f : (angle - keyFrameA.angle) / delta;

			UnityEngine.RenderSettings.fogColor = Color.Lerp( keyFrameA.fogColor, keyFrameB.fogColor, alpha);

			if( sun != null)
			{
				sun.color = Color.Lerp( keyFrameA.sunColor, keyFrameB.sunColor, alpha);
			}

			if( clouds != null)
			{
				clouds.cloudBaseColor = Color.Lerp( keyFrameA.bottomColor, keyFrameB.bottomColor, alpha);
				clouds.cloudTopColor = Color.Lerp( keyFrameA.topColor, keyFrameB.topColor, alpha);
			}

			if( proceduralSkybox)
			{
				proceduralSkybox.SetFloat( "_SunSize", Mathf.Lerp( keyFrameA.sunSize, 
				                                                   keyFrameB.sunSize, alpha));
				proceduralSkybox.SetFloat( "_AtmosphereThickness", Mathf.Lerp( keyFrameA.atmosphereThickness, 
				                                                               keyFrameB.atmosphereThickness, alpha));
			}
		}
		
		public void SortKeyFrames()
		{
			Array.Sort( keyFrames, delegate( TimeOfDayKeyFrame a, TimeOfDayKeyFrame b) { 
				return a.angle.CompareTo( b.angle); 
			});
		}

		private TimeOfDayKeyFrame KeyFrameBelowAngle( float angle)
		{
			return KeyFrameForAngle( angle, false);
		}

		private TimeOfDayKeyFrame KeyFrameAboveAngle( float angle)
		{
			return KeyFrameForAngle( angle, true);
		}
		
		private TimeOfDayKeyFrame KeyFrameForAngle( float angle, bool findGreaterThan)
		{
			if( keyFrames.Length > 0)
			{
				TimeOfDayKeyFrame keyFrameA = keyFrames[ 0];
				TimeOfDayKeyFrame keyFrameB = keyFrames[ keyFrames.Length - 1];

				// Clamp boundaries
				if( angle <= keyFrameA.angle) { return keyFrameA; }
				else if( angle >= keyFrameB.angle) { return keyFrameB; }

				for( int i=0; i<keyFrames.Length; i++)
				{
					keyFrameA = keyFrames[ i];
					keyFrameB = i < keyFrames.Length - 1 ? keyFrames[ i+1] : keyFrameA;

					if( angle >= keyFrameA.angle && 
					    angle <= keyFrameB.angle)
					{
						return findGreaterThan ? keyFrameB : keyFrameA;
					}
				}
			}

			return null;
		}
	}
}
