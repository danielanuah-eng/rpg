using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DungeonBDX.Combat;
using DungeonBDX.Skills;
using UnityEngine;

namespace DungeonBDX.Services
{
    // Integrity checks detect damaged local files; they do not authorize network players.
    public sealed class CampaignJournal
    {
        public const int SchemaVersion = 1, MaxCommands = 4096, MaxJsonBytes = 1024 * 1024;
        [Serializable] internal sealed class SpawnData
        { public string id, profile, signature; public int team, x, y; }
        [Serializable] internal sealed class CommandData
        { public string kind = "", actor = "", skill = ""; public string[] targets = Array.Empty<string>(); public bool hasDestination; public int x, y; }
        [Serializable] internal sealed class JournalData
        {
            public int schema; public string seed, scenario, combatVersion, skillVersion, profileVersion;
            public int mechanics, campaignFlags, mapVersion, width, height;
            public SpawnData[] spawns; public CommandData[] commands; public string state, digest;
        }
        readonly JournalData data;
        readonly List<CommandData> commands = new List<CommandData>();
        string ownedState;
        public DungeonSkillSession Session { get; }
        public ulong Seed { get; }
        public string Scenario => data.scenario;
        public SkillMechanics Mechanics => (SkillMechanics)data.mechanics;
        public int CampaignFlags => data.campaignFlags;
        public int MapVersion => data.mapVersion;
        public int Width => data.width;
        public int Height => data.height;
        public int CommandCount => commands.Count;

        CampaignJournal(JournalData data, DungeonSkillSession session, ulong seed)
        { this.data = data; Session = session; Seed = seed; ownedState = StateDigest(session); }

        public static CampaignJournal Create(ulong seed, string scenario, SkillMechanics mechanics,
            IEnumerable<HeroSpawn> spawns, ISkillBoard board, int campaignFlags = 0,
            int mapVersion = 1, int width = 11, int height = 11)
        {
            CheckId(scenario); CheckMechanics((int)mechanics);
            CheckMap(campaignFlags, mapVersion, width, height);
            var initial = (spawns ?? throw new ArgumentNullException(nameof(spawns))).Take(33).ToArray();
            if (initial.Length < 2 || initial.Length > 32) throw new ArgumentException("Invalid spawn count.");
            var descriptors = initial.Select(spawn => {
                if (spawn == null || spawn.Profile == null) throw new ArgumentException("Missing profile.");
                CheckId(spawn.Id); CheckId(spawn.Profile.Id);
                return new SpawnData { id = spawn.Id, profile = spawn.Profile.Id, signature = ProfileDigest(spawn.Profile),
                    team = spawn.Team, x = spawn.Cell.X, y = spawn.Cell.Y };
            }).ToArray();
            var data = new JournalData { schema = SchemaVersion, seed = seed.ToString(CultureInfo.InvariantCulture),
                scenario = scenario, combatVersion = CombatCaps.Version, skillVersion = DungeonSkillCatalog.Version,
                profileVersion = DungeonHeroProfiles.ConversionVersion, mechanics = (int)mechanics,
                campaignFlags = campaignFlags, mapVersion = mapVersion, width = width, height = height,
                spawns = descriptors, commands = Array.Empty<CommandData>() };
            return new CampaignJournal(data, new DungeonSkillSession(initial, board, seed, mechanics), seed);
        }

        public SkillUseResult Cast(string actorId, string skillId, IReadOnlyList<string> targetIds = null, GridCell? destination = null)
        {
            BeforeCommand(); CheckId(actorId); CheckId(skillId);
            var targets = (targetIds ?? Array.Empty<string>()).ToArray();
            if (targets.Length > 3) throw new ArgumentException("Too many targets.");
            foreach (string target in targets) CheckId(target);
            var result = Session.Cast(actorId, skillId, targets, destination);
            if (result.Success) Append(new CommandData { kind = "cast", actor = actorId, skill = skillId, targets = targets,
                hasDestination = destination.HasValue, x = destination?.X ?? 0, y = destination?.Y ?? 0 });
            return result;
        }

        public SkillFailure Move(string actorId, GridCell destination)
        {
            BeforeCommand(); CheckId(actorId);
            var result = Session.Move(actorId, destination);
            if (result == SkillFailure.None) Append(new CommandData { kind = "move", actor = actorId,
                hasDestination = true, x = destination.X, y = destination.Y });
            return result;
        }

        public bool EndTurn(string actorId)
        {
            BeforeCommand(); CheckId(actorId);
            bool result = Session.EndTurn(actorId);
            if (result) Append(new CommandData { kind = "end", actor = actorId });
            return result;
        }

        void BeforeCommand()
        {
            EnsureOwnedState();
            if (commands.Count >= MaxCommands) throw new InvalidOperationException("Local journal command limit reached.");
        }
        void EnsureOwnedState()
        {
            if (ownedState != StateDigest(Session)) throw new InvalidOperationException("Session changed outside its journal.");
        }
        void Append(CommandData command) { commands.Add(command); ownedState = StateDigest(Session); }

        public string Export()
        {
            EnsureOwnedState(); data.commands = commands.ToArray(); data.state = ownedState; data.digest = "";
            data.digest = Hash(JsonUtility.ToJson(data));
            string json = JsonUtility.ToJson(data); CheckSize(json); return json;
        }

