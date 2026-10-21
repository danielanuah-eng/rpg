using System.Linq;
using NUnit.Framework;

namespace ArenaTatica.Core.Tests
{
    // Elenco de 20/10: Pele D. Negra, L.E.L.A - 0, Gumbald, Daemion e Capitão Ikaito D. Razer.
    public class NewRosterTests
    {
        Match m;
        MatchState S => m.S;
        void New(string[] a,string[] b)
        {
            m=Match.New(new MatchSetup{ flatMap=true, team1=a, team2=b });
            for(int i=0;i<60;i++) m.Forced.Enqueue(Face.Trava);
        }
        Fighter Get(string def,int team=0) => S.fighters.First(f=>f.def==def&&(team==0||f.team==team));
        void Put(Fighter f,int x,int z) => f.cell=new Cell(x,z);
        // Vez de f com PA/PM cheios, sem passar pelo começo de turno.
        void Turn(Fighter f) { S.active=S.timeline.IndexOf(f.id); f.pa=Content.Def(f).pa; f.pm=Content.Def(f).pm; f.used.Clear(); }
        bool Skill(Fighter f,string id,Cell c,Cell c2=default) => m.Apply(new Command{kind="skill",skill=id,cell=c,cell2=c2,fighter=f.id,revision=S.revision},f.team,out _,out _);
        bool Move(Fighter f,Cell c) => m.Apply(new Command{kind="mover",cell=c,fighter=f.id,revision=S.revision},f.team,out _,out _);
        bool Pass(Fighter f) => m.Apply(new Command{kind="passar",fighter=f.id,revision=S.revision},f.team,out _,out _);

        [Test] public void NulificationDispelsFormsBuffsAndGhost()
        {
            New(new[]{"pele-negra"},new[]{"damian"});
            var pele=Get("pele-negra"); var dam=Get("damian"); Put(pele,2,2); Put(dam,3,2);
            dam.form="canhao"; dam.buffs.Add(new Buff{id="x",turns=2,pa=2}); dam.buffs.Add(new Buff{id="confusao",turns=2,pm=-1});
            Turn(pele); Assert.IsTrue(Skill(pele,"nulification",dam.cell));
            Assert.AreEqual("pistola",dam.form); Assert.IsFalse(dam.buffs.Any(b=>b.id=="x")); Assert.IsTrue(dam.buffs.Any(b=>b.id=="confusao"),"debuff fica");
            Assert.AreEqual(Content.Def(dam).hp-2,dam.hp);

            New(new[]{"pele-negra"},new[]{"edgar"});
            pele=Get("pele-negra"); var edgar=Get("edgar"); Put(pele,2,2); Put(edgar,3,2); edgar.form="fantasma";
            Turn(pele); Assert.IsTrue(Skill(pele,"nulification",edgar.cell));
            Assert.AreEqual("normal",edgar.form); Assert.AreEqual(Content.Def(edgar).hp-2,edgar.hp,"anula o fantasma antes do golpe");
        }

        [Test] public void NulificationFieldDrainsAndReturnsPa()
        {
            New(new[]{"pele-negra"},new[]{"talyssa"});
            var pele=Get("pele-negra"); var tal=Get("talyssa"); Put(pele,2,2); Put(tal,3,3);
            Turn(pele); Assert.IsTrue(Pass(pele));
            Assert.AreEqual(tal,S.Active); Assert.AreEqual(Content.Def(tal).pa-3,tal.pa,"começou na diagonal: −3 PA");
            Assert.IsTrue(Move(tal,new Cell(4,3)));
            Assert.AreEqual(Content.Def(tal).pa,tal.pa,"saiu: recupera"); Assert.AreEqual(0,tal.drained);
            Assert.IsTrue(Move(tal,new Cell(3,3)));
            Assert.AreEqual(Content.Def(tal).pa-3,tal.pa,"entrou de novo: perde");
        }

        [Test] public void YamiYamiTakesExtraMeleeDamage()
        {
            New(new[]{"talyssa"},new[]{"pele-negra"});
            var tal=Get("talyssa"); var pele=Get("pele-negra"); Put(tal,2,2); Put(pele,6,9);
            Turn(tal); Put(pele,3,2); Assert.IsTrue(Skill(tal,"lamina-rosa",pele.cell));
            Assert.AreEqual(Content.Def(pele).hp-4,pele.hp,"3 + 1 de golpe colado");
        }

