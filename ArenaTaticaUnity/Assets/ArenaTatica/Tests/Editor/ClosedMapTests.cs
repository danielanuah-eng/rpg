using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace ArenaTatica.Core.Tests
{
    // Mapas fechados e objetos fixos trazidos do DungeonBDX (18/10).
    public class ClosedMapTests
    {
        static Match Closed(int theme,int players,uint seed)
        {
            var team=new PlayerSetup{fighters=new[]{"ciantia"}};
            return Match.New(new MatchSetup{ seed=seed, theme=theme, players=Enumerable.Range(0,players).Select(_=>team).ToArray() });
        }

        [Test] public void ClosedMapsAreFairWalledAndPlayable()
        {
            foreach(int players in new[]{2,4,6}) for(int theme=MapGen.OpenThemes;theme<MapGen.Themes.Length;theme++) for(uint seed=1;seed<=12;seed++)
            {
                var s=Closed(theme,players,seed).S; string where=$"{MapGen.Themes[theme]} {s.width}×{s.width} seed {seed}";
                Assert.Greater(s.tiles.Count(t=>t.terrain==Terrain.Parede),s.tiles.Length/12,"tem paredes: "+where);
                Assert.LessOrEqual(MapGen.Unfairness(s),2,"lados quase justos: "+where);
                var dist=Board.Costs(s,s.fighters[0],true,true);
                Assert.IsTrue(s.fighters.All(f=>dist[Board.Index(s,f.cell)]!=int.MaxValue),"todos se alcançam (abrindo portas): "+where);
                foreach(var f in s.fighters) Assert.AreEqual(Terrain.Livre,s.At(f.cell).terrain,"início livre: "+where);
                Assert.AreEqual(2,s.portals.Count(p=>p.by==0),"um par de portais: "+where);
            }
        }
        [Test] public void OpenThemesDoNotGetWalls()
        {
            for(int theme=0;theme<MapGen.OpenThemes;theme++) Assert.IsFalse(Closed(theme,2,5).S.tiles.Any(t=>t.terrain==Terrain.Parede||t.terrain==Terrain.Porta||t.Obj==TileObj.Vela));
        }
        // Mapas abertos (autor, 18/10): estradas de tijolo livres de obstáculos, desenhos no chão, vilas e barris.
        [Test] public void OpenMapsGetRoadsDrawingsAndVillages()
        {
            int roads=0, drawings=0, villages=0, barrels=0;
            int budget=(int)typeof(ArenaTransport).GetField("RelayPayload",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).GetValue(null);
            foreach(int players in new[]{2,4,6}) for(int theme=0;theme<MapGen.OpenThemes;theme++) for(uint seed=1;seed<=8;seed++)
            {
                var m=Closed(theme,players,seed); var s=m.S; string where=$"{MapGen.Themes[theme]} {s.width}×{s.width} seed {seed}";
                foreach(var d in s.decor.Where(MapGen.IsRoad)) { roads++; foreach(var c in MapGen.PatchCells(s,d)) Assert.AreEqual(Terrain.Livre,s.At(c).terrain,"estrada sem obstáculo: "+where); }
                drawings+=s.decor.Count(d=>d.piece=="piso_mosaic");
                villages+=s.decor.Count(d=>d.piece=="dg_fountain"||d.piece=="enfeite_placa");
                barrels+=s.decor.Count(d=>d.piece=="dg_barrels");
                var packet=JsonUtility.ToJson(new WireMessage{kind="snapshot",state=Match.ViewFor(s,0)});
                Assert.LessOrEqual(System.Text.Encoding.UTF8.GetByteCount(packet),budget,"cabe no Relay: "+where);
            }
            Assert.Greater(roads,0); Assert.Greater(drawings,0); Assert.Greater(villages,0); Assert.Greater(barrels,0);
        }
        [Test] public void ClosedSixPlayerSnapshotFitsRelayBudget()
        {
            var m=Closed(MapGen.OpenThemes,6,8);
            int budget=(int)typeof(ArenaTransport).GetField("RelayPayload",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).GetValue(null);
            var view=Match.ViewFor(m.S,2); var packet=JsonUtility.ToJson(new WireMessage{kind="snapshot",state=view});
            Assert.LessOrEqual(System.Text.Encoding.UTF8.GetByteCount(packet),budget); Assert.IsTrue(ArenaGame.ValidState(view,2));
        }

        // ── Regras dos objetos, num mapa plano ──
        Match m; MatchState S => m.S; Fighter A => S.Active; Fighter Enemy => S.fighters.First(f=>f.team!=A.team);
        [SetUp] public void Setup() { m=Match.New(new MatchSetup{ flatMap=true }); }
        bool Move(int x,int z) => m.Apply(new Command{kind="mover",cell=new Cell(x,z),fighter=A.id,revision=S.revision},A.team,out _,out _);
        void Clear() { foreach(var f in S.fighters) f.cell=new Cell(0,0); }

        [Test] public void WallBlocksWalkingFlyingAndSight()
        {
            S.At(new Cell(5,5)).terrain=Terrain.Parede;
            Assert.IsFalse(Board.Walkable(S,new Cell(5,5),true));
            Assert.IsFalse(Board.LineOfSight(S,new Cell(5,3),new Cell(5,7)));
            Assert.IsTrue(Board.LineOfSight(S,new Cell(4,3),new Cell(4,7)));
        }
        [Test] public void ChestGivesCoinsOnceAndSpikesHurtWithoutStopping()
        {
            Clear(); var a=A; a.cell=new Cell(3,3); Enemy.cell=new Cell(10,10); a.pm=4;
            S.At(new Cell(3,4)).Obj=TileObj.Espetos; S.At(new Cell(3,5)).Obj=TileObj.Bau;
            int hp=a.hp, coins=S.coins[a.team];
            Assert.IsTrue(Move(3,6));
            Assert.AreEqual(new Cell(3,6),a.cell,"os espetos não param");
            Assert.AreEqual(hp-1,a.hp);
            Assert.AreEqual(coins+MapGen.BauMoedas,S.coins[a.team]);
            Assert.AreEqual(TileObj.Nada,S.At(new Cell(3,5)).Obj,"o baú abre uma vez só");
        }
        [Test] public void BrazierGivesActionPointAtTurnStart()
        {
            Clear(); var a=A; var e=Enemy; e.cell=new Cell(6,6); a.cell=new Cell(2,2);
            var t=S.At(new Cell(6,7)); t.terrain=Terrain.Caixa; t.Obj=TileObj.Braseiro;
            int before=e.pa;
            m.Apply(new Command{kind="passar",fighter=a.id,revision=S.revision},a.team,out _,out _);
            Assert.AreEqual(e,S.Active);
            Assert.AreEqual(Content.Def(e).pa+1,e.pa);
        }
        [Test] public void LabyrinthIsOneSizeAboveAndHasDoorsAndOneCandle()
        {
            var s=Closed(MapGen.OpenThemes,2,3).S;
            Assert.AreEqual(Board.LabyrinthSize(2),s.width); Assert.IsTrue(s.fog);
            Assert.AreEqual(1,s.tiles.Count(t=>t.Obj==TileObj.Vela));
            Assert.Greater(s.tiles.Count(t=>t.terrain==Terrain.Porta),0);
            Assert.AreEqual(Board.SizeFor(2),Closed(0,2,3).S.width,"aberto continua do tamanho comum");
        }
        bool Act(string kind,Cell c) => m.Apply(new Command{kind=kind,cell=c,fighter=A.id,revision=S.revision},A.team,out _,out _);
        [Test] public void DoorBlocksUntilOpenedForOnePm()
        {
            Clear(); var a=A; a.cell=new Cell(3,3); Enemy.cell=new Cell(3,6); a.pm=3;
            S.At(new Cell(3,4)).terrain=Terrain.Porta;
            Assert.IsFalse(Board.LineOfSight(S,a.cell,Enemy.cell)); Assert.IsFalse(Board.Walkable(S,new Cell(3,4),true));
            Assert.IsFalse(Act("abrir",new Cell(5,5)),"longe não abre");
            Assert.IsTrue(Act("abrir",new Cell(3,4)));
            Assert.AreEqual(2,a.pm); Assert.AreEqual(Terrain.Livre,S.At(new Cell(3,4)).terrain); Assert.AreEqual(TileObj.PortaAberta,S.At(new Cell(3,4)).Obj);
        }
        [Test] public void FogHidesEnemiesUntilCandleIsLit()
        {
            Clear(); var a=A; var e=Enemy; a.cell=new Cell(2,2); e.cell=new Cell(2,8); S.fog=true;
            for(int x=0;x<12;x++) S.At(new Cell(x,5)).terrain=Terrain.Parede;
            Assert.IsFalse(Match.Sees(S,a.team,e));
            Assert.AreEqual(Cell.Unknown,Match.ViewFor(S,a.team).F(e.id).cell,"rede não manda a posição");
            Assert.IsTrue(ArenaGame.ValidState(Match.ViewFor(S,a.team),a.team));
            var candle=new Cell(2,3); S.At(candle).terrain=Terrain.Caixa; S.At(candle).Obj=TileObj.Vela;
            int coins=S.coins[a.team];
            Assert.IsTrue(Act("acender",candle));
            Assert.IsTrue(S.revealed); Assert.AreEqual(coins+Match.VelaMoedas,S.coins[a.team]);
            Assert.IsTrue(Match.Sees(S,a.team,e)); Assert.IsFalse(Act("acender",candle),"acende uma vez só");
        }
        [Test] public void WalledMapHalvesJumpAndTeleportAndCostsOneMore()
        {
            var tp=Content.Fighters["teantha"].skills.First(x=>x.kind==SkillKind.Teleporte); var a=A;
            int full=m.Range(a,tp,a.cell); int cost=Match.Cost(a,tp,S);
            S.At(new Cell(11,11)).terrain=Terrain.Parede;
            Assert.AreEqual(full/2,m.Range(a,tp,a.cell)); Assert.AreEqual(cost+1,Match.Cost(a,tp,S));
        }
        [Test] public void BossOnlyInLabyrinthWalksOpensDoorsAndHits()
        {
            Clear(); var a=A; a.cell=new Cell(2,2); Enemy.cell=new Cell(11,11); S.fog=true;
            m.Trigger("chefe");
            var boss=S.fighters.Single(f=>f.Alive&&Content.Bosses.Contains(f.def)); Assert.IsTrue(S.boss);
            // Corredor fechado por uma porta entre o boss e o alvo.
            for(int x=0;x<12;x++) for(int z=0;z<12;z++) S.At(new Cell(x,z)).terrain=x==2?Terrain.Livre:Terrain.Parede;
            boss.cell=new Cell(2,5); S.At(new Cell(2,4)).terrain=Terrain.Porta; Enemy.cell=new Cell(2,11);

            typeof(Match).GetMethod("Neutrals",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(m,null);
            Assert.AreEqual(Terrain.Livre,S.At(new Cell(2,4)).terrain,"abriu a porta");
            Assert.AreEqual(1,boss.cell.Distance(a.cell),"chegou colado para bater");
            var open=Match.New(new MatchSetup{ flatMap=true });
            for(int i=0;i<30;i++) open.Trigger(null);
            Assert.IsFalse(open.S.fighters.Any(f=>Content.Bosses.Contains(f.def)),"não sai em mapa aberto");
        }
        [Test] public void EditedSheetsLeaveSixPlayerStateUnderRelayBudget()
        {
            try {
                var edits=new GameEdits(); foreach(var d in Content.Fighters.Values) { var c=Content.Copy(d); c.hp=Mathf.Min(40,c.hp+1); edits.fighters.Add(c); }
                Content.ApplyEdits(JsonUtility.ToJson(edits));
                var team=new PlayerSetup{fighters=new[]{"ciantia","teantha","damian"}};
                var big=Match.New(new MatchSetup{ seed=4, players=Enumerable.Range(0,6).Select(_=>team).ToArray() });
                int budget=(int)typeof(ArenaTransport).GetField("RelayPayload",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).GetValue(null);
                var view=Match.ViewFor(big.S,2); string sheets=view.edits; view.edits=null;
                Assert.LessOrEqual(System.Text.Encoding.UTF8.GetByteCount(JsonUtility.ToJson(new WireMessage{kind="snapshot",state=view})),budget,"estado sem as fichas");
                var parts=ArenaGame.EditParts(sheets).ToList(); Assert.AreEqual(sheets,string.Concat(parts.Select(p=>p.edits)),"as partes remontam as fichas");
                foreach(var p in parts) Assert.LessOrEqual(System.Text.Encoding.UTF8.GetByteCount(JsonUtility.ToJson(p)),budget,"cada parte cabe");
            } finally { Content.ApplyEdits(""); }
        }
        [Test] public void FixedPortalLeadsToMirrorCell()
        {
            Clear(); var a=A; a.cell=new Cell(3,3); Enemy.cell=new Cell(11,0); a.pm=3;
            var gate=new Cell(3,4); S.At(gate).Obj=TileObj.Portal; S.At(S.Mirror(gate)).Obj=TileObj.Portal; MapGen.Portals(S);
            Assert.IsTrue(Move(3,4));
            Assert.AreEqual(1,a.cell.Distance(S.Mirror(gate)),"sai ao lado da casa espelhada");
            Assert.AreEqual(2,S.portals.Count);
        }
    }
}
