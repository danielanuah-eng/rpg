using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace ArenaTatica.Core.Tests
{
    // Lutador grande (autor, 18/10): o Primal Beast ocupa 2×2 casas, uma para cada pé.
    public class BigFighterTests
    {
        static Match Duel(string a,string b) => Match.New(new MatchSetup{ flatMap=true, players=new[]{ new PlayerSetup{fighters=new[]{a}}, new PlayerSetup{fighters=new[]{b}} } });

        [Test] public void PrimalBeastFillsTwoByTwoAndBlocksAllOfThem()
        {
            var m=Duel("colosso","rei-bobett"); var S=m.S; var beast=S.fighters.First(f=>f.def=="colosso"); var foe=S.fighters.First(f=>f!=beast);
            Assert.AreEqual(4,Board.Footprint(beast).Count());
            Assert.IsTrue(Board.Fits(S,beast,beast.cell),"nasce inteiro no mapa");
            Assert.IsTrue(ArenaGame.ValidState(S));
            beast.cell=new Cell(5,5); foe.cell=new Cell(7,6);
            foreach(var c in Board.Footprint(beast)) Assert.AreEqual(beast,Board.At(S,c));
            Assert.AreEqual(1,Board.Gap(beast,foe),"colado pela borda"); Assert.AreEqual(1,Board.Gap(foe,beast));
            Assert.AreEqual(int.MaxValue,Board.Costs(S,foe)[Board.Index(S,new Cell(6,6))],"ninguém entra nas casas dele");
            var reach=Board.Costs(S,beast);
            Assert.AreEqual(int.MaxValue,reach[Board.Index(S,new Cell(S.width-1,5))],"não fica com meio corpo fora do mapa");
            Assert.AreEqual(int.MaxValue,reach[Board.Index(S,new Cell(6,5))],"não pisa em quem está do lado");
            Assert.AreEqual(1,reach[Board.Index(S,new Cell(4,5))]);
            var crown=Content.Def(foe).skills[0];
            Assert.IsTrue(m.InRange(foe,crown,new Cell(6,6),out _),"corpo a corpo em qualquer casa dele");
        }

        [Test] public void StompHitsAroundAllFourCells()
        {
            var m=Duel("colosso","rei-bobett"); var S=m.S; var beast=S.fighters.First(f=>f.def=="colosso"); var foe=S.fighters.First(f=>f!=beast);
            beast.cell=new Cell(5,5); foe.cell=new Cell(7,7);
            S.active=S.timeline.IndexOf(beast.id); beast.pa=6;
            bool ok=m.Apply(new Command{kind="skill",skill="pisao",cell=new Cell(6,6),fighter=beast.id,revision=S.revision},beast.team,out var events,out var reason);
            Assert.IsTrue(ok,reason);
            Assert.IsTrue(events.Any(e=>e.kind=="ataque"&&e.target==foe.id),"a diagonal da casa (6,6) está na área");
        }

        [Test] public void BeastWalksWholeAndKeepsItsFootprintFree()
        {
            var m=Duel("colosso","rei-bobett"); var S=m.S; var beast=S.fighters.First(f=>f.def=="colosso"); var foe=S.fighters.First(f=>f!=beast);
            beast.cell=new Cell(3,3); foe.cell=new Cell(9,9);
            S.active=S.timeline.IndexOf(beast.id); beast.pm=2;
            Assert.IsTrue(m.Apply(new Command{kind="mover",cell=new Cell(5,3),fighter=beast.id,revision=S.revision},beast.team,out _,out var reason),reason);
            Assert.AreEqual(new Cell(5,3),beast.cell); Assert.AreEqual(0,beast.pm);
            Assert.AreEqual(beast,Board.At(S,new Cell(6,4)));
        }

        [Test] public void EditedSheetKeepsTheSize()
        {
            try {
                var d=Content.Copy(Content.Default("colosso")); d.size=0;
                Content.ApplyEdits(JsonUtility.ToJson(new GameEdits{ fighters=new System.Collections.Generic.List<FighterDef>{d} }));
                Assert.AreEqual(2,Content.Fighters["colosso"].size,"ficha salva antes do 2×2 continua grande");
            }
            finally { Content.ApplyEdits(""); }
        }
    }
}
