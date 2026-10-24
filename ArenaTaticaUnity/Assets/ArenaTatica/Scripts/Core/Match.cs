using System;
using System.Collections.Generic;
using System.Linq;

namespace ArenaTatica.Core
{
    // Regras da partida. Comando entra, eventos saem; a apresentação só anima os eventos.
    public sealed partial class Match
    {
        public MatchState S { get; private set; }
        // Só para testes: faces forçadas antes do RNG da partida.
        public readonly Queue<Face> Forced=new Queue<Face>();
        List<GameEvent> events=new List<GameEvent>();
        bool networkPreview;

        public static Match New(MatchSetup setup)
        {
            var players=setup.players!=null&&setup.players.Length>=2?setup.players:new[]{ new PlayerSetup{fighters=setup.team1}, new PlayerSetup{fighters=setup.team2} };
            int n=Math.Min(players.Length,MatchState.MaxPlayers), size=Board.SizeFor(n);
            // Labirinto (autor 22/10): sempre procedural, um tamanho acima do comum; mapa salvo de cenário fechado (legado) é ignorado.
            int theme=MapGen.Fits(setup.map,size)&&setup.map.theme>=0?setup.map.theme%MapGen.Themes.Length:MapGen.Theme(setup);
            bool maze=MapGen.Closed(theme);
            if(maze) size=Board.LabyrinthSize(n);
            var m=new Match(); var s=m.S=new MatchState { rng=setup.seed==0?1:setup.seed, mode=setup.mode, players=n, width=size, height=size, edits=Content.EditsJson,
                turnMode=setup.turns, solidIllusions=setup.solidIllusions, shopPool=(setup.shopItems??new string[0]).Where(id=>Shop.Def(id)!=null).Distinct().ToArray() };
            s.target=setup.target>0?setup.target:setup.mode==VictoryMode.Tesouro?15:5;
            // Times numerados 1..K na ordem dos assentos; time 0 no setup = jogador sozinho.
            var keys=new List<string>();
            for(int p=1;p<=n;p++) { string key=players[p-1].team>0?"t"+players[p-1].team:"p"+p; if(!keys.Contains(key)) keys.Add(key); s.alliance[p]=keys.IndexOf(key)+1; }
            foreach(int t in s.Teams()) s.galeoes[t]=setup.galeoes;
            // O cenário vem antes: os fechados geram salas e corredores (o sorteio do cenário não gasta o rng).
            s.theme=theme;
            MapGen.Generate(s,setup.flatMap);
            if(!setup.flatMap) MapGen.Decorate(s,setup.seed);
            if(!maze&&MapGen.Fits(setup.map,size)) { MapGen.Load(s,setup.map); if(setup.map.theme>=0) s.theme=setup.map.theme%MapGen.Themes.Length; }
            MapGen.Portals(s);
            s.fog=MapGen.Closed(s.theme);
            var slot=SlotOrder(s);
            for(int p=1;p<=n;p++) m.Place(p,slot[p],players[p-1].fighters);
            m.Randomize(setup);
            m.BuildTimeline();
            m.SpawnMonsters();
            m.SetupBoss(setup);
            // Com posicionamento, a ilusão da Ezequiela e o primeiro turno só entram quando todos confirmarem (Position).
            if(setup.placement) s.placing=m.PlaceOrder().FirstOrDefault();
            if(s.placing==0) m.SpawnIllusions();
            if(!setup.flatMap) MapGen.SpawnCoins(s,Math.Max(2,n));
            for(int p=1;p<=n;p++) Shop.Refill(s,p);
            if(s.placing==0) m.BeginTurn(s.Active);
            Fog.Explore(s);
            return m;
        }
        // Monstros de farm do labirinto (autor, 18/10 noite): neutros em pares de lados opostos, longe das salas de início.
        public static readonly string[] Monsters={"monstro-esqueleto","monstro-cultista","monstro-troll"};
        public const int MonsterGold=3, MonsterAggro=3;
        void SpawnMonsters()
        {
            if(!S.fog) return;
            int pairs=Math.Max(2,(int)Math.Round(S.width*S.height/144f/2));
            foreach(var c in MapGen.MonsterSpots(S,pairs)) Spawn(Monsters[Dice.Range(S,Monsters.Length)],MatchState.Neutral,c,0);
        }
        public static bool Monster(Fighter f) => f!=null&&Array.IndexOf(Monsters,f.def)>=0;
        public static Match FromState(MatchState state,bool networkPreview=false) => new Match { S=state, networkPreview=networkPreview };

        void Randomize(MatchSetup setup)
        {
            if(!setup.randomStats&&!setup.randomSkills) return;
            var pool=Content.Generic.Select(x=>{var sk=x.skill.Clone(); sk.id="sorteio:"+sk.id; return sk;}).Where(s=>s.kind!=SkillKind.Forma&&s.kind!=SkillKind.Comando&&s.kind!=SkillKind.Desejo).OrderBy(s=>s.id,StringComparer.Ordinal).ToArray();
            int Roll(int value,int low,int high,int delta) => Math.Max(low,Math.Min(high,value+Dice.Range(S,delta*2+1)-delta));
            foreach(var f in S.fighters)
            {
                bool bot=Array.IndexOf(setup.botSeats??new int[0],f.team)>=0;
                if(!(bot?setup.randomBots:setup.randomPlayers)) continue;
                var d=Content.Def(f); var skills=d.skills.ToList();
                if(setup.randomSkills)
                {
                    // Mantém o tamanho da ficha e pelo menos um ataque, sem duplicatas.
                    int count=Math.Max(1,Math.Min(8,skills.Count)); var available=pool.ToList(); skills.Clear();
                    var attacks=available.Where(s=>s.kind==SkillKind.Dano&&s.damage>0&&s.pa<=(setup.randomStats?5:d.pa)).ToList();
                    if(attacks.Count==0) attacks=available.Where(s=>s.kind==SkillKind.Dano&&s.damage>0).ToList();
                    var attack=attacks[Dice.Range(S,attacks.Count)]; skills.Add(attack); available.Remove(attack);
                    while(skills.Count<count&&available.Count>0) { int i=Dice.Range(S,available.Count); skills.Add(available[i]); available.RemoveAt(i); }
                }
                var roll=new NumericRoll{stats=setup.randomStats,skills=setup.randomSkills,
                    hp=setup.randomStats?Roll(d.hp,4,30,Math.Max(1,d.hp/5)):d.hp,
                    pa=setup.randomStats?Roll(d.pa,5,8,1):d.pa,
                    pm=setup.randomStats?Roll(d.pm,1,6,1):d.pm,
                    initiative=setup.randomStats?Roll(d.initiative,0,12,1):d.initiative};
                roll.powers=skills.Select(sk=>new SkillRoll{id=sk.id,
                    damage=setup.randomStats&&sk.damage>0?Roll(sk.damage,1,8,1):sk.damage,
                    range=setup.randomStats&&sk.maxRange>1&&!sk.selfArea?Roll(sk.maxRange,Math.Min(8,sk.minRange),8,1):sk.maxRange}).ToArray();
                f.roll=roll; f.hp=Content.MaxHp(f);
            }
        }

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
        // Sprint 1.3: posiciona na ordem em que os jogadores jogam (assentos na ordem da linha do tempo). Decisão do Claude, confirmar com o autor.
        public List<int> PlaceOrder() => S.timeline.Select(S.F).Where(f=>f!=null&&MatchState.Player(f.team)).Select(f=>f.team).Distinct().ToList();
        // Casas de início do jogador (o mesmo grupo de spawn de Match.New).
        public Cell[] StartCells(int player) => player>=1&&player<=S.players?MapGen.Spawns(S,SlotOrder(S)[player],S.fighters.Count(f=>f.team==player&&f.summoner==0)):new Cell[0];
        // Casas do posicionamento (autor 17/10): casa livre da borda do mapa, de qualquer lado. Sem colar nos outros: a pelo menos
        // PlaceGap casas de todo herói de outro jogador (posicionado ou ainda no início). O time fica junto: depois do 1º herói do
        // jogador na borda, os outros (f fora da conta) vão até PlaceGroup casas de um já posicionado. As casas de início não são borda.
        public const int PlaceGap=4, PlaceGroup=3;
        bool Edge(Cell c) => c.x==0||c.z==0||c.x==S.width-1||c.z==S.height-1;
        public Cell[] PlaceCells(int player,Fighter f=null)
        {
            // Labirinto: cada jogador arruma os heróis dentro da própria sala de início.
            if(player>=1&&player<=S.players&&MapGen.Room(S,SlotOrder(S)[player]) is int[] room)
                return Enumerable.Range(0,S.tiles.Length).Select(i=>new Cell(i%S.width,i/S.width))
                    .Where(c=>c.x>=room[0]&&c.x<=room[2]&&c.z>=room[1]&&c.z<=room[3]&&Board.Walkable(S,c,false)&&(f==null||Board.Fits(S,f,c))).ToArray();
            var others=S.fighters.Where(x=>x.Alive&&x.summoner==0&&MatchState.Player(x.team)&&x.team!=player).ToList();
            var placed=S.fighters.Where(x=>x.Alive&&x.summoner==0&&x.team==player&&x!=f&&Edge(x.cell)).ToList();
            return Enumerable.Range(0,S.tiles.Length).Select(i=>new Cell(i%S.width,i/S.width))
                .Where(c=>Edge(c)&&Board.Walkable(S,c,false)&&!S.At(c).lava&&(f==null||Board.Fits(S,f,c))&&others.All(x=>Board.Gap(x,c)>=PlaceGap)
                    &&(placed.Count==0||placed.Any(x=>Board.Gap(x,c)<=PlaceGroup))).ToArray();
        }
        // "posicionar": move o herói para uma casa livre da borda (PlaceCells) ou troca com outro herói do mesmo jogador.
        // "passar": confirma; o próximo jogador posiciona e, depois do último, entram as ilusões e o primeiro turno.
        bool Position(Command cmd,int team,out string reason)
        {
            reason="";
            if(team!=S.placing) { reason=$"Aguarde: P{S.placing} está posicionando."; return false; }
            if(cmd.kind=="passar")
            {
                Emit(new GameEvent{kind="posicionado",value=team});
                var order=PlaceOrder(); int i=order.IndexOf(team);
                S.placing=i>=0&&i+1<order.Count?order[i+1]:0;
                if(S.placing==0) { SpawnIllusions(); BeginTurn(S.Active); }
                return true;
            }
            if(cmd.kind!="posicionar") { reason="Fase de posicionamento: arrume seus heróis na borda do mapa."; return false; }
            var f=S.F(cmd.fighter);
            if(f==null||f.team!=team||f.summoner!=0) { reason="Escolha um herói seu."; return false; }
            var other=Board.At(S,cmd.cell);
            if(other==f) { reason="O herói já está nessa casa."; return false; }
            if(other!=null&&(other.team!=team||other.summoner!=0)) { reason="Casa ocupada."; return false; }
            if(other!=null&&(Board.SizeOf(f)>1||Board.SizeOf(other)>1)) { reason="Lutador grande não troca de casa: escolha uma casa livre."; return false; }
            if(other==null&&!PlaceCells(team,f).Contains(cmd.cell)) { reason=Edge(cmd.cell)?$"Longe demais do seu time ou perto de outro jogador (mín. {PlaceGap} casas).":"Só nas casas livres da borda do mapa."; return false; }
            var from=f.cell; f.cell=cmd.cell; Emit(new GameEvent{kind="posicionar",actor=f.id,cell=f.cell,path=new[]{from,f.cell}});
            if(other!=null) { other.cell=from; Emit(new GameEvent{kind="posicionar",actor=other.id,cell=from,path=new[]{cmd.cell,from}}); }
            return true;
        }
        void Place(int player,int slot,string[] defs)
        {
            var spawns=MapGen.Spawns(S,slot,defs.Length);
            // 5º e 6º heróis (limite da sala): a casa extra vira chão livre, sem moeda nem armadilha.
            for(int i=4;i<defs.Length&&i<spawns.Length;i++) { var t=S.At(spawns[i]); t.terrain=Terrain.Livre; t.lava=false; t.coins=0; t.trap=0; }
            for(int i=0;i<defs.Length&&i<spawns.Length;i++) Spawn(defs[i],player,spawns[i],0);
            foreach(var f in S.fighters.Where(x=>x.team==player&&Board.SizeOf(x)>1).ToList()) Settle(f);
        }
        // Lutador grande que não cabe onde está (início, reviver) vai para o canto livre mais perto onde cabe inteiro.
        void Settle(Fighter f)
        {
            if(Board.Fits(S,f,f.cell)) return;
            var spot=Enumerable.Range(0,S.tiles.Length).Select(i=>new Cell(i%S.width,i/S.width)).Where(c=>!S.At(c).lava&&Board.Fits(S,f,c))
                .OrderBy(c=>c.Distance(f.cell)).ThenBy(c=>c.z).ThenBy(c=>c.x).FirstOrDefault();
            if(Board.Fits(S,f,spot)) f.cell=spot;
        }
        Fighter Spawn(string def,int team,Cell cell,int summoner)
        {
            var d=Content.Fighters[def];
            var f=new Fighter { id=S.nextId++, def=def, team=team, hp=d.hp, cell=cell, summoner=summoner, form=d.startForm };
            // Invocação (summoner != 0) não trava o movimento de ninguém nem revela escondidos ao encostar. As flags vão no
            // lutador (runtime) e não só na ficha: a ViewFor esconde o summoner dos inimigos e a ficha não serve de prova.
            f.noLock=summoner!=0||d.noLock; f.noReveal=summoner!=0||d.noReveal;
            // PV pelo número de jogadores (zumbi do Damon): a diferença para a ficha vai em bonusHp, que já viaja na rede.
            if(d.hpByPlayers!=null&&d.hpByPlayers.Length>0) { f.bonusHp=d.hpByPlayers[Math.Max(0,Math.Min(S.players-2,d.hpByPlayers.Length-1))]-d.hp; f.hp=d.hp+f.bonusHp; }
            S.fighters.Add(f); return f;
        }
        // Ezequiela começa com as cópias ilusórias dela coladas (FighterDef.illusionCount; 2 na ficha dela): os inimigos as
        // veem como Ezequiela. Autor (13/10): sempre uma do lado da outra, mas sem dar para saber qual é qual: casa, id, ordem
        // na lista e na linha do tempo são sorteados entre elas.
        void SpawnIllusions()
        {
            LunaStartup();
            foreach(var f in S.fighters.Where(x=>x.summoner==0&&Content.Def(x).illusion).ToList())
                for(int i=0;i<Math.Max(1,Content.Def(f).illusionCount);i++) SpawnIllusion(f);
        }
        void SpawnIllusion(Fighter f)
        {
            bool Free(Cell c) => Board.Contains(S,c)&&Board.Walkable(S,c,false)&&Board.At(S,c)==null;
            var near=Board.Dirs.Select(d=>f.cell+d).Where(Free).ToList();
            if(near.Count==0) near=Board.Dirs.Select(d=>f.cell+d+d).Where(Free).ToList();
            if(near.Count==0) return;
            var copy=Spawn("ilusao",f.team,near[Dice.Range(S,near.Count)],f.id); MakeCopy(f,copy);
            if(Dice.Range(S,2)==0&&S.fighters.Count(x=>x.summoner==f.id)==1) { var c=f.cell; f.cell=copy.cell; copy.cell=c; }
            // A cópia entra na linha do tempo colada na original (antes ou depois dela).
            int at=S.timeline.IndexOf(f.id); S.timeline.Insert(at<0?S.timeline.Count:Dice.Range(S,2)==0?at:at+1,copy.id);
            if(Dice.Range(S,2)!=0) return;
            // Ids trocados: a cópia herda o id e a entrada da linha do tempo da original, e ela entra na entrada recém-criada.
            // Quem era invocação da original passa a apontar para o id novo dela (senão a cópia é que virava dona).
            int real=f.id, fake=copy.id; f.id=fake; copy.id=real;
            foreach(var x in S.fighters) if(x.summoner==real) x.summoner=fake;
            S.fighters.Remove(copy); S.fighters.Insert(S.fighters.IndexOf(f),copy);
        }
        // Cópia ilusória: disfarce da original, a mesma forma e, na Ezequiela, 1 PV (o resto da ficha vai em bonusHp, que
        // viaja na rede). O PM máximo é o da original e o gasto é só dela (moveBudget, recalibrado em SyncCopy).
        void MakeCopy(Fighter owner,Fighter copy)
        {
            copy.disguise=string.IsNullOrEmpty(owner.disguise)?owner.def:owner.disguise;
            copy.form=owner.form;
            if(owner.def=="ezequiela") { copy.bonusHp=1-Content.Def(copy).hp; copy.hp=1; }
            copy.moveBudget=0; copy.pm=CopyCap(copy);
        }
        // Ilusão disfarçada (cópia): a ViewFor apaga o disfarce dos aliados, então o teste de cópia também vale pelo disfarce.
        static bool Copy(Fighter f) => f!=null&&f.summoner!=0&&!string.IsNullOrEmpty(f.disguise);
        // Encostar revela escondidos: só heróis (invocação e ilusão não revelam) e nunca quem tem noReveal.
        static bool Reveals(Fighter f) => f!=null&&f.summoner==0&&!f.noReveal&&!Content.Def(f).noReveal;
        int CopyCap(Fighter f) => Math.Max(0,Content.MaxPm(f,S));
        // PM disponível da cópia = máximo atual da original menos o que ELA já andou neste turno (nunca recarrega por mudar).
        void SyncCopy(Fighter f) { if(Copy(f)) f.pm=Math.Max(0,CopyCap(f)-Math.Max(0,f.moveBudget)); }
        void SyncCopies(Fighter owner)
        {
            if(owner==null||owner.def!="ezequiela") return;
            foreach(var c in S.fighters.Where(x=>x.Alive&&Copy(x)&&x.summoner==owner.id).ToList()) SyncCopy(c);
        }
        // Krosmaster (livro de regras): no turno de um jogador, TODOS os lutadores dele jogam em sequência, da maior Iniciativa para a menor
        // (a "linha do tempo"); depois passa a vez. Começa o time com a maior soma de Iniciativa (empate: o lutador de maior Iniciativa;
        // depois o assento menor). Em times (2v2, 3v3) os jogadores se revezam entre os times. Todos contra todos = cada jogador é um time.
        void BuildTimeline()
        {
            int Init(Fighter f) => EffectiveInitiative(f);
            List<Fighter> Own(int seat) => S.fighters.Where(f=>f.team==seat&&f.summoner==0).OrderByDescending(Init).ThenBy(f=>f.id).ToList();
            (int sum,int top) Score(IEnumerable<Fighter> g) => (g.Sum(Init),g.Select(Init).DefaultIfEmpty(0).Max());
            var seats=S.fighters.Where(f=>f.summoner==0&&MatchState.Player(f.team)).Select(f=>f.team).Distinct().ToList();
            var groups=seats.GroupBy(S.Team).Select(g=>g.OrderByDescending(p=>Score(Own(p)).sum).ThenByDescending(p=>Score(Own(p)).top).ThenBy(p=>p).ToList()).ToList();
            // Sprint 1.4 (espelho, autor 17/10): com o mesmo herói em dois times, a MENOR soma começa; empate na soma vira sorteio
            // (última prioridade, Dice.Range) sem gastar a sequência dos dados: a semente volta ao valor de antes. Sem espelho: maior soma.
            var defs=groups.Select(g=>new HashSet<string>(g.SelectMany(Own).Select(f=>f.def))).ToList();
            bool mirror=defs.Any(a=>defs.Any(b=>a!=b&&a.Overlaps(b)));
            uint seed=S.rng; var draw=groups.Select(_=>mirror?Dice.Range(S,1000):0).ToList(); S.rng=seed;
            var teams=groups.Select((g,i)=>(g,i)).OrderBy(t=>(mirror?1:-1)*Score(t.g.SelectMany(Own)).sum).ThenBy(t=>draw[t.i])
                .ThenByDescending(t=>Score(t.g.SelectMany(Own)).top).ThenBy(t=>S.Team(t.g[0])).Select(t=>t.g).ToList();
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
            if(networkPreview) { reason="A visão de rede só permite prévia; envie o comando ao Host."; return false; }
            var f=S.Active;
            if(cmd==null) { reason="Comando vazio."; return false; }
            if(S.over) { reason="Partida encerrada."; return false; }
            if(cmd.revision!=S.revision) { reason="Estado atualizado. Repita a ação."; return false; }
            if(S.placing>0) { bool placed=Position(cmd,team,out reason); if(placed) { Fog.Explore(S); S.revision++; } return placed; }
            if(f==null||f.id!=cmd.fighter||f.team!=team) { reason="Aguarde seu turno."; return false; }
            bool ok;
            switch(cmd.kind)
            {
                case "mover": ok=Move(f,cmd.cell,out reason); break;
                case "skill": ok=UseSkill(f,cmd.skill,cmd.cell,cmd.slot,cmd.cell2,out reason); break;
                case "comprar": ok=Buy(f,cmd.slot,out reason); break;
                case "abrir": ok=OpenDoor(f,cmd.cell,out reason); break;
                case "acender": ok=Light(f,cmd.cell,out reason); break;
                case "passar": ok=true; Advance(); break;
                default: reason="Ação desconhecida."; ok=false; break;
            }
            if(ok)
            {
                DamianGreeting(f);
                CheckVictory();
                // Nocauteado no próprio turno (armadilha): o turno acaba.
                if(!S.over&&S.Active!=null&&(!S.Active.Alive||S.Active.endedTurn)) Advance();
                // Nulification Field: entrou, perde PA; saiu (ou a Pele caiu), recupera.
                if(!S.over&&S.Active is Fighter act&&act.Alive) Field(act);
                foreach(var owner in S.fighters.Where(x=>x.Alive&&x.def=="ezequiela"&&x.summoner==0)) SyncCopies(owner);
                RevealCelinos();
                Fog.Explore(S);
                S.revision++;
            }
            return ok;
        }

