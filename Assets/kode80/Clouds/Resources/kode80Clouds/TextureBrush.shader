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

Shader "Hidden/kode80/TextureBrush" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader 
	{
		//Tags { "Queue" = "Background+1" }
			
		Pass
		{
			Cull Off 
			ZWrite Off
			Ztest Always 
			//LOD 200
			
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _BrushTexture;
			float _BrushTextureAlpha;
			float _CoverageOpacity;
			float _TypeOpacity;
			float _ShouldDrawCoverage;
			float _ShouldDrawType;
			float _ShouldBlendValues;
			
			struct brush_v {
				float4 vertex : POSITION;
			   	float2 texcoord : TEXCOORD0;
			   	float2 texcoord2 : TEXCOORD1;
			};
			
			struct v2f {
			   float4 position : SV_POSITION;
			   float2 uv : TEXCOORD0;
			   float2 uv2 : TEXCOORD1;
			};
			
			v2f vert( brush_v v)
			{
			   	v2f o;
				o.position = mul( UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord;
				o.uv2 = v.texcoord2;
			   	return o;
			}
			
			half4 frag (v2f input) : COLOR
			{
				half4 background = tex2D( _MainTex, input.uv2);
				float brush = lerp( 1.0, tex2D( _BrushTexture, input.uv).r, _BrushTextureAlpha);
				float a = 1.0 - min( 1.0, length( input.uv * 2.0 - 1.0));
				a *= brush;

				half4 result = background;

				UNITY_BRANCH
				if( _ShouldBlendValues == 1.0)
				{
					result.r = saturate( result.r + a * _CoverageOpacity * _ShouldDrawCoverage);
					result.b = saturate( result.b + a * _TypeOpacity * _ShouldDrawType);
				}
				else
				{
					result.r = lerp( result.r, _CoverageOpacity, a * _ShouldDrawCoverage);
					result.b = lerp( result.b, _TypeOpacity, a * _ShouldDrawType);
				}
				
				return result;
			}
			
			ENDCG
		}
	} 
}
