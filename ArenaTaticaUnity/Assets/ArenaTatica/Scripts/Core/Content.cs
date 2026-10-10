using System.Collections.Generic;

namespace ArenaTatica.Core
{
    // Números iniciais; o balanceamento vem depois.
    public static class Content
    {
        public static readonly Dictionary<string,FighterDef> Fighters=new Dictionary<string,FighterDef>
        {
            ["ciantia"]=new FighterDef { id="ciantia", name="Ciantia", hp=11, pa=6, pm=3, initiative=5, level=4, skills=new[] {
                new SkillDef { id="dragao-dourado", name="Dragão Dourado", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=4, damage=2, line=true, modifiable=true },
                new SkillDef { id="invocacao-dragao", name="Invocação de dragão", kind=SkillKind.Invocacao, pa=4, minRange=1, maxRange=2, perTurn=1, sight=false, summon="dragao-espectral" } } },
            ["dragao-espectral"]=new FighterDef { id="dragao-espectral", name="Dragão espectral", hp=6, pa=4, pm=4, level=0, flying=true, skills=new[] {
                new SkillDef { id="garra", name="Garra", kind=SkillKind.Dano, pa=2, minRange=1, maxRange=1, damage=2 } } },
            ["lobo-de-cinzas"]=new FighterDef { id="lobo-de-cinzas", name="Lobo de cinzas", hp=3, pa=3, pm=3, level=0, skills=new[] {
                new SkillDef { id="mordida", name="Mordida", kind=SkillKind.Dano, pa=2, minRange=1, maxRange=1, damage=1 } } },
        };
        public static FighterDef Def(Fighter f) => Fighters[f.def];
        public static int MaxHp(Fighter f) => Def(f).hp+f.bonusHp;
        // Skill própria ou comprada na loja (enquanto houver carga).
        public static SkillDef Skill(Fighter f,string id)
        {
            var own=System.Array.Find(Def(f).skills,s=>s.id==id);
            if(own!=null) return own;
            return f.charges.Exists(c=>c.skill==id&&c.uses>0)?Shop.BySkill(id)?.skill:null;
        }
    }
}
