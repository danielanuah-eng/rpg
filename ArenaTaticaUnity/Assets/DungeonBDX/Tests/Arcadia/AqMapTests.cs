using System.Linq;
using DungeonBDX.Arcadia;
using NUnit.Framework;

namespace DungeonBDX.Tests
{
    // Reserved in the shared 2026-10-16 Arcadia foundation note before creation.
    public class AqMapTests
    {
        static string Topology(AqScenario s)=>string.Join(";",s.Board.Edges
            .Select(e=>e.A+"/"+e.B+"/"+e.Door+"/"+e.Open).OrderBy(x=>x));

        [Test] public void AdventuresHaveDifferentWallsAndRoomCompositions()
        {
            var maps=AqScenarios.Ids.Select(AqScenarios.Create).ToArray();
            Assert.That(maps.Select(Topology).Distinct().Count(),Is.EqualTo(3));
            Assert.That(maps.Select(s=>string.Join(";",s.Board.Spaces.Select(p=>p.Room+"/"+p.Surface))).Distinct().Count(),Is.EqualTo(3));
            Assert.That(maps[1].Board.Spaces.Count(p=>p.Room=="Praça central"),Is.EqualTo(9));
            Assert.That(maps[2].Board.Spaces.Count(p=>p.Room=="Câmara do Ritual"),Is.EqualTo(9));
        }

        [TestCase("portais-em-ruinas")][TestCase("praca-dos-quatro-emblemas")][TestCase("resgate-nas-catacumbas")]
        public void EveryGuildCanReachEveryObjectiveWhenDoorsAreOpen(string id)
        {
            var s=AqScenarios.Create(id);
            foreach(var edge in s.Board.Edges.Where(e=>e.Door))edge.Open=true;
            Assert.That(s.Starts.Length,Is.EqualTo(4));
            Assert.That(s.Starts.SelectMany(x=>x).Distinct().Count(),Is.EqualTo(8));
            foreach(var start in s.Starts)
            {
                Assert.That(start.Length,Is.EqualTo(2));
                Assert.That(s.Board.Passage(start[0],start[1]),Is.True,"The two guild spawn spaces must connect.");
                foreach(var space in s.Board.Spaces)
                    Assert.That(s.Board.Path(start[0],space.Cell).Count,Is.GreaterThan(0),id+" "+start[0]+" → "+space.Cell);
                foreach(var token in s.Tokens)
                    Assert.That(s.Board.Path(start[0],token.Cell).Count,Is.GreaterThan(0),token.Id);
            }
        }

        [TestCase("portais-em-ruinas")][TestCase("praca-dos-quatro-emblemas")][TestCase("resgate-nas-catacumbas")]
        public void MarkersAreValidAndPortalsAlwaysHaveADestination(string id)
        {
            var s=AqScenarios.Create(id);
            Assert.That(s.Tokens.Select(t=>t.Id).Distinct().Count(),Is.EqualTo(s.Tokens.Count));
            Assert.That(s.Tokens.All(t=>s.Board.Contains(t.Cell)),Is.True);
            Assert.That(s.Monsters.All(m=>s.Board.Contains(m.Cell)),Is.True);
            Assert.That(s.Monsters.All(m=>!s.Starts.SelectMany(c=>c).Contains(m.Cell)),Is.True);
            var entries=s.Tokens.Where(t=>t.Kind=="spawn").ToArray();
            Assert.That(entries.Length,Is.EqualTo(4));
            Assert.That(entries.Select(t=>t.Cell).Distinct().Count(),Is.EqualTo(4));
            Assert.That(entries.Select(t=>t.Color),Is.EquivalentTo(new[]{"melee-melee","melee-ranged","ranged-ranged","melee-critical"}));
            foreach(var portals in s.Tokens.Where(t=>t.Kind=="portal").GroupBy(t=>t.Color))
            {
                Assert.That(portals.Count(),Is.GreaterThanOrEqualTo(2),portals.Key);
                Assert.That(portals.Select(t=>t.Cell).Distinct().Count(),Is.EqualTo(portals.Count()));
            }
        }

