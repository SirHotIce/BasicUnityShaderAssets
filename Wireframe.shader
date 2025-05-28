Shader "Custom/Wireframe/DualSided"
{
    Properties
    {
        _Color("Wire Color", Color) = (0, 1, 0, 1)
        _Thickness("Thickness", Range(0.001, 1.0)) = 0.015
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalRenderPipeline" "Queue" = "Transparent" "RenderType" = "Transparent" }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass
        {
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #include "WireframeShared.hlsl"

            float4 frag(Varyings i, bool isFront : SV_IsFrontFace) : SV_Target
            {
                return fragColor(i, _Color);
            }


            ENDHLSL
        }

       
    }
}

