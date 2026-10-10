using System;
using System.Collections.Generic;
using System.Linq;

namespace ArenaTatica.Core
{
    // Regras da partida. Comando entra, eventos saem; a apresentação só anima os eventos.
    public sealed class Match
    {
        public MatchState S { get; private set; }
        // Só para testes: faces forçadas antes do RNG da partida.
        public readonly Queue<Face> Forced=new Queue<Face>();
        List<GameEvent> events=new List<GameEvent>();

        public static Match New(MatchSetup setup)
        {
            var players=setup.players!=null&&setup.players.Length>=2?setup.players:new[]{ new PlayerSetup{fighters=setup.team1}, new PlayerSetup{fighters=setup.team2} };
            int n=Math.Min(players.Length,MatchState.MaxPlayers), size=Board.SizeFor(n);
            var m=new Match(); var s=m.S=new MatchState { rng=setup.seed==0?1:setup.seed, mode=setup.mode, players=n, width=size, height=size };
            s.target=setup.target>0?setup.target:setup.mode==VictoryMode.Tesouro?15:5;
            // Times numerados 1..K na ordem dos assentos; time 0 no setup = jogador sozinho.
            var keys=new List<string>();
            for(int p=1;p<=n;p++) { string key=players[p-1].team>0?"t"+players[p-1].team:"p"+p; if(!keys.Contains(key)) keys.Add(key); s.alliance[p]=keys.IndexOf(key)+1; }
            foreach(int t in s.Teams()) s.galeoes[t]=setup.galeoes;
            MapGen.Generate(s,setup.flatMap);
            var slot=SlotOrder(s);
            for(int p=1;p<=n;p++) m.Place(p,slot[p],players[p-1].fighters);
            m.BuildTimeline();
            m.SpawnIllusions();
            if(!setup.flatMap) MapGen.SpawnCoins(s,Math.Max(2,n));
            for(int p=1;p<=n;p++) Shop.Refill(s,p);
            m.BeginTurn(s.Active);
            return m;
        }
        public static Match FromState(MatchState state) => new Match { S=state };

        // Grupo de spawn de cada jogador. Com 2 times, cada time fica de um lado do mapa; com 3 (2v2v2), esquerda, direita e centro.
        static int[] SlotOrder(MatchState s)
        {
            var slot=new int[s.players+1]; var teams=s.Teams(); var taken=new bool[s.players];
            int[][] groups=teams.Count==2&&s.players>2?new[]{ new[]{0,2,4}, new[]{1,3,5} }:teams.Count==3&&s.players==6?new[]{ new[]{0,3}, new[]{2,1}, new[]{4,5} }:null;
            var used=new int[teams.Count];
            for(int p=1;p<=s.players;p++)
            {
                slot[p]=-1; if(groups==null) { slot[p]=p-1; taken[p-1]=true; continue; }
                int t=teams.IndexOf(s.alliance[p]); var g=groups[t];
                while(used[t]<g.Length&&slot[p]<0) { int c=g[used[t]++]; if(c<s.players&&!taken[c]) { slot[p]=c; taken[c]=true; } }
            }
            for(int p=1;p<=s.players;p++) if(slot[p]<0) { int c=Array.IndexOf(taken,false); slot[p]=c; taken[c]=true; }
            return slot;
        }
        void Place(int player,int slot,string[] defs)
        {
            var spawns=MapGen.Spawns(S,slot);
            for(int i=0;i<defs.Length&&i<spawns.Length;i++) Spawn(defs[i],player,spawns[i],0);
        }
        Fighter Spawn(string def,int team,Cell cell,int summoner)
        {
            var d=Content.Fighters[def];
            var f=new Fighter { id=S.nextId++, def=def, team=team, hp=d.hp, cell=cell, summoner=summoner, form=d.startForm };
            S.fighters.Add(f); return f;
        }
        // Ezequiela começa com uma cópia ilusória dela numa casa vizinha livre; os inimigos a veem como Ezequiela.
        void SpawnIllusions()
        {
            foreach(var f in S.fighters.Where(x=>x.summoner==0&&Content.Def(x).illusion).ToList())
            {
                var free=Board.Dirs.Select(d=>f.cell+d).Concat(Board.Dirs.Select(d=>f.cell+d+d)).FirstOrDefault(c=>Board.Contains(S,c)&&Board.Walkable(S,c,false)&&Board.At(S,c)==null);
                if(!Board.Contains(S,free)||free==f.cell) continue;
                var copy=Spawn("ilusao",f.team,free,f.id); copy.disguise=f.def;
                S.timeline.Insert(S.timeline.IndexOf(f.id)+1,copy.id);
            }
        }
        // Krosmaster: times alternam em rodízio; dentro do time, maior Iniciativa primeiro; começa o time com a maior Iniciativa
        // (empate: o assento menor). Todos contra todos = cada jogador é um time.
        void BuildTimeline()
        {
            int Init(Fighter f) => Content.Def(f).initiative;
            var groups=S.Teams().Select(t=>S.fighters.Where(f=>S.Team(f.team)==t&&f.summoner==0).OrderByDescending(Init).ThenBy(f=>f.id).ToList())
                .Where(g=>g.Count>0).OrderByDescending(g=>Init(g[0])).ToList();
            S.timeline.Clear();
            for(int i=0;i<groups.Max(g=>g.Count);i++) foreach(var g in groups) if(i<g.Count) S.timeline.Add(g[i].id);
            S.active=0;
        }