        bool Move(Fighter f,Cell to,out string reason)
        {
            reason="";
            if(GuardActive(f)) { reason="Desative a guarda para se mover."; return false; }
            if(LunaBlockMove(f,out reason)) return false;
            // Ilusão: o PM máximo é o da original, recalculado agora e sem devolver o que ela já andou neste turno.
            SyncCopy(f); int pmBefore=f.pm;
            // Névoa (autor, 18/10 noite): o caminho segue o que o time conhece (o escuro conta como chão livre). Se algo
            // escondido bloquear, o movimento para no limite do obstáculo, sem evento que mostre o que era.
            var known=Fog.On(S)&&MatchState.Player(f.team)?S.With(Fog.Known(S,f.team)):S;
            // Cheiro de Sangue (Daemion): rumo a um inimigo ferido, cada PM vale 2 casas (meio PM por passo, arredondado para cima no fim).
            int scale=Scent(S,f,to)?2:1, units=f.pm*scale;
            var path=Board.Path(known,f,to,units);
            if(path==null) { reason="Destino bloqueado, ocupado ou além dos PM."; return false; }
            bool flying=Content.Def(f).flying;
            var walked=new List<Cell>{f.cell}; var after=new List<GameEvent>();
            for(int i=1;i<path.Count;i++)
            {
                // O caminho não denuncia invisíveis; a autoridade interrompe antes da colisão.
                if(Board.Footprint(f,path[i]).Any(c=>Board.At(S,c) is Fighter o&&o!=f&&!Board.PassThrough(S,o))) break;
                int cost=Board.BudgetStep(S,f,f.cell,path[i],units,flying);
                if(cost<0||cost>units) break;
                if(!Tackle(f,after)) break;
                var before=Board.Footprint(f).ToList();
                units-=cost; f.pm=units/scale; f.cell=path[i]; walked.Add(f.cell);
                // Grande: cada casa nova que ele pisa conta (moeda, lava, baú, espetos, armadilha).
                bool stop=false;
                foreach(var cell in Board.Footprint(f).Where(c=>!before.Contains(c)))
                {
                    var t=S.At(cell);
                    CollectGears(f,cell,after);
                    if(t.coins>0) { S.coins[f.team]+=t.coins; S.collected[S.Team(f.team)]+=t.coins; after.Add(new GameEvent{kind="moeda",actor=f.id,cell=cell,value=t.coins}); t.coins=0; }
                    if(t.lava&&!flying) { Burn(f,after); if(!f.Alive) { stop=true; break; } }
                    Enter(f,t,flying,after); if(!f.Alive) { stop=true; break; }
                    if(t.trap!=0&&!S.Ally(t.trap,f.team)&&!flying) { if(Spring(f,t,after)||!f.Alive) { stop=true; break; } }
                }
                if(stop) break;
                // Portal: entra quando qualquer casa nova dele pisa na entrada (o Primal Beast também passa).
                var gate=Board.Footprint(f).Where(c=>!before.Contains(c)).Select(PortalIn).FirstOrDefault(g=>g!=null&&Passes(f,g));
                if(gate!=null) { if(walked.Count>1) Emit(new GameEvent{kind="mover",actor=f.id,path=walked.ToArray()}); walked=new List<Cell>{f.cell}; events.AddRange(after); after.Clear(); Warp(f,gate); break; }
            }
            // Parou em cima de uma ilusão atravessável (armadilha no meio do caminho): volta para a casa anterior.
            while(walked.Count>1&&Board.Footprint(f).Any(c=>S.fighters.Any(o=>o!=f&&o.Alive&&Board.Occupies(o,c)))) { walked.RemoveAt(walked.Count-1); f.cell=walked[walked.Count-1]; }
            if(walked.Count>1) Emit(new GameEvent{kind="mover",actor=f.id,path=walked.ToArray()});
            // O que a cópia gastou de PM neste turno fica anotado (a troca de limite da original não devolve andado).
            if(Copy(f)) f.moveBudget+=Math.Max(0,pmBefore-f.pm);
            // Esbarrar: quem termina o movimento colado num inimigo escondido o revela (a ilusão nunca revela).
            if(f.Alive&&Reveals(f)) foreach(var h in S.fighters.Where(x=>x.Alive&&x.hidden&&!S.Ally(x.team,f.team)&&Board.Gap(x,f)==1).ToList())
            { h.hidden=false; after.Add(new GameEvent{kind="revelar",actor=f.id,target=h.id,cell=h.cell}); }
            events.AddRange(after);
            if(!f.Alive) KO(f);
            return true;
        }
        // Armadilha inimiga: dano fixo menos armadura e o movimento (ou empurrão) acaba ali. Mina terrestre (MatchState.mines): fere,
        // mas não tira PM nem para o movimento ou o empurrão (dá para empurrar por várias minas). Volta true se para.
        bool Spring(Fighter f,Tile t,List<GameEvent> log)
        {
            var source=S.F(t.trapBy);
            int dmg=Ghost(f)?0:DamageFrom(source,f,Math.Max(0,t.trapDamage-Armor(f)));
            int at=Board.Index(S,f.cell); bool mine=S.mines.Contains(at), stop=!mine&&!FormImmune(f);
            if(stop) f.pm=0;
            // Mina de gelo: também tira PA no próximo turno.
            if(t.trapPa>0) AddBuff(f,new Buff{id="gelo",turns=2,pa=-t.trapPa});
            log.Add(new GameEvent{kind="armadilha",actor=t.trap,target=f.id,cell=f.cell,value=dmg,info=t.trapPa>0?"gelo":mine?"mina":null});
            t.trap=0; t.trapBy=0; t.trapDamage=0; t.trapPa=0; S.mines.Remove(at);
            Hurt(f,dmg,log,source);
            return stop;
        }
        // Portal: quem entra na entrada sai numa casa livre vizinha da saída (a 1ª de Board.Dirs); sem casa livre, fica.
        // Lutador grande sai com o corpo inteiro colado na saída (o canto livre mais perto dela onde ele cabe).
        Cell? Exit(Fighter f,Portal gate)
        {
            var exit=new Cell(gate.b%S.width,gate.b/S.width); bool flying=Content.Def(f).flying; int n=Board.SizeOf(f);
            if(n==1) return Board.Dirs.Select(d=>exit+d).Where(x=>Board.Contains(S,x)&&Board.Walkable(S,x,flying)&&Board.At(S,x)==null).Cast<Cell?>().FirstOrDefault();
            var spots=new List<Cell>();
            for(int x=exit.x-n;x<=exit.x+1;x++) for(int z=exit.z-n;z<=exit.z+1;z++) {
                var at=new Cell(x,z); var probe=new Fighter{def=f.def,cell=at};
                if(!Board.Occupies(probe,exit)&&Board.Gap(probe,exit)==1&&Board.Fits(S,f,at)) spots.Add(at);
            }
            return spots.Count==0?(Cell?)null:spots.OrderBy(c=>c.z).ThenBy(c=>c.x).First();
        }
        bool Warp(Fighter f,Portal gate)
        {
            var to=Exit(f,gate);
            if(to==null) return false;
            Emit(new GameEvent{kind="portal",actor=f.id,path=new[]{f.cell,to.Value},cell=to.Value});
            f.cell=to.Value; Collect(f);
            return true;
        }
        void ClosePortals(int by) => S.portals.RemoveAll(p=>p.by==by);
        // Portal só de aliados (Mordomo Pilis): os outros pisam na entrada sem atravessar.
        bool Passes(Fighter f,Portal g) => !TechProtected(f)&&!Unbreakable(f)&&(!g.allies||S.F(g.by) is Fighter o&&S.Ally(o.team,f.team));
        Portal PortalIn(Cell c) { int i=Board.Index(S,c); return S.portals.Find(p=>p.a==i); }
        bool HasPortal(Cell c) { int i=Board.Index(S,c); return S.portals.Exists(p=>p.a==i||p.b==i); }
        // Lava (evento): 2 de dano de fogo, menos armadura.
        void Burn(Fighter f,List<GameEvent> log)
        {
            int dmg=Ghost(f)?0:Math.Max(0,2-Armor(f));
            log.Add(new GameEvent{kind="lava",target=f.id,cell=f.cell,value=dmg});
            Hurt(f,dmg,log);
        }
        // Escondido anda no máximo HiddenPm casas por turno (nerf do autor, 13/10), mais os bônus de PM (botas, buffs, passivas,
        // forma; autor 21/10).
        public const int HiddenPm=2;
        public int HiddenCap(Fighter f) => HiddenPm+Math.Max(0,f.bonusPm+f.buffs.Sum(b=>b.pm)+(Content.Form(f)?.pm??0)+Passive(f,PassiveStat.PM));
        // Forma fantasma (Edgar): golpes de inimigos não o acertam.
        static bool Ghost(Fighter v) => Content.Form(v)?.intangible??false;
        int Armor(Fighter f) => f.armor+Content.Def(f).armor+f.buffs.Sum(b=>b.armor)+(Content.Form(f)?.armor??0)+Passive(f,PassiveStat.Armadura);
        // Passivas do editor (FighterDef.passives): soma do que os donos vivos dão a v.
        public int Passive(Fighter v,PassiveStat stat)
        {
            int sum=0;
            foreach(var o in S.fighters) { if(!o.Alive) continue; foreach(var p in Content.Def(o).passives??new PassiveDef[0]) if(p!=null&&p.stat==stat&&PassiveHits(o,p,v)&&!Pulsed(o,v,stat)&&!(p.amount<0&&FormImmune(v))) sum+=p.amount; }
            return sum;
        }
        // L.E.L.A Pulse: enquanto o aliado tem o buff do Pulso dela num atributo, a passiva dela nesse atributo não soma de novo
        // (Ira ×2 = +4, não +2 da passiva +4 do Pulso; autor 21/10).
        public const string PulseTag="pulso:";
        static bool Pulsed(Fighter o,Fighter v,PassiveStat stat)
        {
            if(v.buffs.Count==0) return false; string tag=PulseTag+o.id;
            foreach(var b in v.buffs) if(b.from==tag&&(stat switch { PassiveStat.Dano=>b.dmg, PassiveStat.Armadura=>b.armor, PassiveStat.PA=>b.pa, PassiveStat.PM=>b.pm, PassiveStat.Alcance=>b.range, PassiveStat.CuraPorTurno=>b.regen, _=>0 })!=0) return true;
            return false;
        }
        // Efeitos negativos e de controle bloqueados: forma imune a efeitos (Berserk, sempre) ou buff de imunidade (Chá
        // protetor). Os buffs de imunidade somam: um immune negativo (o espelho inverte o buff) anula os positivos.
        static bool Immune(Fighter v) => FormImmune(v)||!v.buffs.Exists(b=>b.immune<0)&&v.buffs.Exists(b=>b.immune>0);
        static bool FormImmune(Fighter v) => TechProtected(v)||Unbreakable(v)||(Content.Form(v)?.effectImmune??false);
        static bool Unstoppable(Fighter v) => Content.Form(v)?.unstoppable??false;
        bool PassiveHits(Fighter o,PassiveDef p,Fighter v)
        {
            bool self=o==v, ally=S.Ally(o.team,v.team);
            // Passiva só da forma (3.3) e Lobo solitário (4.1): nenhum aliado do dono até lonely casas em qualquer direção, diagonal
            // incluída (autor 17/10: sozinho = nenhum aliado a 1 casa em volta, o 3×3 do Isolado).
            if(!string.IsNullOrEmpty(p.form)&&o.form!=p.form) return false;
            if(p.lonely>0&&S.fighters.Any(x=>x.Alive&&x!=o&&S.Ally(x.team,o.team)&&Math.Max(Math.Abs(x.cell.x-o.cell.x),Math.Abs(x.cell.z-o.cell.z))<=p.lonely)) return false;
            bool who=p.who switch { PassiveWho.Si=>self, PassiveWho.Aliados=>!self&&ally, PassiveWho.AliadosESi=>ally, PassiveWho.Inimigos=>!ally, _=>!self };
            return who&&(self||p.range<=0||Board.Gap(o,v)<=p.range);
        }
        // Dano já calculado chega ao lutador: guarda do Tchu tcha (aliado até 2 casas absorve 1), revela quem estava escondido.
        // O nocaute (e o renascer) fica para quem chamou, depois do evento do golpe.
        bool SlaysSummon(Fighter source,Fighter target,int damage) => damage>0&&source!=null&&target.summoner!=0&&Content.Def(source).summonSlayer&&!S.Ally(source.team,target.team);
        int DamageFrom(Fighter source,Fighter target,int damage) => !Ghost(target)&&SlaysSummon(source,target,damage)?Math.Max(damage,target.hp):damage;
        // retaliate=false corta o troco da forma que reflete (Berserk): o dano devolvido não volta de novo (sem laço).
        void Hurt(Fighter v,int dmg,List<GameEvent> log,Fighter source=null,bool retaliate=true)
        {
            // Forma fantasma: nada fere (golpes, área, aura, armadilha, lava, batida, eventos).
            if(dmg<=0||Ghost(v)||Unbreakable(v)) return;
            if(Mechanism(v).box.by==v.id&&Mechanism(v).box.hp>0) { Mechanism(v).box.hp=Math.Max(0,Mechanism(v).box.hp-dmg); log.Add(new GameEvent{kind="status",info="matriz-dano",target=v.id,cell=v.cell,value=Mechanism(v).box.hp}); if(Mechanism(v).box.hp==0) CloseBox(v,"matriz-protecao-fim"); return; }
            bool slay=SlaysSummon(source,v,dmg); dmg=DamageFrom(source,v,dmg);
            // Revisão Claude (23/10): preso ou banido não guarda ninguém (mesmo critério da guarda direcional, GuardActive).
            var guard=S.fighters.FirstOrDefault(g=>g.Alive&&g.prisoned==0&&!g.banished&&g.id!=v.id&&S.Ally(g.team,v.team)&&Content.Def(g).guard&&!(Content.Form(g)?.noGuard??false)&&Board.Gap(g,v)<=2);
            if(guard!=null)
            {
                dmg--; int guardHp=guard.hp; guard.hp=SlaysSummon(source,guard,1)?0:Math.Max(0,guard.hp-1);
                // A guarda também guarda de quem veio o dano que ela levou (o nocaute dela pode recrutar o chefe).
                if(guard.hp<guardHp) guard.lastDamageBy=source?.id??0;
                log.Add(new GameEvent{kind="guarda",actor=guard.id,target=v.id,value=1,cell=guard.cell});
            }
            int before=v.hp; v.hp=slay?0:Math.Max(0,v.hp-dmg);
            // Fonte real do dano: 0 = ambiental (lava, evento). É ela que decide o recrutamento do chefe no nocaute.
            int lost=before-v.hp; if(lost>0) v.lastDamageBy=source?.id??0;
            if(v.hidden) { v.hidden=false; log.Add(new GameEvent{kind="revelar",target=v.id,cell=v.cell}); }
            if(guard!=null&&!guard.Alive) KO(guard,log);
            // Berserk (FormDef.reflectDamage): perdeu PV de verdade, mesmo que tenha caído, e o agressor real está de pé:
            // devolve exatamente esse dano a ele, com o troco sem novo troco e com a fonte trocada (é ele quem feriu).
            int back=Content.Form(v)?.reflectDamage??0;
            if(retaliate&&lost>0&&back>0&&source!=null&&source!=v&&source.Alive)
            {
                log.Add(new GameEvent{kind="refletir",actor=v.id,target=source.id,value=back,cell=source.cell});
                Hurt(source,back,log,v,false); if(!source.Alive) KO(source,log);
            }
        }