        [Test] public void BlackHoleBanishesAndHurtsEnemyOnReturn()
        {
            New(new[]{"pele-negra","rei-bobett"},new[]{"talyssa"});
            var pele=Get("pele-negra"); var tal=Get("talyssa"); var king=Get("rei-bobett"); Put(pele,2,2); Put(tal,2,5); Put(king,9,9);
            Turn(pele); Assert.IsTrue(Skill(pele,"black-hole",tal.cell));
            Assert.IsTrue(tal.banished); Assert.AreEqual(1,tal.prisoned);
            Assert.IsFalse(m.CanTarget(pele,Content.AnySkill("nulification"),tal.cell,out _)&&Board.Gap(pele,tal)==1);
            for(int i=0;i<4&&tal.banished;i++) Assert.IsTrue(Pass(S.Active));
            Assert.IsFalse(tal.banished); Assert.AreEqual(0,tal.prisoned); Assert.AreEqual(Content.Def(tal).hp-3,tal.hp,"3 ao voltar");

            New(new[]{"pele-negra","rei-bobett"},new[]{"talyssa"});
            pele=Get("pele-negra"); king=Get("rei-bobett"); Put(pele,2,2); Put(king,2,4); Put(Get("talyssa"),9,9);
            Turn(pele); Assert.IsTrue(Skill(pele,"black-hole",king.cell)); Assert.IsTrue(king.banished);
            for(int i=0;i<6&&king.banished;i++) Assert.IsTrue(Pass(S.Active));
            Assert.AreEqual(Content.Def(king).hp,king.hp,"aliado volta ileso");
        }

        [Test] public void LightSpeedScalesWithPmAndOnlyPaysBeyondThree()
        {
            New(new[]{"pele-negra"},new[]{"talyssa"});
            var pele=Get("pele-negra"); Put(pele,0,0); Put(Get("talyssa"),11,11);
            Turn(pele); Assert.IsTrue(Skill(pele,"light-speed",new Cell(0,3))); Assert.AreEqual(2,pele.pm,"3 casas: grátis");
            Turn(pele); Assert.IsFalse(Skill(pele,"light-speed",new Cell(0,9)),"3 + 2 PM = 5 casas no máximo");
            Assert.IsTrue(Skill(pele,"light-speed",new Cell(0,8))); Assert.AreEqual(0,pele.pm);
        }

        [Test] public void LelaModesPulseAndWarMode()
        {
            New(new[]{"lela","talyssa"},new[]{"rei-bobett"});
            var lela=Get("lela"); var tal=Get("talyssa"); var king=Get("rei-bobett"); Put(lela,2,2); Put(tal,3,2); Put(king,9,9);
            tal.hp=5; Turn(lela);
            Assert.IsTrue(Skill(lela,"lela-pulse",lela.cell)); Assert.AreEqual(7,tal.hp,"Alegria ×2 na hora");
            Assert.IsTrue(Skill(lela,"modo-calma",lela.cell)); Assert.AreEqual("calma",lela.form);
            Assert.IsFalse(Match.Skills(lela).Any(s=>s.id=="modo-calma"));
            lela.pa=6; Assert.IsTrue(Skill(lela,"modo-guerra",lela.cell)); Assert.AreEqual("guerra",lela.form);
            Assert.IsFalse(Match.Skills(lela).Any(s=>s.id.StartsWith("modo-")||s.id=="lela-pulse"),"Guerra trava modos e pulso");
            Assert.AreEqual(2,m.Passive(tal,PassiveStat.Dano),"as 4 auras juntas");
            lela.pa=6; Assert.IsTrue(Skill(lela,"missil-lela",king.cell),"míssil global"); Assert.Less(king.hp,Content.Def(king).hp);
            lela.pa=6; Assert.IsFalse(Skill(lela,"missil-lela",king.cell),"1 por transformação");
        }

        [Test] public void GumbaldMarksJumpAndCataclisma()
        {
            New(new[]{"gumbald"},new[]{"rei-bobett"});
            var cat=Get("gumbald"); var king=Get("rei-bobett"); Put(cat,2,2); Put(king,3,2);
            for(int i=0;i<3;i++) { Turn(cat); Assert.IsTrue(Skill(cat,"garra-do-gato",king.cell)); }
            Assert.AreEqual(3,king.marks);
            var json=UnityEngine.JsonUtility.ToJson(S); Assert.AreEqual(3,UnityEngine.JsonUtility.FromJson<MatchState>(json).F(king.id).marks,"marca no estado");
            Put(cat,2,2); Put(king,2,9); Turn(cat);
            Assert.IsTrue(Skill(cat,"pulo-do-gato",king.cell),"4 + 3 marcas = 7"); Assert.AreEqual(0,cat.pm);
            Turn(cat); int hp=king.hp; Assert.IsTrue(Skill(cat,"cat-a-clisma",king.cell));
            Assert.AreEqual(0,king.marks); Assert.AreEqual(hp-(2-Content.Def(king).armor),king.hp,"3 marcas = +2");
        }

