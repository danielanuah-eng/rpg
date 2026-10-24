using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ArenaTatica.Core;

namespace ArenaTatica.Tests
{
    // Espelho primordial: comandos reais e ciclo da inversão de buffs (Match.Effects.cs).
    public class EffectInversionTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static Match NewMatch() => Match.New(new MatchSetup{seed=5,flatMap=true,team1=new[]{"edgar","ciantia"},team2=new[]{"talyssa","perry"}});
        static Fighter FighterOf(Match m,string def) => m.S.fighters.First(f=>f.def==def);
        static void Apply(Match m,Fighter target,int turns,Fighter by) => typeof(Match).GetMethod("ApplyInversion",Private).Invoke(m,new object[]{target,turns,by});
        static void Add(Match m,Fighter target,Buff buff) => typeof(Match).GetMethod("AddBuff",Private).Invoke(m,new object[]{target,buff});
        static void Clear(Match m,Fighter f) => typeof(Match).GetMethod("ClearInversion",Private).Invoke(m,new object[]{f});
        static void Tick(Match m,Fighter f) => typeof(Match).GetMethod("TickInversion",Private).Invoke(m,new object[]{f});
        static int Healing(Match m,Fighter f,int amount) => (int)typeof(Match).GetMethod("InvertedHealing",Private).Invoke(m,new object[]{f,amount});
        static List<GameEvent> Events(Match m) => (List<GameEvent>)typeof(Match).GetField("events",Private).GetValue(m);
        static Buff Full() => new Buff{id="teste",from="x",turns=3,pa=1,pm=2,armor=3,range=1,extra=1,dmg=2,regen=3,reflect=1,immune=1,taken=2,takenDivisor=4};

        [TestCase(true)] [TestCase(false)]
        public void MirrorCanActuallyBeCastOnEnemiesAndAllies(bool enemy)
        {
            var m=Match.New(new MatchSetup{seed=5,flatMap=true,team1=new[]{"ezequiela","edgar"},team2=new[]{"talyssa"}});
            var e=FighterOf(m,"ezequiela"); var target=FighterOf(m,enemy?"talyssa":"edgar");
            e.cell=new Cell(3,3); target.cell=new Cell(4,3);
            m.S.active=m.S.timeline.IndexOf(e.id); e.pa=6;
            var buff=new Buff{id="protecao-teste",from="teste",turns=3,armor=2};
            target.buffs.Add(buff);
            var cmd=new Command{kind="skill",skill="espelho-primordial",cell=target.cell,fighter=e.id,revision=m.S.revision};
            Assert.IsTrue(m.Apply(cmd,e.team,out var events,out var reason),reason);
            Assert.AreEqual(2,target.inversionTurns); Assert.AreEqual(-2,buff.armor);
            Assert.IsTrue(events.Any(x=>x.kind==Match.InversionEvent&&x.target==target.id));
            var slow=new Buff{id="lentidao-teste",from="teste",turns=2,pm=-2};
            Add(m,target,slow); Assert.AreEqual(2,slow.pm,"novos debuffs também viram buffs");
        }

