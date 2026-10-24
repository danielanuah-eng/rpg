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
            // Guarda ligada (revisão Claude, 23/10): mantém enquanto protege aliado com inimigo perto; senão desliga para agir.
            if(Match.GuardActive(me)) return GuardWorth(s,me)?Pass():new Command{kind="skill",skill=me.guardSkill??"guarda-direcional",cell=me.cell};
            var buy=Buy(s,me); if(buy!=null) return buy;
            // Labirinto: vela apagada colada acende na hora (tira a névoa e dá moedas).
            foreach(var d in Board.Dirs) if(Match.Interaction(s,me.cell+d)=="acender") return new Command{kind="acender",cell=me.cell+d};
            var plan=Search(sim,me,true);
            if(plan.cmd!=null&&plan.score>Worth) return plan.cmd;
            return Approach(s,me)??RaiseGuard(sim,me)??Pass();
        }
        // Guarda direcional vale quando há aliado no raio 2 e inimigo a até 6 casas.
        static bool GuardWorth(MatchState s,Fighter me) =>
            s.fighters.Any(x=>x.Alive&&x!=me&&s.Ally(x.team,me.team)&&Board.Gap(x,me)<=2)
            &&s.fighters.Any(x=>x.Alive&&!x.hidden&&!s.Ally(x.team,me.team)&&Board.Gap(x,me)<=6);
        // Sem nada melhor no fim do turno: liga a guarda virada para o inimigo mais perto.
        static Command RaiseGuard(Match sim,Fighter me)
        {
            var s=sim.S; if(!GuardWorth(s,me)) return null;
            var sk=Content.Def(me).skills.FirstOrDefault(x=>x.kind==SkillKind.GuardaDirecional&&(string.IsNullOrEmpty(x.form)||x.form==(string.IsNullOrEmpty(me.form)?Content.Def(me).startForm:me.form))); if(sk==null) return null;
            var foe=s.fighters.Where(x=>x.Alive&&!x.hidden&&!s.Ally(x.team,me.team)).OrderBy(x=>Board.Gap(x,me)).ThenBy(x=>x.id).FirstOrDefault(); if(foe==null) return null;
            foreach(var d in Board.Dirs.OrderBy(d=>(me.cell+d).Distance(foe.cell)))
                if(sim.CanTarget(me,sk,me.cell+d,out _)) return new Command{kind="skill",skill=sk.id,cell=me.cell+d};
            return null;
        }
        static Command Pass() => new Command{kind="passar"};

        // Tem compra possível: lutador de jogador e há produto que dá para pagar.
        static bool WantsShop(MatchState s,Fighter me) =>
            me.summoner==0&&MatchState.Player(me.team)&&s.shops[me.team].slots.Any(x=>!string.IsNullOrEmpty(x)&&Shop.Def(x).price<=s.coins[me.team]);
        // Loja: o produto mais caro que der para pagar, só numa casa ao lado da loja (invocações não compram).
        static Command Buy(MatchState s,Fighter me)
        {
            if(!Board.NextToShop(s,me)||!WantsShop(s,me)) return null;
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
            int nearOrigin=foes.Count(x=>Board.Gap(x,origin)==1&&!Content.Def(x).noLock);
            bool ranged=Reach(me)>=3;
            double best=0; Command cmd=null;
            foreach(var (c,cost) in cells)
            {
                me.cell=c;
                // Andar custa um pouco; sair colado em inimigos arrisca a Trava; atirador evita terminar colado.
                double place=-.12*cost-(cost>0?.6*nearOrigin:0)-(ranged?.4*foes.Count(x=>Board.Gap(x,c)==1):0);
                foreach(var sk in Match.Skills(me))
                {
                    if(me.pa<Match.Cost(me,sk,s)) continue;
                    if(sk.kind==SkillKind.Forma&&(!allowForm||c!=origin)) continue;
                    if(sk.kind==SkillKind.DoceSalgado&&c==origin) {
                        var friends=s.fighters.Where(x=>x.Alive&&x!=me&&x.prisoned==0&&s.Ally(x.team,me.team)).ToList();
                        foreach(var first in friends) foreach(var second in friends.Where(x=>x!=first).Concat(new[]{me})) {
                            if(!sim.CanTarget(me,sk,first.cell,second.cell,out _)) continue;
                            double value=new[]{me,first,second}.Distinct().Sum(x=>Math.Min(sk.heal,Math.Max(0,Content.MaxHp(x)-x.hp)))*1.1;
                            if(value>best) { best=value;cmd=new Command{kind="skill",skill=sk.id,cell=first.cell,cell2=second.cell}; }
                        }
                        continue;
                    }
                    foreach(var t in Targets(s,me,sk,foes))
                    {
                        if(sk.TwoCells||sk.channeled||!sim.CanTarget(me,sk,t,out _)) continue;
                        int slot=0; double v=Value(sim,me,sk,t,foes,ref slot);
                        if(v<=0) continue; v+=place;
                        if(v>best) { best=v; cmd=c==origin?new Command{kind="skill",skill=sk.id,cell=t,slot=slot,cell2=sk.kind==SkillKind.Comando?sim.Cats(me,sk,t)[0].cell:default}:new Command{kind="mover",cell=c}; }
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
            // Na névoa sem inimigo à vista: vai até a vela apagada ou, sem ela, explora rumo ao lado oposto do mapa.
            var goals=foes.Select(e=>e.cell).ToList();
            if(goals.Count==0&&s.fog) {
                var candle=Enumerable.Range(0,s.tiles.Length).Where(i=>s.tiles[i].Obj==TileObj.Vela&&!s.revealed).Select(i=>new Cell(i%s.width,i/s.width)).ToList();
                goals.Add(candle.Count>0?candle[0]:s.Mirror(me.cell));
            }
            // Distância a pé de cada inimigo até cada casa (ignorando lutadores no caminho e contando as portas).
            var fields=goals.Select(g=>Board.Costs(s,new Fighter{def=me.def,cell=g,climber=me.climber},true,true)).ToList();
            // Porta fechada colada que encurta o caminho: abre (1 PM).
            if(fields.Count>0) {
                int Near(Cell c) { int i=Board.Index(s,c); return fields.Min(f=>f[i]); }
                foreach(var d in Board.Dirs) { var c=me.cell+d; if(Board.Contains(s,c)&&s.At(c).terrain==Terrain.Porta&&Near(c)<Near(me.cell)) return new Command{kind="abrir",cell=c}; }
            }
            int reach=Reach(me), want=reach<=1?1:Math.Max(2,reach-1);
            double coinWeight=s.mode==VictoryMode.Tesouro?3:1.2, shopWeight=WantsShop(s,me)?1.5:0;
            double Score(Cell c,int cost)
            {
                int i=Board.Index(s,c);
                double sc=-.05*cost+s.At(c).coins*coinWeight;
                if(fields.Count>0) { int d=fields.Min(f=>f[i]==int.MaxValue?99:f[i]); sc-=Math.Abs(d-want)*(d<want?.6:1); }
                if(s.mode==VictoryMode.Dominio&&s.At(c).zone) sc+=2;
                if(s.At(c).lava) sc-=3;
                // Objetos fixos (18/10): baú vale como moedas, espetos ferem, braseiro colado dá PA (e ponto no Domínio).
                var obj=s.At(c).Obj;
                if(obj==TileObj.Bau) sc+=MapGen.BauMoedas*coinWeight; else if(obj==TileObj.Espetos&&!Content.Def(me).flying) sc-=1.5;
                if(Board.Dirs.Any(d=>Board.Contains(s,c+d)&&s.At(c+d).Obj==TileObj.Braseiro)) sc+=s.mode==VictoryMode.Dominio?1.5:.8;
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
        static bool SelfCast(SkillDef sk) => sk.selfArea||sk.kind==SkillKind.Forma||sk.kind==SkillKind.Pulso||sk.kind==SkillKind.Esconder||sk.kind==SkillKind.Aura||sk.kind==SkillKind.Desejo&&(string.IsNullOrEmpty(sk.wish)||sk.wish=="f");

        // Casas que vale testar para cada tipo de skill (o CanTarget confirma alcance, visão e regras).
        static IEnumerable<Cell> Targets(MatchState s,Fighter me,SkillDef sk,List<Fighter> foes)
        {
            if(SelfCast(sk)) return new[]{me.cell};
            var allies=s.fighters.Where(x=>x.Alive&&s.Ally(x.team,me.team)).Select(x=>x.cell);
            IEnumerable<Cell> Free(int r) => Ring(s,me.cell,r).Where(c=>Board.At(s,c)==null).OrderBy(c=>foes.Count==0?0:foes.Min(f=>Board.Gap(f,c))).Take(12);
            switch(sk.kind)
            {
                case SkillKind.Dano: case SkillKind.Espelho: case SkillKind.Salto: case SkillKind.Zona: return foes.Select(x=>x.cell);
                case SkillKind.Comando: return sk.step>0?Enumerable.Empty<Cell>():foes.Select(x=>x.cell).Concat(allies);
                case SkillKind.Cura: case SkillKind.Suporte: return allies;
                case SkillKind.Armadilha: return foes.SelectMany(f=>Board.Dirs.Select(d=>f.cell+d)).Where(c=>Board.Contains(s,c)&&Board.At(s,c)==null).Distinct();
                case SkillKind.Invocacao: case SkillKind.Reviver: return Free(Math.Min(sk.maxRange,6));
                // Teleporte entre sombras e teleporte de si (Blink Dagger): emerge colado num inimigo.
                case SkillKind.Teleporte: return sk.shadowStep||!sk.TwoCells?foes.SelectMany(f=>Board.Dirs.Select(d=>f.cell+d)).Where(c=>Board.Contains(s,c)&&Board.At(s,c)==null).Distinct():Enumerable.Empty<Cell>();
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
                    double val=list.Sum(x=>Math.Min(sk.heal,Content.MaxHp(x)-x.hp)+(sk.buffPa+sk.buffPm*.5+sk.buffArmor*(foes.Any(f=>Board.Gap(f,x)<=3)?1.2:.3)+sk.buffRange*.5)*.8);
                    if(sk.summonCount>0) val+=1.5*sk.summonCount;
                    return val;
                }
                case SkillKind.Invocacao: return foes.Count==0?0:2.6-.05*foes.Min(f=>Board.Gap(f,t));
                case SkillKind.Armadilha: return foes.Any(f=>Board.Gap(f,t)<=1)?.9:0;
                case SkillKind.Salto:
                {
                    // Vale se depois do salto ainda sobra PA para bater colado.
                    int left=me.pa-sk.pa;
                    return Match.Skills(me).Any(k=>k.kind==SkillKind.Dano&&k.minRange<=1&&k.pa<=left)?1.4:.3;
                }
                case SkillKind.Forma:
                {
                    // Forma temporária (Berserk, Asas, Motor): liga quando há inimigo perto. Forma fixa (Damian): compara a melhor jogada nas duas.
                    if(sk.turns>0) return foes.Any(f=>Board.Gap(f,me)<=4)?3:0;
                    string form=me.form; int pa=me.pa, pm=me.pm;
                    double now=Search(sim,me,false).score;
                    me.form=sk.toForm; me.pa-=sk.pa;
                    double after=Search(sim,me,false).score;
                    me.form=form; me.pa=pa; me.pm=pm;
                    return after-now-.3;
                }
                case SkillKind.Esconder: return foes.Any(f=>Board.Gap(f,me)<=1)?0:me.pa<=2?.6:.25;
                case SkillKind.Zona: return foes.Count(f=>Inside(t,f.cell,sk.area,sk.square))*.6;
                case SkillKind.Comando:
                    if(v==null) return 0;
                    return s.Ally(v.team,me.team)?Math.Min(2,Content.MaxHp(v)-v.hp)*.8:2;
                case SkillKind.Reviver: return 6;
                case SkillKind.Teleporte:
                {
                    int left=me.pa-Match.Cost(me,sk,s,t,null);
                    if(sk.shadowStep) return Match.Skills(me).Any(k=>k.kind==SkillKind.Dano&&k.damage>0&&k.pa<=left)?1.5:0;
                    // Teleporte de si: só se ainda não está colado em inimigo e sobra PA para bater corpo a corpo depois.
                    if(foes.Any(f=>Board.Gap(f,me)<=1)) return 0;
                    return Match.Skills(me).Any(k=>k.kind==SkillKind.Dano&&k.damage>0&&k.minRange<=1&&k.id!=sk.id&&k.pa<=left)?1.5:0;
                }
                // Pulso: vale pelos aliados que a aura da forma atual pega agora.
                case SkillKind.Pulso: return Content.Def(me).passives.Where(p=>p!=null&&(string.IsNullOrEmpty(p.form)||p.form==me.form)&&p.who!=PassiveWho.Si&&p.who!=PassiveWho.Inimigos)
                    .Sum(p=>s.fighters.Count(x=>x.Alive&&x!=me&&s.Ally(x.team,me.team)&&(p.range<=0||Board.Gap(x,me)<=p.range)))*1.5;
                case SkillKind.Aura:
                {
                    // Nunca desliga sozinho; liga quando há gente para curar ou ferir no alcance.
                    if(me.auraOn) return 0;
                    int count=s.fighters.Count(x=>x.Alive&&x!=me&&Board.Gap(x,me)<=sk.area&&(!s.Ally(x.team,me.team)&&!x.hidden||s.Ally(x.team,me.team)&&x.hp<Content.MaxHp(x)));
                    return count==0?0:1+count;
                }
                case SkillKind.Desejo:
                    switch(sk.wish)
                    {
                        case null: case "": return foes.Any(f=>Board.Gap(f,me)<=1)?1:3;
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
            bool alone=sk.isolatedBonus>0&&Match.Isolated(s,v);
            // Escudo ou Coringa no dado de defesa: 2 em 6.
            double average=sk.maxDamage>sk.damage?((double)sk.damage+sk.maxDamage)*.5:sk.damage;
            double dmg=average<=0?0:Math.Max(0,average+(alone?sk.isolatedBonus:0)-(sk.pierce||sk.ignoreArmor?0:Armor(v))-(sk.pierce||sk.precise?0:1/3.0));
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
