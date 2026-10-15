using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DungeonBDX.Skills
{
    [Flags]
    public enum SkillEffect { Attack = 1, Heal = 2, Shield = 4, Aim = 8, Move = 16, Slow = 32, Summon = 64, Recall = 128, Armor = 256 }
    public enum SkillTarget { Enemy, Ally, Self, Destination }
    [Flags]
    public enum SkillMechanics { None = 0, Companions = 1, Control = 2, Teleport = 4, FriendlyFire = 8, Default = Companions | Control | Teleport }

    public sealed class DungeonSkill
    {
        public string Id { get; }
        public string Name { get; }
        public SkillEffect Effects { get; }
        public SkillTarget Target { get; }
        public int Cost { get; }
        public int Cooldown { get; }
        public int MinRange { get; }
        public int Range { get; }
        public int MaxTargets { get; }
        public int AreaRadius { get; }
        public int Dice { get; }
        public int Power { get; }
        public int Penetration { get; }
        public int Healing { get; }
        public int Shield { get; }
        public int Steps { get; }
        public int Duration { get; }
        public int UsesPerTurn { get; }
        public bool RequiresPet { get; }
        public bool Teleport { get; }
        public string AnimationId => "dbdx.anim." + Id.Substring(5);
        public string SoundId => "dbdx.sfx." + Id.Substring(5);
        public string IconId => "dbdx.icon." + Id.Substring(5);

        internal DungeonSkill(string id, string name, SkillEffect effects, SkillTarget target, int cost,
            int cooldown = 0, int minRange = 0, int range = 0, int maxTargets = 1, int areaRadius = 0,
            int dice = 0, int power = 0, int penetration = 0, int healing = 0, int shield = 0,
            int steps = 0, int duration = 0, int uses = 1, bool requiresPet = false, bool teleport = false)
        {
            Id = "dbdx." + id; Name = name; Effects = effects; Target = target; Cost = cost; Cooldown = cooldown;
            MinRange = minRange; Range = range; MaxTargets = maxTargets; AreaRadius = areaRadius;
            Dice = dice; Power = power; Penetration = penetration; Healing = healing; Shield = shield;
            Steps = steps; Duration = duration; UsesPerTurn = uses; RequiresPet = requiresPet; Teleport = teleport;
        }
        public bool Has(SkillEffect effect) => (Effects & effect) != 0;
    }

    public static class DungeonSkillCatalog
    {
        public const string Version = "1.0.0";
        static readonly ReadOnlyDictionary<string, DungeonSkill> skills = Build();
        public static IReadOnlyDictionary<string, DungeonSkill> All => skills;
        public static bool TryGet(string id, out DungeonSkill skill)
        { skill = null; return id != null && skills.TryGetValue(id, out skill); }

        static ReadOnlyDictionary<string, DungeonSkill> Build()
        {
            var entries = new[]
            {
                new DungeonSkill("basico.golpe", "Golpe firme", SkillEffect.Attack, SkillTarget.Enemy, 1, minRange:1, range:1, dice:3, power:4, uses:3),
                new DungeonSkill("basico.avanco", "Avanço protegido", SkillEffect.Move | SkillEffect.Shield, SkillTarget.Destination, 1, cooldown:1, range:2, steps:2, shield:4, duration:1),
                new DungeonSkill("basico.revigorar", "Revigorar", SkillEffect.Heal, SkillTarget.Ally, 2, cooldown:2, range:2, healing:10),
                new DungeonSkill("basico.baluarte", "Baluarte", SkillEffect.Shield | SkillEffect.Armor, SkillTarget.Self, 1, cooldown:2, shield:8, duration:1),
                new DungeonSkill("pistola.duplo", "Disparo duplo", SkillEffect.Attack, SkillTarget.Enemy, 1, minRange:1, range:5, dice:2, power:4, uses:2),
                new DungeonSkill("pistola.ricochete", "Ricochete", SkillEffect.Attack, SkillTarget.Enemy, 2, cooldown:1, minRange:2, range:5, maxTargets:2, areaRadius:1, dice:2, power:3),
                new DungeonSkill("pistola.mira", "Mira estável", SkillEffect.Aim, SkillTarget.Self, 1, cooldown:2, duration:1),
                new DungeonSkill("pistola.ruptura", "Tiro de ruptura", SkillEffect.Attack, SkillTarget.Enemy, 2, cooldown:2, minRange:1, range:4, dice:3, power:7, penetration:20),
                new DungeonSkill("arcano.lanca", "Lança prismática", SkillEffect.Attack, SkillTarget.Enemy, 1, minRange:1, range:4, dice:2, power:5, uses:3),
                new DungeonSkill("arcano.geada", "Geada convergente", SkillEffect.Attack | SkillEffect.Slow, SkillTarget.Enemy, 2, cooldown:1, minRange:1, range:3, maxTargets:3, areaRadius:1, dice:2, power:2, duration:1),
                new DungeonSkill("arcano.egide", "Égide arcana", SkillEffect.Shield, SkillTarget.Ally, 1, cooldown:1, range:3, shield:8, duration:1),
                new DungeonSkill("arcano.transposicao", "Transposição", SkillEffect.Move, SkillTarget.Destination, 1, cooldown:2, range:2, steps:2, teleport:true),
                new DungeonSkill("arcano.pulso", "Pulso vital", SkillEffect.Heal, SkillTarget.Ally, 2, cooldown:2, range:3, maxTargets:2, areaRadius:1, healing:8),
                new DungeonSkill("draco.invocar", "Vínculo dracônico", SkillEffect.Summon, SkillTarget.Destination, 2, cooldown:2, minRange:1, range:1),
                new DungeonSkill("draco.comandar", "Comando de garra", SkillEffect.Attack, SkillTarget.Enemy, 1, minRange:1, range:4, dice:2, power:3, uses:2, requiresPet:true),
                new DungeonSkill("draco.sopro", "Sopro em leque", SkillEffect.Attack, SkillTarget.Enemy, 2, cooldown:1, minRange:1, range:3, maxTargets:3, areaRadius:1, dice:3, power:6, requiresPet:true),
                new DungeonSkill("draco.pacto", "Pacto protetor", SkillEffect.Shield | SkillEffect.Heal, SkillTarget.Ally, 1, cooldown:2, range:3, shield:6, healing:4, duration:1),
                new DungeonSkill("draco.recolher", "Recolher vínculo", SkillEffect.Recall | SkillEffect.Heal, SkillTarget.Self, 1, cooldown:1, healing:6, requiresPet:true)
            };
            return new ReadOnlyDictionary<string, DungeonSkill>(entries.ToDictionary(skill => skill.Id, StringComparer.Ordinal));
        }
    }
}