        [Test] public void ActiveBuffsFlipAndRestoreWithoutTouchingDuration()
        {
            var m=NewMatch(); var f=FighterOf(m,"edgar"); var by=FighterOf(m,"ciantia"); var b=Full();
            Add(m,f,b); Assert.AreEqual(3,b.armor);
            Apply(m,f,2,by);
            Assert.AreEqual(2,f.inversionTurns);
            Assert.IsTrue(b.inverted);
            Assert.AreEqual(-1,b.pa); Assert.AreEqual(-2,b.pm); Assert.AreEqual(-3,b.armor); Assert.AreEqual(-1,b.range);
            Assert.AreEqual(-1,b.extra); Assert.AreEqual(-2,b.dmg); Assert.AreEqual(-3,b.regen);
            Assert.AreEqual(4,b.taken); Assert.AreEqual(2,b.takenDivisor);
            var ev=Events(m).Last(e=>e.kind==Match.InversionEvent);
            Assert.AreEqual(Match.InversionEvent,ev.kind); Assert.AreEqual(by.id,ev.actor); Assert.AreEqual(f.id,ev.target); Assert.AreEqual(2,ev.value);
            Tick(m,f); Assert.AreEqual(1,f.inversionTurns); Assert.IsTrue(b.inverted);
            Tick(m,f);
            Assert.AreEqual(0,f.inversionTurns); Assert.IsFalse(b.inverted);
            Assert.AreEqual(1,b.pa); Assert.AreEqual(3,b.armor); Assert.AreEqual(2,b.dmg); Assert.AreEqual(1,b.reflect);
            Assert.AreEqual(3,b.turns,"o fim da inversao nao mexe na duracao do buff");
        }
        [Test] public void NewBuffsDuringInversionEnterFlipped()
        {
            var m=NewMatch(); var f=FighterOf(m,"edgar"); var by=FighterOf(m,"ciantia");
            Apply(m,f,2,by);
            var b=new Buff{id="novo",from="x",turns=4,armor=2,dmg=1};
            Add(m,f,b);
            Assert.IsTrue(b.inverted); Assert.AreEqual(-2,b.armor); Assert.AreEqual(-1,b.dmg);
            Tick(m,f); Tick(m,f);
            Assert.IsFalse(b.inverted); Assert.AreEqual(2,b.armor); Assert.AreEqual(1,b.dmg); Assert.AreEqual(4,b.turns);
        }
        [Test] public void SecondCastRenewsAndDoesNotFlipTwice()
        {
            var m=NewMatch(); var f=FighterOf(m,"edgar"); var by=FighterOf(m,"ciantia"); var b=Full();
            Add(m,f,b); Apply(m,f,2,by); Apply(m,f,2,by);
            Assert.AreEqual(2,f.inversionTurns); Assert.IsTrue(b.inverted);
            Assert.AreEqual(-3,b.armor,"o 2o cast so renova a duracao");
            Assert.AreEqual(4,b.taken); Assert.AreEqual(2,b.takenDivisor);
            Assert.AreEqual(2,Events(m).Count(e=>e.kind==Match.InversionEvent));
        }
        [Test] public void ExpiredBuffIsNotResurrectedNorProlonged()
        {
            var m=NewMatch(); var f=FighterOf(m,"edgar"); var by=FighterOf(m,"ciantia");
            var b=new Buff{id="curto",from="x",turns=1,armor=2};
            Add(m,f,b); Apply(m,f,2,by); Assert.AreEqual(-2,b.armor);
            f.buffs.Remove(b); // expirou no fim do turno (Match.RemoveAll turns<=0)
            Tick(m,f); Tick(m,f);
            Assert.AreEqual(0,f.inversionTurns);
            Assert.IsFalse(f.buffs.Contains(b),"buff expirado nao volta para a lista");
            Assert.AreEqual(1,b.turns,"a inversao nao prolonga a duracao");
            Clear(m,f); Assert.AreEqual(0,f.inversionTurns);
        }
        [Test] public void EffectImmuneFormRefusesInversionAndKeepsOnlyPositives()
        {
            var m=NewMatch(); var f=FighterOf(m,"edgar"); var by=FighterOf(m,"ciantia");
            var form=Content.Form(f); Assert.IsNotNull(form);
            bool saved=form.effectImmune;
            try {
                form.effectImmune=true;
                Assert.IsTrue(Content.Form(f).effectImmune,"a forma mutada e a mesma que o Match consulta");
                var existing=Full(); Add(m,f,existing); Assert.AreEqual(3,existing.armor);
                Apply(m,f,2,by);
                Assert.AreEqual(0,f.inversionTurns); Assert.IsFalse(existing.inverted); Assert.AreEqual(3,existing.armor);
                Assert.IsFalse(Events(m).Any(e=>e.kind==Match.InversionEvent));
                var bad=new Buff{id="mau",from="x",turns=2,pa=-1,pm=-2,armor=-3,range=-1,extra=-1,dmg=-2,regen=-3,reflect=-1,immune=-1,taken=2};
                Add(m,f,bad);
                Assert.IsTrue(f.buffs.Contains(bad));
                Assert.AreEqual(0,bad.pa); Assert.AreEqual(0,bad.pm); Assert.AreEqual(0,bad.armor); Assert.AreEqual(0,bad.range);
                Assert.AreEqual(0,bad.extra); Assert.AreEqual(0,bad.dmg); Assert.AreEqual(0,bad.regen);
                var good=new Buff{id="bom",from="x",turns=2,armor=2,regen=1,dmg=1};
                Add(m,f,good);
                Assert.AreEqual(2,good.armor); Assert.AreEqual(1,good.regen); Assert.AreEqual(1,good.dmg);
                var noHeal=new Buff{id=Match.NoHealBuff,from="x",turns=5};
                Add(m,f,noHeal);
                Assert.IsFalse(f.buffs.Contains(noHeal),"sem-cura e recusado por forma imune a efeitos");
            } finally { form.effectImmune=saved; }
        }
        [Test] public void ReflectImmuneAndTakenAreReversible()
        {
            var m=NewMatch(); var f=FighterOf(m,"edgar"); var by=FighterOf(m,"ciantia");
            var b=new Buff{id="guarda",from="x",turns=2,reflect=1,immune=1,taken=3,takenDivisor=1};
            Add(m,f,b); Apply(m,f,2,by);
            Assert.AreEqual(1,b.taken); Assert.AreEqual(3,b.takenDivisor);
            Tick(m,f); Tick(m,f);
            Assert.AreEqual(3,b.taken); Assert.AreEqual(1,b.takenDivisor);
        }
        [Test] public void NoHealAndInvertedHealSwapWhileDoubleHealingHolds()
        {
            var m=NewMatch(); var f=FighterOf(m,"edgar"); var by=FighterOf(m,"ciantia");
            Add(m,f,new Buff{id=Match.NoHealBuff,from="x",turns=5});
            Assert.AreEqual(3,Healing(m,f,3));
            Apply(m,f,2,by);
            var b=f.buffs.Single(x=>x.id==Match.InvertedHealBuff);
            Assert.IsTrue(b.inverted);
            Assert.AreEqual(6,Healing(m,f,3),"espelho-cura dobra a cura positiva");
            Assert.AreEqual(-2,Healing(m,f,-2),"cura negativa (dano) nao dobra");
            Assert.AreEqual(0,Healing(m,f,0));
            Tick(m,f); Tick(m,f);
            Assert.AreEqual(Match.NoHealBuff,f.buffs.Single(x=>x.turns==5).id);
            Assert.AreEqual(3,Healing(m,f,3));
        }
    }
}
