Shader "Custom/ShaderToyBitmapWaterDistortion"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _iTime ("ShaderToy Time", Float) = 0
        _iResolution ("ShaderToy Resolution", Vector) = (1920, 1080, 0, 0)
        _iMouse ("ShaderToy Mouse", Vector) = (0, 0, 0, 0)
        _DistortionStrength ("Distortion Strength", Range(0, 0.05)) = 0.015
        _LightStrength ("Light Strength", Range(0, 1)) = 0.35
        _FlowSpeed ("Flow Speed", Range(0, 1)) = 0.18
        _TextureTiling ("Texture Tiling", Float) = 1
        _NormalStrength ("Normal Strength", Range(0, 1)) = 0.35
        _WaveScale ("Wave Scale", Float) = 1.5
        _RippleStrength ("Ripple Strength", Range(0, 2)) = 1
        _DebugRipples ("Debug Ripples", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "ShaderToyBitmapWaterDistortionUI"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                float _iTime;
                float4 _iResolution;
                float4 _iMouse;
                float _DistortionStrength;
                float _LightStrength;
                float _FlowSpeed;
                float _TextureTiling;
                float _NormalStrength;
                float _WaveScale;
                float _RippleStrength;
                float _DebugRipples;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                return output;
            }

            float Hash(float2 p)
            {
                float h = dot(p, float2(17.1, 311.7));
                return -1.0 + 2.0 * frac(sin(h) * 4358.5453);
            }

            float Noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);

                float a = lerp(Hash(i + float2(0.0, 0.0)), Hash(i + float2(1.0, 0.0)), u.x);
                float b = lerp(Hash(i + float2(0.0, 1.0)), Hash(i + float2(1.0, 1.0)), u.x);
                return lerp(a, b, u.y);
            }

            float2 VecNoise(float2 samplePoint)
            {
                return float2(Noise(samplePoint), Noise(samplePoint + 0.33));
            }

            float DistortionNoise(float2 samplePoint, float distortion)
            {
                float2 offset = VecNoise(samplePoint) * distortion;
                return Noise(samplePoint + offset);
            }

            float DistFbmNoise(float2 p)
            {
                const int octaves = 8;
                const float lacunarity = 2.0;
                const float H = 0.5;

                float value = 0.0;
                [unroll]
                for (int i = 0; i < octaves; i++)
                {
                    value += DistortionNoise(p, 1.0) * pow(lacunarity, -H * i);
                    p *= lacunarity;
                }

                return value;
            }

            float FbmNoise(float2 p)
            {
                const int octaves = 4;
                const float lacunarity = 2.0;
                const float H = 0.8;

                float value = 0.0;
                [unroll]
                for (int i = 0; i < octaves; i++)
                {
                    value += Noise(p) * pow(lacunarity, -H * i);
                    p *= lacunarity;
                }

                return value;
            }

            float Offset(float3 pos)
            {
                return FbmNoise(pos.xz);
            }

            float DistOffset(float3 pos)
            {
                return 0.4 * DistFbmNoise(pos.xz);
            }

            float2 DistOffset2D(float3 pos, float time)
            {
                float2 samplePoint = pos.xz * max(_WaveScale, 0.0001);
                float2 timeOffset = float2(time, time * 0.73);
                float x = DistFbmNoise(samplePoint + timeOffset);
                float y = DistFbmNoise(samplePoint + float2(11.7, -4.3) + timeOffset.yx);
                return float2(x, y) * _RippleStrength;
            }

            float3 CalcNormal(float3 pos)
            {
                float3 normal = normalize(cross(ddy(pos), ddx(pos)));
                return normalize(lerp(float3(0.0, 1.0, 0.0), normal, saturate(_NormalStrength)));
            }

            float4 MainImage(float2 fragCoord)
            {
                float2 resolution = max(_iResolution.xy, float2(1.0, 1.0));
                float2 st = fragCoord / resolution;
                float2 uv = fragCoord / resolution.y;
                float textureTiling = max(_TextureTiling, 0.0001);

                float3 normal = float3(0.0, 1.0, 0.0);
                float3 pos = float3(uv.x, 0.0, uv.y);

                float flowTime = _iTime * _FlowSpeed;
                float offset = Offset(pos + flowTime);
                float2 rippleOffset = DistOffset2D(pos, flowTime);

                pos += 0.4 * offset * normal;
                normal = CalcNormal(pos);

                const float3 lightPosition = float3(1.0, 1.0, -0.4);
                float3 lightDirection = normalize(lightPosition - pos);
                float diffuseMask = saturate(dot(normal, lightDirection));
                diffuseMask = smoothstep(0.35, 1.0, diffuseMask);
                float3 diffuse = diffuseMask * float3(0.9, 0.9, 0.6);

                float2 distortedUv = st * textureTiling + rippleOffset * _DistortionStrength;
                float4 bitmap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, distortedUv);
                float3 colorRgb = bitmap.rgb + _LightStrength * diffuse;
                colorRgb = min(colorRgb, 1.0);
                if (_DebugRipples > 0.5)
                {
                    float debugValue = saturate(0.5 + 0.5 * (rippleOffset.x + rippleOffset.y));
                    colorRgb = debugValue.xxx;
                }
                float4 color = float4(colorRgb, 1.0);
                color.a = 1.0;
                return color;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 resolution = max(_iResolution.xy, float2(1.0, 1.0));
                float2 fragCoord = input.uv * resolution;
                return half4(MainImage(fragCoord)) * input.color;
            }
            ENDHLSL
        }
    }
}
