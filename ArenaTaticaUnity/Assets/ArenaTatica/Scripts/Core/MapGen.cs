using System;
using System.Collections.Generic;
using System.Linq;

namespace ArenaTatica.Core
{
    // Mapa aleatório (autor, 18/10: chega de cenário espelhado): blocos temáticos prontos (podem sair girados ou espelhados)
    // e manchas sorteadas entre eles, a loja e a zona no meio. Os times começam em lados opostos (Spawns) e o mapa é refeito
    // se um lado ficar mais perto da loja, da vela ou de um baú (Fair). O tamanho vem de Board.SizeFor(jogadores).
    public static class MapGen
    {
        // Cenários só mudam o visual; o sorteio usa a seed sem consumir o rng.
        // Os 3 últimos são fechados (labirintos, 18/10): salas e corredores com paredes, portas, névoa, vela e boss.
        public static readonly string[] Themes={"Pedreira","Bosque","Deserto","Neve","Vulcão","Cripta","Biblioteca","Jardim"};
        public const int OpenThemes=5, BauMoedas=3;
        public static bool Closed(int theme) => theme>=OpenThemes&&theme<Themes.Length;
        // Sorteado (-1): só os abertos; o labirinto é escolhido no menu (Cripta, Biblioteca, Jardim).
        public static int Theme(MatchSetup setup) => setup.theme>=0?setup.theme%Themes.Length:(int)((setup.seed*2654435761u)>>16)%OpenThemes;
        // Gera pelo cenário de s.theme (já escolhido): aberto ou fechado.
        public static void Generate(MatchState s,bool flat)
        {
            if(!flat&&Closed(s.theme)) { GenerateClosed(s); return; }
            float k=s.width*s.height/144f; var best=new Best();
            for(int attempt=0;attempt<30;attempt++)
            {
                Clear(s); s.decor=new List<Decor>();
                if(flat) return;
                for(int i=0;i<UnityEngine.Mathf.RoundToInt(2*k);i++) Plateau(s);
                var used=new bool[s.tiles.Length];
                Roads(s,used);
                int blocks=UnityEngine.Mathf.RoundToInt(2+2*k);
                for(int i=0;i<blocks;i++) PlaceBlock(s,OpenBlocks,used,false);
                // Poucos desenhos no chão (autor, 18/10: nada de quadrados aleatórios) e, entre os blocos, grupinhos de árvores,
                // arbustos e pedras e barris soltos.
                for(int i=Dice.Range(s,2)+(k>1.5f?1:0);i>0;i--) Motif(s,used,false);
                for(int i=0;i<UnityEngine.Mathf.RoundToInt(2*k);i++) Cluster(s,used,Terrain.Arvore,2+Dice.Range(s,2));
                for(int i=0;i<UnityEngine.Mathf.RoundToInt(2*k);i++) Cluster(s,used,Terrain.Arbusto,2+Dice.Range(s,3));
                for(int i=0;i<UnityEngine.Mathf.RoundToInt(k);i++) Cluster(s,used,Terrain.Caixa,1+Dice.Range(s,2));
                for(int i=0;i<UnityEngine.Mathf.RoundToInt(1+k);i++) Barrel(s,used);
                foreach(var c in ShopCells(s)) s.At(c).height=1;
                foreach(var c in ZoneRing(s)) s.At(c).height=1;
                Center(s,true);
                for(int slot=0;slot<s.players;slot++) foreach(var c in Spawns(s,slot)) ClearAround(s,c);
                if(SpawnsConnected(s)&&Keep(s,best)) return;
            }
            if(Restore(s,best)) return;
            Clear(s); s.decor=new List<Decor>(); Center(s,true);
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
            foreach(var c in ShopCells(s)) { var t=s.At(c); t.terrain=Terrain.Loja; t.zone=false; t.coins=0; t.obj=0; }
            foreach(var c in ZoneRing(s)) { var t=s.At(c); t.terrain=Terrain.Livre; t.obj=0; if(ring) t.zone=true; }
        }
        // Loja no meio; mapa sem zona fora dela (os antigos tinham a zona nas 4 casas do meio) ganha a zona no anel.
        public static void Center(MatchState s) { Center(s,false); if(!s.tiles.Any(t=>t.zone)) Center(s,true); }
        // Peça do kit que faz o papel de um terreno (árvore, arbusto, caixa); Livre = enfeite só visual.
        public static Terrain SkinTerrain(string piece)
        {
            if(string.IsNullOrEmpty(piece)) return Terrain.Livre;
            if(piece.StartsWith("arvore")) return Terrain.Arvore;
            if(piece=="arbusto") return Terrain.Arbusto;
            if(piece=="caixa"||piece=="caixa_rachada"||piece=="pedra"||Array.IndexOf(DungeonBlocks,piece)>=0) return Terrain.Caixa;
            return Terrain.Livre;
        }
        // Props do DungeonBDX que fazem o papel de caixa (obstáculo que tapa a visão) num mapa do editor.
        public static readonly string[] DungeonBlocks={"dg_sarcophagus","dg_statue","dg_pillar","dg_crates","dg_table","dg_bookshelf","dg_barrels","dg_fountain","dg_deadtree"};

