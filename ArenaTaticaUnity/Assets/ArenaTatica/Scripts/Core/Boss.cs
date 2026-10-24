using System;
using System.Collections.Generic;
using System.Linq;

namespace ArenaTatica.Core
{
    // Chefe com IA (Rei Thaagarys, autor 22/10). Tudo nos dados (FighterDef.boss + SkillDef.weight), editável no Editor:
    // on: a ficha é um chefe. every: o chefe entra na linha do tempo a cada N turnos de jogadores. phase2: % da vida em que ele
    // passa à fase 2 (forma phase2Form). actions1/actions2: ações por turno em cada fase. wake: levanta do trono no fim dessa
    // rodada (0 = já começa de pé). moveWeight: peso do "movimento normal" no pool. hpPlayer: PV a mais por jogador além do 1º.
    // events: pool do evento aleatório (ids de Match.Cataclysms separados por vírgula; vazio = o sorteio normal).
    // seatForm/standForm/phase2Form: formas sentado no trono, de pé e na fase 2 (sprites e música vêm de cada FormDef).
    [Serializable] public class BossDef
    {
        public bool on;
        public int every=2, phase2=50, actions1=1, actions2=2, wake=1, moveWeight=3, hpPlayer=15;
        public string events="", seatForm="trono", standForm="espada", phase2Form="duas-espadas";
        // Recompensa (autor): derrotado pelo golpe final de um jogador, o chefe vira aliado dele na MESMA partida.
        // rewardAlly: liga o recrutamento. allyHpFactor: multiplica os PV do aliado. allyForm: forma em que ele passa a lutar.
        public bool rewardAlly=true;
        public float allyHpFactor=1f;
        public string allyForm="espada";
    }

    public sealed partial class Match
    {
        // Recrutado (recruitedBoss) deixa de ser chefe: é aliado controlável de um jogador, não IA neutra e não paga recompensa 2×.
        public static BossDef BossOf(Fighter f) => f!=null&&!f.recruitedBoss&&Content.Fighters.ContainsKey(f.def??"")&&Content.Def(f).boss is BossDef b&&b.on?b:null;
        public bool BossFight => !string.IsNullOrEmpty(S.bossFight);
        public Fighter Boss => S.fighters.FirstOrDefault(x=>BossOf(x)!=null&&x.summoner==0);
        public static bool Seated(Fighter f) => BossOf(f) is BossDef b&&!string.IsNullOrEmpty(b.seatForm)&&f.form==b.seatForm;
        public static int BossPhase(Fighter f) => BossOf(f) is BossDef b&&f.form==b.phase2Form?2:1;

