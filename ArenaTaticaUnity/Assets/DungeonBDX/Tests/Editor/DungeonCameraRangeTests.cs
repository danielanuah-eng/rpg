using System;
using System.Linq;
using DungeonBDX.Skills;
using DungeonBDX.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace DungeonBDX.Tests
{
    public sealed class DungeonCameraRangeTests
    {
        [TestCase(DungeonMap.Crypt)]
        [TestCase(DungeonMap.Library)]
        [TestCase(DungeonMap.Nest)]
        public void MovementPreviewMatchesAcceptedMovesAndNeverMutatesSession(string scenario)
        {
            var board = DungeonMap.Generate(42, scenario);
            var spawns = DungeonExpedition.CreateSpawns(board, DungeonHeroProfiles.NewHeroes.Select(p => p.Id).ToArray(), false);
            var session = new DungeonSkillSession(spawns, board, 99);
            string before = session.PersistenceState;
            var preview = DungeonActionRange.Compute(session, board, true);
            Assert.That(preview.Cells.Count, Is.GreaterThan(0));
            foreach (var cell in board.Floors)
            {
                var fresh = new DungeonSkillSession(spawns, board, 99);
                bool accepted = fresh.Move(fresh.ActiveActorId, cell) == SkillFailure.None;
                Assert.That(preview.Cells.Any(c => c.Cell.Equals(cell)), Is.EqualTo(accepted), cell.ToString());
            }
            foreach (var cell in preview.Cells)
                Assert.That(cell.Distance, Is.InRange(1, DungeonProfiles.Resolve(session.Unit(session.ActiveActorId).ProfileId).Movement));
            Assert.That(session.PersistenceState, Is.EqualTo(before));
        }

        [Test]
        public void CompanionCommandRangeUsesPetAndRejectsWallsWithoutSpendingItsLastAction()
        {
            var board = DungeonMap.Generate(42, DungeonMap.Crypt);
            var spawns = new[] {
                new HeroSpawn("owner", 0, DungeonProfiles.Resolve("vaelis-vinculo"), board.HeroSpawns[0]),
                new HeroSpawn("enemy", 1, DungeonProfiles.Resolve("colosso"), board.EnemySpawns[0])
            };
            var session = new DungeonSkillSession(spawns, board, 99);
            var summon = DungeonActionRange.Compute(session, board, false, "dbdx.draco.invocar");
            Assert.That(summon.Cells.Count, Is.GreaterThan(0));
            Assert.That(session.Cast("owner", "dbdx.draco.invocar", destination: summon.Cells[0].Cell).Success, Is.True);
            string before = session.PersistenceState;
            var preview = DungeonActionRange.Compute(session, board, false, "dbdx.draco.comandar");
            Assert.That(preview.Origin, Is.EqualTo(session.Unit(session.PetId("owner")).Cell));
            Assert.That(preview.Cells.Count, Is.GreaterThan(0));
            foreach (var cell in preview.Cells)
            {
                Assert.That(cell.Distance, Is.InRange(1, 4));
                Assert.That(cell.Distance, Is.EqualTo(GridCell.Distance(preview.Origin, cell.Cell)));
                Assert.That(board.HasLineOfSight(preview.Origin, cell.Cell), Is.True);
            }
            Assert.That(session.PersistenceState, Is.EqualTo(before));
            var disabled = DungeonActionRange.Compute(session, board, false, "dbdx.draco.comandar", SkillMechanics.None);
            Assert.That(disabled.Cells, Is.Empty);
        }

        [Test]
        public void TeleportPreviewAllowsVacantCellThroughWallAsDomainDoes()
        {
            var board = DungeonMap.Generate(42, DungeonMap.Crypt);
            var pair = board.Floors.SelectMany(a => board.Floors.Select(b => (a, b)))
                .First(p => GridCell.Distance(p.a, p.b) == 2 && !board.HasLineOfSight(p.a, p.b)
                    && board.FindPath(p.a, p.b).Count > 3);
            var enemy = board.Floors.First(c => !c.Equals(pair.a) && !c.Equals(pair.b));
            var session = new DungeonSkillSession(new[] {
                new HeroSpawn("mage", 0, DungeonProfiles.Resolve("orion-prisma"), pair.a),
                new HeroSpawn("enemy", 1, DungeonProfiles.Resolve("colosso"), enemy)
            }, board);
            string before = session.PersistenceState;
            var preview = DungeonActionRange.Compute(session, board, false, "dbdx.arcano.transposicao");
            Assert.That(preview.Cells.Any(c => c.Cell.Equals(pair.b)), Is.True);
            Assert.That(session.PersistenceState, Is.EqualTo(before));
            Assert.That(session.Cast("mage", "dbdx.arcano.transposicao", destination: pair.b).Success, Is.True);
        }

        [TestCase(1600, 900, 45)]
        [TestCase(900, 1600, 135)]
        [TestCase(1600, 900, 270)]
        public void RotatedWholeBoardStillFitsBetweenHudPanels(int width, int height, float yaw)
        {
            var go = new GameObject("Orbit camera fixture", typeof(Camera));
            try
            {
                var camera = go.GetComponent<Camera>(); camera.orthographic = true;
                DungeonWorldView.Frame(camera, 11, 11, new Rect(0, 0, width, height), width, height, yaw, 1, Vector2.zero);
                foreach (float x in new[] { -.5f, 10.5f }) foreach (float z in new[] { -.5f, 10.5f })
                {
                    var point = camera.WorldToViewportPoint(new Vector3(x, 0, z));
                    Assert.That(point.x, Is.InRange(.019f, .981f));
                    Assert.That(point.y, Is.InRange(width < height ? .399f : .319f, .871f));
                    Assert.That(point.z, Is.GreaterThan(0));
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }

        [Test]
        public void WheelZoomDuringHeldLeftButtonCancelsTheBoardActionOnRelease()
        {
            var board = DungeonMap.Generate(42, DungeonMap.Crypt);
            var session = new DungeonSkillSession(DungeonExpedition.CreateSpawns(board,
                DungeonHeroProfiles.NewHeroes.Select(p => p.Id).ToArray(), false), board);
            string before = session.PersistenceState;
            var go = new GameObject("Mouse zoom cancellation fixture");
            var mouse = InputSystem.AddDevice<Mouse>();
            var touch = InputSystem.AddDevice<Touchscreen>();
            try
            {
                var world = go.AddComponent<DungeonWorldView>(); world.Build(board, session);
                var input = new DungeonCameraInput();
                InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(400, 400), buttons = 1 });
                InputSystem.Update();
                Assert.That(input.Read(world, _ => false, out _), Is.False);
                InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(400, 400), buttons = 1, scroll = new Vector2(0, 120) });
                InputSystem.Update();
                Assert.That(input.Read(world, _ => false, out _), Is.False);
                Assert.That(world.CameraZoom, Is.GreaterThan(1));
                InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(400, 400) });
                InputSystem.Update();
                Assert.That(input.Read(world, _ => false, out _), Is.False, "Zoom must not become a paid board tap.");
                Assert.That(session.PersistenceState, Is.EqualTo(before));
            }
            finally
            {
                InputSystem.RemoveDevice(mouse); InputSystem.RemoveDevice(touch);
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void DragAndHudCrossingNeverBecomeTapsButFreshTapStillWorks()
        {
            var input = new DungeonCameraInput();
            input.PointerDown(new Vector2(100, 100), false);
            input.PointerMoved(new Vector2(150, 100), false);
            Assert.That(input.PointerUp(new Vector2(150, 100), false), Is.False);
            input.PointerDown(new Vector2(100, 100), false);
            input.PointerMoved(new Vector2(102, 100), true);
            Assert.That(input.PointerUp(new Vector2(100, 100), false), Is.False);
            input.PointerDown(new Vector2(100, 100), true);
            Assert.That(input.PointerUp(new Vector2(100, 100), false), Is.False);
            input.PointerDown(new Vector2(100, 100), false);
            input.CancelGesture();
            Assert.That(input.PointerUp(new Vector2(100, 100), false), Is.False);
            input.Reset();
            input.PointerDown(new Vector2(100, 100), false);
            Assert.That(input.PointerUp(new Vector2(102, 102), false), Is.True);
        }
    }
}
