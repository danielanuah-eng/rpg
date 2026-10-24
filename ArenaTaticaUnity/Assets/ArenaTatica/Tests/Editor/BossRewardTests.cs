using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace ArenaTatica.Core.Tests
{
    // Arena do Rei (Boss.cs, autor): os jogadores competem entre si desde o início e o golpe final de um deles recruta a ficha do
    // chefe como aliado na MESMA partida (BossDef.rewardAlly, Match.TryRecruitBoss), sem criar unidade nova. BossTests (co-op
    // antigo) não é tocado aqui.
    public class BossRewardTests
    {
        const string King="rei-thaagarys";
        const System.Reflection.BindingFlags Private=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
        Match m;
        MatchState S => m.S;

        // 1 herói no assento 1, 2 heróis no assento 2 (PV diferentes) para a média por personagem aparecer.
        static Match New(params string[][] seats) => Match.New(new MatchSetup{ flatMap=true, boss=King,
            players=seats.Select(x=>new PlayerSetup{fighters=x}).ToArray() });

        [SetUp] public void Setup() { m=New(new[]{"ciantia"},new[]{"ciantia","talyssa"}); for(int i=0;i<40;i++) m.Forced.Enqueue(Face.Trava); }
        [TearDown] public void Clean() => Content.ApplyEdits("");

        Fighter Hero(int seat) => S.fighters.First(f=>MatchState.Player(f.team)&&f.team==seat&&f.summoner==0);
        List<Fighter> EnemiesOf(int seat) => S.fighters.Where(f=>f.summoner==0&&MatchState.Player(f.team)&&!S.Ally(f.team,seat)).ToList();
        int Average(int seat) { var foes=EnemiesOf(seat); return Mathf.Max(1,(foes.Sum(f=>Content.MaxHp(f))+foes.Count/2)/foes.Count); }
        Fighter Summon(string def,int team,Cell cell,int owner) =>
            (Fighter)typeof(Match).GetMethod("Spawn",Private).Invoke(m,new object[]{def,team,cell,owner});
        void CheckVictory() => typeof(Match).GetMethod("CheckVictory",Private).Invoke(m,null);

        [Test] public void PlayersCompeteBetweenThemselvesFromTheStart()
        {
            Assert.AreNotEqual(S.alliance[1],S.alliance[2],"sem forçar alliance 1: cada jogador tem time próprio");
            Assert.AreEqual(2,S.Teams().Count);
            Assert.AreEqual(2,EnemiesOf(1).Count,"2 inimigos do assento 1");
            Assert.AreEqual(1,EnemiesOf(2).Count,"1 inimigo do assento 2");
            Assert.AreEqual(King,S.bossFight); Assert.AreEqual(VictoryMode.Eliminacao,S.mode);
        }

        [Test] public void AllyHpIsTheRoundedAverageOfTheEnemyCharactersPerSeat()
        {
            Assert.AreEqual(Average(1),S.bossAllyHp[1],"assento 1: média dos PV dos personagens do assento 2");
            Assert.AreEqual(Average(2),S.bossAllyHp[2],"assento 2: média dos PV dos personagens do assento 1");
            // Média por PERSONAGEM, não a soma do time: fica abaixo do total inimigo.
            Assert.Less(S.bossAllyHp[1],EnemiesOf(1).Sum(f=>Content.MaxHp(f)));
            Assert.AreEqual(EnemiesOf(1)[0].bonusHp+Content.Def(EnemiesOf(1)[0]).hp,Content.MaxHp(EnemiesOf(1)[0]));
            // Invocações e neutros ficam fora da conta (cálculo do próprio SetupBoss, chamado aqui de novo).
            var helper=typeof(Match).GetMethod("BossAllyHp",Private);
            int before=(int)helper.Invoke(m,new object[]{1});
            Summon("guarda-do-trono",2,new Cell(S.width-1,S.height-1),Hero(2).id).bonusHp=99;
            Summon("monstro-esqueleto",MatchState.Neutral,new Cell(0,S.height-1),0).bonusHp=99;
            Assert.AreEqual(before,(int)helper.Invoke(m,new object[]{1}),"invocações e neutros não entram na média");
        }

        [Test] public void TheFinalBlowTurnsTheFallenKingIntoAnAlly()
        {
            var b=m.Boss; var killer=Hero(1);
            Assert.AreEqual(1,S.timeline.Count(x=>x==b.id),"3 heróis: uma entrada a cada 2 turnos");
            S.active=S.timeline.IndexOf(killer.id);
            int fighters=S.fighters.Count, id=b.id; var cell=b.cell;
            b.hp=0; b.deathProcessed=true; b.buffs.Add(new Buff{id="maldicao",from="x",turns=3});
            b.cooldowns.Add(new Charge{skill="lamina-real",uses=2}); b.usedMatch.Add("lamina-real"); b.hidden=true;
            var log=new List<GameEvent>();
            Assert.IsTrue(m.TryRecruitBoss(b,killer,log),"chefe caído + golpe final de jogador = recruta");
            Assert.AreEqual(1,b.team,"passa ao assento de quem deu o golpe final");
            Assert.AreEqual(killer.id,b.summoner); Assert.IsTrue(b.recruitedBoss);
            int hp=S.bossAllyHp[1];
            Assert.AreEqual(hp,b.hp); Assert.AreEqual(hp,Content.MaxHp(b)); Assert.AreEqual(hp-Content.Def(b).hp,b.bonusHp);
            Assert.IsFalse(b.deathProcessed); Assert.AreEqual("espada",b.form); Assert.AreEqual(0,b.formTurns);
            Assert.AreEqual(0,b.pa); Assert.AreEqual(0,b.pm); Assert.IsFalse(b.hidden);
            Assert.AreEqual(0,b.buffs.Count); Assert.AreEqual(0,b.cooldowns.Count);
            Assert.IsTrue(b.usedMatch.Contains("lamina-real"),"usos por partida do dono não são apagados");
            Assert.AreEqual(id,b.id); Assert.AreEqual(cell,b.cell); Assert.AreEqual(fighters,S.fighters.Count,"mesma ficha, sem unidade nova");
            var ev=log.Single(); Assert.AreEqual("chefe-aliado",ev.kind); Assert.AreEqual(killer.id,ev.actor);
            Assert.AreEqual(b.id,ev.target); Assert.AreEqual(hp,ev.value);
            // Deixa de ser chefe (IA neutra e recompensa) e fica com UMA entrada controlável, logo depois do golpe.
            Assert.IsNull(Match.BossOf(b)); Assert.IsFalse(Match.Seated(b));
            Assert.AreEqual(1,S.timeline.Count(x=>x==b.id),"sem duplicatas na linha do tempo");
            Assert.AreEqual(3+1,S.timeline.Count,"heróis + 1 entrada do aliado (turno especial do chefe suprimido)");
            Assert.AreEqual(killer.id,S.timeline[S.active],"ativo atual preservado");
            Assert.AreEqual(b.id,S.timeline[S.active+1],"age como aliado na sequência de quem o recrutou");
        }

        [Test] public void OwnershipComesFromTheSummonerHero()
        {
            var b=m.Boss; var hero=Hero(2); var guard=Summon("guarda-do-trono",2,new Cell(2,S.height-1),hero.id);
            var log=new List<GameEvent>();
            b.hp=0;
            Assert.IsTrue(m.TryRecruitBoss(b,guard,log),"golpe final de uma invocação conta para o dono");
            Assert.AreEqual(2,b.team,"assento do herói dono da invocação");
            Assert.AreEqual(hero.id,b.summoner,"dono é o herói original, não a invocação");
            Assert.AreEqual(guard.id,log.Single().actor,"o evento mostra quem deu o golpe");
            Assert.AreEqual(S.bossAllyHp[2],b.hp);
        }

        [Test] public void NeverRecruitsTwiceAndTheSnapshotKeepsIt()
        {
            var b=m.Boss; var killer=Hero(1); b.hp=0;
            Assert.IsFalse(m.TryRecruitBoss(b,null),"sem golpe não recruta");
            var neutral=Summon("monstro-esqueleto",MatchState.Neutral,new Cell(0,S.height-1),0);
            Assert.IsFalse(m.TryRecruitBoss(b,neutral),"golpe de neutro não recruta");
            Assert.IsFalse(b.recruitedBoss); Assert.AreEqual(0,b.hp);
            Assert.IsTrue(m.TryRecruitBoss(b,killer));
            int hp=b.hp, entries=S.timeline.Count(x=>x==b.id);
            Assert.IsFalse(m.TryRecruitBoss(b,killer),"não recruta 2×");
            Assert.AreEqual(hp,b.hp,"PV do aliado não sobem de novo"); Assert.AreEqual(entries,S.timeline.Count(x=>x==b.id));
            // Estado da rede: o snapshot leva recruitedBoss e os PV por assento.
            var copy=JsonUtility.FromJson<MatchState>(JsonUtility.ToJson(S));
            Assert.IsTrue(copy.F(b.id).recruitedBoss); Assert.IsNull(Match.BossOf(copy.F(b.id)));
            Assert.AreEqual(hp,copy.F(b.id).hp); Assert.AreEqual(S.bossAllyHp[1],copy.bossAllyHp[1]);
        }

        [Test] public void RecruitedAllyDoesNotWinTheMatchByItself()
        {
            var b=m.Boss; b.hp=0; Assert.IsTrue(m.TryRecruitBoss(b,Hero(1)));
            CheckVictory();
            Assert.IsFalse(S.over,"2 times de heróis de pé: o confronto PvP continua");
            // O aliado não é mais chefe: BossVictory devolve false e quem decide é o CheckVictory normal.
            Assert.IsFalse((bool)typeof(Match).GetMethod("BossVictory",Private).Invoke(m,null));
        }

        [Test] public void HostileBossKeepsTheFightButDropsDeadSeats()
        {
            foreach(var h in S.fighters.Where(x=>x.team==2&&x.summoner==0)) h.hp=0;
            CheckVictory();
            Assert.IsTrue(S.eliminated[2],"assento sem heróis sai da partida mesmo com o chefe de pé");
            Assert.IsFalse(S.over); Assert.IsTrue(m.Boss.Alive);
            // Todos os heróis caíram: o neutro vence.
            foreach(var h in S.fighters.Where(x=>x.team==1&&x.summoner==0)) h.hp=0;
            CheckVictory();
            Assert.IsTrue(S.over); Assert.AreEqual(MatchState.Neutral,S.winner);
        }

        [Test] public void TheKingFallingDoesNotHandAnAutomaticWin()
        {
            m.Boss.hp=0;
            CheckVictory();
            Assert.IsFalse(S.over,"chefe no chão com 2 times de heróis: nada de vencedor automático");
        }

        [Test] public void InvalidAllyFormFallsBackToTheStandForm()
        {
            var d=Content.Copy(Content.Default(King)); d.boss.allyForm="nao-existe"; d.boss.standForm="duas-espadas";
            Content.ApplyEdits(JsonUtility.ToJson(new GameEdits{fighters=new List<FighterDef>{d}}));
            m=New(new[]{"ciantia"},new[]{"ciantia","talyssa"});
            var b=m.Boss; b.hp=0;
            Assert.IsTrue(m.TryRecruitBoss(b,Hero(1)));
            Assert.AreEqual("duas-espadas",b.form,"allyForm inválida cai na standForm");
        }
    }
}