        // Batalha contra o chefe: os jogadores competem entre si desde o início (arena, PvP), vale a Eliminação, o mapa só tem o
        // Trono no meio (as 4 casas da loja: comprar colado nele) e o chefe começa sentado nele. Quem der o golpe final no chefe
        // ganha a ficha dele como aliado na mesma partida (BossDef.rewardAlly, TryRecruitBoss). O Trono ganha o chefe a cada every turnos.
        void SetupBoss(MatchSetup setup)
        {
            if(string.IsNullOrEmpty(setup.boss)||!Content.Fighters.TryGetValue(setup.boss,out var d)||d.boss==null||!d.boss.on) return;
            var b=d.boss; S.bossFight=d.id; S.mode=VictoryMode.Eliminacao;
            S.galeoes[1]=setup.galeoes;
            S.fighters.RemoveAll(Monster);
            var throne=MapGen.ShopCells(S);
            foreach(var c in throne) { var t=S.At(c); t.terrain=Terrain.Loja; t.coins=0; t.obj=0; t.trap=0; t.lava=false; }
            // Em volta do Trono fica chão livre para ele levantar e para comprar.
            int x0=throne.Min(c=>c.x)-2, z0=throne.Min(c=>c.z)-2;
            for(int x=x0;x<=x0+5;x++) for(int z=z0;z<=z0+5;z++) { var c=new Cell(x,z); if(!Board.Contains(S,c)||throne.Contains(c)) continue;
                var t=S.At(c); if(t.terrain!=Terrain.Livre) t.terrain=Terrain.Livre; t.obj=0; t.trap=0; t.lava=false; }
            S.mines.RemoveAll(i=>S.tiles[i].trap==0);
            BuildTimeline();
            var corner=new Cell(throne.Min(c=>c.x),throne.Min(c=>c.z));
            var boss=Spawn(d.id,MatchState.Neutral,corner,0);
            boss.form=b.wake>0&&!string.IsNullOrEmpty(b.seatForm)?b.seatForm:b.standForm;
            boss.bonusHp=Math.Max(0,b.hpPlayer)*(S.players-1); boss.hp=Content.MaxHp(boss);
            // O chefe entra depois de cada every turnos de jogadores (pelo menos uma vez por rodada).
            int every=Math.Max(1,b.every), n=0; var line=new List<int>();
            foreach(int id in S.timeline) { line.Add(id); if(++n%every==0) line.Add(boss.id); }
            if(!line.Contains(boss.id)) line.Add(boss.id);
            S.timeline.Clear(); S.timeline.AddRange(line); S.active=0;
            // PV do chefe aliado, por assento: média (arredondada, mín. 1) dos PV máximos dos PERSONAGENS inimigos iniciais de cada
            // jogador (sem invocações nem neutros, e não a média dos totais de time). Cada conquistador resgata o valor do seu assento.
            if(S.bossAllyHp==null||S.bossAllyHp.Length<MatchState.MaxPlayers+1) S.bossAllyHp=new int[MatchState.MaxPlayers+1];
            for(int p=1;p<=S.players;p++) S.bossAllyHp[p]=BossAllyHp(p);
        }
        // PV do chefe recrutado por um assento: média arredondada (mín. 1) do MaxHp dos personagens (summoner 0) inimigos dele, que
        // já estão em campo no SetupBoss. Invocações (summoner != 0) e neutros ficam fora da conta.
        int BossAllyHp(int seat)
        {
            var foes=S.fighters.Where(x=>x.summoner==0&&MatchState.Player(x.team)&&!S.Ally(x.team,seat)).ToList();
            if(foes.Count==0) return 1;
            return Math.Max(1,(foes.Sum(x=>Content.MaxHp(x))+foes.Count/2)/foes.Count);
        }

        // Neutro na linha do tempo (chefe e as invocações dele): joga sozinho. false = lutador de jogador (espera o comando).
        bool AutoTurn(Fighter f)
        {
            if(f.team!=MatchState.Neutral) return false;
            if(BossOf(f) is BossDef b) BossAct(f,b); else Minion(f);
            CheckVictory();
            return true;
        }
        List<Fighter> Foes(Fighter f) => S.fighters.Where(v=>v.Alive&&!S.Ally(v.team,f.team)&&v.team!=MatchState.Neutral&&v.prisoned==0&&!Concealed(S,f.team,v)).ToList();

