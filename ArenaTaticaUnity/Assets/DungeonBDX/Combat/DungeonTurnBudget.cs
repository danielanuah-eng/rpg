using System;

namespace DungeonBDX.Combat
{
    public enum CombatResource { Damage, Healing, Shield }

    // One instance per authoritative owner and turn. Pets, dots, AoE and reactions share it.
    // The game/server owns turn identity; constructing a new budget is not a client authorization.
    public sealed class TurnBudget
    {
        readonly int[] spent = new int[3];
        ActionBudget active;
        public int RemainingActions { get; private set; }
        public int ActionsStarted { get; private set; }
        public bool Ended { get; private set; }

        public TurnBudget(int actionPoints = CombatCaps.Actions)
        { RemainingActions = CombatCaps.Clamp(actionPoints, 0, CombatCaps.Actions); }

        public bool TryBeginAction(int cost, out ActionBudget action)
        {
            action = null;
            if (Ended || active != null || cost < 1 || cost > RemainingActions || ActionsStarted >= CombatCaps.Actions) return false;
            RemainingActions -= cost; ActionsStarted++;
            active = action = new ActionBudget(this);
            return true;
        }

        public int Spent(CombatResource resource) => spent[Index(resource)];
        public int Remaining(CombatResource resource) => Ended ? 0 : Math.Max(0, TurnCap(resource) - Spent(resource));
        internal bool IsActive(ActionBudget action) => !Ended && ReferenceEquals(active, action);
        internal void Charge(CombatResource resource, int amount) { spent[Index(resource)] += amount; }
        internal void Close(ActionBudget action) { if (ReferenceEquals(active, action)) active = null; }
        public void EndTurn() { active?.Dispose(); Ended = true; RemainingActions = 0; }

        internal static int Index(CombatResource resource)
        {
            int index = (int)resource;
            if (index < 0 || index > 2) throw new ArgumentOutOfRangeException(nameof(resource));
            return index;
        }
        internal static int TurnCap(CombatResource resource)
        {
            switch (resource)
            {
                case CombatResource.Damage: return CombatCaps.DamagePerTurn;
                case CombatResource.Healing: return CombatCaps.HealingPerTurn;
                case CombatResource.Shield: return CombatCaps.ShieldPerTurn;
                default: throw new ArgumentOutOfRangeException(nameof(resource));
            }
        }
        internal static int ActionCap(CombatResource resource)
        {
            switch (resource)
            {
                case CombatResource.Damage: return CombatCaps.DamagePerAction;
                case CombatResource.Healing: return CombatCaps.HealingPerAction;
                case CombatResource.Shield: return CombatCaps.ShieldPerAction;
                default: throw new ArgumentOutOfRangeException(nameof(resource));
            }
        }
    }

    public sealed class ActionBudget : IDisposable
    {
        readonly TurnBudget owner;
        readonly int[] spent = new int[3];
        public bool Closed { get; private set; }
        internal ActionBudget(TurnBudget owner) { this.owner = owner; }
        public int Spent(CombatResource resource) => spent[TurnBudget.Index(resource)];
        public int Remaining(CombatResource resource)
        {
            int actionRemaining = TurnBudget.ActionCap(resource) - Spent(resource);
            return Closed || !owner.IsActive(this) ? 0 : Math.Min(actionRemaining, owner.Remaining(resource));
        }
        public int Spend(CombatResource resource, int requested)
        {
            int amount = Math.Min(Math.Max(0, requested), Remaining(resource));
            spent[TurnBudget.Index(resource)] += amount;
            owner.Charge(resource, amount);
            return amount;
        }
        public void Dispose() { if (!Closed) { Closed = true; owner.Close(this); } }
    }
}