        // ── Pisos de cenário (só visual): "piso_<tile do DungeonBDX>" em Decor, centro (x,z) e raio em scale ──
        // A base é o kit 3D do Blender; as manchas trocam a tampa das casas por grama, pedra etc.
        public const string FloorPrefix="piso_";
        static readonly string[][] SceneryFloors={ new[]{"street","crypt"}, new[]{"garden","street"}, new[]{"terracotta","street"}, new[]{"marble","street"},
            new[]{"crypt","street"}, new[]{"crypt","marble"}, new[]{"wood","mosaic"}, new[]{"garden","street"} };
        static string SceneryFloor(int theme,int which) { var f=SceneryFloors[Math.Max(0,theme)%SceneryFloors.Length]; return f[which%f.Length]; }
        // A forma vai em rot: 0 quadrado de borda irregular (mapas antigos), Losango, Moldura (só a borda), Cruz, Quadrado cheio;
        // Estrada+d é um trecho reto na direção Board.Dirs[d], de scale casas além da primeira.
        public const int Losango=1, Moldura=2, Cruz=3, Quadrado=4, Estrada=10;
        static void Patch(MatchState s,Cell c,float radius,string tile,int shape=0) { if(!string.IsNullOrEmpty(tile)&&radius>=0) s.decor.Add(new Decor{ piece=FloorPrefix+tile, x=c.x, z=c.z, scale=radius, rot=shape }); }
        static string Alt(string tile) => tile switch { "garden"=>"street", "street"=>"mosaic", "terracotta"=>"street", "crypt"=>"marble", "wood"=>"mosaic", _=>"marble" };
        public static bool IsRoad(Decor d) => d.piece!=null&&d.piece.StartsWith(FloorPrefix)&&d.rot>=Estrada;
        public static IEnumerable<Cell> PatchCells(MatchState s,Decor d)
        {
            int r=(int)Math.Round(d.scale), cx=(int)Math.Round(d.x), cz=(int)Math.Round(d.z), shape=(int)Math.Round(d.rot);
            if(shape>=Estrada) {
                var dir=Board.Dirs[(shape-Estrada)%4];
                for(int i=0;i<=r;i++) { var c=new Cell(cx+dir.x*i,cz+dir.z*i); if(Board.Contains(s,c)) yield return c; }
                yield break;
            }
            for(int x=cx-r;x<=cx+r;x++) for(int z=cz-r;z<=cz+r;z++) {
                var c=new Cell(x,z); if(!Board.Contains(s,c)) continue;
                int dx=Math.Abs(x-cx), dz=Math.Abs(z-cz);
                bool inside=shape switch { Losango=>dx+dz<=r, Moldura=>Math.Max(dx,dz)==r, Cruz=>dx==0||dz==0, Quadrado=>true,
                    _=>!(r>0&&dx==r&&dz==r&&((x*7+z*13)&1)==0) };
                if(inside) yield return c;
            }
        }
        // Casas com piso de cenário: a última mancha que cobre a casa vale, e as estradas ficam por cima de tudo.
        public static Dictionary<Cell,string> FloorPatches(MatchState s)
        {
            var map=new Dictionary<Cell,string>();
            foreach(bool roads in new[]{false,true}) foreach(var d in s.decor) {
                if(d.piece==null||!d.piece.StartsWith(FloorPrefix)||IsRoad(d)!=roads) continue;
                foreach(var c in PatchCells(s,d)) map[c]=d.piece.Substring(FloorPrefix.Length);
            }
            return map;
        }
        // Estrada de tijolos (autor, 18/10: "às vezes só"): nenhuma, uma ou duas, da beira esquerda ou direita até a zona, em
        // três trechos retos (entra, dobra, chega). As casas dela ficam fora dos blocos e grupinhos.
        static void Roads(MatchState s,bool[] used)
        {
            int roads=Dice.Range(s,3), first=Dice.Range(s,2); var ring=ZoneRing(s);
            for(int i=0;i<roads;i++) {
                bool left=(first+i)%2==0; int dir=left?1:0;
                var goal=ring[(left?4:6)+Dice.Range(s,2)];
                var c=new Cell(left?0:s.width-1,s.height/3+Dice.Range(s,Math.Max(1,s.height/3)));
                int bend=1+Dice.Range(s,Math.Max(1,Math.Abs(goal.x-c.x)-2));
                c=Segment(s,used,c,dir,bend);
                c=Segment(s,used,c,goal.z>c.z?3:2,Math.Abs(goal.z-c.z));
                Segment(s,used,c,dir,Math.Abs(goal.x-c.x));
            }
        }
        static Cell Segment(MatchState s,bool[] used,Cell from,int dir,int length)
        {
            if(length<=0) return from;
            var road=new Decor{ piece=FloorPrefix+"terracotta", x=from.x, z=from.z, rot=Estrada+dir, scale=length }; s.decor.Add(road);
            foreach(var c in PatchCells(s,road)) used[Board.Index(s,c)]=true;
            var d=Board.Dirs[dir]; return new Cell(from.x+d.x*length,from.z+d.z*length);
        }
        // Desenho no chão: losango, cruz ou moldura do piso do cenário com o centro em mosaico, longe dos blocos e do meio.
        // closed: só em casas que já são chão.
        static void Motif(MatchState s,bool[] used,bool closed)
        {
            for(int tries=0;tries<30;tries++) {
                int r=1+Dice.Range(s,2), shape=new[]{Losango,Cruz,Moldura}[Dice.Range(s,3)];
                var c=new Cell(r+Dice.Range(s,Math.Max(1,s.width-2*r)),r+Dice.Range(s,Math.Max(1,s.height-2*r)));
                var cells=PatchCells(s,new Decor{ x=c.x, z=c.z, scale=r, rot=shape }).ToList();
                if(cells.Any(p=>used[Board.Index(s,p)]||Reserved(s,p)||closed&&s.At(p).terrain!=Terrain.Livre)) continue;
                Patch(s,c,r,SceneryFloor(s.theme,Dice.Range(s,2)),shape); Patch(s,c,0,"mosaic",Quadrado);
                foreach(var p in cells) used[Board.Index(s,p)]=true;
                return;
            }
        }
        // Barril solto numa casa livre entre os blocos.
        static void Barrel(MatchState s,bool[] used)
        {
            for(int tries=0;tries<20;tries++) {
                var c=RandomAll(s); int i=Board.Index(s,c);
                if(used[i]||Reserved(s,c)||s.At(c).terrain!=Terrain.Livre||s.At(c).obj!=0) continue;
                Stamp(s,c,'R',0); used[i]=true; return;
            }
        }

