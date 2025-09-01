Shader "Sprites/Glow2D"
{
    Properties
    {
        _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _GlowColor ("Glow Color (HDR)", Color) = (1,1,1,1)
        _GlowSize ("Glow Size (px)", Range(0,20)) = 6
        _GlowSoftness ("Glow Softness", Range(0,5)) = 2
        _GlowIntensity ("Glow Intensity", Range(0,5)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize; // x=1/width, y=1/height
            float4 _Color;
            float4 _GlowColor;
            float _GlowSize;
            float _GlowSoftness;
            float _GlowIntensity;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            // Sample alpha with a small ring of offsets
            float SampleGlowMask(float2 uv, float size)
            {
                // Convert size from pixels to UV
                float2 px = float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y);
                float2 r = px * size;

                // 8-direction taps (cheap but decent)
                float a = 0.0;
                a += tex2D(_MainTex, uv + float2( r.x,  0)).a;
                a += tex2D(_MainTex, uv + float2(-r.x,  0)).a;
                a += tex2D(_MainTex, uv + float2( 0 ,  r.y)).a;
                a += tex2D(_MainTex, uv + float2( 0 , -r.y)).a;
                a += tex2D(_MainTex, uv + r * normalize(float2( 1,  1))).a;
                a += tex2D(_MainTex, uv + r * normalize(float2(-1,  1))).a;
                a += tex2D(_MainTex, uv + r * normalize(float2( 1, -1))).a;
                a += tex2D(_MainTex, uv + r * normalize(float2(-1,-1))).a;
                return a / 8.0;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;

                // base alpha
                float a = col.a;

                // blurred alpha ring
                float mask = SampleGlowMask(i.uv, _GlowSize);

                // soften edges
                mask = smoothstep(0.0, 1.0 / max(0.001, _GlowSoftness), mask);

                // remove inner fill so glow sits outside: keep only where mask > original alpha
                float halo = saturate(mask - a);

                // compose
                fixed4 glow = _GlowColor * (halo * _GlowIntensity);
                glow.a = halo * _GlowIntensity;  // makes glow contribute to alpha for nicer blending

                // Additive-ish onto background while keeping sprite normal
                fixed4 outCol;
                outCol.rgb = col.rgb + glow.rgb;
                outCol.a   = saturate(col.a + glow.a);
                return outCol;
            }
            ENDCG
        }
    }
}
