using System;
using System.Collections.Generic;
using System.Linq;

namespace ArenaTatica.Core
{
    // Bots da partida local. Decide UMA ação do lutador ativo por chamada; o jogo chama de novo depois de cada animação,
    // até ela devolver "passar". Joga só com o que o próprio time vê (Match.ViewFor): sem a semente dos dados, sem as armadilhas
    // inimigas e sem quem está escondido. As notas são estimativas (dano médio), nunca uma simulação dos dados.
    public static class Bot
    {
        // Nota mínima para gastar PA numa skill; abaixo disso o bot só se posiciona.
        const double Worth=.35;

        public static Command Decide(Match real)
        {
            var active=real.S.Active; if(active==null) return Pass();
            var sim=Match.FromState(Match.ViewFor(real.S,active.team)); var s=sim.S; var me=s.F(active.id);
            if(me==null||!me.Alive||me.channel>0) return Pass();
            var buy=Buy(s,me); if(buy!=null) return buy;
            var plan=Search(sim,me,true);
            if(plan.cmd!=null&&plan.score>Worth) return plan.cmd;
            return Approach(s,me)??Pass();
        }
        static Command Pass() => new Command{kind="passar"};

        // Tem compra possível: lutador de jogador e há produto que dá para pagar.
        static bool WantsShop(MatchState s,Fighter me) =>
            me.summoner==0&&MatchState.Player(me.team)&&s.shops[me.team].slots.Any(x=>!string.IsNullOrEmpty(x)&&Shop.Def(x).price<=s.coins[me.team]);
        // Loja: o produto mais caro que der para pagar, só numa casa ao lado da loja (invocações não compram).
        static Command Buy(MatchState s,Fighter me)
        {
            if(!Board.NextToShop(s,me.cell)||!WantsShop(s,me)) return null;
            var slots=s.shops[me.team].slots; int pick=-1, price=0;
            for(int i=0;i<slots.Length;i++)
            {
                var d=string.IsNullOrEmpty(slots[i])?null:Shop.Def(slots[i]);
                if(d!=null&&d.price<=s.coins[me.team]&&d.price>price) { pick=i; price=d.price; }
            }
            return pick<0?null:new Command{kind="comprar",slot=pick};
        }

        // Melhor skill a partir de cada casa alcançável. Se a melhor exige andar antes, devolve o "mover"; a skill sai na próxima chamada.
        // allowForm=false na busca interna que mede quanto vale trocar de forma (sem recursão infinita).
        static (double score,Command cmd) Search(Match sim,Fighter me,bool allowForm)
        {
            var s=sim.S; var origin=me.cell; var foes=Foes(s,me);
            var cells=new List<(Cell c,int cost)>{(origin,0)};
            var costs=Board.Costs(s,me);
            for(int i=0;i<costs.Length;i++)
            {
                if(costs[i]<=0||costs[i]>me.pm) continue;
                var c=new Cell(i%s.width,i/s.width);
                if(Board.At(s,c)==null&&!s.At(c).lava) cells.Add((c,costs[i]));
            }
            int nearOrigin=foes.Count(x=>x.cell.Distance(origin)==1&&!Content.Def(x).noLock);
            bool ranged=Reach(me)>=3;
            double best=0; Command cmd=null;
            foreach(var (c,cost) in cells)
            {
                me.cell=c;
                // Andar custa um pouco; sair colado em inimigos arrisca a Trava; atirador evita terminar colado.
                double place=-.12*cost-(cost>0?.6*nearOrigin:0)-(ranged?.4*foes.Count(x=>x.cell.Distance(c)==1):0);
                foreach(var sk in Match.Skills(me))
                {
                    if(me.pa<Match.Cost(me,sk)) continue;
                    if(sk.kind==SkillKind.Forma&&(!allowForm||c!=origin)) continue;
                    foreach(var t in Targets(s,me,sk,foes))
                    {
                        if(!sim.CanTarget(me,sk,t,out _)) continue;
                        int slot=0; double v=Value(sim,me,sk,t,foes,ref slot);
                        if(v<=0) continue; v+=place;
                        if(v>best) { best=v; cmd=c==origin?new Command{kind="skill",skill=sk.id,cell=t,slot=slot}:new Command{kind="mover",cell=c}; }
                    }
                }
            }
            me.cell=origin;
            return (best,cmd);
        }

