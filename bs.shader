Shader "Unlit/BarycentricWireframe"
{
    Properties
    {
        _LineColor ("Line Color", Color) = (0, 0, 0, 1)
        _FillColor ("Fill Color", Color) = (1, 1, 1, 1)
        _LineWidth ("Line Width", Range(0.001, 0.1)) = 0.02
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR; // Barycentric stored in vertex color
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 bary : COLOR;
            };

            float _LineWidth;
            float4 _LineColor;
            float4 _FillColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.bary = v.color.rgb;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float minBary = min(i.bary.x, min(i.bary.y, i.bary.z));
                float edge = smoothstep(0.0, _LineWidth, minBary);
                return lerp(_LineColor, _FillColor, edge);
            }
            ENDCG
        }
    }
}