        [Test] public void DaemionLifestealBloodlustAndScent()
        {
            New(new[]{"daemion"},new[]{"rei-bobett"});
            var vamp=Get("daemion"); var king=Get("rei-bobett"); Put(vamp,2,2); Put(king,3,2);
            vamp.hp=5; king.hp=5; Turn(vamp);
            Assert.IsTrue(Skill(vamp,"mordida-vampirica",king.cell));
            Assert.AreEqual(8,vamp.hp,"cura 3"); Assert.AreEqual(Content.Def(vamp).pm+2,vamp.pm,"alvo ferido: +2 PM");
            Put(vamp,2,2); Put(king,2,10); Turn(vamp); vamp.pm=3;
            Assert.IsTrue(Move(vamp,new Cell(2,8)),"6 casas com 3 PM rumo ao ferido"); Assert.AreEqual(0,vamp.pm);
            Turn(vamp); vamp.pm=3; Assert.IsFalse(Move(vamp,new Cell(8,2)),"para longe, PM normal");
        }

        [Test] public void IkaitoRoomShamblesSwordAndInjection()
        {
            New(new[]{"ikaito","talyssa"},new[]{"rei-bobett"});
            var cap=Get("ikaito"); var tal=Get("talyssa"); var king=Get("rei-bobett"); Put(cap,2,2); Put(tal,10,10); Put(king,3,2);
            king.hp=6; Turn(cap);
            Assert.IsTrue(Skill(cap,"corte-negro",king.cell)); Assert.AreEqual(3,king.hp,"fura armadura");
            Assert.IsTrue(king.buffs.Any(b=>b.id==Match.NoHealBuff));
            Assert.IsTrue(Pass(cap)); Assert.IsFalse(king.buffs.Any(b=>b.id==Match.NoHealBuff),"acaba com o turno dele");
            Turn(cap); Put(king,8,2); Assert.IsFalse(Skill(cap,"shambles-ikaito",cap.cell,king.cell),"fora da Room: alcance 4");
            Put(king,4,2); Assert.IsFalse(Skill(cap,"shambles-ikaito",cap.cell,king.cell),"fora da Room: só o próprio time"); Put(king,8,2);
            Assert.IsTrue(Skill(cap,"room",cap.cell));
            for(int z=0;z<S.height;z++) S.At(new Cell(5,z)).terrain=Terrain.Parede;
            Assert.IsTrue(Skill(cap,"shambles-ikaito",cap.cell,king.cell),"na Room: 75% do mapa, por cima do muro e sem visão");
            Assert.AreEqual(new Cell(8,2),cap.cell);
            Turn(cap); Put(tal,9,2); cap.hp=5; tal.hp=5;
            Assert.IsTrue(Skill(cap,"injection-shot",tal.cell)); Assert.AreEqual(7,tal.hp); Assert.AreEqual(7,cap.hp);
        }

        // Partida inteira entre bots só com o elenco novo, num mapa sorteado: a cada jogada o estado do Host volta igual pelo
        // JSON (reconexão), a visão de cada jogador é válida e o pacote cabe no Relay.
        [TestCase(1u)] [TestCase(7u)] public void NewRosterSurvivesNetworkRoundTrips(uint seed)
        {
            m=Match.New(new MatchSetup{ seed=seed, team1=new[]{"pele-negra","lela","gumbald"}, team2=new[]{"daemion","ikaito","pele-negra"} });
            int budget=(int)typeof(ArenaTatica.ArenaTransport).GetField("RelayPayload",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).GetValue(null);
            for(int i=0;i<400&&!S.over;i++)
            {
                var f=S.Active; var cmd=Bot.Decide(m) ?? new Command{kind="passar"}; cmd.fighter=f.id; cmd.revision=S.revision;
                if(!m.Apply(cmd,f.team,out var events,out _)) { Assert.IsTrue(m.Apply(new Command{kind="passar",fighter=f.id,revision=S.revision},f.team,out events,out _)); }
                string json=UnityEngine.JsonUtility.ToJson(S);
                Assert.AreEqual(json,UnityEngine.JsonUtility.ToJson(UnityEngine.JsonUtility.FromJson<MatchState>(json)),"estado do Host igual depois do JSON");
                for(int viewer=1;viewer<=2;viewer++)
                {
                    var view=Match.ViewFor(S,viewer); Assert.IsTrue(ArenaTatica.ArenaGame.ValidState(view,viewer),$"visão P{viewer}, jogada {i}");
                    Assert.IsNotNull(Match.EventsFor(events,S,viewer));
                    var packet=UnityEngine.JsonUtility.ToJson(new ArenaTatica.WireMessage{kind="state",state=view});
                    Assert.LessOrEqual(System.Text.Encoding.UTF8.GetByteCount(packet),budget);
                }
            }
        }