        // ── Blocos temáticos ──
        // . livre · T árvore · b arbusto · c caixa · p pedra · h/H relevo +1/+2 · $ baú · ^ espetos · props do DungeonBDX como
        // obstáculo: S sarcófago, U estátua, P pilar, B estante, N mesa, R barris, F fonte, D árvore seca; enfeites do kit em
        // casa livre: i tocha, f cerca, l placa, o fogueira.
        sealed class Block { public string name, floor; public string[] rows; public Block(string n,string f,params string[] r) { name=n; floor=f; rows=r; } }
        static readonly Block[] OpenBlocks={
            new Block("clareira","garden", "T...T","..b..",".b.b.","..b..","T...T"),
            new Block("ruina","street", "pp.pp","p...p","..$..","p...p","pp.pp"),
            new Block("colina",null, ".hhh.","hhHhh","hHHHh","hhHhh",".hhh."),
            new Block("bosque","garden", "TbT.","b..b",".bTb","T..."),
            new Block("acampamento","terracotta", ".c.c.","c...c","..$..",".bTb."),
            new Block("mirante","street", "hhh","hHh","hhh"),
            new Block("passagem-perigosa","crypt", "p.p",".^.","p.p"),
            new Block("muralha","street", "ppp.","...p","p.^."),
            // Cidadezinhas (autor, 18/10): praça com fonte, feira e poço, com barris, caixas, cercas e tochas.
            new Block("praca","street", "R.l.c",".....","f.F.f",".....","c.i.R"),
            new Block("feira","terracotta", "NiN.","....","RcR."),
            new Block("poco","street", "R.i",".F.","i.R"),
            new Block("armazem","terracotta", "RR.f","R...","..oc") };
        static readonly Block[][] ClosedBlocks={
            new[]{ new Block("sarcofagos","crypt","S.S","...","S.S"), new Block("ossario","crypt","U.U",".$.","U.U"), new Block("pilares","marble","P.P","...","P.P"), new Block("cova","crypt",".^.","^$^") },
            new[]{ new Block("estantes","wood","BBB","...",".N."), new Block("leitura","mosaic","N.N","...","R.R"), new Block("deposito","wood","RR.","R.$","..."), new Block("pilares","mosaic","P.P","...","P.P") },
            new[]{ new Block("fonte","garden",".b.","bFb",".b."), new Block("estatuas","street","U.U","...","U.U"), new Block("canteiro","garden","bb.","..$",".bb"), new Block("bosquete","garden","D.D",".^.","D.D") } };
        // Bloco sorteado, girado (0–3) e talvez espelhado, numa área livre de outros blocos, das casas de início e do meio.
        // closed: só cabe em casas que já são chão (dentro das salas) e é desfeito se cortar o caminho.
        static bool PlaceBlock(MatchState s,Block[] pool,bool[] used,bool closed)
        {
            var b=pool[Dice.Range(s,pool.Length)]; int turns=Dice.Range(s,4); bool flip=Dice.Range(s,2)==0;
            int h=b.rows.Length, w=b.rows.Max(r=>r.Length);
            int W=turns%2==0?w:h, H=turns%2==0?h:w;
            for(int tries=0;tries<80;tries++)
            {
                var o=new Cell(Dice.Range(s,Math.Max(1,s.width-W+1)),Dice.Range(s,Math.Max(1,s.height-H+1)));
                var cells=new List<(Cell c,char ch)>();
                for(int z=0;z<h;z++) for(int x=0;x<w;x++) {
                    char ch=x<b.rows[z].Length?b.rows[z][x]:'.';
                    int fx=flip?w-1-x:x, rx=fx, rz=z;
                    for(int t=0;t<turns;t++) { int nx=(t%2==0?h:w)-1-rz; rz=rx; rx=nx; }
                    cells.Add((o+new Cell(rx,rz),ch));
                }
                if(cells.Any(p=>!Board.Contains(s,p.c)||used[Board.Index(s,p.c)]||Reserved(s,p.c)||closed&&s.At(p.c).terrain!=Terrain.Livre)) continue;
                var backup=cells.Select(p=>Copy(s.At(p.c))).ToList(); int decor=s.decor.Count;
                foreach(var (c,ch) in cells) Stamp(s,c,ch,90*turns);
                if(closed&&!SpawnsConnected(s)) { for(int i=0;i<cells.Count;i++) s.tiles[Board.Index(s,cells[i].c)]=backup[i]; s.decor.RemoveRange(decor,s.decor.Count-decor); continue; }
                foreach(var p in cells) used[Board.Index(s,p.c)]=true;
                var mid=cells.Aggregate((x:0f,z:0f),(a,p)=>(a.x+p.c.x,a.z+p.c.z));
                // Piso desenhado embaixo do bloco: moldura com miolo de outro piso, losango com centro em mosaico, ou cruz.
                var at=new Cell((int)Math.Round(mid.x/cells.Count),(int)Math.Round(mid.z/cells.Count)); int r=(Math.Max(W,H)+1)/2;
                if(b.floor!=null) switch(Dice.Range(s,3)) {
                    case 0: Patch(s,at,r,b.floor,Quadrado); Patch(s,at,r-1,Alt(b.floor),Quadrado); break;
                    case 1: Patch(s,at,r+1,b.floor,Losango); Patch(s,at,0,"mosaic",Quadrado); break;
                    default: Patch(s,at,r,b.floor,Quadrado); Patch(s,at,r,Alt(b.floor),Cruz); break;
                }
                return true;
            }
            return false;
        }
        static Tile Copy(Tile t) => new Tile{height=t.height,coins=t.coins,trap=t.trap,trapDamage=t.trapDamage,trapPa=t.trapPa,frozenBy=t.frozenBy,obj=t.obj,terrain=t.terrain,zone=t.zone,lava=t.lava,mark=t.mark};
        static void Stamp(MatchState s,Cell c,char ch,int rot)
        {
            var t=s.At(c);
            void Prop(string id) { t.terrain=Terrain.Caixa; s.decor.Add(new Decor{ piece="dg_"+id, x=c.x, z=c.z }); }
            void Ornament(string piece) { t.terrain=Terrain.Livre; s.decor.Add(new Decor{ piece=piece, x=c.x, z=c.z, rot=rot }); }
            switch(ch) {
                case 'T': t.terrain=Terrain.Arvore; break;
                case 'b': t.terrain=Terrain.Arbusto; break;
                case 'c': t.terrain=Terrain.Caixa; break;
                case 'p': t.terrain=Terrain.Caixa; s.decor.Add(new Decor{ piece="pedra", x=c.x, z=c.z, rot=90*((c.x+c.z)%4) }); break;
                case 'h': t.height=Math.Max(t.height,1); break;
                case 'H': t.height=Math.Max(t.height,2); break;
                case '$': t.terrain=Terrain.Livre; t.Obj=TileObj.Bau; break;
                case '^': t.terrain=Terrain.Livre; t.Obj=TileObj.Espetos; break;
                case 'S': Prop("sarcophagus"); break; case 'U': Prop("statue"); break; case 'P': Prop("pillar"); break;
                case 'B': Prop("bookshelf"); break; case 'N': Prop("table"); break; case 'R': Prop("barrels"); break;
                case 'F': Prop("fountain"); break; case 'D': Prop("deadtree"); break;
                case 'i': Ornament("enfeite_tocha"); break; case 'f': Ornament("enfeite_cerca"); break;
                case 'l': Ornament("enfeite_placa"); break; case 'o': Ornament("enfeite_fogueira"); break;
                default: t.terrain=Terrain.Livre; break;
            }
        }
        // Colado nas casas de início (inclusive diagonal) ou na loja e zona: nada de bloco nem mancha.
        static bool Reserved(MatchState s,Cell c)
        {
            int x0=s.width/2-2, z0=s.height/2-2;
            if(c.x>=x0&&c.x<=x0+3&&c.z>=z0&&c.z<=z0+3) return true;
            for(int slot=0;slot<s.players;slot++) foreach(var sp in Spawns(s,slot,6)) if(Math.Abs(sp.x-c.x)<=1&&Math.Abs(sp.z-c.z)<=1) return true;
            return false;
        }
        // Grupinho de count peças do mesmo terreno em volta de uma casa sorteada.
        static void Cluster(MatchState s,bool[] used,Terrain terrain,int count)
        {
            var o=RandomAll(s);
            for(int i=0;i<count;i++) {
                var c=o+new Cell(Dice.Range(s,3)-1,Dice.Range(s,3)-1);
                if(Board.Contains(s,c)&&!used[Board.Index(s,c)]&&!Reserved(s,c)&&s.At(c).obj==0) s.At(c).terrain=terrain;
            }
        }

