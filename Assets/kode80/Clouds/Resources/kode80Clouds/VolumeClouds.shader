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

Shader "Hidden/kode80/VolumeClouds" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
			
		Pass
		{
			ZTest off Cull Off ZWrite Off
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma exclude_renderers d3d9
			#include "UnityCG.cginc"
			#include "VolumeCloudsCommon.cginc"
			
			
			sampler2D _MainTex;
			sampler2D _CameraDepthTexture;
			
			sampler3D _Perlin3D;
			sampler3D _Detail3D;
			sampler2D _Coverage;
			sampler2D _Curl2D;
			
			
			float _CloudBottomFade;
			
			float3 _BaseOffset;
			float3 _DetailOffset;
			float2 _CoverageOffset;
			float _BaseScale;
			float _CoverageScale;
			float _HorizonFadeStartAlpha;
			float _OneMinusHorizonFadeStartAlpha;
			float _HorizonFadeScalar;					// Fades clouds on horizon, 1.0 -> 10.0 (1.0 = smooth fade, 10 = no fade)
			float3 _LightDirection;
			float3 _LightColor;
			float _LightScalar;
			float _AmbientScalar;
			float3 _CloudBaseColor;
			float3 _CloudTopColor;
			float4 _CloudHeightGradient1;				// x,y,z,w = 4 positions of a black,white,white,black gradient
			float4 _CloudHeightGradient2;				// x,y,z,w = 4 positions of a black,white,white,black gradient
			float4 _CloudHeightGradient3;				// x,y,z,w = 4 positions of a black,white,white,black gradient
			float _SunRayLength;
			float _ConeRadius;
		 	float _MaxIterations;
			float _MaxRayDistance;
			float _RayStepLength;
			float _SampleScalar;
			float _SampleThreshold;
			float _DetailScale;
			float _ErosionEdgeSize;
			float _CloudDistortion;
			float _CloudDistortionScale;
			float _Density;
			float _ForwardScatteringG;
			float _BackwardScatteringG;
			float _DarkOutlineScalar;

			float _HorizonCoverageStart;
			float _HorizonCoverageEnd;
			
			float _LODDistance;
			float _RayMinimumY;

		 	float4x4 _NormalMatrix;

			float3 _Random0;
			float3 _Random1;
			float3 _Random2;
			float3 _Random3;
			float3 _Random4;
			float3 _Random5;
			
		 	#define FLOAT4_COVERAGE( f)	f.r
		 	#define FLOAT4_RAIN( f)		f.g
		 	#define FLOAT4_TYPE( f)		f.b


			clouds_v2f vert(appdata_img v)
			{
			   	clouds_v2f o;
				o.position = mul( UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord;
				
				o.cameraRay = UVToCameraRay( o.uv);
				
			   	return o;
			}
			
			inline float3 ScreenSpaceToViewSpace( float3 cameraRay, float depth)
			{
				return (cameraRay * depth);
			}
			
			inline float NormalizedAtmosphereY( float3 ray)
			{
				float y = length( ray) - _EarthRadius - _StartHeight;
				return y / _AtmosphereThickness;
			}
			
			inline float GradientStep( float a, float4 gradient)
			{
				return smoothstep( gradient.x, gradient.y, a) - smoothstep( gradient.z, gradient.w, a);
			}
			
			inline float4 SampleCoverage( float3 ray, float csRayHeight, float lod)
			{
				float2 unit = ray.xz * _CoverageScale;
				float2 uv = unit * 0.5 + 0.5;
				uv += _CoverageOffset;

				float depth = distance( ray, _CameraPosition) / _MaxDistance;
				float4 coverage = tex2Dlod( _Coverage, float4( uv, 0.0, 0.0));
				float4 coverageB = float4( 1.0, 0.0, 0.0, 0.0);
				//coverageB.b = saturate( smoothstep( _HorizonCoverageEnd, _HorizonCoverageStart, depth) * 2.0);
				float alpha = smoothstep( _HorizonCoverageStart, _HorizonCoverageEnd, depth);

				coverageB = float4( smoothstep( _HorizonCoverageStart, _HorizonCoverageEnd, depth),
							   0.0,
							   smoothstep( _HorizonCoverageEnd, _HorizonCoverageStart + (_HorizonCoverageEnd - _HorizonCoverageStart) * 0.5, depth),
							   0.0);

				return lerp( coverage, coverageB, alpha);
			}
			
			inline float SmoothThreshold( float value, float threshold, float edgeSize)
			{
				return smoothstep( threshold, threshold + edgeSize, value);
			}
			
			inline float3 SmoothThreshold( float3 value, float threshold, float edgeSize)
			{
				value.r = smoothstep( threshold, threshold + edgeSize, value.r);
				value.g = smoothstep( threshold, threshold + edgeSize, value.g);
				value.b = smoothstep( threshold, threshold + edgeSize, value.b);
				
				return value;
			}
			
			inline float MixNoise( float value, float noise, float a, float b, float height)
			{
				float s = smoothstep( a, b, height);
				value += noise * s;
				//value *= lerp( 1.0, 0.5, s);
				
				return value;
			}
			
			inline float Lerp3( float v0, float v1, float v2, float a)
			{
				return a < 0.5 ? lerp( v0, v1, a * 2.0) : lerp( v1, v2, (a-0.5) * 2.0);
			}
			
			inline float4 Lerp3( float4 v0, float4 v1, float4 v2, float a)
			{
				return float4( Lerp3( v0.x, v1.x, v2.x, a),
							   Lerp3( v0.y, v1.y, v2.y, a),
							   Lerp3( v0.z, v1.z, v2.z, a),
							   Lerp3( v0.w, v1.w, v2.w, a));
			}

			inline float SampleCloud(float3 ray, float rayDensity, float4 coverage, float csRayHeight, float lod)
			{
				float value = 0.0;
				float4 coord = float4(ray * _BaseScale + _BaseOffset, 0.0);
				float4 noiseSample = tex3Dlod(_Perlin3D, coord);
				float4 gradientScalar = float4( 1.0,
					GradientStep(csRayHeight, _CloudHeightGradient1),
					GradientStep(csRayHeight, _CloudHeightGradient2),
					GradientStep(csRayHeight, _CloudHeightGradient3));

				noiseSample *= gradientScalar;

				float noise = saturate((noiseSample.r + noiseSample.g + noiseSample.b + noiseSample.a) / 4.0);

				
				float4 gradient = Lerp3(_CloudHeightGradient3,
										_CloudHeightGradient2,
										_CloudHeightGradient1,
										FLOAT4_TYPE(coverage));
				noise *= GradientStep( csRayHeight, gradient);

				noise = SmoothThreshold(noise, _SampleThreshold, _ErosionEdgeSize);
				noise = saturate(noise - (1.0 - FLOAT4_COVERAGE(coverage))) * FLOAT4_COVERAGE(coverage);
				
				if (noise > 0.0 && noise < 1.0 && lod == 0)
				{
					float4 distUV = float4(ray.xy * _BaseScale * _CloudDistortionScale, 0.0, 0.0);
					float3 curl = tex2Dlod(_Curl2D, distUV) * 2.0 - 1.0;

					coord = float4(ray * _BaseScale * _DetailScale, 0.0);
					coord.xyz += _DetailOffset;

					curl *= _CloudDistortion * csRayHeight;
					coord.xyz += curl;

					float3 detail = 1.0 - tex3Dlod(_Detail3D, coord);
					detail *= gradientScalar.gba;
					float detailValue = detail.r + detail.g + detail.b;
					detailValue /= 3.0;
					detailValue *= smoothstep( 1.0, 0.0, noise) * 0.5;
					noise -= detailValue;

					noise = saturate(noise);
				}

				return noise * _SampleScalar * smoothstep(0.0, _CloudBottomFade * 1.0, csRayHeight);
			}

			float HenyeyGreensteinPhase( float cosAngle, float g)
			{
				float g2 = g * g;
				return (1.0 - g2) / pow( 1.0 + g2 - 2.0 * g * cosAngle, 1.5);
			}

			inline float BeerTerm( float densityAtSample)
			{
				return exp( -_Density * densityAtSample);
			}
			
			inline float PowderTerm( float densityAtSample, float cosTheta)
			{
				float powder = 1.0 - exp( -_Density * densityAtSample * 2.0);
				powder = saturate( powder * _DarkOutlineScalar * 2.0);
				return lerp( 1.0, powder, smoothstep( 0.5, -0.5, cosTheta));
			}

			inline float3 SampleLight( float3 origin, float originDensity, float pixelAlpha, float3 cosAngle, float2 debugUV, float rayDistance, float3 RandomUnitSphere[6])
			{
				const float iterations = 5.0;
				
				float3 rayStep = -_LightDirection * (_SunRayLength / iterations);
				float3 ray = origin + rayStep;
				
				float atmosphereY = 0.0;

				float lod = step( 0.3, originDensity) * 3.0;
				lod = 0.0;

				float value = 0.0;

				float4 coverage;

				float3 randomOffset = float3( 0.0, 0.0, 0.0);
				float coneRadius = 0.0;
				const float coneStep = _ConeRadius / iterations;
				float energy = 0.0;

				float thickness = 0.0;

				for( float i=0.0; i<iterations; i++)
				{
					randomOffset = RandomUnitSphere[i] * coneRadius;
					ray += rayStep;
					atmosphereY = NormalizedAtmosphereY( ray);

					coverage = SampleCoverage( ray + randomOffset, atmosphereY, lod);
					value = SampleCloud( ray + randomOffset, originDensity, coverage, atmosphereY, lod);
					value *= float( atmosphereY <= 1.0);

					thickness += value;

					coneRadius += coneStep;
				}

				float far = 8.0;
				ray += rayStep * far;
				atmosphereY = NormalizedAtmosphereY( ray);
				coverage = SampleCoverage( ray, atmosphereY, lod);
				value = SampleCloud( ray, originDensity, coverage, atmosphereY, lod);
				value *= float( atmosphereY <= 1.0);
				thickness += value;


				float forwardP = HenyeyGreensteinPhase( cosAngle, _ForwardScatteringG);
				float backwardsP = HenyeyGreensteinPhase( cosAngle, _BackwardScatteringG);
				float P = (forwardP + backwardsP) / 2.0;

				return _LightColor * BeerTerm( thickness) * PowderTerm( originDensity, cosAngle) * P;
			}

			inline float3 SampleAmbientLight( float atmosphereY, float depth)
			{
				return lerp(_CloudBaseColor, _CloudTopColor, atmosphereY);
			}
			
			half4 frag( clouds_v2f i) : COLOR
			{
				half4 color = half4( 0.0, 0.0, 0.0, 0.0);
				float3 rayDirection = normalize( i.cameraRay);

				if( rayDirection.y > _RayMinimumY)
				{
					float2 uv = i.uv;
					float3 ray = InternalRaySphereIntersect(_EarthRadius + _StartHeight, _CameraPosition, rayDirection);
					float3 rayStep = rayDirection * _RayStepLength;
					float i=0;
					
					float atmosphereY = 0.0;
					float transmittance = 1.0;
					float rayStepScalar = 1.0;

					float cosAngle = dot( rayDirection, -_LightDirection);

					float normalizedDepth = 0.0;
					float zeroThreshold = 4.0;
					float zeroAccumulator = 0.0;
					const float3 RandomUnitSphere[6] = { _Random0, _Random1, _Random2, _Random3, _Random4, _Random5 };
					float value = 1.0;
					while( true)
					{
						if( i >= _MaxIterations || color.a >= 1.0 || atmosphereY >= 1.0)
						{
							break;
						}
						
						normalizedDepth = distance( _CameraPosition, ray) / _MaxDistance;
						float lod = step( _LODDistance, normalizedDepth);
						float4 coverage = SampleCoverage( ray, atmosphereY, lod);
						value = SampleCloud( ray, color.a, coverage, atmosphereY, lod);
						float4 particle = float4( value, value, value, value);

						if( value > 0.0)
						{
							zeroAccumulator = 0.0;

							if( rayStepScalar > 1.0)
							{
								ray -= rayStep * rayStepScalar;
								i -= rayStepScalar;

								atmosphereY = NormalizedAtmosphereY( ray);
								normalizedDepth = distance( _CameraPosition, ray) / _MaxDistance;
								lod = step( _LODDistance, normalizedDepth);
								coverage = SampleCoverage( ray, atmosphereY, lod);
								value = SampleCloud( ray, color.a, coverage, atmosphereY, lod);
								particle = float4( value, value, value, value);
							}

							float T = 1.0 - particle.a;
							transmittance *= T;

							float3 ambientLight = SampleAmbientLight( atmosphereY, normalizedDepth);
							float3 sunLight = SampleLight( ray, particle.a, color.a, cosAngle, uv, normalizedDepth, RandomUnitSphere);

							sunLight *= _LightScalar;
							ambientLight *= _AmbientScalar;

							particle.a = 1.0 - T;
							particle.rgb = sunLight + ambientLight;
							particle.rgb *= particle.a;

							color = (1.0 - color.a) * particle + color;
						}

						zeroAccumulator += float( value <= 0.0);
						rayStepScalar = 1.0 + step( zeroThreshold, zeroAccumulator) * 0.0;
						i += rayStepScalar;

						ray += rayStep * rayStepScalar;
						atmosphereY = NormalizedAtmosphereY( ray);
					}
					
					float fade = smoothstep( _RayMinimumY, 
											 _RayMinimumY + (1.0 - _RayMinimumY) * _HorizonFadeScalar, 
											 rayDirection.y);
					color *= _HorizonFadeStartAlpha + fade * _OneMinusHorizonFadeStartAlpha;
				}

				return color;
			}
			
			ENDCG
		}
	} 
}
