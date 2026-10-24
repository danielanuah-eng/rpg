// Sprite dos lutadores e props (ArenaLuz): o mesmo desenho do Sprite-Unlit-Default do URP, mais
// (1) o tom global do mapa (_ArenaTom/_ArenaTomForca, ex. Lua de Sangue; força 0 = sem efeito) e
// (2) um passe ShadowCaster: só projeta sombra se o SpriteRenderer tiver shadowCastingMode On (ArenaLuz.Sombra).
// A sombra usa o recorte do sprite virado para a luz, então não some nem afina quando a câmera gira.
Shader "Arena/SpriteLuz"
{
    Properties
    {
        _MainTex ("Sprite", 2D) = "white" {}
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)
        _Cutoff ("Recorte da sombra", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "RenderPipeline"="UniversalPipeline" }
        Cull Off

        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
            ZWrite Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial) half4 _Color; half _Cutoff; CBUFFER_END
            half4 _ArenaTom; half _ArenaTomForca;
            struct Attributes { float3 positionOS:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            Varyings vert(Attributes i)
            {
                Varyings o; UNITY_SETUP_INSTANCE_ID(i); SetUpSpriteInstanceProperties();
                i.positionOS=UnityFlipSprite(i.positionOS,unity_SpriteProps.xy);
                o.positionCS=TransformObjectToHClip(i.positionOS); o.uv=i.uv;
                o.color=i.color*_Color*unity_SpriteColor;
                o.color.rgb*=lerp(half3(1,1,1),_ArenaTom.rgb,saturate(_ArenaTomForca));
                return o;
            }
            half4 frag(Varyings i):SV_Target { return i.color*SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv); }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial) half4 _Color; half _Cutoff; CBUFFER_END
            float3 _LightDirection; float3 _LightPosition;
            struct Attributes { float3 positionOS:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; half alpha:TEXCOORD1; };
            Varyings vert(Attributes i)
            {
                Varyings o; UNITY_SETUP_INSTANCE_ID(i); SetUpSpriteInstanceProperties();
                float3 p=UnityFlipSprite(i.positionOS,unity_SpriteProps.xy);
                // Recoloca o quadro em pé (eixo Y do mundo) e de frente para a luz, a partir do pivô.
                float3 pivot=TransformObjectToWorld(float3(0,0,0));
                float sx=length(float3(UNITY_MATRIX_M._m00,UNITY_MATRIX_M._m10,UNITY_MATRIX_M._m20));
                float sy=length(float3(UNITY_MATRIX_M._m01,UNITY_MATRIX_M._m11,UNITY_MATRIX_M._m21));
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 toLight=normalize(_LightPosition-pivot);
                #else
                    float3 toLight=_LightDirection;
                #endif
                float3 flat=float3(toLight.x,0,toLight.z); flat=dot(flat,flat)>1e-4?normalize(flat):float3(0,0,1);
                float3 right=cross(float3(0,1,0),flat);
                float3 positionWS=pivot+right*p.x*sx+float3(0,1,0)*p.y*sy;
                float4 positionCS=TransformWorldToHClip(ApplyShadowBias(positionWS,flat,toLight));
                #if UNITY_REVERSED_Z
                    positionCS.z=min(positionCS.z,UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z=max(positionCS.z,UNITY_NEAR_CLIP_VALUE);
                #endif
                o.positionCS=positionCS; o.uv=i.uv; o.alpha=i.color.a*_Color.a*unity_SpriteColor.a;
                return o;
            }
            half4 frag(Varyings i):SV_Target
            {
                clip(SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).a*i.alpha-_Cutoff);
                return 0;
            }
            ENDHLSL
        }
    }
}
