using System;
using System.Collections.Generic;
using System.Linq;
namespace ArenaTatica.Core
{
    // Viúva da Agulha (autor 24/10): a Teia cobre um quadrado no chão (SkillDef.side; 4×4 desde 25/10); em cima dela a Viúva e as aranhas ficam invisíveis.
    // As aranhas (até 3, invocadas pela skill de aranhas) agem no fim do turno dela: andam 3 PM rumo ao último inimigo que
    // pisou na teia ou começou o turno nela; sem ninguém marcado, seguem a mãe e atacam quem ela atacou. 1 de dano; alvo
    // fora da teia: 50% de erro.
    public sealed partial class Match
    {
        public const string WebEffect="teia-viuva", Spider="aranha-viuva";
        LunaWeb WebOf(int by) => S.Luna.webs.FirstOrDefault(w=>w.by==by);
        public static bool OnWeb(MatchState s,int by,Cell c) => s.Luna.webs.Any(w=>(by==0||w.by==by)&&w.cells!=null&&w.cells.Contains(Board.Index(s,c)));
        bool OnAnyWeb(Fighter f) => f!=null&&Board.Footprint(f).Any(c=>OnWeb(S,0,c));
        // Mãe da família da teia: a própria Viúva ou a dona da aranha.
        int WebFamily(Fighter f) => f==null?0:f.def==Spider?f.summoner:f.def=="viuva-da-agulha"&&f.summoner==0?f.id:0;

