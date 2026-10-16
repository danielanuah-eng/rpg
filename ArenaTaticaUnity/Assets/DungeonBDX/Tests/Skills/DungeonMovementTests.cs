using System.Linq;
using DungeonBDX.Skills;
using NUnit.Framework;

namespace DungeonBDX.Tests.Skills
{
    public sealed class DungeonMovementTests
    {
        static DungeonSkillSession Session(ISkillBoard board = null) => new DungeonSkillSession(new[] {
            new HeroSpawn("hero", 0, DungeonHeroProfiles.NewHeroes[0], new GridCell(1,1)),
            new HeroSpawn("enemy", 1, DungeonHeroProfiles.NewHeroes[1], new GridCell(5,1))
        }, board ?? new DungeonSkillBoard(8,8), 731);

        [Test]
        public void WalkingSpendsOneActionAndLeavesAttackBudgetAndRngUntouched()
        {
            var session = Session(); var rng = session.RandomState;
            Assert.That(session.Move("hero", new GridCell(3,1)), Is.EqualTo(SkillFailure.None));
            Assert.That(session.RemainingActions, Is.EqualTo(2));
            Assert.That(session.Unit("hero").Cell, Is.EqualTo(new GridCell(3,1)));
            Assert.That(session.DamageSpentThisTurn, Is.Zero);
            Assert.That(session.RandomState, Is.EqualTo(rng));
            Assert.That(session.Cast("hero", "dbdx.pistola.duplo", new[] {"enemy"}).Success, Is.True);
        }

        [Test]
        public void WalkCannotCrossWallsOccupiedCellsOrExceedMovementAndActions()
        {
            var wall = Enumerable.Range(0,8).Select(y => new GridCell(2,y));
            var session = Session(new DungeonSkillBoard(8,8,wall)); var rng = session.RandomState;
            foreach (var destination in new[] {new GridCell(3,1),new GridCell(1,1),new GridCell(5,1),new GridCell(int.MaxValue,0)})
                Assert.That(session.Move("hero",destination), Is.EqualTo(SkillFailure.InvalidDestination));
            Assert.That(session.Move("enemy",new GridCell(5,2)), Is.EqualTo(SkillFailure.NotYourTurn));
            Assert.That(session.RemainingActions, Is.EqualTo(3)); Assert.That(session.RandomState, Is.EqualTo(rng));
            for (int i=0;i<3;i++) Assert.That(session.Move("hero",new GridCell(1,i+2)), Is.EqualTo(SkillFailure.None));
            Assert.That(session.Move("hero",new GridCell(1,5)), Is.EqualTo(SkillFailure.NoActions));
        }
    }
}
