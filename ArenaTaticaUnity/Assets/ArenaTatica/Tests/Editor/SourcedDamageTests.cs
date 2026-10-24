
using System;
using System.Linq;
using System.Reflection;
using ArenaTatica.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArenaTatica.Tests
{
    public class SourcedDamageTests
    {
        Match match;
        MatchState S => match.S;
        Fighter pis, cria;
        void SetupFight()
        {
            match=Match.New(new MatchSetup{flatMap=true,team1=new[]{"daemion"},team2=new[]{"pistoleira","perry"}});
            for(int i=0;i<80;i++) match.Forced.Enqueue(Face.Trava);
            var damon=S.fighters.First(f=>f.def=="daemion"); pis=S.fighters.First(f=>f.def=="pistoleira");
            damon.cell=new Cell(2,2); pis.cell=new Cell(4,2);
            Turn(damon); Assert.IsTrue(Skill(damon,"transformar-em-vampiro",new Cell(3,2)));
            cria=S.fighters.First(f=>f.def=="cria-vampirica"); cria.bonusHp+=10; cria.hp=Content.MaxHp(cria);
        }
        void Turn(Fighter f){S.active=S.timeline.IndexOf(f.id);f.pa=6;f.pm=3;f.used.Clear();}
        bool Skill(Fighter f,string id,Cell cell)=>match.Apply(new Command{kind="skill",fighter=f.id,skill=id,cell=cell,revision=S.revision},f.team,out _,out _);

        [TestCase(true)] [TestCase(false)]
        public void ReturningDamageFromPistoleiraKillsTheAttackingSummon(bool reflect)
        {
            SetupFight();
            if(reflect) pis.buffs.Add(new Buff{id="reflexo-teste",turns=2,reflect=1});
            else pis.thorns=1;
            Turn(cria); Assert.IsTrue(Skill(cria,"garras-da-cria",pis.cell));
            Assert.IsTrue(pis.Alive); Assert.IsFalse(cria.Alive,"reflexo e espinhos também têm a fonte Pistoleira");
        }

        [TestCase(true)] [TestCase(false)]
        public void TrapDamageKeepsItsActualOwnerAcrossPackedTiles(bool pistoleiraOwns)
        {
            SetupFight();
            pis.cell=new Cell(9,9); // sem outro herói travando a saída antes da armadilha
            var source=pistoleiraOwns?pis:S.fighters.First(f=>f.def=="perry");
            source.cell=new Cell(4,3); source.charges.Add(new Charge{skill="armadilha-de-espinhos",uses=1}); Turn(source);
            var at=new Cell(3,3); Assert.IsTrue(Skill(source,"armadilha-de-espinhos",at));
            Assert.AreEqual(source.id,S.At(at).trapBy);
            S.tiles=MatchState.Unpack(MatchState.Pack(S.tiles),S.tiles.Length);
            Assert.AreEqual(source.id,S.At(at).trapBy);
            Turn(cria);
            Assert.IsTrue(match.Apply(new Command{kind="mover",fighter=cria.id,cell=at,revision=S.revision},cria.team,out _,out _));
            Assert.AreEqual(!pistoleiraOwns,cria.Alive,"armadilha do aliado da Pistoleira não recebe insta-kill");
            Assert.AreEqual(0,S.At(at).trapBy,"consumir a armadilha apaga sua fonte");
        }

        [Test] public void DelayedDamageSourceSurvivesSnapshot()
        {
            var fighter=new Fighter{banishBy=17,banished=true,banishDamage=3};
            var copy=JsonUtility.FromJson<Fighter>(JsonUtility.ToJson(fighter));
            Assert.AreEqual(17,copy.banishBy); Assert.AreEqual(3,copy.banishDamage); Assert.IsTrue(copy.banished);
        }

        [Test] public void OldPackedTilesWithoutOwnerRemainReadable()
        {
            var tile=MatchState.Unpack("0.0.0.0.0.2.1",1)[0];
            Assert.AreEqual(2,tile.trap); Assert.AreEqual(1,tile.trapDamage); Assert.AreEqual(0,tile.trapBy);
        }
    }
}
