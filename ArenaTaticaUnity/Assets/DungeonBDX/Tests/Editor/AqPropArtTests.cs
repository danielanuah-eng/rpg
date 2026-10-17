using System.Linq;
using DungeonBDX.Arcadia;
using NUnit.Framework;
using UnityEngine;

namespace DungeonBDX.Tests
{
    // PixelLab scenery and HUD art: every prop a chapter places is drawn and stays clear of the walls.
    public class AqPropArtTests
    {
        const float WallReach = AqBoardView.WallThickness*.5f;

        [Test] public void EveryScenarioPropHasArtAndStaysOffTheWalls()
        {
            foreach (var id in AqScenarios.Ids)
            {
                var s = AqScenarios.Create(id);
                foreach (var space in s.Board.Spaces.Where(p => !string.IsNullOrEmpty(p.Prop) && p.Prop != "rune"))
                {
                    var art = Resources.Load<Texture2D>("DungeonBDX/Iso/Props/" + space.Prop);
                    Assert.That(art, Is.Not.Null, id + " " + space.Prop);
                    float half = art.width*.5f/AqBoardView.PropPixelsPerUnit(space.Prop, art);
                    Assert.That(half*2, Is.LessThanOrEqualTo(AqBoardView.PropMaxWidth + .001f), space.Prop);
                    var at = AqBoardView.PropAnchor(s.Board, space.Cell, half);
                    var c = space.Cell;
                    void Side(AqCell n, float reach)
                    {
                        bool wall = !s.Board.Contains(n) || s.Board.Edge(c, n) != null;
                        if (wall) Assert.That(reach + half, Is.LessThanOrEqualTo(.5f - WallReach + .0001f), id + " " + space.Prop + " " + c);
                    }
                    Side(new AqCell(c.X+1, c.Y), at.x - c.X); Side(new AqCell(c.X-1, c.Y), c.X - at.x);
                    Side(new AqCell(c.X, c.Y+1), at.z - c.Y); Side(new AqCell(c.X, c.Y-1), c.Y - at.z);
                }
            }
        }

        [Test] public void HudArtForTabsLifeBarAndActionsIsDelivered()
        {
            foreach (var ui in new[] {"tab", "bar-frame", "bar-fill"})
                Assert.That(Resources.Load<Texture2D>("DungeonBDX/Iso/UI/" + ui), Is.Not.Null, ui);
            foreach (var icon in new[] {"move","door","ability","rest","endturn","sword","claw","skip","rotate-left","rotate-right","zoom-in","zoom-out","reset","menu","sound","life","emblem","die","coin","save","trophy","quest"})
                Assert.That(Resources.Load<Texture2D>("DungeonBDX/Iso/UI/Icons/" + icon), Is.Not.Null, icon);
        }

        [Test] public void EveryChapterMonsterHasSpriteArt()
        {
            foreach (var id in AqScenarios.Ids)
                foreach (var m in AqScenarios.Create(id).Monsters)
                {
                    string folder = m.SpriteFolder ?? (ArenaTatica.Core.Content.Fighters.TryGetValue(m.HeroId, out var f) ? f.sprites : null);
                    Assert.That(folder, Is.Not.Null, m.HeroId);
                    Assert.That(Resources.LoadAll<Texture2D>("Sprites/" + folder), Is.Not.Empty, m.HeroId);
                }
        }

        [Test] public void BookRoomsWearShelfWallsInsteadOfAShelfProp()
        {
            Assert.That(Resources.Load<Texture2D>("DungeonBDX/Iso/Walls/wall-books"), Is.Not.Null);
            int shelved = 0;
            foreach (var id in AqScenarios.Ids)
            {
                var s = AqScenarios.Create(id);
                Assert.That(s.Board.Spaces.Any(p => p.Prop == "books"), Is.False, id);
                shelved += s.Board.Spaces.Count(p => p.Wall == "books");
            }
            Assert.That(shelved, Is.GreaterThan(0));
        }

        [Test] public void VillainsWalkAndBreatheOnEveryDiagonalOrItsMirror()
        {
            foreach (var folder in new[] {"TrollEsgoto","CultistaEncapuzado","EsqueletoLanceiro","ReiGoblinFerrugem","NecromanteCripta","CondeVesperal"})
                foreach (var (east, west) in new[] {("south-east","south-west"), ("north-east","north-west")})
                {
                    // The animator flips the opposite diagonal, so one side of each pair is enough.
                    bool Has(string clip, string d) => Resources.Load<Texture2D>("Sprites/" + folder + "/" + clip + "_" + d) != null;
                    Assert.That(Has("walk", east) || Has("walk", west), folder + " walk " + east);
                    Assert.That(Has("idle", east) || Has("idle", west), folder + " idle " + east);
                    Assert.That(Has("attack", east) || Has("attack", west), folder + " attack " + east);
                    Assert.That(Has("hit", east) || Has("hit", west), folder + " hit " + east);
                }
        }
    }
}
