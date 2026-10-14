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
        RectTransform side,top,footer,networkPanel,logPanel,actions,shopGrid,shopWindow,viewPanel;
        Text turn,score,status,metrics,result,help,mobileStats,fighters,shopLabel,log,shopTitle,shopHelp,portraitText;
        RectTransform portrait; Image portraitArt; string portraitDef;
        readonly List<Text> labels=new List<Text>();
        readonly List<Button> skillButtons=new List<Button>(), shopButtons=new List<Button>();
        // Figura de cada produto (Sprites/Loja/<id>, entregue pelo Codex) à esquerda do botão.
        readonly List<Image> shopIcons=new List<Image>();
        readonly Dictionary<string,Sprite> iconCache=new Dictionary<string,Sprite>();
        public Sprite ShopIcon(string id) { if(string.IsNullOrEmpty(id)) return null; if(!iconCache.TryGetValue(id,out var sp)) iconCache[id]=sp=Resources.Load<Sprite>("Sprites/Loja/"+id); return sp; }
        readonly List<string> logLines=new List<string>();
        Button move,pass,host,room,join,resume,seats,format,theme,start,restart,openShop;
        // Linha do tempo no topo (020-ux): retrato, nome curto, PV/PA/PM, estado e as marcas de turno ATUAL e PRÓXIMO.
        RectTransform timeline,timelineContent; ScrollRect timelineScroll;
        readonly List<RectTransform> timelineCards=new List<RectTransform>();
        readonly List<Image> timelineFaces=new List<Image>();
        readonly List<Text> timelineTexts=new List<Text>();
        readonly Dictionary<string,Sprite> faceCache=new Dictionary<string,Sprite>();
        string timelineKey; int timelineActive=-1;
        // Confirmação em dois toques (020-ux): o HUD guarda o MatchState para zerar a mira mesmo se vier outro
        // estado com a MESMA revisão (Fighter/atores iguais) — nenhuma mira sobrevive a uma troca de estado.
        readonly ArenaActionConfirmation confirm=new ArenaActionConfirmation();
        MatchState confirmState;
        int actionInputFrame=-1;
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
            // Tabuleiro na tela inteira: cabeçalho e rodapé são faixas finas translúcidas; o resto fica no painel lateral recolhível.
            top=Panel("Cabecalho",root.transform); top.GetComponent<Image>().color=new Color(.03f,.04f,.07f,.55f);
            turn=Text("",top,15,ink); turn.alignment=TextAnchor.MiddleLeft; Anchors(turn.rectTransform,Vector2.zero,new Vector2(.55f,1),new Vector2(14,0),new Vector2(0,0));
            score=Text("",top,13,muted); score.alignment=TextAnchor.MiddleRight; Anchors(score.rectTransform,new Vector2(.45f,0),Vector2.one,Vector2.zero,new Vector2(-14,0));
            side=Panel("Comandos",root.transform); side.GetComponent<Image>().color=new Color(panel.r,panel.g,panel.b,.93f);
            var layout=side.gameObject.AddComponent<VerticalLayoutGroup>(); layout.padding=new RectOffset(12,12,8,10); layout.spacing=6; layout.childControlWidth=true; layout.childControlHeight=true; layout.childForceExpandHeight=false;
            // Abas do painel: Ações (lutadores, skills, loja, registro) e Partida (nova partida, salas, vista, menu); ◀ recolhe.
            var tabs=new GameObject("Abas",typeof(RectTransform),typeof(HorizontalLayoutGroup),typeof(LayoutElement)).GetComponent<RectTransform>(); tabs.SetParent(side,false);
            var tabRow=tabs.GetComponent<HorizontalLayoutGroup>(); tabRow.spacing=5; tabRow.childControlWidth=tabRow.childControlHeight=true; tabRow.childForceExpandWidth=false; var tabSize=tabs.GetComponent<LayoutElement>(); tabSize.preferredHeight=tabSize.minHeight=34; tabSize.flexibleHeight=0;
            actionsTab=Button("Ações",tabs,()=>ShowPage(false)); actionsTab.GetComponent<LayoutElement>().flexibleWidth=1;
            matchTab=Button("Partida",tabs,()=>ShowPage(true)); matchTab.GetComponent<LayoutElement>().flexibleWidth=1;
            Button("◀",tabs,()=>SetSideOpen(false),40);
            actionsPage=new GameObject("PaginaAcoes",typeof(RectTransform),typeof(VerticalLayoutGroup),typeof(LayoutElement)).GetComponent<RectTransform>(); actionsPage.SetParent(side,false);
            var pageLayout=actionsPage.GetComponent<VerticalLayoutGroup>(); pageLayout.spacing=5; pageLayout.childControlWidth=pageLayout.childControlHeight=true; pageLayout.childForceExpandHeight=false;
            actionsPage.GetComponent<LayoutElement>().flexibleHeight=1;
            labels.Add(LabelRow("LUTADORES",actionsPage,12,muted,18));
            fighters=LabelRow("",actionsPage,13,ink,112); labels.Add(fighters);
            labels.Add(LabelRow("AÇÕES",actionsPage,12,muted,18));
            actions=Grid("Acoes",actionsPage);
            move=Button("Mover",actions,()=>{ confirm.Reset(); Game.Mode=""; });
            // 8 botões: Asamyh tem 5 skills e o Desejo do Kellen abre 6 opções.
            for(int i=0;i<8;i++) { int k=i; skillButtons.Add(Button("",actions,()=>ChooseSkill(k))); }
            // Passar (e o Espaço) usa o mesmo método: o primeiro toque destaca, o segundo passa o turno.
            pass=Button("Passar  [Espaço]",actions,ConfirmPass);
            shopLabel=LabelRow("LOJA",actionsPage,12,muted,18); labels.Add(shopLabel);
            shopGrid=Grid("Loja",actionsPage);
            openShop=Button("Abrir loja",shopGrid,()=>shopWindow.gameObject.SetActive(!shopWindow.gameObject.activeSelf));
            help=LabelRow("",actionsPage,12,muted,44); labels.Add(help);
            result=LabelRow("",actionsPage,16,ArenaPresentation.Gold,28);
            // Registro no fim do painel lateral, ocupando o que sobra: não cobre mais o tabuleiro.
            logPanel=Panel("Registro",actionsPage); logPanel.GetComponent<Image>().color=new Color(.03f,.04f,.07f,.6f);
            var logSize=logPanel.gameObject.AddComponent<LayoutElement>(); logSize.flexibleHeight=1; logSize.minHeight=40;
            log=Text("",logPanel,12,ink); log.verticalOverflow=VerticalWrapMode.Truncate; log.alignment=TextAnchor.LowerLeft; Anchors(log.rectTransform,Vector2.zero,Vector2.one,new Vector2(10,6),new Vector2(-10,-6));
            footer=Panel("Rodape",root.transform); footer.GetComponent<Image>().color=new Color(.03f,.04f,.07f,.55f); footer.GetComponent<Image>().raycastTarget=false;
            status=Text("",footer,13,ink); status.alignment=TextAnchor.MiddleLeft; Anchors(status.rectTransform,Vector2.zero,new Vector2(.62f,1),new Vector2(12,0),Vector2.zero);
            metrics=Text("",footer,10,muted); metrics.alignment=TextAnchor.MiddleRight; Anchors(metrics.rectTransform,new Vector2(.62f,0),Vector2.one,Vector2.zero,new Vector2(-12,0));
            metrics.gameObject.SetActive(!Application.isMobilePlatform);
            // Barra da partida: página "Partida" do painel lateral, em 2 colunas.
            networkPanel=Panel("Conexao",side); networkPanel.GetComponent<Image>().color=Color.clear;
            var row=networkPanel.gameObject.AddComponent<GridLayoutGroup>(); row.spacing=new Vector2(6,6); row.constraint=GridLayoutGroup.Constraint.FixedColumnCount; row.constraintCount=2;
            networkPanel.gameObject.AddComponent<LayoutElement>().flexibleHeight=1;
            Button("Nova local",networkPanel,Game.NewLocal,84);
            Button("Lutadores",networkPanel,OpenPicker,84);
            seats=Button("",networkPanel,Game.CycleSeats,92);
            format=Button("",networkPanel,Game.CycleFormat,118);
            theme=Button("",networkPanel,Game.CycleTheme,128);
            // Sala em 3 passos: abrir (configurar) → liberar → iniciar. Host LAN e Criar sala abrem a configuração.
            host=Button("Criar sala",networkPanel,Game.OpenLobby,82);
            room=Button("Sala",networkPanel,()=>Game.Menu?.Open("sala"),90);
            address=Input(networkPanel,"","Código da sala ou ws://IP:9080");
            join=Button("Entrar",networkPanel,()=>Game.Join(address.text),62);
            start=Button("Iniciar",networkPanel,()=>{ if(Game.Torneio!=null) Game.NextTournamentMatch(); else Game.StartRoom(); },66);
            restart=Button("Reiniciar partida",networkPanel,Game.RestartRoom,120);
            resume=Button("Retomar local",networkPanel,Game.ResumeLocal,104);
            fullscreen=Button("Tela cheia",networkPanel,ToggleFullScreen,88); fullscreen.gameObject.SetActive(!Application.isMobilePlatform);
            Button("Girar",networkPanel,()=>Game.View.RotateView(1),56);
            Button("Menu",networkPanel,()=>Game.Menu?.Open(),64);
            Button("Fechar a Partida",networkPanel,Game.CloseMatch,120);
            // Girar a vista: sempre à mão no canto do tabuleiro (também: botão direito arrastado, segurar e arrastar, Q/E).
            // Dois dedos não giram: a pinça é só zoom (020-ux).
            viewPanel=Panel("Vista",root.transform);
            var viewRow=viewPanel.gameObject.AddComponent<HorizontalLayoutGroup>(); viewRow.spacing=6; viewRow.padding=new RectOffset(6,6,4,4); viewRow.childControlHeight=true; viewRow.childControlWidth=true; viewRow.childForceExpandWidth=false;
            // Giro aos poucos (30° deslizando) e "Reto" volta ao lado reto mais próximo.
            Button("◄",viewPanel,()=>Game.View.RotateBy(-30),44); Button("Reto",viewPanel,Game.View.StraightenView,60); Button("►",viewPanel,()=>Game.View.RotateBy(30),44);
            // Painel recolhido: só esta aba na borda esquerda, abaixo da faixa do topo.
            sideTab=Button("☰",root.transform,()=>SetSideOpen(true)); sideTab.GetComponentInChildren<Text>().fontSize=20;
            networkPanel.gameObject.SetActive(false);
            // Retrato do lutador ativo (tarefa 012 de nitidez): na mesa ele tem ~45 px em 1280×720; aqui, 120 px de arte.
            portrait=Panel("Retrato",root.transform); var frame=portrait.GetComponent<Image>(); frame.color=new Color(.03f,.04f,.07f,.72f); frame.raycastTarget=false;
            portraitArt=new GameObject("Arte",typeof(RectTransform),typeof(Image)).GetComponent<Image>(); portraitArt.transform.SetParent(portrait,false);
            portraitArt.preserveAspect=true; portraitArt.raycastTarget=false; Anchors(portraitArt.rectTransform,Vector2.zero,Vector2.one,new Vector2(4,24),new Vector2(-4,-4));
            portraitText=Text("",portrait,12,ink); portraitText.alignment=TextAnchor.MiddleCenter; portraitText.raycastTarget=false; Anchors(portraitText.rectTransform,Vector2.zero,new Vector2(1,0),new Vector2(4,3),new Vector2(-4,23));
            mobileStats=Text("",root.transform,14,ink);
            Anchors(mobileStats.rectTransform,new Vector2(0,.87f),new Vector2(1,.935f),new Vector2(20,0),new Vector2(-20,0));
            BuildShopWindow(root.transform);
            BuildCopyWindow(root.transform);
            BuildInspect(root.transform);
            BuildPicker(root.transform);
            BuildTimeline(root.transform);
            Layout();
            // Ao abrir o jogo, escolher os lutadores (execuções automáticas por linha de comando pulam).
            // Sem o menu (ArenaMenu), o seletor abre direto como antes.
            if(!System.Environment.GetCommandLineArgs().Any(a=>a.StartsWith("--arena-"))&&Game.Menu==null) OpenPicker();
        }
        // Janela da loja: cenário do castor lojista, itens sobre o balcão e a descrição do item sob o cursor.
        void BuildShopWindow(Transform root)
        {
            shopWindow=Panel("JanelaLoja",root); shopWindow.GetComponent<Image>().color=new Color(.03f,.04f,.07f,.97f);
            var scene=new GameObject("Cena",typeof(RectTransform),typeof(Image)).GetComponent<Image>(); scene.transform.SetParent(shopWindow,false);
            // Cena animada do castor (Sprites/Vendedor/frame-NN, tarefa 004); sem ela, a imagem parada.
            vendor=Resources.LoadAll<Sprite>("Sprites/Vendedor").OrderBy(x=>x.name).ToArray(); shopScene=scene;
            scene.sprite=vendor.Length>0?vendor[0]:Resources.Load<Sprite>("Sprites/Cenarios/loja"); scene.preserveAspect=true; scene.raycastTarget=false; if(!scene.sprite) scene.color=Color.clear;
            Anchors(scene.rectTransform,new Vector2(0,.44f),Vector2.one,new Vector2(10,0),new Vector2(-10,-42));
            shopTitle=Text("",shopWindow,17,ArenaPresentation.Gold); shopTitle.alignment=TextAnchor.MiddleLeft;
            Anchors(shopTitle.rectTransform,new Vector2(0,1),Vector2.one,new Vector2(16,-38),new Vector2(-60,-6));
            var close=Button("✕",shopWindow,()=>shopWindow.gameObject.SetActive(false));
            Anchors((RectTransform)close.transform,Vector2.one,Vector2.one,new Vector2(-46,-38),new Vector2(-8,-6));
            var items=new GameObject("Itens",typeof(RectTransform),typeof(HorizontalLayoutGroup)).GetComponent<RectTransform>(); items.SetParent(shopWindow,false);
            var row=items.GetComponent<HorizontalLayoutGroup>(); row.spacing=8; row.childControlWidth=row.childControlHeight=true; row.childForceExpandWidth=true; row.childForceExpandHeight=true;
            // Cada item: figura à esquerda, nome e preço em cima e, embaixo, a descrição do que ele faz (no celular não há cursor).
            Anchors(items,new Vector2(0,0),new Vector2(1,.36f),new Vector2(16,12),new Vector2(-16,0));
            for(int i=0;i<Shop.Slots;i++) {
                int k=i; var b=Button("",items,()=>Game.Submit("comprar",slot:k)); shopButtons.Add(b);
                var icon=new GameObject("Icone",typeof(RectTransform),typeof(Image)).GetComponent<Image>(); icon.transform.SetParent(b.transform,false); icon.preserveAspect=true; icon.raycastTarget=false;
                Anchors(icon.rectTransform,new Vector2(0,.5f),new Vector2(0,.5f),new Vector2(8,-44),new Vector2(94,44)); shopIcons.Add(icon);
                var label=b.GetComponentInChildren<Text>(); label.alignment=TextAnchor.MiddleLeft; label.fontSize=16; label.verticalOverflow=VerticalWrapMode.Truncate;
                Anchors(label.rectTransform,Vector2.zero,Vector2.one,new Vector2(100,8),new Vector2(-10,-8));
            }
            shopHelp=Text("",shopWindow,13,muted); shopHelp.alignment=TextAnchor.MiddleCenter; shopHelp.horizontalOverflow=HorizontalWrapMode.Wrap;
            Anchors(shopHelp.rectTransform,new Vector2(0,.36f),new Vector2(1,.44f),new Vector2(16,0),new Vector2(-16,0));
            shopWindow.gameObject.SetActive(false);
        }
        // Linha do tempo no TOPO (020-ux): rolagem horizontal (ScrollRect + RectMask2D) com um cartão por lutador.
        // Não cobre o tabuleiro: a faixa entra em BoardArea (BoardArea reserva TopBar+TimelineBar no alto da tela).
        void BuildTimeline(Transform root)
        {
            timeline=Panel("LinhaDoTempo",root); timeline.GetComponent<Image>().color=new Color(.03f,.04f,.07f,.82f);
            var view=timeline.gameObject.AddComponent<ScrollRect>(); timelineScroll=view;
            var viewport=new GameObject("Area",typeof(RectTransform),typeof(RectMask2D)).GetComponent<RectTransform>();
            viewport.SetParent(timeline,false); viewport.GetComponent<RectMask2D>().padding=Vector4.zero;
            Anchors(viewport,Vector2.zero,Vector2.one,new Vector2(4,3),new Vector2(-4,-3));
            timelineContent=new GameObject("Cartoes",typeof(RectTransform),typeof(HorizontalLayoutGroup),typeof(ContentSizeFitter)).GetComponent<RectTransform>();
            timelineContent.SetParent(viewport,false);
            var row=timelineContent.GetComponent<HorizontalLayoutGroup>();
            row.spacing=6; row.padding=new RectOffset(0,0,0,0); row.childControlWidth=row.childControlHeight=true;
            row.childForceExpandWidth=false; row.childForceExpandHeight=false;
            var fit=timelineContent.GetComponent<ContentSizeFitter>(); fit.horizontalFit=ContentSizeFitter.FitMode.PreferredSize;
            timelineContent.anchorMin=new Vector2(0,0); timelineContent.anchorMax=new Vector2(0,1); timelineContent.pivot=new Vector2(0,.5f);
            timelineContent.sizeDelta=Vector2.zero; timelineContent.anchoredPosition=Vector2.zero;
            view.content=timelineContent; view.viewport=viewport;
            view.horizontal=true; view.vertical=false; view.movementType=ScrollRect.MovementType.Clamped;
            view.inertia=!Application.isMobilePlatform; view.scrollSensitivity=24; // arrastar a lista com um dedo; roda do mouse também rola
            view.horizontalScrollbar=null; view.verticalScrollbar=null;
        }
        // Figura do lutador para o cartão/retrato: uma leitura por pasta de sprites.
        Sprite Face(string art) { if(string.IsNullOrEmpty(art)) return null; if(!faceCache.TryGetValue(art,out var sp)) faceCache[art]=sp=Resources.Load<Sprite>("Sprites/"+art+"/south"); return sp; }
        // Nome curto do cartão (o nome inteiro fica no alto e na lista de lutadores).
        static string Short(string name) { if(string.IsNullOrEmpty(name)) return "?"; int i=name.IndexOf(' '); return i>0?name.Substring(0,i):name; }
        // Cartões: ordem de iniciativa, PV/PA/PM, estado e as marcas de turno ATUAL e PRÓXIMO. A identidade mostrada
        // é a do disfarce (clone ilusório aparece como o conjurador; nenhuma etiqueta de Ilusão em lugar nenhum).
        void RefreshTimeline(MatchState s)
        {
            if(!timelineContent||s==null) return;
            var list=s.timeline.Select(id=>s.F(id)).Where(x=>x!=null&&x.Alive).ToList();
            int current=s.over?0:s.Active?.id??0;
            int start=list.FindIndex(x=>x.id==current);
            if(start>0) list=list.Skip(start).Concat(list.Take(start)).ToList();
            string key=string.Join(",",list.Select(x=>x.id));
            if(key!=timelineKey) {
                timelineKey=key;
                while(timelineCards.Count<list.Count) {
                    var card=Panel("Lutador",timelineContent);
                    var le=card.gameObject.AddComponent<LayoutElement>();
                    le.preferredWidth=TimelineCard; le.minWidth=TimelineCard; le.flexibleWidth=0; le.preferredHeight=le.minHeight=TimelineBar-6;
                    var face=new GameObject("Face",typeof(RectTransform),typeof(Image)).GetComponent<Image>(); face.transform.SetParent(card,false);
                    face.preserveAspect=true; face.raycastTarget=false;
                    float box=TimelineBar-14; Anchors(face.rectTransform,new Vector2(0,.5f),new Vector2(0,.5f),new Vector2(3,-box*.5f),new Vector2(3+box,box*.5f));
                    var text=Text("",card,11,ink); text.alignment=TextAnchor.MiddleLeft; text.horizontalOverflow=HorizontalWrapMode.Wrap;
                    Anchors(text.rectTransform,Vector2.zero,Vector2.one,new Vector2(box+4,1),new Vector2(-4,-1));
                    timelineCards.Add(card); timelineFaces.Add(face); timelineTexts.Add(text);
                }
                for(int i=0;i<timelineCards.Count;i++) timelineCards[i].gameObject.SetActive(i<list.Count);
            }
            int nextId=list.Skip(1).FirstOrDefault(x=>x.prisoned==0)?.id??0;
            for(int i=0;i<list.Count;i++) {
                var x=list[i]; var d=ArenaPresentation.Shown(x); var form=Content.Form(x);
                var sp=Face(form?.sprites??d.sprites);
                timelineFaces[i].sprite=sp; timelineFaces[i].enabled=sp; timelineFaces[i].color=Color.white;
                bool on=x.id==current; bool next=current!=0&&x.id==nextId;
                string mark=on?"AGORA":next?"PRÓXIMO":"";
                string vital=x.prisoned>0?"preso":x.channel>0?"canalizando":x.hidden&&s.Ally(x.team,Game.LocalTeam)?"escondido":"";
                timelineTexts[i].text=$"<b>{Short(d.name)}</b> {mark}\n{x.hp}/{Content.MaxHp(x)} PV · {x.pa} PA · {x.pm} PM{(vital==""?"":" · "+vital)}";
                timelineCards[i].GetComponent<Image>().color=on?chosen:next?new Color(.13f,.29f,.38f):Color.Lerp(panel,ArenaPresentation.TeamColor(x.team),.15f);
            }
            if(current!=timelineActive) { timelineActive=current; if(timelineScroll) timelineScroll.horizontalNormalizedPosition=0; }
        }
        // Confirmação em dois toques (020-ux): a mira armada cai quando o contexto muda — perder CanAct, abrir
        // menu/picker/loja (InMenu do ArenaGame zera CanAct) ou outro MatchState, mesmo com a mesma revisão.
        void SyncConfirmation()
        {
            var s=Game.State;
            if(!ReferenceEquals(confirmState,s)) { confirmState=s; confirm.Reset(); }
            if(!confirm.Armed) return;
            if(!Game.CanAct||PickerOpen||InspectOpen||(copyWindow&&copyWindow.gameObject.activeSelf)||(shopWindow&&shopWindow.gameObject.activeSelf)||confirm.Actor!=(Game.Active?.id??0)||s==null||confirm.Revision!=s.revision) confirm.Reset();
        }
        // A interface só pergunta ao HUD (o gesto de câmera da apresentação chama ClearConfirmation).
        public bool Confirming(string key) => confirm.Is(key);
        public bool HasPendingConfirmation => confirm.Armed;
        public void ClearConfirmation() { confirm.Reset(); Game.Mode=""; }
        bool PressAction(string key,Fighter f)
        {
            // Submit do botão e atalho de teclado no mesmo quadro contam como um único toque.
            if(actionInputFrame==Time.frameCount) return false;
            actionInputFrame=Time.frameCount; return confirm.Press(key,f.id,Game.State.revision);
        }
        // Ficha de um lutador do tabuleiro (autor, 13/10): tocar nele com Mover mostra atributos, estado e o que cada skill faz.
        RectTransform inspect; Text inspectTitle,inspectBody; Image inspectArt; int inspectId;
        void BuildInspect(Transform root)
        {
            inspect=Panel("FichaLutador",root); inspect.GetComponent<Image>().color=new Color(.03f,.04f,.07f,.96f);
            inspectArt=new GameObject("Retrato",typeof(RectTransform),typeof(Image)).GetComponent<Image>(); inspectArt.transform.SetParent(inspect,false);
            inspectArt.preserveAspect=true; inspectArt.raycastTarget=false; Anchors(inspectArt.rectTransform,new Vector2(0,1),new Vector2(0,1),new Vector2(10,-96),new Vector2(96,-10));
            inspectTitle=Text("",inspect,18,ArenaTema.Selected); inspectTitle.alignment=TextAnchor.UpperLeft; inspectTitle.verticalOverflow=VerticalWrapMode.Overflow;
            Anchors(inspectTitle.rectTransform,new Vector2(0,1),Vector2.one,new Vector2(104,-96),new Vector2(-46,-10));
            inspectBody=Text("",inspect,13,ink); inspectBody.alignment=TextAnchor.UpperLeft; inspectBody.horizontalOverflow=HorizontalWrapMode.Wrap; inspectBody.verticalOverflow=VerticalWrapMode.Truncate;
            Anchors(inspectBody.rectTransform,Vector2.zero,Vector2.one,new Vector2(12,10),new Vector2(-12,-104));
            var close=Button("✕",inspect,()=>inspect.gameObject.SetActive(false)); Anchors((RectTransform)close.transform,Vector2.one,Vector2.one,new Vector2(-42,-40),new Vector2(-8,-8));
            inspect.gameObject.SetActive(false);
        }
        public bool InspectOpen => inspect&&inspect.gameObject.activeSelf;
        public void Inspect(Fighter f)
        {
            if(!inspect||f==null) return;
            if(InspectOpen&&inspectId==f.id) { inspect.gameObject.SetActive(false); return; }
            inspectId=f.id; inspect.gameObject.SetActive(true); RefreshInspect();
        }
        void RefreshInspect()
        {
            var s=Game.State; var f=s?.F(inspectId);
            if(f==null||!f.Alive||(f.hidden&&!s.Ally(f.team,Game.LocalTeam))) { inspect.gameObject.SetActive(false); return; }
            // Disfarce: o clone ilusório mostra a identidade do conjurador (nome, retrato, atributos e habilidades).
            bool veiled=!string.IsNullOrEmpty(f.disguise);
            var d=veiled?ArenaPresentation.Shown(f):Content.Def(f); var form=Content.Form(f); string mu=ArenaTema.Html(muted);
            string team=MatchState.Player(f.team)?$"<color=#{ColorUtility.ToHtmlStringRGB(ArenaPresentation.TeamColor(f.team))}>P{f.team}</color>":"Neutro";
            var art=form?.sprites??d.sprites; var sp=string.IsNullOrEmpty(art)?null:Face(art);
            inspectArt.sprite=sp; inspectArt.color=sp?Color.white:Color.clear;
            inspectTitle.text=$"{d.name}{(d.legendary?" ★":"")}  {team}\n<size=13><color=#{ArenaTema.Html(ArenaTema.Hp)}>{f.hp}/{Content.MaxHp(f)} PV</color> · <color=#{ArenaTema.Html(ArenaTema.Pm)}>{f.pa} PA · {f.pm} PM</color> · Iniciativa {d.initiative} · Nível {d.level}</size>";
            var state=new List<string>();
            if(form!=null&&f.form!=d.startForm) state.Add($"forma {form.name}{(form.intangible?" (imune a dano)":"")}{(f.formTurns>0?$", {f.formTurns} turno(s)":"")}");
            if(f.hidden) state.Add($"escondido (anda até {Match.HiddenPm})"); if(f.prisoned>0) state.Add($"preso {f.prisoned} turno(s)"); if(f.channel>0) state.Add("canalizando o Desejo");
            if(f.auraOn) state.Add("aura ligada");
            foreach(var b in f.buffs) state.Add($"{b.id}{(b.pa!=0?$" {b.pa:+0;-0} PA":"")}{(b.pm!=0?$" {b.pm:+0;-0} PM":"")}{(b.armor!=0?$" {b.armor:+0;-0} armadura":"")} ({b.turns})");
            int armor=f.armor+d.armor+f.buffs.Sum(b=>b.armor)+(form?.armor??0);
            if(armor!=0) state.Add($"armadura {armor}");
            var text=new List<string>();
            string traits=Content.Traits(d); if(traits!="") text.Add($"<color=#d27aa6>{traits}</color>");
            if(state.Count>0) text.Add($"<color=#{mu}>Estado:</color> {string.Join(" · ",state)}");
            text.Add($"<color=#{mu}>SKILLS</color>");
            foreach(var sk in Match.Skills(f)) {
                var wait=f.cooldowns.Find(x=>x.skill==sk.id); var charge=f.charges.Find(x=>x.skill==sk.id);
                text.Add("• "+Content.Describe(sk)+(wait!=null?$" <color=#{mu}>(recarga: {wait.uses})</color>":"")+(charge!=null?$" <color=#{mu}>(×{charge.uses})</color>":""));
            }
            if(d.forms!=null&&d.forms.Length>1) text.Add($"<color=#{mu}>Formas:</color> {string.Join(", ",d.forms.Select(x=>x.name))}");
            inspectBody.text=string.Join("\n",text);
        }
        // Desejo D: depois de tocar no lutador, a lista das skills dele que dá para copiar; o botão manda a escolhida (Command.slot).
        RectTransform copyWindow; Text copyTitle; readonly List<Button> copyButtons=new List<Button>(); Cell copyCell;
        void BuildCopyWindow(Transform root)
        {
            copyWindow=Panel("CopiarSkill",root); copyWindow.GetComponent<Image>().color=new Color(.03f,.04f,.07f,.97f);
            Anchors(copyWindow,new Vector2(.28f,.22f),new Vector2(.72f,.78f),Vector2.zero,Vector2.zero);
            copyTitle=Text("",copyWindow,17,ArenaPresentation.Gold); copyTitle.alignment=TextAnchor.MiddleCenter;
            Anchors(copyTitle.rectTransform,new Vector2(0,1),Vector2.one,new Vector2(12,-44),new Vector2(-12,-8));
            var list=new GameObject("Skills",typeof(RectTransform),typeof(VerticalLayoutGroup)).GetComponent<RectTransform>(); list.SetParent(copyWindow,false);
            var v=list.GetComponent<VerticalLayoutGroup>(); v.spacing=6; v.childControlWidth=v.childControlHeight=true; v.childForceExpandHeight=false;
            Anchors(list,Vector2.zero,Vector2.one,new Vector2(12,58),new Vector2(-12,-50));
            for(int i=0;i<8;i++) { int k=i; copyButtons.Add(Button("",list,()=>{ copyWindow.gameObject.SetActive(false); Game.Submit("skill",copyCell,"desejo-d",k); })); }
            var cancel=Button("Cancelar",copyWindow,()=>copyWindow.gameObject.SetActive(false));
            Anchors((RectTransform)cancel.transform,new Vector2(.3f,0),new Vector2(.7f,0),new Vector2(0,10),new Vector2(0,48));
            copyWindow.gameObject.SetActive(false);
        }
        public void ChooseCopy(Cell c)
        {
            var f=Game.Active; if(f==null||!Game.CanAct) return;
            var wish=Content.Skill(f,"desejo-d"); var v=Board.At(Game.State,c);
            if(!Game.Preview.CanTarget(f,wish,c,out string reason)) { Game.Notify(reason); return; }
            if(v==null||(v.hidden&&!Game.State.Ally(v.team,f.team))) { Game.Submit("skill",c,"desejo-d"); return; }
            var options=Match.Copyables(f,v); copyCell=c;
            if(options.Count==0) { Game.Submit("skill",c,"desejo-d"); return; }
            copyTitle.text=$"Copiar uma skill de {Content.Fighters[v.def].name}";
            for(int i=0;i<copyButtons.Count;i++) {
                bool show=i<options.Count; copyButtons[i].gameObject.SetActive(show);
                if(show) SetLabel(copyButtons[i],$"{options[i].name} · {options[i].pa} PA{(options[i].damage>0&&options[i].kind==SkillKind.Dano?$" · {options[i].damage} de dano":"")}");
            }
            copyWindow.gameObject.SetActive(true);
        }
        // Seletor de lutadores: cada assento local monta o time (Nível até ArenaGame.TeamLevel, sem repetir).
        // Em rede só o time deste aparelho, que vale na próxima sala. Também serve ao torneio: pool, bans e escolha do draft.
        // Organização (autor, 13/10): cartões por Nível e nome, filtro por Nível e a ficha (skills) do último lutador tocado.
        enum PickMode { Team, Pool, Ban, Draft }
        PickMode pickMode;
        RectTransform picker,pickerGrid,pickerSide,pickerRow,pickerFilters; Text pickerTitle,pickerInfo,pickerDetail; Button pickerNext; readonly List<Button> pickButtons=new List<Button>();
        readonly List<Button> filterButtons=new List<Button>();
        readonly List<string> picking=new List<string>(); string[][] picked; int pickSeat;
        // Ordem dos cartões: Nível, depois nome. allowed: só esses aparecem (draft); pickLimit: máximo no modo Pool/Ban.
        static string[] pickOrder;
        HashSet<string> allowed; int pickLimit, levelFilter; string detailId; System.Action<string[]> pickDone;
        static readonly (string name,int min,int max)[] LevelFilters={("Todos",0,99),("Nível 1–3",0,3),("Nível 4",4,4),("Nível 5+",5,99)};
        public bool PickerOpen => picker&&picker.gameObject.activeSelf;
        bool OnlineSession => Game.Network&&(Game.Network.IsHost||Game.Network.IsConnected);
        void BuildPicker(Transform root)
        {
            pickOrder=Content.Roster.OrderBy(id=>Content.Fighters[id].level).ThenBy(id=>Content.Fighters[id].name).ToArray();
            picker=Panel("Seletor",root); picker.GetComponent<Image>().color=new Color(.03f,.04f,.07f,.97f); Anchors(picker,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            pickerTitle=Text("",picker,22,ArenaPresentation.Gold); pickerTitle.alignment=TextAnchor.MiddleCenter; Anchors(pickerTitle.rectTransform,new Vector2(0,1),Vector2.one,new Vector2(16,-50),new Vector2(-16,-8));
            // Filtros por Nível, logo abaixo do título.
            pickerFilters=new GameObject("Filtros",typeof(RectTransform),typeof(HorizontalLayoutGroup)).GetComponent<RectTransform>(); pickerFilters.SetParent(picker,false);
            var fr=pickerFilters.GetComponent<HorizontalLayoutGroup>(); fr.spacing=6; fr.childControlWidth=fr.childControlHeight=true; fr.childForceExpandWidth=false; fr.childAlignment=TextAnchor.MiddleLeft;
            for(int i=0;i<LevelFilters.Length;i++) { int k=i; filterButtons.Add(Button(LevelFilters[i].name,pickerFilters,()=>{ levelFilter=k; LayoutPicker(); RefreshPicker(); },96)); }
            var grid=pickerGrid=new GameObject("Lutadores",typeof(RectTransform),typeof(GridLayoutGroup)).GetComponent<RectTransform>(); grid.SetParent(picker,false);
            // Cartões do modelo 017: retrato, nome, custo, Vida/PA/PM; a equipe fica no painel lateral à direita.
            var g=grid.GetComponent<GridLayoutGroup>(); g.cellSize=new Vector2(110,132); g.spacing=new Vector2(6,6); g.childAlignment=TextAnchor.UpperLeft;
            foreach(string id in pickOrder) {
                var d=Content.Fighters[id]; var b=Button("",grid,()=>TogglePick(id)); ArenaTema.StyleFocus(b); pickButtons.Add(b);
                b.gameObject.AddComponent<ArenaDica>().Enter=()=>{ detailId=id; RefreshDetail(); };
                var icon=new GameObject("Retrato",typeof(RectTransform),typeof(Image)).GetComponent<Image>(); icon.transform.SetParent(b.transform,false); icon.preserveAspect=true; icon.raycastTarget=false;
                icon.sprite=d.sprites==null?null:Resources.Load<Sprite>("Sprites/"+d.sprites+"/south"); if(!icon.sprite) icon.color=Color.clear;
                Anchors(icon.rectTransform,new Vector2(0,.42f),Vector2.one,new Vector2(6,0),new Vector2(-6,-4));
                var label=b.GetComponentInChildren<Text>(); label.fontSize=12; label.alignment=TextAnchor.LowerCenter;
                label.text=CardText(d,false);
                Anchors(label.rectTransform,Vector2.zero,new Vector2(1,.44f),new Vector2(3,3),new Vector2(-3,0));
            }
            // Painel lateral: equipe, custo total, ficha do lutador tocado e avanço (desabilitado com a equipe vazia).
            var side=pickerSide=Panel("Equipe",picker); side.GetComponent<Image>().color=ArenaTema.Panel;
            var sideTitle=Text("EQUIPE",side,14,ArenaTema.Selected); sideTitle.fontStyle=FontStyle.Bold; Anchors(sideTitle.rectTransform,new Vector2(0,1),Vector2.one,new Vector2(14,-36),new Vector2(-14,-10));
            pickerInfo=Text("",side,14,ink); pickerInfo.alignment=TextAnchor.UpperLeft; pickerInfo.verticalOverflow=VerticalWrapMode.Overflow;
            pickerDetail=Text("",side,12,muted); pickerDetail.alignment=TextAnchor.UpperLeft; pickerDetail.horizontalOverflow=HorizontalWrapMode.Wrap; pickerDetail.verticalOverflow=VerticalWrapMode.Truncate;
            var row=pickerRow=new GameObject("Botoes",typeof(RectTransform),typeof(VerticalLayoutGroup)).GetComponent<RectTransform>(); row.SetParent(side,false);
            var h=row.GetComponent<VerticalLayoutGroup>(); h.spacing=8; h.childAlignment=TextAnchor.LowerCenter; h.childControlWidth=h.childControlHeight=true; h.childForceExpandWidth=true; h.childForceExpandHeight=false;
            Button("Aleatório",row,RandomPick); Button("Limpar",row,()=>{ picking.Clear(); RefreshPicker(); });
            pickerNext=Button("",row,NextPick); pickerNext.GetComponent<LayoutElement>().preferredHeight=48; ArenaTema.StyleFocus(pickerNext);
            picker.gameObject.SetActive(false);
        }
        void ShowPicker(PickMode mode,IEnumerable<string> start,IEnumerable<string> only,int limit,System.Action<string[]> done)
        {
            shopWindow.gameObject.SetActive(false); Game.Menu?.Close();
            pickMode=mode; allowed=only==null?null:new HashSet<string>(only); pickLimit=limit; pickDone=done; levelFilter=0;
            picking.Clear(); picking.AddRange((start??new string[0]).Where(id=>allowed==null||allowed.Contains(id)));
            detailId=picking.FirstOrDefault()??allowed?.FirstOrDefault()??pickOrder[0];
            picker.gameObject.SetActive(true); LayoutPicker(); RefreshPicker();
        }
        public void OpenPicker()
        {
            // Contra bots, só o P1 escolhe; os bots recebem times aleatórios em StartLocal.
            pickSeat=0; picked=new string[OnlineSession||Game.VsBots||Game.Setup?1:Game.Seats][];
            ShowPicker(PickMode.Team,ArenaGame.MyTeam,null,ArenaGame.TeamSize,null);
        }
        // Pool do torneio (até ArenaGame.PoolSize, sem regra de Nível).
        public void OpenPool() => ShowPicker(PickMode.Pool,ArenaGame.MyPool,null,ArenaGame.PoolSize,ids=>{ ArenaGame.MyPool=ids; Game.SendChoice(); Game.Menu?.Open("sala"); });
        // Draft do torneio: "ban" escolhe até limit da pool do outro; "pick" monta o time com o que sobrou da sua.
        public void OpenDraft(string phase,string[] options,int limit,System.Action<string[]> done)
        {
            options=(options??new string[0]).Where(Content.Fighters.ContainsKey).ToArray();
            if(phase=="ban") ShowPicker(PickMode.Ban,new string[0],options,Mathf.Clamp(limit,0,ArenaGame.MaxBans),done);
            else ShowPicker(PickMode.Draft,ArenaGame.MyTeam,options,ArenaGame.TeamSize,done);
        }
        // Cartão: retrato, nome, custo e Vida/PA/PM; compacto quando o cartão encolhe para caber o elenco.
        static string CardText(FighterDef d,bool compact)
        {
            string hp=ArenaTema.Html(ArenaTema.Hp), pm=ArenaTema.Html(ArenaTema.Pm), mu=ArenaTema.Html(ArenaTema.Muted);
            return compact?$"{d.name}{(d.legendary?" ★":"")}\n<size=9><color=#{mu}>C{d.level}</color> <color=#{hp}>{d.hp}PV</color></size>"
                :$"{d.name}{(d.legendary?" ★":"")}\n<size=10><color=#{mu}>Custo {d.level}</color>\n<color=#{hp}>{d.hp} PV</color> · <color=#{pm}>{d.pa} PA · {d.pm} PM</color></size>";
        }
        // Medidas do cartão para caber n lutadores em w×h (unidades do Canvas): encolhe até 70% antes de aceitar sobra.
        public static Vector2 CardSize(int n,float w,float h)
        {
            for(float s=1;s>.7f;s-=.05f) {
                var cell=new Vector2(110,132)*s; int cols=Mathf.Max(1,(int)((w+6)/(cell.x+6)));
                if(Mathf.CeilToInt(n/(float)cols)*(cell.y+6)-6<=h) return cell;
            }
            return new Vector2(110,132)*.7f;
        }
        bool Shown(string id)
        {
            var d=Content.Fighters[id]; var f=LevelFilters[levelFilter];
            return (allowed==null||allowed.Contains(id))&&d.level>=f.min&&d.level<=f.max;
        }
        // Paisagem: equipe no painel à direita. Retrato: equipe numa faixa embaixo (lista à esquerda, botões à direita).
        void LayoutPicker()
        {
            if(!pickerGrid) return;
            float k=UiScale,w=Screen.width/k,h=Screen.height/k;
            if(Portrait) {
                Anchors(pickerFilters,new Vector2(0,1),Vector2.one,new Vector2(10,-88),new Vector2(-10,-54));
                Anchors(pickerGrid,Vector2.zero,Vector2.one,new Vector2(10,244),new Vector2(-10,-96));
                Anchors(pickerSide,Vector2.zero,new Vector2(1,0),new Vector2(10,10),new Vector2(-10,234));
                Anchors(pickerInfo.rectTransform,Vector2.zero,new Vector2(.56f,1),new Vector2(14,10),new Vector2(-6,-42));
                pickerDetail.gameObject.SetActive(false);
                Anchors(pickerRow,new Vector2(.56f,0),Vector2.one,new Vector2(6,12),new Vector2(-12,-12));
                w-=20; h-=340;
            } else {
                Anchors(pickerFilters,new Vector2(0,1),new Vector2(1,1),new Vector2(16,-88),new Vector2(-316,-54));
                Anchors(pickerGrid,Vector2.zero,Vector2.one,new Vector2(16,14),new Vector2(-316,-96));
                Anchors(pickerSide,new Vector2(1,0),Vector2.one,new Vector2(-300,14),new Vector2(-16,-54));
                Anchors(pickerInfo.rectTransform,new Vector2(0,.55f),Vector2.one,new Vector2(14,0),new Vector2(-14,-42));
                Anchors(pickerDetail.rectTransform,Vector2.zero,new Vector2(1,.55f),new Vector2(14,190),new Vector2(-14,-6)); pickerDetail.gameObject.SetActive(true);
                Anchors(pickerRow,Vector2.zero,new Vector2(1,0),new Vector2(14,14),new Vector2(-14,180));
                w-=332; h-=110;
            }
            for(int i=0;i<pickButtons.Count;i++) pickButtons[i].gameObject.SetActive(Shown(pickOrder[i]));
            var cell=CardSize(pickButtons.Count(b=>b.gameObject.activeSelf),w,h); pickerGrid.GetComponent<GridLayoutGroup>().cellSize=cell;
            for(int i=0;i<pickButtons.Count;i++) pickButtons[i].GetComponentInChildren<Text>().text=CardText(Content.Fighters[pickOrder[i]],cell.x<100);
        }
        void TogglePick(string id)
        {
            detailId=id;
            if(picking.Contains(id)) picking.Remove(id);
            else if(pickMode==PickMode.Pool||pickMode==PickMode.Ban) { if(picking.Count<pickLimit) picking.Add(id); else Game.Notify(pickMode==PickMode.Ban?$"Até {pickLimit} ban{(pickLimit==1?"":"s")}.":$"Pool de até {pickLimit} lutadores."); }
            else if(ArenaGame.ValidTeam(picking.Append(id)).Contains(id)) picking.Add(id);
            else Game.Notify(picking.Count>=ArenaGame.TeamSize?$"No máximo {ArenaGame.TeamSize} lutadores.":Content.Fighters[id].legendary&&picking.Any(x=>Content.Fighters[x].legendary)?"Só 1 Lendária por time.":$"Passa do limite de Nível {ArenaGame.TeamLevel}.");
            RefreshPicker();
        }
        void RandomPick()
        {
            var pool=(allowed!=null?allowed.AsEnumerable():Content.Roster).OrderBy(_=>UnityEngine.Random.value).ToList();
            picking.Clear();
            if(pickMode==PickMode.Pool||pickMode==PickMode.Ban) picking.AddRange(pool.Take(pickLimit));
            else picking.AddRange(ArenaGame.ValidTeam(pool).Where(id=>allowed==null||allowed.Contains(id)));
            RefreshPicker();
        }
        void NextPick()
        {
            if(picking.Count==0&&pickMode!=PickMode.Ban) { Game.Notify("Escolha pelo menos 1 lutador."); return; }
            if(pickMode!=PickMode.Team) { picker.gameObject.SetActive(false); if(pickMode==PickMode.Draft) ArenaGame.MyTeam=ArenaGame.ValidTeam(picking); pickDone?.Invoke(picking.ToArray()); return; }
            picked[pickSeat++]=picking.ToArray();
            if(pickSeat<picked.Length) { picking.Clear(); picking.AddRange(Game.Picks!=null&&pickSeat<Game.Picks.Length?Game.Picks[pickSeat]:ArenaGame.MyTeam); RefreshPicker(); return; }
            picker.gameObject.SetActive(false);
            if(OnlineSession||Game.Setup) { ArenaGame.MyTeam=picked[0]; Game.SendChoice(); Game.Notify("Time salvo para a sala."); if(Game.Setup||Game.Lobby) Game.Menu?.Open("sala"); }
            else Game.StartLocal(picked);
        }
        void RefreshDetail()
        {
            if(!pickerDetail||detailId==null||!Content.Fighters.TryGetValue(detailId,out var d)) return;
            string traits=Content.Traits(d);
            pickerDetail.text=$"<b>{d.name}</b> · Nível {d.level} · {d.hp} PV · {d.pa} PA · {d.pm} PM · Ini {d.initiative}\n"+(traits!=""?$"<color=#d27aa6>{traits}</color>\n":"")+string.Join("\n",d.skills.Select(s=>"• "+Content.Describe(s)));
        }
        void RefreshPicker()
        {
            int level=picking.Sum(id=>Content.Fighters[id].level); string mu=ArenaTema.Html(ArenaTema.Muted);
            string list=string.Join("\n",picking.Select(id=>$"• {Content.Fighters[id].name} <color=#{mu}>· custo {Content.Fighters[id].level}</color>"));
            switch(pickMode)
            {
                case PickMode.Pool:
                    pickerTitle.text="SUA POOL DO TORNEIO";
                    pickerInfo.text=(picking.Count==0?"Toque nos lutadores da sua pool.":list)+$"\n\n<b>{picking.Count}/{pickLimit}</b>\n<size=12>No torneio, o adversário bane até {ArenaGame.MaxBans} da sua pool; você monta o time com o resto.</size>";
                    break;
                case PickMode.Ban:
                    pickerTitle.text=$"TORNEIO · BANIR  <size=15>(até {pickLimit} da pool do adversário)</size>";
                    pickerInfo.text=(picking.Count==0?"Ninguém banido ainda.":list)+$"\n\n<b>Bans {picking.Count}/{pickLimit}</b>";
                    break;
                default:
                    pickerTitle.text=pickMode==PickMode.Draft?"TORNEIO · MONTE SEU TIME  <size=15>(da sua pool, sem os banidos)</size>":OnlineSession||Game.Setup?"SEU TIME":$"TIME DE P{pickSeat+1}  <size=15>({pickSeat+1} de {picked.Length})</size>";
                    pickerInfo.text=(picking.Count==0?"Toque nos lutadores para montar o time.":list)+$"\n\n<b>Custo total {level}/{ArenaGame.TeamLevel}</b>\n<size=12>até {ArenaGame.TeamSize} lutadores · sem repetir</size>";
                    break;
            }
            for(int i=0;i<pickButtons.Count;i++) ArenaTema.MarkSelected(pickButtons[i],picking.Contains(pickOrder[i]),font);
            for(int i=0;i<filterButtons.Count;i++) filterButtons[i].GetComponent<Image>().color=i==levelFilter?chosen:idle;
            RefreshDetail();
            // Equipe vazia: avanço desabilitado e indicando o que fazer (banir ninguém vale).
            pickerNext.interactable=pickMode==PickMode.Ban||ArenaTema.CanAdvance(picking.Count);
            SetLabel(pickerNext,pickMode==PickMode.Ban?"Confirmar bans":pickMode==PickMode.Pool?"Salvar pool":pickMode==PickMode.Draft?ArenaTema.AdvanceLabel(picking.Count,"Confirmar time"):ArenaTema.AdvanceLabel(picking.Count,pickSeat+1<picked.Length?"Próximo jogador":OnlineSession||Game.Setup?"Salvar time":"Começar"));
        }
        // Antimissclick (020-ux): o PRIMEIRO toque no botão só destaca; o SEGUNDO toque no mesmo botão e no mesmo
        // contexto é que age — skill só em si mesma executa, skill com alvo entra no modo de mira (o toque na casa
        // depois conjura). Trocar de botão, de ator, de revisão/turno, perder CanAct, abrir menu/picker/loja ou
        // começar um gesto de câmera zera a confirmação (SyncConfirmation acima).
        void ChooseSkill(int index)
        {
            if(Game.View?.TouchGestureBusy==true) { ClearConfirmation(); return; }
            var f=Game.Active; if(f==null) return;
            var skills=Match.Skills(f); if(index>=skills.Count) return;
            var sk=skills[index];
            SyncConfirmation();
            if(!Game.CanAct) { confirm.Reset(); return; }
            if(!PressAction(ArenaActionConfirmation.Skill(sk.id),f)) { Game.Mode=""; return; } // 1º toque: só destaca
            if(Game.Mode==sk.id) { Game.Mode=""; return; }                                     // tocar de novo enquanto mira cancela a mira
            if(SelfOnly(sk)) { Game.Mode=""; Game.Submit("skill",f.cell,sk.id); return; }       // skill só em si mesma: executa agora
            Game.Mode=sk.id;                                                                    // skill com alvo: entra no modo de mira
        }
        // Passar também é em dois toques, e o botão e a tecla Espaço usam este mesmo método (020-ux).
        public void ConfirmPass()
        {
            if(Game.View?.TouchGestureBusy==true) { ClearConfirmation(); return; }
            SyncConfirmation();
            var f=Game.Active;
            if(f==null||!Game.CanAct) { confirm.Reset(); return; }
            if(!PressAction(ArenaActionConfirmation.Pass,f)) { Game.Mode=""; Game.Notify("Toque novamente para passar."); return; }
            Game.Mode=""; Game.Submit("passar");
        }
        public static bool SelfOnly(SkillDef sk) => sk.kind==SkillKind.Forma||sk.kind==SkillKind.Esconder||sk.kind==SkillKind.Aura||(sk.kind==SkillKind.Desejo&&string.IsNullOrEmpty(sk.wish));
        // Aviso do Cataclisma (entrega do Sol, 11/10): tira Sprites/Avisos/<id>, 12 quadros de 160 ms, um ciclo no centro do tabuleiro.
        public void ShowWarning(string id)
        {
            if(!canvas||string.IsNullOrEmpty(id)) return;
            var frames=Resources.LoadAll<Sprite>("Sprites/Avisos/"+id).OrderBy(x=>int.TryParse(x.name.Substring(x.name.LastIndexOf('_')+1),out int n)?n:0).ToArray();
            if(frames.Length>0) StartCoroutine(Warning(frames));
        }
        IEnumerator Warning(Sprite[] frames)
        {
            var go=new GameObject("AvisoCataclisma",typeof(RectTransform),typeof(Image),typeof(CanvasGroup)); var rect=go.GetComponent<RectTransform>(); rect.SetParent(canvas.transform,false);
            rect.anchorMin=rect.anchorMax=Vector2.zero; rect.anchoredPosition=BoardArea().center/UiScale; rect.sizeDelta=new Vector2(192,192);
            var image=go.GetComponent<Image>(); image.raycastTarget=false; image.preserveAspect=true; var group=go.GetComponent<CanvasGroup>();
            const float frame=.16f; float cycle=frames.Length*frame;
            for(float t=0;t<cycle;t+=Time.deltaTime) {
                image.sprite=frames[Mathf.Min((int)(t/frame),frames.Length-1)];
                group.alpha=Mathf.Clamp01(Mathf.Min(t/.15f,(cycle-t)/.25f));
                yield return null;
            }
            Destroy(go);
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
            var b=rect.gameObject.AddComponent<Button>(); b.targetGraphic=image; var colors=b.colors; colors.highlightedColor=new Color(.82f,.9f,1); colors.pressedColor=ArenaPresentation.Gold; colors.disabledColor=new Color(.4f,.43f,.5f); b.colors=colors; ArenaTema.Lerp(b); b.onClick.AddListener(action);
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
        // Painel lateral: aberto ou recolhido (só a aba ☰). O tabuleiro ocupa a tela inteira e se centraliza na parte livre.
        RectTransform actionsPage; Button actionsTab,matchTab,sideTab; bool matchPage;
        public static bool SideOpen=true;
        void SetSideOpen(bool open)
        {
            SideOpen=open; side.gameObject.SetActive(open); sideTab.gameObject.SetActive(!open);
            Layout(); Game.View?.FitCamera();
        }
        // Atalhos dos menus de teste do editor (conferir painel, aba e loja sem clicar).
        public static bool PeekShop;
        public void ToggleSide() => SetSideOpen(!SideOpen);
        public void TogglePage() => ShowPage(!matchPage);
        public void ToggleShopPeek() { PeekShop=!PeekShop; shopWindow.gameObject.SetActive(PeekShop); }
        void ShowPage(bool match)
        {
            matchPage=match; actionsPage.gameObject.SetActive(!match); networkPanel.gameObject.SetActive(match);
            actionsTab.GetComponent<Image>().color=match?idle:chosen; matchTab.GetComponent<Image>().color=match?chosen:idle;
        }
        // Escala da interface: a mesma do CanvasScaler (referência 1280×720, meio a meio), mas no PC não passa de 1,
        // para que em tela grande os botões não inchem e o espaço ganho vá para o tabuleiro.
        public static float UiScale => Mathf.Max(.5f,Mathf.Min(Application.isMobilePlatform?4:1,Mathf.Sqrt(Screen.width/1280f*Screen.height/720f)));
        // Área livre para o tabuleiro em pixels, entre os painéis montados em Layout (mesmas medidas).
        // Faixas fixas (unidades do Canvas): topo, rodapé e largura do painel lateral aberto.
        public const float TopBar=40,BottomBar=28,SideWidth=288,SideBand=222;
        // Faixa da linha do tempo (020-ux) e medidas do cartão de cada lutador.
        public const float TimelineBar=64,TimelineCard=210;
        // Parte da tela que o tabuleiro tem livre (pixels). A câmera cobre a tela toda; o tabuleiro se centraliza aqui.
        public static Rect BoardArea()
        {
            float k=UiScale,w=Screen.width,h=Screen.height,top=h-(TopBar+TimelineBar)*k,low=BottomBar*k;
            if(w<h) { if(SideOpen) low=(BottomBar+SideBand+8)*k; return new Rect(0,low,w,Mathf.Max(10,top-low)); }
            float left=SideOpen?(SideWidth+8)*k:0; return new Rect(left,low,Mathf.Max(10,w-left),Mathf.Max(10,top-low));
        }
        void Layout()
        {
            width=Screen.width; height=Screen.height; canvas.GetComponent<CanvasScaler>().scaleFactor=UiScale;
            mobileStats.gameObject.SetActive(Portrait); score.gameObject.SetActive(!Portrait); logPanel.gameObject.SetActive(!Portrait);
            foreach(var l in labels) l.gameObject.SetActive(!Portrait);
            Anchors(top,new Vector2(0,1),Vector2.one,new Vector2(0,-TopBar),Vector2.zero);
            // Linha do tempo logo abaixo da faixa do turno; daí para baixo é tabuleiro (BoardArea).
            Anchors(timeline,new Vector2(0,1),Vector2.one,new Vector2(0,-(TopBar+TimelineBar)),new Vector2(0,-TopBar));
            Anchors(footer,Vector2.zero,new Vector2(1,0),Vector2.zero,new Vector2(0,BottomBar));
            var v=side.GetComponent<VerticalLayoutGroup>();
            var board=BoardArea(); float k=UiScale, freeLeft=board.xMin/k, freeLow=board.yMin/k;
            if(Portrait) {
                Anchors(side,Vector2.zero,new Vector2(1,0),new Vector2(8,BottomBar+4),new Vector2(-8,BottomBar+SideBand));
                v.padding=new RectOffset(10,10,6,6);
                Anchors((RectTransform)sideTab.transform,Vector2.zero,Vector2.zero,new Vector2(8,BottomBar+6),new Vector2(56,BottomBar+54));
                Anchors(shopWindow,new Vector2(0,.3f),new Vector2(1,.8f),new Vector2(8,0),new Vector2(-8,0));
                Anchors(viewPanel,new Vector2(1,0),new Vector2(1,0),new Vector2(-190,freeLow+6),new Vector2(-8,freeLow+48));
                Anchors(inspect,new Vector2(0,.5f),new Vector2(1,1),new Vector2(8,0),new Vector2(-8,-(TopBar+TimelineBar)-6));
            } else {
                Anchors(side,Vector2.zero,new Vector2(0,1),new Vector2(0,BottomBar),new Vector2(SideWidth,-(TopBar+TimelineBar)));
                v.padding=new RectOffset(12,12,8,10);
                Anchors((RectTransform)sideTab.transform,new Vector2(0,1),new Vector2(0,1),new Vector2(8,-(TopBar+TimelineBar)-54),new Vector2(56,-(TopBar+TimelineBar)-6));
                Anchors(shopWindow,new Vector2(.25f,.1f),new Vector2(.98f,.9f),Vector2.zero,Vector2.zero);
                // Canto inferior esquerdo da parte livre: o losango da mesa deixa esse canto vazio.
                Anchors(portrait,Vector2.zero,Vector2.zero,new Vector2(freeLeft+8,BottomBar+6),new Vector2(freeLeft+120,BottomBar+150));
                Anchors(viewPanel,new Vector2(1,0),new Vector2(1,0),new Vector2(-190,BottomBar+6),new Vector2(-8,BottomBar+48));
                // Ficha do lutador tocado: à direita, sem cobrir o painel lateral.
                Anchors(inspect,new Vector2(1,0),Vector2.one,new Vector2(-380,BottomBar+56),new Vector2(-8,-(TopBar+TimelineBar)-6));
            }
            side.gameObject.SetActive(SideOpen); sideTab.gameObject.SetActive(!SideOpen);
            ShowPage(matchPage);
            float inner=(Portrait?width/k-16:SideWidth)-v.padding.left-v.padding.right;
            var net=networkPanel.GetComponent<GridLayoutGroup>(); net.constraintCount=Portrait?4:2;
            net.cellSize=new Vector2((inner-net.spacing.x*(net.constraintCount-1))/net.constraintCount,36);
            LayoutPicker();
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
        static string Describe(SkillDef sk) => Content.Describe(sk);
        static string Describe(ShopDef d) => Content.Describe(d);
        // Descrição sob o item: o texto da loja sem repetir o nome que já está na primeira linha do botão.
        public static string ItemEffect(ShopDef d) { string text=Describe(d); return text.StartsWith(d.name+": ")?text.Substring(d.name.Length+2):text; }
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
            SyncConfirmation(); // a mira armada cai antes de desenhar: perder CanAct, menu/picker/loja ou troca de estado
            var s=Game.State; var f=Game.Active;
            string Team(int t) => $"<color=#{ColorUtility.ToHtmlStringRGB(ArenaPresentation.TeamColor(t))}>P{t}</color>";
            turn.text=f==null?$"RODADA {s.round}":$"RODADA {s.round}   •   {ArenaPresentation.Shown(f).name} {Team(f.team)}   •   {f.pa} PA   •   {f.pm} PM";
            string Goal(int t) => s.eliminated[t]?"fora":s.mode==VictoryMode.Galeoes?$"{s.galeoes[t]} Galeões":s.mode==VictoryMode.Tesouro?$"{s.collected[t]}/{s.target} moedas":s.mode==VictoryMode.Dominio?$"domínio {s.domain[t]}/{s.target}":"eliminação";
            // Placar por time: em todos contra todos, um item por jogador; em times, os jogadores de cada time juntos.
            // PV, moedas e placar esperam a animação: não mostram o dano antes de os dados caírem.
            if(Game.View==null||!Game.View.Busy) {
            score.text=string.Join("     ",s.Teams().Select(t=>{
                var members=Enumerable.Range(1,s.players).Where(p=>s.alliance[p]==t).Select(Team);
                return s.TeamPlay?$"Time {t} ({string.Join(" ",members)}) {Goal(t)}":$"{Team(t)} {Goal(t)}";
            }));
            // Na ordem da linha do tempo (Krosmaster: cada jogador joga o time inteiro, da maior Iniciativa para a menor).
            int Order(Fighter x) { int i=s.timeline.IndexOf(x.id); return i<0?999:i; }
            fighters.text=string.Join("\n",s.fighters.Where(x=>x.Alive||x.summoner==0).OrderBy(Order).Select(x=>{
                string n=ArenaPresentation.Shown(x).name; bool on=f!=null&&x.id==f.id; // disfarce: o clone aparece como o conjurador
                return $"{(on?"▶ ":"")}{(MatchState.Player(x.team)?Team(x.team):"Neutro")} {n}{(x.summoner==0&&MatchState.Player(x.team)?$" <color=#a7b4c2>Ini {ArenaPresentation.Shown(x).initiative}</color>":"")}  {(x.Alive?$"{x.hp}/{Content.MaxHp(x)} PV":"nocauteado")}{(x.summoner==0&&MatchState.Player(x.team)?$" · {s.coins[x.team]} moedas":"")}{(x.prisoned>0?" · preso":"")}";
            }));
            mobileStats.text=score.text;
            var art=f==null?null:Content.Form(f)?.sprites??ArenaPresentation.Shown(f).sprites;
            portrait.gameObject.SetActive(!Portrait&&!string.IsNullOrEmpty(art));
            if(portrait.gameObject.activeSelf) {
                if(portraitDef!=art) { portraitDef=art; portraitArt.sprite=Face(art); portraitArt.color=portraitArt.sprite?Color.white:Color.clear; }
                portraitText.text=$"{ArenaPresentation.Shown(f).name}  {f.hp}/{Content.MaxHp(f)} PV";
            }
            RefreshTimeline(s); // retratos, PV/PA/PM, estado e turno ATUAL/PRÓXIMO, a cada revisão/turno/HP
            }
            var skills=f==null?new List<SkillDef>():Match.Skills(f);
            for(int i=0;i<skillButtons.Count;i++) {
                var b=skillButtons[i]; bool show=i<skills.Count; b.gameObject.SetActive(show); if(!show) continue;
                var sk=skills[i]; var charge=f.charges.Find(c=>c.skill==sk.id);
                var wait=f.cooldowns.Find(x=>x.skill==sk.id);
                bool spent=sk.perMatch>0&&f.usedMatch.Count(u=>u==sk.id)>=sk.perMatch, early=sk.minRound>s.round;
                string cost=sk.kind==SkillKind.Aura?(f.auraOn?"ligada · desligar":f.auraPaid?"religar":$"{sk.pa} PA/turno"):$"{sk.pa} PA";
                SetLabel(b,$"{sk.name} · {(spent?"usada":early?$"rodada {sk.minRound}":wait!=null?$"recarga {wait.uses}":cost)}{(charge!=null?$" ×{charge.uses}":"")}");
                b.interactable=Game.CanAct&&f.pa>=Match.Cost(f,sk)&&wait==null&&!spent&&!early&&f.channel==0;
                // Destaque do 1º toque (mira armada) e do modo de mira: o mesmo tom escolhido de sempre.
                b.GetComponent<Image>().color=Game.Mode==sk.id||Confirming(ArenaActionConfirmation.Skill(sk.id))?chosen:idle;
            }
            move.interactable=pass.interactable=Game.CanAct;
            move.GetComponent<Image>().color=Game.Mode==""&&!confirm.Armed?new Color(.21f,.36f,.43f):idle;
            SetLabel(pass,Confirming(ArenaActionConfirmation.Pass)?"Toque novamente para passar":"Passar  [Espaço]");
            pass.GetComponent<Image>().color=Confirming(ArenaActionConfirmation.Pass)?chosen:idle;
            int team=f?.team??1; var slots=s.shops[team].slots;
            shopLabel.text=$"LOJA DE P{team} · {s.coins[team]} MOEDAS";
            shopTitle.text=$"Loja do Castor  ·  P{team}  ·  {s.coins[team]} moedas";
            // Só dá para abrir a loja numa casa ao lado dela; saindo dali, a janela fecha.
            bool atShop=f!=null&&Board.NextToShop(s,f.cell);
            if(!atShop&&!PeekShop) shopWindow.gameObject.SetActive(false);
            openShop.interactable=atShop;
            SetLabel(openShop,!atShop?"Loja: fique ao lado dela":shopWindow.gameObject.activeSelf?"Fechar loja":"Abrir loja");
            if(UnityEngine.InputSystem.Keyboard.current?.escapeKey.wasPressedThisFrame==true) { shopWindow.gameObject.SetActive(false); inspect.gameObject.SetActive(false); }
            if(InspectOpen&&(Game.View==null||!Game.View.Busy)) RefreshInspect();
            for(int i=0;i<shopButtons.Count;i++) {
                var d=i<slots.Length?Shop.Def(slots[i]):null; var b=shopButtons[i];
                SetLabel(b,d==null?"—":$"<b>{d.name}</b>\n<color=#{ArenaTema.Html(ArenaPresentation.Gold)}>{d.price} moedas</color>\n<size=14><color=#{ArenaTema.Html(muted)}>{ItemEffect(d)}</color></size>");
                b.interactable=d!=null&&Game.CanAct&&atShop&&f.summoner==0&&s.coins[team]>=d.price;
                var sp=ShopIcon(d?.id); shopIcons[i].sprite=sp; shopIcons[i].enabled=sp; shopIcons[i].color=b.interactable||d==null?Color.white:new Color(1,1,1,.5f);
                // Sem figura, o texto usa a largura toda do cartão.
                b.GetComponentInChildren<Text>().rectTransform.offsetMin=new Vector2(sp?100:12,8);
            }
            // Muitas skills (gatos, Desejo do Kellen): 2 colunas no painel lateral para não passar da tela.
            FitGrid(actions,Portrait?4:skills.Count>4?2:1); FitGrid(shopGrid,Portrait?3:1);
            var chosenSkill=skills.FirstOrDefault(x=>x.id==Game.Mode);
            var armed=skills.FirstOrDefault(x=>Confirming(ArenaActionConfirmation.Skill(x.id)));
            help.text=chosenSkill!=null?Describe(chosenSkill)+"\nToque numa casa laranja, mesmo vazia."
                :armed!=null?Describe(armed)+"\nToque de novo no mesmo botão para conjurar."
                :Confirming(ArenaActionConfirmation.Pass)?"Toque novamente para passar."
                :"Casas azuis: alcance dos PM. Clique para mover.";
            shopHelp.text=f!=null&&f.summoner!=0?"Invocações não compram.":"Toque num item para comprar. Compre quantos puder pagar.";
            result.text=s.over?(s.winner==0?"EMPATE":$"VITÓRIA · {ArenaPresentation.TeamName(s,s.winner).ToUpperInvariant()}"):Game.Recovery?"CONEXÃO INTERROMPIDA":Game.Lobby?"AGUARDANDO JOGADORES":"";
            status.text=Game.Status;
            log.text=string.Join("\n",logLines);
            metrics.text=$"{Game.Network.Role}   •   {(Game.Network.IsConnected?"Conectado":"Jogo local / aguardando")}   •   RTT {Game.Rtt} ms   •   Sondas expiradas {Game.ExpiredPercent:0}%   •   Cliques filtrados {Game.Filtered}   •   {1/Mathf.Max(.001f,Time.smoothDeltaTime):0} FPS";
            resume.interactable=Game.Network.Role!="Local";
            SetLabel(seats,$"{Game.Seats} jogadores"); seats.interactable=Game.Network.Role=="Local";
            SetLabel(format,ArenaGame.FormatName(Game.Format)); format.interactable=Game.Network.Role=="Local"||(Game.Network.IsHost&&Game.Lobby);
            SetLabel(theme,ArenaGame.ThemeName(Game.Theme)); theme.interactable=format.interactable;
            // Iniciar na sala; no torneio, "Próxima partida" quando a anterior acabou. Reiniciar: mesma sala, partida nova.
            bool cupNext=Game.Torneio!=null&&Game.Torneio.recorded;
            start.gameObject.SetActive(Game.Network.IsHost&&(Game.Lobby&&Game.Torneio==null||cupNext)); start.interactable=Game.Filled>=2; SetLabel(start,cupNext?"Próxima partida":"Iniciar");
            restart.gameObject.SetActive(Game.Network.IsHost&&!Game.Lobby);
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
        // Textos sobre o mesmo lutador empilham (dano, "Preso!", "+1 moeda") em vez de se cobrir; dano em negrito e maior,
        // contorno escuro para ler sobre lava/neve, e o texto nunca sai da tela.
        readonly List<(Vector3 at,float until)> floating=new List<(Vector3,float)>();
        public static int FloatSlot(List<(Vector3 at,float until)> active,Vector3 world,float now)
        {
            active.RemoveAll(x=>x.until<now);
            int slot=active.Count(x=>(x.at-world).sqrMagnitude<.5f); active.Add((world,now+.45f)); return slot;
        }
        IEnumerator Float(Vector3 world,string value,Color color)
        {
            int slot=FloatSlot(floating,world,Time.time);
            bool damage=value.StartsWith("−");
            var text=Text(value,canvas.transform,damage?27:20,color); text.alignment=TextAnchor.MiddleCenter; text.rectTransform.sizeDelta=new Vector2(220,40);
            text.horizontalOverflow=HorizontalWrapMode.Overflow; if(damage) text.fontStyle=FontStyle.Bold;
            var edge=text.gameObject.AddComponent<Outline>(); edge.effectColor=new Color(0,0,0,.85f); edge.effectDistance=new Vector2(1.5f,-1.5f);
            float k=UiScale;
            for(float t=0;t<.75f;t+=Time.deltaTime) {
                Vector2 p=Game.View.BoardCamera.WorldToScreenPoint(world+Vector3.up*(1.2f+t)); p.y+=slot*26*k;
                p.x=Mathf.Clamp(p.x,110*k,Screen.width-110*k); p.y=Mathf.Clamp(p.y,20*k,Screen.height-20*k);
                text.transform.position=p; text.color=new Color(color.r,color.g,color.b,t<.5f?1:1-(t-.5f)/.25f); yield return null;
            }
            Destroy(text.gameObject);
        }
    }
}
