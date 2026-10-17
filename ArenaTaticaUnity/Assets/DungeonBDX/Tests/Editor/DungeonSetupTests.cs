using System;
using System.Linq;
using ArenaTatica.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DungeonBDX.Tests
{
    public sealed class DungeonSetupTests
    {
        [Test]
        public void CatalogKeepsEveryPlayableSourceHeroAndSkill()
        {
            var heroes = DungeonHeroCatalog.ImportArena();
            var sources = Content.Fighters.Values.Where(f => f.picker && f.level > 0).ToArray();
            Assert.That(heroes.Count, Is.EqualTo(sources.Length).And.GreaterThan(0));
            Assert.That(heroes.Select(h => h.Id).Distinct().Count(), Is.EqualTo(heroes.Count));
            foreach (var hero in heroes)
            {
                var source = Content.Fighters[hero.Id];
                Assert.That(hero.Name, Is.Not.Null.And.Not.Empty);
                Assert.That(hero.Health, Is.EqualTo(source.hp)); Assert.That(hero.Actions, Is.EqualTo(source.pa));
                Assert.That(hero.Movement, Is.EqualTo(source.pm)); Assert.That(hero.Armor, Is.EqualTo(source.armor));
                Assert.That(hero.Skills.Select(s => s.Id), Is.EqualTo(source.skills.Select(s => s.id)));
                foreach (var skill in hero.Skills) Assert.That(skill.Name, Is.Not.Null.And.Not.Empty);
            }
        }

        [Test]
        public void SnapshotsAreIndependentOfLaterSourceEdits()
        {
            var hero = DungeonHeroCatalog.ImportArena()[0]; var source = Content.Fighters[hero.Id];
            int oldHp = source.hp; string oldSkillName = source.skills[0].name;
            try
            {
                source.hp++; source.skills[0].name = "changed";
                Assert.That(hero.Health, Is.EqualTo(oldHp)); Assert.That(hero.Skills[0].Name, Is.EqualTo(oldSkillName));
            }
            finally { source.hp = oldHp; source.skills[0].name = oldSkillName; }
        }

        [Test]
        public void PartyRejectsUnknownHeroesDuplicatesAndOverflow()
        {
            var heroes = DungeonHeroCatalog.ImportArena(); var setup = new DungeonSetup(heroes);
            Assert.That(setup.ToggleHero("missing"), Is.False); Assert.That(setup.ToggleHero(null), Is.False);
            foreach (var hero in heroes.Take(3)) Assert.That(setup.ToggleHero(hero.Id), Is.True);
            Assert.That(setup.ToggleHero(heroes[3].Id), Is.False);
            Assert.That(setup.ToggleHero(heroes[0].Id), Is.True); Assert.That(setup.Party.Count, Is.EqualTo(2));
            Assert.That(setup.ToggleHero(heroes[3].Id), Is.True);
            Assert.That(setup.Party.Distinct().Count(), Is.EqualTo(3));
        }

        [Test]
        public void CampaignModulesCanBeEnabledIndependently()
        {
            var setup = new DungeonSetup(DungeonHeroCatalog.ImportArena());
            Assert.That(setup.Mechanics,Is.EqualTo(CampaignMechanics.None));
            setup.SetMechanic(CampaignMechanics.Objectives,true);
            setup.SetMechanic(CampaignMechanics.Traps, false);
            setup.SetMechanic(CampaignMechanics.DragonEncounters, true);
            Assert.That(setup.HasMechanic(CampaignMechanics.Objectives), Is.True);
            Assert.That(setup.HasMechanic(CampaignMechanics.Traps), Is.False);
            Assert.That(setup.HasMechanic(CampaignMechanics.DragonEncounters), Is.True);
            Assert.That(setup.HasMechanic(CampaignMechanics.FriendlyFire), Is.False);
            Assert.That(setup.HasMechanic(CampaignMechanics.None), Is.False);
            Assert.Throws<ArgumentOutOfRangeException>(() => setup.SetMechanic((CampaignMechanics)128, true));
        }

        [TestCase(30)] [TestCase(60)] [TestCase(144)]
        public void MotionConvergesEquallyAcrossFrameRates(int fps)
        {
            float value = 0;
            for (int i = 0; i < fps; i++) value = Mathf.Lerp(value, 1, DungeonMotion.Blend(8, 1f / fps));
            Assert.That(value, Is.EqualTo(1 - Mathf.Exp(-8)).Within(.00001));
        }

        [TestCase(1080, 2400, 60, 100, 960, 2150)]
        [TestCase(2400, 1080, 100, 20, 2200, 1040)]
        public void SafeAreaRespectsNotchesInBothOrientations(int w, int h, int x, int y, int width, int height)
        {
            var go = new GameObject("Safe", typeof(RectTransform));
            try
            {
                var rect = go.GetComponent<RectTransform>();
                DungeonShell.ApplySafeArea(rect, new Rect(x, y, width, height), w, h);
                Assert.That(rect.anchorMin.x, Is.EqualTo(x / (float)w).Within(.00001));
                Assert.That(rect.anchorMax.y, Is.EqualTo((y + height) / (float)h).Within(.00001));
                Assert.That(rect.offsetMin, Is.EqualTo(Vector2.zero));
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }

        [Test]
        public void AllSixDungeonOrionSkillsCanBeConsultedThroughTheBottomBar()
        {
            var go = new GameObject("DungeonBDX paging fixture");
            try
            {
                var shell = go.AddComponent<DungeonShell>(); shell.Initialize(); shell.SelectHero("orion-prisma");
                var seen = new System.Collections.Generic.HashSet<string>();
                for (int page = 0; page < 3; page++)
                {
                    var hud = go.GetComponentsInChildren<RectTransform>().First(r => r.name == "Bottom Action Bar");
                    var buttons = hud.GetComponentsInChildren<UnityEngine.UI.Button>();
                    foreach (var button in buttons.Where(b => b.name.StartsWith("Action ")))
                        seen.Add(button.name.Substring("Action ".Length));
                    buttons.Last().onClick.Invoke();
                }
                Assert.That(seen, Is.EquivalentTo(DungeonProfiles.Resolve("orion-prisma").SkillIds));
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }

        [Test]
        public void ShellKeepsSetupAcrossTabsAndDoesNotCreateArenaMatch()
        {
            var go = new GameObject("DungeonBDX fixture");
            try
            {
                var shell = go.AddComponent<DungeonShell>(); shell.Initialize();
                string id = DungeonHeroCatalog.ImportPlayable()[0].Id;
                shell.SelectHero(id); shell.Setup.ToggleHero(id);
                shell.ShowTab(DungeonTab.Campaign); shell.ShowTab(DungeonTab.Session); shell.ShowTab(DungeonTab.Heroes);
                Assert.That(shell.SelectedHeroId, Is.EqualTo(id)); Assert.That(shell.Setup.Party, Does.Contain(id));
                Assert.That(go.GetComponent<ArenaTatica.ArenaGame>(), Is.Null);
                Assert.That(go.GetComponentInChildren<UnityEngine.UI.GraphicRaycaster>(), Is.Not.Null);
                Assert.That(go.GetComponentInChildren<Camera>(), Is.Not.Null);
                // EventSystem registers current during Play; EditMode checks scene wiring.
                Assert.That(UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
                Assert.That(go.GetComponentInChildren<BaseInputModule>(), Is.Not.Null);
                Assert.That(go.GetComponentsInChildren<UnityEngine.UI.Button>().Length, Is.GreaterThan(6));
                foreach (var image in go.GetComponentsInChildren<UnityEngine.UI.Image>())
                    if (!image.GetComponent<UnityEngine.UI.Button>()) Assert.That(image.raycastTarget, Is.False);
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }
    }
}
