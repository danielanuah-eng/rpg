using System;
using System.Collections.Generic;
using DungeonBDX.Skills;

namespace DungeonBDX.World
{
    /// <summary>Decoration only: props do not change movement, cover or combat.</summary>
    public sealed class DungeonProp
    {
        public GridCell Cell { get; }
        public string Kind { get; }
        public int RotationQuarterTurns { get; }
        internal DungeonProp(GridCell cell, string kind, int rotation)
        { Cell = cell; Kind = kind; RotationQuarterTurns = rotation; }
    }

    /// <summary>Versioned, engine-independent floor plan. Grid Y becomes Unity world Z.</summary>
    public sealed class DungeonMap : ISkillBoard
    {
        public const int GeneratorVersion = 1;
        public const string Crypt = "cripta-de-bronze";
        public const string Library = "biblioteca-do-crepusculo";
        public const string Nest = "ninho-do-eclipse";
        static readonly GridCell[] Directions =
        { new GridCell(0, 1), new GridCell(1, 0), new GridCell(0, -1), new GridCell(-1, 0) };

        readonly bool[,] floor;
        public int Width { get; }
        public int Height { get; }
        public ulong Seed { get; }
        public string Scenario { get; }
        public IReadOnlyList<GridCell> Floors { get; }
        public IReadOnlyList<GridCell> HeroSpawns { get; }
        public IReadOnlyList<GridCell> EnemySpawns { get; }
        public IReadOnlyList<GridCell> Objectives { get; }
        public IReadOnlyList<DungeonProp> Props { get; }

        DungeonMap(ulong seed, string scenario, int width, int height)
        {
            Width = width; Height = height; Seed = seed; Scenario = scenario;
            floor = new bool[width, height];
            var rng = new MapRandom(seed, scenario, width, height);
            var heroes = new[] { new GridCell(2, 2), new GridCell(1, 2), new GridCell(2, 1) };
            var enemies = new[]
            {
                new GridCell(width - 3, height - 3), new GridCell(width - 2, height - 3),
                new GridCell(width - 3, height - 2)
            };
            var objectives = new[] { new GridCell(width / 2, height / 2) };
            HeroSpawns = Array.AsReadOnly(heroes); EnemySpawns = Array.AsReadOnly(enemies);
            Objectives = Array.AsReadOnly(objectives);

            // Four corner chambers and a central objective chamber are joined before
            // optional rooms and obstacles, so every generated floor has a route.
            var centers = new List<GridCell>
            {
                heroes[0], new GridCell(width - 3, 2), enemies[0],
                new GridCell(2, height - 3), objectives[0]
            };
            for (int i = 0; i < centers.Count; i++)
                CarveRoom(centers[i], 1 + rng.Next(2), 1 + rng.Next(2));
            for (int i = 1; i < centers.Count; i++)
                CarvePassage(centers[i - 1], centers[i], rng.Next(2) == 0);
            CarvePassage(centers[4], centers[0], rng.Next(2) == 0);
            int extraRooms = 1 + width * height / 120;
            for (int i = 0; i < extraRooms; i++)
            {
                var center = new GridCell(2 + rng.Next(width - 4), 2 + rng.Next(height - 4));
                CarveRoom(center, 1 + rng.Next(2), 1 + rng.Next(2));
                CarvePassage(center, centers[rng.Next(centers.Count)], rng.Next(2) == 0);
            }

            var protectedCells = new HashSet<GridCell>(heroes);
            protectedCells.UnionWith(enemies); protectedCells.UnionWith(objectives);
            // Keep space immediately around spawn/objective cells. Elsewhere reject any
            // obstacle that disconnects the floor; no player can be stranded by a seed.
            var reserved = new List<GridCell>(protectedCells);
            foreach (var cell in reserved) foreach (var direction in Directions)
                protectedCells.Add(new GridCell(cell.X + direction.X, cell.Y + direction.Y));
            int attempts = width * height / 7;
            for (int i = 0; i < attempts; i++)
            {
                var cell = new GridCell(1 + rng.Next(width - 2), 1 + rng.Next(height - 2));
                if (!IsFloor(cell) || protectedCells.Contains(cell)) continue;
                floor[cell.X, cell.Y] = false;
                if (!AllFloorConnected(heroes[0])) floor[cell.X, cell.Y] = true;
            }

            var floors = new List<GridCell>();
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
                if (floor[x, y]) floors.Add(new GridCell(x, y));
            Floors = floors.AsReadOnly();
            var props = new List<DungeonProp>();
            string kind = scenario == Crypt ? "bronze-urn" : scenario == Library ? "book-pile" : "eclipse-crystal";
            foreach (var cell in floors)
            {
                if (props.Count >= 24) break;
                if (!protectedCells.Contains(cell) && rng.Next(9) == 0)
                    props.Add(new DungeonProp(cell, kind, rng.Next(4)));
            }
            Props = props.AsReadOnly();
        }

