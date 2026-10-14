using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace ArenaTatica.Core.Tests
{
    public class CoreMatchTests
    {
        Match m;
        MatchState S => m.S;
        [SetUp] public void Setup() { m=Match.New(new MatchSetup{ flatMap=true }); }
        Fighter A => S.Active;
        Fighter Enemy => S.fighters.First(f=>f.team!=A.team);
        Command Cmd(string kind,Cell cell=default,string skill=null) => new Command{kind=kind,cell=cell,skill=skill,fighter=A.id,revision=S.revision};
        bool Do(string kind,Cell cell=default,string skill=null) => m.Apply(Cmd(kind,cell,skill),A.team,out _,out _);
        void Put(Fighter f,int x,int z) => f.cell=new Cell(x,z);

        [Test] public void NetworkSnapshotHidesEnemyCellsButKeepsTurnsAndAuthority()
        {
            var hidden=S.fighters.First(f=>f.team==1); hidden.hidden=true; var cell=hidden.cell;
            var enemy=Match.ViewFor(S,2); var ally=Match.ViewFor(S,1);
            Assert.AreEqual(Cell.Unknown,enemy.F(hidden.id).cell);
            Assert.AreEqual(cell,ally.F(hidden.id).cell);
            CollectionAssert.AreEqual(S.timeline,enemy.timeline);
            Assert.AreEqual(cell,S.F(hidden.id).cell,"projeção não altera a autoridade");
            Assert.IsTrue(ArenaTatica.ArenaGame.ValidState(enemy,2));
            Assert.IsFalse(ArenaTatica.ArenaGame.ValidState(enemy),"estado autoritativo não aceita posição desconhecida");
            Assert.IsFalse(ArenaTatica.ArenaGame.ValidState(enemy,1),"não pode esconder posição própria");
            var wire=JsonUtility.FromJson<ArenaTatica.WireMessage>(JsonUtility.ToJson(new ArenaTatica.WireMessage{kind="snapshot",state=enemy}));
            Assert.AreEqual(Cell.Unknown,wire.state.F(hidden.id).cell,"snapshot/reconexão serializados");
            hidden.hidden=false; Assert.AreEqual(cell,Match.ViewFor(S,2).F(hidden.id).cell);
        }

        [Test] public void MultipleUnknownEnemiesAndAlliesKeepValidProjectedState()
        {
            m=Match.New(new MatchSetup{flatMap=true,players=new[]{new PlayerSetup{team=1,fighters=new[]{"perry","ezequiela"}},new PlayerSetup{team=2},new PlayerSetup{team=1}}});
            foreach(var f in S.fighters.Where(x=>x.team==1)) f.hidden=true;
            var enemy=Match.ViewFor(S,2); Assert.IsTrue(ArenaTatica.ArenaGame.ValidState(enemy,2));
            Assert.IsTrue(enemy.fighters.Where(x=>x.team==1).All(x=>x.cell==Cell.Unknown));
            Assert.IsTrue(Match.ViewFor(S,3).fighters.Where(x=>x.team==1).All(x=>Board.Contains(S,x.cell)),"aliados veem posições");
        }

        [Test] public void HiddenPathStaysSecretEvenWhenTheFinalStateRevealsItsFighter()
        {
            var f=S.fighters.First(x=>x.team==1); var old=new Cell(2,3); f.cell=new Cell(4,3); f.hidden=false;
            var events=new System.Collections.Generic.List<GameEvent>{
                new GameEvent{kind="mover",actor=f.id,path=new[]{old,new Cell(3,3),f.cell}},
                new GameEvent{kind="moeda",actor=f.id,cell=new Cell(3,3)},
                new GameEvent{kind="revelar",target=f.id,cell=f.cell}};
            var visible=Match.EventsFor(events,S,2,new[]{f.id});
            Assert.AreEqual(1,visible.Count); Assert.AreEqual("revelar",visible[0].kind); Assert.AreEqual(f.cell,visible[0].cell);
            Assert.AreEqual(3,Match.EventsFor(events,S,1,new[]{f.id}).Count,"aliado conserva animações");
            Assert.AreEqual(old,events[0].path[0],"não altera eventos do Host");
        }

        [Test] public void EnemyReceivesNoHiddenTeleportOrTurnCoordinates()
        {
            var f=S.fighters.First(x=>x.team==1); f.hidden=true;
            var events=new System.Collections.Generic.List<GameEvent>{new GameEvent{kind="teleporte",actor=f.id,cell=f.cell,path=new[]{new Cell(2,2),f.cell}},new GameEvent{kind="turno",actor=f.id,cell=f.cell}};
            var result=Match.EventsFor(events,S,2,new[]{f.id});
            Assert.AreEqual(1,result.Count); Assert.AreEqual("turno",result[0].kind); Assert.AreEqual(Cell.Unknown,result[0].cell); Assert.IsNull(result[0].path);
        }

        [Test] public void ProjectedMeleeCanBeSubmittedButOnlyHostDecidesTheTarget()
        {
            var setup=new MatchSetup{flatMap=true,team1=new[]{"perry"},team2=new[]{"ciantia"}}; m=Match.New(setup);
            var foe=S.fighters.First(x=>x.team==1); var mine=S.fighters.First(x=>x.team==2);
            foe.hidden=true; Put(foe,5,5); Put(mine,5,6); mine.pa=20;
            var skill=Match.Skills(mine).First(x=>x.kind==SkillKind.Dano&&!x.selfArea);
            var view=Match.ViewFor(S,2); var preview=Match.FromState(view,true);
            Assert.IsTrue(preview.CanTarget(view.F(mine.id),skill,foe.cell,out _));
            Assert.IsTrue(m.CanTarget(mine,skill,foe.cell,out _));
            Assert.IsTrue(m.CanTarget(mine,skill,new Cell(6,6),out _),"Host aceita conjuração no chão");
            Assert.IsFalse(preview.Apply(new Command{kind="passar"},2,out _,out _),"prévia não aplica regras");
        }

        [Test] public void HiddenIceTrapLeavesNoSecondaryMarkerInEnemySnapshot()
        {
            var at=new Cell(3,3); S.At(at).trap=1; S.At(at).trapDamage=2; S.At(at).trapPa=1;
            var enemy=Match.ViewFor(S,2).At(at); Assert.AreEqual(0,enemy.trap); Assert.AreEqual(0,enemy.trapDamage); Assert.AreEqual(0,enemy.trapPa);
            Assert.AreEqual(1,Match.ViewFor(S,1).At(at).trapPa);
        }

        [Test] public void GeneratedMapIsMirroredAndConnected()
        {
            for(uint seed=1;seed<30;seed++)
            {
                var s=Match.New(new MatchSetup{ seed=seed }).S;
                for(int i=0;i<s.tiles.Length;i++) { var c=new Cell(i%s.width,i/s.width); var a=s.At(c); var b=s.At(s.Mirror(c)); Assert.AreEqual(a.height,b.height); Assert.AreEqual(a.terrain,b.terrain); }
                Assert.IsTrue(Board.Connected(s),"seed "+seed);
            }
        }
        // Krosmaster: o jogador joga o time inteiro em sequência (maior Iniciativa primeiro); começa a maior soma de Iniciativa.
        [Test] public void KrosmasterTimelinePlaysWholeTeamAndStartsWithFullStats()
        {
            m=Match.New(new MatchSetup{ flatMap=true, team1=new[]{"ciantia","ciantia"}, team2=new[]{"ciantia","ciantia"} });
            CollectionAssert.AreEqual(new[]{1,1,2,2},S.timeline.Select(id=>S.F(id).team).ToArray());
            Assert.AreEqual(6,A.pa); Assert.AreEqual(3,A.pm);
            m=Match.New(new MatchSetup{ flatMap=true, team1=new[]{"devon","gryvro"}, team2=new[]{"teantha","diantus"} });
            CollectionAssert.AreEqual(new[]{2,2,1,1},S.timeline.Select(id=>S.F(id).team).ToArray(),"soma 12 contra 9 começa, mesmo sem o mais rápido");
            CollectionAssert.AreEqual(new[]{"gryvro","devon"},S.timeline.Skip(2).Select(id=>S.F(id).def).ToArray(),"dentro do time, maior Iniciativa primeiro");
        }
        [Test] public void EditorPassivesFollowTargetAndRange()
        {
            var def=Content.Copy(Content.Fighters["ciantia"]);
            def.passives=new[]{ new PassiveDef{stat=PassiveStat.Armadura,who=PassiveWho.Aliados,amount=1,range=2}, new PassiveDef{stat=PassiveStat.Dano,who=PassiveWho.Si,amount=2},
                new PassiveDef{stat=PassiveStat.PM,who=PassiveWho.Inimigos,amount=-1} };
            Content.ApplyEdits(JsonUtility.ToJson(new GameEdits{ fighters=new System.Collections.Generic.List<FighterDef>{def} }));
            try {
                m=Match.New(new MatchSetup{ flatMap=true, team1=new[]{"ciantia","talyssa"}, team2=new[]{"teantha"} });
                var cia=S.fighters.First(f=>f.def=="ciantia"); var tal=S.fighters.First(f=>f.def=="talyssa"); var foe=S.fighters.First(f=>f.def=="teantha");
                Put(cia,5,5); Put(tal,5,7); Put(foe,10,10);
                Assert.AreEqual(2,m.Passive(cia,PassiveStat.Dano)); Assert.AreEqual(0,m.Passive(tal,PassiveStat.Dano),"Si só vale para o dono");
                Assert.AreEqual(1,m.Passive(tal,PassiveStat.Armadura)); Assert.AreEqual(0,m.Passive(cia,PassiveStat.Armadura),"Aliados não inclui o dono");
                Put(tal,5,8); Assert.AreEqual(0,m.Passive(tal,PassiveStat.Armadura),"fora do alcance");
                Assert.AreEqual(-1,m.Passive(foe,PassiveStat.PM),"alcance 0 = mapa todo");
                cia.hp=0; Assert.AreEqual(0,m.Passive(foe,PassiveStat.PM),"dono nocauteado não dá passiva");
            } finally { Content.ApplyEdits(""); }
        }
        [Test] public void ClimbingCostsAndCoinPickup()
        {
            Put(A,5,5); S.At(new Cell(6,5)).height=2; S.At(new Cell(5,7)).height=3; S.At(new Cell(5,6)).coins=1;
            Assert.IsNull(Board.Path(S,A,new Cell(5,7),3),"3 níveis bloqueia");
            Assert.IsTrue(Do("mover",new Cell(5,6)));
            Assert.AreEqual(1,S.coins[A.team]); Assert.AreEqual(0,S.At(new Cell(5,6)).coins); Assert.AreEqual(2,A.pm);
            Assert.IsTrue(Do("mover",new Cell(6,6)));
            Assert.IsFalse(Do("mover",new Cell(6,5)),"subir 2 custa 2 PM e só sobra 1");
        }
        [Test] public void GoldenDragonNeedsLineSightAndRange()
        {
            Put(A,5,5); Put(Enemy,5,9);
            Assert.IsFalse(Do("skill",new Cell(5,10),"dragao-dourado"),"casa vazia");
            Put(Enemy,6,8); Assert.IsFalse(Do("skill",new Cell(6,8),"dragao-dourado"),"fora da linha reta");
            Put(Enemy,5,10); Assert.IsFalse(Do("skill",new Cell(5,10),"dragao-dourado"),"alcance 4");
            S.At(new Cell(5,5)).height=1; m.Forced.Enqueue(Face.Escudo); m.Forced.Enqueue(Face.Trava);
            Assert.IsTrue(Do("skill",new Cell(5,10),"dragao-dourado"),"+1 de alcance no alto");
            S.At(new Cell(5,5)).height=0; Put(Enemy,5,8); S.At(new Cell(5,7)).terrain=Terrain.Arvore;
            Assert.IsFalse(Do("skill",new Cell(5,8),"dragao-dourado"),"árvore bloqueia visão");
            S.At(new Cell(5,7)).terrain=Terrain.Arbusto; m.Forced.Enqueue(Face.Escudo); m.Forced.Enqueue(Face.Escudo);
            Assert.IsTrue(Do("skill",new Cell(5,8),"dragao-dourado"),"arbusto não bloqueia visão");
            Assert.AreEqual(0,A.pa); Assert.IsFalse(Do("skill",new Cell(5,8),"dragao-dourado"),"sem PA");
        }
        [Test] public void ArmorReducesDamageAndCataclysmOnlyFromAttackDie()
        {
            Put(A,5,5); Put(Enemy,5,7); int hp=Enemy.hp;
            m.Forced.Enqueue(Face.Cataclisma); m.Forced.Enqueue(Face.Escudo);
            Assert.IsTrue(m.Apply(Cmd("skill",new Cell(5,7),"dragao-dourado"),A.team,out var ev,out _));
            Assert.AreEqual(hp-1,Enemy.hp);
            Assert.IsTrue(ev.Any(e=>e.kind=="cataclisma"));
            Put(Enemy,5,6); m.Forced.Enqueue(Face.Escudo); m.Forced.Enqueue(Face.Cataclisma);
            Assert.IsTrue(m.Apply(Cmd("skill",new Cell(5,6),"dragao-dourado"),A.team,out ev,out _));
            Assert.IsFalse(ev.Any(e=>e.kind=="cataclisma"),"Cataclisma do defensor não conta");
        }
        [Test] public void FailedTackleRemovesActionPoints()
        {
            Put(A,5,5); Put(Enemy,5,6);
            m.Forced.Enqueue(Face.Trava); m.Forced.Enqueue(Face.Escudo);
            Assert.IsTrue(Do("mover",new Cell(3,5)));
            Assert.AreEqual(new Cell(5,5),A.cell); Assert.AreEqual(0,A.pa); Assert.AreEqual(0,A.pm);
        }
        [Test] public void DodgedTackleLetsMove()
        {
            Put(A,5,5); Put(Enemy,5,6);
            m.Forced.Enqueue(Face.Trava); m.Forced.Enqueue(Face.Esquiva);
            Assert.IsTrue(Do("mover",new Cell(3,5)));
            Assert.AreEqual(new Cell(3,5),A.cell);
        }
        [Test] public void OnlyOneSummonAndItActsNext()
        {
            Put(A,5,5); int owner=A.id;
            Assert.IsTrue(Do("skill",new Cell(5,6),"invocacao-dragao"));
            A.pa=6; A.used.Clear();
            Assert.IsFalse(Do("skill",new Cell(6,5),"invocacao-dragao"),"máximo 1 ativa");
            Assert.IsTrue(Do("passar"));
            Assert.AreEqual(owner,A.summoner);
        }
        [Test] public void KnockoutCostsGaleoesAndEndsMatch()
        {
            m=Match.New(new MatchSetup{ flatMap=true, galeoes=4 });
            var e=Enemy; Put(A,5,5); Put(e,5,6); e.hp=1;
            m.Forced.Enqueue(Face.Escudo); m.Forced.Enqueue(Face.Trava);
            Assert.IsTrue(Do("skill",new Cell(5,6),"dragao-dourado"));
            Assert.AreEqual(0,S.galeoes[e.team]); Assert.IsTrue(S.over); Assert.AreEqual(3-e.team,S.winner);
        }
        [Test] public void RejectsWrongTeamAndStaleRevision()
        {
            Assert.IsFalse(m.Apply(Cmd("passar"),3-A.team,out _,out _));
            var c=Cmd("passar"); c.revision--; Assert.IsFalse(m.Apply(c,A.team,out _,out _));
        }
        [Test] public void SameSeedSameCommandsSameState()
        {
            string Run() { var x=Match.New(new MatchSetup{ seed=7 }); for(int i=0;i<6;i++) x.Apply(new Command{kind="passar",fighter=x.S.Active.id,revision=x.S.revision},x.S.Active.team,out _,out _); return JsonUtility.ToJson(x.S); }
            Assert.AreEqual(Run(),Run());
        }
        void Stock(string item,int coins=10) { S.shops[A.team].slots[0]=item; S.coins[A.team]=coins; }
        // Compra numa casa ao lado da loja: põe uma casa de loja vizinha ao lutador só durante o comando.
        bool Buy(int slot=0)
        {
            var n=Board.Dirs.Select(d=>A.cell+d).First(x=>Board.Contains(S,x)); var t=S.At(n); var old=t.terrain; t.terrain=Terrain.Loja;
            var c=Cmd("comprar"); c.slot=slot; bool ok=m.Apply(c,A.team,out _,out _); t.terrain=old; return ok;
        }
        [Test] public void BuyingOnlyNextToTheShop()
        {
            m=Match.New(new MatchSetup{ seed=3 }); Stock("botas-aladas");
            var c=Cmd("comprar"); c.slot=0; Assert.IsFalse(m.Apply(c,A.team,out _,out _),"longe da loja");
            var ring=MapGen.ZoneRing(S); Assert.IsTrue(ring.All(x=>Board.NextToShop(S,x)),"as 8 casas encostadas");
            Assert.IsFalse(Board.NextToShop(S,MapGen.ShopCells(S)[0]+new Cell(-1,-1)),"diagonal não vale");
            Put(A,ring[0].x,ring[0].z); c=Cmd("comprar"); c.slot=0; Assert.IsTrue(m.Apply(c,A.team,out _,out _));
        }

        [Test] public void ShopStartsWithThreeDistinctSlotsPerTeam()
        {
            foreach(int t in new[]{1,2}) { var slots=S.shops[t].slots; Assert.AreEqual(Shop.Slots,slots.Length); Assert.IsTrue(slots.All(x=>Shop.Def(x)!=null)); Assert.AreEqual(slots.Length,slots.Distinct().Count()); }
        }
        [Test] public void BuyingCostsCoinsAllowsSeveralAndRefillsAtTurnEnd()
        {
            Stock("botas-aladas",3); Assert.IsFalse(Buy(),"moedas insuficientes");
            S.coins[A.team]=10; var me=A;
            Assert.IsTrue(Buy()); Assert.AreEqual(6,S.coins[me.team]); Assert.AreEqual(4,me.pm,"+1 PM na hora");
            Assert.IsNull(S.shops[me.team].slots[0]);
            S.shops[me.team].slots[1]="botas-aladas"; Assert.IsTrue(Buy(1),"mais de uma compra no turno"); Assert.AreEqual(2,S.coins[me.team]);
            S.shops[me.team].slots[2]="botas-aladas"; Assert.IsFalse(Buy(2),"sem moedas");
            Assert.IsTrue(Do("passar")); Assert.IsNotNull(S.shops[me.team].slots[0],"renova no fim do turno");
            Assert.IsTrue(Do("passar")); Assert.AreSame(me,A); Assert.AreEqual(5,me.pm,"item é permanente");
        }
        [Test] public void SpellIsSingleUse()
        {
            Put(A,5,5); Put(Enemy,5,8); Stock("raio-arcano");
            Assert.IsFalse(Do("skill",new Cell(5,8),"raio-arcano"),"sem comprar");
            Assert.IsTrue(Buy()); m.Forced.Enqueue(Face.Escudo); m.Forced.Enqueue(Face.Trava);
            int hp=Enemy.hp; Assert.IsTrue(Do("skill",new Cell(5,8),"raio-arcano")); Assert.AreEqual(hp-3,Enemy.hp);
            Assert.IsFalse(Do("skill",new Cell(5,8),"raio-arcano"),"carga gasta");
        }
        [Test] public void ArmorItemReducesDamageAndHealCapsAtMax()
        {
            var e=Enemy; e.armor=1; Put(A,5,5); Put(e,5,7); int hp=e.hp;
            m.Forced.Enqueue(Face.Escudo); m.Forced.Enqueue(Face.Trava);
            Assert.IsTrue(Do("skill",new Cell(5,7),"dragao-dourado")); Assert.AreEqual(hp-1,e.hp);
            A.hp=8; Stock("pocao-de-cura"); Assert.IsTrue(Buy());
            Assert.IsTrue(Do("skill",A.cell,"pocao-de-cura")); Assert.AreEqual(11,A.hp,"cura até o máximo");
        }
        [Test] public void TrapHurtsEnemyStopsMoveAndIgnoresOwner()
        {
            Put(A,5,5); var e=Enemy; Put(e,5,11); Stock("armadilha-de-espinhos");
            Assert.IsTrue(Buy()); Assert.IsTrue(Do("skill",new Cell(5,8),"armadilha-de-espinhos"));
            Assert.AreEqual(A.team,S.At(new Cell(5,8)).trap);
            Assert.IsTrue(Do("mover",new Cell(5,8)),"dono passa"); Assert.AreEqual(A.team,S.At(new Cell(5,8)).trap);
            Put(A,2,2); Assert.IsTrue(Do("passar")); Assert.AreSame(e,A); Put(e,5,9); int hp=e.hp;
            Assert.IsTrue(Do("mover",new Cell(5,6)));
            Assert.AreEqual(new Cell(5,8),e.cell,"para na armadilha"); Assert.AreEqual(hp-2,e.hp); Assert.AreEqual(0,e.pm);
            Assert.AreEqual(0,S.At(new Cell(5,8)).trap,"some depois de disparar");
        }
        [Test] public void BuffLastsTwoTurns()
        {
            var me=A; Stock("furia"); Assert.IsTrue(Buy()); Assert.AreEqual(8,me.pa);
            void Round() { Do("passar"); Do("passar"); }
            Round(); Assert.AreSame(me,A); Assert.AreEqual(8,me.pa,"segundo turno");
            Round(); Assert.AreEqual(6,me.pa,"acabou");
        }
        Fighter Solo(string def) { var f=S.fighters.First(x=>x.def==def); S.active=S.timeline.IndexOf(f.id); f.pa=6; return f; }
        bool Use(Fighter f,string skill,Cell c) => m.Apply(new Command{kind="skill",cell=c,skill=skill,fighter=f.id,revision=S.revision},f.team,out _,out _);
        [Test] public void ScytheHitsAdjacentAndTwoAway()
        {
            m=Match.New(new MatchSetup{ flatMap=true, team1=new[]{"teantha"}, team2=new[]{"ciantia"} });
            var t=Solo("teantha"); var foe=S.fighters.First(f=>f.team!=t.team);
            Put(t,5,5); Put(foe,5,6); Assert.IsTrue(Use(t,"foice-verde",foe.cell),"colado");
            Put(foe,5,7); Assert.IsTrue(Use(t,"foice-verde",foe.cell),"a 2 casas");
        }
        // Lutadores não bloqueiam a visão: o tiro sai e, às vezes, acerta quem está no caminho (nunca quem atira).
        [Test] public void ShotsPassOverFightersAndMayHitWhoIsInTheWay()
        {
            int hitWay=0,hitTarget=0;
            for(uint seed=1;seed<=30;seed++) {
                m=Match.New(new MatchSetup{ seed=seed, flatMap=true, team1=new[]{"damian","ciantia"}, team2=new[]{"ciantia"} });
                var d=Solo("damian"); var ally=S.fighters.First(f=>f.team==d.team&&f!=d); var foe=S.fighters.First(f=>f.team!=d.team);
                d.form="canhao"; Put(d,5,5); Put(ally,5,6); Put(foe,5,8);
                int allyHp=ally.hp,foeHp=foe.hp,dHp=d.hp;
                Assert.IsTrue(Use(d,"canhao-de-mao",foe.cell),"aliado no caminho não bloqueia");
                Assert.AreEqual(dHp,d.hp,"quem atira não se acerta");
                if(ally.hp<allyHp) hitWay++; if(foe.hp<foeHp) hitTarget++;
            }
            Assert.Greater(hitWay,0); Assert.Greater(hitTarget,0);
        }
        [Test] public void MirrorReflectsTheLastAttackAndHasCooldown()
        {
            m=Match.New(new MatchSetup{ flatMap=true, team1=new[]{"ezequiela"}, team2=new[]{"devon"} });
            var e=Solo("ezequiela"); var foe=S.fighters.First(f=>f.def=="devon");
            Put(e,5,5); Put(foe,5,8);
            Assert.IsTrue(m.CanTarget(e,Content.Skill(e,"espelho-primordial"),foe.cell,out _),"conjuração permitida mesmo antes de existir ataque para refletir");
            foe.lastAttack="golpe-necrotico"; int hp=foe.hp;
            m.Forced.Enqueue(Face.Trava); m.Forced.Enqueue(Face.Esquiva);
            Assert.IsTrue(Use(e,"espelho-primordial",foe.cell));
            Assert.AreEqual(hp-4,foe.hp,"golpe necrótico (4) refletido");
            e.pa=6; Assert.IsFalse(Use(e,"espelho-primordial",foe.cell),"recarga");
            Assert.AreEqual(3,S.fighters.First(f=>f.def=="ezequiela").cooldowns[0].uses);
        }
        [Test] public void EveryUltimateHasALimit()
        {
            var ults=Content.Fighters.Values.SelectMany(d=>d.skills).Where(k=>k.ultimate).ToList();
            Assert.GreaterOrEqual(ults.Count,4);
            foreach(var k in ults) Assert.IsTrue(k.perTurn>0||k.perMatch>0||k.cooldown>0,k.id);
        }
        // Dano em área acerta aliados na área, mas nunca quem ataca (sem auto-hit).
        [Test] public void AreaDamageHitsAlliesButNotTheCaster()
        {
            m=Match.New(new MatchSetup{ flatMap=true, team1=new[]{"talyssa","ciantia"}, team2=new[]{"ciantia"} });
            var tal=S.fighters.First(f=>f.def=="talyssa"); var ally=S.fighters.First(f=>f.team==tal.team&&f!=tal); var foe=S.fighters.First(f=>f.team!=tal.team);
            S.active=S.timeline.IndexOf(tal.id); tal.pa=6;
            Put(tal,5,5); Put(foe,5,6); Put(ally,6,6);
            int talHp=tal.hp, allyHp=ally.hp, foeHp=foe.hp;
            m.Forced.Enqueue(Face.Trava); m.Forced.Enqueue(Face.Esquiva);
            Assert.IsTrue(m.Apply(new Command{kind="skill",cell=foe.cell,skill="corte-em-arco",fighter=tal.id,revision=S.revision},tal.team,out _,out _));
            Assert.Less(foe.hp,foeHp,"alvo"); Assert.Less(ally.hp,allyHp,"aliado na área"); Assert.AreEqual(talHp,tal.hp,"quem ataca não se acerta");
        }
        // Até 6 jogadores: todos contra todos (time 0) ou times (mesmo número = mesmo time).
        Match Players(params int[] teams) => Match.New(new MatchSetup{ flatMap=true, galeoes=4, players=teams.Select(t=>new PlayerSetup{team=t}).ToArray() });

        [Test] public void SixPlayersGrowTheMapAndRotateTurns()
        {
            m=Players(0,0,0,0,0,0);
            Assert.AreEqual(18,S.width); Assert.AreEqual(6,S.players); Assert.IsFalse(S.TeamPlay);
            CollectionAssert.AreEqual(new[]{1,2,3,4,5,6},S.timeline.Select(id=>S.F(id).team).ToArray());
            Assert.AreEqual(6,S.fighters.Select(f=>f.cell).Distinct().Count());
            for(int p=1;p<=6;p++) { Assert.AreEqual(p,S.Team(p)); Assert.AreEqual(Shop.Slots,S.shops[p].slots.Length); }
        }
        [Test] public void GeneratedMapsWorkForEveryPlayerCount()
        {
            for(int n=2;n<=6;n++) for(uint seed=1;seed<8;seed++)
            {
                var s=Match.New(new MatchSetup{ seed=seed, players=Enumerable.Range(0,n).Select(_=>new PlayerSetup()).ToArray() }).S;
                Assert.AreEqual(Board.SizeFor(n),s.width);
                for(int i=0;i<s.tiles.Length;i++) { var c=new Cell(i%s.width,i/s.width); Assert.AreEqual(s.At(c).height,s.At(s.Mirror(c)).height); }
                Assert.IsTrue(Board.Connected(s),$"{n} jogadores, seed {seed}");
                Assert.AreEqual(n,s.fighters.Select(f=>f.cell).Distinct().Count());
                Assert.IsTrue(s.fighters.All(f=>s.At(f.cell).terrain==Terrain.Livre));
            }
        }
        [Test] public void LastPlayerStandingWins()
        {
            m=Players(0,0,0);
            var p2=S.fighters.First(f=>f.team==2); var p3=S.fighters.First(f=>f.team==3);
            Put(A,5,5); Put(p2,5,6); Put(p3,5,7); p2.hp=1; p3.hp=1;
            m.Forced.Enqueue(Face.Escudo); m.Forced.Enqueue(Face.Trava);
            Assert.IsTrue(Do("skill",new Cell(5,6),"dragao-dourado"));
            Assert.IsTrue(S.eliminated[2]); Assert.IsFalse(S.over,"ainda há 2 jogadores");
            m.Forced.Enqueue(Face.Escudo); m.Forced.Enqueue(Face.Trava);
            Assert.IsTrue(Do("skill",new Cell(5,7),"dragao-dourado"));
            Assert.IsTrue(S.over); Assert.AreEqual(1,S.winner);
        }
        [Test] public void TeamsShareGaleoesAndAlliesAreNotEnemies()
        {
            m=Players(1,2,1,2); m.S.galeoes[1]=m.S.galeoes[2]=12;
            Assert.AreEqual(16,S.width); Assert.IsTrue(S.TeamPlay); Assert.IsTrue(S.Ally(1,3)); Assert.IsFalse(S.Ally(1,2));
            CollectionAssert.AreEqual(new[]{1,2,1,2},S.timeline.Select(id=>S.Team(S.F(id).team)).ToArray());
            Assert.IsTrue(S.fighters.Where(f=>S.Team(f.team)==1).All(f=>f.cell.z<S.height/2),"aliados do mesmo lado");
            var ally=S.fighters.First(f=>f.team==3); var enemy=S.fighters.First(f=>f.team==2);
            Put(A,5,5); Put(ally,5,6); Put(enemy,9,9);
            Assert.IsTrue(Do("skill",new Cell(5,6),"dragao-dourado"),"conjuração livre no chão");
            Assert.IsTrue(m.Apply(Cmd("mover",new Cell(4,5)),A.team,out var ev,out _));
            Assert.IsFalse(ev.Any(e=>e.kind=="trava"),"aliado não trava");
            Put(enemy,4,7); enemy.hp=1; m.Forced.Enqueue(Face.Escudo); m.Forced.Enqueue(Face.Trava);
            Assert.IsTrue(Do("skill",new Cell(4,7),"dragao-dourado"));
            Assert.AreEqual(12-Content.Def(enemy).level,S.galeoes[2]); Assert.AreEqual(12,S.galeoes[1]);
            Assert.IsFalse(S.eliminated[2],"P4 segue no time 2"); Assert.IsFalse(S.over);
        }
        [Test] public void AlliesSeeEachOthersTraps()
        {
            m=Players(1,2,1,2); S.At(new Cell(3,3)).trap=1;
            Assert.AreEqual(1,Match.ViewFor(S,3).At(new Cell(3,3)).trap);
            Assert.AreEqual(0,Match.ViewFor(S,2).At(new Cell(3,3)).trap);
        }
        [Test] public void ThreeVsThreeAndTwoTeamsOfThreeSpawnApart()
        {
            m=Players(1,2,1,2,1,2);
            Assert.IsTrue(S.fighters.Where(f=>S.Team(f.team)==1).All(f=>f.cell.z<S.height/2));
            Assert.IsTrue(S.fighters.Where(f=>S.Team(f.team)==2).All(f=>f.cell.z>=S.height/2));
            m=Players(1,2,3,1,2,3);
            Assert.AreEqual(3,S.Teams().Count); Assert.AreEqual(6,S.fighters.Select(f=>f.cell).Distinct().Count());
            CollectionAssert.AreEqual(new[]{1,2,3,1,2,3},S.timeline.Select(id=>S.Team(S.F(id).team)).ToArray());
        }
        [Test] public void PetCoexistsWithDragon()
        {
            Put(A,5,5); Stock("lobo-de-cinzas");
            Assert.IsTrue(Do("skill",new Cell(5,6),"invocacao-dragao"));
            Assert.IsTrue(Buy()); Assert.IsTrue(Do("skill",new Cell(6,5),"chamar-lobo"));
            Assert.AreEqual(2,S.fighters.Count(x=>x.Alive&&x.summoner==A.id));
        }

        // ── Eventos, controle, suporte e elenco de 10/10 (EVENTOS-LOJA-ELENCO.md) ──
        void Duel(string a,string b) { m=Match.New(new MatchSetup{ flatMap=true, team1=new[]{a}, team2=new[]{b} }); }
        void Hit(Face attack=Face.Trava,Face defense=Face.Trava) { m.Forced.Enqueue(attack); m.Forced.Enqueue(defense); }
        void PassRound() { int round=S.round; for(int i=0;i<20&&S.round==round&&!S.over;i++) Do("passar"); }

        [Test] public void DrillHookPushesUntilObstacleAndBumpHurts()
        {
            Duel("cheiro-pneu","ogro-de-pedra"); Put(A,5,5); Put(Enemy,5,6); S.At(new Cell(5,9)).terrain=Terrain.Arvore;
            Hit(); Assert.IsTrue(Do("skill",new Cell(5,6),"gancho-broca"));
            Assert.AreEqual(new Cell(5,8),Enemy.cell); Assert.AreEqual(16-4-1,Enemy.hp,"4 do golpe + 1 da batida");
            Assert.IsFalse(Do("skill",new Cell(5,7),"gancho-broca"),"recarga");
        }
        [Test] public void GhostFormDodgesBlowsForOneTurn()
        {
            Duel("edgar","rei-bobett"); var edgar=A; var king=Enemy; Put(edgar,5,5); Put(king,5,6);
            Assert.IsTrue(Do("skill",new Cell(5,5),"forma-fantasma")); Assert.AreEqual("fantasma",edgar.form);
            Do("passar"); Assert.AreSame(king,A);
            Hit(); Assert.IsTrue(Do("skill",new Cell(5,5),"coroada"),"conjura no chão ocupado pelo fantasma"); Assert.AreEqual(13,edgar.hp,"fantasma segue imune");
            Do("passar"); Assert.AreSame(edgar,A); Assert.AreEqual("normal",edgar.form,"dura 1 turno");
            Do("passar"); Hit(); Assert.IsTrue(Do("skill",new Cell(5,5),"coroada")); Assert.AreEqual(13-2,edgar.hp);
        }
        [Test] public void GhostShipUltimateSummonsOnce()
        {
            Duel("edgar","rei-bobett"); Put(A,5,5); Put(Enemy,9,9);
            Assert.IsTrue(Do("skill",new Cell(5,6),"navio-fantasma"));
            Assert.AreEqual("navio-fantasma",Board.At(S,new Cell(5,6)).def);
            Assert.IsFalse(Do("skill",new Cell(6,5),"navio-fantasma"),"1 por partida");
        }
        [Test] public void ImmovableAndAnchorResistPush()
        {
            Duel("cheiro-pneu","colosso"); Put(A,5,5); Put(Enemy,5,6);
            Hit(); Assert.IsTrue(Do("skill",new Cell(5,6),"gancho-broca"));
            Assert.AreEqual(new Cell(5,6),Enemy.cell);
        }
        [Test] public void PrisonSkipsTurnBlocksTargetingAndDodgeEscapes()
        {
            Duel("principe-kellen","rei-bobett"); var kellen=A; var king=Enemy; Put(kellen,5,5); Put(king,5,7);
            Hit(Face.Trava,Face.Esquiva); Assert.IsTrue(Do("skill",new Cell(5,7),"prisao-dos-pesadelos"));
            Assert.AreEqual(0,king.prisoned,"Esquiva escapa");
            kellen.cooldowns.Clear();
            Hit(Face.Trava,Face.Escudo); Assert.IsTrue(Do("skill",new Cell(5,7),"prisao-dos-pesadelos"));
            Assert.AreEqual(1,king.prisoned);
            Put(king,5,6); Assert.IsFalse(Do("skill",new Cell(5,6),"pauzinho"),"preso não é alvo");
            Do("passar");
            Assert.AreEqual(kellen.id,A.id,"o turno do preso foi pulado"); Assert.AreEqual(0,king.prisoned); Assert.AreEqual(2,S.round);
        }
        [Test] public void IceCostsOneMoreMovementUntilCastersNextTurn()
        {
            Duel("principe-kellen","rei-bobett"); var king=Enemy; Put(A,5,5); Put(king,5,9);
            Assert.IsTrue(Do("skill",new Cell(5,8),"chao-de-gelo"));
            Assert.AreNotEqual(0,S.At(new Cell(5,8)).frozenBy);
            Assert.IsNull(Board.Path(S,king,new Cell(5,7),2),"5,8 e 5,7 congeladas: 4 PM");
            Assert.IsNotNull(Board.Path(S,king,new Cell(5,8),2));
            PassRound();
            Assert.AreEqual(0,S.At(new Cell(5,8)).frozenBy,"derrete no turno seguinte de quem congelou");
        }
        [Test] public void KingIgnoresOneDamage()
        {
            Duel("principe-kellen","rei-bobett"); Put(A,5,5); Put(Enemy,5,8);
            Hit(); Assert.IsTrue(Do("skill",new Cell(5,8),"bola-de-fogo"));
            Assert.AreEqual(12-2,Enemy.hp);
        }
        [Test] public void NeedlePiercesArmorAndShieldAndPunishesIsolation()
        {
            Duel("viuva-da-agulha","rei-bobett"); Put(A,5,5); Put(Enemy,5,6);
            Hit(Face.Trava,Face.Escudo); Assert.IsTrue(Do("skill",new Cell(5,6),"agulha"));
            Assert.AreEqual(12-4,Enemy.hp,"2 + 2 (isolado), ignora Escudo e a armadura do rei");
        }
        [Test] public void ColossusDevoursTheMainSkillWithOneMorePa()
        {
            m=Match.New(new MatchSetup{ flatMap=true, team1=new[]{"colosso"}, team2=new[]{"talyssa","ogro-de-pedra"} });
            var victim=S.fighters.First(f=>f.def=="talyssa"); while(A.def!="colosso") Do("passar"); Put(A,5,5); Put(victim,5,6); victim.hp=1;
            Hit(); Assert.IsTrue(Do("skill",new Cell(5,6),"pancada-colossal"));
            Assert.IsFalse(victim.Alive); CollectionAssert.Contains(A.stolen,"lamina-rosa");
            var stolen=Match.Skills(A).First(s=>s.id==Content.Stolen+"lamina-rosa");
            Assert.AreEqual(4,stolen.pa); Assert.AreEqual("steal-slam",stolen.anim);
        }
        [Test] public void CatsHaveNoTurnObeyAndVanishAfterShooting()
        {
            Duel("asamyh","ogro-de-pedra"); Put(A,5,5); Put(Enemy,5,9);
            Assert.IsTrue(Do("skill",new Cell(5,6),"chamar-gato"));
            var cat=S.fighters.Last(); cat.def="gato-laranja";
            CollectionAssert.DoesNotContain(S.timeline,cat.id);
            Assert.IsTrue(Do("skill",new Cell(5,8),"mover-gato")); Assert.AreEqual(new Cell(5,8),cat.cell);
            Hit(); Assert.IsTrue(Do("skill",new Cell(5,9),"faisca-da-varinha"));
            Assert.AreEqual(16-2,Enemy.hp); Assert.IsFalse(cat.Alive);
            Assert.IsFalse(Do("skill",new Cell(5,9),"faisca-da-varinha"),"sem gatos");
        }
        [Test] public void NightOfCatsHealsShieldsAndFillsCats()
        {
            Duel("asamyh","ogro-de-pedra"); Put(A,5,5); A.hp=5;
            Assert.IsTrue(Do("skill",A.cell,"noite-dos-gatos"));
            Assert.AreEqual(7,A.hp); Assert.AreEqual(2,S.fighters.Count(x=>x.Alive&&x.summoner==A.id));
            Assert.IsTrue(A.buffs.Any(b=>b.armor==1));
        }
        [Test] public void BarkGivesArmorToAnAlly()
        {
            m=Match.New(new MatchSetup{ flatMap=true, team1=new[]{"guardiao-de-musgo","rei-bobett"}, team2=new[]{"ogro-de-pedra"} });
            var ally=S.fighters.First(f=>f.def=="rei-bobett"); Put(A,5,5); Put(ally,5,7);
            Assert.IsTrue(Do("skill",new Cell(5,7),"casca"));
            Assert.IsTrue(ally.buffs.Any(b=>b.armor==2));
            Assert.IsFalse(Do("skill",Enemy.cell,"casca"),"só aliado");
        }
        [Test] public void WishNeedsRoundTenChannelsTwoTurnsThenGrantsAChoice()
        {
            Duel("principe-kellen","rei-bobett"); var kellen=A; Put(kellen,5,5);
            Assert.IsFalse(Do("skill",kellen.cell,"desejo"),"antes da rodada 10");
            S.round=10; Assert.IsTrue(Do("skill",kellen.cell,"desejo")); Assert.AreEqual(0,kellen.pa);
            PassRound(); Assert.AreEqual(kellen.id,A.id); Assert.AreEqual(0,A.pa,"2º turno só canaliza");
            Assert.IsFalse(Match.Skills(A).Any(s=>s.kind==SkillKind.Desejo));
            PassRound(); Assert.IsTrue(kellen.wishReady);
            kellen.hp=2; Assert.IsTrue(Do("skill",kellen.cell,"desejo-b")); Assert.AreEqual(7,kellen.hp);
            Assert.IsFalse(kellen.wishReady); Assert.IsFalse(Do("skill",kellen.cell,"desejo"),"uso único");
        }
        [Test] public void WishCanCancelTheActiveEvent()
        {
            Duel("principe-kellen","rei-bobett"); A.wishReady=true; m.Trigger("nevoeiro");
            Assert.AreEqual("nevoeiro",S.ev);
            Assert.IsTrue(Do("skill",A.cell,"desejo-f")); Assert.AreEqual("",S.ev);
        }
        [Test] public void LavaSpreadsBurnsAndCoolsDown()
        {
            m.Trigger("mare-de-lava");
            Assert.AreEqual("mare-de-lava",S.ev); Assert.AreEqual(2,S.tiles.Count(t=>t.lava));
            PassRound(); Assert.Greater(S.tiles.Count(t=>t.lava),2,"espalha");
            var lava=Enumerable.Range(0,S.tiles.Length).First(i=>S.tiles[i].lava);
            int hp=A.hp; A.cell=new Cell(lava%S.width,lava/S.width); Put(Enemy,0,0);
            var cell=A.cell; var free=Board.Dirs.Select(d=>cell+d).First(c=>Board.Contains(S,c)&&!S.At(c).lava&&Board.At(S,c)==null);
            A.cell=free; Assert.IsTrue(Do("mover",cell)); Assert.AreEqual(hp-2,A.hp,"entrar na lava queima");
            PassRound(); PassRound();
            Assert.AreEqual("",S.ev); Assert.AreEqual(0,S.tiles.Count(t=>t.lava));
        }
        [Test] public void FireworksExplodeAtTheEndOfTheRound()
        {
            m.Trigger("festival-de-fogos");
            var mark=Enumerable.Range(0,S.tiles.Length).First(i=>S.tiles[i].mark);
            var target=Enemy; Put(target,mark%S.width,mark/S.width); int hp=target.hp;
            m.Forced.Enqueue(Face.Trava); PassRound();
            Assert.Less(target.hp,hp); Assert.AreEqual(0,S.tiles.Count(t=>t.mark)); Assert.AreEqual("",S.ev);
        }
        [Test] public void WanderingBossHuntsAndLeaves()
        {
            m.Trigger("errante");
            var boss=S.fighters.Single(f=>f.def=="brutamontes-errante"); Assert.AreEqual(MatchState.Neutral,boss.team);
            CollectionAssert.DoesNotContain(S.timeline,boss.id);
            int d0=S.fighters.Where(f=>f.team!=MatchState.Neutral).Min(f=>f.cell.Distance(boss.cell));
            PassRound();
            Assert.Less(S.fighters.Where(f=>f.team!=MatchState.Neutral&&f.Alive).Min(f=>f.cell.Distance(boss.cell)),d0);
            for(int i=0;i<5&&!S.over;i++) PassRound();
            Assert.IsFalse(boss.Alive); Assert.AreEqual("",S.ev);
        }
        [Test] public void KillingTheCoinEaterPaysItsStomach()
        {
            m.Trigger("papa-moedas");
            var eater=S.fighters.First(f=>f.def=="papa-moedas"); eater.carried=3; Put(A,5,5); Put(eater,5,6);
            int coins=S.coins[A.team];
            Hit(); Assert.IsTrue(Do("skill",new Cell(5,6),"dragao-dourado"));
            Hit(); Do("skill",new Cell(5,6),"dragao-dourado");
            Assert.IsFalse(eater.Alive); Assert.AreEqual(coins+5,S.coins[A.team]);
        }
        [Test] public void EveryEventRunsForSixRoundsWithoutBreaking()
        {
            foreach(var e in Match.Cataclysms)
            {
                Setup(); m=Match.New(new MatchSetup{ seed=7, team1=new[]{"ciantia","colosso"}, team2=new[]{"principe-kellen","asamyh"} });
                m.Trigger(e.id);
                for(int i=0;i<6&&!S.over;i++) PassRound();
                Assert.IsTrue(e.rounds==0||S.ev!=e.id,e.id);
            }
        }
        [Test] public void NewShopItemsApply()
        {
            S.coins[A.team]=20;
            foreach(var id in new[]{"ancora","gancho-de-escalada","cristal-de-foco","manto-de-espinhos"})
            { S.shops[A.team].slots[0]=id; A.bought=false; Assert.IsTrue(Buy(),id); }
            Assert.IsTrue(A.anchored); Assert.IsTrue(A.climber); Assert.IsTrue(A.focused); Assert.AreEqual(1,A.thorns);
            Put(A,5,5); S.At(new Cell(5,6)).height=3; A.pm=3;
            Assert.IsNotNull(Board.Path(S,A,new Cell(5,6),2),"gancho sobe 3 com 2 PM");
        }
        [Test] public void IceMineTakesMovementAndAction()
        {
            S.coins[A.team]=20; S.shops[A.team].slots[0]="mina-de-gelo"; Assert.IsTrue(Buy());
            Put(A,5,5); Assert.IsTrue(Do("skill",new Cell(5,7),"mina-de-gelo"));
            Do("passar"); Put(A,5,8); Assert.IsTrue(Do("mover",new Cell(5,7)));
            Assert.AreEqual(0,A.pm); var enemy=A;
            Do("passar"); Do("passar"); Assert.AreEqual(enemy.id,A.id); Assert.AreEqual(4,A.pa,"−2 PA no turno seguinte");
        }
        [Test] public void PhoenixFeatherRevivesAnAlly()
        {
            m=Match.New(new MatchSetup{ flatMap=true, team1=new[]{"ciantia","talyssa"}, team2=new[]{"ogro-de-pedra"} });
            var ally=S.fighters.First(f=>f.def=="talyssa"); ally.hp=0; Put(A,5,5);
            S.coins[A.team]=20; S.shops[A.team].slots[0]="pena-de-fenix"; Assert.IsTrue(Buy());
            Assert.IsTrue(Do("skill",new Cell(5,6),"pena-de-fenix"));
            Assert.AreEqual(4,ally.hp); Assert.AreEqual(new Cell(5,6),ally.cell);
        }
        [Test] public void BaltazarAuraCostsOnePaPerTurnHealsAlliesAndHurtsFoes()
        {
            m=Match.New(new MatchSetup{ flatMap=true, team1=new[]{"baltazar","ogro-de-pedra"}, team2=new[]{"ogro-de-pedra"} });
            var b=A; Assert.AreEqual("baltazar",b.def);
            var ally=S.fighters.First(f=>f.team==b.team&&f!=b); var foe=Enemy;
            Put(b,5,5); Put(ally,5,7); Put(foe,7,5); ally.hp=10; int foeHp=foe.hp;
            Assert.IsTrue(Do("skill",b.cell,"baltazar-dualidade"));
            Assert.IsTrue(b.auraOn); Assert.AreEqual(5,b.pa); Assert.AreEqual(11,ally.hp); Assert.AreEqual(foeHp-1,foe.hp);
            // Desligar e religar no mesmo turno: sem custo e sem aplicar de novo.
            Assert.IsTrue(Do("skill",b.cell,"baltazar-dualidade")); Assert.IsFalse(b.auraOn);
            Assert.IsTrue(Do("skill",b.cell,"baltazar-dualidade")); Assert.IsTrue(b.auraOn);
            Assert.AreEqual(5,b.pa); Assert.AreEqual(11,ally.hp); Assert.AreEqual(foeHp-1,foe.hp);
            // Próximo turno dele: cobra 1 PA e aplica de novo; fora do alcance, nada.
            Put(ally,5,9); PassRound();
            Assert.AreSame(b,A); Assert.IsTrue(b.auraOn); Assert.AreEqual(5,b.pa); Assert.AreEqual(11,ally.hp); Assert.AreEqual(foeHp-2,foe.hp);
            // Sem PA para manter: desliga.
            b.buffs.Add(new Buff{id="teste",turns=3,pa=-6}); PassRound();
            Assert.AreSame(b,A); Assert.IsFalse(b.auraOn); Assert.AreEqual(foeHp-2,foe.hp);
        }
        [Test] public void WishCopyLetsThePlayerPickWhichSkill()
        {
            Duel("principe-kellen","ogro-de-pedra"); A.wishReady=true; Put(A,5,5); Put(Enemy,5,8);
            CollectionAssert.AreEqual(new[]{"clava","arremessar-pedra"},Match.Copyables(A,Enemy).Select(x=>x.id).ToArray());
            Command Copy(int slot) => new Command{kind="skill",skill="desejo-d",cell=Enemy.cell,slot=slot,fighter=A.id,revision=S.revision};
            Assert.IsFalse(m.Apply(Copy(5),A.team,out _,out _),"opção que não existe");
            Assert.IsTrue(m.Apply(Copy(1),A.team,out _,out _));
            CollectionAssert.AreEqual(new[]{"arremessar-pedra"},A.learned);
        }
        [Test] public void DamianFormsHoldTheirWeaponPose()
        {
            Duel("damian","ogro-de-pedra");
            Assert.IsNull(Content.Form(A).pose,"pistola usa o idle normal");
            Assert.IsTrue(Do("skill",A.cell,"camaleao-martelo")); Assert.AreEqual("hammer",Content.Form(A).pose);
            Assert.IsTrue(Do("skill",A.cell,"camaleao-canhao")); Assert.AreEqual("cannon",Content.Form(A).pose);
        }
        [Test] public void SavedMapIsUsedAndKeepsSpawnsFree()
        {
            var map=MapGen.NewMap(12);
            Tile T(int x,int z) => map.tiles[z*12+x];
            T(4,3).terrain=Terrain.Arvore; T(4,3).height=2; T(2,1).terrain=Terrain.Caixa; T(6,6).zone=true; T(3,8).zone=true;
            Assert.IsNull(MapGen.Problem(map));
            m=Match.New(new MatchSetup{ map=map, team1=new[]{"ciantia"}, team2=new[]{"ciantia"} });
            Assert.AreEqual(Terrain.Arvore,S.At(new Cell(4,3)).terrain); Assert.AreEqual(2,S.At(new Cell(4,3)).height);
            Assert.AreEqual(Terrain.Livre,S.At(new Cell(2,1)).terrain,"casa de início fica livre");
            // O meio é a loja (bloqueia); a zona marcada no mapa continua valendo fora dela.
            Assert.AreEqual(Terrain.Loja,S.At(new Cell(6,6)).terrain); Assert.IsFalse(S.At(new Cell(6,6)).zone); Assert.IsTrue(S.At(new Cell(3,8)).zone);
            // Mapa de outro tamanho é ignorado (3 jogadores usam 16×16).
            m=Match.New(new MatchSetup{ map=map, players=new[]{ new PlayerSetup(), new PlayerSetup(), new PlayerSetup() } });
            Assert.AreEqual(16,S.width);
            // Muralha de árvores separando os lados: não dá para jogar.
            for(int x=0;x<12;x++) T(x,3).terrain=Terrain.Arvore;
            Assert.IsNotNull(MapGen.Problem(map));
        }
        // Decisão do autor (11/10): a loja ocupa as 4 casas do meio, ninguém entra e ela tapa a visão; a zona são as 8 casas encostadas.
        [Test] public void ShopBlocksTheMiddleAndZoneIsTheRing()
        {
            m=Match.New(new MatchSetup{ seed=5, team1=new[]{"ciantia"}, team2=new[]{"ciantia"} });
            foreach(var c in MapGen.ShopCells(S)) { Assert.AreEqual(Terrain.Loja,S.At(c).terrain); Assert.IsFalse(Board.Walkable(S,c,true)); Assert.IsFalse(S.At(c).zone); }
            Assert.AreEqual(8,S.tiles.Count(t=>t.zone)); foreach(var c in MapGen.ZoneRing(S)) Assert.IsTrue(S.At(c).zone);
            Assert.IsFalse(Board.LineOfSight(S,new Cell(4,5),new Cell(7,5)),"a loja tapa a visão");
            Assert.Greater(S.decor.Count,0,"mapa gerado ganha enfeites");
        }
        // Os 16 mapas do Codex (Resources/Mapas/codex.json) passam na validação e entram na partida com a loja no meio.
        [Test] public void CodexMapsAreAllPlayable()
        {
            ArenaMaps.Reset();
            Assert.AreEqual(22,ArenaMaps.Builtin.Count,"algum mapa do Codex não passou em MapGen.Problem");
            foreach(var map in ArenaMaps.Builtin)
            {
                int players=MapGen.PlayersFor(map.width);
                m=Match.New(new MatchSetup{ map=map, players=Enumerable.Range(0,players).Select(_=>new PlayerSetup()).ToArray() });
                Assert.AreEqual(map.width,S.width,map.name); Assert.AreEqual(map.theme,S.theme,map.name);
                Assert.IsTrue(MapGen.ShopCells(S).All(c=>S.At(c).terrain==Terrain.Loja),map.name);
                Assert.Greater(S.decor.Count,0,map.name); Assert.IsTrue(Board.Connected(S),map.name);
            }
        }
        [Test] public void BotsPlayOnlyLegalActionsAndFight()
        {
            foreach(var (a,b) in new[]{ ("baltazar","damian"), ("tchu-tcha","principe-kellen"), ("asamyh","arqueiro-jack"), ("colosso","ezequiela"), ("edgar","brumak"), ("ossyro","gryvro"), ("alvyr","raizor"), ("nixira","edgar") })
            {
                m=Match.New(new MatchSetup{ seed=7, team1=new[]{a,"perry"}, team2=new[]{b,"teantha"} });
                int actions=0;
                while(!S.over&&S.round<8&&actions<800)
                {
                    var cmd=Bot.Decide(m); cmd.fighter=A.id; cmd.revision=S.revision; actions++; string who=A.def;
                    bool ok=m.Apply(cmd,A.team,out _,out var reason);
                    Assert.IsTrue(ok,$"{a}×{b}: {who} {cmd.kind} {cmd.skill} recusado: {reason}");
                }
                Assert.IsTrue(S.over||S.round>=8,$"{a}×{b}: travou na rodada {S.round}");
                Assert.IsTrue(S.fighters.Any(f=>f.summoner==0&&f.hp<Content.MaxHp(f)),$"{a}×{b}: ninguém se feriu");
            }
        }

        // ── Pedidos do autor de 13/10 ──
        [Test] public void AlternateInitiativeInterleavesTeams()
        {
            m=Match.New(new MatchSetup{ flatMap=true, turns=TurnMode.Alternada, team1=new[]{"devon","gryvro"}, team2=new[]{"teantha","diantus"} });
            CollectionAssert.AreEqual(new[]{2,1,2,1},S.timeline.Select(id=>S.F(id).team).ToArray(),"um de cada time por vez, começando pela maior soma");
            CollectionAssert.AreEqual(new[]{"gryvro","devon"},S.timeline.Where(id=>S.F(id).team==1).Select(id=>S.F(id).def).ToArray(),"maior Iniciativa primeiro");
            m=Match.New(new MatchSetup{ flatMap=true, team1=new[]{"devon","gryvro"}, team2=new[]{"teantha","diantus"} });
            CollectionAssert.AreEqual(new[]{2,2,1,1},S.timeline.Select(id=>S.F(id).team).ToArray(),"padrão continua Krosmaster");
        }
        [Test] public void GhostFormIgnoresEveryDamage()
        {
            Duel("edgar","rei-bobett"); var edgar=A; Put(edgar,5,5); Put(Enemy,9,9);
            Assert.IsTrue(Do("skill",new Cell(5,5),"forma-fantasma"));
            var t=S.At(new Cell(5,6)); t.trap=Enemy.team; t.trapDamage=4; S.At(new Cell(5,7)).lava=true;
            Assert.IsTrue(Do("mover",new Cell(5,6))); Assert.AreEqual(13,edgar.hp,"armadilha não fere o fantasma");
        }
        [Test] public void HiddenMovesLessAndAdjacentFoesFindIt()
        {
            Duel("perry","teantha"); var perry=A; var foe=Enemy; Put(perry,5,5); Put(foe,9,9); S.At(new Cell(5,4)).terrain=Terrain.Arvore;
            Assert.IsTrue(Do("skill",new Cell(5,5),"esconder")); Assert.IsTrue(perry.hidden);
            Assert.AreEqual(Match.HiddenPm,perry.pm,"escondido anda no máximo 2");
            Do("passar"); Assert.AreSame(foe,A); Put(foe,5,7);
            Assert.IsTrue(m.CanTarget(foe,Content.Skill(foe,"foice-verde"),perry.cell,out _),"conjuração no chão permite tentar acertar invisível");
            Assert.IsTrue(Do("mover",new Cell(5,6))); Assert.IsFalse(perry.hidden,"quem encosta revela");
            perry.hidden=true; Assert.IsTrue(m.CanTarget(foe,Content.Skill(foe,"foice-verde"),perry.cell,out _),"colado acerta");
        }
        [Test] public void IllusionAlwaysNextToEzequielaWithRandomSide()
        {
            var sides=new System.Collections.Generic.HashSet<string>(); int copyFirst=0;
            for(uint seed=1;seed<=40;seed++) {
                m=Match.New(new MatchSetup{ seed=seed, flatMap=true, team1=new[]{"ezequiela"}, team2=new[]{"ciantia"} });
                var eze=S.fighters.First(f=>f.def=="ezequiela"); var copy=S.fighters.First(f=>f.def=="ilusao");
                Assert.AreEqual(1,eze.cell.Distance(copy.cell),"sempre colada"); Assert.AreEqual(eze.id,copy.summoner);
                sides.Add($"{copy.cell.x-eze.cell.x},{copy.cell.z-eze.cell.z}");
                if(S.timeline.IndexOf(copy.id)<S.timeline.IndexOf(eze.id)) copyFirst++;
                var view=Match.ViewFor(S,2); Assert.IsTrue(view.fighters.Where(f=>f.team==1).All(f=>f.def=="ezequiela"&&f.summoner==0),"inimigo vê duas Ezequielas");
            }
            Assert.Greater(sides.Count,1,"o lado da cópia varia"); Assert.Greater(copyFirst,0); Assert.Less(copyFirst,40);
        }
        [Test] public void RoomShopOnlyOffersChosenItems()
        {
            m=Match.New(new MatchSetup{ flatMap=true, shopItems=new[]{"furia","bencao-de-pedra"} });
            for(int p=1;p<=2;p++) foreach(var id in S.shops[p].slots) Assert.IsTrue(id==null||id=="furia"||id=="bencao-de-pedra",id);
            Assert.AreEqual(2,S.shops[1].slots.Count(x=>x!=null),"só 2 itens liberados: o 3º espaço fica vazio");
        }
        [Test] public void GenericSkillLibraryIsValid()
        {
            Assert.GreaterOrEqual(Content.Generic.Length,30);
            Assert.AreEqual(Content.Generic.Length,Content.Generic.Select(g=>g.skill.id).Distinct().Count());
            foreach(var (_,sk) in Content.Generic) {
                var d=Content.Copy(Content.Fighters["ciantia"]); d.skills=new[]{ sk.Clone() };
                Assert.AreEqual(sk.kind,Content.Sanitize(d).skills[0].kind,sk.id+" sobrevive ao Sanitize");
                Assert.IsFalse(string.IsNullOrEmpty(Content.Describe(sk)));
            }
        }
    }
}