        // Enfeites baixos espalhados nas casas livres (sem espelho). Sorteio próprio pela seed: não mexe no rng da partida.
        // Mantém os pisos de cenário e os modelos de peça que o gerador já pôs.
        public static readonly string[] SmallDecor={"enfeite_capim","enfeite_flores","enfeite_seixos","enfeite_pedra_baixa","enfeite_cogumelos","enfeite_raiz_chao"};
        // Enfeites que os blocos de vila põem (tocha, cerca, placa, fogueira) ficam; o sorteio só refaz os de SmallDecor.
        static readonly string[] VillageDecor={"enfeite_tocha","enfeite_cerca","enfeite_placa","enfeite_fogueira"};
        static bool Kept(Decor d) => d.piece!=null&&(d.piece.StartsWith(FloorPrefix)||SkinTerrain(d.piece)!=Terrain.Livre||Array.IndexOf(VillageDecor,d.piece)>=0);
        static bool HasOrnament(MatchState s,Cell c) => s.decor.Exists(d=>d.piece!=null&&!d.piece.StartsWith(FloorPrefix)&&(int)Math.Round(d.x)==c.x&&(int)Math.Round(d.z)==c.z);
        public static void Decorate(MatchState s,uint seed)
        {
            if(Closed(s.theme)) { DecorateClosed(s,seed); return; }
            s.decor.RemoveAll(d=>!Kept(d)); var rng=new System.Random((int)(seed%int.MaxValue)); int count=s.width*s.height/12, placed=0;
            bool Bare(Cell c) => Board.Contains(s,c)&&s.At(c).terrain==Terrain.Livre&&!s.At(c).zone&&s.At(c).obj==0;
            // Tochas de um lado da estrada, uma a cada 3 casas (às vezes falta uma).
            var road=new HashSet<Cell>();
            foreach(var d in s.decor.Where(IsRoad).ToList()) {
                var dir=Board.Dirs[((int)Math.Round(d.rot)-Estrada)%4]; int i=0, side=rng.Next(2)*2-1;
                foreach(var c in PatchCells(s,d)) {
                    road.Add(c);
                    if(i++%3!=1||!Bare(c)||rng.Next(4)==0) continue;
                    s.decor.Add(new Decor{ piece="enfeite_tocha", x=c.x-dir.z*side*.42f, z=c.z+dir.x*side*.42f, rot=rng.Next(360) });
                }
            }
            // Os outros em canteiros: 2 a 4 do mesmo enfeite em volta de uma casa, fora da estrada.
            for(int tries=0;tries<count*4&&placed<count;tries++)
            {
                var o=new Cell(rng.Next(s.width),rng.Next(s.height)); string piece=SmallDecor[rng.Next(SmallDecor.Length)];
                for(int n=2+rng.Next(3);n>0;n--) {
                    var c=o+new Cell(rng.Next(3)-1,rng.Next(3)-1);
                    if(!Bare(c)||road.Contains(c)||HasOrnament(s,c)) continue;
                    float rot=rng.Next(360), dx=(float)(rng.NextDouble()-.5)*.4f, dz=(float)(rng.NextDouble()-.5)*.4f;
                    s.decor.Add(new Decor{ piece=piece, x=c.x+dx, z=c.z+dz, rot=rot }); placed++;
                }
            }
        }
        // Casas de um grupo de spawn (4 lutadores; count até 6 para times maiores da sala, Sprint 3.6). Slots pares embaixo,
        // ímpares do lado oposto (giro de 180°): os times começam em lados opostos mesmo com o cenário aleatório.
        public static Cell[] Spawns(MatchState s,int slot,int count=4)
        {
            Cell[] c;
            if(s.width==Board.Size) c=new[]{ new Cell(2,1), new Cell(4,1), new Cell(7,1), new Cell(9,1), new Cell(3,2), new Cell(8,2) };
            else
            {
                int[] xs=s.width<=16?new[]{4,s.width-5}:new[]{3,s.width-4,s.width/2};
                int x=xs[(slot/2)%xs.Length];
                c=new[]{ new Cell(x,1), new Cell(x-2,1), new Cell(x+2,1), new Cell(x,3), new Cell(x-2,3), new Cell(x+2,3) };
            }
            c=c.Take(Math.Max(4,Math.Min(count,c.Length))).ToArray();
            if(slot%2==1) for(int i=0;i<c.Length;i++) c[i]=s.Mirror(c[i]);
            return c;
        }
        // Lados justos: em cada par de lados opostos (0↔1, 2↔3, 4↔5), a distância a pé (abrindo portas) até a zona,
        // até a vela e até o baú mais perto quase igual.
        public static bool Fair(MatchState s) => Unfairness(s)==0;
        // Quanto passa das folgas (0 = justo); o gerador fica com a tentativa de menor valor se nenhuma sair justa.
        public static int Unfairness(MatchState s)
        {
            int over=0;
            int Near(int[] d,IEnumerable<Cell> cells) { int best=int.MaxValue; foreach(var c in cells) best=Math.Min(best,d[Board.Index(s,c)]); return best; }
            var all=Enumerable.Range(0,s.tiles.Length).Select(i=>new Cell(i%s.width,i/s.width)).ToList();
            var chests=all.Where(c=>s.At(c).Obj==TileObj.Bau).ToList(); var candle=all.Where(c=>s.At(c).Obj==TileObj.Vela).SelectMany(c=>Board.Dirs.Select(d=>c+d)).Where(c=>Board.Contains(s,c)).ToList();
            for(int a=0;a+1<s.players;a+=2)
            {
                var da=Board.Costs(s,new Fighter{ def="ciantia", cell=Spawns(s,a)[0], hp=1 },true,true);
                var db=Board.Costs(s,new Fighter{ def="ciantia", cell=Spawns(s,a+1)[0], hp=1 },true,true);
                over+=Math.Max(0,Math.Abs(Near(da,ZoneRing(s))-Near(db,ZoneRing(s)))-2);
                if(candle.Count>0) over+=Math.Max(0,Math.Abs(Near(da,candle)-Near(db,candle))-2);
                if(chests.Count>0) over+=Math.Max(0,Math.Abs(Near(da,chests)-Near(db,chests))-3);
            }
            return over;
        }
        // Melhor tentativa até agora (menos injusta), para não cair no mapa vazio.
        sealed class Best { public int score=int.MaxValue; public Tile[] tiles; public List<Decor> decor; }
        static bool Keep(MatchState s,Best best)
        {
            int u=Unfairness(s); if(u==0) return true;
            if(u<best.score) { best.score=u; best.tiles=s.tiles.Select(Copy).ToArray(); best.decor=s.decor.Select(d=>new Decor{piece=d.piece,x=d.x,z=d.z,rot=d.rot,scale=d.scale}).ToList(); }
            return false;
        }
        static bool Restore(MatchState s,Best best) { if(best.tiles==null) return false; s.tiles=best.tiles; s.decor=best.decor; return true; }
        // ── Mapas do editor ──
        public static int PlayersFor(int size) => size<=Board.Size?2:size<=16?4:6;
        public static bool Fits(MapDef m,int size) => m?.tiles!=null&&m.width==size&&m.tiles.Length==size*size;
        // Copia relevo, peças e zona; as casas de início de todos os grupos ficam livres.
        public static void Load(MatchState s,MapDef m)
        {
            for(int i=0;i<s.tiles.Length;i++) { var t=m.tiles[i]; s.tiles[i]=new Tile{ height=UnityEngine.Mathf.Clamp(t.height,0,Board.MaxHeight), terrain=t.terrain==Terrain.Loja?Terrain.Livre:t.terrain, zone=t.zone, obj=t.obj }; }
            Center(s);
            for(int slot=0;slot<PlayersFor(s.width);slot++) foreach(var c in Spawns(s,slot)) { s.At(c).terrain=Terrain.Livre; s.At(c).obj=0; }
            FixObjects(s);
            s.decor=m.decor==null?new List<Decor>():m.decor.Select(d=>new Decor{ piece=d.piece, x=d.x, z=d.z, rot=d.rot, scale=d.scale }).ToList();
        }
        // Mapa novo: plano, ou o sorteado de uma seed (para partir de algo pronto).
        public static MapDef NewMap(int size,uint seed=0,int theme=-1)
        {
            var s=new MatchState{ width=size, height=size, players=PlayersFor(size), rng=seed==0?1:seed, theme=Math.Max(0,theme) };
            Generate(s,seed==0); if(seed!=0) Decorate(s,seed);
            return new MapDef{ width=size, theme=theme, tiles=s.tiles, decor=s.decor };
        }
        // Problema que impede jogar no mapa (null = pode jogar): casas de início presas ou a zona central inalcançável.
        public static string Problem(MapDef m)
        {
            if(m?.tiles==null||!Board.ValidSize(m.width)||m.tiles.Length!=m.width*m.width) return "Tamanho inválido.";
            var s=new MatchState{ width=m.width, height=m.width, players=PlayersFor(m.width) }; s.tiles=new Tile[m.tiles.Length]; Load(s,m);
            var probe=new Fighter{ def="ciantia", cell=Spawns(s,0)[0], hp=1 }; var dist=Board.Costs(s,probe,true,true);
            for(int slot=0;slot<s.players;slot++) foreach(var c in Spawns(s,slot)) if(dist[Board.Index(s,c)]==int.MaxValue) return "Há casas de início sem caminho até as outras.";
            for(int i=0;i<s.tiles.Length;i++) if(s.tiles[i].zone&&dist[i]==int.MaxValue) return "A zona central não tem caminho.";
            return null;
        }
        static void Clear(MatchState s) { s.tiles=new Tile[s.width*s.height]; for(int i=0;i<s.tiles.Length;i++) s.tiles[i]=new Tile(); }
        static Cell RandomAll(MatchState s) => new Cell(Dice.Range(s,s.width),Dice.Range(s,s.height));
        static void Plateau(MatchState s)
        {
            var o=RandomAll(s); int w=2+Dice.Range(s,2), d=2+Dice.Range(s,2);
            for(int x=o.x;x<o.x+w;x++) for(int z=o.z;z<o.z+d;z++) { var c=new Cell(x,z); if(Board.Contains(s,c)) s.At(c).height=1; }
            if(Dice.Range(s,2)==0) for(int x=o.x+1;x<o.x+w-1;x++) for(int z=o.z+1;z<o.z+d-1;z++) { var c=new Cell(x,z); if(Board.Contains(s,c)) s.At(c).height=2; }
        }
        static void ClearAround(MatchState s,Cell c)
        {
            for(int x=-1;x<=1;x++) for(int z=-1;z<=1;z++) { var n=c+new Cell(x,z); if(Board.Contains(s,n)) { var t=s.At(n); t.terrain=Terrain.Livre; t.height=0; t.obj=0; } }
        }
        static bool SpawnsConnected(MatchState s)
        {
            var probe=new Fighter{ def="ciantia", cell=Spawns(s,0)[0], hp=1 };
            var dist=Board.Costs(s,probe,true,true);
            for(int slot=0;slot<s.players;slot++) foreach(var c in Spawns(s,slot)) if(dist[Board.Index(s,c)]==int.MaxValue) return false;
            foreach(var c in ZoneRing(s)) if(dist[Board.Index(s,c)]==int.MaxValue) return false;
            return true;
        }
        // Moedas em pares de lados opostos (justo mesmo com o cenário aleatório), só em casas livres e vazias.
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
        public static bool Free(MatchState s,Cell c) { var t=s.At(c); return t.terrain==Terrain.Livre&&t.coins==0&&t.obj==0&&Board.At(s,c)==null; }

