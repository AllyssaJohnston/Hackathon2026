Shader "Custom/ShaderToyBubbleOverlay"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("UI Tint", Color) = (1, 1, 1, 1)

        _iTime ("ShaderToy Time", Float) = 0
        _BubbleTime ("Bubble Local Time", Float) = 0
        _BubbleTravelTime ("Bubble Travel Time", Range(0.5, 5)) = 0.9
        _BubbleSpawnWindow ("Bubble Spawn Window", Range(0, 2)) = 0.7
        _BubbleStartOffset ("Bubble Start Offset", Range(0, 2)) = 0.2
        _iResolution ("ShaderToy Resolution", Vector) = (1920, 1080, 0, 0)

        _BubbleTint ("Bubble Tint", Color) = (1.0, 1.0, 1.0, 1.0)
        _BubbleOpacity ("Bubble Opacity", Range(0, 2)) = 0.0
        _BubbleDensity ("Bubble Density", Range(0, 1)) = 0.0
        _AutoMove ("Auto Move", Range(0, 1)) = 1.0
        _RiseSpeed ("Auto Rise Speed", Range(0, 3)) = 1.0
        _MinBubbleSpeed ("Min Bubble Speed", Range(0.01, 2)) = 0.8
        _MaxBubbleSpeed ("Max Bubble Speed", Range(0.01, 2)) = 1.3
        _EdgeFade ("Edge Fade", Range(0.01, 0.5)) = 0.18
        _BubbleScale ("Bubble Scale", Range(0.25, 4)) = 1.0
        _BubbleIntensity ("Bubble Intensity", Range(0, 4)) = 1.5
        _AlphaPower ("Alpha Sharpness", Range(0.25, 4)) = 1.0
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
            Name "BubbleOverlayUI"

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
                float _BubbleTime;
                float _BubbleTravelTime;
                float _BubbleSpawnWindow;
                float _BubbleStartOffset;
                float4 _iResolution;

                half4 _BubbleTint;
                float _BubbleOpacity;
                float _BubbleDensity;
                float _AutoMove;
                float _RiseSpeed;
                float _MinBubbleSpeed;
                float _MaxBubbleSpeed;
                float _EdgeFade;
                float _BubbleScale;
                float _BubbleIntensity;
                float _AlphaPower;
            CBUFFER_END

            static const float4 NC0 = float4(0.0, 157.0, 113.0, 270.0);
            static const float4 NC1 = float4(1.0, 158.0, 114.0, 271.0);

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                return output;
            }

            float2 Mod2(float2 p, float c)
            {
                return p - c * floor(p / c);
            }

            float HashNull(float2 x)
            {
                return frac(523.0 * sin(dot(x, float2(53.3158, 43.6143))));
            }

            float4 Hash4(float4 n)
            {
                return frac(sin(n) * 753.5453123);
            }

            float2 Hash2(float2 n)
            {
                return frac(sin(n) * 753.5453123);
            }

            float Noise2(float2 x)
            {
                float2 p = floor(x);
                float2 f = frac(x);
                f = f * f * (3.0 - 2.0 * f);

                float n = p.x + p.y * 157.0;
                float2 s1 = lerp(
                    Hash2(float2(n, n) + NC0.xy),
                    Hash2(float2(n, n) + NC1.xy),
                    float2(f.x, f.x)
                );

                return lerp(s1.x, s1.y, f.y);
            }

            float Noise3(float3 x)
            {
                float3 p = floor(x);
                float3 f = frac(x);
                f = f * f * (3.0 - 2.0 * f);

                float n = p.x + dot(p.yz, float2(157.0, 113.0));
                float4 s1 = lerp(
                    Hash4(float4(n, n, n, n) + NC0),
                    Hash4(float4(n, n, n, n) + NC1),
                    float4(f.x, f.x, f.x, f.x)
                );

                return lerp(
                    lerp(s1.x, s1.y, f.y),
                    lerp(s1.z, s1.w, f.y),
                    f.z
                );
            }

            float4 Bubble(float2 te, float2 pos, float numCells)
            {
                float d = dot(te, te);

                float2 te1 = te + (pos - float2(0.5, 0.5)) * 0.4 / numCells;
                float2 te2 = -te1;

                float zb1 = max(pow(Noise2(te2 * 1000.11 * d), 10.0), 0.01);
                float zb2 = Noise2(te1 * 1000.11 * d);
                float zb3 = Noise2(te1 * 200.11 * d);
                float zb4 = Noise2(te1 * 200.11 * d + float2(20.0, 20.0));

                float4 colorb = float4(1.0, 1.0, 1.0, 1.0);
                colorb.rgb *= 0.7 + Noise2(te1 * 1000.11 * d) * 0.3;

                zb2 = max(pow(zb2, 20.1), 0.01);
                colorb.rgb *= zb2 * 1.9;

                float4 color = float4(
                    Noise2(te2 * 10.8),
                    Noise2(te2 * 9.5 + float2(15.0, 15.0)),
                    Noise2(te2 * 11.2 + float2(12.0, 12.0)),
                    1.0
                );

                color = lerp(
                    color,
                    float4(1.0, 1.0, 1.0, 1.0),
                    Noise2(te2 * 20.5 + float2(200.0, 200.0))
                );

                color.rgb *= 0.7 + Noise2(te2 * 1000.11 * d) * 0.3;
                color.rgb *= 0.2 + zb1 * 1.9;

                float r1 = max(
                    min((0.033 - min(0.04, d)) * 100.0 / sqrt(numCells), 1.0),
                    -1.6
                );

                float d2 = (0.06 - min(0.06, d)) * 10.0;
                d = (0.04 - min(0.04, d)) * 10.0;

                color.rgb += colorb.rgb * d * 1.5;

                float f1 = min(d * 10.0, 0.5 - d) * 2.2;
                f1 = pow(f1, 4.0);

                float f2 = min(min(d * 4.1, 0.9 - d) * 2.0 * r1, 1.0);

                float f3 = min(d2 * 2.0, 0.7 - d2) * 2.2;
                f3 = pow(f3, 4.0);

                return color * max(min(f1 + f2, 1.0), -0.5)
                    + float4(zb3, zb3, zb3, zb3) * f3
                    - float4(zb4, zb4, zb4, zb4) * (f2 * 0.5 + f1) * 0.5;
            }

            float4 Cells(inout float2 uvState, float2 p, float2 move, float numCells, float baseChance, float blur)
            {
                float density = saturate(_BubbleDensity);

                if (density <= 0.001)
                {
                    return float4(0.0, 0.0, 0.0, 0.0);
                }

                float occupancyChance = saturate(baseChance * density);
                float minSpeed = min(_MinBubbleSpeed, _MaxBubbleSpeed);
                float maxSpeed = max(_MinBubbleSpeed, _MaxBubbleSpeed);
                float edgeFade = max(_EdgeFade, 0.001);

                float2 inp = p + move;
                inp *= numCells;

                float d = 1.0;
                float2 pos = float2(0.0, 0.0);
                float bubbleEdgeFade = 0.0;

                [unroll]
                for (int xo = -1; xo <= 1; xo++)
                {
                    [unroll]
                    for (int yo = -2; yo <= 2; yo++)
                    {
                        float2 cellId = floor(inp) + float2(xo, yo);
                        float2 hashBase = cellId + move * 37.0;

                        float presence = HashNull(hashBase + 61.0);

                        if (presence > occupancyChance)
                        {
                            continue;
                        }

                        float randomDelay = HashNull(hashBase) * _BubbleSpawnWindow;
                        float randomSpeedMultiplier = lerp(minSpeed, maxSpeed, HashNull(hashBase + 17.0));
                        float randomX = lerp(0.14, 0.86, HashNull(hashBase + 29.0));
                        float localTime = _BubbleTime - randomDelay;
                        float progress = localTime / max(_BubbleTravelTime, 0.001);

                        if (progress < 0.0 || progress > 1.0)
                        {
                            continue;
                        }

                        float riseProgress = saturate(progress * _RiseSpeed * randomSpeedMultiplier);
                        float bubbleY = lerp(-_BubbleStartOffset, 1.0 + edgeFade, riseProgress);

                        float2 tp = cellId + float2(randomX, bubbleY);

                        float swayPhase = HashNull(hashBase + 43.0) * 6.28318;
                        float swaySpeed = lerp(0.6, 1.2, HashNull(hashBase + 44.0));
                        float swayAmount = lerp(0.015, 0.045, HashNull(hashBase + 45.0));
                        tp.x += sin(_BubbleTime * swaySpeed + swayPhase) * swayAmount;

                        float2 l = inp - tp;
                        float dr = dot(l, l);

                        if (d > dr)
                        {
                            d = dr;
                            pos = tp;

                            float fadeIn = smoothstep(0.0, 0.12, progress);
                            float fadeOut = 1.0 - smoothstep(0.82, 1.0, progress);
                            bubbleEdgeFade = saturate(fadeIn * fadeOut);
                        }
                    }
                }

                if (d >= 0.06)
                {
                    return float4(0.0, 0.0, 0.0, 0.0);
                }

                float2 te = inp - pos;

                if (d < 0.04)
                {
                    uvState += te * d * 2.0;
                }

                if (blur > 0.0001)
                {
                    float4 c = float4(0.0, 0.0, 0.0, 0.0);

                    [unroll]
                    for (int ix = 0; ix < 4; ix++)
                    {
                        [unroll]
                        for (int iy = 0; iy < 4; iy++)
                        {
                            float x = -1.0 + ix * 0.5;
                            float y = -1.0 + iy * 0.5;

                            c += Bubble(te + float2(x, y) * blur, p, numCells);
                        }
                    }

                    return c * 0.05 * bubbleEdgeFade;
                }

                return Bubble(te, p, numCells) * bubbleEdgeFade;
            }

            float4 MainImage(float2 fragCoord)
            {
                float2 resolution = max(_iResolution.xy, float2(1.0, 1.0));
                float2 screenUV = fragCoord.xy / resolution.xy;

                float scale = max(_BubbleScale, 0.001);
                float2 uvState = float2(screenUV.x * (resolution.x / resolution.y), screenUV.y) / scale;

                float4 e = float4(0.0, 0.0, 0.0, 0.0);

                float4 cr1 = Cells(uvState, uvState, float2(11.73, 42.91), 6.0, 0.050, 0.002);
                float4 cr2 = Cells(uvState, uvState, float2(23.31, 61.18), 8.0, 0.040, 0.003);
                float4 cr3 = Cells(uvState, uvState, float2(47.62, 13.27), 10.0, 0.030, 0.006);
                float4 cr4 = Cells(uvState, uvState, float2(72.14, 95.44), 12.0, 0.025, 0.008);
                float4 cr5 = Cells(uvState, uvState, float2(5.24, 78.38), 16.0, 0.015, 0.012);
                float4 cr6 = Cells(uvState, uvState, float2(90.51, 27.66), 20.0, 0.010, 0.014);

                e = max(e - float4(dot(cr6, cr6), dot(cr6, cr6), dot(cr6, cr6), dot(cr6, cr6)) * 0.08, 0.0) + cr6 * 1.1;
                e = max(e - float4(dot(cr5, cr5), dot(cr5, cr5), dot(cr5, cr5), dot(cr5, cr5)) * 0.08, 0.0) + cr5 * 1.2;
                e = max(e - float4(dot(cr4, cr4), dot(cr4, cr4), dot(cr4, cr4), dot(cr4, cr4)) * 0.08, 0.0) + cr4 * 1.25;
                e = max(e - float4(dot(cr3, cr3), dot(cr3, cr3), dot(cr3, cr3), dot(cr3, cr3)) * 0.08, 0.0) + cr3 * 1.3;
                e = max(e - float4(dot(cr2, cr2), dot(cr2, cr2), dot(cr2, cr2), dot(cr2, cr2)) * 0.08, 0.0) + cr2 * 1.35;
                e = max(e - float4(dot(cr1, cr1), dot(cr1, cr1), dot(cr1, cr1), dot(cr1, cr1)) * 0.08, 0.0) + cr1 * 1.45;

                float intensity = max(max(e.r, e.g), e.b);
                intensity = saturate(intensity * _BubbleIntensity);

                float alpha = pow(intensity, _AlphaPower);
                alpha *= _BubbleOpacity;
                alpha = saturate(alpha);

                float3 bubbleColor = saturate(e.rgb * _BubbleIntensity);
                bubbleColor *= _BubbleTint.rgb;

                float highlight = saturate(intensity * 1.25);
                bubbleColor = lerp(bubbleColor, float3(1.0, 1.0, 1.0), highlight * 0.22);

                return float4(bubbleColor, alpha);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 resolution = max(_iResolution.xy, float2(1.0, 1.0));
                float2 fragCoord = input.uv * resolution;

                float4 color = MainImage(fragCoord);

                color.rgb *= input.color.rgb;
                color.a *= input.color.a;

                return half4(color);
            }

            ENDHLSL
        }
    }
}
