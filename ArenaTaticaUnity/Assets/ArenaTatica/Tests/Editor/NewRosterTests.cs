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
            Turn(cap); Assert.IsFalse(Match.Skills(cap).Any(s=>s.id=="shambles-ikaito"),"Shambles só na Room");
            Assert.IsTrue(Skill(cap,"room",cap.cell)); Put(king,8,2);
            Assert.IsTrue(Skill(cap,"shambles-ikaito",cap.cell,king.cell),"metade do mapa (6)");
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
}
