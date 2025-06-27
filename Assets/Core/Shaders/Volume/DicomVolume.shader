Shader "VolumeRendering/FoDicom"
{

Properties
{
    [Header(Rendering)]
    _Volume("Volume", 3D) = "" {}
    _Transfer("Transfer", 2D) = "" {}
    _Iteration("Iteration", Int) = 1000
    _Intensity("Intensity", Range(0.0, 1.0)) = 0.1
    _DataMin("Data min", Range(-1000, 8920)) = 0
    _DataMax("Data max", Range(-1000, 8920)) = 8920
    _yOffset("Y Offset", Range(-1, 1)) = 0.58
    _VolumeSpacing("Volume Spacing", Vector) = (1,2,1)

    [Header(Ranges)]
    _MinX("MinX", Range(0, 1)) = 0.0
    _MaxX("MaxX", Range(0, 1)) = 1.0
    _MinY("MinY", Range(0, 1)) = 0.0
    _MaxY("MaxY", Range(0, 1)) = 1.0
    _MinZ("MinZ", Range(0, 1)) = 0.0
    _MaxZ("MaxZ", Range(0, 1)) = 1.0
}

CGINCLUDE

#include "UnityCG.cginc"

struct appdata
{
    float4 vertex : POSITION;
};

struct v2f
{
    float4 vertex   : SV_POSITION;
    float4 localPos : TEXCOORD0;
    float4 worldPos : TEXCOORD1;
};

sampler3D _Volume;
sampler2D _Transfer;
int _Iteration;
float _Intensity;
float _MinX, _MaxX, _MinY, _MaxY, _MinZ, _MaxZ;
float _DataMin, _DataMax;
float _yOffset;

float3 _VolumeSpacing;

struct Ray
{
    float3 from;
    float3 dir;
    float tmax;
};

void intersection(inout Ray ray)
{
    float3 invDir = 1.0 / ray.dir;
    float3 t1 = (-0.5 - ray.from) * invDir;
    float3 t2 = (+0.5 - ray.from) * invDir;
    float3 tmax3 = max(t1, t2);
    float2 tmax2 = min(tmax3.xx, tmax3.yz);
    ray.tmax = min(tmax2.x, tmax2.y);
}

inline float sampleVolume(float3 pos)
{
   bool inside = all(pos >= float3(_MinX, _MinY, _MinZ)) &&
                  all(pos <= float3(_MaxX, _MaxY, _MaxZ));

    if (!inside) {
        return 0.0;
    }

    pos.y += _yOffset;

    float val = tex3D(_Volume, pos).r;
    float normalizedVal = saturate((val - _DataMin) / (_DataMax - _DataMin));

    return normalizedVal;
}

inline float4 transferFunction(float t)
{
    return tex2D(_Transfer, float2(t, 0));
}

v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.localPos = v.vertex;
    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
    return o;
}

float4 frag(v2f i) : SV_Target
{
     float3 worldDir = i.worldPos - _WorldSpaceCameraPos;
    float3 localDir = normalize(mul(unity_WorldToObject, float4(worldDir, 0)).xyz);

    Ray ray;
    ray.from = i.localPos;
    ray.dir = localDir;
    intersection(ray);

    // Normalize the spacing to ensure consistent traversal density
    float3 invSpacing = 1.0 / _VolumeSpacing;
    float3 adjustedDir = localDir * invSpacing;
    float dirLength = length(adjustedDir);
    adjustedDir /= dirLength;

    int n = 100;
    float stepSize = ray.tmax / n;

    float3 localStep = adjustedDir * stepSize;
    float3 localPos = ray.from + 0.5;

    float4 output = 0;

    [unroll(n)]
    for (int i = 0; i < n; ++i)
    {
        // Clamp position based on bounding box
        if (any(localPos < 0.0) || any(localPos > 1.0)) break;

        float volume = sampleVolume(localPos);
        float4 color = transferFunction(volume) * volume * _Intensity;
        output += (1.0 - output.a) * color;
        localPos += localStep;
    }

    return output;
}

ENDCG

SubShader
{

Tags 
{ 
    "Queue" = "Transparent"
    "RenderType" = "Transparent" 
}

Pass
{
    Cull Back
    ZWrite Off
    ZTest LEqual
    Blend One OneMinusSrcAlpha
    Lighting Off

    CGPROGRAM
    #pragma vertex vert
    #pragma fragment frag
    ENDCG
}

}

}