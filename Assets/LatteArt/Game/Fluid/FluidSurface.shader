Shader "LatteArt/FluidSurface"
{
    Properties
    {
        _CoffeeColor ("Coffee Color", Color) = (0.36, 0.2, 0.1, 1)
        _DilutedCoffeeColor ("Diluted Coffee Color", Color) = (0.55, 0.36, 0.22, 1)
        [HideInInspector] _CoffeeDilution ("Coffee Dilution", Float) = 0
        _MilkColor ("Milk Color", Color) = (0.96, 0.92, 0.84, 1)
        _EdgeSoftness ("Edge Softness", Range(0.01, 0.5)) = 0.1
        _HaloColor ("Halo Color", Color) = (0.62, 0.42, 0.25, 1)
        _HaloReach ("Halo Reach", Range(0.01, 0.5)) = 0.25
        _CremaScale ("Crema Scale", Range(1, 50)) = 12
        _CremaStrength ("Crema Strength", Range(0, 0.5)) = 0.12
        _CremaDrift ("Crema Drift", Range(0, 0.5)) = 0.03
        _CremaRimWidth ("Crema Rim Width", Range(0, 0.2)) = 0.06
        _CremaRimDarkness ("Crema Rim Darkness", Range(0, 1)) = 0.3
        _CremaBubbleScale ("Crema Bubble Scale", Range(20, 600)) = 160
        _CremaBubbleStrength ("Crema Bubble Strength", Range(0, 1)) = 0.25
        _CremaBubbleCoverage ("Crema Bubble Coverage", Range(0.05, 1)) = 0.5
        _CremaBubblePopRate ("Crema Bubble Pop Rate", Range(0, 2)) = 0.15
        _GrainScale ("Grain Scale", Range(50, 800)) = 300
        _GrainStrength ("Grain Edge Strength", Range(0, 0.5)) = 0.12
        _GrainCenterStrength ("Grain Center Strength", Range(0, 0.5)) = 0.02
        _GrainEdgeReach ("Grain Edge Reach (texels)", Range(1, 40)) = 12
        _GrainMaxStretch ("Grain Max Stretch", Range(1.1, 4)) = 1.8
        _BubbleWallSoftness ("Bubble Wall Softness", Range(0.02, 0.5)) = 0.15
        _FoamShadeStrength ("Foam Shade Strength", Range(0, 1)) = 0.25
        _FoamShadeSpread ("Foam Shade Spread (texels)", Range(0.5, 4)) = 1.5
        _LightDirection ("Light Direction", Vector) = (-0.6, 0.8, 0, 0)
        _RimWidth ("Rim Width", Range(0, 0.05)) = 0.01
        [NoScaleOffset] _DyeTex ("Dye", 2D) = "black" {}
        [NoScaleOffset] _FoamCoordsTex ("Foam Coords", 2D) = "black" {}
        [HideInInspector] _FoamPhase ("Foam Phase", Float) = 0
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
            TEXTURE2D(_FoamCoordsTex);
            SAMPLER(sampler_FoamCoordsTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _CoffeeColor;
                half4 _DilutedCoffeeColor;
                half _CoffeeDilution;
                half4 _MilkColor;
                half _EdgeSoftness;
                half4 _HaloColor;
                half _HaloReach;
                float _CremaScale;
                half _CremaStrength;
                float _CremaDrift;
                float _CremaRimWidth;
                half _CremaRimDarkness;
                float _CremaBubbleScale;
                half _CremaBubbleStrength;
                half _CremaBubbleCoverage;
                float _CremaBubblePopRate;
                float _GrainScale;
                half _GrainStrength;
                half _GrainCenterStrength;
                float _GrainEdgeReach;
                half _GrainMaxStretch;
                half _BubbleWallSoftness;
                float _FoamPhase;
                half _FoamShadeStrength;
                float _FoamShadeSpread;
                float4 _LightDirection;
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

            static const int CremaOctaves = 4;
            static const int EdgeSampleCount = 8;
            static const float2 SecondGrainOffset = float2(37.1, 91.7);
            static const float2 SmallBubbleOffset = float2(53.9, 17.3);
            static const float2 SecondHashOffset = float2(19.19, 7.31);
            static const float2 RadiusHashOffset = float2(71.7, 33.1);
            static const float2 LifeHashOffset = float2(12.9, 87.4);
            static const float MinFoamScale = 1e-4;
            static const float BubbleJitter = 0.9;
            static const float BubbleMinRadius = 0.18;
            static const float BubbleMaxRadius = 0.5;
            static const float BubbleHatchedSize = 0.7;
            static const float BubbleFadeLife = 0.08;
            static const float SmallBubbleScale = 2.3;
            static const float MilkBubbleCoverage = 0.85;

            struct CarriedFoam
            {
                float4 coords;
                float firstWeight;
                float secondWeight;
            };

            float Hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float2 Hash2(float2 p)
            {
                return float2(Hash(p), Hash(p + SecondHashOffset));
            }

            float ValueNoise(float2 p)
            {
                float2 cell = floor(p);
                float2 local = frac(p);
                float2 blend = local * local * (3 - 2 * local);

                float bottom = lerp(Hash(cell), Hash(cell + float2(1, 0)), blend.x);
                float top = lerp(Hash(cell + float2(0, 1)), Hash(cell + float2(1, 1)), blend.x);
                return lerp(bottom, top, blend.y);
            }

            float Fbm(float2 p)
            {
                float sum = 0;
                float amplitude = 0.5;
                float totalAmplitude = 0;
                for (int octave = 0; octave < CremaOctaves; octave++)
                {
                    sum += amplitude * ValueNoise(p);
                    totalAmplitude += amplitude;
                    p *= 2.03;
                    amplitude *= 0.5;
                }

                return sum / totalAmplitude;
            }

            float2 BubbleCells(float2 p, float coverage, float popRate)
            {
                float2 cell = floor(p);
                float2 local = frac(p);
                float2 rimAndFill = 0;

                [unroll]
                for (int y = -1; y <= 1; y++)
                {
                    [unroll]
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 bubbleCell = cell + float2(x, y);
                        float life = frac(Hash(bubbleCell + LifeHashOffset) + _Time.y * popRate);
                        float visibility = smoothstep(0, BubbleFadeLife, life) *
                            (1 - smoothstep(coverage - BubbleFadeLife, coverage, life));
                        float growth = lerp(BubbleHatchedSize, 1, saturate(life / coverage));
                        float radius = lerp(BubbleMinRadius, BubbleMaxRadius, Hash(bubbleCell + RadiusHashOffset)) * growth;

                        float2 bubbleCenter = float2(x, y) + 0.5 + (Hash2(bubbleCell) - 0.5) * BubbleJitter;
                        float distanceToCenter = length(bubbleCenter - local);
                        float rim = 1 - smoothstep(0, _BubbleWallSoftness, abs(distanceToCenter - radius));
                        float fill = 1 - smoothstep(radius - _BubbleWallSoftness, radius, distanceToCenter);
                        rimAndFill = max(rimAndFill, float2(rim, fill) * visibility);
                    }
                }

                return rimAndFill;
            }

            float2 BubbleField(float2 p, float coverage, float popRate)
            {
                return max(
                    BubbleCells(p, coverage, popRate),
                    BubbleCells(p * SmallBubbleScale + SmallBubbleOffset, coverage, popRate));
            }

            half SampleDye(float2 uv)
            {
                return SAMPLE_TEXTURE2D_LOD(_DyeTex, sampler_DyeTex, uv, 0).r;
            }

            float4 SampleFoamCoords(float2 uv)
            {
                return SAMPLE_TEXTURE2D_LOD(_FoamCoordsTex, sampler_FoamCoordsTex, uv, 0);
            }

            float StretchFade(float2 coordsChangeAlongX, float2 coordsChangeAlongY)
            {
                float a = coordsChangeAlongX.x;
                float b = coordsChangeAlongY.x;
                float c = coordsChangeAlongX.y;
                float d = coordsChangeAlongY.y;
                float scaleRotation = length(float2(a + d, c - b)) * 0.5;
                float shear = length(float2(a - d, c + b)) * 0.5;
                float stretch = (scaleRotation + shear) / max(abs(scaleRotation - shear), MinFoamScale);
                return 1 - saturate((stretch - 1) / (_GrainMaxStretch - 1));
            }

            CarriedFoam SampleCarriedFoam(float2 uv)
            {
                float width;
                float height;
                _FoamCoordsTex.GetDimensions(width, height);
                float2 texel = 1 / float2(width, height);

                float4 coordsChangeAlongX = (SampleFoamCoords(uv + float2(texel.x, 0)) -
                    SampleFoamCoords(uv - float2(texel.x, 0))) / (2 * texel.x);
                float4 coordsChangeAlongY = (SampleFoamCoords(uv + float2(0, texel.y)) -
                    SampleFoamCoords(uv - float2(0, texel.y))) / (2 * texel.y);

                float firstPhaseWeight = 1 - abs(2 * _FoamPhase - 1);
                float secondPhaseWeight = 1 - firstPhaseWeight;
                float normalization = rsqrt(firstPhaseWeight * firstPhaseWeight + secondPhaseWeight * secondPhaseWeight);

                CarriedFoam foam;
                foam.coords = SampleFoamCoords(uv);
                foam.firstWeight = firstPhaseWeight * normalization *
                    StretchFade(coordsChangeAlongX.xy, coordsChangeAlongY.xy);
                foam.secondWeight = secondPhaseWeight * normalization *
                    StretchFade(coordsChangeAlongX.zw, coordsChangeAlongY.zw);
                return foam;
            }

            float2 CarriedBubbles(CarriedFoam foam, float scale, float coverage, float popRate)
            {
                float2 first = BubbleField(foam.coords.xy * scale, coverage, popRate);
                float2 second = BubbleField(foam.coords.zw * scale + SecondGrainOffset, coverage, popRate);
                return saturate(first * foam.firstWeight + second * foam.secondWeight);
            }

            half MilkEdgeness(float2 uv)
            {
                float width;
                float height;
                _DyeTex.GetDimensions(width, height);
                float2 reach = _GrainEdgeReach / float2(width, height);

                float surroundingDye = 0;
                [unroll]
                for (int i = 0; i < EdgeSampleCount; i++)
                {
                    float angle = i * TWO_PI / EdgeSampleCount;
                    surroundingDye += SampleDye(uv + reach * float2(cos(angle), sin(angle)));
                }

                return saturate(2 * (1 - surroundingDye / EdgeSampleCount));
            }

            half3 CremaColor(float2 uv, float distanceFromCenter, CarriedFoam foam)
            {
                float2 position = uv * _CremaScale;
                float drift = _Time.y * _CremaDrift * _CremaScale;
                float mottling = 0.5 * (
                    Fbm(position + drift * float2(1, 0.3)) +
                    Fbm(position * 1.3 - drift * float2(0.4, 1)));
                half3 coffee = lerp(_CoffeeColor.rgb, _DilutedCoffeeColor.rgb, _CoffeeDilution);
                half3 crema = coffee * (1 + (mottling - 0.5) * 2 * _CremaStrength);

                float bubbles = CarriedBubbles(foam, _CremaBubbleScale, _CremaBubbleCoverage, _CremaBubblePopRate).y;
                crema *= 1 + bubbles * _CremaBubbleStrength;

                half rimShadow = smoothstep(_CupRadius - _CremaRimWidth, _CupRadius, distanceFromCenter);
                return crema * (1 - rimShadow * _CremaRimDarkness);
            }

            half3 MicrofoamColor(float2 uv, CarriedFoam foam)
            {
                float bubbleWalls = CarriedBubbles(foam, _GrainScale, MilkBubbleCoverage, 0).x;
                half grainStrength = lerp(_GrainCenterStrength, _GrainStrength, MilkEdgeness(uv));
                return _MilkColor.rgb * (1 - bubbleWalls * grainStrength);
            }

            half3 SurfaceColor(float2 uv, float distanceFromCenter, half dye)
            {
                CarriedFoam foam = SampleCarriedFoam(uv);
                half halo = smoothstep(0, _HaloReach, dye);
                half milk = smoothstep(0.5 - _EdgeSoftness, 0.5 + _EdgeSoftness, dye);

                half3 coffee = 0;
                [branch]
                if (milk < 1) coffee = lerp(CremaColor(uv, distanceFromCenter, foam), _HaloColor.rgb, halo);

                half3 microfoam = 0;
                [branch]
                if (milk > 0) microfoam = MicrofoamColor(uv, foam);

                return lerp(coffee, microfoam, milk);
            }

            half FoamLight(float2 uv)
            {
                float width;
                float height;
                _DyeTex.GetDimensions(width, height);
                float2 offset = _FoamShadeSpread / float2(width, height);

                float2 foamSlope = float2(
                    SampleDye(uv + float2(offset.x, 0)) - SampleDye(uv - float2(offset.x, 0)),
                    SampleDye(uv + float2(0, offset.y)) - SampleDye(uv - float2(0, offset.y)));
                float2 towardLight = normalize(_LightDirection.xy);
                return -dot(foamSlope, towardLight) * _FoamShadeStrength;
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

                half rimClearance = 1 - smoothstep(_CupRadius - _RimWidth - edgeWidth, _CupRadius, distanceFromCenter);
                half dye = SampleDye(input.uv) * rimClearance;
                half3 color = saturate(SurfaceColor(input.uv, distanceFromCenter, dye) * (1 + FoamLight(input.uv) * rimClearance));
                color = lerp(color, _BrushColor.rgb, BrushRing(input.uv) * _BrushColor.a);

                half alpha = 1 - smoothstep(_CupRadius - edgeWidth, _CupRadius + edgeWidth, distanceFromCenter);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}
