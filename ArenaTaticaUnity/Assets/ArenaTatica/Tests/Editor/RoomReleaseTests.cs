using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using ArenaTatica.Core;

namespace ArenaTatica.Tests
{
    // Controlador real de sala/draft; sockets e Relay são verificados no player.
    public class RoomReleaseTests
    {
        GameObject root;
        ArenaGame game;
        string savedTeam,savedPool,savedEdits,savedStats,savedPreset;
        int savedTurnTime,savedTeamLevel,savedTeamSize;
        static void TeamLimits(int level,int size) => typeof(ArenaGame).GetMethod("ApplyTeamLimits",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{level,size});
        int[] savedTeams;
        UnityEngine.Random.State random;
        static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        object Call(string name,params object[] args) => typeof(ArenaGame).GetMethod(name,Private).Invoke(game,args);
        void Property(string name,object value) => typeof(ArenaGame).GetProperty(name).SetValue(game,value);

        [SetUp] public void Setup()
        {
            savedTeam=PlayerPrefs.GetString("arena-time",""); savedPool=PlayerPrefs.GetString("arena-pool",""); savedTeams=ArenaGame.CustomTeams.ToArray();
            savedEdits=Content.EditsJson; savedStats=PlayerPrefs.GetString("arena-estatisticas",""); savedPreset=PlayerPrefs.GetString("arena-preset",""); savedTurnTime=PlayerPrefs.GetInt("arena-tempo-turno",0);
            // Limites do time: o aparelho do autor pode ter outros; os testes começam no padrão (12 / 4).
            savedTeamLevel=PlayerPrefs.GetInt("arena-time-custo",ArenaGame.DefaultTeamLevel); savedTeamSize=PlayerPrefs.GetInt("arena-time-tamanho",ArenaGame.DefaultTeamSize);
            PlayerPrefs.SetInt("arena-time-custo",ArenaGame.DefaultTeamLevel); PlayerPrefs.SetInt("arena-time-tamanho",ArenaGame.DefaultTeamSize); TeamLimits(ArenaGame.DefaultTeamLevel,ArenaGame.DefaultTeamSize);
            random=UnityEngine.Random.state; UnityEngine.Random.InitState(19013);
            ArenaGame.MyTeam=new[]{"ciantia"}; ArenaGame.MyPool=Content.Roster.Take(5).ToArray();
            root=new GameObject("019-room-test"); game=root.AddComponent<ArenaGame>();
            if(game.Network==null) Call("Awake");
            // Os menus são construídos no Start do player.
            Property("Menu",null); Property("Hud",null);
            // Testes antigos começam direto no primeiro turno; o posicionamento tem teste próprio.
            game.Placement=false;
        }
        [TearDown] public void Cleanup()
        {
            game.Network.Close(); Object.DestroyImmediate(root);
            if(clientRoot) { client.Network.Close(); Object.DestroyImmediate(clientRoot); }
            PlayerPrefs.SetString("arena-time",savedTeam); PlayerPrefs.SetString("arena-pool",savedPool);
            PlayerPrefs.SetString("arena-preset",savedPreset); PlayerPrefs.SetInt("arena-tempo-turno",savedTurnTime);
            PlayerPrefs.SetInt("arena-time-custo",savedTeamLevel); PlayerPrefs.SetInt("arena-time-tamanho",savedTeamSize); TeamLimits(ArenaGame.DefaultTeamLevel,ArenaGame.DefaultTeamSize);
            // Execute grava estatísticas: devolve as do aparelho e solta o cache.
            PlayerPrefs.SetString("arena-estatisticas",savedStats); typeof(ArenaStats).GetField("data",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,null);
            ArenaEdits.Folder=null; Content.ApplyEdits(savedEdits);
            for(int i=0;i<savedTeams.Length;i++) ArenaGame.CustomTeams[i]=savedTeams[i];
            UnityEngine.Random.state=random;
        }
        // ── Sprint 1: 1.1 desync de turno e 1.2 presets ──
        GameObject clientRoot;
        ArenaGame client;
        bool drop;
        readonly List<WireMessage> toClient=new List<WireMessage>();
        // Fichas do Host: vão em partes, numa mensagem própria (protocolo 7).
        readonly List<WireMessage> editsToClient=new List<WireMessage>();
        string LastEdits() { int i=editsToClient.FindLastIndex(m=>m.text.StartsWith("0/")); return i<0?null:string.Concat(editsToClient.Skip(i).Select(m=>m.edits)); }
        // Cliente real no mesmo processo: o que o Host manda ao peer 102 chega ao cliente e vice-versa (drop = mensagem perdida).
        void Wire()
        {
            toClient.Clear(); drop=false;
            clientRoot=new GameObject("019-client-test"); client=clientRoot.AddComponent<ArenaGame>();
            if(client.Network==null) typeof(ArenaGame).GetMethod("Awake",Private).Invoke(client,null);
            typeof(ArenaGame).GetProperty("Menu").SetValue(client,null); typeof(ArenaGame).GetProperty("Hud").SetValue(client,null);
            typeof(ArenaTransport).GetProperty("Role").SetValue(client.Network,"Client");
            var receive=typeof(ArenaGame).GetMethod("Receive",Private);
            game.Sent+=(peer,m)=>{ if(peer!=102) return; if(m.kind!="edits") toClient.Add(m); else editsToClient.Add(m); if(!drop) receive.Invoke(client,new object[]{0,JsonUtility.ToJson(m)}); };
            client.Sent+=(peer,m)=>receive.Invoke(game,new object[]{102,JsonUtility.ToJson(m)});
        }
        void Pass() { var f=game.Active; Call("Execute",new Command{kind="passar",fighter=f.id,revision=game.State.revision},f.team,-1); }
        // O tempo não anda dentro do teste: zera as travas de repetição do cliente e do Host.
        void ResetResyncThrottle() { typeof(ArenaGame).GetField("resyncAt",Private).SetValue(client,-9f); ((Dictionary<string,float>)typeof(ArenaGame).GetField("throttle",Private).GetValue(game)).Clear(); }
        [Test] public void LostEndTurnIsRecoveredBySnapshot()
        {
            Wire(); Host(2); game.StartRoom();
            Assert.AreEqual(2,client.Seat); Assert.IsTrue(client.Synced); Assert.AreEqual(game.State.revision,client.State.revision);
            // 1) Salto: o EndTurn se perde e o estado seguinte chega com revisão +2 → cliente pede snapshot e o Host reenvia.
            drop=true; Pass(); drop=false; Pass();
            Assert.AreEqual(game.State.revision,client.State.revision,"salto de revisão volta pelo snapshot");
            Assert.IsTrue(client.Synced); Assert.AreEqual(game.State.active,client.State.active);
            Assert.IsTrue(toClient.Any(m=>m.kind=="snapshot"&&m.text=="resync"));
            // 2) Atraso: o último EndTurn se perde e nada mais chega; o pong mostra o Host à frente → snapshot.
            ResetResyncThrottle(); drop=true; Pass(); drop=false;
            Assert.Less(client.State.revision,game.State.revision);
            Call("Receive",102,JsonUtility.ToJson(new WireMessage{kind="ping",ping=7}));
            Assert.AreEqual(game.State.revision,client.State.revision,"pong com revisão maior faz o cliente pedir o estado");
            // 3) Comando com revisão velha: o Host recusa e reenvia o estado sem o cliente pedir.
            ResetResyncThrottle(); drop=true; Pass(); drop=false; int stale=client.State.revision; toClient.Clear();
            var mine=game.State.fighters.First(f=>f.team==2);
            Call("Receive",102,JsonUtility.ToJson(new WireMessage{kind="command",command=new Command{kind="passar",fighter=mine.id,revision=stale}}));
            Assert.AreEqual("reject",toClient[0].kind); Assert.AreEqual(game.State.revision,client.State.revision);
        }
        [Test] public void FullOutboxIsReplacedByOneSnapshot()
        {
            Wire(); Host(2); toClient.Clear(); Call("Resend",102);
            Assert.AreEqual("lobby",toClient.Single().kind,"na sala reenvia o resumo");
            ((Dictionary<int,float>)typeof(ArenaGame).GetField("resent",Private).GetValue(game)).Clear();
            game.StartRoom(); drop=true; Pass(); Pass(); drop=false; toClient.Clear();
            Assert.Less(client.State.revision,game.State.revision);
            Call("Resend",102); Call("Resend",102);
            Assert.AreEqual(1,toClient.Count,"no máximo 1 snapshot por segundo"); Assert.AreEqual("resync",toClient[0].text);
            Assert.AreEqual(game.State.revision,client.State.revision); Assert.IsTrue(client.Synced);
            Assert.AreEqual(32,ArenaTransport.OutboxCapacity/2,"limpa a 50% da fila");
        }
        [Test] public void TurnTimeoutPassesStalledTurnOnHost()
        {
            Host(2); game.StartRoom(); int rev=game.State.revision, first=game.Active.id;
            Call("TurnClock",0f); Call("TurnClock",500f); Assert.AreEqual(rev,game.State.revision,"sem limite não passa");
            game.SetTurnTime(30);
            Call("TurnClock",1000f); Call("TurnClock",1029f); Assert.AreEqual(rev,game.State.revision,"dentro do tempo");
            Call("TurnClock",1031f); Assert.AreEqual(rev+1,game.State.revision,"estourou: Host passa sozinho"); Assert.AreNotEqual(first,game.Active.id);
            Call("TurnClock",1032f); Call("TurnClock",1060f); Assert.AreEqual(rev+1,game.State.revision,"o próximo lutador ganha tempo cheio");
            Call("TurnClock",1063f); Assert.AreEqual(rev+2,game.State.revision);
            StringAssert.Contains("Tempo de turno: <b>30 s</b>",game.LobbySummary());
        }
        [Test] public void PlacementPhaseAcceptsRemoteCommandOnHostAndTimesOut()
        {
            game.Placement=true; Host(2); game.StartRoom();
            Assert.IsTrue(game.Placing); Assert.IsNull(game.Active);
            var match=game.Match; var mine=game.State.fighters.First(f=>f.team==2);
            if(game.State.placing==1) Execute1("passar");
            Assert.AreEqual(2,game.State.placing);
            var free=match.PlaceCells(2,mine).First(c=>Board.At(game.State,c)==null);
            Call("Receive",102,JsonUtility.ToJson(new WireMessage{kind="command",command=new Command{kind="posicionar",fighter=mine.id,cell=free,revision=game.State.revision}}));
            Assert.AreEqual(free,mine.cell,"Host aceita posicionar do cliente");
            // O relógio da sala também vale no posicionamento.
            game.SetTurnTime(30); Call("TurnClock",0f); Call("TurnClock",31f);
            if(game.Placing) { Assert.AreEqual(1,game.State.placing); Execute1("passar"); }
            Assert.IsFalse(game.Placing); Assert.IsNotNull(game.Active,"depois de todos, começa o primeiro turno");
        }
        // Sprint 3.6: custo e tamanho do time são da sala; o cliente valida com os do Host e não muda os da sala.
        [Test] public void TeamLimitsAreRoomSettingsSentToClients()
        {
            Wire(); Host(2);
            game.SetTeamLimits(9,6);
            Assert.AreEqual(9,ArenaGame.TeamLevel); Assert.AreEqual(6,ArenaGame.TeamSize);
            Assert.AreEqual(9,PlayerPrefs.GetInt("arena-time-custo"),"fica salvo no aparelho do Host");
            var lobby=toClient.Last(m=>m.kind=="lobby"); Assert.AreEqual(9,lobby.teamLevel); Assert.AreEqual(6,lobby.teamSize);
            StringAssert.Contains("Times: <b>custo 9</b> · <b>até 6 heróis</b>",game.LobbySummary());
            // Host e cliente dividem os estáticos no mesmo processo: zera para o padrão e entrega o "lobby" ao cliente.
            TeamLimits(ArenaGame.DefaultTeamLevel,ArenaGame.DefaultTeamSize);
            typeof(ArenaGame).GetMethod("Receive",Private).Invoke(client,new object[]{0,JsonUtility.ToJson(lobby)});
            Assert.AreEqual(9,ArenaGame.TeamLevel,"cliente recebe o custo da sala"); Assert.AreEqual(6,ArenaGame.TeamSize);
            client.SetTeamLimits(24,1); Assert.AreEqual(9,ArenaGame.TeamLevel,"cliente não muda os limites"); Assert.AreEqual(6,ArenaGame.TeamSize);
            Assert.LessOrEqual(ArenaGame.ValidTeam(Content.Roster).Sum(id=>Content.Fighters[id].level),9);
            // Time grande: até 6 heróis na partida da sala.
            game.SetTeamLimits(24,6);
            ArenaGame.MyTeam=Content.Roster.OrderBy(id=>Content.Fighters[id].level).ToArray(); int size=ArenaGame.MyTeam.Length;
            Assert.Greater(size,ArenaGame.DefaultTeamSize); Assert.LessOrEqual(size,6);
            game.StartRoom();
            var mine=game.State.fighters.Where(f=>f.team==1&&f.summoner==0).ToList();
            Assert.AreEqual(size,mine.Count); Assert.AreEqual(mine.Count,mine.Select(f=>f.cell).Distinct().Count());
        }
        // Bugs só do cliente remoto: eventos pelo JSON, tempo de turno da sala e 1ª casa velha de skill de 2 casas.
        [Test] public void ClientGetsCleanEventsTurnTimeAndDropsStaleFirstCell()
        {
            var sent=new WireMessage{kind="state",events=new[]{new GameEvent{kind="ataque",actor=1,target=2,value=1},new GameEvent{kind="trava",dice=new[]{Face.Trava},info="x"}}};
            var got=JsonUtility.FromJson<WireMessage>(JsonUtility.ToJson(sent)).events;
            Assert.IsNotNull(got[0].dice,"causa: o JSON devolve [] no lugar de null (alvo extra da área virava golpe principal)");
            var list=ArenaGame.FromWire(got);
            Assert.IsNull(list[0].dice); Assert.IsNull(list[0].path); Assert.IsNull(list[0].info);
            Assert.AreEqual(1,list[1].dice.Length); Assert.AreEqual("x",list[1].info);
            CollectionAssert.IsEmpty(ArenaGame.FromWire(null));

            game.SetTurnTime(30); Wire(); Host(2); game.StartRoom();
            Assert.IsTrue(client.Synced); Assert.AreEqual(30,toClient.Last(m=>m.kind=="lobby").turnTime);
            var clock=typeof(ArenaGame).GetMethod("TurnClock",Private); float now=Time.unscaledTime;
            clock.Invoke(client,new object[]{now});
            Assert.Greater(client.TurnLeft,29f,"cliente conta o tempo da sala"); Assert.LessOrEqual(client.TurnLeft,30f);
            int rev=client.State.revision; clock.Invoke(client,new object[]{now+100});
            Assert.AreEqual(rev,client.State.revision,"quem passa a vez é o Host, não o cliente");

            client.FirstFor="portal"; client.FirstCell=new Cell(1,1); Pass();
            Assert.AreEqual(game.State.revision,client.State.revision); Assert.AreEqual("",client.FirstFor,"estado novo descarta a 1ª casa guardada");
        }
        void Execute1(string kind) => Call("Execute",new Command{kind=kind,revision=game.State.revision},1,-1);
        [Test] public void BalancePresetIsSavedChosenBeforeDraftAndValidatesTeams()
        {
            ArenaEdits.Folder=System.IO.Path.Combine(Application.temporaryCachePath,"019-presets-"+System.Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(ArenaEdits.Folder);
            try {
                game.SetPreset(ArenaEdits.Default);
                string other=Content.Roster.First(id=>id!="ciantia"&&Content.Fighters[id].level>=1&&!Content.Fighters[id].legendary&&Content.Fighters[id].level+Content.Fighters["ciantia"].level<=ArenaGame.TeamLevel);
                ArenaGame.MyTeam=new[]{"ciantia",other}; Assert.AreEqual(2,ArenaGame.MyTeam.Length);
                var caro=Content.Copy(Content.Default("ciantia")); caro.level=ArenaGame.TeamLevel; ArenaEdits.Put(caro);
                Assert.IsTrue(ArenaEdits.SavePreset(" caro ")); Assert.IsFalse(ArenaEdits.SavePreset(ArenaEdits.Default)); ArenaEdits.ResetAll();
                CollectionAssert.AreEqual(new[]{"caro"},ArenaEdits.PresetNames); Assert.IsNull(ArenaEdits.Get("ciantia"));
                Wire(); Host(2);
                game.SetPreset("caro"); Assert.AreEqual(ArenaGame.TeamLevel,Content.Fighters["ciantia"].level);
                CollectionAssert.AreEqual(new[]{"ciantia"},ArenaGame.ValidTeam(new[]{"ciantia",other}),"custo do preset vale na sala");
                var lobby=toClient.Last(m=>m.kind=="lobby"); Assert.IsTrue(string.IsNullOrEmpty(lobby.edits),"fichas fora do resumo da sala");
                Assert.AreEqual(ArenaEdits.PresetJson("caro"),LastEdits(),"cliente valida com o preset do Host");
                StringAssert.Contains("Balanceamento: <b>caro</b>",game.LobbySummary());
                Content.ApplyEdits(""); game.StartRoom();
                Assert.AreEqual(ArenaEdits.PresetJson("caro"),game.State.edits);
                CollectionAssert.AreEqual(new[]{"ciantia"},game.State.fighters.Where(f=>f.team==1&&f.summoner==0).Select(f=>f.def).ToArray(),"time do P1 cortado pelo Nível do preset");
                game.SetPreset(ArenaEdits.Default); Assert.AreEqual("caro",game.Preset,"não muda com a partida em andamento");
                game.BackToLobby(); game.SetPreset(ArenaEdits.Default); game.StartRoom();
                Assert.AreEqual("",game.State.edits); Assert.AreEqual(2,game.State.fighters.Count(f=>f.team==1&&f.summoner==0));
                Assert.IsTrue(ArenaEdits.LoadPreset("caro")); Assert.AreEqual(ArenaGame.TeamLevel,ArenaEdits.Get("ciantia").level,"carregar põe o preset no editor");
                ArenaEdits.ResetAll(); ArenaEdits.DeletePreset("caro"); Assert.IsEmpty(ArenaEdits.PresetNames); Assert.AreEqual(ArenaEdits.Saved,ArenaEdits.PresetJson("caro"));
            } finally { try { System.IO.Directory.Delete(ArenaEdits.Folder,true); } catch(System.Exception) { } }
        }
        [Test] public void AutomaticPoolStaysTheSameAcrossDraftReads()
        {
            PlayerPrefs.SetString("arena-pool","");
            var first=ArenaGame.MyPool;
            Assert.GreaterOrEqual(first.Length,ArenaGame.MaxBans+3);
            for(int i=0;i<10;i++) CollectionAssert.AreEqual(first,ArenaGame.MyPool);
            Assert.AreEqual(string.Join(",",first),PlayerPrefs.GetString("arena-pool"));
        }
        [TestCase(2)] [TestCase(3)] [TestCase(6)]
        public void FullTeamsFitRelayPacket(int players)
        {
            var team=ArenaGame.ValidTeam(Content.Roster.OrderBy(id=>Content.Fighters[id].level));
            int budget=(int)typeof(ArenaTransport).GetField("RelayPayload",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);
            int largest=0;
            for(uint seed=1;seed<=5;seed++) {
                var match=Match.New(new MatchSetup{seed=seed,map=ArenaMaps.Pick(Board.SizeFor(players),seed),players=Enumerable.Range(0,players).Select(_=>new PlayerSetup{fighters=team}).ToArray()});
                for(int viewer=1;viewer<=players;viewer++) {
                    // O que vai pela rede (protocolo 11): casas compactadas e o JSON comprimido.
                    var packet=ArenaGame.Zip(ArenaGame.Wire(new WireMessage{kind="snapshot",state=Match.ViewFor(match.S,viewer)}));
                    largest=System.Math.Max(largest,System.Text.Encoding.UTF8.GetByteCount(packet));
                }
            }
            TestContext.WriteLine($"{players} participantes, {team.Length} lutadores/time: maior snapshot {largest} bytes, limite Relay {budget}.");
            Assert.LessOrEqual(largest,budget,"Uma sala válida precisa caber no transporte Relay.");
        }
        void Host(int players,bool tournament=false)
        {
            game.SetSeats(players); game.SetTournament(tournament); game.OpenLobby();
            Assert.IsTrue(game.Setup); Assert.IsFalse(game.Lobby); Assert.AreEqual("Local",game.Network.Role);
            var old=game.State; game.StartRoom(); Assert.AreSame(old,game.State,"não inicia antes de liberar");
            // Somente simula a escuta liberada: não toma a porta de ferramentas de rede concorrentes.
            typeof(ArenaTransport).GetProperty("IsHost").SetValue(game.Network,true);
            typeof(ArenaTransport).GetProperty("Role").SetValue(game.Network,"Host");
            Property("Lobby",true); Property("Setup",false); Property("Seat",1);
            game.StartRoom(); Assert.IsTrue(game.Lobby,"sozinho espera outro jogador");
            for(int seat=2;seat<=players;seat++) Call("Hello",100+seat,"019-P"+seat,new[]{"ciantia"},ArenaGame.MyPool);
            Assert.AreEqual(players,game.Filled);
        }
        [TestCase(2)] [TestCase(3)] [TestCase(6)]
        public void RoomRestartsAndReturnsToLobbyWithoutLosingParticipants(int players)
        {
            Host(players); game.StartRoom(); Assert.IsFalse(game.Lobby); Assert.AreEqual(players,game.State.players);
            var before=game.State; game.RestartRoom(); Assert.AreNotSame(before,game.State); Assert.AreEqual(players,game.Filled);
            Call("Left",102); Assert.AreEqual(players,game.Filled,"assento reservado durante partida");
            Call("Hello",202,"019-P2",new[]{"ciantia"},ArenaGame.MyPool);
            game.BackToLobby(); Assert.IsTrue(game.Lobby); Assert.AreEqual(players,game.Filled);
            game.StartRoom(); Assert.IsFalse(game.Lobby); Assert.AreEqual(players,game.State.players);
        }
        [Test] public void RoomFormatAndShopSettingsReachTheAuthoritativeMatch()
        {
            Host(3); Property("Seats",4); game.SetFormat("2v2"); game.StartRoom(); Assert.IsTrue(game.Lobby,"formato incompleto não inicia");
            game.SetFormat("equipes"); ArenaGame.CustomTeams[0]=1; ArenaGame.CustomTeams[1]=2; ArenaGame.CustomTeams[2]=1;
            var saved=ArenaGame.ShopItems.ToArray();
            try {
                ArenaGame.ShopItems.Clear(); ArenaGame.ShopItems.Add(Shop.Catalog.First().id); game.Turns=TurnMode.Alternada;
                game.StartRoom(); Assert.IsFalse(game.Lobby); Assert.IsTrue(game.State.Ally(1,3)); Assert.IsFalse(game.State.Ally(1,2));
                Assert.AreEqual(TurnMode.Alternada,game.State.turnMode); CollectionAssert.AreEquivalent(ArenaGame.ShopItems,game.State.shopPool);
                var p3=game.State.fighters.First(f=>f.team==3); game.State.active=game.State.timeline.IndexOf(p3.id);
                Assert.IsTrue(game.Match.Apply(new Command{kind="passar",fighter=p3.id,revision=game.State.revision},game.MatchTeam(3),out _,out _),"P3 controla seus lutadores no time aliado de P1");
            } finally { ArenaGame.ShopItems.Clear(); foreach(var id in saved) ArenaGame.ShopItems.Add(id); }
        }
        [Test] public void CloseLocalMatchBlocksFurtherActionsAndNewMatchCanStart()
        {
            game.NewLocal(); var old=game.State; game.CloseMatch();
            Assert.IsTrue(game.InMenu); Assert.IsFalse(game.CanAct); Assert.AreEqual("Local",game.Network.Role);
            Assert.AreSame(old,game.State,"preserva só o fundo visual sem inventar vencedor");
            game.NewLocal(); Assert.IsFalse(game.InMenu); Assert.AreNotSame(old,game.State);
        }
        [Test] public void HostCloseMatchReturnsEveryoneToExistingLobby()
        {
            Host(3); game.StartRoom(); Assert.IsFalse(game.Lobby); int filled=game.Filled;
            game.CloseMatch(); Assert.IsTrue(game.Lobby); Assert.AreEqual(filled,game.Filled); Assert.IsTrue(game.Network.IsHost);
            game.StartRoom(); Assert.IsFalse(game.Lobby); Assert.IsFalse(game.InMenu);
        }
        [Test] public void HumanScopeRandomizerReachesNetworkMatch()
        {
            Host(3); game.SetRandomizer(true,true,false,true); game.StartRoom();
            Assert.IsTrue(game.State.fighters.Where(f=>f.summoner==0).All(f=>f.roll!=null));
            Assert.IsTrue(ArenaGame.ValidState(Match.ViewFor(game.State,2),2));
        }
        [TestCase(2)] [TestCase(3)] [TestCase(6)]
        public void CompleteTournamentRespectsBansByeMappingAndChampion(int players)
        {
            Host(players,true); game.StartRoom(); var t=game.Torneio; Assert.IsNotNull(t); int games=0;
            while(t.champion==0&&games<players) {
                Assert.AreEqual("ban",t.phase); int number=t.number;
                game.NextTournamentMatch(); Assert.AreEqual(number,t.number,"não pula draft");
                game.RestartRoom(); Assert.AreEqual("ban",t.phase,"reinício durante draft não lança exceção");
                int a=t.a,b=t.b;
                Call("DraftChoice",a,"ban",game.DraftOptions(a).Take(2).ToArray()); Call("DraftChoice",b,"ban",game.DraftOptions(b).Take(2).ToArray());
                Assert.AreEqual("pick",t.phase);
                var choicesA=game.DraftOptions(a); var choicesB=game.DraftOptions(b);
                Assert.AreEqual(3,choicesA.Length,"5 na pool menos 2 bans permite time de até 3"); Assert.IsFalse(choicesA.Intersect(t.bans[b]).Any());
                Call("DraftChoice",a,"pick",ArenaGame.ValidTeam(choicesA)); Call("DraftChoice",b,"pick",ArenaGame.ValidTeam(choicesB));
                Assert.AreEqual("",t.phase); Assert.IsFalse(game.Lobby); Assert.AreEqual(2,game.State.players);
                Assert.AreEqual(1,game.MatchTeam(a)); Assert.AreEqual(2,game.MatchTeam(b));
                CollectionAssert.AreEquivalent(t.picks[a],game.State.fighters.Where(f=>f.team==1&&f.summoner==0).Select(f=>f.def));
                game.RestartRoom(); Assert.IsFalse(game.State.over); Assert.AreEqual(number,t.number);
                game.State.over=true; game.State.winner=1; Call("RecordTournament"); Call("RecordTournament");
                Assert.AreEqual(1,t.winners.Count(x=>x==a),"não duplica vencedor"); games++; game.NextTournamentMatch();
            }
            Assert.AreEqual(players-1,games); Assert.That(t.champion,Is.InRange(1,players)); Assert.IsTrue(game.Lobby);
            if(players==3) Assert.IsTrue(t.log.Any(x=>x.Contains("passa direto")));
        }
        [Test] public void ReconnectAndChoiceCannotChangePoolDuringDraft()
        {
            Host(2,true); game.StartRoom(); var options=game.DraftOptions(1);
            Call("Left",102); Call("Hello",202,"019-P2",new[]{"perry"},Content.Roster.Skip(8).Take(8).ToArray());
            CollectionAssert.AreEqual(options,game.DraftOptions(1));
            Call("Choice",202,new[]{"perry"},Content.Roster.Skip(8).Take(8).ToArray()); CollectionAssert.AreEqual(options,game.DraftOptions(1));
        }
        [Test] public void RestartOfRecordedTournamentMatchRollsBackItsWinner()
        {
            Host(2,true); game.StartRoom(); var t=game.Torneio;
            Call("DraftChoice",t.a,"ban",new string[0]); Call("DraftChoice",t.b,"ban",new string[0]);
            Call("DraftChoice",t.a,"pick",ArenaGame.ValidTeam(game.DraftOptions(t.a)));
            Call("DraftChoice",t.b,"pick",ArenaGame.ValidTeam(game.DraftOptions(t.b)));
            game.State.over=true; game.State.winner=1; Call("RecordTournament"); Assert.AreEqual(1,t.winners.Count);
            game.RestartRoom(); Assert.IsEmpty(t.winners); Assert.IsFalse(t.recorded);
            game.State.over=true; game.State.winner=2; Call("RecordTournament");
            int winner=t.b; CollectionAssert.AreEqual(new[]{winner},t.winners); game.NextTournamentMatch(); Assert.AreEqual(winner,t.champion);
        }
    }
}
