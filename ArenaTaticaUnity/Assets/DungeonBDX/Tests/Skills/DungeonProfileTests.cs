using System;
using System.Linq;
using ArenaTatica.Core;
using DungeonBDX.Combat;
using DungeonBDX.Skills;
using NUnit.Framework;

namespace DungeonBDX.Tests.Skills
{
    public sealed class DungeonProfileTests
    {
        [Test]
        public void EveryPlayableArenaIdHasAnExplicitBoundedIndependentAdaptation()
        {
            var originals = Content.Fighters.Values.Where(hero => hero.picker && hero.level > 0).ToArray();
            Assert.That(DungeonHeroProfiles.ArenaSources.Keys, Is.EquivalentTo(originals.Select(hero => hero.id)));
            foreach (var hero in originals)
            {
                int hp = hero.hp, armor = hero.armor, movement = hero.pm;
                var profile = DungeonHeroProfiles.AdaptArena(hero.id, hero.name, hp, armor, movement, hero.skills.Select(skill => skill.id));
                Assert.That(profile.Imported, Is.True); Assert.That(profile.Id, Is.EqualTo(hero.id));
                Assert.That(profile.SourceHealth, Is.EqualTo(hp)); Assert.That(profile.SourceArmor, Is.EqualTo(armor));
                Assert.That(profile.SourceSkillIds, Is.EqualTo(hero.skills.Select(skill => skill.id)));
                Assert.That(profile.Health, Is.InRange(40, 60)); Assert.That(profile.Armor, Is.InRange(0, 50));
                Assert.That(profile.Movement, Is.InRange(2, 5)); Assert.That(profile.Actions, Is.EqualTo(CombatCaps.Actions));
                Assert.That(profile.SkillIds.Count, Is.InRange(1, 6));
                Assert.That(profile.SkillIds, Does.Contain("dbdx.basico.golpe"));
                Assert.That(hero.hp, Is.EqualTo(hp)); Assert.That(hero.armor, Is.EqualTo(armor)); Assert.That(hero.pm, Is.EqualTo(movement));
            }
        }

        [Test]
        public void ConversionUsesInt64AndSnapshotsSourceLists()
        {
            string[] sourceSkills = { "original" };
            var high = DungeonHeroProfiles.AdaptArena("damian", "Damian", int.MaxValue, int.MaxValue, int.MaxValue, sourceSkills);
            var low = DungeonHeroProfiles.AdaptArena("damian", "Damian", int.MinValue, int.MinValue, int.MinValue);
            sourceSkills[0] = "changed";
            Assert.That(high.Health, Is.EqualTo(60)); Assert.That(high.Armor, Is.EqualTo(50)); Assert.That(high.Movement, Is.EqualTo(5));
            Assert.That(low.Health, Is.EqualTo(40)); Assert.That(low.Armor, Is.Zero); Assert.That(low.Movement, Is.EqualTo(2));
            Assert.That(high.SourceSkillIds[0], Is.EqualTo("original"));
            Assert.Throws<ArgumentException>(() => DungeonHeroProfiles.AdaptArena("unknown", "X", 10, 0, 3));
        }

        [Test]
        public void ThreeNewArchetypesHaveUniqueIdsAndValidLoadouts()
        {
            var heroes = DungeonHeroProfiles.NewHeroes;
            Assert.That(heroes.Count, Is.EqualTo(3)); Assert.That(heroes.Select(hero => hero.Id).Distinct().Count(), Is.EqualTo(3));
            Assert.That(heroes.Select(hero => hero.Archetype), Is.EquivalentTo(new[] { HeroArchetype.Gunslinger, HeroArchetype.Arcanist, HeroArchetype.DragonBinder }));
            foreach (var hero in heroes)
            {
                Assert.That(hero.Imported, Is.False); Assert.That(DungeonHeroProfiles.ArenaSources.ContainsKey(hero.Id), Is.False);
                Assert.That(hero.SkillIds.All(DungeonSkillCatalog.All.ContainsKey), Is.True);
            }
        }

        [Test]
        public void AllEighteenSkillsDeclareBoundedCostsAndStableArtBindings()
        {
            var all = DungeonSkillCatalog.All.Values.ToArray();
            Assert.That(all.Length, Is.EqualTo(18));
            Assert.That(all.Select(skill => skill.AnimationId).Distinct().Count(), Is.EqualTo(18));
            Assert.That(all.Select(skill => skill.SoundId).Distinct().Count(), Is.EqualTo(18));
            foreach (var skill in all)
            {
                Assert.That(skill.Cost, Is.InRange(1, 3)); Assert.That(skill.MaxTargets, Is.InRange(1, 3));
                Assert.That(skill.Dice, Is.InRange(0, CombatCaps.Dice)); Assert.That(skill.Power, Is.InRange(0, CombatCaps.BasePower));
                Assert.That(skill.Healing, Is.InRange(0, CombatCaps.HealingPerAction));
                Assert.That(skill.Shield, Is.InRange(0, CombatCaps.ShieldPerAction));
                Assert.That(skill.Duration, Is.InRange(0, 1)); Assert.That(skill.Cooldown, Is.InRange(0, 2));
                Assert.That(skill.Name, Is.Not.Empty); Assert.That(skill.AnimationId, Does.StartWith("dbdx.anim."));
            }
        }
    }
}
