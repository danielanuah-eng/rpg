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
        // Planejamento usa apenas ocupantes conhecidos por quem está andando.
        public static Fighter KnownAt(MatchState s,Cell c,Fighter viewer)
        {
            var f=At(s,c); return f!=null&&f.hidden&&!s.Ally(f.team,viewer.team)?null:f;
        }
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
                    if(cost<0||(!ignoreFighters&&KnownAt(s,nb,f)!=null)) continue;
                    int i=Index(s,nb); if(dist[best]+cost<dist[i]) dist[i]=dist[best]+cost;
                }
            }
            return dist;
        }
        // Risco de Trava ao SAIR de c: inimigos conhecidos, soltos e que travam, colados nela.
        public static int LockRisk(MatchState s,Fighter f,Cell c)
        {
            int n=0;
            foreach(var e in s.fighters) if(e.Alive&&e!=f&&!s.Ally(e.team,f.team)&&!(e.hidden)&&e.prisoned==0&&!Content.Def(e).noLock&&e.cell.Distance(c)==1) n++;
            return n;
        }
        // Caminho (sprint 1, 2.1): dentro dos PM, primeiro o que passa por menos casas de Trava (cada inimigo colado conta 1;
        // a casa de saída e a de chegada não contam, pois não dá para evitá-las); empate, o mais barato em PM.
        // Rótulos por (casa, PM gastos): exato com orçamento, e o mapa máximo (18×18 × PM) é pequeno.
        public static List<Cell> Path(MatchState s,Fighter f,Cell to,int budget)
        {
            if(!Contains(s,to)||to==f.cell||budget<=0) return null;
            bool flying=Content.Def(f).flying; int n=s.width*s.height, b=budget+1, start=Index(s,f.cell), goal=Index(s,to);
            var risk=new int[n*b]; var prev=new int[n*b];
            for(int i=0;i<risk.Length;i++) { risk[i]=int.MaxValue; prev[i]=-1; }
            risk[start*b]=0;
            var lockAt=new int[n]; for(int i=0;i<n;i++) lockAt[i]=-1;
            int Lock(int i) { if(lockAt[i]<0) lockAt[i]=LockRisk(s,f,new Cell(i%s.width,i/s.width)); return lockAt[i]; }
            // PM só cresce: varrer por PM gasto é uma ordem topológica.
            for(int k=0;k<=budget;k++) for(int i=0;i<n;i++)
            {
                int r=risk[i*b+k]; if(r==int.MaxValue||i==goal) continue;
                var c=new Cell(i%s.width,i/s.width); int leave=i==start?0:Lock(i);
                foreach(var d in Dirs)
                {
                    var nb=c+d; int cost=StepCost(s,c,nb,flying,f.climber);
                    if(cost<=0||k+cost>budget||KnownAt(s,nb,f)!=null) continue;
                    int j=Index(s,nb)*b+k+cost;
                    if(r+leave<risk[j]) { risk[j]=r+leave; prev[j]=i*b+k; }
                }
            }
            int best=-1;
            for(int k=0;k<=budget;k++) { int j=goal*b+k; if(risk[j]!=int.MaxValue&&(best<0||risk[j]<risk[best])) best=j; }
            if(best<0) return null;
            var path=new List<Cell>();
            for(int j=best;j>=0;j=prev[j]) { int i=j/b; path.Add(new Cell(i%s.width,i/s.width)); }
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
