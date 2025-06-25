Shader "Unlit/VolumeTexture"
{
    Properties
    {
        _VolumeTex ("Volume Texture", 3D) = "" {}
        _DataMin ("Data Min", Float) = 0
        _DataMax ("Data Max", Float) = 8920
        _Steps ("Ray Steps", Int) = 75
        _StepSize ("Step Size", Float) = 0.005
        _Intensity ("Intensity", Range(0.001, 1)) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            #define VOLUME_DIM float3(512, 256, 512)

            sampler3D _VolumeTex;
            float _DataMin, _DataMax;
            int _Steps;
            float _StepSize;
            float _Intensity;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 localPos : TEXCOORD1; // Pass local position
            };

            v2f vert(float4 vertex : POSITION)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(vertex);
                o.worldPos = mul(unity_ObjectToWorld, vertex).xyz;
                o.localPos = vertex.xyz; // [-0.5, 0.5] for unit cube
                return o;
            }

            float4 getTF(float value)
            {
                // Only show values between 100 and 400 (soft tissue)
                if (value < 0.1 || value > 0.5)
                    return float4(0, 0, 0, 0); // transparent

                // Map value to grayscale
                return float4(value, value, value, 0.1);
            }

            bool intersectBox(float3 origin, float3 dir, float3 boxMin, float3 boxMax, out float t0, out float t1)
            {
                float3 inv_dir = 1.0 / dir;
                float3 t_min = (boxMin - origin) * inv_dir;
                float3 t_max = (boxMax - origin) * inv_dir;
                float3 t1_pos = max(t_min, t_max);
                float3 t0_neg = min(t_min, t_max);

                t0 = max(max(t0_neg.x, t0_neg.y), t0_neg.z);
                t1 = min(min(t1_pos.x, t1_pos.y), t1_pos.z);

                return t0 <= t1;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 rayOrigin = i.localPos; // Start inside the cube
                float3 objCamPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1)).xyz;

                float3 viewDir = normalize(rayOrigin - objCamPos); // In local space

                float3 boxMin = float3(-0.5, -0.5, -0.5);
                float3 boxMax = float3(0.5, 0.5, 0.5);

                float t0, t1;
                if (!intersectBox(rayOrigin, viewDir, boxMin, boxMax, t0, t1))
                    return float4(0, 0, 0, 0);

                float tStart = t0;
                float tEnd = t1;
                int steps = min(_Steps, 100);
                float stepSize = (tEnd - tStart) / steps;
                float4 color = float4(0, 0, 0, 0);

                [unroll]
                for (int j = 0; j < steps; j++)
                {
                    float t = tStart + j * stepSize;
                    float3 samplePos = rayOrigin + viewDir * t;

                    float3 volUV = (samplePos + 0.5) * VOLUME_DIM;
                    volUV /= max(VOLUME_DIM.x, max(VOLUME_DIM.y, VOLUME_DIM.z));

                    if (any(volUV < 0) || any(volUV > 1)) continue;

                    float val = tex3D(_VolumeTex, volUV).r;
                    val = saturate((val - _DataMin) / (_DataMax - _DataMin));
                    val *= _Intensity;

                    if (val < 0.01) continue;

                    float opacity = 0.02 * val;
                    float4 sampleColor = float4(val, val, val, opacity);
                    color += sampleColor * (1.0 - color.a);

                    if (color.a > 0.95) break;
                }

                return color;
            }
            
            ENDCG
        }
    }
}