using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace ArenaTatica.Core.Tests
{
    public class CoreMatchTests
    {
        Match m;
        MatchState S => m.S;
        [SetUp] public void Setup() { m=Match.New(new MatchSetup{ flatMap=true }); }
        Fighter A => S.Active;
        Fighter Enemy => S.fighters.First(f=>f.team!=A.team);
        Command Cmd(string kind,Cell cell=default,string skill=null) => new Command{kind=kind,cell=cell,skill=skill,fighter=A.id,revision=S.revision};
        bool Do(string kind,Cell cell=default,string skill=null) => m.Apply(Cmd(kind,cell,skill),A.team,out _,out _);
        void Put(Fighter f,int x,int z) => f.cell=new Cell(x,z);

        [Test] public void GeneratedMapIsMirroredAndConnected()
        {
            for(uint seed=1;seed<30;seed++)
            {
                var s=Match.New(new MatchSetup{ seed=seed }).S;
                for(int i=0;i<s.tiles.Length;i++) { var c=new Cell(i%s.width,i/s.width); var a=s.At(c); var b=s.At(s.Mirror(c)); Assert.AreEqual(a.height,b.height); Assert.AreEqual(a.terrain,b.terrain); }
                Assert.IsTrue(Board.Connected(s),"seed "+seed);
            }
        }
        [Test] public void TimelineAlternatesTeamsAndStartsWithFullStats()
        {
            m=Match.New(new MatchSetup{ flatMap=true, team1=new[]{"ciantia","ciantia"}, team2=new[]{"ciantia","ciantia"} });
            var teams=S.timeline.Select(id=>S.F(id).team).ToArray();
            CollectionAssert.AreEqual(new[]{1,2,1,2},teams);
            Assert.AreEqual(6,A.pa); Assert.AreEqual(3,A.pm);
        }
        [Test] public void ClimbingCostsAndCoinPickup()
        {
            Put(A,5,5); S.At(new Cell(6,5)).height=2; S.At(new Cell(5,7)).height=3; S.At(new Cell(5,6)).coins=1;
            Assert.IsNull(Board.Path(S,A,new Cell(5,7),3),"3 níveis bloqueia");
            Assert.IsTrue(Do("mover",new Cell(5,6)));
            Assert.AreEqual(1,S.coins[A.team]); Assert.AreEqual(0,S.At(new Cell(5,6)).coins); Assert.AreEqual(2,A.pm);
            Assert.IsTrue(Do("mover",new Cell(6,6)));
            Assert.IsFalse(Do("mover",new Cell(6,5)),"subir 2 custa 2 PM e só sobra 1");
        }
        [Test] public void GoldenDragonNeedsLineSightAndRange()
        {
            Put(A,5,5); Put(Enemy,5,9);
            Assert.IsFalse(Do("skill",new Cell(5,10),"dragao-dourado"),"casa vazia");
            Put(Enemy,6,8); Assert.IsFalse(Do("skill",new Cell(6,8),"dragao-dourado"),"fora da linha reta");
            Put(Enemy,5,10); Assert.IsFalse(Do("skill",new Cell(5,10),"dragao-dourado"),"alcance 4");
            S.At(new Cell(5,5)).height=1; m.Forced.Enqueue(Face.Escudo); m.Forced.Enqueue(Face.Trava);
            Assert.IsTrue(Do("skill",new Cell(5,10),"dragao-dourado"),"+1 de alcance no alto");
            S.At(new Cell(5,5)).height=0; Put(Enemy,5,8); S.At(new Cell(5,7)).terrain=Terrain.Arvore;
            Assert.IsFalse(Do("skill",new Cell(5,8),"dragao-dourado"),"árvore bloqueia visão");
            S.At(new Cell(5,7)).terrain=Terrain.Arbusto; m.Forced.Enqueue(Face.Escudo); m.Forced.Enqueue(Face.Escudo);
            Assert.IsTrue(Do("skill",new Cell(5,8),"dragao-dourado"),"arbusto não bloqueia visão");
            Assert.AreEqual(0,A.pa); Assert.IsFalse(Do("skill",new Cell(5,8),"dragao-dourado"),"sem PA");
        }
        [Test] public void ArmorReducesDamageAndCataclysmOnlyFromAttackDie()
        {
            Put(A,5,5); Put(Enemy,5,7); int hp=Enemy.hp;
            m.Forced.Enqueue(Face.Cataclisma); m.Forced.Enqueue(Face.Escudo);
            Assert.IsTrue(m.Apply(Cmd("skill",new Cell(5,7),"dragao-dourado"),A.team,out var ev,out _));
            Assert.AreEqual(hp-1,Enemy.hp);
            Assert.IsTrue(ev.Any(e=>e.kind=="cataclisma"));
            Put(Enemy,5,6); m.Forced.Enqueue(Face.Escudo); m.Forced.Enqueue(Face.Cataclisma);
            Assert.IsTrue(m.Apply(Cmd("skill",new Cell(5,6),"dragao-dourado"),A.team,out ev,out _));
            Assert.IsFalse(ev.Any(e=>e.kind=="cataclisma"),"Cataclisma do defensor não conta");
        }
        [Test] public void FailedTackleRemovesActionPoints()
        {
            Put(A,5,5); Put(Enemy,5,6);
            m.Forced.Enqueue(Face.Trava); m.Forced.Enqueue(Face.Escudo);
            Assert.IsTrue(Do("mover",new Cell(3,5)));
            Assert.AreEqual(new Cell(5,5),A.cell); Assert.AreEqual(0,A.pa); Assert.AreEqual(0,A.pm);
        }
        [Test] public void DodgedTackleLetsMove()
        {
            Put(A,5,5); Put(Enemy,5,6);
            m.Forced.Enqueue(Face.Trava); m.Forced.Enqueue(Face.Esquiva);
            Assert.IsTrue(Do("mover",new Cell(3,5)));
            Assert.AreEqual(new Cell(3,5),A.cell);
        }
        [Test] public void OnlyOneSummonAndItActsNext()
        {
            Put(A,5,5); int owner=A.id;
            Assert.IsTrue(Do("skill",new Cell(5,6),"invocacao-dragao"));
            A.pa=6; A.used.Clear();
            Assert.IsFalse(Do("skill",new Cell(6,5),"invocacao-dragao"),"máximo 1 ativa");
            Assert.IsTrue(Do("passar"));
            Assert.AreEqual(owner,A.summoner);
        }
        [Test] public void KnockoutCostsGaleoesAndEndsMatch()
        {
            m=Match.New(new MatchSetup{ flatMap=true, galeoes=4 });
            var e=Enemy; Put(A,5,5); Put(e,5,6); e.hp=1;
            m.Forced.Enqueue(Face.Escudo); m.Forced.Enqueue(Face.Trava);
            Assert.IsTrue(Do("skill",new Cell(5,6),"dragao-dourado"));
            Assert.AreEqual(0,S.galeoes[e.team]); Assert.IsTrue(S.over); Assert.AreEqual(3-e.team,S.winner);
        }
        [Test] public void RejectsWrongTeamAndStaleRevision()
        {
            Assert.IsFalse(m.Apply(Cmd("passar"),3-A.team,out _,out _));
            var c=Cmd("passar"); c.revision--; Assert.IsFalse(m.Apply(c,A.team,out _,out _));
        }
        [Test] public void SameSeedSameCommandsSameState()
        {
            string Run() { var x=Match.New(new MatchSetup{ seed=7 }); for(int i=0;i<6;i++) x.Apply(new Command{kind="passar",fighter=x.S.Active.id,revision=x.S.revision},x.S.Active.team,out _,out _); return JsonUtility.ToJson(x.S); }
            Assert.AreEqual(Run(),Run());
        }
        void Stock(string item,int coins=10) { S.shops[A.team].slots[0]=item; S.coins[A.team]=coins; }
        bool Buy(int slot=0) { var c=Cmd("comprar"); c.slot=slot; return m.Apply(c,A.team,out _,out _); }

        [Test] public void ShopStartsWithThreeDistinctSlotsPerTeam()
        {
            foreach(int t in new[]{1,2}) { var slots=S.shops[t].slots; Assert.AreEqual(Shop.Slots,slots.Length); Assert.IsTrue(slots.All(x=>Shop.Def(x)!=null)); Assert.AreEqual(slots.Length,slots.Distinct().Count()); }
        }
        [Test] public void BuyingCostsCoinsOncePerTurnAndRefillsAtTurnEnd()
        {
            Stock("botas-aladas",3); Assert.IsFalse(Buy(),"moedas insuficientes");
            S.coins[A.team]=10; var me=A;
            Assert.IsTrue(Buy()); Assert.AreEqual(6,S.coins[me.team]); Assert.AreEqual(4,me.pm,"+1 PM na hora");
            Assert.IsNull(S.shops[me.team].slots[0]);
            Assert.IsFalse(Buy(1),"1 compra por turno");
            Assert.IsTrue(Do("passar")); Assert.IsNotNull(S.shops[me.team].slots[0],"renova no fim do turno");
            Assert.IsTrue(Do("passar")); Assert.AreSame(me,A); Assert.AreEqual(4,me.pm,"item é permanente");
        }
        [Test] public void SpellIsSingleUse()
        {
            Put(A,5,5); Put(Enemy,5,8); Stock("raio-arcano");
            Assert.IsFalse(Do("skill",new Cell(5,8),"raio-arcano"),"sem comprar");
            Assert.IsTrue(Buy()); m.Forced.Enqueue(Face.Escudo); m.Forced.Enqueue(Face.Trava);
            int hp=Enemy.hp; Assert.IsTrue(Do("skill",new Cell(5,8),"raio-arcano")); Assert.AreEqual(hp-3,Enemy.hp);
            Assert.IsFalse(Do("skill",new Cell(5,8),"raio-arcano"),"carga gasta");
        }
        [Test] public void ArmorItemReducesDamageAndHealCapsAtMax()
        {
            var e=Enemy; e.armor=1; Put(A,5,5); Put(e,5,7); int hp=e.hp;
            m.Forced.Enqueue(Face.Escudo); m.Forced.Enqueue(Face.Trava);
            Assert.IsTrue(Do("skill",new Cell(5,7),"dragao-dourado")); Assert.AreEqual(hp-1,e.hp);
            A.hp=8; Stock("pocao-de-cura"); Assert.IsTrue(Buy());
            Assert.IsTrue(Do("skill",A.cell,"pocao-de-cura")); Assert.AreEqual(11,A.hp,"cura até o máximo");
        }
        [Test] public void TrapHurtsEnemyStopsMoveAndIgnoresOwner()
        {
            Put(A,5,5); var e=Enemy; Put(e,5,11); Stock("armadilha-de-espinhos");
            Assert.IsTrue(Buy()); Assert.IsTrue(Do("skill",new Cell(5,8),"armadilha-de-espinhos"));
            Assert.AreEqual(A.team,S.At(new Cell(5,8)).trap);
            Assert.IsTrue(Do("mover",new Cell(5,8)),"dono passa"); Assert.AreEqual(A.team,S.At(new Cell(5,8)).trap);
            Put(A,2,2); Assert.IsTrue(Do("passar")); Assert.AreSame(e,A); Put(e,5,9); int hp=e.hp;
            Assert.IsTrue(Do("mover",new Cell(5,6)));
            Assert.AreEqual(new Cell(5,8),e.cell,"para na armadilha"); Assert.AreEqual(hp-2,e.hp); Assert.AreEqual(0,e.pm);
            Assert.AreEqual(0,S.At(new Cell(5,8)).trap,"some depois de disparar");
        }
        [Test] public void BuffLastsTwoTurns()
        {
            var me=A; Stock("furia"); Assert.IsTrue(Buy()); Assert.AreEqual(8,me.pa);
            void Round() { Do("passar"); Do("passar"); }
            Round(); Assert.AreSame(me,A); Assert.AreEqual(8,me.pa,"segundo turno");
            Round(); Assert.AreEqual(6,me.pa,"acabou");
        }
        // Até 6 jogadores: todos contra todos (time 0) ou times (mesmo número = mesmo time).
        Match Players(params int[] teams) => Match.New(new MatchSetup{ flatMap=true, galeoes=4, players=teams.Select(t=>new PlayerSetup{team=t}).ToArray() });

        [Test] public void SixPlayersGrowTheMapAndRotateTurns()
        {
            m=Players(0,0,0,0,0,0);
            Assert.AreEqual(18,S.width); Assert.AreEqual(6,S.players); Assert.IsFalse(S.TeamPlay);
            CollectionAssert.AreEqual(new[]{1,2,3,4,5,6},S.timeline.Select(id=>S.F(id).team).ToArray());
            Assert.AreEqual(6,S.fighters.Select(f=>f.cell).Distinct().Count());
            for(int p=1;p<=6;p++) { Assert.AreEqual(p,S.Team(p)); Assert.AreEqual(Shop.Slots,S.shops[p].slots.Length); }
        }
        [Test] public void GeneratedMapsWorkForEveryPlayerCount()
        {
            for(int n=2;n<=6;n++) for(uint seed=1;seed<8;seed++)
            {
                var s=Match.New(new MatchSetup{ seed=seed, players=Enumerable.Range(0,n).Select(_=>new PlayerSetup()).ToArray() }).S;
                Assert.AreEqual(Board.SizeFor(n),s.width);
                for(int i=0;i<s.tiles.Length;i++) { var c=new Cell(i%s.width,i/s.width); Assert.AreEqual(s.At(c).height,s.At(s.Mirror(c)).height); }
                Assert.IsTrue(Board.Connected(s),$"{n} jogadores, seed {seed}");
                Assert.AreEqual(n,s.fighters.Select(f=>f.cell).Distinct().Count());
                Assert.IsTrue(s.fighters.All(f=>s.At(f.cell).terrain==Terrain.Livre));
            }
        }
        [Test] public void LastPlayerStandingWins()
        {
            m=Players(0,0,0);
            var p2=S.fighters.First(f=>f.team==2); var p3=S.fighters.First(f=>f.team==3);
            Put(A,5,5); Put(p2,5,6); Put(p3,5,7); p2.hp=1; p3.hp=1;
            m.Forced.Enqueue(Face.Escudo); m.Forced.Enqueue(Face.Trava);
            Assert.IsTrue(Do("skill",new Cell(5,6),"dragao-dourado"));
            Assert.IsTrue(S.eliminated[2]); Assert.IsFalse(S.over,"ainda há 2 jogadores");
            m.Forced.Enqueue(Face.Escudo); m.Forced.Enqueue(Face.Trava);
            Assert.IsTrue(Do("skill",new Cell(5,7),"dragao-dourado"));
            Assert.IsTrue(S.over); Assert.AreEqual(1,S.winner);
        }
        [Test] public void TeamsShareGaleoesAndAlliesAreNotEnemies()
        {
            m=Players(1,2,1,2); m.S.galeoes[1]=m.S.galeoes[2]=12;
            Assert.AreEqual(16,S.width); Assert.IsTrue(S.TeamPlay); Assert.IsTrue(S.Ally(1,3)); Assert.IsFalse(S.Ally(1,2));
            CollectionAssert.AreEqual(new[]{1,2,1,2},S.timeline.Select(id=>S.Team(S.F(id).team)).ToArray());
            Assert.IsTrue(S.fighters.Where(f=>S.Team(f.team)==1).All(f=>f.cell.z<S.height/2),"aliados do mesmo lado");
            var ally=S.fighters.First(f=>f.team==3); var enemy=S.fighters.First(f=>f.team==2);
            Put(A,5,5); Put(ally,5,6); Put(enemy,9,9);
            Assert.IsFalse(Do("skill",new Cell(5,6),"dragao-dourado"),"aliado não é alvo");
            Assert.IsTrue(m.Apply(Cmd("mover",new Cell(4,5)),A.team,out var ev,out _));
            Assert.IsFalse(ev.Any(e=>e.kind=="trava"),"aliado não trava");
            Put(enemy,4,7); enemy.hp=1; m.Forced.Enqueue(Face.Escudo); m.Forced.Enqueue(Face.Trava);
            Assert.IsTrue(Do("skill",new Cell(4,7),"dragao-dourado"));
            Assert.AreEqual(12-Content.Def(enemy).level,S.galeoes[2]); Assert.AreEqual(12,S.galeoes[1]);
            Assert.IsFalse(S.eliminated[2],"P4 segue no time 2"); Assert.IsFalse(S.over);
        }
        [Test] public void AlliesSeeEachOthersTraps()
        {
            m=Players(1,2,1,2); S.At(new Cell(3,3)).trap=1;
            Assert.AreEqual(1,Match.ViewFor(S,3).At(new Cell(3,3)).trap);
            Assert.AreEqual(0,Match.ViewFor(S,2).At(new Cell(3,3)).trap);
        }
        [Test] public void ThreeVsThreeAndTwoTeamsOfThreeSpawnApart()
        {
            m=Players(1,2,1,2,1,2);
            Assert.IsTrue(S.fighters.Where(f=>S.Team(f.team)==1).All(f=>f.cell.z<S.height/2));
            Assert.IsTrue(S.fighters.Where(f=>S.Team(f.team)==2).All(f=>f.cell.z>=S.height/2));
            m=Players(1,2,3,1,2,3);
            Assert.AreEqual(3,S.Teams().Count); Assert.AreEqual(6,S.fighters.Select(f=>f.cell).Distinct().Count());
            CollectionAssert.AreEqual(new[]{1,2,3,1,2,3},S.timeline.Select(id=>S.Team(S.F(id).team)).ToArray());
        }
        [Test] public void PetCoexistsWithDragon()
        {
            Put(A,5,5); Stock("lobo-de-cinzas");
            Assert.IsTrue(Do("skill",new Cell(5,6),"invocacao-dragao"));
            Assert.IsTrue(Buy()); Assert.IsTrue(Do("skill",new Cell(6,5),"chamar-lobo"));
            Assert.AreEqual(2,S.fighters.Count(x=>x.Alive&&x.summoner==A.id));
        }
    }
}