        public bool Apply(Command cmd,int team,out List<GameEvent> result,out string reason)
        {
            events=new List<GameEvent>(); result=events; reason="";
            var f=S.Active;
            if(cmd==null) { reason="Comando vazio."; return false; }
            if(S.over) { reason="Partida encerrada."; return false; }
            if(cmd.revision!=S.revision) { reason="Estado atualizado. Repita a ação."; return false; }
            if(f==null||f.id!=cmd.fighter||f.team!=team) { reason="Aguarde seu turno."; return false; }
            bool ok;
            switch(cmd.kind)
            {
                case "mover": ok=Move(f,cmd.cell,out reason); break;
                case "skill": ok=UseSkill(f,cmd.skill,cmd.cell,out reason); break;
                case "comprar": ok=Buy(f,cmd.slot,out reason); break;
                case "passar": ok=true; Advance(); break;
                default: reason="Ação desconhecida."; ok=false; break;
            }
            if(ok)
            {
                CheckVictory();
                // Nocauteado no próprio turno (armadilha): o turno acaba.
                if(!S.over&&S.Active!=null&&!S.Active.Alive) Advance();
                S.revision++;
            }
            return ok;
        }

        bool Move(Fighter f,Cell to,out string reason)
        {
            reason="";
            var path=Board.Path(S,f,to,f.pm);
            if(path==null) { reason="Destino bloqueado, ocupado ou além dos PM."; return false; }
            bool flying=Content.Def(f).flying;
            var walked=new List<Cell>{f.cell}; var after=new List<GameEvent>();
            for(int i=1;i<path.Count;i++)
            {
                if(!Tackle(f,after)) break;
                f.pm-=Board.StepCost(S,f.cell,path[i],flying); f.cell=path[i]; walked.Add(f.cell);
                var t=S.At(f.cell);
                if(t.coins>0) { S.coins[f.team]+=t.coins; S.collected[S.Team(f.team)]+=t.coins; after.Add(new GameEvent{kind="moeda",actor=f.id,cell=f.cell,value=t.coins}); t.coins=0; }
                if(t.trap!=0&&!S.Ally(t.trap,f.team)&&!flying) { Spring(f,t,after); break; }
            }
            if(walked.Count>1) Emit(new GameEvent{kind="mover",actor=f.id,path=walked.ToArray()});
            events.AddRange(after);
            if(!f.Alive) KO(f);
            return true;
        }
        // Armadilha inimiga: dano fixo menos armadura e o movimento acaba ali.
        void Spring(Fighter f,Tile t,List<GameEvent> log)
        {
            int dmg=Math.Max(0,t.trapDamage-Armor(f));
            f.pm=0;
            log.Add(new GameEvent{kind="armadilha",actor=t.trap,target=f.id,cell=f.cell,value=dmg});
            t.trap=0; t.trapDamage=0;
            Hurt(f,dmg,log);
        }
        int Armor(Fighter f) => f.armor+f.buffs.Sum(b=>b.armor)+(Content.Form(f)?.armor??0);
        // Dano já calculado chega ao lutador: guarda do Tchu tcha (aliado até 2 casas absorve 1), revela quem estava escondido.
        // O nocaute (e o renascer) fica para quem chamou, depois do evento do golpe.
        void Hurt(Fighter v,int dmg,List<GameEvent> log)
        {
            if(dmg<=0) return;
            var guard=S.fighters.FirstOrDefault(g=>g.Alive&&g.id!=v.id&&S.Ally(g.team,v.team)&&Content.Def(g).guard&&!(Content.Form(g)?.noGuard??false)&&g.cell.Distance(v.cell)<=2);
            if(guard!=null) { dmg--; guard.hp=Math.Max(0,guard.hp-1); log.Add(new GameEvent{kind="guarda",actor=guard.id,target=v.id,value=1,cell=guard.cell}); }
            v.hp=Math.Max(0,v.hp-dmg);
            if(v.hidden) { v.hidden=false; log.Add(new GameEvent{kind="revelar",target=v.id,cell=v.cell}); }
            if(guard!=null&&!guard.Alive) KO(guard,log);
        }

