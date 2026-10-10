using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using ArenaTatica.Core;

namespace ArenaTatica
{
    public sealed class ArenaHud : MonoBehaviour
    {
        public ArenaGame Game;
        Canvas canvas;
        RectTransform side,top,footer,networkPanel,logPanel,actions,shopGrid,shopWindow;
        Text turn,score,status,metrics,result,help,mobileStats,fighters,shopLabel,log,shopTitle,shopHelp;
        readonly List<Text> labels=new List<Text>();
        readonly List<Button> skillButtons=new List<Button>(), shopButtons=new List<Button>();
        // Figura de cada produto (Sprites/Loja/<id>, entregue pelo Codex) à esquerda do botão.
        readonly List<Image> shopIcons=new List<Image>();
        readonly Dictionary<string,Sprite> iconCache=new Dictionary<string,Sprite>();
        public Sprite ShopIcon(string id) { if(string.IsNullOrEmpty(id)) return null; if(!iconCache.TryGetValue(id,out var sp)) iconCache[id]=sp=Resources.Load<Sprite>("Sprites/Loja/"+id); return sp; }
        readonly List<string> logLines=new List<string>();
        Button move,pass,host,room,join,resume,seats,format,theme,start,openShop;
        InputField address;
        Font font;
        int width,height;
        readonly Color ink=new Color(.91f,.93f,.98f),muted=new Color(.65f,.7f,.8f),panel=new Color(.065f,.085f,.13f,.97f),idle=new Color(.14f,.19f,.27f),chosen=new Color(.45f,.29f,.16f);

