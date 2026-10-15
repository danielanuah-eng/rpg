using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;

namespace DungeonBDX
{
    public enum DungeonTab { Heroes, Campaign, Session }

    public sealed class DungeonShell : MonoBehaviour
    {
        static readonly Color Background = Hex("100F14"), Panel = Hex("211E27"), Card = Hex("2E2934");
        static readonly Color Bronze = Hex("C59B65"), Wine = Hex("70404F"), Ink = Hex("F2E5D1"), Muted = Hex("B8ACB6");
        readonly Dictionary<string, Sprite> portraits = new Dictionary<string, Sprite>();
        readonly List<DungeonMotion> nav = new List<DungeonMotion>();
        IReadOnlyList<DungeonHero> heroes;
        Canvas canvas;
        RectTransform safe, body, hud;
        CanvasGroup transition;
        TMP_FontAsset fallbackFont;
        TextMeshProUGUI breadcrumb;
        EventSystem ownedEvents;
        string selected, feedback = "Escolha seus heróis para a expedição.";
        int heroPage, skillPage, selectedSkill = -1;
        int lastWidth, lastHeight;
        Rect lastSafe;
        bool portrait;

        public DungeonSetup Setup { get; private set; }
        public DungeonTab CurrentTab { get; private set; }
        public string SelectedHeroId => selected;
        public int HeroCount => heroes?.Count ?? 0;

        void Awake() => Initialize();

