using System;
using System.Collections.Generic;
using System.Linq;
using DungeonBDX.Skills;

namespace DungeonBDX
{
    public static class DungeonTactics
    {
        // Shortest legal path to a hostile neighbour, allowing detours away from the target.
        public static GridCell? Advance(DungeonSkillSession session,ISkillBoard board)
        {
            var actor=session.Unit(session.ActiveActorId);if(actor==null)return null;
            var enemies=session.Units.Where(u=>u.Alive && u.Team!=actor.Team).ToArray();
            var occupied=new HashSet<GridCell>(session.Units.Where(u=>u.Alive && u.Id!=actor.Id).Select(u=>u.Cell));
            var previous=new Dictionary<GridCell,GridCell>();var queue=new Queue<GridCell>();
            queue.Enqueue(actor.Cell);previous.Add(actor.Cell,actor.Cell);GridCell? goal=null;
            int[] dx={0,1,0,-1},dy={1,0,-1,0};
            while(queue.Count>0 && previous.Count<=16384)
            {
                var cell=queue.Dequeue();
                if(enemies.Any(e=>GridCell.Distance(cell,e.Cell)==1)){goal=cell;break;}
                for(int i=0;i<4;i++)
                {
                    long x=(long)cell.X+dx[i],y=(long)cell.Y+dy[i];
                    if(x<int.MinValue || x>int.MaxValue || y<int.MinValue || y>int.MaxValue)continue;
                    var next=new GridCell((int)x,(int)y);
                    if(previous.ContainsKey(next) || occupied.Contains(next) || !board.CanStand(next))continue;
                    previous.Add(next,cell);queue.Enqueue(next);
                }
            }
            if(!goal.HasValue || goal.Value.Equals(actor.Cell))return null;
            var path=new List<GridCell>();var cursor=goal.Value;
            while(!cursor.Equals(actor.Cell)){path.Add(cursor);cursor=previous[cursor];}
            path.Reverse();int steps=Math.Max(1,DungeonProfiles.Resolve(actor.ProfileId).Movement-actor.Slow);
            return path[Math.Min(steps,path.Count)-1];
        }
    }
}
