using System;
using System.Linq;
using DungeonBDX.Combat;
using DungeonBDX.Skills;
using NUnit.Framework;

namespace DungeonBDX.Tests.Skills
{
    public sealed class DungeonSkillTests
    {
        sealed class ConstantDie : IDieRoller
        {
            readonly int face;
            public ConstantDie(int face) { this.face = face; }
            public int NextDie() => face;
        }
        static DungeonHeroProfile Native(HeroArchetype type) => DungeonHeroProfiles.NewHeroes.Single(hero => hero.Archetype == type);
        static DungeonHeroProfile Tank => DungeonHeroProfiles.AdaptArena("colosso", "Colosso", 18, 0, 2);
        static DungeonSkillSession Duel(DungeonHeroProfile caster, IDieRoller dice = null,
            SkillMechanics mechanics = SkillMechanics.Default, ISkillBoard board = null, GridCell? enemy = null)
        {
            return new DungeonSkillSession(new[] { new HeroSpawn("caster", 0, caster, new GridCell(1, 1)),
                new HeroSpawn("enemy", 1, Tank, enemy ?? new GridCell(3, 1)) }, board ?? new DungeonSkillBoard(10, 10),
                seed:123, mechanics:mechanics, random:dice);
        }
        static void NextCaster(DungeonSkillSession session)
        {
            Assert.That(session.EndTurn(session.ActiveActorId), Is.True);
            for (int i = 0; i < 32 && session.ActiveActorId != "caster" && !session.Finished; i++)
                Assert.That(session.EndTurn(session.ActiveActorId), Is.True);
            Assert.That(session.ActiveActorId, Is.EqualTo("caster"));
        }

        [TestCase("unknown", "dbdx.arcano.lanca", "enemy", SkillFailure.NotYourTurn)]
        [TestCase("caster", "unknown", "enemy", SkillFailure.UnknownSkill)]
        [TestCase("caster", "dbdx.pistola.duplo", "enemy", SkillFailure.NotInLoadout)]
        [TestCase("caster", "dbdx.arcano.lanca", "missing", SkillFailure.InvalidTarget)]
        [TestCase("caster", "dbdx.arcano.lanca", "caster", SkillFailure.InvalidTarget)]
        public void RejectedCommandsAreAtomicAndDoNotConsumeRng(string actor, string skill, string target, SkillFailure failure)
        {
            var session = Duel(Native(HeroArchetype.Arcanist)); ulong? state = session.RandomState;
            var result = session.Cast(actor, skill, new[] { target });
            Assert.That(result.Failure, Is.EqualTo(failure)); Assert.That(session.RemainingActions, Is.EqualTo(3));
            Assert.That(session.RandomState, Is.EqualTo(state)); Assert.That(session.DamageSpentThisTurn, Is.Zero);
        }

