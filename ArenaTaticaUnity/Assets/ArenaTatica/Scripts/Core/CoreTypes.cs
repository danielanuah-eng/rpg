using System;
using System.Collections.Generic;

namespace ArenaTatica
{
    [Serializable] public struct Cell : IEquatable<Cell>
    {
        public int x, z;
        public Cell(int x, int z) { this.x=x; this.z=z; }
        public bool Equals(Cell c) => x==c.x && z==c.z;
        public override bool Equals(object o) => o is Cell c && Equals(c);
        public override int GetHashCode() => x*397 ^ z;
        public static bool operator ==(Cell a, Cell b) => a.Equals(b);
        public static bool operator !=(Cell a, Cell b) => !a.Equals(b);
        public static Cell operator +(Cell a, Cell b) => new Cell(a.x+b.x,a.z+b.z);
        public int Distance(Cell b) => Math.Abs(x-b.x)+Math.Abs(z-b.z);
    }
}

// Núcleo estilo Krosmaster (Documentacao/DESIGN-KROSMASTER.md). C# puro, determinístico, roda no Host.
namespace ArenaTatica.Core
{
    public enum Terrain { Livre, Arbusto, Arvore, Caixa }
    // A face de crítico do Krosmaster virou Cataclisma (decisão 11); o coringa Crítico/Esquiva virou Esquiva/Trava.
    public enum Face { Cataclisma, Escudo, Trava, Esquiva, CoringaEsquivaTrava, Coringa }
    public enum VictoryMode { Galeoes, Eliminacao, Tesouro, Dominio }
    public enum SkillKind { Dano, Invocacao, Cura, Armadilha }
    public enum ShopKind { Item, Magia, Pet, Armadilha, Buff }

    [Serializable] public class Tile { public int height, coins, trap, trapDamage; public Terrain terrain; public bool zone; }

    [Serializable] public class SkillDef
    {
        public string id, name, summon;
        public SkillKind kind;
        public int pa, minRange=1, maxRange=1, damage, perTurn;
        public bool line, sight=true, modifiable;
    }
    [Serializable] public class FighterDef
    {
        public string id, name;
        public int hp, pa=6, pm=3, initiative, level;
        public bool flying;
        public SkillDef[] skills;
    }
    // Loja: Item e Buff valem na hora; Magia, Pet e Armadilha dão skills de uso limitado (cargas).
    [Serializable] public class ShopDef
    {
        public string id, name;
        public ShopKind kind;
        public int price, weight=1, pa, pm, hp, armor, range, turns, uses=1;
        public SkillDef skill;
    }
    [Serializable] public class Buff { public string id; public int turns, pa, pm, armor, range; }
    [Serializable] public class Charge { public string skill; public int uses; }
    [Serializable] public class TeamShop { public string[] slots=new string[0]; }
    [Serializable] public class Fighter
    {
        public int id, team, hp, pa, pm, summoner, bonusPa, bonusPm, bonusHp, armor, bonusRange;
        public string def;
        public Cell cell;
        public bool bought;
        public List<string> used=new List<string>();
        public List<Charge> charges=new List<Charge>();
        public List<Buff> buffs=new List<Buff>();
        public bool Alive => hp>0;
    }
    // Jogador (assento 1..6): controla lutadores, tem moedas e loja próprias. Time: divide Galeões e vitória (0 = time próprio).
    [Serializable] public class PlayerSetup { public string[] fighters={"ciantia"}; public int team; }
    [Serializable] public class MatchSetup
    {
        public uint seed=1;
        public VictoryMode mode=VictoryMode.Galeoes;
        public int galeoes=12, target;
        public bool flatMap;
        // Vazio = duelo com team1 e team2.
        public PlayerSetup[] players;
        public string[] team1={"ciantia"}, team2={"ciantia"};
    }
    [Serializable] public class MatchState
    {
        public const int MaxPlayers=6;
        public int width=Board.Size, height=Board.Size, round=1, active, revision=1, winner, nextId=1, target, players=2;
        public uint rng;
        public bool over;
        public VictoryMode mode;
        public Tile[] tiles;
        public List<Fighter> fighters=new List<Fighter>();
        public List<int> timeline=new List<int>();
        // Índice 0 sem uso. Fighter.team, coins e shops são por jogador; galeoes, collected, domain e eliminated por time (alliance[jogador]).
        public int[] alliance={0,1,2,3,4,5,6}, galeoes=new int[MaxPlayers+1], coins=new int[MaxPlayers+1], collected=new int[MaxPlayers+1], domain=new int[MaxPlayers+1];
        public bool[] eliminated=new bool[MaxPlayers+1];
        public TeamShop[] shops=Shops();
        static TeamShop[] Shops() { var a=new TeamShop[MaxPlayers+1]; for(int i=0;i<a.Length;i++) a[i]=new TeamShop(); return a; }
        public Tile At(Cell c) => tiles[c.z*width+c.x];
        public Fighter F(int id) => fighters.Find(f=>f.id==id);
        public Fighter Active => over||timeline.Count==0?null:F(timeline[active]);
        public Cell Mirror(Cell c) => new Cell(width-1-c.x,height-1-c.z);
        public int Team(int player) => player>=1&&player<=MaxPlayers?alliance[player]:0;
        public bool Ally(int a,int b) => a==b||(Team(a)!=0&&Team(a)==Team(b));
        // Times ainda na partida, na ordem dos assentos.
        public List<int> Teams() { var list=new List<int>(); for(int p=1;p<=players;p++) if(!list.Contains(alliance[p])) list.Add(alliance[p]); return list; }
        // Times de verdade (3v3, 2v2v2) ou todos contra todos.
        public bool TeamPlay => Teams().Count<players;
    }
    [Serializable] public class Command
    {
        public string kind, skill;
        public int revision, fighter, slot;
        public Cell cell;
    }
    // kind: turno, mover, trava, moeda, armadilha, armadilha-posta, compra, loja, cura, ataque, cataclisma, invocar, nocaute, eliminado, rodada, dominio, vitoria
    [Serializable] public class GameEvent
    {
        public string kind, info;
        public int actor, target, value;
        public Cell cell;
        public Cell[] path;
        public Face[] dice;
    }

    public static class Dice
    {
        public static uint Next(MatchState s) { uint x=s.rng==0?0x9E3779B9u:s.rng; x^=x<<13; x^=x>>17; x^=x<<5; s.rng=x; return x; }
        public static int Range(MatchState s,int n) => (int)(Next(s)%(uint)n);
        public static Face Roll(MatchState s) => (Face)Range(s,6);
        public static bool Lock(Face f) => f==Face.Trava||f==Face.CoringaEsquivaTrava||f==Face.Coringa;
        public static bool Dodge(Face f) => f==Face.Esquiva||f==Face.CoringaEsquivaTrava||f==Face.Coringa;
        public static bool Armor(Face f) => f==Face.Escudo||f==Face.Coringa;
    }
}
