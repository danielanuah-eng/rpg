using System;
using System.Collections.Generic;
using System.Linq;
using DungeonBDX.Combat;

namespace DungeonBDX.Skills
{
    public readonly struct GridCell : IEquatable<GridCell>
    {
        public int X { get; }
        public int Y { get; }
        public GridCell(int x, int y) { X = x; Y = y; }
        public bool Equals(GridCell other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is GridCell other && Equals(other);
        public override int GetHashCode() { unchecked { return X * 397 ^ Y; } }
        public static int Distance(GridCell a, GridCell b) => CombatCaps.Clamp(Math.Abs((long)a.X - b.X) + Math.Abs((long)a.Y - b.Y), 0, int.MaxValue);
    }

    public interface ISkillBoard
    {
        bool CanStand(GridCell cell);
        bool HasLineOfSight(GridCell from, GridCell to);
    }

    // Small integer test/prototype board. Stage 5 adapts the real procedural world to ISkillBoard.
    public sealed class DungeonSkillBoard : ISkillBoard
    {
        readonly int width, height;
        readonly HashSet<GridCell> walls;
        public DungeonSkillBoard(int width, int height, IEnumerable<GridCell> blocked = null)
        {
            if (width < 1 || width > 128 || height < 1 || height > 128) throw new ArgumentOutOfRangeException(nameof(width));
            this.width = width; this.height = height;
            walls = new HashSet<GridCell>();
            if (blocked != null) foreach (var cell in blocked)
            {
                if (walls.Count >= width * height) throw new ArgumentException("Too many blocked cells.", nameof(blocked));
                if (cell.X < 0 || cell.Y < 0 || cell.X >= width || cell.Y >= height) throw new ArgumentException("Wall outside board.", nameof(blocked));
                walls.Add(cell);
            }
        }
        public bool CanStand(GridCell cell) => cell.X >= 0 && cell.Y >= 0 && cell.X < width && cell.Y < height && !walls.Contains(cell);
        public bool HasLineOfSight(GridCell from, GridCell to)
        {
            if (!CanStand(from) || !CanStand(to)) return false;
            int x = from.X, y = from.Y, dx = Math.Abs(to.X - x), dy = Math.Abs(to.Y - y);
            int sx = x < to.X ? 1 : -1, sy = y < to.Y ? 1 : -1, error = dx - dy;
            while (x != to.X || y != to.Y)
            {
                int twice = 2 * error; bool moveX = twice > -dy, moveY = twice < dx;
                if (moveX && moveY && (!CanStand(new GridCell(x + sx, y)) || !CanStand(new GridCell(x, y + sy)))) return false;
                if (moveX) { error -= dy; x += sx; }
                if (moveY) { error += dx; y += sy; }
                if (!CanStand(new GridCell(x, y))) return false;
            }
            return true;
        }
    }

    public sealed class HeroSpawn
    {
        public string Id { get; }
        public int Team { get; }
        public DungeonHeroProfile Profile { get; }
        public GridCell Cell { get; }
        public HeroSpawn(string id, int team, DungeonHeroProfile profile, GridCell cell)
        { Id = id; Team = team; Profile = profile; Cell = cell; }
    }

    public sealed class UnitSnapshot
    {
        public string Id { get; }
        public string OwnerId { get; }
        public string ProfileId { get; }
        public int Team { get; }
        public int Health { get; }
        public int MaxHealth { get; }
        public int Shield { get; }
        public int Armor { get; }
        public int Slow { get; }
        public GridCell Cell { get; }
        public bool IsSummon => Id != OwnerId;
        public bool Alive => Health > 0;
        internal UnitSnapshot(SkillUnit unit)
        {
            Id = unit.Id; OwnerId = unit.Owner; ProfileId = unit.Profile?.Id ?? "dbdx.draco.companheiro";
            Team = unit.Team; Health = unit.Health; MaxHealth = unit.MaxHealth; Shield = unit.Shield;
            Armor = unit.Armor; Slow = unit.Slow; Cell = unit.Cell;
        }
    }

    sealed class SkillUnit
    {
        public string Id, Owner;
        public DungeonHeroProfile Profile;
        public int Team, Health, MaxHealth, BaseArmor, Shield, ArmorBonus, Slow;
        public GridCell Cell;
        public long OwnTurn, ShieldExpires, ArmorExpires, SlowExpires, AimExpires;
        public bool Aim;
        public readonly Dictionary<string, long> ReadyAt = new Dictionary<string, long>(StringComparer.Ordinal);
        public readonly Dictionary<string, int> Uses = new Dictionary<string, int>(StringComparer.Ordinal);
        public int Armor => CombatCaps.Clamp((long)BaseArmor + ArmorBonus, 0, CombatCaps.ArmorHard);
    }

    public enum SkillFailure { None, Finished, NotYourTurn, UnknownSkill, NotInLoadout, DisabledMechanic, Cooldown,
        UsesExhausted, NoActions, InvalidTarget, OutOfRange, BlockedSight, InvalidDestination, MissingPet, PetAlreadyPresent }

    public sealed class SkillImpact
    {
        public string UnitId { get; }
        public int HealthDelta { get; }
        public int ShieldDelta { get; }
        public int RequestedRaw { get; }
        public int GrantedRaw { get; }
        public int MitigationBasisPoints { get; }
        public int Damage { get; }
        internal SkillImpact(SkillUnit unit, int previousHealth, int previousShield, DamageResolution damage)
        {
            UnitId = unit.Id; HealthDelta = unit.Health - previousHealth; ShieldDelta = unit.Shield - previousShield;
            RequestedRaw = damage.RequestedRaw; GrantedRaw = damage.GrantedRaw;
            MitigationBasisPoints = damage.MitigationBasisPoints; Damage = damage.Damage;
        }
    }

    public sealed class SkillUseResult
    {
        public SkillFailure Failure { get; }
        public bool Success => Failure == SkillFailure.None;
        public string ActorId { get; }
        public string SkillId { get; }
        public string AnimationId { get; }
        public string SoundId { get; }
        public string SummonId { get; }
        public GridCell Origin { get; }
        public GridCell Destination { get; }
        public AttackRoll? Roll { get; }
        public IReadOnlyList<SkillImpact> Impacts { get; }
        internal SkillUseResult(SkillFailure failure, string actorId, DungeonSkill skill = null,
            IEnumerable<SkillImpact> impacts = null, AttackRoll? roll = null, string summonId = null,
            GridCell origin = default, GridCell destination = default)
        {
            Failure = failure; ActorId = actorId; SkillId = skill?.Id; AnimationId = skill?.AnimationId; SoundId = skill?.SoundId;
            SummonId = summonId; Origin = origin; Destination = destination; Roll = roll;
            Impacts = Array.AsReadOnly((impacts ?? Array.Empty<SkillImpact>()).ToArray());
        }
    }

    // Synchronous domain engine. The eventual authoritative host serializes/authenticates commands.
    // No login, Unity scene, networking, save format or player-presence gate lives here.
    public sealed class DungeonSkillSession
    {
        readonly Dictionary<string, SkillUnit> units = new Dictionary<string, SkillUnit>(StringComparer.Ordinal);
        readonly List<string> turnOrder = new List<string>();
        readonly ISkillBoard board;
        readonly IDieRoller random;
        readonly SkillMechanics mechanics;
        TurnBudget budget;
        int turnCursor = -1;
        public string ActiveActorId { get; private set; }
        public long TurnIndex { get; private set; }
        public bool Finished { get; private set; }
        public int RemainingActions => Finished ? 0 : budget?.RemainingActions ?? 0;
        public int DamageSpentThisTurn => budget?.Spent(CombatResource.Damage) ?? 0;
        public int HealingSpentThisTurn => budget?.Spent(CombatResource.Healing) ?? 0;
        public int ShieldSpentThisTurn => budget?.Spent(CombatResource.Shield) ?? 0;
        public ulong? RandomState => random is DungeonRandom seeded ? seeded.State : (ulong?)null;
        public IReadOnlyList<UnitSnapshot> Units => Array.AsReadOnly(units.Values.OrderBy(unit => unit.Id, StringComparer.Ordinal).Select(unit => new UnitSnapshot(unit)).ToArray());

        public DungeonSkillSession(IEnumerable<HeroSpawn> initial, ISkillBoard board, ulong seed = 1,
            SkillMechanics mechanics = SkillMechanics.Default, IDieRoller random = null)
        {
            this.board = board ?? throw new ArgumentNullException(nameof(board));
            this.random = random ?? new DungeonRandom(seed); this.mechanics = mechanics;
            if ((mechanics & ~(SkillMechanics.Default | SkillMechanics.FriendlyFire)) != 0) throw new ArgumentOutOfRangeException(nameof(mechanics));
            if (initial == null) throw new ArgumentNullException(nameof(initial));
            foreach (var spawn in initial)
            {
                if (turnOrder.Count >= 32 || spawn == null || spawn.Profile == null || string.IsNullOrEmpty(spawn.Id) ||
                    spawn.Id.Length > 48 || spawn.Id.Any(c => !char.IsLetterOrDigit(c) && c != '-' && c != '_') ||
                    spawn.Team < 0 || spawn.Team > 7 || !IsVacant(spawn.Cell) || units.ContainsKey(spawn.Id))
                    throw new ArgumentException("Invalid or overlapping hero spawn.", nameof(initial));
                int hp = CombatCaps.Clamp(spawn.Profile.Health, 1, CombatCaps.HealthCapacity);
                units.Add(spawn.Id, new SkillUnit { Id = spawn.Id, Owner = spawn.Id, Profile = spawn.Profile,
                    Team = spawn.Team, Health = hp, MaxHealth = hp, BaseArmor = spawn.Profile.Armor, Cell = spawn.Cell });
                turnOrder.Add(spawn.Id);
            }
            if (turnOrder.Count == 0) throw new ArgumentException("No heroes.", nameof(initial));
            AdvanceTurn();
        }

        public UnitSnapshot Unit(string id) => id != null && units.TryGetValue(id, out var unit) ? new UnitSnapshot(unit) : null;
        public string PetId(string ownerId) => ownerId != null && units.TryGetValue("pet:" + ownerId, out var pet) && pet.Health > 0 ? pet.Id : null;
        bool Enabled(SkillMechanics flag) => (mechanics & flag) != 0;
        bool IsVacant(GridCell cell) => board.CanStand(cell) && !units.Values.Any(unit => unit.Health > 0 && unit.Cell.Equals(cell));

        public bool EndTurn(string actorId)
        {
            if (Finished || actorId != ActiveActorId) return false;
            budget.EndTurn(); AdvanceTurn(); return true;
        }
        void AdvanceTurn()
        {
            if (turnOrder.Where(id => units[id].Health > 0).Select(id => units[id].Team).Distinct().Count() <= 1)
            { Finished = true; ActiveActorId = null; budget?.EndTurn(); return; }
            for (int i = 0; i < turnOrder.Count; i++)
            {
                turnCursor = (turnCursor + 1) % turnOrder.Count; var actor = units[turnOrder[turnCursor]];
                if (actor.Health <= 0) continue;
                ActiveActorId = actor.Id; actor.OwnTurn = checked(actor.OwnTurn + 1); TurnIndex = checked(TurnIndex + 1);
                actor.Uses.Clear(); budget = new TurnBudget();
                foreach (var owned in units.Values.Where(unit => unit.Owner == actor.Id))
                {
                    if (actor.OwnTurn >= owned.ShieldExpires) owned.Shield = 0;
                    if (actor.OwnTurn >= owned.ArmorExpires) owned.ArmorBonus = 0;
                    if (actor.OwnTurn >= owned.SlowExpires) owned.Slow = 0;
                    if (actor.OwnTurn >= owned.AimExpires) owned.Aim = false;
                }
                return;
            }
        }

        public SkillUseResult Cast(string actorId, string skillId, IReadOnlyList<string> targetIds = null, GridCell? destination = null)
        {
            SkillUseResult Fail(SkillFailure reason, DungeonSkill known = null) => new SkillUseResult(reason, actorId, known);
            if (Finished) return Fail(SkillFailure.Finished);
            if (actorId != ActiveActorId) return Fail(SkillFailure.NotYourTurn);
            if (!DungeonSkillCatalog.TryGet(skillId, out var skill)) return Fail(SkillFailure.UnknownSkill);
            var actor = units[actorId];
            if (!actor.Profile.SkillIds.Contains(skillId)) return Fail(SkillFailure.NotInLoadout, skill);
            if ((skill.Has(SkillEffect.Summon) || skill.RequiresPet) && !Enabled(SkillMechanics.Companions) ||
                skill.Teleport && !Enabled(SkillMechanics.Teleport)) return Fail(SkillFailure.DisabledMechanic, skill);
            if (actor.ReadyAt.TryGetValue(skillId, out long ready) && ready > actor.OwnTurn) return Fail(SkillFailure.Cooldown, skill);
            if (actor.Uses.TryGetValue(skillId, out int uses) && uses >= skill.UsesPerTurn) return Fail(SkillFailure.UsesExhausted, skill);
            if (RemainingActions < skill.Cost) return Fail(SkillFailure.NoActions, skill);
            var pet = PetId(actorId) is string petId ? units[petId] : null;
            if (skill.RequiresPet && pet == null) return Fail(SkillFailure.MissingPet, skill);
            if (skill.Has(SkillEffect.Summon) && pet != null) return Fail(SkillFailure.PetAlreadyPresent, skill);
            var origin = skill.RequiresPet && skill.Has(SkillEffect.Attack) ? pet.Cell : actor.Cell;
            var focus = actor.Cell;
            var targets = new List<SkillUnit>();

            if (skill.Target == SkillTarget.Destination)
            {
                if (targetIds != null && targetIds.Count != 0 || !destination.HasValue || !IsVacant(destination.Value)) return Fail(SkillFailure.InvalidDestination, skill);
                int distance = GridCell.Distance(actor.Cell, destination.Value);
                if (distance < Math.Max(1, skill.MinRange) || distance > skill.Range ||
                    skill.Has(SkillEffect.Move) && !skill.Teleport && !CanWalk(actor.Cell, destination.Value, Math.Max(1, skill.Steps - actor.Slow)))
                    return Fail(SkillFailure.InvalidDestination, skill);
                targets.Add(actor);
            }
            else if (skill.Target == SkillTarget.Self)
            {
                if (destination.HasValue || targetIds != null && targetIds.Count > 0 && (targetIds.Count != 1 || targetIds[0] != actorId)) return Fail(SkillFailure.InvalidTarget, skill);
                targets.Add(actor);
            }
            else
            {
                if (destination.HasValue || targetIds == null || targetIds.Count < 1 || targetIds.Count > skill.MaxTargets ||
                    targetIds.Any(string.IsNullOrEmpty) || targetIds.Distinct(StringComparer.Ordinal).Count() != targetIds.Count) return Fail(SkillFailure.InvalidTarget, skill);
                if (!units.TryGetValue(targetIds[0], out var anchor)) return Fail(SkillFailure.InvalidTarget, skill);
                focus = anchor.Cell; // The selected center is independent of budget iteration order.
                foreach (string id in targetIds.OrderBy(id => id, StringComparer.Ordinal))
                {
                    if (!units.TryGetValue(id, out var target) || target.Health <= 0 ||
                        skill.Target == SkillTarget.Ally && target.Team != actor.Team ||
                        skill.Target == SkillTarget.Enemy && target.Team == actor.Team && !Enabled(SkillMechanics.FriendlyFire)) return Fail(SkillFailure.InvalidTarget, skill);
                    int distance = GridCell.Distance(origin, target.Cell);
                    if (distance < skill.MinRange || distance > skill.Range) return Fail(SkillFailure.OutOfRange, skill);
                    if (!board.HasLineOfSight(origin, target.Cell)) return Fail(SkillFailure.BlockedSight, skill);
                    if (GridCell.Distance(focus, target.Cell) > skill.AreaRadius) return Fail(SkillFailure.InvalidTarget, skill);
                    targets.Add(target);
                }
            }
            if (!budget.TryBeginAction(skill.Cost, out var action)) return Fail(SkillFailure.NoActions, skill);
            actor.Uses[skillId] = uses + 1;
            actor.ReadyAt[skillId] = checked(actor.OwnTurn + (skill.Cooldown > 0 ? skill.Cooldown + 1 : 0));
            var impacts = new List<SkillImpact>(); AttackRoll? roll = null; string changedPet = null;
            using (action)
            {
                bool aimed = skill.Has(SkillEffect.Attack) && actor.Aim;
                var modifiers = aimed ? DamageModifiers.Combine(new[] { new DamageModifier(0, 1500) }) : default;
                if (skill.Has(SkillEffect.Attack))
                { roll = CombatMath.RollAttack(skill.Dice, aimed ? 1 : 0, random); actor.Aim = false; }
                if (skill.Has(SkillEffect.Move)) actor.Cell = destination.Value;
                if (skill.Has(SkillEffect.Summon))
                {
                    changedPet = "pet:" + actorId;
                    units.Add(changedPet, new SkillUnit { Id = changedPet, Owner = actorId, Team = actor.Team,
                        Health = 12, MaxHealth = 12, BaseArmor = 5, Cell = destination.Value });
                }
                if (skill.Has(SkillEffect.Recall)) { changedPet = pet.Id; RemovePet(actorId); }
                foreach (var target in targets)
                {
                    if (target.Health <= 0) continue;
                    int previousHealth = target.Health, previousShield = target.Shield; DamageResolution damage = default;
                    if (roll.HasValue)
                    {
                        damage = CombatMath.Resolve(roll.Value, skill.Power, modifiers, target.Armor, action, skill.Penetration);
                        int absorbed = Math.Min(target.Shield, damage.Damage); target.Shield -= absorbed;
                        target.Health = Math.Max(0, target.Health - (damage.Damage - absorbed));
                        if (target.Health == 0)
                        { if (target.Owner != target.Id) units.Remove(target.Id); else RemovePet(target.Id); }
                    }
                    if (target.Health > 0)
                    {
                        long clock = units[target.Owner].OwnTurn;
                        long expiry = checked(clock + skill.Duration + 1);
                        if (skill.Has(SkillEffect.Heal)) target.Health = Math.Min(target.MaxHealth, target.Health + action.Spend(CombatResource.Healing, skill.Healing));
                        if (skill.Has(SkillEffect.Shield))
                        { target.Shield = Math.Min(CombatCaps.ShieldCapacity, target.Shield + action.Spend(CombatResource.Shield, skill.Shield)); target.ShieldExpires = expiry; }
                        if (skill.Has(SkillEffect.Armor)) { target.ArmorBonus = 10; target.ArmorExpires = expiry; }
                        if (skill.Has(SkillEffect.Aim)) { target.Aim = true; target.AimExpires = expiry; }
                        if (skill.Has(SkillEffect.Slow) && Enabled(SkillMechanics.Control) && damage.GrantedRaw > 0)
                        { target.Slow = 1; target.SlowExpires = expiry; }
                    }
                    impacts.Add(new SkillImpact(target, previousHealth, previousShield, damage));
                }
            }
            if (turnOrder.Where(id => units[id].Health > 0).Select(id => units[id].Team).Distinct().Count() <= 1)
            { Finished = true; ActiveActorId = null; budget.EndTurn(); }
            return new SkillUseResult(SkillFailure.None, actorId, skill, impacts, roll, changedPet, origin, destination ?? focus);
        }

        void RemovePet(string ownerId)
        { if (units.TryGetValue("pet:" + ownerId, out var pet)) { pet.Health = 0; units.Remove(pet.Id); } }

        bool CanWalk(GridCell from, GridCell to, int steps)
        {
            var queue = new Queue<(GridCell cell, int distance)>(); var visited = new HashSet<GridCell> { from };
            queue.Enqueue((from, 0)); int[] dx = { 1, 0, -1, 0 }, dy = { 0, 1, 0, -1 };
            while (queue.Count > 0)
            {
                var item = queue.Dequeue(); if (item.cell.Equals(to)) return true;
                if (item.distance >= steps) continue;
                for (int i = 0; i < 4; i++)
                {
                    long x = (long)item.cell.X + dx[i], y = (long)item.cell.Y + dy[i];
                    if (x < int.MinValue || x > int.MaxValue || y < int.MinValue || y > int.MaxValue) continue;
                    var next = new GridCell((int)x, (int)y);
                    if (visited.Add(next) && IsVacant(next)) queue.Enqueue((next, item.distance + 1));
                }
            }
            return false;
        }
    }
}
