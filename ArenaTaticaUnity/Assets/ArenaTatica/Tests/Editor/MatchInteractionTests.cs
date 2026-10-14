using System.Linq;
using NUnit.Framework;
using UnityEngine;
using ArenaTatica.Core;

namespace ArenaTatica.Tests
{
    public class MatchInteractionTests
    {
        Match Duel(string mine="ciantia",string enemy="ogro-de-pedra") => Match.New(new MatchSetup{flatMap=true,team1=new[]{mine},team2=new[]{enemy}});
        bool Skill(Match m,string id,Cell c,out System.Collections.Generic.List<GameEvent> events) => m.Apply(new Command{kind="skill",fighter=m.S.Active.id,revision=m.S.revision,skill=id,cell=c},m.S.Active.team,out events,out _);
        void Activate(Match m,Fighter f) { m.S.active=m.S.timeline.IndexOf(f.id); f.pa=30; f.pm=10; }

        [Test] public void GroundAttackConsumesCostAndEmitsMissAtChosenTile()
        {
            var m=Duel(); var f=m.S.fighters.First(x=>x.team==1); Activate(m,f); f.cell=new Cell(3,3);
            var sk=Match.Skills(f).First(x=>x.kind==SkillKind.Dano&&!x.selfArea); var c=new Cell(3,3+sk.minRange); int pa=f.pa;
            Assert.IsTrue(Skill(m,sk.id,c,out var events)); Assert.AreEqual(pa-sk.pa,f.pa);
            Assert.IsTrue(events.Any(e=>e.kind=="ataque"&&e.target==0&&e.cell==c));
            Assert.IsFalse(m.CanTarget(f,sk,new Cell(11,11),out _));
        }
        [Test] public void GroundAttackHitsHiddenEnemyBeyondAdjacentRange()
        {
            var m=Duel(); var f=m.S.fighters.First(x=>x.team==1); var v=m.S.fighters.First(x=>x.team==2); Activate(m,f);
            f.cell=new Cell(3,3); v.cell=new Cell(3,5); v.hidden=true;
            var sk=Match.Skills(f).First(x=>x.kind==SkillKind.Dano&&x.minRange<=2&&x.maxRange>=2); int hp=v.hp;
            var projected=Match.ViewFor(m.S,f.team); Assert.AreEqual(Cell.Unknown,projected.F(v.id).cell);
            Assert.IsTrue(Match.FromState(projected,true).CanTarget(projected.F(f.id),sk,v.cell,out _));
            m.Forced.Enqueue(Face.Trava); m.Forced.Enqueue(Face.Trava);
            Assert.IsTrue(Skill(m,sk.id,v.cell,out var events)); Assert.Less(v.hp,hp); Assert.IsFalse(v.hidden);
            Assert.IsTrue(events.Any(e=>e.kind=="revelar"&&e.target==v.id));
        }
        [TestCase(0)] [TestCase(2)] public void HiddenCollisionStopsBeforeOccupantAndPreservesUnspentPm(int steps)
        {
            var m=Duel(); var f=m.S.fighters.First(x=>x.team==1); var v=m.S.fighters.First(x=>x.team==2); Activate(m,f);
            f.cell=new Cell(2,2); v.cell=new Cell(3+steps,2); v.hidden=true; int pm=f.pm;
            var path=Board.Path(m.S,f,v.cell,f.pm); Assert.IsNotNull(path);
            var view=Match.ViewFor(m.S,f.team); CollectionAssert.AreEqual(path,Board.Path(view,view.F(f.id),v.cell,f.pm));
            Assert.IsTrue(m.Apply(new Command{kind="mover",fighter=f.id,revision=m.S.revision,cell=v.cell},f.team,out var events,out _));
            Assert.AreEqual(new Cell(2+steps,2),f.cell); Assert.AreEqual(pm-steps,f.pm); Assert.IsFalse(v.hidden);
            Assert.IsFalse(events.Any(e=>e.path!=null&&e.path.Contains(v.cell))); Assert.AreNotEqual(f.cell,v.cell);
        }
        [Test] public void KnownEnemyAndFriendlyHiddenOccupantsStillBlockPlanning()
        {
            var m=Duel(); var f=m.S.Active; var v=m.S.fighters.First(x=>x.team!=f.team); f.cell=new Cell(2,2); v.cell=new Cell(3,2);
            Assert.IsNull(Board.Path(m.S,f,v.cell,10)); v.hidden=true; v.team=f.team; Assert.IsNull(Board.Path(m.S,f,v.cell,10));
        }
        [Test] public void EmptyHealingAndSupportNeverHealEnemyOrThrow()
        {
            var m=Duel(); var f=m.S.fighters.First(x=>x.team==1); Activate(m,f); f.cell=new Cell(3,3);
            foreach(var id in new[]{"palavra-curativa","chuva-de-cura","espelho-primordial"}) {
                f.learned.Add(id); var sk=Content.Skill(f,id); if(sk==null) continue;
                var c=new Cell(3,4); if(!m.InRange(f,sk,c,out _)) continue;
                Assert.IsTrue(Skill(m,id,c,out _),id); Assert.IsTrue(ArenaGame.ValidState(m.S));
            }
        }
        [Test] public void ShadowCloneUsesCasterIdentityAndEnemyProjectionKeepsDisguise()
        {
            var m=Duel(); var f=m.S.fighters.First(x=>x.team==1); Activate(m,f); f.cell=new Cell(3,3); f.learned.Add("clone-das-sombras");
            Assert.IsTrue(Skill(m,"clone-das-sombras",new Cell(3,4),out var events)); var clone=m.S.fighters.Last();
            Assert.AreEqual(f.def,clone.disguise); Assert.AreEqual(f.team,clone.team); Assert.AreEqual(f.id,clone.summoner);
            var view=Match.ViewFor(m.S,2); Assert.AreEqual(f.def,view.F(clone.id).def); Assert.AreEqual(0,view.F(clone.id).summoner);
            Assert.AreEqual(f.def,Match.EventsFor(events,m.S,2).First(e=>e.kind=="invocar").info); Assert.IsTrue(ArenaGame.ValidState(view,2));
        }
        [Test] public void CopyWishCanBeCastOnEmptyTileWithoutTargetPickerOrException()
        {
            var m=Duel("principe-kellen"); var f=m.S.fighters.First(x=>x.team==1); Activate(m,f); f.wishReady=true; f.cell=new Cell(3,3);
            var sk=Content.Skill(f,"desejo-d"); var c=new Cell(3,3+sk.minRange);
            Assert.IsTrue(Skill(m,sk.id,c,out _)); Assert.IsFalse(f.wishReady); Assert.AreEqual(0,f.learned.Count);
        }
        [Test] public void ColossusDevoursDeathCausedByDifferentFighterIncludingAlly()
        {
            var m=Match.New(new MatchSetup{flatMap=true,team1=new[]{"colosso","talyssa"},team2=new[]{"ogro-de-pedra"}});
            var col=m.S.fighters.First(x=>x.def=="colosso"); var killer=m.S.fighters.First(x=>x.def=="ogro-de-pedra"); var v=m.S.fighters.First(x=>x.def=="talyssa");
            Activate(m,killer); killer.cell=new Cell(3,3); v.cell=new Cell(3,4); v.hp=1;
            m.Forced.Enqueue(Face.Trava); m.Forced.Enqueue(Face.Trava);
            Assert.IsTrue(Skill(m,Content.Def(killer).skills[0].id,v.cell,out var events));
            CollectionAssert.Contains(col.stolen,"lamina-rosa"); Assert.AreEqual(1,events.Count(e=>e.kind=="devorar"&&e.actor==col.id));
            Assert.IsTrue(v.deathProcessed);
        }
        [Test] public void ColossusObservesTrapDeathAndDoesNotTriggerTwice()
        {
            var m=Match.New(new MatchSetup{flatMap=true,team1=new[]{"colosso"},team2=new[]{"talyssa","ogro-de-pedra"}});
            var col=m.S.fighters.First(x=>x.def=="colosso"); var v=m.S.fighters.First(x=>x.def=="talyssa"); Activate(m,v); v.cell=new Cell(3,3); v.hp=1;
            var c=new Cell(3,4); m.S.At(c).trap=1; m.S.At(c).trapDamage=20;
            Assert.IsTrue(m.Apply(new Command{kind="mover",fighter=v.id,revision=m.S.revision,cell=c},v.team,out var events,out _));
            CollectionAssert.Contains(col.stolen,"lamina-rosa"); Assert.AreEqual(1,events.Count(e=>e.kind=="devorar"&&e.actor==col.id));
        }
    }
}
