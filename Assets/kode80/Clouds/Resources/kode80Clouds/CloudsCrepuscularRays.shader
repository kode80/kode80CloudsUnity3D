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

Shader "Hidden/kode80/CloudsCrepuscularRays"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			
			#include "UnityCG.cginc"
		
			sampler2D _MainTex;
			sampler2D _Clouds;
			sampler2D _CameraDepthTexture;

			float2 _SunScreenSpace;
			float _SampleCount;
			float _Density;
			float _Decay;
			float _Weight;
			float _Exposure;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			inline float SampleSun( float2 uv)
			{
				return smoothstep( 0.035, 0.0, distance( uv, _SunScreenSpace)) * 0.41;
			}
			
			inline half SampleLight( float2 uv)
			{
				float4 uvLOD = float4( uv, 0.0, 0.0);
				float depth = Linear01Depth( tex2Dlod( _CameraDepthTexture, uvLOD).r);
				float cloud = 1.0 - tex2Dlod( _Clouds, uvLOD).a;
				float occlusion = smoothstep( 0.5, 1.0, cloud) * step( 1.0, depth);
				
				return occlusion * SampleSun( uv); 
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 pixel = tex2D( _MainTex, i.uv);
				fixed4 cloud = tex2D( _Clouds, i.uv);
				
				float2 texCoord = i.uv;
				half2 deltaTexCoord = (texCoord - _SunScreenSpace);  
				deltaTexCoord *= 1.0f / _SampleCount * _Density;
				
				half light = SampleLight( texCoord);
				half origLight = light;
				half illuminationDecay = 1.0f;  
				for (int i = 0; i < _SampleCount; i++)
				{  
					texCoord -= deltaTexCoord;  
				    half sample = SampleLight( texCoord);
				    sample *= illuminationDecay * _Weight;  
				    light += sample;  
				    illuminationDecay *= _Decay;
				} 

				return pixel + light * _Exposure;
			}
			ENDCG
		}
	}
}