        public void Initialize()
        {
            if (canvas) return;
            heroes = DungeonHeroCatalog.ImportArena();
            Setup = new DungeonSetup(heroes);
            selected = heroes.FirstOrDefault()?.Id;
            foreach (var hero in heroes) portraits[hero.Id] = hero.LoadPortrait();
            // Keep URP's render loop active in the standalone selection scene.
            var view = new GameObject("DungeonBDX Camera", typeof(Camera));
            view.transform.SetParent(transform, false); view.transform.localPosition = new Vector3(0, 0, -10);
            var camera = view.GetComponent<Camera>(); camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Background; camera.orthographic = true;
            var go = new GameObject("DungeonBDX Canvas", typeof(RectTransform), typeof(Canvas),
                typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            go.transform.SetParent(transform, false);
            canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
            if (!EventSystem.current)
            {
                var events = new GameObject("DungeonBDX Events", typeof(EventSystem), typeof(InputSystemUIInputModule));
                events.transform.SetParent(transform, false); ownedEvents = events.GetComponent<EventSystem>();
            }
            Rebuild();
        }

        void Update()
        {
            if (!canvas) return;
            if (lastWidth != Screen.width || lastHeight != Screen.height || lastSafe != Screen.safeArea) Rebuild();
            if (transition) transition.alpha = Mathf.Lerp(transition.alpha, 1, DungeonMotion.Blend(12, Time.unscaledDeltaTime));
        }

        void Rebuild()
        {
            lastWidth = Screen.width; lastHeight = Screen.height; lastSafe = Screen.safeArea;
            portrait = Screen.width < Screen.height;
            canvas.GetComponent<UnityEngine.UI.CanvasScaler>().referenceResolution = portrait
                ? new Vector2(720, 1280) : new Vector2(1280, 800);
            Clear(canvas.transform); nav.Clear();
            var bg = Box("Backdrop", canvas.transform, Vector2.zero, Vector2.one, Background);
            var art = bg.GetComponent<UnityEngine.UI.Image>();
            art.sprite = Resources.Load<Sprite>("Sprites/Menu/mapa-bosque");
            art.color = art.sprite ? new Color(.25f, .22f, .3f, 1) : Background;
            Box("Shade", canvas.transform, Vector2.zero, Vector2.one, new Color(.04f, .03f, .06f, .65f));
            safe = Box("Safe Area", canvas.transform, Vector2.zero, Vector2.one, Color.clear);
            ApplySafeArea(safe, Screen.safeArea, Screen.width, Screen.height);
            var header = Box("Header", safe, new Vector2(0, .89f), Vector2.one, Panel);
            Label(header, "DUNGEON<size=70%>BDX</size>", new Vector2(.02f, .38f), new Vector2(.48f, 1), portrait ? 32 : 37, Bronze);
            Label(header, "CRÔNICAS DO ABISMO", new Vector2(.02f, .04f), new Vector2(.7f, .4f), 13, Muted);
            breadcrumb = Label(header, "", new Vector2(.65f, .05f), new Vector2(.98f, .95f), 13, Muted, TextAlignmentOptions.Right);
            var tabs = Box("Tabs", safe, new Vector2(.015f, .815f), new Vector2(.985f, .885f), Color.clear);
            string[] names = { "01  HERÓIS", "02  CAMPANHA", "03  SESSÃO" };
            for (int i = 0; i < names.Length; i++)
            {
                var tab = (DungeonTab)i;
                var button = Button(tabs, names[i], new Vector2(i / 3f, 0), new Vector2((i + 1) / 3f, 1), () => ShowTab(tab));
                nav.Add(button.GetComponent<DungeonMotion>());
            }
            float hudTop = portrait ? .22f : .23f;
            body = Box("Page", safe, new Vector2(.015f, hudTop + .01f), new Vector2(.985f, .805f), Color.clear);
            hud = Box("Bottom Action Bar", safe, new Vector2(.015f, .01f), new Vector2(.985f, hudTop), Panel);
            RenderPage();
        }

        public static void ApplySafeArea(RectTransform target, Rect area, int width, int height)
        {
            if (width < 1 || height < 1) return;
            target.anchorMin = new Vector2(area.xMin / width, area.yMin / height);
            target.anchorMax = new Vector2(area.xMax / width, area.yMax / height);
            target.offsetMin = target.offsetMax = Vector2.zero;
        }

        public void ShowTab(DungeonTab tab)
        {
            if (!Enum.IsDefined(typeof(DungeonTab), tab)) throw new ArgumentOutOfRangeException(nameof(tab));
            CurrentTab = tab; RenderPage();
        }

        public bool SelectHero(string id)
        {
            if (!heroes.Any(h => h.Id == id)) return false;
            selected = id; skillPage = 0; selectedSkill = -1; RenderPage(); return true;
        }

        void RenderPage()
        {
            Clear(body); Clear(hud);
            breadcrumb.text = "PRÉVIA  /  " + (CurrentTab == DungeonTab.Heroes ? "HERÓIS" : CurrentTab == DungeonTab.Campaign ? "CAMPANHA" : "SESSÃO");
            for (int i = 0; i < nav.Count; i++) nav[i].Chosen = i == (int)CurrentTab;
            transition = body.GetComponent<CanvasGroup>();
            if (!transition) transition = body.gameObject.AddComponent<CanvasGroup>();
            transition.alpha = Application.isPlaying ? .15f : 1;
            switch (CurrentTab)
            {
                case DungeonTab.Heroes: HeroesPage(); break;
                case DungeonTab.Campaign: CampaignPage(); break;
                case DungeonTab.Session: SessionPage(); break;
            }
            ActionBar();
        }

        DungeonHero Selected => heroes.FirstOrDefault(h => h.Id == selected);

        void HeroesPage()
        {
            var roster = Box("Roster", body, portrait ? new Vector2(0, .49f) : Vector2.zero,
                portrait ? Vector2.one : new Vector2(.56f, 1), Panel);
            Label(roster, "O CHAMADO DOS HERÓIS", new Vector2(.025f, .86f), new Vector2(.975f, .99f), 19, Bronze);
            int columns = portrait ? 4 : 4, perPage = portrait ? 4 : 8;
            int pages = Mathf.Max(1, Mathf.CeilToInt(heroes.Count / (float)perPage));
            heroPage = Mathf.Clamp(heroPage, 0, pages - 1);
            for (int i = 0; i < perPage && heroPage * perPage + i < heroes.Count; i++)
            {
                var hero = heroes[heroPage * perPage + i];
                int row = i / columns, col = i % columns;
                float rows = portrait ? 1 : 2;
                var tile = Button(roster, "", new Vector2(.015f + col * .97f / columns, .15f + (rows - 1 - row) * .69f / rows),
                    new Vector2(.015f + (col + 1) * .97f / columns, .15f + (rows - row) * .69f / rows), () => SelectHero(hero.Id));
                tile.name = "Hero " + hero.Id;
                tile.GetComponent<DungeonMotion>().Chosen = hero.Id == selected;
                var visual = tile.transform.GetChild(0);
                Portrait(visual, hero, new Vector2(.05f, .22f), new Vector2(.95f, .95f));
                Label(visual, hero.Name, new Vector2(.025f, .015f), new Vector2(.975f, .24f), portrait ? 15 : 17, Ink, TextAlignmentOptions.Center);
                if (Setup.Party.Contains(hero.Id)) Label(visual, "+", new Vector2(.77f, .72f), Vector2.one, 20, Bronze, TextAlignmentOptions.Center);
            }
            Button(roster, "‹", new Vector2(.02f, .01f), new Vector2(.18f, .14f), () => { heroPage = (heroPage + pages - 1) % pages; RenderPage(); });
            Label(roster, $"{heroPage + 1} / {pages}   •   {heroes.Count} HERÓIS", new Vector2(.2f, .01f), new Vector2(.8f, .14f), 13, Muted, TextAlignmentOptions.Center);
            Button(roster, "›", new Vector2(.82f, .01f), new Vector2(.98f, .14f), () => { heroPage = (heroPage + 1) % pages; RenderPage(); });

            var detail = Box("Hero Card", body, portrait ? Vector2.zero : new Vector2(.575f, 0),
                portrait ? new Vector2(1, .475f) : Vector2.one, Panel);
            var heroSelected = Selected;
            if (heroSelected == null) { Label(detail, "Nenhum herói disponível.", Vector2.zero, Vector2.one, 22, Ink); return; }
            Box("Card Accent", detail, new Vector2(0, .975f), Vector2.one, Bronze);
            Label(detail, "FICHA DO HERÓI", new Vector2(.04f, .87f), new Vector2(.96f, .98f), 12, Bronze);
            Label(detail, heroSelected.Name, new Vector2(.04f, .75f), new Vector2(.96f, .89f), 30, Ink);
            Portrait(detail, heroSelected, new Vector2(.02f, .23f), new Vector2(.38f, .77f));
            Label(detail, $"VIDA  <b>{heroSelected.Health}</b>     PA  <b>{heroSelected.Actions}</b>\nMOV  <b>{heroSelected.Movement}</b>      ARM BASE  <b>{heroSelected.Armor}</b>",
                new Vector2(.4f, .53f), new Vector2(.97f, .76f), 18, Bronze);
            string description = selectedSkill >= 0 && selectedSkill < heroSelected.Skills.Count
                ? heroSelected.Skills[selectedSkill].Description
                : "Selecione uma habilidade na barra inferior para consultar a ficha de origem.";
            Label(detail, description, new Vector2(.4f, .19f), new Vector2(.96f, .53f), 17, Muted);
            string caption = Setup.Party.Contains(selected) ? "REMOVER DA EXPEDIÇÃO" : "RECRUTAR HERÓI";
            Button(detail, caption, new Vector2(.04f, .025f), new Vector2(.96f, .18f), ToggleSelected);
        }

        void ToggleSelected()
        {
            bool changed = Setup.ToggleHero(selected);
            feedback = changed ? $"Expedição: {Setup.Party.Count}/{Setup.PartyCapacity} heróis."
                : $"Expedição completa. Remova um herói para trocar ({Setup.PartyCapacity} vagas).";
            RenderPage();
        }

        void CampaignPage()
        {
            var panel = Box("Campaign Setup", body, Vector2.zero, Vector2.one, Panel);
            Label(panel, "AS REGRAS DA SUA CRÔNICA", new Vector2(.04f, .8f), new Vector2(.96f, .97f), 25, Bronze);
            Label(panel, "Escolha os módulos da campanha. Configuração local de preparação.", new Vector2(.04f, .69f), new Vector2(.96f, .82f), 17, Muted);
            CampaignMechanics[] flags = { CampaignMechanics.Objectives, CampaignMechanics.Traps, CampaignMechanics.FriendlyFire, CampaignMechanics.DragonEncounters };
            string[] names = { "Objetivos de cenário", "Armadilhas da masmorra", "Fogo amigo", "Encontros com dragões" };
            for (int i = 0; i < flags.Length; i++)
            {
                var flag = flags[i]; bool enabled = Setup.HasMechanic(flag);
                float top = .67f - i * .145f;
                var b = Button(panel, (enabled ? "[x]  ATIVO    " : "[ ]  INATIVO    ") + names[i],
                    new Vector2(.04f, top - .13f), new Vector2(.96f, top), () => { Setup.SetMechanic(flag, !Setup.HasMechanic(flag)); RenderPage(); });
                b.GetComponent<DungeonMotion>().Chosen = enabled;
            }
        }

        void SessionPage()
        {
            var panel = Box("Session Preview", body, Vector2.zero, Vector2.one, Panel);
            Label(panel, "À PORTA DA MASMORRA", new Vector2(.06f, .7f), new Vector2(.94f, .94f), 30, Bronze);
            Label(panel, "Contas protegidas • Salas privadas • Convites no jogo", new Vector2(.06f, .52f), new Vector2(.94f, .72f), 22, Ink);
            Label(panel, "Os serviços online ainda não estão disponíveis nesta prévia.\n\nA seleção de heróis e os módulos da campanha podem ser preparados localmente.",
                new Vector2(.06f, .14f), new Vector2(.94f, .52f), 21, Muted);
        }

        void ActionBar()
        {
            string partyNames = Setup.Party.Count == 0 ? "Nenhum herói recrutado" : string.Join("  /  ", Setup.Party.Select(id => heroes.First(h => h.Id == id).Name));
            Label(hud, $"EXPEDIÇÃO  {Setup.Party.Count}/{Setup.PartyCapacity}   ·   {partyNames}", new Vector2(.025f, .74f), new Vector2(.975f, .98f), 16, Bronze);
            var hero = Selected;
            int perPage = portrait ? 3 : 4;
            int pages = Mathf.Max(1, Mathf.CeilToInt((hero?.Skills.Count ?? 0) / (float)perPage));
            skillPage = Mathf.Clamp(skillPage, 0, pages - 1);
            if (hero != null)
            {
                int visible = Mathf.Min(perPage, hero.Skills.Count - skillPage * perPage);
                float actionStart = .08f + (perPage - visible) * .42f / perPage;
                Button(hud, "‹", new Vector2(.015f, .28f), new Vector2(.075f, .7f), () => { skillPage = (skillPage + pages - 1) % pages; RenderPage(); });
                for (int i = 0; i < perPage; i++)
                {
                    int index = skillPage * perPage + i;
                    if (index >= hero.Skills.Count) break;
                    var action = hero.Skills[index];
                    var b = Button(hud, $"{index + 1}  {action.Name}\n<size=80%>{action.Cost} PA · CONSULTAR</size>",
                        new Vector2(actionStart + i * .84f / perPage, .28f), new Vector2(actionStart + (i + 1) * .84f / perPage, .7f),
                        () => { selectedSkill = index; CurrentTab = DungeonTab.Heroes; RenderPage(); });
                    b.name = "Action " + action.Id; b.GetComponent<DungeonMotion>().Chosen = selectedSkill == index;
                }
                Button(hud, "›", new Vector2(.925f, .28f), new Vector2(.985f, .7f), () => { skillPage = (skillPage + 1) % pages; RenderPage(); });
            }
            Label(hud, feedback + $"   •   Habilidades {skillPage + 1}/{pages}", new Vector2(.025f, .01f), new Vector2(.975f, .25f), 14, Muted, TextAlignmentOptions.Center);
        }

        void Portrait(Transform parent, DungeonHero hero, Vector2 min, Vector2 max)
        {
            var frame = Box("Portrait " + hero.Id, parent, min, max, Color.clear);
            if (portraits.TryGetValue(hero.Id, out var sprite) && sprite)
            {
                var img = frame.GetComponent<UnityEngine.UI.Image>(); img.sprite = sprite;
                img.color = Color.white; img.preserveAspect = true;
            }
            else Label(frame, hero.Name.Substring(0, 1), Vector2.zero, Vector2.one, 42, Bronze, TextAlignmentOptions.Center);
        }

        RectTransform Button(Transform parent, string text, Vector2 min, Vector2 max, Action click)
        {
            var hit = Box("Button " + text, parent, min, max, Color.clear);
            hit.offsetMin = new Vector2(5, 5); hit.offsetMax = new Vector2(-5, -5);
            var image = hit.GetComponent<UnityEngine.UI.Image>(); image.raycastTarget = true;
            var button = hit.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = image; button.transition = UnityEngine.UI.Selectable.Transition.None;
            button.onClick.AddListener(() => click());
            var visual = Box("Visual", hit, Vector2.zero, Vector2.one, Card);
            var motion = hit.gameObject.AddComponent<DungeonMotion>(); motion.Configure(visual, Card, Wine);
            Label(visual, text, new Vector2(.03f, .03f), new Vector2(.97f, .97f), 18, Ink, TextAlignmentOptions.Center);
            return hit;
        }

        TextMeshProUGUI Label(Transform parent, string text, Vector2 min, Vector2 max, float size, Color color,
            TextAlignmentOptions align = TextAlignmentOptions.Left)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            var rect = (RectTransform)go.transform; rect.SetParent(parent, false);
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var label = go.GetComponent<TextMeshProUGUI>();
            var font = TMP_Settings.instance ? TMP_Settings.defaultFontAsset : null;
            if (!font)
            {
                if (!fallbackFont) fallbackFont = TMP_FontAsset.CreateFontAsset(Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));
                font = fallbackFont;
            }
            label.font = font; label.text = text; label.fontSize = size; label.color = color;
            label.alignment = align;
            label.enableAutoSizing = true; label.fontSizeMin = Mathf.Min(12, size); label.fontSizeMax = size;
            label.raycastTarget = false; label.overflowMode = TextOverflowModes.Ellipsis;
            return label;
        }

