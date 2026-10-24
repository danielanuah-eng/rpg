using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DungeonBDX.Combat;

namespace DungeonBDX.Skills
{
    public enum HeroArchetype { Vanguard, Skirmisher, Ranger, Gunslinger, Arcanist, DragonBinder, Support }

    public sealed class DungeonHeroProfile
    {
        public string Id { get; }
        public string Name { get; }
        public HeroArchetype Archetype { get; }
        public int Health { get; }
        public int Armor { get; }
        public int Movement { get; }
        public int Actions => CombatCaps.Actions;
        public bool Imported { get; }
        public int SourceHealth { get; }
        public int SourceArmor { get; }
        public int SourceMovement { get; }
        public IReadOnlyList<string> SourceSkillIds { get; }
        public IReadOnlyList<string> SkillIds { get; }
        internal DungeonHeroProfile(string id, string name, HeroArchetype archetype, int health, int armor,
            int movement, IEnumerable<string> skills, bool imported = false, int sourceHealth = 0,
            int sourceArmor = 0, int sourceMovement = 0, IEnumerable<string> sourceSkills = null)
        {
            Id = id; Name = name; Archetype = archetype; Health = health; Armor = armor; Movement = movement;
            Imported = imported; SourceHealth = sourceHealth; SourceArmor = sourceArmor; SourceMovement = sourceMovement;
            SourceSkillIds = Array.AsReadOnly((sourceSkills ?? Array.Empty<string>()).ToArray());
            SkillIds = Array.AsReadOnly(skills.Select(idValue => "dbdx." + idValue).ToArray());
            if (SkillIds.Count < 1 || SkillIds.Count > 6 || SkillIds.Distinct().Count() != SkillIds.Count ||
                SkillIds.Any(skill => !DungeonSkillCatalog.All.ContainsKey(skill))) throw new ArgumentException("Invalid profile loadout.");
        }
    }

    // Explicit adapter, never an in-place conversion of Content.Fighters or presentation snapshots.
    public static class DungeonHeroProfiles
    {
        public const string ConversionVersion = "arena-to-dbdx-1";
        static readonly ReadOnlyDictionary<string, HeroArchetype> sources = BuildSources();
        public static IReadOnlyDictionary<string, HeroArchetype> ArenaSources => sources;
        public static IReadOnlyList<DungeonHeroProfile> NewHeroes { get; } = Array.AsReadOnly(new[]
        {
            new DungeonHeroProfile("lyra-faisca", "Lyra Faísca", HeroArchetype.Gunslinger, 44, 10, 4, Loadout(HeroArchetype.Gunslinger)),
            new DungeonHeroProfile("orion-prisma", "Orion Prisma", HeroArchetype.Arcanist, 42, 8, 3, Loadout(HeroArchetype.Arcanist)),
            new DungeonHeroProfile("vaelis-vinculo", "Vaelis do Vínculo", HeroArchetype.DragonBinder, 48, 14, 3, Loadout(HeroArchetype.DragonBinder))
        });

        public static DungeonHeroProfile AdaptArena(string id, string name, int health, int armor, int movement,
            IEnumerable<string> sourceSkillIds = null)
        {
            if (id == null || !sources.TryGetValue(id, out var archetype)) throw new ArgumentException("Unknown Arena hero ID.", nameof(id));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Missing hero name.", nameof(name));
            var sourceSkills = (sourceSkillIds ?? Array.Empty<string>()).Take(17).ToArray();
            if (sourceSkills.Length > 16 || sourceSkills.Any(string.IsNullOrWhiteSpace)) throw new ArgumentException("Invalid source skills.", nameof(sourceSkillIds));
            int normalizedHealth = CombatCaps.Clamp(24L + 2L * health, 40, 60);
            int normalizedArmor = CombatCaps.Clamp(10L + 20L * armor + (health >= 14 ? 10 : 0), 0, 50);
            return new DungeonHeroProfile(id, name, archetype, normalizedHealth, normalizedArmor,
                CombatCaps.Clamp(movement, 2, 5), Loadout(archetype), true, health, armor, movement, sourceSkills);
        }

        static string[] Loadout(HeroArchetype archetype)
        {
            switch (archetype)
            {
                case HeroArchetype.Gunslinger: case HeroArchetype.Ranger:
                    return new[] { "pistola.duplo", "pistola.ricochete", "pistola.mira", "pistola.ruptura", "basico.golpe" };
                case HeroArchetype.Arcanist:
                    return new[] { "arcano.lanca", "arcano.geada", "arcano.egide", "arcano.transposicao", "arcano.pulso", "basico.golpe" };
                case HeroArchetype.DragonBinder:
                    return new[] { "draco.invocar", "draco.comandar", "draco.sopro", "draco.pacto", "draco.recolher", "basico.golpe" };
                case HeroArchetype.Support:
                    return new[] { "arcano.lanca", "arcano.egide", "arcano.pulso", "basico.revigorar", "basico.golpe" };
                case HeroArchetype.Skirmisher:
                    return new[] { "basico.golpe", "basico.avanco", "arcano.transposicao", "pistola.mira" };
                default:
                    return new[] { "basico.golpe", "basico.baluarte", "basico.avanco", "basico.revigorar" };
            }
        }

        static ReadOnlyDictionary<string, HeroArchetype> BuildSources()
        {
            var map = new Dictionary<string, HeroArchetype>(StringComparer.Ordinal);
            Add(map, HeroArchetype.DragonBinder, "ciantia", "ossyro", "nikola");
            Add(map, HeroArchetype.Gunslinger, "damian", "cheiro-pneu", "pistoleira");
            Add(map, HeroArchetype.Ranger, "arqueiro-jack", "nixira", "bifia");
            Add(map, HeroArchetype.Arcanist, "ezequiela", "principe-kellen", "domador-de-correntes", "edgar", "pele-negra", "ikaito", "espelhos", "desland", "nepturius");
            Add(map, HeroArchetype.Support, "asamyh", "baltazar", "alvyr", "lela", "pilis");
            Add(map, HeroArchetype.Skirmisher, "somnax", "teantha", "diantus", "perry", "vespa-cinzenta", "viuva-da-agulha", "gryvro", "gumbald", "daemion", "ellie", "espadachim");
            Add(map, HeroArchetype.Vanguard, "talyssa", "devon", "tchu-tcha", "colosso", "rei-bobett", "guardiao-de-musgo", "ogro-de-pedra", "raizor", "brumak", "ganso", "death-crimson");
            return new ReadOnlyDictionary<string, HeroArchetype>(map);
        }
        static void Add(Dictionary<string, HeroArchetype> map, HeroArchetype archetype, params string[] ids)
        { foreach (string id in ids) map.Add(id, archetype); }
    }
}
