using System;
using DungeonBDX.Combat;
using NUnit.Framework;

namespace DungeonBDX.Tests.Combat
{
    public sealed class DungeonBudgetTests
    {
        [TestCase(CombatResource.Damage, 30, 60)]
        [TestCase(CombatResource.Healing, 12, 18)]
        [TestCase(CombatResource.Shield, 8, 12)]
        public void RepeatedAreaPetAndReactionChargesShareActionAndTurnCaps(CombatResource resource, int actionCap, int turnCap)
        {
            var turn = new TurnBudget(int.MaxValue);
            int total = 0;
            for (int i = 0; i < 3; i++)
            {
                Assert.That(turn.TryBeginAction(1, out var action), Is.True);
                int current = 0;
                for (int target = 0; target < 100; target++) current += action.Spend(resource, int.MaxValue);
                Assert.That(current, Is.LessThanOrEqualTo(actionCap)); total += current;
                action.Dispose(); Assert.That(action.Spend(resource, 1), Is.Zero);
            }
            Assert.That(total, Is.EqualTo(turnCap)); Assert.That(turn.Spent(resource), Is.EqualTo(turnCap));
            Assert.That(turn.TryBeginAction(1, out _), Is.False);
        }

        [Test]
        public void ActionHandlesCannotOverlapBeReusedOrResetWithDispose()
        {
            var turn = new TurnBudget();
            Assert.That(turn.TryBeginAction(1, out var old), Is.True);
            Assert.That(turn.TryBeginAction(1, out _), Is.False);
            Assert.That(old.Spend(CombatResource.Damage, 30), Is.EqualTo(30)); old.Dispose();
            Assert.That(turn.TryBeginAction(1, out var current), Is.True);
            old.Dispose(); Assert.That(old.Spend(CombatResource.Damage, 30), Is.Zero);
            Assert.That(current.Spend(CombatResource.Damage, 30), Is.EqualTo(30));
            Assert.That(turn.Spent(CombatResource.Damage), Is.EqualTo(60)); turn.EndTurn();
            Assert.That(current.Spend(CombatResource.Damage, 1), Is.Zero);
            Assert.That(turn.RemainingActions, Is.Zero); Assert.That(turn.TryBeginAction(1, out _), Is.False);
        }

        [TestCase(int.MinValue)] [TestCase(0)] [TestCase(4)] [TestCase(int.MaxValue)]
        public void InvalidActionCostDoesNotSpendAnything(int cost)
        {
            var turn = new TurnBudget(); Assert.That(turn.TryBeginAction(cost, out _), Is.False);
            Assert.That(turn.RemainingActions, Is.EqualTo(3)); Assert.That(turn.ActionsStarted, Is.Zero);
        }

        [Test]
        public void SpentBudgetCannotBeResurrectedByMinimumDamageRounding()
        {
            var turn = new TurnBudget(); turn.TryBeginAction(1, out var action);
            action.Spend(CombatResource.Damage, 30);
            var roll = CombatMath.RollAttack(6, 0, new DungeonRandom(100));
            var result = CombatMath.Resolve(roll, 12, default, 150, action);
            Assert.That(result.GrantedRaw, Is.Zero); Assert.That(result.Damage, Is.Zero);
        }

        [Test]
        public void NegativeResourcesAndInvalidEnumsCannotCreateCredits()
        {
            var turn = new TurnBudget(); turn.TryBeginAction(1, out var action);
            Assert.That(action.Spend(CombatResource.Damage, int.MinValue), Is.Zero);
            Assert.That(action.Remaining(CombatResource.Damage), Is.EqualTo(30));
            Assert.Throws<ArgumentOutOfRangeException>(() => action.Spend((CombatResource)99, 1));
            Assert.That(turn.Spent(CombatResource.Damage), Is.Zero);
        }

        [Test]
        public void ZeroActionPointActorCannotStartOrBorrowAnAction()
        {
            var turn = new TurnBudget(int.MinValue);
            Assert.That(turn.TryBeginAction(1, out _), Is.False);
            Assert.That(turn.TryBeginAction(0, out _), Is.False);
        }
    }
}
