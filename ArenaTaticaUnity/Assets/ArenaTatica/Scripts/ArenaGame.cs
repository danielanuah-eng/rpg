using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ArenaTatica.Core;

namespace ArenaTatica
{
    // Protocolo 3: hello{token} sobe e recebe welcome{assento}; a sala manda lobby até começar; depois o estado filtrado
    // por jogador e os eventos descem e os comandos sobem. O Host decide tudo.
    [Serializable] public class WireMessage
    {
        public int protocol=3;
        public string kind, text, token;
        public string[] fighters; // hello: time escolhido no aparelho do jogador
        public int seat, seats;
        public MatchState state;
        public Command command;
        public GameEvent[] events;
        public long ping;
    }
    public sealed class ArenaGame : MonoBehaviour
    {
        public Match Match { get; private set; }
        public MatchState State { get; private set; }
        public ArenaTransport Network { get; private set; }
        public ArenaPresentation View { get; private set; }
        public ArenaHud Hud { get; private set; }
        public string Status { get; private set; }="Escolha Mover ou uma skill e clique no tabuleiro.";
        // "" = Mover; senão o id da skill escolhida.
        public string Mode="";
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
        public static readonly string[] Formats={"todos","2v2","3v3","2v2v2"};
        public float ExpiredPercent => probes.Count==0?0:100f*expired/probes.Count;
        public bool Client => !Network.IsHost&&Network.Role!="Local";
        public Fighter Active => State?.Active;
        // Armadilhas visíveis: Host vê as do P1 e aliados, cliente já recebe filtrado, jogo local mostra todas (0).
        public int ViewerTeam => Client?Seat:Network.IsHost?1:0;
        public int LocalTeam => Client?Seat:Network.IsHost?1:(Active?.team??1);
        public bool CanAct => Synced&&!Recovery&&!Lobby&&State!=null&&!State.over&&Active!=null&&Active.team==LocalTeam&&View.Ready&&!View.Busy;
        // Regras de leitura (alcance, alvos) também no cliente, sobre o estado recebido.
        public Match Preview => Match??(preview!=null&&preview.S==State?preview:preview=Match.FromState(State));
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
        // Regra do Krosmaster: soma de Nível até 12, sem repetir lutador; no máximo 4 (casas de início).
        public const int TeamLevel=12, TeamSize=4;
        public static string[] ValidTeam(IEnumerable<string> ids)
        {
            var team=new List<string>(); int level=0;
            foreach(string id in ids??Array.Empty<string>()) {
                if(team.Count>=TeamSize||team.Contains(id)||!Content.Roster.Contains(id)) continue;
                int l=Content.Fighters[id].level; if(level+l>TeamLevel) continue;
                team.Add(id); level+=l;
            }
            return team.Count>0?team.ToArray():new[]{"ciantia"};
        }
        // Time deste aparelho (usado na rede e como sugestão no seletor), salvo entre sessões.
        public static string[] MyTeam
        {
            get => ValidTeam(PlayerPrefs.GetString("arena-time","ciantia").Split(','));
            set { PlayerPrefs.SetString("arena-time",string.Join(",",ValidTeam(value))); PlayerPrefs.Save(); }
        }
        // Seletor: times de cada assento local; a partida recomeça com eles.
        public void StartLocal(string[][] picks) { Picks=picks.Select(ValidTeam).ToArray(); MyTeam=Picks[0]; NewLocal(); }
        float lastPing,pingSent; long pingId; bool pingWaiting;
        int expired;

