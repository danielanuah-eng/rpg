using System;
using System.Collections.Generic;
using System.Linq;
using DungeonBDX.Skills;
using DungeonBDX.World;
using NUnit.Framework;

namespace DungeonBDX.Tests.World
{
    public sealed class DungeonMapTests
    {
        static readonly string[] Scenarios = { DungeonMap.Crypt, DungeonMap.Library, DungeonMap.Nest };
        static readonly GridCell[] Directions =
        { new GridCell(0, 1), new GridCell(1, 0), new GridCell(0, -1), new GridCell(-1, 0) };

        [TestCase(9, 9)]
        [TestCase(11, 11)]
        [TestCase(16, 13)]
        [TestCase(31, 31)]
        public void SeedMatrixHasConnectedFloorAndSeparatedReachableUniqueSpawns(int width, int height)
        {
            foreach (string scenario in Scenarios) for (ulong seed = 0; seed < 30; seed++)
            {
                var map = DungeonMap.Generate(seed, scenario, width, height);
                Assert.That(map.Width, Is.EqualTo(width)); Assert.That(map.Height, Is.EqualTo(height));
                Assert.That(map.Seed, Is.EqualTo(seed)); Assert.That(map.Scenario, Is.EqualTo(scenario));
                Assert.That(map.Floors.Count, Is.GreaterThan(20));
                Assert.That(map.HeroSpawns.Count, Is.EqualTo(3)); Assert.That(map.EnemySpawns.Count, Is.EqualTo(3));
                Assert.That(map.Objectives.Count, Is.EqualTo(1));
                var occupied = map.HeroSpawns.Concat(map.EnemySpawns).Concat(map.Objectives).ToList();
                Assert.That(occupied.Distinct().Count(), Is.EqualTo(7));
                Assert.That(Reachable(map, map.HeroSpawns[0]).Count, Is.EqualTo(map.Floors.Count), scenario + " seed " + seed);
                foreach (var cell in occupied)
                {
                    Assert.That(map.CanStand(cell), Is.True);
                    Assert.That(map.FindPath(map.HeroSpawns[0], cell).Last(), Is.EqualTo(cell));
                }
                foreach (var hero in map.HeroSpawns) foreach (var enemy in map.EnemySpawns)
                    Assert.That(GridCell.Distance(hero, enemy), Is.GreaterThanOrEqualTo(7));
                foreach (var cell in map.Floors)
                {
                    Assert.That(cell.X, Is.InRange(1, width - 2)); Assert.That(cell.Y, Is.InRange(1, height - 2));
                    Assert.That(map.IsFloor(cell), Is.True);
                }
                Assert.That(map.Props.Select(p => p.Cell).Distinct().Count(), Is.EqualTo(map.Props.Count));
                foreach (var prop in map.Props)
                {
                    Assert.That(map.CanStand(prop.Cell), Is.True);
                    Assert.That(occupied.Contains(prop.Cell), Is.False);
                    Assert.That(prop.RotationQuarterTurns, Is.InRange(0, 3));
                }
            }
        }

        [Test]
        public void SameSeedProducesIdenticalTopologyAndDecorationsAcrossInstances()
        {
            foreach (string scenario in Scenarios) foreach (ulong seed in new[] { 0UL, 1UL, 123456789UL, ulong.MaxValue })
            {
                var a = DungeonMap.Generate(seed, scenario, 17, 19);
                var b = DungeonMap.Generate(seed, scenario, 17, 19);
                Assert.That(Signature(a), Is.EqualTo(Signature(b)));
            }
        }

        [Test]
        public void VersionOneSeed42ReferenceTopologyIsStableForSaveReplay()
        {
            var map = DungeonMap.Generate(42, DungeonMap.Crypt);
            var rows = new List<string>();
            for (int y = 0; y < map.Height; y++)
            {
                string row = "";
                for (int x = 0; x < map.Width; x++) row += map.IsFloor(new GridCell(x, y)) ? "." : "#";
                rows.Add(row);
            }
            Assert.That(DungeonMap.GeneratorVersion, Is.EqualTo(1));
            Assert.That(string.Join("/", rows), Is.EqualTo(
                "###########/#....#...##/#....#..#.#/#.........#/#.....#####/##.....#.##/##.#.....##/#......#..#/#.........#/#.##.##...#/###########"));
            Assert.That(map.Props.Count, Is.EqualTo(10));
            Assert.That(map.Props[0].Cell, Is.EqualTo(new GridCell(4, 1)));
        }

