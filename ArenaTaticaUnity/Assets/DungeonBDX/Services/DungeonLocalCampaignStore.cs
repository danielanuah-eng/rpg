using System;
using System.IO;
using System.Globalization;
using System.Text;
using DungeonBDX.Skills;
using UnityEngine;

namespace DungeonBDX.Services
{
    public sealed class SaveConflictException : IOException
    { public SaveConflictException() : base("Local save revision changed. Reload before writing.") { } }
    public sealed class StoredCampaign
    {
        public CampaignJournal Journal { get; }
        public long Revision { get; }
        public bool RecoveredBackup { get; }
        internal StoredCampaign(CampaignJournal journal, long revision, bool recovered)
        { Journal = journal; Revision = revision; RecoveredBackup = recovered; }
    }

    // One offline profile/slot per directory. Paths never incorporate unvalidated player IDs.
    public sealed class LocalCampaignStore
    {
        [Serializable] sealed class Envelope
        { public int schema; public string profile; public long revision; public string operation, journal, digest; }
        readonly string directory, primary, backup, temporary, lockPath, profile, revisionPath;
        const int MaxFileBytes = CampaignJournal.MaxJsonBytes * 2 + 8192;
        public LocalCampaignStore(string rootDirectory, string offlineProfileId)
        {
            if (string.IsNullOrEmpty(rootDirectory)) throw new ArgumentException("Missing app storage directory.");
            if (string.IsNullOrEmpty(offlineProfileId) || offlineProfileId.Length > 32) throw new ArgumentException("Invalid offline profile.");
            foreach (char c in offlineProfileId)
                if (!(c >= 'a' && c <= 'z') && !(c >= '0' && c <= '9') && c != '-' && c != '_')
                    throw new ArgumentException("Invalid offline profile.");
            profile = offlineProfileId;
            directory = Path.Combine(Path.GetFullPath(rootDirectory), "offline-" + profile);
            primary = Path.Combine(directory, "campaign.json"); backup = primary + ".bak";
            temporary = primary + ".tmp"; lockPath = Path.Combine(directory, "writer.lock");
            revisionPath = Path.Combine(directory, "revision.txt");
        }

        public StoredCampaign Load(Func<string, DungeonHeroProfile> profileResolver, Func<ulong, string, ISkillBoard> boardFactory)
        {
            Directory.CreateDirectory(directory);
            using (AcquireWriter())
            {
                bool found = false;
                foreach (string path in new[] { primary, backup })
                {
                    if (!File.Exists(path)) continue;
                    found = true;
                    try
                    {
                        var envelope = Read(path);
                        var journal = CampaignJournal.Restore(envelope.journal, profileResolver, boardFactory);
                        return new StoredCampaign(journal, envelope.revision, path == backup);
                    }
                    catch (Exception error) when (IsInvalid(error)) { }
                }
                if (found) throw new InvalidDataException("No valid local campaign or backup could be replayed.");
                return null;
            }
        }

        public long Save(CampaignJournal journal, long expectedRevision, string operationId)
        {
            if (journal == null) throw new ArgumentNullException(nameof(journal));
            if (expectedRevision < 0 || expectedRevision == long.MaxValue) throw new ArgumentOutOfRangeException(nameof(expectedRevision));
            CampaignJournal.CheckId(operationId);
            string payload = journal.Export();
            Directory.CreateDirectory(directory);
            using (AcquireWriter())
            {
                Envelope current = null; bool recovered = false, existed = false;
                foreach (string path in new[] { primary, backup })
                {
                    if (!File.Exists(path)) continue;
                    existed = true;
                    try { current = Read(path); recovered = path == backup; break; }
                    catch (Exception error) when (IsInvalid(error)) { }
                }
                if (current == null && existed) throw new InvalidDataException("Refusing to overwrite corrupt campaign history.");
                if (current != null && current.operation == operationId)
                {
                    if (current.journal != payload) throw new SaveConflictException();
                    if (recovered) AtomicText(primary, JsonUtility.ToJson(current));
                    return current.revision;
                }
                if ((current?.revision ?? 0) != expectedRevision) throw new SaveConflictException();
                long watermark = ReadWatermark();
                if (watermark == long.MaxValue) throw new InvalidDataException("Local revision limit reached.");
                var next = new Envelope { schema = 1, profile = profile, revision = Math.Max(expectedRevision, watermark) + 1,
                    operation = operationId, journal = payload, digest = "" };
                next.digest = CampaignJournal.Hash(JsonUtility.ToJson(next));
                byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(next));
                if (bytes.Length > MaxFileBytes) throw new InvalidDataException("Local save size limit exceeded.");
                using (var file = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
                { file.Write(bytes, 0, bytes.Length); file.Flush(true); }
                // Persist a high-water mark first. An interrupted transaction can leave a harmless gap,
                // but restoring a backup never reuses a revision another running client observed.
                string revision = next.revision.ToString(CultureInfo.InvariantCulture);
                AtomicText(revisionPath, revision + ":" + CampaignJournal.Hash(profile + ":" + revision));
                if (File.Exists(primary)) File.Replace(temporary, primary, recovered ? null : backup);
                else File.Move(temporary, primary);
                if (!File.Exists(backup)) AtomicText(backup, JsonUtility.ToJson(next));
                return next.revision;
            }
        }

        FileStream AcquireWriter()
        {
            try { return new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
            catch (IOException) { throw new SaveConflictException(); }
        }
        long ReadWatermark()
        {
            if (!File.Exists(revisionPath)) return 0;
            if (new FileInfo(revisionPath).Length > 128) throw new InvalidDataException("Revision metadata invalid.");
            string[] parts = File.ReadAllText(revisionPath).Split(':');
            if (parts.Length != 2 || !long.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out long value) ||
                value < 1 || CampaignJournal.Hash(profile + ":" + parts[0]) != parts[1])
                throw new InvalidDataException("Revision metadata corrupt.");
            return value;
        }
        static void AtomicText(string path, string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            using (var stream = new FileStream(path + ".tmp", FileMode.Create, FileAccess.Write, FileShare.None))
            { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
            if (File.Exists(path)) File.Replace(path + ".tmp", path, null); else File.Move(path + ".tmp", path);
        }
        Envelope Read(string path)
        {
            var info = new FileInfo(path);
            if (info.Length < 1 || info.Length > MaxFileBytes) throw new InvalidDataException("Local save size invalid.");
            string text = File.ReadAllText(path, new UTF8Encoding(false, true));
            Envelope value;
            try { value = JsonUtility.FromJson<Envelope>(text); }
            catch (Exception error) { throw new InvalidDataException("Local save JSON invalid.", error); }
            if (value == null || value.schema != 1 || value.profile != profile || value.revision < 1 || JsonUtility.ToJson(value) != text)
                throw new InvalidDataException("Local save schema or profile mismatch.");
            CampaignJournal.CheckId(value.operation);
            string digest = value.digest; value.digest = "";
            if (CampaignJournal.Hash(JsonUtility.ToJson(value)) != digest) throw new InvalidDataException("Local save integrity mismatch.");
            value.digest = digest; CampaignJournal.Decode(value.journal); return value;
        }
        static bool IsInvalid(Exception error) => error is FormatException || error is ArgumentException ||
            error is InvalidDataException || error is InvalidOperationException || error is DecoderFallbackException;
    }
}
