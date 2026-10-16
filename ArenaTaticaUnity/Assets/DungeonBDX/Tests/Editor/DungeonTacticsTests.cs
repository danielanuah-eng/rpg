using System.Linq;
using DungeonBDX.Skills;
using NUnit.Framework;

namespace DungeonBDX.Tests
{
    public sealed class DungeonTacticsTests
    {
        [Test]
        public void EnemyCanWalkAwayToEscapeADeadEndWithoutCrossingWallOrAlly()
        {
            var wall=Enumerable.Range(0,6).Select(y=>new GridCell(3,y));
            var board=new DungeonSkillBoard(8,8,wall);
            var session=new DungeonSkillSession(new[]{
                new HeroSpawn("a",0,DungeonProfiles.Resolve("orion-prisma"),new GridCell(2,1)),
                new HeroSpawn("b",1,DungeonProfiles.Resolve("lyra-faisca"),new GridCell(4,1))},board);
            int distance=GridCell.Distance(session.Unit("a").Cell,session.Unit("b").Cell);
            var move=DungeonTactics.Advance(session,board);
            Assert.That(move.HasValue,Is.True);
            Assert.That(GridCell.Distance(move.Value,session.Unit("b").Cell),Is.GreaterThan(distance));
            Assert.That(session.Move("a",move.Value),Is.EqualTo(SkillFailure.None));
        }
    }
}
