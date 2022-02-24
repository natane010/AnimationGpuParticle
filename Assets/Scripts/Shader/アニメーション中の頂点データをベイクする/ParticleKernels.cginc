#include "Common.cginc"
#include "SimplexNoiseGrad3D.cginc"

sampler2D _SourcePositionBuffer0;
sampler2D _SourcePositionBuffer1;
sampler2D _PositionBuffer;
sampler2D _VelocityBuffer;
sampler2D _RotationBuffer;

half2 _Damper;      
half3 _Gravity;
half2 _Life;        
half2 _Spin;        
half2 _NoiseParams; 
float3 _NoiseOffset;

float3 RotationAxis(float2 uv)
{
    float u = UVRandom(uv, 10) * 2 - 1;
    float u2 = sqrt(1 - u * u);
    float sn, cs;
    sincos(UVRandom(uv, 11) * UNITY_PI * 2, sn, cs);
    return float3(u2 * cs, u2 * sn, u);
}


float4 NewParticlePosition(float2 uv)
{
    uv = float2(UVRandom(uv, _Time.x), 0.5);    
    float3 p = tex2D(_SourcePositionBuffer1, uv).xyz;
    return float4(p, 0.5);
}

float4 NewParticleVelocity(float2 uv)
{
    uv = float2(UVRandom(uv, _Time.x), 0.5);
    float3 p0 = tex2D(_SourcePositionBuffer0, uv).xyz;
    float3 p1 = tex2D(_SourcePositionBuffer1, uv).xyz;
    float3 v = (p1 - p0) * unity_DeltaTime.y;
    v *= 1 - UVRandom(uv, 12) * 0.5;
    return float4(v, length(v));
}

float4 NewParticleRotation(float2 uv)
{
    float r = UVRandom(uv, 13);
    float r1 = sqrt(1 - r);
    float r2 = sqrt(r);
    float t1 = UNITY_PI * 2 * UVRandom(uv, 14);
    float t2 = UNITY_PI * 2 * UVRandom(uv, 15);
    return float4(sin(t1) * r1, cos(t1) * r1, sin(t2) * r2, cos(t2) * r2);
}

float4 InitializePositionFragment(v2f_img i) : SV_Target
{
    return float4(1e+6, 1e+6, 1e+6, UVRandom(i.uv, 16) - 0.5);
}

float4 InitializeVelocityFragment(v2f_img i) : SV_Target
{
    return 0;
}

float4 InitializeRotationFragment(v2f_img i) : SV_Target
{
    return NewParticleRotation(i.uv);
}

float4 UpdatePositionFragment(v2f_img i) : SV_Target
{
    float4 p = tex2D(_PositionBuffer, i.uv);
    float4 v = tex2D(_VelocityBuffer, i.uv);

    float rnd = 1 + UVRandom(i.uv, 17) * 0.5;
    p.w -= max(_Life.x,  _Life.y / v.w) * rnd;

    if (p.w > -0.5)
    {
        float lv = max(length(v.xyz), 1e-6);
        v.xyz = v * min(lv, _Damper.y) / lv;

        p.xyz += v.xyz * unity_DeltaTime.x;
        return p;
    }
    else
    {
        return NewParticlePosition(i.uv);
    }
}

float4 UpdateVelocityFragment(v2f_img i) : SV_Target
{
    float4 p = tex2D(_PositionBuffer, i.uv);
    float4 v = tex2D(_VelocityBuffer, i.uv);

    if (p.w < 0.5)
    {
        v.xyz = v.xyz * _Damper.x + _Gravity.xyz;

        float3 np = (p.xyz + _NoiseOffset) * _NoiseParams.x;
        float3 n1 = snoise_grad(np);
        float3 n2 = snoise_grad(np + float3(21.83, 13.28, 7.32));
        v.xyz += cross(n1, n2) * _NoiseParams.y;
        return v;
    }
    else
    {
        return NewParticleVelocity(i.uv);
    }
}

float4 UpdateRotationFragment(v2f_img i) : SV_Target
{
    float4 r = tex2D(_RotationBuffer, i.uv);
    float4 v = tex2D(_VelocityBuffer, i.uv);

    float delta = min(_Spin.x, length(v.xyz) * _Spin.y);
    delta *= 1 - UVRandom(i.uv, 18) * 0.5;

    float sn, cs;
    sincos(delta, sn, cs);
    float4 dq = float4(RotationAxis(i.uv) * sn, cs);

    return normalize(QMult(dq, r));
}