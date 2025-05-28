Shader "URP/Unlit/BarycentricWireframe"
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
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 color : COLOR; // RGB = barycentric coords
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 bary : COLOR;
            };

            float _LineWidth;
            float4 _LineColor;
            float4 _FillColor;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS);
                output.bary = input.color.rgb;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float minBary = min(input.bary.x, min(input.bary.y, input.bary.z));
                float edge = smoothstep(0.0, _LineWidth, minBary);
                return lerp(_LineColor, _FillColor, edge);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}