        // ── Objetos fixos (TileObj) ──
        // Braseiro e vela são obstáculo (casa de Caixa); os outros ficam em casa livre. Portal sem par vira casa vazia.
        public static void FixObjects(MatchState s)
        {
            for(int i=0;i<s.tiles.Length;i++) {
                var t=s.tiles[i];
                if(t.Obj==TileObj.Braseiro||t.Obj==TileObj.Vela) { if(t.terrain!=Terrain.Loja) t.terrain=Terrain.Caixa; else t.obj=0; }
                else if(t.obj!=0&&t.terrain!=Terrain.Livre) t.obj=0;
            }
            var gates=Enumerable.Range(0,s.tiles.Length).Where(i=>s.tiles[i].Obj==TileObj.Portal).ToList();
            if(gates.Count%2==1) s.tiles[gates.Last()].obj=0;
        }
        // Portais fixos (by=0, nunca fecham), em pares pela ordem das casas: 1º↔2º, 3º↔4º…
        public static void Portals(MatchState s)
        {
            s.portals??=new List<Portal>(); s.portals.RemoveAll(p=>p.by==0);
            var gates=Enumerable.Range(0,s.tiles.Length).Where(i=>s.tiles[i].Obj==TileObj.Portal).ToList();
            for(int i=0;i+1<gates.Count;i+=2) { s.portals.Add(new Portal{ a=gates[i], b=gates[i+1] }); s.portals.Add(new Portal{ a=gates[i+1], b=gates[i] }); }
        }

