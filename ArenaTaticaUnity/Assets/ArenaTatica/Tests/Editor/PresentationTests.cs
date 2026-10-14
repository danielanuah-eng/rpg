using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using ArenaTatica.Core;

namespace ArenaTatica.Tests
{
    // Entrega 017: importação das tiras, fases de efeito, visibilidade e seleção de equipe (sem cena).
    public class PresentationTests
    {
        static float Pivot(Sprite s) => s.pivot.y/s.rect.height;

        [TestCase("esconder","bomba-de-fumaca","bomba-de-fumaca/uso")]
        [TestCase("armadilha-posta","mina-de-gelo","mina-de-gelo/uso")]
        [TestCase("invocar","erguer-totem","totem-de-cura/uso")]
        [TestCase("cura","totem-de-cura","totem-de-cura/uso")]
        [TestCase("compra","cristal-de-foco","cristal-de-foco/uso")]
        [TestCase("compra","ancora","ancora/uso")]
        [TestCase("compra","manto-de-espinhos","manto-de-espinhos/uso")]
        [TestCase("compra","bencao-de-pedra","bencao-de-pedra/uso")]
        public void RecentEventUsesItsAvailableSpritePhase(string kind,string id,string phase)
        {
            Assert.AreEqual(phase,ArenaEfeitos.UsePhase(kind,id));
            string resource="Sprites/Efeitos/"+phase;
            Assert.IsNotNull(ArenaEfeitos.Entrada(resource));
            Assert.AreEqual(ArenaEfeitos.Entrada(resource).frames,ArenaEfeitos.Frames(resource).Length);
        }
        [TestCase("aquecer-motor","conjuracao")]
        [TestCase("aquecer-motor","impacto")]
        [TestCase("chao-de-gelo","conjuracao")]
        public void RecentlyUncalledPhasesLoadWithTheirDeclaredTiming(string id,string phase)
        {
            string resource="Sprites/Efeitos/"+ArenaEfeitos.SkillPhase(id,phase);
            var entry=ArenaEfeitos.Entrada(resource); Assert.IsNotNull(entry);
            Assert.AreEqual(entry.frames,ArenaEfeitos.Frames(resource).Length);
            Assert.AreEqual(entry.impactFrame0/entry.fps,ArenaEfeitos.ImpactTime(resource),.0001f);
        }
        [TestCase("bola-de-fogo","impacto")]
        [TestCase("bomba-de-fumaca","uso")]
        public void CopiedAndRandomSkillsKeepTheSameSpritesAndTiming(string id,string phase)
        {
            string original="Sprites/Efeitos/"+id+"/"+phase;
            string variant="Sprites/Efeitos/roubada:sorteio:"+id+"/"+phase;
            Assert.AreSame(ArenaEfeitos.Entrada(original),ArenaEfeitos.Entrada(variant));
            var method=typeof(ArenaPresentation).GetMethod("EffectFrames",BindingFlags.NonPublic|BindingFlags.Static);
            CollectionAssert.AreEqual(ArenaEfeitos.Frames(original),(Sprite[])method.Invoke(null,new object[]{"roubada:sorteio:"+id+"/"+phase}));
        }
        [Test] public void BuyingAMagicOrPetDoesNotPlayItsCastOrSummonEarly()
        {
            Assert.IsNull(ArenaEfeitos.UsePhase("compra","bomba-de-fumaca"));
            Assert.IsNull(ArenaEfeitos.UsePhase("compra","totem-de-cura"));
            Assert.IsNull(ArenaEfeitos.UsePhase("cura","pauzinho"),"Dreno não repete a arte do golpe");
        }

        [Test] public void EveryManifestResourceLoadsWithItsDeclaredFrames()
        {
            var entries=ArenaEfeitos.Entradas.ToList();
            Assert.IsNotEmpty(entries,"Resources/Efeitos/manifesto.json vazio ou ausente");
            var bad=entries.Where(e=>(ArenaEfeitos.Frames(e.resource)?.Length??0)!=e.frames).Select(e=>e.resource).ToList();
            Assert.IsEmpty(bad,"Sem os quadros declarados (rode Arena Tática > Importar entrega visual): "+string.Join(", ",bad.Take(10)));
        }