        void BossAct(Fighter f,BossDef b)
        {
            if(Seated(f)) { Emit(new GameEvent{kind="chefe-trono",actor=f.id,cell=f.cell}); return; }
            BossPhaseCheck(f,b);
            int actions=Math.Max(1,BossPhase(f)==2?b.actions2:b.actions1);
            for(int i=0;i<actions&&f.Alive&&!S.over;i++) { if(!BossAction(f,b)) break; CheckVictory(); }
        }
        // Uma ação: sorteia no pool (skills com peso, na forma atual, e o movimento) pelos pesos; sem alvo, tira e sorteia de novo.
        // O foco é o próximo jogador do ciclo (BossSeat): só repete um assento depois de atender todos os outros.
        bool BossAction(Fighter f,BossDef b)
        {
            var cycle=BossCycleOf(f); int seat=BossSeat(f,cycle);
            var pool=Content.Def(f).skills.Where(s=>s.weight>0&&Content.Skill(f,s.id)!=null).Select(s=>(skill:s,weight:s.weight)).ToList();
            if(b.moveWeight>0) pool.Add((null,b.moveWeight));
            while(pool.Count>0&&f.Alive&&!S.over)
            {
                int roll=Dice.Range(S,pool.Sum(p=>p.weight)), k=0;
                while(roll>=pool[k].weight) { roll-=pool[k].weight; k++; }
                var pick=pool[k]; pool.RemoveAt(k);
                if(pick.skill==null) { if(BossMove(f,seat)) return true; continue; }
                if(BossAim(f,pick.skill,seat,cycle) is Cell c)
                {
                    // A vez do jogador só é dada quando uma skill ofensiva chega nele de verdade (erro e bloqueio contam).
                    int before=events.Count;
                    if(UseSkill(f,pick.skill.id,c,0,c,out _)) { BossHitSeats(f,cycle,before); return true; }
                }
            }
            // Nada serviu: ao menos anda até um lutador do jogador da vez.
            return BossMove(f,seat);
        }
        // --- Ciclo de jogadores-alvo (autor): cada chefe automático ataca um jogador (assento = Fighter.team) por vez, na ordem
        // sorteada, guardada no MatchState (sobrevive a save/reconnect). Não muda a IA das invocações (Minion) nem a do chefe
        // recrutado, que deixa de ser chefe em BossOf. ---
        BossCycle BossCycleOf(Fighter f)
        {
            if(S.bossCycles==null) S.bossCycles=new List<BossCycle>();
            foreach(var x in S.bossCycles) if(x!=null&&x.boss==f.id) return x;
            var c=new BossCycle{boss=f.id}; S.bossCycles.Add(c); return c;
        }
        // Assentos elegíveis agora: um por jogador, dos adversários visíveis, vivos e não presos (Foes), respeitando S.Ally.
        List<int> BossSeats(Fighter f) => Foes(f).Where(v=>MatchState.Player(v.team)).Select(v=>v.team).Distinct().OrderBy(x=>x).ToList();
        // Fisher-Yates com o dado da partida (Dice.Range): igual no Host e nos clientes, nunca System.Random/UnityEngine.Random.
        void BossShuffle(List<int> seats)
        {
            for(int i=seats.Count-1;i>0;i--) { int k=Dice.Range(S,i+1); int t=seats[i]; seats[i]=seats[k]; seats[k]=t; }
        }
        // Novo ciclo: embaralha os assentos e, se a permutação saiu igual à do ciclo anterior, roda por um deslocamento não zero
        // (com dois jogadores pode começar pelo último do ciclo anterior, pois todos já foram atendidos).
        void BossNewCycle(BossCycle c,List<int> seats)
        {
            var next=new List<int>(seats); BossShuffle(next);
            if(next.Count>1&&next.SequenceEqual(c.order.Where(seats.Contains)))
            {
                int k=Dice.Range(S,next.Count-1)+1;
                next=next.Skip(k).Concat(next.Take(k)).ToList();
            }
            var paused=c.pending.Where(s=>!seats.Contains(s)).ToList();
            c.order=paused.Concat(next).ToList(); c.pending=new List<int>(c.order);
        }
        // Assento em foco nesta ação: o primeiro pendente que está elegível. Jogador eliminado sai do ciclo; oculto/preso continua
        // pendente (entra antes dos demais ao reaparecer) sem travar os elegíveis. Ciclo atendido = sorteia outro na ação seguinte.
        int BossSeat(Fighter f,BossCycle c)
        {
            var ok=BossSeats(f); if(ok.Count==0) return 0;
            c.pending.RemoveAll(s=>!ok.Contains(s)&&!S.fighters.Any(v=>v.Alive&&v.team==s));
            c.order.RemoveAll(s=>!ok.Contains(s)&&!S.fighters.Any(v=>v.Alive&&v.team==s));
            if(c.order.Count==0) { BossNewCycle(c,ok); return c.pending[0]; }
            // Assento que voltou ao campo (ou entrou) depois do sorteio entra nos pendentes, na frente de quem já foi atendido.
            foreach(int s in ok) if(!c.order.Contains(s)) { c.order.Add(s); c.pending.Add(s); }
            var open=c.pending.Where(ok.Contains).ToList();
            if(open.Count>0) return open[0];
            // Recomeça o sorteio dos elegíveis e guarda os indisponíveis na frente para quando voltarem.
            BossNewCycle(c,ok); return c.pending.First(ok.Contains);
        }
        // Jogadores realmente atingidos por uma skill ofensiva: os alvos dos eventos novos ('ataque' com alvo > 0: acerto, bloqueio
        // ou erro; 'troca-vida' do swapHp). Strike também emite um ataque por vítima das áreas.
        void BossHitSeats(Fighter f,BossCycle c,int start)
        {
            var seats=new List<int>();
            void Add(int id) { var v=id>0?S.F(id):null; if(v!=null&&MatchState.Player(v.team)&&!S.Ally(v.team,f.team)&&!seats.Contains(v.team)) seats.Add(v.team); }
            for(int i=Math.Max(0,start);i<events.Count;i++) { var e=events[i]; if(e!=null&&e.actor==f.id&&e.target>0&&(e.kind=="ataque"||e.kind=="troca-vida")) Add(e.target); }
            foreach(int s in seats) { if(!c.order.Contains(s)) c.order.Add(s); c.pending.Remove(s); }
        }
        // Previsão conservadora das vítimas de um golpe (mesma geometria do Strike, sem os dados): quem está na área. Serve para não
        // repetir jogador já atendido no meio de uma área; a contagem usa os eventos reais do golpe.
        List<Fighter> BossBlast(Fighter f,SkillDef sk,Cell c)
        {
            var centers=sk.selfArea?Board.Footprint(f).ToList():new List<Cell>{c};
            bool Blast(Fighter x) => centers.Any(o=>Board.Footprint(x).Any(xc=>InArea(o,xc,sk.area,sk.square)&&Board.WallClear(S,o,xc)));
            var list=new List<Fighter>();
            if(!sk.selfArea&&Board.At(S,c) is Fighter main&&main!=f) list.Add(main);
            foreach(var x in S.fighters) if(x.Alive&&x!=f&&x.prisoned==0&&!list.Contains(x)&&Blast(x)&&!Concealed(S,f.team,x)&&(sk.Custom?Hits(f,sk,x):!sk.foes||!S.Ally(x.team,f.team))) list.Add(x);
            return list;
        }
        // Ataque em área que pegaria jogador já atendido enquanto outro continua pendente: melhor outra ação (ou caminhar), sem
        // filtrar vítimas no Strike. Só considera skills de Dano com área; o resto do alvo é o mesmo de sempre.
        bool BossAreaFree(Fighter f,BossCycle c,SkillDef sk,Cell at)
        {
            if(sk.kind!=SkillKind.Dano||sk.area<=0) return true;
            // Só contam os pendentes que dá para atacar agora: um jogador oculto não trava a área dos elegíveis.
            var wait=c.pending.Where(BossSeats(f).Contains).ToList();
            if(wait.Count==0) return true;
            foreach(var v in BossBlast(f,sk,at))
            {
                if(!MatchState.Player(v.team)||wait.Contains(v.team)) continue;
                if(wait.Any(s=>s!=v.team)) return false;
            }
            return true;
        }
        // Alvo de cada tipo de skill do pool (null = sem alvo útil agora), sempre nos lutadores do jogador da vez (seat).
        Cell? BossAim(Fighter f,SkillDef sk,int seat,BossCycle cyc)
        {
            // Sem assento elegível (todos ocultos/presos/eliminados) só saem as Zonas, que não dependem de alvo (cataclisma, revelar).
            if(seat<=0&&sk.kind!=SkillKind.Zona) return null;
            var foes=seat>0?Foes(f).Where(v=>v.team==seat).ToList():new List<Fighter>();
            bool Ok(Cell c2) => Check(f,sk,c2,false,out _);
            bool Safe(Cell c2) => BossAreaFree(f,cyc,sk,c2);
            if(sk.selfArea) return foes.Any(v=>Board.Gap(v,f)<=Math.Max(1,sk.area))&&Ok(f.cell)&&Safe(f.cell)?f.cell:(Cell?)null;
            switch(sk.kind)
            {
                case SkillKind.Dano:
                    // Troca de vida: só vale contra quem tem mais PV que ele.
                    var hit=foes.Where(v=>!sk.swapHp||v.hp>f.hp).OrderBy(v=>sk.swapHp?-v.hp:v.hp).ThenBy(v=>v.id);
                    foreach(var v in hit) if(Ok(v.cell)&&Safe(v.cell)) return v.cell;
                    return null;
                case SkillKind.Teleporte:
                {
                    // Para colado no inimigo mais fraco que ele alcança; só se ainda não está colado em ninguém.
                    if(foes.Count==0||foes.Any(v=>Board.Gap(v,f)<=1)) return null;
                    var prey=foes.OrderBy(v=>v.hp).ThenBy(v=>v.id).First(); int now=Board.Gap(prey,f);
                    Cell? best=null; int top=now;
                    foreach(var c in Cells()) { var probe=new Fighter{def=f.def,cell=c}; int g=Board.Gap(prey,probe); if(g>=1&&g<top&&Ok(c)) { top=g; best=c; } }
                    return best;
                }
                case SkillKind.Invocacao:
                {
                    var near=foes.OrderBy(v=>Board.Gap(v,f)).FirstOrDefault();
                    return Cells().Where(c=>Board.At(S,c)==null&&Ok(c)).OrderBy(c=>near==null?0:near.cell.Distance(c)).ThenBy(c=>c.z).ThenBy(c=>c.x).Cast<Cell?>().FirstOrDefault();
                }
                case SkillKind.Zona:
                    if(sk.cataclysm||sk.resetUlts||sk.reveal&&sk.global)
                    {
                        if(sk.reveal&&!S.fighters.Any(v=>v.Alive&&v.hidden&&!S.Ally(v.team,f.team))) return null;
                        if(sk.resetUlts&&!S.fighters.Any(v=>v.Alive&&(v.cooldowns.Count>0||v.usedMatch.Count>0))) return null;
                        return Ok(f.cell)?f.cell:(Cell?)null;
                    }
                    if(sk.reveal) return S.fighters.Where(v=>v.Alive&&v.hidden&&!S.Ally(v.team,f.team)).Select(v=>v.cell).Where(Ok).Cast<Cell?>().FirstOrDefault();
                    // Muralhas: no meio do grupo de inimigos (mais inimigos na área).
                    return foes.Select(v=>v.cell).Where(Ok).OrderByDescending(c=>foes.Count(v=>Board.Gap(v,c)<=Math.Max(1,sk.area))).Cast<Cell?>().FirstOrDefault();
                default:
                    foreach(var v in foes) if(Ok(v.cell)) return v.cell;
                    return Ok(f.cell)?f.cell:(Cell?)null;
            }
        }
        IEnumerable<Cell> Cells() => Enumerable.Range(0,S.tiles.Length).Select(i=>new Cell(i%S.width,i/S.width));
        // Movimento normal: anda pelos PM até a casa que mais o aproxima de um lutador do jogador da vez (colado já basta).
        bool BossMove(Fighter f,int seat)
        {
            if(seat<=0) return false;
            var foes=Foes(f).Where(v=>v.team==seat).ToList(); if(foes.Count==0||f.pm<=0||foes.Any(v=>Board.Gap(v,f)<=1)) return false;
            var prey=foes.OrderBy(v=>Board.Gap(v,f)).ThenBy(v=>v.id).First();
            return Approach(f,prey) is Cell to&&Move(f,to,out _);
        }
        // Casa ao alcance dos PM (onde cabe inteiro e há caminho) que mais o aproxima da presa; null = não chega mais perto.
        Cell? Approach(Fighter f,Fighter prey)
        {
            var dist=Board.Costs(S,f); int now=Board.Gap(prey,f);
            int Gap(Cell c) => Board.Gap(prey,new Fighter{def=f.def,cell=c});
            // Planeja até uma casa colada na presa. Um desvio ao redor do Trono pode aumentar a distância
            // em linha reta neste turno; rejeitar esse passo deixava o chefe parado atrás da própria loja.
            var goals=Cells().Where(c=>c!=f.cell&&Gap(c)==1&&Board.Fits(S,f,c)&&dist[Board.Index(S,c)]<int.MaxValue)
                .OrderBy(c=>dist[Board.Index(S,c)]).ThenBy(c=>c.z).ThenBy(c=>c.x);
            foreach(var goal in goals) {
                int cost=dist[Board.Index(S,goal)];
                var path=Board.Path(S,f,goal,cost); if(path==null) continue;
                int spent=0; Cell? to=null;
                for(int i=1;i<path.Count;i++) {
                    int step=Board.Step(S,f,path[i-1],path[i],Content.Def(f).flying,f.climber);
                    if(step<=0||spent+step>f.pm) break;
                    spent+=step; if(Board.Fits(S,f,path[i])) to=path[i];
                }
                if(to.HasValue) return to;
            }
            return Cells().Where(c=>c!=f.cell&&dist[Board.Index(S,c)]<=f.pm&&Board.Fits(S,f,c)&&Gap(c)<now&&Gap(c)>=1)
                .OrderBy(Gap).ThenBy(c=>dist[Board.Index(S,c)]).ThenBy(c=>c.z).ThenBy(c=>c.x)
                .Where(c=>Board.Path(S,f,c,f.pm)!=null).Cast<Cell?>().FirstOrDefault();
        }
        // Invocação do chefe: caça o inimigo visível mais perto e bate colado com a 1ª skill.
        void Minion(Fighter f)
        {
            var foes=Foes(f); var skills=Content.Def(f).skills; if(foes.Count==0||skills.Length==0) return;
            var prey=foes.OrderBy(v=>Board.Gap(v,f)).ThenBy(v=>v.id).First();
            if(Board.Gap(prey,f)>1&&f.pm>0&&Approach(f,prey) is Cell to) Move(f,to,out _);
            if(f.Alive&&prey.Alive&&Check(f,skills[0],prey.cell,false,out _)) UseSkill(f,skills[0].id,prey.cell,0,prey.cell,out _);
        }

