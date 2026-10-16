using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using DungeonBDX.Services;
using DungeonBDX.Skills;
using NUnit.Framework;

namespace DungeonBDX.Tests.Services
{
    public sealed class DungeonCampaignSaveTests
    {
        string directory;
        static DungeonHeroProfile Profile(string id) => DungeonHeroProfiles.NewHeroes.SingleOrDefault(hero => hero.Id == id);
        static ISkillBoard Board(ulong seed, string scenario) => new DungeonSkillBoard(8, 8);
        static CampaignJournal Campaign() => CampaignJournal.Create(91234, "cripta-de-bronze", SkillMechanics.Default,
            new[] {
                new HeroSpawn("binder", 0, DungeonHeroProfiles.NewHeroes[2], new GridCell(1, 1)),
                new HeroSpawn("gunner", 1, DungeonHeroProfiles.NewHeroes[0], new GridCell(4, 1)),
                new HeroSpawn("mage", 0, DungeonHeroProfiles.NewHeroes[1], new GridCell(2, 1))
            }, Board(0, ""));
        [SetUp] public void Setup() { directory = Path.Combine(Path.GetTempPath(), "dbdx-save-tests-" + Guid.NewGuid().ToString("N")); }
        [TearDown] public void Cleanup() { if (Directory.Exists(directory)) Directory.Delete(directory, true); }

        [Test]
        public void ReplayPreservesSummonCooldownBudgetsRngAndNextOutcome()
        {
            var source = Campaign();
            Assert.That(source.Cast("binder", "dbdx.draco.invocar", destination: new GridCell(1, 2)).Success, Is.True);
            Assert.That(source.Cast("binder", "dbdx.draco.comandar", new[] { "gunner" }).Success, Is.True);
            Assert.That(source.EndTurn("binder"), Is.True);
            Assert.That(source.Cast("gunner", "dbdx.pistola.duplo", new[] { "binder" }).Success, Is.True);
            Assert.That(source.EndTurn("gunner"), Is.True);
            Assert.That(source.Cast("mage", "dbdx.arcano.egide", new[] { "binder" }).Success, Is.True);
            Assert.That(source.EndTurn("mage"), Is.True);
            var restored = CampaignJournal.Restore(source.Export(), Profile, Board);
            Assert.That(restored.Export(), Is.EqualTo(source.Export()));
            Assert.That(restored.Session.RandomState, Is.EqualTo(source.Session.RandomState));
            Assert.That(restored.Session.PetId("binder"), Is.EqualTo(source.Session.PetId("binder")));
            Assert.That(restored.Cast("binder", "dbdx.draco.invocar", destination: new GridCell(0, 1)).Failure,
                Is.EqualTo(source.Cast("binder", "dbdx.draco.invocar", destination: new GridCell(0, 1)).Failure));
            var a = source.Cast("binder", "dbdx.draco.comandar", new[] { "gunner" });
            var b = restored.Cast("binder", "dbdx.draco.comandar", new[] { "gunner" });
            Assert.That(b.Success, Is.True); Assert.That(b.Roll?.Points, Is.EqualTo(a.Roll?.Points));
            Assert.That(restored.Export(), Is.EqualTo(source.Export()));
        }

        [Test]
        public void InvalidCommandsDoNotEnterJournalAndMovementReplays()
        {
            var campaign = Campaign();
            Assert.That(campaign.EndTurn("gunner"), Is.False);
            Assert.That(campaign.Cast("binder", "dbdx.draco.comandar", new[] { "gunner" }).Success, Is.False);
            Assert.That(campaign.Move("binder", new GridCell(4, 1)), Is.Not.EqualTo(SkillFailure.None));
            Assert.That(campaign.CommandCount, Is.Zero);
            Assert.That(campaign.Move("binder", new GridCell(0, 1)), Is.EqualTo(SkillFailure.None));
            Assert.That(campaign.CommandCount, Is.EqualTo(1));
            Assert.That(CampaignJournal.Restore(campaign.Export(), Profile, Board).Session.Unit("binder").Cell,
                Is.EqualTo(new GridCell(0, 1)));
        }

