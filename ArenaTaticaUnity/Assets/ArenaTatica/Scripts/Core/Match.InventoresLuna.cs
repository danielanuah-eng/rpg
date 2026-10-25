using System;
using System.Collections.Generic;
using System.Linq;

namespace ArenaTatica.Core
{
    // Expansão de 22/10 (pedido do autor, desenho técnico do Codex): Nikola/Blip/Celinos (inventores), Bifia (arco lunar),
    // Desland (roque e xadrez), Nepturius (esfera de antimatéria) e os tubarões das Docas inundadas.
    // Todo o comportamento novo mora AQUI; o núcleo (Match.cs, Board.cs, MapGen.cs) só ganhou hooks de uma linha.
    // Regras do pedido respeitadas: toda validação acontece em CheckLuna (antes de gastar PA/RNG ou mexer no estado) e o
    // ResolveLuna só roda depois do custo pago. Comando rejeitado não muda nada. Reutiliza Board.Path/Costs, Strike, Hurt,
    // Heal, AddBuff, Push, Shop (compra), Summon/Spawn e os GameEvent genéricos.
    public sealed partial class Match
    {
        // ── Configuração (lida das fichas; 0 = padrão do desenho técnico) ──
        public const string ShipTech="tecnologia";   // info do buff/evento da proteção tecnológica
        static int Cfg(int? value,int fallback) => value.HasValue&& value.Value>0?value.Value:fallback;
        static LunaDef Luna(Fighter f) => f==null?null:Content.Def(f)?.luna;
        static string Family(Fighter f) => Luna(f)?.family;
        static bool LunaOutside(out string reason) { reason="Fora do mapa."; return false; }
        static bool TechProtected(Fighter f) => f!=null&&(Luna(f)?.techProtect??false);
        static bool Unbreakable(Fighter f) => f!=null&&(Luna(f)?.invulnerable??false);
        static bool InFamily(Fighter f,Fighter owner) => f!=null&&owner!=null&&(f.id==owner.id||f.summoner==owner.id&&f.Alive&&Family(f)==Family(owner));
        // Robôs do Nikola: Blip/Blipware e Celinos vivem como invocações dele e não recebem comandos diretos enquanto
        // automáticos (não estão na linha do tempo).
        static bool Robot(Fighter f) => f!=null&&f.summoner!=0&&(f.def=="blip"||f.def=="blipware"||f.def=="celinos");
        static bool Piece(Fighter f) => f!=null&&Family(f)=="desland"&&f.def!=null&&f.def.StartsWith("peca-");
        LunaMachine Mechanism(Fighter owner) => S.Luna.machines.FirstOrDefault(x=>x.by==owner.id)??new LunaMachine{by=owner.id};
        void StoreMechanism(Fighter owner) { if(!S.Luna.machines.Any(x=>x.by==owner.id)) S.Luna.machines.Add(new LunaMachine{by=owner.id}); }

        // Contador compartilhado de engrenagens da família (fica no estado: os robôs leem o do dono mesmo morto).
        public int FamilyGears(int owner)
        {
            return S.Luna.families.FirstOrDefault(x=>x.owner==owner)?.gears??0;
        }
        void AddFamilyGears(int owner,int n)
        {
            var f=S.Luna.families.FirstOrDefault(x=>x.owner==owner);
            if(f==null) { f=new LunaFamily{owner=owner}; S.Luna.families.Add(f); }
            f.gears=Math.Max(0,f.gears+n);
        }
        // Casa livre (chão, sem lutador, sem água funda nem zona) mais perto de from.
        // skipSelf: a própria casa da origem não serve (saída da matriz sai para a casa AO LADO da engrenagem).
        Cell LunaSpot(Cell from,int radius,bool skipSelf=false)
        {
            var list=new List<Cell>();
            for(int x=-radius;x<=radius;x++) for(int z=-radius;z<=radius;z++)
            {
                if(Math.Abs(x)+Math.Abs(z)>radius) continue;
                var c=from+new Cell(x,z);
                if(skipSelf&&c==from) continue;
                if(!Board.Contains(S,c)||Board.At(S,c)!=null||!Board.Walkable(S,c,false)||S.At(c).lava||S.At(c).zone||S.At(c).Obj!=TileObj.Nada) continue;
                list.Add(c);
            }
            return list.Count==0?Cell.Unknown:list.OrderBy(c=>c.Distance(from)).ThenBy(c=>c.z).ThenBy(c=>c.x).First();
        }
        Fighter RobotOf(Fighter owner,string def)
        {
            if(owner==null) return null;
            if(def=="blip") return S.fighters.FirstOrDefault(x=>x.Alive&&x.summoner==owner.id&&(x.def=="blip"||x.def=="blipware"));
            return S.fighters.FirstOrDefault(x=>x.Alive&&x.summoner==owner.id&&x.def==def);
        }
        // ── Início da partida (depois do posicionamento): Blip + Celinos colados no Nikola; esfera colada no Nepturius ──
        void LunaStartup()
        {
            foreach(var f in S.fighters.Where(x=>x.Alive&&x.summoner==0&&x.def=="nikola").ToList()) { StoreMechanism(f); SpawnRobots(f); }
            foreach(var f in S.fighters.Where(x=>x.Alive&&x.summoner==0&&x.def=="nepturius").ToList()) { StoreMechanism(f); SpawnSphere(f); }
        }
        // Repõe SÓ o robô ausente (o transformado em Blipware conta como presente). Sem casa livre, não nasce nada.
        void SpawnRobots(Fighter owner,string only=null)
        {
            foreach(var def in new[]{"blip","celinos"}.Where(x=>only==null||x==only))
            {
                if(RobotOf(owner,def)!=null) continue;
                var spot=LunaSpot(owner.cell,2);
                if(spot==Cell.Unknown) continue;
                var r=Spawn(def,owner.team,spot,owner.id);
                Emit(new GameEvent{kind="invocar",actor=owner.id,target=r.id,info=def,cell=spot});
            }
        }
        Fighter SpawnSphere(Fighter owner)
        {
            var mechanism=Mechanism(owner);
            var alive=S.fighters.FirstOrDefault(x=>x.Alive&&x.def=="esfera-nepturius"&&x.summoner==owner.id);
            if(alive==null)
            {
                var spot=LunaSpot(owner.cell,2);
                if(spot==Cell.Unknown) return null;
                alive=Spawn("esfera-nepturius",owner.team,spot,owner.id);
                alive.pm=Content.Def(alive).pm;
                Emit(new GameEvent{kind="invocar",actor=owner.id,target=alive.id,info=alive.def,cell=spot});
            }
            mechanism.sphere=alive.id; mechanism.sphereBy=owner.id;
            return alive;
        }
        Fighter Sphere(Fighter owner) => owner==null?null:S.fighters.FirstOrDefault(x=>x.Alive&&x.def=="esfera-nepturius"&&x.summoner==owner.id);

        // ── Hook de fim de rodada (robôs órfãos e esfera enlouquecida) ──
        void LunaEndRound()
        {
            // Nikola morto: os robôs continuam agindo no fim da rodada.
            foreach(var owner in S.fighters.Where(x=>!x.Alive&&x.summoner==0&&x.def=="nikola").ToList()) ActRobots(owner);
            // Iniciativa do Viva Luna: a nova ordem vale na rodada seguinte (sem repetir nem perder o turno em curso).
            if(S.fighters.Any(x=>x.def=="bifia"||x.initBonus!=0)) RebuildTimelineKeepingSummons();
        }
        // Ordem com a iniciativa efetiva (ficha + bônus do Viva Luna), mantendo invocações controláveis na linha do tempo.
        public static int EffectiveInitiative(Fighter f) => Content.Def(f).initiative+(f?.initBonus??0);
        void RebuildTimelineKeepingSummons()
        {
            // Revisão 020: BuildTimeline só põe lutadores de jogador. Quem mais estava na ordem (invocações, o chefe e suas
            // entradas a cada N turnos) volta logo depois do mesmo lutador de jogador que vinha antes dele.
            var old=S.timeline.ToList();
            BuildTimeline();
            var players=new HashSet<int>(S.timeline);
            var after=new Dictionary<int,List<int>>(); int anchor=0;
            foreach(int id in old)
            {
                if(players.Contains(id)) { anchor=id; continue; }
                if(!(S.F(id) is Fighter x&&x.Alive)) continue;
                if(!after.TryGetValue(anchor,out var list)) after[anchor]=list=new List<int>();
                list.Add(id);
            }
            var line=new List<int>();
            if(after.TryGetValue(0,out var head)) line.AddRange(head);
            foreach(int id in S.timeline) { line.Add(id); if(after.TryGetValue(id,out var tail)) line.AddRange(tail); }
            S.timeline.Clear(); S.timeline.AddRange(line);
            S.active=0;
        }