        [Test]
        public void DifferentSeedsAndScenarioStreamsProduceVariety()
        {
            var signatures = new HashSet<string>();
            for (ulong seed = 0; seed < 32; seed++) signatures.Add(Signature(DungeonMap.Generate(seed, DungeonMap.Crypt)));
            Assert.That(signatures.Count, Is.GreaterThan(24));
            Assert.That(Scenarios.Select(s => Signature(DungeonMap.Generate(4, s))).Distinct().Count(), Is.EqualTo(3));
        }

        [Test]
        public void PathUsesAdjacentFloorCellsAndIncludesEndpoints()
        {
            var map = DungeonMap.Generate(42, DungeonMap.Library);
            var path = map.FindPath(map.HeroSpawns[0], map.EnemySpawns[0]);
            Assert.That(path.First(), Is.EqualTo(map.HeroSpawns[0]));
            Assert.That(path.Last(), Is.EqualTo(map.EnemySpawns[0]));
            Assert.That(path.Distinct().Count(), Is.EqualTo(path.Count));
            for (int i = 0; i < path.Count; i++)
            {
                Assert.That(map.CanStand(path[i]), Is.True);
                if (i > 0) Assert.That(GridCell.Distance(path[i - 1], path[i]), Is.EqualTo(1));
            }
            Assert.That(map.FindPath(path[0], path[0]).Count, Is.EqualTo(1));
        }

        [Test]
        public void WallsAndInvalidCoordinatesCannotBeUsedAsEndpoints()
        {
            var map = DungeonMap.Generate(99, DungeonMap.Nest);
            foreach (var cell in new[] { new GridCell(0, 0), new GridCell(-1, 2), new GridCell(11, 4), new GridCell(int.MaxValue, int.MinValue) })
            {
                Assert.That(map.CanStand(cell), Is.False);
                Assert.That(map.HasLineOfSight(cell, map.HeroSpawns[0]), Is.False);
                Assert.That(map.FindPath(map.HeroSpawns[0], cell), Is.Empty);
                Assert.That(map.FindPath(cell, map.HeroSpawns[0]), Is.Empty);
            }
        }

        [Test]
        public void LineOfSightIsSymmetricAndCannotCutBlockedCorners()
        {
            int testedCorners = 0;
            for (ulong seed = 0; seed < 12; seed++)
            {
                var map = DungeonMap.Generate(seed, DungeonMap.Crypt);
                foreach (var cell in map.Floors)
                {
                    Assert.That(map.HasLineOfSight(cell, cell), Is.True);
                    var diagonal = new GridCell(cell.X + 1, cell.Y + 1);
                    if (map.CanStand(diagonal) && (!map.CanStand(new GridCell(cell.X + 1, cell.Y)) || !map.CanStand(new GridCell(cell.X, cell.Y + 1))))
                    {
                        Assert.That(map.HasLineOfSight(cell, diagonal), Is.False); testedCorners++;
                    }
                }
                for (int i = 0; i < map.Floors.Count; i += 3) for (int j = 0; j < map.Floors.Count; j += 5)
                    Assert.That(map.HasLineOfSight(map.Floors[i], map.Floors[j]), Is.EqualTo(map.HasLineOfSight(map.Floors[j], map.Floors[i])));
            }
            Assert.That(testedCorners, Is.GreaterThan(0));
        }

        [TestCase(8, 11)] [TestCase(32, 11)] [TestCase(11, 8)] [TestCase(11, 32)]
        public void InvalidSizeIsRejected(int width, int height)
        { Assert.Throws<ArgumentOutOfRangeException>(() => DungeonMap.Generate(0, DungeonMap.Crypt, width, height)); }

        [TestCase(null)] [TestCase("")] [TestCase("unknown")] [TestCase("Cripta-de-bronze")]
        public void UnknownScenarioIsRejected(string scenario)
        { Assert.Throws<ArgumentException>(() => DungeonMap.Generate(0, scenario)); }

        static HashSet<GridCell> Reachable(DungeonMap map, GridCell start)
        {
            var result = new HashSet<GridCell> { start }; var queue = new Queue<GridCell>(); queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                foreach (var direction in Directions)
                {
                    var next = new GridCell(cell.X + direction.X, cell.Y + direction.Y);
                    if (map.CanStand(next) && result.Add(next)) queue.Enqueue(next);
                }
            }
            return result;
        }

        static string Signature(DungeonMap map) => string.Join(";", map.Floors.Select(c => c.X + "," + c.Y)) + "|" +
            string.Join(";", map.Props.Select(p => p.Cell.X + "," + p.Cell.Y + "," + p.Kind + "," + p.RotationQuarterTurns));
    }
}
