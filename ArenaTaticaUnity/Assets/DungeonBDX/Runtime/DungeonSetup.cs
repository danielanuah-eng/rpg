using System;
using System.Collections.Generic;

namespace DungeonBDX
{
    [Flags]
    public enum CampaignMechanics { None = 0, Objectives = 1, Traps = 2, FriendlyFire = 4, DragonEncounters = 8 }

    // Setup only. Campaign execution and persistence are separate pipeline stages.
    public sealed class DungeonSetup
    {
        readonly List<string> party = new List<string>();
        readonly HashSet<string> available;
        public int PartyCapacity { get; }
        public IReadOnlyList<string> Party => party.AsReadOnly();
        public CampaignMechanics Mechanics { get; private set; } = CampaignMechanics.Objectives | CampaignMechanics.Traps;

        public DungeonSetup(IEnumerable<DungeonHero> heroes, int partyCapacity = 3)
        {
            if (partyCapacity < 1 || partyCapacity > 6) throw new ArgumentOutOfRangeException(nameof(partyCapacity));
            PartyCapacity = partyCapacity;
            available = new HashSet<string>();
            foreach (var hero in heroes) available.Add(hero.Id);
        }

        public bool ToggleHero(string id)
        {
            if (string.IsNullOrEmpty(id) || !available.Contains(id)) return false;
            if (party.Remove(id)) return true;
            if (party.Count >= PartyCapacity) return false;
            party.Add(id); return true;
        }

        public void SetMechanic(CampaignMechanics mechanic, bool enabled)
        {
            const CampaignMechanics supported = CampaignMechanics.Objectives | CampaignMechanics.Traps |
                CampaignMechanics.FriendlyFire | CampaignMechanics.DragonEncounters;
            if (mechanic == CampaignMechanics.None || (mechanic & ~supported) != 0)
                throw new ArgumentOutOfRangeException(nameof(mechanic));
            Mechanics = enabled ? Mechanics | mechanic : Mechanics & ~mechanic;
        }

        public bool HasMechanic(CampaignMechanics mechanic) => mechanic != CampaignMechanics.None && (Mechanics & mechanic) == mechanic;
    }
}