        // 1 compra por turno do lutador; Item e Buff valem na hora, o resto vira carga de skill.
        bool Buy(Fighter f,int slot,out string reason)
        {
            reason="";
            var shop=S.shops[f.team].slots;
            if(f.summoner!=0) { reason="Invocações não compram."; return false; }
            if(f.bought) { reason="Já comprou neste turno."; return false; }
            if(slot<0||slot>=shop.Length||string.IsNullOrEmpty(shop[slot])) { reason="Espaço vazio."; return false; }
            var d=Shop.Def(shop[slot]);
            if(S.coins[f.team]<d.price) { reason="Moedas insuficientes."; return false; }
            S.coins[f.team]-=d.price; shop[slot]=null; f.bought=true;
            switch(d.kind)
            {
                case ShopKind.Item: f.bonusPa+=d.pa; f.bonusPm+=d.pm; f.bonusHp+=d.hp; f.armor+=d.armor; f.bonusRange+=d.range; f.pa+=d.pa; f.pm+=d.pm; f.hp+=d.hp; break;
                case ShopKind.Buff: f.buffs.Add(new Buff{id=d.id,turns=d.turns,pa=d.pa,pm=d.pm,armor=d.armor,range=d.range}); f.pa+=d.pa; f.pm+=d.pm; break;
                default:
                    var c=f.charges.Find(x=>x.skill==d.skill.id);
                    if(c==null) f.charges.Add(new Charge{skill=d.skill.id,uses=d.uses}); else c.uses+=d.uses;
                    break;
            }
            Emit(new GameEvent{kind="compra",actor=f.id,target=slot,value=d.price,info=d.id});
            return true;
        }
        // Sair de casa vizinha a inimigos: cada inimigo rola 1 dado; cada Trava precisa de uma Esquiva. Falhou: perde PA e PM.
        bool Tackle(Fighter f,List<GameEvent> log)
        {
            var enemies=S.fighters.Where(e=>e.Alive&&!S.Ally(e.team,f.team)&&e.cell.Distance(f.cell)==1).ToList();
            if(enemies.Count==0) return true;
            var locks=enemies.Select(_=>Roll()).ToArray(); int need=locks.Count(Dice.Lock);
            var dodges=Enumerable.Range(0,need).Select(_=>Roll()).ToArray();
            bool free=dodges.Count(Dice.Dodge)>=need;
            log.Add(new GameEvent{kind="trava",actor=f.id,cell=f.cell,value=free?0:1,dice=locks.Concat(dodges).ToArray()});
            if(!free) { f.pa=0; f.pm=0; }
            return free;
        }

