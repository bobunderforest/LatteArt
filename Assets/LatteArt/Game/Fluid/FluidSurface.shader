Shader "LatteArt/FluidSurface"
{
    Properties
    {
        _CoffeeColor ("Coffee Color", Color) = (0.36, 0.2, 0.1, 1)
        _MilkColor ("Milk Color", Color) = (0.96, 0.92, 0.84, 1)
        _EdgeSoftness ("Edge Softness", Range(0.01, 0.5)) = 0.1
        _RimWidth ("Rim Width", Range(0, 0.05)) = 0.01
        [NoScaleOffset] _DyeTex ("Dye", 2D) = "black" {}
        _CupRadius ("Cup Radius", Range(0.1, 0.5)) = 0.48
        _BrushColor ("Brush Color", Color) = (1, 1, 1, 0.6)
        _BrushWidth ("Brush Width (px)", Range(0.5, 4)) = 1.5
        [HideInInspector] _BrushCenter ("Brush Center", Vector) = (0.5, 0.5, 0, 0)
        [HideInInspector] _BrushRadius ("Brush Radius", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "Unlit"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_DyeTex);
            SAMPLER(sampler_DyeTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _CoffeeColor;
                half4 _MilkColor;
                half _EdgeSoftness;
                float _RimWidth;
                float _CupRadius;
                half4 _BrushColor;
                float _BrushWidth;
                float4 _BrushCenter;
                float _BrushRadius;
            CBUFFER_END

            half BrushRing(float2 uv)
            {
                if (_BrushRadius <= 0) return 0;

                float distanceFromCenter = distance(uv, _BrushCenter.xy);
                float pixelSize = fwidth(distanceFromCenter);
                float distanceFromRing = abs(distanceFromCenter - _BrushRadius);
                return 1 - smoothstep(0, _BrushWidth * pixelSize, distanceFromRing);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float distanceFromCenter = distance(input.uv, 0.5);
                float edgeWidth = fwidth(distanceFromCenter);

                half dye = SAMPLE_TEXTURE2D(_DyeTex, sampler_DyeTex, input.uv).r;
                half milk = smoothstep(0.5 - _EdgeSoftness, 0.5 + _EdgeSoftness, dye);
                half rimClearance = 1 - smoothstep(_CupRadius - _RimWidth - edgeWidth, _CupRadius, distanceFromCenter);
                half3 color = lerp(_CoffeeColor.rgb, _MilkColor.rgb, milk * rimClearance);
                color = lerp(color, _BrushColor.rgb, BrushRing(input.uv) * _BrushColor.a);

                half alpha = 1 - smoothstep(_CupRadius - edgeWidth, _CupRadius + edgeWidth, distanceFromCenter);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}