        public static DungeonMap Generate(ulong seed, string scenario, int width = 11, int height = 11)
        {
            if (width < 9 || width > 31) throw new ArgumentOutOfRangeException(nameof(width), "Expected 9..31 cells.");
            if (height < 9 || height > 31) throw new ArgumentOutOfRangeException(nameof(height), "Expected 9..31 cells.");
            if (scenario != Crypt && scenario != Library && scenario != Nest)
                throw new ArgumentException("Unknown DungeonBDX scenario.", nameof(scenario));
            return new DungeonMap(seed, scenario, width, height);
        }

        public bool IsFloor(GridCell cell) => cell.X >= 0 && cell.Y >= 0 && cell.X < Width && cell.Y < Height && floor[cell.X, cell.Y];
        public bool CanStand(GridCell cell) => IsFloor(cell);

        public bool HasLineOfSight(GridCell from, GridCell to)
        {
            if (!CanStand(from) || !CanStand(to)) return false;
            int x = from.X, y = from.Y, nx = Math.Abs(to.X - x), ny = Math.Abs(to.Y - y);
            int sx = Math.Sign(to.X - x), sy = Math.Sign(to.Y - y), ix = 0, iy = 0;
            // Integer supercover traversal is symmetric, including exact corner crossings.
            while (ix < nx || iy < ny)
            {
                int comparison = (1 + 2 * ix) * ny - (1 + 2 * iy) * nx;
                if (comparison == 0)
                {
                    if (!CanStand(new GridCell(x + sx, y)) || !CanStand(new GridCell(x, y + sy))) return false;
                    x += sx; y += sy; ix++; iy++;
                }
                else if (comparison < 0) { x += sx; ix++; }
                else { y += sy; iy++; }
                if (!CanStand(new GridCell(x, y))) return false;
            }
            return true;
        }

        /// <summary>Shortest four-neighbour route including both endpoints; empty if either is blocked.</summary>
        public IReadOnlyList<GridCell> FindPath(GridCell from, GridCell to)
        {
            if (!CanStand(from) || !CanStand(to)) return Array.Empty<GridCell>();
            var queue = new Queue<GridCell>();
            var previous = new Dictionary<GridCell, GridCell> { [from] = from };
            queue.Enqueue(from);
            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                if (cell.Equals(to))
                {
                    var path = new List<GridCell> { to };
                    while (!cell.Equals(from)) { cell = previous[cell]; path.Add(cell); }
                    path.Reverse(); return path.AsReadOnly();
                }
                foreach (var direction in Directions)
                {
                    var next = new GridCell(cell.X + direction.X, cell.Y + direction.Y);
                    if (!CanStand(next) || previous.ContainsKey(next)) continue;
                    previous.Add(next, cell); queue.Enqueue(next);
                }
            }
            return Array.Empty<GridCell>();
        }

        void CarveRoom(GridCell center, int halfWidth, int halfHeight)
        {
            for (int y = Math.Max(1, center.Y - halfHeight); y <= Math.Min(Height - 2, center.Y + halfHeight); y++)
                for (int x = Math.Max(1, center.X - halfWidth); x <= Math.Min(Width - 2, center.X + halfWidth); x++) floor[x, y] = true;
        }

        void CarvePassage(GridCell from, GridCell to, bool horizontalFirst)
        {
            int x = from.X, y = from.Y;
            floor[x, y] = true;
            if (horizontalFirst)
            {
                while (x != to.X) { x += Math.Sign(to.X - x); floor[x, y] = true; }
                while (y != to.Y) { y += Math.Sign(to.Y - y); floor[x, y] = true; }
            }
            else
            {
                while (y != to.Y) { y += Math.Sign(to.Y - y); floor[x, y] = true; }
                while (x != to.X) { x += Math.Sign(to.X - x); floor[x, y] = true; }
            }
        }

        bool AllFloorConnected(GridCell start)
        {
            int count = 0;
            for (int y = 1; y < Height - 1; y++) for (int x = 1; x < Width - 1; x++) if (floor[x, y]) count++;
            var seen = new HashSet<GridCell> { start };
            var queue = new Queue<GridCell>(); queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                foreach (var direction in Directions)
                {
                    var next = new GridCell(cell.X + direction.X, cell.Y + direction.Y);
                    if (CanStand(next) && seen.Add(next)) queue.Enqueue(next);
                }
            }
            return count == seen.Count;
        }

        sealed class MapRandom
        {
            ulong state;
            internal MapRandom(ulong seed, string scenario, int width, int height)
            {
                unchecked
                {
                    ulong hash = 14695981039346656037UL;
                    foreach (char c in scenario) hash = (hash ^ c) * 1099511628211UL;
                    state = seed ^ hash ^ ((ulong)width << 32) ^ (uint)height;
                }
            }
            internal int Next(int exclusiveMaximum)
            {
                unchecked
                {
                    state += 0x9E3779B97F4A7C15UL;
                    ulong z = state; z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                    z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL; z ^= z >> 31;
                    return (int)((uint)(z >> 32) % (uint)exclusiveMaximum);
                }
            }
        }
    }
}