        void Awake()
        {
            Application.targetFrameRate=60; Application.runInBackground=true;
            NewMatch(Seats);
            Network=gameObject.AddComponent<ArenaTransport>();
            View=gameObject.AddComponent<ArenaPresentation>(); View.Game=this;
            Hud=gameObject.AddComponent<ArenaHud>(); Hud.Game=this;
            Network.Message+=Receive;
            Network.RoomReady+=code=> { GUIUtility.systemCopyBuffer=code; Hud.ShowRoom(code); room=code; LobbyStatus(); };
            Network.Connected+=peer=> {
                if(Network.IsHost) return; // o assento só sai depois do hello
                Recovery=false; throttle.Clear(); Synced=false; Status="Conectado. Entrando na sala…";
                Send(0,new WireMessage{kind="hello",token=Token(),fighters=MyTeam});
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
            return null;
        }
        public static bool FormatFits(string format,int players) => format=="todos"||FormatTeams(format,players)!=null;
        public static string FormatName(string format) => format=="todos"?"Todos contra todos":format;
        void NewMatch(int players,bool flat=false,Func<int,string[]> pick=null)
        {
            var teams=FormatTeams(Format,players);
            pick??=seat=>Picks!=null&&seat<=Picks.Length?Picks[seat-1]:MyTeam;
            var setup=new MatchSetup{ seed=(uint)Environment.TickCount|1, flatMap=flat, theme=Theme,
                players=Enumerable.Range(0,players).Select(i=>new PlayerSetup{team=teams?[i]??0,fighters=ValidTeam(pick(i+1))}).ToArray() };
            Match=Match.New(setup);
            State=Match.S; preview=null; Mode="";
        }
        void Start() { View.Sync(State,true); StartCoroutine(CommandLineRun()); }
        void Update()
        {
            if(Client&&Network.IsConnected) {
                if(pingWaiting&&Time.unscaledTime-pingSent>3) { RecordProbe(true); pingWaiting=false; }
                if(!pingWaiting&&Time.unscaledTime-lastPing>1) { lastPing=pingSent=Time.unscaledTime; pingId++; pingWaiting=true; Send(0,new WireMessage{kind="ping",ping=pingId}); }
            }
        }
        void RecordProbe(bool lost) { probes.Enqueue(lost); if(lost) expired++; if(probes.Count>60&&probes.Dequeue()) expired--; }
        void ResetMetrics() { Rtt=0; Filtered=0; expired=0; probes.Clear(); pingWaiting=false; lastPing=Time.unscaledTime; }
        void ResetRoom() { tokenPick.Clear(); tokenSeat.Clear(); seatPeer.Clear(); peerSeat.Clear(); Lobby=false; Seat=0; }
        public void Notify(string text) { Status=text; }
        public void NewLocal()
        {
            StopAllCoroutines(); Network.Close(); ResetRoom(); Recovery=false; Synced=true;
            if(!FormatFits(Format,Seats)) Format="todos";
            NewMatch(Seats); throttle.Clear(); ResetMetrics(); View.ResetView(State);
            Status=$"Partida local: {Seats} jogadores neste aparelho ({FormatName(Format)}).";
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
        public void Host() { NewLocal(); Network.Host(); if(!Network.IsHost) { Status="Não foi possível iniciar Host."; return; } Lobby=true; Seat=1; room="LAN"; LobbyStatus(); }
        public void HostRoom() { NewLocal(); Network.HostRoom(Seats); Lobby=true; Seat=1; Status="Criando sala na internet…"; }
        public void Join(string address)
        {
            if(ArenaTransport.ValidRoomCode(address)) { JoinRoom(address); return; }
            Leave(); room=address?.Trim(); Status="Conectando ao Host…"; Network.Join(address);
        }
        void JoinRoom(string code)
        {
            Leave(); room=code.Trim().ToUpperInvariant();
            Status="Entrando na sala "+room+"…"; Network.JoinRoom(code);
        }
        void Leave() { StopAllCoroutines(); throttle.Clear(); ResetMetrics(); ResetRoom(); Recovery=false; Synced=false; Match=null; preview=null; View.CancelAnimations(); }
        // Token salvo por sala: quem fecha e reabre o jogo volta ao mesmo assento.
        string Token()
        {
            if(tokenOverride!=null) return tokenOverride;
            string key="arena-token:"+room, token=PlayerPrefs.GetString(key,"");
            if(token=="") { token=Guid.NewGuid().ToString("N"); PlayerPrefs.SetString(key,token); PlayerPrefs.Save(); }
            return token;
        }

        void LobbyStatus()
        {
            if(!Network.IsHost||!Lobby) return;
            string where=room=="LAN"?"Host LAN na porta 9080":room==null?"Sala":$"Sala {room} (código copiado)";
            Status=$"{where} · {Filled}/{Seats} jogadores · {FormatName(Format)}. Começa ao encher ou em Iniciar.";
        }
        void BroadcastLobby()
        {
            foreach(var kv in seatPeer) Send(kv.Value,new WireMessage{kind="lobby",seat=kv.Key,seats=Seats,text=$"{Filled}/{Seats} jogadores · {FormatName(Format)}"});
        }
        void Hello(int peer,string token,string[] fighters)
        {
            if(string.IsNullOrEmpty(token)||token.Length>64||peerSeat.ContainsKey(peer)) return;
            if(Lobby) tokenPick[token]=ValidTeam(fighters);
            if(!tokenSeat.TryGetValue(token,out int seat))
            {
                if(Lobby) for(int s=2;s<=Seats&&seat==0;s++) if(!tokenSeat.ContainsValue(s)) seat=s;
                // Sem assento: o cliente fecha a conexão sozinho ao receber "full".
                if(seat==0) { Send(peer,new WireMessage{kind="full",text=Lobby?"A sala está cheia.":"A partida já começou."}); return; }
                tokenSeat[token]=seat;
            }
            if(seatPeer.TryGetValue(seat,out int old)&&old!=peer) { peerSeat.Remove(old); Network.Kick(old); }
            seatPeer[seat]=peer; peerSeat[peer]=seat;
            Send(peer,new WireMessage{kind="welcome",seat=seat,seats=Lobby?Seats:State.players});
            if(Lobby) { BroadcastLobby(); LobbyStatus(); if(Filled>=Seats) StartRoom(); return; }
            Send(peer,new WireMessage{kind="snapshot",seat=seat,state=Match.ViewFor(State,seat)});
            Status=$"P{seat} voltou à partida.";
        }
        void Left(int peer)
        {
            if(!peerSeat.TryGetValue(peer,out int seat)) return;
            peerSeat.Remove(peer); if(seatPeer.TryGetValue(seat,out int p)&&p==peer) seatPeer.Remove(seat);
            if(Lobby) { foreach(var t in tokenSeat.Where(kv=>kv.Value==seat).Select(kv=>kv.Key).ToList()) tokenSeat.Remove(t); BroadcastLobby(); LobbyStatus(); return; }
            Status=$"P{seat} desconectado. A partida espera a volta dele com o mesmo código.";
        }
        // Começa com quem está na sala; os assentos ocupados ficam em sequência (P1, P2, P3…).
        public void StartRoom()
        {
            if(!Network.IsHost||!Lobby) return;
            int n=Filled;
            if(n<2) { Status="Espere pelo menos mais 1 jogador."; return; }
            if(!FormatFits(Format,n)) { Status=$"{FormatName(Format)} não fecha com {n} jogadores. Troque o formato ou espere."; return; }
            var order=tokenSeat.OrderBy(kv=>kv.Value).ToList(); var oldPeer=new Dictionary<int,int>(seatPeer);
            tokenSeat.Clear(); seatPeer.Clear(); peerSeat.Clear();
            for(int i=0;i<order.Count;i++) {
                int seat=i+2; tokenSeat[order[i].Key]=seat;
                if(oldPeer.TryGetValue(order[i].Value,out int peer)) { seatPeer[seat]=peer; peerSeat[peer]=seat; }
            }
            Lobby=false; NewMatch(n,pick:seat=>seat==1?MyTeam:tokenPick.TryGetValue(tokenSeat.FirstOrDefault(kv=>kv.Value==seat).Key??"",out var t)?t:null); throttle.Clear(); View.ResetView(State);
            foreach(var kv in seatPeer) {
                Send(kv.Value,new WireMessage{kind="welcome",seat=kv.Key,seats=n});
                Send(kv.Value,new WireMessage{kind="snapshot",seat=kv.Key,state=Match.ViewFor(State,kv.Key)});
            }
            Status=$"Partida com {n} jogadores ({FormatName(Format)}). Você é P1.";
        }

        public void Submit(string kind,Cell cell=default,string skill=null,int slot=0)
        {
            if(!CanAct) { Status=Lobby?"A partida ainda não começou.":Recovery?"Retome a partida ou reconecte.":"Aguarde seu turno e o fim da animação."; return; }
            var cmd=new Command{kind=kind,cell=cell,skill=skill,slot=slot,fighter=Active.id,revision=State.revision};
            if(!Allow("local:"+kind)) return;
            if(Client) Send(0,new WireMessage{kind="command",command=cmd}); else Execute(cmd,LocalTeam);
        }
        bool Allow(string key)
        {
            if(throttle.TryGetValue(key,out var t)&&Time.unscaledTime-t<.12f) { Filtered++; return false; }
            throttle[key]=Time.unscaledTime; return true;
        }
        void Execute(Command cmd,int seat)
        {
            if(Recovery||Match==null) return;
            if(Match.Apply(cmd,seat,out var events,out var reason)) {
                State=Match.S; Mode="";
                Status=cmd.kind=="passar"?"Turno encerrado.":cmd.kind=="comprar"?"Compra feita.":"Ação confirmada.";
                View.Play(events,State);
                if(Network.IsHost) foreach(var kv in seatPeer)
                    Send(kv.Value,new WireMessage{kind="state",state=Match.ViewFor(State,kv.Key),events=Match.EventsFor(events,State,kv.Key).ToArray()});
            } else { Status=reason; if(Network.IsHost&&seatPeer.TryGetValue(seat,out int peer)) Send(peer,new WireMessage{kind="reject",text=reason}); }
        }
        void Send(int peer,WireMessage message) { Network.Send(peer,JsonUtility.ToJson(message)); }
        public static bool ValidState(MatchState s)
        {
            const int n=MatchState.MaxPlayers+1;
            if(s==null||!Board.ValidSize(s.width)||s.height!=s.width||s.players<2||s.players>MatchState.MaxPlayers||s.tiles==null||s.tiles.Length!=s.width*s.height) return false;
            if(s.fighters==null||s.fighters.Count==0||s.fighters.Count>64||s.revision<0||s.timeline==null||s.timeline.Any(id=>s.F(id)==null)) return false;
            if(s.galeoes?.Length!=n||s.coins?.Length!=n||s.collected?.Length!=n||s.domain?.Length!=n||s.eliminated?.Length!=n||s.alliance?.Length!=n||s.shops?.Length!=n||s.shops.Any(x=>x==null)) return false;
            for(int p=1;p<=s.players;p++) if(s.alliance[p]<1||s.alliance[p]>s.players) return false;
            var ids=new HashSet<int>(); var cells=new HashSet<Cell>();
            foreach(var f in s.fighters) {
                if(f==null||!ids.Add(f.id)||f.team<1||f.team>s.players||f.def==null||!Content.Fighters.ContainsKey(f.def)) return false;
                if(f.hp<0||f.hp>Content.MaxHp(f)||f.pa<0||f.pm<0||!Board.Contains(s,f.cell)) return false;
                if(f.Alive&&!cells.Add(f.cell)) return false;
            }
            return true;
        }
        void Receive(int peer,string text)
        {
            WireMessage m;
            try { m=JsonUtility.FromJson<WireMessage>(text); } catch(Exception) { return; }
            if(m==null||m.protocol!=3) return;
            if(m.kind=="ping") { if(Allow("remote:ping:"+peer)) Send(peer,new WireMessage{kind="pong",ping=m.ping}); return; }
            if(m.kind=="pong") { if(pingWaiting&&m.ping==pingId) { Rtt=Mathf.RoundToInt((Time.unscaledTime-pingSent)*1000); RecordProbe(false); pingWaiting=false; } return; }
            if(Network.IsHost) {
                if(m.kind=="hello") { Hello(peer,m.token,m.fighters); return; }
                if(!Lobby&&peerSeat.TryGetValue(peer,out int seat)&&m.kind=="command"&&m.command!=null&&new[]{"mover","skill","comprar","passar"}.Contains(m.command.kind)&&Allow("remote:"+seat+":"+m.command.kind)) Execute(m.command,seat);
                return;
            }
            if(m.kind=="full") { Network.Close(); Recovery=true; Synced=false; Status=m.text??"A sala está cheia."; return; }
            if(m.seats>=2&&m.seats<=MatchState.MaxPlayers) Seats=m.seats;
            if(m.kind=="welcome") { if(m.seat>=2&&m.seat<=MatchState.MaxPlayers) Seat=m.seat; return; }
            if(m.kind=="lobby") { if(m.seat>=2&&m.seat<=MatchState.MaxPlayers) Seat=m.seat; Lobby=true; Status=$"Na sala como P{Seat} · {m.text}. Aguardando o Host iniciar."; return; }
            if(m.kind=="reject") { Status=m.text??"Ação recusada."; return; }
            if(m.kind!="snapshot"&&m.kind!="state") return;
            if(Seat==0||!ValidState(m.state)||Seat>m.state.players||(!Synced&&m.kind!="snapshot")||(Synced&&State!=null&&m.state.revision<=State.revision&&m.kind!="snapshot")) return;
            bool initial=m.kind=="snapshot";
            State=m.state; preview=null; Synced=true; Recovery=false; Lobby=false; Mode="";
            if(initial) View.ResetView(State); else View.Play(m.events?.ToList()??new List<GameEvent>(),State);
            Status=$"Partida sincronizada. Você é P{Seat}.";
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
            }
        }
    }
}
