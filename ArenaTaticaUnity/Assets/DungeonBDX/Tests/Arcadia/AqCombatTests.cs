using System;
using System.Linq;
using DungeonBDX.Arcadia;
using NUnit.Framework;

namespace DungeonBDX.Tests
{
    // Fixtures isolate combat on an open board; assertions use public commands and their effects.
    public class AqCombatTests
    {
        static AqSession Duel(uint seed, string attacker = "teantha", string defender = "damian")
        {
            var s = new AqSession(AqScenarios.Ids[0], new[] {
                new[] { attacker, "perry", "talyssa" },
                new[] { defender, "diantus", "ossyro" }
            }, seed);
            s.Board.Edges.Clear();
            foreach (var t in s.Scenario.Tokens) t.Taken = true;
            foreach (var u in s.Units)
            {
                if (u.Monster) u.Health = 0;
                else { u.Health = u.MaxHealth = 1000; u.Defense = 0; }
                foreach (var g in u.Gear) g.Defense = 0;
            }
            s.Unit(0).Cell = new AqCell(3, 4);
            s.Unit(1).Cell = new AqCell(0, 0);
            s.Unit(2).Cell = new AqCell(0, 1);
            s.Unit(3).Cell = new AqCell(4, 4);
            s.Unit(4).Cell = new AqCell(8, 8);
            s.Unit(5).Cell = new AqCell(8, 7);
            return s;
        }

        static AqResult Apply(AqSession s, string kind, int actor = -1, int target = -1,
            int card = -1, int x = 0, int y = 0)
        {
            var r = s.Apply(new AqCommand(kind, actor, target, card, x, y));
            Assert.That(r.Accepted, Is.True, kind + ": " + r.Message);
            return r;
        }

        static int Wounds(AqResult r, int target) =>
            r.Effects.Where(e => e.Kind == "hit" && e.Target == target).Sum(e => e.Amount);

        static AqResult Attack(AqSession s, int target = 3) => Apply(s, "attack", 0, target, 0);

        static void RefreshAttacker(AqSession s)
        {
            Apply(s, "end"); // Rival guild.
            Apply(s, "end"); // Attacking guild rests instead of activating a hero.
            Apply(s, "rest");
            Apply(s, "end");
            Apply(s, "end");
        }

        [TestCase(1)] [TestCase(65)] [TestCase(129)] [TestCase(193)]
        public void CriticalsStayLinearAndExcessEquipmentDiceCannotBypassTheCap(int firstSeed)
        {
            bool sawCriticalBonus = false;
            for (uint seed = (uint)firstSeed; seed < firstSeed + 64; seed++)
            {
                var legal = Duel(seed); var excessive = Duel(seed);
                legal.Unit(0).Gear[0].Dice = 6;
                excessive.Unit(0).Gear[0].Dice = 100000;
                int bounded = Wounds(Attack(legal), 3);
                int overflow = Wounds(Attack(excessive), 3);
                Assert.That(overflow, Is.EqualTo(bounded), "seed " + seed);
                Assert.That(bounded, Is.InRange(0, 9), "Six dice plus at most three linear critical successes.");
                sawCriticalBonus |= bounded > 6;
            }
            Assert.That(sawCriticalBonus, Is.True, "The seed sample must exercise critical bonuses.");
        }

        [TestCase(1)] [TestCase(97)]
        public void ArbitrarilyLargeDefenseCannotMitigateMoreThanSixtyPercent(int firstSeed)
        {
            bool sawBlock = false;
            for (uint seed = (uint)firstSeed; seed < firstSeed + 96; seed++)
            {
                var bare = Duel(seed); var armored = Duel(seed);
                bare.Unit(0).Gear[0].Dice = armored.Unit(0).Gear[0].Dice = 6;
                armored.Unit(3).Defense = 100000;
                int incoming = Wounds(Attack(bare), 3);
                int taken = Wounds(Attack(armored), 3);
                Assert.That(taken, Is.InRange(incoming - (int)Math.Floor(incoming * .6), incoming), "seed " + seed);
                sawBlock |= taken < incoming;
            }
            Assert.That(sawBlock, Is.True, "Defense must still block damage within the cap.");
        }

        [Test]
        public void DragonBenefitsFromAnAllyNearTheTargetWithoutSharingTheirSpace()
        {
            int boostedTotal = 0, ordinaryTotal = 0;
            for (uint seed = 1; seed <= 64; seed++)
            {
                var solo = Duel(seed, "ciantia"); var supported = Duel(seed, "ciantia");
                supported.Unit(1).Cell = new AqCell(4, 5);
                int ordinary = Wounds(Attack(solo), 3), boosted = Wounds(Attack(supported), 3);
                Assert.That(boosted, Is.GreaterThanOrEqualTo(ordinary), "seed " + seed);
                boostedTotal += boosted; ordinaryTotal += ordinary;
            }
            Assert.That(boostedTotal, Is.GreaterThan(ordinaryTotal));
        }

