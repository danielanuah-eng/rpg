using System;
using System.Linq;

namespace ArenaTatica.Core
{
    // Névoa do labirinto (autor, 18/10 noite): cada time vê as casas coladas (as 8 em volta, diagonal incluída) nos seus
    // lutadores vivos e a sala inteira onde cada um está (MatchState.areas, 19/10). O que o time já viu fica na memória (paredes, chão, portas, portais, braseiro, vela), escurecido e sem
    // lutadores, armadilhas, moedas nem baús. Acender a vela tira a névoa do mapa todo para todos. O Host guarda a memória
    // (MatchState.explored) e cada jogador recebe só o que conhece (Match.ViewFor); andar no escuro segue o que ele conhece.
    public static class Fog
    {
        public static bool On(MatchState s) => s!=null&&s.fog&&!s.revealed;
        // Casas que o jogador vê agora.
        public static bool[] Vision(MatchState s,int player)
        {
            var seen=new bool[s.tiles.Length];
            foreach(var f in s.fighters) {
                if(!f.Alive||!s.Ally(f.team,player)||!Board.Contains(s,f.cell)) continue;
                foreach(var c in Board.Footprint(f)) {
                    for(int x=-1;x<=1;x++) for(int z=-1;z<=1;z++) { var n=c+new Cell(x,z); if(Board.Contains(s,n)) seen[Board.Index(s,n)]=true; }
                    // Dentro de uma sala vê a sala inteira, com as paredes e portas em volta (autor, 19/10).
                    var a=s.areas; for(int i=0;a!=null&&i+3<a.Length;i+=4) if(c.x>=a[i]&&c.x<=a[i+2]&&c.z>=a[i+1]&&c.z<=a[i+3])
                        for(int x=a[i]-1;x<=a[i+2]+1;x++) for(int z=a[i+1]-1;z<=a[i+3]+1;z++) { var n=new Cell(x,z); if(Board.Contains(s,n)) seen[Board.Index(s,n)]=true; }
                }
            }
            return seen;
        }
        // Memória do time do jogador (inclui o que ele vê agora, depois de Explore).
        public static bool[] Memory(MatchState s,int player)
        {
            int team=s.Team(player);
            return Decode(s.explored!=null&&team>=0&&team<s.explored.Length?s.explored[team]:null,s.tiles.Length);
        }
        public static bool Explored(MatchState s,int player,int index)
        {
            int team=s.Team(player); var hex=s.explored!=null&&team>=0&&team<s.explored.Length?s.explored[team]:null;
            int k=index/4; if(hex==null||k<0||k>=hex.Length) return false;
            return (Nibble(hex[k])>>(index%4)&1)!=0;
        }
        // Junta à memória de cada time o que ele vê agora (depois de cada ação e no começo da partida).
        public static void Explore(MatchState s)
        {
            if(s==null||!s.fog||s.tiles==null) return;
            if(s.explored==null||s.explored.Length!=MatchState.MaxPlayers+1) s.explored=new string[MatchState.MaxPlayers+1];
            for(int p=1;p<=s.players;p++) {
                int team=s.Team(p); if(team<=0||team>=s.explored.Length) continue;
                var mem=Decode(s.explored[team],s.tiles.Length); var seen=Vision(s,p);
                for(int i=0;i<mem.Length;i++) mem[i]|=seen[i];
                s.explored[team]=Encode(mem);
            }
        }
        // Estrutura que fica na memória: o resto (baú, espetos, moedas, armadilhas) só aparece colado.
        static bool Fixed(TileObj o) => o==TileObj.Portal||o==TileObj.Braseiro||o==TileObj.Vela||o==TileObj.PortaAberta;
        // Casas como o jogador as conhece: vistas agora inteiras (menos espetos que não saíram e armadilhas de outros times),
        // lembradas só com a estrutura, e o resto chão vazio (andar no escuro passa por ali até bater no que existir).
        public static Tile[] Known(MatchState s,int player)
        {
            var seen=Vision(s,player); var mem=Memory(s,player); var tiles=new Tile[s.tiles.Length];
            for(int i=0;i<tiles.Length;i++) {
                var t=s.tiles[i];
                if(seen[i]) {
                    var k=Copy(t);
                    if(k.Obj==TileObj.Espetos&&!s.sprung.Contains(i)) k.obj=0;
                    if(k.trap!=0&&!s.Ally(k.trap,player)) { k.trap=0; k.trapDamage=0; k.trapPa=0; }
                    tiles[i]=k;
                }
                else if(mem[i]) tiles[i]=new Tile{ terrain=t.terrain, height=t.height, zone=t.zone, lava=t.lava, obj=Fixed(t.Obj)?t.obj:0 };
                else tiles[i]=new Tile();
            }
            return tiles;
        }
        public static Tile Copy(Tile t) => new Tile{height=t.height,coins=t.coins,trap=t.trap,trapDamage=t.trapDamage,trapPa=t.trapPa,frozenBy=t.frozenBy,obj=t.obj,terrain=t.terrain,zone=t.zone,lava=t.lava,mark=t.mark,shadow=t.shadow};
        static int Nibble(char c) => c>='0'&&c<='9'?c-'0':c>='a'&&c<='f'?c-'a'+10:0;
        static bool[] Decode(string hex,int n)
        {
            var bits=new bool[n]; if(string.IsNullOrEmpty(hex)) return bits;
            for(int i=0;i<n;i++) { int k=i/4; if(k>=hex.Length) break; bits[i]=(Nibble(hex[k])>>(i%4)&1)!=0; }
            return bits;
        }
        static string Encode(bool[] bits)
        {
            var chars=new char[(bits.Length+3)/4];
            for(int k=0;k<chars.Length;k++) { int v=0; for(int b=0;b<4;b++) { int i=k*4+b; if(i<bits.Length&&bits[i]) v|=1<<b; } chars[k]="0123456789abcdef"[v]; }
            return new string(chars);
        }
    }
}