        public int Range(Fighter f,SkillDef sk,Cell target) => sk.maxRange+(sk.modifiable?(S.At(f.cell).height>S.At(target).height?1:0)+f.bonusRange+f.buffs.Sum(b=>b.range):0);
        // Só geometria (alcance, linha reta, visão): usado também para desenhar a área da skill.
        public bool InRange(Fighter f,SkillDef sk,Cell c,out string reason)
        {
            reason="";
            if(!Board.Contains(S,c)) { reason="Fora do mapa."; return false; }
            int dist=f.cell.Distance(c);
            if(dist<sk.minRange||dist>Range(f,sk,c)) { reason="Fora de alcance."; return false; }
            if(sk.line&&f.cell.x!=c.x&&f.cell.z!=c.z) { reason="Só em linha reta."; return false; }
            if(sk.sight&&!Board.LineOfSight(S,f.cell,c)) { reason="Sem linha de visão."; return false; }
            return true;
        }
        // Skills que o lutador pode usar agora: as próprias e as cargas compradas.
        // Formas: só aparecem as skills da forma atual, e a troca para a forma em que já está some.
        public static List<SkillDef> Skills(Fighter f)
        {
            var list=Content.Def(f).skills.Where(s=>(string.IsNullOrEmpty(s.form)||s.form==f.form)&&!(s.kind==SkillKind.Forma&&s.toForm==f.form)).ToList();
            foreach(var c in f.charges) { var sk=Content.AnySkill(c.skill); if(sk!=null&&c.uses>0&&!list.Contains(sk)) list.Add(sk); }
            return list;
        }
        // Cópia do estado que um jogador pode ver: sem a semente do RNG e sem as armadilhas de quem não é aliado.
        public static MatchState ViewFor(MatchState s,int player)
        {
            var v=UnityEngine.JsonUtility.FromJson<MatchState>(UnityEngine.JsonUtility.ToJson(s));
            v.rng=0;
            foreach(var t in v.tiles) if(t.trap!=0&&!s.Ally(t.trap,player)) { t.trap=0; t.trapDamage=0; }
            // Ilusão: para os inimigos é igual à original (mesmo nome, PV e sem marca de invocação).
            foreach(var f in v.fighters.Where(x=>!string.IsNullOrEmpty(x.disguise)&&!s.Ally(x.team,player)))
            {
                var real=v.fighters.Find(x=>x.id==f.summoner);
                f.def=f.disguise; f.summoner=0; if(real!=null) { f.hp=real.Alive?real.hp:f.hp; f.form=real.form; }
            }
            return v;
        }
        public static List<GameEvent> EventsFor(List<GameEvent> events,MatchState s,int player) =>
            events.Where(e=>e.kind!="armadilha-posta"||(s.F(e.actor) is Fighter a&&s.Ally(a.team,player))).ToList();