        // ── Hooks de turno ──
        void LunaBeginTurn(Fighter f)
        {
            var mechanism=Mechanism(f);
            if(f==null||!f.Alive) return;
            var l=Luna(f);
            // Rede de segurança: forma do domínio sobrando sem contagem volta ao normal (as peças ficam, autor 24/10).
            if(f.def=="desland"&&f.form=="dominio"&&(mechanism.chessBy!=f.id||mechanism.chessTurns<=0)) EndChess(f);
            // Blipware: recupera PV no começo do turno controlável.
            if(Cfg(l?.healTurn,0)>0) Heal(f,Cfg(l.healTurn,0),f,"blipware");
            // Blip/Celinos acertam o PM pelo contador compartilhado do dono.
            RobotTurn(f);
            // Nikola vivo: funilaria (3 engrenagens, rodada sim, rodada não; autor 23/10) e respawn do robô ausente.
            if(f.def=="nikola")
            {
                Funilaria(f);
            }
            // Esfera: PM recarrega a cada turno do dono; aura passiva de quem começa o turno perto dela.
            if(f.def=="nepturius"&&Sphere(f) is Fighter sp)
            {
                sp.pm=Content.Def(sp).pm;
                mechanism.sphereUsed=false; mechanism.sphereCrazy=false;
            }
            SphereAura(f);
            // Começar o turno na teia da Viúva conta como pisar nela.
            WebTouch(f,f.cell,null);
        }
        void LunaEndTurn(Fighter f)
        {
            var mechanism=Mechanism(f);
            if(f==null) return;
            var l=Luna(f);
            // Nikola: os dois robôs agem, um após o outro, no fim do turno dele.
            if(f.def=="nikola"&&f.Alive) ActRobots(f);
            // Caixa da matriz: expira no fim do próximo turno do Nikola (turno atual mais seguinte).
            if(mechanism.box.by==f.id&&mechanism.box.hp>0&&--mechanism.box.turns<=0) CloseBox(f,"matriz-protecao-fim");
            // Viva Luna: conta os turnos da Bifia e desfaz no fim.
            foreach(var v in S.Luna.fields.Where(x=>x.by==f.id).ToList()) if(--v.turns<=0) EndVivaLuna(v);
            // Nepturius: nenhuma skill de esfera usada no turno → um cubo sorteado leva 2 de dano e cura o dono.
            if(f.def=="nepturius"&&f.Alive&&!mechanism.sphereUsed) AntimatterCube(f);
            // Viúva da Agulha: as aranhas agem depois dela.
            if(f.def=="viuva-da-agulha"&&f.summoner==0) SpidersAct(f);
            // Esfera enlouquecida (dono morto): age no fim de todo turno (autor 23/10).
            foreach(var machine in S.Luna.machines.Where(x=>x.sphereMad).ToList()) if(S.F(machine.by) is Fighter owner) MadSphere(owner);
            // Desland: peças do domínio agem uma de cada vez depois dele. Clone do eco conta os próprios turnos
            // (autor 25/10: dura 2 turnos); clone sem turno próprio conta os do Desland.
            if(f.def=="desland")
            {
                ChessPiecesAct(f);
                foreach(var c in S.Luna.clones.Where(x=>x.by==f.id&&S.F(x.id) is Fighter g&&Content.Def(g).noTurn).ToList()) if(--c.turns<=0) EndClone(c);
            }
            if(S.Luna.clones.FirstOrDefault(x=>x.id==f.id) is LunaClone own&&--own.turns<=0) EndClone(own);
        }
        // Morte: marcas do Celinos, direito do eco-de-aliado, peças do domínio e a esfera órfã.
        void LunaDeath(Fighter v,List<GameEvent> log)
        {
            var mechanism=Mechanism(v);
            S.Luna.marks.RemoveAll(m=>m.id==v.id);
            // Revisão 020: o Viva Luna conta os turnos da Bifia; caída ela não tem turno, então o efeito acaba agora
            // (antes o bônus de iniciativa/PM ficava para sempre).
            foreach(var field in S.Luna.fields.Where(x=>x.by==v.id).ToList()) EndVivaLuna(field);
            // Desland matou um inimigo (só ele mesmo: o clone não dispara a passiva de novo).
            var killer=striker??S.F(v.lastDamageBy);
            if(killer!=null&&killer.Alive&&killer.summoner==0&&killer.def=="desland"&&!S.Ally(killer.team,v.team)&&!S.Luna.rights.Contains(killer.id)&&!killer.cooldowns.Any(x=>x.skill=="eco-de-aliado"))
                S.Luna.rights.Add(killer.id);
            // Esfera do dono: enlouquece, vira neutra (ninguém comanda) e continua batendo em qualquer time.
            if(v.summoner==0&&v.def=="nepturius"&&Sphere(v) is Fighter sp&&sp.Alive)
            {
                mechanism.sphereMad=true; sp.team=MatchState.Neutral;
                Emit(new GameEvent{kind="status",info="esfera-orfa",actor=v.id,target=sp.id,cell=sp.cell});
            }
            // Viúva caída: a teia some e as aranhas também.
            if(v.def=="viuva-da-agulha"&&v.summoner==0)
            {
                S.Luna.webs.RemoveAll(w=>w.by==v.id);
                foreach(var spider in S.fighters.Where(x=>x.Alive&&x.def==Spider&&x.summoner==v.id).ToList()) { spider.hp=0; spider.deathProcessed=true; Emit(new GameEvent{kind="sumir",target=spider.id,cell=spider.cell}); }
            }
            // Peças do domínio: sem dono, somem.
            if(v.def=="desland") foreach(var p in S.fighters.Where(x=>x.Alive&&x.summoner==v.id&&x.def.StartsWith("peca-")).ToList()) { p.hp=0; p.deathProcessed=true; Emit(new GameEvent{kind="sumir",target=p.id,cell=p.cell}); }
        }

        // ── Nikola: funilaria (engrenagens) ──
        // Intervalo automático (autor 23/10): x1 a cada 4 rodadas, x3 a cada 3, x4 a cada 2, x5/x6 toda rodada.
        // Em equipe conta um jogador a menos (dupla x dupla = x3).
        public int GearEvery()
        {
            int players=Math.Max(2,S.players);
            int teams=S.fighters.Where(x=>x.summoner==0&&x.team!=MatchState.Neutral).Select(x=>x.team).Distinct().Count();
            if(teams>=2&&teams<players) players--;
            return players<=2?4:players==3?3:players==4?2:1;
        }
        void Funilaria(Fighter owner)
        {
            var l=Luna(owner); int drops=Cfg(l?.gearDrops,2), special=Cfg(l?.gearSpecial,20), every=Math.Max(1,Cfg(l?.gearEvery,GearEvery()));
            if((S.round-1)%every!=0) return;
            for(int i=0;i<drops;i++)
            {
                var cells=new List<Cell>();
                for(int x=0;x<S.width;x++) for(int z=0;z<S.height;z++)
                {
                    var c=new Cell(x,z); var t=S.At(c);
                    if(t.terrain!=Terrain.Livre||t.lava||t.zone||t.Obj!=TileObj.Nada) continue;
                    if(Board.At(S,c)!=null) continue;
                    if(S.Luna.gears.Any(g=>g.cell==Board.Index(S,c))) continue;
                    cells.Add(c);
                }
                if(cells.Count==0) return;
                var pick=cells[Dice.Range(S,cells.Count)];
                int value=Dice.Range(S,100)<special?2:Math.Max(1,Cfg(l?.gearValue,1));
                S.Luna.gears.Add(new Gear{cell=Board.Index(S,pick),owner=owner.id,value=value,hits=Math.Max(1,Cfg(l?.gearHits,3))});
                Emit(new GameEvent{kind="skill",info="funilaria",actor=owner.id,cell=pick,value=value});
            }
        }
        // Pisar na engrenagem: a família soma no contador; outro aliado ganha +1 PA e +2 PM por valor na hora; adversário
        // não coleta (só destrói batendo). Contador e drop ficam no estado (sobrevivem ao snapshot).
        void CollectGears(Fighter f,Cell cell,List<GameEvent> log)
        {
            WebTouch(f,cell,log);
            if(f==null||!f.Alive||S.Luna.gears.Count==0) return;
            int at=Board.Index(S,cell);
            var g=S.Luna.gears.FirstOrDefault(x=>x.cell==at);
            if(g==null) return;
            var owner=S.F(g.owner);
            // Adversário não coleta: o drop fica no chão (só ataques o destroem).
            // Autor 24/10: oponente (de qualquer time, invocação ou neutro) passa por cima sem pegar nada.
            if(owner==null||f.team!=owner.team&&!S.Ally(f.team,owner.team)) return;
            if(owner!=null&&(f.id==g.owner||f.summoner==g.owner))
            {
                AddFamilyGears(g.owner,g.value);
                S.Luna.gears.Remove(g);
            }
            else
            {
                // Outro aliado: ganho imediato, só até o fim deste turno (sem buff futuro).
                f.pa+=g.value;
                f.pm+=g.value*2;
                S.Luna.gears.Remove(g);
            }
            var e=new GameEvent{kind="skill",info="funilaria",actor=f.id,cell=cell,value=FamilyGears(g.owner)};
            (log??events).Add(e);
            // Aos N engrenagens o Blip vira Blipware (transformação permanente).
            if(owner!=null&&RobotOf(owner,"blip") is Fighter blip&&blip.def=="blip"&&FamilyGears(owner.id)>=Cfg(Luna(blip)?.gearMorph,10)) MorphBlipware(blip);
        }
        bool EnemyGearAt(Fighter by,Cell cell) => Board.Contains(S,cell)&&S.Luna.gears.Any(g=>g.cell==Board.Index(S,cell)&&S.F(g.owner) is Fighter owner&&!S.Ally(owner.team,by.team));
        // Ataque num drop inimigo tira 1 marcador por acerto/ação (3 acertos destroem), independente do dano.
        void GearHits(Fighter by,IEnumerable<Cell> cells,List<GameEvent> log)
        {
            if(by==null||S.Luna.gears.Count==0) return;
            foreach(var cell in cells.Distinct().ToList())
            {
                int at=Board.Index(S,cell);
                var g=S.Luna.gears.FirstOrDefault(x=>x.cell==at);
                if(g==null) continue;
                var owner=S.F(g.owner);
                if(owner!=null&&S.Ally(owner.team,by.team)) continue;
                g.hits--;
                (log??events).Add(new GameEvent{kind="skill",info="funilaria-golpe",actor=by.id,cell=cell,value=g.hits});
                if(g.hits<=0) { S.Luna.gears.Remove(g); (log??events).Add(new GameEvent{kind="skill",info="funilaria-fim",actor=by.id,cell=cell,value=0}); }
            }
        }

