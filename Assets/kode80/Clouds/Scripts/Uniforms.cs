using UnityEngine;

namespace kode80.Clouds
{
    internal static class Uniforms
    {
        internal static readonly int _CloudBottomFade = Shader.PropertyToID("_CloudBottomFade");
        internal static readonly int _MaxIterations = Shader.PropertyToID("_MaxIterations");
        internal static readonly int _SampleScalar = Shader.PropertyToID("_SampleScalar");
        internal static readonly int _SampleThreshold = Shader.PropertyToID("_SampleThreshold");
        internal static readonly int _LODDistance = Shader.PropertyToID("_LODDistance");
        internal static readonly int _RayMinimumY = Shader.PropertyToID("_RayMinimumY");
        internal static readonly int _DetailScale = Shader.PropertyToID("_DetailScale");
        internal static readonly int _ErosionEdgeSize = Shader.PropertyToID("_ErosionEdgeSize");
        internal static readonly int _CloudDistortion = Shader.PropertyToID("_CloudDistortion");
        internal static readonly int _CloudDistortionScale = Shader.PropertyToID("_CloudDistortionScale");
        internal static readonly int _HorizonFadeScalar = Shader.PropertyToID("_HorizonFadeScalar");
        internal static readonly int _HorizonFadeStartAlpha = Shader.PropertyToID("_HorizonFadeStartAlpha");
        internal static readonly int _OneMinusHorizonFadeStartAlpha = Shader.PropertyToID("_OneMinusHorizonFadeStartAlpha");
        internal static readonly int _Perlin3D = Shader.PropertyToID("_Perlin3D");
        internal static readonly int _Detail3D = Shader.PropertyToID("_Detail3D");
        internal static readonly int _BaseOffset = Shader.PropertyToID("_BaseOffset");
        internal static readonly int _DetailOffset = Shader.PropertyToID("_DetailOffset");
        internal static readonly int _BaseScale = Shader.PropertyToID("_BaseScale");
        internal static readonly int _LightScalar = Shader.PropertyToID("_LightScalar");
        internal static readonly int _AmbientScalar = Shader.PropertyToID("_AmbientScalar");
        internal static readonly int _CloudHeightGradient1 = Shader.PropertyToID("_CloudHeightGradient1");
        internal static readonly int _CloudHeightGradient2 = Shader.PropertyToID("_CloudHeightGradient2");
        internal static readonly int _CloudHeightGradient3 = Shader.PropertyToID("_CloudHeightGradient3");
        internal static readonly int _Coverage = Shader.PropertyToID("_Coverage");
        internal static readonly int _LightDirection = Shader.PropertyToID("_LightDirection");
        internal static readonly int _LightColor = Shader.PropertyToID("_LightColor");
        internal static readonly int _CloudBaseColor = Shader.PropertyToID("_CloudBaseColor");
        internal static readonly int _CloudTopColor = Shader.PropertyToID("_CloudTopColor");
        internal static readonly int _HorizonCoverageStart = Shader.PropertyToID("_HorizonCoverageStart");
        internal static readonly int _HorizonCoverageEnd = Shader.PropertyToID("_HorizonCoverageEnd");
        internal static readonly int _Density = Shader.PropertyToID("_Density");
        internal static readonly int _ForwardScatteringG = Shader.PropertyToID("_ForwardScatteringG");
        internal static readonly int _BackwardScatteringG = Shader.PropertyToID("_BackwardScatteringG");
        internal static readonly int _DarkOutlineScalar = Shader.PropertyToID("_DarkOutlineScalar");
        internal static readonly int _SunRayLength = Shader.PropertyToID("_SunRayLength");
        internal static readonly int _ConeRadius = Shader.PropertyToID("_ConeRadius");
        internal static readonly int _RayStepLength = Shader.PropertyToID("_RayStepLength");
        internal static readonly int _Curl2D = Shader.PropertyToID("_Curl2D");
        internal static readonly int _CoverageScale = Shader.PropertyToID("_CoverageScale");
        internal static readonly int _CoverageOffset = Shader.PropertyToID("_CoverageOffset");
        internal static readonly int _MaxRayDistance = Shader.PropertyToID("_MaxRayDistance");
        internal static readonly int _Random0 = Shader.PropertyToID("_Random0");
        internal static readonly int _Random1 = Shader.PropertyToID("_Random1");
        internal static readonly int _Random2 = Shader.PropertyToID("_Random2");
        internal static readonly int _Random3 = Shader.PropertyToID("_Random3");
        internal static readonly int _Random4 = Shader.PropertyToID("_Random4");
        internal static readonly int _Random5 = Shader.PropertyToID("_Random5");

