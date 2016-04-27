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

Shader "Hidden/kode80/LinearDepth" 
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Pass
		{
			Tags 
			{ 
				"RenderType" = "Opaque" 
				"ForceNoShadowCasting" = "True"
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v2f
			{
				float4 position : POSITION;
			    float4 linearDepth : TEXCOORD0;
			};

			v2f vert( appdata_base v) 
			{
			    v2f output;
			    output.position = mul( UNITY_MATRIX_MVP, v.vertex);
			    output.linearDepth = float4( 0.0, 0.0, COMPUTE_DEPTH_01, 0.0);
			    return output;
			}
			
			float4 frag( v2f input) : COLOR 
			{
				float r = input.linearDepth.z == 1.0;
				return float4( r, 0.0, 0.0, 0.0);
			}

			ENDCG
		}
	}
}