        // ── Blip/Celinos automáticos ──
        void RobotTurn(Fighter f)
        {
            if(!Robot(f)) return;
            var owner=S.F(f.summoner);
            int gears=owner!=null?FamilyGears(owner.id):0;
            var l=Luna(f);
            if(f.def=="blip") f.pm=Math.Max(0,Cfg(l?.gearPmBase,4)+gears);
            else if(f.def=="celinos") f.pm=Math.Max(0,Content.Def(f).pm+gears/Math.Max(1,Cfg(l?.gearStep,3))*Cfg(l?.gearPm,1));
            else if(f.def=="blipware") f.pm=Math.Max(0,Content.Def(f).pm);
        }
        void ActRobots(Fighter owner)
        {
            if(owner==null) return;
            foreach(var def in new[]{"blip","blipware","celinos"})
            {
                var r=RobotOf(owner,def);
                if(r==null||!r.Alive) continue;
                if(r.def=="blip") ActBlip(r,owner);
                else if(r.def=="celinos") ActCelinos(r,owner);
            }
        }
        // Blip: procura engrenagens e moedas alternando as categorias; sem dano; nada fica guardado nele.
        void ActBlip(Fighter r,Fighter owner)
        {
            ActRobotsBegin(r,owner);
            var gears=S.Luna.gears.Where(g=>g.owner==owner.id).Select(g=>new Cell(g.cell%S.width,g.cell/S.width)).ToList();
            var coins=Enumerable.Range(0,S.tiles.Length).Where(i=>S.tiles[i].coins>0).Select(i=>new Cell(i%S.width,i/S.width)).ToList();
            var first=r.ai++%2==0?gears:coins; var second=r.ai%2==0?coins:gears;
            var target=Closest(r,first)??Closest(r,second);
            if(target==null) return;
            WalkRobot(r,owner,target.Value);
        }
        public static int CelinosRadarRadius(MatchState state,Fighter robot)
        {
            if(robot==null||robot.def!="celinos"||!robot.Alive)return 0;
            var cfg=Luna(robot);int gears=state.Luna.families.FirstOrDefault(x=>x.owner==robot.summoner)?.gears??0;
            return Cfg(cfg?.revealBase,4)+gears/Math.Max(1,Cfg(cfg?.gearStep,3))*Cfg(cfg?.gearReveal,1);
        }
        void RevealCelinos()
        {
            // Autor (23/10): o radar mostra o escondido só ao time do robô; para os outros ele continua escondido.
            // Recalculado a cada ação: saiu do raio (ou deixou de estar escondido), sai do radar.
            var before=S.Luna.radar.ToList(); var now=new List<LunaSeen>();
            foreach(var robot in S.fighters.Where(x=>x.Alive&&x.def=="celinos").ToList())
                foreach(var hidden in S.fighters.Where(x=>x.Alive&&x.hidden&&!S.Ally(x.team,robot.team)&&Board.Gap(robot,x)<=CelinosRadarRadius(S,robot)).ToList())
                {
                    if(now.Any(r=>r.id==hidden.id&&S.Ally(r.team,robot.team))) continue;
                    now.Add(new LunaSeen{team=robot.team,id=hidden.id});
                    if(!before.Any(r=>r.id==hidden.id&&S.Ally(r.team,robot.team)))
                        Emit(new GameEvent{kind="revelar",info="radar",actor=robot.id,target=hidden.id,cell=hidden.cell});
                }
            S.Luna.radar=now;
        }
        public static bool OnRadar(MatchState s,int team,Fighter f) => f!=null&&f.hidden&&s.luna!=null&&s.luna.radar.Any(r=>r.id==f.id&&s.Ally(r.team,team));
        // Celinos: revela escondidos no raio, escolhe alvo (marcado > mais perto > menor PV), anda até o alcance de tiro e
        // dá 1 tiro por ativação (até 2 inimigos: o segundo a até 2 casas do primeiro).
        void ActCelinos(Fighter r,Fighter owner)
        {
            ActRobotsBegin(r,owner);
            var l=Luna(r); int gears=owner!=null?FamilyGears(owner.id):0, step=Math.Max(1,Cfg(l?.gearStep,3));
            int reveal=Cfg(l?.revealBase,4)+gears/step*Cfg(l?.gearReveal,1);
            int range=Cfg(l?.shotRange,3), chain=Math.Max(1,Cfg(l?.chainRange,2));
            int damage=Content.Def(r).skills.Length>0?Content.Def(r).skills[0].damage:1;
            damage+=gears/step*Cfg(l?.gearDamage,1);
            RevealCelinos();
            // Alvo marcado (autor 24/10): o Celinos fica travado nele até ele cair ou passarem 2 rodadas sem comando novo.
            S.Luna.marks.RemoveAll(m=>m.by==owner.id&&S.round-m.round>2);
            var marks=S.Luna.marks.Where(m=>m.by==owner.id&&S.F(m.id) is Fighter v&&v.Alive).Select(m=>S.F(m.id)).ToList();
            var visible=S.fighters.Where(x=>x.Alive&&!S.Ally(x.team,r.team)&&!Concealed(S,r.team,x)).ToList();
            var target=marks.FirstOrDefault()??visible.OrderBy(v=>Board.Gap(r,v)).ThenBy(v=>v.hp).ThenBy(v=>v.id).FirstOrDefault();
            if(target==null) return;
            // Anda até ficar no alcance de tiro (caminho legal, coletando o que passar).
            if(Board.Gap(r,target)>range)
            {
                var cells=Enumerable.Range(0,S.tiles.Length).Select(i=>new Cell(i%S.width,i/S.width))
                    .Where(c=>Board.At(S,c)==null&&Board.Gap(target,c)>=1&&Board.Gap(target,c)<=range);
                var best=Closest(r,cells);if(best.HasValue)WalkRobot(r,owner,best.Value);
            }
            RevealCelinos();
            if(!r.Alive||!target.Alive||Board.Gap(r,target)>range) return;
            var shot=Content.Def(r).skills.Length>0?Content.Def(r).skills[0].Clone():null;
            if(shot==null) return;
            shot.damage=damage; shot.maxRange=range;
            Strike(r,shot,target.cell);
            // Segundo inimigo, a até chain casas do primeiro (o total do tiro é 2 alvos).
            var second=S.fighters.Where(x=>x.Alive&&x!=target&&!S.Ally(x.team,r.team)&&!x.hidden&&Board.Gap(x,target)<=chain).OrderBy(x=>Board.Gap(x,target)).ThenBy(x=>x.hp).ThenBy(x=>x.id).FirstOrDefault();
            if(second!=null&&r.Alive) Strike(r,shot,second.cell);
        }
        void ActRobotsBegin(Fighter r,Fighter owner) { r.pa=0; RobotTurn(r); Emit(new GameEvent{kind="turno",actor=r.id,value=S.round}); }
        Cell? Closest(Fighter r,IEnumerable<Cell> cells)
        {
            if(cells==null) return null;
            var list=cells.Where(c=>Board.Contains(S,c)&&Board.At(S,c)==null).ToList();
            if(list.Count==0) return null;
            var dist=Board.Costs(S,r);
            var reachable=list.Where(c=>dist[Board.Index(S,c)]!=int.MaxValue&&dist[Board.Index(S,c)]<=r.pm).ToList();
            var possible=list.Where(c=>dist[Board.Index(S,c)]!=int.MaxValue).OrderBy(c=>dist[Board.Index(S,c)]).ThenBy(c=>c.z).ThenBy(c=>c.x).ToList();
            if(possible.Count==0)return null;var goal=possible[0];if(dist[Board.Index(S,goal)]<=r.pm)return goal;
            var probe=new Fighter{def=r.def,cell=goal,climber=r.climber};var back=Board.Costs(S,probe);
            var advance=Enumerable.Range(0,dist.Length).Where(i=>dist[i]>0&&dist[i]<=r.pm&&back[i]!=int.MaxValue).OrderBy(i=>back[i]).ThenBy(i=>dist[i]).ToList();
            return advance.Count>0?new Cell(advance[0]%S.width,advance[0]/S.width):(Cell?)null;
        }
        // Anda pelo caminho legal de Board.Path, coletando moeda e engrenagem de cada casa nova (Blip e Celinos).
        void WalkRobot(Fighter r,Fighter owner,Cell to)
        {
            if(to==r.cell) { CollectGears(r,r.cell,null); return; }
            var path=Board.Path(S,r,to,r.pm);
            if(path==null) { CollectGears(r,r.cell,null); return; }
            var walked=new List<Cell>{r.cell}; var log=new List<GameEvent>(); int units=r.pm;
            for(int i=1;i<path.Count;i++)
            {
                int cost=Board.BudgetStep(S,r,r.cell,path[i],units,false);
                if(cost<0) break;
                if(Board.At(S,path[i]) is Fighter o&&o!=r&&!Board.PassThrough(S,o)) break;
                units-=cost; r.cell=path[i]; walked.Add(r.cell);
                var t=S.At(r.cell);
                if(t.coins>0&&MatchState.Player(r.team)) { S.coins[r.team]+=t.coins; S.collected[S.Team(r.team)]+=t.coins; log.Add(new GameEvent{kind="moeda",actor=r.id,cell=r.cell,value=t.coins}); t.coins=0; }
                if(t.lava) { Burn(r,log); if(!r.Alive) break; }
                if(t.trap!=0&&!S.Ally(t.trap,r.team)) { if(Spring(r,t,log)||!r.Alive) break; }
                CollectGears(r,r.cell,log);
            }
            r.pm=Math.Max(0,units);
            if(walked.Count>1) Emit(new GameEvent{kind="mover",actor=r.id,path=walked.ToArray()});
            events.AddRange(log);
            if(!r.Alive) KO(r);
        }
        // Blip aos 10 de engrenagem: vira Blipware (ficha própria), mantém os 3 PV máximos e passa a ter turno controlável.
        void MorphBlipware(Fighter blip)
        {
            if(blip==null||blip.def!="blip") return;
            blip.def="blipware";
            int max=Content.Def(blip).hp;
            blip.bonusHp=Math.Min(3,max)-max;
            blip.hp=Math.Min(Math.Max(1,blip.hp),Content.MaxHp(blip));
            blip.used.Clear(); blip.usedMatch.Clear(); blip.cooldowns.Clear(); blip.charges.Clear();
            if(!S.timeline.Contains(blip.id)) S.timeline.Insert(Math.Min(S.timeline.Count,S.active+1),blip.id);
            Emit(new GameEvent{kind="morph",actor=blip.id,info="blipware",cell=blip.cell,value=FamilyGears(blip.summoner)});
        }

