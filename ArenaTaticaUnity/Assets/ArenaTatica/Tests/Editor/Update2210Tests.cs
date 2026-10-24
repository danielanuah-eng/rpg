using System.Linq;
using NUnit.Framework;

namespace ArenaTatica.Core.Tests
{
    // Pacote de 22/10: nomes novos, Pistoleira mata invocações, zumbi do Damon pelo número de jogadores e Blink Dagger na loja.
    public class Update2210Tests
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
        void Turn(Fighter f) { S.active=S.timeline.IndexOf(f.id); f.pa=Content.Def(f).pa; f.pm=Content.Def(f).pm; f.used.Clear(); }
        bool Skill(Fighter f,string id,Cell c) => m.Apply(new Command{kind="skill",skill=id,cell=c,fighter=f.id,revision=S.revision},f.team,out _,out _);
        bool Pass(Fighter f) => m.Apply(new Command{kind="passar",fighter=f.id,revision=S.revision},f.team,out _,out _);
        // Compra com uma casa de loja vizinha só durante o comando.
        bool Buy(Fighter f,int slot=0)
        {
            var n=Board.Dirs.Select(d=>f.cell+d).First(x=>Board.Contains(S,x)&&Board.At(S,x)==null); var t=S.At(n); var old=t.terrain; t.terrain=Terrain.Loja;
            bool ok=m.Apply(new Command{kind="comprar",slot=slot,fighter=f.id,revision=S.revision},f.team,out _,out _); t.terrain=old; return ok;
        }

        [Test] public void DisplayNamesChangeButIdsStay()
        {
            Assert.AreEqual("Damon",Content.Fighters["daemion"].name);
            Assert.AreEqual("Mirhor",Content.Fighters["espelhos"].name);
            Assert.AreEqual("PatoMacho",Content.Fighters["ganso"].name);
        }

        [Test] public void PistoleiraKillsEnemySummonsOnAnyDamage()
        {
            New(new[]{"daemion"},new[]{"pistoleira"});
            var dae=Get("daemion"); var pis=Get("pistoleira"); Put(dae,2,2); Put(pis,5,2);
            Turn(dae); Assert.IsTrue(Skill(dae,"transformar-em-vampiro",new Cell(3,2)));
            var cria=Get("cria-vampirica"); cria.bonusHp=10; cria.hp=Content.MaxHp(cria);
            Turn(pis); Assert.IsTrue(Skill(pis,"bala-de-impacto",cria.cell));
            Assert.IsFalse(cria.Alive,"1 de dano numa invocação mata na hora");
            int hp=dae.hp; pis.pa=6; pis.used.Clear();
            Assert.IsTrue(Skill(pis,"bala-de-impacto",dae.cell));
            Assert.AreEqual(hp-1,dae.hp,"herói leva o dano normal");
        }

        [TestCase(2,2)] [TestCase(3,4)] [TestCase(4,6)] [TestCase(5,6)] [TestCase(6,6)]
        public void DamonZombieHpFollowsPlayerCount(int players,int hp)
        {
            m=Match.New(new MatchSetup{ flatMap=true, players=Enumerable.Range(0,players).Select(i=>new PlayerSetup{fighters=new[]{i==0?"daemion":"rei-bobett"}}).ToArray() });
            Assert.AreEqual(players,S.players);
            var dae=Get("daemion"); Turn(dae);
            var spot=Board.Dirs.Select(d=>dae.cell+d).First(c=>Board.Contains(S,c)&&Board.At(S,c)==null&&Board.Walkable(S,c,false));
            Assert.IsTrue(Skill(dae,"transformar-em-vampiro",spot));
            var cria=Get("cria-vampirica");
            Assert.AreEqual(hp,Content.MaxHp(cria)); Assert.AreEqual(hp,cria.hp);
        }

        [Test] public void BlinkDaggerIsSoldAndShowsInTheShop()
        {
            var d=Shop.Def("blink-dagger"); Assert.IsNotNull(d); Assert.AreEqual(6,d.price);
            Assert.AreEqual(SkillKind.Teleporte,d.skill.kind); Assert.AreEqual(2,d.skill.pa); Assert.AreEqual(6,d.skill.maxRange); Assert.AreEqual(1,d.skill.cooldown);
            m=Match.New(new MatchSetup{ flatMap=true, shopItems=new[]{"blink-dagger"} });
            for(int p=1;p<=2;p++) Assert.Contains("blink-dagger",S.shops[p].slots);
        }

        [Test] public void BlinkDaggerTeleportsSixWithCooldownAndStays()
        {
            New(new[]{"ganso"},new[]{"rei-bobett"});
            var me=Get("ganso"); Put(me,0,0); Put(Get("rei-bobett"),11,11);
            Turn(me); S.shops[me.team].slots[0]="blink-dagger"; S.coins[me.team]=6;
            Assert.IsTrue(Buy(me)); Assert.AreEqual(0,S.coins[me.team]);
            Assert.IsFalse(Skill(me,"blink-dagger",new Cell(7,0)),"7 casas é longe demais");
            Assert.IsTrue(Skill(me,"blink-dagger",new Cell(6,0))); Assert.AreEqual(new Cell(6,0),me.cell); Assert.AreEqual(4,me.pa,"2 PA");
            Assert.IsFalse(Skill(me,"blink-dagger",new Cell(6,3)),"recarga");
            for(int i=0;i<6&&S.Active!=me;i++) Pass(S.Active);
            if(S.Active==me) Pass(me);
            for(int i=0;i<6&&S.Active!=me;i++) Pass(S.Active);
            Assert.AreEqual(me,S.Active);
            Assert.IsTrue(Match.Skills(me).Any(k=>k.id=="blink-dagger"),"fica com quem comprou");
            Assert.IsTrue(Skill(me,"blink-dagger",new Cell(6,4)),"volta depois da recarga");
            Assert.IsTrue(me.charges.Any(c=>c.skill=="blink-dagger"),"não gasta carga");
        }

        [Test] public void BotBlinksNextToAFarFoe()
        {
            New(new[]{"ganso"},new[]{"rei-bobett"});
            var me=Get("ganso"); var foe=Get("rei-bobett"); Put(me,0,0); Put(foe,7,0);
            Turn(me); me.charges.Add(new Charge{skill="blink-dagger",uses=ShopDef.Permanent});
            var cmd=Bot.Decide(m);
            Assert.AreEqual("skill",cmd.kind); Assert.AreEqual("blink-dagger",cmd.skill);
            Assert.AreEqual(1,Board.Gap(foe,cmd.cell),"cola no inimigo");
        }
    }
}
