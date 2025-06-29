Shader "Custom/MedicalVolume" {
    Properties {
        [NoScaleOffset] _Volume("3D Texture", 3D) = "" {}

        _SliceAxis1Min("Slice along axis X: min", Range(0, 1)) = 0
        _SliceAxis1Max("Slice along axis X: max", Range(0, 1)) = 1
        _SliceAxis2Min("Slice along axis Y: min", Range(0, 1)) = 0
        _SliceAxis2Max("Slice along axis Y: max", Range(0, 1)) = 1
        _SliceAxis3Min("Slice along axis Z: min", Range(0, 1)) = 0
        _SliceAxis3Max("Slice along axis Z: max", Range(0, 1)) = 1

        _DataMin("Data threshold: min", Range(0, 1)) = 0
        _DataMax("Data threshold: max", Range(0, 1)) = 1
        _Iterations("RayMarching Iterations", Int) = 2048
        _Normalisation("Intensity normalisation", Float) = 1
        _VolumeScale("Volume Scale", Vector) = (1,1,1,1)
    }

    SubShader {
        Pass {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler3D _Volume;
            float _SliceAxis1Min, _SliceAxis1Max;
            float _SliceAxis2Min, _SliceAxis2Max;
            float _SliceAxis3Min, _SliceAxis3Max;

            float _DataMin, _DataMax;
            int _Iterations;
            float _Normalisation;
            float3 _VolumeScale;

            bool intersect_box(float3 ray_o, float3 ray_d, float3 boxMin, float3 boxMax, out float tNear, out float tFar)
            {
                float3 invR = 1.0 / ray_d;
                float3 tBot = invR * (boxMin - ray_o);
                float3 tTop = invR * (boxMax - ray_o);

                float3 tMin = min(tBot, tTop);
                float3 tMax = max(tBot, tTop);

                float largest_tMin = max(max(tMin.x, tMin.y), tMin.z);
                float smallest_tMax = min(min(tMax.x, tMax.y), tMax.z);

                bool hit = (largest_tMin <= smallest_tMax);
                tNear = largest_tMin;
                tFar = smallest_tMax;

                return hit;
            }

            struct vert_input {
                float4 pos : POSITION;
            };

            struct frag_input {
                float4 pos : SV_POSITION;
                float3 ray_o : TEXCOORD0;
                float3 ray_d : TEXCOORD1;
            };

            frag_input vert(vert_input i) {
                   frag_input o;

                    // Get world-space camera info
                    float3 camPosWS = _WorldSpaceCameraPos;
                    float4x4 objToWorld = unity_ObjectToWorld;
                    float4x4 worldToObj = unity_WorldToObject;

                    // World space vertex position
                    float3 vertexWS = mul(objToWorld, i.pos).xyz;

                    // Ray direction in world space
                    float3 rayDirWS = normalize(vertexWS - camPosWS);

                    // Convert to object space
                    float3 rayOriginOS = mul(worldToObj, float4(camPosWS, 1)).xyz;
                    float3 rayDirOS = mul(worldToObj, float4(rayDirWS, 0)).xyz;

                    o.ray_o = rayOriginOS;
                    o.ray_d = rayDirOS;

                    o.pos = UnityObjectToClipPos(i.pos);
                    return o;
            }

            float4 get_data(float3 pos) {
                if (any(pos < 0) || any(pos > 1))
                    return 0;

                float4 data = tex3Dlod(_Volume, float4(pos, 0));

                float mask =
                    step(_SliceAxis1Min, pos.x) * step(pos.x, _SliceAxis1Max) *
                    step(_SliceAxis2Min, pos.y) * step(pos.y, _SliceAxis2Max) *
                    step(_SliceAxis3Min, pos.z) * step(pos.z, _SliceAxis3Max);

                float dataMask = step(_DataMin, data.a) * step(data.a, _DataMax);

                float4 result = data;
                result.a *= mask * dataMask;

                return result;
            }

            float4 frag(frag_input i) : COLOR {
               
                float3 boxMin = -_VolumeScale * 0.5;
                float3 boxMax =  _VolumeScale * 0.5;

                float tNear, tFar;
                bool hit = intersect_box(i.ray_o, i.ray_d, boxMin, boxMax, tNear, tFar);
                if (!hit) discard;

                if (tNear < 0.0) tNear = 0.0;

                // Compute start and end points along ray inside volume
                float3 pNear = i.ray_o + i.ray_d * tNear;
                float3 pFar = i.ray_o + i.ray_d * tFar;

                // Convert to [0,1] UV space relative to scaled volume
                float3 start = (pNear - boxMin) / (_VolumeScale);
                float3 end = (pFar - boxMin) / (_VolumeScale);

                float3 delta = end - start;
                float length = distance(start, end);
                float3 direction = normalize(delta);

                float3 rayStep = direction * (length / _Iterations);

                float4 accumColor = 0;
                float3 pos = start;

                [loop] for (int k = 0; k < _Iterations; ++k) {
                    float4 sample = get_data(pos);

                    float4 src = sample;
                    src.rgb = src.aaa; 

                    accumColor.rgb += (1 - accumColor.a) * src.a * src.rgb;
                    accumColor.a += (1 - accumColor.a) * src.a;

                    pos += rayStep;

                    if (any(pos < 0) || any(pos > 1))
                        break;
                }

                if (accumColor.a < 0.01)
                    discard;

                return accumColor * _Normalisation;
            }
            ENDCG
        }
    }
    FallBack Off
}