        // Fase 2 (vida ≤ phase2%): saca a segunda espada (forma phase2Form). Sentado e ferido até ali, levanta antes.
        void BossPhaseCheck(Fighter f,BossDef b)
        {
            if(!f.Alive||string.IsNullOrEmpty(b.phase2Form)||f.form==b.phase2Form||f.hp*100>Content.MaxHp(f)*b.phase2) return;
            if(Seated(f)) Wake(f,b);
            SetForm(f,b.phase2Form,0,"chefe-fase");
            Emit(new GameEvent{kind="chefe-fase",actor=f.id,value=2,info=b.phase2Form,cell=f.cell});
        }
        // Fim da rodada: paredes contam e caem; o chefe levanta do trono no fim da rodada wake.
        void BossRound()
        {
            if(S.walls.Count>0)
            {
                var fall=new List<Cell>(); var keep=new List<int>();
                foreach(int w in S.walls) { int i=w/16, left=w%16-1; if(left>0) { keep.Add(i*16+left); continue; }
                    if(i<S.tiles.Length&&S.tiles[i].terrain==Terrain.Parede) { S.tiles[i].terrain=Terrain.Livre; fall.Add(new Cell(i%S.width,i/S.width)); } }
                S.walls=keep;
                if(fall.Count>0) Emit(new GameEvent{kind="muralha-fim",path=fall.ToArray(),value=fall.Count});
            }
            foreach(var f in S.fighters.Where(x=>x.Alive&&Seated(x)).ToList()) if(S.round>=BossOf(f).wake) Wake(f,BossOf(f));
        }
        // Levanta do trono: vai para a casa livre mais perto onde cabe inteiro, fora do Trono, de frente para o centro do mapa.
        void Wake(Fighter f,BossDef b)
        {
            var from=f.cell;
            var spot=Cells().Where(c=>Board.Fits(S,f,c)).OrderBy(c=>c.Distance(from)).ThenBy(c=>c.z).ThenBy(c=>c.x).Cast<Cell?>().FirstOrDefault();
            if(spot is Cell to) f.cell=to;
            SetForm(f,b.standForm,0,"chefe-levanta");
            Emit(new GameEvent{kind="chefe-levanta",actor=f.id,path=new[]{from,f.cell},cell=f.cell});
        }

