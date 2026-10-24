using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ArenaTatica.Core;

namespace ArenaTatica.Tests
{
    public class ShopArtTests
    {
        public static IEnumerable<string> Items() { foreach(var item in Shop.Catalog) yield return item.id; }

        [TestCaseSource(nameof(Items))]
        public void EveryShopItemLoadsItsOwnTransparentIcon(string id)
        {
            var sprite=Resources.Load<Sprite>("Sprites/Loja/"+id);
            Assert.IsNotNull(sprite,id+" sem imagem na loja");
            Assert.Greater(sprite.rect.width,0,id); Assert.Greater(sprite.rect.height,0,id);
            var texture=new Texture2D(2,2);
            try {
                Assert.IsTrue(texture.LoadImage(System.IO.File.ReadAllBytes("Assets/ArenaTatica/Resources/Sprites/Loja/"+id+".png")));
                var pixels=texture.GetPixels32();bool clear=false,solid=false;
                foreach(var p in pixels) { clear|=p.a==0; solid|=p.a==255; }
                Assert.IsTrue(clear&&solid,id+" precisa de objeto visível e fundo transparente");
            } finally { Object.DestroyImmediate(texture); }
        }
    }
}
