// Silhueta do lutador atrás de props e relevo: desenha só onde algo opaco está NA FRENTE (ZTest Greater),
// na cor do time (SpriteRenderer.color: no URP chega por unity_SpriteColor, não pela cor de vértice). Os sprites não gravam profundidade, então um lutador não marca o outro.
Shader "Arena/Silhueta"
{
    Properties { _MainTex ("Sprite", 2D) = "white" {} }
    SubShader
    {
        Tags { "Queue"="Transparent+100" "RenderType"="Transparent" "IgnoreProjector"="True" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            ZWrite Off ZTest Greater Cull Off Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial) float4 _MainTex_ST; CBUFFER_END
            struct Attributes { float4 position:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            struct Varyings { float4 position:SV_POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            Varyings vert(Attributes i) { Varyings o; o.position=TransformObjectToHClip(i.position.xyz); o.uv=i.uv; o.color=i.color*unity_SpriteColor; return o; }
            half4 frag(Varyings i):SV_Target
            {
                half a=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).a;
                clip(a-.4h);
                return half4(i.color.rgb,i.color.a);
            }
            ENDHLSL
        }
    }
}