        public bool CanTarget(Fighter f,SkillDef sk,Cell c,out string reason)
        {
            reason="";
            if(sk==null) { reason="Skill desconhecida."; return false; }
            if(!Board.Contains(S,c)) { reason="Fora do mapa."; return false; }
            if(f.pa<sk.pa) { reason="PA insuficientes."; return false; }
            if(sk.perTurn>0&&f.used.Count(u=>u==sk.id)>=sk.perTurn) { reason="Limite por turno atingido."; return false; }
            if(sk.perMatch>0&&f.usedMatch.Count(u=>u==sk.id)>=sk.perMatch) { reason="Já usada nesta partida."; return false; }
            if(!InRange(f,sk,c,out reason)) return false;
            var v=Board.At(S,c);
            // Escondido: para o inimigo a casa parece vazia.
            if(v!=null&&v.hidden&&!S.Ally(v.team,f.team)) v=null;
            switch(sk.kind)
            {
                case SkillKind.Dano: if(v==null||S.Ally(v.team,f.team)) { reason="Alvo inválido."; return false; } break;
                case SkillKind.Cura: if(v==null||!S.Ally(v.team,f.team)) { reason="Alvo inválido."; return false; } break;
                case SkillKind.Armadilha: if(v!=null||!Board.Walkable(S,c,false)||S.At(c).trap!=0) { reason="Casa ocupada ou bloqueada."; return false; } break;
                case SkillKind.Teleporte: if(Board.At(S,c)!=null||!Board.Walkable(S,c,Content.Def(f).flying)) { reason="Casa ocupada ou bloqueada."; return false; } break;
                case SkillKind.Salto:
                    if(v==null||S.Ally(v.team,f.team)) { reason="Salte na direção de um inimigo."; return false; }
                    if(!Landing(f,v,out _)) { reason="Não há casa livre junto do alvo."; return false; }
                    break;
                case SkillKind.Forma:
                    if(c!=f.cell) { reason="Use em si mesmo."; return false; }
                    if(sk.toForm==f.form) { reason="Já está nessa forma."; return false; }
                    break;
                case SkillKind.Esconder:
                    if(c!=f.cell) { reason="Use em si mesmo."; return false; }
                    if(f.hidden) { reason="Já está escondido."; return false; }
                    if(!Cover(f.cell)) { reason="Precisa estar junto a relevo mais alto, árvore ou caixa."; return false; }
                    break;
                case SkillKind.Espelho:
                    if(v==null||string.IsNullOrEmpty(v.lastSkill)) { reason="Escolha alguém que já usou uma skill."; return false; }
                    if(Content.AnySkill(v.lastSkill)?.kind==SkillKind.Espelho) { reason="O espelho não copia a si mesmo."; return false; }
                    break;
                default:
                    var d=Content.Fighters[sk.summon];
                    // Máximo 1 ativa por tipo: o lobo da loja convive com o dragão da Ciantia.
                    if(S.fighters.Any(x=>x.Alive&&x.summoner==f.id&&x.def==sk.summon)) { reason="Já existe uma invocação ativa."; return false; }
                    if(v!=null||!Board.Walkable(S,c,d.flying)) { reason="Casa ocupada ou bloqueada."; return false; }
                    break;
            }
            return true;
        }
        bool UseSkill(Fighter f,string id,Cell c,out string reason)
        {
            var sk=Content.Skill(f,id);
            if(!CanTarget(f,sk,c,out reason)) return false;
            f.pa-=sk.pa; f.used.Add(sk.id); if(sk.perMatch>0) f.usedMatch.Add(sk.id);
            if(sk.kind!=SkillKind.Espelho) f.lastSkill=sk.id;
            var charge=f.charges.Find(x=>x.skill==sk.id);
            if(charge!=null&&Array.Find(Content.Def(f).skills,x=>x.id==sk.id)==null&&--charge.uses<=0) f.charges.Remove(charge);
            switch(sk.kind)
            {
                case SkillKind.Cura:
                {
                    var v=Board.At(S,c); int heal=Math.Min(sk.damage,Content.MaxHp(v)-v.hp); v.hp+=heal;
                    Emit(new GameEvent{kind="cura",actor=f.id,target=v.id,value=heal,info=sk.id,cell=c});
                    break;
                }
                case SkillKind.Armadilha:
                    S.At(c).trap=f.team; S.At(c).trapDamage=sk.damage;
                    Emit(new GameEvent{kind="armadilha-posta",actor=f.id,info=sk.id,cell=c});
                    break;
                case SkillKind.Dano: Strike(f,sk,c); break;
                case SkillKind.Teleporte:
                    Emit(new GameEvent{kind="teleporte",actor=f.id,info=sk.id,path=new[]{f.cell,c},cell=c});
                    f.cell=c; Collect(f);
                    break;
                case SkillKind.Salto:
                {
                    Landing(f,Board.At(S,c),out var land);
                    Emit(new GameEvent{kind="salto",actor=f.id,target=Board.At(S,c).id,info=sk.id,path=new[]{f.cell,land},cell=land});
                    f.cell=land; Collect(f);
                    break;
                }
                case SkillKind.Forma: SetForm(f,sk.toForm,sk.turns,sk.id); break;
                case SkillKind.Esconder:
                    f.hidden=true; Emit(new GameEvent{kind="esconder",actor=f.id,info=sk.id,cell=f.cell});
                    break;
                case SkillKind.Espelho:
                {
                    var v=Board.At(S,c); var copied=v.lastSkill;
                    var have=f.charges.Find(x=>x.skill==copied); if(have==null) f.charges.Add(new Charge{skill=copied,uses=1}); else have.uses++;
                    Emit(new GameEvent{kind="espelho",actor=f.id,target=v.id,info=copied,cell=c});
                    break;
                }
                default:
                {
                    var s=Spawn(sk.summon,f.team,c,f.id);
                    S.timeline.Insert(S.active+1,s.id);
                    Emit(new GameEvent{kind="invocar",actor=f.id,target=s.id,info=sk.summon,cell=c});
                    break;
                }
            }
            if(sk.endsForm) SetForm(f,Content.Def(f).startForm,0,sk.id);
            return true;
        }
        // Ataque: dado de ataque e de defesa, bônus de aura, área, erro do golpe impreciso, roubo, dreno e perda de PM.
        void Strike(Fighter f,SkillDef sk,Cell c)
        {
            Face a=Roll(), d=Roll();
            var main=Board.At(S,c);
            bool missed=sk.wild&&(a==Face.Esquiva||a==Face.CoringaEsquivaTrava);
            bool ambush=f.hidden;
            int bonus=S.fighters.Where(g=>g.Alive&&g.id!=f.id&&S.Ally(g.team,f.team)&&Content.Def(g).allyBonus>0&&g.cell.Distance(f.cell)<=Content.Def(g).auraRange).Select(g=>Content.Def(g).allyBonus).DefaultIfEmpty(0).Max();
            if(ambush) { bonus++; f.hidden=false; }
            var targets=new List<Fighter>{main};
            if(sk.area>0) targets.AddRange(S.fighters.Where(x=>x.Alive&&x!=main&&!S.Ally(x.team,f.team)&&x.cell.Distance(c)<=sk.area));
            var log=new List<GameEvent>(); int dealt=0;
            foreach(var v in targets)
            {
                int dmg=missed?0:Math.Max(0,sk.damage+bonus-(Dice.Armor(d)&&!sk.precise?1:0)-Armor(v));
                Emit(new GameEvent{kind="ataque",actor=f.id,target=v.id,value=missed?-1:dmg,info=sk.id,cell=v.cell,dice=v==main?new[]{a,d}:null});
                Hurt(v,dmg,log); dealt+=dmg;
                if(dmg>0&&sk.pmLoss>0) v.buffs.Add(new Buff{id="confusao",turns=2,pm=-sk.pmLoss});
                if(dmg>0&&v==main&&Content.Def(f).steal>0&&S.coins[v.team]>0&&Dice.Range(S,100)<Content.Def(f).steal*(ambush?2:1))
                {
                    S.coins[v.team]--; S.coins[f.team]++;
                    log.Add(new GameEvent{kind="roubo",actor=f.id,target=v.id,value=1,cell=v.cell});
                }
            }
            if(ambush) events.Insert(events.Count-targets.Count,new GameEvent{kind="revelar",target=f.id,cell=f.cell});
            if(sk.drain>0&&dealt>0&&f.Alive) { int heal=Math.Min(sk.drain,Content.MaxHp(f)-f.hp); if(heal>0) { f.hp+=heal; log.Add(new GameEvent{kind="cura",actor=f.id,target=f.id,value=heal,info=sk.id,cell=f.cell}); } }
            events.AddRange(log);
            foreach(var v in targets) if(!v.Alive) KO(v);
            // Decisão B: Cataclisma só sai no dado de ataque.
            if(a==Face.Cataclisma) Cataclysm();
        }
        void SetForm(Fighter f,string form,int turns,string info)
        {
            var before=Content.Form(f); f.form=form; f.formTurns=turns; var after=Content.Form(f);
            // Bônus de PA/PM da forma nova valem já neste turno; os da antiga saem.
            f.pa=Math.Max(0,f.pa-(before?.pa??0)+(after?.pa??0)); f.pm=Math.Max(0,f.pm-(before?.pm??0)+(after?.pm??0));
            Emit(new GameEvent{kind="forma",actor=f.id,info=form,value=turns,cell=f.cell});
        }
        // Moeda na casa onde o lutador chega sem andar (teleporte, salto).
        void Collect(Fighter f)
        {
            var t=S.At(f.cell);
            if(t.coins>0) { S.coins[f.team]+=t.coins; S.collected[S.Team(f.team)]+=t.coins; Emit(new GameEvent{kind="moeda",actor=f.id,cell=f.cell,value=t.coins}); t.coins=0; }
        }
        // Salto: casa livre vizinha ao alvo, a mais perto de quem salta (empate: a primeira de Board.Dirs).
        bool Landing(Fighter f,Fighter target,out Cell land)
        {
            land=default; if(target==null) return false;
            var options=Board.Dirs.Select(d=>target.cell+d).Where(x=>Board.Contains(S,x)&&Board.Walkable(S,x,Content.Def(f).flying)&&(Board.At(S,x)==null||x==f.cell)).ToList();
            if(options.Count==0) return false;
            land=options.OrderBy(x=>x.Distance(f.cell)).First(); return true;
        }
        // Cobertura para se esconder: casa vizinha mais alta, árvore ou caixa.
        bool Cover(Cell c) => Board.Dirs.Select(d=>c+d).Any(x=>Board.Contains(S,x)&&(S.At(x).height>S.At(c).height||S.At(x).terrain==Terrain.Arvore||S.At(x).terrain==Terrain.Caixa));
        void KO(Fighter v,List<GameEvent> log=null)
        {
            var add=log??events;
            // Devon: o primeiro nocaute vira renascimento, sem perder Galeões.
            if(Content.Def(v).revive>0&&!v.revived)
            {
                v.revived=true; v.hp=Math.Min(Content.Def(v).revive,Content.MaxHp(v));
                add.Add(new GameEvent{kind="nocaute",target=v.id,cell=v.cell,info="renasce"});
                add.Add(new GameEvent{kind="renascer",target=v.id,value=v.hp,cell=v.cell});
                return;
            }
            v.hp=0; v.hidden=false; add.Add(new GameEvent{kind="nocaute",target=v.id,cell=v.cell});
            if(v.summoner==0) { int t=S.Team(v.team); S.galeoes[t]=Math.Max(0,S.galeoes[t]-Content.Def(v).level); }
            foreach(var s in S.fighters.Where(x=>x.Alive&&x.summoner==v.id).ToList()) KO(s,log);
        }

