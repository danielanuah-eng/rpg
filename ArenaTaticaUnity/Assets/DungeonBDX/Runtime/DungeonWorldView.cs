using System;
using System.Collections.Generic;
using System.Linq;
using DungeonBDX.Skills;
using DungeonBDX.World;
using UnityEngine;

namespace DungeonBDX
{
    public sealed class DungeonWorldView : MonoBehaviour
    {
        readonly Dictionary<string, Transform> actors = new Dictionary<string, Transform>();
        readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
        readonly Dictionary<string,GridCell> logicalCells = new Dictionary<string,GridCell>();
        readonly Dictionary<string,Queue<Vector3>> routes = new Dictionary<string,Queue<Vector3>>();
        readonly List<(GameObject visual, float expires)> effects = new List<(GameObject,float)>();
        DungeonMap map;
        DungeonSkillSession session;
        Transform marker;
        Transform architecture;
        public Camera ViewCamera { get; private set; }

        public void Build(DungeonMap plan, DungeonSkillSession game)
        {
            map = plan; session = game;
            architecture=new GameObject("Dungeon architecture").transform;architecture.SetParent(transform,false);
            var cameraObject = new GameObject("Expedition Camera", typeof(Camera));
            cameraObject.transform.SetParent(transform, false); ViewCamera = cameraObject.GetComponent<Camera>();
            ViewCamera.orthographic = true; ViewCamera.clearFlags = CameraClearFlags.SolidColor;
            ViewCamera.backgroundColor = new Color(.035f,.032f,.05f); ViewCamera.farClipPlane = 80;
            ViewCamera.nearClipPlane = .1f;
            var lightObject = new GameObject("Moonlight",typeof(Light)); lightObject.transform.SetParent(transform,false);
            var light = lightObject.GetComponent<Light>(); light.type = LightType.Directional;
            light.color = new Color(.74f,.8f,1); light.intensity = 1.6f; light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(55,-30,0);
            string floor = map.Scenario == DungeonMap.Library ? "floor-library" : map.Scenario == DungeonMap.Nest ? "floor-nest" : "floor-bronze";
            for (int x=0;x<map.Width;x++) for (int y=0;y<map.Height;y++)
            {
                var cell = new GridCell(x,y); var location = Position(cell);
                if (map.IsFloor(cell))
                {
                    var tile = Model(floor, location, Vector3.one);
                    if (!tile) Cube("Flagstone",location-new Vector3(0,.09f,0),new Vector3(.97f,.18f,.97f),
                        (x+y)%2==0 ? new Color(.22f,.19f,.23f) : new Color(.27f,.23f,.26f));
                }
                else if (map.Floors.Any(c=>GridCell.Distance(c,cell)==1))
                {
                    var wall = Model("wall-bronze",location,new Vector3(1,.45f,1));
                    if (!wall) Cube("Wall",location+Vector3.up*.22f,new Vector3(.96f,.44f,.96f),new Color(.15f,.13f,.18f));
                }
            }
            foreach (var prop in map.Props)
            {
                string name = prop.Kind == "book-pile" ? "bookcase" : prop.Kind == "eclipse-crystal" ? "nest-spire" : "column-bronze";
                var visual = Model(name,Position(prop.Cell),Vector3.one*.42f);
                if (visual) { visual.transform.Rotate(0,90*prop.RotationQuarterTurns,0); visual.transform.position += new Vector3(.3f,0,.3f); }
            }
            if(Application.isPlaying)StaticBatchingUtility.Combine(architecture.gameObject);
            marker = Primitive(PrimitiveType.Cylinder,"Turn ring",Vector3.zero,new Vector3(.78f,.025f,.78f),new Color(.87f,.62f,.24f)).transform;
            FitCamera(); Sync(true);
        }

        public static Vector3 Position(GridCell cell) => new Vector3(cell.X,0,cell.Y);

        public void FitCamera()
        {
            Frame(ViewCamera,map.Width,map.Height,Screen.safeArea,Screen.width,Screen.height);
        }

        public static void Frame(Camera camera,int mapWidth,int mapHeight,Rect safe,int width,int height)
        {
            float w=Mathf.Max(1,width),h=Mathf.Max(1,height),aspect=w/h;
            bool portrait=width<height;
            var usable=new Rect((safe.x+safe.width*.02f)/w,(safe.y+safe.height*(portrait?.40f:.32f))/h,
                safe.width*.96f/w,safe.height*(portrait?.47f:.55f)/h);
            var center=new Vector3((mapWidth-1)*.5f,.65f,(mapHeight-1)*.5f);
            camera.aspect=aspect;camera.transform.position=center+new Vector3(0,15,-9);camera.transform.LookAt(center);
            float halfHeight=mapHeight*.5f*Mathf.Abs(camera.transform.up.z)+.7f*Mathf.Abs(camera.transform.up.y);
            camera.orthographicSize=Mathf.Max(halfHeight/Mathf.Max(.1f,usable.height),mapWidth*.5f/aspect/Mathf.Max(.1f,usable.width));
            camera.transform.position-=camera.transform.right*((usable.center.x-.5f)*2*camera.orthographicSize*aspect)
                +camera.transform.up*((usable.center.y-.5f)*2*camera.orthographicSize);
        }