        // ── Skills novas: validação ANTES de qualquer custo/RNG/estado (Check) e resolução depois do custo pago (Resolve) ──
        // Toda skill com SkillDef.effect passa por aqui; o kind dela não decide nada.
        bool LunaCheck(Fighter f,SkillDef sk,Cell c,Cell c2,bool free,out string reason)
        {
            var mechanism=Mechanism(f);
            reason="";
            if(f==null||sk==null) { reason="Skill desconhecida."; return false; }
            if(!free)
            {
                if(f.channel>0) { reason="Canalizando o Desejo."; return false; }
                if(!string.IsNullOrEmpty(f.channeling)) { reason="Canalizando."; return false; }
                bool closed=mechanism.box.by==f.id&&mechanism.box.hp>0;
                if(closed&&sk.effect!="saida-matriz") { reason="A matriz está fechada: só dá para sair (saida-matriz)."; return false; }
                if(f.pa<Cost(f,sk,S,c,c2)) { reason="PA insuficientes."; return false; }
                if(sk.perTurn>0&&f.used.Count(u=>u==sk.id)>=sk.perTurn+f.buffs.Sum(b=>b.extra)+(Content.Form(f)?.extra??0)) { reason="Limite por turno atingido."; return false; }
                if(sk.perMatch>0&&f.usedMatch.Count(u=>u==sk.id)>=sk.perMatch) { reason="Já usada nesta partida."; return false; }
                if(f.cooldowns.Find(x=>x.skill==sk.id) is Charge wait) { reason=$"Recarga: falta{(wait.uses>1?"m":"")} {wait.uses} turno{(wait.uses>1?"s":"")}."; return false; }
                if(sk.minRound>S.round) { reason=$"Disponível a partir da rodada {sk.minRound}."; return false; }
            }
            var l=Luna(f); int step=Math.Max(1,Cfg(l?.gearStep,2)), gears=FamilyGears(f.id), bonus=gears/step;
            if((sk.effect=="eletaser"||sk.effect=="flecha-lunar"||sk.effect=="xeque-mate")&&Board.At(S,c)==null&&EnemyGearAt(f,c)){
                var probe=sk.Clone();probe.kind=SkillKind.Dano;if(sk.effect=="eletaser")probe.maxRange+=bonus*Cfg(l?.gearRange,1);return InRange(f,probe,c,out reason);
            }
            switch(sk.effect)
            {
                case "comando-blip":
                    if(RobotOf(f,"blip")!=null) return true;       // loja remota: não precisa de alvo
                    if(LunaSpot(f.cell,2)==Cell.Unknown) { reason="Sem casa livre ao lado para o Blip nascer."; return false; }
                    return true;
                case "comando-celinos":
                {
                    var cel=RobotOf(f,"celinos");
                    if(cel==null) { if(LunaSpot(f.cell,2)==Cell.Unknown) { reason="Sem casa livre ao lado para o Celinos nascer."; return false; } return true; }
                    if(!f.Alive) { reason="Com o dono caído o Celinos não marca ninguém."; return false; }
                    var v=Board.At(S,c);
                    if(v==null||S.Ally(v.team,f.team)||Concealed(S,f.team,v)) { reason="Marque um inimigo visível, em qualquer lugar do mapa."; return false; }
                    return true;
                }
                case "eletaser":
                {
                    var probe=sk.Clone(); probe.kind=SkillKind.Dano;
                    probe.maxRange=sk.maxRange+bonus*Cfg(l?.gearRange,1);
                    var v=Board.At(S,c);
                    if(v==null||v==f||S.Ally(v.team,f.team)) { reason="Escolha um inimigo."; return false; }
                    if(!InRange(f,probe,c,out reason)) return false;
                    return true;
                }
                case "matriz-protecao":
                    if(mechanism.box.by==f.id&&mechanism.box.hp>0) { reason="A matriz já está fechada."; return false; }
                    if(!Board.Occupies(f,c)) { reason="A caixa fecha sobre o próprio Nikola."; return false; }
                    return true;
                case "saida-matriz":
                {
                    if(!(mechanism.box.by==f.id&&mechanism.box.hp>0)) { reason="A matriz não está fechada."; return false; }
                    // Autor 23/10: não sai no mesmo turno em que fechou (turns só cai no fim do turno; 2 = turno em que fechou).
                    if(mechanism.box.turns>=2) { reason="A matriz acabou de fechar: só dá para sair a partir do próximo turno."; return false; }
                    var gear=S.Luna.gears.FirstOrDefault(g=>g.cell==Board.Index(S,c));
                    if(gear==null) { reason="Escolha uma engrenagem no chão, em qualquer ponto do mapa."; return false; }
                    if(LunaSpot(new Cell(gear.cell%S.width,gear.cell/S.width),3,true)==Cell.Unknown) { reason="Sem casa livre perto dessa engrenagem."; return false; }
                    return true;
                }
                case "flecha-lunar":
                {
                    var probe=sk.Clone(); probe.kind=SkillKind.Dano;
                    var v=Board.At(S,c);
                    if(v==null||v==f||S.Ally(v.team,f.team)||v.prisoned>0) { reason="Escolha um inimigo."; return false; }
                    if(!InRange(f,probe,c,out reason)) return false;
                    return true;
                }
                case "tiro-sonus":
                {
                    var probe=sk.Clone(); probe.kind=SkillKind.Cura;
                    var v=Board.At(S,c);
                    if(v==null||v==f||!S.Ally(v.team,f.team)||v.prisoned>0||TechProtected(v)||Unbreakable(v)) { reason="Escolha um aliado."; return false; }
                    if(!InRange(f,probe,c,out reason)) return false;
                    return true;
                }
                case "viva-luna":
                    if(!Board.Contains(S,c)) { reason="Escolha um ponto do mapa."; return false; }
                    return true;
                case "roque":
                    return RoqueCheck(f,sk,c,c2,out reason);
                case "xeque-mate":
                {
                    var probe=sk.Clone(); probe.kind=SkillKind.Dano;
                    var v=Board.At(S,c);
                    if(v==null||v==f||S.Ally(v.team,f.team)||v.prisoned>0) { reason="Escolha um inimigo."; return false; }
                    if(!InRange(f,probe,c,out reason)) return false;
                    return true;
                }
                case "eco-de-aliado":
                {
                    if(!S.Luna.rights.Contains(f.id)) { reason="O eco só abre depois que o Desland nocauteia um inimigo."; return false; }
                    var v=Board.At(S,c);
                    if(v==null||v==f||!S.Ally(v.team,f.team)||v.summoner!=0||!v.Alive) { reason="Escolha um aliado seu."; return false; }
                    if(LunaSpot(f.cell,2)==Cell.Unknown) { reason="Sem casa livre ao lado do Desland para o clone."; return false; }
                    return true;
                }
                case "dominio-xadrez":
                {
                    if(mechanism.chessBy==f.id&&mechanism.chessTurns>0) { reason="O domínio de xadrez já está no tabuleiro."; return false; }
                    int need=MissingPieces(f).Count;
                    if(FreeAround(f.cell,need).Count<need) { reason=$"Sem {need} casas livres em volta para as peças."; return false; }
                    return true;
                }
                case "mover-peao": case "mover-torre": case "mover-cavalo": case "mover-bispo": case "mover-rainha":
                {
                    if(f.form!="dominio") { reason="As peças só se movem com o domínio de xadrez ligado."; return false; }
                    if(c2==Cell.Unknown) return true;
                    var who=Board.At(S,c);
                    if(who==null) { reason="Escolha um personagem ou peça."; return false; }
                    // Autor 24/10: peça do xadrez só anda com o movimento dela (torre com Mover torre...).
                    if(who.def.StartsWith("peca-")&&EffectFor(who.def)!=sk.effect) { reason="Essa peça só se move com o movimento dela."; return false; }
                    if(!Board.Contains(S,c2)) { reason="Fora do mapa."; return false; }
                    if(Board.At(S,c2)!=null) { reason="A casa de destino precisa estar livre."; return false; }
                    if(!Board.Walkable(S,c2,Content.Def(who).flying)||!Board.Fits(S,who,c2)) { reason="Casa de destino bloqueada."; return false; }
                    if(!(IsPiece(who)&&who.summoner==f.id)&&(Fixed(who)||Unbreakable(who))) { reason="Esse personagem não pode ser movido."; return false; }
                    // Autor (23/10): mover um inimigo vale uma vez por domínio (ultimate).
                    if(!S.Ally(who.team,f.team)&&mechanism.chessFoe) { reason="Já moveu um inimigo neste domínio."; return false; }
                    if(!ChessMove(who.cell,c2,sk.effect)) { reason="Movimento fora da geometria dessa peça ou com o caminho tapado."; return false; }
                    return true;
                }
                case "puxao-blipware":
                {
                    var v=Board.At(S,c);
                    if(v==null||v==f||S.Ally(v.team,f.team)||v.prisoned>0) { reason="Escolha um inimigo."; return false; }
                    if(Board.Gap(f,v)>sk.maxRange) { reason="Fora de alcance."; return false; }
                    if(Fixed(v)||Unbreakable(v)) { reason="Esse alvo não é puxado."; return false; }
                    if(LunaSpot(f.cell,1)==Cell.Unknown) { reason="Sem casa livre ao lado do Blipware."; return false; }
                    return true;
                }
                case WebEffect: return CheckWeb(f,sk,c,out reason);
                case "mover-esfera":
                {
                    var sp=Sphere(f);
                    if(sp==null) { reason="Nenhuma esfera em campo."; return false; }
                    if(!Board.Contains(S,c)||Board.At(S,c)!=null||!Board.Walkable(S,c,false)||!Board.Fits(S,sp,c)) { reason="Casa bloqueada."; return false; }
                    if(c==sp.cell) { reason="A esfera já está aí."; return false; }
                    if(Board.Path(S,sp,c,sp.pm)==null) { reason=$"A esfera não chega lá com os {sp.pm} PM dela."; return false; }
                    return true;
                }
                case "agarrao-negro":
                case "explosao-antimateria":
                case "loucura-negra":
                    if(Sphere(f)==null) { reason="Nenhuma esfera em campo."; return false; }
                    if(sk.effect!="loucura-negra"&&mechanism.sphereCrazy) { reason="A esfera está em loucura: só pode se mover até o fim do turno."; return false; }
                    if(sk.effect=="loucura-negra"&&mechanism.sphereCrazy) { reason="A loucura negra já foi usada neste turno."; return false; }
                    return true;
            }
            reason="Habilidade sem efeito conhecido."; return false;
        }
        void ResolveLuna(Fighter f,SkillDef sk,Cell c,int slot,Cell c2)
        {
            StoreMechanism(f); var mechanism=Mechanism(f);
            var l=Luna(f); int step=Math.Max(1,Cfg(l?.gearStep,2)), gears=FamilyGears(f.id), bonus=gears/step;
            if((sk.effect=="eletaser"||sk.effect=="flecha-lunar"||sk.effect=="xeque-mate")&&Board.At(S,c)==null&&EnemyGearAt(f,c)){
                var probe=sk.Clone();probe.kind=SkillKind.Dano;if(sk.effect=="eletaser")probe.maxRange+=bonus*Cfg(l?.gearRange,1);Strike(f,probe,c);return;
            }
            switch(sk.effect)
            {
                case "comando-blip":
                {
                    var blip=RobotOf(f,"blip");
                    if(blip==null) { SpawnRobots(f,"blip"); break; }
                    // Loja remota: o Nikola compra neste turno; preço, pagamento e estoque continuam os da loja.
                    f.remoteShop=true;
                    Emit(new GameEvent{kind="skill",info="blip-loja",actor=f.id,target=blip.id,cell=blip.cell,value=S.coins[f.team]});
                    break;
                }
                case "comando-celinos":
                {
                    var cel=RobotOf(f,"celinos");
                    if(cel==null) { SpawnRobots(f,"celinos"); break; }
                    var v=Board.At(S,c);
                    S.Luna.marks.RemoveAll(m=>m.id==v.id||m.by==f.id);
                    S.Luna.marks.Add(new LunaMark{by=f.id,id=v.id,x=v.cell.x,z=v.cell.z,round=S.round});
                    Emit(new GameEvent{kind="skill",info="comando-celinos",actor=f.id,target=v.id,cell=v.cell});
                    break;
                }
                case "eletaser":
                {
                    var probe=sk.Clone(); probe.kind=SkillKind.Dano;
                    probe.maxRange=sk.maxRange+bonus*Cfg(l?.gearRange,1);
                    // Autor 24/10: engrenagem aumenta só o alcance do Eletaser (o dano é o da ficha).
                    probe.damage=sk.damage;
                    var v=Board.At(S,c); int before=events.Count;
                    Strike(f,probe,c);
                    bool hit=v!=null&&events.Skip(before).Any(e=>e.kind=="ataque"&&e.target==v.id&&e.value>0);
                    if(hit&&v.Alive&&v.pa>0&&!Immune(v)&&!TechProtected(v))
                    {
                        v.pa=Math.Max(1,v.pa/2);
                        Emit(new GameEvent{kind="status",info="eletaser",actor=f.id,target=v.id,value=v.pa,cell=v.cell});
                    }
                    break;
                }
                case "matriz-protecao":
                {
                    int hp=3+gears/2;
                    mechanism.box=new LunaBox{by=f.id,hp=hp,cell=Board.Index(S,f.cell),turns=2};
                    Emit(new GameEvent{kind="skill",info="matriz-protecao",actor=f.id,cell=f.cell,value=hp});
                    break;
                }
                case "saida-matriz":
                {
                    var gear=S.Luna.gears.FirstOrDefault(g=>g.cell==Board.Index(S,c));
                    var origin=f.cell; var from=new Cell(gear.cell%S.width,gear.cell/S.width);
                    var spot=LunaSpot(from,3,true);
                    S.Luna.gears.Remove(gear);
                    CloseBox(f,"caixa-voo");
                    f.cell=spot; Collect(f);
                    Emit(new GameEvent{kind="mover",actor=f.id,info="saida-matriz",path=new[]{origin,spot},cell=spot});
                    break;
                }
                case "flecha-lunar":
                {
                    var probe=sk.Clone(); probe.kind=SkillKind.Dano; var v=Board.At(S,c); int before=events.Count;
                    Strike(f,probe,c);
                    bool hit=v!=null&&events.Skip(before).Any(e=>e.kind=="ataque"&&e.target==v.id&&e.value>0);
                    if(!hit||v==null||!v.Alive||TechProtected(v)||Unbreakable(v)) break;
                    // Roubo real: tira no máximo o PM que o alvo tem, nada nasce do zero.
                    int take=Math.Min(1,Math.Max(0,v.pm));
                    v.pm-=take; if(take>0&&f.Alive) f.pm+=take;
                    Emit(new GameEvent{kind="status",info="flecha-lunar",actor=f.id,target=v.id,value=take,cell=v.cell});
                    break;
                }
                case "tiro-sonus":
                {
                    var v=Board.At(S,c); if(v==null) break;
                    int heal=Math.Max(0,f.pm/2);
                    if(heal>0) Heal(v,heal,f,sk.id);
                    // Zera o PM restante do aliado; o PM da Bifia fica intacto (não é roubo).
                    v.pm=0;
                    Emit(new GameEvent{kind="status",info="tiro-sonus",actor=f.id,target=v.id,value=0,cell=v.cell});
                    break;
                }
                case "viva-luna": VivaLuna(f,c,sk); break;
                case "roque": Roque(f,c,c2); break;
                case "xeque-mate":
                {
                    var v=Board.At(S,c); if(v==null) break;
                    int extra=S.fighters.Count(x=>x.Alive&&x!=f&&x!=v&&S.Ally(x.team,f.team)&&Board.Gap(x,v)<=1);
                    var probe=sk.Clone(); probe.kind=SkillKind.Dano; probe.damage=sk.damage+2*extra;
                    Strike(f,probe,c);
                    break;
                }
                case "eco-de-aliado":
                {
                    var ally=Board.At(S,c);
                    if(ally==null||CreateClone(f,ally)==null) break;
                    S.Luna.rights.Remove(f.id);
                    int cd=Math.Max(1,Cfg(l?.ecoCooldown,2));
                    f.cooldowns.Add(new Charge{skill=sk.id,uses=cd});
                    break;
                }
                case "dominio-xadrez": OpenChess(f); break;
                case "mover-peao": case "mover-torre": case "mover-cavalo": case "mover-bispo": case "mover-rainha":
                {
                    var who=Board.At(S,c);
                    if(who==null||Board.At(S,c2)!=null||!ChessMove(who.cell,c2,sk.effect)||who.def.StartsWith("peca-")&&EffectFor(who.def)!=sk.effect) break;
                    if(!S.Ally(who.team,f.team)) Mechanism(f).chessFoe=true;
                    var from=who.cell; who.cell=c2; Collect(who);
                    Emit(new GameEvent{kind="mover",actor=who.id,info=sk.effect,path=new[]{from,c2},cell=c2});
                    if(who.summoner==f.id) Promote(who,f);
                    break;
                }
                case "puxao-blipware":
                {
                    var v=Board.At(S,c); if(v==null) break;
                    var spot=LunaSpot(f.cell,1); if(spot==Cell.Unknown) break;
                    var from=v.cell; v.cell=spot; Collect(v);
                    Emit(new GameEvent{kind="mover",actor=f.id,target=v.id,info="puxao-blipware",path=new[]{from,spot},cell=spot});
                    break;
                }
                case "mover-esfera": MoveSphere(f,c); break;
                case "agarrao-negro": SphereGrab(f,false); break;
                case "loucura-negra":
                {
                    var sp=Sphere(f); if(sp==null) break;
                    sp.pm+=4; mechanism.sphereCrazy=true; mechanism.sphereUsed=true;
                    Emit(new GameEvent{kind="status",info="loucura-negra",actor=sp.id,value=sp.pm,cell=sp.cell});
                    break;
                }
                case "explosao-antimateria": SphereBlast(f,false); break;
                case WebEffect: CastWeb(f,c); break;
            }
        }
        // Caixa da matriz: fecha (quebra ou expira) e avisa a apresentação.
        void CloseBox(Fighter owner,string info)
        {
            var mechanism=Mechanism(owner);
            var box=mechanism.box;
            if(box==null||box.by==0) return;
            var target=S.F(box.by);
            if(target!=null) Emit(new GameEvent{kind="skill",info=info,actor=box.by,cell=target.cell,value=box.hp});
            mechanism.box=new LunaBox();
        }
        // Blip: loja remota liberada no Buy (o preço, o pagamento e o estoque continuam iguais).
        bool LunaShop(Fighter f) => f!=null&&f.remoteShop;
        // A matriz fechada impede andar (só saida-matriz resolve).
        bool LunaBlockMove(Fighter f,out string reason)
        {
            var mechanism=Mechanism(f);
            reason="";
            if(f==null||mechanism.box.by!=f.id||mechanism.box.hp<=0) return false;
            reason="A matriz está fechada: use saida-matriz (3 PA).";
            return true;
        }
        // Casas livres em volta (até raio casas), para as peças do domínio.
        List<Cell> FreeAround(Cell center,int radius)
        {
            var list=new List<Cell>();
            for(int x=-radius;x<=radius;x++) for(int z=-radius;z<=radius;z++)
            {
                if(Math.Abs(x)+Math.Abs(z)>radius) continue;
                var c=center+new Cell(x,z);
                if(!Board.Contains(S,c)||Board.At(S,c)!=null||!Board.Walkable(S,c,false)||S.At(c).lava||S.At(c).zone) continue;
                list.Add(c);
            }
            return list.OrderBy(c=>c.Distance(center)).ThenBy(c=>c.z).ThenBy(c=>c.x).ToList();
        }
        // ── Bifia: Viva Luna ──
        void VivaLuna(Fighter f,Cell center,SkillDef sk)
        {
            var l=Luna(f); int radius=Cfg(l?.vivaRadius,6), init=Cfg(l?.vivaInit,1), pm=Cfg(l?.vivaPm,2);
            int turns=Math.Max(1,Cfg(sk.turns>0?sk.turns:0,Cfg(l?.vivaTurns,2)));
            var field=new LunaField{by=f.id,center=Board.Index(S,center),radius=radius,turns=turns,init=init};
            var who=S.fighters.Where(x=>x.Alive&&!TechProtected(x)&&!Unbreakable(x)&&Board.Gap(x,center)<=radius).OrderBy(x=>x.id).ToList();
            foreach(var v in who)
            {
                bool ally=S.Ally(v.team,f.team);
                int delta=ally?init:-init;
                v.initBonus+=delta;
                int value=ally?pm:-pm;
                if(value!=0) AddBuff(v,new Buff{id="viva-luna",from=f.id.ToString(),turns=turns,pm=value});
                field.affected.Add(v.id); if(!ally) field.enemies.Add(v.id);
                Emit(new GameEvent{kind="status",info="viva-luna",actor=f.id,target=v.id,value=v.initBonus,cell=v.cell});
            }
            S.Luna.fields.Add(field);
            Emit(new GameEvent{kind="skill",info="viva-luna",actor=f.id,cell=center,value=radius});
            // Custa o turno inteiro: encerra e zera o PM (o PM da Bifia também vai a 0).
            f.pa=0; f.pm=0; f.endedTurn=true;
        }
        void EndVivaLuna(LunaField field)
        {
            S.Luna.fields.Remove(field);
            foreach(var id in field.affected)
            {
                var f=S.F(id); if(f==null) continue;
                f.initBonus-=field.enemies.Contains(id)?-field.init:field.init;
                f.buffs.RemoveAll(b=>b.id=="viva-luna"&&b.from==field.by.ToString());
                Emit(new GameEvent{kind="skill",info="viva-luna-fim",actor=field.by,target=f.id,cell=f.cell,value=EffectiveInitiative(f)});
            }
            field.affected.Clear();
        }

