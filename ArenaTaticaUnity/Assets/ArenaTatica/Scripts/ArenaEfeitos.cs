using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArenaTatica
{
    // Entrega visual 017: manifesto das tiras (Resources/Efeitos/manifesto.json, copiado de assets.visual.json sem a Biblioteca).
    [Serializable] public class EfeitoEntrada
    {
        public string resource; public int frameSize, frames; public float fps; public bool loop;
        public int impactFrame0, peakFrame0; public float[] unityPivot; public float pixelsPerUnit;
        public bool ownerOnly, inheritTargetVisibility, moveFromTargetToCaster, repeatPulse, casterOverlay, staticProjectile;
    }
    [Serializable] class EfeitoManifesto { public EfeitoEntrada[] entries; }

    // Carga de fases de efeito e regras de visibilidade, sem depender de cena (testável em EditMode).
    public static class ArenaEfeitos
    {
        static Dictionary<string,EfeitoEntrada> manifest;
        public static IReadOnlyCollection<EfeitoEntrada> Entradas { get { Load(); return manifest.Values; } }
        static void Load()
        {
            if(manifest!=null) return;
            manifest=new Dictionary<string,EfeitoEntrada>();
            var text=Resources.Load<TextAsset>("Efeitos/manifesto");
            if(!text) return;
            var data=JsonUtility.FromJson<EfeitoManifesto>(text.text);
            if(data?.entries!=null) foreach(var e in data.entries) manifest[e.resource]=e;
        }
        public static void Reset() { manifest=null; }
        // resource = caminho Resources do manifesto, ex.: Sprites/Efeitos/mare-de-lava/inicio.
        public static EfeitoEntrada Entrada(string resource) { Load(); return resource!=null&&manifest.TryGetValue(resource,out var e)?e:null; }
        public static float Fps(string resource,float fallback) { var e=Entrada(resource); return e!=null&&e.fps>0?e.fps:fallback; }

        // Carrega UMA fase (um arquivo) e só os quadros dela: Resources.LoadAll num caminho que também é pasta traria as outras fases.
        public static Sprite[] Frames(string resource)
        {
            if(string.IsNullOrEmpty(resource)) return null;
            string leaf=resource.Substring(resource.LastIndexOf('/')+1);
            var frames=Resources.LoadAll<Sprite>(resource).Where(x=>x.name==leaf||x.name.StartsWith(leaf+"_")).OrderBy(x=>Index(x.name)).ToArray();
            return frames.Length>0?frames:null;
        }
        static int Index(string name) => int.TryParse(name.Substring(name.LastIndexOf('_')+1),out int n)?n:0;

        // Um efeito preso a um dono (mina armada) só aparece para quem o dono é aliado; um efeito de estado (ou com
        // inheritTargetVisibility) acompanha a visibilidade do alvo: nunca revela unidade escondida para o inimigo.
        public static bool Visible(bool ownerOnly,bool viewerIsAllyOfOwner,bool inheritTargetVisibility,bool targetHidden,bool viewerIsAllyOfTarget)
        {
            if(ownerOnly&&!viewerIsAllyOfOwner) return false;
            if(inheritTargetVisibility&&targetHidden&&!viewerIsAllyOfTarget) return false;
            return true;
        }
        public static bool Visible(string resource,bool viewerIsAllyOfOwner,bool targetHidden,bool viewerIsAllyOfTarget)
        {
            var e=Entrada(resource);
            bool state=resource!=null&&resource.Contains("/Estados/");
            return Visible(e!=null&&e.ownerOnly,viewerIsAllyOfOwner,state||(e!=null&&e.inheritTargetVisibility),targetHidden,viewerIsAllyOfTarget);
        }
    }

    // Troca os quadros de um SpriteRenderer em laço (terreno lava/gelo/fogos, loops de evento e de objeto).
    public sealed class SpriteLoop : MonoBehaviour
    {
        public Sprite[] frames; public float fps=10; public bool billboard; SpriteRenderer sr; float t;
        void Awake() { sr=GetComponent<SpriteRenderer>(); }
        void Update()
        {
            if(billboard&&Camera.main) transform.rotation=Camera.main.transform.rotation;
            if(frames==null||frames.Length==0||!sr) return;
            t+=Time.deltaTime; sr.sprite=frames[(int)(t*fps)%frames.Length];
        }
    }
}
