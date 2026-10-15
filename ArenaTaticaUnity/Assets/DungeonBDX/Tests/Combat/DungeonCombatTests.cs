using System;
using System.Linq;
using DungeonBDX.Combat;
using NUnit.Framework;

namespace DungeonBDX.Tests.Combat
{
    public sealed class DungeonCombatTests
    {
        sealed class Faces : IDieRoller
        {
            readonly int[] faces; int index;
            public Faces(params int[] faces) { this.faces = faces; }
            public int NextDie() => faces[index++ % faces.Length];
        }

        [Test]
        public void CriticalsNeverGenerateAnotherRollEvenAtExtremeInputs()
        {
            var roll = CombatMath.RollAttack(int.MaxValue, int.MaxValue, new Faces(6));
            Assert.That(roll.Dice, Is.EqualTo(6)); Assert.That(roll.Rolls, Is.EqualTo(6));
            Assert.That(roll.Hits, Is.EqualTo(6)); Assert.That(roll.Criticals, Is.EqualTo(6));
            Assert.That(roll.Points, Is.EqualTo(12));
        }

        [Test]
        public void MissCanRerollOnlyOnceAndCannotTriggerOnAHit()
        {
            var missed = CombatMath.RollAttack(6, 100, new Faces(1));
            Assert.That(missed.Rolls, Is.EqualTo(12)); Assert.That(missed.Hits, Is.Zero);
            var replaced = CombatMath.RollAttack(2, 1, new Faces(1, 6, 3));
            Assert.That(replaced.Rolls, Is.EqualTo(3)); Assert.That(replaced.Points, Is.EqualTo(3));
            Assert.That(replaced.Criticals, Is.EqualTo(1));
        }

        [Test]
        public void ExhaustiveSingleDieProbabilitiesAndExpectedDamageAreExact()
        {
            int hits = 0, crits = 0, rawSum = 0;
            for (int face = 1; face <= 6; face++)
            {
                var roll = CombatMath.RollAttack(1, 0, new Faces(face));
                hits += roll.Hits; crits += roll.Criticals; rawSum += CombatMath.RawDamage(roll, 6);
            }
            Assert.That(hits, Is.EqualTo(4)); Assert.That(crits, Is.EqualTo(1)); Assert.That(rawSum, Is.EqualTo(29));
            hits = crits = rawSum = 0;
            for (int first = 1; first <= 6; first++) for (int second = 1; second <= 6; second++)
            {
                var roll = CombatMath.RollAttack(1, 1, new Faces(first, second));
                hits += roll.Hits; crits += roll.Criticals; rawSum += CombatMath.RawDamage(roll, 6);
            }
            Assert.That(hits, Is.EqualTo(32)); Assert.That(crits, Is.EqualTo(8)); Assert.That(rawSum, Is.EqualTo(232));
        }

        [Test]
        public void MissStaysZeroDespiteExtremePositiveBuffsAndArmorFloor()
        {
            var miss = CombatMath.RollAttack(1, 0, new Faces(1));
            var mods = DamageModifiers.Combine(new[] { new DamageModifier(int.MaxValue, int.MaxValue) });
            Assert.That(CombatMath.RawDamage(miss, int.MaxValue, mods), Is.Zero);
            Assert.That(CombatMath.DamageAfterArmor(0, long.MaxValue), Is.Zero);
        }

        [Test]
        public void ModifiersAreSummedOnceAndAreIndependentOfOrder()
        {
            var input = new[] { new DamageModifier(int.MaxValue, int.MaxValue),
                new DamageModifier(int.MinValue, int.MinValue), new DamageModifier(3, 201) };
            var forward = DamageModifiers.Combine(input); var reverse = DamageModifiers.Combine(input.Reverse());
            Assert.That(forward.Flat, Is.EqualTo(2)); Assert.That(forward.BonusBasisPoints, Is.EqualTo(200));
            Assert.That(reverse.Flat, Is.EqualTo(forward.Flat)); Assert.That(reverse.BonusBasisPoints, Is.EqualTo(forward.BonusBasisPoints));
            var extreme = DamageModifiers.Combine(Enumerable.Repeat(new DamageModifier(int.MaxValue, int.MaxValue), 16));
            Assert.That(extreme.Flat, Is.EqualTo(8)); Assert.That(extreme.BonusBasisPoints, Is.EqualTo(5000));
            Assert.Throws<ArgumentException>(() => DamageModifiers.Combine(Enumerable.Repeat(new DamageModifier(1), 17)));
        }

