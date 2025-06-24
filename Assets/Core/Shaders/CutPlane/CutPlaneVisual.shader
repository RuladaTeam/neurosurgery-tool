Shader "Custom/CutPlaneVisual"
{
    Properties
    {
        _EdgeColor ("Edge Color", Color) = (1,1,1,1)
        _EdgeWidth ("Edge Width", Range(0.001, 0.5)) = 0.05
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {


            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _EdgeColor;
            float _EdgeWidth;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Flip the distance so center has highest value, edges lowest
                float2 clampedUV = saturate(i.uv); // Clamp UVs to [0..1]
                float distX = min(clampedUV.x, 1.0 - clampedUV.x);
                float distY = min(clampedUV.y, 1.0 - clampedUV.y);
                float distToEdge = min(distX, distY);

                // We want to keep only where distToEdge < _EdgeWidth
                // So we make alpha = 1 if inside edge, 0 otherwise
                float edgeMask = step(distToEdge, _EdgeWidth);

                // Return color only on edge, transparent elsewhere
                return fixed4(_EdgeColor.rgb, edgeMask * _EdgeColor.a);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}