        [Test] public void NewFightersSurviveNetworkViewAndBots()
        {
            foreach(var id in new[]{"pele-negra","lela","gumbald","daemion","ikaito"})
            {
                New(new[]{id},new[]{"ciantia"});
                var view=Match.ViewFor(S,1); Assert.IsTrue(ArenaTatica.ArenaGame.ValidState(view,1),id);
                for(int i=0;i<60&&!S.over;i++)
                {
                    var f=S.Active; var cmd=Bot.Decide(m) ?? new Command{kind="passar",fighter=f.id};
                    cmd.revision=S.revision; cmd.fighter=f.id;
                    if(!m.Apply(cmd,f.team,out _,out _)) Assert.IsTrue(Pass(f),id);
                }
            }
        }
    }
    // Pedido do autor de 21/10: correções (rede, Black Hole em neutros, PM escondido, Pulso), Ellie, Cria Vampírica, extras e eventos.
    public class Roster2110Tests
    {
        Match m;
        MatchState S => m.S;
        void New(string[] a,string[] b,bool solid=true)
        {
            m=Match.New(new MatchSetup{ flatMap=true, team1=a, team2=b, solidIllusions=solid });
            for(int i=0;i<200;i++) m.Forced.Enqueue(Face.Trava);
        }
        Fighter Get(string def,int team=0) => S.fighters.First(f=>f.def==def&&(team==0||f.team==team));
        void Put(Fighter f,int x,int z) => f.cell=new Cell(x,z);
        void Turn(Fighter f) { S.active=S.timeline.IndexOf(f.id); f.pa=Content.Def(f).pa; f.pm=Content.Def(f).pm; f.used.Clear(); }
        bool Skill(Fighter f,string id,Cell c,Cell c2=default) => m.Apply(new Command{kind="skill",skill=id,cell=c,cell2=c2,fighter=f.id,revision=S.revision},f.team,out _,out _);
        bool Move(Fighter f,Cell c) => m.Apply(new Command{kind="mover",cell=c,fighter=f.id,revision=S.revision},f.team,out _,out _);
        bool Pass(Fighter f) => m.Apply(new Command{kind="passar",fighter=f.id,revision=S.revision},f.team,out _,out _);

        [Test] public void PulseIraIsTwiceNotThrice()
        {
            New(new[]{"lela","talyssa"},new[]{"rei-bobett"});
            var lela=Get("lela"); var tal=Get("talyssa"); Put(lela,2,2); Put(tal,3,2); Put(Get("rei-bobett"),9,9);
            Turn(lela); Assert.IsTrue(Skill(lela,"modo-ira",lela.cell));
            Assert.AreEqual(2,m.Passive(tal,PassiveStat.Dano),"passiva sozinha +2");
            Assert.IsTrue(Skill(lela,"lela-pulse",lela.cell));
            Assert.AreEqual(4,m.Passive(tal,PassiveStat.Dano)+tal.buffs.Sum(b=>b.dmg),"Pulso ×2 = +4, sem somar a passiva");
        }

        [Test] public void HiddenKeepsPmBonuses()
        {
            New(new[]{"perry"},new[]{"rei-bobett"});
            var perry=Get("perry"); Put(perry,0,1); Put(Get("rei-bobett"),9,9); S.At(new Cell(1,1)).terrain=Terrain.Caixa;
            perry.bonusPm=2; Turn(perry); perry.pm=6;
            Assert.IsTrue(Skill(perry,"esconder",perry.cell)); Assert.AreEqual(4,perry.pm,"2 + 2 das botas");
            Assert.AreEqual(4,m.HiddenCap(perry));
        }

