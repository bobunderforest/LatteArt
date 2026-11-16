Shader "LatteArt/FluidSky"
{
    Properties
    {
        [Header(Palette)]
        _CoffeeColor ("Coffee Color", Color) = (0.06, 0.03, 0.02, 1)
        _HaloColor ("Halo Color", Color) = (0.36, 0.19, 0.1, 1)
        _MilkColor ("Milk Color", Color) = (0.95, 0.9, 0.82, 1)

        [Header(Swirls)]
        _VortexStrength ("Vortex Strength", Range(0, 12)) = 5
        _VortexRadius ("Vortex Radius", Range(0.02, 1)) = 0.25
        _VortexPulse ("Vortex Pulse", Range(0, 1)) = 0.3

        [Header(Flow)]
        _FlowScale ("Flow Scale", Range(0.2, 6)) = 1.4
        _FlowStrength ("Flow Strength", Range(0, 3)) = 0.8
        _FoldStrength ("Fold Strength", Range(0, 2)) = 0.6
        _FlowSpeed ("Flow Speed", Range(0, 0.5)) = 0.05

        [Header(Milk Areas)]
        _RegionScale ("Region Scale", Range(0.2, 6)) = 1.3
        _MilkCoverage ("Milk Coverage", Range(-0.5, 0.5)) = -0.05
        _LineFrequency ("Line Frequency", Range(1, 20)) = 6
        _LineInfluence ("Line Influence", Range(0, 1)) = 0.35
        _EdgeSoftness ("Edge Softness", Range(0, 0.2)) = 0.02
        _HaloWidth ("Halo Width", Range(0.01, 0.5)) = 0.12

        [Header(Crema)]
        _CremaScale ("Crema Scale", Range(1, 40)) = 10
        _CremaStrength ("Crema Strength", Range(0, 0.5)) = 0.15

        [Header(Stars)]
        _StarColor ("Star Color", Color) = (1, 0.95, 0.85, 1)
        _StarScale ("Star Scale", Range(10, 300)) = 80
        _StarDensity ("Star Density", Range(0, 0.2)) = 0.03
        _StarSize ("Star Size", Range(0.05, 0.5)) = 0.25
        _StarTwinkleSpeed ("Star Twinkle Speed", Range(0, 4)) = 1.5
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Background"
            "Queue" = "Background"
            "RenderPipeline" = "UniversalPipeline"
            "PreviewType" = "Skybox"
        }

        Cull Off
        ZWrite Off

        Pass
        {
            Name "FluidSky"

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            static const int VortexCount = 6;
            static const float3 VortexSeed = float3(7.13, 3.31, 1.79);
            static const float VortexWander = 0.35;
            static const int FlowOctaves = 3;
            static const int CremaOctaves = 3;
            static const float FoldFrequency = 1.7;
            static const float3 FlowDrift = float3(0.21, 0.67, 0.43);
            static const float3 FoldOffset = float3(13.7, 3.1, 29.3);

            CBUFFER_START(UnityPerMaterial)
                half4 _CoffeeColor;
                half4 _HaloColor;
                half4 _MilkColor;
                float _VortexStrength;
                float _VortexRadius;
                float _VortexPulse;
                float _FlowScale;
                float _FlowStrength;
                float _FoldStrength;
                float _FlowSpeed;
                float _RegionScale;
                half _MilkCoverage;
                float _LineFrequency;
                half _LineInfluence;
                half _EdgeSoftness;
                half _HaloWidth;
                float _CremaScale;
                half _CremaStrength;
                half4 _StarColor;
                float _StarScale;
                float _StarDensity;
                float _StarSize;
                float _StarTwinkleSpeed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 directionOS : TEXCOORD0;
            };

            struct FlowSample
            {
                float3 position;
                float region;
            };

            struct MilkMask
            {
                half milk;
                half halo;
            };

            float3 Hash33(float3 p)
            {
                p = frac(p * float3(0.1031, 0.1030, 0.0973));
                p += dot(p, p.yxz + 33.33);
                return frac((p.xxy + p.yxx) * p.zyx);
            }

            float3 VectorNoise(float3 p)
            {
                float3 cell = floor(p);
                float3 local = frac(p);
                float3 blend = local * local * (3 - 2 * local);

                float3 x00 = lerp(Hash33(cell), Hash33(cell + float3(1, 0, 0)), blend.x);
                float3 x10 = lerp(Hash33(cell + float3(0, 1, 0)), Hash33(cell + float3(1, 1, 0)), blend.x);
                float3 x01 = lerp(Hash33(cell + float3(0, 0, 1)), Hash33(cell + float3(1, 0, 1)), blend.x);
                float3 x11 = lerp(Hash33(cell + float3(0, 1, 1)), Hash33(cell + float3(1, 1, 1)), blend.x);

                return lerp(lerp(x00, x10, blend.y), lerp(x01, x11, blend.y), blend.z);
            }

            float3 VectorFbm(float3 p, int octaves)
            {
                float3 sum = 0;
                float amplitude = 0.5;
                float totalAmplitude = 0;
                for (int octave = 0; octave < octaves; octave++)
                {
                    sum += amplitude * VectorNoise(p);
                    totalAmplitude += amplitude;
                    p = p * 2.03 + 1.7;
                    amplitude *= 0.5;
                }

                return sum / totalAmplitude;
            }

            float3 RotateAround(float3 p, float3 axis, float angle)
            {
                float sine = sin(angle);
                float cosine = cos(angle);
                return p * cosine + cross(axis, p) * sine + axis * dot(axis, p) * (1 - cosine);
            }

            float3 VortexAxis(int index, float time)
            {
                float3 seed = Hash33(VortexSeed * (index + 1));
                float3 wander = sin(time * (0.5 + seed.zxy) + seed * TWO_PI) * VortexWander;
                return normalize(seed * 2 - 1 + wander);
            }

            float3 Twist(float3 direction, float time)
            {
                float3 twisted = direction;
                for (int i = 0; i < VortexCount; i++)
                {
                    float3 axis = VortexAxis(i, time);
                    float distanceFromCore = 1 - dot(twisted, axis);
                    float influence = exp(-distanceFromCore / _VortexRadius);
                    float spinDirection = (i & 1) == 0 ? 1 : -1;
                    float pulse = 1 + _VortexPulse * sin(time * 3 + i * 1.7);
                    twisted = RotateAround(twisted, axis, influence * _VortexStrength * pulse * spinDirection);
                }

                return twisted;
            }

            FlowSample FlowAt(float3 direction, float time)
            {
                direction = Twist(direction, time);

                float3 drift = FlowDrift * time;
                float3 first = VectorFbm(direction * _FlowScale + drift, FlowOctaves);
                float3 folded = direction + (first - 0.5) * _FlowStrength;
                float3 second = VectorFbm(folded * _FlowScale * FoldFrequency - drift + FoldOffset, FlowOctaves);

                FlowSample flow;
                flow.position = folded + (second - 0.5) * _FlowStrength * _FoldStrength;
                flow.region = VectorFbm(flow.position * _RegionScale + FoldOffset, FlowOctaves).y;
                return flow;
            }

            MilkMask MilkAt(FlowSample flow)
            {
                half stripe = 0.5 + 0.5 * cos(flow.position.y * _LineFrequency * TWO_PI);
                half dye = flow.region + _MilkCoverage + (stripe - 0.5) * _LineInfluence;
                half softness = max(_EdgeSoftness, fwidth(dye));

                MilkMask mask;
                mask.milk = smoothstep(0.5 - softness, 0.5 + softness, dye);
                mask.halo = smoothstep(0.5 - _HaloWidth, 0.5, dye) * (1 - mask.milk);
                return mask;
            }

            half3 StarLight(float3 direction)
            {
                float3 p = direction * _StarScale;
                float3 cell = floor(p);
                float3 local = frac(p) - 0.5;

                float seed = Hash33(cell).x;
                float isStar = step(1 - _StarDensity, seed);
                float glow = smoothstep(_StarSize, 0, length(local)) * isStar;
                float twinkle = 0.65 + 0.35 * sin(_Time.y * _StarTwinkleSpeed + seed * 32);

                return _StarColor.rgb * glow * twinkle;
            }

            half3 CremaColor(float3 position)
            {
                float mottling = VectorFbm(position * _CremaScale, CremaOctaves).x;
                return _CoffeeColor.rgb * (1 + (mottling - 0.5) * 2 * _CremaStrength);
            }

            half3 SkyColor(float3 direction)
            {
                FlowSample flow = FlowAt(direction, _Time.y * _FlowSpeed);
                MilkMask mask = MilkAt(flow);

                half3 coffee = 0;
                [branch]
                if (mask.milk < 1)
                {
                    coffee = lerp(CremaColor(flow.position), _HaloColor.rgb, mask.halo);
                    coffee += StarLight(direction) * (1 - mask.halo);
                }

                return lerp(coffee, _MilkColor.rgb, mask.milk);
            }

            Varyings Vertex(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.directionOS = input.positionOS.xyz;
                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                return half4(SkyColor(normalize(input.directionOS)), 1);
            }
            ENDHLSL
        }
    }
}
