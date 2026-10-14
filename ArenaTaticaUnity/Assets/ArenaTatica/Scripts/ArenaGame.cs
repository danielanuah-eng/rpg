using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ArenaTatica.Core;

namespace ArenaTatica
{
    // Protocolo 5: atributos e skills sorteados por lutador, com autoridade do Host.
    // hello{token} sobe e recebe welcome{assento}; a sala manda lobby até começar; depois o estado filtrado
    // por jogador e os eventos descem e os comandos sobem. O Host decide tudo.
    [Serializable] public class WireMessage
    {
        public int protocol=5;
        public string kind, text, token;
        public string[] fighters; // hello: time escolhido no aparelho do jogador
        public string[] pool;     // hello/escolha: pool do torneio
        public int seat, seats;
        public MatchState state;
        public Command command;
        public GameEvent[] events;
        public long ping;
    }
    public sealed class ArenaGame : MonoBehaviour
    {
        // O runner usa uma partida limpa em memória; nunca altera as preferências do jogador.
        static bool SmokeFixture => !Application.isEditor&&Array.IndexOf(Environment.GetCommandLineArgs(),"--arena-smoke")>=0&&(Application.isBatchMode||Array.IndexOf(Environment.GetCommandLineArgs(),"--arena-qa-render")>=0);
        static string[] smokeTeam={"ciantia"}, smokePool={"ciantia","perry","ezequiela","damian","baltazar"};
        public Match Match { get; private set; }
        public MatchState State { get; private set; }
        public ArenaTransport Network { get; private set; }
        public ArenaPresentation View { get; private set; }
        public ArenaHud Hud { get; private set; }
        public ArenaMenu Menu { get; private set; }
        public ArenaMapEditor MapEditor { get; private set; }
        // Mapa salvo usado nas próximas partidas (aba Mapas); vazio = sorteado.
        public string MapName="";
        public bool EditingMap => MapEditor!=null&&MapEditor.Active;
        // Modo de vitória e Galeões escolhidos no menu (Jogar).
        public VictoryMode Victory=VictoryMode.Galeoes;
        public int Galeoes=12;
        public string Status { get; private set; }="Escolha Mover ou uma skill e clique no tabuleiro.";
        // "" = Mover; senão o id da skill escolhida.
        public string Mode="";
        public bool InMenu { get; private set; }
        public bool RandomSkills, RandomStats, RandomBots=true, RandomPlayers;
        public void SetRandomizer(bool skills,bool stats,bool bots,bool players)
        {
            if(Client||(Network.IsHost&&!Lobby)) return;
            RandomSkills=skills; RandomStats=stats; RandomBots=bots; RandomPlayers=players; Changed();
        }
        public bool Recovery, Synced=true;
        public int Rtt, Filtered;
        // Sala: assentos (2..6), formato e se ainda espera o início. Seat = assento deste aparelho na rede.
        public int Seats { get; private set; }=2;
        public string Format { get; private set; }="todos";
        // Cenário escolhido no menu; -1 = aleatório.
        public int Theme { get; private set; }=-1;
        public bool Lobby { get; private set; }
        public int Seat { get; private set; }
        public int Filled => 1+tokenSeat.Count;
        // "equipes": times livres, cada assento escolhe o time (CustomTeams), de 2 a 6 jogadores.
        public static readonly string[] Formats={"todos","2v2","3v3","2v2v2","equipes"};
        public static readonly int[] CustomTeams={1,2,1,2,1,2};
        // Ordem dos turnos e itens da loja escolhidos no menu (Jogar/Sala); valem na próxima partida local ou sala.
        // Lidos no Awake: PlayerPrefs não pode ser lido no construtor de um MonoBehaviour.
        public TurnMode Turns;
        public void SetTurns(TurnMode t) { Turns=t; if(!SmokeFixture) { PlayerPrefs.SetInt("arena-turnos",(int)t); PlayerPrefs.Save(); } Changed(); }
        public static readonly HashSet<string> ShopItems=new HashSet<string>();
        public static void SaveShopItems() { if(!SmokeFixture) { PlayerPrefs.SetString("arena-loja",string.Join(",",ShopItems)); PlayerPrefs.Save(); } }
        // Sala em 3 passos (autor, 13/10): Setup = configurando, sem rede; Liberar abre a sala (Lobby); Iniciar começa.
        // AutoStart: só por linha de comando (testes de rede), começa ao encher como antes.
        public bool Setup { get; private set; }
        public bool AutoStart;
        public int LobbyVersion { get; private set; }
        public string LobbyText { get; private set; }="";
        public string RoomName => room;
        void Changed() { LobbyVersion++; if(Network&&Network.IsHost&&Lobby) BroadcastLobby(); }
        // Torneio (autor, 13/10): até 6 jogadores, cada um com uma pool de lutadores; em cada partida (mata-mata, 1 contra 1)
        // cada jogador bane até Bans lutadores da pool do outro e depois monta o time com o que sobrou da sua.
        public bool Tournament;
        public int Bans=2;
        public const int PoolSize=8, MaxBans=2;
        public static string[] MyPool
        {
            get
            {
                if(SmokeFixture) return smokePool.ToArray();
                var saved=PlayerPrefs.GetString("arena-pool","");
                var pool=ValidPool(saved.Split(',')); var value=string.Join(",",pool);
                if(saved!=value) { PlayerPrefs.SetString("arena-pool",value); PlayerPrefs.Save(); }
                return pool;
            }
            set { if(SmokeFixture) { smokePool=ValidPool(value); return; } PlayerPrefs.SetString("arena-pool",string.Join(",",ValidPool(value))); PlayerPrefs.Save(); }
        }
        // Pool: lutadores escolhíveis, sem repetir, até PoolSize; pequena demais completa com sorteados.
        public static string[] ValidPool(IEnumerable<string> ids)
        {
            var pool=(ids??Array.Empty<string>()).Where(id=>Content.Roster.Contains(id)).Distinct().Take(PoolSize).ToList();
            foreach(var id in Content.Roster.OrderBy(_=>UnityEngine.Random.value)) { if(pool.Count>=MaxBans+3) break; if(!pool.Contains(id)) pool.Add(id); }
            return pool.ToArray();
        }
        public sealed class Cup
        {
            public List<int> entrants=new List<int>(), winners=new List<int>();
            public List<int[]> round=new List<int[]>();
            public int match=-1, a, b, champion, number, recordedWinner;
            public string phase="";
            public bool recorded;
            public readonly Dictionary<int,string[]> bans=new Dictionary<int,string[]>(), picks=new Dictionary<int,string[]>();
            public readonly List<string> log=new List<string>();
        }
        public Cup Torneio { get; private set; }
        readonly Dictionary<string,string[]> tokenPool=new Dictionary<string,string[]>();
        // Assento da sala → time na partida em curso (torneio: só os 2 da partida jogam; 0 = assiste).
        public int MatchTeam(int seat) => Torneio!=null&&Torneio.a>0?(seat==Torneio.a?1:seat==Torneio.b?2:0):seat;
        int HostTeam => MatchTeam(1);
        public float ExpiredPercent => probes.Count==0?0:100f*expired/probes.Count;
        public bool Client => !Network.IsHost&&Network.Role!="Local";
        public Fighter Active => State?.Active;
        // Armadilhas visíveis: Host vê as do P1 e aliados, cliente já recebe filtrado, jogo local mostra todas (0).
        public int ViewerTeam => Client?Seat:Network.IsHost?HostTeam:0;
        public int LocalTeam => Client?Seat:Network.IsHost?HostTeam:BotsPlaying?1:(Active?.team??1);
        public bool CanAct => !InMenu&&Menu?.IsOpen!=true&&Synced&&!Recovery&&!Lobby&&State!=null&&!State.over&&Active!=null&&Active.team==LocalTeam&&!IsBot(Active.team)&&View.Ready&&!View.Busy;
        // Partida local contra bots (menu Jogar): P1 é a pessoa; P2 em diante, bots (Core.Bot). Em rede ainda não há bots.
        public bool VsBots;
        bool BotsPlaying => VsBots&&Network!=null&&Network.Role=="Local";
        public bool IsBot(int seat) => BotsPlaying&&seat>=2&&seat<=MatchState.MaxPlayers;
        float botWait; int botActions, botFighter=-1, botRevision=-1;
        // Regras de leitura (alcance, alvos) também no cliente, sobre o estado recebido.
        public Match Preview => Match??(preview!=null&&preview.S==State?preview:preview=Match.FromState(State,networkPreview:Client));
        Match preview;
        readonly Dictionary<string,float> throttle=new Dictionary<string,float>();
        readonly Queue<bool> probes=new Queue<bool>();
        // Host: token do jogador → assento; assento ↔ conexão.
        readonly Dictionary<string,int> tokenSeat=new Dictionary<string,int>();
        readonly Dictionary<int,int> seatPeer=new Dictionary<int,int>(), peerSeat=new Dictionary<int,int>();
        string room, tokenOverride;
        // Times escolhidos: Picks por assento na partida local; tokenPick pelo token na sala de rede.
        public string[][] Picks;
        readonly Dictionary<string,string[]> tokenPick=new Dictionary<string,string[]>();
        // Regra do Krosmaster: soma de Nível até 12, sem repetir lutador; no máximo 4 (casas de início); 1 Lendária por time.
        public const int TeamLevel=12, TeamSize=4;
        public static string[] ValidTeam(IEnumerable<string> ids)
        {
            var team=new List<string>(); int level=0;
            foreach(string id in ids??Array.Empty<string>()) {
                if(team.Count>=TeamSize||team.Contains(id)||!Content.Roster.Contains(id)) continue;
                if(Content.Fighters[id].legendary&&team.Any(x=>Content.Fighters[x].legendary)) continue;
                int l=Content.Fighters[id].level; if(level+l>TeamLevel) continue;
                team.Add(id); level+=l;
            }
            return team.Count>0?team.ToArray():new[]{"ciantia"};
        }
        // Time deste aparelho (usado na rede e como sugestão no seletor), salvo entre sessões.
        public static string[] MyTeam
        {
            get => ValidTeam(SmokeFixture?smokeTeam:PlayerPrefs.GetString("arena-time","ciantia").Split(','));
            set { if(SmokeFixture) { smokeTeam=ValidTeam(value); return; } PlayerPrefs.SetString("arena-time",string.Join(",",ValidTeam(value))); PlayerPrefs.Save(); }
        }
        // Seletor: times de cada assento local; a partida recomeça com eles.
        public void StartLocal(string[][] picks)
        {
            Picks=picks.Select(ValidTeam).ToArray(); MyTeam=Picks[0];
            // Bots sem time escolhido: times aleatórios dentro da regra de Nível.
            if(VsBots&&Picks.Length<Seats) Picks=Picks.Concat(Enumerable.Range(0,Seats-Picks.Length).Select(_=>ValidTeam(Content.Roster.OrderBy(__=>UnityEngine.Random.value)))).ToArray();
            NewLocal();
        }
        float lastPing,pingSent; long pingId; bool pingWaiting;
        int expired;

