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
                int gates=s.portals.Count(p=>p.by==0);
                Assert.GreaterOrEqual(gates,4,"pares de portais pelo mapa: "+where); Assert.AreEqual(0,gates%2,"portais em pares: "+where);
            }
        }
        // Labirinto (autor, 18/10 noite): mapa todo labirinto com corredores de 2 casas, área ×2 por jogador, salas de início
        // fechadas por portas em lados opostos, monstros de farm e espetos escondidos.
        [Test] public void LabyrinthFillsTheMapWithWideCorridorsRoomsAndMonsters()
        {
            foreach(int players in new[]{2,3,4,6}) for(uint seed=1;seed<=4;seed++)
            {
                var team=new PlayerSetup{fighters=new[]{"colosso","ciantia"}};
                var m=Match.New(new MatchSetup{ seed=seed, theme=MapGen.OpenThemes+(int)(seed%3), players=Enumerable.Range(0,players).Select(_=>team).ToArray() });
                var s=m.S; string where=$"{players}j seed {seed}";
                Assert.AreEqual(Board.LabyrinthSize(players),s.width,where);
                // Todo chão e porta é alcançável pelo Primal Beast (2×2) abrindo portas.
                var beast=s.fighters.First(f=>f.def=="colosso"); var dist=Board.Costs(s,beast,true,true);
                var cover=new bool[s.tiles.Length];
                for(int i=0;i<s.tiles.Length;i++) if(dist[i]!=int.MaxValue) { var c=new Cell(i%s.width,i/s.width); foreach(var p in Board.Footprint(beast,c)) if(Board.Contains(s,p)) cover[Board.Index(s,p)]=true; }
                for(int i=0;i<s.tiles.Length;i++) if(s.tiles[i].terrain==Terrain.Livre) Assert.IsTrue(cover[i],$"o Primal Beast chega em ({i%s.width},{i/s.width}): "+where);
                // Salas de início: cada slot na sua, o par em lados opostos (qual lado é sorteado), fechadas por portas.
                for(int slot=0;slot<players;slot++) {
                    var r=MapGen.Room(s,slot); Assert.IsNotNull(r,"sala do slot "+slot+": "+where);
                    bool bottom=r[1]<s.height/2, firstBottom=MapGen.Room(s,0)[1]<s.height/2; Assert.AreEqual(slot%2==0?firstBottom:!firstBottom,bottom,"lado da sala "+slot+": "+where);
                    for(int x=r[0]-1;x<=r[2]+1;x++) for(int z=r[1]-1;z<=r[3]+1;z++) {
                        if(x>=r[0]&&x<=r[2]&&z>=r[1]&&z<=r[3]) continue; var t=s.At(new Cell(x,z)).terrain;
                        Assert.IsTrue(t==Terrain.Parede||t==Terrain.Porta,"sala fechada por parede e portas: "+where);
                    }
                }
                foreach(var f in s.fighters.Where(f=>MatchState.Player(f.team)))
                    Assert.IsTrue(Enumerable.Range(0,players).Select(sl=>MapGen.Room(s,sl)).Any(r=>Board.Footprint(f).All(c=>c.x>=r[0]&&c.x<=r[2]&&c.z>=r[1]&&c.z<=r[3])),"herói começa na sala: "+where);
                Assert.GreaterOrEqual(s.fighters.Count(Match.Monster),4,"monstros de farm: "+where);
                Assert.IsTrue(s.fighters.Where(Match.Monster).All(f=>f.team==MatchState.Neutral),where);
                // Espetos poucos e escondidos de todos até alguém pisar.
                int spikes=s.tiles.Count(t=>t.Obj==TileObj.Espetos);
                Assert.Greater(spikes,0,where); Assert.LessOrEqual(spikes,s.width*s.height/100,"poucos espetos: "+where);
                Assert.AreEqual(0,Match.ViewFor(s,1).tiles.Count(t=>t.Obj==TileObj.Espetos),"espetos ocultos: "+where);
                // O estado de 6 jogadores cabe na mensagem da rede.
                var json=ArenaGame.Wire(new WireMessage{kind="snapshot",state=Match.ViewFor(s,1)});
                Assert.LessOrEqual(System.Text.Encoding.UTF8.GetByteCount(json),65536,"cabe no WebSocket: "+where);
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
            var view=Match.ViewFor(m.S,2); var packet=ArenaGame.Wire(new WireMessage{kind="snapshot",state=view});
            Assert.LessOrEqual(System.Text.Encoding.UTF8.GetByteCount(packet),budget); Assert.IsTrue(ArenaGame.ValidState(view,2));
            // Do outro lado: as casas voltam iguais.
            var back=JsonUtility.FromJson<WireMessage>(packet).state; var tiles=MatchState.Unpack(back.packed,back.width*back.height);
            Assert.AreEqual(JsonUtility.ToJson(new MapDef{tiles=view.tiles}),JsonUtility.ToJson(new MapDef{tiles=tiles}));
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
        [Test] public void LabyrinthSpawnRoomsAreShuffledAndSavedMazesIgnored()
        {
            var firsts=new System.Collections.Generic.HashSet<int>();
            for(uint seed=1;seed<=12;seed++) { var r=MapGen.Room(Closed(MapGen.OpenThemes,2,seed).S,0); Assert.IsNotNull(r); firsts.Add(r[1]); }
            Assert.AreEqual(2,firsts.Count(),"o time 1 nasce às vezes embaixo, às vezes em cima");
            int size=Board.SizeFor(2); var saved=new MapDef{ width=size, theme=MapGen.OpenThemes, tiles=new Tile[size*size] };
            for(int i=0;i<saved.tiles.Length;i++) saved.tiles[i]=new Tile();
            var s=Match.New(new MatchSetup{ seed=5, map=saved, players=new[]{ new PlayerSetup{fighters=new[]{"talyssa"}}, new PlayerSetup{fighters=new[]{"talyssa"}} } }).S;
            Assert.AreEqual(Board.LabyrinthSize(2),s.width,"labirinto salvo vira procedural"); Assert.IsTrue(s.tiles.Any(t=>t.terrain==Terrain.Parede));
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
        // Autor (18/10 noite): salto e teleporte por cima do muro custam +2 PA; sem muro no caminho, custo e alcance normais.
        [Test] public void JumpOrTeleportOverWallCostsTwoMore()
        {
            var tp=Content.Fighters["teantha"].skills.First(x=>x.kind==SkillKind.Teleporte); var a=A;
            Clear(); a.cell=new Cell(3,3); Enemy.cell=new Cell(11,11);
            int full=m.Range(a,tp,a.cell), cost=Match.Cost(a,tp,S);
            for(int z=0;z<12;z++) S.At(new Cell(5,z)).terrain=Terrain.Parede;
            Assert.AreEqual(full,m.Range(a,tp,a.cell),"o alcance não cai mais pela metade");
            Assert.AreEqual(cost,Match.Cost(a,tp,S,new Cell(3,5),null),"sem muro no caminho");
            Assert.AreEqual(cost+Match.WallJump,Match.Cost(a,tp,S,new Cell(6,3),null),"por cima do muro");
            a.pa=cost+1; Assert.IsFalse(m.CanTarget(a,tp,new Cell(6,3),out _),"sem PA para o muro");
            a.pa=cost+Match.WallJump; Assert.IsTrue(m.CanTarget(a,tp,new Cell(6,3),out string why),"passa por cima pagando: "+why);
        }
        // Autor (18/10 noite): dano à distância nunca acerta atrás do muro, nem a skill que ignora a visão, nem a explosão.
        [Test] public void RangedDamageCannotTargetBehindWalls()
        {
            Clear(); var a=A; a.cell=new Cell(3,3); Enemy.cell=new Cell(3,6);
            S.At(new Cell(3,5)).terrain=Terrain.Parede;
            var blind=new SkillDef{ id="teste-sem-visao", name="t", kind=SkillKind.Dano, pa=1, minRange=1, maxRange=6, damage=2, sight=false };
            Assert.IsFalse(m.InRange(a,blind,Enemy.cell,out string why)); Assert.AreEqual("Atrás do muro.",why);
            S.At(new Cell(3,5)).terrain=Terrain.Livre;
            Assert.IsTrue(m.InRange(a,blind,Enemy.cell,out _));
        }
        // Névoa (autor, 18/10 noite): só as 8 casas em volta; memória de paredes/chão; andar no escuro para no obstáculo
        // escondido; espetos ocultos saem ao pisar; monstro de farm dá moedas.
        [Test] public void FogShowsOnlyAdjacentRemembersAndStopsBlindWalks()
        {
            Clear(); var a=A; var e=Enemy; a.cell=new Cell(2,2); e.cell=new Cell(2,4); S.fog=true; a.pm=6;
            Assert.IsFalse(Match.Sees(S,a.team,e),"a 2 casas já some"); e.cell=new Cell(3,3); Assert.IsTrue(Match.Sees(S,a.team,e),"diagonal colada aparece"); e.cell=new Cell(11,11);
            var box=new Cell(2,6); S.At(box).terrain=Terrain.Caixa; S.At(new Cell(8,8)).Obj=TileObj.Bau; Fog.Explore(S);
            var view=Match.ViewFor(S,a.team);
            Assert.AreEqual(Terrain.Livre,view.At(box).terrain,"caixa longe não aparece"); Assert.AreEqual(TileObj.Nada,view.At(new Cell(8,8)).Obj,"baú longe não aparece");
            Assert.IsTrue(ArenaGame.ValidState(view,a.team));
            // Manda andar até (2,8): o caminho conhecido passa pela caixa escondida e para colado nela, sem evento dela.
            Assert.IsTrue(Move(2,8));
            Assert.AreEqual(new Cell(2,5),a.cell,"parou no limite do obstáculo");
            Assert.AreEqual(Terrain.Caixa,Match.ViewFor(S,a.team).At(box).terrain,"agora colada, aparece");
            // Memória: depois de sair, a caixa continua lembrada (estrutura).
            a.cell=new Cell(9,2); Fog.Explore(S);
            Assert.IsTrue(Fog.Explored(S,a.team,Board.Index(S,box))); Assert.AreEqual(Terrain.Caixa,Match.ViewFor(S,a.team).At(box).terrain);
            // Vela acesa: o mapa todo para todos.
            var candle=new Cell(9,3); S.At(candle).terrain=Terrain.Caixa; S.At(candle).Obj=TileObj.Vela;
            Assert.IsTrue(Act("acender",candle)); Assert.AreEqual(TileObj.Bau,Match.ViewFor(S,e.team).At(new Cell(8,8)).Obj);
        }
        // Sala (autor, 19/10): quem está numa sala vê a sala inteira, com paredes e portas em volta, e quem estiver nela.
        [Test] public void FogShowsTheWholeRoomAroundTheFighter()
        {
            Clear(); var a=A; var e=Enemy; a.cell=new Cell(2,2); e.cell=new Cell(5,5); S.fog=true; S.areas=new[]{1,1,5,5};
            var door=new Cell(3,6); S.At(door).terrain=Terrain.Porta; var far=new Cell(3,7); S.At(far).terrain=Terrain.Caixa; Fog.Explore(S);
            Assert.IsTrue(Match.Sees(S,a.team,e),"inimigo na mesma sala aparece");
            var view=Match.ViewFor(S,a.team);
            Assert.AreEqual(Terrain.Porta,view.At(door).terrain,"porta da sala aparece"); Assert.AreEqual(Terrain.Livre,view.At(far).terrain,"fora da sala não");
            a.cell=new Cell(8,8); Assert.IsFalse(Match.Sees(S,a.team,e),"fora da sala só as coladas");
        }
        // Labirinto de verdade: o herói vê a própria sala de início (com as portas) e sai dela abrindo uma porta.
        [Test] public void HeroSeesStartRoomDoorsAndWalksOut()
        {
            foreach(uint seed in new uint[]{1,2,3,4,5,6}) {
                var team=new PlayerSetup{fighters=new[]{"ciantia"}};
                var m=Match.New(new MatchSetup{ seed=seed, theme=MapGen.OpenThemes, players=new[]{team,team} });
                var s=m.S; var f=s.Active; string where="seed "+seed;
                var mine=Enumerable.Range(0,s.players).Select(sl=>MapGen.Room(s,sl)).First(r=>f.cell.x>=r[0]&&f.cell.x<=r[2]&&f.cell.z>=r[1]&&f.cell.z<=r[3]);
                Assert.Greater(s.areas.Length,8,"salas guardadas: "+where);
                var view=Match.ViewFor(s,f.team);
                var doors=Enumerable.Range(0,s.tiles.Length).Select(i=>new Cell(i%s.width,i/s.width)).Where(c=>s.At(c).terrain==Terrain.Porta&&c.x>=mine[0]-1&&c.x<=mine[2]+1&&c.z>=mine[1]-1&&c.z<=mine[3]+1).ToList();
                Assert.Greater(doors.Count,0,"sala com porta: "+where);
                Assert.IsTrue(doors.All(d=>view.At(d).terrain==Terrain.Porta),"portas da sala à vista: "+where);
                // Anda até colar numa porta, abre e passa.
                for(int turn=0;turn<6&&!doors.Any(d=>Board.Gap(f,d)==1);turn++) {
                    f.pm=3; var costs=Board.Costs(Match.ViewFor(s,f.team),f); var door=doors.OrderBy(d=>d.Distance(f.cell)).First();
                    var to=Enumerable.Range(0,s.tiles.Length).Where(i=>costs[i]>0&&costs[i]<=f.pm).Select(i=>new Cell(i%s.width,i/s.width)).OrderBy(c=>c.Distance(door)).First();
                    Assert.IsTrue(m.Apply(new Command{kind="mover",fighter=f.id,cell=to,revision=s.revision},f.team,out _,out var why),why);
                }
                var next=doors.First(d=>Board.Gap(f,d)==1);
                f.pm=System.Math.Max(f.pm,1);
                Assert.IsTrue(m.Apply(new Command{kind="abrir",fighter=f.id,cell=next,revision=s.revision},f.team,out _,out var reason),reason+" "+where);
                Assert.AreEqual(Terrain.Livre,s.At(next).terrain,where);
            }
        }
        [Test] public void HiddenSpikesRiseWhenSteppedAndMonsterPaysGold()
        {
            Clear(); var a=A; a.cell=new Cell(3,3); Enemy.cell=new Cell(10,10); a.pm=3; S.fog=true; S.revealed=true;
            var spike=new Cell(3,4); S.At(spike).Obj=TileObj.Espetos;
            Assert.AreEqual(TileObj.Nada,Match.ViewFor(S,a.team).At(spike).Obj,"escondidos até pisar");
            Assert.IsTrue(m.Apply(new Command{kind="mover",cell=new Cell(3,5),fighter=a.id,revision=S.revision},a.team,out var events,out _));
            Assert.IsTrue(events.Any(x=>x.kind=="espetos"&&x.cell==spike),"saem do chão na casa pisada");
            Assert.AreEqual(TileObj.Espetos,Match.ViewFor(S,Enemy.team).At(spike).Obj,"depois ficam à vista");
            // Monstro de farm: quem derruba ganha MonsterGold.
            var spawn=typeof(Match).GetMethod("Spawn",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
            var mob=(Fighter)spawn.Invoke(m,new object[]{"monstro-esqueleto",MatchState.Neutral,new Cell(3,6),0}); mob.hp=1;
            int coins=S.coins[a.team];
            var hit=Match.Skills(a).First(x=>x.kind==SkillKind.Dano&&x.damage>0&&x.minRange<=1&&!x.selfArea);
            for(int i=0;i<6&&mob.Alive;i++) { a.pa=9; a.used.Clear(); a.cooldowns.Clear(); m.Forced.Enqueue(Face.Trava); m.Forced.Enqueue(Face.Trava); m.Apply(new Command{kind="skill",skill=hit.id,cell=mob.cell,fighter=a.id,revision=S.revision},a.team,out _,out _); }
            Assert.IsFalse(mob.Alive,"derrubou"); Assert.AreEqual(coins+Match.MonsterGold,S.coins[a.team]);
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
