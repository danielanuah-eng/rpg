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
        readonly string[] heroes;
        readonly uint seed;
        public AqSession Session {get;}
        public AqJournal(string scenario,string[][] parties,uint seed,bool autoMonsters)
        {Session=new AqSession(scenario,parties,seed,autoMonsters);heroes=parties.SelectMany(p=>p).ToArray();this.seed=seed;}
        public AqRecord Export()=>new AqRecord{Scenario=Session.Scenario.Id,Seed=seed,AutoMonsters=Session.AutoMonsters,Heroes=heroes.ToArray(),
            Commands=Session.Commands.Select(c=>new AqCommand(c.Kind,c.Actor,c.Target,c.Card,c.X,c.Y)).ToArray(),Fingerprint=Session.Fingerprint()};
        public static AqJournal Restore(AqRecord record)
        {
            if(record==null||record.Version!=1||record.Heroes==null||record.Heroes.Length<6||record.Heroes.Length>12||record.Heroes.Length%3!=0||
                record.Commands==null||record.Commands.Length>4096||record.Fingerprint==null||record.Fingerprint.Length>65536)throw new ArgumentException("Save AQ inválido.");
            var groups=Enumerable.Range(0,record.Heroes.Length/3).Select(i=>record.Heroes.Skip(i*3).Take(3).ToArray()).ToArray();
            var journal=new AqJournal(record.Scenario,groups,record.Seed,record.AutoMonsters);
            foreach(var c in record.Commands)
                if(c==null||c.Kind==null||c.Kind.Length>32||!journal.Session.Apply(c).Accepted)throw new ArgumentException("Comando AQ inválido no histórico.");
            if(journal.Session.Fingerprint()!=record.Fingerprint)throw new ArgumentException("Histórico AQ não corresponde ao estado salvo.");
            return journal;
        }
    }
}