        [Test] public void BanishedNeutralDoesNotActAndReturns()
        {
            New(new[]{"pele-negra"},new[]{"rei-bobett"});
            var pele=Get("pele-negra"); Put(pele,5,5); Put(Get("rei-bobett"),11,11);
            m.Trigger("vampiro"); var boss=Get("chefe-cria-vampirica"); Put(boss,5,7); var at=boss.cell;
            Turn(pele); Assert.IsTrue(Skill(pele,"black-hole",boss.cell)); Assert.IsTrue(boss.banished);
            int hp=pele.hp;
            for(int i=0;i<8&&boss.prisoned>0;i++) Assert.IsTrue(Pass(S.Active));
            Assert.AreEqual(at,boss.cell,"não andou no buraco negro"); Assert.AreEqual(hp,pele.hp,"não bateu");
            Assert.IsFalse(boss.banished); Assert.AreEqual(Content.Def(boss).hp-3,boss.hp,"dano da volta");
        }

        [Test] public void VampireBossSplitsAndEventEndsWithReplicas()
        {
            New(new[]{"talyssa"},new[]{"rei-bobett"});
            var tal=Get("talyssa"); Put(tal,5,5); Put(Get("rei-bobett"),11,11);
            m.Trigger("vampiro"); Assert.AreEqual("vampiro",S.ev); var boss=Get("chefe-cria-vampirica"); Put(boss,6,5); boss.hp=1;
            Turn(tal); Assert.IsTrue(Skill(tal,"lamina-rosa",boss.cell)); Assert.IsFalse(boss.Alive);
            var reps=S.fighters.Where(f=>f.Alive&&f.def=="replica-vampirica").ToList(); Assert.AreEqual(3,reps.Count); Assert.AreEqual("vampiro",S.ev,"réplicas mantêm o evento");
            foreach(var r in reps.Skip(1)) { r.hp=0; r.deathProcessed=true; } Put(reps[0],5,6); reps[0].hp=1;
            tal.pa=6; tal.used.Clear(); Assert.IsTrue(Skill(tal,"lamina-rosa",reps[0].cell)); Assert.AreEqual("",S.ev,"sem chefe nem réplicas: acabou");
        }

        [Test] public void EllieShadowsTeleportAndBonuses()
        {
            New(new[]{"ellie"},new[]{"talyssa"});
            var ellie=Get("ellie"); var tal=Get("talyssa"); Put(ellie,1,1); Put(tal,8,8);
            S.active=S.timeline.IndexOf(tal.id); Assert.IsTrue(Pass(tal)); if(S.Active!=ellie) Assert.IsTrue(Pass(S.Active));
            Assert.AreEqual(ellie,S.Active);
            int n=S.tiles.Count(t=>t.shadow==ellie.id); Assert.That(n,Is.InRange(4,10));
            var shadow=Enumerable.Range(0,S.tiles.Length).Select(i=>new Cell(i%S.width,i/S.width)).First(c=>S.At(c).shadow==ellie.id&&Board.At(S,c)==null);
            int pm=ellie.pm; Assert.IsTrue(Skill(ellie,"passo-das-sombras",shadow)); Assert.AreEqual(shadow,ellie.cell); Assert.AreEqual(pm+5,ellie.pm);
            Turn(ellie); var far=new Cell(0,11); S.At(far).shadow=0; Assert.IsFalse(Skill(ellie,"passo-das-sombras",far),"nem sombra nem inimigo colado");
            Assert.IsTrue(Skill(ellie,"passo-das-sombras",new Cell(8,7)),"emerge colada no inimigo");
            foreach(var t in S.tiles) t.shadow=0; S.At(tal.cell).shadow=ellie.id;
            Assert.IsTrue(Skill(ellie,"kunai-corrente",tal.cell)); Assert.AreEqual(Content.Def(tal).hp-4,tal.hp,"1 + 3 da sombra");
        }

        [Test] public void StunSkipsTurnButFuryIsUnstoppable()
        {
            New(new[]{"pistoleira"},new[]{"talyssa","death-crimson"});
            var gun=Get("pistoleira"); var tal=Get("talyssa"); var dc=Get("death-crimson"); Put(gun,2,2); Put(tal,4,2); Put(dc,2,4);
            Turn(gun); Assert.IsTrue(Skill(gun,"agarrao-espectral",tal.cell)); Assert.AreEqual(1,tal.stunned);
            var json=UnityEngine.JsonUtility.ToJson(tal); Assert.AreEqual(1,UnityEngine.JsonUtility.FromJson<Fighter>(json).stunned,"atordoado viaja na rede");
            dc.form="furia"; gun.cooldowns.Clear(); gun.pa=6; Assert.IsTrue(Skill(gun,"agarrao-espectral",dc.cell)); Assert.AreEqual(0,dc.stunned,"Fúria não é atordoada");
            for(int i=0;i<6&&S.Active!=tal&&tal.stunned>0;i++) Assert.IsTrue(Pass(S.Active));
            Assert.AreEqual(0,tal.stunned); Assert.AreNotEqual(tal,S.Active,"perdeu o turno");
        }