        public bool Pick(Vector2 screen, out GridCell cell)
        {
            // Elevated meshes/billboards must select their owner, rather than the floor
            // cell behind their head. Bases remain reliable in overlapping silhouettes.
            foreach(var unit in session.Units.Where(u=>u.Alive))
            {
                Vector2 foot=ViewCamera.WorldToScreenPoint(Position(unit.Cell));
                Vector2 edge=ViewCamera.WorldToScreenPoint(Position(unit.Cell)+ViewCamera.transform.right*.28f);
                if(Vector2.Distance(screen,foot)<=Mathf.Max(6,Vector2.Distance(foot,edge)))
                {cell=unit.Cell;return true;}
            }
            UnitSnapshot chosen=null;float nearest=float.MaxValue;
            foreach(var unit in session.Units.Where(u=>u.Alive))
            {
                if(!actors.TryGetValue(unit.Id,out var visual))continue;
                var renderers=visual.GetComponentsInChildren<Renderer>();if(renderers.Length==0)continue;
                var bounds=renderers[0].bounds;foreach(var renderer in renderers.Skip(1))bounds.Encapsulate(renderer.bounds);
                Vector2 min=new Vector2(float.MaxValue,float.MaxValue),max=new Vector2(float.MinValue,float.MinValue);
                for(int corner=0;corner<8;corner++)
                {
                    var point=ViewCamera.WorldToScreenPoint(bounds.center+Vector3.Scale(bounds.extents,
                        new Vector3((corner&1)==0?-1:1,(corner&2)==0?-1:1,(corner&4)==0?-1:1)));
                    min=Vector2.Min(min,point);max=Vector2.Max(max,point);
                }
                var rect=Rect.MinMaxRect(min.x,min.y,max.x,max.y);float screenDistance=(screen-rect.center).sqrMagnitude;
                if(rect.Contains(screen) && screenDistance<nearest){nearest=screenDistance;chosen=unit;}
            }
            if(chosen!=null){cell=chosen.Cell;return true;}
            var ray = ViewCamera.ScreenPointToRay(screen);
            if (new Plane(Vector3.up,Vector3.zero).Raycast(ray,out float distance))
            {
                var point = ray.GetPoint(distance); cell = new GridCell(Mathf.RoundToInt(point.x),Mathf.RoundToInt(point.z));
                return map.CanStand(cell);
            }
            cell = default; return false;
        }

        public void Sync(bool immediate = false,string teleportActor=null)
        {
            foreach (var unit in session.Units)
            {
                if (!actors.TryGetValue(unit.Id,out var visual))
                {
                    var root = new GameObject("Unit " + unit.Id); root.transform.SetParent(transform,false);
                    root.transform.position = Position(unit.Cell); visual = root.transform; actors.Add(unit.Id,visual);
                    logicalCells.Add(unit.Id,unit.Cell);routes.Add(unit.Id,new Queue<Vector3>());
                    string model = unit.IsSummon ? "companion-dragon" : unit.ProfileId;
                    var imported = Resources.Load<GameObject>("DungeonBDX/Models/"+model);
                    if (imported)
                    {
                        var body = Instantiate(imported,visual); body.transform.localPosition = Vector3.zero;
                        body.transform.localScale = Vector3.one*.75f; FixMaterials(body);
                    }
                    else
                    {
                        var source = DungeonHeroCatalog.ImportArena().FirstOrDefault(h=>h.Id==unit.ProfileId);
                        var portrait = source?.LoadPortrait();
                        if (portrait)
                        {
                            var spriteObject = new GameObject("Arena silhouette",typeof(SpriteRenderer)); spriteObject.transform.SetParent(visual,false);
                            var sprite = spriteObject.GetComponent<SpriteRenderer>(); sprite.sprite = portrait;
                            spriteObject.transform.rotation = ViewCamera.transform.rotation;
                            float height = Mathf.Max(.01f,portrait.bounds.size.y),scale=1.05f/height;
                            spriteObject.transform.localScale = Vector3.one*scale;
                            spriteObject.transform.localPosition=Vector3.up*.06f-ViewCamera.transform.up*(portrait.bounds.min.y*scale)
                                -ViewCamera.transform.right*(portrait.bounds.center.x*scale);
                        }
                        else
                        {
                            var body = Primitive(PrimitiveType.Capsule,"Adventurer",Position(unit.Cell)+Vector3.up*.45f,
                                new Vector3(.4f,.45f,.4f),unit.Team==0 ? new Color(.31f,.57f,.65f) : new Color(.65f,.24f,.28f));
                            body.transform.SetParent(visual,true);
                        }
                    }
                    var baseObject = Primitive(PrimitiveType.Cylinder,"Team base",Position(unit.Cell)+Vector3.up*.03f,
                        new Vector3(.59f,.035f,.59f),unit.Team==0 ? new Color(.26f,.55f,.59f) : new Color(.65f,.21f,.29f));
                    baseObject.transform.SetParent(visual,true);
                }
                bool reappeared=unit.Alive && !visual.gameObject.activeSelf;
                visual.gameObject.SetActive(unit.Alive);
                if(immediate || reappeared || unit.Id==teleportActor) {routes[unit.Id].Clear();visual.position=Position(unit.Cell);}
                else if(!logicalCells[unit.Id].Equals(unit.Cell))
                    foreach(var step in map.FindPath(logicalCells[unit.Id],unit.Cell).Skip(1))routes[unit.Id].Enqueue(Position(step));
                logicalCells[unit.Id]=unit.Cell;
            }
            foreach (var pair in actors) if (session.Unit(pair.Key)==null) pair.Value.gameObject.SetActive(false);
        }

