#include "UnityCG.cginc"


float _RandomSeed;


float UVRandom(float2 uv, float salt)
{
    uv += float2(salt, _RandomSeed);
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}


float4 QMult(float4 q1, float4 q2)
{
    float3 ijk = q2.xyz * q1.w + q1.xyz * q2.w + cross(q1.xyz, q2.xyz);
    return float4(ijk, q1.w * q2.w - dot(q1.xyz, q2.xyz));
}


float3 RotateVector(float3 v, float4 r)
{
    float4 r_c = r * float4(-1, -1, -1, 1);
    return QMult(r, QMult(float4(v, 0), r_c)).xyz;
}


half2 StereoProjection(half3 n)
{
    return n.xy / (1 - n.z);
}

half3 StereoInverseProjection(half2 p)
{
    float d = 2 / (dot(p.xy, p.xy) + 1);
    return float3(p.xy * d, 1 - d);
}


half TriangleArea(half a, half b, half c)
{
    half s = 0.5 * (a + b + c);
    return sqrt(s * (s - a) * (s - b) * (s - c));
}


half3 HueToRGB(half h)
{
    h = frac(h);
    half r = abs(h * 6 - 3) - 1;
    half g = 2 - abs(h * 6 - 2);
    half b = 2 - abs(h * 6 - 4);
    half3 rgb = saturate(half3(r, g, b));
#if UNITY_COLORSPACE_GAMMA
    return rgb;
#else
    return GammaToLinearSpace(rgb);
#endif
}


half _BaseHue;
half _HueRandomness;
half _Saturation;
half _Brightness;
half _EmissionProb;
half _HueShift;
half _BrightnessOffs;

half3 ColorAnimation(float id, half intensity)
{
    half phase = UVRandom(id, 30) * 32 + _Time.y * 4;
    half lfo = abs(sin(phase * UNITY_PI));
    lfo *= UVRandom(id + floor(phase), 31) < _EmissionProb;    
    half hue = _BaseHue + UVRandom(id, 32) * _HueRandomness + _HueShift * intensity;
    half3 rgb = lerp(1, HueToRGB(hue), _Saturation);

    
    return rgb * (_Brightness * lfo + _BrightnessOffs * intensity);
}

float ParticleScale(float id, half life, half speed, half2 params)
{
    
    half s = min((1 - life) * 20, min(life * 3, 1));
   
    s *= min(speed * params.y, params.x);
   
    s *= 1 - 0.5 * UVRandom(id, 20);
    return s;
}
