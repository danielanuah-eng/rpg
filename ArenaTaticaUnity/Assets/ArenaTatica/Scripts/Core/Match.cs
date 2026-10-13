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
            var m=new Match(); var s=m.S=new MatchState { rng=setup.seed==0?1:setup.seed, mode=setup.mode, players=n, width=size, height=size, edits=Content.EditsJson,
                turnMode=setup.turns, shopPool=(setup.shopItems??new string[0]).Where(id=>Shop.Def(id)!=null).Distinct().ToArray() };
            s.target=setup.target>0?setup.target:setup.mode==VictoryMode.Tesouro?15:5;
            // Times numerados 1..K na ordem dos assentos; time 0 no setup = jogador sozinho.
            var keys=new List<string>();
            for(int p=1;p<=n;p++) { string key=players[p-1].team>0?"t"+players[p-1].team:"p"+p; if(!keys.Contains(key)) keys.Add(key); s.alliance[p]=keys.IndexOf(key)+1; }
            foreach(int t in s.Teams()) s.galeoes[t]=setup.galeoes;
            MapGen.Generate(s,setup.flatMap);
            if(!setup.flatMap) MapGen.Decorate(s,setup.seed);
            s.theme=MapGen.Theme(setup);
            if(MapGen.Fits(setup.map,size)) { MapGen.Load(s,setup.map); if(setup.map.theme>=0) s.theme=setup.map.theme%MapGen.Themes.Length; }
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
        // Ezequiela começa com uma cópia ilusória dela colada (vizinha livre sorteada); os inimigos a veem como Ezequiela.
        // Autor (13/10): sempre uma do lado da outra, mas sem dar para saber qual é qual: casa, id, ordem na lista e na
        // linha do tempo são sorteados entre as duas.
        void SpawnIllusions()
        {
            foreach(var f in S.fighters.Where(x=>x.summoner==0&&Content.Def(x).illusion).ToList())
            {
                bool Free(Cell c) => Board.Contains(S,c)&&Board.Walkable(S,c,false)&&Board.At(S,c)==null;
                var near=Board.Dirs.Select(d=>f.cell+d).Where(Free).ToList();
                if(near.Count==0) near=Board.Dirs.Select(d=>f.cell+d+d).Where(Free).ToList();
                if(near.Count==0) continue;
                var copy=Spawn("ilusao",f.team,near[Dice.Range(S,near.Count)],f.id); copy.disguise=f.def;
                if(Dice.Range(S,2)==0) { var c=f.cell; f.cell=copy.cell; copy.cell=c; }
                if(Dice.Range(S,2)==0)
                {
                    int i=S.timeline.IndexOf(f.id); (f.id,copy.id)=(copy.id,f.id); copy.summoner=f.id; S.timeline[i]=f.id;
                    S.fighters.Remove(copy); S.fighters.Insert(S.fighters.IndexOf(f),copy);
                }
                int at=S.timeline.IndexOf(f.id); S.timeline.Insert(Dice.Range(S,2)==0?at:at+1,copy.id);
            }
        }
        // Krosmaster (livro de regras): no turno de um jogador, TODOS os lutadores dele jogam em sequência, da maior Iniciativa para a menor
        // (a "linha do tempo"); depois passa a vez. Começa o time com a maior soma de Iniciativa (empate: o lutador de maior Iniciativa;
        // depois o assento menor). Em times (2v2, 3v3) os jogadores se revezam entre os times. Todos contra todos = cada jogador é um time.
        void BuildTimeline()
        {
            int Init(Fighter f) => Content.Def(f).initiative;
            List<Fighter> Own(int seat) => S.fighters.Where(f=>f.team==seat&&f.summoner==0).OrderByDescending(Init).ThenBy(f=>f.id).ToList();
            (int sum,int top) Score(IEnumerable<Fighter> g) => (g.Sum(Init),g.Select(Init).DefaultIfEmpty(0).Max());
            var seats=S.fighters.Where(f=>f.summoner==0&&MatchState.Player(f.team)).Select(f=>f.team).Distinct().ToList();
            var teams=seats.GroupBy(S.Team).Select(g=>g.OrderByDescending(p=>Score(Own(p)).sum).ThenByDescending(p=>Score(Own(p)).top).ThenBy(p=>p).ToList())
                .OrderByDescending(g=>Score(g.SelectMany(Own)).sum).ThenByDescending(g=>Score(g.SelectMany(Own)).top).ThenBy(g=>S.Team(g[0])).ToList();
            S.timeline.Clear();
            if(S.turnMode==TurnMode.Alternada)
            {
                // Iniciativa alternada: cada time põe na fila o seu próximo lutador (maior Iniciativa primeiro; no empate,
                // os jogadores do time se revezam).
                var queues=teams.Select(g=>{ var lists=g.Select(Own).ToList(); var q=new List<Fighter>();
                    for(int i=0;i<lists.Max(l=>l.Count);i++) foreach(var l in lists) if(i<l.Count) q.Add(l[i]);
                    return q.OrderByDescending(Init).ThenBy(f=>q.IndexOf(f)).ToList(); }).ToList();
                for(int i=0;i<queues.Max(q=>q.Count);i++) foreach(var q in queues) if(i<q.Count) S.timeline.Add(q[i].id);
            }
            else for(int i=0;i<teams.Max(g=>g.Count);i++) foreach(var g in teams) if(i<g.Count) S.timeline.AddRange(Own(g[i]).Select(f=>f.id));
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
                case "skill": ok=UseSkill(f,cmd.skill,cmd.cell,cmd.slot,out reason); break;
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
                f.pm-=Board.StepCost(S,f.cell,path[i],flying,f.climber); f.cell=path[i]; walked.Add(f.cell);
                var t=S.At(f.cell);
                if(t.coins>0) { S.coins[f.team]+=t.coins; S.collected[S.Team(f.team)]+=t.coins; after.Add(new GameEvent{kind="moeda",actor=f.id,cell=f.cell,value=t.coins}); t.coins=0; }
                if(t.lava&&!flying) { Burn(f,after); if(!f.Alive) break; }
                if(t.trap!=0&&!S.Ally(t.trap,f.team)&&!flying) { Spring(f,t,after); break; }
            }
            if(walked.Count>1) Emit(new GameEvent{kind="mover",actor=f.id,path=walked.ToArray()});
            // Esbarrar: quem termina o movimento colado num inimigo escondido o revela.
            if(f.Alive) foreach(var h in S.fighters.Where(x=>x.Alive&&x.hidden&&!S.Ally(x.team,f.team)&&x.cell.Distance(f.cell)==1).ToList())
            { h.hidden=false; after.Add(new GameEvent{kind="revelar",actor=f.id,target=h.id,cell=h.cell}); }
            events.AddRange(after);
            if(!f.Alive) KO(f);
            return true;
        }
        // Armadilha inimiga: dano fixo menos armadura e o movimento acaba ali.
        void Spring(Fighter f,Tile t,List<GameEvent> log)
        {
            int dmg=Ghost(f)?0:Math.Max(0,t.trapDamage-Armor(f));
            f.pm=0;
            // Mina de gelo: também tira PA no próximo turno.
            if(t.trapPa>0) f.buffs.Add(new Buff{id="gelo",turns=2,pa=-t.trapPa});
            log.Add(new GameEvent{kind="armadilha",actor=t.trap,target=f.id,cell=f.cell,value=dmg,info=t.trapPa>0?"gelo":null});
            t.trap=0; t.trapDamage=0; t.trapPa=0;
            Hurt(f,dmg,log);
        }
        // Lava (evento): 2 de dano de fogo, menos armadura.
        void Burn(Fighter f,List<GameEvent> log)
        {
            int dmg=Ghost(f)?0:Math.Max(0,2-Armor(f));
            log.Add(new GameEvent{kind="lava",target=f.id,cell=f.cell,value=dmg});
            Hurt(f,dmg,log);
        }
        // Escondido anda no máximo HiddenPm casas por turno (nerf do autor, 13/10).
        public const int HiddenPm=2;
        // Forma fantasma (Edgar): golpes de inimigos não o acertam.
        static bool Ghost(Fighter v) => Content.Form(v)?.intangible??false;
        int Armor(Fighter f) => f.armor+Content.Def(f).armor+f.buffs.Sum(b=>b.armor)+(Content.Form(f)?.armor??0)+Passive(f,PassiveStat.Armadura);
        // Passivas do editor (FighterDef.passives): soma do que os donos vivos dão a v.
        public int Passive(Fighter v,PassiveStat stat)
        {
            int sum=0;
            foreach(var o in S.fighters) { if(!o.Alive) continue; foreach(var p in Content.Def(o).passives??new PassiveDef[0]) if(p!=null&&p.stat==stat&&PassiveHits(o,p,v)) sum+=p.amount; }
            return sum;
        }
        bool PassiveHits(Fighter o,PassiveDef p,Fighter v)
        {
            bool self=o==v, ally=S.Ally(o.team,v.team);
            bool who=p.who switch { PassiveWho.Si=>self, PassiveWho.Aliados=>!self&&ally, PassiveWho.AliadosESi=>ally, PassiveWho.Inimigos=>!ally, _=>!self };
            return who&&(self||p.range<=0||o.cell.Distance(v.cell)<=p.range);
        }
        // Dano já calculado chega ao lutador: guarda do Tchu tcha (aliado até 2 casas absorve 1), revela quem estava escondido.
        // O nocaute (e o renascer) fica para quem chamou, depois do evento do golpe.
        void Hurt(Fighter v,int dmg,List<GameEvent> log)
        {
            // Forma fantasma: nada fere (golpes, área, aura, armadilha, lava, batida, eventos).
            if(dmg<=0||Ghost(v)) return;
            var guard=S.fighters.FirstOrDefault(g=>g.Alive&&g.id!=v.id&&S.Ally(g.team,v.team)&&Content.Def(g).guard&&!(Content.Form(g)?.noGuard??false)&&g.cell.Distance(v.cell)<=2);
            if(guard!=null) { dmg--; guard.hp=Math.Max(0,guard.hp-1); log.Add(new GameEvent{kind="guarda",actor=guard.id,target=v.id,value=1,cell=guard.cell}); }
            v.hp=Math.Max(0,v.hp-dmg);
            if(v.hidden) { v.hidden=false; log.Add(new GameEvent{kind="revelar",target=v.id,cell=v.cell}); }
            if(guard!=null&&!guard.Alive) KO(guard,log);
        }

        // Compras à vontade enquanto houver moedas (autor, 11/10), numa casa ao lado da loja; a loja renova no fim do turno de quem comprou; Item e Buff valem na hora, o resto vira carga de skill.
        bool Buy(Fighter f,int slot,out string reason)
        {
            reason="";
            var shop=S.shops[f.team].slots;
            if(f.summoner!=0) { reason="Invocações não compram."; return false; }
            if(!Board.NextToShop(S,f.cell)) { reason="Fique numa casa ao lado da loja para comprar."; return false; }
            if(slot<0||slot>=shop.Length||string.IsNullOrEmpty(shop[slot])) { reason="Espaço vazio."; return false; }
            var d=Shop.Def(shop[slot]);
            if(S.coins[f.team]<d.price) { reason="Moedas insuficientes."; return false; }
            S.coins[f.team]-=d.price; shop[slot]=null; f.bought=true;
            Grant(f,d);
            Emit(new GameEvent{kind="compra",actor=f.id,target=slot,value=d.price,info=d.id});
            return true;
        }
        // Produto da loja chega ao lutador (compra, Desejo E, baú do chefe).
        void Grant(Fighter f,ShopDef d)
        {
            switch(d.kind)
            {
                case ShopKind.Item:
                    f.bonusPa+=d.pa; f.bonusPm+=d.pm; f.bonusHp+=d.hp; f.armor+=d.armor; f.bonusRange+=d.range; f.pa+=d.pa; f.pm+=d.pm; f.hp+=d.hp;
                    f.anchored|=d.anchor; f.climber|=d.climb; f.focused|=d.focus; f.thorns+=d.thorns;
                    break;
                case ShopKind.Buff: f.buffs.Add(new Buff{id=d.id,turns=d.turns,pa=d.pa,pm=d.pm,armor=d.armor,range=d.range}); f.pa+=d.pa; f.pm+=d.pm; break;
                default:
                    var c=f.charges.Find(x=>x.skill==d.skill.id);
                    if(c==null) f.charges.Add(new Charge{skill=d.skill.id,uses=d.uses}); else c.uses+=d.uses;
                    break;
            }
        }
        // Sair de casa vizinha a inimigos: cada inimigo rola 1 dado; cada Trava precisa de uma Esquiva. Falhou: perde PA e PM.
        bool Tackle(Fighter f,List<GameEvent> log)
        {
            var enemies=S.fighters.Where(e=>e.Alive&&!S.Ally(e.team,f.team)&&e.cell.Distance(f.cell)==1&&!Content.Def(e).noLock&&e.prisoned==0).ToList();
            if(enemies.Count==0) return true;
            var locks=enemies.Select(_=>Roll()).ToArray(); int need=locks.Count(Dice.Lock);
            var dodges=Enumerable.Range(0,need).Select(_=>Roll()).ToArray();
            bool free=dodges.Count(Dice.Dodge)>=need;
            log.Add(new GameEvent{kind="trava",actor=f.id,cell=f.cell,value=free?0:1,dice=locks.Concat(dodges).ToArray()});
            if(!free) { f.pa=0; f.pm=0; }
            return free;
        }

        // Nevoeiro (evento): alcance máximo −2, sem baixar de 1 nem do mínimo.
        public int Range(Fighter f,SkillDef sk,Cell target)
        {
            int r=sk.maxRange+(sk.modifiable?(S.At(f.cell).height>S.At(target).height?1:0)+f.bonusRange+f.buffs.Sum(b=>b.range):0);
            if(S.ev=="nevoeiro"&&sk.maxRange>1) r=Math.Max(Math.Max(1,sk.minRange),r-2);
            if(sk.maxRange>1&&!sk.selfArea) r=Math.Max(Math.Max(1,sk.minRange),r+Passive(f,PassiveStat.Alcance));
            return r;
        }
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
            var list=Content.Def(f).skills.Where(s=>(string.IsNullOrEmpty(s.form)||s.form==f.form)&&!(s.kind==SkillKind.Forma&&s.toForm==f.form)&&!(s.kind==SkillKind.Desejo&&(f.channel>0||f.wishReady))).ToList();
            foreach(var id in f.stolen) { var sk=Content.StolenSkill(id); if(sk!=null) list.Add(sk); }
            foreach(var id in f.learned) { var sk=Content.AnySkill(id); if(sk!=null&&!list.Contains(sk)) list.Add(sk); }
            if(f.wishReady) list.AddRange(Content.Wishes);
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
                // PV mostrado = PV da original menos o dano que a ilusão já levou.
                int lost=Content.Fighters["ilusao"].hp-f.hp;
                f.def=f.disguise; f.summoner=0; if(real!=null) { f.hp=real.Alive?Math.Max(1,real.hp-lost):f.hp; f.form=real.form; }
            }
            return v;
        }
        public static List<GameEvent> EventsFor(List<GameEvent> events,MatchState s,int player) =>
            events.Where(e=>e.kind!="armadilha-posta"||(s.F(e.actor) is Fighter a&&s.Ally(a.team,player))).ToList();

        public bool CanTarget(Fighter f,SkillDef sk,Cell c,out string reason) => Check(f,sk,c,false,out reason);
        // free: sem custo nem limites (o gato dispara pela Faísca da varinha, que já pagou).
        bool Check(Fighter f,SkillDef sk,Cell c,bool free,out string reason)
        {
            reason="";
            if(sk==null) { reason="Skill desconhecida."; return false; }
            if(!Board.Contains(S,c)) { reason="Fora do mapa."; return false; }
            if(!free)
            {
                if(f.channel>0) { reason="Canalizando o Desejo."; return false; }
                if(f.pa<Cost(f,sk)) { reason="PA insuficientes."; return false; }
                if(sk.perTurn>0&&f.used.Count(u=>u==sk.id)>=sk.perTurn+f.buffs.Sum(b=>b.extra)) { reason="Limite por turno atingido."; return false; }
                if(sk.perMatch>0&&f.usedMatch.Count(u=>u==sk.id)>=sk.perMatch) { reason="Já usada nesta partida."; return false; }
                if(f.cooldowns.Find(x=>x.skill==sk.id) is Charge wait) { reason=$"Recarga: falta{(wait.uses>1?"m":"")} {wait.uses} turno{(wait.uses>1?"s":"")}."; return false; }
                if(sk.minRound>S.round) { reason=$"Disponível a partir da rodada {sk.minRound}."; return false; }
            }
            if(sk.kind!=SkillKind.Comando&&!InRange(f,sk,c,out reason)) return false;
            var v=Board.At(S,c);
            // Escondido: para o inimigo a casa parece vazia, menos colado nele (golpe de frente acerta; nerf do autor, 13/10).
            if(v!=null&&v.hidden&&!S.Ally(v.team,f.team)&&f.cell.Distance(c)>1) v=null;
            // Preso na estrutura da Prisão dos pesadelos: ninguém o alcança.
            if(v!=null&&v!=f&&v.prisoned>0&&!sk.selfArea) { reason="Preso na estrutura: não pode ser alvo."; return false; }
            switch(sk.kind)
            {
                case SkillKind.Dano:
                    if(sk.selfArea) { if(c!=f.cell) { reason="Use em si mesmo."; return false; } break; }
                    if(v==null||S.Ally(v.team,f.team)) { reason="Alvo inválido."; return false; }
                    if(Ghost(v)) { reason="Forma fantasma: golpes não o acertam."; return false; }
                    break;
                case SkillKind.Cura: if(v==null||!S.Ally(v.team,f.team)) { reason="Alvo inválido."; return false; } break;
                case SkillKind.Suporte:
                    if(sk.selfArea) { if(c!=f.cell) { reason="Use em si mesmo."; return false; } break; }
                    if(v==null||!S.Ally(v.team,f.team)) { reason="Escolha um aliado."; return false; }
                    break;
                case SkillKind.Zona: break;
                case SkillKind.Comando: if(!FindUnit(f,sk,c,out _,out reason)) return false; break;
                case SkillKind.Reviver:
                    if(v!=null||!Board.Walkable(S,c,false)) { reason="Casa ocupada ou bloqueada."; return false; }
                    if(DeadAlly(f)==null) { reason="Nenhum aliado nocauteado."; return false; }
                    break;
                case SkillKind.Desejo: if(!WishTarget(f,sk,c,v,out reason)) return false; break;
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
                case SkillKind.Aura: if(c!=f.cell) { reason="Use em si mesmo."; return false; } break;
                case SkillKind.Esconder:
                    if(c!=f.cell) { reason="Use em si mesmo."; return false; }
                    if(f.hidden) { reason="Já está escondido."; return false; }
                    if(!sk.noCover&&!Cover(f.cell)) { reason="Precisa estar junto a relevo mais alto, árvore ou caixa."; return false; }
                    break;
                case SkillKind.Espelho:
                    if(v==null||S.Ally(v.team,f.team)) { reason="Escolha um inimigo."; return false; }
                    if(string.IsNullOrEmpty(v.lastAttack)||Content.AnySkill(v.lastAttack)==null) { reason="Esse inimigo ainda não atacou."; return false; }
                    break;
                default:
                {
                    // Máximo por grupo (padrão 1 por tipo): o lobo da loja convive com o dragão da Ciantia; os gatos são 2.
                    var kinds=sk.summon.Split('|'); var d=Content.Fighters[kinds[0]];
                    if(S.fighters.Count(x=>x.Alive&&x.summoner==f.id&&kinds.Contains(x.def))>=Math.Max(1,sk.maxActive)) { reason="Limite de invocações ativas."; return false; }
                    if(v!=null||!Board.Walkable(S,c,d.flying)) { reason="Casa ocupada ou bloqueada."; return false; }
                    break;
                }
            }
            return true;
        }
        static bool InArea(Cell center,Cell c,int r,bool square) => square?Math.Max(Math.Abs(c.x-center.x),Math.Abs(c.z-center.z))<=r:center.Distance(c)<=r;
        IEnumerable<Cell> Area(Cell center,int r,bool square)
        {
            for(int x=-r;x<=r;x++) for(int z=-r;z<=r;z++) { var c=center+new Cell(x,z); if(Board.Contains(S,c)&&InArea(center,c,r,square)) yield return c; }
        }
        // Aliado nocauteado que volta (Pena de fênix, Desejo A): o de maior Nível.
        Fighter DeadAlly(Fighter f) => S.fighters.Where(x=>!x.Alive&&x.summoner==0&&S.Ally(x.team,f.team)&&MatchState.Player(x.team)&&!S.eliminated[S.Team(x.team)])
            .OrderByDescending(x=>Content.Def(x).level).ThenBy(x=>x.id).FirstOrDefault();
        // Invocação sem turno que obedece (gato): para andar, a que chega mais perto; para disparar, a que alcança o alvo.
        bool FindUnit(Fighter f,SkillDef sk,Cell c,out Fighter unit,out string reason)
        {
            reason=""; unit=null;
            var units=S.fighters.Where(x=>x.Alive&&x.summoner==f.id&&Content.Def(x).noTurn&&Content.Def(x).skills.Length>0).OrderBy(x=>x.cell.Distance(c)).ThenBy(x=>x.id).ToList();
            if(units.Count==0) { reason="Nenhum gato em campo."; return false; }
            foreach(var u in units)
            {
                if(sk.step>0) { if(Board.At(S,c)==null&&Board.Path(S,u,c,sk.step)!=null) { unit=u; return true; } continue; }
                if(Check(u,Content.Def(u).skills[0],c,true,out _)) { unit=u; return true; }
            }
            reason=sk.step>0?$"Nenhum gato chega lá ({sk.step} casas).":"Nenhum gato alcança esse alvo.";
            return false;
        }
        bool WishTarget(Fighter f,SkillDef sk,Cell c,Fighter v,out string reason)
        {
            reason="";
            switch(sk.wish)
            {
                case null: case "": if(c!=f.cell) { reason="Use em si mesmo."; return false; } return true;
                case "a":
                    if(v!=null||!Board.Walkable(S,c,false)) { reason="Escolha uma casa livre vizinha."; return false; }
                    if(DeadAlly(f)==null) { reason="Nenhum aliado nocauteado."; return false; }
                    return true;
                case "b": if(v==null) { reason="Escolha um lutador."; return false; } return true;
                case "c": if(v==null||!S.Ally(v.team,f.team)) { reason="Escolha um aliado."; return false; } return true;
                case "d": if(v==null||Copyables(f,v).Count==0) { reason="Não há skill para copiar desse lutador."; return false; } return true;
                case "e":
                    if(v==null||!S.Ally(v.team,f.team)) { reason="Escolha um aliado."; return false; }
                    if(!S.shops[f.team].slots.Any(x=>!string.IsNullOrEmpty(x))) { reason="A loja está vazia."; return false; }
                    return true;
                case "f": if(string.IsNullOrEmpty(S.ev)) { reason="Nenhum evento ativo."; return false; } return true;
            }
            reason="Opção desconhecida."; return false;
        }
        // Desejo D: quem usa escolhe o lutador e, na lista dele, a skill (Command.slot). Fora: ultimate, forma, Desejo e as que o Kellen já tem.
        // Ordem da ficha, igual no Host e nos clientes.
        public static List<SkillDef> Copyables(Fighter f,Fighter v) => Content.Def(v).skills.Concat(v.learned.Select(Content.AnySkill))
            .Where(s=>s!=null&&!s.ultimate&&s.kind!=SkillKind.Forma&&s.kind!=SkillKind.Desejo&&s.kind!=SkillKind.Aura&&string.IsNullOrEmpty(s.form)&&Content.Skill(f,s.id)==null)
            .Distinct().ToList();
        // Aura ligada (ou já paga neste turno): desligar e religar não custam PA.
        public static int Cost(Fighter f,SkillDef sk) => sk.kind==SkillKind.Aura&&(f.auraOn||f.auraPaid)?0:sk.pa;
        bool UseSkill(Fighter f,string id,Cell c,int slot,out string reason)
        {
            var sk=Content.Skill(f,id);
            if(!CanTarget(f,sk,c,out reason)) return false;
            if(sk.wish=="d"&&(slot<0||slot>=Copyables(f,Board.At(S,c)).Count)) { reason="Escolha a skill que vai copiar."; return false; }
            f.pa-=Cost(f,sk); f.used.Add(sk.id); if(sk.perMatch>0) f.usedMatch.Add(sk.id);
            if(sk.cooldown>0) f.cooldowns.Add(new Charge{skill=sk.id,uses=sk.cooldown});
            if(sk.kind!=SkillKind.Espelho&&sk.kind!=SkillKind.Desejo) f.lastSkill=sk.id;
            if(sk.kind==SkillKind.Dano) f.lastAttack=sk.id;
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
                    S.At(c).trap=f.team; S.At(c).trapDamage=sk.damage; S.At(c).trapPa=sk.paLoss;
                    Emit(new GameEvent{kind="armadilha-posta",actor=f.id,info=sk.id,cell=c});
                    break;
                case SkillKind.Dano: Strike(f,sk,Intercept(f,c)); break;
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
                    f.hidden=true; f.pm=Math.Min(f.pm,HiddenPm); Emit(new GameEvent{kind="esconder",actor=f.id,info=sk.id,cell=f.cell});
                    break;
                case SkillKind.Espelho:
                {
                    // Reflete no alvo o último ataque dele, na hora, com o dano da skill copiada (magia: nada intercepta).
                    var v=Board.At(S,c); var copied=Content.AnySkill(v.lastAttack);
                    Emit(new GameEvent{kind="espelho",actor=f.id,target=v.id,info=copied.id,cell=c});
                    Strike(f,copied,c);
                    break;
                }
                case SkillKind.Suporte: Support(f,sk,c); break;
                case SkillKind.Zona:
                {
                    var cells=Area(c,sk.area,sk.square).ToArray();
                    if(sk.freeze) foreach(var x in cells) S.At(x).frozenBy=f.id;
                    Emit(new GameEvent{kind="zona",actor=f.id,info=sk.id,cell=c,path=cells});
                    break;
                }
                case SkillKind.Comando: Order(f,sk,c); break;
                case SkillKind.Reviver: Revive(f,DeadAlly(f),c,sk.heal,sk.id); break;
                case SkillKind.Desejo: Wish(f,sk,c,slot); break;
                case SkillKind.Aura:
                    if(f.auraOn) { f.auraOn=false; Emit(new GameEvent{kind="aura",actor=f.id,info="desligar",cell=f.cell}); break; }
                    f.auraOn=true;
                    // Ligar paga a manutenção deste turno e aplica na hora; religada no mesmo turno, só volta a girar.
                    if(f.auraPaid) Emit(new GameEvent{kind="aura",actor=f.id,info="ativar",cell=f.cell,value=sk.area});
                    else { f.auraPaid=true; AuraPulse(f,sk,"ativar"); }
                    break;
                default: Summon(f,sk.summon,c); break;
            }
            if(sk.endsForm) SetForm(f,Content.Def(f).startForm,0,sk.id);
            return true;
        }
        Fighter Summon(Fighter f,string summon,Cell c)
        {
            var kinds=summon.Split('|');
            var s=Spawn(kinds[kinds.Length==1?0:Dice.Range(S,kinds.Length)],f.team,c,f.id);
            // Invocação sem turno (gato, totem) não entra na linha do tempo.
            if(!Content.Def(s).noTurn) S.timeline.Insert(S.active+1,s.id);
            Emit(new GameEvent{kind="invocar",actor=f.id,target=s.id,info=s.def,cell=c});
            return s;
        }
        void Heal(Fighter v,int amount,Fighter by,string info)
        {
            int heal=Math.Min(amount,Content.MaxHp(v)-v.hp); if(heal<=0) return;
            v.hp+=heal; Emit(new GameEvent{kind="cura",actor=by.id,target=v.id,value=heal,info=info,cell=v.cell});
        }
        // Aura (Dualidade guardiã): aliados até area casas curam heal (o dono não); inimigos, inclusive neutros, levam damage fixo
        // (sem dado nem armadura). Uma aplicação por alvo por turno do dono: ligar e manter cobram e aplicam só uma vez (auraPaid).
        void AuraPulse(Fighter f,SkillDef sk,string info)
        {
            Emit(new GameEvent{kind="aura",actor=f.id,info=info,cell=f.cell,value=sk.area});
            var inside=S.fighters.Where(x=>x.Alive&&x!=f&&x.prisoned==0&&x.cell.Distance(f.cell)<=sk.area).ToList();
            foreach(var a in inside.Where(x=>S.Ally(x.team,f.team))) if(sk.heal>0) Heal(a,sk.heal,f,sk.id);
            var log=new List<GameEvent>(); var hit=inside.Where(x=>!S.Ally(x.team,f.team)).ToList();
            foreach(var v in hit) if(sk.damage>0) { log.Add(new GameEvent{kind="dano-evento",actor=f.id,target=v.id,value=Ghost(v)?0:sk.damage,info=sk.id,cell=v.cell}); Hurt(v,sk.damage,log); }
            events.AddRange(log); striker=f;
            foreach(var v in hit) if(!v.Alive) KO(v);
            striker=null;
        }
        // Suporte: aliados no alvo/área (ou em volta de si) curam e ganham o buff; pode invocar (Noite dos Gatos).
        void Support(Fighter f,SkillDef sk,Cell c)
        {
            var center=sk.selfArea?f.cell:c; var list=new List<Fighter>();
            if(!sk.selfArea&&Board.At(S,c) is Fighter main) list.Add(main);
            if(sk.selfArea||sk.area>0) list.AddRange(S.fighters.Where(x=>x.Alive&&!list.Contains(x)&&S.Ally(x.team,f.team)&&x.prisoned==0&&InArea(center,x.cell,sk.area,sk.square)));
            Emit(new GameEvent{kind="suporte",actor=f.id,info=sk.id,cell=center,value=sk.area});
            foreach(var v in list)
            {
                if(sk.heal>0) Heal(v,sk.heal,f,sk.id);
                if(sk.buffPa!=0||sk.buffPm!=0||sk.buffArmor!=0||sk.buffRange!=0)
                {
                    v.buffs.Add(new Buff{id=sk.id,turns=Math.Max(1,sk.turns),pa=sk.buffPa,pm=sk.buffPm,armor=sk.buffArmor,range=sk.buffRange});
                    Emit(new GameEvent{kind="buff",actor=f.id,target=v.id,info=sk.id,value=sk.buffArmor,cell=v.cell});
                }
            }
            if(string.IsNullOrEmpty(sk.summon)||sk.summonCount<=0) return;
            var kinds=sk.summon.Split('|'); var flying=Content.Fighters[kinds[0]].flying;
            var free=Area(f.cell,2,false).Where(x=>x!=f.cell&&Board.At(S,x)==null&&Board.Walkable(S,x,flying)).OrderBy(x=>x.Distance(f.cell)).ThenBy(x=>x.z).ThenBy(x=>x.x).ToList();
            for(int i=0;i<sk.summonCount&&free.Count>0;i++)
            {
                if(S.fighters.Count(x=>x.Alive&&x.summoner==f.id&&kinds.Contains(x.def))>=Math.Max(1,sk.maxActive)) break;
                Summon(f,sk.summon,free[0]); free.RemoveAt(0);
            }
        }
        // Comando a um gato: andar (step) ou disparar a skill dele de onde está; depois do disparo ele some.
        void Order(Fighter f,SkillDef sk,Cell c)
        {
            FindUnit(f,sk,c,out var u,out _);
            if(sk.step>0)
            {
                var path=Board.Path(S,u,c,sk.step); u.cell=c;
                Emit(new GameEvent{kind="mover",actor=u.id,info=sk.id,path=path.ToArray()});
                Collect(u);
                return;
            }
            var zap=Content.Def(u).skills[0];
            Emit(new GameEvent{kind="comando",actor=f.id,target=u.id,info=zap.id,cell=c});
            if(zap.kind==SkillKind.Dano) { f.lastAttack=zap.id; Strike(u,zap,Intercept(u,c)); }
            else if(zap.kind==SkillKind.Cura&&Board.At(S,c) is Fighter v) Heal(v,zap.damage,u,zap.id);
            if(u.Alive) { u.hp=0; Emit(new GameEvent{kind="sumir",target=u.id,cell=u.cell}); }
        }
        void Revive(Fighter by,Fighter v,Cell c,int hp,string info)
        {
            if(v==null) return;
            v.hp=Math.Max(1,Math.Min(hp,Content.MaxHp(v))); v.cell=c; v.prisoned=0; v.channel=0; v.wishReady=false; v.hidden=false;
            Emit(new GameEvent{kind="reviver",actor=by.id,target=v.id,value=v.hp,info=info,cell=c});
            Collect(v);
        }
        // Desejo: o 1º uso começa a canalizar (2 turnos); no turno seguinte ao fim, uma das opções a–f.
        void Wish(Fighter f,SkillDef sk,Cell c,int slot)
        {
            if(string.IsNullOrEmpty(sk.wish)) { f.channel=2; f.pa=0; f.pm=0; Emit(new GameEvent{kind="desejo",actor=f.id,info="",cell=f.cell}); return; }
            f.wishReady=false; var v=Board.At(S,c);
            Emit(new GameEvent{kind="desejo",actor=f.id,target=v?.id??0,info=sk.wish,cell=c});
            switch(sk.wish)
            {
                case "a": var dead=DeadAlly(f); Revive(f,dead,c,(Content.MaxHp(dead)+1)/2,sk.id); break;
                case "b": Heal(v,Content.MaxHp(v),f,sk.id); break;
                case "c":
                    v.buffs.Add(new Buff{id="desejo",turns=2,pa=Content.Def(v).pa+v.bonusPa,extra=1});
                    Emit(new GameEvent{kind="buff",actor=f.id,target=v.id,info=sk.id,cell=v.cell});
                    break;
                case "d":
                {
                    var copy=Copyables(f,v)[slot]; f.learned.Add(copy.id);
                    Emit(new GameEvent{kind="aprender",actor=f.id,target=v.id,info=copy.id,cell=f.cell});
                    break;
                }
                case "e":
                {
                    var shop=S.shops[f.team].slots;
                    var picks=Enumerable.Range(0,shop.Length).Where(i=>!string.IsNullOrEmpty(shop[i])).OrderByDescending(i=>Shop.Def(shop[i]).price).Take(2).ToList();
                    foreach(int i in picks) { Grant(v,Shop.Def(shop[i])); Emit(new GameEvent{kind="compra",actor=v.id,target=i,value=0,info=shop[i]}); shop[i]=null; }
                    Shop.Refill(S,f.team); Emit(new GameEvent{kind="loja",actor=f.team,info=string.Join(",",shop)});
                    break;
                }
                case "f": EndEvent(); break;
            }
        }
        // Tiro à distância que passa por cima de alguém (aliado ou inimigo): 1 chance em 3 de acertar esse primeiro lutador no caminho.
        Cell Intercept(Fighter f,Cell c)
        {
            if(f.cell.Distance(c)<=1) return c;
            foreach(var cell in Board.Between(S,f.cell,c))
            {
                var b=Board.At(S,cell); if(b==null||!b.Alive||b==f) continue;
                if(Dice.Range(S,3)!=0) return c;
                Emit(new GameEvent{kind="desvio",actor=f.id,target=b.id,cell=cell}); return cell;
            }
            return c;
        }
        // Ataque: dado de ataque e de defesa, bônus de aura, área, erro do golpe impreciso, roubo, dreno, controle (PM, PA,
        // prisão, empurrão, puxão), espinhos e a devoração do Colosso.
        Fighter striker;
        void Strike(Fighter f,SkillDef sk,Cell c)
        {
            Face a=Roll(), d=Roll();
            var main=sk.selfArea?null:Board.At(S,c);
            bool missed=sk.wild&&(a==Face.Esquiva||a==Face.CoringaEsquivaTrava);
            bool ambush=f.hidden;
            int bonus=S.fighters.Where(g=>g.Alive&&g.id!=f.id&&S.Ally(g.team,f.team)&&Content.Def(g).allyBonus>0&&g.cell.Distance(f.cell)<=Content.Def(g).auraRange).Select(g=>Content.Def(g).allyBonus).DefaultIfEmpty(0).Max();
            bonus+=Passive(f,PassiveStat.Dano);
            if(ambush) { bonus++; f.hidden=false; }
            if(f.focused&&!f.focusUsed&&sk.damage>0) { bonus++; f.focusUsed=true; }
            var targets=new List<Fighter>(); if(main!=null) targets.Add(main);
            var center=sk.selfArea?f.cell:c;
            // Área acerta aliados também (decisão do autor, 10/10), salvo foes; quem ataca nunca se acerta; preso não é alcançado.
            if(sk.area>0) targets.AddRange(S.fighters.Where(x=>x.Alive&&x!=main&&x!=f&&x.prisoned==0&&InArea(center,x.cell,sk.area,sk.square)&&(!sk.foes||!S.Ally(x.team,f.team))));
            var first=targets.FirstOrDefault();
            var log=new List<GameEvent>(); int dealt=0; striker=f;
            // Golpe em volta de si: a apresentação anima a conjuração mesmo sem ninguém na área.
            if(sk.selfArea) Emit(new GameEvent{kind="area",actor=f.id,info=sk.id,cell=f.cell,value=sk.area,dice=new[]{a,d}});
            foreach(var v in targets)
            {
                bool ghost=Ghost(v);
                bool shield=Dice.Armor(d)&&!sk.precise&&!sk.pierce;
                int alone=sk.isolatedBonus>0&&!S.fighters.Any(x=>x.Alive&&x!=v&&S.Ally(x.team,v.team)&&x.cell.Distance(v.cell)==1)?sk.isolatedBonus:0;
                int dmg=missed||ghost||sk.damage<=0?0:Math.Max(0,sk.damage+bonus+alone-(shield?1:0)-(sk.pierce?0:Armor(v)));
                Emit(new GameEvent{kind="ataque",actor=f.id,target=v.id,value=missed||ghost?-1:dmg,info=sk.id,cell=v.cell,dice=v==first&&!sk.selfArea?new[]{a,d}:null});
                Hurt(v,dmg,log); dealt+=dmg;
                bool hit=!missed&&!ghost&&(dmg>0||sk.damage<=0);
                if(hit&&sk.pmLoss>0) v.buffs.Add(new Buff{id="confusao",from=sk.id,turns=2,pm=-sk.pmLoss});
                if(hit&&sk.paLoss>0) v.buffs.Add(new Buff{id="atordoado",from=sk.id,turns=2,pa=-sk.paLoss});
                if(dmg>0&&v==main&&!S.Ally(v.team,f.team)&&Content.Def(f).steal>0&&MatchState.Player(v.team)&&MatchState.Player(f.team)&&S.coins[v.team]>0&&Dice.Range(S,100)<Content.Def(f).steal*(ambush?2:1))
                {
                    S.coins[v.team]--; S.coins[f.team]++;
                    log.Add(new GameEvent{kind="roubo",actor=f.id,target=v.id,value=1,cell=v.cell});
                }
                // Manto de espinhos: quem acerta colado leva o troco.
                if(dmg>0&&v.thorns>0&&f.Alive&&f.cell.Distance(v.cell)==1) { log.Add(new GameEvent{kind="espinhos",actor=v.id,target=f.id,value=v.thorns,cell=f.cell}); Hurt(f,v.thorns,log); }
                // Prisão: o alvo escapa com Esquiva (ou coringa) no dado de defesa.
                if(hit&&v==main&&sk.prison>0)
                {
                    if(Dice.Dodge(d)) log.Add(new GameEvent{kind="escapou",target=v.id,cell=v.cell,info=sk.id});
                    else { v.prisoned=sk.prison; log.Add(new GameEvent{kind="prisao",actor=f.id,target=v.id,value=sk.prison,cell=v.cell,info=sk.id}); }
                }
            }
            if(ambush) events.Insert(events.Count-targets.Count,new GameEvent{kind="revelar",target=f.id,cell=f.cell});
            if(sk.drain>0&&dealt>0&&f.Alive) { int heal=Math.Min(sk.drain,Content.MaxHp(f)-f.hp); if(heal>0) { f.hp+=heal; log.Add(new GameEvent{kind="cura",actor=f.id,target=f.id,value=heal,info=sk.id,cell=f.cell}); } }
            events.AddRange(log);
            foreach(var v in targets) if(!v.Alive) { KO(v); if(!v.Alive) Devour(f,v); }
            if(!f.Alive) KO(f);
            // Empurrão e puxão depois do dano, em quem continua de pé.
            if(!missed) foreach(var v in targets.Where(x=>x.Alive&&x.prisoned==0&&!(Ghost(x)&&!S.Ally(x.team,f.team))))
            {
                if(sk.push>0) { var dir=Dir(v==main||sk.selfArea?f.cell:c,v.cell); if(dir==default) dir=Dir(f.cell,v.cell); Push(v,dir,sk.push,f); }
                if(sk.pull&&v==main) Pull(v,f);
            }
            striker=null;
            // Decisão B: Cataclisma só sai no dado de ataque.
            if(a==Face.Cataclisma) Cataclysm();
        }
        // Colosso: devora a 1ª skill de dano (não ultimate) de quem nocauteia; guarda 2, a 3ª tira a mais antiga.
        void Devour(Fighter f,Fighter v)
        {
            if(!Content.Def(f).devour||v.summoner!=0||Content.Def(v).neutral||S.Ally(v.team,f.team)) return;
            var sk=Content.Def(v).skills.FirstOrDefault(s=>s.kind==SkillKind.Dano&&!s.ultimate&&s.damage>0);
            if(sk==null||f.stolen.Contains(sk.id)||Array.Exists(Content.Def(f).skills,s=>s.id==sk.id)) return;
            f.stolen.Add(sk.id); if(f.stolen.Count>2) f.stolen.RemoveAt(0);
            Emit(new GameEvent{kind="devorar",actor=f.id,target=v.id,info=Content.Stolen+sk.id,cell=f.cell});
        }
        // Direção dominante (eixo com maior diferença) de a para b.
        static Cell Dir(Cell a,Cell b)
        {
            int dx=b.x-a.x, dz=b.z-a.z; if(dx==0&&dz==0) return default;
            return Math.Abs(dx)>=Math.Abs(dz)?new Cell(Math.Sign(dx),0):new Cell(0,Math.Sign(dz));
        }
        bool Fixed(Fighter v) => Content.Def(v).immovable||v.anchored||v.prisoned>0;
        // Empurrão: anda n casas na direção; parede, relevo de 2+, obstáculo ou lutador param: +1 de dano nele e 1 no atingido.
        void Push(Fighter v,Cell dir,int n,Fighter by,bool pull=false)
        {
            if(n<=0||dir==default||!v.Alive) return;
            if(Fixed(v)) { Emit(new GameEvent{kind="firme",actor=by?.id??0,target=v.id,cell=v.cell}); return; }
            bool flying=Content.Def(v).flying, bump=false; var path=new List<Cell>{v.cell}; var log=new List<GameEvent>(); Fighter other=null;
            for(int i=0;i<n&&v.Alive;i++)
            {
                var next=v.cell+dir;
                if(!Board.Contains(S,next)||!Board.Walkable(S,next,flying)||(!flying&&S.At(next).height-S.At(v.cell).height>=2)) { bump=true; break; }
                other=Board.At(S,next); if(other!=null) { bump=true; break; }
                v.cell=next; path.Add(next);
                var t=S.At(next);
                if(t.coins>0&&MatchState.Player(v.team)) { S.coins[v.team]+=t.coins; S.collected[S.Team(v.team)]+=t.coins; log.Add(new GameEvent{kind="moeda",actor=v.id,cell=next,value=t.coins}); t.coins=0; }
                if(t.lava&&!flying) Burn(v,log);
                if(t.trap!=0&&!S.Ally(t.trap,v.team)&&!flying) { Spring(v,t,log); break; }
            }
            Emit(new GameEvent{kind=pull?"puxao":"empurrao",actor=by?.id??0,target=v.id,path=path.ToArray(),cell=v.cell,value=bump?1:0});
            if(bump&&!pull&&v.Alive)
            {
                log.Add(new GameEvent{kind="colisao",actor=other?.id??0,target=v.id,cell=v.cell,value=1});
                Hurt(v,1,log); if(other!=null) Hurt(other,1,log);
            }
            events.AddRange(log);
            if(!v.Alive) KO(v);
            if(other!=null&&!other.Alive) KO(other);
        }
        void Pull(Fighter v,Fighter by) => Push(v,Dir(v.cell,by.cell),v.cell.Distance(by.cell)-1,by,true);
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
        bool Cover(Cell c) => Board.Dirs.Select(d=>c+d).Any(x=>Board.Contains(S,x)&&(S.At(x).height>S.At(c).height||S.At(x).terrain==Terrain.Arvore||S.At(x).terrain==Terrain.Caixa||S.At(x).terrain==Terrain.Loja));
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
            v.hp=0; v.hidden=false; v.channel=0; v.wishReady=false; v.prisoned=0; add.Add(new GameEvent{kind="nocaute",target=v.id,cell=v.cell});
            if(v.auraOn) { v.auraOn=false; add.Add(new GameEvent{kind="aura",actor=v.id,info="desligar",cell=v.cell}); }
            foreach(var t in S.tiles) if(t.frozenBy==v.id) t.frozenBy=0;
            if(v.summoner==0&&MatchState.Player(v.team)) { int t=S.Team(v.team); S.galeoes[t]=Math.Max(0,S.galeoes[t]-Content.Def(v).level); }
            foreach(var s in S.fighters.Where(x=>x.Alive&&x.summoner==v.id).ToList()) KO(s,log);
            // Neutros: recompensa para quem nocauteou (ou o dono da invocação); o chefe derrotado encerra o evento.
            if(Content.Def(v).neutral&&striker!=null)
            {
                var who=striker.summoner!=0?S.F(striker.summoner)??striker:striker;
                if(MatchState.Player(who.team))
                {
                    bool boss=v.def=="brutamontes-errante"; int coins=boss?5:v.carried+2; string item=null;
                    S.coins[who.team]+=coins;
                    if(boss) { var items=Shop.Catalog.Where(d=>d.kind==ShopKind.Item).ToArray(); var it=items[Dice.Range(S,items.Length)]; Grant(who,it); item=it.id; }
                    add.Add(new GameEvent{kind="recompensa",actor=who.id,target=v.id,value=coins,info=item,cell=v.cell});
                }
            }
            if(v.def=="brutamontes-errante"&&S.ev=="errante") EndEvent();
        }

        void BeginTurn(Fighter f)
        {
            if(f==null) return;
            foreach(var b in f.buffs) b.turns--;
            f.buffs.RemoveAll(b=>b.turns<=0);
            // Forma com duração: conta os turnos do próprio lutador e volta à forma inicial.
            if(f.formTurns>0&&--f.formTurns==0) { f.form=Content.Def(f).startForm; Emit(new GameEvent{kind="forma",actor=f.id,info=f.form,cell=f.cell}); }
            var d=Content.Def(f); var form=Content.Form(f);
            foreach(var w in f.cooldowns) w.uses--;
            f.cooldowns.RemoveAll(w=>w.uses<=0);
            f.pa=Math.Max(0,d.pa+f.bonusPa+f.buffs.Sum(b=>b.pa)+(form?.pa??0)+Passive(f,PassiveStat.PA)); f.pm=Math.Max(0,d.pm+f.bonusPm+f.buffs.Sum(b=>b.pm)+(form?.pm??0)+Passive(f,PassiveStat.PM)); f.used.Clear(); f.bought=false; f.focusUsed=false;
            if(f.hidden) f.pm=Math.Min(f.pm,HiddenPm);
            // Chão de gelo dura até o próximo turno de quem congelou.
            foreach(var t in S.tiles) if(t.frozenBy==f.id) t.frozenBy=0;
            Emit(new GameEvent{kind="turno",actor=f.id,value=S.round});
            // Desejo: 2º turno só canaliza; no 3º a escolha fica disponível.
            if(f.channel>0)
            {
                if(--f.channel>0) { f.pa=0; f.pm=0; Emit(new GameEvent{kind="canalizar",actor=f.id,cell=f.cell}); }
                else { f.wishReady=true; Emit(new GameEvent{kind="desejo-pronto",actor=f.id,cell=f.cell}); }
            }
            // Aura ligada: paga a manutenção e aplica; sem PA, desliga.
            f.auraPaid=false;
            if(f.auraOn&&Content.Def(f).skills.FirstOrDefault(x=>x.kind==SkillKind.Aura) is SkillDef aura)
            {
                if(f.pa>=aura.pa) { f.pa-=aura.pa; f.auraPaid=true; AuraPulse(f,aura,"manter"); }
                else { f.auraOn=false; Emit(new GameEvent{kind="aura",actor=f.id,info="desligar",cell=f.cell}); }
            }
            // Totem: no turno do dono, cura aliados até 2 casas.
            foreach(var t in S.fighters.Where(x=>x.Alive&&x.summoner==f.id&&Content.Def(x).healAura>0).ToList())
                foreach(var a in S.fighters.Where(x=>x.Alive&&S.Ally(x.team,f.team)&&x.cell.Distance(t.cell)<=2).ToList()) Heal(a,Content.Def(t).healAura,t,"totem-de-cura");
            // Lava: quem começa o turno nela queima.
            if(S.At(f.cell).lava&&!d.flying) { var log=new List<GameEvent>(); Burn(f,log); events.AddRange(log); if(!f.Alive) KO(f); }
            // Passivas de cura/dano por turno (editor), no começo do turno de quem é pego.
            if(!f.Alive) return;
            int regen=Passive(f,PassiveStat.CuraPorTurno), bleed=Passive(f,PassiveStat.DanoPorTurno);
            if(regen>0) Heal(f,regen,f,"passiva");
            if(bleed>0) { var log=new List<GameEvent>{ new GameEvent{kind="dano-evento",actor=f.id,target=f.id,value=bleed,info="passiva",cell=f.cell} }; Hurt(f,bleed,log); events.AddRange(log); if(!f.Alive) KO(f); }
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
                var next=S.Active;
                if(!next.Alive) continue;
                // Preso: perde este turno inteiro e sai da estrutura.
                if(next.prisoned>0) { next.prisoned--; Emit(new GameEvent{kind="preso",target=next.id,value=next.prisoned,cell=next.cell}); continue; }
                BeginTurn(next); if(next.Alive) return;
            }
        }
        void EndRound()
        {
            if(S.mode==VictoryMode.Dominio)
            {
                var present=S.fighters.Where(f=>f.Alive&&f.summoner==0&&S.At(f.cell).zone).Select(f=>S.Team(f.team)).Distinct().ToList();
                if(present.Count==1) { S.domain[present[0]]++; Emit(new GameEvent{kind="dominio",actor=present[0],value=S.domain[present[0]]}); }
            }
            Neutrals(); TickEvent();
            S.round++; MapGen.SpawnCoins(S,Math.Max(1,S.players/2));
            Emit(new GameEvent{kind="rodada",value=S.round});
            CheckVictory();
        }

        // Eventos espelhados para não favorecer um time. Mudanças que isolam alguém são desfeitas.
        // (id, peso, rodadas): rodadas 0 = instantâneo; só 1 evento com duração por vez (EVENTOS-LOJA-ELENCO.md).
        public static readonly (string id,int weight,int rounds)[] Cataclysms=
        {
            ("chuva-de-moedas",3,0), ("queda-de-pedras",2,0), ("terremoto",2,0), ("mare-de-lava",2,3), ("portal-caotico",2,0), ("eco-ancestral",1,0),
            ("mare-de-energia",2,0), ("festival-de-fogos",2,1), ("nevoeiro",2,1), ("vendaval",2,0), ("papa-moedas",2,0), ("errante",1,5),
        };
        // Só para testes e para o editor: dispara um evento escolhido.
        public List<GameEvent> Trigger(string id) { events=new List<GameEvent>(); Cataclysm(id); S.revision++; return events; }
        void Cataclysm(string forced=null)
        {
            var pool=Cataclysms.Where(x=>string.IsNullOrEmpty(S.ev)||x.rounds==0).ToArray();
            string kind=forced;
            if(kind==null) { int roll=Dice.Range(S,pool.Sum(x=>x.weight)); foreach(var x in pool) { if(roll<x.weight) { kind=x.id; break; } roll-=x.weight; } }
            int rounds=Cataclysms.First(x=>x.id==kind).rounds;
            if(rounds>0&&!string.IsNullOrEmpty(S.ev)) EndEvent();
            var cells=new List<Cell>();
            var ev=new GameEvent{kind="cataclisma",info=kind,value=rounds}; Emit(ev);
            switch(kind)
            {
                case "chuva-de-moedas": MapGen.SpawnCoins(S,3); break;
                case "queda-de-pedras": case "terremoto": Reshape(kind,cells); break;
                case "mare-de-lava":
                    for(int tries=0;tries<80&&cells.Count==0;tries++)
                    {
                        var c=new Cell(Dice.Range(S,S.width),Dice.Range(S,S.height)); var m=S.Mirror(c);
                        if(c==m||!LavaOk(c,true)||!LavaOk(m,true)) continue;
                        foreach(var x in new[]{c,m}) { S.At(x).lava=true; S.At(x).coins=0; cells.Add(x); }
                    }
                    if(cells.Count==0) rounds=0;
                    break;
                case "portal-caotico":
                {
                    var pool2=S.fighters.Where(x=>x.Alive&&x.summoner==0&&!Content.Def(x).neutral&&!Fixed(x)).ToList();
                    if(pool2.Count<2) break;
                    int n=Math.Min(pool2.Count,2+Dice.Range(S,3)); var picked=new List<Fighter>();
                    for(int i=0;i<n;i++) { int k=Dice.Range(S,pool2.Count); picked.Add(pool2[k]); pool2.RemoveAt(k); }
                    var spots=picked.Select(x=>x.cell).ToList();
                    for(int i=0;i<n;i++) picked[i].cell=spots[(i+1)%n];
                    cells.AddRange(spots);
                    break;
                }
                case "eco-ancestral":
                    foreach(var x in S.fighters) { x.usedMatch.RemoveAll(id=>id!="desejo"&&Content.AnySkill(id)?.ultimate==true); x.cooldowns.Clear(); }
                    break;
                case "mare-de-energia":
                    foreach(var x in S.fighters.Where(x=>x.Alive&&!Content.Def(x).neutral)) x.buffs.Add(new Buff{id="mare-de-energia",turns=2,pa=1,pm=1});
                    break;
                case "festival-de-fogos":
                    for(int tries=0;tries<60&&cells.Count<6;tries++)
                    {
                        var c=new Cell(Dice.Range(S,S.width),Dice.Range(S,S.height)); var m=S.Mirror(c);
                        if(c==m||S.At(c).mark) continue;
                        S.At(c).mark=S.At(m).mark=true; cells.Add(c); cells.Add(m);
                    }
                    break;
                case "nevoeiro": break;
                case "vendaval":
                {
                    var dir=Board.Dirs[Dice.Range(S,4)];
                    ev.cell=dir;
                    // Quem está mais à frente na direção do vento anda primeiro, para não bater em quem ainda vai sair.
                    foreach(var x in S.fighters.Where(x=>x.Alive).OrderByDescending(x=>x.cell.x*dir.x+x.cell.z*dir.z).ThenBy(x=>x.id).ToList()) Push(x,dir,1,null);
                    break;
                }
                case "papa-moedas":
                    for(int tries=0;tries<60&&cells.Count==0;tries++)
                    {
                        var c=new Cell(Dice.Range(S,S.width),Dice.Range(S,S.height)); var m=S.Mirror(c);
                        if(c==m||!MapGen.Free(S,c)||!MapGen.Free(S,m)) continue;
                        foreach(var x in new[]{c,m}) { var p=Spawn("papa-moedas",MatchState.Neutral,x,0); Emit(new GameEvent{kind="invocar",target=p.id,info=p.def,cell=x}); cells.Add(x); }
                    }
                    break;
                case "errante":
                {
                    var spot=Area(new Cell(S.width/2,S.height/2),3,false).Where(x=>Board.Walkable(S,x,false)&&Board.At(S,x)==null).OrderBy(x=>x.Distance(new Cell(S.width/2,S.height/2))).ThenBy(x=>x.z).ThenBy(x=>x.x).ToList();
                    if(spot.Count==0) { rounds=0; break; }
                    var b=Spawn("brutamontes-errante",MatchState.Neutral,spot[0],0);
                    Emit(new GameEvent{kind="invocar",target=b.id,info=b.def,cell=spot[0]}); cells.Add(spot[0]);
                    break;
                }
            }
            ev.path=cells.ToArray(); ev.value=rounds;
            if(rounds>0) { S.ev=kind; S.evRounds=rounds; }
        }
        bool LavaOk(Cell c,bool farFromFighters)
        {
            var t=S.At(c);
            return t.height==0&&t.terrain==Terrain.Livre&&!t.lava&&!t.zone&&(!farFromFighters||S.fighters.All(f=>!f.Alive||f.cell.Distance(c)>=2));
        }
        // Fim de rodada: o evento ativo age (lava se espalha, fogos explodem) e conta uma rodada.
        void TickEvent()
        {
            if(string.IsNullOrEmpty(S.ev)) return;
            var cells=new List<Cell>();
            if(S.ev=="mare-de-lava")
            {
                // Espalha para 2 pares de casas baixas vizinhas da lava.
                var options=new List<Cell>();
                for(int i=0;i<S.tiles.Length;i++)
                {
                    var c=new Cell(i%S.width,i/S.width); var m=S.Mirror(c);
                    if(Board.Index(S,c)>Board.Index(S,m)||c==m||!LavaOk(c,false)||!LavaOk(m,false)) continue;
                    if(Board.Dirs.Any(d=>Board.Contains(S,c+d)&&S.At(c+d).lava)) options.Add(c);
                }
                for(int i=0;i<2&&options.Count>0;i++)
                {
                    int k=Dice.Range(S,options.Count); var c=options[k]; options.RemoveAt(k);
                    foreach(var x in new[]{c,S.Mirror(c)}) { S.At(x).lava=true; S.At(x).coins=0; cells.Add(x); }
                }
            }
            else if(S.ev=="festival-de-fogos")
            {
                // Cada marca explode em área 1; o Escudo do dado de defesa e a armadura reduzem.
                for(int i=0;i<S.tiles.Length;i++)
                {
                    if(!S.tiles[i].mark) continue;
                    var c=new Cell(i%S.width,i/S.width); cells.Add(c); S.tiles[i].mark=false;
                    Emit(new GameEvent{kind="explosao",cell=c,info=S.ev});
                    foreach(var v in S.fighters.Where(x=>x.Alive&&x.prisoned==0&&x.cell.Distance(c)<=1).ToList())
                    {
                        var d=Roll(); int dmg=Ghost(v)?0:Math.Max(0,2-(Dice.Armor(d)?1:0)-Armor(v)); var log=new List<GameEvent>();
                        Emit(new GameEvent{kind="dano-evento",target=v.id,value=dmg,info=S.ev,cell=v.cell,dice=new[]{d}});
                        Hurt(v,dmg,log); events.AddRange(log); if(!v.Alive) KO(v);
                    }
                }
            }
            Emit(new GameEvent{kind="evento-tick",info=S.ev,value=S.evRounds-1,path=cells.ToArray()});
            if(--S.evRounds<=0) EndEvent();
        }
        // Encerra o evento ativo (fim das rodadas, chefe derrotado ou Desejo F) e desfaz o que ele deixou no mapa.
        void EndEvent()
        {
            if(string.IsNullOrEmpty(S.ev)) return;
            string id=S.ev; S.ev=""; S.evRounds=0;
            foreach(var t in S.tiles) { t.lava=false; t.mark=false; }
            Emit(new GameEvent{kind="evento-fim",info=id});
            if(id=="errante") foreach(var b in S.fighters.Where(x=>x.Alive&&x.def=="brutamontes-errante").ToList()) { b.hp=0; Emit(new GameEvent{kind="sumir",target=b.id,cell=b.cell}); }
        }
        // Neutros agem no fim da rodada: o papa-moedas foge de quem chega perto e come moedas; o Errante caça o mais próximo.
        void Neutrals()
        {
            foreach(var n in S.fighters.Where(x=>x.Alive&&Content.Def(x).neutral).ToList())
            {
                if(!n.Alive||S.over) continue;
                n.pm=Content.Def(n).pm;
                var foes=S.fighters.Where(x=>x.Alive&&!Content.Def(x).neutral&&!x.hidden).ToList();
                if(n.def=="papa-moedas")
                {
                    var coins=Enumerable.Range(0,S.tiles.Length).Where(i=>S.tiles[i].coins>0).Select(i=>new Cell(i%S.width,i/S.width)).ToList();
                    if(foes.Any(x=>x.cell.Distance(n.cell)<=2)) WalkTo(n,Best(n,c=>foes.Min(x=>x.cell.Distance(c))));
                    else if(coins.Count>0) WalkTo(n,Best(n,c=>-coins.Min(k=>k.Distance(c))));
                    continue;
                }
                n.ai++;
                var target=foes.Where(x=>x.prisoned==0).OrderBy(x=>x.cell.Distance(n.cell)).ThenBy(x=>x.id).FirstOrDefault();
                if(target==null) continue;
                var skills=Content.Def(n).skills;
                if(n.ai%2==0&&skills.Length>1&&foes.Any(x=>x.cell.Distance(n.cell)<=2)) Strike(n,skills[1],n.cell);
                if(n.Alive&&n.cell.Distance(target.cell)>1) WalkTo(n,Best(n,c=>-c.Distance(target.cell)));
                if(n.Alive&&target.Alive&&n.cell.Distance(target.cell)==1) Strike(n,skills[0],target.cell);
            }
        }
        // Casa alcançável (PM do neutro, livre) com a maior nota; empate fica onde está.
        Cell Best(Fighter n,Func<Cell,int> score)
        {
            var dist=Board.Costs(S,n); Cell best=n.cell; int top=score(n.cell);
            for(int i=0;i<dist.Length;i++)
            {
                if(dist[i]>n.pm) continue;
                var c=new Cell(i%S.width,i/S.width); if(Board.At(S,c)!=null) continue;
                int sc=score(c); if(sc>top) { top=sc; best=c; }
            }
            return best;
        }
        void WalkTo(Fighter n,Cell to)
        {
            var path=to==n.cell?null:Board.Path(S,n,to,n.pm); if(path==null) return;
            var log=new List<GameEvent>(); bool flying=Content.Def(n).flying;
            for(int i=1;i<path.Count;i++)
            {
                n.cell=path[i]; var t=S.At(n.cell);
                if(t.coins>0&&n.def=="papa-moedas") { n.carried+=t.coins; log.Add(new GameEvent{kind="comer",actor=n.id,cell=n.cell,value=t.coins}); t.coins=0; }
                if(t.lava&&!flying) { Burn(n,log); if(!n.Alive) break; }
            }
            Emit(new GameEvent{kind="mover",actor=n.id,path=path.Take(path.IndexOf(n.cell)+1).ToArray()});
            events.AddRange(log);
            if(!n.Alive) KO(n);
        }
        // Queda de pedras e terremoto (eventos originais).
        void Reshape(string kind,List<Cell> cells)
        {
            {
                var backup=S.tiles.Select(t=>new Tile{height=t.height,coins=t.coins,trap=t.trap,trapDamage=t.trapDamage,trapPa=t.trapPa,frozenBy=t.frozenBy,terrain=t.terrain,zone=t.zone,lava=t.lava,mark=t.mark}).ToArray();
                if(kind=="queda-de-pedras")
                    for(int i=0;i<1;i++) { var c=new Cell(Dice.Range(S,S.width),Dice.Range(S,S.height)); if(c==S.Mirror(c)||!MapGen.Free(S,c)||!MapGen.Free(S,S.Mirror(c))||S.At(c).zone) continue; S.At(c).terrain=S.At(S.Mirror(c)).terrain=Terrain.Caixa; cells.Add(c); cells.Add(S.Mirror(c)); }
                else
                {
                    var o=new Cell(1+Dice.Range(S,S.width-2),1+Dice.Range(S,S.height/2-1)); int delta=Dice.Range(S,2)==0?1:-1;
                    for(int x=-1;x<=1;x++) for(int z=-1;z<=1;z++) foreach(var c in new[]{o+new Cell(x,z),S.Mirror(o+new Cell(x,z))})
                        if(Board.Contains(S,c)&&!cells.Contains(c)&&S.At(c).terrain!=Terrain.Loja) { S.At(c).height=Math.Max(0,Math.Min(Board.MaxHeight,S.At(c).height+delta)); cells.Add(c); }
                }
                if(!Board.Connected(S)) { S.tiles=backup; cells.Clear(); }
            }
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
