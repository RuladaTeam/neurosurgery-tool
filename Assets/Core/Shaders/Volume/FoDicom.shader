Shader "VolumeRendering/FoDicom"
{
    Properties
    {
        _VolumeTex ("Volume Texture", 3D) = "white" {}
        _DataMin ("Data Min", Float) = -1000
        _DataMax ("Data Max", Float) = 4000
        _StepSize ("Step Size", Range(0.001, 0.1)) = 0.01
        _Intensity ("Intensity", Range(0, 5)) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert

        sampler3D _VolumeTex;
        float _DataMin;
        float _DataMax;
        float _StepSize;
        float _Intensity;

        struct Input
        {
            float3 worldPos;
            float3 barycentric;
        };

        float3 boxMin;
        float3 boxMax;

        void vert (inout appdata_full v, out Input data)
        {
            UNITY_INITIALIZE_OUTPUT(Input, data);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            data.worldPos = worldPos;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 rayOrigin = IN.worldPos;
            float3 rayDir = normalize(WorldSpaceViewDir(IN.worldPos)).xyz;

            boxMin = mul(unity_ObjectToWorld, float4(-0.5, -0.5, -0.5, 1)).xyz;
            boxMax = mul(unity_ObjectToWorld, float4(0.5, 0.5, 0.5, 1)).xyz;

            float3 bbmin = (boxMin - rayOrigin) / rayDir;
            float3 bbmax = (boxMax - rayOrigin) / rayDir;
            float3 tmin = min(bbmin, bbmax);
            float t0 = max(max(tmin.x, tmin.y), tmin.z);
            float3 tmax = max(bbmin, bbmax);
            float t1 = min(min(tmax.x, tmax.y), tmax.z);

            if (t0 > t1 || t1 < 0)
            {
                discard;
            }

            float3 start = rayOrigin + rayDir * t0;
            float3 end = rayOrigin + rayDir * t1;
            float3 delta = end - start;
            float length = sqrt(dot(delta, delta));
            int steps = ceil(length / _StepSize);
            float3 stepDir = delta / steps;

            float4 accumulatedColor = float4(0, 0, 0, 0);
            float3 pos = start;

            for (int i = 0; i < steps && accumulatedColor.a < 0.99; ++i)
            {
                float3 localPos = (mul(unity_WorldToObject, float4(pos, 1)).xyz + 0.5);
                float density = tex3D(_VolumeTex, localPos).r;
                float opacity = smoothstep(0, 1, density) * _Intensity;
                float4 color = float4(density, density, density, opacity);
                accumulatedColor.rgb += (1.0 - accumulatedColor.a) * color.rgb * color.a;
                accumulatedColor.a += color.a;
                pos += stepDir;
            }

            o.Albedo = accumulatedColor.rgb;
            o.Alpha = accumulatedColor.a;
            o.Metallic = 0;
            o.Smoothness = 0;
        }
        ENDCG
    }

    FallBack "Diffuse"
}