        // ── Desland: roque ──
        // Dois aliados na mesma linha/coluna, separados por 3 a 8 casas, caminho livre. O primeiro é o rei (anda 2 na
        // direção do segundo) e o segundo vai para a casa ao lado da origem do rei (origem + 1 passo).
        bool RoqueCheck(Fighter f,SkillDef sk,Cell c,Cell c2,out string reason)
        {
            reason="";
            // Autor (23/10, 2ª decisão): quaisquer duas unidades aliadas, a qualquer distância do Desland.
            var king=Board.At(S,c); if(c2==Cell.Unknown) { reason=king!=null&&S.Ally(king.team,f.team)&&!Fixed(king)?"":"Escolha um aliado que possa se mover."; return reason==""; } var rook=Board.At(S,c2);
            if(king==null||rook==null||king==rook) { reason="Escolha dois aliados."; return false; }
            if(!S.Ally(king.team,f.team)||!S.Ally(rook.team,f.team)) { reason="Os dois têm de ser seus aliados."; return false; }
            int dx=rook.cell.x-king.cell.x, dz=rook.cell.z-king.cell.z;
            if(dx!=0&&dz!=0) { reason="Os dois aliados precisam estar na mesma linha ou coluna."; return false; }
            int dist=Math.Abs(dx)+Math.Abs(dz);
            if(dist<3||dist>8) { reason="Os dois aliados ficam de 3 a 8 casas um do outro."; return false; }
            var dir=new Cell(Math.Sign(dx),Math.Sign(dz));
            // Caminho livre entre os dois (as casas do meio, sem lutador nem obstáculo).
            for(var x=king.cell+dir;x!=rook.cell;x=x+dir)
                if(!Board.Contains(S,x)||!Board.Walkable(S,x,false)||Board.At(S,x)!=null) { reason="O caminho entre os dois está tapado."; return false; }
            var kingTo=king.cell+new Cell(dir.x*2,dir.z*2);
            var rookTo=king.cell+dir;
            if(!RoqueSpot(king,kingTo,rook.cell)) { reason="O rei não tem casa livre 2 passos à frente."; return false; }
            if(!RoqueSpot(rook,rookTo,king.cell)) { reason="O segundo aliado não tem casa livre ao lado da origem do rei."; return false; }
            if(Fixed(king)||Fixed(rook)) { reason="Um dos dois não pode ser movido."; return false; }
            return true;
        }
        bool RoqueSpot(Fighter who,Cell to,Cell other)
        {
            if(!Board.Contains(S,to)||Board.At(S,to)!=null||!Board.Walkable(S,to,Content.Def(who).flying)||!Board.Fits(S,who,to)) return false;
            return true;
        }
        void Roque(Fighter f,Cell c,Cell c2)
        {
            var king=Board.At(S,c); var rook=Board.At(S,c2);
            if(king==null||rook==null) return;
            var dir=new Cell(Math.Sign(rook.cell.x-king.cell.x),Math.Sign(rook.cell.z-king.cell.z));
            var kingFrom=king.cell; var rookFrom=rook.cell;
            var kingTo=kingFrom+new Cell(dir.x*2,dir.z*2); var rookTo=kingFrom+dir;
            if(!RoqueSpot(king,kingTo,rookFrom)||!RoqueSpot(rook,rookTo,kingFrom)) { GroundMiss(f,Content.Skill(f,"roque"),c); return; }
            king.cell=kingTo; rook.cell=rookTo; Collect(king); Collect(rook);
            Emit(new GameEvent{kind="mover",actor=king.id,info="roque",path=new[]{kingFrom,kingTo},cell=kingTo});
            Emit(new GameEvent{kind="mover",actor=rook.id,info="roque",path=new[]{rookFrom,rookTo},cell=rookTo});
        }