        // Efeitos genéricos novos das skills (SkillDef.walls, resetUlts, cataclysm, reveal global), depois do efeito do tipo.
        void BossExtra(Fighter f,SkillDef sk,Cell c)
        {
            if(sk.walls>0) RaiseWalls(f,sk,c);
            if(sk.reveal&&sk.global) foreach(var h in S.fighters.Where(x=>x.Alive&&x.hidden&&!S.Ally(x.team,f.team)).ToList())
            { h.hidden=false; Emit(new GameEvent{kind="revelar",actor=f.id,target=h.id,cell=h.cell}); }
            if(sk.resetUlts)
            {
                // Como o Eco ancestral, para todos; a própria skill continua em recarga.
                foreach(var x in S.fighters) { x.usedMatch.RemoveAll(id=>id!="desejo"&&id!=sk.id&&Content.AnySkill(id) is SkillDef u&&u.ultimate&&!u.noRefresh); x.cooldowns.RemoveAll(w=>w.skill!=sk.id); }
                Emit(new GameEvent{kind="reinicio",actor=f.id,info=sk.id,cell=f.cell});
            }
            if(sk.cataclysm) BossEvent(f);
        }
        // Evento aleatório pela API de eventos (Match.Cataclysms, que inclui os MapEvents), do pool do chefe.
        void BossEvent(Fighter f)
        {
            var ids=(BossOf(f)?.events??"").Split(new[]{','},StringSplitOptions.RemoveEmptyEntries).Select(x=>x.Trim()).Where(x=>x!="").ToList();
            var pool=Cataclysms.Where(x=>(ids.Count==0?x.weight>0:ids.Contains(x.id))&&(string.IsNullOrEmpty(S.ev)||x.rounds==0)&&x.id!="chefe"&&MapEvents.Allowed(x.id,S.fog)).ToArray();
            if(pool.Length==0) return;
            int roll=Dice.Range(S,pool.Sum(x=>Math.Max(1,x.weight)));
            foreach(var x in pool) { int w=Math.Max(1,x.weight); if(roll<w) { Emit(new GameEvent{kind="chefe-evento",actor=f.id,info=x.id,cell=f.cell}); Cataclysm(x.id); return; } roll-=w; }
        }
        // Muralhas do labirinto: até walls casas livres da área viram parede por turns rodadas; parede que isolaria alguém não sobe.
        void RaiseWalls(Fighter f,SkillDef sk,Cell c)
        {
            int rounds=Math.Max(1,Math.Min(15,sk.turns>0?sk.turns:2));
            var spots=Area(c,Math.Max(1,sk.area),sk.square).Where(x=>{ var t=S.At(x); return t.terrain==Terrain.Livre&&t.Obj==TileObj.Nada&&!t.lava&&t.trap==0&&Board.At(S,x)==null&&!HasPortal(x); }).ToList();
            var raised=new List<Cell>();
            while(raised.Count<sk.walls&&spots.Count>0)
            {
                int k=Dice.Range(S,spots.Count); var x=spots[k]; spots.RemoveAt(k);
                S.At(x).terrain=Terrain.Parede;
                if(!Board.Connected(S)) { S.At(x).terrain=Terrain.Livre; continue; }
                raised.Add(x); S.walls.Add(Board.Index(S,x)*16+rounds);
            }
            Emit(new GameEvent{kind="muralha",actor=f.id,info=sk.id,cell=c,path=raised.ToArray(),value=rounds});
        }

