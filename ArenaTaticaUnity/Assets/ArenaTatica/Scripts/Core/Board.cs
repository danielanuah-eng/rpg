using System;
using System.Collections.Generic;

namespace ArenaTatica.Core
{
    // Relevo (decisão A): subir 1 nível custa 1 PM, subir 2 custa 2 PM, 3+ bloqueia; descer é livre.
    // Voadores ignoram relevo e arbustos. Quem está mais alto ganha +1 de alcance em skills modificáveis.
    public static class Board
    {
        public const int MaxHeight=3, Size=12;
        // O mapa cresce com os jogadores: 2 → 12×12, 3–4 → 16×16, 5–6 → 18×18.
        public static int SizeFor(int players) => players<=2?12:players<=4?16:18;
        public static bool ValidSize(int size) => size==12||size==16||size==18;
        public static readonly Cell[] Dirs={ new Cell(-1,0), new Cell(1,0), new Cell(0,-1), new Cell(0,1) };
        public static bool Contains(MatchState s,Cell c) => c.x>=0&&c.z>=0&&c.x<s.width&&c.z<s.height;
        // Casa encostada (lado, sem diagonal) numa casa da loja: só dali se compra.
        public static bool NextToShop(MatchState s,Cell c) { foreach(var d in Dirs) if(Contains(s,c+d)&&s.At(c+d).terrain==Terrain.Loja) return true; return false; }
        public static Fighter At(MatchState s,Cell c) => s.fighters.Find(f=>f.Alive&&f.cell==c);
        public static bool Walkable(MatchState s,Cell c,bool flying) { var t=s.At(c).terrain; return t==Terrain.Livre||(flying&&t==Terrain.Arbusto); }
        // Gancho de escalada (climber): subir 2 custa 1 e dá para subir 3 (2 PM). Casa congelada: +1 PM (voador não sente).
        public static int StepCost(MatchState s,Cell a,Cell b,bool flying,bool climber=false)
        {
            if(!Contains(s,b)||!Walkable(s,b,flying)) return -1;
            if(flying) return 1;
            int up=s.At(b).height-s.At(a).height, ice=s.At(b).frozenBy!=0?1:0;
            if(climber) return up>=4?-1:(up==3?2:1)+ice;
            return up>=3?-1:(up==2?2:1)+ice;
        }
        // Custo mínimo em PM até cada casa (Dijkstra); int.MaxValue = inalcançável. Lutadores são obstáculos.
        public static int[] Costs(MatchState s,Fighter f,bool ignoreFighters=false)
        {
            bool flying=Content.Def(f).flying, climber=f.climber;
            int n=s.width*s.height; var dist=new int[n]; var done=new bool[n];
            for(int i=0;i<n;i++) dist[i]=int.MaxValue;
            dist[Index(s,f.cell)]=0;
            while(true)
            {
                int best=-1;
                for(int i=0;i<n;i++) if(!done[i]&&dist[i]!=int.MaxValue&&(best<0||dist[i]<dist[best])) best=i;
                if(best<0) break;
                done[best]=true; var c=new Cell(best%s.width,best/s.width);
                foreach(var d in Dirs)
                {
                    var nb=c+d; int cost=StepCost(s,c,nb,flying,climber);
                    if(cost<0||(!ignoreFighters&&At(s,nb)!=null)) continue;
                    int i=Index(s,nb); if(dist[best]+cost<dist[i]) dist[i]=dist[best]+cost;
                }
            }
            return dist;
        }
        public static List<Cell> Path(MatchState s,Fighter f,Cell to,int budget)
        {
            if(!Contains(s,to)||to==f.cell) return null;
            var dist=Costs(s,f); bool flying=Content.Def(f).flying;
            if(dist[Index(s,to)]>budget) return null;
            var path=new List<Cell>{to}; var c=to;
            while(c!=f.cell)
            {
                Cell prev=c; bool found=false;
                foreach(var d in Dirs)
                {
                    var p=c+d; if(!Contains(s,p)||dist[Index(s,p)]==int.MaxValue) continue;
                    int cost=StepCost(s,p,c,flying,f.climber);
                    if(cost>0&&dist[Index(s,p)]+cost==dist[Index(s,c)]) { prev=p; found=true; break; }
                }
                if(!found) return null;
                c=prev; path.Add(c);
            }
            path.Reverse(); return path;
        }
        public static int Index(MatchState s,Cell c) => c.z*s.width+c.x;
        // Supercover como no port Godot. Bloqueiam: árvore, caixa, lutador e relevo 2+ acima do mais baixo dos dois.
        // Casas entre a e b (sem as pontas), no mesmo traçado da linha de visão. Lutadores não bloqueiam a visão
        // (decisão do autor, 10/10): um tiro que passa por cima de alguém pode acertá-lo (Match.Intercept).
        public static List<Cell> Between(MatchState s,Cell a,Cell b)
        {
            var cells=new List<Cell>(); int nx=Math.Abs(b.x-a.x), nz=Math.Abs(b.z-a.z), sx=Math.Sign(b.x-a.x), sz=Math.Sign(b.z-a.z), ix=0, iz=0; var cur=a;
            while(ix<nx||iz<nz)
            {
                int h=(2*ix+1)*nz, v=(2*iz+1)*nx;
                if(h==v) { cur+=new Cell(sx,sz); ix++; iz++; } else if(h<v) { cur+=new Cell(sx,0); ix++; } else { cur+=new Cell(0,sz); iz++; }
                if(cur!=b) cells.Add(cur);
            }
            return cells;
        }
        public static bool LineOfSight(MatchState s,Cell a,Cell b)
        {
            if(a==b||!Contains(s,a)||!Contains(s,b)) return false;
            int low=Math.Min(s.At(a).height,s.At(b).height);
            bool Blocked(Cell c) { if(c==b) return false; if(!Contains(s,c)) return true; var t=s.At(c); return t.terrain==Terrain.Arvore||t.terrain==Terrain.Caixa||t.terrain==Terrain.Loja||t.height>=low+2; }
            int nx=Math.Abs(b.x-a.x), nz=Math.Abs(b.z-a.z), sx=Math.Sign(b.x-a.x), sz=Math.Sign(b.z-a.z), ix=0, iz=0;
            var cur=a;
            while(ix<nx||iz<nz)
            {
                int h=(2*ix+1)*nz, v=(2*iz+1)*nx;
                if(h==v) { if(Blocked(cur+new Cell(sx,0))||Blocked(cur+new Cell(0,sz))) return false; cur+=new Cell(sx,sz); ix++; iz++; }
                else if(h<v) { cur+=new Cell(sx,0); ix++; }
                else { cur+=new Cell(0,sz); iz++; }
                if(Blocked(cur)) return false;
            }
            return true;
        }
        // Todos os lutadores terrestres vivos ainda se alcançam (ignorando lutadores como obstáculo)?
        public static bool Connected(MatchState s)
        {
            var ground=s.fighters.FindAll(f=>f.Alive&&!Content.Def(f).flying);
            if(ground.Count<2) return true;
            var dist=Costs(s,ground[0],true);
            return ground.TrueForAll(f=>dist[Index(s,f.cell)]!=int.MaxValue);
        }
    }
}
