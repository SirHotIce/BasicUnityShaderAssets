Shader "Custom/URP/Wireframe"
{
    Properties
    {
        _Color ("Wire Color", Color) = (0, 1, 0, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }

        Pass
        {
            Name "WireframePass"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _Color;

            struct appdata
            {
                float3 positionOS : POSITION;
            };

            struct v2g
            {
                float4 positionWS : POSITION;
            };

            struct g2f
            {
                float4 positionCS : SV_POSITION;
            };

            v2g vert(appdata IN)
            {
                v2g OUT;
                OUT.positionWS = float4(TransformObjectToWorld(IN.positionOS), 1.0);
                return OUT;
            }

            [maxvertexcount(6)]
            void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream)
            {
                float4 p0 = TransformWorldToHClip(input[0].positionWS.xyz);
                float4 p1 = TransformWorldToHClip(input[1].positionWS.xyz);
                float4 p2 = TransformWorldToHClip(input[2].positionWS.xyz);

                g2f o;

                // Line 1
                o.positionCS = p0; triStream.Append(o);
                o.positionCS = p1; triStream.Append(o);
                triStream.RestartStrip();

                // Line 2
                o.positionCS = p1; triStream.Append(o);
                o.positionCS = p2; triStream.Append(o);
                triStream.RestartStrip();

                // Line 3
                o.positionCS = p2; triStream.Append(o);
                o.positionCS = p0; triStream.Append(o);
                triStream.RestartStrip();
            }

            float4 frag(g2f IN) : SV_Target
            {
                return _Color;
            }

            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}