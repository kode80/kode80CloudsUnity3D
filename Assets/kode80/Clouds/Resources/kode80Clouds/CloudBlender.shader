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

Shader "Hidden/kode80/CloudBlender" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader 
	{
		Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
			
		Pass
		{
			Cull Off 
			ZWrite Off
			Ztest LEqual 
			//LOD 200
			Blend One OneMinusSrcAlpha
			
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			bool _IsGamma;
			
			struct v2f {
			   float4 position : SV_POSITION;
			   float2 uv : TEXCOORD0;
			};
			
			v2f vert(appdata_base v)
			{
			   	v2f o;
				o.position = float4(v.vertex.xyz, 1.0);
				o.uv = v.texcoord;

				if( _ProjectionParams.x < 0) {
				        o.uv.y = 1-o.uv.y;
				}

			   	return o;
			}
			
			half4 frag (v2f input) : COLOR
			{
				half4 output = tex2D( _MainTex, input.uv);

				// IsGamma() not available in Unity 5.2.4
				return _IsGamma ? output : pow( output, 2.2);
			}
			
			ENDCG
		}
	} 
}