        [TestCase("portais-em-ruinas")][TestCase("praca-dos-quatro-emblemas")][TestCase("resgate-nas-catacumbas")]
        public void FourGuildsIncludingColossoStartWithoutOverfillingSpaces(string id)
        {
            var parties=new[]{new[]{"colosso","alvyr","talyssa"},new[]{"damian","ciantia","diantus"},
                new[]{"teantha","devon","ezequiela"},new[]{"brumak","raizor","nixira"}};
            var session=new AqSession(id,parties);
            foreach(var space in session.Board.Spaces)
                Assert.That(session.Occupants(space.Cell).Sum(u=>u.Occupancy),Is.LessThanOrEqualTo(2),space.Cell.ToString());
        }

        [Test] public void AdventureTextAndObjectiveMarkersAgree()
        {
            var portal=AqScenarios.Create(AqScenarios.Ids[0]);
            Assert.That(portal.Quests[1],Does.Contain("relíquia"));
            Assert.That(portal.Tokens.Count(t=>t.Kind=="clue"),Is.EqualTo(4));
            Assert.That(portal.Tokens.Count(t=>t.Kind=="relic"),Is.EqualTo(4));
            var plaza=AqScenarios.Create(AqScenarios.Ids[1]);
            Assert.That(plaza.Tokens.Count(t=>t.Kind=="emblem"),Is.EqualTo(4));
            Assert.That(plaza.Monsters.Count(m=>m.HeroId=="brutamontes-errante"),Is.EqualTo(1));
            Assert.That(plaza.Tokens.Any(t=>t.Kind=="clue"||t.Kind=="relic"),Is.False);
            var crypt=AqScenarios.Create(AqScenarios.Ids[2]);
            Assert.That(crypt.Tokens.Count(t=>t.Kind=="captive"),Is.EqualTo(2));
            Assert.That(crypt.Tokens.Count(t=>t.Kind=="ritual"),Is.EqualTo(4));
            Assert.That(crypt.Tokens.Count(t=>t.Kind=="hazard"),Is.EqualTo(2));
        }

        [Test] public void RosterHasTheFourteenRpgHeroesAndJackIsNoLongerAnEnemy()
        {
            Assert.That(AqContent.Playable.Length,Is.EqualTo(14));
            Assert.That(AqContent.Playable.Distinct().Count(),Is.EqualTo(14));
            Assert.That(AqContent.Playable.All(AqContent.Heroes.ContainsKey));
            Assert.That(AqContent.Hero("colosso").Name,Is.EqualTo("Primal Beast"));
            foreach(var id in AqScenarios.Ids)
                Assert.That(AqScenarios.Create(id).Monsters.Any(m=>AqContent.Playable.Contains(m.HeroId)),Is.False,id);
        }

        [Test] public void EveryChapterFieldsFourMeleeAndFourRangedMinionKinds()
        {
            foreach(var id in AqScenarios.Ids)
            {
                // Spawn rows: three minor minions and one major minion per side; wild pets come on top.
                var minions=AqScenarios.Create(id).Monsters.Where(m=>(m.Tier=="minion"||m.Tier=="major")&&m.Profile!="pet").ToArray();
                var melee=minions.Where(m=>!m.Ranged).Select(m=>m.HeroId).Distinct().ToArray();var ranged=minions.Where(m=>m.Ranged).Select(m=>m.HeroId).Distinct().ToArray();
                Assert.That(melee,Has.Length.EqualTo(4),id);Assert.That(ranged,Has.Length.EqualTo(4),id);
                // Monster dice are looked up by id, so one id never plays both profiles in a chapter.
                Assert.That(melee.Intersect(ranged),Is.Empty,id);
            }
        }

