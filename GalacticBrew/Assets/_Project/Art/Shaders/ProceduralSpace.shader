Shader "GalacticBrew/ProceduralSpace"
{
    Properties
    {
        [Header(Rendering)]
        [Enum(Front,2,Back,1,Both,0)] _CullMode ("Cull Mode", Float) = 2

        [Header(Background)]
        _BaseColor ("Base Color", Color) = (0.01, 0.005, 0.02, 1)
        _NebulaColor1 ("Nebula Color 1", Color) = (0.15, 0.02, 0.25, 1)
        _NebulaColor2 ("Nebula Color 2", Color) = (0.02, 0.08, 0.3, 1)
        _NebulaIntensity ("Nebula Intensity", Range(0, 1)) = 0.3
        _NebulaScale ("Nebula Scale", Range(0.5, 10)) = 3

        [Header(Stars)]
        _StarDensity ("Star Density", Range(5, 50)) = 20
        _StarBrightness ("Star Brightness", Range(0.5, 5)) = 2.5
        _StarSize ("Star Size", Range(0.01, 0.15)) = 0.05
        _TwinkleSpeed ("Twinkle Speed", Range(0.5, 10)) = 3
        _TwinkleAmount ("Twinkle Amount", Range(0, 1)) = 0.6

        [Header(Star Layers)]
        _SmallStarDensity ("Small Star Density", Range(10, 100)) = 50
        _SmallStarBrightness ("Small Star Brightness", Range(0.1, 2)) = 0.6
        _SmallStarSize ("Small Star Size", Range(0.005, 0.05)) = 0.02
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Background"
            "Queue" = "Background"
            "RenderPipeline" = "UniversalPipeline"
        }

        ZWrite Off
        Cull [_CullMode]

        Pass
        {
            Name "ProceduralSpace"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _NebulaColor1;
                float4 _NebulaColor2;
                float _NebulaIntensity;
                float _NebulaScale;
                float _StarDensity;
                float _StarBrightness;
                float _StarSize;
                float _TwinkleSpeed;
                float _TwinkleAmount;
                float _SmallStarDensity;
                float _SmallStarBrightness;
                float _SmallStarSize;
            CBUFFER_END

            // ---- Hash / Noise helpers ----

            float2 Hash22(float2 p)
            {
                float3 q = float3(dot(p, float2(127.1, 311.7)),
                                  dot(p, float2(269.5, 183.3)),
                                  dot(p, float2(419.2, 371.9)));
                return frac(sin(q.xy) * 43758.5453);
            }

            float Hash21(float2 p)
            {
                float h = dot(p, float2(127.1, 311.7));
                return frac(sin(h) * 43758.5453);
            }

            // Value noise for nebula
            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f); // smoothstep

                float a = Hash21(i);
                float b = Hash21(i + float2(1, 0));
                float c = Hash21(i + float2(0, 1));
                float d = Hash21(i + float2(1, 1));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            // FBM (fractal Brownian motion)
            float FBM(float2 p, int octaves)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;

                for (int i = 0; i < octaves; i++)
                {
                    value += amplitude * ValueNoise(p * frequency);
                    frequency *= 2.0;
                    amplitude *= 0.5;
                }
                return value;
            }

            // ---- Star layer ----

            float StarField(float2 uv, float density, float starSize, float brightness,
                            float twinkleSpeed, float twinkleAmount, float time)
            {
                float stars = 0.0;
                float2 gridUV = uv * density;
                float2 cellID = floor(gridUV);
                float2 cellUV = frac(gridUV);

                // Check 3x3 neighborhood to avoid edge clipping
                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 offset = float2(x, y);
                        float2 neighbor = cellID + offset;
                        float2 randPos = Hash22(neighbor);

                        // Position of star within cell
                        float2 diff = offset + randPos - cellUV;
                        float dist = length(diff);

                        // Per-star random values
                        float randBright = Hash21(neighbor * 1.31);
                        float randPhase = Hash21(neighbor * 2.17) * 6.2831;
                        float randSpeed = Hash21(neighbor * 3.53) * 0.5 + 0.75;

                        // Twinkle
                        float twinkle = sin(time * twinkleSpeed * randSpeed + randPhase);
                        twinkle = twinkle * 0.5 + 0.5; // remap to 0..1
                        float tw = lerp(1.0, twinkle, twinkleAmount);

                        // Star shape — smooth falloff
                        float star = 1.0 - smoothstep(0.0, starSize, dist);
                        star = star * star; // sharper falloff

                        // Accumulate
                        stars += star * brightness * (0.3 + 0.7 * randBright) * tw;
                    }
                }

                return stars;
            }

            // ---- Vertex / Fragment ----

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float time = _Time.y;

                // --- Background ---
                float3 col = _BaseColor.rgb;

                // --- Nebula ---
                float2 nebulaUV = uv * _NebulaScale;
                float n1 = FBM(nebulaUV + float2(0.3, 0.7), 5);
                float n2 = FBM(nebulaUV * 1.3 + float2(5.1, 1.3), 5);
                float nebulaMask = saturate(n1 * n2 * 2.5);
                float3 nebulaCol = lerp(_NebulaColor1.rgb, _NebulaColor2.rgb, n2);
                col += nebulaCol * nebulaMask * _NebulaIntensity;

                // --- Big bright stars ---
                float bigStars = StarField(uv, _StarDensity, _StarSize, _StarBrightness,
                                           _TwinkleSpeed, _TwinkleAmount, time);

                // --- Small dim stars ---
                float smallStars = StarField(uv + float2(13.7, 29.3),
                                             _SmallStarDensity, _SmallStarSize, _SmallStarBrightness,
                                             _TwinkleSpeed * 1.5, _TwinkleAmount * 0.5, time);

                // --- Tiny background star dust (no twinkle) ---
                float dust = StarField(uv + float2(71.1, 53.9),
                                       80, 0.008, 0.25,
                                       0, 0, time);

                // Combine
                float totalStars = bigStars + smallStars + dust;

                // Slight color tint per star layer
                float3 starColor = float3(1, 1, 1) * totalStars;
                // Add warm tint to bright stars
                starColor += float3(0.3, 0.15, 0.0) * bigStars * 0.3;
                // Add cool tint to small stars
                starColor += float3(0.0, 0.1, 0.25) * smallStars * 0.2;

                col += starColor;

                return float4(saturate(col), 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
