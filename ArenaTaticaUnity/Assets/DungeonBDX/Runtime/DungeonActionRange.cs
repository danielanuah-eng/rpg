using System;
using System.Collections.Generic;
using System.Linq;
using DungeonBDX.Skills;
using DungeonBDX.World;

namespace DungeonBDX
{
    public sealed class DungeonRangeCell
    {
        public GridCell Cell { get; }
        public int Distance { get; }
        public bool CanTarget { get; }
        internal DungeonRangeCell(GridCell cell,int distance,bool target) { Cell=cell;Distance=distance;CanTarget=target; }
    }

    // Presentation query only. The session still validates every command and its cooldown.
    public sealed class DungeonActionPreview
    {
        public GridCell Origin { get; }
        public bool Movement { get; }
        public string SkillId { get; }
        public IReadOnlyList<DungeonRangeCell> Cells { get; }
        public IReadOnlyList<string> TargetIds { get; }
        internal DungeonActionPreview(GridCell origin,bool movement,string id,List<DungeonRangeCell> cells,List<string> targets)
        { Origin=origin;Movement=movement;SkillId=id;Cells=cells.AsReadOnly();TargetIds=targets.AsReadOnly(); }
    }

    public static class DungeonActionRange
    {
        static readonly GridCell[] Directions = { new GridCell(1,0),new GridCell(0,1),new GridCell(-1,0),new GridCell(0,-1) };

        public static GridCell Origin(DungeonSkillSession session,UnitSnapshot actor,DungeonSkill skill)
        {
            var pet=skill.RequiresPet && skill.Has(SkillEffect.Attack) ? session.Unit(session.PetId(actor.Id)) : null;
            return pet?.Cell ?? actor.Cell;
        }

        public static bool EligibleTarget(DungeonSkillSession session,ISkillBoard board,UnitSnapshot actor,DungeonSkill skill,
            UnitSnapshot target,SkillMechanics mechanics)
        {
            if(target==null || !target.Alive)return false;
            if(skill.Target==SkillTarget.Ally && target.Team!=actor.Team ||
                skill.Target==SkillTarget.Enemy && target.Team==actor.Team && (mechanics&SkillMechanics.FriendlyFire)==0)return false;
            if(skill.RequiresPet && session.PetId(actor.Id)==null)return false;
            var origin=Origin(session,actor,skill);int distance=GridCell.Distance(origin,target.Cell);
            return distance>=skill.MinRange && distance<=skill.Range && board.HasLineOfSight(origin,target.Cell);
        }

        public static DungeonActionPreview Compute(DungeonSkillSession session,DungeonMap board,bool movement,string skillId=null,
            SkillMechanics mechanics=SkillMechanics.Default)
        {
            if(session==null || board==null)throw new ArgumentNullException();
            var actor=session.Unit(session.ActiveActorId);
            var cells=new List<DungeonRangeCell>();var targets=new List<string>();
            var origin=actor?.Cell ?? default;
            DungeonActionPreview Result()=>new DungeonActionPreview(origin,movement,skillId,cells,targets);
            if(actor==null || !actor.Alive || session.Finished)return Result();
            var occupied=new HashSet<GridCell>(session.Units.Where(u=>u.Alive && u.Id!=actor.Id).Select(u=>u.Cell));
            if(movement)
            {
                if(session.RemainingActions<1)return Result();
                int steps=Math.Max(1,DungeonProfiles.Resolve(actor.ProfileId).Movement-actor.Slow);
                foreach(var entry in Walk(board,actor.Cell,occupied,steps).OrderBy(e=>e.Key.Y).ThenBy(e=>e.Key.X))
                    if(!entry.Key.Equals(actor.Cell))cells.Add(new DungeonRangeCell(entry.Key,entry.Value,true));
                return Result();
            }
            if(!DungeonSkillCatalog.TryGet(skillId,out var skill) || session.RemainingActions<skill.Cost ||
                !DungeonProfiles.Resolve(actor.ProfileId).SkillIds.Contains(skillId))return Result();
            if((skill.Has(SkillEffect.Summon)||skill.RequiresPet) && (mechanics&SkillMechanics.Companions)==0 ||
                skill.Teleport && (mechanics&SkillMechanics.Teleport)==0 ||
                skill.RequiresPet && session.PetId(actor.Id)==null || skill.Has(SkillEffect.Summon) && session.PetId(actor.Id)!=null)return Result();
            origin=Origin(session,actor,skill);
            if(skill.Target==SkillTarget.Self)
            {cells.Add(new DungeonRangeCell(actor.Cell,0,true));targets.Add(actor.Id);return Result();}
            if(skill.Target==SkillTarget.Destination)
            {
                var walk=skill.Has(SkillEffect.Move) && !skill.Teleport ? Walk(board,actor.Cell,occupied,Math.Max(1,skill.Steps-actor.Slow)) : null;
                foreach(var cell in board.Floors)
                {
                    int distance=GridCell.Distance(actor.Cell,cell);
                    if(cell.Equals(actor.Cell)||occupied.Contains(cell)||distance<Math.Max(1,skill.MinRange)||distance>skill.Range)continue;
                    if(walk!=null && !walk.ContainsKey(cell))continue;
                    cells.Add(new DungeonRangeCell(cell,walk!=null?walk[cell]:distance,true));
                }
                return Result();
            }
            foreach(var target in session.Units)
                if(EligibleTarget(session,board,actor,skill,target,mechanics))targets.Add(target.Id);
            var legal=new HashSet<string>(targets,StringComparer.Ordinal);
            var byCell=session.Units.Where(u=>u.Alive).ToDictionary(u=>u.Cell,u=>u.Id);
            foreach(var cell in board.Floors)
            {
                int distance=GridCell.Distance(origin,cell);
                if(distance<skill.MinRange || distance>skill.Range || !board.HasLineOfSight(origin,cell))continue;
                cells.Add(new DungeonRangeCell(cell,distance,byCell.TryGetValue(cell,out var id) && legal.Contains(id)));
            }
            return Result();
        }

        static Dictionary<GridCell,int> Walk(ISkillBoard board,GridCell origin,HashSet<GridCell> occupied,int steps)
        {
            var distances=new Dictionary<GridCell,int>{{origin,0}};var queue=new Queue<GridCell>();queue.Enqueue(origin);
            while(queue.Count>0)
            {
                var cell=queue.Dequeue();int distance=distances[cell];if(distance>=steps)continue;
                foreach(var direction in Directions)
                {
                    var next=new GridCell(cell.X+direction.X,cell.Y+direction.Y);
                    if(!board.CanStand(next)||occupied.Contains(next)||distances.ContainsKey(next))continue;
                    distances.Add(next,distance+1);queue.Enqueue(next);
                }
            }
            return distances;
        }
    }
}