        [Test]
        public void DuplicateTargetsAndExtremeCoordinatesAreRejectedBeforeSpending()
        {
            var session = Duel(Native(HeroArchetype.Arcanist));
            Assert.That(session.Cast("caster", "dbdx.arcano.geada", new[] { "enemy", "enemy" }).Failure, Is.EqualTo(SkillFailure.InvalidTarget));
            Assert.That(session.Cast("caster", "dbdx.arcano.transposicao", destination:new GridCell(int.MaxValue, int.MinValue)).Failure, Is.EqualTo(SkillFailure.InvalidDestination));
            Assert.That(session.RemainingActions, Is.EqualTo(3));
            Assert.That(GridCell.Distance(new GridCell(int.MinValue, 0), new GridCell(int.MaxValue, 0)), Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void PaidActionsUsesAndCooldownCannotBeResetWithinTheSameTurn()
        {
            var session = Duel(Native(HeroArchetype.Gunslinger), new ConstantDie(6));
            for (int i = 0; i < 2; i++) Assert.That(session.Cast("caster", "dbdx.pistola.duplo", new[] { "enemy" }).Success, Is.True);
            Assert.That(session.Cast("caster", "dbdx.pistola.duplo", new[] { "enemy" }).Failure, Is.EqualTo(SkillFailure.UsesExhausted));
            Assert.That(session.RemainingActions, Is.EqualTo(1));
            Assert.That(session.Cast("caster", "dbdx.pistola.mira", Array.Empty<string>()).Success, Is.True);
            Assert.That(session.RemainingActions, Is.Zero);
            Assert.That(session.Cast("caster", "dbdx.basico.golpe", new[] { "enemy" }).Failure, Is.EqualTo(SkillFailure.NoActions));
        }

        [Test]
        public void AimRerollsMissesOnlyForOneAttackAndCannotCreateAnAttackLoop()
        {
            var session = Duel(Native(HeroArchetype.Gunslinger), new ConstantDie(1));
            Assert.That(session.Cast("caster", "dbdx.pistola.mira").Success, Is.True);
            var first = session.Cast("caster", "dbdx.pistola.duplo", new[] { "enemy" });
            var second = session.Cast("caster", "dbdx.pistola.duplo", new[] { "enemy" });
            Assert.That(first.Roll.Value.Rolls, Is.EqualTo(4)); Assert.That(second.Roll.Value.Rolls, Is.EqualTo(2));
            Assert.That(first.Impacts[0].Damage, Is.Zero); Assert.That(session.DamageSpentThisTurn, Is.Zero);
        }

        [Test]
        public void OneCooldownSkipsExactlyOneFutureOwnerTurn()
        {
            var session = Duel(Native(HeroArchetype.Arcanist));
            Assert.That(session.Cast("caster", "dbdx.arcano.egide", new[] { "caster" }).Success, Is.True);
            Assert.That(session.Unit("caster").Shield, Is.EqualTo(8)); NextCaster(session);
            Assert.That(session.Cast("caster", "dbdx.arcano.egide", new[] { "caster" }).Failure, Is.EqualTo(SkillFailure.Cooldown));
            Assert.That(session.Unit("caster").Shield, Is.EqualTo(8)); NextCaster(session);
            Assert.That(session.Unit("caster").Shield, Is.Zero);
            Assert.That(session.Cast("caster", "dbdx.arcano.egide", new[] { "caster" }).Success, Is.True);
        }

        [Test]
        public void PetAndAreaUseTheOwnersBudgetAndHaveNoIndependentTurn()
        {
            var session = new DungeonSkillSession(new[] {
                new HeroSpawn("caster",0,Native(HeroArchetype.DragonBinder),new GridCell(1,1)),
                new HeroSpawn("enemy-a",1,Tank,new GridCell(3,1)), new HeroSpawn("enemy-b",1,Tank,new GridCell(3,2)),
                new HeroSpawn("enemy-c",1,Tank,new GridCell(4,1)) }, new DungeonSkillBoard(10,10), random:new ConstantDie(6));
            Assert.That(session.Cast("caster", "dbdx.draco.invocar", destination:new GridCell(2,1)).Success, Is.True);
            Assert.That(session.Unit("pet:caster").OwnerId, Is.EqualTo("caster"));
            Assert.That(session.Cast("pet:caster", "dbdx.draco.comandar", new[] { "enemy-a" }).Failure, Is.EqualTo(SkillFailure.NotYourTurn));
            var command = session.Cast("caster", "dbdx.draco.comandar", new[] { "enemy-a" });
            Assert.That(command.Success, Is.True); Assert.That(session.DamageSpentThisTurn, Is.EqualTo(command.Impacts.Sum(impact => impact.GrantedRaw)));
            Assert.That(session.RemainingActions, Is.Zero); NextCaster(session);
            var area = session.Cast("caster", "dbdx.draco.sopro", new[] { "enemy-a", "enemy-c", "enemy-b" });
            Assert.That(area.Success, Is.True); Assert.That(area.Impacts.Sum(impact => impact.RequestedRaw), Is.EqualTo(36));
            Assert.That(area.Impacts.Sum(impact => impact.GrantedRaw), Is.EqualTo(30));
            Assert.That(area.Impacts.Select(impact => impact.UnitId), Is.EqualTo(new[] { "enemy-a", "enemy-b", "enemy-c" }));
            Assert.That(area.Roll.Value.Rolls, Is.EqualTo(3));
            Assert.That(session.Cast("caster", "dbdx.draco.comandar", new[] { "enemy-a" }).Success, Is.True);
            Assert.That(session.DamageSpentThisTurn, Is.LessThanOrEqualTo(60)); Assert.That(session.RemainingActions, Is.Zero);
        }

        [Test]
        public void AreaAnchorIsTheSelectedTargetIndependentOfOrdinalBudgetOrder()
        {
            var session = new DungeonSkillSession(new[] {
                new HeroSpawn("caster",0,Native(HeroArchetype.Arcanist),new GridCell(1,1)),
                new HeroSpawn("z-center",1,Tank,new GridCell(3,1)), new HeroSpawn("a-left",1,Tank,new GridCell(2,1)),
                new HeroSpawn("b-right",1,Tank,new GridCell(4,1)) },new DungeonSkillBoard(10,10),random:new ConstantDie(6));
            var result = session.Cast("caster","dbdx.arcano.geada",new[] { "z-center", "b-right", "a-left" });
            Assert.That(result.Success, Is.True);
            Assert.That(result.Destination, Is.EqualTo(new GridCell(3,1)));
            Assert.That(result.Impacts.Select(impact => impact.UnitId), Is.EqualTo(new[] { "a-left", "b-right", "z-center" }));
        }

        [Test]
        public void DeadPetCannotActOrBlockFutureSummoningForever()
        {
            var session = Duel(Native(HeroArchetype.DragonBinder), new ConstantDie(6));
            Assert.That(session.Cast("caster", "dbdx.draco.invocar", destination:new GridCell(2,1)).Success, Is.True);
            Assert.That(session.EndTurn("caster"), Is.True);
            Assert.That(session.Cast("enemy", "dbdx.basico.golpe", new[] { "pet:caster" }).Success, Is.True);
            Assert.That(session.Cast("enemy", "dbdx.basico.golpe", new[] { "pet:caster" }).Success, Is.True);
            Assert.That(session.PetId("caster"), Is.Null); Assert.That(session.Unit("pet:caster"), Is.Null);
            Assert.That(session.EndTurn("enemy"), Is.True);
            Assert.That(session.Cast("caster", "dbdx.draco.comandar", new[] { "enemy" }).Failure, Is.EqualTo(SkillFailure.MissingPet));
            Assert.That(session.Cast("caster", "dbdx.draco.invocar", destination:new GridCell(2,1)).Failure, Is.EqualTo(SkillFailure.Cooldown));
            NextCaster(session); NextCaster(session);
            Assert.That(session.Cast("caster", "dbdx.draco.invocar", destination:new GridCell(2,1)).Success, Is.True);
        }

        [Test]
        public void OwnerDeathDespawnsThePetAndEndsADefeatedTeam()
        {
            var session = Duel(Native(HeroArchetype.DragonBinder), new ConstantDie(6), enemy:new GridCell(2,1));
            Assert.That(session.Cast("caster", "dbdx.draco.invocar", destination:new GridCell(1,2)).Success, Is.True);
            for (int round = 0; round < 2; round++)
            {
                Assert.That(session.EndTurn("caster"), Is.True);
                for (int hit = 0; hit < 3; hit++) Assert.That(session.Cast("enemy", "dbdx.basico.golpe", new[] { "caster" }).Success, Is.True);
                if (!session.Finished) Assert.That(session.EndTurn("enemy"), Is.True);
            }
            Assert.That(session.Finished, Is.True); Assert.That(session.Unit("caster").Health, Is.Zero);
            Assert.That(session.PetId("caster"), Is.Null); Assert.That(session.Cast("caster", "dbdx.draco.pacto", new[] { "caster" }).Failure, Is.EqualTo(SkillFailure.Finished));
        }

        [Test]
        public void PetRecallIsPaidAndDoesNotResetSummonCooldownOrActions()
        {
            var session = Duel(Native(HeroArchetype.DragonBinder));
            Assert.That(session.Cast("caster", "dbdx.draco.invocar", destination:new GridCell(2,1)).Success, Is.True);
            Assert.That(session.Cast("caster", "dbdx.draco.invocar", destination:new GridCell(1,2)).Failure, Is.EqualTo(SkillFailure.Cooldown));
            Assert.That(session.Cast("caster", "dbdx.draco.recolher").Success, Is.True);
            Assert.That(session.PetId("caster"), Is.Null); Assert.That(session.RemainingActions, Is.Zero);
            Assert.That(session.HealingSpentThisTurn, Is.EqualTo(6)); Assert.That(session.Unit("caster").Health, Is.EqualTo(48));
            Assert.That(session.Cast("caster", "dbdx.draco.invocar", destination:new GridCell(2,1)).Failure, Is.EqualTo(SkillFailure.Cooldown));
        }

        [Test]
        public void ShieldStackingAcrossOwnersStaysCappedAtTwelveAndExpires()
        {
            var session = new DungeonSkillSession(new[] {
                new HeroSpawn("caster",0,Native(HeroArchetype.DragonBinder),new GridCell(1,1)),
                new HeroSpawn("ally",0,Native(HeroArchetype.Arcanist),new GridCell(1,2)),
                new HeroSpawn("enemy",1,Tank,new GridCell(8,8)) }, new DungeonSkillBoard(10,10));
            Assert.That(session.Cast("caster", "dbdx.draco.pacto", new[] { "caster" }).Success, Is.True);
            Assert.That(session.EndTurn("caster"), Is.True);
            Assert.That(session.Cast("ally", "dbdx.arcano.egide", new[] { "caster" }).Success, Is.True);
            Assert.That(session.Unit("caster").Shield, Is.EqualTo(12));
            Assert.That(session.EndTurn("ally"), Is.True); Assert.That(session.EndTurn("enemy"), Is.True);
            Assert.That(session.Unit("caster").Shield, Is.EqualTo(12)); NextCaster(session);
            Assert.That(session.Unit("caster").Shield, Is.Zero);
        }

        [Test]
        public void HealingSharesAreaBudgetAndNeverOverflowsMaxHealth()
        {
            var support = DungeonHeroProfiles.AdaptArena("alvyr", "Alvyr", 10, 0, 4);
            var session = new DungeonSkillSession(new[] {
                new HeroSpawn("caster",0,support,new GridCell(1,1)),
                new HeroSpawn("ally",0,Tank,new GridCell(1,2)), new HeroSpawn("enemy",1,Tank,new GridCell(2,1)) },
                new DungeonSkillBoard(10,10),random:new ConstantDie(6));
            Assert.That(session.EndTurn("caster"), Is.True); Assert.That(session.EndTurn("ally"), Is.True);
            for (int i = 0; i < 3; i++) Assert.That(session.Cast("enemy", "dbdx.basico.golpe", new[] { "caster" }).Success, Is.True);
            Assert.That(session.EndTurn("enemy"), Is.True); int before = session.Unit("caster").Health;
            var heal = session.Cast("caster", "dbdx.arcano.pulso", new[] { "caster", "ally" });
            Assert.That(heal.Success, Is.True); Assert.That(session.HealingSpentThisTurn, Is.EqualTo(12));
            Assert.That(session.Unit("caster").Health, Is.EqualTo(before + 4));
            Assert.That(session.Units.All(unit => unit.Health <= unit.MaxHealth && unit.Shield <= 12), Is.True);
        }

        [Test]
        public void MovementChecksOccupancyPathsAndWallsWhileTeleportCanCrossAWall()
        {
            var board = new DungeonSkillBoard(10,10,new[] { new GridCell(2,1) });
            var walker = Duel(Tank, board:board, enemy:new GridCell(8,8));
            Assert.That(walker.Cast("caster", "dbdx.basico.avanco", destination:new GridCell(3,1)).Failure, Is.EqualTo(SkillFailure.InvalidDestination));
            Assert.That(walker.RemainingActions, Is.EqualTo(3));
            var mage = Duel(Native(HeroArchetype.Arcanist),board:board,enemy:new GridCell(8,8));
            Assert.That(mage.Cast("caster", "dbdx.arcano.transposicao",destination:new GridCell(3,1)).Success, Is.True);
            Assert.That(mage.Unit("caster").Cell, Is.EqualTo(new GridCell(3,1)));
        }

        [Test]
        public void AttackLineOfSightCannotPassThroughWallsOrBlockedCorners()
        {
            var board = new DungeonSkillBoard(10,10,new[] { new GridCell(2,1) });
            var session = Duel(Native(HeroArchetype.Arcanist),board:board);
            Assert.That(session.Cast("caster", "dbdx.arcano.lanca",new[] { "enemy" }).Failure, Is.EqualTo(SkillFailure.BlockedSight));
            Assert.That(board.HasLineOfSight(new GridCell(1,1),new GridCell(2,2)), Is.False);
            Assert.That(session.RemainingActions, Is.EqualTo(3));
        }

        [Test]
        public void DisabledModulesDoNotDisableDamageCapsOrTheBasicFallback()
        {
            var tamer = Duel(Native(HeroArchetype.DragonBinder),new ConstantDie(6),SkillMechanics.None,enemy:new GridCell(2,1));
            Assert.That(tamer.Cast("caster", "dbdx.draco.invocar",destination:new GridCell(1,2)).Failure, Is.EqualTo(SkillFailure.DisabledMechanic));
            Assert.That(tamer.Cast("caster", "dbdx.basico.golpe",new[] { "enemy" }).Success, Is.True);
            Assert.That(tamer.DamageSpentThisTurn, Is.LessThanOrEqualTo(30));
            var mage = Duel(Native(HeroArchetype.Arcanist),new ConstantDie(6),SkillMechanics.None);
            Assert.That(mage.Cast("caster", "dbdx.arcano.transposicao",destination:new GridCell(1,2)).Failure, Is.EqualTo(SkillFailure.DisabledMechanic));
            Assert.That(mage.Cast("caster", "dbdx.arcano.geada",new[] { "enemy" }).Success, Is.True);
            Assert.That(mage.Unit("enemy").Slow, Is.Zero);
        }

        [Test]
        public void FriendlyFireIsAnIndependentExplicitCampaignChoice()
        {
            DungeonSkillSession Create(SkillMechanics rules) => new DungeonSkillSession(new[] {
                new HeroSpawn("caster",0,Native(HeroArchetype.Arcanist),new GridCell(1,1)),
                new HeroSpawn("ally",0,Tank,new GridCell(2,1)), new HeroSpawn("enemy",1,Tank,new GridCell(8,8)) },
                new DungeonSkillBoard(10,10), mechanics:rules,random:new ConstantDie(6));
            var safe = Create(SkillMechanics.Default);
            Assert.That(safe.Cast("caster","dbdx.arcano.lanca",new[] { "ally" }).Failure, Is.EqualTo(SkillFailure.InvalidTarget));
            var enabled = Create(SkillMechanics.Default | SkillMechanics.FriendlyFire);
            Assert.That(enabled.Cast("caster","dbdx.arcano.lanca",new[] { "ally" }).Success, Is.True);
            Assert.That(enabled.Unit("ally").Health, Is.LessThan(60));
        }

        [Test]
        public void IdenticalSeedAndCommandsReplayIdenticalDomainOutcomes()
        {
            var a = Duel(Native(HeroArchetype.Gunslinger)); var b = Duel(Native(HeroArchetype.Gunslinger));
            foreach (var session in new[] { a, b })
            {
                session.Cast("caster", "dbdx.pistola.mira"); session.Cast("caster", "dbdx.pistola.duplo", new[] { "enemy" });
                session.Cast("caster", "dbdx.pistola.duplo", new[] { "enemy" });
            }
            Assert.That(a.RandomState, Is.EqualTo(b.RandomState));
            Assert.That(a.Units.Select(unit => (unit.Id,unit.Health,unit.Shield,unit.Cell)), Is.EqualTo(b.Units.Select(unit => (unit.Id,unit.Health,unit.Shield,unit.Cell))));
            Assert.That(a.DamageSpentThisTurn, Is.EqualTo(b.DamageSpentThisTurn));
        }
    }
}