        [Test]
        public void BondHealsOnlyOneWoundedNearbyAllyAfterActuallyDealingDamage()
        {
            bool sawHit = false;
            for (uint seed = 1; seed <= 32; seed++)
            {
                var s = Duel(seed, "vaelis-vinculo");
                s.Unit(1).Cell = new AqCell(3, 5); s.Unit(1).Health = 1;
                s.Unit(2).Cell = new AqCell(2, 4); s.Unit(2).Health = 2;
                var r = Attack(s); bool hit = Wounds(r, 3) > 0; sawHit |= hit;
                Assert.That(s.Unit(1).Health, Is.EqualTo(hit ? 2 : 1));
                Assert.That(s.Unit(2).Health, Is.EqualTo(2));
                Assert.That(r.Effects.Where(e => e.Kind == "heal").Sum(e => e.Amount), Is.EqualTo(hit ? 1 : 0));
            }
            Assert.That(sawHit, Is.True);
        }

        [Test]
        public void WardRestHealsLivingGuildMembersOnceAndDoesNotResurrectOrHealEnemies()
        {
            var s = Duel(11, "ezequiela");
            s.Unit(0).Health = 2; s.Unit(1).Health = 3; s.Unit(2).Health = 0; s.Unit(3).Health = 2;
            Apply(s, "rest");
            Assert.That(new[] { s.Unit(0).Health, s.Unit(1).Health, s.Unit(2).Health, s.Unit(3).Health },
                Is.EqualTo(new[] { 3, 4, 0, 2 }));
            Assert.That(s.Apply(new AqCommand("rest")).Accepted, Is.False);
            Assert.That(s.Unit(0).Health, Is.EqualTo(3));
        }

        [TestCase(1)] [TestCase(17)] [TestCase(41)]
        public void RebirthPreventsOnlyTheFirstLethalHitAndDoesNotPayADeathReward(int firstSeed)
        {
            bool exercised = false;
            for (uint seed = (uint)firstSeed; seed < firstSeed + 16; seed++)
            {
                var s = Duel(seed, "teantha", "devon"); var defender = s.Unit(3);
                defender.Health = 1; s.Unit(0).Gear[0].Dice = 6;
                var first = Attack(s); if (Wounds(first, defender.Id) == 0) continue;
                exercised = true;
                Assert.That(defender.Health, Is.EqualTo(1)); Assert.That(defender.SpentLife, Is.True);
                Assert.That(defender.Deaths, Is.Zero); Assert.That(s.Guilds[0].Coins, Is.Zero);
                Assert.That(first.Effects.Any(e => e.Kind == "death" && e.Actor == defender.Id), Is.False);
                for (int attempts = 0; defender.Alive && attempts < 8; attempts++)
                { RefreshAttacker(s); Attack(s); }
                Assert.That(defender.Alive, Is.False); Assert.That(defender.Deaths, Is.EqualTo(1));
                int reward = s.Guilds[0].Coins;
                Assert.That(reward, Is.EqualTo(2), "One kill coin plus the first PvP quest bonus.");
                Assert.That(s.Apply(new AqCommand("attack", 0, defender.Id, 0)).Accepted, Is.False);
                Assert.That(s.Guilds[0].Coins, Is.EqualTo(reward)); Assert.That(defender.Deaths, Is.EqualTo(1));
            }
            Assert.That(exercised, Is.True);
        }

        [TestCase(false)] [TestCase(true)]
        public void ThornsReflectsExactlyOneWoundOnlyAgainstMeleeHits(bool ranged)
        {
            bool sawHit = false;
            for (uint seed = 1; seed <= 32; seed++)
            {
                var s = Duel(seed, "teantha", "raizor"); s.Unit(0).Gear[0].Ranged = ranged;
                var r = Attack(s); bool hit = Wounds(r, 3) > 0; sawHit |= hit;
                int reflected = hit && !ranged ? 1 : 0;
                Assert.That(Wounds(r, 0), Is.EqualTo(reflected));
                Assert.That(s.Unit(0).Health, Is.EqualTo(1000 - reflected));
            }
            Assert.That(sawHit, Is.True);
        }

        [Test]
        public void ChainReachesTwoSpacesButCannotPassAClosedDoorOrReachThree()
        {
            var s = Duel(29, "domador-de-correntes"); var a = s.Unit(0); var b = s.Unit(3);
            b.Cell = new AqCell(5, 4);
            Assert.That(s.CanTarget(a, b, a.Gear[0]), Is.True);
            b.Cell = new AqCell(6, 4);
            Assert.That(s.Apply(new AqCommand("attack", a.Id, b.Id, 0)).Accepted, Is.False);
            b.Cell = new AqCell(5, 4);
            s.Board.SetEdge(a.Cell, new AqCell(4, 4), true, false);
            Assert.That(s.Apply(new AqCommand("attack", a.Id, b.Id, 0)).Accepted, Is.False);
            s.Board.Edges[0].Open = true;
            Assert.That(Attack(s).Effects.Any(e => e.Kind == "attack" && e.Actor == a.Id), Is.True);
        }

