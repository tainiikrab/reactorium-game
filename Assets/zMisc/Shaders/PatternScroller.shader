Shader "Custom/HexPatternScroll_UI_URP_Correct"
{
    Properties
    {
        _BaseMap ("Icon", 2D) = "white" {}
        _Scale ("Icon Scale", Float) = 0.5
        _Spacing ("Spacing", Float) = 1.0
        _Alpha ("Alpha", Range(0,1)) = 1.0
        _ScrollDir ("Scroll Direction", Vector) = (1,0,0,0)
        _ScrollSpeed ("Scroll Speed", Float) = 0.5
        _Aspect ("Aspect (W/H)", Float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float _Scale;
                float _Spacing;
                float _Alpha;
                float4 _ScrollDir;
                float _ScrollSpeed;
                float _Aspect;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                o.color = v.color;
                return o;
            }

            float2 ApplyAspect(float2 uv, float aspect)
            {
                float2 c = uv - 0.5;
                if (aspect > 1.0) c.x *= aspect;
                else c.y /= aspect;
                return c + 0.5;
            }

            float2 GetHexCenter(float2 uv, float spacing)
            {
                float q = (2.0 / 3.0 * uv.x) / spacing;
                float r = (-1.0 / 3.0 * uv.x + 0.57735027 * uv.y) / spacing;

                float x = q;
                float z = r;
                float y = -x - z;

                float rx = round(x);
                float ry = round(y);
                float rz = round(z);

                float dx = abs(rx - x);
                float dy = abs(ry - y);
                float dz = abs(rz - z);

                if (dx > dy && dx > dz)
                    rx = -ry - rz;
                else if (dy > dz)
                    ry = -rx - rz;
                else
                    rz = -rx - ry;

                float2 center;
                center.x = spacing * (3.0 / 2.0 * rx);
                center.y = spacing * (1.7320508 * (rz + rx * 0.5));

                return center;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 uv = i.uv;

                float2 dir = normalize(_ScrollDir.xy + 1e-5);
                uv += dir * _ScrollSpeed * _Time.y;

                uv = ApplyAspect(uv, _Aspect);

                float2 center = GetHexCenter(uv, _Spacing);

                float2 centered = (uv - center) / _Scale + 0.5;

                float inside =
                    step(0, centered.x) * step(centered.x, 1) *
                    step(0, centered.y) * step(centered.y, 1);

                half4 col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, centered);
                col *= i.color;
                col.a *= inside * _Alpha;

                return col;
            }
            ENDHLSL
        }
    }
}