        // ── Desland: clones do eco-de-aliado ──
        Fighter CreateClone(Fighter owner,Fighter ally)
        {
            var spot=LunaSpot(owner.cell,2);
            if(spot==Cell.Unknown) return null;
            var clone=Spawn(ally.def,owner.team,spot,owner.id);
            clone.roll=ally.roll;
            clone.hp=Content.MaxHp(clone); clone.pa=Content.Def(clone).pa; clone.pm=Content.Def(clone).pm;
            if(!Content.Def(clone).noTurn&&!S.timeline.Contains(clone.id)) S.timeline.Insert(Math.Min(S.timeline.Count,S.active+1),clone.id);
            S.Luna.clones.Add(new LunaClone{id=clone.id,by=owner.id,turns=Math.Max(1,Cfg(Luna(owner)?.cloneTurns,1))});
            Emit(new GameEvent{kind="invocar",actor=owner.id,target=clone.id,info=ally.def,cell=spot,value=1});
            return clone;
        }
        void EndClone(LunaClone clone)
        {
            S.Luna.clones.Remove(clone);
            var f=S.F(clone.id); if(f==null||!f.Alive) return;
            f.hp=0; f.deathProcessed=true;
            Emit(new GameEvent{kind="sumir",target=f.id,cell=f.cell});
            AnyDeath(f);
        }

