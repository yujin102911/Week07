Shader "Custom/URP2DOutlineSprite"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _OutlineSize("Outline Size", Range(0, 5)) = 1
    }

    SubShader
    {
        Tags{
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderPipeline"="UniversalRenderPipeline"
            "UniversalMaterialType"="SpriteUnlit"
            "ShaderModel"="2.0"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            Name "SpriteUnlit"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core2D.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            float4 _Color;
            float4 _OutlineColor;
            float _OutlineSize;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 baseCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * _Color;

                float2 offset = _MainTex_TexelSize.xy * _OutlineSize;
                float alpha = 0.0;

                // 주변 8방향
                alpha += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-offset.x, 0)).a;
                alpha += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(offset.x, 0)).a;
                alpha += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0, -offset.y)).a;
                alpha += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0, offset.y)).a;
                alpha += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(offset.x, offset.y)).a;
                alpha += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-offset.x, offset.y)).a;
                alpha += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(offset.x, -offset.y)).a;
                alpha += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-offset.x, -offset.y)).a;

                float outlineMask = step(0.1, alpha) * step(baseCol.a, 0.1);

                half4 outline = _OutlineColor;
                outline.a *= outlineMask;

                return baseCol + outline * (1 - baseCol.a);
            }
            ENDHLSL
        }
    }
}
