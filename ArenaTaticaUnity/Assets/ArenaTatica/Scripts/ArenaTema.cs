using UnityEngine;
using UnityEngine.UI;

namespace ArenaTatica
{
    // Tokens do modelo de tela da entrega 017 (ui/modelo-tela.visual.json). O dourado é só acento de interface:
    // TeamColor/PlayerColors continuam sendo as cores das equipes.
    public static class ArenaTema
    {
        public static readonly Color Background=Hex(0x10151d), Panel=Hex(0x19222e), Raised=Hex(0x202c3a), Border=Hex(0x344252),
            Ink=Hex(0xede7d9), Muted=Hex(0xa7b4c2), Selected=Hex(0xe8c278), SelectedFill=Hex(0x4d3d1f), Healing=Hex(0x8cccb3), Hp=Hex(0xea9d98), Pm=Hex(0xa3cae6);
        public const int Radius=14, FocusOutline=3;
        public static string Html(Color c) => ColorUtility.ToHtmlStringRGB(c);
        public static Color Hex(int rgb) => new Color((rgb>>16&255)/255f,(rgb>>8&255)/255f,(rgb&255)/255f);

        // Avanço da seleção de equipe: só com pelo menos 1 lutador.
        public static bool CanAdvance(int teamCount) => teamCount>0;
        public static string AdvanceLabel(int teamCount,string ready) => CanAdvance(teamCount)?ready:"Escolha um lutador";

        // Seleção por borda dourada + fundo + selo "OK" (não só por cor).
        public static void MarkSelected(Button b,bool on,Font font)
        {
            var img=b.GetComponent<Image>(); if(img) img.color=on?SelectedFill:Raised;
            var outline=b.GetComponent<Outline>(); if(!outline) { outline=b.gameObject.AddComponent<Outline>(); outline.effectColor=Selected; outline.effectDistance=new Vector2(FocusOutline,-FocusOutline); }
            outline.enabled=on;
            var badge=b.transform.Find("Selo");
            if(!badge) {
                var go=new GameObject("Selo",typeof(RectTransform),typeof(Text)); go.transform.SetParent(b.transform,false);
                var t=go.GetComponent<Text>(); t.font=font; t.fontSize=13; t.fontStyle=FontStyle.Bold; t.color=Selected; t.text="OK"; t.alignment=TextAnchor.UpperRight; t.raycastTarget=false;
                var r=t.rectTransform; r.anchorMin=r.anchorMax=Vector2.one; r.pivot=Vector2.one; r.anchoredPosition=new Vector2(-6,-4); r.sizeDelta=new Vector2(36,20);
                badge=go.transform;
            }
            badge.gameObject.SetActive(on);
        }
        // Foco de teclado visível (Tab/setas): tinge o botão selecionado pelo EventSystem.
        public static void StyleFocus(Button b)
        {
            var c=b.colors; c.selectedColor=Hex(0x3a4a5e); c.highlightedColor=Hex(0x2c3b4d); c.pressedColor=Selected; b.colors=c;
        }
        // Botões com transição suave (autor, 13/10: LERP em todos): a cor desliza (fadeDuration) e a escala cresce sob o cursor
        // e afunda ao apertar. A escala não mexe no layout.
        public static Button Lerp(Button b)
        {
            if(!b) return b;
            var c=b.colors; c.fadeDuration=.16f; b.colors=c;
            if(!b.GetComponent<ArenaBotaoLerp>()) b.gameObject.AddComponent<ArenaBotaoLerp>();
            return b;
        }
    }
    public sealed class ArenaBotaoLerp : MonoBehaviour, UnityEngine.EventSystems.IPointerEnterHandler, UnityEngine.EventSystems.IPointerExitHandler,
        UnityEngine.EventSystems.IPointerDownHandler, UnityEngine.EventSystems.IPointerUpHandler
    {
        public const float Hover=1.045f, Press=.94f, Speed=18;
        bool over, down; Selectable target;
        void Awake() { target=GetComponent<Selectable>(); }
        public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData e) { over=true; }
        public void OnPointerExit(UnityEngine.EventSystems.PointerEventData e) { over=false; down=false; }
        public void OnPointerDown(UnityEngine.EventSystems.PointerEventData e) { down=true; }
        public void OnPointerUp(UnityEngine.EventSystems.PointerEventData e) { down=false; }
        void OnDisable() { over=down=false; transform.localScale=Vector3.one; }
        public static float Goal(bool interactable,bool over,bool down) => !interactable?1:down?Press:over?Hover:1;
        void Update()
        {
            float goal=Goal(!target||target.interactable,over,down), s=transform.localScale.x;
            if(Mathf.Abs(s-goal)<.0005f) return;
            s=Mathf.Lerp(s,goal,1-Mathf.Exp(-Speed*Time.unscaledDeltaTime)); transform.localScale=new Vector3(s,s,1);
        }
    }
    // Dica ao passar o cursor (não bloqueia a rolagem como o EventTrigger).
    public sealed class ArenaDica : MonoBehaviour, UnityEngine.EventSystems.IPointerEnterHandler
    {
        public System.Action Enter;
        public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData e) => Enter?.Invoke();
    }
}
