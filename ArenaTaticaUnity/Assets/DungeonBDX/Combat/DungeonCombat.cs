using System;
using System.Collections.Generic;

namespace DungeonBDX.Combat
{
    public static class CombatCaps
    {
        public const string Version = "1.0.0";
        public const int Basis = 10000;
        public const int Dice = 6;
        public const int Rerolls = 1;
        public const int BasePower = 12;
        public const int FlatDamage = 8;
        public const int DamageBonus = 5000;
        public const int ModifierCount = 16;
        public const int ArmorSoft = 50;
        public const int ArmorHard = 150;
        public const int Mitigation = 6000;
        public const int Actions = 3;
        public const int DamagePerAction = 30;
        public const int DamagePerTurn = 60;
        public const int HealingPerAction = 12;
        public const int HealingPerTurn = 18;
        public const int ShieldPerAction = 8;
        public const int ShieldPerTurn = 12;
        public const int ShieldCapacity = 12;
        public const int HealthCapacity = 200;

        public static int Clamp(long value, int min, int max) =>
            value < min ? min : value > max ? max : (int)value;
    }

    public interface IDieRoller { int NextDie(); }

    // SplitMix64: unchecked integer arithmetic gives identical streams across Unity platforms.
    public sealed class DungeonRandom : IDieRoller
    {
        ulong state;
        public ulong State => state;
        public DungeonRandom(ulong seed) { state = seed; }
        public uint NextUInt()
        {
            unchecked
            {
                ulong z = state += 0x9E3779B97F4A7C15UL;
                z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                return (uint)((z ^ (z >> 31)) >> 32);
            }
        }
        public int NextDie()
        {
            // Reject the four excess uint values, rather than biasing face probabilities.
            uint value;
            do { value = NextUInt(); } while (value < 4u);
            return (int)(value % 6u) + 1;
        }
    }

    public readonly struct DamageModifier
    {
        public int Flat { get; }
        public int BonusBasisPoints { get; }
        public DamageModifier(int flat, int bonusBasisPoints = 0)
        { Flat = flat; BonusBasisPoints = bonusBasisPoints; }
    }

    public readonly struct DamageModifiers
    {
        public int Flat { get; }
        public int BonusBasisPoints { get; }
        DamageModifiers(long flat, long bonus)
        {
            Flat = CombatCaps.Clamp(flat, -CombatCaps.FlatDamage, CombatCaps.FlatDamage);
            BonusBasisPoints = CombatCaps.Clamp(bonus, -CombatCaps.DamageBonus, CombatCaps.DamageBonus);
        }
        public static DamageModifiers Combine(IEnumerable<DamageModifier> source)
        {
            if (source == null) return default;
            long flat = 0, bonus = 0; int count = 0;
            foreach (var modifier in source)
            {
                if (++count > CombatCaps.ModifierCount) throw new ArgumentException("Too many damage modifiers.", nameof(source));
                flat += modifier.Flat; bonus += modifier.BonusBasisPoints;
            }
            // Clamp after summing, preserving order independence even for opposing modifiers.
            return new DamageModifiers(flat, bonus);
        }
    }

    public readonly struct AttackRoll
    {
        public int Dice { get; }
        public int Rolls { get; }
        public int Hits { get; }
        public int Criticals { get; }
        public int Points => Hits + Criticals; // Every critical is already one hit; it adds exactly one point.
        internal AttackRoll(int dice, int rolls, int hits, int criticals)
        { Dice = dice; Rolls = rolls; Hits = hits; Criticals = criticals; }
    }

    public readonly struct DamageResolution
    {
        public int RequestedRaw { get; }
        public int GrantedRaw { get; }
        public int MitigationBasisPoints { get; }
        public int Damage { get; }
        internal DamageResolution(int requested, int granted, int mitigation, int damage)
        { RequestedRaw = requested; GrantedRaw = granted; MitigationBasisPoints = mitigation; Damage = damage; }
    }

    public static class CombatMath
    {
        public static AttackRoll RollAttack(int dice, int rerolls, IDieRoller random)
        {
            if (random == null) throw new ArgumentNullException(nameof(random));
            dice = CombatCaps.Clamp(dice, 1, CombatCaps.Dice);
            rerolls = CombatCaps.Clamp(rerolls, 0, CombatCaps.Rerolls);
            int hits = 0, criticals = 0, rolls = 0;
            for (int i = 0; i < dice; i++)
            {
                int face = Face(random); rolls++;
                if (face <= 2 && rerolls == 1) { face = Face(random); rolls++; }
                if (face >= 3) hits++;
                if (face == 6) criticals++;
            }
            return new AttackRoll(dice, rolls, hits, criticals);
        }
        static int Face(IDieRoller random)
        {
            int face = random.NextDie();
            if (face < 1 || face > 6) throw new InvalidOperationException("Die source returned a face outside 1..6.");
            return face;
        }

        public static int RawDamage(AttackRoll roll, int basePower, DamageModifiers modifiers = default)
        {
            if (roll.Hits == 0) return 0;
            int sum = Math.Max(0, CombatCaps.Clamp(basePower, 0, CombatCaps.BasePower) + roll.Points + modifiers.Flat);
            long scaled = (long)sum * (CombatCaps.Basis + modifiers.BonusBasisPoints) / CombatCaps.Basis;
            return CombatCaps.Clamp(scaled, 0, CombatCaps.DamagePerAction);
        }

        public static int Mitigation(long armor, int penetration = 0, int resistanceBasisPoints = 0)
        {
            int bounded = CombatCaps.Clamp(armor, 0, CombatCaps.ArmorHard);
            bounded = Math.Max(0, bounded - CombatCaps.Clamp(penetration, 0, CombatCaps.ArmorHard));
            // Effective armor in quarter units: above the soft cap, each point has 1/4 weight.
            int quarters = bounded <= CombatCaps.ArmorSoft ? bounded * 4 :
                CombatCaps.ArmorSoft * 4 + bounded - CombatCaps.ArmorSoft;
            int armorMitigation = quarters * CombatCaps.Basis / (quarters + 200);
            return CombatCaps.Clamp((long)armorMitigation + resistanceBasisPoints, 0, CombatCaps.Mitigation);
        }

        public static int DamageAfterArmor(int rawDamage, long armor, int penetration = 0, int resistanceBasisPoints = 0)
        {
            int raw = CombatCaps.Clamp(rawDamage, 0, CombatCaps.DamagePerAction);
            if (raw == 0) return 0;
            int mitigation = Mitigation(armor, penetration, resistanceBasisPoints);
            return (raw * (CombatCaps.Basis - mitigation) + CombatCaps.Basis - 1) / CombatCaps.Basis;
        }

        public static DamageResolution Resolve(AttackRoll roll, int basePower, DamageModifiers modifiers,
            long armor, ActionBudget action, int penetration = 0, int resistanceBasisPoints = 0)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            int raw = RawDamage(roll, basePower, modifiers);
            int granted = action.Spend(CombatResource.Damage, raw);
            return new DamageResolution(raw, granted, Mitigation(armor, penetration, resistanceBasisPoints),
                DamageAfterArmor(granted, armor, penetration, resistanceBasisPoints));
        }
    }
}
