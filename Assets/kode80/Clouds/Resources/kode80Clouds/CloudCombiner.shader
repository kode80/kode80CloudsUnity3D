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

Shader "Hidden/kode80/CloudCombiner" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader 
	{
		Pass
		{
			Tags { "RenderType"="Opaque" }
			LOD 200
			Cull Off
			ZWrite Off
			Ztest LEqual
			
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"
			#include "VolumeCloudsCommon.cginc"

			sampler2D _MainTex;
			sampler2D _SubFrame;
			sampler2D _PrevFrame;
			sampler2D _LinearDepthTex;
			
			struct v2f {
			   float4 position : SV_POSITION;
			   float2 uv : TEXCOORD0;
			};
			
			v2f vert(appdata_img v)
			{
			   	v2f o;
				o.position = mul( UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord;
				
			   	return o;
			}
			
			half4 frag (v2f input) : COLOR
			{
				float decodedDepth = tex2D( _LinearDepthTex, input.uv).r;
				if( decodedDepth == 0.0)
				{
					return half4( 1.0, 0.0, 1.0, 0.0);
				}

				float2 uv = floor(input.uv * _FrameSize);
				float2 uv2 = (floor(input.uv * _SubFrameSize) + 0.5) / _SubFrameSize;
				
				float x = fmod( uv.x, _SubPixelSize);
				float y = fmod( uv.y, _SubPixelSize);
				float frame = y * _SubPixelSize + x;
				float4 cloud;
				
				if( frame == _SubFrameNumber)
				{ 
					cloud = tex2D( _SubFrame, uv2); 
				} 
				else
				{
					float4 prevPos = float4( input.uv * 2.0 - 1.0, 1.0, 1.0);
					prevPos = mul( _InverseProjection, prevPos);
					prevPos = prevPos / prevPos.w;
					prevPos.xyz = mul( (float3x3)_InverseRotation, prevPos.xyz);
					prevPos.xyz = mul( (float3x3)_PreviousRotation, prevPos.xyz);
					float4 reproj = mul( _Projection, prevPos);
					reproj /= reproj.w;
					reproj.xy = reproj.xy * 0.5 + 0.5;

					if( reproj.y < 0.0 || reproj.y > 1.0 || reproj.x < 0.0 || reproj.x > 1.0)
					{
						cloud = tex2D( _SubFrame, input.uv);
					}
					else
					{
						// We need to use texture filtering when reprojecting otherwise
						// you end up with noticable pixel-stepping as the camera rotates
						// due to low resolution...
						// ...however, with texture filtering turned on it makes it impossible
						// to detect the hot-pink 'occluded' pixels as they are likely blended
						// with neighbors. We can fix this by doing point sampling of the 
						// reprojected pixel and it's 4 neighbours (N/S/E/W), if any of these
						// point sampled pixels are hotpink, then there will be some blended in
						// with the reprojected pixel, so we should reject and just use the
						// low rez current frame pixel to fill in. 
						//
						// This fixes all edge ghosting caused by occluding geometry when
						// reprojecting previous frame.
						float2 reproj2 = (floor(reproj.xy * _FrameSize) + 0.5) / _FrameSize;
						float2 pixelSize = 1.0 / _FrameSize;
						int check = 0;

						cloud = tex2D( _PrevFrame, reproj2.xy);
						check += ( cloud.r == 1 && cloud.g == 0 && cloud.b == 1 && cloud.a == 0);

						cloud = tex2D( _PrevFrame, reproj2.xy + float2( pixelSize.x, 0.0));
						check += ( cloud.r == 1 && cloud.g == 0 && cloud.b == 1 && cloud.a == 0);

						cloud = tex2D( _PrevFrame, reproj2.xy + float2( -pixelSize.x, 0.0));
						check += ( cloud.r == 1 && cloud.g == 0 && cloud.b == 1 && cloud.a == 0);


						cloud = tex2D( _PrevFrame, reproj2.xy + float2( 0.0, pixelSize.y));
						check += ( cloud.r == 1 && cloud.g == 0 && cloud.b == 1 && cloud.a == 0);

						cloud = tex2D( _PrevFrame, reproj2.xy + float2( 0.0, -pixelSize.y));
						check += ( cloud.r == 1 && cloud.g == 0 && cloud.b == 1 && cloud.a == 0);

						if( check > 0)
						{
							cloud = tex2D( _SubFrame, uv2);
						}
						else
						{
							cloud = tex2D( _PrevFrame, reproj.xy);
						}
					}
				}

				return cloud;
			}
			
			ENDCG
		}
	} 
}
