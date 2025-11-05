Shader "Sprites/BlackholeSwirl_GradientTransparent"
{
    Properties
    {
        _InnerColor ("Inner Color", Color) = (0.96, 0.59, 0.07, 1)
        _OuterColor ("Outer Color", Color) = (0.05, 0.0, 0.2, 1)
        _Frequency ("Noise Frequency", Range(0.5, 3.0)) = 1.4
        _Distortion ("Distortion", Range(0.0, 0.05)) = 0.01
        _AlphaThreshold ("Alpha Threshold", Range(0.0, 1.0)) = 0.25
        _Intensity ("Emission Intensity", Range(0.0, 5.0)) = 1.2
        _Rotation ("Rotation Speed", Range(-5.0, 5.0)) = 1.0
        _Offset ("UV Offset", Vector) = (0, 0, 0, 0)

    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _InnerColor;
            float4 _OuterColor;
            float _Frequency;
            float _Distortion;
            float _AlphaThreshold;
            float _Intensity;
            float _Rotation;
                        float4 _Offset;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * 2.0 - 1.0; // range (-1,1)
                                o.uv -= _Offset.xy;      // <<=== geser pusat swirl

                return o;
            }

            float3 rotateZ(float3 v, float angle)
            {
                float s = sin(angle), c = cos(angle);
                return float3(v.x * c - v.y * s, v.x * s + v.y * c, v.z);
            }

            float hash21(float2 p) { p = frac(p * float2(123.34, 456.21)); p += dot(p, p + 45.32); return frac(p.x * p.y); }
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            float fbm(float2 p)
            {
                float v = 0.0;
                float a = 0.5;
                for (int i = 0; i < 5; i++)
                {
                    v += a * noise(p);
                    p *= 2.0;
                    a *= 0.5;
                }
                return v;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float t = _Time.y * _Rotation;

                // swirl rotation based on radius
                float angle = -log2(length(uv) + 0.01);
                float3 colVec = float3(uv, 0.5);
                colVec = rotateZ(colVec, angle + t);

                // fbm swirl noise
                float3 noiseCol;
                noiseCol.x = fbm(colVec.xy * _Frequency + 0.0);
                noiseCol.y = fbm(colVec.xy * _Frequency + 1.0);
                noiseCol.z = fbm(colVec.xy * _Frequency + 2.0);
                noiseCol += _Distortion;

                // glow intensity falloff
                float nLen = length(noiseCol * 0.5 + uv.xyxy.xy);
                nLen = 0.77 - nLen;
                nLen = saturate(pow(nLen * 4.2, 1.0));

                // radial gradient mix
                float radius = saturate(length(uv));
                float3 gradColor = lerp(_InnerColor.rgb, _OuterColor.rgb, radius);

                // final emission + gradient
                float3 finalColor = gradColor * nLen * _Intensity;

                // transparency control (outer fade)
                float alpha = nLen - _AlphaThreshold;
                alpha = saturate(alpha);

                return float4(finalColor, alpha);
            }
            ENDCG
        }
    }
    FallBack "Sprites/Default"
}