        // Sem skill que valha: anda para a distância certa do inimigo (colado para quem bate perto, no alcance para quem atira),
        // pegando moedas e a zona central quando o modo de vitória pede.
        static Command Approach(MatchState s,Fighter me)
        {
            if(me.pm<=0) return null;
            var foes=Foes(s,me).Where(x=>!Content.Def(x).neutral).ToList(); if(foes.Count==0) foes=Foes(s,me);
            var costs=Board.Costs(s,me);
            // Distância a pé de cada inimigo até cada casa (ignorando lutadores no caminho).
            var fields=foes.Select(e=>Board.Costs(s,new Fighter{def=me.def,cell=e.cell,climber=me.climber},true)).ToList();
            int reach=Reach(me), want=reach<=1?1:Math.Max(2,reach-1);
            double coinWeight=s.mode==VictoryMode.Tesouro?3:1.2, shopWeight=WantsShop(s,me)?1.5:0;
            double Score(Cell c,int cost)
            {
                int i=Board.Index(s,c);
                double sc=-.05*cost+s.At(c).coins*coinWeight;
                if(fields.Count>0) { int d=fields.Min(f=>f[i]==int.MaxValue?99:f[i]); sc-=Math.Abs(d-want)*(d<want?.6:1); }
                if(s.mode==VictoryMode.Dominio&&s.At(c).zone) sc+=2;
                if(s.At(c).lava) sc-=3;
                // Com compra possível, puxa de leve para o meio e premia a casa ao lado da loja.
                if(shopWeight>0) sc+=Board.NextToShop(s,c)?shopWeight:-.1*MapGen.ShopCells(s).Min(x=>x.Distance(c));
                return sc;
            }
            Cell best=me.cell; double top=Score(me.cell,0)+.01;
            for(int i=0;i<costs.Length;i++)
            {
                if(costs[i]<=0||costs[i]>me.pm) continue;
                var c=new Cell(i%s.width,i/s.width); if(Board.At(s,c)!=null) continue;
                double sc=Score(c,costs[i]); if(sc>top) { top=sc; best=c; }
            }
            return best==me.cell?null:new Command{kind="mover",cell=best};
        }

        static List<Fighter> Foes(MatchState s,Fighter me) => s.fighters.Where(x=>x.Alive&&!s.Ally(x.team,me.team)&&!x.hidden).ToList();
        // Maior alcance entre as skills de dano que ele tem agora.
        static int Reach(Fighter me) => Match.Skills(me).Where(k=>k.kind==SkillKind.Dano&&!k.selfArea).Select(k=>k.maxRange).DefaultIfEmpty(1).Max();
        static bool SelfCast(SkillDef sk) => sk.selfArea||sk.kind==SkillKind.Forma||sk.kind==SkillKind.Esconder||sk.kind==SkillKind.Aura||sk.kind==SkillKind.Desejo&&(string.IsNullOrEmpty(sk.wish)||sk.wish=="f");

