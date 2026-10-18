using System;
using System.Linq;
using DungeonBDX.Arcadia;
using NUnit.Framework;

namespace DungeonBDX.Tests
{
    // Chapter carry-over (coins, bought cards, host adjustments) and the hidden history compaction at 50%.
    public class AqCampaignTests
    {
        static readonly string[][] Parties = { new[] { "teantha", "perry", "talyssa" }, new[] { "damian", "diantus", "ossyro" } };

        static void Ok(AqSession s, AqCommand c) { var r = s.Apply(c); Assert.That(r.Accepted, Is.True, c.Kind + ": " + r.Message); }
        static int Item(string id) => Array.FindIndex(AqShop.Items, i => i.Id == id);

        [Test]
        public void NextChapterKeepsCoinsCardsAndHostAdjustments()
        {
            var a = new AqSession(AqScenarios.Ids[0], Parties, 7);
            Ok(a, new AqCommand("tune", card: AqTuning.HeroHealth, x: 1));
            Ok(a, new AqCommand("tune", card: AqTuning.StartCoins, x: 9));
            Ok(a, new AqCommand("rest"));
            Ok(a, new AqCommand("buy", 1, card: Item("keen-dagger")));
            var b = new AqSession(AqScenarios.Ids[1], Parties, 8);
            foreach (var c in a.CarryCommands()) Ok(b, c);
            Assert.That(b.Guilds[0].Coins, Is.EqualTo(5));
            Assert.That(b.Guilds[1].Coins, Is.EqualTo(9));
            Assert.That(b.Unit(1).Gear.Any(g => g.Id.StartsWith("keen-dagger.") && g.Sharp), Is.True);
            Assert.That(b.Tuning, Is.EqualTo(a.Tuning));
            Assert.That(b.Unit(0).MaxHealth, Is.EqualTo(AqContent.Hero("teantha").Health + 1));
            Assert.That(b.UpgradeOpen && b.UpgradeLevel == 1, Is.True, "The carried chapter opens the Upgrade phase.");
            Assert.That(b.Apply(new AqCommand("end")).Accepted, Is.False, "No play during the Upgrade phase.");
            while (b.UpgradeOpen) Ok(b, b.Drafting ? new AqCommand("draft", b.UpgradeGuild, card: 0) : new AqCommand("ready", b.UpgradeGuild));
            Assert.That(b.Guilds.All(g => g.Coins <= AqShop.KeptCoins), Is.True, "Only one coin is kept.");
            Ok(b, new AqCommand("end"));
            Assert.That(b.Apply(new AqCommand("purse", 0, x: 99)).Accepted, Is.False, "Only before the first move.");
        }

        [Test]
        public void CarriedCardsReplaceTheSpareWeaponWhenTheHandIsFull()
        {
            var a = new AqSession(AqScenarios.Ids[0], Parties, 7); a.Guilds[0].Coins = 20;
            Ok(a, new AqCommand("rest"));
            Ok(a, new AqCommand("discard", 0, card: 1)); // Spare sword out.
            for (int i = 0; i < 3; i++) Ok(a, new AqCommand("buy", 0, card: Item("yew-bow")));
            var b = new AqSession(AqScenarios.Ids[1], Parties, 8);
            foreach (var c in a.CarryCommands()) Ok(b, c);
            Assert.That(b.Unit(0).Gear.Count, Is.EqualTo(4));
            Assert.That(b.Unit(0).Gear.Count(g => g.Id.StartsWith("yew-bow.")), Is.EqualTo(3));
        }

        [Test]
        public void HalfFullHistoryIsCompactedWithoutChangingTheGame()
        {
            var compacted = new AqJournal(AqScenarios.Ids[0], Parties, 11, true);
            var plain = new AqJournal(AqScenarios.Ids[0], Parties, 11, true);
            Assert.That(compacted.ShouldCompact, Is.False);
            while (compacted.Session.Commands.Count < AqJournal.CompactAt)
            { Ok(compacted.Session, new AqCommand("end")); Ok(plain.Session, new AqCommand("end")); }
            Assert.That(compacted.CommandLoad, Is.GreaterThanOrEqualTo(.5f));
            string before = compacted.Session.Fingerprint();
            Assert.That(compacted.Compact(), Is.True);
            Assert.That(compacted.Session.Commands, Is.Empty);
            Assert.That(compacted.Base, Is.Not.Null);
            Assert.That(compacted.Session.Fingerprint(), Is.EqualTo(before));
            foreach (var j in new[] { compacted, plain })
            { Ok(j.Session, new AqCommand("rest")); Ok(j.Session, new AqCommand("end")); Ok(j.Session, new AqCommand("end")); }
            Assert.That(compacted.Session.Fingerprint(), Is.EqualTo(plain.Session.Fingerprint()), "Same game after compaction.");
            var reopened = AqJournal.Restore(compacted.Export(), compacted.Base, compacted.Compactions);
            Assert.That(reopened.Session.Fingerprint(), Is.EqualTo(plain.Session.Fingerprint()));
            Assert.That(reopened.Compactions, Is.EqualTo(1));
        }

        [Test]
        public void CompactionWaitsForTheTurnBoundary()
        {
            var j = new AqJournal(AqScenarios.Ids[0], Parties, 11, true);
            while (j.Session.Commands.Count < AqJournal.CompactAt - 1) Ok(j.Session, new AqCommand("end"));
            Ok(j.Session, new AqCommand("rest"));
            Assert.That(j.ShouldCompact, Is.False, "Resting guild: wait for the end of the turn.");
            Assert.That(j.Compact(), Is.False);
            Ok(j.Session, new AqCommand("end"));
            Assert.That(j.Compact(), Is.True);
        }
    }
}
