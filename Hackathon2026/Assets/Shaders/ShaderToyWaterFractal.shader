Shader "Custom/ShaderToyWaterFractal"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _iTime ("ShaderToy Time", Float) = 0
        _iResolution ("ShaderToy Resolution", Vector) = (1920, 1080, 0, 0)
        _iMouse ("ShaderToy Mouse", Vector) = (0, 0, 0, 0)
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
            Name "ShaderToyWaterFractalUI"

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
            CBUFFER_END

            static const float3 WaterColor = float3(0.3, 0.7, 1.0);
            static const float MaxDistance = 5.0;
            static const float SurfaceDistance = 0.001;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                return output;
            }

            float2x2 Rotate2D(float degrees)
            {
                float angle = radians(degrees);
                float s = sin(angle);
                float c = cos(angle);
                return float2x2(c, s, -s, c);
            }

            float3x3 LookAt(float3 forward, float3 up)
            {
                forward = normalize(forward);
                float3 right = normalize(cross(forward, normalize(up)));
                return float3x3(right, cross(right, forward), forward);
            }

            float Fractal(float3 p, float time)
            {
                p += cos(p.z * 3.0 + time * 4.0) * 0.02;
                float depth = smoothstep(0.0, 6.0, -p.z + 5.0);
                p *= 0.3;
                p = abs(2.0 - fmod(p + float3(0.4, 0.7, time * 0.07), 4.0));

                float lastLength = 0.0;
                float change = 0.0;
                [unroll]
                for (int i = 0; i < 6; i++)
                {
                    p = abs(p) / min(dot(p, p), 1.0) - 0.9;
                    float currentLength = length(p);
                    change += abs(currentLength - lastLength);
                    lastLength = currentLength;
                }

                return 0.15 + smoothstep(0.0, 50.0, change) * depth * 4.0;
            }

            float3 March(float3 origin, float3 direction, float3 lightDirection, float time)
            {
                float3 startDirection = direction;
                float3 p = origin + direction * 2.0;
                float foreground = Fractal(p + direction, time) * 0.55;
                float3 color = 0.0;
                float totalDistance = 0.0;
                float volume = 0.0;
                float reflectance = 1.0;

                // The ShaderToy source had an uninitialized distance and no loop.
                // This treats the fractal field as a soft volume/surface and accumulates glow while marching.
                [loop]
                for (int stepIndex = 0; stepIndex < 72; stepIndex++)
                {
                    p = origin + direction * totalDistance;
                    float field = Fractal(p, time);
                    float distanceToField = max(0.015, field * 0.085);
                    float density = smoothstep(0.65, 3.0, field);
                    float fade = smoothstep(MaxDistance * 0.2, MaxDistance * 0.9, MaxDistance - totalDistance);

                    volume += density * fade;
                    color += WaterColor * density * fade * 0.026;

                    if (distanceToField < SurfaceDistance * 2.0)
                    {
                        color = lerp(WaterColor * 0.15, color, fade);
                        reflectance *= 0.85;
                        break;
                    }

                    totalDistance += distanceToField;
                    if (totalDistance > MaxDistance)
                    {
                        break;
                    }
                }

                color *= normalize(WaterColor + 1.5) * 1.7;

                p = MaxDistance * startDirection;
                float3 background = Fractal(p, time) * reflectance * WaterColor;
                float glow = pow(max(0.0, dot(startDirection, -lightDirection)), 1.5);
                float3 glowWater = normalize(WaterColor + 1.0);
                background += glowWater * (glow + pow(glow, 8.0) * 1.5) * reflectance;
                color += volume * 0.06 * glow * reflectance * glowWater;
                color += background + foreground * WaterColor;

                return color;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 resolution = max(_iResolution.xy, float2(1.0, 1.0));
                float2 fragCoord = input.uv * resolution;
                float time = fmod(_iTime, 600.0);
                float3 lightDirection = normalize(float3(-0.3, 0.2, 1.0));

                float2 uv = fragCoord / resolution - 0.5;
                float2 uvUnscaled = uv;
                float aspectRatio = resolution.x / resolution.y;
                uv.x *= aspectRatio;

                float2 mouse = (_iMouse.xy / resolution - 0.5) * 4.0;
                float cameraTime = (time + 67.0) * 0.05;
                float zCamera = smoothstep(0.7, 1.0, cos(cameraTime)) * 1.8 - 0.3;
                zCamera -= smoothstep(0.7, 1.0, -cos(cameraTime)) * 1.6;

                if (_iMouse.z < 0.1)
                {
                    mouse = float2(sin(time * 0.15) * aspectRatio, zCamera);
                }

                float3 direction = normalize(float3(uv, 0.9));
                float3 origin = float3(1.0, 0.0, -0.5 + mouse.y) * 1.25;
                origin.xy = mul(Rotate2D(-mouse.x * 40.0), origin.xy);
                float3 target = float3(sin(time * 0.5) * 0.3, cos(time * 0.25) * 0.1, 0.0);
                direction = mul(LookAt(normalize(-origin + target), float3(0.0, 0.0, -1.0)), direction);

                float3 color = March(origin, direction, lightDirection, time);
                color *= float3(1.1, 0.9, 0.8);
                color += dot(uvUnscaled, uvUnscaled) * float3(0.0, 0.6, 1.0) * 0.8;
                color = 1.0 - exp(-color * 0.85);

                return half4(color, 1.0) * input.color;
            }
            ENDHLSL
        }
    }
}