        // Casas que vale testar para cada tipo de skill (o CanTarget confirma alcance, visão e regras).
        static IEnumerable<Cell> Targets(MatchState s,Fighter me,SkillDef sk,List<Fighter> foes)
        {
            if(SelfCast(sk)) return new[]{me.cell};
            var allies=s.fighters.Where(x=>x.Alive&&s.Ally(x.team,me.team)).Select(x=>x.cell);
            IEnumerable<Cell> Free(int r) => Ring(s,me.cell,r).Where(c=>Board.At(s,c)==null).OrderBy(c=>foes.Count==0?0:foes.Min(f=>f.cell.Distance(c))).Take(12);
            switch(sk.kind)
            {
                case SkillKind.Dano: case SkillKind.Espelho: case SkillKind.Salto: case SkillKind.Zona: return foes.Select(x=>x.cell);
                case SkillKind.Comando: return sk.step>0?Enumerable.Empty<Cell>():foes.Select(x=>x.cell).Concat(allies);
                case SkillKind.Cura: case SkillKind.Suporte: return allies;
                case SkillKind.Armadilha: return foes.SelectMany(f=>Board.Dirs.Select(d=>f.cell+d)).Where(c=>Board.Contains(s,c)&&Board.At(s,c)==null).Distinct();
                case SkillKind.Invocacao: case SkillKind.Reviver: return Free(Math.Min(sk.maxRange,6));
                case SkillKind.Desejo:
                    if(sk.wish=="a") return Free(1);
                    if(sk.wish=="d") return s.fighters.Where(x=>x.Alive&&x!=me&&!x.hidden).Select(x=>x.cell);
                    return allies;
                default: return Enumerable.Empty<Cell>();
            }
        }
        static IEnumerable<Cell> Ring(MatchState s,Cell center,int r)
        {
            for(int x=-r;x<=r;x++) for(int z=-r;z<=r;z++) { var c=center+new Cell(x,z); if((x!=0||z!=0)&&Board.Contains(s,c)&&center.Distance(c)<=r) yield return c; }
        }

        static double Value(Match sim,Fighter me,SkillDef sk,Cell t,List<Fighter> foes,ref int slot)
        {
            var s=sim.S; var v=Board.At(s,t);
            switch(sk.kind)
            {
                case SkillKind.Dano: return Strike(s,me,sk,t);
                case SkillKind.Espelho: { var copied=Content.AnySkill(v?.lastAttack); return copied==null?0:Hit(s,me,copied,v,true); }
                case SkillKind.Cura: return v==null?0:Math.Min(sk.damage,Content.MaxHp(v)-v.hp)*1.1;
                case SkillKind.Suporte:
                {
                    var center=sk.selfArea?me.cell:t;
                    var list=s.fighters.Where(x=>x.Alive&&s.Ally(x.team,me.team)&&(x.cell==t&&!sk.selfArea||(sk.selfArea||sk.area>0)&&Inside(center,x.cell,sk.area,sk.square))).ToList();
                    double val=list.Sum(x=>Math.Min(sk.heal,Content.MaxHp(x)-x.hp)+(sk.buffPa+sk.buffPm*.5+sk.buffArmor*(foes.Any(f=>f.cell.Distance(x.cell)<=3)?1.2:.3)+sk.buffRange*.5)*.8);
                    if(sk.summonCount>0) val+=1.5*sk.summonCount;
                    return val;
                }
                case SkillKind.Invocacao: return foes.Count==0?0:2.6-.05*foes.Min(f=>f.cell.Distance(t));
                case SkillKind.Armadilha: return foes.Any(f=>f.cell.Distance(t)<=1)?.9:0;
                case SkillKind.Salto:
                {
                    // Vale se depois do salto ainda sobra PA para bater colado.
                    int left=me.pa-sk.pa;
                    return Match.Skills(me).Any(k=>k.kind==SkillKind.Dano&&k.minRange<=1&&k.pa<=left)?1.4:.3;
                }
                case SkillKind.Forma:
                {
                    // Forma temporária (Berserk, Asas, Motor): liga quando há inimigo perto. Forma fixa (Damian): compara a melhor jogada nas duas.
                    if(sk.turns>0) return foes.Any(f=>f.cell.Distance(me.cell)<=4)?3:0;
                    string form=me.form; int pa=me.pa, pm=me.pm;
                    double now=Search(sim,me,false).score;
                    me.form=sk.toForm; me.pa-=sk.pa;
                    double after=Search(sim,me,false).score;
                    me.form=form; me.pa=pa; me.pm=pm;
                    return after-now-.3;
                }
                case SkillKind.Esconder: return foes.Any(f=>f.cell.Distance(me.cell)<=1)?0:me.pa<=2?.6:.25;
                case SkillKind.Zona: return foes.Count(f=>Inside(t,f.cell,sk.area,sk.square))*.6;
                case SkillKind.Comando:
                    if(v==null) return 0;
                    return s.Ally(v.team,me.team)?Math.Min(2,Content.MaxHp(v)-v.hp)*.8:2;
                case SkillKind.Reviver: return 6;
                case SkillKind.Aura:
                {
                    // Nunca desliga sozinho; liga quando há gente para curar ou ferir no alcance.
                    if(me.auraOn) return 0;
                    int count=s.fighters.Count(x=>x.Alive&&x!=me&&x.cell.Distance(me.cell)<=sk.area&&(!s.Ally(x.team,me.team)&&!x.hidden||s.Ally(x.team,me.team)&&x.hp<Content.MaxHp(x)));
                    return count==0?0:1+count;
                }
                case SkillKind.Desejo:
                    switch(sk.wish)
                    {
                        case null: case "": return foes.Any(f=>f.cell.Distance(me.cell)<=1)?1:3;
                        case "a": return 6;
                        case "b": return v==null||!s.Ally(v.team,me.team)?0:Content.MaxHp(v)-v.hp;
                        case "c": return v==null?0:v==me?2.5:3;
                        case "d":
                        {
                            if(v==null) return 0;
                            var options=Match.Copyables(me,v); if(options.Count==0) return 0;
                            slot=Enumerable.Range(0,options.Count).OrderByDescending(i=>options[i].damage+options[i].heal).First();
                            return 2+options[slot].damage+options[slot].heal;
                        }
                        case "e": return 4;
                        case "f": return 1.5;
                    }
                    return 0;
            }
            return 0;
        }
        static bool Inside(Cell center,Cell c,int r,bool square) => square?Math.Max(Math.Abs(c.x-center.x),Math.Abs(c.z-center.z))<=r:center.Distance(c)<=r;

