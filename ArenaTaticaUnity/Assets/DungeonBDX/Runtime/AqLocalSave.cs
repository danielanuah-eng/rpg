using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DungeonBDX.Arcadia;
using UnityEngine;

namespace DungeonBDX
{
    // Separate scenario slots; replay validation, exclusive writer, atomic snapshots and monotonic revisions.
    public sealed class AqLocalSave
    {
        [Serializable] sealed class Envelope
        {public int Version=1;public long Revision;public string Operation,Digest;public AqRecord Record;}
        // Version 2: the compacted history keeps its state snapshot next to the remaining commands.
        [Serializable] sealed class Envelope2
        {public int Version=2;public long Revision;public string Operation,Digest;public AqRecord Record;public AqSnapshot Base;public int Compactions;}
        [Serializable] sealed class VersionProbe{public int Version;}
        // Common in-memory form of both file versions.
        sealed class Slot{public long Revision;public string Operation;public AqRecord Record;public AqSnapshot Base;public int Compactions;}
        const int MaxBytes=2*1024*1024;
        readonly string scenario,directory,path,backup,lockPath,revisionPath;
        public long Revision {get;private set;}
        public bool RecoveredBackup {get;private set;}
        public AqLocalSave(string root,string scenario)
        {
            if(!AqScenarios.Ids.Contains(scenario))throw new ArgumentException("Cenário inválido.");
            this.scenario=scenario;
            directory=Path.Combine(Path.GetFullPath(root),"Arcadia-v1");path=Path.Combine(directory,scenario+".json");backup=path+".bak";lockPath=path+".lock";revisionPath=path+".revision";
        }
        public bool Exists=>File.Exists(path)||File.Exists(backup)||File.Exists(revisionPath);
        public AqJournal Load()
        {
            Directory.CreateDirectory(directory);using(var guard=Lock())
            {
                RecoveredBackup=false;
                if(!Exists){Revision=0;return null;}
                var e=ReadRecover(out bool restored);Revision=e.Revision;RecoveredBackup=restored;return AqJournal.Restore(e.Record,e.Base,e.Compactions);
            }
        }
        public void Save(AqJournal journal,string operation)
        {
            if(journal==null||!Guid.TryParseExact(operation,"N",out _))throw new ArgumentException("Operação inválida.");
            var record=journal.Export();
            if(record.Scenario!=scenario)throw new ArgumentException("A partida pertence a outro cenário.");
            AqJournal.Restore(record,journal.Base,journal.Compactions); // Reject state changes that were not committed as replayable commands.
            Directory.CreateDirectory(directory);
            using(var guard=Lock())
            {
                Slot prior=null;if(Exists)prior=ReadRecover(out _);
                if(prior!=null&&prior.Operation==operation)
                {
                    if(JsonUtility.ToJson(prior.Record)!=JsonUtility.ToJson(record)||Json(prior.Base)!=Json(journal.Base))throw new IOException("Conflito na operação salva.");
                    Revision=prior.Revision;RecoveredBackup=false;return;
                }
                if((prior?.Revision??0)!=Revision)throw new IOException("Outro processo alterou a partida. Reabra o cenário.");
                var e=new Slot{Revision=checked(Revision+1),Operation=operation,Record=record,Base=journal.Base,Compactions=journal.Compactions};
                WriteSnapshot(e,true);Revision=e.Revision;RecoveredBackup=false;
            }
        }
        FileStream Lock()=>new FileStream(lockPath,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);
        static string Json(AqSnapshot s)=>s==null?"":JsonUtility.ToJson(s);
        Slot ReadRecover(out bool restored)
        {
            long high=ReadRevision();var primary=Read(path);var secondary=Read(backup);
            var e=primary;
            if(secondary!=null&&(e==null||secondary.Revision>e.Revision))e=secondary;
            if(e==null)throw new InvalidDataException("Partida e cópia de segurança inválidas; arquivos preservados.");
            high=Math.Max(high,Math.Max(primary?.Revision??0,secondary?.Revision??0));
            restored=e!=primary;
            if(restored||e.Revision<high)
            {
                // Recovery is a new committed state: clients holding the lost revision must reopen.
                e.Revision=checked(high+1);WriteSnapshot(e,false);
            }
            else if(ReadRevision()!=e.Revision)WriteRevision(e.Revision);
            return e;
        }
        Slot Read(string file)
        {
            if(!File.Exists(file))return null;
            try
            {
                if(new FileInfo(file).Length>MaxBytes)return null;
                string json=File.ReadAllText(file,new UTF8Encoding(false,true));var probe=JsonUtility.FromJson<VersionProbe>(json);
                Slot slot;
                if(probe!=null&&probe.Version==2)
                {
                    var e=JsonUtility.FromJson<Envelope2>(json);
                    if(e==null||e.Revision<1||e.Record==null||e.Base==null||e.Compactions<1||e.Record.Scenario!=scenario||!Guid.TryParseExact(e.Operation,"N",out _)||JsonUtility.ToJson(e)!=json)return null;
                    string digest=e.Digest;e.Digest="";if(Hash(JsonUtility.ToJson(e))!=digest)return null;
                    slot=new Slot{Revision=e.Revision,Operation=e.Operation,Record=e.Record,Base=e.Base,Compactions=e.Compactions};
                }
                else
                {
                    var e=JsonUtility.FromJson<Envelope>(json);
                    if(e==null||e.Version!=1||e.Revision<1||e.Record==null||e.Record.Scenario!=scenario||
                        !Guid.TryParseExact(e.Operation,"N",out _)||JsonUtility.ToJson(e)!=json)return null;
                    string digest=e.Digest;e.Digest="";if(Hash(JsonUtility.ToJson(e))!=digest)return null;
                    slot=new Slot{Revision=e.Revision,Operation=e.Operation,Record=e.Record};
                }
                AqJournal.Restore(slot.Record,slot.Base,slot.Compactions);return slot;
            }
            catch(Exception ex)when(ex is ArgumentException||ex is InvalidOperationException||ex is DecoderFallbackException){return null;}
        }
        long ReadRevision()
        {
            if(!File.Exists(revisionPath))return 0;
            if(new FileInfo(revisionPath).Length>128)throw new InvalidDataException("Revisão da partida inválida.");
            var parts=File.ReadAllText(revisionPath,Encoding.ASCII).Split('\n');
            if(parts.Length!=2||!long.TryParse(parts[0],NumberStyles.None,CultureInfo.InvariantCulture,out long value)||value<1||
                parts[0]!=value.ToString(CultureInfo.InvariantCulture)||parts[1]!=Hash(parts[0]))throw new InvalidDataException("Revisão da partida inválida.");
            return value;
        }
        void WriteRevision(long value)
        {
            string number=value.ToString(CultureInfo.InvariantCulture);
            AtomicWrite(revisionPath,number+"\n"+Hash(number),null);
        }
        void WriteSnapshot(Slot slot,bool preservePrevious)
        {
            // Uncompacted games keep writing version 1, byte-identical to older builds.
            string text;
            if(slot.Base==null){var e=new Envelope{Revision=slot.Revision,Operation=slot.Operation,Record=slot.Record,Digest=""};e.Digest=Hash(JsonUtility.ToJson(e));text=JsonUtility.ToJson(e);}
            else{var e=new Envelope2{Revision=slot.Revision,Operation=slot.Operation,Record=slot.Record,Base=slot.Base,Compactions=slot.Compactions,Digest=""};e.Digest=Hash(JsonUtility.ToJson(e));text=JsonUtility.ToJson(e);}
            if(Encoding.UTF8.GetByteCount(text)>MaxBytes)throw new IOException("Save excedeu o limite.");
            AtomicWrite(path,text,preservePrevious?backup:null);
            // Commit the snapshot first. A crash before the watermark is repaired from its revision.
            WriteRevision(slot.Revision);
            if(!File.Exists(backup))File.Copy(path,backup);
        }
        static void AtomicWrite(string destination,string text,string oldCopy)
        {
            string temp=destination+".tmp";byte[] bytes=Encoding.UTF8.GetBytes(text);
            using(var f=new FileStream(temp,FileMode.Create,FileAccess.Write,FileShare.None)){f.Write(bytes,0,bytes.Length);f.Flush(true);}
            if(File.Exists(destination))File.Replace(temp,destination,oldCopy);else File.Move(temp,destination);
        }
        static string Hash(string text)
        {using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-","");}
    }
}
