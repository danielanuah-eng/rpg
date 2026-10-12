using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ArenaTatica.Core;

namespace ArenaTatica
{
    // Fichas editadas neste aparelho (aba Editor): Application.persistentDataPath/arena-edicoes.json.
    // Valem na próxima partida local e na sala que este aparelho hospeda (o Host manda as dele no estado).
    public static class ArenaEdits
    {
        static string FilePath => Path.Combine(Application.persistentDataPath,"arena-edicoes.json");
        static string saved;
        public static string Saved
        {
            get { if(saved==null) { try { saved=File.Exists(FilePath)?File.ReadAllText(FilePath):""; } catch(Exception) { saved=""; } } return saved; }
        }
        public static GameEdits Load()
        {
            try { return string.IsNullOrEmpty(Saved)?new GameEdits():JsonUtility.FromJson<GameEdits>(Saved)??new GameEdits(); } catch(Exception) { return new GameEdits(); }
        }
        static void Save(GameEdits e)
        {
            saved=e.fighters.Count==0?"":JsonUtility.ToJson(e);
            try { if(saved=="") { if(File.Exists(FilePath)) File.Delete(FilePath); } else File.WriteAllText(FilePath,saved); } catch(Exception ex) { Debug.LogWarning("Edições: "+ex.Message); }
        }
        public static void Put(FighterDef d) { var e=Load(); e.fighters.RemoveAll(x=>x.id==d.id); e.fighters.Add(Content.Sanitize(Content.Copy(d))); Save(e); }
        public static void Reset(string id) { var e=Load(); e.fighters.RemoveAll(x=>x.id==id); Save(e); }
        public static void ResetAll() => Save(new GameEdits());
        public static FighterDef Get(string id) => Load().fighters.Find(x=>x.id==id);
    }

    // Estatísticas deste aparelho (aba Estatísticas), contadas pelos eventos que a tela anima.
    [Serializable] public class FighterStat { public string id; public int games, wins, damage, kos, heal; }
    [Serializable] public class NameCount { public string id; public int n; }
    [Serializable] public class StatsData { public int games, finished, rounds; public List<FighterStat> fighters=new List<FighterStat>(); public List<NameCount> events=new List<NameCount>(), modes=new List<NameCount>(); }
    public static class ArenaStats
    {
        const string Key="arena-estatisticas";
        static StatsData data; static bool pending;
        static readonly Dictionary<int,int> lastHit=new Dictionary<int,int>();
        public static StatsData Data => data??=Load();
        static StatsData Load()
        {
            try { string json=PlayerPrefs.GetString(Key,""); return json==""?new StatsData():JsonUtility.FromJson<StatsData>(json)??new StatsData(); } catch(Exception) { return new StatsData(); }
        }
        static void Save() { PlayerPrefs.SetString(Key,JsonUtility.ToJson(Data)); PlayerPrefs.Save(); }
        // A partida só conta depois da primeira ação (o jogo abre com uma partida pronta que pode nem ser jogada).
        public static void Begin() { pending=true; lastHit.Clear(); }
        public static FighterStat Of(string id) { var f=Data.fighters.Find(x=>x.id==id); if(f==null) Data.fighters.Add(f=new FighterStat{id=id}); return f; }
        static void Count(List<NameCount> list,string id) { if(string.IsNullOrEmpty(id)) return; var c=list.Find(x=>x.id==id); if(c==null) list.Add(c=new NameCount{id=id}); c.n++; }
        static Fighter Owner(MatchState s,Fighter f) => f!=null&&f.summoner!=0?s.F(f.summoner)??f:f;
        public static void Record(List<GameEvent> events,MatchState s)
        {
            if(events==null||events.Count==0||s==null) return;
            if(pending) { pending=false; Data.games++; foreach(var f in s.fighters.Where(f=>f.summoner==0&&MatchState.Player(f.team))) Of(f.def).games++; }
            foreach(var e in events)
            {
                var actor=Owner(s,s.F(e.actor)); bool player=actor!=null&&MatchState.Player(actor.team);
                switch(e.kind)
                {
                    case "ataque": if(e.value>0&&player) { Of(actor.def).damage+=e.value; lastHit[e.target]=actor.id; } break;
                    case "cura": if(e.value>0&&player) Of(actor.def).heal+=e.value; break;
                    case "nocaute": if(e.info!="renasce"&&lastHit.TryGetValue(e.target,out int k)&&s.F(k) is Fighter killer) Of(killer.def).kos++; break;
                    case "cataclisma": Count(Data.events,e.info); break;
                    case "vitoria":
                        Data.finished++; Data.rounds+=s.round; Count(Data.modes,e.info);
                        foreach(var f in s.fighters.Where(f=>f.summoner==0&&MatchState.Player(f.team)&&e.actor!=0&&s.Team(f.team)==e.actor)) Of(f.def).wins++;
                        break;
                }
            }
            Save();
        }
        public static void Reset() { data=new StatsData(); Save(); }
    }

    // Frontend do jogo (APK e PC), no estilo do site do Arcanoclisma: barra de seções em cima e o conteúdo embaixo.
    // Abre ao iniciar (sem opções --arena-*), pelo botão Menu da HUD ou pelo Esc/voltar do celular.
    public sealed class ArenaMenu : MonoBehaviour
    {
        public ArenaGame Game;
        public bool IsOpen => root&&root.gameObject.activeSelf;
        // Paleta do site (Arcanoclisma/online/public/css): fundo, painel, borda, texto, apagado, ouro, azul, verde, vermelho, rosa.
        static readonly Color Bg=ArenaTema.Background, Panel=ArenaTema.Panel, Edge=ArenaTema.Border, Ink=ArenaTema.Ink, Muted=ArenaTema.Muted,
            Gold=ArenaTema.Selected, Blue=ArenaTema.Pm, Green=ArenaTema.Healing, Red=ArenaTema.Hp, Pink=Hex(0xd27aa6), Idle=ArenaTema.Raised, Chosen=ArenaTema.SelectedFill;
        static Color Hex(int rgb) => new Color((rgb>>16&255)/255f,(rgb>>8&255)/255f,(rgb&255)/255f);
        static readonly (string id,string name)[] Tabs={("inicio","Início"),("jogar","Jogar"),("personagens","Personagens"),("loja","Loja e eventos"),
            ("editor","Editor"),("mapas","Mapas"),("estatisticas","Estatísticas"),("regras","Regras"),("opcoes","Opções")};
        Font font; Canvas canvas; RectTransform root, body; Text title;
        readonly Dictionary<string,Image> tabButtons=new Dictionary<string,Image>();
        string tab="inicio", selected="ciantia", editorId="ciantia", code="";
        bool Portrait => Screen.width<Screen.height;

