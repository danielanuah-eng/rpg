using NUnit.Framework;
using UnityEngine;

namespace ArenaTatica.Tests
{
    // Iluminação (ArenaLuz): luzes montadas com sombra, tom global para eventos (Lua de Sangue) e o gancho do ArenaEventTint.
    public class ArenaLuzTests
    {
        bool ligadaAntes;
        [SetUp] public void Antes() { ligadaAntes=ArenaLuz.Ligada; ArenaLuz.Ligada=true; ArenaLuz.Montar(); }
        [TearDown] public void Depois() { ArenaLuz.Desmontar(); ArenaLuz.Ligada=ligadaAntes; }

        static Light Luz(string name) => System.Array.Find(Object.FindObjectsByType<Light>(),l=>l.name==name||(name=="LuzPrincipal"&&l.type==LightType.Directional&&l.name!="LuzPreenchimento"));

        [Test] public void SolTemSombraSuaveEPreenchimentoNao()
        {
            Assert.AreEqual(LightShadows.Soft,Luz("LuzPrincipal").shadows);
            Assert.AreEqual(LightShadows.None,Luz("LuzPreenchimento").shadows,"só o sol projeta sombra (desempenho)");
            Assert.AreEqual(UnityEngine.Rendering.AmbientMode.Trilight,RenderSettings.ambientMode);
            Assert.IsNotNull(ArenaLuz.SpriteMaterial,"material de sprite com passe de sombra em Resources");
        }

        [Test] public void TingirMudaLuzAmbienteESpritesELimparDevolve()
        {
            var antes=Luz("LuzPrincipal").color; var ceu=RenderSettings.ambientSkyColor;
            ArenaLuz.Tingir(new Color(1,.1f,.1f),1,0);
            Assert.Less(Luz("LuzPrincipal").color.g,antes.g*.5f,"sol puxado para o vermelho");
            Assert.Less(RenderSettings.ambientSkyColor.g,ceu.g*.5f,"ambiente puxado para o vermelho");
            Assert.AreEqual(1f,Shader.GetGlobalFloat("_ArenaTomForca"),"sprites recebem o tom");
            ArenaLuz.LimparTom(0);
            Assert.AreEqual(antes,Luz("LuzPrincipal").color); Assert.AreEqual(0f,Shader.GetGlobalFloat("_ArenaTomForca"));
        }

        [Test] public void EventoComCorUsaOGanchoDaIluminacao()
        {
            Assert.IsNotNull(ArenaEventTint.Hook,"ArenaEventTint passa a tingir pelo ArenaLuz");
            ArenaEventTint.Hook(new Color(.8f,.1f,.1f));
            Assert.AreNotEqual(Color.white,ArenaLuz.Tom);
        }

        [Test] public void DesligadaVoltaAoVisualAntigo()
        {
            ArenaLuz.Ligada=false;
            Assert.AreEqual(LightShadows.None,Luz("LuzPrincipal").shadows);
            Assert.IsFalse(Luz("LuzPreenchimento").enabled);
            Assert.AreEqual(UnityEngine.Rendering.AmbientMode.Flat,RenderSettings.ambientMode);
        }
    }
}
