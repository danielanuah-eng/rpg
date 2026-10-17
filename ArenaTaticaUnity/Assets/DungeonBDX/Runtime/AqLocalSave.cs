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
                var e=ReadRecover(out bool restored);Revision=e.Revision;RecoveredBackup=restored;return AqJournal.Restore(e.Record);
            }
        }
        public void Save(AqJournal journal,string operation)
        {
            if(journal==null||!Guid.TryParseExact(operation,"N",out _))throw new ArgumentException("Operação inválida.");
            var record=journal.Export();
            if(record.Scenario!=scenario)throw new ArgumentException("A partida pertence a outro cenário.");
            AqJournal.Restore(record); // Reject state changes that were not committed as replayable commands.
            Directory.CreateDirectory(directory);
            using(var guard=Lock())
            {
                Envelope prior=null;if(Exists)prior=ReadRecover(out _);
                if(prior!=null&&prior.Operation==operation)
                {
                    if(JsonUtility.ToJson(prior.Record)!=JsonUtility.ToJson(record))throw new IOException("Conflito na operação salva.");
                    Revision=prior.Revision;RecoveredBackup=false;return;
                }
                if((prior?.Revision??0)!=Revision)throw new IOException("Outro processo alterou a partida. Reabra o cenário.");
                var e=new Envelope{Revision=checked(Revision+1),Operation=operation,Record=record};
                WriteSnapshot(e,true);Revision=e.Revision;RecoveredBackup=false;
            }
        }
        FileStream Lock()=>new FileStream(lockPath,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);
        Envelope ReadRecover(out bool restored)
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
        Envelope Read(string file)
        {
            if(!File.Exists(file))return null;
            try
            {
                if(new FileInfo(file).Length>MaxBytes)return null;
                string json=File.ReadAllText(file,new UTF8Encoding(false,true));var e=JsonUtility.FromJson<Envelope>(json);
                if(e==null||e.Version!=1||e.Revision<1||e.Record==null||e.Record.Scenario!=scenario||
                    !Guid.TryParseExact(e.Operation,"N",out _)||JsonUtility.ToJson(e)!=json)return null;
                string digest=e.Digest;e.Digest="";if(Hash(JsonUtility.ToJson(e))!=digest)return null;e.Digest=digest;
                AqJournal.Restore(e.Record);return e;
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
        void WriteSnapshot(Envelope e,bool preservePrevious)
        {
            e.Digest="";e.Digest=Hash(JsonUtility.ToJson(e));string text=JsonUtility.ToJson(e);
            if(Encoding.UTF8.GetByteCount(text)>MaxBytes)throw new IOException("Save excedeu o limite.");
            AtomicWrite(path,text,preservePrevious?backup:null);
            // Commit the snapshot first. A crash before the watermark is repaired from its revision.
            WriteRevision(e.Revision);
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
