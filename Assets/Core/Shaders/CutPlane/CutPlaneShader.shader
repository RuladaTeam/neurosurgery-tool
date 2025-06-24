Shader "Custom/CutPlaneWithVertexColor"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _CutPlanePoint ("Cut Plane Point (Local Space)", Vector) = (0,0,0,1)
        _CutPlaneNormal ("Cut Plane Normal (Local Space)", Vector) = (0,1,0,0)
        _CutSmoothness ("Cut Smoothness", Range(0.001, 0.5)) = 0.01
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            Name "ForwardBase"
            Tags { "LightMode" = "ForwardBase" }
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 worldPos : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float4 vertexColor : COLOR;
                float4 localPos : TEXCOORD2; // Local position for cut plane
            };

            fixed4 _BaseColor;
            float4 _CutPlanePoint;
            float4 _CutPlaneNormal;
            float _CutSmoothness;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.vertexColor = v.color;
                o.localPos = v.vertex; // Store local space position
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // Signed distance to the cut plane in LOCAL SPACE
                float3 planeNormal = normalize(_CutPlaneNormal.xyz);
                float dist = dot(i.localPos.xyz - _CutPlanePoint.xyz, planeNormal);

                // Smoothstep-based clipping
                float cutFactor = smoothstep(-_CutSmoothness, _CutSmoothness, dist);
                clip(cutFactor - 0.5); // Discard fragments behind the plane

                // Lighting
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float ndotl = max(0.0, dot(i.normal, lightDir));
                float3 diffuseTerm = ndotl * _LightColor0.rgb;

                // Combine base color + vertex color + lighting
                fixed4 finalColor = _BaseColor * i.vertexColor;
                finalColor.rgb *= (diffuseTerm + UNITY_LIGHTMODEL_AMBIENT.rgb);

                return finalColor;
            }
            ENDCG
        }

        // Additional lights pass
        Pass
        {
            Name "ForwardAdd"
            Tags { "LightMode" = "ForwardAdd" }
            Blend One One
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 worldPos : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float4 vertexColor : COLOR;
                float4 localPos : TEXCOORD2;
            };

            fixed4 _BaseColor;
            float4 _CutPlanePoint;
            float4 _CutPlaneNormal;
            float _CutSmoothness;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.vertexColor = v.color;
                o.localPos = v.vertex;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // Signed distance to the cut plane in LOCAL SPACE
                float3 planeNormal = normalize(_CutPlaneNormal.xyz);
                float dist = dot(i.localPos.xyz - _CutPlanePoint.xyz, planeNormal);

                // Smoothstep-based clipping
                float cutFactor = smoothstep(-_CutSmoothness, _CutSmoothness, dist);
                clip(cutFactor - 0.5); // Discard fragments behind the plane

                // Lighting
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float ndotl = max(0.0, dot(i.normal, lightDir));
                float3 diffuseTerm = ndotl * _LightColor0.rgb;

                // Combine base color + vertex color + lighting
                fixed4 finalColor = _BaseColor * i.vertexColor;
                finalColor.rgb *= diffuseTerm;

                return finalColor;
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}