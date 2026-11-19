// Soft additive glow quad (camera-facing or backdrop) with a soft round texture; used as a pool of light behind the turntable model.
// Universal render pipeline.
Shader "KMaxLauncher/SoftGlow" {
    Properties {
        _MainTex ("Soft dot", 2D) = "white" {}
        _Color ("Colour", Color) = (1, 1, 1, 1)
    }
    SubShader {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent+20" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Blend SrcAlpha One
        ZWrite Off
        ZTest LEqual
        Cull Off

        Pass {
            Name "Halo"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
            CBUFFER_END

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input) {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 frag(Varyings input) : SV_Target {
                half4 dot = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                return half4(_Color.rgb, dot.a * _Color.a);
            }
            ENDHLSL
        }
    }
}
