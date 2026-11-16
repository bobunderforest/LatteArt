Shader "LatteArt/VelvetCurtain"
{
    Properties
    {
        [Header(Velvet)]
        _BaseColor ("Base Color", Color) = (0.45, 0.02, 0.03, 1)
        _SheenColor ("Sheen Color", Color) = (1, 0.3, 0.3, 1)
        _SheenPower ("Sheen Power", Range(0.5, 8)) = 3
        _Smoothness ("Smoothness", Range(0, 1)) = 0.2

        [Header(Folds)]
        _FoldsPerMeter ("Folds Per Meter", Range(0.5, 10)) = 3
        _FoldDepth ("Fold Depth", Range(0, 0.3)) = 0.06
        _FoldShadow ("Fold Shadow", Range(0, 1)) = 0.6

        [Header(Sway)]
        _SwayAmount ("Sway Amount", Range(0, 1)) = 0.15
        _SwaySpeed ("Sway Speed", Range(0, 5)) = 0.6
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
        float4 _SheenColor;
        float _SheenPower;
        float _Smoothness;
        float _FoldsPerMeter;
        float _FoldDepth;
        float _FoldShadow;
        float _SwayAmount;
        float _SwaySpeed;
        CBUFFER_END

        struct CurtainVertex
        {
            float3 positionOS;
            float3 normalOS;
            float wave;
        };

        CurtainVertex FoldCurtain(float3 positionOS, float3 normalOS, float4 tangentOS, float2 uv)
        {
            float3 tangent = normalize(tangentOS.xyz);
            float across = dot(positionOS, tangent);
            float hangFactor = 1.0 - saturate(uv.y);
            float sway = sin(_Time.y * _SwaySpeed + across * 0.7) * _SwayAmount * hangFactor;

            float frequency = TWO_PI * _FoldsPerMeter;
            float phase = across * frequency + sway * TWO_PI;
            float wave = sin(phase);
            float slope = cos(phase) * frequency * _FoldDepth;

            CurtainVertex result;
            result.positionOS = positionOS + normalOS * wave * _FoldDepth;
            result.normalOS = normalize(normalOS - tangent * slope);
            result.wave = wave;
            return result;
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float4 shadowCoord : TEXCOORD2;
                float fogFactor : TEXCOORD3;
                float wave : TEXCOORD4;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                CurtainVertex curtain = FoldCurtain(input.positionOS.xyz, input.normalOS, input.tangentOS, input.uv);
                VertexPositionInputs positionInputs = GetVertexPositionInputs(curtain.positionOS);
                output.positionHCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = TransformObjectToWorldNormal(curtain.normalOS);
                output.shadowCoord = GetShadowCoord(positionInputs);
                output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
                output.wave = curtain.wave;
                return output;
            }

            InputData CreateInputData(Varyings input)
            {
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.positionCS = input.positionHCS;
                inputData.normalWS = NormalizeNormalPerPixel(input.normalWS);
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                #if defined(_MAIN_LIGHT_SHADOWS_CASCADE)
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                #else
                inputData.shadowCoord = input.shadowCoord;
                #endif
                inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactor);
                inputData.bakedGI = SampleSH(inputData.normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionHCS);
                inputData.shadowMask = half4(1, 1, 1, 1);
                return inputData;
            }

            half3 VelvetSheen(InputData inputData)
            {
                Light mainLight = GetMainLight(inputData.shadowCoord);
                half3 incomingLight = inputData.bakedGI
                    + mainLight.color * mainLight.distanceAttenuation * mainLight.shadowAttenuation;
                half grazing = 1.0 - saturate(dot(inputData.normalWS, inputData.viewDirectionWS));
                return _SheenColor.rgb * pow(grazing, _SheenPower) * incomingLight;
            }

            SurfaceData CreateSurfaceData(InputData inputData, float wave)
            {
                half foldLight = lerp(1.0 - _FoldShadow, 1.0, wave * 0.5 + 0.5);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = _BaseColor.rgb * foldLight;
                surfaceData.smoothness = _Smoothness;
                surfaceData.normalTS = half3(0, 0, 1);
                surfaceData.emission = VelvetSheen(inputData) * foldLight;
                surfaceData.occlusion = foldLight;
                surfaceData.alpha = 1;
                return surfaceData;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                InputData inputData = CreateInputData(input);
                half4 color = UniversalFragmentPBR(inputData, CreateSurfaceData(inputData, input.wave));
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                return color;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings Vertex(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);

                CurtainVertex curtain = FoldCurtain(input.positionOS.xyz, input.normalOS, input.tangentOS, input.uv);
                float3 positionWS = TransformObjectToWorld(curtain.positionOS);
                float3 normalWS = TransformObjectToWorldNormal(curtain.normalOS);
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                float3 lightDirectionWS = _LightDirection;
                #endif

                Varyings output;
                output.positionHCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
                output.positionHCS = ApplyShadowClamping(output.positionHCS);
                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ColorMask R

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                CurtainVertex curtain = FoldCurtain(input.positionOS.xyz, input.normalOS, input.tangentOS, input.uv);
                output.positionHCS = TransformObjectToHClip(curtain.positionOS);
                return output;
            }

            half Fragment(Varyings input) : SV_Target
            {
                return input.positionHCS.z;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                CurtainVertex curtain = FoldCurtain(input.positionOS.xyz, input.normalOS, input.tangentOS, input.uv);
                output.positionHCS = TransformObjectToHClip(curtain.positionOS);
                output.normalWS = TransformObjectToWorldNormal(curtain.normalOS);
                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                return half4(NormalizeNormalPerPixel(input.normalWS), 0);
            }
            ENDHLSL
        }
    }
}
