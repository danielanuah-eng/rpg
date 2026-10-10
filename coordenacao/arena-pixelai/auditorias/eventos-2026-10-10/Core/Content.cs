using System.Collections.Generic;
using System.Linq;

namespace ArenaTatica.Core
{
    // Números iniciais; o balanceamento vem depois. Kits do elenco descritos pelo autor em 10/10 (DESIGN-KROSMASTER.md, passo 7).
    public static class Content
    {
        public static readonly Dictionary<string,FighterDef> Fighters=new Dictionary<string,FighterDef>
        {
            ["ciantia"]=new FighterDef { id="ciantia", name="Ciantia", sprites="Ciantia", hp=11, pa=6, pm=3, initiative=5, level=4, skills=new[] {
                new SkillDef { id="dragao-dourado", name="Dragão Dourado", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=4, damage=2, line=true, modifiable=true },
                new SkillDef { id="invocacao-dragao", name="Invocação de dragão", kind=SkillKind.Invocacao, pa=4, minRange=1, maxRange=2, perTurn=1, sight=false, summon="dragao-espectral", anim="summon" } } },
            // Guerreira de espada rosa: corpo a corpo, resistente na medida; aliados perto dela batem mais forte.
            ["talyssa"]=new FighterDef { id="talyssa", name="Talyssa", sprites="Talyssa", hp=12, pa=6, pm=3, initiative=4, level=4, allyBonus=1, auraRange=2, skills=new[] {
                new SkillDef { id="lamina-rosa", name="Lâmina rosa", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=3 },
                new SkillDef { id="corte-em-arco", name="Corte em arco", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=1, damage=2, area=1, perTurn=1, anim="attack" } } },
            // Foice: acerta a 2 casas, anda muito e se teleporta girando a foice.
            ["teantha"]=new FighterDef { id="teantha", name="Teantha", sprites="Teantha", hp=10, pa=6, pm=4, initiative=6, level=3, skills=new[] {
                new SkillDef { id="foice-verde", name="Foice verde", kind=SkillKind.Dano, pa=3, minRange=2, maxRange=2, damage=3 },
                new SkillDef { id="giro-sombrio", name="Giro sombrio", kind=SkillKind.Teleporte, pa=2, minRange=1, maxRange=5, perTurn=1, sight=false, anim="teleport" } } },
            // Bruto de lâmina necrótica: golpe pesado; no primeiro nocaute ele renasce.
            ["devon"]=new FighterDef { id="devon", name="Devon", sprites="Devon", hp=13, pa=6, pm=3, initiative=2, level=5, revive=5, skills=new[] {
                new SkillDef { id="golpe-necrotico", name="Golpe necrótico", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=1, damage=4 } } },
            // Esgrimista: salta para junto do inimigo e se cura ao acertar.
            ["diantus"]=new FighterDef { id="diantus", name="Diantus", sprites="Diantus", hp=11, pa=6, pm=3, initiative=6, level=4, skills=new[] {
                new SkillDef { id="estocada", name="Estocada flamejante", kind=SkillKind.Dano, pa=2, minRange=1, maxRange=1, damage=2, drain=1 },
                new SkillDef { id="salto-do-duelista", name="Salto do duelista", kind=SkillKind.Salto, pa=2, minRange=2, maxRange=4, perTurn=1, sight=true, anim="leap" } } },
            // Pistoleiro: o camaleão vira pistola (precisa, armadura normal), canhão (área, impreciso, armadura menor) ou martelo (perto, armadura maior).
            ["damian"]=new FighterDef { id="damian", name="Damian", sprites="Damian", hp=10, pa=6, pm=4, initiative=5, level=4, startForm="pistola",
                forms=new[] {
                    new FormDef { id="pistola", name="Pistola", armor=0 },
                    new FormDef { id="canhao", name="Canhão de mão", armor=-1 },
                    new FormDef { id="martelo", name="Martelo", armor=2 } },
                skills=new[] {
                    new SkillDef { id="tiro-de-pistola", name="Tiro de pistola", kind=SkillKind.Dano, pa=3, minRange=2, maxRange=5, damage=2, precise=true, modifiable=true, form="pistola", anim="pistol" },
                    new SkillDef { id="canhao-de-mao", name="Canhão de mão", kind=SkillKind.Dano, pa=4, minRange=2, maxRange=4, damage=3, area=1, wild=true, form="canhao", anim="cannon" },
                    new SkillDef { id="martelada", name="Martelada", kind=SkillKind.Dano, pa=4, minRange=1, maxRange=1, damage=4, form="martelo", anim="hammer" },
                    new SkillDef { id="camaleao-pistola", name="Camaleão: pistola", kind=SkillKind.Forma, pa=1, minRange=0, maxRange=0, sight=false, toForm="pistola", anim="morph" },
                    new SkillDef { id="camaleao-canhao", name="Camaleão: canhão", kind=SkillKind.Forma, pa=1, minRange=0, maxRange=0, sight=false, toForm="canhao", anim="morph" },
                    new SkillDef { id="camaleao-martelo", name="Camaleão: martelo", kind=SkillKind.Forma, pa=1, minRange=0, maxRange=0, sight=false, toForm="martelo", anim="morph" } } },
            // Ilusionista: começa com uma cópia ilusória; ilusões tiram PM; Espelho primordial copia a última skill de alguém.
            ["ezequiela"]=new FighterDef { id="ezequiela", name="Ezequiela", sprites="Ezequiela", hp=9, pa=6, pm=3, initiative=4, level=3, illusion=true, skills=new[] {
                new SkillDef { id="ilusao-confusa", name="Ilusão confusa", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=3, damage=2, pmLoss=1, modifiable=true },
                new SkillDef { id="espelho-primordial", name="Espelho primordial", kind=SkillKind.Espelho, pa=2, minRange=0, maxRange=6, perMatch=1, sight=false, anim="mirror" } } },
            ["ilusao"]=new FighterDef { id="ilusao", name="Ilusão", sprites="Ezequiela", hp=1, pa=0, pm=3, level=0, picker=false, skills=new SkillDef[0] },
            // Viking tanque: protege aliados perto; o Olho abissal o transforma por 3 turnos numa máquina de combate sem proteção, que pode errar.
            ["tchu-tcha"]=new FighterDef { id="tchu-tcha", name="Tchu tcha", sprites="TchuTcha", hp=15, pa=6, pm=2, initiative=1, level=5, guard=true, startForm="normal",
                forms=new[] {
                    new FormDef { id="normal", name="Normal", armor=1 },
                    new FormDef { id="berserk", name="Berserk", sprites="TchuTchaBerserk", pa=2, pm=2, armor=0, noGuard=true } },
                skills=new[] {
                    new SkillDef { id="escudada", name="Escudada", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=2, pmLoss=1, form="normal" },
                    new SkillDef { id="olho-abissal", name="Olho abissal", kind=SkillKind.Forma, pa=2, minRange=0, maxRange=0, sight=false, toForm="berserk", turns=3, perMatch=1, form="normal", anim="transform" },
                    new SkillDef { id="furia-negra", name="Fúria negra", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=5, wild=true, form="berserk" } } },
            // Anjo caído: tiro carregado de longe; a ultimate abre as asas e prepara uma flecha devastadora.
            ["arqueiro-jack"]=new FighterDef { id="arqueiro-jack", name="Arqueiro Jack", sprites="ArqueiroJack", hp=9, pa=6, pm=4, initiative=7, level=4, startForm="capuz",
                forms=new[] { new FormDef { id="capuz", name="Capuz" }, new FormDef { id="asas", name="Asas abertas", armor=-1 } },
                skills=new[] {
                    new SkillDef { id="tiro-carregado", name="Tiro carregado", kind=SkillKind.Dano, pa=4, minRange=3, maxRange=7, damage=3, perTurn=1, modifiable=true },
                    new SkillDef { id="asas-abissais", name="Asas abissais", kind=SkillKind.Forma, pa=2, minRange=0, maxRange=0, sight=false, toForm="asas", turns=2, perMatch=1, form="capuz", anim="ultcharge" },
                    new SkillDef { id="flecha-abissal", name="Flecha abissal", kind=SkillKind.Dano, pa=4, minRange=2, maxRange=8, damage=7, line=true, endsForm=true, form="asas", anim="ultshot" } } },
            // Halfling ladino: mais PA e PM, se esconde junto a relevo; acertos podem roubar moedas (mais forte se estava escondido).
            ["perry"]=new FighterDef { id="perry", name="Perry", sprites="Perry", hp=8, pa=7, pm=4, initiative=8, level=3, steal=20, skills=new[] {
                new SkillDef { id="adagas-gemeas", name="Adagas gêmeas", kind=SkillKind.Dano, pa=2, minRange=1, maxRange=1, damage=2, perTurn=2 },
                new SkillDef { id="esconder", name="Esconder-se", kind=SkillKind.Esconder, pa=1, minRange=0, maxRange=0, sight=false, perTurn=1, anim="hide" } } },
            ["dragao-espectral"]=new FighterDef { id="dragao-espectral", name="Dragão espectral", sprites="DragaoEspectral", hp=6, pa=4, pm=4, level=0, flying=true, picker=false, skills=new[] {
                new SkillDef { id="baforada", name="Baforada estelar", kind=SkillKind.Dano, pa=2, minRange=1, maxRange=2, damage=2, line=true } } },
            ["lobo-de-cinzas"]=new FighterDef { id="lobo-de-cinzas", name="Lobo de cinzas", hp=3, pa=3, pm=3, level=0, picker=false, skills=new[] {
                new SkillDef { id="mordida", name="Mordida", kind=SkillKind.Dano, pa=2, minRange=1, maxRange=1, damage=1 } } },
        };
        // Lutadores que dá para escolher (sem invocações), na ordem do seletor.
        public static readonly string[] Roster=Fighters.Values.Where(d=>d.picker).Select(d=>d.id).ToArray();
        public static FighterDef Def(Fighter f) => Fighters[f.def];
        public static int MaxHp(Fighter f) => Def(f).hp+f.bonusHp;
        public static FormDef Form(Fighter f) => Def(f).forms?.FirstOrDefault(x=>x.id==f.form);
        // Qualquer skill do jogo pelo id: de um lutador ou da loja (Espelho primordial e cargas).
        public static SkillDef AnySkill(string id) =>
            Fighters.Values.SelectMany(d=>d.skills).FirstOrDefault(s=>s.id==id)??Shop.BySkill(id)?.skill;
        // Skill própria (na forma atual) ou carga (loja, espelho) enquanto houver usos.
        public static SkillDef Skill(Fighter f,string id)
        {
            var own=System.Array.Find(Def(f).skills,s=>s.id==id);
            if(own!=null) return string.IsNullOrEmpty(own.form)||own.form==f.form?own:null;
            return f.charges.Exists(c=>c.skill==id&&c.uses>0)?AnySkill(id):null;
        }
    }
}
