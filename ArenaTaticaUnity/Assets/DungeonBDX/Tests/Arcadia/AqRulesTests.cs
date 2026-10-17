using System;
using System.Linq;
using DungeonBDX.Arcadia;
using NUnit.Framework;

namespace DungeonBDX.Tests
{
    public class AqRulesTests
    {
        static string[][] Parties()=>new[]{new[]{"colosso","alvyr","talyssa"},new[]{"damian","ciantia","diantus"}};
        static AqSession New=>new AqSession(AqScenarios.Ids[0],Parties(),29);
        static void ClearMonsters(AqSession s){foreach(var m in s.Units.Where(u=>u.Monster))m.Health=0;}
        static void Skip(AqSession s){for(int n=0;s.Reaction!=null&&n<50;n++)Assert.That(s.Apply(new AqCommand("react-skip")).Accepted);}
        static AqResult Command(AqSession s,string kind,int actor=-1,int target=-1,int card=-1,int x=0,int y=0)=>s.Apply(new AqCommand(kind,actor,target,card,x,y));
        [Test] public void GuildsHaveThreeHeroesFiveCardsAndLegalSharedSpawns()
        {
            var s=New;Assert.That(s.Guilds.Count,Is.EqualTo(2));
            foreach(var g in s.Guilds){var units=s.Units.Where(u=>u.Guild==g.Index).ToArray();Assert.That(units.Length,Is.EqualTo(3));Assert.That(units.Sum(u=>u.Gear.Count),Is.EqualTo(5));}
            foreach(var cell in s.Board.Spaces)Assert.That(s.Occupants(cell.Cell).Sum(u=>u.Occupancy),Is.LessThanOrEqualTo(2));
        }
        [Test] public void DuplicateHeroesAndMalformedGuildsAreRejected()
        {Assert.Throws<ArgumentException>(()=>new AqSession(AqScenarios.Ids[0],new[]{new[]{"alvyr","alvyr","colosso"},Parties()[1]}));}
        [TestCase("portais-em-ruinas")][TestCase("praca-dos-quatro-emblemas")][TestCase("resgate-nas-catacumbas")]
        public void ScenariosUseNineTilesAndEverySpaceCanBeReachedWithOpenDoors(string id)
        {
            var s=AqScenarios.Create(id);foreach(var e in s.Board.Edges.Where(e=>e.Door))e.Open=true;
            Assert.That(s.Board.Spaces.Select(p=>p.Tile).Distinct().Count(),Is.EqualTo(9));
            foreach(var p in s.Board.Spaces)Assert.That(s.Board.Path(new AqCell(1,6),p.Cell).Count,Is.GreaterThan(0),p.Cell.ToString()+" "+p.Room);
        }
        [Test] public void DoorCostsOneMovementAndNoAPSystem()
        {
            var s=New;var r=Command(s,"door",1,x:2,y:7);Assert.That(r.Accepted,Is.True,r.Message);Assert.That(s.MovesLeft,Is.EqualTo(2));Assert.That(s.ActiveId,Is.EqualTo(1));Assert.That(s.Board.Passage(new AqCell(2,6),new AqCell(2,7)));
        }
        [Test] public void CannotActivateAnotherHeroOrRestAfterActing()
        {var s=New;Command(s,"door",1,x:2,y:7);Assert.That(Command(s,"rest").Accepted,Is.False);Assert.That(Command(s,"move",2,x:2,y:7).Accepted,Is.False);}
        [Test] public void InvalidMoveDoesNotConsumeMovementRandomOrJournal()
        {var s=New;string before=s.Fingerprint();Assert.That(Command(s,"move",1,x:8,y:8).Accepted,Is.False);Assert.That(s.Fingerprint(),Is.EqualTo(before));Assert.That(s.Commands.Count,Is.Zero);}
        [Test] public void TwoEnemiesBlockAndTwoAlliesOnlyPreventStopping()
        {
            var s=New;ClearMonsters(s);var a=s.Unit(1);a.Cell=new AqCell(4,7);var b=s.Unit(3);var c=s.Unit(4);b.Cell=c.Cell=new AqCell(3,7);
            Assert.That(s.Blocked(a,b.Cell));Assert.That(s.CanStop(a,b.Cell),Is.False);b.Guild=a.Guild;
            Assert.That(s.Blocked(a,b.Cell),Is.False);Assert.That(s.CanStop(a,b.Cell),Is.False);
        }
        [Test] public void RangedTargetsSameSpaceAndUnlimitedClearDistance()
        {
            var s=New;ClearMonsters(s);foreach(var edge in s.Board.Edges)edge.Open=edge.Door=true;
            var a=s.Unit(1);var b=s.Unit(3);a.Cell=new AqCell(0,4);b.Cell=new AqCell(8,4);
            Assert.That(s.CanTarget(a,b,a.Gear[0]));b.Cell=a.Cell;Assert.That(s.CanTarget(a,b,a.Gear[0]));
        }
        [Test] public void ClosedDoorBlocksSightButOpenDoorDoesNot()
        {var b=new AqBoard(3,3);b.SetEdge(new AqCell(0,1),new AqCell(1,1),true,false);Assert.That(b.Sight(new AqCell(0,1),new AqCell(2,1)),Is.False);b.Edges[0].Open=true;Assert.That(b.Sight(new AqCell(0,1),new AqCell(2,1)));}
        [Test] public void SightCanGrazeOneCornerButNotCrossTwoOpposingWalls()
        {
            var b=new AqBoard(3,3);b.SetEdge(new AqCell(0,0),new AqCell(1,0));Assert.That(b.Sight(new AqCell(0,0),new AqCell(1,1)));
            b.SetEdge(new AqCell(0,0),new AqCell(0,1));Assert.That(b.Sight(new AqCell(0,0),new AqCell(1,1)),Is.False);
        }
        [Test] public void MeleeDoesNotReachAcrossWallOrDiagonally()
        {var b=new AqBoard(3,3);b.SetEdge(new AqCell(0,0),new AqCell(1,0));Assert.That(b.Close(new AqCell(0,0),new AqCell(1,0)),Is.False);Assert.That(b.Close(new AqCell(0,0),new AqCell(1,1)),Is.False);Assert.That(b.Close(new AqCell(0,0),new AqCell(0,0)));}
        [Test] public void AttackExhaustsCardAndOnlyOneAttackIsAllowed()
        {
            var s=New;ClearMonsters(s);var a=s.Unit(1);var b=s.Unit(3);b.Cell=a.Cell;
            Assert.That(Command(s,"attack",a.Id,b.Id,0).Accepted);Assert.That(a.Gear[0].Exhausted);Assert.That(Command(s,"attack",a.Id,b.Id,0).Accepted,Is.False);
        }
        [Test] public void RestRestoresCardsButDoesNotHealLivingHeroes()
        {var s=New;var a=s.Unit(1);a.Gear[0].Exhausted=true;a.Health--;int hp=a.Health;Assert.That(Command(s,"rest").Accepted);Assert.That(a.Gear[0].Exhausted,Is.False);Assert.That(a.Health,Is.EqualTo(hp));Assert.That(Command(s,"move",1,x:2,y:7).Accepted,Is.False);}
        [Test] public void RestResurrectionUsesOnlyExistingAlliesOrStartingArea()
        {
            var s=New;var a=s.Unit(2);a.Health=0;Command(s,"rest");Assert.That(Command(s,"revive",2,x:8,y:8).Accepted,Is.False);
            var cell=s.ResurrectionCells(a).First();Assert.That(Command(s,"revive",2,x:cell.X,y:cell.Y).Accepted);Assert.That(a.Health,Is.EqualTo(a.MaxHealth));
        }
        [Test] public void GearTransferIsOnlyDuringRestAndRespectsFourSlots()
        {
            var s=New;Assert.That(Command(s,"transfer",0,1,0).Accepted,Is.False);Command(s,"rest");Assert.That(Command(s,"transfer",0,1,0).Accepted);
            while(s.Unit(1).Gear.Count<4)s.Unit(1).Gear.Add(new AqGear());Assert.That(Command(s,"transfer",0,1,0).Accepted,Is.False);
        }
        [Test] public void MoveBeforeAttackCannotContinueAfterAttack()
        {
            var s=New;ClearMonsters(s);Command(s,"door",1,x:2,y:7);Assert.That(Command(s,"move",1,x:2,y:7).Accepted);s.Unit(3).Cell=s.Unit(1).Cell;
            Assert.That(Command(s,"attack",1,3,0).Accepted);Assert.That(s.MovementClosed);Assert.That(Command(s,"move",1,x:3,y:7).Accepted,Is.False);
        }
        [Test] public void LeavingGuardedSpacePausesBeforeMovingForRightHandPlayer()
        {
            var s=New;ClearMonsters(s);var a=s.Unit(1);a.Cell=new AqCell(4,7);var m=s.Units.First(u=>u.Monster);m.Health=m.MaxHealth;m.Cell=new AqCell(3,7);
            var r=Command(s,"move",1,x:5,y:7);Assert.That(r.Accepted,Is.True,r.Message);Assert.That(s.Reaction,Is.Not.Null);Assert.That(a.Cell,Is.EqualTo(new AqCell(4,7)));Assert.That(s.Controller,Is.EqualTo(1));
            Skip(s);Assert.That(a.Cell,Is.EqualTo(new AqCell(5,7)));
        }
        [Test] public void VengeanceCanMoveButGuardCannot()
        {
            var s=New;ClearMonsters(s);var a=s.Unit(1);a.Cell=new AqCell(4,7);var m=s.Units.First(u=>u.Monster);m.Health=99;m.Cell=new AqCell(3,7);
            Command(s,"attack",1,m.Id,0);Assert.That(s.Reaction.Vengeance);Assert.That(s.Reaction.Movement,Is.GreaterThan(0));
        }
        [Test] public void PortalDoesNotTriggerGuard()
        {
            var s=New;ClearMonsters(s);var a=s.Unit(1);a.Cell=new AqCell(1,8);var m=s.Units.First(u=>u.Monster);m.Health=m.MaxHealth;m.Cell=a.Cell;
            var r=Command(s,"move",1,x:7,y:8);Assert.That(r.Accepted,Is.True,r.Message);Assert.That(s.Reaction,Is.Null);Assert.That(s.MovesLeft,Is.EqualTo(2));
        }
        [Test] public void EveryHeroHasItsOwnNaturalAbility()
        {Assert.That(AqContent.Heroes.Count,Is.EqualTo(31));Assert.That(AqContent.Heroes.Values.Select(h=>h.Ability).Distinct().Count(),Is.EqualTo(31));Assert.That(AqContent.Heroes.Values.All(h=>h.AbilityText.Length>20));}
        [Test] public void FreeDoorStillStartsMovementAndAttackClosesThatMovement()
        {
            var p=Parties();p[0][1]="brumak";var s=new AqSession(AqScenarios.Ids[0],p);ClearMonsters(s);
            Assert.That(Command(s,"door",1,x:2,y:7).Accepted);Assert.That(s.MovesLeft,Is.EqualTo(3));
            s.Unit(3).Cell=s.Unit(1).Cell;Assert.That(Command(s,"attack",1,3,0).Accepted);
            Assert.That(s.MovementClosed);Assert.That(Command(s,"move",1,x:2,y:7).Accepted,Is.False);
        }
        [Test] public void MonsterEntryRequiresAnEntirelyEmptySpace()
        {
            for(uint seed=1;seed<=64;seed++)
            {
                var s=new AqSession(AqScenarios.Ids[0],Parties(),seed);ClearMonsters(s);
                var markers=s.Scenario.Tokens.Where(t=>t.Kind=="spawn").ToArray();Assert.That(markers.Length,Is.EqualTo(4));
                for(int i=0;i<markers.Length;i++)s.Unit(i).Cell=markers[i].Cell;
                var monster=s.Units.First(u=>u.Monster);s.SpawnQueue.Add(monster.Id);Assert.That(Command(s,"end").Accepted);
                Assert.That(monster.Alive,Is.False,"seed "+seed);Assert.That(s.SpawnQueue,Is.Empty);
            }
        }
        [Test] public void TeanthaCanCrossOneClosedDoorWithoutOpeningIt()
        {
            var p=Parties();p[0][1]="teantha";var s=new AqSession(AqScenarios.Ids[0],p);var r=Command(s,"phase",1,x:2,y:7);
            Assert.That(r.Accepted,Is.True,r.Message);Assert.That(s.Board.Edge(new AqCell(2,6),new AqCell(2,7)).Open,Is.False);Assert.That(s.MovesLeft,Is.EqualTo(2));Assert.That(Command(s,"phase",1,x:2,y:6).Accepted,Is.False);
        }
        [Test] public void DamianSwitchesRangeAndAuthoredAnimation()
        {
            var p=Parties();p[0][1]="damian";p[1][0]="alvyr";var s=new AqSession(AqScenarios.Ids[0],p);Assert.That(Command(s,"stance",1).Accepted);Assert.That(s.Unit(1).Gear[0].Ranged,Is.False);Assert.That(s.Unit(1).Gear[0].Clip,Is.EqualTo("hammer"));
        }
        [Test] public void JournalReplaysDoorMovementReactionAndRestExactly()
        {
            var j=new AqJournal(AqScenarios.Ids[0],Parties(),29,false);Command(j.Session,"door",1,x:2,y:7);Command(j.Session,"move",1,x:3,y:7);Skip(j.Session);Command(j.Session,"end");Command(j.Session,"rest");
            var restored=AqJournal.Restore(j.Export());Assert.That(restored.Session.Fingerprint(),Is.EqualTo(j.Session.Fingerprint()));
        }
        [Test] public void TamperedJournalIsRejected()
        {var j=new AqJournal(AqScenarios.Ids[0],Parties(),29,false);var data=j.Export();data.Fingerprint+="bad";Assert.Throws<ArgumentException>(()=>AqJournal.Restore(data));}
        [Test] public void CompletingTwoPveQuestsWinsWithoutEliminatingGuild()
        {
            var s=New;ClearMonsters(s);var u=s.Unit(1);var clues=s.Scenario.Tokens.Where(t=>t.Kind=="clue").Take(2).ToArray();
            foreach(var t in clues){t.Taken=true;t.Owner=u.Id;u.Carry.Add(t.Id);}var relic=s.Scenario.Tokens.First(t=>t.Kind=="relic");relic.Taken=true;relic.Owner=u.Id;u.Carry.Add(relic.Id);
            u.Cell=new AqCell(0,8);var r=Command(s,"move",1,x:1,y:8);Assert.That(r.Accepted,Is.True,r.Message);Assert.That(s.Winner,Is.EqualTo(0));Assert.That(s.Units.Where(v=>v.Guild==1).All(v=>v.Alive));
        }
    }
}
