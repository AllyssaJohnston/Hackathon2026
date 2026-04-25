Shader "Custom/ShaderToySunRays"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _iTime ("ShaderToy Time", Float) = 0
        _iResolution ("ShaderToy Resolution", Vector) = (1920, 1080, 0, 0)
        _iMouse ("ShaderToy Mouse", Vector) = (0, 0, 0, 0)
        _RayOpacity ("Ray Opacity", Range(0, 1)) = 0.65
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
            Name "ShaderToySunRaysUI"

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
                float _RayOpacity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                return output;
            }

            float RayStrength(
                float2 raySource,
                float2 rayRefDirection,
                float2 coord,
                float seedA,
                float seedB,
                float speed,
                float2 resolution
            )
            {
                float2 sourceToCoord = coord - raySource;
                float sourceDistance = max(length(sourceToCoord), 0.0001);
                float cosAngle = dot(normalize(sourceToCoord), rayRefDirection);

                return clamp(
                    (0.45 + 0.15 * sin(cosAngle * seedA + _iTime * speed)) +
                    (0.3 + 0.2 * cos(-cosAngle * seedB + _iTime * speed)),
                    0.0,
                    1.0
                ) * clamp((resolution.x - sourceDistance) / resolution.x, 0.5, 1.0);
            }

            float4 MainImage(float2 fragCoord)
            {
                float2 resolution = max(_iResolution.xy, float2(1.0, 1.0));

                float2 uv = fragCoord.xy / resolution.xy;
                uv.y = 1.0 - uv.y;

                float2 coord = float2(fragCoord.x, resolution.y - fragCoord.y);

                float2 rayPos1 = float2(resolution.x * 0.7, resolution.y * -0.4);
                float2 rayRefDir1 = normalize(float2(1.0, -0.116));
                float raySeedA1 = 36.2214;
                float raySeedB1 = 21.11349;
                float raySpeed1 = 1.5;

                float2 rayPos2 = float2(resolution.x * 0.8, resolution.y * -0.6);
                float2 rayRefDir2 = normalize(float2(1.0, 0.241));
                float raySeedA2 = 22.39910;
                float raySeedB2 = 18.0234;
                float raySpeed2 = 1.1;

                float rays1 = RayStrength(rayPos1, rayRefDir1, coord, raySeedA1, raySeedB1, raySpeed1, resolution);
                float rays2 = RayStrength(rayPos2, rayRefDir2, coord, raySeedA2, raySeedB2, raySpeed2, resolution);

                float rayAmount = rays1 * 0.5 + rays2 * 0.4;

                // Same bottom attenuation idea, but applied to alpha instead of color.
                float brightness = 1.0 - (coord.y / resolution.y);
                float alphaFade = 0.15 + brightness * 0.85;

                float alpha = saturate(rayAmount * alphaFade * _RayOpacity);

                // No color tint. White rays with transparent alpha.
                return float4(1.0, 1.0, 1.0, alpha);
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