        [Test] public void ChaptersClimbTheArcadiaLevelsUpToADragon()
        {
            foreach(var id in AqScenarios.Ids)
            {
                var s=AqScenarios.Create(id);
                foreach(var tier in new[]{"minion","major","villain","boss"})Assert.That(s.Monsters.Any(m=>m.Tier==tier),Is.True,id+" "+tier);
                int Coins(string tier)=>s.Monsters.Where(m=>m.Tier==tier).Max(m=>m.Reward);
                // Arcadia coins: minor minion 1, major minion 2, villains 2-3, major villain 3.
                Assert.That(Coins("minion"),Is.LessThan(s.Monsters.Where(m=>m.Tier=="major").Min(m=>m.Reward)),id);
                Assert.That(s.Monsters.Where(m=>m.Tier=="villain").Min(m=>m.Reward),Is.GreaterThanOrEqualTo(2),id);
                Assert.That(s.Monsters.Where(m=>m.Tier=="boss").Min(m=>m.Reward),Is.EqualTo(3),id);
                Assert.That(s.Monsters.Where(m=>m.Tier=="villain"||m.Tier=="boss").Select(m=>m.HeroId).Intersect(AqContent.Playable),Is.Empty,id);
            }
            var crypt=AqScenarios.Create(AqScenarios.Ids[2]);var dragon=crypt.Monsters.Single(m=>m.Tier=="dragon");
            Assert.That(dragon.Huge,Is.True);Assert.That(dragon.Health,Is.GreaterThan(crypt.Monsters.Where(m=>m.Tier!="dragon").Max(m=>m.Health)));
            Assert.That(dragon.Reward,Is.EqualTo(4));
            // A huge figure fills its space: nobody else starts there.
            Assert.That(crypt.Monsters.Count(m=>m.Cell.Equals(dragon.Cell)),Is.EqualTo(1));
            var session=new AqSession(AqScenarios.Ids[2],new[]{new[]{"colosso","ciantia","talyssa"},new[]{"damian","perry","diantus"}});
            Assert.That(session.Units.Single(u=>u.HeroId=="dragao-espectral").Huge,Is.True);
        }

        [Test] public void EveryEnemyUsesTheWholeArcadiaLineOfItsProfile()
        {
            foreach(var id in AqScenarios.Ids)
            {
                var s=AqScenarios.Create(id);var session=new AqSession(id,new[]{new[]{"colosso","ciantia","talyssa"},new[]{"damian","perry","diantus"}});
                foreach(var m in s.Monsters)
                {
                    Assert.That(AqProfiles.All.ContainsKey(m.Profile??""),Is.True,m.HeroId);var p=AqProfiles.All[m.Profile];
                    Assert.That(new[]{m.Health,m.Overkill,m.Defense,m.Movement,m.Dice,m.Reward},Is.EqualTo(new[]{p.Life,p.Overkill,p.Defense,p.Move,p.Attack,p.Coins}),m.HeroId);
                    Assert.That(m.Ranged,Is.EqualTo(p.Ranged),m.HeroId);Assert.That(m.Tier,Is.EqualTo(p.Tier),m.HeroId);
                    // The live unit carries the ability so the session can apply it.
                    Assert.That(session.Units.First(u=>u.Monster&&u.HeroId==m.HeroId).Ability,Is.EqualTo(p.Ability),m.HeroId);
                }
            }
            Assert.That(AqProfiles.All["troll"].Reference,Is.EqualTo("Troll"));
            Assert.That(AqProfiles.All.Values.Count(p=>p.Ability!=null&&p.AbilityText==null),Is.Zero);
        }

        [Test] public void ChaptersVaryMinionsAndBossesUseExportedArt()
        {
            var lineups=AqScenarios.Ids.Select(id=>string.Join(",",AqScenarios.Create(id).Monsters.Where(m=>m.Reward<2).Select(m=>m.HeroId).Distinct().OrderBy(x=>x))).ToArray();
            Assert.That(lineups.Distinct().Count(),Is.EqualTo(lineups.Length));
            Assert.That(AqScenarios.Create(AqScenarios.Ids[0]).Monsters.Count(m=>m.HeroId=="rei-goblin-ferrugem"),Is.EqualTo(1));
            Assert.That(AqScenarios.Create(AqScenarios.Ids[2]).Monsters.Count(m=>m.Tier=="boss"),Is.EqualTo(2));
            foreach(var id in AqScenarios.Ids)
                foreach(var m in AqScenarios.Create(id).Monsters.Where(m=>m.SpriteFolder!=null))
                    Assert.That(UnityEngine.Resources.Load<UnityEngine.Sprite>("Sprites/"+m.SpriteFolder+"/south"),Is.Not.Null,m.SpriteFolder);
        }
    }
}
