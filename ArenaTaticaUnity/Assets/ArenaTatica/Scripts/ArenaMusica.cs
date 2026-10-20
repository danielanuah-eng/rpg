using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ArenaTatica.Core;

namespace ArenaTatica
{
    // Trilha eletrônica das formas com música (FormDef.music, Modos da L.E.L.A - 0, autor 21/10). Sintetizada na hora (sem
    // arquivos): bumbo, chimbal, baixo e arpejo num laço de 4 compassos; cada estilo muda andamento, escala e timbre. Toca
    // enquanto há um lutador vivo numa forma com música (o do time de quem olha primeiro) e troca com fade ao mudar de Modo.
    public class ArenaMusica : MonoBehaviour
    {
        const int Rate=22050;
        public const float Volume=.22f;
        AudioSource a, b; string playing="";
        static readonly Dictionary<string,AudioClip> cache=new Dictionary<string,AudioClip>();

        // bpm, nota base (Hz), escala (semitons), onda do baixo (0 seno, 1 serra), arpejo em semicolcheias, brilho do arpejo.
        static readonly Dictionary<string,(float bpm,float root,int[] scale,float saw,bool fast,float bright)> Styles=new Dictionary<string,(float,float,int[],float,bool,float)>
        {
            ["alegria"]=(128,65.41f,new[]{0,4,7,9,12,16},.3f,false,.9f),     // dó maior, house alegre
            ["ira"]=(150,55f,new[]{0,3,6,7,10,12},1f,true,.6f),              // lá menor com trítono, baixo serrilhado
            ["calma"]=(96,73.42f,new[]{0,2,4,7,9,12},0f,false,.35f),         // ré pentatônica, chill
            ["ansiedade"]=(140,61.74f,new[]{0,1,4,5,8,11},.6f,true,.75f),    // si com semitons, arpejo nervoso
            ["guerra"]=(165,49f,new[]{0,3,5,6,7,10},1f,true,1f),             // sol menor, drum'n'bass de guerra
        };

        void Awake()
        {
            a=Source(); b=Source();
        }
        AudioSource Source() { var s=gameObject.AddComponent<AudioSource>(); s.loop=true; s.playOnAwake=false; s.volume=0; return s; }

        // Chamado pela apresentação a cada quadro com o estado visto.
        public void Follow(MatchState s,int viewer)
        {
            string want="";
            if(s!=null&&!s.over) {
                var who=s.fighters.Where(f=>f.Alive&&!string.IsNullOrEmpty(Content.Form(f)?.music)).OrderBy(f=>s.Ally(f.team,viewer)?0:1).ThenBy(f=>f.id).FirstOrDefault();
                want=who==null?"":Content.Form(who).music;
            }
            if(want!=playing) {
                playing=want; (a,b)=(b,a);
                if(want!="") { a.clip=Clip(want); a.time=0; a.Play(); }
            }
            float step=Time.unscaledDeltaTime/.8f;
            a.volume=Mathf.MoveTowards(a.volume,playing==""?0:Volume,step*Volume);
            b.volume=Mathf.MoveTowards(b.volume,0,step*Volume);
            if(b.volume<=0&&b.isPlaying) b.Stop();
        }

        public static AudioClip Clip(string style)
        {
            if(cache.TryGetValue(style,out var c)&&c) return c;
            if(!Styles.TryGetValue(style,out var st)) st=Styles["alegria"];
            float beat=60f/st.bpm; int bars=4, n=(int)(beat*4*bars*Rate);
            var data=new float[n]; var rng=new System.Random(style.GetHashCode());
            float Note(int degree,int octave) => st.root*Mathf.Pow(2,(st.scale[degree%st.scale.Length]+12*octave)/12f);
            // Progressão de 4 graus por compasso.
            int[] prog={0,3,4,2};
            for(int i=0;i<n;i++)
            {
                float t=i/(float)Rate, inBeat=t%beat, bar=t/(beat*4); int barIndex=(int)bar%bars, step=(int)(t/(beat/4));
                float v=0;
                // Bumbo 4 por compasso (queda de tom rápida).
                float kt=inBeat; v+=Mathf.Sin(2*Mathf.PI*(50+120*Mathf.Exp(-kt*30))*kt)*Mathf.Exp(-kt*9)*.9f;
                // Chimbal no contratempo (ruído curto).
                float ht=(t+beat/2)%beat; v+=((float)rng.NextDouble()*2-1)*Mathf.Exp(-ht*60)*.18f;
                // Palma no 2 e no 4.
                if(((int)(t/beat))%2==1) v+=((float)rng.NextDouble()*2-1)*Mathf.Exp(-inBeat*25)*.22f;
                // Baixo: colcheias na raiz do grau do compasso, serra/seno conforme o estilo.
                float bf=Note(prog[barIndex],0), bt=t%(beat/2), ph=bf*t%1f;
                float bass=Mathf.Lerp(Mathf.Sin(2*Mathf.PI*ph),2*ph-1,st.saw)*Mathf.Min(1,bt*60)*Mathf.Exp(-bt*4);
                v+=bass*.35f*(1-Mathf.Exp(-kt*12)); // abaixa sob o bumbo (sidechain)
                // Arpejo: graus subindo; semicolcheias nos estilos rápidos, colcheias nos outros.
                int arpStep=st.fast?step:step/2; float len=st.fast?beat/4:beat/2, at=t%len;
                float af=Note(prog[barIndex]+arpStep%4*2,2), aph=af*t%1f;
                float arp=(aph<.5f?1:-1)*st.bright*.5f+Mathf.Sin(2*Mathf.PI*aph)*(1-st.bright*.5f);
                v+=arp*Mathf.Exp(-at*(st.fast?14:7))*.16f;
                // Pad leve na tônica do compasso (só nos calmos aparece mais).
                v+=Mathf.Sin(2*Mathf.PI*Note(prog[barIndex],1)*t)*.05f*(1.2f-st.bright);
                data[i]=Mathf.Clamp(v*.7f,-1,1);
            }
            // Bordas suaves para o laço não estalar.
            for(int i=0;i<200&&i<n;i++) { float k=i/200f; data[i]*=k; data[n-1-i]*=k; }
            c=AudioClip.Create("lela-"+style,n,1,Rate,false); c.SetData(data,0);
            return cache[style]=c;
        }
    }
}
