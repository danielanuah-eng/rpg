using UnityEngine;
using UnityEngine.UI;

namespace ArenaTatica
{
    // Chibi da L.E.L.A no canto da tela (autor 22/10): cada clique passa ao próximo modo de som, que fica salvo (PlayerPrefs).
    // Novo (cantando): trilhas gravadas, vozes dos personagens e ambiente de RPG. Clássico (dois dedos na testa, piscando):
    // o som de antes, só a trilha sintetizada dos Modos. Mudo (X na boca): sem música e sem vozes.
    public class ArenaSom : MonoBehaviour
    {
        public enum Modo { Novo, Classico, Mudo }
        const string Key="som-modo";
        public static Modo Atual { get { try { return (Modo)Mathf.Clamp(PlayerPrefs.GetInt(Key,0),0,2); } catch { return Modo.Novo; } } }
        public static bool On => Atual!=Modo.Mudo;
        // Trilhas gravadas, ambiente e vozes das skills.
        public static bool Recorded => Atual==Modo.Novo;
        static readonly string[] Faces={"lela-cantando","lela-classico","lela-muda"};
        static readonly string[] Tips={"Som novo","Som clássico","Sem som"};
        Image face; Text tip; readonly Sprite[] sprites=new Sprite[3]; RectTransform rect; Canvas canvas; float tipUntil;

        void Start()
        {
            for(int i=0;i<3;i++) sprites[i]=Load(Faces[i]);
            var go=new GameObject("SomLela",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            go.transform.SetParent(transform,false); canvas=go.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=150;
            var btn=new GameObject("Chibi",typeof(RectTransform),typeof(Image),typeof(Button)); btn.transform.SetParent(go.transform,false);
            rect=btn.GetComponent<RectTransform>(); rect.anchorMin=rect.anchorMax=rect.pivot=Vector2.one; rect.sizeDelta=new Vector2(48,48);
            rect.anchoredPosition=new Vector2(-8,-(ArenaHud.TopBar+ArenaHud.TimelineBar+8));
            face=btn.GetComponent<Image>(); face.preserveAspect=true;
            btn.GetComponent<Button>().onClick.AddListener(Next);
            tip=new GameObject("Dica",typeof(RectTransform),typeof(Text)).GetComponent<Text>(); tip.transform.SetParent(btn.transform,false);
            tip.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); tip.fontSize=12; tip.fontStyle=FontStyle.Bold; tip.alignment=TextAnchor.UpperCenter; tip.color=Color.white; tip.raycastTarget=false;
            tip.horizontalOverflow=HorizontalWrapMode.Overflow; var tr=tip.rectTransform; tr.anchorMin=tr.anchorMax=new Vector2(.5f,0); tr.pivot=new Vector2(.5f,1); tr.sizeDelta=new Vector2(0,16); tr.anchoredPosition=new Vector2(0,-2);
            tip.gameObject.AddComponent<Outline>().effectColor=Color.black;
            Refresh();
        }
        static Sprite Load(string name)
        {
            var tex=Resources.Load<Texture2D>("Sprites/UI/"+name); if(!tex) return null;
            tex.filterMode=FilterMode.Point; return Sprite.Create(tex,new Rect(0,0,tex.width,tex.height),new Vector2(.5f,.5f),100);
        }
        void Next() { try { PlayerPrefs.SetInt(Key,((int)Atual+1)%3); PlayerPrefs.Save(); } catch { } tipUntil=Time.unscaledTime+1.6f; Refresh(); }
        void Refresh()
        {
            if(!face) return; int m=(int)Atual; face.sprite=sprites[m];
            face.color=face.sprite?Color.white:new[]{new Color(.6f,.4f,1),new Color(.4f,.7f,1),new Color(.4f,.4f,.4f)}[m];
            tip.text=Tips[m];
        }
        void Update()
        {
            if(!canvas) return;
            // Acima do menu e do HUD; some só na tela de carregamento (sem câmera).
            canvas.GetComponent<CanvasScaler>().scaleFactor=ArenaHud.UiScale;
            tip.enabled=Time.unscaledTime<tipUntil;
            // Cantando: balança no ritmo. Clássico: leve aceno. Mudo: parada.
            var m=Atual; float w=Time.unscaledTime*6;
            rect.localRotation=Quaternion.Euler(0,0,m==Modo.Novo?Mathf.Sin(w)*6:m==Modo.Classico?Mathf.Sin(w*.4f)*2:0);
            rect.localScale=Vector3.one*(m==Modo.Novo?1+.04f*Mathf.Abs(Mathf.Sin(w)):1);
        }
    }
}