        // ── Labirintos (18/10): tudo começa parede; abre o salão do meio, a sala de início de cada time, salas sorteadas e
        // corredores (1 ou 2 casas) até o meio. Bolsões sem caminho voltam a ser parede. Depois blocos temáticos nas salas,
        // caixas, braseiros, baús, espetos, um par de portais, portas e a vela, sem cortar o caminho entre os inícios.
        static void GenerateClosed(MatchState s)
        {
            float k=s.width*s.height/144f; int theme=(s.theme-OpenThemes)%ClosedBlocks.Length; var best=new Best();
            for(int attempt=0;attempt<80;attempt++)
            {
                Clear(s); s.decor=new List<Decor>(); foreach(var t in s.tiles) t.terrain=Terrain.Parede;
                int cx=s.width/2-1, cz=s.height/2-1;
                int hall0=s.width<=Board.Size?1:2; var rooms=new List<(int x0,int z0,int x1,int z1)>{ (cx-hall0,cz-hall0,cx+1+hall0,cz+1+hall0) };
                for(int slot=0;slot<s.players;slot++) { var sp=Spawns(s,slot,6); rooms.Add((sp.Min(c=>c.x)-1,sp.Min(c=>c.z)-1,sp.Max(c=>c.x)+1,sp.Max(c=>c.z)+1)); }
                int extra=UnityEngine.Mathf.RoundToInt(3+3*k);
                for(int i=0;i<extra;i++) {
                    int w=3+Dice.Range(s,3), d=3+Dice.Range(s,2), x0=Dice.Range(s,s.width-w+1), z0=Dice.Range(s,s.height-d+1);
                    rooms.Add((x0,z0,x0+w-1,z0+d-1));
                }
                foreach(var r in rooms) for(int x=r.x0;x<=r.x1;x++) for(int z=r.z0;z<=r.z1;z++) Open(s,new Cell(x,z));
                // Corredores em L do centro de cada sala ao salão do meio; às vezes também até a sala anterior (dá voltas).
                var hall=new Cell(cx+Dice.Range(s,2),cz+Dice.Range(s,2));
                for(int i=1;i<rooms.Count;i++) {
                    var r=rooms[i]; var from=new Cell((r.x0+r.x1)/2,(r.z0+r.z1)/2);
                    Corridor(s,from,hall,1+Dice.Range(s,2),Dice.Range(s,2)==0);
                    if(i>1&&Dice.Range(s,2)==0) { var p=rooms[i-1]; Corridor(s,from,new Cell((p.x0+p.x1)/2,(p.z0+p.z1)/2),1,Dice.Range(s,2)==0); }
                }
                foreach(var c in ShopCells(s)) s.At(c).height=1;
                foreach(var c in ZoneRing(s)) s.At(c).height=1;
                Center(s,true);
                for(int slot=0;slot<s.players;slot++) foreach(var c in Spawns(s,slot)) ClearAround(s,c);
                if(!SpawnsConnected(s)) continue;
                var dist=Board.Costs(s,new Fighter{ def="ciantia", cell=Spawns(s,0)[0], hp=1 },true);
                for(int i=0;i<s.tiles.Length;i++) if(s.tiles[i].terrain==Terrain.Livre&&dist[i]==int.MaxValue) s.tiles[i].terrain=Terrain.Parede;
                int open=s.tiles.Count(t=>t.terrain==Terrain.Livre);
                if(open<s.tiles.Length*.4f||open>s.tiles.Length*.85f) continue;
                var used=new bool[s.tiles.Length];
                for(int i=0;i<UnityEngine.Mathf.RoundToInt(1+2*k);i++) PlaceBlock(s,ClosedBlocks[theme],used,true);
                for(int i=Dice.Range(s,2)+(k>1.5f?1:0);i>0;i--) Motif(s,used,true);
                Place(s,UnityEngine.Mathf.RoundToInt(3*k),c=>Room(s,c)&&Away(s,c,2),t=>t.terrain=Terrain.Caixa);
                Place(s,2,c=>Room(s,c)&&Away(s,c,3),t=>{ t.terrain=Terrain.Caixa; t.Obj=TileObj.Braseiro; });
                // Baús em pares de lados opostos (como as moedas): o prêmio fica justo mesmo com o labirinto aleatório.
                Place(s,Math.Max(1,UnityEngine.Mathf.RoundToInt(k)),c=>Away(s,c,4)&&Free(s,s.Mirror(c))&&s.Mirror(c)!=c,t=>t.Obj=TileObj.Bau,true);
                Place(s,UnityEngine.Mathf.RoundToInt(4*k),c=>!Room(s,c)&&Away(s,c,2),t=>t.Obj=TileObj.Espetos);
                Place(s,1,c=>Away(s,c,2)&&c.z<s.height/2,t=>t.Obj=TileObj.Portal);
                Place(s,1,c=>Away(s,c,2)&&c.z>=s.height/2,t=>t.Obj=TileObj.Portal);
                FixObjects(s);
                // Portas nos corredores retos (as duas vizinhas livres em linha), longe das casas de início.
                Place(s,UnityEngine.Mathf.RoundToInt(4*k),c=>Away(s,c,2)&&Straight(s,c),t=>t.terrain=Terrain.Porta);
                Candle(s);
                if(Keep(s,best)) return;
            }
            if(Restore(s,best)) return;
            Clear(s); s.decor=new List<Decor>(); Center(s,true);
        }
        static bool Straight(MatchState s,Cell c)
        {
            bool Free(Cell x) => Board.Contains(s,x)&&s.At(x).terrain==Terrain.Livre;
            int n=Board.Dirs.Count(d=>Free(c+d));
            return n==2&&(Free(c+new Cell(1,0))&&Free(c+new Cell(-1,0))||Free(c+new Cell(0,1))&&Free(c+new Cell(0,-1)));
        }
        // Uma vela só, fora das casas de início, na casa mais justa: a menor diferença de caminho entre os lados opostos de
        // todos os pares de times (empate: sorteio).
        static void Candle(MatchState s)
        {
            var fields=Enumerable.Range(0,s.players).Select(slot=>Board.Costs(s,new Fighter{ def="ciantia", cell=Spawns(s,slot)[0], hp=1 },true,true)).ToList();
            int Gap(Cell c) { int i=Board.Index(s,c), g=0; for(int a=0;a+1<s.players;a+=2) { if(fields[a][i]==int.MaxValue||fields[a+1][i]==int.MaxValue) return int.MaxValue; g+=Math.Abs(fields[a][i]-fields[a+1][i]); } return g; }
            var options=Enumerable.Range(0,s.tiles.Length).Select(i=>new Cell(i%s.width,i/s.width)).Where(c=>Free(s,c)&&Away(s,c,3)).Select(c=>(c,g:Gap(c))).Where(x=>x.g!=int.MaxValue).ToList();
            foreach(var group in options.GroupBy(x=>x.g).OrderBy(g=>g.Key)) {
                var list=group.Select(x=>x.c).ToList();
                while(list.Count>0) {
                    var c=list[Dice.Range(s,list.Count)]; list.Remove(c);
                    s.At(c).terrain=Terrain.Caixa; s.At(c).Obj=TileObj.Vela;
                    if(SpawnsConnected(s)) return;
                    s.At(c).terrain=Terrain.Livre; s.At(c).obj=0;
                }
            }
        }
        static void Open(MatchState s,Cell c) { if(Board.Contains(s,c)) s.At(c).terrain=Terrain.Livre; }
        static void Corridor(MatchState s,Cell a,Cell b,int width,bool xFirst)
        {
            var corner=xFirst?new Cell(b.x,a.z):new Cell(a.x,b.z);
            foreach(var (p,q) in new[]{(a,corner),(corner,b)}) {
                int sx=Math.Sign(q.x-p.x), sz=Math.Sign(q.z-p.z); var c=p;
                while(true) {
                    Open(s,c); if(width>1) Open(s,c+(sx!=0?new Cell(0,1):new Cell(1,0)));
                    if(c==q) break; c+=new Cell(sx,sz);
                }
            }
        }
        // Casa livre no meio de uma sala (3+ vizinhas livres), fora da zona e da loja.
        static bool Room(MatchState s,Cell c) => Board.Dirs.Count(d=>Board.Contains(s,c+d)&&s.At(c+d).terrain==Terrain.Livre)>=3;
        // Longe (mais de dist casas) de todas as casas de início e fora da zona.
        static bool Away(MatchState s,Cell c,int dist)
        {
            if(s.At(c).zone||Board.NextToShop(s,c)) return false;
            for(int slot=0;slot<s.players;slot++) foreach(var sp in Spawns(s,slot,6)) if(sp.Distance(c)<=dist) return false;
            return true;
        }
        // Até count casas livres sorteadas que passam no filtro; desfaz a que cortar o caminho.
        // pair: também na casa do lado oposto (giro de 180°).
        static void Place(MatchState s,int count,Func<Cell,bool> ok,Action<Tile> set,bool pair=false)
        {
            for(int placed=0,tries=0;placed<count&&tries<Math.Max(120,count*40);tries++) {
                var c=RandomAll(s); var cells=pair?new[]{c,s.Mirror(c)}:new[]{c};
                if(!Free(s,c)||!ok(c)) continue;
                var before=cells.Select(x=>(s.At(x).terrain,s.At(x).obj)).ToArray();
                foreach(var x in cells) set(s.At(x));
                if(SpawnsConnected(s)) { placed++; continue; }
                for(int i=0;i<cells.Length;i++) { s.At(cells[i]).terrain=before[i].terrain; s.At(cells[i]).obj=before[i].obj; }
            }
        }
        // Enfeites pequenos (props do DungeonBDX, "dg_<id>") encostados nas paredes; só visual.
        public static readonly string[][] ClosedDecor={ new[]{"dg_bones","dg_candles","dg_mushrooms","dg_weapons"}, new[]{"dg_candles","dg_weapons","dg_bones","dg_candles"}, new[]{"dg_bush","dg_mushrooms","dg_bush","dg_candles"} };
        static void DecorateClosed(MatchState s,uint seed)
        {
            s.decor.RemoveAll(d=>!Kept(d)); var rng=new System.Random((int)(seed%int.MaxValue)); int count=s.width*s.height/20, placed=0;
            var pieces=ClosedDecor[(s.theme-OpenThemes)%ClosedDecor.Length];
            for(int tries=0;tries<count*8&&placed<count;tries++)
            {
                var c=new Cell(rng.Next(s.width),rng.Next(s.height));
                if(s.At(c).terrain!=Terrain.Livre||s.At(c).zone||s.At(c).obj!=0||HasOrnament(s,c)) continue;
                var wall=Board.Dirs.Where(d=>Board.Contains(s,c+d)&&s.At(c+d).terrain==Terrain.Parede).Cast<Cell?>().FirstOrDefault();
                if(wall==null) continue;
                string piece=pieces[rng.Next(pieces.Length)];
                s.decor.Add(new Decor{ piece=piece, x=c.x+wall.Value.x*.32f, z=c.z+wall.Value.z*.32f, scale=.8f }); placed++;
            }
        }
    }
}
