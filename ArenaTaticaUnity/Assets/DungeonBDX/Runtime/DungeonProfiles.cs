using System;
using System.Linq;
using DungeonBDX.Skills;

namespace DungeonBDX
{
    public static class DungeonProfiles
    {
        static readonly System.Collections.Generic.Dictionary<string,DungeonHeroProfile> cache = new System.Collections.Generic.Dictionary<string,DungeonHeroProfile>(StringComparer.Ordinal);
        public static DungeonHeroProfile Resolve(string id)
        {
            if (id==null) throw new ArgumentNullException(nameof(id));
            if (cache.TryGetValue(id,out var resolved)) return resolved;
            var native = DungeonHeroProfiles.NewHeroes.FirstOrDefault(p => p.Id == id);
            if (native != null) {cache.Add(id,native);return native;}
            var source = DungeonHeroCatalog.ImportArena().FirstOrDefault(p => p.Id == id);
            if (source == null) throw new ArgumentException("Perfil desconhecido: " + id);
            var profile = DungeonHeroProfiles.AdaptArena(source.Id, source.Name, source.Health, source.Armor,
                source.Movement, source.Skills.Select(s => s.Id));
            cache.Add(id,profile);return profile;
        }
    }
}
