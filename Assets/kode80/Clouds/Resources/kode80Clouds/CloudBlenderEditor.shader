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

Shader "Hidden/kode80/CloudBlenderEditor" 
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
			#include "VolumeCloudsCommon.cginc"


			sampler2D _MainTex;
			sampler2D _Coverage;
			float _DrawCoverage;
			float _DrawType;
			float _DrawCursor;
			float3 _Cursor;
			float _CursorRadius;
			bool _IsGamma;

			float4 _MainTex_TexelSize;
			
			clouds_v2f vert(appdata_base v)
			{
			   	clouds_v2f o;
				o.position = float4(v.vertex.xyz, 1.0);
				o.uv = v.texcoord.xy;

				if( _ProjectionParams.x < 0) {
				        o.uv.y = 1-o.uv.y;
				}
				
				o.cameraRay = UVToCameraRay( o.uv);
				
			   	return o;
			}
			
			half4 frag( clouds_v2f input) : COLOR
			{	
				half4 color = tex2D( _MainTex, input.uv);
				float3 rayDirection = normalize( input.cameraRay);
				float2 uv = input.uv;
				float3 midIntersect = InternalRaySphereIntersect( _EarthRadius + _StartHeight, _CameraPosition, rayDirection);
				float3 maxIntersect = InternalRaySphereIntersect( _EarthRadius + _EndHeight, _CameraPosition, rayDirection);
					
				float2 bPos = float2( _Cursor.x, _Cursor.z);
				float2 v0 = float2( 0.0, 0.0);
				float2 v1 = maxIntersect.xz;
				
				float bA = 0.5;
				float3 bC = float3( 1.0, 0.0, 0.0);
				
				float innerR = _MaxDistance * _Cursor.y;
				float outerR = innerR + (_MaxDistance / 256.0 * 4.0);
				float rL = distance( _Cursor.xz, maxIntersect.xz);
				
				
				float2 coverageUV = (maxIntersect.xz / _MaxDistance) * 0.5 + 0.5;
				half4 coverage = tex2D( _Coverage, coverageUV);
				
				if( rayDirection.y >= 0.0)
				{
					if( _DrawCursor && distance( bPos, maxIntersect.xz) <= _CursorRadius)
					{
						bC = float3( 1.0, 0.0, 0.0);
						bA = 0.85;
						
						color.rgb *= 0.95;
						color.a *= 0.95;
						
						color.rgb = (bC * bA) * (1.0 - color.a) + color.rgb;
						color.a += bA * (1.0 - color.a);
						
						
						//color.rgb *= 1.0 - bA;
						//color.rgb += bC * bA;
						//color.a += bA * (1.0 - color.a);
					}
					
					if( _DrawCursor && distance( bPos, midIntersect.xz) <= _CursorRadius)
					{
						bC = float3( 1.0, 1.0, 0.0);
						bA = 0.5;
						
						color.rgb *= 1.0 - bA;
						color.rgb += bC * bA;
						color.a += bA * (1.0 - color.a);
					}
					
					if( _DrawCoverage == 1.0)
					{
						color.rgb += half3( coverage.r, coverage.r, coverage.r) * (1.0 - color.a);
						color.a = 1.0;
					}
				}

				// IsGamma() not available in Unity 5.2.4
				return _IsGamma ? color : pow( color, 2.2);
			}
			
			ENDCG
		}
	} 
}
