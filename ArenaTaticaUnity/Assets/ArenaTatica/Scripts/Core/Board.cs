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
        // Labirinto (autor, 18/10 noite): a área do 12×12 vezes 2× os jogadores, lado par para a loja ficar no meio
        // (2 → 24, 3 → 30, 4 → 34, 5 → 38, 6 → 42).
        public static int LabyrinthSize(int players) => 2*(int)Math.Round(6*Math.Sqrt(2*Math.Max(2,players)));
        public const int MaxSize=42;
        public static bool ValidSize(int size) => size>=Size&&size<=MaxSize&&size%2==0;
        public static readonly Cell[] Dirs={ new Cell(-1,0), new Cell(1,0), new Cell(0,-1), new Cell(0,1) };
        public static bool Contains(MatchState s,Cell c) => c.x>=0&&c.z>=0&&c.x<s.width&&c.z<s.height;
        // Casa encostada (lado, sem diagonal) numa casa da loja: só dali se compra.
        public static bool NextToShop(MatchState s,Cell c) { foreach(var d in Dirs) if(Contains(s,c+d)&&s.At(c+d).terrain==Terrain.Loja) return true; return false; }
        // Lutador grande compra, aquece no braseiro e acende a vela de qualquer casa dele.
        public static bool NextToShop(MatchState s,Fighter f) { foreach(var c in Footprint(f)) if(NextToShop(s,c)) return true; return false; }
        public static Fighter At(MatchState s,Cell c) => s.fighters.Find(f=>f.Alive&&Occupies(f,c));
        // ── Lutador grande (FighterDef.size, autor 18/10: Primal Beast em 2×2) ──
        // f.cell é o canto de menor x e z; ele ocupa size×size casas, bate e é atingido por qualquer uma delas.
        public static int SizeOf(Fighter f) => Math.Max(1,Content.Def(f).size);
        public static IEnumerable<Cell> Footprint(Fighter f,Cell at) { int n=SizeOf(f); for(int x=0;x<n;x++) for(int z=0;z<n;z++) yield return at+new Cell(x,z); }
        public static IEnumerable<Cell> Footprint(Fighter f) => Footprint(f,f.cell);
        public static bool Occupies(Fighter f,Cell c)
        {
            if(f.cell.x<0) return false; int n=SizeOf(f);
            return c.x>=f.cell.x&&c.z>=f.cell.z&&c.x<f.cell.x+n&&c.z<f.cell.z+n;
        }
        // Distância (em casas, sem diagonal) entre dois quadrados: canto a com lado na e canto b com lado nb.
        static int Gap(Cell a,int na,Cell b,int nb) => Math.Max(0,Math.Max(a.x-(b.x+nb-1),b.x-(a.x+na-1)))+Math.Max(0,Math.Max(a.z-(b.z+nb-1),b.z-(a.z+na-1)));
        public static int Gap(Fighter f,Cell c) => Gap(f.cell,SizeOf(f),c,1);
        public static int Gap(Fighter a,Fighter b) => Gap(a.cell,SizeOf(a),b.cell,SizeOf(b));
        // Colados, diagonal incluída (névoa): os quadrados dos dois ficam a no máximo 1 casa em x e em z.
        public static bool Touch(Fighter a,Fighter b)
        {
            int na=SizeOf(a), nb=SizeOf(b);
            int dx=Math.Max(0,Math.Max(a.cell.x-(b.cell.x+nb-1),b.cell.x-(a.cell.x+na-1))), dz=Math.Max(0,Math.Max(a.cell.z-(b.cell.z+nb-1),b.cell.z-(a.cell.z+na-1)));
            return dx<=1&&dz<=1;
        }
        // Casa do lutador mais perto de c (de onde ele mira e mede a linha).
        public static Cell Nearest(Fighter f,Cell c) { int n=SizeOf(f)-1; return new Cell(Math.Max(f.cell.x,Math.Min(c.x,f.cell.x+n)),Math.Max(f.cell.z,Math.Min(c.z,f.cell.z+n))); }
        // O lutador cabe inteiro com o canto em at: todas as casas no mapa, de chão e sem outro lutador.
        public static bool Fits(MatchState s,Fighter f,Cell at)
        {
            foreach(var c in Footprint(f,at)) { if(!Contains(s,c)||!Walkable(s,c,Content.Def(f).flying)) return false; var o=At(s,c); if(o!=null&&o!=f) return false; }
            return true;
        }
        // Passo do lutador inteiro de a para b: cada casa dele faz o mesmo passo; vale o mais caro. -1 = não passa.
        public static int Step(MatchState s,Fighter f,Cell a,Cell b,bool flying,bool climber=false,bool doors=false)
        {
            // Esfera do Nepturius (autor 23/10): nada no jogo a afeta, então água, altura e gelo não custam a mais.
            if(Content.Def(f).luna?.invulnerable??false) return Contains(s,b)&&Walkable(s,b,false)?1:-1;
            int n=SizeOf(f); if(n==1) return StepCost(s,a,b,flying,climber,doors);
            // Gigante (autor 24/10, Primal Beast nas Docas): só paga a água se o corpo inteiro cair nela.
            int worst=-1; bool dry=false;
            for(int x=0;x<n;x++) for(int z=0;z<n;z++) { var o=new Cell(x,z); if(Contains(s,b+o)&&s.At(b+o).terrain!=Terrain.Agua) dry=true; }
            for(int x=0;x<n;x++) for(int z=0;z<n;z++) { var o=new Cell(x,z); int c=StepCost(s,a+o,b+o,flying,climber,doors); if(c<0) return -1; if(dry&&s.At(b+o).terrain==Terrain.Agua) c-=WaterCost-1; worst=Math.Max(worst,c); }
            return worst;
        }
        // Outro lutador conhecido em alguma casa que f ocuparia com o canto em at.
        static bool Crowded(MatchState s,Fighter f,Cell at)
        {
            foreach(var c in Footprint(f,at)) { var o=KnownAt(s,c,f); if(o!=null&&o!=f&&!PassThrough(s,o)) return true; }
            return false;
        }
        // Ilusão atravessável (MatchState.solidIllusions desligado): não bloqueia o caminho, mas ninguém para em cima dela.
        public static bool PassThrough(MatchState s,Fighter o) => !s.solidIllusions&&!string.IsNullOrEmpty(o.disguise);
        // Planejamento usa apenas ocupantes conhecidos por quem está andando (escondidos e os da névoa não contam).
        public static Fighter KnownAt(MatchState s,Cell c,Fighter viewer)
        {
            var f=At(s,c); return f!=null&&!s.Ally(f.team,viewer.team)&&(f.hidden||!Match.Sees(s,viewer.team,f))?null:f;
        }
        // Água (Docas inundadas, expansão de 22/10) é caminhável para todos; o custo é que muda (StepCost).
        public static bool Walkable(MatchState s,Cell c,bool flying) { var t=s.At(c).terrain; return t==Terrain.Livre||t==Terrain.Agua||(flying&&t==Terrain.Arbusto); }
        // Água custa o dobro de um passo normal: metade do deslocamento.
        public const int WaterCost=2;
        // Gancho de escalada (climber): subir 2 custa 1 e dá para subir 3 (2 PM). Casa congelada: +1 PM (voador não sente).
        // doors: conta a porta fechada como passagem (+1 PM de abrir); só para medir caminhos (gerador, bot, boss), nunca para andar.
        public static int StepCost(MatchState s,Cell a,Cell b,bool flying,bool climber=false,bool doors=false)
        {
            if(!Contains(s,b)) return -1;
            int door=doors&&s.At(b).terrain==Terrain.Porta?1:0;
            if(door==0&&!Walkable(s,b,flying)) return -1;
            if(flying) return 1+door+(s.At(b).terrain==Terrain.Agua?WaterCost-1:0);
            int water=s.At(b).terrain==Terrain.Agua?WaterCost-1:0;
            int up=s.At(b).height-s.At(a).height, ice=s.At(b).frozenBy!=0?1:0;
            if(climber) return up>=4?-1:(up==3?2:1)+ice+door+water;
            return up>=3?-1:(up==2?2:1)+ice+door+water;
        }
        // Passo com o orçamento que sobra (autor, 22/10): a água custa 2, mas quem tem só 1 unidade ainda faz UMA primeira
        // casa de água. A unidade é consumida inteira: nunca sobra passo seco de graça. -1 = não dá.
        public static int BudgetStep(MatchState s,Fighter f,Cell a,Cell b,int remaining,bool flying)
        {
            int cost=Step(s,f,a,b,flying,f.climber);
            if(cost<=0) return -1;
            if(cost<=remaining) return cost;
            if(remaining==1&&cost==WaterCost&&s.At(b).terrain==Terrain.Agua&&a==f.cell) return 1;
            return -1;
        }
        // Custo mínimo em PM até cada casa (Dijkstra com fila de prioridade: o labirinto de 42×42 tem 1764 casas);
        // int.MaxValue = inalcançável. Lutadores são obstáculos.
        public static int[] Costs(MatchState s,Fighter f,bool ignoreFighters=false,bool throughDoors=false)
        {
            bool flying=Content.Def(f).flying, climber=f.climber;
            int n=s.width*s.height; var dist=new int[n]; var done=new bool[n];
            for(int i=0;i<n;i++) dist[i]=int.MaxValue;
            int start=Index(s,f.cell); if(start<0||start>=n) return dist;
            dist[start]=0;
            var heap=new List<(int d,int i)>{(0,start)};
            while(heap.Count>0)
            {
                var (d0,best)=Pop(heap);
                if(done[best]||d0>dist[best]) continue;
                done[best]=true; var c=new Cell(best%s.width,best/s.width);
                foreach(var d in Dirs)
                {
                    var nb=c+d; int cost=Step(s,f,c,nb,flying,climber,throughDoors);
                    if(cost<0||(!ignoreFighters&&Crowded(s,f,nb))) continue;
                    int i=Index(s,nb); if(dist[best]+cost<dist[i]) { dist[i]=dist[best]+cost; Push(heap,(dist[i],i)); }
                }
            }
            return dist;
        }
        // Fila de prioridade mínima (heap binário) por distância e, no empate, pela casa.
        static bool Less((int d,int i) a,(int d,int i) b) => a.d<b.d||a.d==b.d&&a.i<b.i;
        static void Push(List<(int d,int i)> h,(int d,int i) x)
        {
            h.Add(x); int k=h.Count-1;
            while(k>0) { int p=(k-1)/2; if(!Less(h[k],h[p])) break; (h[k],h[p])=(h[p],h[k]); k=p; }
        }
        static (int d,int i) Pop(List<(int d,int i)> h)
        {
            var top=h[0]; h[0]=h[h.Count-1]; h.RemoveAt(h.Count-1); int k=0;
            while(true) { int l=2*k+1, r=l+1, m=k; if(l<h.Count&&Less(h[l],h[m])) m=l; if(r<h.Count&&Less(h[r],h[m])) m=r; if(m==k) break; (h[k],h[m])=(h[m],h[k]); k=m; }
            return top;
        }
        // Trava: herói sem noLock. A ilusão disfarçada trava como a original (autor 25/10: ninguém distingue as duas).
        public static bool Locks(Fighter e) => !string.IsNullOrEmpty(e.disguise)&&e.summoner!=0&&Content.Fighters.TryGetValue(e.disguise,out var d)?!d.noLock:e.summoner==0&&!e.noLock&&!Content.Def(e).noLock;
        // Risco de Trava ao SAIR de c: inimigos conhecidos, soltos e que travam, colados nela.
        public static int LockRisk(MatchState s,Fighter f,Cell c)
        {
            int n=0;
            foreach(var e in s.fighters) if(e.Alive&&e!=f&&!s.Ally(e.team,f.team)&&!(e.hidden)&&e.prisoned==0&&Locks(e)&&Gap(e.cell,SizeOf(e),c,SizeOf(f))==1) n++;
            return n;
        }
        // Caminho (sprint 1, 2.1): dentro dos PM, primeiro o que passa por menos casas de Trava (cada inimigo colado conta 1;
        // a casa de saída e a de chegada não contam, pois não dá para evitá-las); empate, o mais barato em PM.
        // Rótulos por (casa, PM gastos): exato com orçamento, e o mapa máximo (18×18 × PM) é pequeno.
        public static List<Cell> Path(MatchState s,Fighter f,Cell to,int budget)
        {
            if(!Contains(s,to)||to==f.cell||budget<=0) return null;
            foreach(var c in Footprint(f,to)) if(At(s,c) is Fighter g&&g!=f&&PassThrough(s,g)) return null;
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
                    var nb=c+d; int cost=BudgetStep(s,f,c,nb,budget-k,flying);
                    if(cost<=0||Crowded(s,f,nb)) continue;
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
        // Supercover como no port Godot. Bloqueiam: árvore, caixa, parede, lutador e relevo 2+ acima do mais baixo dos dois.
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
        // overWalls: salto e teleporte miram por cima do muro (pagando Match.WallJump); o resto ainda tapa.
        public static bool LineOfSight(MatchState s,Cell a,Cell b,bool overWalls=false)
        {
            if(a==b||!Contains(s,a)||!Contains(s,b)) return false;
            int low=Math.Min(s.At(a).height,s.At(b).height);
            bool Blocked(Cell c) { if(c==b) return false; if(!Contains(s,c)) return true; var t=s.At(c); return t.terrain==Terrain.Arvore||t.terrain==Terrain.Caixa||t.terrain==Terrain.Loja||!overWalls&&(t.terrain==Terrain.Parede||t.terrain==Terrain.Porta)||t.height>=low+2; }
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
        // Muro (parede ou porta fechada) entre a e b, no mesmo traçado da linha de visão: tiro à distância não passa
        // (autor, 18/10 noite) nem que a skill ignore a visão; vale também para a área da explosão.
        public static bool WallClear(MatchState s,Cell a,Cell b)
        {
            if(a==b) return true; if(!Contains(s,a)||!Contains(s,b)) return false;
            bool Wall(Cell c) { if(c==b) return false; if(!Contains(s,c)) return true; var t=s.At(c).terrain; return t==Terrain.Parede||t==Terrain.Porta; }
            int nx=Math.Abs(b.x-a.x), nz=Math.Abs(b.z-a.z), sx=Math.Sign(b.x-a.x), sz=Math.Sign(b.z-a.z), ix=0, iz=0; var cur=a;
            while(ix<nx||iz<nz)
            {
                int h=(2*ix+1)*nz, v=(2*iz+1)*nx;
                if(h==v) { if(Wall(cur+new Cell(sx,0))||Wall(cur+new Cell(0,sz))) return false; cur+=new Cell(sx,sz); ix++; iz++; }
                else if(h<v) { cur+=new Cell(sx,0); ix++; }
                else { cur+=new Cell(0,sz); iz++; }
                if(Wall(cur)) return false;
            }
            return true;
        }
        // Algum muro no caminho em linha reta entre as casas (salto e teleporte por cima do muro custam +2 PA).
        public static bool OverWall(MatchState s,Cell a,Cell b) => !WallClear(s,a,b);
        // Todos os lutadores terrestres vivos ainda se alcançam (ignorando lutadores como obstáculo)?
        public static bool Connected(MatchState s)
        {
            var ground=s.fighters.FindAll(f=>f.Alive&&!Content.Def(f).flying);
            if(ground.Count<2) return true;
            var dist=Costs(s,ground[0],true,true);
            return ground.TrueForAll(f=>dist[Index(s,f.cell)]!=int.MaxValue);
        }
    }
}
