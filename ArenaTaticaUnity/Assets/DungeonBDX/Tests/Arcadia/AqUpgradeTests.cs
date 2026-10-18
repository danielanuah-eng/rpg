using System;
using System.Linq;
using DungeonBDX.Arcadia;
using NUnit.Framework;

namespace DungeonBDX.Tests
{
    // Arcadia Upgrade phase between chapters: level deck, 6-card draft (2 + 2 kept, 2 passed), up to 3 purchases, 1 coin kept.
    public class AqUpgradeTests
    {
        static readonly string[][] Parties = { new[] { "teantha", "perry", "talyssa" }, new[] { "damian", "diantus", "ossyro" } };
        static readonly string[][] Four = { new[] { "teantha", "perry", "talyssa" }, new[] { "damian", "diantus", "ossyro" }, new[] { "arqueiro-jack", "asamyh", "cheiro-pneu" }, new[] { "rei-bobett", "principe-kellen", "baltazar" } };

        static void Ok(AqSession s, AqCommand c) { var r = s.Apply(c); Assert.That(r.Accepted, Is.True, c.Kind + ": " + r.Message); }

        static AqSession Chapter(int index, int coins, uint seed = 8, string[][] parties = null)
        {
            var s = new AqSession(AqScenarios.Ids[index], parties ?? Parties, seed);
            for (int g = 0; g < s.Guilds.Count; g++) Ok(s, new AqCommand("purse", g, x: coins));
            Ok(s, new AqCommand("upgrade"));
            return s;
        }

        static void Draft(AqSession s) { while (s.Drafting) Ok(s, new AqCommand("draft", s.UpgradeGuild, card: 0)); }

        [Test]
        public void EachLevelDeckFeedsFourGuilds()
        {
            foreach (int level in new[] { 1, 2 })
            {
                var deck = AqShop.Deck(level);
                Assert.That(deck.Length, Is.GreaterThanOrEqualTo(4 * AqShop.Hand), "level " + level);
                Assert.That(deck.All(i => AqShop.Items[i].Level == level), Is.True);
            }
        }

        [Test]
        public void ChapterOneHasNoUpgradePhase()
        {
            var s = new AqSession(AqScenarios.Ids[0], Parties, 8);
            Assert.That(s.Apply(new AqCommand("upgrade")).Accepted, Is.False);
            var b = Chapter(2, 0);
            Assert.That(b.UpgradeLevel, Is.EqualTo(2), "Chapter III uses the level-2 deck.");
            Assert.That(b.Hand(0).All(i => AqShop.Items[i].Level == 2), Is.True);
            Assert.That(b.Apply(new AqCommand("upgrade")).Accepted, Is.False, "Only once.");
        }

        [Test]
        public void DraftKeepsTwoPassesTheRestAndEndsWithSixCards()
        {
            var s = Chapter(1, 9);
            Assert.That(s.Hand(0).Count, Is.EqualTo(6));
            Assert.That(s.Hand(1).Count, Is.EqualTo(6));
            var secondHand = s.Hand(1).ToArray();
            Assert.That(s.UpgradeGuild, Is.EqualTo(0));
            Assert.That(s.Apply(new AqCommand("draft", 1, card: 0)).Accepted, Is.False, "Guilds pick in order.");
            Ok(s, new AqCommand("draft", 0, card: 0));
            Ok(s, new AqCommand("draft", 0, card: 0));
            Assert.That(s.UpgradeGuild, Is.EqualTo(1));
            Ok(s, new AqCommand("draft", 1, card: 0));
            Ok(s, new AqCommand("draft", 1, card: 0));
            Assert.That(s.DraftRound, Is.EqualTo(1));
            Assert.That(s.Hand(0).Count, Is.EqualTo(4), "Guild 1 now holds the 4 cards passed by guild 2.");
            Assert.That(s.Hand(0).SequenceEqual(secondHand.Skip(2)), Is.True);
            Draft(s);
            Assert.That(s.Drafting, Is.False);
            Assert.That(s.Picks(0).Count, Is.EqualTo(6));
            Assert.That(s.Picks(1).Count, Is.EqualTo(6));
            Assert.That(s.Apply(new AqCommand("end")).Accepted, Is.False, "Play waits for the purchases.");
        }

