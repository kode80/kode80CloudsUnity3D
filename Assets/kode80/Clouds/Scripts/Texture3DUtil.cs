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
	public static class Texture3DUtil 
	{
		public static Texture3D Load( string resourcePath, int cubeSize, TextureFormat format=TextureFormat.RGBA32)
		{
			return Load( Resources.Load<TextAsset>( resourcePath).bytes, cubeSize, cubeSize, cubeSize, format);
		}

		public static Texture3D Load( string resourcePath, int width, int height, int depth, TextureFormat format=TextureFormat.RGBA32)
		{
			return Load( Resources.Load<TextAsset>( resourcePath).bytes, width, height, depth, format);
		}

		public static Texture3D Load( byte[] bytes, int width, int height, int depth, TextureFormat format=TextureFormat.RGBA32)
		{
			if( format != TextureFormat.RGBA32 && format != TextureFormat.RGB24)
			{
				throw new ArgumentException( "Unsupported TextureFormat");
			}

			int count = width * height * depth;
			Color32[] colors = new Color32[ count];

			int j=0;

			for( int i=0; i<count; i++)
			{
				colors[i].r = bytes[j++];
				colors[i].g = bytes[j++];
				colors[i].b = bytes[j++];
				colors[i].a = format == TextureFormat.RGBA32 ? bytes[j++] : (byte)255;
			}

			Texture3D texture = new Texture3D( width, height, depth, format, true);
			texture.wrapMode = TextureWrapMode.Repeat;
			texture.filterMode = FilterMode.Bilinear;
			texture.SetPixels32( colors, 0);
			texture.Apply();

			return texture;
		}
	}
}