        [Test] public void MirrorPurgeCleanseSealReflectReveal()
        {
            New(new[]{"espelhos","talyssa"},new[]{"tchu-tcha","perry"});
            var mir=Get("espelhos"); var tal=Get("talyssa"); var tchu=Get("tchu-tcha"); var perry=Get("perry");
            Put(mir,2,2); Put(tal,3,2); Put(tchu,2,4); Put(perry,10,10);
            tchu.buffs.Add(new Buff{id="b",turns=2,armor=2}); tal.buffs.Add(new Buff{id="confusao",turns=2,pm=-2}); tal.marks=3;
            Turn(mir); Assert.IsTrue(Skill(mir,"espelho-purificador",tchu.cell)); Assert.IsFalse(tchu.buffs.Any(b=>b.id=="b"),"tira buff do inimigo");
            mir.cooldowns.Clear(); Assert.IsTrue(Skill(mir,"espelho-purificador",tal.cell)); Assert.IsFalse(tal.buffs.Any(b=>b.pm<0)); Assert.AreEqual(0,tal.marks,"limpa o aliado");
            tchu.form="berserk"; Turn(mir); Assert.IsTrue(Skill(mir,"selar-ultimate",tchu.cell)); Assert.AreEqual("normal",tchu.form);
            Assert.IsFalse(m.CanTarget(tchu,Content.AnySkill("olho-abissal"),tchu.cell,out _),"ultimate selada");
            Turn(mir); Assert.IsTrue(Skill(mir,"espelho-de-retorno",mir.cell));
            Turn(tchu); Put(tchu,2,3); int before=tchu.hp; Assert.IsTrue(Skill(tchu,"escudada",mir.cell));
            Assert.Less(tchu.hp,before,"o golpe voltou"); Assert.IsFalse(mir.buffs.Any(b=>b.reflect>0));
            perry.hidden=true; Turn(mir); Assert.IsTrue(Skill(mir,"olhar-do-espelho",new Cell(9,9))); Assert.IsFalse(perry.hidden,"revelado de longe");
        }

        [Test] public void PilisPortalImmunityAndDoubleDamage()
        {
            New(new[]{"pilis","talyssa"},new[]{"rei-bobett","domador-de-correntes"});
            var pil=Get("pilis"); var tal=Get("talyssa"); var king=Get("rei-bobett"); var dom=Get("domador-de-correntes");
            Put(pil,2,2); Put(tal,3,2); Put(king,8,8); Put(dom,10,10);
            Turn(pil); Assert.IsTrue(Skill(pil,"portal-do-mordomo",new Cell(2,4),new Cell(6,2)));
            Put(king,2,5); Turn(king); Assert.IsTrue(Move(king,new Cell(2,4))); Assert.AreEqual(new Cell(2,4),king.cell,"inimigo não atravessa");
            Put(king,8,8); Put(tal,2,3); Turn(tal); Assert.IsTrue(Move(tal,new Cell(2,4))); Assert.AreEqual(new Cell(5,2),tal.cell,"aliado sai do outro lado");
            Put(tal,3,2); Turn(pil); Assert.IsTrue(Skill(pil,"cha-protetor",pil.cell)); Assert.IsTrue(tal.buffs.Any(b=>b.immune>0));
            Put(dom,3,5); Turn(dom); Assert.IsTrue(Skill(dom,"corrente",tal.cell)); Assert.AreEqual(new Cell(3,2),tal.cell,"imune ao puxão");
            Put(tal,3,5); Put(king,2,5); Turn(pil); Assert.IsTrue(Skill(pil,"explosao-do-cha",king.cell)); Assert.IsTrue(king.buffs.Any(b=>b.taken==2));
            Turn(tal); int hp=king.hp; Assert.IsTrue(Skill(tal,"lamina-rosa",king.cell)); Assert.AreEqual(hp-(3-1)*2,king.hp,"(3 − 1 armadura) × 2");
        }

