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

        [Test] public void MenuKeepsTheNineTabs()
        {
            var tabs=(System.Array)typeof(ArenaMenu).GetField("Tabs",BindingFlags.NonPublic|BindingFlags.Static).GetValue(null);
            Assert.AreEqual(9,tabs.Length);
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
    }
}