        static RectTransform Box(string name, Transform parent, Vector2 min, Vector2 max, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            var rect = (RectTransform)go.transform; rect.SetParent(parent, false);
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var img = go.GetComponent<UnityEngine.UI.Image>(); img.color = color; img.raycastTarget = false;
            return rect;
        }

        static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out var c); return c; }
        static void DestroyObject(UnityEngine.Object obj)
        {
            if (Application.isPlaying) Destroy(obj); else DestroyImmediate(obj);
        }
        static void Clear(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                var child = parent.GetChild(i); child.gameObject.SetActive(false); DestroyObject(child.gameObject);
            }
        }

        void OnDestroy()
        {
            if (ownedEvents) DestroyObject(ownedEvents.gameObject);
            if (fallbackFont)
            {
                foreach (var atlas in fallbackFont.atlasTextures) if (atlas) DestroyObject(atlas);
                if (fallbackFont.material) DestroyObject(fallbackFont.material);
                DestroyObject(fallbackFont);
            }
        }

        IEnumerator Start()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var args = Environment.GetCommandLineArgs();
            int capture = Array.IndexOf(args, "-dungeonbdx-capture");
            if (capture >= 0 && capture + 1 < args.Length)
            {
                int tab = Array.IndexOf(args, "-dungeonbdx-tab");
                if (tab >= 0 && tab + 1 < args.Length && Enum.TryParse<DungeonTab>(args[tab + 1], true, out var requested)) ShowTab(requested);
                for (int i = 0; i < 30; i++) yield return null;
                try { CapturePreview(args[capture + 1]); Application.Quit(); }
                catch (Exception error) { Debug.LogException(error); Application.Quit(1); }
            }
