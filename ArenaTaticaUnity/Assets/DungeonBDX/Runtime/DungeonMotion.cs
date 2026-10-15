using UnityEngine;
using UnityEngine.EventSystems;

namespace DungeonBDX
{
    // The hit target remains still; only its visual child rises, avoiding hover oscillation.
    public sealed class DungeonMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        ISelectHandler, IDeselectHandler, IPointerDownHandler, IPointerUpHandler
    {
        RectTransform visual;
        UnityEngine.UI.Image image;
        UnityEngine.UI.Selectable button;
        Color idle, accent;
        bool hovered, focused, pressed, chosen;
        public bool Chosen { get => chosen; set => chosen = value; }

        public static float Blend(float speed, float delta) => 1f - Mathf.Exp(-Mathf.Max(0, speed) * Mathf.Max(0, delta));

        public void Configure(RectTransform target, Color normal, Color active)
        {
            visual = target; image = visual.GetComponent<UnityEngine.UI.Image>();
            button = GetComponent<UnityEngine.UI.Selectable>(); idle = normal; accent = active;
        }

        void Update()
        {
            if (!visual) return;
            bool enabled = !button || button.IsInteractable();
            bool active = enabled && (hovered || focused || chosen);
            float blend = Blend(15, Time.unscaledDeltaTime);
            visual.anchoredPosition = Vector2.Lerp(visual.anchoredPosition,
                new Vector2(0, enabled && pressed ? -2 : active ? 5 : 0), blend);
            visual.localScale = Vector3.Lerp(visual.localScale,
                Vector3.one * (enabled && pressed ? .98f : active ? 1.015f : 1), blend);
            Color target = enabled && chosen ? accent : active ? Color.Lerp(idle, accent, .45f) : idle;
            if (image) image.color = Color.Lerp(image.color, target, blend);
        }

        void OnDisable()
        {
            hovered = focused = pressed = false;
            if (visual) { visual.anchoredPosition = Vector2.zero; visual.localScale = Vector3.one; }
            if (image) image.color = idle;
        }
        public void OnPointerEnter(PointerEventData data) => hovered = true;
        public void OnPointerExit(PointerEventData data) { hovered = false; pressed = false; }
        public void OnSelect(BaseEventData data) => focused = true;
        public void OnDeselect(BaseEventData data) { focused = false; pressed = false; }
        public void OnPointerDown(PointerEventData data) { if (data.button == PointerEventData.InputButton.Left) pressed = true; }
        public void OnPointerUp(PointerEventData data) => pressed = false;
    }
}
