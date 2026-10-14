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
        string savedTeam,savedPool;
        int[] savedTeams;
        UnityEngine.Random.State random;
        static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        object Call(string name,params object[] args) => typeof(ArenaGame).GetMethod(name,Private).Invoke(game,args);
        void Property(string name,object value) => typeof(ArenaGame).GetProperty(name).SetValue(game,value);

        [SetUp] public void Setup()
        {
            savedTeam=PlayerPrefs.GetString("arena-time",""); savedPool=PlayerPrefs.GetString("arena-pool",""); savedTeams=ArenaGame.CustomTeams.ToArray();
            random=UnityEngine.Random.state; UnityEngine.Random.InitState(19013);
            ArenaGame.MyTeam=new[]{"ciantia"}; ArenaGame.MyPool=Content.Roster.Take(5).ToArray();
            root=new GameObject("019-room-test"); game=root.AddComponent<ArenaGame>();
            if(game.Network==null) Call("Awake");
            // Os menus são construídos no Start do player.
            Property("Menu",null); Property("Hud",null);
        }
        [TearDown] public void Cleanup()
        {
            game.Network.Close(); Object.DestroyImmediate(root);
            PlayerPrefs.SetString("arena-time",savedTeam); PlayerPrefs.SetString("arena-pool",savedPool);
            for(int i=0;i<savedTeams.Length;i++) ArenaGame.CustomTeams[i]=savedTeams[i];
            UnityEngine.Random.state=random;
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
                    var packet=JsonUtility.ToJson(new WireMessage{kind="snapshot",state=Match.ViewFor(match.S,viewer)});
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
