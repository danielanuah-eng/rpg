using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DungeonBDX.Arcadia
{
    [Serializable] public sealed class AqCommand
    {
        public string Kind;
        public int Actor=-1,Target=-1,Card=-1,X,Y;
        public AqCommand(){}
        public AqCommand(string kind,int actor=-1,int target=-1,int card=-1,int x=0,int y=0)
        {Kind=kind;Actor=actor;Target=target;Card=card;X=x;Y=y;}
    }
    public sealed class AqReaction
    {
        public int Monster,Hero,Movement;
        public bool Vengeance,Dying;
    }
    // Independent ruleset: old AP/cooldown saves remain readable by the old expedition.
    public sealed class AqSession
    {
        public const int Version=1,MaxDice=6,MaxDamageAction=30,MaxDamageTurn=60,MaxHealingAction=12,MaxHealingTurn=18;
        public AqScenario Scenario {get;}
        public AqBoard Board=>Scenario.Board;
        public readonly List<AqUnit> Units=new List<AqUnit>();
        public readonly List<AqGuild> Guilds=new List<AqGuild>();
        public readonly List<AqCommand> Commands=new List<AqCommand>();
        public readonly List<int> SpawnQueue=new List<int>();
        public int CurrentGuild {get;private set;}
        public int Turn {get;private set;}=1;
        public int ActiveId {get;private set;}=-1;
        public int MovesLeft {get;private set;}=3;
        public int Moved {get;private set;}
        public bool Attacked {get;private set;}
        public bool MovementClosed {get;private set;}
        public bool MovementStarted {get;private set;}
        public bool Resting {get;private set;}
        public int Winner {get;private set;}=-1;
        public bool AutoMonsters {get;}
        public int Controller=>Reaction==null?CurrentGuild:(CurrentGuild+Guilds.Count-1)%Guilds.Count;
        public AqReaction Reaction=>reactions.Count>0?reactions[0]:null;
        readonly List<AqReaction> reactions=new List<AqReaction>();
        readonly int[] damage=new int[5],healing=new int[5];
        readonly HashSet<string> firstQuests=new HashSet<string>();
        readonly HashSet<int> restAllies=new HashSet<int>();
        readonly HashSet<int> royalReward=new HashSet<int>();
        readonly HashSet<int> resolvedDeaths=new HashSet<int>();
        readonly List<AqCell> moveRoute=new List<AqCell>();
        int routeIndex,attackTarget=-1,attackCard=-1;
        bool moveStepGuarded,waitingAttack,usedPhase,usedScout,usedPortal,usedDoor,usedResonance;
        uint rng;
        public uint RandomState=>rng;
        public AqSession(string scenario,IEnumerable<IEnumerable<string>> parties,uint seed=19,bool autoMonsters=false)
        {
            Scenario=AqScenarios.Create(scenario);AutoMonsters=autoMonsters;rng=seed==0?19:seed;
            var groups=parties?.Select(p=>p.ToArray()).ToArray();
            if(groups==null||groups.Length<2||groups.Length>4||groups.Any(p=>p.Length!=3)||groups.SelectMany(p=>p).Distinct().Count()!=groups.Length*3)
                throw new ArgumentException("Escolha 2 a 4 guildas com três heróis diferentes cada.");
            for(int g=0;g<groups.Length;g++)
            {
                Guilds.Add(new AqGuild{Index=g});
                foreach(var id in groups[g])
                {
                    var h=AqContent.Hero(id);var u=new AqUnit{Id=Units.Count,Guild=g,HeroId=h.Id,Name=h.Name,Ability=h.Ability,AbilityText=h.AbilityText,
                        MaxHealth=h.Health,Health=h.Health,Defense=h.Defense,Huge=h.Ability=="colossus",Cell=Scenario.Starts[g][0]};
                    u.Gear.Add(AqContent.Signature(h));
                    var slots=Scenario.Starts[g].Where(c=>Occupants(c).Sum(v=>v.Occupancy)+u.Occupancy<=2).ToArray();
                    if(slots.Length==0)throw new ArgumentException("Guilda sem espaço inicial.");u.Cell=slots[0];Units.Add(u);
                }
                var first=Units.First(u=>u.Guild==g);first.Gear.Add(new AqGear{Id="sword."+g,Name="Espada de reserva",Dice=3});
                Units.Last(u=>u.Guild==g).Gear.Add(new AqGear{Id="bow."+g,Name="Arco de reserva",Dice=2,Ranged=true});
            }
            foreach(var m in Scenario.Monsters)
                Units.Add(new AqUnit{Id=Units.Count,Guild=-1,Monster=true,HeroId=m.HeroId,Name=m.Name??(m.HeroId=="brutamontes-errante"?"Guardião da praça":"Saqueador de pedra"),SpriteFolder=m.SpriteFolder,
                    Cell=m.Cell,Health=m.Health,MaxHealth=m.Health,Defense=m.Defense,Dice=m.Dice,Ranged=m.Ranged,Movement=m.Movement,Overkill=m.Overkill,Reward=m.Reward,Huge=m.Huge||m.HeroId=="brutamontes-errante"});
        }
        public AqUnit Unit(int id)=>Units.FirstOrDefault(u=>u.Id==id);
        public IEnumerable<AqUnit> Occupants(AqCell cell)=>Units.Where(u=>u.Alive&&u.Cell.Equals(cell));
        public bool Enemy(AqUnit a,AqUnit b)=>a!=b&&(a.Monster?!b.Monster:b.Monster||a.Guild!=b.Guild);
        public bool Blocked(AqUnit u,AqCell c)=>Occupants(c).Where(v=>v.Id!=u.Id&&Enemy(u,v)).Sum(v=>v.Occupancy)>=2;
        public bool CanStop(AqUnit u,AqCell c)=>Board.Contains(c)&&Occupants(c).Where(v=>v.Id!=u.Id).Sum(v=>v.Occupancy)+u.Occupancy<=2;
        public IEnumerable<AqCell> Portals(AqCell c)
        {
            var colors=Scenario.Tokens.Where(t=>t.Kind=="portal"&&t.Cell.Equals(c)).Select(t=>t.Color).ToArray();
            return Scenario.Tokens.Where(t=>t.Kind=="portal"&&colors.Contains(t.Color)&&!t.Cell.Equals(c)).Select(t=>t.Cell).Distinct();
        }
        public List<AqCell> Path(AqUnit u,AqCell c)=>Board.Path(u.Cell,c,v=>Blocked(u,v)&&u.Ability!="feline",Portals);
        public bool CanTarget(AqUnit u,AqUnit target,AqGear gear)
        {
            if(u==null||target==null||gear==null||!target.Alive||!Enemy(u,target))return false;
            if(!gear.Ranged&&u.Ability!="chain")return Board.Close(u.Cell,target.Cell);
            if(!gear.Ranged&&u.Cell.Distance(target.Cell)>2)return false;
            return Board.Sight(u.Cell,target.Cell,c=>Blocked(u,c));
        }
        public Dictionary<AqCell,int> Range(int actor,bool movement,int card=0)
        {
            var result=new Dictionary<AqCell,int>();var u=Unit(actor);if(u==null||!u.Alive)return result;
            foreach(var p in Board.Spaces)
            {
                if(movement)
                {
                    var path=Path(u,p.Cell);int cost=PathCost(u,path);
                    if(path.Count>1&&cost<=MovesLeft&&CanStop(u,p.Cell)&&!MovementClosed)result[p.Cell]=cost;
                }
                else if(card>=0&&card<u.Gear.Count)
                {
                    var gear=u.Gear[card];bool range=gear.Ranged?Board.Sight(u.Cell,p.Cell,c=>Blocked(u,c)):
                        u.Ability=="chain"?u.Cell.Distance(p.Cell)<=2&&Board.Sight(u.Cell,p.Cell,c=>Blocked(u,c)):Board.Close(u.Cell,p.Cell);
                    if(range)result[p.Cell]=u.Cell.Distance(p.Cell);
                }
            }
            return result;
        }
        public AqResult Apply(AqCommand command)
        {
            if(command==null)return Fail("Comando inválido.");
            if(Winner>=0)return Fail("Cenário concluído.");
            if(Commands.Count>=4096)return Fail("Limite de comandos deste cenário atingido.");
            var r=Execute(command);
            if(r.Accepted)
            {
                if(AutoMonsters)AutoReactions(r);
                if(ActiveId>=0&&Reaction==null&&Winner<0)Collect(Unit(ActiveId),r);
                Commands.Add(new AqCommand(command.Kind,command.Actor,command.Target,command.Card,command.X,command.Y));
                if(Winner>=0)r.Message=$"GUILDA {Winner+1} VENCEU! "+Scenario.Ending;
                else if(Reaction!=null)r.Message+=$" · Guilda {Controller+1}: controle a reação de {Unit(Reaction.Monster).Name}.";
            }
            return r;
        }
        AqResult Execute(AqCommand c)
        {
            if(Reaction!=null)return React(c);
            if(c.Kind=="end")return End();
            if(c.Kind=="rest")return Rest();
            if(c.Kind=="revive")return Revive(c);
            if(c.Kind=="transfer")return Transfer(c);
            var u=Unit(c.Actor);
            if(u==null||u.Monster||u.Guild!=CurrentGuild||!u.Alive||Resting||ActiveId>=0&&ActiveId!=u.Id)return Fail("Escolha um herói vivo da guilda ativa. Só um herói pode ser ativado por turno.");
            if(c.Kind=="move")return Move(u,new AqCell(c.X,c.Y));
            if(c.Kind=="door")return Door(u,new AqCell(c.X,c.Y));
            if(c.Kind=="phase")
            {
                var to=new AqCell(c.X,c.Y);var e=Board.Edge(u.Cell,to);
                if(u.Ability!="phase"||usedPhase||MovementClosed||MovesLeft<1||e==null||!e.Door||e.Open||!CanStop(u,to))return Fail("Passo entre sombras exige uma porta fechada adjacente e 1 movimento.");
                Bind(u);usedPhase=true;MovementStarted=true;MovesLeft--;Moved++;var from=u.Cell;u.Cell=to;
                var r=Ok("Passo entre sombras.");r.Effects.Add(new AqEffect{Kind="move",Actor=u.Id,From=from,To=to,Clip="teleport"});Collect(u,r);return r;
            }
            if(c.Kind=="attack")return Attack(u,Unit(c.Target),c.Card);
            if(c.Kind=="stance")
            {
                if(u.Ability!="arsenal"||Attacked)return Fail("A forma só pode mudar antes do ataque de Damian.");
                var signature=u.Gear.FirstOrDefault(g=>g.Id=="signature."+u.HeroId);
                if(signature==null)return Fail("O camaleão está com outro herói.");
                Bind(u);signature.Ranged=!signature.Ranged;signature.Dice=signature.Ranged?2:3;signature.Clip=signature.Ranged?"pistol":"hammer";
                return Ok(signature.Ranged?"Camaleão: pistola.":"Camaleão: martelo.");
            }
            return Fail("Ação desconhecida.");
        }
        void Bind(AqUnit u){if(ActiveId<0)ActiveId=u.Id;}
        int PathCost(AqUnit u,List<AqCell> path)
        {
            int cost=Math.Max(0,path.Count-1);
            if(u.Ability=="ghost"&&!usedPortal&&path.Zip(path.Skip(1),(a,b)=>a.Distance(b)>1).Any(v=>v))cost--;
            return cost;
        }
        AqResult Move(AqUnit u,AqCell to)
        {
            if(MovementClosed)return Fail("O movimento não pode ser dividido por um ataque.");
            var path=Path(u,to);int cost=PathCost(u,path);
            if(path.Count<2||cost>MovesLeft||!CanStop(u,to))return Fail("Destino sem espaço ou fora dos pontos de movimento.");
            Bind(u);MovementStarted=true;var r=Ok("Movimento.");Collect(u,r);
            if(!u.Alive||Winner>=0)return r;
            moveRoute.Clear();moveRoute.AddRange(path.Skip(1));routeIndex=0;moveStepGuarded=false;
            Continue(r);return r;
        }
        AqResult Door(AqUnit u,AqCell to)
        {
            var e=Board.Edge(u.Cell,to);bool free=u.Ability=="breaker"&&!usedDoor&&e!=null&&!e.Open;
            if(MovementClosed||e==null||!e.Door||MovesLeft<(free?0:1))return Fail("Porta fora do alcance ou sem movimento.");
            Bind(u);MovementStarted=true;if(!free)MovesLeft--;else usedDoor=true;e.Open=!e.Open;
            var r=Ok(e.Open?"Porta aberta.":"Porta fechada.");r.Effects.Add(new AqEffect{Kind="door",Actor=u.Id,From=u.Cell,To=to});return r;
        }
        AqResult Attack(AqUnit u,AqUnit target,int card)
        {
            if(Attacked||card<0||card>=u.Gear.Count||u.Gear[card].Exhausted)return Fail("Escolha uma carta restaurada. Apenas um ataque por ativação.");
            var gear=u.Gear[card];if(!CanTarget(u,target,gear))return Fail("Alvo sem alcance ou linha de visão.");
            Bind(u);Attacked=true;if(MovementStarted)MovementClosed=true;
            gear.Exhausted=true;attackTarget=target.Id;attackCard=card;waitingAttack=true;
            var r=Ok(gear.Name+" — carta exausta.");QueueGuards(u,target.Id);Continue(r);return r;
        }
        void QueueGuards(AqUnit hero,int excluded=-1)
        {
            foreach(var m in Units.Where(m=>m.Alive&&m.Monster&&m.Id!=excluded&&Board.Close(m.Cell,hero.Cell)).OrderBy(m=>m.Id))
            {
                if(hero.Ability=="scout"&&!usedScout){usedScout=true;continue;}
                reactions.Add(new AqReaction{Monster=m.Id,Hero=hero.Id});
            }
        }
        void Continue(AqResult r)
        {
            var u=Unit(ActiveId);if(u==null||Winner>=0||Reaction!=null)return;
            if(!u.Alive){moveRoute.Clear();waitingAttack=false;return;}
            while(routeIndex<moveRoute.Count)
            {
                var to=moveRoute[routeIndex];bool portal=Portals(u.Cell).Contains(to);
                if(!moveStepGuarded&&!portal){moveStepGuarded=true;QueueGuards(u);if(Reaction!=null)return;}
                var from=u.Cell;bool free=portal&&u.Ability=="ghost"&&!usedPortal;
                if(free)usedPortal=true;else MovesLeft--;Moved++;u.Cell=to;
                r.Effects.Add(new AqEffect{Kind="move",Actor=u.Id,From=from,To=to});routeIndex++;moveStepGuarded=false;
                Collect(u,r);
                if(!u.Alive||Winner>=0){moveRoute.Clear();return;}
            }
            moveRoute.Clear();routeIndex=0;
            if(waitingAttack)
            {
                waitingAttack=false;var target=Unit(attackTarget);var gear=u.Gear[attackCard];
                if(target==null||!target.Alive)return;
                var dealt=Strike(u,target,gear,r);
                if(target.Monster&&dealt<target.Overkill)
                    reactions.Add(new AqReaction{Monster=target.Id,Hero=u.Id,Vengeance=true,Movement=u.Ability=="web"&&dealt>0?0:target.Movement,Dying=!target.Alive});
                else if(!target.Alive)Kill(target,u,r);
                if(target.Monster&&dealt>0&&u.Ability=="sting")target.Dice=Math.Max(1,target.Dice-1);
            }
        }
        AqResult React(AqCommand c)
        {
            var rx=Reaction;var m=Unit(rx.Monster);var hero=Unit(rx.Hero);
            if(c.Kind=="react-skip")
            {var skipped=Ok("Reação encerrada.");FinishReaction(skipped);return skipped;}
            if(c.Kind=="react-move")
            {
                var to=new AqCell(c.X,c.Y);var path=Path(m,to);
                if(!rx.Vengeance||path.Count<2||path.Count-1>rx.Movement||!CanStop(m,to))return Fail("Movimento inválido para a reação.");
                var r=Ok("Monstro reposicionado.");for(int i=1;i<path.Count;i++)r.Effects.Add(new AqEffect{Kind="move",Actor=m.Id,From=path[i-1],To=path[i]});
                m.Cell=to;rx.Movement-=path.Count-1;return r;
            }
            if(c.Kind=="react-door")
            {
                var e=Board.Edge(m.Cell,new AqCell(c.X,c.Y));
                if(!rx.Vengeance||rx.Movement<1||e==null||!e.Door)return Fail("Porta inválida para a reação.");
                e.Open=!e.Open;rx.Movement--;return Ok("Porta acionada pelo monstro.");
            }
            if(c.Kind!="react-attack")return Fail("Resolva a reação do monstro primeiro.");
            if(!hero.Alive)return Fail("O alvo já foi derrotado. Encerre a reação.");
            var attack=new AqGear{Dice=m.Dice,Ranged=m.Ranged,Clip="attack"};
            if(!CanTarget(m,hero,attack))return Fail("O monstro não alcança o herói ativo.");
            var result=Ok("Reação de "+m.Name+".");Strike(m,hero,attack,result);
            if(!hero.Alive)Kill(hero,m,result);FinishReaction(result);return result;
        }
        void FinishReaction(AqResult r)
        {
            var rx=Reaction;var m=Unit(rx.Monster);var hero=Unit(rx.Hero);
            if(!m.Alive)Kill(m,hero,r);
            var spawn=Scenario.Monsters.FirstOrDefault(s=>s.HeroId==m.HeroId);if(spawn!=null)m.Dice=spawn.Dice;
            reactions.RemoveAt(0);Continue(r);
        }
        void AutoReactions(AqResult r)
        {
            for(int safe=0;Reaction!=null&&safe<100;safe++)
            {
                var rx=Reaction;var m=Unit(rx.Monster);var h=Unit(rx.Hero);var gear=new AqGear{Dice=m.Dice,Ranged=m.Ranged};
                if(rx.Vengeance&&h.Alive&&!CanTarget(m,h,gear)&&rx.Movement>0)
                {
                    var choices=Board.Spaces.Where(p=>CanStop(m,p.Cell)).Select(p=>new{p.Cell,Path=Path(m,p.Cell)})
                        .Where(p=>p.Path.Count>1&&p.Path.Count-1<=rx.Movement).OrderBy(p=>p.Cell.Distance(h.Cell)).ThenBy(p=>p.Path.Count).ToArray();
                    if(choices.Length>0){var p=choices[0];Merge(r,React(new AqCommand("react-move",x:p.Cell.X,y:p.Cell.Y)));}
                }
                Merge(r,React(new AqCommand(h.Alive&&CanTarget(m,h,gear)?"react-attack":"react-skip")));
            }
        }
        int NextFace(){rng^=rng<<13;rng^=rng>>17;rng^=rng<<5;return (int)(rng%6)+1;}
        int Roll(int dice,bool ranged,bool defense,bool reroll,out int crit)
        {
            int successes=0;crit=0;bool rerolled=false;
            for(int i=0;i<Math.Max(0,Math.Min(MaxDice,dice));i++)
            {
                int face=NextFace();bool hit=face==6||(defense?face<=2:ranged?face==4||face==5:face<=3);
                if(!hit&&reroll&&!rerolled){face=NextFace();rerolled=true;hit=face==6||(defense?face<=2:ranged?face==4||face==5:face<=3);}
                if(hit)successes++;if(face==6)crit++;
            }
            // Author exception: linear critical bonus, no recursive extra dice.
            return successes+Math.Min(3,crit);
        }
        int Strike(AqUnit a,AqUnit b,AqGear gear,AqResult r)
        {
            int dice=gear.Dice;bool allyNear=Units.Any(v=>v.Alive&&v.Id!=a.Id&&v.Guild==a.Guild&&!v.Monster&&Board.Close(v.Cell,b.Cell));
            if(a.Ability=="dragon"&&gear.Ranged&&allyNear)dice++;
            if(!gear.Ranged&&Units.Any(v=>v.Alive&&v.Id!=a.Id&&v.Ability=="banner"&&v.Guild==a.Guild&&Board.Close(v.Cell,a.Cell)))dice++;
            if(a.Ability=="fury"&&!gear.Ranged&&a.Health<a.MaxHealth||a.Ability=="sniper"&&gear.Ranged&&Moved==0||a.Ability=="colossus"&&!gear.Ranged&&b.Monster||a.Ability=="charge"&&!gear.Ranged&&Moved>=3||a.Ability=="reaper"&&b.Health<b.MaxHealth||a.Ability=="frost"&&Occupants(b.Cell).Count()>1)dice++;
            if(a.Ability=="flank"&&Units.Any(v=>v.Alive&&v!=a&&Enemy(b,v)&&Board.Close(v.Cell,b.Cell)))dice++;
            if(a.Ability=="isolate"&&!Units.Any(v=>v.Alive&&v!=b&&!Enemy(b,v)&&Board.Close(v.Cell,b.Cell)))dice++;
            int hits=Roll(dice,gear.Ranged,false,a.Ability=="focus"&&gear.Ranged,out int crit);
            if(a.Ability=="berserk"&&a.Health==1&&!gear.Ranged)hits++;
            int defense=b.Defense+b.Gear.Sum(g=>g.Defense);
            if(b.Ability=="moss"&&Board.Space(b.Cell).Room!=""||b.Ability=="stone"&&!gear.Ranged)defense++;
            if(a.Ability=="gunslinger"&&gear.Ranged)defense=Math.Max(0,defense-1);
            int saves=Roll(defense,false,true,false,out _);
            // Same hard mitigation ceiling requested for DungeonBDX; defense dice replace armor ratings.
            saves=Math.Min(saves,(int)Math.Floor(hits*.6));
            int amount=Math.Min(MaxDamageAction,Math.Max(0,hits-saves));int pool=a.Monster?4:a.Guild;
            amount=Math.Min(amount,Math.Max(0,MaxDamageTurn-damage[pool]));damage[pool]+=amount;
            r.Effects.Add(new AqEffect{Kind="attack",Actor=a.Id,Target=b.Id,From=a.Cell,To=b.Cell,Clip=gear.Clip});
            Hurt(b,amount,a,r);
            if(amount>0&&a.Ability=="drain")Heal(a,a,1,r);
            if(amount>0&&a.Ability=="bond")
            {
                var friend=Units.Where(v=>v.Alive&&v!=a&&v.Guild==a.Guild&&!v.Monster&&v.Health<v.MaxHealth&&Board.Close(a.Cell,v.Cell)).OrderBy(v=>v.Health).ThenBy(v=>v.Id).FirstOrDefault();
                if(friend!=null)Heal(a,friend,1,r);
            }
            if(crit>0&&a.Ability=="arcane"&&!usedResonance)
            {var g=a.Gear.FirstOrDefault(g=>g!=gear&&g.Exhausted);if(g!=null){g.Exhausted=false;usedResonance=true;}}
            if(amount>0&&b.Ability=="thorns"&&!gear.Ranged)
            {int slot=b.Monster?4:b.Guild;if(damage[slot]<MaxDamageTurn){damage[slot]++;Hurt(a,1,b,r);if(!a.Alive&&!a.Monster)Kill(a,b,r);}}
            r.Message+=$" {a.Name}: {Math.Min(MaxDice,dice)} dados, {hits} acertos, {saves} bloqueios, {amount} ferimentos.";
            return amount;
        }
        void Hurt(AqUnit target,int n,AqUnit source,AqResult r)
        {
            target.Health=Math.Max(0,target.Health-n);
            if(target.Health==0&&target.Ability=="rebirth"&&!target.SpentLife){target.Health=1;target.SpentLife=true;}
            if(n>0)r.Effects.Add(new AqEffect{Kind="hit",Actor=source?.Id??-1,Target=target.Id,Amount=n,To=target.Cell});
        }
        void Heal(AqUnit source,AqUnit target,int n,AqResult r)
        {
            int p=source.Monster?4:source.Guild;n=Math.Min(n,Math.Min(MaxHealingAction,MaxHealingTurn-healing[p]));n=Math.Min(n,target.MaxHealth-target.Health);
            if(n<=0||!target.Alive)return;healing[p]+=n;target.Health+=n;r.Effects.Add(new AqEffect{Kind="heal",Actor=source.Id,Target=target.Id,Amount=n,To=target.Cell});
        }
        void Kill(AqUnit dead,AqUnit killer,AqResult r)
        {
            if(dead.Health>0||!resolvedDeaths.Add(dead.Id))return;
            r.Effects.Add(new AqEffect{Kind="death",Actor=dead.Id,Target=dead.Id,To=dead.Cell});
            if(dead.Monster)
            {
                if(killer!=null&&!killer.Monster)Guilds[killer.Guild].Coins+=dead.Reward;
                if(dead.HeroId=="brutamontes-errante"&&killer!=null&&!killer.Monster)Quest(killer.Guild,"pve.guardian",r);
                else if(!SpawnQueue.Contains(dead.Id)&&SpawnQueue.Count<5)SpawnQueue.Add(dead.Id);
            }
            else
            {
                dead.Deaths++;
                if(killer!=null&&!killer.Monster&&killer.Guild!=dead.Guild){Guilds[killer.Guild].Coins++;Quest(killer.Guild,"pvp."+dead.Guild,r);}
                else foreach(var g in Guilds.Where(g=>g.Index!=dead.Guild))g.Coins++;
                foreach(string id in dead.Carry){var t=Scenario.Tokens.First(v=>v.Id==id);t.Taken=false;t.Owner=-1;t.Cell=dead.Cell;}dead.Carry.Clear();
            }
        }
        void Collect(AqUnit hero,AqResult r)
        {
            if(hero.Monster||!hero.Alive||Occupants(hero.Cell).Any(v=>Enemy(hero,v)))return;
            foreach(var token in Scenario.Tokens.Where(t=>!t.Taken&&t.Cell.Equals(hero.Cell)).ToArray())
            {
                if(token.Kind=="portal"||token.Kind=="hazard"||token.Kind=="spawn")continue;
                if(token.Kind=="explore")
                {
                    token.Taken=true;int face=NextFace();
                    if(face<=3){Guilds[hero.Guild].Coins++;r.Message+=" Tesouro: +1 moeda.";}
                    else if(face<=5){Heal(hero,hero,2,r);r.Message+=" Poção de cura.";}
                    else {Hurt(hero,1,null,r);r.Message+=" Armadilha revelada: 1 ferimento.";if(!hero.Alive){Kill(hero,null,r);return;}}
                }
                else if(token.Kind=="clue"||token.Kind=="relic"||token.Kind=="captive")
                {
                    if(token.Kind=="relic"&&Units.Where(u=>u.Guild==hero.Guild).Any(u=>u.Carry.Any(id=>Scenario.Tokens.First(t=>t.Id==id).Kind=="relic")))continue;
                    token.Taken=true;token.Owner=hero.Id;hero.Carry.Add(token.Id);r.Message+=" Coletado: "+token.Kind+".";
                }
                else if(token.Kind=="emblem"){token.Taken=true;Guilds[hero.Guild].Emblems++;if(Guilds[hero.Guild].Emblems>=2)Quest(hero.Guild,"pve.emblems",r);}
                else if(token.Kind=="ritual"){token.Taken=true;Guilds[hero.Guild].Rituals++;if(Guilds[hero.Guild].Rituals>=2)Quest(hero.Guild,"pve.rituals",r);}
                r.Effects.Add(new AqEffect{Kind="token",Actor=hero.Id,To=hero.Cell});
            }
            if(Scenario.Id==AqScenarios.Ids[0])
            {
                if(Units.Where(u=>u.Guild==hero.Guild).Sum(u=>u.Carry.Count(id=>Scenario.Tokens.First(t=>t.Id==id).Kind=="clue"))>=2)Quest(hero.Guild,"pve.clues",r);
                if(Scenario.Tokens.Any(t=>t.Kind=="portal"&&t.Color=="blue"&&t.Cell.Equals(hero.Cell)))
                {string relic=hero.Carry.FirstOrDefault(id=>Scenario.Tokens.First(t=>t.Id==id).Kind=="relic");if(relic!=null){hero.Carry.Remove(relic);Quest(hero.Guild,"pve.portal",r);}}
            }
            if(Scenario.Id==AqScenarios.Ids[2]&&Scenario.Starts[hero.Guild].Contains(hero.Cell))
            {string captive=hero.Carry.FirstOrDefault(id=>Scenario.Tokens.First(t=>t.Id==id).Kind=="captive");if(captive!=null){hero.Carry.Remove(captive);Quest(hero.Guild,"pve.rescue",r);}}
        }
        void Quest(int guild,string id,AqResult r)
        {
            var g=Guilds[guild];if(!g.Completed.Add(id))return;
            if(firstQuests.Add(id))g.Coins++;
            if(!royalReward.Contains(guild)&&Units.Any(u=>u.Guild==guild&&u.Ability=="royal")){g.Coins++;royalReward.Add(guild);}
            r.Message+=$" MISSÃO CONCLUÍDA ({g.Completed.Count}).";
            int needed=Guilds.Count==2?2:3;if(g.Completed.Count>=needed&&g.Completed.Any(q=>q.StartsWith("pve.")))Winner=guild;
        }
        AqResult Rest()
        {
            if(ActiveId>=0||Resting)return Fail("Descansar ocupa todo o turno; não pode ativar herói junto.");
            Resting=true;restAllies.Clear();foreach(var u in Units.Where(u=>!u.Monster&&u.Guild==CurrentGuild))
            {if(u.Alive)restAllies.Add(u.Id);foreach(var g in u.Gear)g.Exhausted=false;}
            var r=Ok("Guilda descansando: cartas restauradas. Reorganize itens e escolha onde ressuscitar seus heróis.");
            var healer=Units.FirstOrDefault(u=>u.Guild==CurrentGuild&&u.Ability=="ward"&&u.Alive);
            if(healer!=null)foreach(var u in Units.Where(u=>u.Guild==CurrentGuild&&u.Alive))Heal(healer,u,1,r);
            return r;
        }
        public IEnumerable<AqCell> ResurrectionCells(AqUnit hero)=>Board.Spaces.Where(p=>CanStop(hero,p.Cell)&&
            (Scenario.Starts[hero.Guild].Contains(p.Cell)||Units.Any(u=>restAllies.Contains(u.Id)&&u.Alive&&Board.Close(u.Cell,p.Cell)))).Select(p=>p.Cell);
        AqResult Revive(AqCommand c)
        {
            var u=Unit(c.Actor);var cell=new AqCell(c.X,c.Y);
            if(!Resting||u==null||u.Monster||u.Guild!=CurrentGuild||u.Alive||!ResurrectionCells(u).Contains(cell))return Fail("Escolha um espaço livre da área inicial ou Perto de um aliado que já estava vivo.");
            u.Cell=cell;u.Health=u.MaxHealth;resolvedDeaths.Remove(u.Id);var r=Ok(u.Name+" ressuscitado.");r.Effects.Add(new AqEffect{Kind="heal",Actor=u.Id,Target=u.Id,To=cell});return r;
        }
        AqResult Transfer(AqCommand c)
        {
            var a=Unit(c.Actor);var b=Unit(c.Target);
            if(!Resting||a==null||b==null||a==b||a.Monster||b.Monster||a.Guild!=CurrentGuild||b.Guild!=CurrentGuild||c.Card<0||c.Card>=a.Gear.Count||b.Gear.Count>=4)return Fail("Durante descanso, transfira uma carta para um aliado com espaço (máximo 4).");
            var gear=a.Gear[c.Card];a.Gear.RemoveAt(c.Card);b.Gear.Add(gear);return Ok(gear.Name+" transferida para "+b.Name+".");
        }
        AqResult End()
        {
            var r=Ok("Próxima guilda.");
            foreach(var t in Scenario.Tokens.Where(t=>t.Kind=="hazard"))
                if(Turn%2==0)foreach(var u in Occupants(t.Cell).Where(u=>!u.Monster).ToArray()){Hurt(u,1,null,r);if(!u.Alive)Kill(u,null,r);}
            if(SpawnQueue.Count>=5||!Units.Any(u=>u.Monster&&u.Alive))
            {
                foreach(int id in SpawnQueue.ToArray())
                {
                    string a=SpawnSymbol(NextFace()),b=SpawnSymbol(NextFace());
                    var marker=Scenario.Tokens.FirstOrDefault(t=>t.Kind=="spawn"&&(t.Color==a+"-"+b||t.Color==b+"-"+a));
                    if(marker==null||Occupants(marker.Cell).Any())continue;
                    var m=Unit(id);m.Cell=marker.Cell;m.Health=m.MaxHealth;resolvedDeaths.Remove(id);
                    r.Effects.Add(new AqEffect{Kind="heal",Actor=id,Target=id,To=m.Cell});
                }
                SpawnQueue.Clear();r.Message+=" Entrada de monstros resolvida.";
            }
            CurrentGuild=(CurrentGuild+1)%Guilds.Count;Turn++;ActiveId=-1;MovesLeft=3;Moved=0;
            Attacked=MovementClosed=MovementStarted=Resting=usedPhase=usedScout=usedPortal=usedDoor=usedResonance=false;
            Array.Clear(damage,0,damage.Length);Array.Clear(healing,0,healing.Length);restAllies.Clear();return r;
        }
        public string Fingerprint()
        {
            var s=new StringBuilder();s.Append(Version).Append('|').Append(Scenario.Id).Append('|').Append(rng).Append('|').Append(CurrentGuild).Append('|').Append(Turn).Append('|').Append(ActiveId).Append('|').Append(MovesLeft).Append('|').Append(Moved).Append('|').Append(Attacked).Append('|').Append(MovementClosed).Append('|').Append(Resting).Append('|').Append(Winner);
            s.Append('|').Append(MovementStarted).Append('|').Append(string.Join(",",resolvedDeaths.OrderBy(v=>v)));
            foreach(var u in Units){s.Append(';').Append(u.Id).Append(':').Append(u.HeroId).Append(':').Append(u.Cell).Append(':').Append(u.Health).Append(':').Append(u.Deaths).Append(':').Append(u.SpentLife).Append(':').Append(u.Dice);foreach(var g in u.Gear)s.Append('/').Append(g.Id).Append(',').Append(g.Exhausted).Append(',').Append(g.Ranged).Append(',').Append(g.Dice).Append(',').Append(g.Defense).Append(',').Append(g.Clip);s.Append('[').Append(string.Join(",",u.Carry)).Append(']');}
            foreach(var g in Guilds)s.Append(';').Append(g.Coins).Append(':').Append(g.Emblems).Append(':').Append(g.Rituals).Append(':').Append(string.Join(",",g.Completed.OrderBy(x=>x)));
            foreach(var e in Board.Edges)s.Append(';').Append(e.A).Append(':').Append(e.B).Append(':').Append(e.Open);
            foreach(var t in Scenario.Tokens)s.Append(';').Append(t.Id).Append(':').Append(t.Cell).Append(':').Append(t.Taken).Append(':').Append(t.Owner);
            foreach(var rx in reactions)s.Append(';').Append(rx.Monster).Append(':').Append(rx.Hero).Append(':').Append(rx.Movement).Append(':').Append(rx.Vengeance).Append(':').Append(rx.Dying);
            s.Append('|').Append(string.Join(",",SpawnQueue)).Append('|').Append(string.Join(",",damage)).Append('|').Append(string.Join(",",healing)).Append('|').Append(string.Join(",",firstQuests.OrderBy(v=>v))).Append('|').Append(string.Join(",",restAllies.OrderBy(v=>v))).Append('|').Append(string.Join(",",royalReward.OrderBy(v=>v)));
            s.Append('|').Append(string.Join(";",moveRoute)).Append(':').Append(routeIndex).Append(':').Append(moveStepGuarded).Append(':').Append(waitingAttack).Append(':').Append(attackTarget).Append(':').Append(attackCard).Append(':').Append(usedPhase).Append(':').Append(usedScout).Append(':').Append(usedPortal).Append(':').Append(usedDoor).Append(':').Append(usedResonance);
            return s.ToString();
        }
        static AqResult Fail(string s)=>new AqResult(false,s);
        static string SpawnSymbol(int face)=>face<=3?"melee":face<=5?"ranged":"critical";
        static AqResult Ok(string s)=>new AqResult(true,s);
        static void Merge(AqResult a,AqResult b){a.Effects.AddRange(b.Effects);a.Message+=" "+b.Message;}
    }
}
