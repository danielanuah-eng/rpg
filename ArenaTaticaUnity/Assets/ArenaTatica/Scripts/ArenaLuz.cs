using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ArenaTatica
{
    // Iluminação do tabuleiro (URP 3D, autor 21/10): sol quente com sombra suave, preenchimento frio sem sombra, duas luzes
    // pontuais fracas nos cantos e ambiente em três tons (céu/horizonte/chão) a partir da cor do cenário. Os lutadores e props
    // em sprite projetam sombra pelo shader Arena/SpriteLuz. Desligada, volta ao visual antigo (luz chapada, sem sombras).
    // Tom global do mapa (Lua de Sangue e outros eventos): ArenaLuz.Tingir(cor,força,segundos) / ArenaLuz.LimparTom(segundos).
    public class ArenaLuz : MonoBehaviour
    {
        // --- Calibragem do autor ---
        public const float IntensidadePadrao=1f;          // multiplica todas as luzes; o jogador ajusta por ArenaLuz.Intensidade (0–2)
        public const float Sol=1.15f, Preenchimento=.35f, Pontuais=.5f;
        public const float SombraForca=.6f;               // 0 sem sombra, 1 preta: .6 escurece sem esconder as casas
        public const float SombraDistancia=110;           // a câmera ortográfica fica a 62 do centro: com os 50 do URP as sombras sumiam
        public const float AmbienteCeu=1.05f, AmbienteHorizonte=.85f, AmbienteChao=.5f;
        static readonly Color CorSol=new Color(1f,.94f,.84f), CorPreenchimento=new Color(.6f,.7f,1f), CorQuente=new Color(1f,.78f,.55f), CorFria=new Color(.55f,.7f,1f);
        static readonly Vector3 RotacaoSol=new Vector3(52,-35,0), RotacaoPreenchimento=new Vector3(35,145,0);
        // Luz antiga (desligada): a mesma de antes desta iluminação.
        const float SolAntigo=1.25f;

        const string KeyOn="luz-ligada", KeyIntensidade="luz-intensidade";
        public static bool Ligada { get { try { return PlayerPrefs.GetInt(KeyOn,1)==1; } catch { return true; } } set { PlayerPrefs.SetInt(KeyOn,value?1:0); Aplicar(); } }
        public static float Intensidade { get { try { return Mathf.Clamp(PlayerPrefs.GetFloat(KeyIntensidade,IntensidadePadrao),0,2); } catch { return IntensidadePadrao; } } set { PlayerPrefs.SetFloat(KeyIntensidade,Mathf.Clamp(value,0,2)); Aplicar(); } }
        // Multiplicador de cor em vigor (branco = sem tom), já com a transição; útil para tingir também céu/HUD.
        public static Color Tom => instance?instance.tom:Color.white;
        public static Material SpriteMaterial { get { if(!spriteMaterial) spriteMaterial=Resources.Load<Material>("Materials/ArenaSpriteLuz"); return spriteMaterial; } }

        static ArenaLuz instance; static Material spriteMaterial;
        Light sol, preenchimento; readonly Light[] pontos=new Light[2];
        Color ambiente=new Color(.55f,.6f,.72f), tom=Color.white, tomDe=Color.white, tomPara=Color.white;
        float tomInicio, tomDuracao; float largura=12;
        static readonly int TomId=Shader.PropertyToID("_ArenaTom"), TomForcaId=Shader.PropertyToID("_ArenaTomForca");
        UniversalRenderPipelineAsset urp; float sombraAntes; int cascatasAntes; float divisaoAntes;

        // Chamado pelo ArenaPresentation ao montar o tabuleiro. Reaproveita a direcional de uma cena que já tenha uma.
        public static ArenaLuz Montar()
        {
            if(instance) { instance.Tingir(Color.white,0); return instance; }
            instance=new GameObject("Iluminacao").AddComponent<ArenaLuz>();
            var cena=FindObjectsByType<Light>(); instance.sol=System.Array.Find(cena,l=>l.type==LightType.Directional);
            if(!instance.sol) instance.sol=Nova("LuzPrincipal",LightType.Directional,instance.transform);
            instance.preenchimento=Nova("LuzPreenchimento",LightType.Directional,instance.transform);
            for(int i=0;i<2;i++) instance.pontos[i]=Nova(i==0?"LuzQuente":"LuzFria",LightType.Point,instance.transform);
            instance.urp=GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if(instance.urp) { instance.sombraAntes=instance.urp.shadowDistance; instance.cascatasAntes=instance.urp.shadowCascadeCount; instance.divisaoAntes=instance.urp.cascade2Split; }
            // Evento de dados com cor (Lua de Sangue): ArenaEventTint passa a usar este tom em vez da tinta simples dele.
            ArenaEventTint.Hook=c=>{ if(c is Color cor) Tingir(cor,ArenaEventTint.Strength,1.5f); else LimparTom(1.5f); };
            Aplicar(); return instance;
        }
        static Light Nova(string name,LightType type,Transform parent)
        {
            var l=new GameObject(name).AddComponent<Light>(); l.transform.SetParent(parent,false); l.type=type; l.shadows=LightShadows.None; return l;
        }
        // Cor do ambiente do cenário (Palette.ambient) e largura do mapa (posição das luzes pontuais).
        public static void Ambiente(Color cor,float larguraMapa=0)
        {
            if(!instance) { RenderSettings.ambientMode=AmbientMode.Flat; RenderSettings.ambientLight=cor; return; }
            instance.ambiente=cor; if(larguraMapa>0) instance.largura=larguraMapa; Aplicar();
        }
        // Tinge o mapa: cor multiplicada nas luzes, no ambiente e nos sprites. força 0–1; segundos = transição.
        public static void Tingir(Color cor,float forca=1,float segundos=1) { if(instance) instance.Tingir(Color.Lerp(Color.white,cor,Mathf.Clamp01(forca)),segundos); }
        public static void LimparTom(float segundos=1) => Tingir(Color.white,0,segundos);
        // Sprite que projeta sombra (lutador, prop): troca para o material com passe de sombra.
        public static void Sombra(SpriteRenderer r)
        {
            if(!r||!SpriteMaterial) return;
            r.sharedMaterial=SpriteMaterial; r.shadowCastingMode=ShadowCastingMode.On; r.receiveShadows=false;
        }

        void Tingir(Color alvo,float segundos)
        {
            tomDe=tom; tomPara=alvo; tomInicio=Time.unscaledTime; tomDuracao=Mathf.Max(0,segundos);
            if(tomDuracao==0||!Application.isPlaying) { tom=alvo; Aplicar(); }
        }
        void Update()
        {
            if(tom==tomPara) return;
            float t=tomDuracao<=0?1:Mathf.Clamp01((Time.unscaledTime-tomInicio)/tomDuracao);
            tom=Color.Lerp(tomDe,tomPara,t*t*(3-2*t)); if(t>=1) tom=tomPara; Aplicar();
        }
        public static void Aplicar()
        {
            var me=instance; if(!me||!me.sol) return;
            bool on=Ligada; float k=on?Intensidade:1; Color t=me.tom;
            me.sol.transform.rotation=Quaternion.Euler(RotacaoSol);
            me.sol.intensity=on?Sol*k:SolAntigo; me.sol.color=(on?CorSol:Color.white)*t;
            me.sol.shadows=on?LightShadows.Soft:LightShadows.None; me.sol.shadowStrength=SombraForca;
            me.sol.shadowBias=.05f; me.sol.shadowNormalBias=.4f;
            me.preenchimento.enabled=on; me.preenchimento.transform.rotation=Quaternion.Euler(RotacaoPreenchimento);
            me.preenchimento.intensity=Preenchimento*k; me.preenchimento.color=CorPreenchimento*t;
            float d=me.largura*.42f;
            for(int i=0;i<2;i++) {
                var p=me.pontos[i]; p.enabled=on&&Pontuais>0; p.transform.position=i==0?new Vector3(-d,3.2f,-d):new Vector3(d,3.2f,d);
                p.range=me.largura*.95f; p.intensity=Pontuais*k; p.color=(i==0?CorQuente:CorFria)*t;
            }
            var a=me.ambiente;
            if(on) {
                RenderSettings.ambientMode=AmbientMode.Trilight;
                RenderSettings.ambientSkyColor=a*AmbienteCeu*t; RenderSettings.ambientEquatorColor=a*AmbienteHorizonte*t; RenderSettings.ambientGroundColor=a*AmbienteChao*t;
            }
            else { RenderSettings.ambientMode=AmbientMode.Flat; RenderSettings.ambientLight=a*t; }
            Shader.SetGlobalColor(TomId,t); Shader.SetGlobalFloat(TomForcaId,t==Color.white?0:1);
            if(me.urp) {
                me.urp.shadowDistance=on?Mathf.Max(me.sombraAntes,SombraDistancia):me.sombraAntes;
                // Duas cascatas: a primeira (perto da câmera) fica vazia; a segunda cobre o tabuleiro com mais resolução.
                me.urp.shadowCascadeCount=on?2:me.cascatasAntes; if(on) me.urp.cascade2Split=.5f; else me.urp.cascade2Split=me.divisaoAntes;
            }
        }
        // O asset do URP é compartilhado: no editor, devolve os valores salvos para não sujar Settings/PC_RPAsset.
        void OnDestroy() { if(instance==this) Desmontar(); }
        public static void Desmontar()
        {
            var me=instance; if(!me) return; instance=null;
            if(me.urp) { me.urp.shadowDistance=me.sombraAntes; me.urp.shadowCascadeCount=me.cascatasAntes; me.urp.cascade2Split=me.divisaoAntes; }
            Shader.SetGlobalFloat(TomForcaId,0); ArenaEventTint.Hook=null;
            if(me) { if(Application.isPlaying) Destroy(me.gameObject); else DestroyImmediate(me.gameObject); }
        }
    }
}
