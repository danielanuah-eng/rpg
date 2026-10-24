using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace ArenaTatica.Core.Tests
{
    // Batalha contra o chefe Rei Thaagarys (Boss.cs, autor 22/10): trono, ordem de turnos, fases e o pool da IA.
    public class BossTests
    {
        const string King="rei-thaagarys";
        Match m;
        MatchState S => m.S;
        Fighter Boss => m.Boss;
        List<Fighter> Heroes => S.fighters.Where(f=>f.Alive&&MatchState.Player(f.team)&&f.summoner==0).ToList();

        static Match New(int players=2,params string[] team) => Match.New(new MatchSetup{ flatMap=true, boss=King,
            players=Enumerable.Range(0,players).Select(_=>new PlayerSetup{fighters=team.Length>0?team:new[]{"ciantia"}}).ToArray() });
        [SetUp] public void Setup() { m=New(); for(int i=0;i<60;i++) m.Forced.Enqueue(Face.Trava); }
        [TearDown] public void Clean() => Content.ApplyEdits("");

        // Só essa skill no pool (peso 10); o movimento fica com o peso dado.
        static void Only(string skill,int move=0)
        {
            var d=Content.Copy(Content.Default(King));
            foreach(var s in d.skills) s.weight=s.id==skill?10:0;
            d.boss.moveWeight=move;
            Content.ApplyEdits(JsonUtility.ToJson(new GameEdits{fighters=new List<FighterDef>{d}}));
        }
        // De pé, colado no Trono (lado de baixo), e os heróis longe, salvo quem o teste aproxima.
        Fighter Stand()
        {
            var b=Boss; b.form="espada"; var throne=MapGen.ShopCells(S); b.cell=new Cell(throne.Min(c=>c.x),throne.Min(c=>c.z)-2);
            int k=0; foreach(var h in Heroes) h.cell=new Cell(k++,S.height-1);
            // Estes testes isolam as skills com um foco conhecido; o sorteio é coberto em BossTargetCycleTests.
            S.bossCycles.Add(new BossCycle{boss=b.id,order=Heroes.Select(h=>h.team).Distinct().OrderBy(t=>t).ToList(),pending=Heroes.Select(h=>h.team).Distinct().OrderBy(t=>t).ToList()});
            return b;
        }
        // Passa o turno do herói logo antes de uma entrada do chefe: o chefe joga e a vez volta a um herói.
        List<GameEvent> BossTurn()
        {
            int i=Enumerable.Range(1,S.timeline.Count-1).First(k=>S.timeline[k]==Boss.id&&S.F(S.timeline[k-1]) is Fighter h&&h.Alive&&MatchState.Player(h.team));
            S.active=i-1; var hero=S.Active;
            Assert.IsTrue(m.Apply(new Command{kind="passar",fighter=hero.id,revision=S.revision},hero.team,out var events,out var why),why);
            return events;
        }

        [Test] public void SetupSeatsTheKingOnTheThroneAndKeepsPvp()
        {
            var b=Boss; var throne=MapGen.ShopCells(S);
            Assert.IsNotNull(b); Assert.AreEqual(MatchState.Neutral,b.team); Assert.AreEqual("trono",b.form); Assert.IsTrue(Match.Seated(b));
            CollectionAssert.AreEquivalent(throne,Board.Footprint(b).ToArray(),"sentado em cima das 4 casas do Trono");
            Assert.IsTrue(throne.All(c=>S.At(c).terrain==Terrain.Loja),"o Trono é a loja");
            Assert.AreNotEqual(S.alliance[1],S.alliance[2],"PvP desde o início"); Assert.AreEqual(VictoryMode.Eliminacao,S.mode);
            Assert.AreEqual(50+15,Content.MaxHp(b)); Assert.AreEqual(Content.MaxHp(b),b.hp);
            Assert.IsTrue(ArenaGame.ValidState(S));
            // Estado da rede: os campos novos voltam iguais.
            var copy=JsonUtility.FromJson<MatchState>(JsonUtility.ToJson(S));
            Assert.AreEqual(King,copy.bossFight); Assert.AreEqual(b.form,copy.F(b.id).form);
            Assert.AreEqual(b.hp,copy.F(b.id).hp); Assert.IsTrue(ArenaGame.ValidState(copy));
        }

        [Test] public void BossTakesATurnAfterEveryTwoPlayerTurns()
        {
            m=New(2,"ciantia","talyssa");
            var line=S.timeline.Select(id=>S.F(id)).ToList();
            Assert.AreEqual(6,line.Count,"4 heróis + o chefe 2 vezes");
            for(int i=0;i<line.Count;i++) Assert.AreEqual(i%3==2,BossOf(line[i]),$"posição {i}");
            // Rodada 1: o chefe observa do trono; no fim dela, levanta.
            var log=new List<GameEvent>();
            for(int k=0;k<12&&S.round==1;k++) { var a=S.Active; Assert.IsTrue(m.Apply(new Command{kind="passar",fighter=a.id,revision=S.revision},a.team,out var ev,out var why),why); log.AddRange(ev); }
            Assert.AreEqual(2,log.Count(e=>e.kind=="chefe-trono"),"2 turnos do chefe sentado na rodada 1");
            Assert.AreEqual(2,S.round);
            var up=log.FindIndex(e=>e.kind=="chefe-levanta"); Assert.Greater(up,log.FindLastIndex(e=>e.kind=="chefe-trono"),"levanta no fim da rodada 1");
            Assert.AreEqual("espada",Boss.form); Assert.IsFalse(Match.Seated(Boss));
            Assert.IsFalse(Board.Footprint(Boss).Any(c=>S.At(c).terrain==Terrain.Loja),"saiu de cima do Trono");
            Assert.IsTrue(Board.Fits(S,Boss,Boss.cell));
            Assert.IsTrue(MatchState.Player(S.Active.team),"a vez volta a um herói");
        }
        static bool BossOf(Fighter f) => Match.BossOf(f)!=null;

        [Test] public void PhaseTwoDrawsTheSecondSwordAndStrikesTwice()
        {
            Only("lamina-real"); var b=Stand(); var hero=Heroes[0]; hero.cell=new Cell(b.cell.x+2,b.cell.z); hero.hp=99;
            var first=BossTurn();
            Assert.AreEqual(1,first.Count(e=>e.kind=="ataque"&&e.actor==b.id),"fase 1: 1 ação");
            b.hp=Content.MaxHp(b)/2; hero.cell=new Cell(b.cell.x+2,b.cell.z);
            Heroes[1].hp=99; Heroes[1].cell=new Cell(b.cell.x-1,b.cell.z);
            var second=BossTurn();
            Assert.AreEqual("duas-espadas",b.form); Assert.AreEqual(2,Match.BossPhase(b));
            Assert.IsTrue(second.Any(e=>e.kind=="chefe-fase"),"evento da transição");
            Assert.AreEqual(2,second.Count(e=>e.kind=="ataque"&&e.actor==b.id&&e.info=="lamina-real"),"fase 2: 2 ações e golpe 2× no turno");
        }

        [Test] public void PhaseTwoVortexPullsNearbyHeroes()
        {
            Only("vortice-real"); var b=Stand(); b.form="duas-espadas"; var hero=Heroes[0]; hero.hp=99; hero.cell=new Cell(b.cell.x+4,b.cell.z);
            Assert.AreEqual(3,Board.Gap(b,hero));
            var ev=BossTurn();
            Assert.IsTrue(ev.Any(e=>e.kind=="area"&&e.actor==b.id));
            Assert.AreEqual(1,Board.Gap(b,hero),"sugado até colar");
        }

        [Test] public void SwapsLifeOnlyWhenItHelps()
        {
            Only("troca-da-coroa"); var b=Stand(); var hero=Heroes[0]; hero.cell=new Cell(b.cell.x+3,b.cell.z); hero.hp=10;
            b.form="duas-espadas"; b.hp=Content.MaxHp(b)/2;
            BossTurn(); Assert.AreEqual(Content.MaxHp(b)/2,b.hp,"o herói tem menos PV: não troca");
            b.hp=4; b.cooldowns.Clear(); BossTurn();
            Assert.AreEqual(10,b.hp); Assert.AreEqual(4,hero.hp);
        }

        [Test] public void TeleportsNextToTheWeakestHero()
        {
            Only("passo-do-trono"); var b=Stand(); var weak=Heroes[0]; weak.hp=3; weak.cell=new Cell(9,8); Heroes[1].cell=new Cell(0,0); Heroes[1].hp=20;
            var ev=BossTurn();
            Assert.IsTrue(ev.Any(e=>e.kind=="teleporte"&&e.actor==b.id)); Assert.AreEqual(1,Board.Gap(b,weak));
        }

        [Test] public void SummonsGuardsThatPlayTheirOwnTurns()
        {
            Only("chamado-da-guarda"); var b=Stand(); foreach(var h in Heroes) h.hp=99; Heroes[0].cell=new Cell(b.cell.x+4,b.cell.z);
            var ev=BossTurn();
            var guard=S.fighters.FirstOrDefault(f=>f.def=="guarda-do-trono");
            Assert.IsNotNull(guard); Assert.AreEqual(MatchState.Neutral,guard.team); Assert.AreEqual(b.id,guard.summoner);
            Assert.IsTrue(S.timeline.Contains(guard.id));
            // A guarda joga sozinha: a vez nunca para nela.
            for(int k=0;k<10;k++) { var a=S.Active; Assert.IsTrue(MatchState.Player(a.team)); m.Apply(new Command{kind="passar",fighter=a.id,revision=S.revision},a.team,out _,out _); }
        }

        [Test] public void RaisesMazeWallsThatFallAfterTwoRounds()
        {
            Only("muralhas-do-labirinto"); var b=Stand(); var hero=Heroes[0]; hero.cell=new Cell(b.cell.x+3,b.cell.z); hero.hp=99;
            var ev=BossTurn(); var wall=ev.First(e=>e.kind=="muralha");
            Assert.Greater(wall.path.Length,0); Assert.IsTrue(wall.path.All(c=>S.At(c).terrain==Terrain.Parede));
            Assert.AreEqual(wall.path.Length,S.walls.Count); Assert.IsTrue(Board.Connected(S),"ninguém fica preso");
            int round=S.round; foreach(var h in Heroes) h.hp=99;
            for(int k=0;k<40&&S.round<round+2&&!S.over;k++) { var a=S.Active; m.Apply(new Command{kind="passar",fighter=a.id,revision=S.revision},a.team,out _,out _); }
            Assert.IsFalse(wall.path.Any(c=>S.At(c).terrain==Terrain.Parede),"caem depois de 2 rodadas"); Assert.AreEqual(0,S.walls.Count);
        }

        [Test] public void RevealsHiddenHeroesAnywhere()
        {
            Only("olhar-do-rei"); Stand(); var hero=Heroes[1]; hero.hidden=true;
            var ev=BossTurn();
            Assert.IsFalse(hero.hidden); Assert.IsTrue(ev.Any(e=>e.kind=="revelar"&&e.target==hero.id));
        }

        [Test] public void ResetsEveryonesUltimates()
        {
            Only("coroacao-renovada"); Stand(); var hero=Heroes[0];
            hero.usedMatch.Add("troca-de-vida"); hero.cooldowns.Add(new Charge{skill="tiro-arcano",uses=3});
            var ev=BossTurn();
            Assert.IsTrue(ev.Any(e=>e.kind=="reinicio"));
            Assert.IsFalse(hero.usedMatch.Contains("troca-de-vida")); Assert.AreEqual(0,hero.cooldowns.Count);
            Assert.IsTrue(Boss.cooldowns.Any(c=>c.skill=="coroacao-renovada"),"a própria skill continua em recarga");
        }

        [Test] public void TriggersARandomEventFromItsPool()
        {
            Only("decreto-do-caos"); var d=Content.Copy(Content.Fighters[King]); d.boss.events="chuva-de-moedas"; Content.ApplyEdits(JsonUtility.ToJson(new GameEdits{fighters=new List<FighterDef>{d}}));
            Stand();
            var ev=BossTurn();
            Assert.IsTrue(ev.Any(e=>e.kind=="cataclisma"&&e.info=="chuva-de-moedas"),"usa a API de eventos com o pool do chefe");
        }

        [Test] public void WalksWhenNothingElseFits()
        {
            Only("none",move:5); var b=Stand(); var hero=Heroes[0]; hero.cell=new Cell(b.cell.x,S.height-1);
            int before=Heroes.Min(h=>Board.Gap(b,h)); var from=b.cell; var log=BossTurn();
            Assert.AreNotEqual(from,b.cell,"inicia o desvio ao redor do Trono");
            for(int i=0;i<3&&Heroes.Min(h=>Board.Gap(b,h))>=before;i++) BossTurn();
            Assert.Less(Heroes.Min(h=>Board.Gap(b,h)),before,"aproxima depois do desvio");
        }

        [Test] public void TheKingFallingKeepsPvpAndAllHeroesDownLose()
        {
            var b=Stand(); var hero=Heroes[0]; S.active=S.timeline.IndexOf(hero.id); b.hp=0;
            m.Apply(new Command{kind="passar",fighter=hero.id,revision=S.revision},hero.team,out _,out _);
            Assert.IsFalse(S.over,"o PvP continua depois da queda do chefe");
            m=New(); var king=m.Boss; foreach(var h in m.S.fighters.Where(f=>MatchState.Player(f.team))) h.hp=0;
            var a=m.S.timeline.Select(m.S.F).First(f=>MatchState.Player(f.team)); a.hp=1; m.S.active=m.S.timeline.IndexOf(a.id);
            a.hp=0; m.Apply(new Command{kind="passar",fighter=a.id,revision=m.S.revision},a.team,out _,out _);
            Assert.IsTrue(m.S.over); Assert.AreEqual(MatchState.Neutral,m.S.winner,"o chefe vence");
            Assert.IsTrue(king.Alive);
        }

        [Test] public void EditorKeepsBossDataAndLimits()
        {
            var d=Content.Copy(Content.Default(King));
            Assert.IsTrue(d.boss.on); Assert.AreEqual(9,d.skills.Length);
            d.hp=120; d.boss.every=3; d.skills[0].weight=7;
            var clean=Content.Sanitize(Content.Copy(d));
            Assert.AreEqual(120,clean.hp,"chefe passa de 40 PV"); Assert.AreEqual(9,clean.skills.Length,"chefe tem mais de 8 skills");
            Content.ApplyEdits(JsonUtility.ToJson(new GameEdits{fighters=new List<FighterDef>{d}}));
            Assert.AreEqual(3,Content.Fighters[King].boss.every); Assert.AreEqual(7,Content.Fighters[King].skills[0].weight);
            var normal=Content.Sanitize(Content.Copy(Content.Fighters["ciantia"])); normal.hp=120;
            Assert.AreEqual(40,Content.Sanitize(normal).hp,"lutador comum continua até 40");
            // Ritmo editado vale na próxima partida.
            m=New(2,"ciantia","talyssa");
            Assert.AreEqual(1,S.timeline.Count(id=>id==Boss.id),"4 heróis, chefe a cada 3: 1 entrada");
        }
    }
}