#endif
            yield break;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        void CapturePreview(string path)
        {
            var camera = GetComponentInChildren<Camera>();
            var output = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var previousMode = canvas.renderMode;
            var previousCamera = canvas.worldCamera;
            Texture2D pixels = null;
            try
            {
                camera.targetTexture = output;
                canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
                Canvas.ForceUpdateCanvases();
                if (GraphicsSettings.currentRenderPipeline)
                {
                    var request = new RenderPipeline.StandardRequest { destination = output };
                    if (!RenderPipeline.SupportsRenderRequest(camera, request)) throw new InvalidOperationException("Pipeline sem suporte à captura da prévia.");
                    camera.SubmitRenderRequest(request);
                }
                else camera.Render();
                RenderTexture.active = output;
                pixels = new Texture2D(output.width, output.height, TextureFormat.RGB24, false);
                pixels.ReadPixels(new Rect(0, 0, output.width, output.height), 0, 0); pixels.Apply();
                File.WriteAllBytes(path, pixels.EncodeToPNG());
                Debug.Log("DUNGEONBDX_CAPTURE_OK " + output.width + "x" + output.height);
            }
            finally
            {
                camera.targetTexture = previousTarget; RenderTexture.active = previousActive;
                canvas.renderMode = previousMode; canvas.worldCamera = previousCamera;
                if (pixels) DestroyObject(pixels);
                RenderTexture.ReleaseTemporary(output);
            }
        }
#endif
    }
}