        [Test] public void GooseSwordsmanAndCrimson()
        {
            New(new[]{"ganso","talyssa"},new[]{"espadachim","death-crimson"});
            var goose=Get("ganso"); var tal=Get("talyssa"); var sw=Get("espadachim"); var dc=Get("death-crimson");
            Put(goose,1,1); Put(tal,1,2); Put(sw,10,10); Put(dc,11,11);
            Assert.AreEqual(2,m.Passive(tal,PassiveStat.PM)); Assert.AreEqual(2,m.Passive(goose,PassiveStat.PM)); Assert.AreEqual(0,m.Passive(sw,PassiveStat.PM));
            // Raio para no 1º lutador da linha.
            Put(sw,5,1); Put(tal,5,4); Put(goose,5,6); Turn(sw);
            Assert.IsTrue(Skill(sw,"corte-cosmico",new Cell(5,6))); Assert.AreEqual(Content.Def(tal).hp-2,tal.hp,"parou na Talyssa"); Assert.AreEqual(Content.Def(goose).hp,goose.hp);
            // Duelo: ninguém mais perto → +2.
            Put(tal,5,2); Put(goose,0,11); Turn(sw); int hp=tal.hp;
            Assert.IsTrue(Skill(sw,"laminas-gemeas",tal.cell)); Assert.AreEqual(hp-5,tal.hp,"3 + 2 de duelo");
            // Death Crimson: Fúria (2 golpes), cura de golpe cheio, explosão no fim do turno (nele também) e troca de vida.
            Put(dc,8,8); Put(goose,8,9); Put(sw,0,11); Put(tal,11,0); dc.hp=6; Turn(dc);
            Assert.IsTrue(Skill(dc,"furia-crimson",dc.cell)); Assert.AreEqual(Content.Def(dc).pm+2,dc.pm); dc.pa=6;
            Assert.IsTrue(Skill(dc,"garras-crimson",goose.cell)); Assert.IsTrue(Skill(dc,"garras-crimson",goose.cell),"2× na Fúria");
            Assert.AreEqual(14,dc.hp,"cura 4 por golpe cheio"); Assert.AreEqual(Content.Def(goose).hp-4,goose.hp,"(3 − 1) × 2");
            int gh=goose.hp; Assert.IsTrue(Pass(dc));
            Assert.AreEqual(gh-3,goose.hp,"explosão em volta"); Assert.AreEqual(11,dc.hp,"nele também");
            dc.form="normal"; Turn(dc); dc.hp=3; goose.hp=15; Assert.IsTrue(Skill(dc,"troca-de-vida",goose.cell));
            Assert.AreEqual(14,dc.hp,"até o PV máximo dele"); Assert.AreEqual(3,goose.hp);
        }

        [Test] public void InvisibleBombsSplitByQuadrantAndHidden()
        {
            New(new[]{"talyssa"},new[]{"rei-bobett"});
            var ev=m.Trigger("bombas-invisiveis").First(e=>e.kind=="cataclisma");
            Assert.IsTrue(ev.path==null||ev.path.Length==0,"o evento não entrega as casas");
            int half=S.width/2; var counts=new int[4];
            foreach(int i in S.mines) { int x=i%S.width, z=i/S.width; counts[(x>=half?1:0)+(z>=half?2:0)]++; Assert.AreEqual(MatchState.Neutral,S.tiles[i].trap); }
            Assert.That(counts.Distinct().Count(),Is.EqualTo(1),"mesmo número por quadrante"); Assert.Greater(counts[0],0);
            for(int p=1;p<=2;p++) Assert.IsFalse(Match.ViewFor(S,p).tiles.Any(t=>t.trap!=0),"ninguém vê");
        }

        [Test] public void EdgarShipStaysOncePerMatchAfterEcho()
        {
            New(new[]{"edgar"},new[]{"rei-bobett"});
            var ed=Get("edgar"); Put(ed,2,2); Put(Get("rei-bobett"),9,9);
            Turn(ed); Assert.IsTrue(Skill(ed,"navio-fantasma",new Cell(2,3)));
            m.Trigger("eco-ancestral"); Assert.IsTrue(ed.usedMatch.Contains("navio-fantasma"),"Eco ancestral não devolve o barco");
        }

        [Test] public void ShamblesSwapsPropsOutsideRoom()
        {
            New(new[]{"ikaito","talyssa"},new[]{"rei-bobett"});
            var cap=Get("ikaito"); var tal=Get("talyssa"); var king=Get("rei-bobett"); Put(cap,2,2); Put(tal,4,2); Put(king,10,10);
            S.At(new Cell(2,4)).terrain=Terrain.Caixa;
            Turn(cap); Assert.IsTrue(Skill(cap,"shambles-ikaito",tal.cell,new Cell(2,4)));
            Assert.AreEqual(new Cell(2,4),tal.cell); Assert.AreEqual(Terrain.Caixa,S.At(new Cell(4,2)).terrain); Assert.AreEqual(Terrain.Livre,S.At(new Cell(2,4)).terrain);
        }