        [Test]
        public void GuildsBuyAtMostThreeAndKeepOneCoin()
        {
            var s = Chapter(1, 99);
            Assert.That(s.Apply(new AqCommand("buy", 0, card: 0)).Accepted, Is.False, "No purchase during the draft.");
            Draft(s);
            Ok(s, new AqCommand("discard", 0, card: 1)); // Spare sword out to make room.
            Assert.That(s.Unit(0).Gear.Count, Is.EqualTo(1));
            for (int i = 0; i < 3; i++) Ok(s, new AqCommand("buy", 0, card: 0));
            Assert.That(s.Unit(0).Gear.Count, Is.EqualTo(4));
            Assert.That(s.Bought(0), Is.EqualTo(3));
            Assert.That(s.Picks(0).Count, Is.EqualTo(3));
            Assert.That(s.Apply(new AqCommand("buy", 1, card: 0)).Accepted, Is.False, "Three purchases per phase.");
            Assert.That(s.Apply(new AqCommand("buy", 3, card: 0)).Accepted, Is.False, "Guild 2 waits for guild 1.");
            Ok(s, new AqCommand("ready", 0));
            Assert.That(s.Guilds[0].Coins, Is.EqualTo(1));
            Assert.That(s.UpgradeGuild, Is.EqualTo(1));
            Ok(s, new AqCommand("ready", 1));
            Assert.That(s.Guilds[1].Coins, Is.EqualTo(1));
            Assert.That(s.UpgradeOpen, Is.False);
            Ok(s, new AqCommand("end"));
        }

        [Test]
        public void PurchasesRespectCoins()
        {
            var s = Chapter(1, 0); Draft(s);
            Assert.That(s.Apply(new AqCommand("buy", 2, card: 0)).Accepted, Is.False, "No coins, no card.");
            Ok(s, new AqCommand("ready", 0));
            Assert.That(s.Guilds[0].Coins, Is.EqualTo(0));
        }

        [Test]
        public void FourGuildsDraftInARing()
        {
            var s = Chapter(1, 5, 3, Four);
            var hands = Enumerable.Range(0, 4).Select(g => s.Hand(g).ToArray()).ToArray();
            for (int g = 0; g < 4; g++) { Ok(s, new AqCommand("draft", g, card: 0)); Ok(s, new AqCommand("draft", g, card: 0)); }
            for (int g = 0; g < 4; g++) Assert.That(s.Hand((g + 1) % 4).SequenceEqual(hands[g].Skip(2)), Is.True, "guild " + g);
            Draft(s);
            Assert.That(Enumerable.Range(0, 4).All(g => s.Picks(g).Count == 6), Is.True);
        }

        [Test]
        public void UpgradePhaseReplaysAndKeepsTheDice()
        {
            var j = new AqJournal(AqScenarios.Ids[1], Parties, 21, false);
            var plain = new AqSession(AqScenarios.Ids[1], Parties, 21);
            foreach (var c in new[] { new AqCommand("purse", 0, x: 7), new AqCommand("purse", 1, x: 4), new AqCommand("upgrade") }) Ok(j.Session, c);
            Ok(j.Session, new AqCommand("draft", 0, card: 2));
            var mid = AqJournal.Restore(j.Export());
            Assert.That(mid.Session.Fingerprint(), Is.EqualTo(j.Session.Fingerprint()), "Mid-draft save.");
            Draft(j.Session);
            Ok(j.Session, new AqCommand("buy", 0, card: 0));
            Ok(j.Session, new AqCommand("ready", 0)); Ok(j.Session, new AqCommand("ready", 1));
            Assert.That(j.Session.RandomState, Is.EqualTo(plain.RandomState), "The deal does not consume the dice.");
            var copy = AqJournal.Restore(j.Export());
            Assert.That(copy.Session.Fingerprint(), Is.EqualTo(j.Session.Fingerprint()));
            Assert.That(j.Session.Fingerprint().Contains("|up:1:done"), Is.True);
            Assert.That(plain.Fingerprint().Contains("|up:"), Is.False, "Chapters without the phase keep the old fingerprint.");
        }

        [Test]
        public void CompactionAfterTheUpgradePhaseKeepsItClosed()
        {
            var j = new AqJournal(AqScenarios.Ids[1], Parties, 13, true);
            foreach (var c in new[] { new AqCommand("purse", 0, x: 3), new AqCommand("purse", 1, x: 3), new AqCommand("upgrade") }) Ok(j.Session, c);
            Assert.That(j.ShouldCompact, Is.False);
            Draft(j.Session); Ok(j.Session, new AqCommand("ready", 0)); Ok(j.Session, new AqCommand("ready", 1));
            while (j.Session.Commands.Count < AqJournal.CompactAt) Ok(j.Session, new AqCommand("end"));
            string before = j.Session.Fingerprint();
            Assert.That(j.Compact(), Is.True);
            var reopened = AqJournal.Restore(j.Export(), j.Base, j.Compactions);
            Assert.That(reopened.Session.Fingerprint(), Is.EqualTo(before));
            Assert.That(reopened.Session.UpgradeOpen, Is.False);
        }
    }
}
