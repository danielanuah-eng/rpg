using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
namespace ArenaTatica.Core.Tests {
// Revisão 020 (multiplayer da expansão Inventores e Lua): o que o cliente recebe não denuncia quem está escondido.
public class Revisao020Tests {
 static Match New() { var m=Match.New(new MatchSetup{flatMap=true,seed=71,mode=VictoryMode.Eliminacao,players=new[]{new PlayerSetup{fighters=new[]{"nikola"}},new PlayerSetup{fighters=new[]{"ciantia"}}}}); foreach(var t in m.S.tiles){t.terrain=Terrain.Livre;t.coins=0;t.height=0;t.obj=0;t.zone=false;t.lava=false;} m.S.Luna.gears.Clear(); return m; }
 static Fighter Hero(Match m,int team)=>m.S.fighters.First(f=>f.team==team&&f.summoner==0);
 [Test] public void EnemyGearsOutsideVisionAreNotSentInFog(){
  var m=New(); var n=Hero(m,1); var e=Hero(m,2); m.S.fog=true; m.S.revealed=false;
  e.cell=new Cell(1,1); foreach(var f in m.S.fighters.Where(x=>x.team==1)) f.cell=new Cell(m.S.width-2,m.S.height-2);
  int far=Board.Index(m.S,new Cell(m.S.width-3,m.S.height-3)), near=Board.Index(m.S,new Cell(2,1));
  m.S.Luna.gears.Add(new Gear{owner=n.id,cell=far,value=1,hits=3}); m.S.Luna.gears.Add(new Gear{owner=n.id,cell=near,value=1,hits=3});
  Assume.That(Fog.On(m.S)); Assume.That(Fog.Vision(m.S,2)[near]&&!Fog.Vision(m.S,2)[far]);
  var enemyView=Match.ViewFor(m.S,2); CollectionAssert.AreEquivalent(new[]{near},enemyView.Luna.gears.Select(g=>g.cell));
  Assert.AreEqual(2,Match.ViewFor(m.S,1).Luna.gears.Count,"a família do Nikola vê as próprias engrenagens");
 }
 [Test] public void MarkOnHiddenTargetDoesNotRevealItsCell(){
  var m=New(); var n=Hero(m,1); var e=Hero(m,2); e.cell=new Cell(6,6);
  m.S.Luna.marks.Add(new LunaMark{by=n.id,id=e.id,x=6,z=6}); e.hidden=true;
  var mark=Match.ViewFor(m.S,1).Luna.marks.Single(); Assert.AreEqual(e.id,mark.id); Assert.AreEqual(-1,mark.x); Assert.AreEqual(-1,mark.z);
  e.hidden=false; var shown=Match.ViewFor(m.S,1).Luna.marks.Single(); Assert.AreEqual(6,shown.x);
 }
 static object Call(Match m,string method,params object[] args) => typeof(Match).GetMethod(method,System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(m,args);
 [Test] public void BifiaRebuildKeepsTheBossTurns(){
  var m=Match.New(new MatchSetup{flatMap=true,boss="rei-thaagarys",players=new[]{new PlayerSetup{fighters=new[]{"bifia"}},new PlayerSetup{fighters=new[]{"ciantia"}}}});
  var boss=m.Boss; Assume.That(boss!=null);
  int before=m.S.timeline.Count(id=>id==boss.id); Assume.That(before>0);
  Call(m,"RebuildTimelineKeepingSummons");
  Assert.AreEqual(before,m.S.timeline.Count(id=>id==boss.id),"o chefe continua com as mesmas entradas na ordem");
  Assert.IsTrue(m.S.fighters.Where(f=>f.Alive&&f.summoner==0&&MatchState.Player(f.team)).All(f=>m.S.timeline.Contains(f.id)));
 }
 // Autor (23/10): o radar do Celinos revela só para o time dele.
 [Test] public void CelinosRadarRevealsOnlyToItsTeam(){
  var m=Match.New(new MatchSetup{flatMap=true,seed=71,mode=VictoryMode.Eliminacao,players=new[]{new PlayerSetup{fighters=new[]{"nikola"}},new PlayerSetup{fighters=new[]{"ciantia"}},new PlayerSetup{fighters=new[]{"ciantia"}}}});
  var r=m.S.fighters.First(x=>x.def=="celinos"); var hid=Hero(m,2); r.cell=new Cell(1,4); hid.cell=new Cell(4,4); hid.hidden=true;
  Call(m,"RevealCelinos");
  Assert.IsTrue(hid.hidden,"continua escondido no estado autoritativo");
  Assert.IsFalse(Match.Concealed(m.S,1,hid)); Assert.IsTrue(Match.Concealed(m.S,3,hid));
  Assert.IsFalse(Match.ViewFor(m.S,1).F(hid.id).hidden);
  var third=Match.ViewFor(m.S,3); Assert.IsTrue(third.F(hid.id).hidden); Assert.AreEqual(Cell.Unknown,third.F(hid.id).cell); Assert.IsEmpty(third.Luna.radar);
  var ev=new List<GameEvent>{new GameEvent{kind="revelar",info="radar",actor=r.id,target=hid.id,cell=hid.cell}};
  Assert.IsEmpty(Match.EventsFor(ev,m.S,3)); Assert.IsNotEmpty(Match.EventsFor(ev,m.S,1));
  hid.cell=new Cell(m.S.width-2,m.S.height-2); Call(m,"RevealCelinos"); Assert.IsFalse(Match.OnRadar(m.S,1,hid),"saiu do raio, sai do radar");
 }
 // Autor (23/10, 2ª decisão): o Roque vale para quaisquer duas unidades aliadas, a qualquer distância do Desland.
 [Test] public void RoqueWorksOnAlliesAnywhere(){
  var m=Match.New(new MatchSetup{flatMap=true,seed=71,mode=VictoryMode.Eliminacao,players=new[]{new PlayerSetup{fighters=new[]{"desland","ciantia","talyssa"}},new PlayerSetup{fighters=new[]{"ciantia"}}}});
  foreach(var t in m.S.tiles){t.terrain=Terrain.Livre;t.obj=0;t.height=0;t.lava=false;}
  var d=m.S.fighters.First(x=>x.def=="desland"); var a=m.S.fighters.First(x=>x.team==1&&x!=d&&x.summoner==0); var b=m.S.fighters.Last(x=>x.team==1&&x!=d&&x.summoner==0);
  foreach(var x in m.S.fighters) x.cell=new Cell(0,m.S.fighters.IndexOf(x)); int w=m.S.width;
  d.cell=new Cell(0,w-1); a.cell=new Cell(w-8,2); b.cell=new Cell(w-3,2);
  var sk=Content.Def(d).skills.First(s=>s.id=="roque"); object[] args={d,sk,a.cell,b.cell,null};
  var check=typeof(Match).GetMethod("RoqueCheck",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
  Assume.That(Board.Gap(d,a)>sk.maxRange);
  Assert.IsTrue((bool)check.Invoke(m,args),(string)args[4]);
 }
 // Autor (23/10): no domínio, mover um inimigo só uma vez por ultimate; aliados e peças, à vontade.
 [Test] public void ChessDomainMovesAnEnemyOnlyOncePerUltimate(){
  var m=Match.New(new MatchSetup{flatMap=true,seed=71,mode=VictoryMode.Eliminacao,players=new[]{new PlayerSetup{fighters=new[]{"desland"}},new PlayerSetup{fighters=new[]{"ciantia"}}}});
  foreach(var t in m.S.tiles){t.terrain=Terrain.Livre;t.obj=0;t.height=0;t.lava=false;t.coins=0;}
  var d=Hero(m,1); var e=Hero(m,2); d.cell=new Cell(4,4); e.cell=new Cell(9,9); d.pa=20;
  Call(m,"OpenChess",d); Assume.That(d.form=="dominio");
  bool Move(Cell from,Cell to,out string why){ m.S.active=m.S.timeline.IndexOf(d.id); return m.Apply(new Command{kind="skill",fighter=d.id,skill="mover-peao",cell=from,cell2=to,revision=m.S.revision},d.team,out _,out why); }
  Assert.IsTrue(Move(e.cell,new Cell(9,10),out var w1),w1);
  Assert.IsFalse(Move(e.cell,new Cell(9,11),out _),"segundo inimigo no mesmo domínio");
  var piece=m.S.fighters.First(x=>x.Alive&&x.summoner==d.id&&x.def=="peca-peao"); var to=new[]{new Cell(1,0),new Cell(-1,0),new Cell(0,1),new Cell(0,-1)}.Select(o=>piece.cell+o).First(c=>Board.Contains(m.S,c)&&Board.At(m.S,c)==null);
  Assert.IsTrue(Move(piece.cell,to,out var w2),w2);
 }
 [Test] public void VivaLunaEndsWhenBifiaFalls(){
  var m=Match.New(new MatchSetup{flatMap=true,seed=71,mode=VictoryMode.Eliminacao,players=new[]{new PlayerSetup{fighters=new[]{"bifia","ciantia"}},new PlayerSetup{fighters=new[]{"ciantia"}}}});
  var b=Hero(m,1); var ally=m.S.fighters.First(f=>f.team==1&&f.summoner==0&&f!=b);
  ally.initBonus=1; m.S.Luna.fields.Add(new LunaField{by=b.id,turns=2,init=1,affected={ally.id}});
  b.hp=0; Call(m,"KO",b,null);
  Assert.IsEmpty(m.S.Luna.fields); Assert.AreEqual(0,ally.initBonus);
 }
}
}
