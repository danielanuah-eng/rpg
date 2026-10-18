using System;
using System.IO;
using DungeonBDX.Arcadia;
using NUnit.Framework;

namespace DungeonBDX.Tests
{
    public sealed class AqPersistenceTests
    {
        const string Scenario="portais-em-ruinas";
        string root;
        string SavePath=>Path.Combine(root,"Arcadia-v1",Scenario+".json");
        static string Operation()=>Guid.NewGuid().ToString("N");
        static AqJournal Journal(string scenario=Scenario)=>new AqJournal(scenario,new[]
        {
            new[]{"colosso","alvyr","talyssa"},new[]{"damian","ciantia","diantus"}
        },19,false);
        static void Advance(AqJournal journal)=>Assert.That(journal.Session.Apply(new AqCommand("end")).Accepted,Is.True);

        [SetUp] public void SetUp()=>root=Path.Combine(Path.GetTempPath(),"DungeonBDX-AqPersistence",Guid.NewGuid().ToString("N"));
        [TearDown] public void TearDown()
        {
            var expected=Path.GetFullPath(Path.Combine(Path.GetTempPath(),"DungeonBDX-AqPersistence"))+Path.DirectorySeparatorChar;
            if(root!=null&&Path.GetFullPath(root).StartsWith(expected,StringComparison.OrdinalIgnoreCase)&&Directory.Exists(root))Directory.Delete(root,true);
        }

        [Test] public void EmptySlotThenRoundTripReplaysCommittedCommands()
        {
            var store=new AqLocalSave(root,Scenario);Assert.That(store.Load(),Is.Null);Assert.That(store.Revision,Is.Zero);
            var journal=Journal();Advance(journal);store.Save(journal,Operation());
            var reopened=new AqLocalSave(root,Scenario);var restored=reopened.Load();
            Assert.That(restored.Session.Fingerprint(),Is.EqualTo(journal.Session.Fingerprint()));
            Assert.That(reopened.Revision,Is.EqualTo(1));Assert.That(reopened.RecoveredBackup,Is.False);
        }

        [Test] public void CompactedHistorySavesItsSnapshotAndReplaysTheRest()
        {
            var store=new AqLocalSave(root,Scenario);var journal=Journal();
            while(journal.Session.Commands.Count<AqJournal.CompactAt)Advance(journal);
            Assert.That(journal.Compact(),Is.True);Advance(journal);store.Save(journal,Operation());
            Assert.That(File.ReadAllText(SavePath),Does.StartWith("{\"Version\":2"));
            var restored=new AqLocalSave(root,Scenario).Load();
            Assert.That(restored.Session.Fingerprint(),Is.EqualTo(journal.Session.Fingerprint()));
            Assert.That(restored.Session.Commands.Count,Is.EqualTo(1));Assert.That(restored.Compactions,Is.EqualTo(1));
            Advance(restored);store.Save(restored,Operation());Assert.That(store.Revision,Is.EqualTo(2));
        }

        [Test] public void UncompactedGamesKeepTheOldFileVersion()
        {
            var store=new AqLocalSave(root,Scenario);var journal=Journal();Advance(journal);store.Save(journal,Operation());
            Assert.That(File.ReadAllText(SavePath),Does.StartWith("{\"Version\":1"));
        }

        [Test] public void OperationRetryIsIdempotentEvenFromANewClient()
        {
            var journal=Journal();var op=Operation();new AqLocalSave(root,Scenario).Save(journal,op);
            var retry=new AqLocalSave(root,Scenario);retry.Save(journal,op);
            Assert.That(retry.Revision,Is.EqualTo(1));Assert.That(retry.Load().Session.Fingerprint(),Is.EqualTo(journal.Session.Fingerprint()));
        }

        [Test] public void SameOperationCannotCommitDifferentContent()
        {
            var journal=Journal();var store=new AqLocalSave(root,Scenario);var op=Operation();store.Save(journal,op);Advance(journal);
            Assert.Throws<IOException>(()=>store.Save(journal,op));Assert.That(store.Load().Session.Turn,Is.EqualTo(1));
        }

        [Test] public void StaleClientCannotOverwriteNewerCommands()
        {
            var first=new AqLocalSave(root,Scenario);var journal=Journal();first.Save(journal,Operation());
            var stale=new AqLocalSave(root,Scenario);var staleJournal=stale.Load();
            Advance(journal);first.Save(journal,Operation());
            Assert.Throws<IOException>(()=>stale.Save(staleJournal,Operation()));
            Assert.That(new AqLocalSave(root,Scenario).Load().Session.Fingerprint(),Is.EqualTo(journal.Session.Fingerprint()));
        }

        [Test] public void CorruptPrimaryRecoversBackupWithoutReusingTheLostRevision()
        {
            var original=new AqLocalSave(root,Scenario);var journal=Journal();original.Save(journal,Operation());
            var backupState=journal.Session.Fingerprint();Advance(journal);original.Save(journal,Operation());
            File.WriteAllText(SavePath,"truncated");
            var recovery=new AqLocalSave(root,Scenario);var recovered=recovery.Load();
            Assert.That(recovery.RecoveredBackup,Is.True);Assert.That(recovery.Revision,Is.EqualTo(3));
            Assert.That(recovered.Session.Fingerprint(),Is.EqualTo(backupState));
            Assert.Throws<IOException>(()=>original.Save(journal,Operation()));
            var reopened=new AqLocalSave(root,Scenario);Assert.That(reopened.Load().Session.Fingerprint(),Is.EqualTo(backupState));
            Assert.That(reopened.Revision,Is.EqualTo(3));Assert.That(reopened.RecoveredBackup,Is.False);
            Advance(recovered);recovery.Save(recovered,Operation());Assert.That(recovery.Revision,Is.EqualTo(4));
        }