        void Start()
        {
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var root=new GameObject("HUD",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            root.transform.SetParent(transform); canvas=root.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=root.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;
            if(!EventSystem.current) new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
            top=Panel("Cabecalho",root.transform); Anchors(top,new Vector2(0,1),Vector2.one,new Vector2(0,-82),Vector2.zero);
            var title=Text("ARENA TÁTICA",top,26,ArenaPresentation.Gold); Anchors(title.rectTransform,new Vector2(0,0),new Vector2(.43f,1),new Vector2(26,28),new Vector2(-4,-12));
            var subtitle=Text("MINIATURAS • ESTRATÉGIA • DUELO",top,11,muted); Anchors(subtitle.rectTransform,Vector2.zero,new Vector2(.5f,0),new Vector2(28,12),new Vector2(0,27));
            turn=Text("",top,17,ink); turn.alignment=TextAnchor.MiddleRight; Anchors(turn.rectTransform,new Vector2(.4f,.45f),Vector2.one,Vector2.zero,new Vector2(-26,-6));
            score=Text("",top,13,muted); score.alignment=TextAnchor.MiddleRight; Anchors(score.rectTransform,new Vector2(.4f,0),new Vector2(1,.45f),new Vector2(0,6),new Vector2(-26,0));
            side=Panel("Comandos",root.transform);
            var layout=side.gameObject.AddComponent<VerticalLayoutGroup>(); layout.padding=new RectOffset(16,16,12,12); layout.spacing=5; layout.childControlWidth=true; layout.childControlHeight=true; layout.childForceExpandHeight=false;
            labels.Add(LabelRow("LUTADORES",side,12,muted,18));
            fighters=LabelRow("",side,13,ink,112); labels.Add(fighters);
            labels.Add(LabelRow("AÇÕES",side,12,muted,18));
            actions=Grid("Acoes",side);
            move=Button("Mover",actions,()=>Game.Mode="");
            for(int i=0;i<4;i++) { int k=i; skillButtons.Add(Button("",actions,()=>ChooseSkill(k))); }
            pass=Button("Passar  [Espaço]",actions,()=>Game.Submit("passar"));
            shopLabel=LabelRow("LOJA",side,12,muted,18); labels.Add(shopLabel);
            shopGrid=Grid("Loja",side);
            openShop=Button("Abrir loja",shopGrid,()=>shopWindow.gameObject.SetActive(!shopWindow.gameObject.activeSelf));
            help=LabelRow("",side,12,muted,44); labels.Add(help);
            result=LabelRow("",side,16,ArenaPresentation.Gold,28);
            // Registro no fim do painel lateral, ocupando o que sobra: não cobre mais o tabuleiro.
            logPanel=Panel("Registro",side); logPanel.GetComponent<Image>().color=new Color(.03f,.04f,.07f,.6f);
            var logSize=logPanel.gameObject.AddComponent<LayoutElement>(); logSize.flexibleHeight=1; logSize.minHeight=40;
            log=Text("",logPanel,12,ink); log.verticalOverflow=VerticalWrapMode.Truncate; log.alignment=TextAnchor.LowerLeft; Anchors(log.rectTransform,Vector2.zero,Vector2.one,new Vector2(10,6),new Vector2(-10,-6));
            footer=Panel("Rodape",root.transform); Anchors(footer,Vector2.zero,new Vector2(1,0),Vector2.zero,new Vector2(0,62));
            status=Text("",footer,14,ink); Anchors(status.rectTransform,Vector2.zero,Vector2.one,new Vector2(22,26),new Vector2(-22,-6));
            metrics=Text("",footer,11,muted); Anchors(metrics.rectTransform,Vector2.zero,Vector2.one,new Vector2(22,7),new Vector2(-22,-33));
            networkPanel=Panel("Conexao",root.transform);
            var row=networkPanel.gameObject.AddComponent<HorizontalLayoutGroup>(); row.spacing=7; row.padding=new RectOffset(8,8,4,4); row.childControlHeight=true; row.childControlWidth=true; row.childForceExpandWidth=false;
            Button("Nova local",networkPanel,Game.NewLocal,84);
            Button("Lutadores",networkPanel,OpenPicker,84);
            seats=Button("",networkPanel,Game.CycleSeats,92);
            format=Button("",networkPanel,Game.CycleFormat,118);
            theme=Button("",networkPanel,Game.CycleTheme,128);
            host=Button("Host LAN",networkPanel,Game.Host,82);
            room=Button("Criar sala",networkPanel,Game.HostRoom,90);
            address=Input(networkPanel,"","Código da sala ou ws://IP:9080");
            join=Button("Entrar",networkPanel,()=>Game.Join(address.text),62);
            start=Button("Iniciar",networkPanel,Game.StartRoom,66);
            resume=Button("Retomar local",networkPanel,Game.ResumeLocal,104);
            fullscreen=Button("Tela cheia",networkPanel,ToggleFullScreen,88); fullscreen.gameObject.SetActive(!Application.isMobilePlatform);
            // Em 1280 px a barra não cabia (Retomar/Tela cheia saíam da tela): os botões encolhem até 60% e o texto quebra.
            foreach(var le in networkPanel.GetComponentsInChildren<LayoutElement>()) le.minWidth=le.preferredWidth>0?le.preferredWidth*.6f:100;
            mobileStats=Text("",root.transform,14,ink);
            Anchors(mobileStats.rectTransform,new Vector2(0,.87f),new Vector2(1,.935f),new Vector2(20,0),new Vector2(-20,0));
            BuildShopWindow(root.transform);
            BuildPicker(root.transform);
            Layout();
            // Ao abrir o jogo, escolher os lutadores (execuções automáticas por linha de comando pulam).
            if(!System.Environment.GetCommandLineArgs().Any(a=>a.StartsWith("--arena-"))) OpenPicker();
        }
        // Janela da loja: cenário do castor lojista, itens sobre o balcão e a descrição do item sob o cursor.
        void BuildShopWindow(Transform root)
        {
            shopWindow=Panel("JanelaLoja",root); shopWindow.GetComponent<Image>().color=new Color(.03f,.04f,.07f,.97f);
            var scene=new GameObject("Cena",typeof(RectTransform),typeof(Image)).GetComponent<Image>(); scene.transform.SetParent(shopWindow,false);
            // Cena animada do castor (Sprites/Vendedor/frame-NN, tarefa 004); sem ela, a imagem parada.
            vendor=Resources.LoadAll<Sprite>("Sprites/Vendedor").OrderBy(x=>x.name).ToArray(); shopScene=scene;
            scene.sprite=vendor.Length>0?vendor[0]:Resources.Load<Sprite>("Sprites/Cenarios/loja"); scene.preserveAspect=true; scene.raycastTarget=false; if(!scene.sprite) scene.color=Color.clear;
            Anchors(scene.rectTransform,new Vector2(0,.27f),Vector2.one,new Vector2(10,0),new Vector2(-10,-42));
            shopTitle=Text("",shopWindow,17,ArenaPresentation.Gold); shopTitle.alignment=TextAnchor.MiddleLeft;
            Anchors(shopTitle.rectTransform,new Vector2(0,1),Vector2.one,new Vector2(16,-38),new Vector2(-60,-6));
            var close=Button("✕",shopWindow,()=>shopWindow.gameObject.SetActive(false));
            Anchors((RectTransform)close.transform,Vector2.one,Vector2.one,new Vector2(-46,-38),new Vector2(-8,-6));
            var items=new GameObject("Itens",typeof(RectTransform),typeof(HorizontalLayoutGroup)).GetComponent<RectTransform>(); items.SetParent(shopWindow,false);
            var row=items.GetComponent<HorizontalLayoutGroup>(); row.spacing=8; row.childControlWidth=row.childControlHeight=true; row.childForceExpandWidth=true; row.childForceExpandHeight=true;
            Anchors(items,new Vector2(0,.13f),new Vector2(1,.26f),new Vector2(16,0),new Vector2(-16,0));
            for(int i=0;i<Shop.Slots;i++) {
                int k=i; var b=Button("",items,()=>Game.Submit("comprar",slot:k)); shopButtons.Add(b);
                var icon=new GameObject("Icone",typeof(RectTransform),typeof(Image)).GetComponent<Image>(); icon.transform.SetParent(b.transform,false); icon.preserveAspect=true; icon.raycastTarget=false;
                Anchors(icon.rectTransform,Vector2.zero,new Vector2(0,1),new Vector2(6,4),new Vector2(62,-4)); shopIcons.Add(icon);
                var label=b.GetComponentInChildren<Text>(); Anchors(label.rectTransform,Vector2.zero,Vector2.one,new Vector2(66,0),new Vector2(-4,0));
            }
            shopHelp=Text("",shopWindow,13,muted); shopHelp.alignment=TextAnchor.MiddleCenter; shopHelp.horizontalOverflow=HorizontalWrapMode.Wrap;
            Anchors(shopHelp.rectTransform,Vector2.zero,new Vector2(1,.13f),new Vector2(16,4),new Vector2(-16,0));
            shopWindow.gameObject.SetActive(false);
        }
        // Seletor de lutadores: cada assento local monta o time (Nível até ArenaGame.TeamLevel, sem repetir).
        // Em rede só o time deste aparelho, que vale na próxima sala.
        RectTransform picker; Text pickerTitle,pickerInfo; Button pickerNext; readonly List<Button> pickButtons=new List<Button>();
        readonly List<string> picking=new List<string>(); string[][] picked; int pickSeat;
        public bool PickerOpen => picker&&picker.gameObject.activeSelf;
        bool OnlineSession => Game.Network&&(Game.Network.IsHost||Game.Network.IsConnected);
        void BuildPicker(Transform root)
        {
            picker=Panel("Seletor",root); picker.GetComponent<Image>().color=new Color(.03f,.04f,.07f,.97f); Anchors(picker,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            pickerTitle=Text("",picker,22,ArenaPresentation.Gold); pickerTitle.alignment=TextAnchor.MiddleCenter; Anchors(pickerTitle.rectTransform,new Vector2(0,1),Vector2.one,new Vector2(16,-56),new Vector2(-16,-12));
            var grid=new GameObject("Lutadores",typeof(RectTransform),typeof(GridLayoutGroup)).GetComponent<RectTransform>(); grid.SetParent(picker,false);
            var g=grid.GetComponent<GridLayoutGroup>(); g.cellSize=new Vector2(150,150); g.spacing=new Vector2(10,10); g.childAlignment=TextAnchor.UpperCenter;
            Anchors(grid,Vector2.zero,Vector2.one,new Vector2(16,110),new Vector2(-16,-64));
            foreach(string id in Content.Roster) {
                var d=Content.Fighters[id]; var b=Button("",grid,()=>TogglePick(id)); pickButtons.Add(b);
                var icon=new GameObject("Retrato",typeof(RectTransform),typeof(Image)).GetComponent<Image>(); icon.transform.SetParent(b.transform,false); icon.preserveAspect=true; icon.raycastTarget=false;
                icon.sprite=d.sprites==null?null:Resources.Load<Sprite>("Sprites/"+d.sprites+"/south"); if(!icon.sprite) icon.color=Color.clear;
                Anchors(icon.rectTransform,new Vector2(0,.3f),Vector2.one,new Vector2(6,0),new Vector2(-6,-4));
                var label=b.GetComponentInChildren<Text>(); label.text=$"{d.name}\n<size=11>Nv {d.level} · {d.hp} PV · {d.pa} PA · {d.pm} PM</size>"; label.alignment=TextAnchor.LowerCenter;
                Anchors(label.rectTransform,Vector2.zero,new Vector2(1,.32f),new Vector2(4,4),new Vector2(-4,0));
            }
            pickerInfo=Text("",picker,15,ink); pickerInfo.alignment=TextAnchor.MiddleCenter; Anchors(pickerInfo.rectTransform,Vector2.zero,new Vector2(1,0),new Vector2(16,62),new Vector2(-16,104));
            var row=new GameObject("Botoes",typeof(RectTransform),typeof(HorizontalLayoutGroup)).GetComponent<RectTransform>(); row.SetParent(picker,false);
            var h=row.GetComponent<HorizontalLayoutGroup>(); h.spacing=10; h.childAlignment=TextAnchor.MiddleCenter; h.childControlWidth=h.childControlHeight=true; h.childForceExpandWidth=false;
            Anchors(row,Vector2.zero,new Vector2(1,0),new Vector2(16,14),new Vector2(-16,54));
            Button("Aleatório",row,RandomPick,120); Button("Limpar",row,()=>{ picking.Clear(); RefreshPicker(); },100);
            pickerNext=Button("",row,NextPick,160);
            picker.gameObject.SetActive(false);
        }
        public void OpenPicker()
        {
            shopWindow.gameObject.SetActive(false);
            pickSeat=0; picked=new string[OnlineSession?1:Game.Seats][];
            picking.Clear(); picking.AddRange(ArenaGame.MyTeam); picker.gameObject.SetActive(true); RefreshPicker();
        }
        void TogglePick(string id)
        {
            if(picking.Contains(id)) picking.Remove(id);
            else if(ArenaGame.ValidTeam(picking.Append(id)).Contains(id)) picking.Add(id);
            else Game.Notify(picking.Count>=ArenaGame.TeamSize?$"No máximo {ArenaGame.TeamSize} lutadores.":$"Passa do limite de Nível {ArenaGame.TeamLevel}.");
            RefreshPicker();
        }
        void RandomPick()
        {
            picking.Clear(); picking.AddRange(ArenaGame.ValidTeam(Content.Roster.OrderBy(_=>UnityEngine.Random.value))); RefreshPicker();
        }
        void NextPick()
        {
            if(picking.Count==0) { Game.Notify("Escolha pelo menos 1 lutador."); return; }
            picked[pickSeat++]=picking.ToArray();
            if(pickSeat<picked.Length) { picking.Clear(); picking.AddRange(Game.Picks!=null&&pickSeat<Game.Picks.Length?Game.Picks[pickSeat]:ArenaGame.MyTeam); RefreshPicker(); return; }
            picker.gameObject.SetActive(false);
            if(OnlineSession) { ArenaGame.MyTeam=picked[0]; Game.Notify("Time salvo: vale na próxima sala."); }
            else Game.StartLocal(picked);
        }
        void RefreshPicker()
        {
            int level=picking.Sum(id=>Content.Fighters[id].level);
            pickerTitle.text=OnlineSession?"SEU TIME":$"TIME DE P{pickSeat+1}  <size=15>({pickSeat+1} de {picked.Length})</size>";
            pickerInfo.text=(picking.Count==0?"Toque nos lutadores para montar o time.":string.Join("  +  ",picking.Select(id=>Content.Fighters[id].name)))+$"\n<size=13>Nível {level}/{ArenaGame.TeamLevel} · até {ArenaGame.TeamSize} lutadores · sem repetir</size>";
            for(int i=0;i<pickButtons.Count;i++) pickButtons[i].GetComponent<Image>().color=picking.Contains(Content.Roster[i])?chosen:idle;
            SetLabel(pickerNext,pickSeat+1<picked.Length?"Próximo jogador":"Começar");
        }
        void ChooseSkill(int index)
        {
            var f=Game.Active; if(f==null) return;
            var skills=Match.Skills(f); if(index<skills.Count) Game.Mode=Game.Mode==skills[index].id?"":skills[index].id;
        }
        RectTransform Panel(string name,Transform parent)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image)); go.transform.SetParent(parent,false); go.GetComponent<Image>().color=panel; return go.GetComponent<RectTransform>();
        }
        RectTransform Grid(string name,Transform parent)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(GridLayoutGroup),typeof(LayoutElement)); go.transform.SetParent(parent,false);
            var g=go.GetComponent<GridLayoutGroup>(); g.spacing=new Vector2(5,5); g.constraint=GridLayoutGroup.Constraint.FixedColumnCount; g.constraintCount=1;
            return go.GetComponent<RectTransform>();
        }
        Text Text(string value,Transform parent,int size,Color color)
        {
            var go=new GameObject("Texto",typeof(RectTransform),typeof(Text)); go.transform.SetParent(parent,false);
            var text=go.GetComponent<Text>(); text.text=value; text.font=font; text.fontSize=size; text.color=color; text.raycastTarget=false; text.supportRichText=true; text.verticalOverflow=VerticalWrapMode.Truncate; return text;
        }
        Text LabelRow(string value,Transform parent,int size,Color color,float h)
        {
            var t=Text(value,parent,size,color); var le=t.gameObject.AddComponent<LayoutElement>(); le.preferredHeight=h; le.minHeight=h; return t;
        }
        Button Button(string name,Transform parent,UnityEngine.Events.UnityAction action,float w=0)
        {
            var rect=Panel(name,parent); var image=rect.GetComponent<Image>(); image.color=idle;
            var b=rect.gameObject.AddComponent<Button>(); b.targetGraphic=image; var colors=b.colors; colors.highlightedColor=new Color(.82f,.9f,1); colors.pressedColor=ArenaPresentation.Gold; colors.disabledColor=new Color(.4f,.43f,.5f); b.colors=colors; b.onClick.AddListener(action);
            var text=Text(name,rect,13,ink); text.alignment=TextAnchor.MiddleCenter; text.horizontalOverflow=HorizontalWrapMode.Wrap; Anchors(text.rectTransform,Vector2.zero,Vector2.one,new Vector2(4,0),new Vector2(-4,0));
            var le=rect.gameObject.AddComponent<LayoutElement>(); le.preferredHeight=34; le.minHeight=30; if(w>0) { le.preferredWidth=w; le.minWidth=w; } return b;
        }
        static void SetLabel(Button b,string text) => b.GetComponentInChildren<Text>().text=text;
        InputField Input(Transform parent,string value,string hint)
        {
            var rect=Panel("Endereco",parent); var input=rect.gameObject.AddComponent<InputField>();
            var text=Text(value,rect,13,ink); text.alignment=TextAnchor.MiddleLeft; Anchors(text.rectTransform,Vector2.zero,Vector2.one,new Vector2(9,0),new Vector2(-9,0));
            var hintText=Text(hint,rect,13,muted); hintText.alignment=TextAnchor.MiddleLeft; Anchors(hintText.rectTransform,Vector2.zero,Vector2.one,new Vector2(9,0),new Vector2(-9,0));
            input.textComponent=text; input.placeholder=hintText; input.text=value; input.characterLimit=240;
            var le=rect.gameObject.AddComponent<LayoutElement>(); le.minWidth=150; le.flexibleWidth=1; return input;
        }
        static void Anchors(RectTransform rect,Vector2 min,Vector2 max,Vector2 offsetMin,Vector2 offsetMax) { rect.anchorMin=min; rect.anchorMax=max; rect.offsetMin=offsetMin; rect.offsetMax=offsetMax; }
        bool Portrait => width<height;
        // Escala da interface: a mesma do CanvasScaler (referência 1280×720, meio a meio), mas no PC não passa de 1,
        // para que em tela grande os botões não inchem e o espaço ganho vá para o tabuleiro.
        public static float UiScale => Mathf.Max(.5f,Mathf.Min(Application.isMobilePlatform?4:1,Mathf.Sqrt(Screen.width/1280f*Screen.height/720f)));
        // Área livre para o tabuleiro em pixels, entre os painéis montados em Layout (mesmas medidas).
        public static Rect BoardArea()
        {
            float k=UiScale,w=Screen.width,h=Screen.height;
            if(w<h) { float bottom=243*k,top=h*.81f-46*k; return new Rect(8*k,bottom,w-16*k,Mathf.Max(10,top-bottom)); }
            float left=304*k,low=70*k,high=h-134*k; return new Rect(left,low,Mathf.Max(10,w-left-12*k),Mathf.Max(10,high-low));
        }
        void Layout()
        {
            width=Screen.width; height=Screen.height; canvas.GetComponent<CanvasScaler>().scaleFactor=UiScale;
            mobileStats.gameObject.SetActive(Portrait); logPanel.gameObject.SetActive(!Portrait);
            foreach(var l in labels) l.gameObject.SetActive(!Portrait);
            var v=side.GetComponent<VerticalLayoutGroup>();
            if(Portrait) {
                Anchors(side,Vector2.zero,new Vector2(1,0),new Vector2(10,65),new Vector2(-10,235));
                v.padding=new RectOffset(10,10,6,6);
                Anchors(networkPanel,new Vector2(0,.81f),new Vector2(1,.81f),new Vector2(8,-38),new Vector2(-8,0));
                Anchors(shopWindow,new Vector2(0,.3f),new Vector2(1,.8f),new Vector2(8,0),new Vector2(-8,0));
                address.GetComponent<LayoutElement>().minWidth=90; host.GetComponent<LayoutElement>().minWidth=45; room.GetComponent<LayoutElement>().minWidth=45; resume.gameObject.SetActive(false); format.gameObject.SetActive(false); theme.gameObject.SetActive(false);
            } else {
                Anchors(side,new Vector2(0,0),new Vector2(0,1),new Vector2(18,75),new Vector2(292,-98));
                v.padding=new RectOffset(16,16,12,12);
                Anchors(networkPanel,new Vector2(0,1),Vector2.one,new Vector2(304,-126),new Vector2(-12,-88));
                Anchors(shopWindow,new Vector2(.25f,.12f),new Vector2(.98f,.86f),Vector2.zero,Vector2.zero);
                resume.gameObject.SetActive(true); format.gameObject.SetActive(true); theme.gameObject.SetActive(true);
            }
        }
        void FitGrid(RectTransform grid,int columns)
        {
            var g=grid.GetComponent<GridLayoutGroup>(); g.constraintCount=columns;
            var pad=side.GetComponent<VerticalLayoutGroup>().padding;
            float w=(side.rect.width-pad.left-pad.right-g.spacing.x*(columns-1))/columns;
            g.cellSize=new Vector2(Mathf.Max(40,w),30);
            int shown=grid.Cast<Transform>().Count(t=>t.gameObject.activeSelf);
            int rows=Mathf.Max(1,Mathf.CeilToInt(shown/(float)columns));
            var le=grid.GetComponent<LayoutElement>(); le.preferredHeight=le.minHeight=rows*30+(rows-1)*g.spacing.y;
        }
        static string Describe(SkillDef sk)
        {
            string area=sk.minRange==sk.maxRange?$"alcance {sk.maxRange}":$"alcance {sk.minRange}–{sk.maxRange}";
            string effect=sk.kind==SkillKind.Dano?$"{sk.damage} de dano":sk.kind==SkillKind.Cura?$"cura {sk.damage}":sk.kind==SkillKind.Armadilha?$"armadilha de {sk.damage}":"invocação";
            return $"{sk.name}: {sk.pa} PA · {area}{(sk.line?" · linha reta":"")}{(sk.modifiable?" · +alcance no alto":"")} · {effect}.";
        }
        static string Describe(ShopDef d)
        {
            if(d.skill!=null) return Describe(d.skill)+(d.uses>1?$" {d.uses} usos.":" 1 uso.");
            var parts=new List<string>();
            if(d.pa!=0) parts.Add($"+{d.pa} PA"); if(d.pm!=0) parts.Add($"+{d.pm} PM"); if(d.hp!=0) parts.Add($"+{d.hp} PV");
            if(d.armor!=0) parts.Add($"+{d.armor} armadura"); if(d.range!=0) parts.Add($"+{d.range} alcance");
            return d.name+": "+string.Join(", ",parts)+(d.kind==ShopKind.Buff?$" por {d.turns} turnos.":" permanente.");
        }
        Sprite[] vendor; Image shopScene;
        // Tela cheia: botão ou F11 (Alt+Enter também funciona no Windows). No celular o jogo já ocupa a tela.
        Button fullscreen;
        public static void ToggleFullScreen()
        {
            if(Application.platform==RuntimePlatform.WebGLPlayer) { Screen.fullScreen=!Screen.fullScreen; return; }
            if(Screen.fullScreen) Screen.SetResolution(1280,720,FullScreenMode.Windowed);
            else Screen.SetResolution(Display.main.systemWidth,Display.main.systemHeight,FullScreenMode.FullScreenWindow);
        }
        void Update()
        {
            if(UnityEngine.InputSystem.Keyboard.current?.f11Key.wasPressedThisFrame==true) ToggleFullScreen();
            if(fullscreen) SetLabel(fullscreen,Screen.fullScreen?"Janela":"Tela cheia");
            if(shopScene&&vendor.Length>1&&shopWindow.gameObject.activeInHierarchy) shopScene.sprite=vendor[(int)(Time.unscaledTime*6)%vendor.Length];
            if(!canvas||Game.State==null) return;
            if(Screen.width!=width||Screen.height!=height) Layout();
            var s=Game.State; var f=Game.Active;
            string Team(int t) => $"<color=#{ColorUtility.ToHtmlStringRGB(ArenaPresentation.TeamColor(t))}>P{t}</color>";
            turn.text=f==null?$"RODADA {s.round}":$"RODADA {s.round}   •   {Content.Fighters[f.def].name} {Team(f.team)}   •   {f.pa} PA   •   {f.pm} PM";
            string Goal(int t) => s.eliminated[t]?"fora":s.mode==VictoryMode.Galeoes?$"{s.galeoes[t]} Galeões":s.mode==VictoryMode.Tesouro?$"{s.collected[t]}/{s.target} moedas":s.mode==VictoryMode.Dominio?$"domínio {s.domain[t]}/{s.target}":"eliminação";
            // Placar por time: em todos contra todos, um item por jogador; em times, os jogadores de cada time juntos.
            // PV, moedas e placar esperam a animação: não mostram o dano antes de os dados caírem.
            if(Game.View==null||!Game.View.Busy) {
            score.text=string.Join("     ",s.Teams().Select(t=>{
                var members=Enumerable.Range(1,s.players).Where(p=>s.alliance[p]==t).Select(Team);
                return s.TeamPlay?$"Time {t} ({string.Join(" ",members)}) {Goal(t)}":$"{Team(t)} {Goal(t)}";
            }));
            fighters.text=string.Join("\n",s.fighters.Where(x=>x.Alive||x.summoner==0).Select(x=>{
                string n=Content.Fighters[x.def].name; bool on=f!=null&&x.id==f.id;
                return $"{(on?"▶ ":"")}{Team(x.team)} {n}  {(x.Alive?$"{x.hp}/{Content.MaxHp(x)} PV":"nocauteado")}{(x.summoner==0?$" · {s.coins[x.team]} moedas":"")}";
            }));
            mobileStats.text=score.text;
            }
            var skills=f==null?new List<SkillDef>():Match.Skills(f);
            for(int i=0;i<skillButtons.Count;i++) {
                var b=skillButtons[i]; bool show=i<skills.Count; b.gameObject.SetActive(show); if(!show) continue;
                var sk=skills[i]; var charge=f.charges.Find(c=>c.skill==sk.id);
                var wait=f.cooldowns.Find(x=>x.skill==sk.id);
                SetLabel(b,$"{sk.name} · {(wait!=null?$"recarga {wait.uses}":$"{sk.pa} PA")}{(charge!=null?$" ×{charge.uses}":"")}");
                b.interactable=Game.CanAct&&f.pa>=sk.pa&&wait==null;
                b.GetComponent<Image>().color=Game.Mode==sk.id?chosen:idle;
            }
            move.interactable=pass.interactable=Game.CanAct;
            move.GetComponent<Image>().color=Game.Mode==""?new Color(.21f,.36f,.43f):idle;
            int team=f?.team??1; var slots=s.shops[team].slots;
            shopLabel.text=$"LOJA DE P{team} · {s.coins[team]} MOEDAS{(f!=null&&f.bought?" · já comprou":"")}";
            shopTitle.text=$"Loja do Castor  ·  P{team}  ·  {s.coins[team]} moedas{(f!=null&&f.bought?"  ·  já comprou neste turno":"")}";
            SetLabel(openShop,shopWindow.gameObject.activeSelf?"Fechar loja":"Abrir loja");
            if(UnityEngine.InputSystem.Keyboard.current?.escapeKey.wasPressedThisFrame==true) shopWindow.gameObject.SetActive(false);
            for(int i=0;i<shopButtons.Count;i++) {
                var d=i<slots.Length?Shop.Def(slots[i]):null; var b=shopButtons[i];
                SetLabel(b,d==null?"—":$"{d.name} ({d.price})");
                b.interactable=d!=null&&Game.CanAct&&f.summoner==0&&!f.bought&&s.coins[team]>=d.price;
                var sp=ShopIcon(d?.id); shopIcons[i].sprite=sp; shopIcons[i].enabled=sp; shopIcons[i].color=b.interactable||d==null?Color.white:new Color(1,1,1,.5f);
            }
            FitGrid(actions,Portrait?4:1); FitGrid(shopGrid,Portrait?3:1);
            var chosenSkill=skills.FirstOrDefault(x=>x.id==Game.Mode);
            help.text=chosenSkill!=null?Describe(chosenSkill)+"\nClique num alvo laranja.":"Casas azuis: alcance dos PM. Clique para mover.";
            shopHelp.text=HoveredShop()??(f!=null&&f.summoner!=0?"Invocações não compram.":"Passe o cursor num item para ver o efeito. 1 compra por turno.");
            result.text=s.over?(s.winner==0?"EMPATE":$"VITÓRIA · {ArenaPresentation.TeamName(s,s.winner).ToUpperInvariant()}"):Game.Recovery?"CONEXÃO INTERROMPIDA":Game.Lobby?"AGUARDANDO JOGADORES":"";
            status.text=Game.Status;
            log.text=string.Join("\n",logLines);
            metrics.text=$"{Game.Network.Role}   •   {(Game.Network.IsConnected?"Conectado":"Jogo local / aguardando")}   •   RTT {Game.Rtt} ms   •   Sondas expiradas {Game.ExpiredPercent:0}%   •   Cliques filtrados {Game.Filtered}   •   {1/Mathf.Max(.001f,Time.smoothDeltaTime):0} FPS";
            resume.interactable=Game.Network.Role!="Local";
            SetLabel(seats,$"{Game.Seats} jogadores"); seats.interactable=Game.Network.Role=="Local";
            SetLabel(format,ArenaGame.FormatName(Game.Format)); format.interactable=Game.Network.Role=="Local"||(Game.Network.IsHost&&Game.Lobby);
            SetLabel(theme,ArenaGame.ThemeName(Game.Theme)); theme.interactable=format.interactable;
            start.gameObject.SetActive(Game.Network.IsHost&&Game.Lobby); start.interactable=Game.Filled>=2;
#if UNITY_WEBGL && !UNITY_EDITOR
            host.interactable=false;
#endif
        }
        // Descrição do item da loja sob o cursor.
        string HoveredShop()
        {
            var es=EventSystem.current; if(!es) return null;
            var pointer=UnityEngine.InputSystem.Pointer.current; if(pointer==null) return null;
            var results=new List<RaycastResult>(); es.RaycastAll(new PointerEventData(es){position=pointer.position.ReadValue()},results);
            var s=Game.State; int team=Game.Active?.team??1;
            foreach(var r in results) { int i=shopButtons.FindIndex(b=>b.gameObject==r.gameObject); if(i>=0) { var d=Shop.Def(s.shops[team].slots[i]); return d==null?null:Describe(d); } }
            return null;
        }
        public void ShowRoom(string code) { address.text=code; }
        public void Log(string line) { logLines.Add(line); if(logLines.Count>7) logLines.RemoveAt(0); }
        // Dados no meio da área do tabuleiro (Krosmaster). Arte do Codex (tarefa 013): Sprites/Dados/<face>.png e,
        // opcional, a tira Sprites/Dados/rolagem.png (quadros do dado girando). Sem arte: quadrado colorido com o nome da face.
        public const float DiceLand=.7f;
        static readonly string[] FaceFiles={"cataclisma","escudo","trava","esquiva","coringa-esquiva-trava","coringa"};
        static readonly string[] FaceNames={"Cataclisma","Escudo","Trava","Esquiva","Esquiva/Trava","Coringa"};
        static readonly Color[] FaceColors={new Color(.85f,.3f,.16f),new Color(.32f,.48f,.72f),new Color(.6f,.42f,.24f),new Color(.28f,.66f,.42f),new Color(.45f,.6f,.5f),new Color(.86f,.68f,.25f)};
        Sprite[] faceSprites,tumbleFrames;
        public void RollDice(Face[] faces,string[] captions) { if(canvas&&faces!=null&&faces.Length>0) StartCoroutine(Roll(faces,captions)); }
        IEnumerator Roll(Face[] faces,string[] captions)
        {
            faceSprites??=FaceFiles.Select(f=>Resources.Load<Sprite>("Sprites/Dados/"+f)).ToArray();
            tumbleFrames??=Resources.LoadAll<Sprite>("Sprites/Dados/rolagem").OrderBy(x=>int.TryParse(x.name.Substring(x.name.LastIndexOf('_')+1),out int n)?n:0).ToArray();
            var box=new GameObject("Dados",typeof(RectTransform)).GetComponent<RectTransform>(); box.SetParent(canvas.transform,false);
            box.anchorMin=box.anchorMax=Vector2.zero; box.anchoredPosition=BoardArea().center/UiScale; box.sizeDelta=Vector2.zero;
            int n=faces.Length; float size=n>4?64:84, gap=size+24;
            var dice=new List<(RectTransform rect,Image image,Text label)>();
            for(int i=0;i<n;i++) {
                var rect=Panel("Dado",box); rect.sizeDelta=new Vector2(size,size); rect.anchoredPosition=new Vector2((i-(n-1)*.5f)*gap,0);
                var image=rect.GetComponent<Image>(); image.preserveAspect=true; image.raycastTarget=false;
                var label=Text("",rect,n>4?11:13,ink); label.alignment=TextAnchor.MiddleCenter; label.horizontalOverflow=HorizontalWrapMode.Overflow; Anchors(label.rectTransform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
                var caption=Text(captions!=null&&i<captions.Length?captions[i]:"",rect,12,muted); caption.alignment=TextAnchor.UpperCenter; caption.horizontalOverflow=HorizontalWrapMode.Overflow;
                Anchors(caption.rectTransform,new Vector2(0,0),new Vector2(1,0),new Vector2(-20,-22),new Vector2(20,-4));
                dice.Add((rect,image,label));
            }
            void Show(int i,int face,bool tumbling)
            {
                var d=dice[i]; var art=tumbling&&tumbleFrames.Length>0?tumbleFrames[(int)(Time.time*14+i*3)%tumbleFrames.Length]:faceSprites[face];
                d.image.sprite=art; d.image.color=art?Color.white:FaceColors[face]; d.label.text=art?"":FaceNames[face];
            }
            // Gira e salta (cada dado com uma fase), freia e cai na face sorteada.
            for(float t=0;t<DiceLand;t+=Time.deltaTime) {
                float k=t/DiceLand;
                for(int i=0;i<n;i++) {
                    if(((int)(t*20)+i)%2==0) Show(i,UnityEngine.Random.Range(0,FaceFiles.Length),true);
                    var r=dice[i].rect; r.localRotation=Quaternion.Euler(0,0,(1-k)*(1-k)*Mathf.Sin(t*40+i)*35);
                    r.anchoredPosition=new Vector2((i-(n-1)*.5f)*gap,Mathf.Abs(Mathf.Sin(k*Mathf.PI*2.5f+i*.4f))*(1-k)*60);
                }
                yield return null;
            }
            for(int i=0;i<n;i++) { Show(i,(int)faces[i],false); dice[i].rect.localRotation=Quaternion.identity; dice[i].rect.anchoredPosition=new Vector2((i-(n-1)*.5f)*gap,0); }
            for(float t=0;t<.18f;t+=Time.deltaTime) { foreach(var d in dice) d.rect.localScale=Vector3.one*(1+.25f*Mathf.Sin(t/.18f*Mathf.PI)); yield return null; }
            yield return new WaitForSeconds(.9f);
            var groups=box.gameObject.AddComponent<CanvasGroup>();
            for(float t=0;t<.3f;t+=Time.deltaTime) { groups.alpha=1-t/.3f; yield return null; }
            Destroy(box.gameObject);
        }
        public void FloatText(Vector3 world,string value,Color color) { if(canvas) StartCoroutine(Float(world,value,color)); }
        IEnumerator Float(Vector3 world,string value,Color color)
        {
            var text=Text(value,canvas.transform,22,color); text.alignment=TextAnchor.MiddleCenter; text.rectTransform.sizeDelta=new Vector2(160,40);
            for(float t=0;t<.75f;t+=Time.deltaTime) { text.transform.position=Game.View.BoardCamera.WorldToScreenPoint(world+Vector3.up*(1.2f+t)); text.color=new Color(color.r,color.g,color.b,1-t/.75f); yield return null; }
            Destroy(text.gameObject);
        }
    }
}
