using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace ArenaTatica.Core.Tests
{
    // Lua de Sangue (MapEvents, 21/10): 3 rodadas, cura única de 2, +3 de dano, lobos de 1 PV agressivos.
    public class BloodMoonTests
    {
        const string Id="lua-de-sangue", Mob="lobo-lua-de-sangue";
        Match m;
        MatchState S => m.S;
        Fighter A => S.Active;
        [SetUp] public void Setup() { m=Match.New(new MatchSetup{ flatMap=true, team1=new[]{"ciantia"}, team2=new[]{"talyssa"} }); }
        Fighter Foe => S.fighters.First(f=>MatchState.Player(f.team)&&f.team!=A.team);
        Command Cmd(string kind,Cell cell=default,string skill=null) => new Command{kind=kind,cell=cell,skill=skill,fighter=A.id,revision=S.revision};
        bool Do(string kind,Cell cell=default,string skill=null) => m.Apply(Cmd(kind,cell,skill),A.team,out _,out _);
        void PassRound() { int round=S.round; for(int i=0;i<20&&S.round==round&&!S.over;i++) Do("passar"); }
        System.Collections.Generic.List<Fighter> Mobs => S.fighters.Where(f=>f.Alive&&f.def==Mob).ToList();
        void ClearMobs() { foreach(var x in Mobs) x.hp=0; }

        [Test] public void LastsThreeFullRoundsAndTheWolvesLeaveWithIt()
        {
            Assert.IsTrue(Match.Cataclysms.Any(x=>x.id==Id&&x.rounds==3&&x.weight>0),"entra no sorteio do Cataclisma");
            m.Trigger(Id);
            Assert.AreEqual(Id,S.ev); Assert.AreEqual(3,S.evRounds);
            foreach(var x in S.fighters.Where(f=>MatchState.Player(f.team))) { x.cell=new Cell(x.team==1?0:S.width-1,0); x.hp=99; }
            foreach(var w in Mobs) w.cell=new Cell(S.width/2,S.height-1-Mobs.IndexOf(w));
            PassRound(); Assert.AreEqual(Id,S.ev); Assert.AreEqual(2,S.evRounds);
            PassRound(); Assert.AreEqual(Id,S.ev); Assert.AreEqual(1,S.evRounds);
            PassRound(); Assert.AreEqual("",S.ev,"acaba no fim da 3ª rodada");
            Assert.AreEqual(0,Mobs.Count,"os lobos somem com o evento");
        }

        [Test] public void HealsTwoOnceAtTheStart()
        {
            var hero=S.fighters.First(f=>MatchState.Player(f.team)); int max=Content.MaxHp(hero); hero.hp=max-5;
            var full=S.fighters.First(f=>MatchState.Player(f.team)&&f!=hero); full.hp=Content.MaxHp(full);
            var events=m.Trigger(Id);
            Assert.AreEqual(max-3,hero.hp); Assert.AreEqual(Content.MaxHp(full),full.hp,"não passa do máximo");
            Assert.IsTrue(events.Any(e=>e.kind=="cura"&&e.target==hero.id&&e.value==2&&e.info==Id));
            ClearMobs(); PassRound();
            Assert.AreEqual(max-3,hero.hp,"a cura é única, não por turno");
        }

        int StrikeDamage(bool moon)
        {
            Setup();
            var ciantia=S.fighters.First(f=>f.def=="ciantia"); for(int i=0;i<6&&A!=ciantia;i++) Do("passar");
            var target=Foe; A.cell=new Cell(5,5); target.cell=new Cell(5,7); target.hp=50;
            if(moon) { m.Trigger(Id); ClearMobs(); }
            m.Forced.Enqueue(Face.Trava); m.Forced.Enqueue(Face.Trava);
            Assert.IsTrue(Do("skill",target.cell,"dragao-dourado"));
            return 50-target.hp;
        }
        [Test] public void PlayersHitThreeHarderWhileItLasts()
        {
            int normal=StrikeDamage(false), moon=StrikeDamage(true);
            Assert.Greater(normal,0); Assert.AreEqual(normal+3,moon);
            Assert.AreEqual(3,m.EventDamage(A)); Assert.AreEqual(0,m.EventDamage(new Fighter{team=MatchState.Neutral,def=Mob}),"mobs não ganham o bônus");
            S.ev=""; Assert.AreEqual(0,m.EventDamage(A),"sem evento, sem bônus");
        }

        [Test] public void WolvesSpawnMirroredWithOneHpAwayFromFighters()
        {
            m.Trigger(Id);
            var wolves=Mobs; var def=MapEvents.Get(Id);
            Assert.AreEqual(def.MobCount(S.players),wolves.Count); Assert.AreEqual(4,wolves.Count,"2 + 1 por jogador num duelo");
            foreach(var w in wolves)
            {
                Assert.AreEqual(MatchState.Neutral,w.team); Assert.AreEqual(1,w.hp); CollectionAssert.DoesNotContain(S.timeline,w.id);
                Assert.IsTrue(S.fighters.Where(f=>MatchState.Player(f.team)).All(f=>Board.Gap(f,w.cell)>=2),"surge longe dos lutadores");
                Assert.IsTrue(wolves.Any(o=>o.cell==S.Mirror(w.cell)),"pares espelhados");
            }
        }

        [Test] public void WolvesHuntWhoIsNearAndRoamOtherwise()
        {
            m.Trigger(Id); var wolves=Mobs; ClearMobs();
            var hunter=wolves[0]; var roamer=wolves[1]; hunter.hp=roamer.hp=1;
            var prey=S.fighters.First(f=>MatchState.Player(f.team)); var other=S.fighters.First(f=>MatchState.Player(f.team)&&f!=prey);
            prey.cell=new Cell(2,2); hunter.cell=new Cell(2,5); other.cell=new Cell(S.width-1,0); roamer.cell=new Cell(S.width-2,S.height-1);
            prey.hp=20; var roamFrom=roamer.cell;
            m.Forced.Enqueue(Face.Trava); m.Forced.Enqueue(Face.Trava);
            PassRound();
            Assert.AreEqual(1,Board.Gap(hunter,prey.cell),"anda até quem está perto");
            Assert.Less(prey.hp,20,"e morde");
            Assert.AreNotEqual(roamFrom,roamer.cell,"sem ninguém perto, vaga");
            Assert.Greater(Board.Gap(roamer,prey.cell),1);
        }

        [Test] public void KillingAWolfIsOneHitAndPays()
        {
            var ciantia=S.fighters.First(f=>f.def=="ciantia"); for(int i=0;i<6&&A!=ciantia;i++) Do("passar");
            m.Trigger(Id); var wolf=Mobs[0]; foreach(var x in Mobs) if(x!=wolf) x.hp=0;
            A.cell=new Cell(5,5); wolf.cell=new Cell(5,7); Foe.cell=new Cell(0,0); int coins=S.coins[A.team];
            m.Forced.Enqueue(Face.Trava); m.Forced.Enqueue(Face.Trava);
            Assert.IsTrue(Do("skill",wolf.cell,"dragao-dourado"));
            Assert.IsFalse(wolf.Alive); Assert.Greater(S.coins[A.team],coins);
        }

        [Test] public void GenericApiFiresEventsById()
        {
            Assert.IsFalse(m.FireEvent("nao-existe"));
            Assert.IsTrue(m.FireEvent(Id)); Assert.AreEqual(Id,S.ev);
            Assert.IsTrue(m.FireEvent("nevoeiro")); Assert.AreEqual("nevoeiro",S.ev,"evento com duração novo encerra o anterior");
            Assert.AreEqual(0,Mobs.Count,"e leva os lobos junto");
            Assert.IsTrue(m.FireEvent(),"sem id: sorteia como a face Cataclisma");
            Assert.IsTrue(MapEvents.Allowed(Id,true)&&MapEvents.Allowed(Id,false),"sai no labirinto e nos mapas abertos");
        }

        [Test] public void OnlineSnapshotCarriesTheEventAndTheWolves()
        {
            m.Trigger(Id);
            var wire=JsonUtility.FromJson<ArenaTatica.WireMessage>(JsonUtility.ToJson(new ArenaTatica.WireMessage{kind="snapshot",state=Match.ViewFor(S,2)}));
            Assert.AreEqual(Id,wire.state.ev); Assert.AreEqual(3,wire.state.evRounds);
            Assert.AreEqual(4,wire.state.fighters.Count(f=>f.Alive&&f.def==Mob));
            Assert.IsTrue(ArenaTatica.ArenaGame.ValidState(wire.state,2));
            var client=Match.FromState(wire.state,true);
            Assert.AreEqual(3,client.EventDamage(wire.state.fighters.First(f=>f.team==2)),"o cliente calcula o mesmo bônus");
        }

        // Uma decisão só (sem partida entre bots): com um lobo colado, o bot decide um comando válido.
        [Test] public void BotDecidesAValidMoveNextToAWolf()
        {
            m.Trigger(Id); var wolf=Mobs[0]; foreach(var x in Mobs) if(x!=wolf) x.hp=0; A.cell=new Cell(5,5); wolf.cell=new Cell(5,6); Foe.cell=new Cell(0,0);
            var cmd=Bot.Decide(m)??new Command{kind="passar"}; cmd.fighter=A.id; cmd.revision=S.revision;
            Assert.IsTrue(m.Apply(cmd,A.team,out _,out var reason),reason);
        }
    }
}
