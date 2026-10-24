using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using ArenaTatica.Core;

namespace ArenaTatica.Tests
{
    public class MatchModifierTests
    {
        MatchSetup Setup(uint seed,bool skills,bool stats,bool bots,bool players) => new MatchSetup{seed=seed,flatMap=true,
            randomSkills=skills,randomStats=stats,randomBots=bots,randomPlayers=players,botSeats=new[]{2},
            team1=new[]{"ciantia","perry"},team2=new[]{"colosso","talyssa"}};
        [TestCase(true,false)] [TestCase(false,true)] [TestCase(true,true)] [TestCase(false,false)]
        public void ScopeSelectsOnlyIntendedParticipants(bool bots,bool players)
        {
            var m=Match.New(Setup(7,true,true,bots,players));
            foreach(var f in m.S.fighters) Assert.AreEqual(f.team==2?bots:players,f.roll!=null);
        }
        [TestCase(true,false)] [TestCase(false,true)] [TestCase(true,true)]
        public void SameSeedProducesSameRollAndLeavesEditorUnchanged(bool skills,bool stats)
        {
            var original=JsonUtility.ToJson(Content.Fighters["ciantia"]); var setup=Setup(987,skills,stats,true,true);
            var a=Match.New(setup); var b=Match.New(setup);
            Assert.AreEqual(JsonUtility.ToJson(a.S),JsonUtility.ToJson(b.S));
            Assert.AreEqual(original,JsonUtility.ToJson(Content.Fighters["ciantia"]));
            Assert.IsTrue(ArenaGame.ValidState(a.S));
            var roundtrip=JsonUtility.FromJson<MatchState>(JsonUtility.ToJson(a.S));
            foreach(var f in a.S.fighters) CollectionAssert.AreEqual(Match.Skills(f).Select(s=>s.id),Match.Skills(roundtrip.F(f.id)).Select(s=>s.id));
        }
        [Test] public void NumericRollsStayBoundedAndSkillsRemainUsableAcrossManySeeds()
        {
            for(uint seed=1;seed<=80;seed++) {
                var m=Match.New(Setup(seed,true,true,true,true));
                foreach(var f in m.S.fighters) {
                    var d=Content.Def(f); Assert.That(d.hp,Is.InRange(4,30)); Assert.That(d.pa,Is.InRange(5,8)); Assert.That(d.pm,Is.InRange(1,6)); Assert.That(d.initiative,Is.InRange(0,12));
                    Assert.AreEqual(d.hp,f.hp); Assert.IsTrue(d.skills.Any(s=>s.kind==SkillKind.Dano&&s.damage>0&&s.pa<=d.pa));
                    Assert.AreEqual(d.skills.Length,d.skills.Select(s=>s.id).Distinct().Count());
                    foreach(var sk in d.skills) { Assert.That(sk.damage,Is.InRange(0,8)); Assert.That(sk.maxRange,Is.InRange(sk.minRange,8)); Assert.IsNotNull(Content.Skill(f,sk.id)); }
                }
            }
        }
        [Test] public void RandomizedSixPlayerSnapshotFitsRelayBudget()
        {
            var team=ArenaGame.ValidTeam(Content.Roster.OrderBy(id=>Content.Fighters[id].level));
            var m=Match.New(new MatchSetup{seed=8,randomSkills=true,randomStats=true,randomPlayers=true,
                players=Enumerable.Range(0,6).Select(_=>new PlayerSetup{fighters=team}).ToArray()});
            int budget=(int)typeof(ArenaTransport).GetField("RelayPayload",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).GetValue(null);
            var view=Match.ViewFor(m.S,2); var packet=ArenaGame.Zip(ArenaGame.Wire(new WireMessage{kind="snapshot",state=view}));
            Assert.LessOrEqual(System.Text.Encoding.UTF8.GetByteCount(packet),budget); Assert.IsTrue(ArenaGame.ValidState(view,2));
        }
        [Test] public void RandomizedCloneHasValidDisguisedSnapshot()
        {
            var m=Match.New(new MatchSetup{flatMap=true,randomStats=true,randomPlayers=true,team1=new[]{"ezequiela"},team2=new[]{"ciantia"}});
            Assert.IsTrue(ArenaGame.ValidState(Match.ViewFor(m.S,2),2));
        }
        [Test] public void StatsOnlyKeepEachEditedFightersOwnSkillCostAndProperties()
        {
            var saved=Content.EditsJson;
            try {
                var a=Content.Fighters["ciantia"].Clone(); var b=Content.Fighters["perry"].Clone();
                var first=Content.Generic.First(x=>x.skill.id=="bola-de-fogo").skill.Clone(); first.pa=1; first.area=2;
                var second=first.Clone(); second.pa=5; second.area=1;
                a.skills=new[]{first}; b.skills=new[]{second};
                Content.ApplyEdits(JsonUtility.ToJson(new GameEdits{fighters=new System.Collections.Generic.List<FighterDef>{a,b}}));
                var m=Match.New(Setup(1,false,true,true,true));
                Assert.AreEqual(1,Content.Def(m.S.fighters.First(f=>f.def==a.id)).skills[0].pa);
                Assert.AreEqual(5,Content.Def(m.S.fighters.First(f=>f.def==b.id)).skills[0].pa);
                Assert.AreEqual(1,Content.Def(m.S.fighters.First(f=>f.def==b.id)).skills[0].area);
            } finally { Content.ApplyEdits(saved); }
        }
        [Test] public void ColossusAbsorbsRolledDamageAndRangeWithoutSharingMutableCache()
        {
            var m=Match.New(Setup(1,false,true,true,true)); var victim=m.S.fighters.First(f=>f.def=="talyssa"); var col=m.S.fighters.First(f=>f.def=="colosso");
            var power=victim.roll.powers.First(p=>p.id=="lamina-rosa"); power.damage=7; power.range=6;
            victim.hp=0; typeof(Match).GetMethod("KO",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(m,new object[]{victim,null});
            var copied=Content.Skill(col,Content.Stolen+power.id); Assert.AreEqual(7,copied.damage); Assert.AreEqual(6,copied.maxRange);
            var state=JsonUtility.FromJson<MatchState>(JsonUtility.ToJson(m.S)); Assert.AreEqual(7,Content.Skill(state.F(col.id),copied.id).damage);
            Assert.AreNotEqual(7,Content.StolenSkill(power.id).damage,"cache global permanece com a ficha base");
        }
    }
}