        internal static JournalData Decode(string json)
        {
            CheckSize(json);
            JournalData data;
            try { data = JsonUtility.FromJson<JournalData>(json); }
            catch (Exception error) { throw new FormatException("Invalid campaign JSON.", error); }
            if (data == null || data.schema != SchemaVersion || data.combatVersion != CombatCaps.Version ||
                data.skillVersion != DungeonSkillCatalog.Version || data.profileVersion != DungeonHeroProfiles.ConversionVersion ||
                data.spawns == null || data.spawns.Length < 2 ||
                data.spawns.Length > 32 || data.commands == null || data.commands.Length > MaxCommands ||
                JsonUtility.ToJson(data) != json) throw new FormatException("Unknown or noncanonical campaign schema.");
            string checksum = data.digest; data.digest = "";
            if (string.IsNullOrEmpty(checksum) || Hash(JsonUtility.ToJson(data)) != checksum) throw new FormatException("Campaign integrity mismatch.");
            data.digest = checksum; CheckId(data.scenario); CheckMechanics(data.mechanics);
            CheckMap(data.campaignFlags, data.mapVersion, data.width, data.height);
            if (!ulong.TryParse(data.seed, NumberStyles.None, CultureInfo.InvariantCulture, out ulong seed) ||
                seed.ToString(CultureInfo.InvariantCulture) != data.seed) throw new FormatException("Invalid seed.");
            return data;
        }

        public static CampaignJournal Restore(string json, Func<string, DungeonHeroProfile> profileResolver,
            Func<ulong, string, ISkillBoard> boardFactory, int expectedMapVersion = 1, int expectedWidth = 11, int expectedHeight = 11)
        {
            if (profileResolver == null || boardFactory == null) throw new ArgumentNullException();
            var data = Decode(json);
            if (data.mapVersion != expectedMapVersion || data.width != expectedWidth || data.height != expectedHeight)
                throw new FormatException("Map generation configuration changed.");
            if (!ulong.TryParse(data.seed, NumberStyles.None, CultureInfo.InvariantCulture, out ulong seed) ||
                seed.ToString(CultureInfo.InvariantCulture) != data.seed) throw new FormatException("Invalid seed.");
            var spawns = data.spawns.Select(spawn => {
                if (spawn == null) throw new FormatException("Missing spawn.");
                CheckId(spawn.id); CheckId(spawn.profile);
                var profile = profileResolver(spawn.profile);
                if (profile == null || profile.Id != spawn.profile || ProfileDigest(profile) != spawn.signature)
                    throw new FormatException("Missing or changed profile.");
                return new HeroSpawn(spawn.id, spawn.team, profile, new GridCell(spawn.x, spawn.y));
            }).ToArray();
            var journal = Create(seed, data.scenario, (SkillMechanics)data.mechanics, spawns, boardFactory(seed, data.scenario),
                data.campaignFlags, data.mapVersion, data.width, data.height);
            foreach (var command in data.commands)
            {
                if (command == null) throw new FormatException("Missing command.");
                bool accepted;
                switch (command.kind)
                {
                    case "cast":
                        accepted = journal.Cast(command.actor, command.skill, command.targets,
                            command.hasDestination ? new GridCell(command.x, command.y) : (GridCell?)null).Success; break;
                    case "move":
                        if (!command.hasDestination || !string.IsNullOrEmpty(command.skill) || command.targets == null || command.targets.Length != 0)
                            throw new FormatException("Malformed movement.");
                        accepted = journal.Move(command.actor, new GridCell(command.x, command.y)) == SkillFailure.None; break;
                    case "end":
                        if (command.hasDestination || !string.IsNullOrEmpty(command.skill) || command.targets == null || command.targets.Length != 0 || command.x != 0 || command.y != 0)
                            throw new FormatException("Malformed end turn.");
                        accepted = journal.EndTurn(command.actor); break;
                    default: throw new FormatException("Unknown command.");
                }
                if (!accepted) throw new FormatException("Rejected replay command.");
            }
            if (StateDigest(journal.Session) != data.state || journal.Export() != json) throw new FormatException("Replay diverged.");
            return journal;
        }

        static string ProfileDigest(DungeonHeroProfile profile) => Hash(FormattableString.Invariant(
            $"{profile.Id}|{profile.Health}|{profile.Armor}|{profile.Movement}|{string.Join(",", profile.SkillIds)}"));
        static string StateDigest(DungeonSkillSession session) => Hash(session.PersistenceState);
        internal static string Hash(string value)
        {
            using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", "").ToLowerInvariant();
        }
        internal static void CheckId(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length > 80 || value.Any(c => !(c >= 'a' && c <= 'z') &&
                !(c >= 'A' && c <= 'Z') && !(c >= '0' && c <= '9') && c != '-' && c != '_' && c != '.' && c != ':'))
                throw new FormatException("Invalid bounded identifier.");
        }
        static void CheckSize(string json)
        { if (json == null || Encoding.UTF8.GetByteCount(json) > MaxJsonBytes) throw new FormatException("Campaign size limit exceeded."); }
        static void CheckMechanics(int value)
        { if (value < 0 || (value & ~15) != 0) throw new FormatException("Unknown mechanics."); }
        static void CheckMap(int flags, int version, int width, int height)
        {
            if (flags < 0 || (flags & ~15) != 0 || version != 1 || width < 1 || width > 128 || height < 1 || height > 128)
                throw new FormatException("Unknown campaign or map configuration.");
        }
    }
}
