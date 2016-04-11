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

#ifndef VOLUME_CLOUDS_COMMON
#define VOLUME_CLOUDS_COMMON

float _EarthRadius;
float _StartHeight;
float _EndHeight;
float _AtmosphereThickness;
float3 _CameraPosition;
float _MaxDistance;
float4x4 _PreviousProjection;
float4x4 _PreviousInverseProjection;
float4x4 _PreviousRotation;
float4x4 _PreviousInverseRotation;
float4x4 _Projection;
float4x4 _InverseProjection;
float4x4 _Rotation;
float4x4 _InverseRotation;
float _SubFrameNumber;
float _SubPixelSize;
float2 _SubFrameSize;
float2 _FrameSize;

struct clouds_v2f {
   float4 position : SV_POSITION;
   float2 uv : TEXCOORD0;
   float3 cameraRay : TEXCOORD2;
};


inline float3 UVToCameraRay( float2 uv)
{
	float4 cameraRay = float4( uv * 2.0 - 1.0, 1.0, 1.0);
	cameraRay = mul( _InverseProjection, cameraRay);
	cameraRay = cameraRay / cameraRay.w;
	
	return mul( (float3x3)_InverseRotation, cameraRay.xyz);
}

inline float3 InternalRaySphereIntersect( float sphereRadius, float3 origin, float3 direction)
{	
	float a0 = sphereRadius * sphereRadius - dot( origin, origin);
	float a1 = dot( origin, direction);
	float result = sqrt(a1 * a1 + a0) - a1;
	
	return origin + direction * result;
}

#endif