        public void PlayResult(SkillUseResult result)
        {
            if (!result.Success) return;
            Color color = result.SkillId.Contains("pistola") ? new Color(1,.68f,.24f) : result.SkillId.Contains("draco") ? new Color(.74f,.31f,.95f) : new Color(.22f,.77f,.95f);
            var effect = new GameObject(result.AnimationId,typeof(LineRenderer)); effect.transform.SetParent(transform,false);
            var line = effect.GetComponent<LineRenderer>(); line.positionCount = 2; line.startWidth = .055f; line.endWidth = .11f;
            line.sharedMaterial = Material(color,true); line.SetPosition(0,Position(result.Origin)+Vector3.up*.55f);
            line.SetPosition(1,Position(result.Destination)+Vector3.up*.55f);
            effects.Add((effect,Time.unscaledTime+.45f));
            var burst = Primitive(PrimitiveType.Sphere,"Skill burst",Position(result.Destination)+Vector3.up*.35f,Vector3.one*.24f,color);
            effects.Add((burst,Time.unscaledTime+.4f));
            Sync(teleportActor:DungeonSkillCatalog.All[result.SkillId].Teleport ? result.ActorId : null);
        }

        void Update()
        {
            if (session==null) return;
            float blend = DungeonMotion.Blend(7,Time.unscaledDeltaTime);
            foreach (var unit in session.Units)
                if (actors.TryGetValue(unit.Id,out var visual) && routes[unit.Id].Count>0)
                {
                    var next=routes[unit.Id].Peek();visual.position=Vector3.Lerp(visual.position,next,blend);
                    if((visual.position-next).sqrMagnitude<.001f){visual.position=next;routes[unit.Id].Dequeue();}
                }
            var active = session.Unit(session.ActiveActorId);
            marker.gameObject.SetActive(active!=null);
            if (active!=null) marker.position = Vector3.Lerp(marker.position,Position(active.Cell)+Vector3.up*.012f,blend);
            for (int i=effects.Count-1;i>=0;i--) if (Time.unscaledTime>=effects[i].expires)
            { Destroy(effects[i].visual); effects.RemoveAt(i); }
        }

        GameObject Model(string name,Vector3 position,Vector3 scale)
        {
            var resource = Resources.Load<GameObject>("DungeonBDX/Models/"+name);
            if (!resource) return null;
            var model = Instantiate(resource,architecture); model.transform.position = position; model.transform.localScale = scale;
            FixMaterials(model); return model;
        }

        void FixMaterials(GameObject root)
        {
            foreach (var renderer in root.GetComponentsInChildren<Renderer>())
                renderer.sharedMaterials = renderer.sharedMaterials.Select(m => Material(m && m.HasProperty("_Color") ? m.color : new Color(.5f,.38f,.3f))).ToArray();
        }
        Material Material(Color color,bool unlit=false)
        {
            string key = ColorUtility.ToHtmlStringRGBA(color)+(unlit?"u":"l");
            if (materials.TryGetValue(key,out var material)) return material;
            var template=Resources.Load<Material>("DungeonBDX/Materials/"+(unlit?"SkillUnlit":"WorldLit"));
            var shader = Shader.Find(unlit?"Universal Render Pipeline/Unlit":"Universal Render Pipeline/Lit") ?? Shader.Find("Universal Render Pipeline/Lit");
            if(!template && !shader)throw new InvalidOperationException("Execute DungeonBuild.Prepare para incluir os materiais DungeonBDX.");
            material = template ? new Material(template) : new Material(shader);
            material.color=color;material.enableInstancing=true;
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness",.22f);
            materials.Add(key,material); return material;
        }
        GameObject Cube(string name,Vector3 position,Vector3 scale,Color color) => Primitive(PrimitiveType.Cube,name,position,scale,color);
        GameObject Primitive(PrimitiveType type,string name,Vector3 position,Vector3 scale,Color color)
        {
            var go = GameObject.CreatePrimitive(type); go.name = name; go.transform.SetParent(transform,false);
            go.transform.position = position; go.transform.localScale = scale; go.GetComponent<Renderer>().sharedMaterial = Material(color);
            var collider = go.GetComponent<Collider>(); if (collider) { if (Application.isPlaying) Destroy(collider); else DestroyImmediate(collider); }
            return go;
        }
        void OnDestroy()
        {
            foreach (var material in materials.Values) if (material) { if (Application.isPlaying) Destroy(material); else DestroyImmediate(material); }
        }
    }
}
