using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace ArenaTatica.Core.Tests
{
    // Regra do autor: cada chefe automático ataca um JOGADOR (assento = Fighter.team) por vez, num ciclo sorteado e guardado no
    // MatchState (Boss.cs/BossCycle). Os testes passam pelo fluxo real (m.Apply "passar" + eventos emitidos), não só pela função
    // auxiliar: o assento é lido do alvo dos eventos de golpe.
    public class BossTargetCycleTests
    {
        const string King="rei-thaagarys";
        Match m;
        MatchState S => m.S;
        Fighter Boss => m.Boss;
        List<Fighter> Heroes => S.fighters.Where(f=>f.Alive&&MatchState.Player(f.team)&&f.summoner==0).ToList();
        BossCycle Cycle => S.bossCycles.First(c=>c.boss==Boss.id);

        // Um herói (ciantia) por jogador, salvo quando o teste passa a lista de fichas.
        static Match New(int players,uint seed=1,params string[] team) => Match.New(new MatchSetup{ flatMap=true, boss=King, seed=seed,
            players=Enumerable.Range(0,players).Select(_=>new PlayerSetup{fighters=team.Length>0?team:new[]{"ciantia"}}).ToArray() });
        // Dados forçados: Trava no ataque (nada de Esquiva) e sempre dá para repetir os turnos.
        void Fill() { for(int i=0;i<600;i++) m.Forced.Enqueue(Face.Trava); }
        Match Fresh(int players,uint seed=1,params string[] team) { m=New(players,seed,team); Fill(); return m; }
        [SetUp] public void Setup() { m=New(2); Fill(); }
        [TearDown] public void Clean() => Content.ApplyEdits("");

        // Só essa skill no pool (peso 10); o movimento fica com o peso dado.
        static void Only(string skill,int move=0,bool wild=false,int actions2=-1)
        {
            var d=Content.Copy(Content.Default(King));
            foreach(var s in d.skills) { s.weight=s.id==skill?10:0; if(s.id==skill) s.wild=wild; }
            d.boss.moveWeight=move; if(actions2>=0) d.boss.actions2=actions2;
            Content.ApplyEdits(JsonUtility.ToJson(new GameEdits{fighters=new List<FighterDef>{d}}));
        }
        // Casas livres e andáveis coladas (gap 1) no corpo de um chefe 2×2 posto em at.
        List<Cell> Ring(Cell at)
        {
            var list=new List<Cell>();
            for(int x=at.x-1;x<=at.x+2;x++) for(int z=at.z-1;z<=at.z+2;z++)
            {
                var c=new Cell(x,z);
                if(!Board.Contains(S,c)||Board.At(S,c)!=null||!Board.Walkable(S,c,false)) continue;
                if(x>=at.x&&x<=at.x+1&&z>=at.z&&z<=at.z+1) continue;
                if(Board.Gap(Boss,c)!=1) continue;
                list.Add(c);
            }
            return list;
        }
        // De pé no primeiro canto livre onde cabem [near] heróis colados nele; PV bem alto para aguentar a bateria de golpes.
        Fighter StandNear(int near)
        {
            var b=Boss; b.form="espada";
            for(int z=1;z<S.height-3;z++) for(int x=1;x<S.width-3;x++)
            {
                var at=new Cell(x,z); b.cell=at;
                if(!Board.Fits(S,b,at)) continue;
                var ring=Ring(at); if(ring.Count<near) continue;
                var heroes=Heroes.Take(near).ToList();
                for(int i=0;i<heroes.Count;i++) { heroes[i].hp=999; heroes[i].cell=ring[i]; }
                return b;
            }
            Assert.Fail("mapa plano sem espaço para o teste"); return b;
        }
        // Passa o turno do herói logo antes de uma entrada do chefe: o chefe joga e a vez volta a um herói.
        List<GameEvent> BossTurn()
        {
            int bossSlot=S.timeline.IndexOf(Boss.id),count=S.timeline.Count;
            S.active=Enumerable.Range(1,count).Select(k=>(bossSlot-k+count)%count)
                .First(i=>S.F(S.timeline[i]) is Fighter h&&h.Alive&&h.prisoned==0&&MatchState.Player(h.team));
            var hero=S.Active;
            Assert.IsTrue(m.Apply(new Command{kind="passar",fighter=hero.id,revision=S.revision},hero.team,out var events,out var why),why);
            return events;
        }
        // Assentos (Fighter.team) atingidos por golpes do chefe, na ordem dos eventos: 'ataque' com alvo e 'troca-vida'.
        List<int> HitSeats(List<GameEvent> ev) => ev.Where(e=>e.actor==Boss.id&&e.target>0&&(e.kind=="ataque"||e.kind=="troca-vida"))
            .Select(e=>S.F(e.target)).Where(v=>v!=null&&MatchState.Player(v.team)).Select(v=>v.team).ToList();
        // Ordem dos assentos atacados até juntar [n] golpes.
        List<int> AttackOrder(int n,int limit=80)
        {
            var seats=new List<int>();
            for(int i=0;i<limit&&seats.Count<n;i++) seats.AddRange(HitSeats(BossTurn()));
            return seats;
        }

        [Test] public void ThreePlayersWalkTheWholeCycleBeforeRepeatingOne()
        {
            Only("lamina-real"); Fresh(3); StandNear(3);
            var seats=AttackOrder(6);
            Assert.AreEqual(6,seats.Count,"um golpe por turno do chefe");
            CollectionAssert.AreEquivalent(new[]{1,2,3},seats.Take(3).ToList(),"1º ciclo atende os três jogadores");
            CollectionAssert.AreEquivalent(new[]{1,2,3},seats.Skip(3).ToList(),"2º ciclo atende os três jogadores");
            CollectionAssert.AreNotEqual(seats.Take(3).ToList(),seats.Skip(3).ToList(),"a permutação nova não repete a anterior");
        }

        [Test] public void FourAndSixPlayersAlsoWalkEveryPlayerBeforeRepeating()
        {
            foreach(int players in new[]{4,6})
            {
                Only("lamina-real"); Fresh(players); StandNear(players);
                var seats=AttackOrder(players*2);
                Assert.AreEqual(players*2,seats.Count,$"{players} jogadores: dois ciclos completos");
                CollectionAssert.AreEquivalent(Enumerable.Range(1,players).ToList(),seats.Take(players).ToList(),$"{players} jogadores: 1º ciclo");
                CollectionAssert.AreEquivalent(Enumerable.Range(1,players).ToList(),seats.Skip(players).ToList(),$"{players} jogadores: 2º ciclo");
                CollectionAssert.AreNotEqual(seats.Take(players).ToList(),seats.Skip(players).ToList(),$"{players} jogadores: ordem nova no 2º ciclo");
            }
        }

        // Mesma seed = mesma ordem (Dice.Range); seeds diferentes variam e não focam sempre o jogador mais perto.
        List<int> TwoCyclesOfThree(uint seed)
        {
            Only("lamina-real"); Fresh(3,seed); StandNear(3);
            return AttackOrder(6);
        }
        [Test] public void SameSeedKeepsTheDrawAndOtherSeedsChangeIt()
        {
            CollectionAssert.AreEqual(TwoCyclesOfThree(1),TwoCyclesOfThree(1),"mesma seed reproduz a ordem");
            var orders=new List<string>(); var firsts=new List<int>();
            foreach(uint seed in new uint[]{2,3,5,7,11,13,17,19})
            {
                var seats=TwoCyclesOfThree(seed);
                orders.Add(string.Join(",",seats)); firsts.Add(seats[0]);
            }
            Assert.Greater(orders.Distinct().Count(),1,"seeds diferentes variam a ordem do ciclo");
            Assert.Greater(firsts.Distinct().Count(),1,"não foca sempre o mesmo assento (o mais perto)");
        }

        [Test] public void TwoHeroesOfTheSamePlayerSpendTheSeatOnlyOnce()
        {
            Only("lamina-real"); Fresh(2,1,"ciantia","talyssa"); StandNear(4);
            var seats=AttackOrder(2);
            CollectionAssert.AreEquivalent(new[]{1,2},seats.Take(2).ToList(),"os dois heróis do mesmo jogador contam uma vez");
        }

        [Test] public void TwoAlliedPlayersAreStillTwoSeatsForTheNeutralBoss()
        {
            Only("lamina-real");
            m=Match.New(new MatchSetup{flatMap=true,boss=King,players=new[]{
                new PlayerSetup{fighters=new[]{"ciantia"},team=1}, new PlayerSetup{fighters=new[]{"ciantia"},team=1}}});
            Fill(); StandNear(2);
            Assert.IsTrue(S.Ally(1,2),"os dois assentos são aliados entre si");
            var seats=AttackOrder(2);
            CollectionAssert.AreEquivalent(new[]{1,2},seats.Take(2).ToList(),"dois aliados continuam dois assentos para o chefe neutro");
        }

        [Test] public void PhaseTwoUsesItsTwoActionsOnDifferentPlayers()
        {
            Only("lamina-real"); Fresh(3); var b=StandNear(3);
            b.form="duas-espadas"; b.hp=Content.MaxHp(b)/2;
            var seats=HitSeats(BossTurn());
            Assert.AreEqual(2,seats.Count,"fase 2: duas ações no mesmo turno");
            Assert.AreNotEqual(seats[0],seats[1],"as duas ações do turno alternam de jogador");
        }

        [Test] public void MovingSummoningAndFailingToReachDoNotSpendTheTurn()
        {
            // Só movimento: o chefe anda (o assento 2 está longe) e ninguém é marcado.
            Only("none",5); Fresh(3); StandNear(3);
            Heroes.First(h=>h.team==2).cell=new Cell(1,S.height-2);
            var walk=BossTurn();
            Assert.IsFalse(walk.Any(e=>e.kind=="ataque"&&e.actor==Boss.id),"andar não ataca");
            Assert.AreEqual(3,Cycle.pending.Count,"andar não gasta a vez de ninguém");
            // Só invocação: chama a guarda e o ciclo continua intacto.
            Only("chamado-da-guarda"); Fresh(3); StandNear(1);
            BossTurn();
            Assert.IsTrue(S.fighters.Any(f=>f.def=="guarda-do-trono"),"invocou a guarda");
            Assert.AreEqual(3,Cycle.pending.Count,"invocar não gasta a vez de ninguém");
            // Golpe que não alcança ninguém (heróis longe, de pé): ninguém é marcado.
            Only("lamina-real"); Fresh(3); Boss.form="espada";
            var far=BossTurn();
            Assert.IsFalse(far.Any(e=>e.kind=="ataque"&&e.actor==Boss.id&&e.target>0),"longe demais para acertar");
            Assert.AreEqual(3,Cycle.pending.Count,"falha de alcance não gasta a vez de ninguém");
        }

        [Test] public void BlockedAndMissedHitsStillSpendTheTurn()
        {
            // Escudo no dado de defesa: o golpe entra bloqueado e o jogador conta.
            Only("lamina-real"); Fresh(3); StandNear(3);
            m.Forced.Clear(); m.Forced.Enqueue(Face.Trava); m.Forced.Enqueue(Face.Escudo); Fill();
            var seated=AttackOrder(1);
            Assert.AreEqual(1,seated.Count,"bloqueio conta como ataque");
            Assert.IsFalse(Cycle.pending.Contains(seated[0]),"o assento atacado saiu dos pendentes");
            // Esquiva no dado de ataque de um golpe impreciso: erra, mas a tentativa contra ele conta.
            Only("lamina-real",0,true); Fresh(3); StandNear(3);
            m.Forced.Clear(); m.Forced.Enqueue(Face.Esquiva); Fill();
            var ev=BossTurn();
            Assert.IsTrue(ev.Any(e=>e.kind=="ataque"&&e.actor==Boss.id&&e.value==-1),"o golpe errou");
            var missed=HitSeats(ev);
            Assert.AreEqual(1,missed.Count,"erro conta como tentativa");
            Assert.IsFalse(Cycle.pending.Contains(missed[0]),"o assento do erro saiu dos pendentes");
        }

        [Test] public void HiddenOrPrisonedPlayerDoesNotBlockAndGoesFirstWhenBack()
        {
            Only("lamina-real"); Fresh(3); StandNear(3);
            AttackOrder(1);
            int hiddenSeat=Cycle.pending[0];
            var benched=Heroes.First(h=>h.team==hiddenSeat); benched.hidden=true;
            AttackOrder(1); // Fecha a parte visível do ciclo que já estava em andamento.
            var seats=AttackOrder(4);
            Assert.IsFalse(seats.Contains(hiddenSeat),"oculto não é alvo");
            Assert.AreEqual(2,seats.Take(2).Distinct().Count(),"os outros dois completam seu ciclo");
            Assert.AreEqual(2,seats.Skip(2).Take(2).Distinct().Count(),"o novo ciclo também alterna os elegíveis");
            Assert.IsTrue(Cycle.pending.Contains(hiddenSeat),"quem está oculto continua esperando a vez");
            benched.hidden=false;
            var back=AttackOrder(1);
            Assert.AreEqual(hiddenSeat,back[0],"ao reaparecer entra antes de repetir os demais");
            // Preso também não trava os elegíveis.
            Only("lamina-real"); Fresh(3); StandNear(3);
            Heroes.First(h=>h.team==3).prisoned=99;
            var jailed=AttackOrder(2);
            CollectionAssert.AreEquivalent(new[]{1,2},jailed.Take(2).ToList(),"preso não trava os elegíveis");
        }

        [Test] public void EliminatedPlayerLeavesTheCycle()
        {
            Only("lamina-real"); Fresh(3); StandNear(3);
            Heroes.First(h=>h.team==3).hp=0;
            var seats=AttackOrder(2);
            CollectionAssert.AreEquivalent(new[]{1,2},seats.Take(2).ToList(),"o assento eliminado fica fora do ciclo");
            Assert.IsFalse(Cycle.pending.Contains(3),"eliminado sai dos pendentes");
        }

        [Test] public void AnAreaAttackCountsEveryPlayerItHitsOnce()
        {
            Only("vortice-real",0,false,1); Fresh(3); var b=StandNear(3); b.form="duas-espadas";
            var ev=BossTurn();
            Assert.IsTrue(ev.Any(e=>e.kind=="area"&&e.actor==b.id),"golpe em área em volta de si");
            Assert.AreEqual(0,Cycle.pending.Count,"a área atingiu os três de uma vez, sem repetir ninguém");
            CollectionAssert.AreEquivalent(new[]{1,2,3},Cycle.order,"os três assentos do ciclo");
        }

        [Test] public void AnAreaThatWouldRepeatAPlayerIsSkippedWhileOthersWait()
        {
            Only("vortice-real",0,false,1); Fresh(3); var b=StandNear(3); b.form="duas-espadas";
            // O assento 3 está longe; 1 e 2 estão na área, mas o 2 já foi atendido neste ciclo.
            Heroes.First(h=>h.team==3).cell=new Cell(1,S.height-2);
            S.bossCycles.Add(new BossCycle{boss=b.id,order=new List<int>{1,2,3},pending=new List<int>{1,3}});
            var ev=BossTurn();
            Assert.IsFalse(ev.Any(e=>e.kind=="area"&&e.actor==b.id),"não gasta a área em quem já foi atendido");
            Assert.IsFalse(ev.Any(e=>e.kind=="ataque"&&e.actor==b.id&&e.target>0),"ninguém é atacado agora");
            Assert.IsTrue(Cycle.pending.Contains(1)&&Cycle.pending.Contains(3),"os pendentes continuam esperando");
        }

        [Test] public void StateRoundTripKeepsTheCycle()
        {
            Only("lamina-real"); Fresh(3); StandNear(3);
            AttackOrder(2);
            var order=Cycle.order.ToList(); var pending=Cycle.pending.ToList();
            Assert.AreEqual(3,order.Count);
            var copy=JsonUtility.FromJson<MatchState>(JsonUtility.ToJson(S));
            var saved=copy.bossCycles.First(c=>c.boss==Boss.id);
            CollectionAssert.AreEqual(order,saved.order,"a ordem sorteada vai no save/reconnect");
            CollectionAssert.AreEqual(pending,saved.pending,"os pendentes vão no save/reconnect");
            Assert.IsEmpty(Match.ViewFor(S,1).bossCycles,"a ordem futura não é enviada aos clientes");
            CollectionAssert.AreEqual(pending,Cycle.pending,"a vista não altera os pendentes do Host");
        }
    }
}