        void BeginTurn(Fighter f)
        {
            if(f==null) return;
            foreach(var b in f.buffs) b.turns--;
            f.buffs.RemoveAll(b=>b.turns<=0);
            // Forma com duração: conta os turnos do próprio lutador e volta à forma inicial.
            if(f.formTurns>0&&--f.formTurns==0) { f.form=Content.Def(f).startForm; Emit(new GameEvent{kind="forma",actor=f.id,info=f.form,cell=f.cell}); }
            var d=Content.Def(f); var form=Content.Form(f);
            f.pa=Math.Max(0,d.pa+f.bonusPa+f.buffs.Sum(b=>b.pa)+(form?.pa??0)); f.pm=Math.Max(0,d.pm+f.bonusPm+f.buffs.Sum(b=>b.pm)+(form?.pm??0)); f.used.Clear(); f.bought=false;
            Emit(new GameEvent{kind="turno",actor=f.id,value=S.round});
        }
        void Advance()
        {
            var ending=S.Active;
            if(ending!=null&&ending.bought)
            {
                Shop.Refill(S,ending.team);
                Emit(new GameEvent{kind="loja",actor=ending.team,info=string.Join(",",S.shops[ending.team].slots)});
            }
            for(int i=0;i<S.timeline.Count&&!S.over;i++)
            {
                if(++S.active>=S.timeline.Count) { S.active=0; EndRound(); if(S.over) return; }
                if(S.Active.Alive) { BeginTurn(S.Active); return; }
            }
        }
        void EndRound()
        {
            if(S.mode==VictoryMode.Dominio)
            {
                var present=S.fighters.Where(f=>f.Alive&&f.summoner==0&&S.At(f.cell).zone).Select(f=>S.Team(f.team)).Distinct().ToList();
                if(present.Count==1) { S.domain[present[0]]++; Emit(new GameEvent{kind="dominio",actor=present[0],value=S.domain[present[0]]}); }
            }
            S.round++; MapGen.SpawnCoins(S,Math.Max(1,S.players/2));
            Emit(new GameEvent{kind="rodada",value=S.round});
            CheckVictory();
        }