        [Test]
        public void ArsenalSwitchChangesActualTargetingAndTheAttackAnimation()
        {
            var ranged = Duel(29, "damian", "teantha"); ranged.Unit(3).Cell = new AqCell(6, 4);
            var pistol = Attack(ranged).Effects.Single(e => e.Kind == "attack" && e.Actor == 0);
            Assert.That(pistol.Clip, Is.EqualTo("pistol"));
            var melee = Duel(29, "damian", "teantha"); melee.Unit(3).Cell = new AqCell(6, 4);
            Apply(melee, "stance", 0);
            Assert.That(melee.Apply(new AqCommand("attack", 0, 3, 0)).Accepted, Is.False);
            melee.Unit(3).Cell = new AqCell(4, 4);
            var hammer = Attack(melee).Effects.Single(e => e.Kind == "attack" && e.Actor == 0);
            Assert.That(hammer.Clip, Is.EqualTo("hammer"));
        }

        [TestCase(false)] [TestCase(true)]
        public void MortalVengeanceSurvivesUntilResolvedButOverkillPaysImmediatelyOnlyOnce(bool overkill)
        {
            bool sawKill = false;
            for (uint seed = 1; seed <= 32; seed++)
            {
                var s = Duel(seed); var m = s.Units.First(u => u.Monster);
                s.Unit(3).Cell = new AqCell(7, 7); s.Unit(0).Gear[0].Dice = 6;
                m.Cell = new AqCell(4, 4); m.Health = 1; m.Defense = 0; m.Ability = "";
                m.Overkill = overkill ? 1 : 100; m.Reward = 3;
                var attack = Attack(s, m.Id); if (m.Alive) continue; sawKill = true;
                int deaths = attack.Effects.Count(e => e.Kind == "death" && e.Actor == m.Id);
                if (overkill) Assert.That(s.Reaction, Is.Null);
                else
                {
                    Assert.That(s.Reaction, Is.Not.Null); Assert.That(s.Reaction.Dying, Is.True);
                    Assert.That(s.Guilds[0].Coins, Is.Zero, "The dying monster still gets its reaction.");
                    var reaction = Apply(s, "react-attack");
                    Assert.That(reaction.Effects.Any(e => e.Kind == "attack" && e.Actor == m.Id), Is.True);
                    deaths += reaction.Effects.Count(e => e.Kind == "death" && e.Actor == m.Id);
                }
                Assert.That(deaths, Is.EqualTo(1)); Assert.That(s.Guilds[0].Coins, Is.EqualTo(3));
                Assert.That(s.SpawnQueue.Count(id => id == m.Id), Is.EqualTo(1));
                Assert.That(s.Apply(new AqCommand("react-skip")).Accepted, Is.False);
                Assert.That(s.Guilds[0].Coins, Is.EqualTo(3));
            }
            Assert.That(sawKill, Is.True);
        }

        [Test]
        public void AMonsterKilledByThornsDuringGuardPaysOneRewardAndEntersTheQueueOnce()
        {
            bool reflectedKill = false;
            for (uint seed = 1; seed <= 48; seed++)
            {
                var s = Duel(seed, "raizor"); var m = s.Units.First(u => u.Monster);
                s.Unit(3).Cell = new AqCell(7, 7);
                m.Health = 1; m.Dice = 6; m.Ranged = false; m.Cell = new AqCell(4, 4); m.Reward = 2;
                Apply(s, "move", 0, x: 2, y: 4);
                Assert.That(s.Reaction, Is.Not.Null); Assert.That(s.Reaction.Vengeance, Is.False);
                var reaction = Apply(s, "react-attack"); if (m.Alive) continue; reflectedKill = true;
                Assert.That(Wounds(reaction, m.Id), Is.EqualTo(1));
                Assert.That(reaction.Effects.Count(e => e.Kind == "death" && e.Actor == m.Id), Is.EqualTo(1));
                Assert.That(s.Guilds[0].Coins, Is.EqualTo(2));
                Assert.That(s.SpawnQueue.Count(id => id == m.Id), Is.EqualTo(1));
                Assert.That(s.Apply(new AqCommand("react-attack")).Accepted, Is.False);
                Assert.That(s.Guilds[0].Coins, Is.EqualTo(2));
            }
            Assert.That(reflectedKill, Is.True);
        }
    }
}
