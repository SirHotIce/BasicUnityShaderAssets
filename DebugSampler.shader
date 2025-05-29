Shader "Hidden/DebugDepth"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off

            HLSLINCLUDE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings  { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.uv = (v.positionOS.xy + 1.0) * 0.5;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float rawDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv).r;
                float lin = LinearEyeDepth(rawDepth, _ZBufferParams);
                
                return float4(1,0,0,1); // swizzle to get Grayscale 
            }
            
            ENDHLSL

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }
}
