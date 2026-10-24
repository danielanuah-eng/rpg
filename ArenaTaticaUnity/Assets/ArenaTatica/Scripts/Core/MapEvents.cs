using System;
using System.Collections.Generic;
using System.Linq;

namespace ArenaTatica.Core
{
    // Onde os mobs de um evento surgem. Espelhado: pares de casas livres espelhadas (justo para os dois lados);
    // Centro: as casas livres mais perto do meio; Longe: as casas livres mais longe de todos os lutadores.
    public enum MobSpawn { Espelhado, Centro, Longe }

    // Evento de mapa descrito só nos dados (21/10): entra no sorteio do Cataclisma junto com os eventos antigos de Match.Cataclysms
    // e o motor aplica os campos sozinho. Evento novo = uma entrada em MapEvents.All (e a ficha do mob em Content.Fighters, se tiver).
    [Serializable] public class MapEventDef
    {
        public string id, name, help;
        // weight: peso no sorteio do Cataclisma (0 = só sai disparado por id). rounds: rodadas completas que dura; 0 = na hora.
        public int weight, rounds;
        // heal: cura única, no começo, em todos os lutadores dos jogadores. dmg: dano a mais nos golpes dos lutadores dos jogadores
        // enquanto o evento durar.
        public int heal, dmg;
        // mob: ficha neutra que surge (Content.Fighters); quantos = mobs + mobsPerPlayer × jogadores. minGap: distância mínima
        // dos lutadores na hora de surgir. keepMobs: os mobs ficam depois do fim (falso = somem com o evento).
        public string mob;
        public int mobs, mobsPerPlayer, minGap=2;
        public MobSpawn spawn;
        public bool keepMobs;
        // Onde pode sair no sorteio: maze = só no labirinto, open = só nos mapas abertos (os dois falsos = em qualquer mapa).
        public bool maze, open;
        // waterOnly (expansão de 22/10): o evento só sai em mapa com água e os mobs surgem/andam só nas casas de água.
        public bool waterOnly;
        // tint: cor do mapa enquanto durar ("#RRGGBB"); a apresentação tinge luz, ambiente e céu.
        public string tint;
        public int MobCount(int players) => Math.Max(0,mobs+mobsPerPlayer*players);
    }

    public static class MapEvents
    {
        public static readonly MapEventDef[] All=
        {
            // Lua de Sangue (autor, 21/10): 3 rodadas de mapa vermelho; todos curam 2 na hora e batem +3; lobos de 1 PV caçam quem
            // estiver perto e vagam quando não há ninguém.
            new MapEventDef { id="lua-de-sangue", name="Lua de Sangue", weight=2, rounds=3, heal=2, dmg=3, mob="lobo-lua-de-sangue", mobs=2, mobsPerPlayer=1,
                spawn=MobSpawn.Espelhado, minGap=2, tint="#B0182A",
                help="Por 3 rodadas o mapa fica vermelho: todos curam 2 PV na hora e causam +3 de dano. Surgem lobos de 1 PV (2 + 1 por jogador) que atacam quem estiver perto e vagam sem rumo; somem no fim." },
            // Tubarões (expansão de 22/10): só nas Docas inundadas, 3 rodadas, 1 tubarão por jogador, 3 PV, 5 PM, mordida de
            // 1 de dano por 3 PA. Andam só na água, caçam quem está na água e circulam sem alvo; somem no fim.
            new MapEventDef { id="tubaroes", name="Tubarões", weight=2, rounds=3, mob="tubarao", mobs=0, mobsPerPlayer=1,
                spawn=MobSpawn.Espelhado, minGap=1, keepMobs=false, waterOnly=true, open=true, tint="#0E5A6E",
                help="Por 3 rodadas as águas ficam perigosas: surge 1 tubarão por jogador (3 PV, 5 PM, mordida de 1 de dano por 3 PA). Eles só nadam na água, caçam quem estiver na água e circulam sem alvo; somem no fim." },
        };
        public static MapEventDef Get(string id) => string.IsNullOrEmpty(id)?null:All.FirstOrDefault(e=>e.id==id);
        // Pode sair no sorteio deste mapa?
        public static bool Allowed(string id,bool maze) { var e=Get(id); return e==null||(!e.maze||maze)&&(!e.open||!maze); }
    }
}
