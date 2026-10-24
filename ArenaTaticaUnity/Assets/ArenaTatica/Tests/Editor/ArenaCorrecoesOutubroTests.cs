using System.Linq;
using NUnit.Framework;
using UnityEngine;
namespace ArenaTatica.Core.Tests {
 public class ArenaCorrecoesOutubroTests {
  static Match Duel(){var m=Match.New(new MatchSetup{flatMap=true,players=new[]{new PlayerSetup{fighters=new[]{"colosso"}},new PlayerSetup{fighters=new[]{"rei-bobett"}}}});var beast=m.S.fighters.First(f=>f.def=="colosso");beast.cell=new Cell(4,4);beast.pm=5;m.S.fighters.First(f=>f!=beast).cell=new Cell(10,10);m.S.active=m.S.timeline.IndexOf(beast.id);return m;}
  [TestCase(1,0)] [TestCase(0,1)] [TestCase(1,1)]
  public void BeastCanMoveToAnchorInsideItsCurrentBody(int x,int z){var m=Duel();var f=m.S.Active;var to=f.cell+new Cell(x,z);Assert.AreSame(f,Board.At(m.S,to));Assert.IsFalse(ArenaPresentation.InspectOnMoveTap(f,f,to,true),"clique deve mover, não abrir ficha");Assert.IsTrue(m.Apply(new Command{kind="mover",fighter=f.id,cell=to,revision=m.S.revision},f.team,out _,out var why),why);Assert.AreEqual(to,f.cell);}
  [Test] public void LargeActiveBodyUsesGroundOnlyForMovement(){var f=Duel().S.Active;Assert.IsTrue(ArenaPresentation.GroundPickForMove(f,f,true,""));Assert.IsFalse(ArenaPresentation.GroundPickForMove(f,f,true,"atacar"));Assert.IsFalse(ArenaPresentation.GroundPickForMove(f,f,false,""));}
  [Test] public void OtherBodyKeepsUnitSelection(){var m=Duel();var other=m.S.fighters.First(f=>f!=m.S.Active);Assert.IsFalse(ArenaPresentation.GroundPickForMove(m.S.Active,other,true,""));}
  [Test] public void SmallActiveBodyKeepsUnitSelection(){var m=Duel();var f=m.S.fighters.First(f=>f!=m.S.Active);Assert.IsFalse(ArenaPresentation.GroundPickForMove(f,f,true,""));}
  [Test] public void BeastAnchorStillOpensItsOwnInfo(){var f=Duel().S.Active;Assert.IsTrue(ArenaPresentation.InspectOnMoveTap(f,f,f.cell,true));}
  [Test] public void OtherFighterStillOpensInfo(){var m=Duel();var other=m.S.fighters.First(f=>f!=m.S.Active);Assert.IsTrue(ArenaPresentation.InspectOnMoveTap(m.S.Active,other,other.cell,true));}
  [Test] public void SpectatorCanInspectAnyBodyCell(){var f=Duel().S.Active;Assert.IsTrue(ArenaPresentation.InspectOnMoveTap(f,f,f.cell+new Cell(1,0),false));}
  [Test] public void DisplayNamesMatchAuthorRequest(){Assert.AreEqual("Pele D. Ark",Content.Default("pele-negra").name);Assert.AreEqual("Mirhor",Content.Default("espelhos").name);}
  [TestCase("pele-negra","Pele D. N","Pele D. Ark")]
  [TestCase("pele-negra","Pele D. Negra","Pele D. Ark")]
  [TestCase("pele-negra","Pele de Negra","Pele D. Ark")]
  [TestCase("pele-negra","Pele Negra","Pele D. Ark")]
  [TestCase("espelhos","Reflectius","Mirhor")]
  [TestCase("espelhos","Homem dos Espelhos","Mirhor")]
  [TestCase("espelhos","Homem ods espelhos","Mirhor")]
  [TestCase("pele-negra","Meu nome próprio","Meu nome próprio")]
  [TestCase("espelhos","Nome personalizado","Nome personalizado")]
  public void SavedEditsMigrateOnlyLegacyNames(string id,string oldName,string expected){string previous=Content.EditsJson;try{var d=Content.Copy(Content.Default(id));d.name=oldName;d.hp=17;var e=new GameEdits();e.fighters.Add(d);Content.ApplyEdits(JsonUtility.ToJson(e));Assert.AreEqual(expected,Content.Fighters[id].name);Assert.AreEqual(17,Content.Fighters[id].hp);}finally{Content.ApplyEdits(previous);}}
 }
}
