Shader "Unlit/BalatroSwirlBackground"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (1, 0.6, 0.2, 1)
        _SecondaryColor ("Secondary Color", Color) = (0.2, 0.3, 1, 1)
        _SwirlStrength ("Swirl Strength", Range(0.0, 1.0)) = 0.3
        _RotationSpeed ("Rotation Speed", Range(0.0, 3.0)) = 0.15
        _PixelDensity ("Pixel Density", Range(100.0, 1500.0)) = 800.0
        _Intensity ("Color Intensity", Range(0.0, 2.0)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Background" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            #define GAMMA 2.2

            // === Properties ===
            float4 _MainColor;
            float4 _SecondaryColor;
            float _SwirlStrength;
            float _RotationSpeed;
            float _PixelDensity;
            float _Intensity;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // --- Rotation matrix ---
            float2 rotate2d(float2 uv, float a)
            {
                float c = cos(a);
                float s = sin(a);
                return mul(uv, float2x2(c, -s, s, c));
            }

            // --- Swirl / vortex effect ---
            float2 swirl(float2 uv, float t, float strength)
            {
                float r = length(uv);
                float a = -atan2(uv.y, uv.x);
                a += strength * r * t;
                return float2(cos(a), sin(a)) * r;
            }

            // --- Random gradient (for noise) ---
            float2 randomGradient(float2 p)
            {
                float rnd = frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
                float angle = rnd * 6.283185 + 0.8 * _Time.y * rnd;
                return float2(cos(angle), sin(angle));
            }

            // --- Quintic smoothing function ---
            float2 quintic(float2 p)
            {
                return p * p * p * (10.0 + p * (-15.0 + p * 6.0));
            }

            // --- Perlin Noise ---
            float pNoise(float2 uv)
            {
                float2 gridId = floor(uv);
                float2 gridUv = frac(uv);

                float2 bl = gridId + float2(0.0, 0.0);
                float2 br = gridId + float2(1.0, 0.0);
                float2 tl = gridId + float2(0.0, 1.0);
                float2 tr = gridId + float2(1.0, 1.0);

                float2 g1 = randomGradient(bl);
                float2 g2 = randomGradient(br);
                float2 g3 = randomGradient(tl);
                float2 g4 = randomGradient(tr);

                float2 d1 = gridUv - float2(0.0, 0.0);
                float2 d2 = gridUv - float2(1.0, 0.0);
                float2 d3 = gridUv - float2(0.0, 1.0);
                float2 d4 = gridUv - float2(1.0, 1.0);

                float v1 = dot(g1, d1);
                float v2 = dot(g2, d2);
                float v3 = dot(g3, d3);
                float v4 = dot(g4, d4);

                float2 w = quintic(gridUv);
                float bot = lerp(v1, v2, w.x);
                float top = lerp(v3, v4, w.x);
                return lerp(bot, top, w.y);
            }

            // --- fBm noise ---
            float fbmNoise(float2 uv)
            {
                float v = 0.0;
                float a = 1.0;
                for (int i = 0; i < 5; i++)
                {
                    v += pNoise(uv) * a;
                    uv *= 2.0;
                    a *= 0.5;
                }
                return v + 0.1;
            }

            // --- Domain warp noise ---
            float warp(float2 uv)
            {
                float fbm1 = fbmNoise(uv + float2(0.0, 0.0));
                float fbm2 = fbmNoise(uv + float2(5.2, 1.3));
                float fbm3 = fbmNoise(uv + 4.0 * fbm1 + float2(1.7, 9.2));
                float fbm4 = fbmNoise(uv + 4.0 * fbm2 + float2(8.3, 2.8));
                return fbmNoise(float2(fbm3, fbm4));
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float pixelSize = length(_ScreenParams.xy) / _PixelDensity;
                float2 fragCoord = i.uv * _ScreenParams.xy;
                float2 uv = (2.0 * floor(fragCoord / pixelSize) * pixelSize - _ScreenParams.xy) / _ScreenParams.y;
                uv *= 0.5;

                float t = _Time.y;

                // Swirl + rotation combo
                uv = swirl(uv, 30.0, _SwirlStrength) + rotate2d(uv, _RotationSpeed * t);

                float n = 0.5 + 0.5 * warp(uv);

                // Color blending
                float3 mainCol = _MainColor.rgb;
                float3 secondCol = _SecondaryColor.rgb;
                float blend = smoothstep(0.3, 0.8, n);
                float3 col = lerp(mainCol, secondCol, blend) * _Intensity;

                return float4(col, 1.0);
            }
            ENDCG
        }
    }
}