        [Test] public void LoadingOnePhaseNeverBringsFramesOfAnother()
        {
            var inicio=ArenaEfeitos.Frames("Sprites/Efeitos/mare-de-lava/inicio"); var loop=ArenaEfeitos.Frames("Sprites/Efeitos/mare-de-lava/loop");
            Assert.IsNotNull(inicio); Assert.IsNotNull(loop);
            Assert.IsTrue(inicio.All(s=>s.name.StartsWith("inicio_")));
            Assert.IsTrue(loop.All(s=>s.name.StartsWith("loop_")));
            Assert.AreEqual(ArenaEfeitos.Entrada("Sprites/Efeitos/mare-de-lava/inicio").frames,inicio.Length);
            Assert.IsEmpty(inicio.Intersect(loop));
            // Uma pasta nunca é uma fase: carregar o id sozinho não concatena conjuração, impacto e uso.
            Assert.IsNull(ArenaEfeitos.Frames("Sprites/Efeitos/mare-de-lava"));
            Assert.IsNull(ArenaEfeitos.Frames("Sprites/Efeitos/gancho"));
        }

        [TestCase("Sprites/Baltazar/idle_west",4,168)]
        [TestCase("Sprites/Baltazar/walk_west",6,176)]
        [TestCase("Sprites/GuardiaoMusgo/idle_north-west",4,176)]
        public void RepairedStripsSliceIntoTheRightCells(string resource,int frames,int cell)
        {
            var sprites=ArenaEfeitos.Frames(resource); var entry=ArenaEfeitos.Entrada(resource);
            Assert.IsNotNull(sprites,resource); Assert.AreEqual(frames,sprites.Length);
            for(int i=0;i<sprites.Length;i++) {
                Assert.AreEqual(cell,sprites[i].rect.width,resource+" largura"); Assert.AreEqual(cell,sprites[i].rect.height,resource+" altura");
                Assert.AreEqual(i*cell,sprites[i].rect.x,resource+" x do quadro "+i);
                Assert.AreEqual(entry.unityPivot[1],Pivot(sprites[i]),.002f,resource+" pivô");
                Assert.AreEqual(128f,sprites[i].pixelsPerUnit);
            }
        }

        [Test] public void MenuKeepsTheTabsPlusRoom()
        {
            var tabs=(System.Array)typeof(ArenaMenu).GetField("Tabs",BindingFlags.NonPublic|BindingFlags.Static).GetValue(null);
            Assert.AreEqual(10,tabs.Length,"9 abas + Sala (13/10)");
        }

        [Test] public void EmptyTeamDisablesAdvanceAndCostFollowsTheTeam()
        {
            Assert.IsFalse(ArenaTema.CanAdvance(0)); Assert.IsTrue(ArenaTema.CanAdvance(1));
            Assert.AreEqual("Escolha um lutador",ArenaTema.AdvanceLabel(0,"Começar")); Assert.AreEqual("Começar",ArenaTema.AdvanceLabel(2,"Começar"));
            var one=ArenaGame.ValidTeam(new[]{"ciantia"}); var two=ArenaGame.ValidTeam(new[]{"ciantia",Content.Roster.First(id=>id!="ciantia"&&Content.Fighters[id].level+Content.Fighters["ciantia"].level<=ArenaGame.TeamLevel&&!(Content.Fighters[id].legendary&&Content.Fighters["ciantia"].legendary))});
            int Cost(string[] t) => t.Sum(id=>Content.Fighters[id].level);
            Assert.Greater(Cost(two),Cost(one)); Assert.LessOrEqual(Cost(two),ArenaGame.TeamLevel);
        }