        [Test] public void IllusionCanBeWalkedThroughAndNeverReveals()
        {
            New(new[]{"ezequiela"},new[]{"perry"},solid:false);
            var real=S.fighters.First(f=>f.def=="ezequiela"); var copy=S.fighters.First(f=>f.def=="ilusao"); var perry=Get("perry");
            Put(real,2,2); Put(copy,3,2); Put(perry,9,9);
            Turn(real); Assert.IsTrue(Move(real,new Cell(4,2)),"atravessa a ilusão"); Assert.AreEqual(new Cell(4,2),real.cell);
            Assert.IsFalse(Move(real,new Cell(3,2)),"não para em cima");
            perry.hidden=true; Put(perry,6,3); S.active=S.timeline.IndexOf(copy.id); copy.pm=3; Assert.IsTrue(Move(copy,new Cell(5,3)));
            Assert.IsTrue(perry.hidden,"ilusão não revela");
        }

        [Test] public void BigPacketsAreCompressedAndIllusionViewIsValid()
        {
            var team=new[]{"ezequiela","lela","death-crimson","espelhos","pilis"};
            m=Match.New(new MatchSetup{ seed=3, players=Enumerable.Range(0,6).Select(_=>new PlayerSetup{fighters=team}).ToArray() });
            foreach(var f in S.fighters.Where(f=>f.def=="ezequiela")) { f.bonusHp=3; f.hp=Content.MaxHp(f); }
            int budget=(int)typeof(ArenaTatica.ArenaTransport).GetField("RelayPayload",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).GetValue(null);
            for(int p=1;p<=6;p++)
            {
                var view=Match.ViewFor(S,p); Assert.IsTrue(ArenaTatica.ArenaGame.ValidState(view,p),"ilusão com PV de item");
                var json=ArenaTatica.ArenaGame.Wire(new ArenaTatica.WireMessage{kind="snapshot",state=view});
                var wire=ArenaTatica.ArenaGame.Zip(json); Assert.Less(wire.Length,budget/2); Assert.AreEqual(json,ArenaTatica.ArenaGame.Unzip(wire));
            }
            Assert.IsNull(ArenaTatica.ArenaGame.Unzip("Znão-é-base64"));
        }

        [Test] public void TeaExplosionKillsButlerRightAway()
        {
            New(new[]{"pilis"},new[]{"rei-bobett"});
            var pilis=Get("pilis"); var rei=Get("rei-bobett"); Put(pilis,2,2); Put(rei,2,5);
            Turn(pilis); Assert.IsTrue(Skill(pilis,"explosao-do-cha",rei.cell));
            Assert.IsFalse(pilis.Alive,"o mordomo cai junto"); Assert.IsTrue(pilis.deathProcessed);
            Assert.IsTrue(rei.buffs.Exists(b=>b.taken==2),"o alvo fica vulnerável mesmo assim");
        }

        [Test] public void EllieTakesFullRangedDamageAndControl()
        {
            New(new[]{"pistoleira"},new[]{"ellie","talyssa"});
            var pist=Get("pistoleira"); var ellie=Get("ellie"); var tal=Get("talyssa"); Put(pist,2,2); Put(ellie,2,5); Put(tal,5,2);
            // Escudo no dado de defesa: bloqueia 1 na Talyssa, não na Ellie.
            m.Forced.Clear(); m.Forced.Enqueue(Face.Trava); m.Forced.Enqueue(Face.Escudo); for(int i=0;i<50;i++) m.Forced.Enqueue(Face.Trava);
            Turn(pist); int hp=ellie.hp; Assert.IsTrue(Skill(pist,"bala-de-impacto",ellie.cell)); Assert.AreEqual(hp-1,ellie.hp,"de longe não bloqueia");
            m.Forced.Clear(); m.Forced.Enqueue(Face.Trava); m.Forced.Enqueue(Face.Escudo); for(int i=0;i<50;i++) m.Forced.Enqueue(Face.Trava);
            pist.pa=6; pist.used.Clear(); hp=tal.hp; Assert.IsTrue(Skill(pist,"bala-de-impacto",tal.cell)); Assert.AreEqual(hp,tal.hp,"os outros bloqueiam");
            // Imunidade (Chá protetor) não segura o atordoamento nela.
            Put(ellie,2,5); ellie.buffs.Add(new Buff{id="cha-protetor",turns=2,immune=1});
            pist.pa=6; pist.used.Clear(); Assert.IsTrue(Skill(pist,"agarrao-espectral",ellie.cell)); Assert.AreEqual(1,ellie.stunned);
        }
    }
}
