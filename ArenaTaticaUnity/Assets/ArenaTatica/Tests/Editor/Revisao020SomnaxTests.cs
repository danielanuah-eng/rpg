using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
namespace ArenaTatica.Core.Tests
{
 // Revisão de Claude (23/10) sobre Somnax, guarda do Tchu Tcha e rede.
 public class Revisao020SomnaxTests
 {
  static Match New(string first,string enemy="perry") {
   var m=Match.New(new MatchSetup{flatMap=true,team1=new[]{first,"ciantia","talyssa"},team2=new[]{enemy}});
   foreach(var t in m.S.tiles){t.terrain=Terrain.Livre;t.height=0;t.obj=0;t.coins=0;t.trap=0;}
   var a=m.S.fighters.First(x=>x.def==first);a.cell=new Cell(2,2);a.pa=20;a.pm=5;
   int z=4;foreach(var f in m.S.fighters.Where(x=>x!=a)) f.cell=new Cell(7,z++);
   m.S.active=m.S.timeline.IndexOf(a.id);m.S.placing=0;return m;
  }
  static Fighter Ally(Match m,Fighter a)=>m.S.fighters.First(x=>x!=a&&x.team==a.team&&x.summoner==0);
  [Test] public void PrisonedTchuDoesNotAbsorbAlliesDamage(){
   var m=New("tchu-tcha"); var t=m.S.Active; var ally=Ally(m,t); ally.cell=new Cell(3,2); ally.hp=10; t.prisoned=2; int hp=t.hp;
   var hurt=typeof(Match).GetMethod("Hurt",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
   hurt.Invoke(m,new object[]{ally,3,new List<GameEvent>(),null,true});
   Assert.AreEqual(hp,t.hp,"preso não guarda"); Assert.AreEqual(7,ally.hp);
  }
  [Test] public void TwoCellSkillChecksBasePaOnTheFirstCell(){
   var m=New("somnax"); var a=m.S.Active; var sk=Match.Skills(a).FirstOrDefault(s=>s.kind==SkillKind.DoceSalgado); Assume.That(sk!=null&&sk.TwoCells);
   var ally=Ally(m,a); ally.cell=new Cell(3,2); a.pa=Match.Cost(a,sk)-1;
   Assert.IsFalse(m.CanTarget(a,sk,ally.cell,out var why)); StringAssert.Contains("PA",why);
  }
  [Test] public void EnemySomnaxPrivateStateIsNotSent(){
   var m=New("somnax"); var a=m.S.Active; var e=m.S.fighters.First(x=>!m.S.Ally(x.team,a.team));
   a.pactTargets.Add(e.id); a.renounceTargets.Add(e.id); a.twinBondUsed=true;
   var view=Match.ViewFor(m.S,e.team).F(a.id); Assert.IsEmpty(view.pactTargets); Assert.IsEmpty(view.renounceTargets); Assert.IsFalse(view.twinBondUsed);
   var own=Match.ViewFor(m.S,a.team).F(a.id); Assert.AreEqual(1,own.pactTargets.Count); Assert.IsTrue(own.twinBondUsed);
  }
  [Test] public void BotRaisesTheGuardWhenItProtectsAnAlly(){
   var m=New("tchu-tcha"); var t=m.S.Active; var ally=Ally(m,t); ally.cell=new Cell(3,2); t.pa=0; t.pm=0; // sem PA nem PM: a guarda (0 PA) é a única jogada útil, e o Perry não morre antes
   var e=m.S.fighters.First(x=>!m.S.Ally(x.team,t.team)); e.cell=new Cell(6,2); m.S.fog=false; m.S.revealed=true; t.usedMatch.Add("olho-abissal"); // sem a ultimate, que o bot prefere
   var cmd=Bot.Decide(m);
   if(cmd.kind=="skill"&&Content.AnySkill(cmd.skill)?.kind==SkillKind.GuardaDirecional) { Assert.Pass(); }
   Assert.IsTrue(m.Apply(new Command{kind=cmd.kind,fighter=t.id,skill=cmd.skill,cell=cmd.cell,cell2=cmd.cell2,slot=cmd.slot,revision=m.S.revision},t.team,out _,out var why),why);
   for(int i=0;i<8&&!Match.GuardActive(t)&&m.S.Active==t;i++) { var c=Bot.Decide(m); if(c.kind=="passar") break; c.fighter=t.id; c.revision=m.S.revision; if(!m.Apply(c,t.team,out _,out _)) break; }
   Assert.IsTrue(Match.GuardActive(t),"o bot liga a guarda antes de passar");
  }
 }
}
