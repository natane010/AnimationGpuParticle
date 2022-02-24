#include "Common.cginc"

sampler2D _PositionBuffer;
sampler2D _VelocityBuffer;
sampler2D _RotationBuffer;

half3 _Albedo;
half _Smoothness;
half _Metallic;

#if defined(SKINNER_TEXTURED)
sampler2D _AlbedoMap;
sampler2D _NormalMap;
half _NormalScale;
#endif

float2 _Scale; 

half _CutoffSpeed;
half _SpeedToIntensity;

struct Input
{
#if defined(SKINNER_TEXTURED)
    float2 uv_AlbedoMap;
#endif
#if defined(SKINNER_TWO_SIDED)
    fixed facing : VFACE;
#endif
    fixed4 color : COLOR;
};

void vert(inout appdata_full data)
{
    float id = data.texcoord1.x;
    float4 texcoord = float4(id, 0.5, 0, 0);
    float4 P = tex2Dlod(_PositionBuffer, texcoord);
    float4 V = tex2Dlod(_VelocityBuffer, texcoord);
    float4 R = tex2Dlod(_RotationBuffer, texcoord);

    half speed = length(V.xyz);
    half scale = ParticleScale(id, P.w + 0.5, V.w, _Scale);
    half intensity = saturate((speed - _CutoffSpeed) * _SpeedToIntensity);

    data.vertex.xyz = RotateVector(data.vertex.xyz, R) * scale + P.xyz;
    data.normal = RotateVector(data.normal, R);
#if defined(SKINNER_TEXTURED)
    data.tangent.xyz = RotateVector(data.tangent.xyz, R);
#endif
    data.color.rgb = ColorAnimation(id, intensity);
}

void surf(Input IN, inout SurfaceOutputStandard o)
{
#if defined(SKINNER_TEXTURED)
    o.Albedo = tex2D(_AlbedoMap, IN.uv_AlbedoMap).rgb * _Albedo;
#else
    o.Albedo = _Albedo;
#endif

#if defined(SKINNER_TEXTURED)
    half3 n = UnpackScaleNormal(tex2D(_NormalMap, IN.uv_AlbedoMap), _NormalScale);
    #if defined(SKINNER_TWO_SIDED)
    o.Normal = n * (IN.facing > 0 ? 1 : -1);
    #else
    o.Normal = n;
    #endif
#else
    #if defined(SKINNER_TWO_SIDED)
    o.Normal = half3(0, 0, IN.facing > 0 ? 1 : -1);
    #endif
#endif

    o.Smoothness = _Smoothness;
    o.Metallic = _Metallic;
    o.Emission = IN.color.rgb;
}