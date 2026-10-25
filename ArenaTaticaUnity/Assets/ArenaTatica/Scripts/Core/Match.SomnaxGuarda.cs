using System;
using System.Collections.Generic;
using System.Linq;
namespace ArenaTatica.Core
{
    public sealed partial class Match
    {
        public static bool GuardActive(Fighter f) => f!=null&&f.Alive&&f.prisoned==0&&!f.banished&&f.directionalGuard&&!(Content.Form(f)?.noGuard??false);
        static SkillDef GuardDefinition(Fighter f) => Content.Skill(f,f.guardSkill??"guarda-direcional")??Content.AnySkill("guarda-direcional");
        public static bool GuardAllowsSkill(Fighter f,SkillDef sk,out string reason)
        {
            reason="";
            if(!GuardActive(f)||sk==null||sk.kind==SkillKind.GuardaDirecional) return true;
            if(sk.kind==SkillKind.Dano||sk.damage>0||sk.kind==SkillKind.Salto||sk.kind==SkillKind.Teleporte||sk.kind==SkillKind.Troca)
            { reason="Desative a guarda para atacar ou se mover."; return false; }
            return true;
        }
        bool CheckDirectionalGuard(Fighter f,Cell c,out string reason)
        {
            reason="";
            if(GuardActive(f)) { if(c==f.cell) return true; reason="Desative a guarda selecionando Tchu Tcha antes de trocar a direção."; return false; }
            if(f.cell.Distance(c)!=1) { reason="Escolha uma casa vizinha para indicar o lado defendido."; return false; }
            return true;
        }
        void SetDirectionalGuard(Fighter f,SkillDef sk,Cell c)
        {
            if(GuardActive(f)) f.directionalGuard=false;
            else { f.directionalGuard=true; f.guardSkill=sk.id; f.guardDirection=new Cell(c.x-f.cell.x,c.z-f.cell.z); }
            Emit(new GameEvent{kind="guarda-direcional",actor=f.id,value=f.directionalGuard?1:0,cell=f.cell,aim=c,info=f.directionalGuard?sk.id:"guarda-desligar"});
        }
        Fighter DirectionalGuardFor(Fighter source,Fighter target)
        {
            if(source==null||target==null||source==target) return null;
            int dx=source.cell.x-target.cell.x,dz=source.cell.z-target.cell.z;
            foreach(var g in S.fighters.Where(g=>g!=target&&GuardActive(g)&&S.Ally(g.team,target.team)).OrderBy(g=>g.id))
            {
                var def=GuardDefinition(g); int radius=Math.Max(0,def?.area??2);
                if(Board.Gap(g,target)>radius) continue;
                int dot=dx*g.guardDirection.x+dz*g.guardDirection.z,side=dx*g.guardDirection.z-dz*g.guardDirection.x;
                if(dot>0&&Math.Abs(side)<=dot) return g;
            }
            return null;
        }
        bool CheckTreatAlly(Fighter f,Cell c,bool second,Cell first,out string reason)
        {
            var ally=Board.KnownAt(S,c,f); reason="";
            if(ally==null||!ally.Alive||ally.prisoned>0||!S.Ally(f.team,ally.team)||!second&&ally==f)
            { reason=second?"Escolha outro aliado vivo ou Somnax para concluir.":"Escolha pelo menos um outro aliado vivo."; return false; }
            if(second&&ally!=f&&Board.At(S,first)==ally) { reason="Esse aliado já foi escolhido."; return false; }
            return true;
        }
        void ShareTreat(Fighter f,SkillDef sk,Cell c,Cell c2)
        {
            var first=Board.At(S,c);var second=Board.At(S,c2);
            Emit(new GameEvent{kind="doce-salgado",actor=f.id,info=sk.id,cell=f.cell});
            foreach(var target in new[]{f,first,second}.Where(x=>x!=null&&x.Alive&&x.prisoned==0&&S.Ally(x.team,f.team)).Distinct()) Heal(target,Math.Max(0,sk.heal),f,sk.id);
        }
        void ResetTwinBond(Fighter f)
        {
            f.twinBondUsed=false;
            if(f.pactTargets==null) f.pactTargets=new List<int>(); else f.pactTargets.Clear();
            if(f.renounceTargets==null) f.renounceTargets=new List<int>(); else f.renounceTargets.Clear();
        }
        int TwinBond(Fighter f,Fighter target,SkillDef sk)
        {
            if(f.def!="somnax"||S.Active!=f||target==f||S.Ally(f.team,target.team)) return 0;
            bool pact=Content.BaseSkill(sk.id)=="pacto", renounce=Content.BaseSkill(sk.id)=="renuncia";
            if(!pact&&!renounce) return 0;
            f.pactTargets??=new List<int>(); f.renounceTargets??=new List<int>();
            var own=pact?f.pactTargets:f.renounceTargets;var other=pact?f.renounceTargets:f.pactTargets;
            bool trigger=!f.twinBondUsed&&other.Contains(target.id); if(!own.Contains(target.id)) own.Add(target.id);
            if(!trigger) return 0;
            f.twinBondUsed=true; Emit(new GameEvent{kind="elo-duas-laminas",actor=f.id,target=target.id,cell=target.cell,value=1}); return 1;
        }
        // Vozes do Somnax (pacote do autor, 25/10): a fala é escolhida no uso aceito, antes de registrar o uso, e viaja no evento;
        // a tela só toca. Pacto: 1º do turno "Eu sou eu", depois "Até o fim". Renúncia: "E mais ninguém" se já houve Pacto no turno.
        void SomnaxVoice(Fighter f,SkillDef sk)
        {
            if(f.def!="somnax") return;
            int pacts=f.used.Count(id=>Content.BaseSkill(id)=="pacto");
            string voice=Content.BaseSkill(sk.id) switch {
                "pacto"=>pacts==0?"somnax_pacto_1":"somnax_pacto_2",
                "renuncia"=>pacts>0?"somnax_renuncia_apos_pacto":"somnax_renuncia_sem_pacto",
                "doce-e-salgado"=>"somnax_doce_e_salgado",
                "passo-do-crepusculo"=>"somnax_passo_do_crepusculo",
                "ruptura-das-memorias"=>"somnax_ruptura_das_memorias",
                _=>null };
            if(voice!=null) Emit(new GameEvent{kind="voz-personagem",actor=f.id,info=voice,cell=f.cell});
        }
        void DamianGreeting(Fighter f)
        {
            const string marker="voz:damian-capitao";
            if(!f.Alive||f.def!="damian"||f.usedMatch.Contains(marker)||!S.fighters.Any(x=>x.Alive&&x.def=="pele-negra")) return;
            f.usedMatch.Add(marker); Emit(new GameEvent{kind="voz-personagem",actor=f.id,info="damian_pele_d_ark",cell=f.cell});
        }
    }
}