        [Test] public void OwnerOnlyEffectIsHiddenFromTheEnemy()
        {
            Assert.IsTrue(ArenaEfeitos.Entrada("Sprites/Objetos/mina-de-gelo-armada/idle").ownerOnly);
            Assert.IsFalse(ArenaEfeitos.Visible("Sprites/Objetos/mina-de-gelo-armada/idle",false,false,false));
            Assert.IsTrue(ArenaEfeitos.Visible("Sprites/Objetos/mina-de-gelo-armada/idle",true,false,true));
            Assert.IsFalse(ArenaEfeitos.Visible(true,false,false,false,false));
        }

        [Test] public void StateEffectFollowsTargetVisibility()
        {
            Assert.IsTrue(ArenaEfeitos.Entrada("Sprites/Efeitos/Estados/sem-pm-oleo/loop").inheritTargetVisibility);
            // Alvo escondido: o inimigo não vê a sobreposição (não revela a unidade); o aliado vê.
            Assert.IsFalse(ArenaEfeitos.Visible("Sprites/Efeitos/Estados/sem-pm-oleo/loop",true,true,false));
            Assert.IsTrue(ArenaEfeitos.Visible("Sprites/Efeitos/Estados/sem-pm-oleo/loop",true,true,true));
            Assert.IsTrue(ArenaEfeitos.Visible("Sprites/Efeitos/Estados/armadura/loop",true,false,false));
            Assert.IsFalse(ArenaEfeitos.Visible("Sprites/Efeitos/Estados/armadura/loop",true,true,false));
        }

        [TestCase("lava")] [TestCase("gelo")] [TestCase("fogos")]
        public void TerrainLoopsHaveMoreThanOneFrame(string id)
        {
            var frames=ArenaEfeitos.Frames("Sprites/Efeitos/"+id+"/loop");
            Assert.IsNotNull(frames,id); Assert.Greater(frames.Length,1,id);
        }

        [Test] public void DamageWaitsForTheCollisionFrameNotThePeak()
        {
            // Martelo perfurante: colide no quadro 6 a 12 FPS; peakFrame0 (1) não decide o instante.
            Assert.AreEqual(.5f,ArenaEfeitos.ImpactTime("Sprites/Efeitos/martelo-perfurante/uso"),1e-4f);
            Assert.AreEqual(0,ArenaEfeitos.ImpactTime("Sprites/Efeitos/nao-existe/impacto"));
        }

        [Test] public void ContinuousStatesFollowTheFighterAndEndWithIt()
        {
            var f=new Fighter{hp=5,prisoned=1,hidden=true,channel=1};
            CollectionAssert.AreEquivalent(new[]{"Estados/escondido/loop","Estados/preso/loop","Estados/desejo-canalizando/loop"},ArenaEfeitos.StateLoops(f));
            f=new Fighter{hp=5}; f.buffs.Add(new Buff{id="bencao",turns=1,armor=1,pa=1});
            CollectionAssert.AreEquivalent(new[]{"Estados/armadura/loop","Estados/buff-pa-pm/loop"},ArenaEfeitos.StateLoops(f));
            // Noite dos Gatos usa a aura própria em vez do genérico de armadura.
            f.buffs.Clear(); f.buffs.Add(new Buff{id="noite-dos-gatos",turns=1,armor=1});
            CollectionAssert.AreEqual(new[]{"noite-dos-gatos/aura"},ArenaEfeitos.StateLoops(f));
            f.buffs.Clear(); f.buffs.Add(new Buff{id="confusao",from="tentaculo-de-oleo",turns=2,pm=-2}); f.buffs.Add(new Buff{id="confusao",from="ferrao",turns=2,pm=-1});
            CollectionAssert.AreEquivalent(new[]{"Estados/sem-pm-oleo/loop","Estados/sem-pm/loop"},ArenaEfeitos.StateLoops(f));
            f.hp=0; Assert.IsEmpty(ArenaEfeitos.StateLoops(f));
            foreach(var id in new[]{"Estados/escondido/loop","Estados/preso/loop","Estados/sem-pm-oleo/loop","noite-dos-gatos/aura"}) Assert.IsNotNull(ArenaEfeitos.Frames("Sprites/Efeitos/"+id),id);
        }