        // ── Desland: domínio de xadrez ──
        static readonly string[] ChessOrder={"peca-peao","peca-torre","peca-cavalo","peca-bispo","peca-rainha"};
        void OpenChess(Fighter f)
        {
            var mechanism=Mechanism(f);
            // Autor 24/10: as peças ficam no tabuleiro depois da ult; ultar de novo só repõe as que caíram.
            var missing=MissingPieces(f);
            var spots=FreeAround(f.cell,missing.Count);
            if(spots.Count<missing.Count) return;
            mechanism.chessBy=f.id; mechanism.chessTurns=Math.Max(1,Cfg(Luna(f)?.chessTurns,2)); f.form="dominio";
            mechanism.chess.Clear(); mechanism.chessFoe=false; mechanism.chessEdge=FarEdge(f.cell);
            mechanism.chess.AddRange(S.fighters.Where(x=>x.Alive&&x.summoner==f.id&&x.def.StartsWith("peca-")).Select(x=>x.id));
            for(int i=0;i<missing.Count;i++)
            {
                var p=Spawn(missing[i],f.team,spots[i],f.id);
                mechanism.chess.Add(p.id);
                Emit(new GameEvent{kind="invocar",actor=f.id,target=p.id,info=p.def,cell=spots[i]});
            }
            SetForm(f,"dominio",mechanism.chessTurns,"dominio-xadrez");
            Emit(new GameEvent{kind="skill",info="dominio-xadrez",actor=f.id,cell=f.cell,value=mechanism.chessTurns,path=spots.Take(missing.Count).ToArray()});
        }
        // Peças que faltam no tabuleiro (a rainha promovida conta como rainha; o peão que virou rainha volta).
        List<string> MissingPieces(Fighter f) => ChessOrder.Where(def=>!S.fighters.Any(x=>x.Alive&&x.summoner==f.id&&x.def==def)).ToList();
        // Fim do domínio: as peças continuam em campo (paradas até a próxima ult) e somem só se o Desland cair.
        void EndChess(Fighter owner)
        {
            var mechanism=Mechanism(owner);
            mechanism.chessTurns=0; mechanism.chessBy=0; if(owner.form=="dominio")owner.form=null;
            if(owner!=null&&owner.Alive&&owner.form=="dominio") SetForm(owner,Content.Def(owner).startForm,0,"dominio-xadrez");
            Emit(new GameEvent{kind="skill",info="dominio-xadrez-fim",actor=owner?.id??0,cell=owner?.cell??Cell.Unknown});
        }
        // Uma peça de cada vez, depois do turno do Desland: captura o primeiro inimigo na direção do movimento; sem captura, avança.
        void ChessPiecesAct(Fighter owner)
        {
            var mechanism=Mechanism(owner);
            if(owner==null||mechanism.chessBy!=owner.id||mechanism.chessTurns<=0) return;
            // Alvos já pegos ou ameaçados por outra peça nesta rodada: as próximas preferem outro (não focam no mesmo).
            var claimed=new HashSet<int>();
            foreach(var def in ChessOrder)
            {
                foreach(var p in S.fighters.Where(x=>x.Alive&&x.def==def&&x.summoner==owner.id).ToList())
                {
                    var hit=p.def=="peca-cavalo"?null:PieceHunt(p,owner);
                    if(hit!=null) { claimed.Add(hit.id); ChessCapture(p,owner); continue; }
                    ChessAdvance(p,owner,claimed);
                }
            }
            if(--mechanism.chessTurns<=0) EndChess(owner);
        }
        // Ataques das peças (autor 24/10): bispo até 3 casas nas diagonais, peão e rainha colados, torre colada e atordoa;
        // o cavalo ataca em área onde cai (ChessAdvance). Peças ignoram terreno e condições.
        static bool IsPiece(Fighter f) => f!=null&&f.def!=null&&f.def.StartsWith("peca-");
        bool ChessCapture(Fighter p,Fighter owner)
        {
            if(p.def=="peca-cavalo") return false;
            var hit=PieceHunt(p,owner); if(hit==null) return false;
            var skill=PieceHit(owner,p);
            Emit(new GameEvent{kind="skill",info="dominio-xadrez",actor=p.id,target=hit.id,cell=hit.cell});
            Strike(p,skill,hit.cell);
            if(p.def=="peca-torre"&&hit.Alive&&hit.lastDamageBy==p.id&&!FormImmune(hit)&&!Unstoppable(hit))
            {
                hit.stunned=Math.Max(hit.stunned,1); hit.stunFrom="agarrao-negro";
                Emit(new GameEvent{kind="status",info="agarrao-negro",actor=p.id,target=hit.id,value=1,cell=hit.cell});
            }
            return true;
        }
        // Torre: +1 de armadura para aliados colados nela.
        int TowerArmor(Fighter f) => f!=null&&!IsPiece(f)&&S.fighters.Any(t=>t.Alive&&t.def=="peca-torre"&&S.Ally(t.team,f.team)&&Board.Gap(t,f)<=1)?1:0;
        // Autor 23/10: rainha 4 de dano, as outras peças 2 (queenDamage/pieceDamage, editáveis).
        SkillDef PieceHit(Fighter owner,Fighter piece)
        {
            var l=Luna(owner); int dmg=piece?.def=="peca-rainha"?Math.Max(1,Cfg(l?.queenDamage,4)):Math.Max(1,Cfg(l?.pieceDamage,2));
            return new SkillDef{ id="dominio-xadrez", name="Domínio de xadrez", kind=SkillKind.Dano, pa=0, minRange=1, maxRange=1, damage=dmg, precise=true };
        }
        // Primeiro inimigo encontrado na direção legal da peça (torre/bispo/rainha param no primeiro obstáculo; cavalo salta).
        Fighter PieceHunt(Fighter p,Fighter owner)
        {
            bool Foe(Fighter v) => v!=null&&v.Alive&&!S.Ally(v.team,owner.team)&&!Unbreakable(v);
            if(p.def=="peca-bispo")
            {
                // Até 3 casas nas diagonais; para no primeiro personagem (o terreno não tapa).
                foreach(var d in ChessDirs(p.def))
                    for(int i=1;i<=3;i++) { var v=Board.At(S,p.cell+new Cell(d.x*i,d.z*i)); if(v==null) continue; if(Foe(v)) return v; break; }
                return null;
            }
            // Peão, rainha e torre: golpe colado.
            return S.fighters.Where(v=>Foe(v)&&Board.Gap(p,v)==1).OrderBy(v=>v.hp).ThenBy(v=>v.id).FirstOrDefault();
        }
        // Sem captura: vai atrás do inimigo mais próximo como no xadrez (torre/bispo/rainha deslizam quantas casas quiserem;
        // peão anda 1 e cavalo salta em L). Peão que chega à borda oposta ao Desland vira rainha.
        // Autor 24/10: movimento otimizado. Cada casa possível vale pelo que a peça ataca DE LÁ (cavalo: quem fica colado
        // onde cai; as outras: o alvo do próximo golpe), preferindo o inimigo mais frágil e um que outra peça ainda não pegou.
        // Sem ataque possível, entra na rota do inimigo mais fácil para o movimento dela.
        void ChessAdvance(Fighter p,Fighter owner,HashSet<int> claimed=null)
        {
            claimed??=new HashSet<int>();
            var foes=S.fighters.Where(x=>x.Alive&&!S.Ally(x.team,owner.team)&&!Unbreakable(x)).ToList();
            if(foes.Count==0) return;
            var dirs=ChessDirs(p.def);
            bool slides=p.def=="peca-torre"||p.def=="peca-bispo"||p.def=="peca-rainha";
            var options=new List<Cell>();
            foreach(var d in dirs)
                for(var c=p.cell+d;Board.Contains(S,c);c=c+d)
                {
                    if(Board.At(S,c)!=null) break;
                    if(!ChessMove(p.cell,c,EffectFor(p.def),p)) break;
                    if(Board.Walkable(S,c,false)) options.Add(c); else if(!slides) break;
                    if(!slides) break;
                }
            if(options.Count==0) return;
            var origin=p.cell; Cell? best=null; double top=double.MinValue;
            foreach(var c in options)
            {
                p.cell=c; double score;
                var threats=p.def=="peca-cavalo"?foes.Where(v=>Board.Gap(v,p)==1).ToList():PieceHunt(p,owner) is Fighter t?new List<Fighter>{t}:new List<Fighter>();
                if(threats.Count>0) score=1000+threats.Count*100-threats.Min(v=>v.hp)*5-(threats.All(v=>claimed.Contains(v.id))?300:0);
                else
                {
                    // Rota: quanto falta para o inimigo mais fácil (menor PV, depois mais perto) ficar na linha da peça.
                    score=-foes.Min(v=>Board.Gap(v,p)*3+v.hp+(claimed.Contains(v.id)?6:0));
                }
                if(score>top) { top=score; best=c; }
            }
            p.cell=origin;
            if(best==null) return;
            if(p.def=="peca-cavalo") foreach(var v in foes.Where(v=>Board.Gap(v,best.Value)==1)) claimed.Add(v.id);
            var from=p.cell; p.cell=best.Value;
            Emit(new GameEvent{kind="mover",actor=p.id,info="dominio-xadrez",path=new[]{from,p.cell},cell=p.cell});
            if(p.def=="peca-cavalo") HorseLanding(p,owner);
            Promote(p,owner);
        }
        // Borda de promoção: a mais longe do Desland quando o domínio abre (0: x=0, 1: x=fim, 2: z=0, 3: z=fim).
        int FarEdge(Cell c)
        {
            var d=new[]{ c.x, S.width-1-c.x, c.z, S.height-1-c.z };
            int best=0; for(int i=1;i<4;i++) if(d[i]>d[best]) best=i; return best;
        }
        void Promote(Fighter p,Fighter owner)
        {
            if(p==null||!p.Alive||p.def!="peca-peao"||owner==null) return;
            var mechanism=Mechanism(owner); int e=mechanism.chessEdge;
            bool end=e==0?p.cell.x==0:e==1?p.cell.x==S.width-1:e==2?p.cell.z==0:p.cell.z==S.height-1;
            if(!end) return;
            var at=p.cell; p.hp=0; p.deathProcessed=true; Emit(new GameEvent{kind="sumir",target=p.id,cell=at});
            mechanism.chess.Remove(p.id);
            var q=Spawn("peca-rainha",p.team,at,owner.id); mechanism.chess.Add(q.id);
            Emit(new GameEvent{kind="invocar",actor=owner.id,target=q.id,info=q.def,cell=at});
        }
        static string EffectFor(string def) => def=="peca-peao"?"mover-peao":def=="peca-torre"?"mover-torre":def=="peca-cavalo"?"mover-cavalo":def=="peca-bispo"?"mover-bispo":"mover-rainha";
        static Cell[] ChessDirs(string def)
        {
            var orth=new[]{ new Cell(-1,0), new Cell(1,0), new Cell(0,-1), new Cell(0,1) };
            var diag=new[]{ new Cell(-1,-1), new Cell(1,-1), new Cell(-1,1), new Cell(1,1) };
            var horse=new[]{ new Cell(-1,-2), new Cell(1,-2), new Cell(-1,2), new Cell(1,2), new Cell(-2,-1), new Cell(2,-1), new Cell(-2,1), new Cell(2,1) };
            return def=="peca-torre"?orth:def=="peca-bispo"?diag:def=="peca-cavalo"?horse:def=="peca-peao"?orth:orth.Concat(diag).ToArray();
        }
        // Geometria das skills de movimento (dois cliques): peão anda 1 em linha reta, cavalo em L com salto, torre em
        // linha/coluna, bispo na diagonal e rainha nas duas, sem atravessar ocupado ou obstáculo.
        // Movimento do xadrez passa por cima de obstáculo, relevo e água: só personagem no caminho tapa.
        bool ChessMove(Cell from,Cell to,string effect,Fighter mover=null)
        {
            if(from==to) return false;
            // Autor 24/10: o caminho de qualquer movimento do xadrez (peça ou personagem) ignora terreno, altura e visão;
            // a casa de destino do personagem continua precisando ser andável (LunaCheck).
            bool piece=true;
            int dx=to.x-from.x, dz=to.z-from.z, adx=Math.Abs(dx), adz=Math.Abs(dz);
            switch(effect)
            {
                case "mover-peao": return adx+adz==1&&(piece||Board.Walkable(S,to,false));
                case "mover-cavalo": return adx*adz==2&&(piece||Board.Walkable(S,to,false));
                case "mover-torre": return (dx==0||dz==0)&&ChessClear(from,to,piece);
                case "mover-bispo": return adx==adz&&adx>0&&ChessClear(from,to,piece);
                case "mover-rainha": return (dx==0||dz==0||adx==adz)&&ChessClear(from,to,piece);
            }
            return false;
        }
        bool ChessClear(Cell from,Cell to,bool piece=false)
        {
            var dir=new Cell(Math.Sign(to.x-from.x),Math.Sign(to.z-from.z));
            if(dir.x==0&&dir.z==0) return false;
            for(var c=from+dir;c!=to;c=c+dir)
                if(!Board.Contains(S,c)||!piece&&!Board.Walkable(S,c,false)||Board.At(S,c)!=null) return false;
            return true;
        }
        // Cavalo cai: dano em todo inimigo colado na casa onde ele parou.
        void HorseLanding(Fighter p,Fighter owner)
        {
            var hits=S.fighters.Where(v=>v.Alive&&!S.Ally(v.team,owner.team)&&!Unbreakable(v)&&Board.Gap(v,p)==1).ToList();
            if(hits.Count==0) return;
            Emit(new GameEvent{kind="skill",info="dominio-xadrez",actor=p.id,cell=p.cell,value=hits.Count});
            var skill=PieceHit(owner,p);
            foreach(var v in hits) if(v.Alive&&p.Alive) Strike(p,skill,v.cell);
        }