        // Compras à vontade enquanto houver moedas (autor, 11/10), numa casa ao lado da loja; a loja renova no fim do turno de quem comprou; Item e Buff valem na hora, o resto vira carga de skill.
        bool Buy(Fighter f,int slot,out string reason)
        {
            reason="";
            var shop=S.shops[f.team].slots;
            if(f.summoner!=0) { reason="Invocações não compram."; return false; }
            if(!Board.NextToShop(S,f)&&!LunaShop(f)) { reason="Fique numa casa ao lado da loja para comprar."; return false; }
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
                case ShopKind.Buff: AddBuff(f,new Buff{id=d.id,turns=d.turns,pa=d.pa,pm=d.pm,armor=d.armor,range=d.range}); f.pa+=d.pa; f.pm+=d.pm; break;
                default:
                    var c=f.charges.Find(x=>x.skill==d.skill.id);
                    // Permanente (uses < 0): comprar de novo não muda nada.
                    if(c==null) f.charges.Add(new Charge{skill=d.skill.id,uses=d.uses}); else if(d.uses<0||c.uses<0) c.uses=ShopDef.Permanent; else c.uses+=d.uses;
                    break;
            }
            // Item de PM muda o máximo da original: as cópias dela recalculam o disponível sem devolver o que já andaram.
            SyncCopies(f);
        }
        // Sair de casa vizinha a inimigos: cada inimigo rola 1 dado; cada Trava precisa de uma Esquiva. Falhou: perde PA e PM.
        bool Tackle(Fighter f,List<GameEvent> log)
        {
            if(TechProtected(f)||Unbreakable(f)) return true;
            // Trava só quem é herói: invocação (summoner != 0) e ficha com noLock nunca travam ninguém.
            var enemies=S.fighters.Where(e=>e.Alive&&!e.hidden&&!S.Ally(e.team,f.team)&&Board.Gap(e,f)==1&&e.summoner==0&&!e.noLock&&!Content.Def(e).noLock&&e.prisoned==0).ToList();
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
            if(sk.global) return S.width+S.height;
            bool half=sk.halfMap&&(string.IsNullOrEmpty(sk.halfMapIn)||f.form==sk.halfMapIn);
            int r=(half?Content.MapReach(S.width,S.height,sk.mapPct):sk.maxRange)+(sk.modifiable?(S.At(f.cell).height>S.At(target).height?1:0)+f.bonusRange+f.buffs.Sum(b=>b.range):0);
            if(sk.pmRange) r+=f.pm;
            if(sk.markRange&&Board.At(S,target) is Fighter marked&&!S.Ally(marked.team,f.team)) r+=marked.marks;
            if(S.ev=="nevoeiro"&&sk.maxRange>1) r=Math.Max(Math.Max(1,sk.minRange),r-2);
            if(sk.maxRange>1&&!sk.selfArea) r=Math.Max(Math.Max(1,sk.minRange),r+Passive(f,PassiveStat.Alcance));
            return r;
        }
        // Salto e teleporte por cima de muro (parede ou porta fechada no caminho reto): +2 PA (autor, 18/10 noite; antes era
        // metade do alcance e +1 PA em todo mapa com paredes).
        static bool Mobility(SkillDef sk) => sk.kind==SkillKind.Salto||sk.kind==SkillKind.Teleporte;
        public const int WallJump=2;
        public static int Cost(Fighter f,SkillDef sk,MatchState s) => Cost(f,sk);
        // Custo com o destino: quem salta (ou quem é teleportado, na skill de 2 casas) e para onde vai.
        public static int Cost(Fighter f,SkillDef sk,MatchState s,Cell c,Cell? c2)
        {
            int pa=Cost(f,sk); if(s==null||!Mobility(sk)) return pa;
            var who=sk.TwoCells?Board.At(s,c):f; var to=sk.TwoCells?c2:c;
            if(who==null||to==null) return pa;
            var from=Board.Nearest(who,to.Value);
            return pa+(Board.OverWall(s,from,to.Value)?WallJump:0);
        }

        // ── Labirinto: portas, vela e névoa ──
        public static string Interaction(MatchState s,Cell c) => !Board.Contains(s,c)?null:s.At(c).terrain==Terrain.Porta?"abrir":s.At(c).Obj==TileObj.Vela&&!s.revealed?"acender":null;
        bool OpenDoor(Fighter f,Cell c,out string reason)
        {
            reason="";
            if(!Board.Contains(S,c)||S.At(c).terrain!=Terrain.Porta) { reason="Não há porta fechada aí."; return false; }
            if(Board.Gap(f,c)!=1) { reason="Encoste na porta para abrir."; return false; }
            if(f.pm<1) { reason="Abrir a porta custa 1 PM."; return false; }
            f.pm--; Open(c,f.id); return true;
        }
        // Porta dupla do corredor de 2 casas: as folhas coladas abrem juntas (o Primal Beast passa).
        void Open(Cell c,int by)
        {
            var t=S.At(c); t.terrain=Terrain.Livre; t.Obj=TileObj.PortaAberta; Emit(new GameEvent{kind="porta",actor=by,cell=c});
            foreach(var d in Board.Dirs) if(Board.Contains(S,c+d)&&S.At(c+d).terrain==Terrain.Porta) Open(c+d,by);
        }
        public const int VelaMoedas=2;
        bool Light(Fighter f,Cell c,out string reason)
        {
            reason="";
            if(!Board.Contains(S,c)||S.At(c).Obj!=TileObj.Vela||S.revealed) { reason="Não há vela apagada aí."; return false; }
            if(Board.Gap(f,c)!=1) { reason="Chegue a 1 casa da vela para acender."; return false; }
            S.revealed=true;
            if(MatchState.Player(f.team)) { S.coins[f.team]+=VelaMoedas; S.collected[S.Team(f.team)]+=VelaMoedas; }
            Emit(new GameEvent{kind="vela",actor=f.id,cell=c,value=VelaMoedas}); return true;
        }
        // Névoa: o time vê quem é aliado e quem está nas casas que ele vê (coladas num aliado vivo ou na sala dele, Fog.Vision).
        public static bool Sees(MatchState s,int team,Fighter f)
        {
            if(!s.fog||s.revealed||team<=0||s.Ally(f.team,team)) return true;
            if(!Board.Contains(s,f.cell)) return false;
            var seen=Fog.Vision(s,team);
            return Board.Footprint(f).Any(c=>Board.Contains(s,c)&&seen[Board.Index(s,c)]);
        }
        // Escondido de quem olha: invisível (Esconder) ou na névoa.
        // Radar do Celinos: o time do robô vê o escondido no raio (Match.OnRadar), mesmo na névoa.
        public static bool Concealed(MatchState s,int team,Fighter f) => !s.Ally(f.team,team)&&!OnRadar(s,team,f)&&(f.hidden||!Sees(s,team,f));
        // Só geometria (alcance, linha reta, visão): usado também para desenhar a área da skill.
        public bool InRange(Fighter f,SkillDef sk,Cell c,out string reason)
        {
            reason="";
            if(!Board.Contains(S,c)) { reason="Fora do mapa."; return false; }
            int dist=Board.Gap(f,c);
            if(dist<sk.minRange||dist>Range(f,sk,c)) { reason="Fora de alcance."; return false; }
            if(sk.global||Free(f,sk)) return true;
            if(sk.line&&!Board.Footprint(f).Any(o=>o.x==c.x||o.z==c.z)) { reason="Só em linha reta."; return false; }
            if(sk.sight&&!Board.Occupies(f,c)&&!Board.Footprint(f).Any(o=>Board.LineOfSight(S,o,c,Mobility(sk)))) { reason="Sem linha de visão."; return false; }
            // Dano à distância nunca passa por muro, mesmo a skill que ignora a visão (autor, 18/10 noite).
            if(sk.kind==SkillKind.Dano&&dist>1&&!Board.Footprint(f).Any(o=>Board.WallClear(S,o,c))) { reason="Atrás do muro."; return false; }
            return true;
        }
        // Skills que o lutador pode usar agora: as próprias e as cargas compradas.
        // Formas: só aparecem as skills da forma atual, e a troca para a forma em que já está some.
        public static List<SkillDef> Skills(Fighter f)
        {
            var list=Content.Def(f).skills.Where(s=>(string.IsNullOrEmpty(s.form)||s.form==f.form)&&(string.IsNullOrEmpty(s.notForm)||s.notForm!=f.form)&&!(s.kind==SkillKind.Forma&&s.toForm==f.form)&&!(s.kind==SkillKind.Desejo&&(f.channel>0||f.wishReady))).ToList();
            foreach(var id in f.stolen) { var sk=Content.StolenSkill(f,id); if(sk!=null) list.Add(sk); }
            foreach(var id in f.learned) { var sk=Content.AnySkill(id); if(sk!=null&&!list.Contains(sk)) list.Add(sk); }
            if(f.wishReady) list.AddRange(Content.Wishes);
            foreach(var c in f.charges) { var sk=Content.AnySkill(c.skill); if(sk!=null&&c.uses!=0&&!list.Contains(sk)) list.Add(sk); }
            return list;
        }
        // Cópia do estado que um jogador pode ver: sem a semente do RNG e sem as armadilhas de quem não é aliado.
        public static MatchState ViewFor(MatchState s,int player)
        {
            var v=UnityEngine.JsonUtility.FromJson<MatchState>(UnityEngine.JsonUtility.ToJson(s));
            v.rng=0;
            v.bossCycles.Clear(); // A ordem futura dos alvos fica só no estado autoritativo, assim como o RNG.
            // Labirinto: espetos só depois de sair do chão; na névoa, só as casas que o time conhece (Fog.Known), os enfeites
            // e portais delas, e a memória do próprio time.
            if(s.fog&&Fog.On(s)&&player>0) {
                v.tiles=Fog.Known(s,player); var mem=Fog.Memory(s,player);
                bool Remembered(float x,float z) { var c=new Cell((int)Math.Round(x),(int)Math.Round(z)); return Board.Contains(s,c)&&mem[Board.Index(s,c)]; }
                v.decor=v.decor.Where(d=>d.piece!=null&&d.piece.StartsWith(MapGen.FloorPrefix)||Remembered(d.x,d.z)).ToList();
                v.portals=v.portals.Where(p=>p.a>=0&&p.a<mem.Length&&mem[p.a]||p.b>=0&&p.b<mem.Length&&mem[p.b]).ToList();
                v.sprung=v.sprung.Where(i=>i>=0&&i<mem.Length&&mem[i]).ToList();
                int team=s.Team(player); v.explored=new string[MatchState.MaxPlayers+1];
                if(s.explored!=null&&team>=0&&team<s.explored.Length&&team<v.explored.Length) v.explored[team]=s.explored[team];
            }
            else if(s.fog) for(int i=0;i<v.tiles.Length;i++) if(v.tiles[i].Obj==TileObj.Espetos&&!s.sprung.Contains(i)) v.tiles[i].obj=0;
            foreach(var t in v.tiles) if(t.trap!=0&&!s.Ally(t.trap,player)) { t.trap=0; t.trapDamage=0; t.trapPa=0; t.trapBy=0; }
            v.mines.RemoveAll(i=>i<0||i>=v.tiles.Length||v.tiles[i].trap==0);
            // Ilusão: para os inimigos é igual à original (mesmo nome, PV e sem marca de invocação).
            foreach(var f in v.fighters.Where(x=>!string.IsNullOrEmpty(x.disguise)&&!s.Ally(x.team,player)))
            {
                var real=v.fighters.Find(x=>x.id==f.summoner);
                // PV mostrado = PV da original menos o dano que a ilusão já levou.
                int lost=Content.MaxHp(f)-f.hp;
                // PV máximo igual ao da original (itens de PV): sem isso o PV mostrado passava do máximo e o cliente recusava todo
                // estado (ValidState), perdendo a sala (bug da Ezequiela, 21/10).
                f.def=f.disguise; f.summoner=0; if(real!=null) { f.bonusHp=real.bonusHp; f.hp=f.Alive&&real.Alive?Math.Max(1,real.hp-lost):f.hp; f.form=real.form; f.roll=real.roll;
                    // Recargas e usos iguais aos da original (3.7): a ficha das duas mostra o mesmo.
                    f.cooldowns=real.cooldowns.Select(c=>new Charge{skill=c.skill,uses=c.uses}).ToList(); f.used=new List<string>(real.used); f.usedMatch=new List<string>(real.usedMatch); }
                f.disguise=null;
            }
            // Névoa: quem o time não vê vai como escondido.
            foreach(var f in v.fighters.Where(x=>x.Alive&&!x.hidden&&!Sees(s,player,s.F(x.id)??x))) f.hidden=true;
            // Radar do Celinos: o time do robô vê o escondido; os outros times nem sabem que ele está no radar.
            foreach(var f in v.fighters.Where(x=>x.Alive&&OnRadar(s,player,s.F(x.id)))) f.hidden=false;
            v.Luna.radar.RemoveAll(r=>!s.Ally(r.team,player));
            foreach(var f in v.fighters.Where(x=>x.hidden&&!s.Ally(x.team,player))) f.cell=Cell.Unknown;
            // Canalizada do inimigo: só a skill (o evento já mostra qual); as casas do alvo ficam com o Host até resolver.
            foreach(var f in v.fighters.Where(x=>!string.IsNullOrEmpty(x.channeling)&&!s.Ally(x.team,player))) f.channeling=f.channeling.Split(';')[0];
            // Névoa (revisão 020): engrenagens de outra família só nas casas que o time vê agora, como as moedas (Fog.Known);
            // marca do Celinos num alvo escondido não diz a casa dele.
            if(s.fog&&Fog.On(s)&&player>0) {
                var seen=Fog.Vision(s,player);
                v.Luna.gears.RemoveAll(g=>!(s.F(g.owner) is Fighter o&&s.Ally(o.team,player))&&!(g.cell>=0&&g.cell<seen.Length&&seen[g.cell]));
            }
            foreach(var m in v.Luna.marks) if(s.F(m.id) is Fighter t&&Concealed(s,player,t)) { m.x=-1; m.z=-1; }
            // Revisão Claude (23/10): em quem o Somnax já usou Pacto/Renúncia e o Elo usado são do time dele; a guarda do inimigo
            // escondido (ligada e o lado) não vai para quem não o vê.
            foreach(var f in v.fighters.Where(x=>!s.Ally(x.team,player))) {
                f.pactTargets.Clear(); f.renounceTargets.Clear(); f.twinBondUsed=false;
                if(f.hidden) { f.directionalGuard=false; f.guardDirection=Cell.Unknown; f.guardSkill=null; }
            }
            return v;
        }
        public static List<GameEvent> EventsFor(List<GameEvent> events,MatchState s,int player,IEnumerable<int> hiddenBefore=null)
        {
            // O estado final pode já ter revelado alguém. A filtragem começa no estado ANTES da ação,
            // para não divulgar o trajeto escondido que terminou num contato/revelação.
            var hidden=new HashSet<int>(hiddenBefore??s.fighters.Where(f=>f.hidden).Select(f=>f.id));
            foreach(var f in s.fighters) if(f.Alive&&!Sees(s,player,f)) hidden.Add(f.id);
            hidden.RemoveWhere(id=>s.F(id) is Fighter f&&(s.Ally(f.team,player)||OnRadar(s,player,f)));
            var result=new List<GameEvent>();
            foreach(var e in events) {
                // Radar do Celinos: o aviso de revelado é só do time do robô.
                if(e.kind=="revelar"&&e.info=="radar"&&!(s.F(e.actor) is Fighter radar&&s.Ally(radar.team,player))) continue;
                if(e.kind=="armadilha-posta"&&!(s.F(e.actor) is Fighter owner&&s.Ally(owner.team,player))) continue;
                if(e.kind=="esconder"&&s.F(e.actor) is Fighter h&&!s.Ally(h.team,player)) hidden.Add(e.actor);
                if(e.kind=="revelar"||e.kind=="renascer"||e.kind=="nocaute") hidden.Remove(e.target);
                bool global=new[]{"turno","rodada","loja","dominio","eliminado","vitoria","cataclisma","evento-duracao","evento-fim","voz-global"}.Contains(e.kind);
                if(!global&&hidden.Contains(e.actor)) continue;
                var copy=UnityEngine.JsonUtility.FromJson<GameEvent>(UnityEngine.JsonUtility.ToJson(e));
                if(!global&&hidden.Contains(e.target)) {
                    // A conjuração continua visível, mas só no destino que o jogador escolheu.
                    // Não revela um invisível que interceptou ou resistiu ao disparo, nem conta alvos secretos da área.
                    if(e.kind!="ataque"||e.dice==null||!Board.Contains(s,e.aim)) continue;
                    copy.target=0; copy.cell=e.aim; copy.value=-1; copy.path=null;
                }
                if(global&&hidden.Contains(e.actor)) { copy.cell=Cell.Unknown; copy.path=null; }
                if(e.kind=="voz-global") { copy.cell=copy.aim=Cell.Unknown; copy.path=null; copy.target=0; }
                // O evento de invocação não deve entregar o tipo real de uma ilusão disfarçada.
                if(e.kind=="invocar"&&s.F(e.target) is Fighter summoned&&!string.IsNullOrEmpty(summoned.disguise)&&!s.Ally(summoned.team,player)) copy.info=summoned.disguise;
                result.Add(copy);
            }
            return result;
        }

