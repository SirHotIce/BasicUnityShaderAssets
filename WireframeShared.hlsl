#ifndef WIREFRAME_SHARED
#define WIREFRAME_SHARED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

CBUFFER_START(UnityPerMaterial)
float4 _Color;
float _Thickness;
CBUFFER_END

struct Attributes
{
    float3 positionOS : POSITION;
    float4 bary : COLOR;
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float3 bary : TEXCOORD0;
};

Varyings vert(Attributes v)
{
    Varyings o;
    float3 worldPos = TransformObjectToWorld(v.positionOS);
    o.positionCS = TransformWorldToHClip(worldPos);
    o.bary = v.bary.rgb;
    return o;
}

float edgeFactor(float3 bary)
{
    float3 d = fwidth(bary);
    float minB = min(min(bary.x, bary.y), bary.z);
    float smoothing = max(max(d.x, d.y), d.z);
    return smoothstep(0.0, _Thickness * smoothing, minB);
}

float4 fragColor(Varyings i, float4 wireColor)
{
    float edge = edgeFactor(i.bary);
    float alpha = 1.0 - edge;
    return float4(wireColor.rgb * alpha, alpha);
}

#endif
