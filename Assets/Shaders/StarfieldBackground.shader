Shader "Custom/StarfieldWithAppearance"
{
    Properties
    {
        _StarDensity ("Star Density", Range(0, 0.1)) = 0.02
        _StarSize ("Star Size", Range(0.001, 0.1)) = 0.008
        _TwinkleSpeed ("Twinkle Speed", Range(0, 5)) = 1.5
        _TwinkleAmount ("Twinkle Amount", Range(0, 1)) = 0.3
        _AppearSpeed ("Appear Speed", Range(0, 3)) = 0.5
        _DisappearSpeed ("Disappear Speed", Range(0, 3)) = 0.3
        _StarColor ("Star Color", Color) = (1, 1, 1, 1)
        _HaloSize ("Halo Size", Range(1, 3)) = 1.5
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "Queue" = "Background"
            "PreviewType" = "Plane"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            float _StarDensity;
            float _StarSize;
            float _TwinkleSpeed;
            float _TwinkleAmount;
            float _AppearSpeed;
            float _DisappearSpeed;
            float4 _StarColor;
            float _HaloSize;

            // Hash function untuk random
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            // Noise function 
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            // Fungsi untuk mengatur kemunculan bintang berdasarkan waktu
            float getStarAppearance(float2 gridPos, float time)
            {
                // Base appearance dari hash
                float baseAppearance = hash(gridPos);
                
                // Modulasi dengan waktu untuk kemunculan bertahap
                float appearCycle = sin(time * _AppearSpeed + hash(gridPos + float2(1.0, 2.0)) * 6.283) * 0.5 + 0.5;
                float disappearCycle = sin(time * _DisappearSpeed + hash(gridPos + float2(3.0, 4.0)) * 6.283) * 0.5 + 0.5;
                
                // Gabungkan efek kemunculan dan penghilangan
                float appearance = baseAppearance * appearCycle * (1.0 - disappearCycle * 0.3);
                
                return appearance;
            }

            // Fungsi bintang dengan falloff yang smooth
            float star(float2 uv, float2 starPos, float size, float brightness)
            {
                float2 d = uv - starPos;
                float dist = length(d);
                
                // Inti bintang yang tajam
                float core = 1.0 - smoothstep(0.0, size, dist);
                
                // Halo bintang yang lembut
                float halo = 1.0 - smoothstep(0.0, size * _HaloSize, dist);
                halo *= 0.3;
                
                return (core + halo) * brightness;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = fixed4(0, 0, 0, 1);
                float2 pixelPos = i.worldPos.xy * 10.0;
                
                float3 starLight = float3(0, 0, 0);
                float gridSize = 8.0;
                float2 gridPos = floor(pixelPos * gridSize);
                float2 cellUV = frac(pixelPos * gridSize);
                
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        float2 neighborGrid = gridPos + float2(x, y);
                        
                        // Gunakan fungsi appearance baru dengan waktu
                        float appearance = getStarAppearance(neighborGrid, _Time.y);
                        if (appearance > _StarDensity) continue;
                        
                        // Posisi bintang dalam grid
                        float2 starOffset = hash(neighborGrid + float2(1.2, 3.4));
                        starOffset = starOffset * 0.8 + 0.1;
                        
                        // Efek kedipan dengan multiple frequencies
                        float time = _Time.y * _TwinkleSpeed;
                        float twinkle1 = (sin(time + appearance * 6.283) + 1.0) * 0.5;
                        float twinkle2 = (sin(time * 1.7 + appearance * 12.566) + 1.0) * 0.25;
                        float twinkle = (twinkle1 + twinkle2) * 0.666;
                        
                        // Brightness dengan kontrol appearance
                        float brightness = (0.7 + appearance * 0.6) * 
                                         (1.0 - _TwinkleAmount + twinkle * _TwinkleAmount);
                        
                        // Modulasi brightness berdasarkan cycle appearance
                        brightness *= (sin(_Time.y * _AppearSpeed * 0.5 + appearance * 3.141) * 0.5 + 0.5);
                        
                        float sizeVariation = hash(neighborGrid + float2(5.6, 7.8));
                        float starSize = _StarSize * (0.5 + sizeVariation * 0.8);
                        
                        float starValue = star(cellUV, starOffset, starSize, brightness);
                        
                        float3 finalStarColor = _StarColor.rgb;
                        float colorVariation = hash(neighborGrid + float2(4.5, 6.7));
                        finalStarColor = lerp(finalStarColor, 
                                            lerp(finalStarColor, float3(0.9, 0.95, 1.0), 0.3),
                                            colorVariation * 0.4);
                        
                        starLight += finalStarColor * starValue;
                    }
                }
                
                col.rgb += starLight;
                return col;
            }
            ENDCG
        }
    }
}