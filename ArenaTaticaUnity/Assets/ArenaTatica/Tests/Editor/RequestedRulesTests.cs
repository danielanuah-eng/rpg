using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace ArenaTatica.Core.Tests
{
    public class RequestedRulesTests
    {
        static object Call(Match m,string name,params object[] args) =>
            typeof(Match).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(m,args);
        static Match New(string a="ezequiela",string b="perry") =>
            Match.New(new MatchSetup{flatMap=true,team1=new[]{a},team2=new[]{b}});
        [TearDown] public void Reset() => Content.ApplyEdits("");
        [Test] public void EzequielaStartsWithTwoOneHpCopiesWhoseMovementTracksHerBudget()
        {
            var m=New(); var s=m.S; var e=s.fighters.Single(x=>x.def=="ezequiela");
            var copies=s.fighters.Where(x=>x.def=="ilusao").ToList();
            Assert.AreEqual(2,copies.Count); Assert.AreEqual(6,Content.MaxHp(e));
            foreach(var copy in copies) {
                Assert.AreEqual(e.id,copy.summoner); Assert.AreEqual(1,copy.hp); Assert.AreEqual(1,Content.MaxHp(copy));
                Assert.AreEqual(3,Content.MaxPm(copy,s)); Assert.Contains(copy.id,s.timeline);
                copy.moveBudget=2;
            }
            e.buffs.Add(new Buff{id="botas-teste",pm=2,turns=4});
            foreach(var c in copies) {
                Call(m,"SyncCopy",c); Assert.AreEqual(5,Content.MaxPm(c,s)); Assert.AreEqual(3,c.pm);
                Call(m,"SyncCopy",c); Assert.AreEqual(3,c.pm,"sincronizar não recarrega PM gasto");
            }
            e.buffs.Clear();
            foreach(var c in copies) { Call(m,"SyncCopy",c); Assert.AreEqual(1,c.pm); }
        }
        [Test] public void EverySummonNeitherLocksNorRevealsByContactEvenWhenItsDefinitionDoes()
        {
            var m=New("ciantia","perry"); var s=m.S; var hero=s.fighters.Single(x=>x.def=="ciantia"); var perry=s.fighters.Single(x=>x.def=="perry");
            hero.cell=new Cell(1,1); perry.cell=new Cell(7,7); perry.hidden=true;
            var summon=(Fighter)Call(m,"Spawn","ciantia",hero.team,new Cell(7,5),hero.id);
            Assert.IsTrue(summon.noLock); Assert.IsTrue(summon.noReveal);
            Assert.AreEqual(0,Board.LockRisk(s,perry,perry.cell));
            summon.pm=3; Assert.IsTrue((bool)Call(m,"Move",summon,new Cell(7,6),null)); Assert.IsTrue(perry.hidden);
            perry.hidden=false;
            var view=Match.ViewFor(s,2); var shown=view.F(summon.id);
            Assert.AreEqual(0,Board.LockRisk(view,view.F(perry.id),perry.cell));
            Assert.IsTrue(shown.noLock); Assert.IsTrue(shown.noReveal);
        }
        [Test] public void EzequielaPortalRequiresRoundTwoAndCopyCannotExceedTwoActive()
        {
            var m=New(); var s=m.S; var e=s.fighters.Single(x=>x.def=="ezequiela");
            e.cell=new Cell(3,3); e.pa=6; var portal=Content.Skill(e,"portal-ezequiela");
            Assert.AreEqual(2,portal.pa); Assert.AreEqual(6,portal.maxRange); Assert.IsFalse(portal.sight);
            Assert.AreEqual(1,portal.perMatch); Assert.IsTrue(portal.ultimate);
            s.round=1; Assert.IsFalse(m.CanTarget(e,portal,new Cell(4,3),new Cell(5,3),out var why));
            Assert.IsTrue(why.Contains("rodada"));
            var copy=Content.Skill(e,"copia-ezequiela");
            Assert.AreEqual(2,copy.maxActive); Assert.AreEqual(3,copy.perMatch); Assert.AreEqual(1,copy.perTurn);
            Assert.IsFalse(m.CanTarget(e,copy,new Cell(3,4),out _));
        }
        [Test] public void BerserkKeepsHelpfulEffectsTakesDamageAndReturnsExactlyOneWithoutRecursion()
        {
            var m=New("tchu-tcha","tchu-tcha"); var s=m.S; var a=s.fighters.First(x=>x.team==1); var b=s.fighters.First(x=>x.team==2);
            a.form=b.form="berserk"; a.hp=b.hp=10; a.cell=new Cell(2,2); b.cell=new Cell(8,8);
            Call(m,"AddBuff",a,new Buff{id="negativo",pm=-2,armor=-3,turns=3});
            Assert.IsFalse(a.buffs.Any(x=>x.pm<0||x.armor<0));
            Call(m,"AddBuff",a,new Buff{id="positivo",pm=2,turns=3});
            Assert.AreEqual(2,a.buffs.Single(x=>x.id=="positivo").pm);
            var log=new List<GameEvent>(); Call(m,"Hurt",a,3,log,b,true);
            Assert.AreEqual(7,a.hp); Assert.AreEqual(9,b.hp);
            Assert.AreEqual(1,log.Count(x=>x.kind=="refletir"));
            Call(m,"Heal",a,2,b,"teste"); Assert.AreEqual(9,a.hp,"cura aliada continua válida");
            Call(m,"Push",a,new Cell(1,0),4,b,false); Assert.AreEqual(new Cell(2,2),a.cell);
        }
    }
}

