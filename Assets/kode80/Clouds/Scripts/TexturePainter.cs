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
	public class TexturePainter : ScriptableObject
	{
		private Material _material;

		void OnEnable()
		{
			_material = new Material( Shader.Find( "Hidden/kode80/TextureBrush"));
			_material.hideFlags = HideFlags.HideAndDontSave;
		}

		void OnDisable()
		{
			DestroyImmediate( _material);
			_material = null;
		}

		public void Render( Vector2 point, 
							float radius, 
							float coverageOpacity,
							float typeOpacity,
							bool drawCoverage, 
							bool drawType, 
							bool blendValues, 
							RenderTexture target, 
							Texture2D brushTexture = null)
		{
			point.x *= target.width;
			point.y *= target.height;

			RenderTexture previous = RenderTexture.active;
			RenderTexture buffer = RenderTexture.GetTemporary( target.width, target.height, target.depth, target.format, RenderTextureReadWrite.Linear);

			Graphics.Blit( target, buffer);
			RenderTexture.active = buffer;


			float tw = target.width;
			float th = target.height;
			float h = radius;
			float w = radius;
			float z = 0.0f;

			_material.SetTexture( "_MainTex", target);
			_material.SetFloat( "_CoverageOpacity", coverageOpacity);
			_material.SetFloat( "_TypeOpacity", typeOpacity);
			_material.SetFloat( "_ShouldDrawCoverage", drawCoverage ? 1.0f : 0.0f);
			_material.SetFloat( "_ShouldDrawType", drawType ? 1.0f : 0.0f);
			_material.SetFloat( "_ShouldBlendValues", blendValues ? 1.0f : 0.0f);

			if( brushTexture != null)
			{
				_material.SetTexture( "_BrushTexture", brushTexture);
			}
			_material.SetFloat( "_BrushTextureAlpha", brushTexture == null ? 0.0f : 1.0f);

			GL.PushMatrix();
			_material.SetPass( 0);
			GL.LoadIdentity();
			GL.LoadPixelMatrix( 0.0f, target.width, 0.0f, target.height);
			GL.Begin( GL.QUADS);

			GL.MultiTexCoord2( 0, 0.0f, 0.0f);
			GL.MultiTexCoord2( 1, (point.x - w) / tw, (point.y - h) / th);
			GL.Vertex( new Vector3( point.x - w, point.y - h, z));
			
			GL.MultiTexCoord2( 0, 1.0f, 0.0f);
			GL.MultiTexCoord2( 1, (point.x + w) / tw, (point.y - h) / th);
			GL.Vertex( new Vector3( point.x + w, point.y - h, z));
			
			GL.MultiTexCoord2( 0, 1.0f, 1.0f);
			GL.MultiTexCoord2( 1, (point.x + w) / tw, (point.y + h) / th);
			GL.Vertex( new Vector3( point.x + w, point.y + h, z));
			
			GL.MultiTexCoord2( 0, 0.0f, 1.0f);
			GL.MultiTexCoord2( 1, (point.x - w) / tw, (point.y + h) / th);
			GL.Vertex( new Vector3( point.x - w, point.y + h, z));

			GL.End();
			GL.PopMatrix();


			Graphics.Blit( buffer, target);

			RenderTexture.ReleaseTemporary( buffer);
			RenderTexture.active = previous;
		}

		public void Clear( RenderTexture target)
		{
			RenderTexture previous = RenderTexture.active;
			RenderTexture.active = target;
			GL.Clear( true, true, Color.black);
			RenderTexture.active = previous;
		}
	}
}