using System;
using System.Collections.Generic;
using System.Linq;

namespace ArenaTatica.Core
{
    // Loja randomizada por time (DESIGN-KROSMASTER.md). Números iniciais; o balanceamento vem depois.
    public static class Shop
    {
        public const int Slots=3;
        public static readonly ShopDef[] Catalog=
        {
            new ShopDef { id="botas-aladas", name="Botas aladas", kind=ShopKind.Item, price=4, weight=3, pm=1 },
            new ShopDef { id="escudo-de-bronze", name="Escudo de bronze", kind=ShopKind.Item, price=5, weight=2, armor=1 },
            new ShopDef { id="amuleto-vital", name="Amuleto vital", kind=ShopKind.Item, price=3, weight=3, hp=3 },
            new ShopDef { id="luneta", name="Luneta", kind=ShopKind.Item, price=4, weight=2, range=1 },
            new ShopDef { id="raio-arcano", name="Raio arcano", kind=ShopKind.Magia, price=3, weight=3,
                skill=new SkillDef { id="raio-arcano", name="Raio arcano", kind=SkillKind.Dano, pa=2, minRange=1, maxRange=5, damage=3, modifiable=true } },
            new ShopDef { id="pocao-de-cura", name="Poção de cura", kind=ShopKind.Magia, price=2, weight=3,
                skill=new SkillDef { id="pocao-de-cura", name="Poção de cura", kind=SkillKind.Cura, pa=1, minRange=0, maxRange=1, damage=4, sight=false } },
            new ShopDef { id="lobo-de-cinzas", name="Lobo de cinzas", kind=ShopKind.Pet, price=4, weight=2,
                skill=new SkillDef { id="chamar-lobo", name="Chamar lobo", kind=SkillKind.Invocacao, pa=2, minRange=1, maxRange=1, sight=false, summon="lobo-de-cinzas" } },
            new ShopDef { id="armadilha-de-espinhos", name="Armadilha de espinhos", kind=ShopKind.Armadilha, price=2, weight=3,
                skill=new SkillDef { id="armadilha-de-espinhos", name="Armadilha de espinhos", kind=SkillKind.Armadilha, pa=1, minRange=1, maxRange=3, damage=2 } },
            new ShopDef { id="furia", name="Fúria", kind=ShopKind.Buff, price=3, weight=2, pa=2, turns=2 },
            new ShopDef { id="passo-leve", name="Passo leve", kind=ShopKind.Buff, price=2, weight=2, pm=2, turns=2 },
            // Itens de 10/10 (EVENTOS-LOJA-ELENCO.md, seção 2).
            new ShopDef { id="martelo-perfurante", name="Martelo perfurante", kind=ShopKind.Magia, price=4, weight=2,
                skill=new SkillDef { id="martelo-perfurante", name="Martelo perfurante", kind=SkillKind.Dano, pa=3, minRange=1, maxRange=1, damage=3, pierce=true } },
            new ShopDef { id="corrente-de-ferro", name="Corrente de ferro", kind=ShopKind.Magia, price=3, weight=2,
                skill=new SkillDef { id="corrente-de-ferro", name="Corrente de ferro", kind=SkillKind.Dano, pa=2, minRange=2, maxRange=4, damage=0, line=true, pull=true } },
            new ShopDef { id="bomba-de-fumaca", name="Bomba de fumaça", kind=ShopKind.Magia, price=3, weight=2,
                skill=new SkillDef { id="bomba-de-fumaca", name="Bomba de fumaça", kind=SkillKind.Esconder, pa=1, minRange=0, maxRange=0, sight=false, noCover=true, anim="hide" } },
            new ShopDef { id="pena-de-fenix", name="Pena de fênix", kind=ShopKind.Magia, price=6, weight=1,
                skill=new SkillDef { id="pena-de-fenix", name="Pena de fênix", kind=SkillKind.Reviver, pa=2, minRange=1, maxRange=1, sight=false, heal=4 } },
            new ShopDef { id="totem-de-cura", name="Totem de cura", kind=ShopKind.Pet, price=4, weight=2,
                skill=new SkillDef { id="erguer-totem", name="Erguer totem", kind=SkillKind.Invocacao, pa=2, minRange=1, maxRange=1, sight=false, summon="totem-de-cura" } },
            new ShopDef { id="mina-de-gelo", name="Mina de gelo", kind=ShopKind.Armadilha, price=3, weight=2,
                skill=new SkillDef { id="mina-de-gelo", name="Mina de gelo", kind=SkillKind.Armadilha, pa=1, minRange=1, maxRange=3, damage=0, paLoss=2 } },
            new ShopDef { id="gancho-de-escalada", name="Gancho de escalada", kind=ShopKind.Item, price=3, weight=2, climb=true },
            new ShopDef { id="cristal-de-foco", name="Cristal de foco", kind=ShopKind.Item, price=5, weight=1, focus=true },
            new ShopDef { id="manto-de-espinhos", name="Manto de espinhos", kind=ShopKind.Item, price=4, weight=2, thorns=1 },
            new ShopDef { id="ancora", name="Âncora", kind=ShopKind.Item, price=3, weight=2, anchor=true },
            new ShopDef { id="bencao-de-pedra", name="Bênção de pedra", kind=ShopKind.Buff, price=3, weight=2, armor=2, turns=2 },
            // Blink Dagger (autor 22/10): teleporte até 6 casas por 2 PA, recarga 1; fica para sempre com quem comprou.
            new ShopDef { id="blink-dagger", name="Blink Dagger", kind=ShopKind.Magia, price=6, weight=2, uses=ShopDef.Permanent,
                skill=new SkillDef { id="blink-dagger", name="Blink Dagger", kind=SkillKind.Teleporte, pa=2, minRange=1, maxRange=6, sight=false, cooldown=1, anim="teleport" } },
        };
        public static ShopDef Def(string id) => Array.Find(Catalog,d=>d.id==id);
        public static ShopDef BySkill(string skill) => Array.Find(Catalog,d=>d.skill!=null&&d.skill.id==skill);

        // Sorteio com peso, sem repetir item dentro da mesma loja.
        public static void Refill(MatchState s,int team)
        {
            var shop=s.shops[team];
            if(shop.slots.Length!=Slots) Array.Resize(ref shop.slots,Slots);
            for(int i=0;i<Slots;i++)
            {
                if(!string.IsNullOrEmpty(shop.slots[i])) continue;
                // Loja configurada na sala: só os itens liberados (sem itens suficientes, o espaço fica vazio).
                var pool=Catalog.Where(d=>!shop.slots.Contains(d.id)&&(s.shopPool==null||s.shopPool.Length==0||s.shopPool.Contains(d.id))).ToList();
                if(pool.Count==0) continue;
                int roll=Dice.Range(s,pool.Sum(d=>d.weight));
                foreach(var d in pool) { if(roll<d.weight) { shop.slots[i]=d.id; break; } roll-=d.weight; }
            }
        }
    }
}