        [TestCase(long.MinValue, 0)] [TestCase(0, 0)] [TestCase(50, 5000)]
        [TestCase(150, 6000)] [TestCase(long.MaxValue, 6000)]
        public void ArmorHasSoftHardAndGlobalCaps(long armor, int expected)
        { Assert.That(CombatMath.Mitigation(armor), Is.EqualTo(expected)); }

        [Test]
        public void ArmorBeyondSoftCapHasLessMarginalEffect()
        {
            Assert.That(CombatMath.Mitigation(51) - CombatMath.Mitigation(50),
                Is.LessThan(CombatMath.Mitigation(50) - CombatMath.Mitigation(49)));
        }

        [Test]
        public void ArmorAndPenetrationAreMonotoneAndNeverCreateInvulnerability()
        {
            for (int raw = 1; raw <= 30; raw++)
            {
                int previous = raw;
                for (int armor = 0; armor <= 300; armor++)
                {
                    int damage = CombatMath.DamageAfterArmor(raw, armor);
                    Assert.That(damage, Is.InRange(1, previous)); previous = damage;
                }
                previous = 0;
                for (int pen = 0; pen <= 180; pen++)
                {
                    int damage = CombatMath.DamageAfterArmor(raw, 150, pen);
                    Assert.That(damage, Is.InRange(previous, raw)); previous = damage;
                }
            }
        }

        [Test]
        public void ExtremeArmorResistanceAndPowerCannotOverflowOrMultiplyCaps()
        {
            Assert.That(CombatMath.Mitigation(long.MaxValue, int.MinValue, int.MaxValue), Is.EqualTo(6000));
            Assert.That(CombatMath.Mitigation(long.MaxValue, int.MaxValue, int.MinValue), Is.Zero);
            Assert.That(CombatMath.DamageAfterArmor(int.MaxValue, long.MaxValue, 0, int.MaxValue), Is.EqualTo(12));
            var roll = CombatMath.RollAttack(6, 1, new Faces(6));
            var mods = DamageModifiers.Combine(new[] { new DamageModifier(int.MaxValue, int.MaxValue) });
            Assert.That(CombatMath.RawDamage(roll, int.MaxValue, mods), Is.EqualTo(30));
            Assert.That(CombatMath.RawDamage(roll, int.MinValue,
                DamageModifiers.Combine(new[] { new DamageModifier(int.MinValue, int.MinValue) })), Is.EqualTo(2));
        }

        [TestCase(0)] [TestCase(7)]
        public void InvalidDieSourceIsRejected(int face)
        { Assert.Throws<InvalidOperationException>(() => CombatMath.RollAttack(1, 0, new Faces(face))); }

        [Test]
        public void SeedZeroHasKnownSplitMix64VectorAndResumeableState()
        {
            var random = new DungeonRandom(0);
            uint[] expected = { 0xE220A839u, 0x6E789E6Au, 0x06C45D18u, 0xF88BB8A8u, 0x1B39896Au };
            foreach (uint value in expected) Assert.That(random.NextUInt(), Is.EqualTo(value));
            var resumed = new DungeonRandom(random.State);
            for (int i = 0; i < 1000; i++) Assert.That(resumed.NextDie(), Is.EqualTo(random.NextDie()));
        }

        [Test]
        public void SameSeedAlwaysProducesTheSameRollsWithinBoundedWork()
        {
            var a = new DungeonRandom(ulong.MaxValue); var b = new DungeonRandom(ulong.MaxValue);
            int[] faces = new int[6];
            for (int i = 0; i < 120000; i++)
            {
                int face = a.NextDie(); Assert.That(face, Is.EqualTo(b.NextDie()).And.InRange(1, 6)); faces[face - 1]++;
            }
            foreach (int count in faces) Assert.That(count, Is.InRange(19400, 20600));
        }
    }
}
