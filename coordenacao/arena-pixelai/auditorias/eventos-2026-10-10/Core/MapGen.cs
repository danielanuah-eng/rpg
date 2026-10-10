namespace ArenaTatica.Core
{
    // Mapa simétrico pelo centro (giro de 180°): platôs, árvores, arbustos, caixas, zona central e moedas.
    // O tamanho vem de Board.SizeFor(jogadores); os grupos de spawn ficam em pares espelhados (0↔1, 2↔3, 4↔5).
    public static class MapGen
    {
        public static void Generate(MatchState s,bool flat)
        {
            float k=s.width*s.height/144f;
            for(int attempt=0;attempt<20;attempt++)
            {
                Clear(s);
                if(flat) return;
                for(int i=0;i<UnityEngine.Mathf.RoundToInt(2*k);i++) Plateau(s);
                Scatter(s,Terrain.Arvore,UnityEngine.Mathf.RoundToInt(3*k)); Scatter(s,Terrain.Arbusto,UnityEngine.Mathf.RoundToInt(3*k)); Scatter(s,Terrain.Caixa,UnityEngine.Mathf.RoundToInt(2*k));
                for(int x=s.width/2-1;x<=s.width/2;x++) for(int z=s.height/2-1;z<=s.height/2;z++) { var t=s.At(new Cell(x,z)); t.terrain=Terrain.Livre; t.height=1; t.zone=true; }
                for(int slot=0;slot<s.players;slot++) foreach(var c in Spawns(s,slot)) ClearAround(s,c);
                if(SpawnsConnected(s)) return;
            }
            Clear(s);
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
            return dist[Board.Index(s,new Cell(s.width/2-1,s.height/2-1))]!=int.MaxValue;
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
