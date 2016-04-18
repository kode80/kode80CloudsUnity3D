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
	public class SharedProperties
	{
		public float _earthRadius;
		public float earthRadius { 
			get { return _earthRadius; }
			set {
				_earthRadius = value;
				_maxDistance = CalculateMaxDistance();
				_maxRayDistance = CalculateMaxRayDistance();
			}
		}

		private float _atmosphereStartHeight;
		public float atmosphereStartHeight {
			get { return _atmosphereStartHeight; }
			set {
				_atmosphereStartHeight = value;
				_maxDistance = CalculateMaxDistance();
				_maxRayDistance = CalculateMaxRayDistance();
			}
		}

		private float _atmosphereEndHeight;
		public float atmosphereEndHeight {
			get { return _atmosphereEndHeight; }
			set { 
				_atmosphereEndHeight = value;
				_maxDistance = CalculateMaxDistance();
				_maxRayDistance = CalculateMaxRayDistance();
			}
		}

		public Vector3 cameraPosition;
		
		private float _maxDistance;
		public float maxDistance { get { return _maxDistance; } }
		
		private float _maxRayDistance;
		public float maxRayDistance { get { return _maxRayDistance; } }

		public Matrix4x4 jitter;
		public Matrix4x4 previousProjection;
		public Matrix4x4 previousInverseRotation;
		public Matrix4x4 previousRotation;
		public Matrix4x4 projection;
		public Matrix4x4 inverseRotation;
		public Matrix4x4 rotation;

		private int _subFrameNumber;
		public int subFrameNumber { get { return _subFrameNumber; } }

		private int _downsample;
		public int downsample {
			get { return _downsample; }
			set {
				_downsample = value;
			}
		}

		private int _subPixelSize;
		public int subPixelSize { 
			get { return _subPixelSize; }
			set {
				_subPixelSize = value;
				_frameNumbers = CreateFrameNumbers( _subPixelSize);
				_subFrameNumber = 0;
			}
		}

		private bool _dimensionsChangedSinceLastFrame;
		public bool dimensionsChangedSinceLastFrame { get { return _dimensionsChangedSinceLastFrame; } }
		
		private int _subFrameWidth;
		public int subFrameWidth { get { return _subFrameWidth; } }

		private int _subFrameHeight;
		public int subFrameHeight { get { return _subFrameHeight; } }

		private int _frameWidth;
		public int frameWidth { get { return _frameWidth; } }

		private int _frameHeight;
		public int frameHeight { get { return _frameHeight; } }

		public bool useFixedDimensions;
		public int fixedWidth;
		public int fixedHeight;

		private int[] _frameNumbers;
		int _renderCount;

		public SharedProperties()
		{
			_renderCount = 0;
			downsample = 2;
			subPixelSize = 2;
		}

		public void BeginFrame( Camera camera)
		{
			UpdateFrameDimensions( camera);

			projection = camera.projectionMatrix;
			rotation = camera.worldToCameraMatrix;
			inverseRotation = camera.cameraToWorldMatrix;
			jitter = CreateJitterMatrix();
		}

		public void EndFrame()
		{
			previousProjection = projection;
			previousRotation = rotation;
			previousInverseRotation = inverseRotation;
			_dimensionsChangedSinceLastFrame = false;
			_renderCount++;
			_subFrameNumber = _frameNumbers[ _renderCount % (subPixelSize * subPixelSize)];
		}

		public void ApplyToMaterial( Material material, bool jitterProjection=false)
		{
			Matrix4x4 inverseProjection = projection.inverse;
			if( jitterProjection) { inverseProjection *= jitter; }

			material.SetFloat( "_EarthRadius", earthRadius);
			material.SetFloat( "_StartHeight", atmosphereStartHeight);
			material.SetFloat( "_EndHeight", atmosphereEndHeight);
			material.SetFloat( "_AtmosphereThickness", atmosphereEndHeight - atmosphereStartHeight);
			material.SetVector( "_CameraPosition", cameraPosition);
			material.SetFloat( "_MaxDistance", maxDistance);
			material.SetMatrix( "_PreviousProjection", previousProjection);
			material.SetMatrix( "_PreviousInverseProjection", previousProjection.inverse);
			material.SetMatrix( "_PreviousRotation", previousRotation);
			material.SetMatrix( "_PreviousInverseRotation", previousInverseRotation);
			material.SetMatrix( "_Projection", projection);
			material.SetMatrix( "_InverseProjection", inverseProjection);
			material.SetMatrix( "_Rotation", rotation);
			material.SetMatrix( "_InverseRotation", inverseRotation);
			material.SetFloat( "_SubFrameNumber", subFrameNumber);
			material.SetFloat( "_SubPixelSize", subPixelSize);
			material.SetVector( "_SubFrameSize", new Vector2( _subFrameWidth, _subFrameHeight));
			material.SetVector( "_FrameSize", new Vector2( _frameWidth, _frameHeight));
		}
		
		public Vector3 NormalizedPointToAtmosphere( Vector2 point, Camera theCamera)
		{
			point.x *= theCamera.pixelWidth;
			point.y *= theCamera.pixelHeight;
			return ScreenPointToAtmosphere( point, theCamera);
		}
		
		private Vector3 ScreenPointToAtmosphere( Vector2 screenPoint, Camera theCamera)
		{
			// Need to rework to use properties of class...

			Vector3 atmospherePoint = new Vector3();
			Vector2 uv = new Vector2( screenPoint.x / theCamera.pixelWidth, screenPoint.y / theCamera.pixelHeight);
			Vector4 screenRay = new Vector4( uv.x * 2.0f - 1.0f, uv.y * 2.0f - 1.0f, 1.0f, 1.0f);
			screenRay = theCamera.projectionMatrix.inverse * screenRay;
			screenRay.x /= screenRay.w;
			screenRay.y /= screenRay.w;
			screenRay.z /= screenRay.w;
			
			Vector3 rayDirection = new Vector3( screenRay.x, screenRay.y, screenRay.z);
			rayDirection = theCamera.cameraToWorldMatrix.MultiplyVector( rayDirection).normalized;
			
			float radius = earthRadius + atmosphereStartHeight;

			atmospherePoint = InternalRaySphereIntersect( radius, cameraPosition, rayDirection);
			
			return atmospherePoint;
		}

		private void UpdateFrameDimensions( Camera camera)
		{
			int newFrameWidth = useFixedDimensions ? fixedWidth : camera.pixelWidth / downsample;
			int newFrameHeight = useFixedDimensions ? fixedHeight : camera.pixelHeight / downsample;

			while( (newFrameWidth % _subPixelSize) != 0) { newFrameWidth++; }
			while( (newFrameHeight % _subPixelSize) != 0) { newFrameHeight++; }

			int newSubFrameWidth = newFrameWidth / _subPixelSize;
			int newSubFrameHeight = newFrameHeight / _subPixelSize;

			_dimensionsChangedSinceLastFrame = newFrameWidth != _frameWidth ||
											   newFrameHeight != _frameHeight ||
											   newSubFrameWidth != _subFrameWidth ||
											   newSubFrameHeight != _subFrameHeight;

			_frameWidth = newFrameWidth;
			_frameHeight = newFrameHeight;
			_subFrameWidth = newSubFrameWidth;
			_subFrameHeight = newSubFrameHeight;
		}

		private int[] CreateFrameNumbers( int subPixelSize)
		{
			int frameCount = subPixelSize * subPixelSize;
			int i=0;
			int[] frameNumbers = new int[ frameCount];

			for( i=0; i<frameCount; i++) 
			{ frameNumbers[i] = i; }
			
			while( i-- > 0) 
			{ 
				int k = frameNumbers[ i];
				int j = (int)(Random.value * 1000.0f) % frameCount;
				frameNumbers[i] = frameNumbers[j];
				frameNumbers[j] = k; 
			}

			return frameNumbers;
		}

		private Matrix4x4 CreateJitterMatrix()
		{
			int x = subFrameNumber % subPixelSize;
			int y = subFrameNumber / subPixelSize;
			
			Vector3 jitter = new Vector3( x * 2.0f / _frameWidth, 
			                   			  y * 2.0f / _frameHeight);

			return Matrix4x4.TRS( jitter, Quaternion.identity, Vector3.one);
		}

		private float CalculateHorizonDistance( float innerRadius, float outerRadius)
		{
			return Mathf.Sqrt( (outerRadius * outerRadius) - (innerRadius * innerRadius));
		}

		public float CalculatePlanetRadius( float atmosphereHeight, float horizonDistance)
		{
			float atmosphereRadius = atmosphereHeight * atmosphereHeight + horizonDistance * horizonDistance;
			atmosphereRadius /= 2.0f * atmosphereHeight;

			return atmosphereRadius - atmosphereHeight;
		}

		private float CalculateMaxDistance()
		{
			return CalculateHorizonDistance( earthRadius, earthRadius + atmosphereEndHeight);
		}
		
		private float CalculateMaxRayDistance()
		{
			float cloudInnerDistance = CalculateHorizonDistance( earthRadius, earthRadius + atmosphereStartHeight);
			float cloudOuterDistance = CalculateHorizonDistance( earthRadius, earthRadius + atmosphereEndHeight);
			return cloudOuterDistance - cloudInnerDistance;
		}
		
		private Vector3 InternalRaySphereIntersect( float sphereRadius, Vector3 origin, Vector3 rayDirection)
		{	
			float a0 = sphereRadius * sphereRadius - Vector3.Dot(origin, origin);
			float a1 = Vector3.Dot( origin, rayDirection);
			float result = Mathf.Sqrt(a1 * a1 + a0) - a1;
			
			return origin + rayDirection * result;
		}
	}
}