        // Recrutamento do chefe (autor): o golpe final de um jogador transforma o chefe caído em aliado dele NA MESMA partida.
        // Não cria unidade nova: a ficha mantém id e casa, só troca de time/dono. false = não era chefe elegível, o golpe não veio
        // de um jogador (nem de uma invocação cujo dono sobe até o herói original), ou ele já foi recrutado (nunca 2×).
        public bool TryRecruitBoss(Fighter defeated,Fighter killer,List<GameEvent> log=null)
        {
            var b=BossOf(defeated);
            if(b==null||!b.rewardAlly||killer==null) return false;
            // Invocação: a posse sobe pela cadeia de summoner até o herói original (summoner 0) e é ele quem conta.
            var owner=killer;
            for(int guard=0;owner!=null&&owner.summoner!=0&&guard<8;guard++) owner=S.F(owner.summoner);
            if(owner==null||!MatchState.Player(owner.team)) return false;
            int seat=owner.team;
            int baseHp=S.bossAllyHp!=null&&seat>0&&seat<S.bossAllyHp.Length&&S.bossAllyHp[seat]>0?S.bossAllyHp[seat]:BossAllyHp(seat);
            float factor=b.allyHpFactor>0f?b.allyHpFactor:1f;
            int hp=Math.Max(1,(int)Math.Round(baseHp*factor));
            var def=Content.Def(defeated);
            defeated.team=seat; defeated.summoner=owner.id; defeated.recruitedBoss=true; defeated.noLock=true; defeated.noReveal=true;
            defeated.bonusHp=hp-def.hp; defeated.hp=hp; defeated.deathProcessed=false;
            // Limpa bônus, status negativos e flags e as recargas do turno; os usos por partida (usedMatch) continuam como estavam,
            // para o dono não ganhar de graça uma suprema recarregada.
            defeated.buffs.Clear(); defeated.used.Clear(); defeated.cooldowns.Clear();
            defeated.hidden=false; defeated.prisoned=0; defeated.channel=0; defeated.wishReady=false; defeated.channeling=null;
            defeated.banished=false; defeated.stunned=0; defeated.marks=0; defeated.drained=0; defeated.banishDamage=0;
            defeated.revived=false; defeated.carried=0; defeated.anchored=false; defeated.climber=false; defeated.focused=false;
            defeated.focusUsed=false; defeated.auraOn=false; defeated.auraPaid=false; defeated.disguise=null; defeated.lastSkill=null;
            defeated.usedForm.Clear(); defeated.formTurns=0;
            bool Has(string id) => !string.IsNullOrEmpty(id)&&def.forms!=null&&def.forms.Any(x=>x!=null&&x.id==id);
            defeated.form=Has(b.allyForm)?b.allyForm:Has(b.standForm)?b.standForm:def.startForm;
            defeated.pa=0; defeated.pm=0;
            // Uma única entrada na linha do tempo, logo depois de quem deu o golpe: aliado controlável, sem o turno extra do chefe.
            // O lutador do turno atual continua sendo o ativo.
            int keep=S.active>=0&&S.active<S.timeline.Count?S.timeline[S.active]:0;
            var line=S.timeline.Where(id=>id!=defeated.id).ToList();
            int at=line.IndexOf(killer.id); if(at<0) at=line.IndexOf(owner.id);
            line.Insert(at>=0?at+1:line.Count,defeated.id);
            S.timeline.Clear(); S.timeline.AddRange(line);
            if(line.Count>0) { int keepAt=keep==0?-1:line.IndexOf(keep); S.active=keepAt>=0?keepAt:Math.Max(0,Math.Min(S.active,line.Count-1)); }
            var ev=new GameEvent{kind="chefe-aliado",actor=killer.id,target=defeated.id,value=hp,info=defeated.form,cell=defeated.cell};
            if(log!=null) log.Add(ev); else Emit(ev);
            return true;
        }

