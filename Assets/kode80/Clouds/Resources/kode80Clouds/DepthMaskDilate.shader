﻿Shader "Hidden/kode80/DepthMaskDilate"
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
			
			#include "UnityCG.cginc"

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
			
			sampler2D _MainTex;
			float4 _MainTex_TexelSize;

			fixed4 frag (v2f i) : SV_Target
			{
				float4 color = tex2D( _MainTex, i.uv);

				float2 originUV = i.uv;
				float2 uv;
				
				if( color.r == 0.0f)
				{
					float4 neighbor;
					
					uv = originUV + float2( -_MainTex_TexelSize.x, -_MainTex_TexelSize.y);
					neighbor = tex2D( _MainTex, uv);
					if( neighbor.r > 0.0f) { return neighbor; }
					
					uv = originUV + float2( _MainTex_TexelSize.x, -_MainTex_TexelSize.y);
					neighbor = tex2D( _MainTex, uv);
					if( neighbor.r > 0.0f) { return neighbor; }
					
					uv = originUV + float2( -_MainTex_TexelSize.x, _MainTex_TexelSize.y);
					neighbor = tex2D( _MainTex, uv);
					if( neighbor.r > 0.0f) { return neighbor; }
					
					uv = originUV + float2( _MainTex_TexelSize.x, _MainTex_TexelSize.y);
					neighbor = tex2D( _MainTex, uv);
					if( neighbor.r > 0.0f) { return neighbor; }
					
					
					uv = originUV + float2( -_MainTex_TexelSize.x, 0.0f);
					neighbor = tex2D( _MainTex, uv);
					if( neighbor.r > 0.0f) { return neighbor; }
					
					uv = originUV + float2( _MainTex_TexelSize.x, 0.0f);
					neighbor = tex2D( _MainTex, uv);
					if( neighbor.r > 0.0f) { return neighbor; }
					
					
					uv = originUV + float2( 0.0f, -_MainTex_TexelSize.y);
					neighbor = tex2D( _MainTex, uv);
					if( neighbor.r > 0.0f) { return neighbor; }
					
					uv = originUV + float2( 0.0f, _MainTex_TexelSize.y);
					neighbor = tex2D( _MainTex, uv);
					if( neighbor.r > 0.0f) { return neighbor; }
				}

				return color.r;

				//return color;
			}
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

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
			
			sampler2D _MainTex;
			sampler2D _DilatedDepthTex;

			fixed4 frag (v2f i) : SV_Target
			{
				float4 color = tex2D( _MainTex, i.uv);
				float depth = tex2D( _DilatedDepthTex, i.uv).r * 0.95;
				float4 depthColor = float4( depth, depth, depth, depth);

				//return depthColor;
				color = float4( 0.5,0.5,0.5, 0.5);
				return (1.0 - color.a) * depthColor + color;
				//return depthColor;
			}
			ENDCG
		}
	}
}