        void Start()
        {
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var go=new GameObject("Menu",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)); go.transform.SetParent(transform);
            canvas=go.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=100;
            var scaler=go.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1280,720); scaler.matchWidthOrHeight=.5f;
            root=Box("Fundo",go.transform,Bg); Fill(root);
            var top=Box("Topo",root,Panel); top.anchorMin=new Vector2(0,1); top.anchorMax=Vector2.one; top.offsetMin=new Vector2(0,-64); top.offsetMax=Vector2.zero;
            var line=Box("Borda",top,Edge); line.anchorMin=Vector2.zero; line.anchorMax=new Vector2(1,0); line.offsetMin=Vector2.zero; line.offsetMax=new Vector2(0,2);
            var bar=top.gameObject.AddComponent<HorizontalLayoutGroup>(); bar.padding=new RectOffset(14,14,10,10); bar.spacing=6; bar.childAlignment=TextAnchor.MiddleLeft;
            bar.childControlWidth=bar.childControlHeight=true; bar.childForceExpandWidth=false; bar.childForceExpandHeight=true;
            title=Label(top,"ARENA TÁTICA",22,Gold,TextAnchor.MiddleLeft); Layout(title.gameObject,190,0);
            foreach(var t in Tabs) { var id=t.id; var b=Btn(top,t.name,()=>Show(id),0,44,14); tabButtons[id]=b.GetComponent<Image>(); b.GetComponent<LayoutElement>().minWidth=58; }
            Btn(top,"Voltar ao jogo",Close,130,44,14).GetComponent<Image>().color=Chosen;
            body=New("Conteudo",root); body.anchorMin=Vector2.zero; body.anchorMax=Vector2.one; body.offsetMin=Vector2.zero; body.offsetMax=new Vector2(0,-64);
            var args=Environment.GetCommandLineArgs();
            int shot=Array.IndexOf(args,"--arena-menu-shot");
            if(shot>=0) StartCoroutine(Shots(shot+1<args.Length?args[shot+1]:"Artifacts/menu"));
            else if(!args.Any(a=>a.StartsWith("--arena-"))) Open("inicio");
            else Close();
        }
        public void Open(string t=null) { root.gameObject.SetActive(true); Show(t??tab); }
        public void Close() { root.gameObject.SetActive(false); }
        void Update()
        {
            if(IsOpen&&UnityEngine.InputSystem.Keyboard.current?.escapeKey.wasPressedThisFrame==true) Close();
            if(title) title.gameObject.SetActive(!Portrait);
        }
        // Capturas de todas as abas (--arena-menu-shot <prefixo>): conferência visual sem clicar.
        IEnumerator Shots(string prefix)
        {
            prefix=Path.GetFullPath(prefix); Directory.CreateDirectory(Path.GetDirectoryName(prefix+"-x.png"));
            foreach(var t in Tabs) { Open(t.id); yield return new WaitForSeconds(.6f); ScreenCapture.CaptureScreenshot($"{prefix}-{Screen.width}x{Screen.height}-{t.id}.png"); yield return new WaitForSeconds(.4f); }
            selected="asamyh"; Open("personagens"); yield return new WaitForSeconds(.6f); ScreenCapture.CaptureScreenshot($"{prefix}-{Screen.width}x{Screen.height}-personagens-asamyh.png"); yield return new WaitForSeconds(.4f);
            editorId="principe-kellen"; Open("editor"); yield return new WaitForSeconds(.6f); ScreenCapture.CaptureScreenshot($"{prefix}-{Screen.width}x{Screen.height}-editor-kellen.png"); yield return new WaitForSeconds(.6f);
            Application.Quit();
        }

        void Show(string t)
        {
            tab=t; refreshers.Clear();
            foreach(Transform c in body) Destroy(c.gameObject);
            foreach(var kv in tabButtons) kv.Value.color=kv.Key==t?Chosen:Idle;
            switch(t)
            {
                case "jogar": Play(); break;
                case "personagens": Fighters(); break;
                case "loja": ShopAndEvents(); break;
                case "editor": Editor(); break;
                case "mapas": Maps(); break;
                case "estatisticas": Stats(); break;
                case "regras": Rules(); break;
                case "opcoes": Options(); break;
                default: Home(); break;
            }
        }

        // ── Início ──
        void Home()
        {
            var c=Scroll(body);
            // Modelo 017: texto e CTA à esquerda (~45%), tabuleiro transparente inteiro à direita.
            var cols=Row(c,430); cols.GetComponent<HorizontalLayoutGroup>().spacing=24; cols.GetComponent<HorizontalLayoutGroup>().childAlignment=TextAnchor.UpperLeft;
            var left=Column(cols); Layout(left.gameObject,0,0,45); var lle=left.GetComponent<LayoutElement>(); lle.preferredWidth=0; lle.minWidth=0; left.GetComponent<VerticalLayoutGroup>().spacing=12; left.GetComponent<VerticalLayoutGroup>().childAlignment=TextAnchor.MiddleLeft;
            var kicker=Label(left,"MINIATURAS TÁTICAS",13,Gold); kicker.fontStyle=FontStyle.Bold;
            var big=Label(left,"ARENA TÁTICA",52,Ink); big.fontStyle=FontStyle.Bold; Layout(big.gameObject,0,70);
            Label(left,"Miniaturas táticas no estilo Krosmaster · de 2 a 6 jogadores · no mesmo aparelho ou pela internet",17,Muted);
            var cta=Btn(left,"Jogar",()=>Show("jogar"),0,60,22); cta.GetComponent<Image>().color=Gold; cta.GetComponentInChildren<Text>().color=Bg; cta.GetComponentInChildren<Text>().fontStyle=FontStyle.Bold;
            var row=Row(left,48);
            Btn(row,"Personagens",()=>Show("personagens"),0,44,16);
            Btn(row,"Editor de fichas e skills",()=>Show("editor"),0,44,15);
            Btn(row,"Voltar à partida",Close,0,44,16);
            var art=New("Arena",cols,typeof(Image)).GetComponent<Image>(); art.preserveAspect=true; art.raycastTarget=false;
            art.sprite=Resources.Load<Sprite>("Sprites/Menu/mapa-bosque"); if(!art.sprite) art.color=Color.clear;
            Layout(art.gameObject,0,420,55); var ale=art.GetComponent<LayoutElement>(); ale.preferredWidth=0; ale.minWidth=0;
            Section(c,"Novidades");
            var card=Card(c);
            Para(card,"• <b>12 eventos de mapa</b> saem da face Cataclisma do dado: maré de lava, portal caótico, eco ancestral, maré de energia, festival de fogos, nevoeiro, vendaval, papa-moedas, o chefe Brutamontes Errante e mais.");
            Para(card,"• <b>10 lutadores novos</b>: Asamyh (Lendária, gatos comandados), O Colosso (devora skills), Cheiro D.Pneu Queimado, Rei Bobett, Príncipe Kellen (Desejo na rodada 10), Vespa Cinzenta, Viúva da Agulha, Guardião de Musgo, Domador de Correntes e Ogro de Pedra.");
            Para(card,"• <b>11 itens novos na loja</b>: martelo perfurante, corrente, bomba de fumaça, pena de fênix, totem de cura, mina de gelo, gancho, cristal de foco, manto de espinhos, âncora e bênção de pedra.");
            Para(card,"• <b>Editor</b>: mude PV, PA, PM, Nível e cada skill, e crie skills novas de dano, suporte e controle de grupo (empurrar, puxar, prender, congelar, tirar PA/PM).");
            Para(card,"<color=#a7b4c2>Algumas artes ainda estão sendo feitas pelo Codex (tarefa 015). Enquanto isso, o lutador aparece como uma figura simples.</color>");
        }

        // ── Jogar ──
        void Play()
        {
            var c=Scroll(body);
            Section(c,"Jogadores");
            var row=Row(c,44);
            for(int n=2;n<=MatchState.MaxPlayers;n++) { int k=n; Pick(Btn(row,n+" jogadores",()=>{ Game.SetSeats(k); Show("jogar"); },0,40),Game.Seats==k); }
            Section(c,"Formato");
            row=Row(c,44);
            foreach(var f in ArenaGame.Formats.Where(f=>ArenaGame.FormatFits(f,Game.Seats))) { var k=f; Pick(Btn(row,ArenaGame.FormatName(f),()=>{ Game.SetFormat(k); Show("jogar"); },0,40),Game.Format==k); }
            Section(c,"Vitória");
            row=Row(c,44);
            foreach(VictoryMode v in Enum.GetValues(typeof(VictoryMode))) { var k=v; Pick(Btn(row,VictoryName(v),()=>{ Game.Victory=k; Show("jogar"); },0,40),Game.Victory==k); }
            Para(c,VictoryHelp(Game.Victory));
            if(Game.Victory==VictoryMode.Galeoes) { row=Row(c,40); foreach(int g in new[]{8,12,16,20}) { int k=g; Pick(Btn(row,g+" Galeões",()=>{ Game.Galeoes=k; Show("jogar"); },0,36),Game.Galeoes==k); } }
            Section(c,"Cenário");
            row=Row(c,44);
            for(int th=-1;th<MapGen.Themes.Length;th++) { int k=th; Pick(Btn(row,ArenaGame.ThemeName(th),()=>{ Game.SetTheme(k); Show("jogar"); },0,40),Game.Theme==k); }
            Para(c,$"Mapa: <b>{MapLabel(Game.MapName)}</b> · troque ou crie mapas na aba Mapas. Os mapas do Codex já vêm com o cenário deles.");
            Section(c,"Adversários");
            row=Row(c,44);
            Pick(Btn(row,"Pessoas (revezando o aparelho)",()=>{ Game.VsBots=false; Show("jogar"); },0,40),!Game.VsBots);
            Pick(Btn(row,"Bots (você é o P1)",()=>{ Game.VsBots=true; Show("jogar"); },0,40),Game.VsBots);
            if(Game.VsBots) Para(c,"Você escolhe só o seu time; P2 em diante são bots com times aleatórios. No 2v2 e no 3v3, seus aliados também são bots.");
            Section(c,"Neste aparelho");
            row=Row(c,56);
            Btn(row,"Escolher lutadores e jogar",()=>{ Close(); Game.Hud.OpenPicker(); },0,52,17).GetComponent<Image>().color=Chosen;
            Btn(row,"Partida rápida (times aleatórios)",()=>{ Close(); Game.StartLocal(Enumerable.Range(0,Game.Seats).Select(_=>ArenaGame.ValidTeam(Content.Roster.OrderBy(__=>UnityEngine.Random.value))).ToArray()); },0,52,16);
            Section(c,"Com amigos");
            Para(c,$"Seu time nas salas: <b>{string.Join(", ",ArenaGame.MyTeam.Select(id=>Content.Fighters[id].name))}</b> (Nível {ArenaGame.MyTeam.Sum(id=>Content.Fighters[id].level)}/{ArenaGame.TeamLevel}). Troque na aba Personagens.");
            row=Row(c,52);
            Btn(row,"Criar sala na internet",()=>{ Close(); Game.HostRoom(); },0,48,16);
            Btn(row,"Host na rede local",()=>{ Close(); Game.Host(); },0,48,16);
            row=Row(c,52);
            var input=Input(row,code,"Código da sala (6 letras) ou ws://IP:9080"); input.onValueChanged.AddListener(v=>code=v);
            Btn(row,"Entrar",()=>{ if(string.IsNullOrWhiteSpace(code)) return; Close(); Game.Join(code.Trim()); },140,48,16);
            Para(c,"<color=#a7b4c2>A sala usa as fichas e as edições do Host. O número de jogadores e o formato valem ao criar a sala.</color>");
        }
        // ── Mapas: escolher o mapa das próximas partidas, editar, apagar e criar (editor dentro do jogo, ArenaMapEditor) ──
        void Maps()
        {
            var c=Scroll(body);
            Section(c,"Mapa das próximas partidas");
            var row=Row(c,44);
            Pick(Btn(row,"Um mapa do Codex a cada partida",()=>{ Game.MapName=""; Show("mapas"); },0,40),Game.MapName=="");
            Pick(Btn(row,"Gerado na hora (MapGen)",()=>{ Game.MapName=ArenaMaps.Generated; Show("mapas"); },0,40),Game.MapName==ArenaMaps.Generated);
            Para(c,"Um mapa vale para o número de jogadores do tamanho dele: 12×12 para 2, 16×16 para 3 ou 4, 18×18 para 5 ou 6. Na sala pela internet vale o mapa do Host. O meio é a loja (ninguém passa); a zona central são as 8 casas encostadas nela.");
            Section(c,"Mapas do Codex");
            foreach(var m in ArenaMaps.Builtin)
            {
                var map=m; row=Row(c,44);
                var title=Label(row,$"{map.name}  <color=#a7b4c2>{map.width}×{map.width} · {MapGen.PlayersFor(map.width)} jog. · {MapGen.Themes[Mathf.Max(0,map.theme)]}</color>",15,Ink,TextAnchor.MiddleLeft); Layout(title.gameObject,0,40,1);
                Pick(Btn(row,"Usar",()=>{ Game.MapName=map.name; Game.SetSeats(MapGen.PlayersFor(map.width)); Show("mapas"); },110,40),Game.MapName==map.name);
                Btn(row,"Editar cópia",()=>Game.MapEditor.Open(map),140,40);
            }
            Section(c,"Seus mapas");
            if(ArenaMaps.All.Count==0) Para(c,"<color=#a7b4c2>Nenhum mapa salvo ainda. Crie um abaixo.</color>");
            foreach(var m in ArenaMaps.All.ToList())
            {
                var map=m; row=Row(c,44);
                var name=Label(row,$"{map.name}  <color=#a7b4c2>{map.width}×{map.width} · {MapGen.PlayersFor(map.width)} jog.{(MapGen.Problem(map) is string p?" · "+p:"")}</color>",15,Ink,TextAnchor.MiddleLeft); Layout(name.gameObject,0,40,1);
                Pick(Btn(row,"Usar",()=>{ Game.MapName=map.name; Game.SetSeats(MapGen.PlayersFor(map.width)); Show("mapas"); },110,40),Game.MapName==map.name);
                Btn(row,"Editar",()=>Game.MapEditor.Open(map),110,40);
                Btn(row,"Apagar",()=>{ ArenaMaps.Delete(map.name); if(Game.MapName==map.name) Game.MapName=""; Show("mapas"); },110,40);
            }
            Section(c,"Criar mapa");
            row=Row(c,44);
            foreach(int size in new[]{12,16,18}) { int k=size; Btn(row,$"Vazio {k}×{k} ({MapGen.PlayersFor(k)} jog.)",()=>Game.MapEditor.Open(MapGen.NewMap(k)),0,40); }
            row=Row(c,44);
            foreach(int size in new[]{12,16,18}) { int k=size; Btn(row,$"A partir de um sorteado {k}×{k}",()=>Game.MapEditor.Open(MapGen.NewMap(k,(uint)Environment.TickCount|1)),0,40); }
            Para(c,"No editor: suba e desça o relevo, coloque árvores, arbustos e caixas, troque o modelo de cada peça (pinheiro, palmeira, pedra…), espalhe enfeites do cenário, marque a zona central e mova as peças tocando na peça e depois no destino. O espelho repete cada mudança na casa oposta, para o mapa ficar justo.");
        }
        static string MapLabel(string name) => name==""?"um do Codex, sorteado":name==ArenaMaps.Generated?"gerado na hora":name;
        static string VictoryName(VictoryMode v) => v==VictoryMode.Galeoes?"Galeões":v==VictoryMode.Eliminacao?"Eliminação":v==VictoryMode.Tesouro?"Tesouro":"Domínio";
        static string VictoryHelp(VictoryMode v) => v switch {
            VictoryMode.Galeoes=>"Cada nocaute tira do time os Galeões iguais ao Nível do lutador. Time sem Galeões sai.",
            VictoryMode.Eliminacao=>"Vence o último time com lutadores de pé.",
            VictoryMode.Tesouro=>"Vence quem juntar 15 moedas pegas no mapa.",
            _=>"Vence quem ficar sozinho na zona central por 5 rodadas." };

        // ── Personagens ──
        void Fighters()
        {
            var left=Box("Lista",body,Panel); left.anchorMin=Vector2.zero; left.anchorMax=new Vector2(Portrait?.42f:.3f,1); left.offsetMin=new Vector2(12,12); left.offsetMax=new Vector2(-6,-12);
            var list=Scroll(left);
            Label(list,"ESCOLHÍVEIS",12,Muted);
            foreach(var id in Content.Roster) FighterButton(list,id,()=>{ selected=id; Show("personagens"); },id==selected);
            Label(list,"INVOCAÇÕES E NEUTROS",12,Muted);
            foreach(var d in Content.Fighters.Values.Where(d=>!d.picker)) { var id=d.id; FighterButton(list,id,()=>{ selected=id; Show("personagens"); },id==selected); }
            var right=Box("Ficha",body,Panel); right.anchorMin=new Vector2(Portrait?.42f:.3f,0); right.anchorMax=Vector2.one; right.offsetMin=new Vector2(6,12); right.offsetMax=new Vector2(-12,-12);
            var c=Scroll(right);
            if(!Content.Fighters.TryGetValue(selected,out var f)) return;
            var head=Row(c,190);
            var art=Portrait2D(head,f,180);
            var info=Column(head); Layout(info.gameObject,0,190,1);
            Label(info,f.name+(f.legendary?"  ★ Lendária":""),28,Gold);
            Label(info,$"Nível {f.level}  ·  {f.hp} PV  ·  {f.pa} PA  ·  {f.pm} PM  ·  Iniciativa {f.initiative}",17,Ink);
            string traits=Content.Traits(f); if(traits!="") Label(info,traits,14,Pink);
            if(!art) Label(info,"Arte pendente: o Codex está fazendo os sprites (tarefa 015).",13,Muted);
            {
                // Toda ficha é editável (PV, PM, custo e alcance das skills, passivas), inclusive invocações e chefes.
                var row=Row(info,44); bool mine=ArenaGame.MyTeam.Contains(f.id);
                if(f.picker) Btn(row,mine?"Tirar do meu time":"Pôr no meu time",()=>{ ToggleMine(f.id); Show("personagens"); },190,40);
                Btn(row,"Editar ficha e skills",()=>{ editorId=f.id; Show("editor"); },200,40).GetComponent<Image>().color=Chosen;
            }
            Section(c,"Skills");
            foreach(var s in f.skills) Para(c,"• "+Content.Describe(s));
            if(f.forms!=null&&f.forms.Length>1) { Section(c,"Formas"); foreach(var fm in f.forms) Para(c,$"• {fm.name}{(fm.armor!=0?$" · armadura {fm.armor:+0;-0}":"")}{(fm.pa!=0?$" · {fm.pa:+0;-0} PA":"")}{(fm.pm!=0?$" · {fm.pm:+0;-0} PM":"")}"); }
            var st=ArenaStats.Data.fighters.Find(x=>x.id==f.id);
            if(st!=null&&st.games>0) { Section(c,"Neste aparelho"); Para(c,$"{st.games} partidas · {st.wins} vitórias · {st.damage} de dano · {st.kos} nocautes · {st.heal} de cura"); }
        }
        void ToggleMine(string id)
        {
            var team=ArenaGame.MyTeam.ToList();
            if(team.Contains(id)) { if(team.Count>1) team.Remove(id); }
            else { var next=ArenaGame.ValidTeam(team.Append(id)); if(!next.Contains(id)) { Game.Notify("Não cabe no time (Nível 12, até 4, 1 Lendária)."); return; } team=next.ToList(); }
            ArenaGame.MyTeam=team.ToArray();
        }
        void FighterButton(Transform list,string id,Action click,bool on)
        {
            var d=Content.Fighters[id]; bool edited=ArenaEdits.Get(id)!=null;
            var b=Btn(list,"",click,0,48); Pick(b,on);
            var icon=Art(d); var img=New("Icone",b.transform,typeof(Image)).GetComponent<Image>(); img.sprite=icon; img.preserveAspect=true; img.raycastTarget=false; if(!icon) img.color=new Color(1,1,1,0);
            var r=(RectTransform)img.transform; r.anchorMin=Vector2.zero; r.anchorMax=new Vector2(0,1); r.offsetMin=new Vector2(4,3); r.offsetMax=new Vector2(46,-3);
            var t=b.GetComponentInChildren<Text>(); t.alignment=TextAnchor.MiddleLeft; t.text=$"{d.name}{(d.legendary?" ★":"")}{(edited?" <color=#7fd1a0>●</color>":"")}\n<size=11><color=#a7b4c2>Nv {d.level} · {d.hp} PV · {d.pa} PA · {d.pm} PM</color></size>";
            ((RectTransform)t.transform).offsetMin=new Vector2(52,0);
        }
        static Sprite Art(FighterDef d) => string.IsNullOrEmpty(d.sprites)?Resources.Load<Sprite>("Sprites/Loja/"+d.id):Resources.Load<Sprite>("Sprites/"+d.sprites+"/south")??Resources.Load<Sprite>("Sprites/Loja/"+d.id);
        Image Portrait2D(Transform parent,FighterDef d,float size)
        {
            var frame=Box("Retrato",parent,Idle); Layout(frame.gameObject,size,size);
            var sp=Art(d); if(!sp) { var q=Label(frame,"?",60,Muted,TextAnchor.MiddleCenter); Fill(q.rectTransform); return null; }
            var img=New("Imagem",frame,typeof(Image)).GetComponent<Image>(); img.sprite=sp; img.preserveAspect=true; Fill((RectTransform)img.transform,6); return img;
        }

        // ── Loja e eventos ──
        static readonly Dictionary<string,string> EventHelp=new Dictionary<string,string>{
            ["chuva-de-moedas"]="3 pares de moedas caem no mapa.",
            ["queda-de-pedras"]="Caixas caem em 1 par de casas livres.",
            ["terremoto"]="Um bloco 3×3 (e o espelho dele) sobe ou desce 1 nível.",
            ["mare-de-lava"]="Lava nasce em 1 par de casas baixas e se espalha por 3 rodadas. Entrar nela ou começar o turno nela: 2 de dano (a armadura reduz).",
            ["portal-caotico"]="De 2 a 4 lutadores trocam de lugar entre si.",
            ["eco-ancestral"]="As ultimates usadas voltam (menos o Desejo) e as recargas zeram.",
            ["mare-de-energia"]="Todos ganham +1 PA e +1 PM no próximo turno.",
            ["festival-de-fogos"]="3 pares de casas ficam marcadas e explodem no fim da rodada: 2 de dano em área 1 (o Escudo e a armadura reduzem). Dá tempo de sair.",
            ["nevoeiro"]="Por 1 rodada, o alcance máximo de todas as skills cai 2 (mínimo 1).",
            ["vendaval"]="Empurra todos 1 casa numa direção. Quem bate em algo leva 1.",
            ["papa-moedas"]="2 Papa-moedas neutros comem moedas e fogem de quem chega perto. Quem derruba um ganha o que ele comeu + 2.",
            ["errante"]="O chefe Brutamontes Errante (20 PV, armadura 1) caça o mais próximo por 5 rodadas e ruge a cada 2. Derrotá-lo dá 5 moedas e um item." };
        void ShopAndEvents()
        {
            var c=Scroll(body);
            Section(c,"Loja do Castor");
            Para(c,"<color=#a7b4c2>A loja sorteia 3 produtos por jogador (peso maior aparece mais). Compre quantos puder pagar no turno, sem custo de PA.</color>");
            foreach(var d in Shop.Catalog)
            {
                var row=Row(c,0); var icon=Resources.Load<Sprite>("Sprites/Loja/"+d.id);
                var img=New("Icone",row,typeof(Image)).GetComponent<Image>(); img.sprite=icon; img.preserveAspect=true; if(!icon) img.color=Idle; Layout(img.gameObject,44,44);
                var t=Label(row,$"<b>{d.name}</b>  <color=#e8c278>{d.price} moedas</color>  <color=#a7b4c2>{KindName(d.kind)} · peso {d.weight}</color>\n{Content.Describe(d)}",14,Ink); Layout(t.gameObject,0,0,1);
            }
            Section(c,"Eventos de mapa (Cataclisma)");
            Para(c,"<color=#a7b4c2>Saem quando o dado de ataque mostra Cataclisma. Só 1 evento com duração fica ativo por vez.</color>");
            foreach(var e in Match.Cataclysms)
                Para(c,$"• <b>{ArenaPresentation.EventName(e.id)}</b> <color=#a7b4c2>({(e.rounds>0?$"{e.rounds} rodada{(e.rounds>1?"s":"")}":"na hora")} · peso {e.weight})</color>: {(EventHelp.TryGetValue(e.id,out var h)?h:"")}");
        }
        static string KindName(ShopKind k) => k==ShopKind.Item?"item permanente":k==ShopKind.Magia?"magia":k==ShopKind.Pet?"pet":k==ShopKind.Armadilha?"armadilha":"buff";

        // ── Editor de fichas e skills ──
        readonly List<Action> refreshers=new List<Action>();
        FighterDef editing; bool dirty; Text editorStatus;
        static readonly SkillKind[] EditableKinds={SkillKind.Dano,SkillKind.Cura,SkillKind.Suporte,SkillKind.Zona,SkillKind.Armadilha,SkillKind.Teleporte,SkillKind.Esconder,SkillKind.Reviver,SkillKind.Invocacao};
        static string KindName(SkillKind k) => k switch { SkillKind.Dano=>"Dano / controle", SkillKind.Cura=>"Cura", SkillKind.Suporte=>"Suporte (buff/cura em área)", SkillKind.Zona=>"Zona (gelo)",
            SkillKind.Armadilha=>"Armadilha", SkillKind.Teleporte=>"Teleporte", SkillKind.Esconder=>"Esconder", SkillKind.Reviver=>"Reviver", SkillKind.Invocacao=>"Invocação", _=>k.ToString() };
        void Editor()
        {
            if(editing==null||editing.id!=editorId) Load(editorId);
            var left=Box("Lista",body,Panel); left.anchorMin=Vector2.zero; left.anchorMax=new Vector2(Portrait?.38f:.26f,1); left.offsetMin=new Vector2(12,12); left.offsetMax=new Vector2(-6,-12);
            var list=Scroll(left);
            foreach(var d in Content.Roster.Select(id=>Content.Fighters[id]).Concat(Content.Fighters.Values.Where(d=>!d.picker)))
            { var id=d.id; FighterButton(list,id,()=>{ if(dirty&&id!=editorId) { editorStatus.text="<color=#ef6f7a>Salve ou descarte antes de trocar de ficha.</color>"; return; } editorId=id; editing=null; Show("editor"); },id==editorId); }
            var right=Box("Ficha",body,Panel); right.anchorMin=new Vector2(Portrait?.38f:.26f,0); right.anchorMax=Vector2.one; right.offsetMin=new Vector2(6,12); right.offsetMax=new Vector2(-12,-12);
            var c=Scroll(right);
            var e=editing;
            Label(c,e.name,26,Gold);
            var row=Row(c,44);
            Btn(row,"Salvar",()=>{ ArenaEdits.Put(editing); dirty=false; editorStatus.text="<color=#7fd1a0>Salvo. Vale na próxima partida (e nas salas que você hospedar).</color>"; },0,40).GetComponent<Image>().color=Chosen;
            Btn(row,"Descartar",()=>{ Load(editorId); Show("editor"); },0,40);
            Btn(row,"Padrão desta ficha",()=>{ ArenaEdits.Reset(editorId); Load(editorId); Show("editor"); },0,40);
            Btn(row,"Restaurar tudo",()=>{ ArenaEdits.ResetAll(); Load(editorId); Show("editor"); },0,40);
            editorStatus=Label(c,dirty?"<color=#e8c278>Alterações não salvas.</color>":"Mude os números com − e +. Alcance e movimento contam em casas.",13,Muted);
            Section(c,"Ficha");
            var grid=Grid(c);
            Stepper(grid,"PV",()=>e.hp,v=>e.hp=v); Stepper(grid,"PA",()=>e.pa,v=>e.pa=v); Stepper(grid,"PM (casas)",()=>e.pm,v=>e.pm=v);
            Stepper(grid,"Iniciativa",()=>e.initiative,v=>e.initiative=v); Stepper(grid,"Nível",()=>e.level,v=>e.level=v); Stepper(grid,"Armadura fixa",()=>e.armor,v=>e.armor=v);
            Flag(grid,"Voa",()=>e.flying,v=>e.flying=v); Flag(grid,"Lendária",()=>e.legendary,v=>e.legendary=v); Flag(grid,"Imóvel",()=>e.immovable,v=>e.immovable=v);
            Flag(grid,"Devora skills",()=>e.devour,v=>e.devour=v);
            Section(c,"Skills");
            Para(c,"<color=#a7b4c2>Em cada skill: PA = custo; Alcance mínimo/máximo = distância do ataque em casas.</color>");
            for(int i=0;i<e.skills.Length;i++) SkillEditor(c,e.skills[i],i);
            Section(c,"Passivas");
            var pg=Grid(c);
            Stepper(pg,"Dano extra dos aliados",()=>e.allyBonus,v=>e.allyBonus=v); Stepper(pg,"Distância desse bônus",()=>e.auraRange,v=>e.auraRange=v);
            Stepper(pg,"Renasce com PV",()=>e.revive,v=>e.revive=v); Stepper(pg,"Rouba moeda (%)",()=>e.steal,v=>e.steal=v);
            Flag(pg,"Protege aliados (2 casas)",()=>e.guard,v=>e.guard=v); Flag(pg,"Começa com ilusão",()=>e.illusion,v=>e.illusion=v);
            Para(c,"<color=#a7b4c2>Passivas livres: escolha o atributo, quem é afetado (si, aliados, inimigos, todos) e a distância em casas (0 = mapa todo, global). Valem enquanto este lutador estiver vivo. Até 6.</color>");
            e.passives??=new PassiveDef[0];
            foreach(var p in e.passives) PassiveEditor(c,p);
            var pr=Row(c,44);
            PassiveTemplate(pr,"Aura de armadura",new PassiveDef{ name="Aura de armadura", stat=PassiveStat.Armadura, who=PassiveWho.Aliados, amount=1, range=2 });
            PassiveTemplate(pr,"Fúria (+dano em si)",new PassiveDef{ name="Fúria", stat=PassiveStat.Dano, who=PassiveWho.Si, amount=1 });
            PassiveTemplate(pr,"Lentidão nos inimigos",new PassiveDef{ name="Lentidão", stat=PassiveStat.PM, who=PassiveWho.Inimigos, amount=-1, range=3 });
            PassiveTemplate(pr,"Bênção global",new PassiveDef{ name="Bênção", stat=PassiveStat.CuraPorTurno, who=PassiveWho.AliadosESi, amount=1, range=0 });
            Section(c,"Criar skill");
            Para(c,"<color=#a7b4c2>Comece de um modelo e ajuste os números e efeitos. Até 8 skills por ficha.</color>");
            var t1=Row(c,44); var t2=Row(c,44);
            Template(t1,"Dano",new SkillDef{ kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=2 });
            Template(t1,"Disparo",new SkillDef{ kind=SkillKind.Dano, pa=3, minRange=2, maxRange=5, damage=2, modifiable=true });
            Template(t1,"Cura",new SkillDef{ kind=SkillKind.Cura, pa=2, minRange=0, maxRange=3, damage=3, perTurn=1 });
            Template(t1,"Suporte (escudo)",new SkillDef{ kind=SkillKind.Suporte, pa=2, minRange=0, maxRange=3, buffArmor=1, turns=1 });
            Template(t1,"Aura em grupo",new SkillDef{ kind=SkillKind.Suporte, pa=4, minRange=0, maxRange=0, sight=false, selfArea=true, area=2, heal=2, perTurn=1 });
            Template(t2,"Empurrão",new SkillDef{ kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=1, push=2 });
            Template(t2,"Puxão",new SkillDef{ kind=SkillKind.Dano, pa=3, minRange=2, maxRange=4, damage=1, line=true, pull=true });
            Template(t2,"Prisão",new SkillDef{ kind=SkillKind.Dano, pa=3, minRange=1, maxRange=3, damage=0, prison=1, cooldown=3 });
            Template(t2,"Grito (grupo)",new SkillDef{ kind=SkillKind.Dano, pa=4, minRange=0, maxRange=0, sight=false, selfArea=true, foes=true, area=2, damage=0, pmLoss=2, paLoss=1, perMatch=1, ultimate=true });
            Template(t2,"Zona de gelo",new SkillDef{ kind=SkillKind.Zona, pa=3, minRange=1, maxRange=4, area=1, freeze=true, perTurn=1 });
            Refresh();
        }
        void Load(string id) { editing=Content.Copy(ArenaEdits.Get(id)??Content.Default(id)??Content.Fighters[id]); editorId=id; dirty=false; }
        void Changed() { dirty=true; if(editorStatus) editorStatus.text="<color=#e8c278>Alterações não salvas.</color>"; Refresh(); }
        void Refresh() { foreach(var r in refreshers) r(); }
        void Template(Transform row,string label,SkillDef model)
        {
            Btn(row,label,()=>{
                if(editing.skills.Length>=8) { editorStatus.text="<color=#ef6f7a>Limite de 8 skills.</color>"; return; }
                var s=model.Clone(); int n=1; while(editing.skills.Any(x=>x.id==$"{editing.id}-nova-{n}")) n++;
                s.id=$"{editing.id}-nova-{n}"; s.name=$"{label} {n}"; s.anim="attack";
                editing.skills=editing.skills.Append(s).ToArray(); dirty=true; Show("editor");
            },0,40,14);
        }
        void PassiveTemplate(Transform row,string label,PassiveDef model)
        {
            Btn(row,label,()=>{
                if(editing.passives.Length>=6) { editorStatus.text="<color=#ef6f7a>Limite de 6 passivas.</color>"; return; }
                editing.passives=editing.passives.Append(model.Clone()).ToArray(); dirty=true; Show("editor");
            },0,40,14);
        }
        static readonly int StatCount=Enum.GetValues(typeof(PassiveStat)).Length, WhoCount=Enum.GetValues(typeof(PassiveWho)).Length;
        static string StatLabel(PassiveStat s) => s switch { PassiveStat.Dano=>"Dano", PassiveStat.Armadura=>"Armadura", PassiveStat.PA=>"PA", PassiveStat.PM=>"PM (movimento)",
            PassiveStat.Alcance=>"Alcance", PassiveStat.CuraPorTurno=>"Cura por turno", _=>"Dano por turno" };
        static string WhoLabel(PassiveWho w) => w switch { PassiveWho.Si=>"só ele", PassiveWho.Aliados=>"aliados", PassiveWho.AliadosESi=>"ele e aliados", PassiveWho.Inimigos=>"inimigos", _=>"todos" };
        void PassiveEditor(Transform c,PassiveDef p)
        {
            var box=Box("Passiva",c,Idle); var v=box.gameObject.AddComponent<VerticalLayoutGroup>(); v.padding=new RectOffset(12,12,10,10); v.spacing=6;
            v.childControlWidth=v.childControlHeight=true; v.childForceExpandHeight=false;
            var head=Row(box,40);
            var name=Input(head,p.name,"Nome da passiva"); name.onValueChanged.AddListener(x=>{ p.name=x; Changed(); });
            Btn(head,"Remover",()=>{ editing.passives=editing.passives.Where(x=>x!=p).ToArray(); dirty=true; Show("editor"); },100,36,13).GetComponent<Image>().color=Hex(0x4a2030);
            var desc=Label(box,"",13,Ink); refreshers.Add(()=>desc.text=Content.Describe(p));
            var row=Row(box,40);
            var stat=Btn(row,"",()=>{ p.stat=(PassiveStat)(((int)p.stat+1)%StatCount); Changed(); },0,36,13);
            var who=Btn(row,"",()=>{ p.who=(PassiveWho)(((int)p.who+1)%WhoCount); Changed(); },0,36,13);
            refreshers.Add(()=>{ stat.GetComponentInChildren<Text>().text="Atributo: "+StatLabel(p.stat); who.GetComponentInChildren<Text>().text="Afeta: "+WhoLabel(p.who); });
            var grid=Grid(box);
            Stepper(grid,"Quanto (+ ou −)",()=>p.amount,x=>p.amount=Mathf.Clamp(x,-5,5)); Stepper(grid,"Distância (0 = global)",()=>p.range,x=>p.range=Mathf.Clamp(x,0,12));
        }
        void SkillEditor(Transform c,SkillDef s,int index)
        {
            var box=Box("Skill",c,Idle); var v=box.gameObject.AddComponent<VerticalLayoutGroup>(); v.padding=new RectOffset(12,12,10,10); v.spacing=6;
            v.childControlWidth=v.childControlHeight=true; v.childForceExpandHeight=false;
            var head=Row(box,40);
            var name=Input(head,s.name,"Nome da skill"); name.onValueChanged.AddListener(x=>{ s.name=x; Changed(); });
            var kind=Btn(head,"",()=>{ int k=Array.IndexOf(EditableKinds,s.kind); s.kind=EditableKinds[(k+1+EditableKinds.Length)%EditableKinds.Length]; if(s.kind==SkillKind.Invocacao&&string.IsNullOrEmpty(s.summon)) s.summon="lobo-de-cinzas"; Changed(); },230,36,13);
            refreshers.Add(()=>kind.GetComponentInChildren<Text>().text="Tipo: "+KindName(s.kind)+(Array.IndexOf(EditableKinds,s.kind)<0?" (fixo)":""));
            Btn(head,"Remover",()=>{ editing.skills=editing.skills.Where(x=>x!=s).ToArray(); dirty=true; Show("editor"); },100,36,13).GetComponent<Image>().color=Hex(0x4a2030);
            var desc=Label(box,"",13,Ink); refreshers.Add(()=>desc.text=Content.Describe(s));
            var grid=Grid(box);
            Stepper(grid,"PA",()=>s.pa,x=>s.pa=x); Stepper(grid,"Dano",()=>s.damage,x=>s.damage=x); Stepper(grid,"Cura (suporte)",()=>s.heal,x=>s.heal=x);
            Stepper(grid,"Alcance mínimo",()=>s.minRange,x=>s.minRange=x); Stepper(grid,"Alcance máximo",()=>s.maxRange,x=>s.maxRange=x); Stepper(grid,"Área",()=>s.area,x=>s.area=x);
            Stepper(grid,"Usos por turno",()=>s.perTurn,x=>s.perTurn=x); Stepper(grid,"Usos por partida",()=>s.perMatch,x=>s.perMatch=x); Stepper(grid,"Recarga (turnos)",()=>s.cooldown,x=>s.cooldown=x);
            Stepper(grid,"Empurra (casas)",()=>s.push,x=>s.push=x); Stepper(grid,"Tira PM",()=>s.pmLoss,x=>s.pmLoss=x); Stepper(grid,"Tira PA",()=>s.paLoss,x=>s.paLoss=x);
            Stepper(grid,"Prisão (turnos)",()=>s.prison,x=>s.prison=x); Stepper(grid,"Bônus se isolado",()=>s.isolatedBonus,x=>s.isolatedBonus=x); Stepper(grid,"Buff armadura",()=>s.buffArmor,x=>s.buffArmor=x);
            Stepper(grid,"Buff PA",()=>s.buffPa,x=>s.buffPa=x); Stepper(grid,"Buff PM",()=>s.buffPm,x=>s.buffPm=x); Stepper(grid,"Duração do buff",()=>s.turns,x=>s.turns=x);
            Stepper(grid,"Rodada mínima",()=>s.minRound,x=>s.minRound=x); Stepper(grid,"Cura ao acertar",()=>s.drain,x=>s.drain=x);
            Flag(grid,"Linha reta",()=>s.line,x=>s.line=x); Flag(grid,"Precisa de visão",()=>s.sight,x=>s.sight=x); Flag(grid,"+alcance no alto",()=>s.modifiable,x=>s.modifiable=x);
            Flag(grid,"Em volta de si",()=>s.selfArea,x=>{ s.selfArea=x; if(x) { s.minRange=0; s.maxRange=0; } }); Flag(grid,"Área nas 8 casas",()=>s.square,x=>s.square=x); Flag(grid,"Área só inimigos",()=>s.foes,x=>s.foes=x);
            Flag(grid,"Puxa até colar",()=>s.pull,x=>s.pull=x); Flag(grid,"Ignora armadura",()=>s.pierce,x=>s.pierce=x); Flag(grid,"Ignora Escudo",()=>s.precise,x=>s.precise=x);
            Flag(grid,"Congela casas (zona)",()=>s.freeze,x=>s.freeze=x); Flag(grid,"Pode errar",()=>s.wild,x=>s.wild=x); Flag(grid,"Ultimate",()=>s.ultimate,x=>{ s.ultimate=x; if(x&&s.perTurn==0&&s.perMatch==0&&s.cooldown==0) s.perMatch=1; });
        }
        void Stepper(Transform grid,string label,Func<int> get,Action<int> set)
        {
            var cell=New(label,grid,typeof(HorizontalLayoutGroup)); var h=cell.GetComponent<HorizontalLayoutGroup>(); h.spacing=4; h.childControlWidth=h.childControlHeight=true; h.childForceExpandWidth=false; h.childAlignment=TextAnchor.MiddleLeft;
            var l=Label(cell,label,13,Muted,TextAnchor.MiddleLeft); Layout(l.gameObject,0,0,1);
            Btn(cell,"−",()=>{ set(get()-1); Changed(); },34,30,16);
            var value=Label(cell,"",16,Ink,TextAnchor.MiddleCenter); Layout(value.gameObject,40,0);
            Btn(cell,"+",()=>{ set(get()+1); Changed(); },34,30,16);
            refreshers.Add(()=>value.text=get().ToString());
        }
        void Flag(Transform grid,string label,Func<bool> get,Action<bool> set)
        {
            var b=Btn(grid,"",()=>{ set(!get()); Changed(); },0,30,13); var t=b.GetComponentInChildren<Text>(); var img=b.GetComponent<Image>();
            refreshers.Add(()=>{ t.text=(get()?"✓ ":"   ")+label; img.color=get()?Hex(0x24503a):Idle; });
        }

        // ── Estatísticas ──
        void Stats()
        {
            var c=Scroll(body); var d=ArenaStats.Data;
            Section(c,"Neste aparelho");
            Para(c,$"<b>{d.games}</b> partidas começadas  ·  <b>{d.finished}</b> terminadas{(d.finished>0?$"  ·  {(float)d.rounds/d.finished:0.#} rodadas em média":"")}");
            if(d.modes.Count>0) Para(c,"Vitórias por modo: "+string.Join("  ·  ",d.modes.OrderByDescending(x=>x.n).Select(x=>$"{x.id} {x.n}")));
            Section(c,"Lutadores");
            var head=Row(c,26); foreach(var (h,w) in new[]{("Lutador",0f),("Partidas",90f),("Vitórias",90f),("% vit.",80f),("Dano",80f),("Nocautes",90f),("Cura",70f)}) { var l=Label(head,h,13,Muted,w==0?TextAnchor.MiddleLeft:TextAnchor.MiddleCenter); Layout(l.gameObject,w,0,w==0?1:0); }
            foreach(var f in d.fighters.Where(x=>Content.Fighters.ContainsKey(x.id)).OrderByDescending(x=>x.games).ThenByDescending(x=>x.damage))
            {
                var row=Row(c,24);
                foreach(var (v,w) in new[]{(Content.Fighters[f.id].name,0f),(f.games.ToString(),90f),(f.wins.ToString(),90f),(f.games>0?$"{100f*f.wins/f.games:0}%":"—",80f),(f.damage.ToString(),80f),(f.kos.ToString(),90f),(f.heal.ToString(),70f)})
                { var l=Label(row,v,14,Ink,w==0?TextAnchor.MiddleLeft:TextAnchor.MiddleCenter); Layout(l.gameObject,w,0,w==0?1:0); }
            }
            if(d.fighters.Count==0) Para(c,"<color=#a7b4c2>Ainda não há partidas jogadas neste aparelho.</color>");
            if(d.events.Count>0) { Section(c,"Eventos que já saíram"); Para(c,string.Join("  ·  ",d.events.OrderByDescending(x=>x.n).Select(x=>$"{ArenaPresentation.EventName(x.id)} {x.n}"))); }
            var row2=Row(c,44); Btn(row2,"Zerar estatísticas",()=>{ ArenaStats.Reset(); Show("estatisticas"); },220,40).GetComponent<Image>().color=Hex(0x4a2030);
        }

        // ── Regras ──
        void Rules()
        {
            var c=Scroll(body);
            Section(c,"Turno");
            Para(c,"Cada lutador tem PA (ações) e PM (movimento) que voltam no começo do turno dele. Andar custa 1 PM por casa; subir 2 níveis custa 2; 3 ou mais não dá. Sair de perto de um inimigo pode ser travado: ele rola Trava e você precisa de Esquiva.");
            Section(c,"Dados");
            Para(c,"Faces: Cataclisma, Escudo, Trava, Esquiva, Coringa (Esquiva/Trava) e Coringa. No ataque rolam 2 dados: o de quem ataca e o de quem defende. Escudo do defensor tira 1 de dano. Cataclisma no dado de ataque dispara um evento de mapa.");
            Section(c,"Times");
            Para(c,"Soma de Nível até 12, até 4 lutadores, sem repetir e só 1 Lendária. Os times jogam em rodízio pela Iniciativa.");
            Section(c,"Altura e visão");
            Para(c,"Mais alto que o alvo: +1 de alcance nas skills marcadas. Relevo 2 níveis acima do mais baixo dos dois bloqueia a visão; árvores e caixas também. Lutadores não bloqueiam, mas um tiro pode acertar quem está no caminho.");
            Section(c,"Controle e suporte");
            Para(c,"Empurrão: o alvo anda em linha; se bater, leva 1 (e quem foi atingido também). Puxão: traz o alvo até colar. Prisão: o alvo perde o próximo turno e não pode ser alvo (Esquiva escapa). Gelo: entrar na casa custa +1 PM. Buffs de armadura duram até o próximo turno do alvo.");
            Section(c,"Loja");
            Para(c,"Moedas ficam no mapa e são pegas ao passar. A loja mostra 3 produtos por jogador; compre quantos puder pagar no turno, sem gastar PA.");
            Section(c,"Vitória");
            Para(c,"Galeões, Eliminação, Tesouro ou Domínio, escolhidos na aba Jogar.");
        }

        // ── Opções ──
        void Options()
        {
            var c=Scroll(body);
            Section(c,"Tela");
            var row=Row(c,48);
            if(!Application.isMobilePlatform) Btn(row,"Tela cheia (F11)",ArenaHud.ToggleFullScreen,220,44);
            Section(c,"Som");
            row=Row(c,48);
            foreach(var (name,vol) in new[]{("Mudo",0f),("Baixo",.35f),("Médio",.7f),("Alto",1f)}) { float k=vol; Pick(Btn(row,name,()=>{ AudioListener.volume=k; PlayerPrefs.SetFloat("arena-volume",k); Show("opcoes"); },0,44),Mathf.Approximately(AudioListener.volume,k)); }
            Section(c,"Jogo");
            row=Row(c,48);
            Btn(row,"Voltar à partida",Close,0,44);
            Btn(row,"Sair do jogo",Application.Quit,0,44).GetComponent<Image>().color=Hex(0x4a2030);
            Para(c,$"<color=#a7b4c2>Edições salvas em {Application.persistentDataPath}</color>");
        }
        void Awake() { AudioListener.volume=PlayerPrefs.GetFloat("arena-volume",1); }

        // ── Peças de interface ──
        RectTransform New(string name,Transform parent,params Type[] comps)
        {
            var go=new GameObject(name,new[]{typeof(RectTransform)}.Concat(comps).ToArray()); go.transform.SetParent(parent,false); return (RectTransform)go.transform;
        }
        RectTransform Box(string name,Transform parent,Color color) { var r=New(name,parent,typeof(Image)); r.GetComponent<Image>().color=color; return r; }
        static void Fill(RectTransform r,float pad=0) { r.anchorMin=Vector2.zero; r.anchorMax=Vector2.one; r.offsetMin=new Vector2(pad,pad); r.offsetMax=new Vector2(-pad,-pad); }
        static void Layout(GameObject go,float w,float h,float flex=0)
        {
            var le=go.GetComponent<LayoutElement>()??go.AddComponent<LayoutElement>();
            if(w>0) { le.preferredWidth=w; le.minWidth=w; } if(h>0) { le.preferredHeight=h; le.minHeight=h; } le.flexibleWidth=flex;
        }
        Text Label(Transform parent,string text,int size,Color color,TextAnchor align=TextAnchor.UpperLeft)
        {
            var t=New("Texto",parent,typeof(Text)).GetComponent<Text>(); t.font=font; t.text=text; t.fontSize=size; t.color=color; t.alignment=align;
            t.supportRichText=true; t.horizontalOverflow=HorizontalWrapMode.Wrap; t.verticalOverflow=VerticalWrapMode.Overflow; t.raycastTarget=false; return t;
        }
        void Section(Transform parent,string text) { var t=Label(parent,text.ToUpperInvariant(),13,Gold); t.fontStyle=FontStyle.Bold; Layout(t.gameObject,0,30); t.alignment=TextAnchor.LowerLeft; }
        void Para(Transform parent,string text) => Label(parent,text,15,Ink);
        Button Btn(Transform parent,string text,Action action,float w=0,float h=38,int size=15)
        {
            var r=Box(text,parent,Idle); var b=r.gameObject.AddComponent<Button>(); b.targetGraphic=r.GetComponent<Image>();
            ArenaTema.StyleFocus(b); b.onClick.AddListener(()=>action());
            var t=Label(r,text,size,Ink,TextAnchor.MiddleCenter); Fill(t.rectTransform,4); t.verticalOverflow=VerticalWrapMode.Truncate;
            var le=r.gameObject.AddComponent<LayoutElement>(); le.preferredHeight=le.minHeight=h;
            if(w>0) { le.preferredWidth=w; le.minWidth=w*.6f; } else { le.flexibleWidth=1; le.minWidth=60; }
            return b;
        }
        static void Pick(Button b,bool on) { if(on) { b.GetComponent<Image>().color=Chosen; var o=b.GetComponent<Outline>()??b.gameObject.AddComponent<Outline>(); o.effectColor=Gold; o.effectDistance=new Vector2(ArenaTema.FocusOutline,-ArenaTema.FocusOutline); } }
        InputField Input(Transform parent,string value,string hint)
        {
            var r=Box("Campo",parent,Hex(0x110d24)); var input=r.gameObject.AddComponent<InputField>();
            var text=Label(r,value,15,Ink,TextAnchor.MiddleLeft); Fill(text.rectTransform,8); text.supportRichText=false;
            var ph=Label(r,hint,14,Muted,TextAnchor.MiddleLeft); Fill(ph.rectTransform,8);
            input.textComponent=text; input.placeholder=ph; input.text=value; input.characterLimit=120;
            var le=r.gameObject.AddComponent<LayoutElement>(); le.flexibleWidth=1; le.minWidth=140; le.minHeight=34; return input;
        }
        RectTransform Row(Transform parent,float h)
        {
            var r=New("Linha",parent,typeof(HorizontalLayoutGroup)); var g=r.GetComponent<HorizontalLayoutGroup>(); g.spacing=8; g.childAlignment=TextAnchor.MiddleLeft;
            g.childControlWidth=g.childControlHeight=true; g.childForceExpandWidth=false; g.childForceExpandHeight=false;
            if(h>0) Layout(r.gameObject,0,h); return r;
        }
        RectTransform Column(Transform parent)
        {
            var r=New("Coluna",parent,typeof(VerticalLayoutGroup)); var g=r.GetComponent<VerticalLayoutGroup>(); g.spacing=6; g.childControlWidth=g.childControlHeight=true; g.childForceExpandHeight=false; return r;
        }
        // Painel do modelo 017: fundo, borda e respiro dos tokens.
        RectTransform Card(Transform parent)
        {
            var r=Box("Painel",parent,Panel); var g=r.gameObject.AddComponent<VerticalLayoutGroup>(); g.padding=new RectOffset(18,18,14,14); g.spacing=8;
            g.childControlWidth=g.childControlHeight=true; g.childForceExpandWidth=true; g.childForceExpandHeight=false;
            var o=r.gameObject.AddComponent<Outline>(); o.effectColor=Edge; o.effectDistance=new Vector2(2,-2); return r;
        }
        RectTransform Grid(Transform parent)
        {
            var r=New("Campos",parent,typeof(GridLayoutGroup)); var g=r.GetComponent<GridLayoutGroup>();
            g.cellSize=new Vector2(Portrait?300:262,32); g.spacing=new Vector2(10,4); g.constraint=GridLayoutGroup.Constraint.FixedColumnCount; g.constraintCount=Portrait?1:3; return r;
        }
        RectTransform Scroll(Transform parent)
        {
            var view=Box("Rolagem",parent,new Color(0,0,0,0)); Fill(view); view.gameObject.AddComponent<RectMask2D>();
            var content=New("Itens",view,typeof(VerticalLayoutGroup),typeof(ContentSizeFitter));
            content.anchorMin=new Vector2(0,1); content.anchorMax=Vector2.one; content.pivot=new Vector2(.5f,1); content.offsetMin=content.offsetMax=Vector2.zero;
            var v=content.GetComponent<VerticalLayoutGroup>(); v.padding=new RectOffset(18,18,14,24); v.spacing=8; v.childControlWidth=v.childControlHeight=true; v.childForceExpandWidth=true; v.childForceExpandHeight=false;
            content.GetComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            var sr=view.gameObject.AddComponent<ScrollRect>(); sr.content=content; sr.viewport=view; sr.horizontal=false; sr.scrollSensitivity=40; sr.movementType=ScrollRect.MovementType.Clamped;
            return content;
        }
    }
}
