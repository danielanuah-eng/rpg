using NUnit.Framework;
using UnityEngine;

namespace ArenaTatica.Tests
{
    // UI comum com o DungeonBDX (16/10): molduras 9-slice compartilhadas e fallback liso quando a arte falta.
    public class UiSkinTests
    {
        [TestCase("button")]
        [TestCase("button-selected")]
        [TestCase("button-disabled")]
        [TestCase("panel")]
        [TestCase("panel-dark")]
        [TestCase("portrait-ring")]
        public void SharedArtLoadsAsCrispNineSlice(string id)
        {
            var sprite=ArenaTema.Skin(id);
            Assert.IsNotNull(sprite,id);
            var t=sprite.texture; float edge=Mathf.Round(Mathf.Min(t.width,t.height)*ArenaTema.SkinEdge);
            Assert.AreEqual(new Vector4(edge,edge,edge,edge),sprite.border);
            Assert.AreEqual(FilterMode.Point,t.filterMode,"pixel art sem suavizar");
        }

        [Test]
        public void MissingArtKeepsFlatLook() => Assert.IsNull(ArenaTema.Skin("nao-existe"));

        [Test]
        public void ButtonFramePerState()
        {
            var go=new GameObject("Botao"); var pele=go.AddComponent<ArenaBotaoPele>();
            try {
                pele.Normal=ArenaTema.Skin("button"); pele.Selected=ArenaTema.Skin("button-selected"); pele.Disabled=ArenaTema.Skin("button-disabled");
                Assert.AreSame(pele.Normal,pele.Current(true));
                pele.Chosen=true; Assert.AreSame(pele.Selected,pele.Current(true));
                Assert.AreSame(pele.Disabled,pele.Current(false),"desligado vence escolhido");
            } finally { Object.DestroyImmediate(go); }
        }
    }
}