        // Vitória da batalha: enquanto o chefe hostil está de pé o combate continua, mas o assento que perdeu todos os heróis sai da
        // partida (PvP). Chefe caído (ou recrutado, que deixa de ser chefe em BossOf) = false, e o CheckVictory normal decide:
        // nada de vencedor automático para os heróis, e 2+ times de heróis de pé seguem se enfrentando. Sem heróis, o neutro vence.
        bool BossVictory()
        {
            if(!BossFight) return false;
            foreach(var f in S.fighters.Where(x=>x.Alive&&BossOf(x)!=null).ToList()) BossPhaseCheck(f,BossOf(f));
            if(!S.fighters.Any(x=>x.Alive&&BossOf(x)!=null)) return false;
            foreach(int t in S.Teams())
            {
                if(S.eliminated[t]) continue;
                if(S.fighters.Any(f=>f.Alive&&S.Team(f.team)==t&&f.summoner==0)) continue;
                S.eliminated[t]=true;
                foreach(var f in S.fighters.Where(f=>f.Alive&&S.Team(f.team)==t).ToList()) { f.hp=0; KO(f); }
                Emit(new GameEvent{kind="eliminado",actor=t});
            }
            var heroes=S.fighters.Where(x=>x.Alive&&x.summoner==0&&MatchState.Player(x.team)).ToList();
            if(heroes.Count>0) return true;
            S.over=true; S.winner=MatchState.Neutral;
            Emit(new GameEvent{kind="vitoria",actor=S.winner,info="Chefe"});
            return true;
        }
    }
}
