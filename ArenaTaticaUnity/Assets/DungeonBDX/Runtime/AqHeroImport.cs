using System.Linq;
using DungeonBDX.Arcadia;
using UnityEngine;

namespace DungeonBDX
{
    public static class AqHeroImport
    {
        public static void Apply(AqSession session)
        {
            foreach(var u in session.Units)
                if(string.IsNullOrEmpty(u.SpriteFolder)&&ArenaTatica.Core.Content.Fighters.TryGetValue(u.HeroId,out var source))u.SpriteFolder=source.sprites;
        }
        public static string[] PlayableIds()=>DungeonHeroCatalog.ImportAll().Select(h=>h.Id).Where(AqContent.Playable.Contains).ToArray();
        public static Sprite Portrait(string id)
        {
            if(ArenaTatica.Core.Content.Fighters.TryGetValue(id,out var h))
                return Resources.Load<Sprite>("Sprites/"+h.sprites+"/south")??Resources.LoadAll<Sprite>("Sprites/"+h.sprites).FirstOrDefault(s=>s.name.StartsWith("idle_south_"));
            return null;
        }
        public static float Height(string id)=>id=="colosso"?2.4f:id=="dragao-espectral"?2.6f:id=="brutamontes-errante"?2.5f:id=="cheiro-pneu"?2.1f:
            id=="principe-kellen"?1f:id=="perry"?1.2f:id=="vespa-cinzenta"?.8f:
            id=="rei-goblin-ferrugem"||id=="conde-vesperal"||id=="necromante-cripta"?1.8f:1.5f;
        public static string[][] DefaultParties(int count=2)=>new[]{new[]{"colosso","arqueiro-jack","talyssa"},new[]{"damian","ciantia","diantus"},
            new[]{"edgar","perry","ezequiela"},new[]{"baltazar","asamyh","principe-kellen"}}.Take(Mathf.Clamp(count,2,4)).ToArray();
    }
}