        [Test]
        public void OutOfBandSessionMutationCannotBeSaved()
        {
            var campaign = Campaign(); campaign.Session.EndTurn("binder");
            Assert.Throws<InvalidOperationException>(() => campaign.Export());
            Assert.Throws<InvalidOperationException>(() => campaign.EndTurn("gunner"));
        }

        [Test]
        public void RejectsCorruptionUnknownSchemaOversizedPayloadAndUnknownProfile()
        {
            string json = Campaign().Export();
            Assert.Throws<FormatException>(() => CampaignJournal.Restore(json.Replace("91234", "91235"), Profile, Board));
            Assert.Throws<FormatException>(() => CampaignJournal.Restore(json.Replace("\"schema\":1", "\"schema\":2"), Profile, Board));
            Assert.Throws<FormatException>(() => CampaignJournal.Restore(" " + json, Profile, Board));
            Assert.Throws<FormatException>(() => CampaignJournal.Restore(json, id => null, Board));
            Assert.Throws<FormatException>(() => CampaignJournal.Restore(new string('x', CampaignJournal.MaxJsonBytes + 1), Profile, Board));
        }

        [Test]
        public void RecomputedChecksumDoesNotBypassCommandValidation()
        {
            var campaign = Campaign(); campaign.EndTurn("binder");
            string Rehash(string json)
            {
                string bare = Regex.Replace(json, "\\\"digest\\\":\\\"[a-f0-9]{64}\\\"", "\"digest\":\"\"");
                using (var sha = SHA256.Create())
                {
                    string digest = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(bare))).Replace("-", "").ToLowerInvariant();
                    return bare.Replace("\"digest\":\"\"", "\"digest\":\"" + digest + "\"");
                }
            }
            Assert.Throws<FormatException>(() => CampaignJournal.Restore(Rehash(campaign.Export().Replace("\"kind\":\"end\"", "\"kind\":\"unknown\"")), Profile, Board));
            Assert.Throws<FormatException>(() => CampaignJournal.Restore(Rehash(campaign.Export().Replace("\"actor\":\"binder\"", "\"actor\":\"gunner\"")), Profile, Board));
        }

        [Test]
        public void CommandLimitCannotBeExceededAndCampaignFlagsRoundTrip()
        {
            var original = Campaign();
            var campaign = CampaignJournal.Create(original.Seed, original.Scenario, original.Mechanics,
                new[] {
                    new HeroSpawn("binder", 0, DungeonHeroProfiles.NewHeroes[2], new GridCell(1, 1)),
                    new HeroSpawn("gunner", 1, DungeonHeroProfiles.NewHeroes[0], new GridCell(4, 1))
                }, Board(0, ""), campaignFlags: 9);
            for (int i = 0; i < CampaignJournal.MaxCommands; i++) Assert.That(campaign.EndTurn(campaign.Session.ActiveActorId), Is.True);
            Assert.Throws<InvalidOperationException>(() => campaign.EndTurn(campaign.Session.ActiveActorId));
            var restored = CampaignJournal.Restore(campaign.Export(), Profile, Board);
            Assert.That(restored.CampaignFlags, Is.EqualTo(9)); Assert.That(restored.CommandCount, Is.EqualTo(CampaignJournal.MaxCommands));
        }

        [Test]
        public void StoreEnforcesRevisionAndIdempotentRetry()
        {
            var store = new LocalCampaignStore(directory, "local"); var campaign = Campaign();
            Assert.That(store.Load(Profile, Board), Is.Null);
            Assert.That(store.Save(campaign, 0, "op1"), Is.EqualTo(1));
            Assert.That(store.Save(campaign, 0, "op1"), Is.EqualTo(1));
            Assert.Throws<SaveConflictException>(() => store.Save(campaign, 0, "op2"));
            campaign.EndTurn("binder");
            Assert.Throws<SaveConflictException>(() => store.Save(campaign, 1, "op1"));
            Assert.That(store.Save(campaign, 1, "op2"), Is.EqualTo(2));
            Assert.That(store.Load(Profile, Board).Journal.Export(), Is.EqualTo(campaign.Export()));
        }

        [Test]
        public void InterruptedTemporaryFileDoesNotReplaceCommittedRevision()
        {
            var store = new LocalCampaignStore(directory, "local");
            store.Save(Campaign(), 0, "op1");
            File.WriteAllText(Path.Combine(directory, "offline-local", "campaign.json.tmp"), "interrupted");
            Assert.That(store.Load(Profile, Board).Revision, Is.EqualTo(1));
        }

        [Test]
        public void RecoversValidBackupAndPreservesItDuringNextWrite()
        {
            var store = new LocalCampaignStore(directory, "local"); var campaign = Campaign();
            store.Save(campaign, 0, "op1"); campaign.EndTurn("binder"); store.Save(campaign, 1, "op2");
            string primary = Path.Combine(directory, "offline-local", "campaign.json");
            File.WriteAllText(primary, "damaged");
            var recovered = store.Load(Profile, Board);
            Assert.That(recovered.Revision, Is.EqualTo(1)); Assert.That(recovered.RecoveredBackup, Is.True);
            Assert.That(store.Save(recovered.Journal, 1, "repair"), Is.EqualTo(3));
            Assert.Throws<SaveConflictException>(() => store.Save(campaign, 2, "stale-instance"));
            File.WriteAllText(primary, "second damage");
            Assert.That(store.Load(Profile, Board).Revision, Is.EqualTo(1));
        }

        [Test]
        public void ProfilesCannotTraverseOrReadEachOthersStorage()
        {
            Assert.Throws<ArgumentException>(() => new LocalCampaignStore(directory, "../other"));
            var a = new LocalCampaignStore(directory, "a"); var b = new LocalCampaignStore(directory, "b");
            a.Save(Campaign(), 0, "op1"); Assert.That(b.Load(Profile, Board), Is.Null);
            Directory.CreateDirectory(Path.Combine(directory, "offline-b"));
            File.Copy(Path.Combine(directory, "offline-a", "campaign.json"), Path.Combine(directory, "offline-b", "campaign.json"));
            Assert.Throws<InvalidDataException>(() => b.Load(Profile, Board));
        }

        [Test]
        public void CorruptHistoryIsNeverSilentlyOverwritten()
        {
            var store = new LocalCampaignStore(directory, "local"); store.Save(Campaign(), 0, "op1");
            File.WriteAllText(Path.Combine(directory, "offline-local", "campaign.json"), "corrupt");
            File.WriteAllText(Path.Combine(directory, "offline-local", "campaign.json.bak"), "corrupt");
            Assert.Throws<InvalidDataException>(() => store.Load(Profile, Board));
            Assert.Throws<InvalidDataException>(() => store.Save(Campaign(), 0, "op2"));
        }

        [Test]
        public void FirstSaveAlreadyHasBackupAndIdempotentRetryRepairsPrimary()
        {
            var store = new LocalCampaignStore(directory, "local"); var campaign = Campaign();
            store.Save(campaign, 0, "op1");
            File.WriteAllText(Path.Combine(directory, "offline-local", "campaign.json"), "damaged");
            Assert.That(store.Load(Profile, Board).RecoveredBackup, Is.True);
            Assert.That(store.Save(campaign, 0, "op1"), Is.EqualTo(1));
            Assert.That(store.Load(Profile, Board).RecoveredBackup, Is.False);
        }

        [Test]
        public void ExclusiveWriterLockIsReportedAsConflict()
        {
            var store = new LocalCampaignStore(directory, "local"); store.Save(Campaign(), 0, "op1");
            using (var writer = new FileStream(Path.Combine(directory, "offline-local", "writer.lock"), FileMode.Open, FileAccess.Write, FileShare.None))
                Assert.Throws<SaveConflictException>(() => store.Save(Campaign(), 1, "op2"));
        }
    }
}
