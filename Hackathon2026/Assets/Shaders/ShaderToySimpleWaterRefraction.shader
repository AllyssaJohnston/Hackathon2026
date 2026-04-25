Shader "Custom/ShaderToySimpleWaterRefraction"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _iTime ("ShaderToy Time", Float) = 0
        _iResolution ("ShaderToy Resolution", Vector) = (1920, 1080, 0, 0)
        _WaterSpeed ("Water Speed", Float) = 0.006
        _SpeedX ("Speed X", Float) = 0.13
        _SpeedY ("Speed Y", Float) = 0.13
        _Emboss ("Emboss", Float) = 0.25
        _Intensity ("Intensity", Float) = 2.4
        _Frequency ("Frequency", Float) = 6.0
        _Delta ("Delta", Float) = 60.0
        _Gain ("Gain", Float) = 100.0
        _ReflectionCutOff ("Reflection Cut Off", Float) = 0.012
        _ReflectionIntensity ("Reflection Intensity", Float) = 200000.0
        _OverallBrightness ("Overall Brightness", Float) = 1.0
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
            Name "SimpleWaterRefractionUI"

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
                float _WaterSpeed;
                float _SpeedX;
                float _SpeedY;
                float _Emboss;
                float _Intensity;
                float _Frequency;
                float _Delta;
                float _Gain;
                float _ReflectionCutOff;
                float _ReflectionIntensity;
                float _OverallBrightness;
            CBUFFER_END

            static const float WATER_PI = 3.1415926535897932;
            static const int steps = 8;
            static const int angle = 7;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                return output;
            }

            float Col(float2 coord, float time)
            {
                float deltaTheta = 2.0 * WATER_PI / float(angle);
                float accum = 0.0;

                [unroll]
                for (int i = 0; i < steps; i++)
                {
                    float theta = deltaTheta * float(i);
                    float2 adjc = coord;
                    adjc.x += cos(theta) * time * _WaterSpeed + time * _SpeedX;
                    adjc.y -= sin(theta) * time * _WaterSpeed - time * _SpeedY;
                    accum += cos((adjc.x * cos(theta) - adjc.y * sin(theta)) * _Frequency) * _Intensity;
                }

                return cos(accum);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 resolution = max(_iResolution.xy, float2(1.0, 1.0));
                float2 fragCoord = input.uv * resolution;
                float time = _iTime * 0.5;

                float2 p = fragCoord / resolution;
                float2 c1 = p;
                float2 c2 = p;

                float safeDelta = max(_Delta, 1.0);
                float cc1 = Col(c1, time);

                c2.x += resolution.x / safeDelta;
                float dx = _Emboss * (cc1 - Col(c2, time)) / safeDelta;

                c2 = p;
                c2.y += resolution.y / safeDelta;
                float dy = _Emboss * (cc1 - Col(c2, time)) / safeDelta;

                float2 refractedUv = p + float2(dx * 2.0, dy * 2.0);
                refractedUv = clamp(refractedUv, float2(0.001, 0.001), float2(0.999, 0.999));

                float alpha = saturate(1.0 + (dx * dy) * _Gain);
                float ddx = max(0.0, dx - _ReflectionCutOff);
                float ddy = max(0.0, dy - _ReflectionCutOff);

                if (ddx > 0.0 && ddy > 0.0)
                {
                    float reflectionPower = saturate(ddx * ddy * _ReflectionIntensity * 0.00001);
                    alpha = lerp(alpha, pow(max(alpha, 0.0001), 1.0 + reflectionPower), 0.35);
                }

                alpha = saturate(alpha);

                float4 bitmap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, refractedUv);
                float3 color = bitmap.rgb * alpha * max(_OverallBrightness, 0.0);
                color = saturate(color);

                return half4(color, bitmap.a) * input.color;
            }
            ENDHLSL
        }
    }
}