        // Skills de 2 casas (SkillDef.TwoCells): sem c2 confere só a 1ª escolha (a mira do HUD); com c2, as duas.
        public bool CanTarget(Fighter f,SkillDef sk,Cell c,out string reason) => Check(f,sk,c,false,out reason);
        public bool CanTarget(Fighter f,SkillDef sk,Cell c,Cell c2,out string reason) => Check(f,sk,c,false,out reason,c2);
        // Bolinhas (3.1): v é alvo válido para quem usa a skill? Padrão (hits 0) fica com a regra de cada tipo.
        public bool Hits(Fighter f,SkillDef sk,Fighter v) => v!=null&&(Mask(f,sk)&(int)(v==f?Hit.Si:S.Ally(v.team,f.team)?Hit.Aliado:Hit.Inimigo))!=0;
        // Fora da forma halfMapIn, hitsOut (se houver) troca os alvos (Shambles fora da Room: só o próprio time).
        public static int Mask(Fighter f,SkillDef sk) => sk.hitsOut!=0&&!string.IsNullOrEmpty(sk.halfMapIn)&&f.form!=sk.halfMapIn?sk.hitsOut:sk.hits;
        // Na forma halfMapIn, freeIn ignora visão e muro (Shambles dentro da Room).
        public static bool Free(Fighter f,SkillDef sk) => sk.freeIn&&!string.IsNullOrEmpty(sk.halfMapIn)&&f.form==sk.halfMapIn;
        // Isolado (4.2): nenhum aliado nas 8 casas em volta (3×3 centrado nele).
        public static bool Isolated(MatchState s,Fighter v) => !s.fighters.Any(x=>x.Alive&&x!=v&&s.Ally(x.team,v.team)&&Math.Max(Math.Abs(x.cell.x-v.cell.x),Math.Abs(x.cell.z-v.cell.z))<=1);
        // Pode ser trocado de casa ou teleportado por outro: não imóvel, ancorado nem preso. Teleporte que só pega aliado é
        // benefício e passa mesmo num aliado imóvel; quem está em forma imune a efeitos recusa o teleporte hostil.
        bool Movable(Fighter f,SkillDef sk,Fighter v)
        {
            if(v==null||TechProtected(v)||Unbreakable(v)) return false;
            bool ally=S.Ally(v.team,f.team);
            if(!ally&&FormImmune(v)) return false;
            if(ally&&sk.kind==SkillKind.Teleporte&&sk.CanHitAlly&&!sk.CanHitEnemy) return true;
            return !Fixed(v)&&(sk.Custom?Hits(f,sk,v):true);
        }
        bool Free(Cell c,bool flying) => Board.Contains(S,c)&&Board.Walkable(S,c,flying)&&Board.At(S,c)==null;
        // Peça do mapa que o Shambles com props troca de lugar (casa sem lutador).
        bool Prop(Cell c) { if(!Board.Contains(S,c)||Board.At(S,c)!=null) return false; var t=S.At(c);
            return t.terrain==Terrain.Arvore||t.terrain==Terrain.Arbusto||t.terrain==Terrain.Caixa||t.Obj==TileObj.Bau||t.Obj==TileObj.Braseiro; }
        bool Swappable(Fighter f,SkillDef sk,Cell c) => Board.KnownAt(S,c,f) is Fighter v?Movable(f,sk,v)&&(!sk.props||Board.SizeOf(v)==1):sk.props&&Prop(c);
        // Teleporte entre sombras (Ellie): casa de sombra dela ou casa livre colada num inimigo que ela vê.
        bool ShadowExit(Fighter f,Cell c) => Board.At(S,c)==null&&(S.At(c).shadow==f.id
            ||S.fighters.Any(v=>v.Alive&&!S.Ally(v.team,f.team)&&!Concealed(S,f.team,v)&&Board.Contains(S,v.cell)&&Board.Gap(v,c)==1));
        // Segunda casa das skills de 2 casas: alcance a partir de quem usa e a regra de cada tipo.
        bool CheckSecond(Fighter f,SkillDef sk,Cell c,Cell c2,out string reason)
        {
            if(c2==c) { reason="Escolha outra casa."; return false; }
            if(!InRange(f,sk,c2,out reason)) return false;
            switch(sk.kind)
            {
                case SkillKind.DoceSalgado: return CheckTreatAlly(f,c2,true,c,out reason);
                case SkillKind.Troca: if(!Swappable(f,sk,c2)) { reason=sk.props?"Escolha um lutador ou uma peça do mapa.":"Escolha dois lutadores que possam ser trocados."; return false; } break;
                case SkillKind.Portal: if(!Board.Walkable(S,c2,false)||HasPortal(c2)) { reason="Casa bloqueada."; return false; } break;
                case SkillKind.Teleporte: if(!Free(c2,Board.At(S,c) is Fighter v&&Content.Def(v).flying)) { reason="Casa bloqueada."; return false; } break;
            }
            return true;
        }
        // free: sem custo nem limites (o gato dispara pela Faísca da varinha, que já pagou).
        bool Check(Fighter f,SkillDef sk,Cell c,bool free,out string reason,Cell? c2=null)
        {
            if(!GuardAllowsSkill(f,sk,out reason)) return false;
            // Matriz fechada (autor 23/10): nenhuma skill, nem ataque comum, até sair (saida-matriz).
            if(!free&&sk!=null&&sk.effect!="saida-matriz"&&LunaBlockMove(f,out _)) { reason="A matriz está fechada: só dá para sair (saida-matriz)."; return false; }
            if(sk!=null&&sk.Custom2) return Board.Contains(S,c)?LunaCheck(f,sk,c,c2??Cell.Unknown,free,out reason):LunaOutside(out reason);
            if(!CheckFirst(f,sk,c,free,out reason)) return false;
            if(c2!=null&&sk.TwoCells&&!CheckSecond(f,sk,c,c2.Value,out reason)) return false;
            // Revisão Claude (23/10): na 1ª casa de uma skill de duas casas o custo base já vale (o extra do muro vem com a 2ª).
            if(!free&&f.pa<(c2!=null||!sk.TwoCells?Cost(f,sk,S,c,c2):Cost(f,sk,S))) { reason=$"PA insuficientes (+{WallJump} para passar por cima do muro)."; return false; }
            return true;
        }
        bool CheckFirst(Fighter f,SkillDef sk,Cell c,bool free,out string reason)
        {
            reason="";
            if(sk==null) { reason="Skill desconhecida."; return false; }
            if(Mechanism(f).box.by==f.id&&Mechanism(f).box.hp>0&&sk.effect!="saida-matriz") { reason="A matriz está fechada."; return false; }
            if(!Board.Contains(S,c)) { reason="Fora do mapa."; return false; }
            if(!free)
            {
                if(f.channel>0) { reason="Canalizando o Desejo."; return false; }
                if(!string.IsNullOrEmpty(f.channeling)) { reason="Canalizando."; return false; }
                if(f.pa<Cost(f,sk,S)) { reason="PA insuficientes."; return false; }
                if(sk.perTurn>0&&f.used.Count(u=>u==sk.id)>=sk.perTurn+f.buffs.Sum(b=>b.extra)+(Content.Form(f)?.extra??0)) { reason="Limite por turno atingido."; return false; }
                if(sk.perMatch>0&&f.usedMatch.Count(u=>u==sk.id)>=sk.perMatch) { reason="Já usada nesta partida."; return false; }
                if(f.cooldowns.Find(x=>x.skill==sk.id) is Charge wait) { reason=$"Recarga: falta{(wait.uses>1?"m":"")} {wait.uses} turno{(wait.uses>1?"s":"")}."; return false; }
                if(sk.minRound>S.round) { reason=$"Disponível a partir da rodada {sk.minRound}."; return false; }
                if(sk.pmCost>0&&f.pm<sk.pmCost) { reason=$"PM insuficientes ({sk.pmCost} PM)."; return false; }
                if(sk.perForm>0&&f.usedForm.Count(u=>u==sk.id)>=sk.perForm) { reason="Já usada nesta transformação."; return false; }
            }
            if(sk.kind!=SkillKind.Comando&&!InRange(f,sk,c,out reason)) return false;
            var v=Board.At(S,c);
            // Conjura-se no chão: existência, invisibilidade ou imunidade de um alvo não bloqueiam a escolha.
            switch(sk.kind)
            {
                case SkillKind.GuardaDirecional: return CheckDirectionalGuard(f,c,out reason);
                case SkillKind.DoceSalgado: return CheckTreatAlly(f,c,false,Cell.Unknown,out reason);
                case SkillKind.Dano:
                    if(sk.selfArea) { if(!Board.Occupies(f,c)) { reason="Use em si mesmo."; return false; } break; }
                    break;
                case SkillKind.Cura: break;
                case SkillKind.Suporte:
                    if(sk.selfArea) { if(!Board.Occupies(f,c)) { reason="Use em si mesmo."; return false; } break; }
                    break;
                case SkillKind.Zona: break;
                case SkillKind.Comando: if(!FindUnit(f,sk,c,null,out _,out reason)) return false; break;
                case SkillKind.Reviver:
                    if(!Board.Walkable(S,c,false)) { reason="Casa bloqueada."; return false; }
                    break;
                case SkillKind.Desejo: if(!WishTarget(f,sk,c,v,out reason)) return false; break;
                case SkillKind.Armadilha: if(!Board.Walkable(S,c,false)||S.At(c).trap!=0) { reason="Casa bloqueada."; return false; } break;
                case SkillKind.Teleporte:
                    // Passo nebuloso com 🟢/🔴: a 1ª casa é quem vai (si só com 🔵), a 2ª o destino.
                    if(sk.TwoCells) { if(!Movable(f,sk,Board.KnownAt(S,c,f))) { reason="Escolha quem vai ser teleportado."; return false; } break; }
                    if(!Board.Walkable(S,c,Content.Def(f).flying)||Board.SizeOf(f)>1&&!Board.Fits(S,f,c)) { reason="Casa bloqueada."; return false; }
                    if(sk.shadowStep&&!ShadowExit(f,c)) { reason="Só numa sombra sua ou colado num inimigo."; return false; }
                    break;
                case SkillKind.Troca: if(!Swappable(f,sk,c)) { reason=sk.props?"Escolha um lutador ou uma peça do mapa.":"Escolha um lutador que possa ser trocado."; return false; } break;
                case SkillKind.Portal: if(!Board.Walkable(S,c,false)||HasPortal(c)) { reason="Casa bloqueada."; return false; } break;
                case SkillKind.Salto:
                    if(!Landing(f,c,out _,true)) { reason="Não há casa livre junto do destino."; return false; }
                    break;
                case SkillKind.Forma:
                    if(!Board.Occupies(f,c)) { reason="Use em si mesmo."; return false; }
                    if(sk.toForm==f.form) { reason="Já está nessa forma."; return false; }
                    break;
                case SkillKind.Aura: case SkillKind.Pulso: if(!Board.Occupies(f,c)) { reason="Use em si mesmo."; return false; } break;
                case SkillKind.Esconder:
                    if(!Board.Occupies(f,c)) { reason="Use em si mesmo."; return false; }
                    if(f.hidden) { reason="Já está escondido."; return false; }
                    if(!sk.noCover&&!Cover(f.cell)) { reason="Precisa estar junto a relevo mais alto, árvore ou caixa."; return false; }
                    break;
                case SkillKind.Espelho:
                    break;
                default:
                {
                    // Máximo por grupo (padrão 1 por tipo): o lobo da loja convive com o dragão da Ciantia; os gatos são 2.
                    var kinds=sk.summon.Split('|'); var d=Content.Fighters[kinds[0]];
                    if(S.fighters.Count(x=>x.Alive&&x.summoner==f.id&&kinds.Contains(x.def))>=Math.Max(1,sk.maxActive)) { reason="Limite de invocações ativas."; return false; }
                    if(!Board.Walkable(S,c,d.flying)) { reason="Casa bloqueada."; return false; }
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
        // Cemitério (Pena de fênix, Chama da fênix, Desejo A): aliados nocauteados que podem voltar; o jogador escolhe (Command.slot = índice).
        // Ordem: maior Nível primeiro (slot 0 = escolha antiga dos bots), depois id; igual no Host e nos clientes.
        public static List<Fighter> Graveyard(MatchState s,Fighter f) => s.fighters.Where(x=>!x.Alive&&x.summoner==0&&s.Ally(x.team,f.team)&&MatchState.Player(x.team)&&!s.eliminated[s.Team(x.team)])
            .OrderByDescending(x=>Content.Def(x).level).ThenBy(x=>x.id).ToList();
        public static bool Revives(SkillDef sk) => sk!=null&&(sk.kind==SkillKind.Reviver||sk.kind==SkillKind.Desejo&&sk.wish=="a");
        Fighter DeadAlly(Fighter f,int slot) { var dead=Graveyard(S,f); return dead.Count==0?null:dead[slot>=0&&slot<dead.Count?slot:0]; }
        // Invocações sem turno que obedecem (gatos) e cumprem a ordem em c: para andar, as que chegam; para disparar, as que alcançam o alvo.
        // Ordem: mais perto de c, depois id (a 1ª é a escolha antiga, que os bots seguem).
        public List<Fighter> Cats(Fighter f,SkillDef sk,Cell c) => S.fighters.Where(x=>x.Alive&&x.summoner==f.id&&Content.Def(x).noTurn&&Content.Def(x).skills.Length>0)
            .OrderBy(x=>Board.Gap(x,c)).ThenBy(x=>x.id)
            .Where(u=>sk.step>0?Board.KnownAt(S,c,u)==null&&Board.Path(S,u,c,sk.step)!=null:Check(u,Content.Def(u).skills[0],c,true,out _)).ToList();
        // Gato da ordem (3.9): cat = casa do escolhido (Command.cell2). Com 2+ gatos que cumprem, a escolha é obrigatória; null = a antiga (mais perto).
        bool FindUnit(Fighter f,SkillDef sk,Cell c,Cell? cat,out Fighter unit,out string reason)
        {
            reason=""; unit=null;
            if(!S.fighters.Any(x=>x.Alive&&x.summoner==f.id&&Content.Def(x).noTurn&&Content.Def(x).skills.Length>0)) { reason="Nenhum gato em campo."; return false; }
            var units=Cats(f,sk,c);
            if(units.Count==0) { reason=sk.step>0?$"Nenhum gato chega lá ({sk.step} casas).":"Nenhum gato alcança esse alvo."; return false; }
            unit=cat==null?units[0]:units.Find(u=>u.cell==cat.Value);
            if(unit==null&&units.Count==1) unit=units[0];
            if(unit==null) { reason="Escolha qual gato vai cumprir a ordem."; return false; }
            return true;
        }
        bool WishTarget(Fighter f,SkillDef sk,Cell c,Fighter v,out string reason)
        {
            reason="";
            if(v==null&&!string.IsNullOrEmpty(sk.wish)&&sk.wish!="a"&&sk.wish!="f") return true;
            switch(sk.wish)
            {
                case null: case "": if(!Board.Occupies(f,c)) { reason="Use em si mesmo."; return false; } return true;
                case "a":
                    if(!Board.Walkable(S,c,false)) { reason="Escolha uma casa livre vizinha."; return false; }
                    return true;
                case "b": case "c": case "d": case "e": return true;
                case "f": if(string.IsNullOrEmpty(S.ev)) { reason="Nenhum evento ativo."; return false; } return true;
            }
            reason="Opção desconhecida."; return false;
        }
        // Desejo D: quem usa escolhe o lutador e, na lista dele, a skill (Command.slot). Fora: ultimate, forma, Desejo e as que o Kellen já tem.
        // Ordem da ficha, igual no Host e nos clientes.
        public static List<SkillDef> Copyables(Fighter f,Fighter v) => v==null?new List<SkillDef>():Content.Def(v).skills.Concat(v.learned.Select(Content.AnySkill))
            .Where(s=>s!=null&&!s.ultimate&&s.kind!=SkillKind.Forma&&s.kind!=SkillKind.Desejo&&s.kind!=SkillKind.Aura&&string.IsNullOrEmpty(s.form)&&Content.Skill(f,s.id)==null)
            .Distinct().ToList();
        // Aura ligada (ou já paga neste turno): desligar e religar não custam PA.
        // Troca de arma livre (freeSwap, Damian 2.4): só cobra se já usou alguma skill que não é de Forma neste turno.
        public static int Cost(Fighter f,SkillDef sk)
        {
            if(sk.kind==SkillKind.Aura&&(f.auraOn||f.auraPaid)) return 0;
            if(sk.kind==SkillKind.Forma&&sk.freeSwap&&!UsedWeapon(f)) return 0;
            return sk.pa;
        }
        public static bool UsedWeapon(Fighter f) => f.used.Any(u=>(Content.Skill(f,u)??Content.AnySkill(u)) is SkillDef s&&s.kind!=SkillKind.Forma);
        bool UseSkill(Fighter f,string id,Cell c,int slot,Cell c2,out string reason)
        {
            var sk=Content.Skill(f,id);
            if(!Check(f,sk,c,false,out reason,c2)) return false;
            if(sk.wish=="d"&&Board.At(S,c) is Fighter copyTarget&&(!copyTarget.hidden||S.Ally(copyTarget.team,f.team))) {
                int options=Copyables(f,copyTarget).Count;
                if(options>0&&(slot<0||slot>=options)) { reason="Escolha a skill que vai copiar."; return false; }
            }
            if(Revives(sk)) { int dead=Graveyard(S,f).Count; if(dead>1&&(slot<0||slot>=dead)) { reason="Escolha quem vai reviver."; return false; } }
            if(sk.kind==SkillKind.Comando&&!FindUnit(f,sk,c,c2,out _,out reason)) return false;
            f.pa-=Cost(f,sk,S,c,c2); f.used.Add(sk.id); if(sk.perMatch>0) f.usedMatch.Add(sk.id);
            f.pm=Math.Max(0,f.pm-sk.pmCost)+sk.gainPm; if(sk.perForm>0) f.usedForm.Add(sk.id);
            if(sk.cooldown>0) f.cooldowns.Add(new Charge{skill=sk.id,uses=sk.cooldown});
            if(sk.kind!=SkillKind.Espelho&&sk.kind!=SkillKind.Desejo) f.lastSkill=sk.id;
            if(sk.kind==SkillKind.Dano) f.lastAttack=sk.id;
            var charge=f.charges.Find(x=>x.skill==sk.id);
            if(charge!=null&&charge.uses>0&&Array.Find(Content.Def(f).skills,x=>x.id==sk.id)==null&&--charge.uses<=0) f.charges.Remove(charge);
            // Canalizada (3.4): paga agora, encerra o turno e resolve no começo do próximo turno (BeginTurn).
            if(sk.channeled&&sk.kind!=SkillKind.Desejo)
            {
                f.channeling=$"{sk.id};{c.x};{c.z};{c2.x};{c2.z};{slot}"; f.pa=0; f.pm=0;
                Emit(new GameEvent{kind="canalizar",actor=f.id,info=sk.id,cell=c,aim=c});
                return true;
            }
            Resolve(f,sk,c,slot,c2);
            return true;
        }
        // Efeito da skill já paga (na hora ou ao fim da canalização).
        void Resolve(Fighter f,SkillDef sk,Cell c,int slot,Cell c2)
        {
            if(sk.Custom2) { ResolveLuna(f,sk,c,slot,c2); return; }
            switch(sk.kind)
            {
                case SkillKind.Cura:
                {
                    var v=Board.At(S,c); int heal=0;
                    bool ok=v!=null&&!TechProtected(v)&&!Unbreakable(v)&&v.prisoned==0&&(sk.Custom?Hits(f,sk,v):S.Ally(v.team,f.team));
                    if(ok&&!NoHeal(v)) { heal=Math.Max(0,Math.Min(InvertedHealing(v,sk.damage),Content.MaxHp(v)-v.hp)); v.hp+=heal; }
                    Emit(new GameEvent{kind="cura",actor=f.id,target=heal>0?v.id:0,value=heal,info=sk.id,cell=c});
                    // Injection Shot: quem usa também se cura.
                    if(ok&&sk.drain>0) Heal(f,sk.drain,f,sk.id);
                    // Cura em área só com bolinhas marcadas (o padrão antigo cura um alvo).
                    if(sk.Custom&&sk.area>0) foreach(var a in S.fighters.Where(x=>x.Alive&&x!=v&&x.prisoned==0&&Hits(f,sk,x)&&Board.Footprint(x).Any(xc=>InArea(c,xc,sk.area,sk.square))).ToList()) Heal(a,sk.damage,f,sk.id);
                    break;
                }
                case SkillKind.Armadilha:
                    S.At(c).trap=f.team; S.At(c).trapBy=f.id; S.At(c).trapDamage=sk.damage; S.At(c).trapPa=sk.paLoss;
                    S.mines.Remove(Board.Index(S,c)); if(sk.mine) S.mines.Add(Board.Index(S,c));
                    Emit(new GameEvent{kind="armadilha-posta",actor=f.id,info=sk.id,cell=c});
                    break;
                case SkillKind.Dano: { var hitCell=sk.beam?Beam(f,c):Intercept(f,c); Strike(f,sk,hitCell,!sk.beam&&hitCell!=c,c); break; }
                case SkillKind.Teleporte:
                {
                    var who=sk.TwoCells?Board.At(S,c):f; var to=sk.TwoCells?c2:c;
                    // Teleporte que só pega aliado é benefício: passa num aliado imóvel/ancorado (o hostil não).
                    bool gift=who!=null&&who!=f&&S.Ally(who.team,f.team)&&sk.Custom&&sk.CanHitAlly&&!sk.CanHitEnemy;
                    if(who==null||Board.At(S,to)!=null||(who!=f&&Fixed(who)&&!gift)||!Board.Fits(S,who,to)) { GroundMiss(f,sk,to); break; }
                    // Light Speed: só os PM das casas além do alcance base.
                    if(sk.pmRange) f.pm=Math.Max(0,f.pm-Math.Max(0,Board.Gap(who,to)-sk.maxRange));
                    Emit(new GameEvent{kind="teleporte",actor=who.id,target=f.id,info=sk.id,path=new[]{who.cell,to},cell=to});
                    who.cell=to; Collect(who);
                    break;
                }
                case SkillKind.Troca:
                {
                    // Shambles (4.3): os dois trocam de casa na hora. Com props, troca também peças do mapa (21/10).
                    var a=Board.At(S,c); var b=Board.At(S,c2);
                    if(sk.props&&(a==null||b==null)) { SwapProps(f,sk,c,c2,a,b); break; }
                    if(a==null||b==null||a==b||Fixed(a)||Fixed(b)) { GroundMiss(f,sk,c); break; }
                    Emit(new GameEvent{kind="troca",actor=f.id,target=a.id,value=b.id,info=sk.id,path=new[]{a.cell,b.cell},cell=c,aim=c2});
                    (a.cell,b.cell)=(b.cell,a.cell); Collect(a); Collect(b);
                    break;
                }
                case SkillKind.Portal:
                {
                    // Portal (4.5): um par por dono; o novo fecha o anterior. Saída marcada com o índice negativo da entrada.
                    ClosePortals(f.id);
                    int turns=sk.turns>0?sk.turns:2;
                    S.portals.Add(new Portal{a=Board.Index(S,c),b=Board.Index(S,c2),by=f.id,turns=turns,allies=sk.alliesOnly});
                    Emit(new GameEvent{kind="portal-aberto",actor=f.id,info=sk.id,path=new[]{c,c2},cell=c,aim=c2,value=turns});
                    break;
                }
                case SkillKind.Salto:
                {
                    if(!Landing(f,c,out var land)) { GroundMiss(f,sk,c); break; }
                    Emit(new GameEvent{kind="salto",actor=f.id,target=Board.At(S,c)?.id??0,info=sk.id,path=new[]{f.cell,land},cell=land});
                    f.cell=land; Collect(f);
                    break;
                }
                case SkillKind.Forma: SetForm(f,sk.toForm,sk.turns,sk.id); break;
                case SkillKind.Esconder:
                    f.hidden=true; f.pm=Math.Min(f.pm,HiddenCap(f)); Emit(new GameEvent{kind="esconder",actor=f.id,info=sk.id,cell=f.cell});
                    break;
                case SkillKind.Espelho:
                {
                    // Reflete no alvo o último ataque dele, na hora, com o dano da skill copiada (magia: nada intercepta).
                    var v=Board.At(S,c); var copied=v==null?null:Content.Skill(v,v.lastAttack)??Content.AnySkill(v.lastAttack);
                    if(v==null||S.Ally(v.team,f.team)||v.prisoned>0||copied==null) { GroundMiss(f,sk,c); break; }
                    Emit(new GameEvent{kind="espelho",actor=f.id,target=v.id,info=copied.id,cell=c});
                    Strike(f,copied,c);
                    break;
                }
                case SkillKind.GuardaDirecional: SetDirectionalGuard(f,sk,c); break;
                case SkillKind.DoceSalgado: ShareTreat(f,sk,c,c2); break;
                case SkillKind.Suporte: Support(f,sk,c); break;
                case SkillKind.Zona:
                {
                    var cells=Area(c,sk.area,sk.square).ToArray();
                    if(sk.freeze) foreach(var x in cells) S.At(x).frozenBy=f.id;
                    Emit(new GameEvent{kind="zona",actor=f.id,info=sk.id,cell=c,path=sk.reveal?new Cell[0]:cells,value=sk.area});
                    // Olhar do espelho: quem está escondido na área aparece.
                    if(sk.reveal) foreach(var h in S.fighters.Where(x=>x.Alive&&x.hidden&&!S.Ally(x.team,f.team)&&Board.Footprint(x).Any(xc=>InArea(c,xc,sk.area,false))).ToList())
                    { h.hidden=false; Emit(new GameEvent{kind="revelar",actor=f.id,target=h.id,cell=h.cell}); }
                    break;
                }
                case SkillKind.Comando: Order(f,sk,c,c2); break;
                case SkillKind.Reviver: if(Board.At(S,c)==null&&DeadAlly(f,slot) is Fighter back) Revive(f,back,c,sk.heal,sk.id); else GroundMiss(f,sk,c); break;
                case SkillKind.Desejo: Wish(f,sk,c,slot); break;
                case SkillKind.Aura:
                    if(f.auraOn) { f.auraOn=false; Emit(new GameEvent{kind="aura",actor=f.id,info="desligar",cell=f.cell}); break; }
                    f.auraOn=true;
                    // Ligar paga a manutenção deste turno e aplica na hora; religada no mesmo turno, só volta a girar.
                    if(f.auraPaid) Emit(new GameEvent{kind="aura",actor=f.id,info="ativar",cell=f.cell,value=sk.area});
                    else { f.auraPaid=true; AuraPulse(f,sk,"ativar"); }
                    break;
                case SkillKind.Pulso: Pulse(f,sk); break;
                default: if(Board.At(S,c)==null) Summon(f,sk.summon,c); else GroundMiss(f,sk,c); break;
            }
            BossExtra(f,sk,c);
            if(sk.endsForm) SetForm(f,Content.Def(f).startForm,0,sk.id);
        }
        // Shambles com peças: lutador ↔ peça (o lutador vai para a casa da peça e ela para a dele) ou peça ↔ peça.
        void SwapProps(Fighter f,SkillDef sk,Cell c,Cell c2,Fighter a,Fighter b)
        {
            var who=a??b; var fighterCell=a!=null?c:c2; var propCell=a!=null?c2:c;
            if(!Prop(propCell)||who!=null&&(Fixed(who)||Board.SizeOf(who)>1)||who==null&&!Prop(fighterCell)) { GroundMiss(f,sk,c); return; }
            var x=S.At(fighterCell); var y=S.At(propCell);
            (x.terrain,y.terrain)=(y.terrain,x.terrain); (x.obj,y.obj)=(y.obj,x.obj);
            Emit(new GameEvent{kind="troca",actor=f.id,target=who?.id??0,value=0,info=sk.id,path=new[]{fighterCell,propCell},cell=c,aim=c2});
            if(who!=null) { who.cell=propCell; Collect(who); }
        }
        // Corte cósmico: anda na linha até o alvo e para no 1º lutador, num obstáculo ou em chão mais alto que o de quem corta.
        Cell Beam(Fighter f,Cell c)
        {
            var from=Board.Nearest(f,c); int h=S.At(from).height; var dir=Dir(from,c); var at=from;
            for(int i=0;i<64&&at!=c;i++)
            {
                var next=at+dir;
                if(!Board.Contains(S,next)) return at;
                if(Board.At(S,next) is Fighter v&&v!=f) return next;
                if(!Board.Walkable(S,next,false)&&S.At(next).terrain!=Terrain.Arbusto||S.At(next).height>h) return next;
                at=next;
            }
            return c;
        }
        void GroundMiss(Fighter f,SkillDef sk,Cell c) => Emit(new GameEvent{kind="ataque",actor=f.id,info=sk.id,cell=c,aim=c,target=0,value=-1});
        Fighter Summon(Fighter f,string summon,Cell c)
        {
            var kinds=summon.Split('|');
            var s=Spawn(kinds[kinds.Length==1?0:Dice.Range(S,kinds.Length)],f.team,c,f.id);
            if(s.def=="ilusao") MakeCopy(f,s);
            // Invocação sem turno (gato, totem) não entra na linha do tempo.
            if(!Content.Def(s).noTurn) S.timeline.Insert(S.active+1,s.id);
            Emit(new GameEvent{kind="invocar",actor=f.id,target=s.id,info=s.def,cell=c});
            return s;
        }
        // Corte negro (Ikaito): quem foi cortado não se cura até o fim do turno de quem cortou.
        static bool NoHeal(Fighter v) => v.buffs.Exists(b=>b.id==NoHealBuff);
        public const string NoHealBuff="sem-cura";
        void Heal(Fighter v,int amount,Fighter by,string info)
        {
            if(NoHeal(v)||Unbreakable(v)||TechProtected(v)&&by!=v) return;
            // Espelho-cura (o sem-cura invertido) dobra a cura positiva.
            int heal=Math.Min(InvertedHealing(v,amount),Content.MaxHp(v)-v.hp); if(heal<=0) return;
            v.hp+=heal; Emit(new GameEvent{kind="cura",actor=by.id,target=v.id,value=heal,info=info,cell=v.cell});
        }
        // Aura (Dualidade guardiã): aliados até area casas curam heal (o dono não); inimigos, inclusive neutros, levam damage fixo
        // (sem dado nem armadura). Uma aplicação por alvo por turno do dono: ligar e manter cobram e aplicam só uma vez (auraPaid).
        void AuraPulse(Fighter f,SkillDef sk,string info)
        {
            Emit(new GameEvent{kind="aura",actor=f.id,info=info,cell=f.cell,value=sk.area});
            var inside=S.fighters.Where(x=>x.Alive&&x!=f&&x.prisoned==0&&Board.Gap(x,f)<=sk.area).ToList();
            foreach(var a in inside.Where(x=>S.Ally(x.team,f.team))) if(sk.heal>0) Heal(a,sk.heal,f,sk.id);
            var log=new List<GameEvent>(); var hit=inside.Where(x=>!S.Ally(x.team,f.team)).ToList();
            foreach(var v in hit) if(sk.damage>0) { log.Add(new GameEvent{kind="dano-evento",actor=f.id,target=v.id,value=Ghost(v)?0:sk.damage,info=sk.id,cell=v.cell}); Hurt(v,sk.damage,log,f); }
            events.AddRange(log); striker=f;
            foreach(var v in hit) if(!v.Alive) KO(v);
            striker=null;
        }
        // L.E.L.A Pulse: cada passiva da forma atual que pega aliados vira, em quem ela pega agora, o efeito × amplify (cura na
        // hora) e um buff com o mesmo valor que dura até o fim do próximo turno do aliado (PA/PM entram no começo dele; a cura repete).
        void Pulse(Fighter f,SkillDef sk)
        {
            int k=Math.Max(1,sk.amplify);
            Emit(new GameEvent{kind="pulso",actor=f.id,info=f.form,cell=f.cell,value=k});
            foreach(var p in Content.Def(f).passives??new PassiveDef[0])
            {
                if(p==null||p.who==PassiveWho.Si||p.who==PassiveWho.Inimigos) continue;
                foreach(var v in S.fighters.Where(x=>x.Alive&&x.prisoned==0&&S.Ally(x.team,f.team)&&PassiveHits(f,p,x)).ToList())
                {
                    int a=p.amount*k; var b=new Buff{id=sk.id,from=PulseTag+f.id,turns=2};
                    switch(p.stat)
                    {
                        case PassiveStat.Dano: b.dmg=a; break;
                        case PassiveStat.Armadura: b.armor=a; break;
                        case PassiveStat.PA: b.pa=a; break;
                        case PassiveStat.PM: b.pm=a; break;
                        case PassiveStat.Alcance: b.range=a; break;
                        case PassiveStat.CuraPorTurno: Heal(v,a,f,sk.id); b.regen=a; break;
                        default: continue;
                    }
                    AddBuff(v,b); Emit(new GameEvent{kind="buff",actor=f.id,target=v.id,info=sk.id,value=b.armor,cell=v.cell});
                }
            }
        }
        // Nulification Field: maior perda de PA entre os campos inimigos vivos (e fora do buraco negro) que pegam f.
        int FieldDrain(Fighter f) => S.fighters.Where(o=>o.Alive&&o.prisoned==0&&!S.Ally(o.team,f.team)&&Content.Def(o).fieldPa>0
            &&Board.Footprint(f).Any(c=>Math.Max(Math.Abs(c.x-o.cell.x),Math.Abs(c.z-o.cell.z))<=Content.Def(o).fieldRange)).Select(o=>Content.Def(o).fieldPa).DefaultIfEmpty(0).Max();
        void Field(Fighter f)
        {
            int want=FormImmune(f)?0:FieldDrain(f);
            if(want>0&&f.drained==0) { int d=Math.Min(f.pa,want); if(d>0) { f.pa-=d; f.drained=d; Emit(new GameEvent{kind="anulacao",target=f.id,value=d,cell=f.cell}); } }
            else if(want==0&&f.drained>0) { f.pa+=f.drained; Emit(new GameEvent{kind="anulacao-fim",target=f.id,value=f.drained,cell=f.cell}); f.drained=0; }
        }
        // Nulification: volta à forma inicial, perde buffs bons, canalização, Desejo e aura.
        void Dispel(Fighter by,Fighter v)
        {
            var d=Content.Def(v);
            if(!string.IsNullOrEmpty(d.startForm)&&v.form!=d.startForm&&!(Content.Form(v)?.locked??false)) SetForm(v,d.startForm,0,"nulification");
            Purge(v);
            v.channeling=null; v.channel=0; v.wishReady=false; v.usedForm.Clear();
            if(v.auraOn) { v.auraOn=false; Emit(new GameEvent{kind="aura",actor=v.id,info="desligar",cell=v.cell}); }
            Emit(new GameEvent{kind="anular",actor=by.id,target=v.id,cell=v.cell});
        }
        // Buff bom/ruim: o espelho inverte os números, então cada atributo conta dos dois lados (alcance, usos por turno, cura
        // por turno, reflexo e imunidade). Resistência (takenDivisor>1) é boa; vulnerabilidade (taken>1) é ruim.
        static bool Good(Buff b) => b.pa>0||b.pm>0||b.armor>0||b.range>0||b.extra>0||b.dmg>0||b.regen>0||b.reflect>0||b.immune>0||b.takenDivisor>1;
        static bool Bad(Buff b) => b.pa<0||b.pm<0||b.armor<0||b.dmg<0||b.range<0||b.extra<0||b.regen<0||b.reflect<0||b.immune<0||b.taken>1||b.id==NoHealBuff;
        static void Purge(Fighter v) => v.buffs.RemoveAll(Good);
        // Espelho purificador no aliado: tira debuffs, corte da cura, atordoamento e Marcas da Maldição.
        void Cleanse(Fighter by,Fighter v)
        {
            ClearInversion(v); v.buffs.RemoveAll(Bad); v.stunned=0; v.marks=0;
            Emit(new GameEvent{kind="purificar",actor=by.id,target=v.id,cell=v.cell});
        }
        // Selar ultimate: as ultimates do alvo entram em recarga e a transformação de ultimate cai (Fúria não cai).
        void Seal(Fighter by,Fighter v,int turns)
        {
            var d=Content.Def(v);
            foreach(var sk in d.skills.Where(x=>x.ultimate)) { var w=v.cooldowns.Find(x=>x.skill==sk.id); if(w==null) v.cooldowns.Add(new Charge{skill=sk.id,uses=turns}); else w.uses=Math.Max(w.uses,turns); }
            if(!string.IsNullOrEmpty(d.startForm)&&v.form!=d.startForm&&!(Content.Form(v)?.locked??false)&&d.skills.Any(x=>x.ultimate&&x.kind==SkillKind.Forma&&x.toForm==v.form))
                SetForm(v,d.startForm,0,"selar-ultimate");
            Emit(new GameEvent{kind="selar",actor=by.id,target=v.id,value=turns,cell=v.cell});
        }
        // Cheiro de Sangue: o destino fica mais perto de um inimigo visível com PV ≤ 50%.
        public static bool Scent(MatchState s,Fighter f,Cell to) => Content.Def(f).bloodScent&&s.fighters.Any(v=>v.Alive&&!s.Ally(v.team,f.team)&&!Concealed(s,f.team,v)
            &&Board.Contains(s,v.cell)&&v.hp*2<=Content.MaxHp(v)&&Board.Gap(v,to)<Board.Gap(v,f));
        // Suporte: aliados no alvo/área (ou em volta de si) curam e ganham o buff; pode invocar (Noite dos Gatos).
        void Support(Fighter f,SkillDef sk,Cell c)
        {
            var center=sk.selfArea?f.cell:c; var centers=sk.selfArea?Board.Footprint(f).ToList():new List<Cell>{c}; var list=new List<Fighter>();
            bool Blast(Fighter x) => centers.Any(o=>Board.Footprint(x).Any(xc=>InArea(o,xc,sk.area,sk.square)));
            bool Takes(Fighter x) => !TechProtected(x)&&!Unbreakable(x)&&(sk.Custom?Hits(f,sk,x):S.Ally(x.team,f.team));
            if(!sk.selfArea&&Board.At(S,c) is Fighter main&&Takes(main)&&main.prisoned==0) list.Add(main);
            if(sk.selfArea||sk.area>0) list.AddRange(S.fighters.Where(x=>x.Alive&&!list.Contains(x)&&Takes(x)&&x.prisoned==0&&Blast(x)));
            Emit(new GameEvent{kind="suporte",actor=f.id,info=sk.id,cell=center,value=sk.area});
            foreach(var v in list)
            {
                if(sk.heal>0) Heal(v,sk.heal,f,sk.id);
                if(sk.buffPa!=0||sk.buffPm!=0||sk.buffArmor!=0||sk.buffRange!=0||sk.buffReflect||sk.buffImmune)
                {
                    AddBuff(v,new Buff{id=sk.id,turns=Math.Max(1,sk.turns),pa=sk.buffPa,pm=sk.buffPm,armor=sk.buffArmor,range=sk.buffRange,reflect=sk.buffReflect?1:0,immune=sk.buffImmune?1:0});
                    Emit(new GameEvent{kind="buff",actor=f.id,target=v.id,info=sk.id,value=sk.buffArmor,cell=v.cell});
                }
                // Espelho primordial: o alvo fica com os buffs de cabeça para baixo por N turnos dele (fim em Advance).
                if(sk.invertTurns>0) ApplyInversion(v,sk.invertTurns,f);
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
        void Order(Fighter f,SkillDef sk,Cell c,Cell cat)
        {
            if(!FindUnit(f,sk,c,cat,out var u,out _)&&!FindUnit(f,sk,c,null,out u,out _)) return;
            if(sk.step>0)
            {
                var path=Board.Path(S,u,c,sk.step); var walked=new List<Cell>{u.cell};
                foreach(var step in path.Skip(1)) { if(Board.At(S,step)!=null) break; u.cell=step; walked.Add(step); }
                if(walked.Count>1) Emit(new GameEvent{kind="mover",actor=u.id,info=sk.id,path=walked.ToArray()});
                if(Reveals(u)) foreach(var h in S.fighters.Where(x=>x.Alive&&x.hidden&&!S.Ally(x.team,u.team)&&Board.Gap(x,u)==1).ToList()) { h.hidden=false; Emit(new GameEvent{kind="revelar",actor=u.id,target=h.id,cell=h.cell}); }
                Collect(u);
                return;
            }
            var zap=Content.Def(u).skills[0];
            Emit(new GameEvent{kind="comando",actor=f.id,target=u.id,info=zap.id,cell=c});
            if(zap.kind==SkillKind.Dano) { f.lastAttack=zap.id; var hit=Intercept(u,c); Strike(u,zap,hit,hit!=c,c); }
            else if(zap.kind==SkillKind.Cura&&Board.At(S,c) is Fighter v) Heal(v,zap.damage,u,zap.id);
            if(u.Alive) { u.hp=0; u.deathProcessed=true; Emit(new GameEvent{kind="sumir",target=u.id,cell=u.cell}); AnyDeath(u); }
        }
        void Revive(Fighter by,Fighter v,Cell c,int hp,string info)
        {
            if(v==null) return;
            v.hp=Math.Max(1,Math.Min(hp,Content.MaxHp(v))); v.deathProcessed=false; v.cell=c; v.prisoned=0; v.channel=0; v.wishReady=false; v.hidden=false;
            // Reviver devolve os Galeões do nocaute (como o renascer do Devon): sem isso o próximo nocaute do time zerava os
            // Galeões e eliminava o time inteiro de uma vez (CheckVictory).
            if(v.summoner==0&&MatchState.Player(v.team)) S.galeoes[S.Team(v.team)]+=Content.Def(v).level;
            Settle(v);
            Emit(new GameEvent{kind="reviver",actor=by.id,target=v.id,value=v.hp,info=info,cell=v.cell});
            Collect(v);
        }
        // Desejo: o 1º uso começa a canalizar (2 turnos); no turno seguinte ao fim, uma das opções a–f.
        void Wish(Fighter f,SkillDef sk,Cell c,int slot)
        {
            if(string.IsNullOrEmpty(sk.wish)) { f.channel=2; f.pa=0; f.pm=0; Emit(new GameEvent{kind="desejo",actor=f.id,info="",cell=f.cell}); return; }
            f.wishReady=false; var v=Board.At(S,c);
            Emit(new GameEvent{kind="desejo",actor=f.id,target=v?.id??0,info=sk.wish,cell=c});
            if(v==null&&sk.wish!="a"&&sk.wish!="f") return;
            if(v!=null&&v.prisoned>0) return;
            switch(sk.wish)
            {
                case "a": var dead=DeadAlly(f,slot); if(dead!=null&&Board.At(S,c)==null) Revive(f,dead,c,(Content.MaxHp(dead)+1)/2,sk.id); break;
                case "b": Heal(v,Content.MaxHp(v),f,sk.id); break;
                case "c":
                    if(!S.Ally(v.team,f.team)) break;
                    AddBuff(v,new Buff{id="desejo",turns=2,pa=Content.Def(v).pa+v.bonusPa,extra=1});
                    Emit(new GameEvent{kind="buff",actor=f.id,target=v.id,info=sk.id,cell=v.cell});
                    break;
                case "d":
                {
                    var options=Copyables(f,v); if(slot<0||slot>=options.Count) break;
                    var copy=options[slot]; f.learned.Add(copy.id);
                    Emit(new GameEvent{kind="aprender",actor=f.id,target=v.id,info=copy.id,cell=f.cell});
                    break;
                }
                case "e":
                {
                    if(!S.Ally(v.team,f.team)) break;
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
            if(Board.Gap(f,c)<=1) return c;
            foreach(var cell in Board.Between(S,Board.Nearest(f,c),c))
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
        void Strike(Fighter f,SkillDef sk,Cell c,bool intercepted=false,Cell? aim=null)
        {
            Face a=Roll(), d=Roll();
            var main=sk.selfArea?null:Board.At(S,c);
            // Bolinhas (3.1): com hits marcado, alvo e área seguem só as bolinhas (🔵 pode acertar e nocautear quem usa).
            if(main!=null&&sk.Custom&&!intercepted&&(main.prisoned>0||!Hits(f,sk,main))) main=null;
            if(main!=null&&!sk.Custom&&(main.prisoned>0||(!intercepted&&S.Ally(main.team,f.team))||(sk.foes&&S.Ally(main.team,f.team)))) main=null;
            bool missed=sk.wild&&(a==Face.Esquiva||a==Face.CoringaEsquivaTrava);
            bool ambush=f.hidden;
            int bonus=S.fighters.Where(g=>g.Alive&&g.id!=f.id&&S.Ally(g.team,f.team)&&Content.Def(g).allyBonus>0&&Board.Gap(g,f)<=Content.Def(g).auraRange).Select(g=>Content.Def(g).allyBonus).DefaultIfEmpty(0).Max();
            bonus+=Passive(f,PassiveStat.Dano)+f.buffs.Sum(b=>b.dmg)+EventDamage(f);
            if(ambush) { bonus++; f.hidden=false; Emit(new GameEvent{kind="revelar",target=f.id,cell=f.cell}); }
            if(f.focused&&!f.focusUsed&&sk.damage>0) { bonus++; f.focusUsed=true; }
            var targets=new List<Fighter>(); if(main!=null) targets.Add(main);
            var fd=Content.Def(f); int heals=0;
            int baseDamage=sk.damage+(sk.maxDamage>sk.damage?Dice.Range(S,sk.maxDamage-sk.damage+1):0);
            var centers=sk.selfArea?Board.Footprint(f).ToList():new List<Cell>{c};
            // A explosão não atravessa muro (autor, 18/10 noite).
            bool Blast(Fighter x) => centers.Any(o=>Board.Footprint(x).Any(xc=>InArea(o,xc,sk.area,sk.square)&&Board.WallClear(S,o,xc)));
            // Área acerta aliados também (decisão do autor, 10/10), salvo foes; quem ataca nunca se acerta; preso não é alcançado.
            if(sk.area>0&&sk.Custom) targets.AddRange(S.fighters.Where(x=>x.Alive&&x!=main&&x.prisoned==0&&Blast(x)&&Hits(f,sk,x)));
            else if(sk.area>0) targets.AddRange(S.fighters.Where(x=>x.Alive&&x!=main&&x!=f&&x.prisoned==0&&Blast(x)&&(!sk.foes||!S.Ally(x.team,f.team))));
            var first=targets.FirstOrDefault();
            var log=new List<GameEvent>(); int dealt=0; striker=f;
            GearHits(f,centers.SelectMany(o=>sk.area>0?Area(o,sk.area,sk.square):new List<Cell>{o}),log);
            // Golpe em volta de si: a apresentação anima a conjuração mesmo sem ninguém na área.
            if(sk.selfArea) Emit(new GameEvent{kind="area",actor=f.id,info=sk.id,cell=f.cell,value=sk.area,dice=new[]{a,d}});
            else if(targets.Count==0) Emit(new GameEvent{kind="ataque",actor=f.id,target=0,value=-1,info=sk.id,cell=c,aim=aim??c,dice=new[]{a,d}});
            // Nulification: anula antes do dano (a forma fantasma também cai e o golpe entra). Efeito hostil: quem é imune a
            // efeitos (forma ou Chá protetor) não é anulado, mas leva o dano da skill normalmente.
            if(sk.dispel&&!missed) foreach(var v in targets.Where(x=>!Immune(x))) Dispel(f,v);
            foreach(var v in targets)
            {
                // Imune a golpes de longe (Espadachim) e esquiva na sombra (Ellie: golpe colado erra com shadowDodge %).
                var vd=Content.Def(v); int gap=Board.Gap(f,v);
                // Exposta (Ellie): de longe não erra nem bloqueia; males e atordoamento entram mesmo com imunidade ou imparável.
                bool bare=vd.exposed&&f!=v, far=bare&&gap>1, miss=missed&&!far;
                bool ghost=Unbreakable(v)||Ghost(v)||vd.farImmune>0&&gap>=vd.farImmune&&f!=v
                    ||vd.shadowDodge>0&&gap<=1&&f!=v&&Board.Footprint(v).Any(xc=>S.At(xc).shadow!=0)&&Dice.Range(S,100)<vd.shadowDodge;
                bool shield=Dice.Armor(d)&&!sk.precise&&!sk.pierce&&!far;
                SphereGuard(v);
                int alone=sk.isolatedBonus>0&&Isolated(S,v)?sk.isolatedBonus:0;
                // Cat-a-Clisma: +2 a cada 2 marcas do alvo principal. Yami-yami: +meleeWeak de golpe colado.
                int curse=sk.consumeMarks&&v==main?v.marks/2*2:0, weak=Board.Gap(f,v)<=1?Content.Def(v).meleeWeak:0;
                int power=baseDamage+curse;
                int bond=!miss&&!ghost&&power>0?TwinBond(f,v,sk):0;
                var directional=sk.pierce||sk.ignoreArmor?null:DirectionalGuardFor(f,v);
                int guardArmor=directional==null?0:Math.Max(0,GuardDefinition(directional)?.buffArmor??1);
                // Sombra (Ellie): alvo numa casa de sombra. Duelo (Espadachim): ninguém mais perto dele nem do alvo.
                int shade=fd.shadowBonus>0&&Board.Footprint(v).Any(xc=>S.At(xc).shadow!=0)?fd.shadowBonus:0;
                int duel=fd.duelBonus>0&&v!=f&&!S.fighters.Any(x=>x.Alive&&x!=f&&x!=v&&(Board.Gap(x,f)<=fd.duelRange||Board.Gap(x,v)<=fd.duelRange))?fd.duelBonus:0;
                int dmg=miss||ghost||power<=0?0:Math.Max(0,power+bonus+alone+weak+shade+duel+bond-(shield?1:0)-(sk.pierce||sk.ignoreArmor?0:Armor(v)+guardArmor));
                // Explosão do chá: o alvo leva o dobro (multiplicadores somam por buff). Resistência divide o dano recebido.
                foreach(var b in v.buffs) { if(b.taken>1) dmg*=b.taken; if(b.takenDivisor>1&&dmg>0) dmg=Math.Max(0,dmg/b.takenDivisor); }
                // Pistoleira (summonSlayer): qualquer dano numa invocação inimiga mata na hora (nem a guarda segura).
                bool slay=SlaysSummon(f,v,dmg);
                if(slay) dmg=Math.Max(dmg,v.hp);
                bool wounded=!S.Ally(v.team,f.team)&&v.hp*2<=Content.MaxHp(v);
                Emit(new GameEvent{kind="ataque",actor=f.id,target=v.id,value=miss||ghost?-1:dmg,info=sk.id,cell=v.cell,aim=aim??c,dice=v==first&&!sk.selfArea?new[]{a,d}:null});
                if(directional!=null&&!miss&&!ghost&&power+bonus+alone+weak+shade+duel+bond-(shield?1:0)>Armor(v))
                    log.Add(new GameEvent{kind="protecao-direcional",actor=directional.id,target=v.id,cell=v.cell,value=guardArmor});
                Hurt(v,dmg,log,f); dealt+=dmg;
                bool hit=!miss&&!ghost&&(dmg>0||power<=0);
                // Resistência a efeito ruim/controle: a forma imune a efeitos (Berserk) nunca é furada, nem pela Exposta (Ellie);
                // o buff de imunidade (Chá protetor) ainda cede para a Exposta. guarded = só os efeitos hostis (foe).
                bool foe=!S.Ally(v.team,f.team);
                bool resist=Immune(v)&&(!bare||FormImmune(v)), guarded=resist&&foe;
                // Death Crimson: golpe cheio (sem o Escudo do defensor) cura.
                if(hit&&dmg>0&&fd.fullHitHeal>0&&!shield) heals+=fd.fullHitHeal;
                // Espelho de retorno: o golpe que não derrubou volta em quem bateu.
                if(dmg>0&&v.Alive&&v!=f&&f.Alive&&v.buffs.Find(b=>b.reflect>0) is Buff mirror)
                { v.buffs.Remove(mirror); log.Add(new GameEvent{kind="refletir",actor=v.id,target=f.id,value=dmg,cell=f.cell}); Hurt(f,dmg,log,v); }
                // Reflexo invertido: o próximo golpe que o portador causar volta contra ele.
                if(dmg>0&&f.Alive&&f.buffs.Find(b=>b.reflect<0) is Buff curseMirror) { f.buffs.Remove(curseMirror); log.Add(new GameEvent{kind="refletir",actor=v.id,target=f.id,value=dmg,cell=f.cell}); Hurt(f,dmg,log,v,false); }
                // Troca de vida hostil: efeito ruim, bloqueado por imunidade a efeitos (o dano da skill continua valendo).
                if(hit&&sk.swapHp&&v==main&&v!=f&&v.Alive&&f.Alive&&!guarded)
                {
                    int mine=f.hp, theirs=v.hp; f.hp=Math.Max(1,Math.Min(theirs,Content.MaxHp(f))); v.hp=Math.Max(1,Math.Min(mine,Content.MaxHp(v)));
                    log.Add(new GameEvent{kind="troca-vida",actor=f.id,target=v.id,value=v.hp,cell=v.cell});
                }
                if(hit&&sk.purge&&foe&&!guarded) { Purge(v); log.Add(new GameEvent{kind="anular",actor=f.id,target=v.id,cell=v.cell,info=sk.id}); }
                if(hit&&sk.cleanse&&!foe) Cleanse(f,v);
                if(hit&&sk.sealUlt>0&&foe&&!guarded) Seal(f,v,sk.sealUlt);
                if(hit&&sk.doubleDamage>0&&foe&&!guarded) { AddBuff(v,new Buff{id="vulneravel",from=sk.id,turns=sk.doubleDamage,taken=2}); log.Add(new GameEvent{kind="vulneravel",actor=f.id,target=v.id,value=sk.doubleDamage,cell=v.cell}); }
                if(hit&&sk.stun>0&&foe&&!guarded&&(bare||!Unstoppable(v))) { v.stunned=Math.Max(v.stunned,sk.stun); log.Add(new GameEvent{kind="atordoar",actor=f.id,target=v.id,value=sk.stun,cell=v.cell}); }
                if(hit&&sk.consumeMarks&&v==main&&v.marks>0) { log.Add(new GameEvent{kind="marca",actor=f.id,target=v.id,value=-v.marks,cell=v.cell}); v.marks=0; }
                if(hit&&sk.mark>0&&foe&&!guarded) { v.marks+=sk.mark; log.Add(new GameEvent{kind="marca",actor=f.id,target=v.id,value=v.marks,cell=v.cell}); }
                if(hit&&sk.noHeal&&foe&&!guarded&&!NoHeal(v)) { AddBuff(v,new Buff{id=NoHealBuff,from=f.id.ToString(),turns=99}); log.Add(new GameEvent{kind="sem-cura",actor=f.id,target=v.id,cell=v.cell}); }
                // Bloodlust (Daemion): acertar inimigo que já estava com PV ≤ 50% dá PM.
                if(dmg>0&&wounded&&Content.Def(f).bloodlust>0&&f.Alive) { f.pm+=Content.Def(f).bloodlust; log.Add(new GameEvent{kind="sede",actor=f.id,target=v.id,value=Content.Def(f).bloodlust,cell=f.cell}); }
                // Perda de PM/PA é efeito ruim: a imunidade a efeitos segura (o dano da skill continua valendo).
                if(hit&&sk.pmLoss>0&&!resist) AddBuff(v,new Buff{id="confusao",from=sk.id,turns=2,pm=-sk.pmLoss});
                if(hit&&sk.paLoss>0&&!resist) AddBuff(v,new Buff{id="atordoado",from=sk.id,turns=2,pa=-sk.paLoss});
                // Roubo de moedas também é efeito hostil.
                if(dmg>0&&v==main&&!S.Ally(v.team,f.team)&&!resist&&Content.Def(f).steal>0&&MatchState.Player(v.team)&&MatchState.Player(f.team)&&S.coins[v.team]>0&&Dice.Range(S,100)<Content.Def(f).steal*(ambush?2:1))
                {
                    S.coins[v.team]--; S.coins[f.team]++;
                    log.Add(new GameEvent{kind="roubo",actor=f.id,target=v.id,value=1,cell=v.cell});
                }
                // Manto de espinhos: quem acerta colado leva o troco.
                if(dmg>0&&v.thorns>0&&f.Alive&&Board.Gap(f,v)==1) { log.Add(new GameEvent{kind="espinhos",actor=v.id,target=f.id,value=v.thorns,cell=f.cell}); Hurt(f,v.thorns,log,v); }
                // Prisão: o alvo escapa com Esquiva (ou coringa) no dado de defesa; a imunidade a efeitos e a forma imparável
                // seguram (a Exposta só fura imunidade de buff, nunca a da forma).
                if(hit&&v==main&&sk.prison>0&&!resist&&(bare||!Unstoppable(v)))
                {
                    // Black Hole: some no buraco negro (sem Esquiva); o inimigo leva returnDamage ao voltar, o aliado volta ileso.
                    if(sk.banish) { v.prisoned=sk.prison; v.banished=true; v.banishBy=f.id; v.banishDamage=S.Ally(v.team,f.team)?0:sk.returnDamage; v.channeling=null; v.hidden=false;
                        log.Add(new GameEvent{kind="prisao",actor=f.id,target=v.id,value=sk.prison,cell=v.cell,info=sk.id}); }
                    else if(Dice.Dodge(d)&&!bare) log.Add(new GameEvent{kind="escapou",target=v.id,cell=v.cell,info=sk.id});
                    else { v.prisoned=sk.prison; v.channeling=null; log.Add(new GameEvent{kind="prisao",actor=f.id,target=v.id,value=sk.prison,cell=v.cell,info=sk.id}); }
                }
            }
            // Dreno da skill e roubo de vida da ficha (Daemion: todo ataque que fere cura).
            int drain=sk.drain+(dealt>0?Content.Def(f).lifesteal:0)+heals;
            if(drain>0&&dealt>0&&f.Alive&&!NoHeal(f)) { int heal=Math.Min(InvertedHealing(f,drain),Content.MaxHp(f)-f.hp); if(heal>0) { f.hp+=heal; log.Add(new GameEvent{kind="cura",actor=f.id,target=f.id,value=heal,info=sk.id,cell=f.cell}); } }
            // Explosão do chá: quem usa cai assim que o golpe resolve, antes de qualquer outro efeito.
            if(sk.selfKill&&f.Alive) { log.Add(new GameEvent{kind="autodestruir",actor=f.id,target=f.id,value=f.hp,info=sk.id,cell=f.cell}); f.hp=0; }
            events.AddRange(log);
            foreach(var v in targets) if(!v.Alive) KO(v);
            if(!f.Alive) KO(f);
            // Empurrão e puxão depois do dano, em quem continua de pé.
            if(!missed) foreach(var v in targets.Where(x=>x.Alive&&x.prisoned==0&&!(Ghost(x)&&!S.Ally(x.team,f.team))&&!(Immune(x)&&!S.Ally(x.team,f.team))))
            {
                if(sk.push>0) { var dir=Dir(v==main||sk.selfArea?f.cell:c,v.cell); if(dir==default) dir=Dir(f.cell,v.cell); Push(v,dir,sk.push,f); }
                if(sk.pull&&(v==main||sk.selfArea)) Pull(v,f);
            }
            striker=null;
            // Decisão B: Cataclisma só sai no dado de ataque.
            if(a==Face.Cataclisma) Cataclysm();
        }
        // Colosso: qualquer morte definitiva no mapa notifica todos os Colossos vivos.
        void Devour(Fighter f,Fighter v,List<GameEvent> log=null)
        {
            if(!f.Alive||f==v||!Content.Def(f).devour) return;
            var sk=Content.Def(v).skills.FirstOrDefault(s=>s.kind==SkillKind.Dano&&!s.ultimate&&s.damage>0);
            if(sk==null||f.stolen.Contains(sk.id)||Array.Exists(Content.Def(f).skills,s=>s.id==sk.id)) return;
            f.stolen.Add(sk.id); if(f.stolen.Count>2) f.stolen.RemoveAt(0);
            f.stolenPowers.Add(new SkillRoll{id=sk.id,damage=sk.damage,range=sk.maxRange});
            f.stolenPowers.RemoveAll(p=>!f.stolen.Contains(p.id));
            (log??events).Add(new GameEvent{kind="devorar",actor=f.id,target=v.id,info=Content.Stolen+sk.id,cell=f.cell});
            if(f.def=="colosso") (log??events).Add(new GameEvent{kind="voz-global",actor=f.id,info="primal_cacador",cell=Cell.Unknown,aim=Cell.Unknown});
        }
        // Direção dominante (eixo com maior diferença) de a para b.
        static Cell Dir(Cell a,Cell b)
        {
            int dx=b.x-a.x, dz=b.z-a.z; if(dx==0&&dz==0) return default;
            return Math.Abs(dx)>=Math.Abs(dz)?new Cell(Math.Sign(dx),0):new Cell(0,Math.Sign(dz));
        }
        bool Fixed(Fighter v) => TechProtected(v)||Unbreakable(v)||Content.Def(v).immovable||v.anchored||v.prisoned>0;
        // Empurrão: anda n casas na direção; parede, relevo de 2+, obstáculo ou lutador param: +1 de dano nele e 1 no atingido.
        void Push(Fighter v,Cell dir,int n,Fighter by,bool pull=false)
        {
            if(n<=0||dir==default||!v.Alive) return;
            if(Fixed(v)||FormImmune(v)&&(by==null||!S.Ally(by.team,v.team))) { Emit(new GameEvent{kind="firme",actor=by?.id??0,target=v.id,cell=v.cell}); return; }
            bool flying=Content.Def(v).flying, bump=false; var path=new List<Cell>{v.cell}; var log=new List<GameEvent>(); Fighter other=null; Portal warp=null;
            for(int i=0;i<n&&v.Alive;i++)
            {
                var next=v.cell+dir;
                if(!Board.Contains(S,next)||!Board.Walkable(S,next,flying)||(!flying&&S.At(next).height-S.At(v.cell).height>=2)) { bump=true; break; }
                other=Board.At(S,next); if(other!=null) { bump=true; break; }
                v.cell=next; path.Add(next);
                var t=S.At(next);
                if(t.coins>0&&MatchState.Player(v.team)) { S.coins[v.team]+=t.coins; S.collected[S.Team(v.team)]+=t.coins; log.Add(new GameEvent{kind="moeda",actor=v.id,cell=next,value=t.coins}); t.coins=0; }
                if(t.lava&&!flying) Burn(v,log);
                Enter(v,t,flying,log);
                if(t.trap!=0&&!S.Ally(t.trap,v.team)&&!flying) { if(Spring(v,t,log)) break; }
                if(PortalIn(next) is Portal g) { warp=g; break; }
            }
            Emit(new GameEvent{kind=pull?"puxao":"empurrao",actor=by?.id??0,target=v.id,path=path.ToArray(),cell=v.cell,value=bump?1:0});
            if(warp!=null&&v.Alive) { events.AddRange(log); log.Clear(); Warp(v,warp); }
            if(bump&&!pull&&v.Alive)
            {
                log.Add(new GameEvent{kind="colisao",actor=other?.id??0,target=v.id,cell=v.cell,value=1});
                Hurt(v,1,log,by); if(other!=null) Hurt(other,1,log,by);
            }
            events.AddRange(log);
            if(!v.Alive) KO(v);
            if(other!=null&&!other.Alive) KO(other);
        }
        void Pull(Fighter v,Fighter by) => Push(v,Dir(v.cell,by.cell),Board.Gap(v,by)-1,by,true);
        void SetForm(Fighter f,string form,int turns,string info)
        {
            f.directionalGuard=false;
            var before=Content.Form(f); f.form=form; f.formTurns=turns; var after=Content.Form(f); f.usedForm.Clear();
            // Bônus de PA/PM da forma nova valem já neste turno; os da antiga saem.
            f.pa=Math.Max(0,f.pa-(before?.pa??0)+(after?.pa??0)); f.pm=Math.Max(0,f.pm-(before?.pm??0)+(after?.pm??0));
            // Forma imune a efeitos (Berserk): ao entrar, a inversão acaba e os buffs ruins e status negativos saem na hora.
            if(after!=null&&after.effectImmune) { ClearInversion(f); f.buffs.RemoveAll(Bad); f.stunned=0; f.marks=0; }
            // A forma pode mudar o PM máximo: as cópias da original recalculam o disponível sem devolver o andado.
            SyncCopies(f);
            Emit(new GameEvent{kind="forma",actor=f.id,info=form,value=turns,cell=f.cell});
        }
        // Moeda na casa onde o lutador chega sem andar (teleporte, salto).
        void Collect(Fighter f)
        {
            CollectGears(f,f.cell,null);
            var t=S.At(f.cell);
            if(t.coins>0) { S.coins[f.team]+=t.coins; S.collected[S.Team(f.team)]+=t.coins; Emit(new GameEvent{kind="moeda",actor=f.id,cell=f.cell,value=t.coins}); t.coins=0; }
            if(t.Obj==TileObj.Bau) { var log=new List<GameEvent>(); Enter(f,t,true,log); events.AddRange(log); }
        }
        // Objetos fixos do mapa (TileObj, 18/10): o baú abre para quem entra; os espetos ferem quem entra sem voar e não param ninguém.
        void Enter(Fighter f,Tile t,bool flying,List<GameEvent> log)
        {
            if(t.Obj==TileObj.Bau&&MatchState.Player(f.team)) {
                t.obj=0; S.coins[f.team]+=MapGen.BauMoedas; S.collected[S.Team(f.team)]+=MapGen.BauMoedas;
                log.Add(new GameEvent{kind="bau",actor=f.id,cell=f.cell,value=MapGen.BauMoedas});
            }
            // Labirinto: os espetos ficam escondidos no chão até alguém pisar; aí saem e ficam à vista (MatchState.sprung).
            if(t.Obj==TileObj.Espetos&&!flying) {
                int at=Array.IndexOf(S.tiles,t); if(at>=0&&!S.sprung.Contains(at)) S.sprung.Add(at);
                int dmg=Ghost(f)?0:1; log.Add(new GameEvent{kind="espetos",target=f.id,cell=at>=0?new Cell(at%S.width,at/S.width):f.cell,value=dmg}); Hurt(f,dmg,log);
            }
        }
        // Braseiro colado (lado, sem diagonal) na casa.
        bool NextToBrazier(Cell c) => Board.Dirs.Any(d=>Board.Contains(S,c+d)&&S.At(c+d).Obj==TileObj.Braseiro);
        // Salto: casa livre vizinha ao alvo, a mais perto de quem salta (empate: a primeira de Board.Dirs).
        bool Landing(Fighter f,Cell target,out Cell land,bool planning=false)
        {
            land=default;
            var options=Board.Dirs.Select(d=>target+d).Where(x=>Board.Contains(S,x)&&Board.Walkable(S,x,Content.Def(f).flying)&&((planning?Board.KnownAt(S,x,f):Board.At(S,x))==null||x==f.cell)&&(Board.SizeOf(f)==1||Board.Fits(S,f,x))).ToList();
            if(options.Count==0) return false;
            land=options.OrderBy(x=>x.Distance(f.cell)).First(); return true;
        }
        // Cobertura para se esconder: casa vizinha mais alta, árvore ou caixa.
        bool Cover(Cell c) => Board.Dirs.Select(d=>c+d).Any(x=>Board.Contains(S,x)&&(S.At(x).height>S.At(c).height||S.At(x).terrain==Terrain.Arvore||S.At(x).terrain==Terrain.Caixa||S.At(x).terrain==Terrain.Loja||S.At(x).terrain==Terrain.Parede));
        void KO(Fighter v,List<GameEvent> log=null)
        {
            if(v.deathProcessed) return;
            var add=log??events;
            // Devon: o primeiro nocaute vira renascimento, sem perder Galeões.
            if(Content.Def(v).revive>0&&!v.revived&&!S.eliminated[S.Team(v.team)])
            {
                v.revived=true; v.hp=Math.Min(Content.Def(v).revive,Content.MaxHp(v));
                add.Add(new GameEvent{kind="nocaute",target=v.id,cell=v.cell,info="renasce"});
                add.Add(new GameEvent{kind="renascer",target=v.id,value=v.hp,cell=v.cell});
                return;
            }
            v.directionalGuard=false; v.deathProcessed=true; v.hp=0; v.hidden=false; v.channel=0; v.wishReady=false; v.prisoned=0; v.banished=false; v.stunned=0; v.channeling=null; ClosePortals(v.id); add.Add(new GameEvent{kind="nocaute",target=v.id,cell=v.cell});
            // Chefe derrotado pelo golpe final de um jogador: vira aliado dele na mesma partida em vez de sair do jogo
            // (Boss.cs). Quem conta é a fonte REAL do último dano (lastDamageBy; 0 = ambiental → vale o striker só se houver).
            if(!v.recruitedBoss&&TryRecruitBoss(v,S.F(v.lastDamageBy),add)) return;
            AnyDeath(v,add);
            foreach(var t in S.tiles) if(t.shadow==v.id) t.shadow=0;
            // Cria Vampírica Ancestral: se divide em réplicas nas casas livres em volta.
            var vd=Content.Def(v);
            if(!string.IsNullOrEmpty(vd.split)&&vd.splitCount>0&&Content.Fighters.ContainsKey(vd.split))
            {
                var spots=Area(v.cell,2,true).Where(c=>c!=v.cell&&Board.Walkable(S,c,false)&&Board.At(S,c)==null&&!S.At(c).lava).OrderBy(c=>c.Distance(v.cell)).ThenBy(c=>c.z).ThenBy(c=>c.x).Take(vd.splitCount).ToList();
                foreach(var c in spots) { var r=Spawn(vd.split,v.team,c,v.summoner); if(!Content.Def(r).noTurn&&!Content.Def(r).neutral) S.timeline.Add(r.id); add.Add(new GameEvent{kind="invocar",actor=v.id,target=r.id,info=r.def,cell=c}); }
            }
            if(v.auraOn) { v.auraOn=false; add.Add(new GameEvent{kind="aura",actor=v.id,info="desligar",cell=v.cell}); }
            foreach(var t in S.tiles) if(t.frozenBy==v.id) t.frozenBy=0;
            if(v.summoner==0&&MatchState.Player(v.team)) { int t=S.Team(v.team); S.galeoes[t]=Math.Max(0,S.galeoes[t]-Content.Def(v).level); }
            LunaDeath(v,add);
            foreach(var s in S.fighters.Where(x=>x.Alive&&x.summoner==v.id&&!(Luna(x)?.survivesOwner??false)).ToList()) KO(s,log);
            // Neutros: recompensa para quem nocauteou (ou o dono da invocação); o chefe derrotado encerra o evento.
            if(Content.Def(v).neutral&&striker!=null)
            {
                var who=striker.summoner!=0?S.F(striker.summoner)??striker:striker;
                if(MatchState.Player(who.team))
                {
                    bool boss=v.def=="brutamontes-errante"; int coins=boss?5:Monster(v)?MonsterGold:v.carried+2; string item=null;
                    S.coins[who.team]+=coins;
                    if(boss) { var items=Shop.Catalog.Where(d=>d.kind==ShopKind.Item).ToArray(); var it=items[Dice.Range(S,items.Length)]; Grant(who,it); item=it.id; }
                    add.Add(new GameEvent{kind="recompensa",actor=who.id,target=v.id,value=coins,info=item,cell=v.cell});
                }
            }
            if(v.def=="brutamontes-errante"&&S.ev=="errante") EndEvent();
            if((S.ev=="chefe"||S.ev=="vampiro")&&BossKind(v.def)&&!S.fighters.Any(x=>x.Alive&&BossKind(x.def))) EndEvent();
        }
        // Chefes do evento e as réplicas deles.
        static bool BossKind(string def) => Content.Bosses.Contains(def)||def=="replica-vampirica";
        void AnyDeath(Fighter v,List<GameEvent> log=null)
        {
            foreach(var colossus in S.fighters.Where(x=>x.Alive&&Content.Def(x).devour).ToList()) Devour(colossus,v,log);
        }

        void BeginTurn(Fighter f)
        {
            if(f==null) return;
            ResetTwinBond(f);
            foreach(var b in f.buffs) b.turns--;
            f.buffs.RemoveAll(b=>b.turns<=0);
            // Forma com duração: conta os turnos do próprio lutador e volta à forma inicial.
            if(f.formTurns>0&&--f.formTurns==0) { f.directionalGuard=false; f.form=Content.Def(f).startForm; f.usedForm.Clear(); Emit(new GameEvent{kind="forma",actor=f.id,info=f.form,cell=f.cell}); }
            var d=Content.Def(f); var form=Content.Form(f);
            foreach(var w in f.cooldowns) w.uses--;
            f.cooldowns.RemoveAll(w=>w.uses<=0);
            f.pa=Math.Max(0,d.pa+f.bonusPa+f.buffs.Sum(b=>b.pa)+(form?.pa??0)+Passive(f,PassiveStat.PA)); f.pm=Math.Max(0,d.pm+f.bonusPm+f.buffs.Sum(b=>b.pm)+(form?.pm??0)+Passive(f,PassiveStat.PM)); f.used.Clear(); f.bought=false; f.focusUsed=false; f.remoteShop=false; f.endedTurn=false;
            LunaBeginTurn(f);
            // Ilusão: o PM máximo é o da original (com buffs, itens e forma) e o gasto do turno passado não volta.
            if(Copy(f)) { f.moveBudget=0; f.pm=CopyCap(f); }
            // A original mudou de PM máximo (buffs, forma, item): as cópias dela recalibram sem recarregar o que já andaram.
            SyncCopies(f);
            if(f.hidden) f.pm=Math.Min(f.pm,HiddenCap(f));
            f.drained=0;
            Shadows(f);
            // Chão de gelo dura até o próximo turno de quem congelou.
            foreach(var t in S.tiles) if(t.frozenBy==f.id) t.frozenBy=0;
            // Portal: conta os turnos do dono e fecha no fim.
            bool closing=false; foreach(var p in S.portals) if(p.by==f.id&&--p.turns<=0) closing=true;
            if(closing) { ClosePortals(f.id); Emit(new GameEvent{kind="portal-fechado",actor=f.id,cell=f.cell}); }
            Emit(new GameEvent{kind="turno",actor=f.id,value=S.round});
            // Braseiro: quem começa o turno colado nele ganha +1 PA neste turno.
            if(f.summoner==0&&Board.Footprint(f).Any(NextToBrazier)) { f.pa++; Emit(new GameEvent{kind="braseiro",actor=f.id,cell=f.cell,value=1}); }
            // Skill canalizada: resolve agora, de onde ele está; alvo que ficou inválido vira golpe no chão.
            if(!string.IsNullOrEmpty(f.channeling))
            {
                var parts=f.channeling.Split(';'); f.channeling=null; int N(int i) => parts.Length>i&&int.TryParse(parts[i],out int chNum)?chNum:0;
                var sk=Content.Skill(f,parts[0])??Content.AnySkill(parts[0]); var c=new Cell(N(1),N(2)); var c2=new Cell(N(3),N(4)); int slot=N(5);
                if(sk!=null) { if(Check(f,sk,c,true,out _,c2)) Resolve(f,sk,c,slot,c2); else GroundMiss(f,sk,c); CheckVictory(); if(S.over||!f.Alive) return; }
            }
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
                foreach(var a in S.fighters.Where(x=>x.Alive&&S.Ally(x.team,f.team)&&Board.Gap(x,t)<=2).ToList()) Heal(a,Content.Def(t).healAura,t,"totem-de-cura");
            // Lava: quem começa o turno nela queima.
            if(Board.Footprint(f).Any(c=>S.At(c).lava)&&!d.flying) { var log=new List<GameEvent>(); Burn(f,log); events.AddRange(log); if(!f.Alive) KO(f); }
            // Passivas de cura/dano por turno (editor), no começo do turno de quem é pego.
            if(!f.Alive) return;
            int regen=Passive(f,PassiveStat.CuraPorTurno), bleed=Passive(f,PassiveStat.DanoPorTurno);
            if(regen>0) Heal(f,regen,f,"passiva");
            // Pulso: cura por turno positiva cura; a negativa (o Pulso invertido pelo espelho) vira dano.
            int pulse=f.buffs.Sum(b=>b.regen);
            if(pulse>0) Heal(f,pulse,f,"lela-pulse");
            else if(pulse<0) { var log=new List<GameEvent>{ new GameEvent{kind="dano-evento",actor=f.id,target=f.id,value=-pulse,info="lela-pulse",cell=f.cell} }; Hurt(f,-pulse,log,f); events.AddRange(log); if(!f.Alive) KO(f); }
            if(bleed>0) { var source=S.fighters.FirstOrDefault(o=>o.Alive&&Content.Def(o).summonSlayer&&(Content.Def(o).passives??new PassiveDef[0]).Any(p=>p!=null&&p.stat==PassiveStat.DanoPorTurno&&p.amount>0&&PassiveHits(o,p,f)&&!Pulsed(o,f,PassiveStat.DanoPorTurno))); var log=new List<GameEvent>{ new GameEvent{kind="dano-evento",actor=f.id,target=f.id,value=bleed,info="passiva",cell=f.cell} }; Hurt(f,bleed,log,source); events.AddRange(log); if(!f.Alive) KO(f); }
            // Nulification Field: começou o turno na área, perde os PA na hora.
            if(f.Alive) Field(f);
        }
        // Sombras da Ellie: as do turno anterior somem e surgem shadowMin..shadowMax casas novas, sorteadas no mapa.
        void Shadows(Fighter f)
        {
            var d=Content.Def(f); if(d.shadowMax<=0) return;
            foreach(var t in S.tiles) if(t.shadow==f.id) t.shadow=0;
            var free=Enumerable.Range(0,S.tiles.Length).Where(i=>S.tiles[i].shadow==0&&Board.Walkable(S,new Cell(i%S.width,i/S.width),false)&&!S.tiles[i].lava).ToList();
            int n=Math.Min(free.Count,d.shadowMin+Dice.Range(S,d.shadowMax-d.shadowMin+1)); var cells=new List<Cell>();
            for(int i=0;i<n;i++) { int k=Dice.Range(S,free.Count); S.tiles[free[k]].shadow=f.id; cells.Add(new Cell(free[k]%S.width,free[k]/S.width)); free.RemoveAt(k); }
            Emit(new GameEvent{kind="sombras",actor=f.id,cell=f.cell,path=cells.ToArray(),value=n});
        }
        // Preso (ou no buraco negro) perde o turno; ao sair do Black Hole, o inimigo leva o dano da volta. Vale para neutros também.
        void Jailed(Fighter next)
        {
            next.prisoned--; Emit(new GameEvent{kind="preso",target=next.id,value=next.prisoned,cell=next.cell,info=next.banished?"black-hole":null});
            if(next.prisoned==0&&next.banished)
            {
                next.banished=false; var source=S.F(next.banishBy); int dmg=Ghost(next)?0:DamageFrom(source,next,next.banishDamage); next.banishBy=0; next.banishDamage=0; var log=new List<GameEvent>();
                Emit(new GameEvent{kind="retorno",target=next.id,value=dmg,cell=next.cell});
                Hurt(next,dmg,log,source); events.AddRange(log); if(!next.Alive) KO(next);
                CheckVictory();
            }
        }
        // Fúria (endBlast): no fim do turno, dano fixo nas 8 casas em volta, nele também.
        void EndBlast(Fighter f)
        {
            int dmg=Content.Form(f)?.endBlast??0; if(dmg<=0||!f.Alive) return;
            var hit=S.fighters.Where(x=>x.Alive&&x.prisoned==0&&(x==f||Board.Footprint(x).Any(xc=>Board.Footprint(f).Any(o=>Math.Max(Math.Abs(o.x-xc.x),Math.Abs(o.z-xc.z))<=1)))).ToList();
            Emit(new GameEvent{kind="explosao",actor=f.id,cell=f.cell,info="furia",value=1});
            var log=new List<GameEvent>(); striker=f;
            foreach(var v in hit) { int dd=Ghost(v)?0:dmg; log.Add(new GameEvent{kind="dano-evento",actor=f.id,target=v.id,value=dd,info="furia",cell=v.cell}); Hurt(v,dd,log,f); }
            events.AddRange(log);
            foreach(var v in hit) if(!v.Alive) KO(v);
            striker=null; CheckVictory();
        }
        void Advance()
        {
            var ending=S.Active;
            if(ending!=null) LunaEndTurn(ending);
            if(ending!=null&&ending.Alive) { EndBlast(ending); if(S.over) return; }
            // Espelho primordial: a inversão dura 2 turnos COMPLETOS do alvo e acaba no fim do turno dele (não no começo).
            if(ending!=null) TickInversion(ending);
            if(ending!=null&&ending.bought)
            {
                Shop.Refill(S,ending.team);
                Emit(new GameEvent{kind="loja",actor=ending.team,info=string.Join(",",S.shops[ending.team].slots)});
            }
            // Corte negro: a proibição de cura acaba com o turno de quem cortou.
            if(ending!=null) { string by=ending.id.ToString(); foreach(var x in S.fighters) x.buffs.RemoveAll(b=>(b.id==NoHealBuff||b.id==InvertedHealBuff)&&b.from==by); }
            for(int i=0;i<S.timeline.Count&&!S.over;i++)
            {
                if(++S.active>=S.timeline.Count) { S.active=0; EndRound(); if(S.over) return; }
                var next=S.Active;
                if(!next.Alive) continue;
                // Preso: perde este turno inteiro e sai da estrutura.
                if(next.prisoned>0) { Jailed(next); continue; }
                // Atordoado: perde o turno (continua podendo ser alvo).
                if(next.stunned>0) { next.stunned--; Emit(new GameEvent{kind="atordoado",target=next.id,value=next.stunned,cell=next.cell}); continue; }
                BeginTurn(next);
                // Chefe e invocações dele (neutros na linha do tempo): a IA joga o turno na hora (Boss.cs).
                if(next.Alive&&AutoTurn(next)) { if(S.over) return; continue; }
                if(next.Alive) return;
            }
        }
        void EndRound()
        {
            if(S.mode==VictoryMode.Dominio)
            {
                var present=S.fighters.Where(f=>f.Alive&&f.summoner==0&&Board.Footprint(f).Any(c=>S.At(c).zone)).Select(f=>S.Team(f.team)).Distinct().ToList();
                if(present.Count==1) { S.domain[present[0]]++; Emit(new GameEvent{kind="dominio",actor=present[0],value=S.domain[present[0]]}); }
                // Cada braseiro com um só time colado vale mais 1 ponto.
                for(int i=0;i<S.tiles.Length;i++) {
                    if(S.tiles[i].Obj!=TileObj.Braseiro) continue; var c=new Cell(i%S.width,i/S.width);
                    var near=S.fighters.Where(f=>f.Alive&&f.summoner==0&&Board.Gap(f,c)==1).Select(f=>S.Team(f.team)).Distinct().ToList();
                    if(near.Count==1) { S.domain[near[0]]++; Emit(new GameEvent{kind="dominio",actor=near[0],value=S.domain[near[0]],cell=c,info="braseiro"}); }
                }
            }
            LunaEndRound(); Neutrals(); TickEvent(); BossRound();
            S.round++; MapGen.SpawnCoins(S,Math.Max(1,S.players/2));
            Emit(new GameEvent{kind="rodada",value=S.round});
            CheckVictory();
        }

        // Eventos espelhados para não favorecer um time. Mudanças que isolam alguém são desfeitas.
        // (id, peso, rodadas): rodadas 0 = instantâneo; só 1 evento com duração por vez (EVENTOS-LOJA-ELENCO.md).
        public static readonly (string id,int weight,int rounds)[] Cataclysms=new (string id,int weight,int rounds)[]
        {
            ("chuva-de-moedas",3,0), ("queda-de-pedras",2,0), ("terremoto",2,0), ("mare-de-lava",2,3), ("portal-caotico",2,0), ("eco-ancestral",1,0),
            ("mare-de-energia",2,0), ("festival-de-fogos",2,1), ("nevoeiro",2,1), ("vendaval",2,0), ("papa-moedas",2,0), ("errante",1,5), ("chefe",4,6),
            ("bombas-invisiveis",2,0), ("vampiro",1,5),
        }.Concat(MapEvents.All.Select(e=>(e.id,e.weight,e.rounds))).ToArray();
        // Só para testes e para o editor: dispara um evento escolhido.
        public List<GameEvent> Trigger(string id) { events=new List<GameEvent>(); Cataclysm(id); S.revision++; return events; }
        // API genérica (chefes e skills que disparam eventos): dispara o evento pelo id, ou sorteia um como a face Cataclisma
        // (id nulo). Os eventos saem na lista do comando em curso. Falso = id desconhecido.
        public bool FireEvent(string id=null)
        {
            if(id!=null&&!Cataclysms.Any(x=>x.id==id)) return false;
            Cataclysm(id); return true;
        }
        void Cataclysm(string forced=null)
        {
            // O boss só aparece no labirinto, uma vez por partida; o Errante fica para os mapas abertos.
            var pool=Cataclysms.Where(x=>(string.IsNullOrEmpty(S.ev)||x.rounds==0)&&(x.id!="chefe"||S.fog&&!S.boss)&&(x.id!="errante"&&x.id!="vampiro"||!S.fog)&&x.weight>0&&MapEvents.Allowed(x.id,S.fog)&&(!(MapEvents.Get(x.id)?.waterOnly??false)||S.tiles.Any(t=>t.terrain==Terrain.Agua))).ToArray();
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
                    foreach(var x in S.fighters) { x.usedMatch.RemoveAll(id=>id!="desejo"&&Content.AnySkill(id) is SkillDef u&&u.ultimate&&!u.noRefresh); x.cooldowns.Clear(); }
                    break;
                case "mare-de-energia":
                    foreach(var x in S.fighters.Where(x=>x.Alive&&!Content.Def(x).neutral)) AddBuff(x,new Buff{id="mare-de-energia",turns=2,pa=1,pm=1});
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
                case "chefe":
                {
                    // Na casa livre mais longe de todos os lutadores; um dos 3 bosses, sorteado.
                    var free=Enumerable.Range(0,S.tiles.Length).Select(i=>new Cell(i%S.width,i/S.width)).Where(x=>MapGen.Free(S,x)&&!S.At(x).zone).ToList();
                    if(free.Count==0) { rounds=0; break; }
                    var spot=free.OrderByDescending(x=>S.fighters.Where(f=>f.Alive).Select(f=>Board.Gap(f,x)).DefaultIfEmpty(99).Min()).ThenBy(x=>x.z).ThenBy(x=>x.x).First();
                    var b=Spawn(Content.Bosses[Dice.Range(S,Content.Bosses.Length)],MatchState.Neutral,spot,0); S.boss=true;
                    Emit(new GameEvent{kind="invocar",target=b.id,info=b.def,cell=spot}); cells.Add(spot);
                    break;
                }
                case "bombas-invisiveis":
                {
                    // Minas que ninguém vê (dono neutro), o mesmo número em cada quadrante. As casas não vão no evento.
                    int half=S.width/2, per=Math.Max(1,S.width*S.height/144);
                    for(int q=0;q<4;q++)
                    {
                        int x0=q%2*half, z0=q/2*half;
                        var spots=new List<Cell>();
                        for(int x=x0;x<x0+half;x++) for(int z=z0;z<z0+half;z++) { var c=new Cell(x,z); var t=S.At(c); if(Board.Walkable(S,c,false)&&t.trap==0&&!t.lava&&!t.zone&&Board.At(S,c)==null&&t.Obj==TileObj.Nada) spots.Add(c); }
                        for(int i=0;i<per&&spots.Count>0;i++) { int k=Dice.Range(S,spots.Count); var c=spots[k]; spots.RemoveAt(k); var t=S.At(c); t.trap=MatchState.Neutral; t.trapBy=0; t.trapDamage=BombDamage; S.mines.Add(Board.Index(S,c)); }
                    }
                    break;
                }
                case "vampiro":
                case "errante":
                {
                    var spot=Area(new Cell(S.width/2,S.height/2),3,false).Where(x=>Board.Walkable(S,x,false)&&Board.At(S,x)==null).OrderBy(x=>x.Distance(new Cell(S.width/2,S.height/2))).ThenBy(x=>x.z).ThenBy(x=>x.x).ToList();
                    if(spot.Count==0) { rounds=0; break; }
                    var b=Spawn(kind=="vampiro"?"chefe-cria-vampirica":"brutamontes-errante",MatchState.Neutral,spot[0],0);
                    Emit(new GameEvent{kind="invocar",target=b.id,info=b.def,cell=spot[0]}); cells.Add(spot[0]);
                    break;
                }
                default:
                    if(MapEvents.Get(kind) is MapEventDef me) StartMapEvent(me,cells);
                    break;
            }
            ev.path=cells.ToArray(); ev.value=rounds;
            if(kind=="bombas-invisiveis") ev.path=new Cell[0];
            if(rounds>0) { S.ev=kind; S.evRounds=rounds; }
        }
        public const int BombDamage=3;
        // Evento de dados (MapEvents): cura única agora e os mobs nas casas pedidas. O dano a mais vale enquanto S.ev for ele.
        void StartMapEvent(MapEventDef e,List<Cell> cells)
        {
            if(e.heal>0) foreach(var f in S.fighters.Where(x=>x.Alive&&MatchState.Player(x.team)).ToList()) Heal(f,e.heal,f,e.id);
            if(string.IsNullOrEmpty(e.mob)||!Content.Fighters.ContainsKey(e.mob)) return;
            foreach(var c in MobSpots(e,e.MobCount(S.players)))
            {
                var m=Spawn(e.mob,MatchState.Neutral,c,0);
                Emit(new GameEvent{kind="invocar",target=m.id,info=m.def,cell=c}); cells.Add(c);
            }
        }
        // Dano a mais do evento de dados ativo nos golpes dos lutadores dos jogadores (Lua de Sangue: +3).
        public int EventDamage(Fighter f) => MatchState.Player(f.team)&&MapEvents.Get(S.ev) is MapEventDef e?e.dmg:0;
        List<Cell> MobSpots(MapEventDef e,int n)
        {
            var spots=new List<Cell>(); var mid=new Cell(S.width/2,S.height/2);
            var free=Enumerable.Range(0,S.tiles.Length).Select(i=>new Cell(i%S.width,i/S.width))
                .Where(c=>(e.waterOnly?S.At(c).terrain==Terrain.Agua&&Board.At(S,c)==null:MapGen.Free(S,c))&&!S.At(c).zone&&!S.At(c).lava&&S.fighters.All(f=>!f.Alive||Board.Gap(f,c)>=e.minGap)).ToList();
            switch(e.spawn)
            {
                case MobSpawn.Espelhado:
                    // Pares espelhados; número ímpar: o último sai sozinho.
                    while(spots.Count<n&&free.Count>0)
                    {
                        var c=free[Dice.Range(S,free.Count)]; var m=S.Mirror(c); free.Remove(c);
                        if(spots.Count+1==n) spots.Add(c);
                        else if(c!=m&&free.Remove(m)) { spots.Add(c); spots.Add(m); }
                    }
                    break;
                case MobSpawn.Centro:
                    spots.AddRange(free.OrderBy(c=>c.Distance(mid)).ThenBy(c=>c.z).ThenBy(c=>c.x).Take(n)); break;
                case MobSpawn.Longe:
                    spots.AddRange(free.OrderByDescending(c=>S.fighters.Where(f=>f.Alive).Select(f=>Board.Gap(f,c)).DefaultIfEmpty(99).Min()).ThenBy(c=>c.z).ThenBy(c=>c.x).Take(n)); break;
            }
            return spots;
        }
        bool LavaOk(Cell c,bool farFromFighters)
        {
            var t=S.At(c);
            return t.height==0&&t.terrain==Terrain.Livre&&!t.lava&&!t.zone&&(!farFromFighters||S.fighters.All(f=>!f.Alive||Board.Gap(f,c)>=2));
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
                // Lava cercada: nasce um par novo numa casa baixa longe dos lutadores, para toda rodada ganhar lava.
                if(options.Count==0)
                    for(int i=0;i<S.tiles.Length;i++)
                    {
                        var c=new Cell(i%S.width,i/S.width); var m=S.Mirror(c);
                        if(Board.Index(S,c)<=Board.Index(S,m)&&c!=m&&LavaOk(c,true)&&LavaOk(m,true)) options.Add(c);
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
                    foreach(var v in S.fighters.Where(x=>x.Alive&&x.prisoned==0&&Board.Gap(x,c)<=1).ToList())
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
            // O chefe RECRUTADO por um jogador já não é mais chefe: o fim do evento não mata o aliado dele.
            if(id=="chefe"||id=="vampiro") foreach(var b in S.fighters.Where(x=>x.Alive&&BossKind(x.def)&&!x.recruitedBoss).ToList()) { b.hp=0; b.deathProcessed=true; Emit(new GameEvent{kind="sumir",target=b.id,cell=b.cell}); AnyDeath(b); }
            if(id=="errante") foreach(var b in S.fighters.Where(x=>x.Alive&&x.def=="brutamontes-errante").ToList()) { b.hp=0; b.deathProcessed=true; Emit(new GameEvent{kind="sumir",target=b.id,cell=b.cell}); AnyDeath(b); }
            if(MapEvents.Get(id) is MapEventDef me&&!me.keepMobs&&!string.IsNullOrEmpty(me.mob))
                foreach(var b in S.fighters.Where(x=>x.Alive&&x.def==me.mob&&x.team==MatchState.Neutral).ToList()) { b.hp=0; b.deathProcessed=true; Emit(new GameEvent{kind="sumir",target=b.id,cell=b.cell}); AnyDeath(b); }
        }
        // Neutros agem no fim da rodada: o papa-moedas foge de quem chega perto e come moedas; o Errante caça o mais próximo.
        void Neutrals()
        {
            foreach(var n in S.fighters.Where(x=>x.Alive&&x.team==MatchState.Neutral&&Content.Def(x).neutral).ToList())
            {
                if(!n.Alive||S.over) continue;
                // Preso no Black Hole (ou preso/atordoado): a IA não age nem conta a rodada; o retorno vale como nos turnos.
                if(n.prisoned>0) { Jailed(n); continue; }
                if(n.stunned>0) { n.stunned--; Emit(new GameEvent{kind="atordoado",target=n.id,value=n.stunned,cell=n.cell}); continue; }
                n.pm=Content.Def(n).pm;
                var foes=S.fighters.Where(x=>x.Alive&&!Content.Def(x).neutral&&!x.hidden).ToList();
                if(LunaNeutral(n,foes)) continue;
                if(n.def=="papa-moedas")
                {
                    var coins=Enumerable.Range(0,S.tiles.Length).Where(i=>S.tiles[i].coins>0).Select(i=>new Cell(i%S.width,i/S.width)).ToList();
                    if(foes.Any(x=>Board.Gap(x,n)<=2)) WalkTo(n,Best(n,c=>foes.Min(x=>Board.Gap(x,c))));
                    else if(coins.Count>0) WalkTo(n,Best(n,c=>-coins.Min(k=>k.Distance(c))));
                    continue;
                }
                n.ai++;
                if(Content.Bosses.Contains(n.def)) { BossTurn(n,foes); continue; }
                // Monstro de farm: guarda o lugar; só caça quem chega a até MonsterAggro casas e bate se ficar colado.
                if(Monster(n)) {
                    var prey=foes.Where(x=>x.prisoned==0&&Board.Gap(x,n)<=MonsterAggro).OrderBy(x=>Board.Gap(x,n)).ThenBy(x=>x.id).FirstOrDefault();
                    if(prey==null) continue;
                    var hit=Content.Def(n).skills[0];
                    if(Board.Gap(n,prey)>1) WalkTo(n,Best(n,c=>-Board.Gap(prey,c)));
                    if(n.Alive&&prey.Alive&&Board.Gap(n,prey)==1) Strike(n,hit,prey.cell);
                    continue;
                }
                if(Content.Def(n).aggro>0||Content.Def(n).roam) { Prowl(n,foes); continue; }
                var target=foes.Where(x=>x.prisoned==0).OrderBy(x=>Board.Gap(x,n)).ThenBy(x=>x.id).FirstOrDefault();
                if(target==null) continue;
                var skills=Content.Def(n).skills;
                if(n.ai%2==0&&skills.Length>1&&foes.Any(x=>Board.Gap(x,n)<=2)) Strike(n,skills[1],n.cell);
                if(n.Alive&&Board.Gap(n,target)>1) WalkTo(n,Best(n,c=>-Board.Gap(target,c)));
                if(n.Alive&&target.Alive&&Board.Gap(n,target)==1) Strike(n,skills[0],target.cell);
            }
        }
        // Boss do labirinto: caça o lutador mais perto pelo caminho (contando portas), abre as portas no caminho (1 PM),
        // a cada 2 rodadas usa o golpe especial em volta e bate em quem estiver colado.
        void BossTurn(Fighter n,List<Fighter> foes)
        {
            var targets=foes.Where(x=>x.prisoned==0).ToList(); if(targets.Count==0) return;
            var probe=new Fighter{def=n.def,cell=n.cell};
            var mine=Board.Costs(S,probe,true,true);
            var target=targets.OrderBy(x=>mine[Board.Index(S,x.cell)]).ThenBy(x=>x.id).First();
            var skills=Content.Def(n).skills; bool flying=Content.Def(n).flying;
            if(n.ai%2==0&&skills.Length>1&&foes.Any(x=>Board.Gap(x,n)<=2)) Strike(n,skills[1],n.cell);
            var field=Board.Costs(S,new Fighter{def=n.def,cell=target.cell},true,true);
            while(n.Alive&&n.pm>0&&Board.Gap(n,target)>1)
            {
                Cell? next=null; int best=field[Board.Index(S,n.cell)];
                foreach(var d in Board.Dirs) { var c=n.cell+d; if(!Board.Contains(S,c)) continue; int v=field[Board.Index(S,c)]; if(v<best&&(S.At(c).terrain==Terrain.Porta||Board.StepCost(S,n.cell,c,flying)>0&&Board.At(S,c)==null)) { best=v; next=c; } }
                if(next==null) break;
                if(S.At(next.Value).terrain==Terrain.Porta) { Open(next.Value,n.id); n.pm--; continue; }
                int cost=Board.StepCost(S,n.cell,next.Value,flying); if(cost>n.pm) break;
                var from=n.cell; n.cell=next.Value; n.pm-=cost;
                Emit(new GameEvent{kind="mover",actor=n.id,path=new[]{from,n.cell}});
            }
            if(n.Alive&&target.Alive&&Board.Gap(n,target)==1) Strike(n,skills[0],target.cell);
        }
        // Mob de evento (aggro/roam): caça o mais perto até aggro casas e bate colado; sem alvo e com roam, anda para uma casa sorteada.
        void Prowl(Fighter n,List<Fighter> foes)
        {
            var d=Content.Def(n);
            var prey=foes.Where(x=>x.prisoned==0&&(d.aggro<=0||Board.Gap(x,n)<=d.aggro)).OrderBy(x=>Board.Gap(x,n)).ThenBy(x=>x.id).FirstOrDefault();
            if(prey!=null)
            {
                if(Board.Gap(n,prey)>1) WalkTo(n,Best(n,c=>-Board.Gap(prey,c)));
                if(n.Alive&&prey.Alive&&Board.Gap(n,prey)==1&&d.skills.Length>0) Strike(n,d.skills[0],prey.cell);
                return;
            }
            if(!d.roam) return;
            var dist=Board.Costs(S,n); var spots=new List<Cell>();
            for(int i=0;i<dist.Length;i++) { var c=new Cell(i%S.width,i/S.width); if(dist[i]>0&&dist[i]<=n.pm&&Board.At(S,c)==null) spots.Add(c); }
            if(spots.Count>0) WalkTo(n,spots[Dice.Range(S,spots.Count)]);
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
                var backup=S.tiles.Select(Fog.Copy).ToArray();
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
            if(BossVictory()) return;
            // Time sem lutadores ou sem Galeões sai da partida; o que restar dele sai do tabuleiro.
            var standing=new List<int>();
            foreach(int t in S.Teams())
            {
                if(S.eliminated[t]) continue;
                bool alive=S.fighters.Any(f=>f.Alive&&S.Team(f.team)==t&&f.summoner==0);
                if(alive&&!(S.mode==VictoryMode.Galeoes&&S.galeoes[t]<=0)) { standing.Add(t); continue; }
                S.eliminated[t]=true;
                foreach(var f in S.fighters.Where(f=>f.Alive&&S.Team(f.team)==t).ToList()) { f.hp=0; KO(f); }
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