        // ── Nepturius: esfera de antimatéria ──
        void MoveSphere(Fighter owner,Cell to)
        {
            var mechanism=Mechanism(owner);
            var sp=Sphere(owner); if(sp==null) return;
            var path=Board.Path(S,sp,to,sp.pm);
            mechanism.sphereUsed=true;
            if(path==null) { Emit(new GameEvent{kind="ataque",actor=sp.id,info="mover-esfera",cell=to,aim=to,target=0,value=-1}); return; }
            var walked=new List<Cell>{sp.cell}; int units=sp.pm;
            for(int i=1;i<path.Count;i++)
            {
                int cost=Board.BudgetStep(S,sp,sp.cell,path[i],units,false);
                if(cost<0||Board.At(S,path[i])!=null) break;
                units-=cost; sp.cell=path[i]; walked.Add(sp.cell);
            }
            sp.pm=Math.Max(0,units);
            if(walked.Count>1) Emit(new GameEvent{kind="mover",actor=sp.id,info="mover-esfera",path=walked.ToArray(),cell=sp.cell});
        }
        // Dono e esfera nunca se afetam (autor 23/10); num espelho a esfera de um Nepturius pega o outro normalmente.
        static bool SphereSpares(Fighter sp,Fighter x) => x==sp||x.id==sp.summoner||Unbreakable(x);
        // Aura passiva: quem começa o turno até N casas da esfera perde PA e PM (aliados e inimigos; o dono não).
        void SphereAura(Fighter f)
        {
            if(f==null||!f.Alive||TechProtected(f)||Unbreakable(f)) return;
            foreach(var sp in S.fighters.Where(x=>x.Alive&&x.def=="esfera-nepturius"&&!SphereSpares(x,f)).ToList()) {
                var cfg=Luna(S.F(sp.summoner))??Luna(sp);
                if(Board.Gap(sp,f)>Cfg(cfg?.auraRange,4)) continue;
                int pa=Math.Min(f.pa,Cfg(cfg?.auraPa,2)),pm=Math.Min(f.pm,Cfg(cfg?.auraPm,1));
                f.pa-=pa; f.pm-=pm; Emit(new GameEvent{kind="status",info="aura-nepturius",actor=sp.id,target=f.id,value=pa,cell=f.cell});
            }
        }
        // Agarrão: até N outros personagens mais próximos até R casas (aliados incluídos); stun de 1 turno.
        void SphereGrab(Fighter owner,bool mad)
        {
            var mechanism=Mechanism(owner);
            var sp=Sphere(owner); if(sp==null) return;
             var l=Luna(owner)??Luna(sp);
            int count=Cfg(l?.grabCount,4), range=Cfg(l?.grabRange,4);
            var targets=S.fighters.Where(x=>x.Alive&&!SphereSpares(sp,x)&&x.prisoned==0&&Board.Gap(sp,x)<=range&&!TechProtected(x)&&!Unstoppable(x)&&!(Content.Form(x)?.intangible??false)&&(!Immune(x)||Content.Def(x).exposed))
                .OrderBy(x=>Board.Gap(sp,x)).ThenBy(x=>x.id).Take(count).ToList();
            Emit(new GameEvent{kind="skill",info="agarrao-negro",actor=sp.id,cell=sp.cell,value=targets.Count});
            var log=new List<GameEvent>();
            foreach(var v in targets)
            {
                // Imunidades: proteção tecnológica, intangível e imparável seguram o agarrão (o dano não existe aqui).
                if(TechProtected(v)||Unstoppable(v)||(Content.Form(v)?.intangible??false)||Immune(v)&&!Content.Def(v).exposed) continue;
                v.stunned=Math.Max(v.stunned,1); v.stunFrom="agarrao-negro";
                var stun=new GameEvent{kind="status",info="agarrao-negro"}; stun.actor=sp.id; stun.target=v.id; stun.value=1; stun.cell=v.cell;
                log.Add(stun);
            }
            events.AddRange(log);
            mechanism.sphereUsed=true;
        }
        // Explosão de antimatéria: dano em TODOS até N casas da esfera (aliados incluídos); a esfera é imune.
        void SphereBlast(Fighter owner,bool mad)
        {
            var mechanism=Mechanism(owner);
            var sp=Sphere(owner); if(sp==null) return;
            var damageOwner=mad?null:owner; var l=Luna(owner)??Luna(sp);
            int radius=Cfg(l?.blastRadius,3), dmg=Math.Max(1,Cfg(l?.blastDamage,2));
            // Nepturius vivo: a esfera bate mais forte (autor 23/10).
            if(owner.Alive) dmg+=Cfg(l?.sphereBonus,2);
            var targets=S.fighters.Where(x=>x.Alive&&!SphereSpares(sp,x)&&x.prisoned==0&&Board.Gap(sp,x)<=radius).ToList();
            Emit(new GameEvent{kind="skill",info="explosao-antimateria",actor=sp.id,cell=sp.cell,value=radius});
            var log=new List<GameEvent>();
            GearHits(mad?sp:owner,Area(sp.cell,radius,false),log);
            foreach(var v in targets)
            {
                int dealt=Ghost(v)||Unbreakable(v)?0:dmg;
                log.Add(new GameEvent{kind="dano",info="explosao-antimateria",actor=sp.id,target=v.id,value=dealt,cell=v.cell});
                Hurt(v,dealt,log,damageOwner);
            }
            events.AddRange(log);
            foreach(var v in targets) if(!v.Alive) KO(v);
            mechanism.sphereUsed=true;
        }
        // Cubo sorteado no fim do turno do dono (Host): 2 de dano em quem estiver nele e cura 2 PV do Nepturius.
        void AntimatterCube(Fighter owner)
        {
            var l=Luna(owner);
            var c=new Cell(Dice.Range(S,S.width),Dice.Range(S,S.height));
            var v=Board.At(S,c); int dealt=0;var log=new List<GameEvent>();
            GearHits(owner,new[]{c},log);
            if(v!=null&&!Unbreakable(v)&&v!=owner)
            {
                dealt=Math.Max(1,Cfg(l?.cubeDamage,2));
                Hurt(v,dealt,log,owner);
                if(!v.Alive) KO(v);
            }
            events.AddRange(log);
            Emit(new GameEvent{kind="skill",info="nepturius-cubo",actor=owner.id,target=v!=null?v.id:0,cell=c,value=dealt});
            if(dealt>0&&owner.Alive) Heal(owner,Cfg(l?.cubeHeal,2),owner,"nepturius-cubo");
        }
        // Quando o dono sofre ataque: a esfera vai para a casa livre mais perto dele e dá +1 de armadura (renovável, 1 turno).
        void SphereGuard(Fighter v)
        {
            var mechanism=Mechanism(v);
            if(v==null||v.def!="nepturius"||v.summoner!=0) return;
            var sp=Sphere(v); if(sp==null) return;
            var spot=LunaSpot(v.cell,2,true);
            if(spot!=Cell.Unknown&&spot!=sp.cell)
            {
                var from=sp.cell; sp.cell=spot;
                Emit(new GameEvent{kind="mover",actor=sp.id,info="esfera",path=new[]{from,spot},cell=spot});
            }
            var l=Luna(v); int armor=Math.Max(1,Cfg(l?.armorBuff,1)), turns=Math.Max(1,Cfg(l?.armorTurns,1));
            v.buffs.RemoveAll(b=>b.id=="nepturius-esfera");
            AddBuff(v,new Buff{id="nepturius-esfera",from="nepturius",turns=turns,armor=armor});
            Emit(new GameEvent{kind="status",info="nepturius-esfera",actor=sp.id,target=v.id,value=armor,cell=v.cell});
        }
        // Dono morto: a esfera enlouquece e, no fim de todo turno, vaga até 8 PM para uma casa sorteada e usa um ataque
        // sorteado (explosão ou agarrão).
        void MadSphere(Fighter owner)
        {
            var sp=Sphere(owner); if(sp==null||!sp.Alive) return;
            sp.team=MatchState.Neutral;
            sp.pm=8;
            var costs=Board.Costs(S,sp);
            var reach=Enumerable.Range(0,costs.Length).Where(i=>costs[i]>0&&costs[i]<=sp.pm&&Board.At(S,new Cell(i%S.width,i/S.width))==null).ToList();
            if(reach.Count>0)
            {
                int index=reach[Dice.Range(S,reach.Count)];
                MoveSphere(owner,new Cell(index%S.width,index/S.width));
            }
            if(Dice.Range(S,2)==0) SphereBlast(owner,true); else SphereGrab(owner,true);
        }

        // ── Tubarões (evento das Docas inundadas): neutros que só nadam em água ──
        List<Cell> WaterCells() => Enumerable.Range(0,S.tiles.Length).Where(i=>S.tiles[i].terrain==Terrain.Agua).Select(i=>new Cell(i%S.width,i/S.width)).ToList();
        bool LunaNeutral(Fighter n,List<Fighter> foes)
        {
            if(!(Luna(n)?.waterOnly??false)) return false;
            n.pm=Content.Def(n).pm;
            if(S.At(n.cell).terrain!=Terrain.Agua)
            {
                // Fora da água (empurrado para o seco): volta para a água mais próxima andando normalmente.
                var dry=WaterCells().Where(c=>Board.At(S,c)==null).OrderBy(c=>Board.Gap(n,c)).ToList();
                if(dry.Count>0) SharkSwim(n,dry[0]);
                return true;
            }
            var prey=foes.Where(x=>x.prisoned==0&&Board.Contains(S,x.cell)&&S.At(x.cell).terrain==Terrain.Agua).OrderBy(x=>Board.Gap(x,n)).ThenBy(x=>x.id).FirstOrDefault();
            if(prey!=null)
            {
                if(Board.Gap(n,prey)>1) SharkSwim(n,prey.cell);
                var skills=Content.Def(n).skills;
                if(n.Alive&&prey.Alive&&Board.Gap(n,prey)==1&&skills.Length>0)
                {
                    Emit(new GameEvent{kind="skill",info="mordida-tubarao",actor=n.id,target=prey.id,cell=prey.cell});
                    Strike(n,skills[0],prey.cell);
                }
                return true;
            }
            // Sem alvo na água: espreita circulando por uma casa de água sorteada.
            var spots=WaterCells().Where(c=>Board.At(S,c)==null).ToList();
            if(spots.Count>0) SharkSwim(n,spots[Dice.Range(S,spots.Count)]);
            return true;
        }
        // Campo de distâncias em passos SÓ por casas de água (o tubarão nunca persegue em chão seco).
        Dictionary<int,int> SharkField(Fighter n)
        {
            var dist=new Dictionary<int,int>();
            int start=Board.Index(S,n.cell); dist[start]=0;
            var frontier=new List<Cell>{n.cell};
            for(int stepV=1;stepV<=n.pm&&frontier.Count>0;stepV++)
            {
                var next=new List<Cell>();
                foreach(var c in frontier) foreach(var d in Board.Dirs)
                {
                    var nb=c+d; if(!Board.Contains(S,nb)) continue;
                    int i=Board.Index(S,nb);
                    if(dist.ContainsKey(i)||S.At(nb).terrain!=Terrain.Agua||Board.At(S,nb)!=null) continue;
                    dist[i]=stepV; next.Add(nb);
                }
                frontier=next;
            }
            return dist;
        }
        void SharkSwim(Fighter n,Cell to)
        {
            var dist=SharkField(n);
            if(dist.Count<=1) return;
            if(!Board.Contains(S,to)||!dist.ContainsKey(Board.Index(S,to)))
            {
                var best=dist.Keys.Select(i=>new Cell(i%S.width,i/S.width)).OrderBy(c=>c.Distance(to)).ThenBy(c=>c.z).ThenBy(c=>c.x).First();
                to=best; // dist tem casas (Count>1); (0,0) é uma casa válida, não "nenhuma".
            }
            int goal=Board.Index(S,to); if(!dist.ContainsKey(goal)) return;
            var path=new List<Cell>{to}; int cur=goal;
            while(dist[cur]>0)
            {
                var c=new Cell(cur%S.width,cur/S.width); Cell? prev=null; int want=dist[cur]-1;
                foreach(var d in Board.Dirs)
                {
                    var nb=c+d; if(!Board.Contains(S,nb)) continue;
                    if(dist.TryGetValue(Board.Index(S,nb),out int dd)&&dd==want) { prev=nb; break; }
                }
                if(prev==null) break;
                cur=Board.Index(S,prev.Value); path.Insert(0,prev.Value);
            }
            var walked=new List<Cell>{n.cell};
            foreach(var c in path) { if(Board.At(S,c)!=null) break; n.cell=c; walked.Add(c); }
            n.pm=Math.Max(0,n.pm-(path.Count-1));
            if(walked.Count>1) Emit(new GameEvent{kind="mover",actor=n.id,info="tubaroes",path=walked.ToArray()});
        }
    }
}