        void Awake()
        {
            Application.targetFrameRate=60; Application.runInBackground=true; ArenaMaps.Reset();
            Turns=SmokeFixture?TurnMode.Completo:(TurnMode)Mathf.Clamp(PlayerPrefs.GetInt("arena-turnos",0),0,1);
            ShopItems.Clear(); if(!SmokeFixture) foreach(var id in PlayerPrefs.GetString("arena-loja","").Split(',')) if(Shop.Def(id)!=null) ShopItems.Add(id);
            for(int i=0;i<CustomTeams.Length;i++) CustomTeams[i]=SmokeFixture?i%2+1:Mathf.Clamp(PlayerPrefs.GetInt("arena-equipe-"+i,i%2+1),1,3);
            NewMatch(Seats);
            Network=gameObject.AddComponent<ArenaTransport>();
            View=gameObject.AddComponent<ArenaPresentation>(); View.Game=this;
            Hud=gameObject.AddComponent<ArenaHud>(); Hud.Game=this;
            Menu=gameObject.AddComponent<ArenaMenu>(); Menu.Game=this;
            MapEditor=gameObject.AddComponent<ArenaMapEditor>(); MapEditor.Game=this;
            Network.Message+=Receive;
            // Na Web copia o link de convite (abre o jogo já entrando); nos outros, o código (o campo aceita os dois).
            Network.RoomReady+=code=> { GUIUtility.systemCopyBuffer=Application.platform==RuntimePlatform.WebGLPlayer?InviteLink(Application.absoluteURL,code):code; Hud.ShowRoom(code); room=code; LobbyStatus(); Changed(); };
            Application.deepLinkActivated+=url=> { if(RoomFromLink(url) is string linked) Join(linked); };
            Network.Connected+=peer=> {
                if(Network.IsHost) return; // o assento só sai depois do hello
                Recovery=false; throttle.Clear(); Synced=false; Status="Conectado. Entrando na sala…";
                Send(0,new WireMessage{kind="hello",token=Token(),fighters=MyTeam,pool=MyPool});
            };
            Network.Disconnected+=peer=> {
                if(Network.IsHost) { Left(peer); return; }
                Recovery=true; Synced=false; Status="Conexão perdida. Entre de novo com o mesmo código ou inicie uma partida local.";
            };
            Network.Failed+=message=> { Status=message; Debug.LogWarning("Arena network: "+message); if(!Network.IsHost&&!Network.IsConnected) { Recovery=true; Synced=false; } };
        }
        public static int[] FormatTeams(string format,int players)
        {
            if(format=="2v2"&&players==4) return new[]{1,2,1,2};
            if(format=="3v3"&&players==6) return new[]{1,2,1,2,1,2};
            if(format=="2v2v2"&&players==6) return new[]{1,2,3,1,2,3};
            // Equipes livres: precisa de pelo menos 2 times diferentes.
            if(format=="equipes"&&players>=2) { var t=CustomTeams.Take(players).ToArray(); if(t.Distinct().Count()>=2) return t; }
            return null;
        }
        public void CycleCustomTeam(int seat)
        {
            int i=seat-1; if(i<0||i>=CustomTeams.Length) return;
            CustomTeams[i]=CustomTeams[i]%3+1; if(!SmokeFixture) { PlayerPrefs.SetInt("arena-equipe-"+i,CustomTeams[i]); PlayerPrefs.Save(); } Changed();
        }
        public static bool FormatFits(string format,int players) => format=="todos"||FormatTeams(format,players)!=null;
        public static string FormatName(string format) => format=="todos"?"Todos contra todos":format=="equipes"?"Equipes livres":"Equipes "+format;
        void NewMatch(int players,bool flat=false,Func<int,string[]> pick=null,string format=null)
        {
            var teams=FormatTeams(format??Format,players);
            // As edições salvas neste aparelho valem para a partida local e para a sala que ele hospeda.
            Content.ApplyEdits(SmokeFixture?null:ArenaEdits.Saved);
            pick??=seat=>Picks!=null&&seat<=Picks.Length?Picks[seat-1]:MyTeam;
            // Mapa salvo escolhido no menu: só vale se for do tamanho deste número de jogadores.
            // Sorteado: um dos mapas do Codex do tamanho certo; ArenaMaps.Generated: o gerador do MapGen.
            uint seed=SmokeFixture?19013u:(uint)Environment.TickCount|1; int size=Board.SizeFor(players);
            var map=flat||MapName==ArenaMaps.Generated?null:MapName==""?ArenaMaps.Pick(size,seed):ArenaMaps.Find(MapName);
            if(map!=null&&!MapGen.Fits(map,size)) { Debug.LogWarning($"Mapa {MapName} é para {MapGen.PlayersFor(map.width)} jogadores; usando um dos mapas do Codex."); map=ArenaMaps.Pick(size,seed); }
            var setup=new MatchSetup{ seed=seed, flatMap=flat, theme=Theme, mode=Victory, galeoes=Galeoes, map=map, turns=Turns, shopItems=ShopItems.ToArray(),
                randomSkills=RandomSkills,randomStats=RandomStats,randomBots=RandomBots,randomPlayers=RandomPlayers,
                botSeats=BotsPlaying?Enumerable.Range(2,players-1).ToArray():Array.Empty<int>(),
                players=Enumerable.Range(0,players).Select(i=>new PlayerSetup{team=teams?[i]??0,fighters=ValidTeam(pick(i+1))}).ToArray() };
            Match=Match.New(setup);
            State=Match.S; preview=null; Mode=""; InMenu=false;
            ArenaStats.Begin();
        }
        // Menu (Jogar): escolhas que valem para a próxima partida local ou sala.
        public void SetSeats(int n) { if(Network.Role!="Local") { Status="Feche a sala para mudar o número de jogadores."; return; } Seats=Mathf.Clamp(n,2,MatchState.MaxPlayers); if(!FormatFits(Format,Seats)) Format="todos"; Changed(); }
        public void SetFormat(string f) { if(f=="equipes"||FormatFits(f,Seats)) Format=f; Changed(); }
        public void SetTheme(int t) { Theme=Mathf.Clamp(t,-1,MapGen.Themes.Length-1); Changed(); }
        public void SetVictory(VictoryMode v) { Victory=v; Changed(); }
        public void SetGaleoes(int g) { Galeoes=g; Changed(); }
        public void SetTournament(bool on) { if(Network.IsHost&&!Lobby) return; Tournament=on; Changed(); }
        public void SetBans(int n) { Bans=Mathf.Clamp(n,0,MaxBans); Changed(); }
        // Loja da sala: ShopItems vazio = todos os itens. Sempre sobra pelo menos 1 item ligado.
        public static bool ShopItemOn(string id) => ShopItems.Count==0||ShopItems.Contains(id);
        public void ToggleShopItem(string id)
        {
            if(ShopItems.Count==0) foreach(var d in Shop.Catalog) ShopItems.Add(d.id);
            if(ShopItems.Contains(id)) { if(ShopItems.Count>1) ShopItems.Remove(id); } else ShopItems.Add(id);
            if(ShopItems.Count==Shop.Catalog.Length) ShopItems.Clear();
            SaveShopItems(); Changed();
        }
        public void AllShopItems() { ShopItems.Clear(); SaveShopItems(); Changed(); }
        void Start() { View.Sync(State,true); StartCoroutine(CommandLineRun()); }
        void Update()
        {
            if(Client&&Network.IsConnected) {
                if(pingWaiting&&Time.unscaledTime-pingSent>3) { RecordProbe(true); pingWaiting=false; }
                if(!pingWaiting&&Time.unscaledTime-lastPing>1) { lastPing=pingSent=Time.unscaledTime; pingId++; pingWaiting=true; Send(0,new WireMessage{kind="ping",ping=pingId}); }
            }
            DriveBot();
        }
        // Turno do bot: uma ação por vez, depois da animação da anterior; no máximo 16 por turno (depois passa).
        void DriveBot()
        {
            var f=Active;
            if(InMenu||Match==null||State==null||State.over||f==null||!IsBot(f.team)||!View.Ready||View.Busy) { botWait=0; return; }
            if(f.id!=botFighter||State.revision<botRevision) { botFighter=f.id; botActions=0; }
            botWait+=Time.deltaTime; if(botWait<.35f) return; botWait=0;
            var cmd=botActions++>=16?new Command{kind="passar"}:Bot.Decide(Match);
            cmd.fighter=f.id; cmd.revision=State.revision; botRevision=State.revision;
            if(!Match.Apply(cmd,f.team,out var events,out var reason)) {
                Debug.LogWarning($"Bot P{f.team} ({f.def}) {cmd.kind} {cmd.skill} recusado: {reason}");
                cmd=new Command{kind="passar",fighter=f.id,revision=State.revision};
                if(!Match.Apply(cmd,f.team,out events,out reason)) return;
            }
            State=Match.S; Mode="";
            View.Play(events,State); ArenaStats.Record(events,State);
            if(f!=Active) botFighter=-1;
        }
        void RecordProbe(bool lost) { probes.Enqueue(lost); if(lost) expired++; if(probes.Count>60&&probes.Dequeue()) expired--; }
        void ResetMetrics() { Rtt=0; Filtered=0; expired=0; probes.Clear(); pingWaiting=false; lastPing=Time.unscaledTime; }
        void ResetRoom() { tokenPick.Clear(); tokenPool.Clear(); tokenSeat.Clear(); seatPeer.Clear(); peerSeat.Clear(); Lobby=false; Seat=0; }
        public void Notify(string text) { Status=text; }
        // Editor de mapas: o tabuleiro mostra o mapa sendo editado, sem lutadores nem partida.
        public void BeginMapEdit(MatchState board)
        {
            StopAllCoroutines(); Network.Close(); ResetRoom(); Recovery=false; Synced=true;
            Match=null; preview=null; State=board; Mode=""; View.ResetView(State);
            Status="Editor de mapas: escolha uma ferramenta e toque nas casas.";
        }
        public void NewLocal()
        {
            if(EditingMap) MapEditor.Close(false);
            StopAllCoroutines(); Network.Close(); ResetRoom(); Recovery=false; Synced=true;
            Setup=false; Torneio=null; if(!FormatFits(Format,Seats)) Format="todos";
            NewMatch(Seats); throttle.Clear(); ResetMetrics(); View.ResetView(State);
            Status=VsBots?$"Partida contra {Seats-1} bot{(Seats>2?"s":"")} ({FormatName(Format)}). Você é o P1.":$"Partida local: {Seats} jogadores neste aparelho ({FormatName(Format)}).";
        }
        // Número de jogadores só muda fora da rede (as vagas do Relay são fixadas ao criar a sala).
        public void CycleSeats()
        {
            if(Network.Role!="Local") { Status="Feche a sala para mudar o número de jogadores."; return; }
            Seats=Seats>=MatchState.MaxPlayers?2:Seats+1; NewLocal();
        }
        public void CycleFormat()
        {
            if(Client||(Network.IsHost&&!Lobby)) { Status="O formato só muda antes de a partida começar."; return; }
            var fits=Formats.Where(f=>FormatFits(f,Seats)).ToList();
            Format=fits[(fits.IndexOf(Format)+1)%fits.Count];
            if(Network.IsHost) { BroadcastLobby(); LobbyStatus(); } else NewLocal();
        }
        public static string ThemeName(int theme) => theme<0?"Cenário aleatório":MapGen.Themes[theme];
        public void CycleTheme()
        {
            if(Client||(Network.IsHost&&!Lobby)) { Status="O cenário só muda antes de a partida começar."; return; }
            Theme=Theme+1>=MapGen.Themes.Length?-1:Theme+1;
            if(Network.IsHost) LobbyStatus(); else NewLocal();
        }
        public void ResumeLocal()
        {
            if(Client) { NewLocal(); return; }
            Network.Close(); ResetRoom(); ResetMetrics(); Recovery=false; Synced=true; Status="Partida retomada localmente.";
        }
        // Passo 1: abrir a sala para configurar (modo, jogadores, formato, turnos, loja, torneio), ainda sem rede e sem partida.
        public void OpenLobby()
        {
            if(EditingMap) MapEditor.Close(false);
            if(Network.Role!="Local") { Network.Close(); ResetRoom(); }
            Torneio=null; Setup=true; Recovery=false; Synced=true; Mode=""; LobbyVersion++;
            Status="Configure a sala e toque em Liberar.";
            Menu?.Open("sala");
        }
        // Passo 2: liberar (LAN ou internet). A partida só começa no Iniciar (passo 3).
        public void Release(bool internet)
        {
            StopAllCoroutines(); Network.Close(); ResetRoom(); ResetMetrics(); Torneio=null; Recovery=false; Synced=true;
            if(!FormatFits(Format,Seats)&&!Tournament) Format="todos";
            if(internet) { Network.HostRoom(Seats); room=null; Status="Criando sala na internet…"; }
            else { Network.Host(); if(!Network.IsHost) { Status="Não foi possível iniciar Host."; return; } room="LAN"; }
            Setup=false; Lobby=true; Seat=1; LobbyStatus(); Changed();
        }
        public void CloseLobby() { Network.Close(); ResetRoom(); Setup=false; Torneio=null; LobbyVersion++; Status="Sala fechada."; }
        // Linha de comando e testes de rede: liberam direto e começam ao encher.
        public void Host() { AutoStart=true; Release(false); }
        public void HostRoom() { AutoStart=true; Release(true); }
        // Caminho até a sala do Relay: código de 6 letras, link da página (?sala=CÓDIGO) ou deep link (arenatatica://sala/CÓDIGO),
        // vindo do campo Entrar, da URL ao abrir na Web, do deep link no Android ou de --arena-room-join.
        public static string RoomFromLink(string text)
        {
            text=text?.Trim(); if(string.IsNullOrEmpty(text)) return null;
            if(ArenaTransport.ValidRoomCode(text)) return text.ToUpperInvariant();
            var m=System.Text.RegularExpressions.Regex.Match(text,@"(?:[?&#]sala=|^arenatatica://sala/)([A-Za-z0-9]{6})(?![A-Za-z0-9])",System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return m.Success?m.Groups[1].Value.ToUpperInvariant():null;
        }
        public static string InviteLink(string page,string code) =>
            string.IsNullOrEmpty(page)||!page.StartsWith("http",StringComparison.OrdinalIgnoreCase)?"arenatatica://sala/"+code:page.Split('?','#')[0]+"?sala="+code;
        public void Join(string address)
        {
            if(RoomFromLink(address) is string code) { JoinRoom(code); return; }
            Leave(); room=address?.Trim(); Status="Conectando ao Host…"; Network.Join(address);
        }
        void JoinRoom(string code)
        {
            Leave(); room=code.Trim().ToUpperInvariant();
            Status="Entrando na sala "+room+"…"; Network.JoinRoom(code);
        }
        void Leave() { if(EditingMap) MapEditor.Close(false); StopAllCoroutines(); throttle.Clear(); ResetMetrics(); ResetRoom(); Recovery=false; Synced=false; Match=null; preview=null; View.CancelAnimations(); }
        // Token salvo por sala: quem fecha e reabre o jogo volta ao mesmo assento.
        string Token()
        {
            if(tokenOverride!=null) return tokenOverride;
            if(SmokeFixture) return tokenOverride=Guid.NewGuid().ToString("N");
            string key="arena-token:"+room, token=PlayerPrefs.GetString(key,"");
            if(token=="") { token=Guid.NewGuid().ToString("N"); PlayerPrefs.SetString(key,token); PlayerPrefs.Save(); }
            return token;
        }

        void LobbyStatus()
        {
            if(!Network.IsHost||!Lobby) return;
            string where=room=="LAN"?"Host LAN na porta 9080":room==null?"Sala":$"Sala {room} (código copiado)";
            Status=$"{where} · {Filled}/{Seats} jogadores · {(Tournament?"Torneio":FormatName(Format))}. Toque em Iniciar quando todos estiverem prontos.";
        }
        string SeatToken(int seat) => tokenSeat.FirstOrDefault(kv=>kv.Value==seat).Key;
        string[] TeamOf(int seat) => seat==1?MyTeam:SeatToken(seat) is string t&&tokenPick.TryGetValue(t,out var p)?p:null;
        public string[] PoolOf(int seat) => seat==1?MyPool:SeatToken(seat) is string t&&tokenPool.TryGetValue(t,out var p)?p:ValidPool(null);
        static string Names(IEnumerable<string> ids) => string.Join(", ",(ids??Array.Empty<string>()).Where(Content.Fighters.ContainsKey).Select(id=>Content.Fighters[id].name));
        // Resumo da sala (Host monta; os clientes recebem pronto no "lobby").
        public string LobbySummary()
        {
            var lines=new List<string>();
            string where=room=="LAN"?"Rede local (porta 9080)":string.IsNullOrEmpty(room)?"Internet (criando…)":$"Código <b>{room}</b>";
            lines.Add($"{where} · {Filled}/{Seats} jogadores");
            var seats=new[]{1}.Concat(tokenSeat.Values.OrderBy(x=>x)).Distinct().ToList();
            foreach(int s in seats)
            {
                string who=s==1?"P1 (anfitrião)":$"P{s}", team=Format=="equipes"&&!Tournament?$" · Time {CustomTeams[s-1]}":"";
                lines.Add(Tournament?$"• {who}{team} · pool: {Names(PoolOf(s))}":$"• {who}{team} · {Names(TeamOf(s))}");
            }
            lines.Add("");
            lines.Add(Tournament?$"Modo: <b>Torneio</b> (mata-mata 1 contra 1, até {Bans} ban{(Bans==1?"":"s")} por jogador)":$"Formato: <b>{FormatName(Format)}</b>");
            lines.Add($"Turnos: <b>{(Turns==TurnMode.Alternada?"Iniciativa alternada":"Jogador completo")}</b> · Vitória: <b>{Victory}</b> · {ThemeName(Theme)}");
            lines.Add($"Loja: {(ShopItems.Count==0?"todos os itens":$"{ShopItems.Count} itens ({string.Join(", ",ShopItems.Select(id=>Shop.Def(id).name))})")}");
            if(Torneio!=null) { lines.Add(""); lines.Add("<b>TORNEIO</b>"); lines.AddRange(Torneio.log); }
            if(RandomSkills||RandomStats) lines.Add($"Aleatorizador: {(RandomSkills?"Skills ":"")}{(RandomStats?"Atributos limitados ":"")}· {(RandomBots&&RandomPlayers?"todos":RandomBots?"bots":RandomPlayers?"jogadores":"nenhum participante")}");
            return string.Join("\n",lines);
        }
        void BroadcastLobby()
        {
            LobbyText=LobbySummary(); LobbyVersion++;
            foreach(var kv in seatPeer) Send(kv.Value,new WireMessage{kind="lobby",seat=kv.Key,seats=Seats,text=LobbyText});
        }
        void Hello(int peer,string token,string[] fighters,string[] pool)
        {
            if(string.IsNullOrEmpty(token)||token.Length>64||peerSeat.ContainsKey(peer)) return;
            if(Lobby&&Torneio==null) { tokenPick[token]=ValidTeam(fighters); tokenPool[token]=ValidPool(pool); }
            if(!tokenSeat.TryGetValue(token,out int seat))
            {
                if(Lobby) for(int s=2;s<=Seats&&seat==0;s++) if(!tokenSeat.ContainsValue(s)) seat=s;
                // Sem assento: o cliente fecha a conexão sozinho ao receber "full".
                if(seat==0) { Send(peer,new WireMessage{kind="full",text=Lobby?"A sala está cheia.":"A partida já começou."}); return; }
                tokenSeat[token]=seat;
            }
            if(seatPeer.TryGetValue(seat,out int old)&&old!=peer) { peerSeat.Remove(old); Network.Kick(old); }
            seatPeer[seat]=peer; peerSeat[peer]=seat;
            int team=Lobby?seat:MatchTeam(seat);
            Send(peer,new WireMessage{kind="welcome",seat=team>0?team:seat,seats=Lobby?Seats:State.players});
            if(Lobby) { BroadcastLobby(); LobbyStatus(); if(AutoStart&&Filled>=Seats) StartRoom(); return; }
            // Torneio: quem não está na partida (ou está no draft) recebe a sala/o draft de novo.
            if(team==0||Torneio!=null&&Torneio.phase!="") { Send(peer,new WireMessage{kind="lobby",seat=seat,seats=Seats,text=LobbyText}); if(team>0) SendDraft(); return; }
            Send(peer,new WireMessage{kind="snapshot",seat=team,state=Match.ViewFor(State,team)});
            Status=$"P{seat} voltou à partida.";
        }
        // Cliente trocou time/pool na sala.
        void Choice(int peer,string[] fighters,string[] pool)
        {
            if(!Lobby||Torneio!=null||!peerSeat.TryGetValue(peer,out int seat)) return;
            var token=SeatToken(seat); if(token==null) return;
            if(fighters!=null&&fighters.Length>0) tokenPick[token]=ValidTeam(fighters);
            if(pool!=null&&pool.Length>0) tokenPool[token]=ValidPool(pool);
            BroadcastLobby();
        }
        // Este aparelho trocou o time/pool: Host atualiza a sala; cliente avisa o Host.
        public void SendChoice()
        {
            if(Network.IsHost&&Lobby) BroadcastLobby();
            else if(Client&&Network.IsConnected) Send(0,new WireMessage{kind="escolha",fighters=MyTeam,pool=MyPool});
            LobbyVersion++;
        }
        void Left(int peer)
        {
            if(!peerSeat.TryGetValue(peer,out int seat)) return;
            peerSeat.Remove(peer); if(seatPeer.TryGetValue(seat,out int p)&&p==peer) seatPeer.Remove(seat);
            if(Lobby&&Torneio==null) { foreach(var t in tokenSeat.Where(kv=>kv.Value==seat).Select(kv=>kv.Key).ToList()) tokenSeat.Remove(t); BroadcastLobby(); LobbyStatus(); return; }
            Status=$"P{seat} desconectado. A partida espera a volta dele com o mesmo código.";
        }
        // Começa com quem está na sala; os assentos ocupados ficam em sequência (P1, P2, P3…).
        public void StartRoom()
        {
            if(!Network.IsHost||!Lobby) return;
            if(Torneio!=null) { NextTournamentMatch(); return; }
            int n=Filled;
            if(n<2) { Status="Espere pelo menos mais 1 jogador."; return; }
            if(!Tournament&&!FormatFits(Format,n)) { Status=$"{FormatName(Format)} não fecha com {n} jogadores. Troque o formato ou espere."; return; }
            var order=tokenSeat.OrderBy(kv=>kv.Value).ToList(); var oldPeer=new Dictionary<int,int>(seatPeer);
            tokenSeat.Clear(); seatPeer.Clear(); peerSeat.Clear();
            for(int i=0;i<order.Count;i++) {
                int seat=i+2; tokenSeat[order[i].Key]=seat;
                if(oldPeer.TryGetValue(order[i].Value,out int peer)) { seatPeer[seat]=peer; peerSeat[peer]=seat; }
            }
            if(Tournament) { BeginTournament(n); return; }
            Lobby=false; NewMatch(n,pick:TeamOf); throttle.Clear(); View.ResetView(State); Menu?.Close();
            foreach(var kv in seatPeer) {
                Send(kv.Value,new WireMessage{kind="welcome",seat=kv.Key,seats=n});
                Send(kv.Value,new WireMessage{kind="snapshot",seat=kv.Key,state=Match.ViewFor(State,kv.Key)});
            }
            Status=$"Partida com {n} jogadores ({FormatName(Format)}). Você é P1.";
        }
        // Reiniciar sem recriar a sala (autor, 13/10): mesma sala, mesmos jogadores e times, partida nova na hora.
        public void RestartRoom()
        {
            if(!Network.IsHost) { Status="Só o anfitrião reinicia a partida."; return; }
            if(Torneio!=null&&Torneio.a>0) {
                if(Torneio.phase!=""||Torneio.champion>0) { Status="Conclua as escolhas ou volte à sala para iniciar outro torneio."; return; }
                if(Torneio.recorded) {
                    Torneio.winners.Remove(Torneio.recordedWinner);
                    Torneio.log.RemoveAll(line=>line.StartsWith($"Partida {Torneio.number}: <b>"));
                    Torneio.recordedWinner=0;
                }
                Torneio.recorded=false; StartTournamentMatch(); return;
            }
            Lobby=true; StartRoom();
        }
        // Volta todos para a sala (trocar times, formato, loja) sem desconectar ninguém.
        public void BackToLobby()
        {
            if(!Network.IsHost) return;
            Lobby=true; Torneio=null; Mode=""; BroadcastLobby(); LobbyStatus(); Menu?.Open("sala");
        }

        // Fechar só a partida: Host devolve todos ao lobby; cliente sai; local volta ao menu.
        public void CloseMatch()
        {
            StopAllCoroutines(); Mode=""; botWait=0; InMenu=true;
            if(Network.IsHost) { BackToLobby(); Status="Partida fechada. Todos voltaram à sala."; return; }
            Network.Close(); ResetRoom(); Setup=false; Torneio=null; Recovery=false; Synced=true;
            throttle.Clear(); ResetMetrics(); LobbyVersion++; Status="Partida fechada."; Menu?.Open("jogar");
        }

        // ── Torneio ──
        void BeginTournament(int n)
        {
            Torneio=new Cup(); Torneio.entrants.AddRange(Enumerable.Range(1,n).OrderBy(_=>UnityEngine.Random.value));
            Torneio.log.Add($"Chaveamento sorteado: {string.Join(", ",Torneio.entrants.Select(s=>"P"+s))}.");
            NextRound(); NextTournamentMatch();
        }
        void NextRound()
        {
            var t=Torneio; t.round.Clear(); t.winners.Clear(); t.match=-1;
            for(int i=0;i+1<t.entrants.Count;i+=2) t.round.Add(new[]{t.entrants[i],t.entrants[i+1]});
            // Ímpar: o último passa direto (bye).
            if(t.entrants.Count%2==1) { int bye=t.entrants[t.entrants.Count-1]; t.winners.Add(bye); t.log.Add($"P{bye} passa direto nesta fase."); }
        }
        public void NextTournamentMatch()
        {
            var t=Torneio; if(t==null||!Network.IsHost) return;
            if(t.phase!="") { Status="Conclua os bans e a escolha dos times antes de avançar."; return; }
            if(t.a>0&&State!=null&&!State.over&&!Lobby) { Status="A partida do torneio ainda está em andamento."; return; }
            t.match++; t.a=t.b=0; t.phase=""; t.recorded=false; t.recordedWinner=0;
            if(t.match>=t.round.Count)
            {
                if(t.winners.Count<=1) { t.champion=t.winners.FirstOrDefault(); t.log.Add($"<color=#e8c278>Campeão: P{t.champion}!</color>"); Lobby=true; BroadcastLobby(); Status=$"Torneio encerrado. Campeão: P{t.champion}."; Menu?.Open("sala"); return; }
                t.entrants=new List<int>(t.winners); NextRound(); t.match=0;
            }
            t.a=t.round[t.match][0]; t.b=t.round[t.match][1]; t.number++;
            t.bans.Clear(); t.picks.Clear(); t.phase=Bans>0?"ban":"pick";
            t.log.Add($"Partida {t.number}: P{t.a} × P{t.b}.");
            Lobby=true; BroadcastLobby(); SendDraft();
            Status=$"Torneio · partida {t.number}: P{t.a} × P{t.b} ({(t.phase=="ban"?"bans":"escolhas")}).";
        }
        // Draft: no "ban" cada um bane até Bans da pool do outro; no "pick" monta o time com o que sobrou da sua.
        public string[] DraftOptions(int seat)
        {
            var t=Torneio; int other=seat==t.a?t.b:t.a;
            if(t.phase=="ban") return PoolOf(other);
            var banned=t.bans.TryGetValue(other,out var b)?b:Array.Empty<string>();
            return PoolOf(seat).Where(id=>!banned.Contains(id)).ToArray();
        }
        void SendDraft()
        {
            var t=Torneio; if(t==null||t.phase=="") return;
            foreach(int seat in new[]{t.a,t.b})
            {
                if((t.phase=="ban"?t.bans:t.picks).ContainsKey(seat)) continue;
                var options=DraftOptions(seat); int limit=t.phase=="ban"?Bans:TeamSize;
                if(seat==1) Hud?.OpenDraft(t.phase,options,limit,ids=>DraftChoice(1,t.phase,ids));
                else if(seatPeer.TryGetValue(seat,out int peer)) Send(peer,new WireMessage{kind="draft",text=t.phase,fighters=options,seats=limit,seat=seat});
            }
        }
        void DraftChoice(int seat,string phase,string[] ids)
        {
            var t=Torneio; if(t==null||phase!=t.phase||(seat!=t.a&&seat!=t.b)) return;
            var options=DraftOptions(seat); ids=(ids??Array.Empty<string>()).Where(options.Contains).Distinct().ToArray();
            if(phase=="ban") t.bans[seat]=ids.Take(Bans).ToArray();
            else { var team=ValidTeam(ids); if(!team.All(options.Contains)) team=ValidTeam(options.OrderBy(_=>UnityEngine.Random.value)); t.picks[seat]=team; }
            var done=phase=="ban"?t.bans:t.picks;
            if(!done.ContainsKey(t.a)||!done.ContainsKey(t.b)) { Status=$"P{seat} escolheu. Esperando o outro jogador."; return; }
            if(phase=="ban")
            {
                t.log.Add($"P{t.a} baniu {(t.bans[t.a].Length==0?"ninguém":Names(t.bans[t.a]))}; P{t.b} baniu {(t.bans[t.b].Length==0?"ninguém":Names(t.bans[t.b]))}.");
                t.phase="pick"; BroadcastLobby(); SendDraft(); return;
            }
            t.phase=""; StartTournamentMatch();
        }
        void StartTournamentMatch()
        {
            var t=Torneio; if(t==null||t.a==0||t.phase!=""||!t.picks.ContainsKey(t.a)||!t.picks.ContainsKey(t.b)) return;
            Lobby=false; NewMatch(2,pick:p=>p==1?t.picks[t.a]:t.picks[t.b],format:"todos"); throttle.Clear(); View.ResetView(State);
            foreach(var kv in seatPeer) {
                int team=MatchTeam(kv.Key);
                if(team==0) { Send(kv.Value,new WireMessage{kind="lobby",seat=kv.Key,seats=Seats,text=LobbySummary()+$"\n\nResumo: P{t.a} × P{t.b} está em andamento (sem transmissão da partida)."}); continue; }
                Send(kv.Value,new WireMessage{kind="welcome",seat=team,seats=2});
                Send(kv.Value,new WireMessage{kind="snapshot",seat=team,state=Match.ViewFor(State,team)});
            }
            if(HostTeam>0) Menu?.Close();
            Status=$"Torneio · partida {t.number}: P{t.a} ({Names(t.picks[t.a])}) × P{t.b} ({Names(t.picks[t.b])}).{(HostTeam==0?" Você está assistindo.":"")}";
        }
        // Fim de uma partida do torneio: guarda o vencedor; o anfitrião chama a próxima em Iniciar/Próxima partida.
        void RecordTournament()
        {
            var t=Torneio; if(t==null||t.a==0||t.recorded||State==null||!State.over) return;
            t.recorded=true; int w=State.winner==1?t.a:State.winner==2?t.b:(UnityEngine.Random.value<.5f?t.a:t.b);
            t.recordedWinner=w; t.winners.Add(w); t.log.Add($"Partida {t.number}: <b>P{w} venceu</b>{(State.winner==0?" (empate decidido no sorteio)":"")}.");
            Status=$"P{w} venceu a partida {t.number} do torneio. Toque em Próxima partida.";
            LobbyVersion++;
        }

        public void Submit(string kind,Cell cell=default,string skill=null,int slot=0)
        {
            if(!CanAct) { Status=Lobby?"A partida ainda não começou.":Recovery?"Retome a partida ou reconecte.":"Aguarde seu turno e o fim da animação."; return; }
            var cmd=new Command{kind=kind,cell=cell,skill=skill,slot=slot,fighter=Active.id,revision=State.revision};
            if(!Allow("local:"+kind)) return;
            if(Client) Send(0,new WireMessage{kind="command",command=cmd}); else Execute(cmd,LocalTeam,-1);
        }
        bool Allow(string key)
        {
            if(throttle.TryGetValue(key,out var t)&&Time.unscaledTime-t<.12f) { Filtered++; return false; }
            throttle[key]=Time.unscaledTime; return true;
        }
        // team: time na partida; peer: conexão de quem mandou (-1 = este aparelho).
        void Execute(Command cmd,int team,int peer)
        {
            if(Recovery||Match==null) return;
            var hiddenBefore=State.fighters.Where(f=>f.hidden).Select(f=>f.id).ToArray();
            if(Match.Apply(cmd,team,out var events,out var reason)) {
                State=Match.S; Mode="";
                Status=cmd.kind=="passar"?"Turno encerrado.":cmd.kind=="comprar"?"Compra feita.":"Ação confirmada.";
                View.Play(events,State); ArenaStats.Record(events,State);
                if(Network.IsHost) foreach(var kv in seatPeer) { int t=MatchTeam(kv.Key); if(t==0) continue;
                    Send(kv.Value,new WireMessage{kind="state",state=Match.ViewFor(State,t),events=Match.EventsFor(events,State,t,hiddenBefore).ToArray()}); }
                if(Network.IsHost&&State.over) RecordTournament();
            } else { Status=reason; if(Network.IsHost&&peer>=0) Send(peer,new WireMessage{kind="reject",text=reason}); }
        }
        void Send(int peer,WireMessage message) { Network.Send(peer,JsonUtility.ToJson(message)); }
        public static bool ValidState(MatchState s,int viewer=0)
        {
            const int n=MatchState.MaxPlayers+1;
            if(s==null||!Board.ValidSize(s.width)||s.height!=s.width||s.players<2||s.players>MatchState.MaxPlayers||s.tiles==null||s.tiles.Length!=s.width*s.height) return false;
            if(s.fighters==null||s.fighters.Count==0||s.fighters.Count>64||s.revision<0||s.timeline==null||s.timeline.Any(id=>s.F(id)==null)) return false;
            if(s.galeoes?.Length!=n||s.coins?.Length!=n||s.collected?.Length!=n||s.domain?.Length!=n||s.eliminated?.Length!=n||s.alliance?.Length!=n||s.shops?.Length!=n||s.shops.Any(x=>x==null)) return false;
            for(int p=1;p<=s.players;p++) if(s.alliance[p]<1||s.alliance[p]>s.players) return false;
            var ids=new HashSet<int>(); var cells=new HashSet<Cell>();
            foreach(var f in s.fighters) {
                if(f==null||!ids.Add(f.id)||(f.team<1||f.team>s.players)&&f.team!=MatchState.Neutral||f.def==null||!Content.Fighters.ContainsKey(f.def)) return false;
                if(f.roll is NumericRoll r) {
                    if(r.powers==null||r.powers.Length>8||r.powers.Any(p=>p==null||Content.AnySkill(p.id)==null)||r.powers.Select(p=>p.id).Distinct().Count()!=r.powers.Length) return false;
                    if(r.stats&&(r.hp<4||r.hp>30||r.pa<5||r.pa>8||r.pm<1||r.pm>6||r.initiative<0||r.initiative>12||r.powers.Any(p=>p.damage<0||p.damage>8||p.range<0||p.range>8))) return false;
                }
                bool concealed=viewer>0&&f.hidden&&!s.Ally(f.team,viewer)&&f.cell==Cell.Unknown;
                if(f.hp<0||f.hp>Content.MaxHp(f)||f.pa<0||f.pm<0||(!concealed&&!Board.Contains(s,f.cell))) return false;
                if(f.Alive&&!concealed&&!cells.Add(f.cell)) return false;
            }
            return true;
        }
        void Receive(int peer,string text)
        {
            WireMessage m;
            try { m=JsonUtility.FromJson<WireMessage>(text); } catch(Exception) { return; }
            if(m==null) return;
            if(m.protocol!=5) {
                const string update="Versões diferentes. Atualize a Arena Tática nos dois aparelhos.";
                if(Network.IsHost&&m.kind=="hello") Send(peer,new WireMessage{protocol=m.protocol,kind="full",text=update});
                else if(Client) { Network.Close(); Recovery=true; Synced=false; Status=update; }
                return;
            }
            if(m.kind=="ping") { if(Allow("remote:ping:"+peer)) Send(peer,new WireMessage{kind="pong",ping=m.ping}); return; }
            if(m.kind=="pong") { if(pingWaiting&&m.ping==pingId) { Rtt=Mathf.RoundToInt((Time.unscaledTime-pingSent)*1000); RecordProbe(false); pingWaiting=false; } return; }
            if(Network.IsHost) {
                if(m.kind=="hello") { Hello(peer,m.token,m.fighters,m.pool); return; }
                if(m.kind=="escolha"&&Allow("remote:escolha:"+peer)) { Choice(peer,m.fighters,m.pool); return; }
                if(m.kind=="draft"&&Torneio!=null&&peerSeat.TryGetValue(peer,out int who)) { DraftChoice(who,m.text,m.fighters); return; }
                if(!Lobby&&peerSeat.TryGetValue(peer,out int seat)&&MatchTeam(seat)>0&&m.kind=="command"&&m.command!=null&&new[]{"mover","skill","comprar","passar"}.Contains(m.command.kind)&&Allow("remote:"+seat+":"+m.command.kind)) Execute(m.command,MatchTeam(seat),peer);
                return;
            }
            if(m.kind=="full") { Network.Close(); Recovery=true; Synced=false; Status=m.text??"A sala está cheia."; return; }
            if(m.seats>=2&&m.seats<=MatchState.MaxPlayers) Seats=m.seats;
            // Torneio: o cliente pode ser o time 1 da partida.
            if(m.kind=="welcome") { InMenu=false; if(m.seat>=1&&m.seat<=MatchState.MaxPlayers) Seat=m.seat; return; }
            if(m.kind=="lobby") { if(m.seat>=2&&m.seat<=MatchState.MaxPlayers) Seat=m.seat; Lobby=true; LobbyText=m.text??""; LobbyVersion++; Status=$"Na sala como P{Seat}. Aguardando o anfitrião iniciar."; if(Hud==null||!Hud.PickerOpen) Menu?.Open("sala"); return; }
            if(m.kind=="draft") { var phase=m.text; Hud?.OpenDraft(phase,m.fighters,m.seats,ids=>Send(0,new WireMessage{kind="draft",text=phase,fighters=ids})); Status=phase=="ban"?"Torneio: escolha quem banir.":"Torneio: monte seu time."; return; }
            if(m.kind=="reject") { Status=m.text??"Ação recusada."; return; }
            if(m.kind!="snapshot"&&m.kind!="state") return;
            // Fichas do Host valem na sala (o cliente confere PV máximos e skills com elas).
            if(m.state!=null) Content.ApplyEdits(m.state.edits);
            if(Seat==0||!ValidState(m.state,Seat)||Seat>m.state.players||(!Synced&&m.kind!="snapshot")||(Synced&&State!=null&&m.state.revision<=State.revision&&m.kind!="snapshot")) return;
            bool initial=m.kind=="snapshot";
            State=m.state; preview=null; Synced=true; Recovery=false; Lobby=false; Mode="";
            if(initial) { View.ResetView(State); ArenaStats.Begin(); Menu?.Close(); } else { var list=m.events?.ToList()??new List<GameEvent>(); View.Play(list,State); ArenaStats.Record(list,State); }
            Status=$"Partida sincronizada. Você é P{Seat}.";
        }
        // Conferência da câmera sem editor: partida contra bots, uma imagem por vista (4 lados) e fecha.
        // NewLocal para todas as corrotinas: a partida começa antes e a captura depois.
        void BoardShots(string prefix)
        {
            // A captura não troca o time salvo deste aparelho (StartLocal grava o do P1).
            var mine=MyTeam; Menu?.Close(); VsBots=true; StartLocal(new[]{new[]{"damian","baltazar"},new[]{"ogro-de-pedra","perry"}}); MyTeam=mine;
            StartCoroutine(Shots(System.IO.Path.GetFullPath(prefix)));
        }
        IEnumerator Shots(string prefix)
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(prefix+"-x.png"));
            yield return new WaitForSecondsRealtime(2.5f);
            // Renderiza a câmera do tabuleiro numa textura (funciona também com -batchmode, sem janela).
            var cam=View.BoardCamera; var rt=new RenderTexture(1600,900,24); var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);
            for(int v=0;v<4;v++) {
                var old=cam.targetTexture; cam.targetTexture=rt; cam.Render(); cam.targetTexture=old;
                RenderTexture.active=rt; tex.ReadPixels(new Rect(0,0,1600,900),0,0); tex.Apply(); RenderTexture.active=null;
                System.IO.File.WriteAllBytes($"{prefix}-vista{v}.png",tex.EncodeToPNG()); Debug.Log("ARENA_SHOT "+v);
                View.RotateView(1); yield return new WaitForSecondsRealtime(.8f);
            }
            Application.Quit();
        }
        IEnumerator CommandLineRun()
        {
            yield return null;
            string[] args=Environment.GetCommandLineArgs();
            // Opções primeiro: valem para o Host/Join que vem depois.
            for(int i=0;i<args.Length-1;i++) {
                if(args[i]=="--arena-seats"&&int.TryParse(args[i+1],out int seats)) Seats=Mathf.Clamp(seats,2,MatchState.MaxPlayers);
                if(args[i]=="--arena-format"&&Formats.Contains(args[i+1])) Format=args[i+1];
                if(args[i]=="--arena-theme"&&int.TryParse(args[i+1],out int th)) Theme=Mathf.Clamp(th,-1,MapGen.Themes.Length-1);
                if(args[i]=="--arena-token") tokenOverride=args[i+1];
            }
            for(int i=0;i<args.Length;i++) {
                if(args[i]=="--arena-host") Host();
                if(args[i]=="--arena-join"&&i+1<args.Length) Join(args[++i]);
                if(args[i]=="--arena-room") HostRoom();
                if(args[i]=="--arena-room-join"&&i+1<args.Length) Join(args[++i]);
                if(args[i]=="--arena-smoke") gameObject.AddComponent<ArenaSmoke>();
                if(args[i]=="--arena-shot") BoardShots(i+1<args.Length?args[i+1]:"Artifacts/tabuleiro");
            }
            // Aberto por um convite: página com ?sala= ou deep link que iniciou o app.
            if(RoomFromLink(Application.absoluteURL) is string invited) { Menu.Close(); Join(invited); }
        }
    }
}