        bool CheckWeb(Fighter f,SkillDef sk,Cell c,out string reason)
        {
            reason="";
            if(!InRange(f,sk,c,out reason)) return false;
            if(WebCells(sk,c).Count==0) { reason="A teia precisa de chão livre."; return false; }
            return true;
        }
        // Quadrado de lado side (0 = 2) em volta da casa escolhida (2×2 começa nela; 4×4 começa 1 antes). Só chão andável.
        List<int> WebCells(SkillDef sk,Cell c)
        {
            var list=new List<int>(); int n=sk.side>0?sk.side:2, s0=-(n-1)/2;
            for(int dz=0;dz<n;dz++) for(int dx=0;dx<n;dx++)
            {
                var x=new Cell(c.x+s0+dx,c.z+s0+dz); if(Board.Contains(S,x)&&Board.Walkable(S,x,false)) list.Add(Board.Index(S,x));
            }
            return list;
        }
        void CastWeb(Fighter f,SkillDef sk,Cell c)
        {
            S.Luna.webs.RemoveAll(w=>w.by==f.id);
            var web=new LunaWeb{by=f.id,turns=Math.Max(0,sk.turns),cells=WebCells(sk,c).ToArray()};
            S.Luna.webs.Add(web);
            Emit(new GameEvent{kind="skill",info=WebEffect,actor=f.id,cell=c,value=web.cells.Length});
            // Quem já está em cima conta como quem pisou; a família some de vista.
            foreach(var v in S.fighters.Where(x=>x.Alive&&OnAnyWeb(x)).ToList()) WebTouch(v,v.cell,null);
        }
        // Começo do turno da dona: a teia com duração perde 1 turno; ao zerar, some (a família em cima volta a aparecer).
        void WebAge(Fighter f)
        {
            var w=WebOf(f.id); if(w==null||w.turns<=0||w.cells==null||w.cells.Length==0) return;
            if(--w.turns>0) return;
            w.cells=new int[0];
            Emit(new GameEvent{kind="status",info="teia-some",actor=f.id,cell=f.cell});
            foreach(var v in S.fighters.Where(x=>x.Alive&&WebFamily(x)==f.id).ToList()) WebTouch(v,v.cell,null);
        }
        // Pisar ou começar o turno na teia: inimigo vira o alvo das aranhas; a família fica invisível.
        void WebTouch(Fighter f,Cell cell,List<GameEvent> log)
        {
            if(f==null||!f.Alive) return;
            // Fora da teia a família não tem como se esconder (autor 25/10): saiu dela, aparece.
            if(WebFamily(f)!=0&&f.hidden&&!OnWeb(S,0,cell)) { f.hidden=false; (log??events).Add(new GameEvent{kind="revelar",target=f.id,cell=cell}); }
            if(S.Luna.webs.Count==0) return;
            foreach(var w in S.Luna.webs)
            {
                if(w.cells==null||!w.cells.Contains(Board.Index(S,cell))) continue;
                var mother=S.F(w.by); if(mother==null) continue;
                if(WebFamily(f)==w.by)
                {
                    if(!f.hidden) { f.hidden=true; f.pm=Math.Min(f.pm,HiddenCap(f)); (log??events).Add(new GameEvent{kind="esconder",actor=f.id,info=WebEffect,cell=f.cell}); }
                }
                else if(!S.Ally(f.team,mother.team))
                {
                    w.last=f.id;
                    // Grudar (pmLoss da Teia na ficha, autor 24/10): perde os PM uma vez por turno ao pisar ou começar nela.
                    int loss=Content.Def(mother).skills.Where(x=>x.effect==WebEffect).Select(x=>x.pmLoss).DefaultIfEmpty(0).Max();
                    if(loss>0&&!f.used.Contains(WebStuck)&&!Unbreakable(f))
                    {
                        f.used.Add(WebStuck); f.pm=Math.Max(0,f.pm-loss);
                        (log??events).Add(new GameEvent{kind="buff",target=f.id,info=WebEffect,cell=f.cell,value=-loss});
                    }
                }
            }
        }
        const string WebStuck="teia-grudou";
        // A Viúva atacou: as aranhas sem alvo da teia vão atrás desse inimigo.
        void WebPrey(Fighter by,Fighter v)
        {
            if(by==null||v==null||by.def!="viuva-da-agulha"||by.summoner!=0||S.Ally(by.team,v.team)) return;
            var w=WebOf(by.id); if(w==null) { w=new LunaWeb{by=by.id,cells=new int[0]}; S.Luna.webs.Add(w); }
            w.prey=v.id;
        }
        void SpidersAct(Fighter mother)
        {
            if(mother==null||!mother.Alive) return;
            var w=WebOf(mother.id);
            foreach(var sp in S.fighters.Where(x=>x.Alive&&x.def==Spider&&x.summoner==mother.id).ToList())
            {
                if(!sp.Alive||!mother.Alive) break;
                Emit(new GameEvent{kind="turno",actor=sp.id,value=S.round});
                sp.pm=Math.Max(1,Content.Def(sp).pm);
                bool Foe(Fighter v) => v!=null&&v.Alive&&!S.Ally(v.team,mother.team)&&!Unbreakable(v);
                var target=w!=null&&S.F(w.last) is Fighter a&&Foe(a)?a:null;
                var prey=target??(w!=null&&S.F(w.prey) is Fighter p&&Foe(p)?p:null);
                if(target==null)
                {
                    // Ninguém na teia: acompanha a mãe e só ataca quem ela atacou.
                    if(prey!=null&&Board.Gap(sp,prey)>1) WalkRobot(sp,mother,Closest(sp,Around(prey.cell))??sp.cell);
                    else if(prey==null&&Board.Gap(sp,mother)>1) WalkRobot(sp,mother,Closest(sp,Around(mother.cell))??sp.cell);
                }
                else if(Board.Gap(sp,target)>1) WalkRobot(sp,mother,Closest(sp,Around(target.cell))??sp.cell);
                if(!sp.Alive||prey==null||!prey.Alive||Board.Gap(sp,prey)>1) continue;
                // Golpe da aranha sai à vista (sem o bônus de emboscada).
                if(sp.hidden) { sp.hidden=false; Emit(new GameEvent{kind="revelar",target=sp.id,cell=sp.cell}); }
                var bite=new SkillDef{id="mordida-aranha",name="Mordida",kind=SkillKind.Dano,pa=0,minRange=1,maxRange=1,damage=1,
                    missPct=OnAnyWeb(prey)?0:50,anim="attack"};
                Strike(sp,bite,prey.cell);
            }
        }
        IEnumerable<Cell> Around(Cell c) { foreach(var d in Board.Dirs) yield return c+d; }
    }
}