        // Eventos espelhados para não favorecer um time. Mudanças que isolam alguém são desfeitas.
        static readonly string[] Cataclysms={"chuva-de-moedas","queda-de-pedras","terremoto"};
        void Cataclysm()
        {
            string kind=Cataclysms[Dice.Range(S,Cataclysms.Length)];
            var cells=new List<Cell>();
            if(kind=="chuva-de-moedas") { MapGen.SpawnCoins(S,1); }
            else
            {
                var backup=S.tiles.Select(t=>new Tile{height=t.height,coins=t.coins,terrain=t.terrain,zone=t.zone}).ToArray();
                if(kind=="queda-de-pedras")
                    for(int i=0;i<1;i++) { var c=new Cell(Dice.Range(S,S.width),Dice.Range(S,S.height)); if(c==S.Mirror(c)||!MapGen.Free(S,c)||!MapGen.Free(S,S.Mirror(c))||S.At(c).zone) continue; S.At(c).terrain=S.At(S.Mirror(c)).terrain=Terrain.Caixa; cells.Add(c); cells.Add(S.Mirror(c)); }
                else
                {
                    var o=new Cell(1+Dice.Range(S,S.width-2),1+Dice.Range(S,S.height/2-1)); int delta=Dice.Range(S,2)==0?1:-1;
                    for(int x=-1;x<=1;x++) for(int z=-1;z<=1;z++) foreach(var c in new[]{o+new Cell(x,z),S.Mirror(o+new Cell(x,z))})
                        if(Board.Contains(S,c)&&!cells.Contains(c)) { S.At(c).height=Math.Max(0,Math.Min(Board.MaxHeight,S.At(c).height+delta)); cells.Add(c); }
                }
                if(!Board.Connected(S)) { S.tiles=backup; cells.Clear(); }
            }
            Emit(new GameEvent{kind="cataclisma",info=kind,path=cells.ToArray()});
        }

