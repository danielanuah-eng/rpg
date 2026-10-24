using System;
using System.Collections.Generic;
using ArenaTatica.Core;
namespace ArenaTatica
{
    // IDs estáveis: cada recurso é uma fala NOVA; modelos de referência ficam fora de Assets.
    public static class ArenaVoiceCatalog
    {
        public static readonly string[] Ids={"jack_ultimate","jack_flecha_abissal","damian_canhao","damian_pistola","damian_martelo","damian_morte","damian_pele_d_ark","damian_acerto","primal_cacador","tchu_escudo","tchu_sair_ultimate","tchu_proteger_aliado","tchu_possessao","tchu_morte"};
        static readonly Dictionary<string,string> skills=new Dictionary<string,string>{
            ["asas-abissais"]="jack_ultimate",["flecha-abissal"]="jack_flecha_abissal",
            ["tiro-pistola"]="damian_pistola",["canhao-de-mao"]="damian_canhao",["martelada"]="damian_martelo",
            ["camaleao-pistola"]="damian_pistola",["camaleao-canhao"]="damian_canhao",["camaleao-martelo"]="damian_martelo",
            ["guarda-direcional"]="tchu_escudo",["olho-abissal"]="tchu_possessao"};
        public static bool Valid(string id)=>id!=null&&Array.IndexOf(Ids,id)>=0;
        public static string ForSkill(string skill)=>skill!=null&&skills.TryGetValue(Content.BaseSkill(skill),out var id)?id:null;
        public static string ForEvent(GameEvent e,MatchState s)
        {
            if(e==null) return null;
            if(e.kind=="voz-global") return e.info=="primal_cacador"?e.info:null;
            if(e.kind=="voz-personagem") return e.info=="damian_pele_d_ark"?e.info:null;
            var actor=s?.F(e.actor);var target=s?.F(e.target);
            if(e.kind=="nocaute") return target?.def=="damian"?"damian_morte":target?.def=="tchu-tcha"?"tchu_morte":null;
            if(e.kind=="guarda"||e.kind=="protecao-direcional") return actor?.def=="tchu-tcha"?"tchu_proteger_aliado":null;
            if(e.kind=="forma") {
                if(actor?.def=="damian") return e.info=="pistola"?"damian_pistola":e.info=="canhao"?"damian_canhao":e.info=="martelo"?"damian_martelo":null;
                if(actor?.def=="tchu-tcha") return e.info=="berserk"?"tchu_possessao":e.info=="normal"?"tchu_sair_ultimate":null;
                if(actor?.def=="arqueiro-jack"&&e.info=="asas") return "jack_ultimate";
            }
            return null;
        }
        // Animação nativa: 9 quadros; a flecha é liberada no quadro 7 (conferido na prancha).
        public const float JackReleaseFraction=7f/9f;
        public static float JackWindupSeconds(float nativeDuration,float voiceRemaining)=>Math.Max(Math.Max(0,nativeDuration)*JackReleaseFraction,Math.Max(0,voiceRemaining));
    }
}
