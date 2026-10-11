using System;
using System.Collections.Generic;
using System.Linq;

namespace ArenaTatica.Core
{
    // Mapa simétrico pelo centro (giro de 180°): platôs, árvores, arbustos, caixas, zona central e moedas.
    // O tamanho vem de Board.SizeFor(jogadores); os grupos de spawn ficam em pares espelhados (0↔1, 2↔3, 4↔5).
    public static class MapGen
    {
        // Cenários só mudam o visual; o sorteio usa a seed sem consumir o rng (o mapa da seed continua o mesmo).
        public static readonly string[] Themes={"Pedreira","Bosque","Deserto","Neve","Vulcão"};
        public static int Theme(MatchSetup setup) => setup.theme>=0?setup.theme%Themes.Length:(int)((setup.seed*2654435761u)>>16)%Themes.Length;
        public static void Generate(MatchState s,bool flat)
        {
            float k=s.width*s.height/144f;
            for(int attempt=0;attempt<20;attempt++)
            {
                Clear(s);
                if(flat) return;
                for(int i=0;i<UnityEngine.Mathf.RoundToInt(2*k);i++) Plateau(s);
                Scatter(s,Terrain.Arvore,UnityEngine.Mathf.RoundToInt(3*k)); Scatter(s,Terrain.Arbusto,UnityEngine.Mathf.RoundToInt(3*k)); Scatter(s,Terrain.Caixa,UnityEngine.Mathf.RoundToInt(2*k));
                foreach(var c in ShopCells(s)) s.At(c).height=1;
                foreach(var c in ZoneRing(s)) s.At(c).height=1;
                Center(s,true);
                for(int slot=0;slot<s.players;slot++) foreach(var c in Spawns(s,slot)) ClearAround(s,c);
                if(SpawnsConnected(s)) return;
            }
            Clear(s); Center(s,true);
        }
        // Meio do mapa: a loja nas 4 casas centrais e a zona central nas 8 casas encostadas nela (dois por lado).
        public static Cell[] ShopCells(MatchState s) { int x=s.width/2-1, z=s.height/2-1; return new[]{ new Cell(x,z), new Cell(x+1,z), new Cell(x,z+1), new Cell(x+1,z+1) }; }
        public static Cell[] ZoneRing(MatchState s)
        {
            int x=s.width/2-1, z=s.height/2-1;
            return new[]{ new Cell(x,z-1), new Cell(x+1,z-1), new Cell(x,z+2), new Cell(x+1,z+2), new Cell(x-1,z), new Cell(x-1,z+1), new Cell(x+2,z), new Cell(x+2,z+1) };
        }
        // Põe a loja (as casas dela nunca são zona) e deixa o anel livre; ring=true também marca o anel como zona.
        static void Center(MatchState s,bool ring)
        {
            foreach(var c in ShopCells(s)) { var t=s.At(c); t.terrain=Terrain.Loja; t.zone=false; t.coins=0; }
            foreach(var c in ZoneRing(s)) { var t=s.At(c); t.terrain=Terrain.Livre; if(ring) t.zone=true; }
        }
        // Loja no meio; mapa sem zona fora dela (os antigos tinham a zona nas 4 casas do meio) ganha a zona no anel.
        public static void Center(MatchState s) { Center(s,false); if(!s.tiles.Any(t=>t.zone)) Center(s,true); }
        // Peça do kit que faz o papel de um terreno (árvore, arbusto, caixa); Livre = enfeite só visual.
        public static Terrain SkinTerrain(string piece)
        {
            if(string.IsNullOrEmpty(piece)) return Terrain.Livre;
            if(piece.StartsWith("arvore")) return Terrain.Arvore;
            if(piece=="arbusto") return Terrain.Arbusto;
            if(piece=="caixa"||piece=="caixa_rachada"||piece=="pedra") return Terrain.Caixa;
            return Terrain.Livre;
        }
        // Enfeites baixos espalhados em pares espelhados nas casas livres. Sorteio próprio pela seed: não mexe no rng da partida.
        public static readonly string[] SmallDecor={"enfeite_capim","enfeite_flores","enfeite_seixos","enfeite_pedra_baixa","enfeite_cogumelos","enfeite_raiz_chao"};
        public static void Decorate(MatchState s,uint seed)
        {
            s.decor.Clear(); var rng=new System.Random((int)(seed%int.MaxValue)); int pairs=s.width*s.height/24;
            for(int tries=0;tries<pairs*6&&s.decor.Count<pairs*2;tries++)
            {
                var c=new Cell(rng.Next(s.width),rng.Next(s.height)); var m=s.Mirror(c);
                if(c==m||s.At(c).terrain!=Terrain.Livre||s.At(c).zone||s.decor.Exists(d=>(int)Math.Round(d.x)==c.x&&(int)Math.Round(d.z)==c.z)) continue;
                string piece=SmallDecor[rng.Next(SmallDecor.Length)]; float rot=rng.Next(360), dx=(float)(rng.NextDouble()-.5)*.4f, dz=(float)(rng.NextDouble()-.5)*.4f;
                s.decor.Add(new Decor{ piece=piece, x=c.x+dx, z=c.z+dz, rot=rot });
                s.decor.Add(new Decor{ piece=piece, x=m.x-dx, z=m.z-dz, rot=rot+180 });
            }
        }
        // Casas de um grupo de spawn (até 4 lutadores). Slots pares embaixo, ímpares no espelho de cima.
        public static Cell[] Spawns(MatchState s,int slot)
        {
            Cell[] c;
            if(s.width==Board.Size) c=new[]{ new Cell(2,1), new Cell(4,1), new Cell(7,1), new Cell(9,1) };
            else
            {
                int[] xs=s.width<=16?new[]{4,11}:new[]{3,14,9};
                int x=xs[(slot/2)%xs.Length];
                c=new[]{ new Cell(x,1), new Cell(x-2,1), new Cell(x+2,1), new Cell(x,3) };
            }
            if(slot%2==1) for(int i=0;i<c.Length;i++) c[i]=s.Mirror(c[i]);
            return c;
        }
        // ── Mapas do editor ──
        public static int PlayersFor(int size) => size<=Board.Size?2:size<=16?4:6;
        public static bool Fits(MapDef m,int size) => m?.tiles!=null&&m.width==size&&m.tiles.Length==size*size;
        // Copia relevo, peças e zona; as casas de início de todos os grupos ficam livres.
        public static void Load(MatchState s,MapDef m)
        {
            for(int i=0;i<s.tiles.Length;i++) { var t=m.tiles[i]; s.tiles[i]=new Tile{ height=UnityEngine.Mathf.Clamp(t.height,0,Board.MaxHeight), terrain=t.terrain==Terrain.Loja?Terrain.Livre:t.terrain, zone=t.zone }; }
            Center(s);
            for(int slot=0;slot<PlayersFor(s.width);slot++) foreach(var c in Spawns(s,slot)) s.At(c).terrain=Terrain.Livre;
            s.decor=m.decor==null?new List<Decor>():m.decor.Select(d=>new Decor{ piece=d.piece, x=d.x, z=d.z, rot=d.rot, scale=d.scale }).ToList();
        }
        // Mapa novo: plano, ou o sorteado de uma seed (para partir de algo pronto).
        public static MapDef NewMap(int size,uint seed=0)
        {
            var s=new MatchState{ width=size, height=size, players=PlayersFor(size), rng=seed==0?1:seed };
            Generate(s,seed==0); if(seed!=0) Decorate(s,seed);
            return new MapDef{ width=size, tiles=s.tiles, decor=s.decor };
        }
        // Problema que impede jogar no mapa (null = pode jogar): casas de início presas ou a zona central inalcançável.
        public static string Problem(MapDef m)
        {
            if(m?.tiles==null||!Board.ValidSize(m.width)||m.tiles.Length!=m.width*m.width) return "Tamanho inválido.";
            var s=new MatchState{ width=m.width, height=m.width, players=PlayersFor(m.width) }; s.tiles=new Tile[m.tiles.Length]; Load(s,m);
            var probe=new Fighter{ def="ciantia", cell=Spawns(s,0)[0], hp=1 }; var dist=Board.Costs(s,probe,true);
            for(int slot=0;slot<s.players;slot++) foreach(var c in Spawns(s,slot)) if(dist[Board.Index(s,c)]==int.MaxValue) return "Há casas de início sem caminho até as outras.";
            for(int i=0;i<s.tiles.Length;i++) if(s.tiles[i].zone&&dist[i]==int.MaxValue) return "A zona central não tem caminho.";
            return null;
        }
        static void Clear(MatchState s) { s.tiles=new Tile[s.width*s.height]; for(int i=0;i<s.tiles.Length;i++) s.tiles[i]=new Tile(); }
        static Cell Random(MatchState s) => new Cell(Dice.Range(s,s.width),Dice.Range(s,s.height/2));
        static void Both(MatchState s,Cell c,System.Action<Tile> set) { set(s.At(c)); set(s.At(s.Mirror(c))); }
        static void Plateau(MatchState s)
        {
            var o=Random(s); int w=2+Dice.Range(s,2), d=2+Dice.Range(s,2);
            for(int x=o.x;x<o.x+w;x++) for(int z=o.z;z<o.z+d;z++) { var c=new Cell(x,z); if(Board.Contains(s,c)) Both(s,c,t=>t.height=1); }
            if(Dice.Range(s,2)==0) for(int x=o.x+1;x<o.x+w-1;x++) for(int z=o.z+1;z<o.z+d-1;z++) { var c=new Cell(x,z); if(Board.Contains(s,c)) Both(s,c,t=>t.height=2); }
        }
        static void Scatter(MatchState s,Terrain terrain,int count) { for(int i=0;i<count;i++) Both(s,Random(s),t=>t.terrain=terrain); }
        static void ClearAround(MatchState s,Cell c)
        {
            for(int x=-1;x<=1;x++) for(int z=-1;z<=1;z++) { var n=c+new Cell(x,z); if(Board.Contains(s,n)) Both(s,n,t=>{ t.terrain=Terrain.Livre; t.height=0; }); }
        }
        static bool SpawnsConnected(MatchState s)
        {
            var probe=new Fighter{ def="ciantia", cell=Spawns(s,0)[0], hp=1 };
            var dist=Board.Costs(s,probe,true);
            for(int slot=0;slot<s.players;slot++) foreach(var c in Spawns(s,slot)) if(dist[Board.Index(s,c)]==int.MaxValue) return false;
            foreach(var c in ZoneRing(s)) if(dist[Board.Index(s,c)]==int.MaxValue) return false;
            return true;
        }
        // Moedas em pares espelhados, só em casas livres e vazias.
        public static int SpawnCoins(MatchState s,int pairs)
        {
            int placed=0;
            for(int tries=0;tries<pairs*20&&placed<pairs;tries++)
            {
                var c=new Cell(Dice.Range(s,s.width),Dice.Range(s,s.height)); var m=s.Mirror(c);
                if(c==m||!Free(s,c)||!Free(s,m)) continue;
                s.At(c).coins++; s.At(m).coins++; placed++;
            }
            return placed;
        }
        public static bool Free(MatchState s,Cell c) { var t=s.At(c); return t.terrain==Terrain.Livre&&t.coins==0&&Board.At(s,c)==null; }
    }
}