        // Ataque: soma o valor em cada inimigo da área e desconta os aliados atingidos (a área pega aliados, salvo foes).
        static double Strike(MatchState s,Fighter me,SkillDef sk,Cell t)
        {
            var main=sk.selfArea?null:Board.At(s,t); var center=sk.selfArea?me.cell:t;
            var victims=new List<Fighter>(); if(main!=null) victims.Add(main);
            if(sk.area>0) victims.AddRange(s.fighters.Where(x=>x.Alive&&x!=main&&x!=me&&x.prisoned==0&&Inside(center,x.cell,sk.area,sk.square)&&(!sk.foes||!s.Ally(x.team,me.team))));
            double total=victims.Sum(v=>Hit(s,me,sk,v,v==main));
            if(sk.drain>0) total+=Math.Min(sk.drain,Content.MaxHp(me)-me.hp)*.5;
            if(sk.wild) total*=.83;
            return total;
        }
        static double Hit(MatchState s,Fighter me,SkillDef sk,Fighter v,bool main)
        {
            if(v==null) return 0;
            bool alone=sk.isolatedBonus>0&&!s.fighters.Any(x=>x.Alive&&x!=v&&s.Ally(x.team,v.team)&&x.cell.Distance(v.cell)==1);
            // Escudo ou Coringa no dado de defesa: 2 em 6.
            double dmg=sk.damage<=0?0:Math.Max(0,sk.damage+(alone?sk.isolatedBonus:0)-(sk.pierce?0:Armor(v))-(sk.pierce||sk.precise?0:1/3.0));
            if(s.Ally(v.team,me.team)) return -(dmg*1.5+(dmg>=v.hp?5:0));
            double val=dmg+(dmg>=v.hp-.01&&dmg>0?4+Content.Def(v).level:(Content.MaxHp(v)-v.hp)*.05);
            val+=Math.Min(sk.pmLoss,4)*.3+sk.paLoss*.5+(main?sk.prison*2:0)+sk.push*.2+(sk.pull&&main?.3:0);
            if(Content.Def(v).neutral) val*=.6;
            if(v.summoner!=0) val*=.7;
            return val;
        }
        static int Armor(Fighter v) => v.armor+Content.Def(v).armor+v.buffs.Sum(b=>b.armor)+(Content.Form(v)?.armor??0);
    }
}
