using System;
using System.Linq;
using DungeonBDX.Arcadia;
using NUnit.Framework;

namespace DungeonBDX.Tests
{
    // Rest shop (Arcadia-style upgrades bought with coins) and host adjustments recorded as replay commands.
    public class AqShopTests
    {
        static readonly string[][] Parties = { new[] { "teantha", "perry", "talyssa" }, new[] { "damian", "diantus", "ossyro" } };

        static AqSession Duel(uint seed)
        {
            var s = new AqSession(AqScenarios.Ids[0], Parties, seed);
            s.Board.Edges.Clear();
            foreach (var t in s.Scenario.Tokens) t.Taken = true;
            foreach (var u in s.Units)
            {
                if (u.Monster) u.Health = 0;
                else { u.Health = u.MaxHealth = 1000; u.Defense = 0; }
            }
            s.Unit(0).Cell = new AqCell(3, 4);
            s.Unit(1).Cell = new AqCell(0, 0);
            s.Unit(2).Cell = new AqCell(0, 1);
            s.Unit(3).Cell = new AqCell(7, 7);
            s.Unit(4).Cell = new AqCell(8, 8);
            s.Unit(5).Cell = new AqCell(8, 7);
            return s;
        }

        static AqResult Do(AqSession s, string kind, int actor = -1, int target = -1, int card = -1, int x = 0, int y = 0) =>
            s.Apply(new AqCommand(kind, actor, target, card, x, y));

        static int Item(string id) => Array.FindIndex(AqShop.Items, i => i.Id == id);

        [Test]
        public void ShopSellsOnlyWhileRestingAndSpendsGuildCoins()
        {
            var s = Duel(3); s.Guilds[0].Coins = 5; int before = s.Unit(0).Gear.Count;
            Assert.That(Do(s, "buy", 0, card: Item("iron-blade")).Accepted, Is.False, "No shop outside rest.");
            Assert.That(Do(s, "rest").Accepted, Is.True);
            var r = Do(s, "buy", 0, card: Item("iron-blade"));
            Assert.That(r.Accepted, Is.True, r.Message);
            Assert.That(s.Guilds[0].Coins, Is.EqualTo(2));
            Assert.That(s.Unit(0).Gear.Count, Is.EqualTo(before + 1));
            Assert.That(s.Unit(0).Gear.Last().Dice, Is.EqualTo(4));
            Assert.That(Do(s, "buy", 0, card: Item("yew-bow")).Accepted, Is.False, "2 coins do not buy a 3-coin card.");
            Assert.That(s.Guilds[0].Coins, Is.EqualTo(2));
            Assert.That(Do(s, "buy", 3, card: Item("yew-bow")).Accepted, Is.False, "Only the resting guild buys.");
        }

        [Test]
        public void InventoryStopsAtFourCards()
        {
            var s = Duel(4); s.Guilds[0].Coins = 99; Assert.That(Do(s, "rest").Accepted, Is.True);
            while (s.Unit(0).Gear.Count < 4) Assert.That(Do(s, "buy", 0, card: Item("yew-bow")).Accepted, Is.True);
            int coins = s.Guilds[0].Coins;
            Assert.That(Do(s, "buy", 0, card: Item("yew-bow")).Accepted, Is.False);
            Assert.That(s.Guilds[0].Coins, Is.EqualTo(coins));
            Assert.That(Do(s, "discard", 0, card: 0).Accepted, Is.False, "The signature weapon stays.");
            Assert.That(Do(s, "discard", 0, card: 3).Accepted, Is.True);
            Assert.That(Do(s, "buy", 0, card: Item("yew-bow")).Accepted, Is.True);
        }

        [Test]
        public void ArmorIsPassiveAndAddsDefenseDice()
        {
            var s = Duel(5); s.Guilds[0].Coins = 9; Do(s, "rest");
            Assert.That(Do(s, "buy", 0, card: Item("scale-coat")).Accepted, Is.True);
            int armor = s.Unit(0).Gear.Count - 1; var coat = s.Unit(0).Gear[armor];
            Assert.That(coat.Passive && coat.Defense == 2, Is.True);
            Do(s, "end"); Do(s, "end");
            s.Unit(3).Cell = new AqCell(4, 4);
            Assert.That(Do(s, "attack", 0, 3, armor).Accepted, Is.False, "Armor never attacks.");
        }

        [Test]
        public void SharpWeaponsGetThroughMightMoreOften()
        {
            int plain = 0, sharp = 0;
            for (uint seed = 1; seed <= 60; seed++)
            {
                int Wounds(bool keen)
                {
                    var s = Duel(seed); var g = s.Unit(0).Gear[0]; g.Dice = 6; g.Ranged = false; g.Sharp = keen;
                    var m = s.Units.First(u => u.Monster); m.Cell = new AqCell(4, 4); m.Health = m.MaxHealth = 100; m.Defense = 0; m.Overkill = 1000; m.Ability = "might";
                    var r = Do(s, "attack", 0, m.Id, 0); Assert.That(r.Accepted, Is.True, r.Message);
                    return r.Effects.Where(e => e.Kind == "hit" && e.Target == m.Id).Sum(e => e.Amount);
                }
                int a = Wounds(false), b = Wounds(true);
                Assert.That(b, Is.GreaterThanOrEqualTo(a), "seed " + seed);
                plain += a; sharp += b;
            }
            Assert.That(sharp, Is.GreaterThan(plain));
        }

        [Test]
        public void HostAdjustmentsApplyBeforeTheFirstMoveAndReplay()
        {
            var j = new AqJournal(AqScenarios.Ids[0], Parties, 29, false); var s = j.Session;
            string plain = s.Fingerprint();
            Assert.That(plain.Contains("tune:"), Is.False, "Untuned games keep the old fingerprint.");
            Assert.That(Do(s, "tune", card: AqTuning.HeroHealth, x: 2).Accepted, Is.True);
            Assert.That(Do(s, "tune", card: AqTuning.MonsterDefense, x: 1).Accepted, Is.True);
            Assert.That(Do(s, "tune", card: AqTuning.StartCoins, x: 5).Accepted, Is.True);
            Assert.That(Do(s, "tune", card: AqTuning.HeroMove, x: 1).Accepted, Is.True);
            Assert.That(Do(s, "tune", card: AqTuning.MonsterDice, x: 9).Accepted, Is.False, "Out of range.");
            var hero = s.Unit(0); Assert.That(hero.MaxHealth, Is.EqualTo(AqContent.Hero(hero.HeroId).Health + 2)); Assert.That(hero.Health, Is.EqualTo(hero.MaxHealth));
            var m = s.Units.First(u => u.Monster); Assert.That(m.Defense, Is.EqualTo(s.Scenario.Monsters[0].Defense + 1));
            Assert.That(s.Guilds.All(g => g.Coins == 5), Is.True);
            Assert.That(s.MovesLeft, Is.EqualTo(4));
            Assert.That(Do(s, "end").Accepted, Is.True);
            Assert.That(s.MovesLeft, Is.EqualTo(4));
            Assert.That(Do(s, "tune", card: AqTuning.HeroHealth, x: 0).Accepted, Is.False, "Locked after the first real command.");
            var copy = AqJournal.Restore(j.Export());
            Assert.That(copy.Session.Fingerprint(), Is.EqualTo(s.Fingerprint()));
            Assert.That(copy.Session.Tuning, Is.EqualTo(s.Tuning));
        }
    }
}