        [Test] public void RoomPathAcceptsCodePageLinkAndDeepLink()
        {
            Assert.AreEqual("ABC123",ArenaGame.RoomFromLink(" abc123 "));
            Assert.AreEqual("QWERTY",ArenaGame.RoomFromLink("https://site.example/arena/?sala=qwerty"));
            Assert.AreEqual("QWE9TY",ArenaGame.RoomFromLink("https://site.example/arena/index.html?v=2&sala=QWE9TY#x"));
            Assert.AreEqual("ZZZZ11",ArenaGame.RoomFromLink("arenatatica://sala/ZZZZ11"));
            Assert.IsNull(ArenaGame.RoomFromLink("ws://192.168.0.2:9080"));
            Assert.IsNull(ArenaGame.RoomFromLink("https://site.example/?sala=ABC1234"));
            Assert.IsNull(ArenaGame.RoomFromLink(""));
            Assert.AreEqual("https://site.example/arena/?sala=ABC123",ArenaGame.InviteLink("https://site.example/arena/?sala=OLD111#t","ABC123"));
            Assert.AreEqual("arenatatica://sala/ABC123",ArenaGame.InviteLink("","ABC123"));
            Assert.AreEqual("ABC123",ArenaGame.RoomFromLink(ArenaGame.InviteLink("https://a.b/c","ABC123")));
        }

        [Test] public void FloatingTextsOnTheSameSpotStack()
        {
            var active=new System.Collections.Generic.List<(Vector3 at,float until)>();
            Assert.AreEqual(0,ArenaHud.FloatSlot(active,Vector3.zero,0));
            Assert.AreEqual(1,ArenaHud.FloatSlot(active,new Vector3(.2f,0,0),.1f));
            Assert.AreEqual(0,ArenaHud.FloatSlot(active,new Vector3(3,0,0),.1f));
            Assert.AreEqual(0,ArenaHud.FloatSlot(active,Vector3.zero,5));
        }

        [Test] public void TeamPickerCardsFitTheWholeRosterInBothOrientations()
        {
            int n=Content.Roster.Count();
            foreach(var (w,h) in new[]{(1280f-332,720f-78),(720f-20,1280f-308)}) {
                var cell=ArenaHud.CardSize(n,w,h); int cols=Mathf.Max(1,(int)((w+6)/(cell.x+6)));
                Assert.LessOrEqual(Mathf.CeilToInt(n/(float)cols)*(cell.y+6)-6,h+.01f,$"{w}x{h}");
                Assert.GreaterOrEqual(cell.x,77-.01f);
            }
        }
        // 13/10: equipes livres, pool do torneio e LERP dos botões.
        [Test] public void FreeTeamsPoolsAndButtonLerp()
        {
            var saved=ArenaGame.CustomTeams.ToArray();
            try {
                for(int i=0;i<6;i++) ArenaGame.CustomTeams[i]=1;
                Assert.IsNull(ArenaGame.FormatTeams("equipes",3),"um time só não fecha");
                ArenaGame.CustomTeams[2]=2; CollectionAssert.AreEqual(new[]{1,1,2},ArenaGame.FormatTeams("equipes",3),"2 contra 1");
            } finally { saved.CopyTo(ArenaGame.CustomTeams,0); }
            var pool=ArenaGame.ValidPool(new[]{"ciantia","ciantia","nao-existe"});
            Assert.AreEqual(pool.Length,pool.Distinct().Count()); Assert.Contains("ciantia",pool);
            Assert.GreaterOrEqual(pool.Length,ArenaGame.MaxBans+3,"sobra time depois dos bans"); Assert.LessOrEqual(ArenaGame.ValidPool(Content.Roster).Length,ArenaGame.PoolSize);
            Assert.Greater(ArenaBotaoLerp.Goal(true,true,false),1); Assert.Less(ArenaBotaoLerp.Goal(true,true,true),1); Assert.AreEqual(1,ArenaBotaoLerp.Goal(false,true,true));
        }
    }
}