        [Test] public void MissingPrimaryCanRecoverTheFirstBackup()
        {
            var journal=Journal();new AqLocalSave(root,Scenario).Save(journal,Operation());File.Delete(SavePath);
            var recovery=new AqLocalSave(root,Scenario);Assert.That(recovery.Load().Session.Fingerprint(),Is.EqualTo(journal.Session.Fingerprint()));
            Assert.That(recovery.RecoveredBackup,Is.True);Assert.That(recovery.Revision,Is.EqualTo(2));
        }

        [Test] public void ValidOldSnapshotsCannotRollBackTheRevisionWatermark()
        {
            var original=new AqLocalSave(root,Scenario);var journal=Journal();original.Save(journal,Operation());
            string old=File.ReadAllText(SavePath);Advance(journal);original.Save(journal,Operation());
            File.WriteAllText(SavePath,old);File.WriteAllText(SavePath+".bak",old);
            var recovery=new AqLocalSave(root,Scenario);recovery.Load();Assert.That(recovery.Revision,Is.EqualTo(3));
            Assert.Throws<IOException>(()=>original.Save(journal,Operation()));
        }

        [Test] public void ForeignJournalCannotBeSavedIntoAnotherScenarioSlot()
        {
            var store=new AqLocalSave(root,Scenario);
            Assert.Throws<ArgumentException>(()=>store.Save(Journal("resgate-nas-catacumbas"),Operation()));
            Assert.That(store.Exists,Is.False);
        }

        [Test] public void EvenValidHashedFilesFromAnotherScenarioAreRejected()
        {
            var journal=Journal("resgate-nas-catacumbas");new AqLocalSave(root,journal.Session.Scenario.Id).Save(journal,Operation());
            string foreign=Path.Combine(root,"Arcadia-v1",journal.Session.Scenario.Id+".json");
            File.Copy(foreign,SavePath);File.Copy(foreign,SavePath+".bak");
            Assert.Throws<InvalidDataException>(()=>new AqLocalSave(root,Scenario).Load());
        }

        [Test] public void TamperedSnapshotsRemainUntouchedWhenNeitherCanBeRecovered()
        {
            var store=new AqLocalSave(root,Scenario);store.Save(Journal(),Operation());
            const string broken="{\"Version\":1,\"Revision\":999}";
            File.WriteAllText(SavePath,broken);File.WriteAllText(SavePath+".bak",broken);
            Assert.Throws<InvalidDataException>(()=>store.Load());
            Assert.That(File.ReadAllText(SavePath),Is.EqualTo(broken));Assert.That(File.ReadAllText(SavePath+".bak"),Is.EqualTo(broken));
        }

        [Test] public void UnjournaledStateCannotReplaceAReplayableSave()
        {
            var store=new AqLocalSave(root,Scenario);var journal=Journal();store.Save(journal,Operation());
            string committed=File.ReadAllText(SavePath);journal.Session.Units[0].Health--;
            Assert.Throws<ArgumentException>(()=>store.Save(journal,Operation()));Assert.That(File.ReadAllText(SavePath),Is.EqualTo(committed));
        }

        [Test] public void ExclusiveWriterLockRejectsAConcurrentSave()
        {
            var store=new AqLocalSave(root,Scenario);store.Save(Journal(),Operation());
            using(var guard=new FileStream(SavePath+".lock",FileMode.Open,FileAccess.ReadWrite,FileShare.None))
                Assert.Throws<IOException>(()=>store.Save(Journal(),Operation()));
        }

        [Test] public void ExistingSlotWithoutWatermarkMigratesWithoutLosingItsRevision()
        {
            var store=new AqLocalSave(root,Scenario);var journal=Journal();store.Save(journal,Operation());Advance(journal);store.Save(journal,Operation());
            File.Delete(SavePath+".revision");var reopened=new AqLocalSave(root,Scenario);reopened.Load();
            Assert.That(reopened.Revision,Is.EqualTo(2));Assert.That(File.Exists(SavePath+".revision"),Is.True);
        }

        [Test] public void CorruptWatermarkFailsClosedAndPreservesSnapshots()
        {
            var store=new AqLocalSave(root,Scenario);store.Save(Journal(),Operation());string committed=File.ReadAllText(SavePath);
            File.WriteAllText(SavePath+".revision","2\nwrong hash");
            Assert.Throws<InvalidDataException>(()=>store.Load());Assert.Throws<InvalidDataException>(()=>store.Save(Journal(),Operation()));
            Assert.That(File.ReadAllText(SavePath),Is.EqualTo(committed));
        }

        [Test] public void UncommittedTemporaryFileDoesNotReplaceTheSavedGame()
        {
            var store=new AqLocalSave(root,Scenario);var journal=Journal();store.Save(journal,Operation());
            File.WriteAllText(SavePath+".tmp","interrupted write");
            Assert.That(store.Load().Session.Fingerprint(),Is.EqualTo(journal.Session.Fingerprint()));
            Advance(journal);store.Save(journal,Operation());Assert.That(store.Revision,Is.EqualTo(2));
        }

        [Test] public void DeletedSnapshotsWithAnExistingRevisionAreNotAnEmptySlot()
        {
            var store=new AqLocalSave(root,Scenario);store.Save(Journal(),Operation());File.Delete(SavePath);File.Delete(SavePath+".bak");
            Assert.That(store.Exists,Is.True);Assert.Throws<InvalidDataException>(()=>new AqLocalSave(root,Scenario).Load());
        }
    }
}
