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
                foreach (var space in s.Board.Spaces.Where(p => !string.IsNullOrEmpty(p.Prop) && p.Prop != "books" && p.Prop != "rune"))
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
            foreach (var icon in new[] {"move","door","ability","rest","endturn","sword","claw","skip","rotate-left","rotate-right","zoom-in","zoom-out","reset","menu","sound","life"})
                Assert.That(Resources.Load<Texture2D>("DungeonBDX/Iso/UI/Icons/" + icon), Is.Not.Null, icon);
        }
    }
}