        void CheckVictory()
        {
            if(S.over) return;
            // Time sem lutadores ou sem Galeões sai da partida; o que restar dele sai do tabuleiro.
            var standing=new List<int>();
            foreach(int t in S.Teams())
            {
                if(S.eliminated[t]) continue;
                bool alive=S.fighters.Any(f=>f.Alive&&S.Team(f.team)==t&&f.summoner==0);
                if(alive&&!(S.mode==VictoryMode.Galeoes&&S.galeoes[t]<=0)) { standing.Add(t); continue; }
                S.eliminated[t]=true;
                foreach(var f in S.fighters.Where(f=>f.Alive&&S.Team(f.team)==t)) { f.hp=0; Emit(new GameEvent{kind="nocaute",target=f.id,cell=f.cell}); }
                Emit(new GameEvent{kind="eliminado",actor=t});
            }
            int winner=0;
            // Meta de Tesouro/Domínio: o time de quem está agindo tem prioridade (acabou de alcançar).
            int[] score=S.mode==VictoryMode.Tesouro?S.collected:S.mode==VictoryMode.Dominio?S.domain:null;
            if(score!=null) winner=standing.OrderBy(t=>t==S.Team(S.Active?.team??0)?0:1).FirstOrDefault(t=>score[t]>=S.target);
            if(winner==0&&standing.Count>1) return;
            if(winner==0&&standing.Count==1) winner=standing[0];
            // winner 0 = empate (todos caíram juntos).
            S.over=true; S.winner=winner; Emit(new GameEvent{kind="vitoria",actor=winner,info=S.mode.ToString()});
        }

        Face Roll() => Forced.Count>0?Forced.Dequeue():Dice.Roll(S);
        void Emit(GameEvent e) => events.Add(e);
    }
}
