using System;
using System.Linq;

namespace DungeonBDX.Arcadia
{
    [Serializable] public sealed class AqRecord
    {
        public int Version=1;
        public string Scenario;
        public uint Seed;
        public bool AutoMonsters;
        public string[] Heroes;
        public AqCommand[] Commands;
        public string Fingerprint;
    }
    public sealed class AqJournal
    {
        // Hidden command meter: at half the session limit the history is folded into a state snapshot (Base) at a turn boundary.
        public const int CompactAt=AqSession.MaxCommands/2;
        readonly string[] heroes;
        readonly uint seed;
        public AqSession Session {get;}
        public AqSnapshot Base {get;private set;}
        public int Compactions {get;private set;}
        public float CommandLoad=>Session.Commands.Count/(float)AqSession.MaxCommands;
        public bool ShouldCompact=>Session.Commands.Count>=CompactAt&&Session.ActiveId<0&&Session.Reaction==null&&!Session.Resting&&Session.Winner<0;
        public AqJournal(string scenario,string[][] parties,uint seed,bool autoMonsters)
        {Session=new AqSession(scenario,parties,seed,autoMonsters);heroes=parties.SelectMany(p=>p).ToArray();this.seed=seed;}
        public AqRecord Export()=>new AqRecord{Scenario=Session.Scenario.Id,Seed=seed,AutoMonsters=Session.AutoMonsters,Heroes=heroes.ToArray(),
            Commands=Session.Commands.Select(c=>new AqCommand(c.Kind,c.Actor,c.Target,c.Card,c.X,c.Y)).ToArray(),Fingerprint=Session.Fingerprint()};
        // Folds the replayed history into Base; the game state (fingerprint) does not change.
        public bool Compact()
        {
            if(!ShouldCompact)return false;
            string before=Session.Fingerprint();var snapshot=Session.Snapshot();
            var check=new AqJournal(Session.Scenario.Id,Groups(heroes),seed,Session.AutoMonsters);check.Session.Restore(Session.Snapshot());
            if(check.Session.Fingerprint()!=before)return false;
            Base=snapshot;Compactions++;Session.ClearHistory();return true;
        }
        static string[][] Groups(string[] heroes)=>Enumerable.Range(0,heroes.Length/3).Select(i=>heroes.Skip(i*3).Take(3).ToArray()).ToArray();
        public static AqJournal Restore(AqRecord record,AqSnapshot baseState=null,int compactions=0)
        {
            if(record==null||record.Version!=1||record.Heroes==null||record.Heroes.Length<6||record.Heroes.Length>12||record.Heroes.Length%3!=0||
                record.Commands==null||record.Commands.Length>AqSession.MaxCommands||record.Fingerprint==null||record.Fingerprint.Length>65536||compactions<0)throw new ArgumentException("Save AQ inválido.");
            var journal=new AqJournal(record.Scenario,Groups(record.Heroes),record.Seed,record.AutoMonsters);
            if(baseState!=null)
            {
                try{journal.Session.Restore(baseState);}
                catch(Exception e)when(!(e is ArgumentException)){throw new ArgumentException("Retrato AQ inválido.");}
                journal.Base=baseState;journal.Compactions=Math.Max(1,compactions);
            }
            foreach(var c in record.Commands)
                if(c==null||c.Kind==null||c.Kind.Length>32||!journal.Session.Apply(c).Accepted)throw new ArgumentException("Comando AQ inválido no histórico.");
            if(journal.Session.Fingerprint()!=record.Fingerprint)throw new ArgumentException("Histórico AQ não corresponde ao estado salvo.");
            return journal;
        }
    }
}