        internal static readonly int _MainTex = Shader.PropertyToID("_MainTex");
        internal static readonly int _IsGamma = Shader.PropertyToID("_IsGamma");
        internal static readonly int _SubFrame = Shader.PropertyToID("_SubFrame");
        internal static readonly int _PrevFrame = Shader.PropertyToID("_PrevFrame");

        internal static readonly int _EarthRadius = Shader.PropertyToID("_EarthRadius");
        internal static readonly int _StartHeight = Shader.PropertyToID("_StartHeight");
        internal static readonly int _EndHeight = Shader.PropertyToID("_EndHeight");
        internal static readonly int _AtmosphereThickness = Shader.PropertyToID("_AtmosphereThickness");
        internal static readonly int _CameraPosition = Shader.PropertyToID("_CameraPosition");
        internal static readonly int _MaxDistance = Shader.PropertyToID("_MaxDistance");
        internal static readonly int _PreviousProjection = Shader.PropertyToID("_PreviousProjection");
        internal static readonly int _PreviousInverseProjection = Shader.PropertyToID("_PreviousInverseProjection");
        internal static readonly int _PreviousRotation = Shader.PropertyToID("_PreviousRotation");
        internal static readonly int _PreviousInverseRotation = Shader.PropertyToID("_PreviousInverseRotation");
        internal static readonly int _Projection = Shader.PropertyToID("_Projection");
        internal static readonly int _InverseProjection = Shader.PropertyToID("_InverseProjection");
        internal static readonly int _Rotation = Shader.PropertyToID("_Rotation");
        internal static readonly int _InverseRotation = Shader.PropertyToID("_InverseRotation");
        internal static readonly int _SubFrameNumber = Shader.PropertyToID("_SubFrameNumber");
        internal static readonly int _SubPixelSize = Shader.PropertyToID("_SubPixelSize");
        internal static readonly int _SubFrameSize = Shader.PropertyToID("_SubFrameSize");
        internal static readonly int _FrameSize = Shader.PropertyToID("_FrameSize");

        internal static readonly int _Clouds = Shader.PropertyToID("_Clouds");
        internal static readonly int _SunScreenSpace = Shader.PropertyToID("_SunScreenSpace");
        internal static readonly int _SampleCount = Shader.PropertyToID("_SampleCount");
        internal static readonly int _Decay = Shader.PropertyToID("_Decay");
        internal static readonly int _Weight = Shader.PropertyToID("_Weight");
        internal static readonly int _Exposure = Shader.PropertyToID("_Exposure");

        internal static readonly int _CloudCoverage = Shader.PropertyToID("_CloudCoverage");
        internal static readonly int _InvCamera = Shader.PropertyToID("_InvCamera");
        internal static readonly int _InvProjection = Shader.PropertyToID("_InvProjection");
        internal static readonly int _Offset = Shader.PropertyToID("_Offset");
        internal static readonly int _ShadowStrength = Shader.PropertyToID("_ShadowStrength");

        internal static readonly int _CoverageOpacity = Shader.PropertyToID("_CoverageOpacity");
        internal static readonly int _TypeOpacity = Shader.PropertyToID("_TypeOpacity");
        internal static readonly int _ShouldDrawCoverage = Shader.PropertyToID("_ShouldDrawCoverage");
        internal static readonly int _ShouldDrawType = Shader.PropertyToID("_ShouldDrawType");
        internal static readonly int _ShouldBlendValues = Shader.PropertyToID("_ShouldBlendValues");
        internal static readonly int _BrushTexture = Shader.PropertyToID("_BrushTexture");
        internal static readonly int _BrushTextureAlpha = Shader.PropertyToID("_BrushTextureAlpha");

        internal static readonly int _SunSize = Shader.PropertyToID("_SunSize");
    }
}
