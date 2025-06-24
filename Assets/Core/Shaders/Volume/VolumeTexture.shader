Shader "Unlit/VolumeTexture"
{
    Properties
    {
        _VolumeTex ("Volume Texture", 3D) = "black" {}
        _DataMin ("Data Minimum", Float) = -1000
        _DataMax ("Data Maximum", Float) = 4000
    }

    SubShader
    {
        Tags { "Queue"="Geometry" }
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler3D _VolumeTex;
            float _DataMin;
            float _DataMax;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 uv: TEXCOORD1;
            };

            v2f vert (appdata_base v)
            {
                v2f o;
                o.uv = v.vertex.xyz * 0.5 + 0.5; // [-1,1] → [0,1]
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                /*
                float3 viewDir = normalize(i.worldPos - _WorldSpaceCameraPos);
                float stepSize = 0.01;
                float4 color = float4(0,0,0,0);

                // Simple raymarching loop
                for (int j = 0; j < 50; j++)
                {
                    float3 samplePos = i.worldPos + viewDir * j * stepSize;
                    samplePos = saturate(samplePos);
                    float val = tex3D(_VolumeTex, samplePos).r;
                    val = saturate((val - _DataMin) / (_DataMax - _DataMin));
                    color += float4(val, val, val, 0.05);
                    if (color.a > 1.0) break;
                }

                return color;
                */
                float val = tex3D(_VolumeTex, i.uv).r;
                return fixed4(val, val, val, 1);
            }
            ENDCG
        }
    }
}