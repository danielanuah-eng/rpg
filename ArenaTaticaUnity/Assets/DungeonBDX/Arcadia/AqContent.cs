using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonBDX.Arcadia
{
    public sealed class AqHeroDef
    {
        public string Id,Name,Ability,AbilityText,Weapon,Clip;
        public int Health=5,Defense=1;
        public bool Ranged;
    }
    [Serializable] public sealed class AqGear
    {
        public string Id,Name,Clip="attack",Icon="sword";
        public int Dice=3,Defense;
        // Sharp: a 5 also counts as critical (gets through "might"). Reroll: one failed attack die is rolled again.
        public bool Ranged,Exhausted,Sharp,Reroll;
        public bool Passive=>Dice<=0;
    }
    // Shop card in the style of the Arcadia upgrades: bought with guild coins while resting. Names are original.
    public sealed class AqShopItem
    {
        public string Id,Name,Text,Icon,Clip="attack";
        public int Cost,Dice,Defense;
        public bool Ranged,Sharp,Reroll;
        public AqGear Create(string id)=>new AqGear{Id=id,Name=Name,Clip=Clip,Icon=Icon,Dice=Dice,Defense=Defense,Ranged=Ranged,Sharp=Sharp,Reroll=Reroll};
    }
    public static class AqShop
    {
        public static readonly AqShopItem[] Items=
        {
            new AqShopItem{Id="iron-blade",Icon="sword",Name="Lâmina de ferro batido",Cost=3,Dice=4,Text="Ataque próximo, 4 dados."},
            new AqShopItem{Id="yew-bow",Icon="bow",Name="Arco longo de teixo",Cost=3,Dice=3,Ranged=true,Text="Ataque à distância, 3 dados."},
            new AqShopItem{Id="leather-vest",Icon="helmet",Name="Elmo acolchoado",Cost=3,Dice=0,Defense=1,Text="Passiva: +1 dado de defesa."},
            new AqShopItem{Id="keen-dagger",Icon="dagger-poison",Name="Adaga afiada",Cost=4,Dice=3,Sharp=true,Text="Ataque próximo, 3 dados. Afiada: 5 também é crítico."},
            new AqShopItem{Id="echo-staff",Icon="wand",Name="Cajado do eco",Cost=4,Dice=3,Ranged=true,Reroll=true,Clip="cast",Text="Ataque à distância, 3 dados. Rerrola uma falha."},
            new AqShopItem{Id="heavy-crossbow",Icon="bow",Name="Besta pesada",Cost=5,Dice=4,Ranged=true,Text="Ataque à distância, 4 dados."},
            new AqShopItem{Id="rune-greatsword",Icon="axe",Name="Machado rúnico",Cost=6,Dice=5,Text="Ataque próximo, 5 dados."},
            new AqShopItem{Id="glass-arrows",Icon="bow",Name="Flechas de vidro-dragão",Cost=6,Dice=3,Ranged=true,Sharp=true,Text="Ataque à distância, 3 dados. Afiada: 5 também é crítico."},
            new AqShopItem{Id="scale-coat",Icon="shield-wood",Name="Escudo de escamas",Cost=6,Dice=0,Defense=2,Text="Passiva: +2 dados de defesa."},
            new AqShopItem{Id="obsidian-spear",Icon="mace",Name="Maça de obsidiana",Cost=7,Dice=4,Sharp=true,Reroll=true,Text="Ataque próximo, 4 dados. Afiada e rerrola uma falha."},
        };
    }
    // Host adjustments, applied as "tune" commands before the first move so save and replay stay seed + commands.
    public static class AqTuning
    {
        public const int HeroHealth=0,HeroDefense=1,HeroDice=2,HeroMove=3,MonsterHealth=4,MonsterDefense=5,MonsterDice=6,MonsterMove=7,MonsterOverkill=8,StartCoins=9,MonsterReward=10,Count=11;
        public static readonly string[] Labels={"Vida dos heróis","Defesa dos heróis","Dados dos heróis","Movimento dos heróis","Vida dos inimigos","Defesa dos inimigos","Dados dos inimigos","Movimento dos inimigos","Overkill dos inimigos","Moedas iniciais","Moedas por inimigo"};
        public static readonly int[] Min={-3,-1,-2,-1,-5,-2,-3,-1,-5,0,-1},Max={5,3,3,2,5,3,3,2,5,20,3};
        public static bool Valid(int stat,int value)=>stat>=0&&stat<Count&&value>=Min[stat]&&value<=Max[stat];
    }
    [Serializable] public sealed class AqUnit
    {
        public int Id,Guild,Health,MaxHealth,Defense,Deaths,Movement=2,Dice=3,Overkill=5,Reward=1;
        public string HeroId,Name,Ability,AbilityText,SpriteFolder;
        public AqCell Cell;
        public bool Monster,Ranged,Huge,SpentLife;
        public readonly List<AqGear> Gear=new List<AqGear>();
        public readonly List<string> Carry=new List<string>();
        public bool Alive=>Health>0;
        public int Occupancy=>Huge?2:1;
    }
    public sealed class AqGuild
    {
        public int Index,Coins;
        public readonly HashSet<string> Completed=new HashSet<string>();
        public int Rituals,Emblems;
    }
    public sealed class AqEffect
    {
        public string Kind,Clip;
        public int Actor=-1,Target=-1,Amount;
        public AqCell From,To;
    }
    public sealed class AqResult
    {
        public bool Accepted;
        public string Message;
        public readonly List<AqEffect> Effects=new List<AqEffect>();
        public AqResult(bool accepted,string message){Accepted=accepted;Message=message;}
    }
    public static class AqContent
    {
        // Individual natural abilities, not shared archetype loadouts. Equipment remains transferable.
        public static readonly IReadOnlyDictionary<string,AqHeroDef> Heroes=Build();
        // The RPG roster: only these are offered to players. The other definitions stay for fixtures and enemy reuse.
        public static readonly string[] Playable={"principe-kellen","ezequiela","damian","arqueiro-jack","cheiro-pneu","ciantia","tchu-tcha",
            "colosso","baltazar","asamyh","perry","diantus","talyssa","edgar"};
        static Dictionary<string,AqHeroDef> Build()
        {
            var d=new Dictionary<string,AqHeroDef>(StringComparer.Ordinal);
            Add(d,"ciantia","Ciantia","dragon","Vínculo dourado","Ataques à distância: +1 dado se um aliado estiver Perto do alvo.","Dragão dourado",true,"summon");
            Add(d,"talyssa","Talyssa","banner","Estandarte rosa","Aliados Perto recebem +1 dado em ataques próximos.","Lâmina rosa");
            Add(d,"teantha","Teantha","phase","Passo entre sombras","Uma vez na ativação, pode atravessar uma porta fechada gastando 1 movimento.","Foice verde",false,"teleport");
            Add(d,"devon","Devon","rebirth","Último fôlego","Uma vez no cenário, sobrevive a dano letal com 1 Vida.","Gume necrótico",false,"attack",6);
            Add(d,"diantus","Diantus","drain","Sangue do duelista","Após ferir em um ataque, recupera 1 Vida, respeitando o limite de cura.","Estocada flamejante",false,"leap");
            Add(d,"damian","Damian","arsenal","Arsenal camaleão","Na ativação, a carta própria pode alternar entre ataque próximo e à distância.","Camaleão de combate",true,"pistol");
            Add(d,"ezequiela","Ezequiela","ward","Selo de proteção","Ao descansar, cada aliado vivo recupera 1 Vida.","Selo luminoso",true,"cast");
            Add(d,"tchu-tcha","Tchu-tcha","fury","Fúria crescente","Ataques próximos: +1 dado quando está ferido.","Ritmo de batalha",false,"attack",6);
            Add(d,"arqueiro-jack","Arqueiro Jack","sniper","Linha perfeita","Ataque à distância antes do movimento: +1 dado.","Flecha certeira",true,"attack",4);
            Add(d,"perry","Perry","scout","Passos leves","A primeira reação de Alerta contra Perry na ativação é evitada.","Adaga furtiva",false,"attack",4);
            Add(d,"asamyh","Asamyh","feline","Agilidade felina","Pode atravessar espaços com 2 inimigos; deve terminar em espaço livre.","Garras velozes");
            Add(d,"colosso","Primal Beast","colossus","Presença colossal","Ocupa o espaço inteiro. Ataques próximos contra monstros: +1 dado.","Punho titânico",false,"attack",7);
            Add(d,"cheiro-pneu","Cheiro de Pneu","charge","Arrancada","Após mover 3 espaços nesta ativação, recebe +1 dado de ataque próximo.","Impacto da corrida",false,"attack",6);
            Add(d,"rei-bobett","Rei Bobett","royal","Tesouro real","A primeira missão que sua guilda cumpre rende 1 moeda adicional.","Cetro real");
            Add(d,"principe-kellen","Príncipe Kellen","flank","Oportunidade","+1 dado ao atacar um alvo Perto de outro inimigo dele.","Lâmina do príncipe",false,"attack",4);
            Add(d,"baltazar","Baltazar","berserk","Sem recuo","+1 sucesso em ataque próximo quando resta apenas 1 Vida.","Machado de guerra",false,"attack",6);
            Add(d,"vespa-cinzenta","Vespa Cinzenta","sting","Ferrão","Ao ferir um monstro, reduz sua próxima reação de Vingança em 1 dado.","Picada cinzenta",false,"attack",4);
            Add(d,"viuva-da-agulha","Viúva da Agulha","web","Fio de captura","Monstros feridos por ela não se movem na Vingança desse ataque.","Agulha venenosa",true,"attack",4);
            Add(d,"guardiao-de-musgo","Guardião de Musgo","moss","Casca viva","Defende com +1 dado enquanto estiver em uma área interior.","Raiz ancestral",false,"attack",6);
            Add(d,"domador-de-correntes","Domador de Correntes","chain","Alcance da corrente","Sua arma próxima alcança 2 espaços, com linha de visão.","Corrente de ferro");
            Add(d,"ogro-de-pedra","Ogro de Pedra","stone","Pele de granito","Defende com +1 dado contra ataques próximos.","Marreta de pedra",false,"attack",6);
            Add(d,"edgar","Edgar","ghost","Passo espectral","Usar portal não gasta movimento uma vez por ativação.","Lâmina espectral",true,"cast");
            Add(d,"brumak","Brumak","breaker","Quebra-portas","A primeira abertura de porta da ativação não gasta movimento.","Martelo de Brumak",false,"attack",6);
            Add(d,"ossyro","Ossyro","reaper","Ceifador","+1 dado contra inimigos que já estão feridos.","Foice de Ossyro");
            Add(d,"gryvro","Gryvro","isolate","Caçador solitário","+1 dado contra alvo sem aliados Perto.","Garras de Gryvro");
            Add(d,"alvyr","Alvyr","focus","Foco arcano","Pode rerrolar uma falha em ataques à distância.","Raio de Alvyr",true,"cast",4);
            Add(d,"raizor","Raizor","thorns","Espinhos","Ao sofrer dano próximo, causa 1 ferimento ao atacante. Não gera outra retaliação.","Chicote de raízes");
            Add(d,"nixira","Nixira","frost","Gelo cortante","+1 dado contra um alvo que divide seu espaço com outro personagem.","Lança glacial",true,"cast",4);
            Add(d,"lyra-faisca","Lyra Faísca","gunslinger","Tiro cruzado","À distância, ignora 1 dado de defesa do alvo.","Revólver da cinza",true,"attack",4);
            Add(d,"orion-prisma","Orion Prisma","arcane","Ressonância","Ao obter crítico no ataque, restaura outra carta própria exausta uma vez por ativação.","Orbe do prisma",true,"cast",4);
            Add(d,"vaelis-vinculo","Vaelis do Vínculo","bond","Pacto dracônico","Ao causar dano, cura 1 Vida do aliado ferido mais próximo, se estiver Perto.","Sopro vinculado",true,"summon");
            return d;
        }
        static void Add(Dictionary<string,AqHeroDef> d,string id,string name,string ability,string title,string text,string weapon,bool ranged=false,string clip="attack",int hp=5)
        {d.Add(id,new AqHeroDef{Id=id,Name=name,Ability=ability,AbilityText=title+": "+text,Weapon=weapon,Ranged=ranged,Clip=clip,Health=hp,Defense=1});}
        public static AqHeroDef Hero(string id)=>Heroes.TryGetValue(id,out var h)?h:throw new ArgumentException("Herói sem configuração AQ: "+id);
        public static AqGear Signature(AqHeroDef h)=>new AqGear{Id="signature."+h.Id,Name=h.Weapon,Ranged=h.Ranged,Dice=h.Ranged?2:3,Clip=h.Clip,Icon=GearIcon(h.Clip,h.Ranged)};
        public static string GearIcon(string clip,bool ranged)=>clip=="cast"?"wand":clip=="summon"?"orb":clip=="hammer"?"hammer":ranged?"bow":"sword";
    }
}
