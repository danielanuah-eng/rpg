using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using ArenaTatica.Core;
using Terrain = ArenaTatica.Core.Terrain;

namespace ArenaTatica
{
    public sealed class ArenaUnitPick : MonoBehaviour { public int Id; }
    // Só desenha o estado e anima os eventos do núcleo; nenhuma regra mora aqui.
    public sealed class ArenaPresentation : MonoBehaviour
    {
        public ArenaGame Game;
        public Camera BoardCamera { get; private set; }
        public bool Ready { get; private set; }
        public bool Busy => running||queue.Count>0;
        public const float HStep=.45f;
        readonly Dictionary<int,Transform> units=new Dictionary<int,Transform>();
        readonly List<GameObject> highlights=new List<GameObject>();
        readonly List<GameObject> markers=new List<GameObject>();
        readonly List<Transform> wings=new List<Transform>();
        readonly Dictionary<Transform,Quaternion> wingRest=new Dictionary<Transform,Quaternion>();
        readonly List<GameObject> transient=new List<GameObject>();
        readonly List<Mesh> generatedMeshes=new List<Mesh>();
        readonly List<Material> materials=new List<Material>();
        readonly Dictionary<string,AudioClip> sounds=new Dictionary<string,AudioClip>();
        readonly Queue<(List<GameEvent> events,MatchState state)> queue=new Queue<(List<GameEvent>,MatchState)>();
        AudioSource[] voices;
        Transform world,boardRoot,unitRoot,effectRoot;
        Material moveMaterial,rangeMaterial,targetMaterial,hoverMaterial,coinMaterial,trapMaterial,flashMaterial;
        Material[] floorMaterials;
        Mesh cubeMesh;
        GameObject hover,boardBase;
        int boardSize=Board.Size;
        string boardKey,shownKey;
        bool running;
        int screenWidth,screenHeight;
        public static Color Gold=new Color(.93f,.72f,.35f), Blue=new Color(.3f,.73f,.95f), Violet=new Color(.64f,.5f,.98f), Red=new Color(.9f,.32f,.28f), Green=new Color(.45f,.85f,.5f);
        // Cor de cada jogador (P1..P6); invocações puxam para o violeta.
        static readonly Color[] PlayerColors={ Gold, Blue, Red, Green, new Color(.95f,.5f,.8f), new Color(.88f,.9f,.95f) };
        public static Color TeamColor(int player) => PlayerColors[Mathf.Clamp(player-1,0,PlayerColors.Length-1)];
        public static Vector3 Position(MatchState s,Cell c,int height) => new Vector3(c.x-(s.width-1)*.5f,height*HStep,c.z-(s.height-1)*.5f);
        public static Vector3 Position(MatchState s,Cell c) => Position(s,c,s.At(c).height);

        void Awake()
        {
            world=new GameObject("ArenaVisual").transform;
            boardRoot=new GameObject("Tabuleiro").transform; boardRoot.SetParent(world);
            unitRoot=new GameObject("Miniaturas").transform; unitRoot.SetParent(world);
            effectRoot=new GameObject("Efeitos").transform; effectRoot.SetParent(world);
            BoardCamera=Camera.main;
            if(!BoardCamera) { var cameraObject=new GameObject("CameraArena",typeof(Camera),typeof(AudioListener)); BoardCamera=cameraObject.GetComponent<Camera>(); cameraObject.tag="MainCamera"; }
            BoardCamera.orthographic=true; BoardCamera.nearClipPlane=.1f; BoardCamera.farClipPlane=150;
            BoardCamera.backgroundColor=new Color(.045f,.06f,.095f); BoardCamera.clearFlags=CameraClearFlags.SolidColor;
            // O tabuleiro usa parte da tela. Limpar o quadro inteiro evita restos de texto do HUD fora do viewport.
            var backdrop=new GameObject("FundoDaTela",typeof(Camera)).GetComponent<Camera>();
            backdrop.transform.SetParent(world); backdrop.depth=BoardCamera.depth-1;
            backdrop.cullingMask=0; backdrop.clearFlags=CameraClearFlags.SolidColor;
            backdrop.backgroundColor=BoardCamera.backgroundColor;
            BoardCamera.transform.position=new Vector3(36,36,36); BoardCamera.transform.LookAt(Vector3.zero);
            if(FindObjectsByType<Light>().Length==0) {
                var light=new GameObject("LuzPrincipal",typeof(Light)).GetComponent<Light>(); light.type=LightType.Directional; light.intensity=1.25f; light.shadows=LightShadows.Soft; light.transform.rotation=Quaternion.Euler(55,-35,0);
            }
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat; RenderSettings.ambientLight=new Color(.55f,.6f,.72f);
            var prototype=GameObject.CreatePrimitive(PrimitiveType.Cube); cubeMesh=prototype.GetComponent<MeshFilter>().sharedMesh; Destroy(prototype);
            var stone=Resources.Load<Texture2D>("Textures/tile_pedra");
            // 0/1 piso xadrez, 2 zona central, 3 arbusto, 4 tronco, 5 copa, 6 caixa
            floorMaterials=new[] {
                Material(new Color(.67f,.7f,.77f),stone), Material(new Color(.36f,.41f,.49f),stone), Material(new Color(.78f,.66f,.42f),stone),
                Material(new Color(.32f,.58f,.3f)), Material(new Color(.42f,.3f,.2f)), Material(new Color(.18f,.42f,.24f)), Material(new Color(.6f,.43f,.25f)) };
            moveMaterial=Material(new Color(.28f,.49f,.58f)); rangeMaterial=Material(new Color(.55f,.42f,.3f)); targetMaterial=Material(new Color(.95f,.45f,.2f));
            hoverMaterial=Material(new Color(.7f,.78f,.85f)); coinMaterial=Material(Gold); trapMaterial=Material(Red); flashMaterial=Material(Gold);
            hover=Cube("Casa",Vector3.zero,new Vector3(.97f,.015f,.97f),hoverMaterial,world); hover.SetActive(false);
            boardBase=Cube("BaseDoTabuleiro",new Vector3(0,-.4f,0),new Vector3(Board.Size+.4f,.3f,Board.Size+.4f),Material(new Color(.16f,.2f,.27f)),world);
            voices=new AudioSource[12]; for(int i=0;i<voices.Length;i++) { voices[i]=gameObject.AddComponent<AudioSource>(); voices[i].playOnAwake=false; voices[i].volume=.25f; }
            foreach(string sound in new[]{"attack","breath","death","turn"}) sounds[sound]=Resources.Load<AudioClip>("Audio/"+sound);
            Ready=true; FitCamera();
        }
        Material Material(Color color,Texture texture=null)
        {
            var template=Resources.Load<Material>("Materials/ArenaLit");
            if(!template) throw new InvalidOperationException("Execute ArenaBuild.Prepare para incluir o material do tabuleiro.");
            var m=new Material(template); m.color=color; if(texture) m.mainTexture=texture;
            m.SetFloat("_Smoothness",.22f); materials.Add(m); return m;
        }
        GameObject Cube(string name,Vector3 p,Vector3 size,Material material,Transform parent)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube); go.name=name; go.transform.SetParent(parent); go.transform.localPosition=p; go.transform.localScale=size;
            Destroy(go.GetComponent<Collider>()); go.GetComponent<Renderer>().sharedMaterial=material; return go;
        }

        // Malha combinada por material; refeita só quando relevo ou terreno mudam (Cataclisma).
        void BuildBoard(MatchState s)
        {
            string key=string.Join(",",s.tiles.Select(t=>t.height*10+(int)t.terrain));
            if(key==boardKey) return;
            // Tabuleiro de outro tamanho (2, 4 ou 6 jogadores): base e câmera acompanham.
            if(s.width!=boardSize) { boardSize=s.width; boardBase.transform.localScale=new Vector3(boardSize+.4f,.3f,boardSize+.4f); FitCamera(); }
            boardKey=key;
            foreach(Transform child in boardRoot) Destroy(child.gameObject);
            foreach(var m in generatedMeshes) Destroy(m); generatedMeshes.Clear();
            var groups=floorMaterials.Select(_=>new List<CombineInstance>()).ToArray();
            void Add(int group,Vector3 center,Vector3 size) => groups[group].Add(new CombineInstance{mesh=cubeMesh,transform=Matrix4x4.TRS(center,Quaternion.identity,size)});
            for(int x=0;x<s.width;x++) for(int z=0;z<s.height;z++) {
                var c=new Cell(x,z); var t=s.At(c); var top=Position(s,c);
                float h=top.y+.25f;
                Add(t.zone?2:(x+z)%2,new Vector3(top.x,top.y-h/2,top.z),new Vector3(.96f,h,.96f));
                if(t.terrain==Terrain.Arbusto) Add(3,top+Vector3.up*.22f,new Vector3(.78f,.44f,.78f));
                else if(t.terrain==Terrain.Arvore) { Add(4,top+Vector3.up*.45f,new Vector3(.28f,.9f,.28f)); Add(5,top+Vector3.up*1.2f,new Vector3(.86f,.8f,.86f)); }
                else if(t.terrain==Terrain.Caixa) Add(6,top+Vector3.up*.42f,new Vector3(.84f,.84f,.84f));
            }
            for(int g=0;g<groups.Length;g++) {
                if(groups[g].Count==0) continue;
                var mesh=new Mesh{name="Tabuleiro"+g,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32}; mesh.CombineMeshes(groups[g].ToArray()); generatedMeshes.Add(mesh);
                var go=new GameObject("Tabuleiro"+g,typeof(MeshFilter),typeof(MeshRenderer)); go.transform.SetParent(boardRoot,false);
                go.GetComponent<MeshFilter>().sharedMesh=mesh; go.GetComponent<MeshRenderer>().sharedMaterial=floorMaterials[g];
            }
        }
        // Moedas e armadilhas visíveis para quem olha esta tela.
        void RefreshMarkers(MatchState s)
        {
            foreach(var m in markers) Destroy(m); markers.Clear();
            int viewer=Game.ViewerTeam;
            for(int x=0;x<s.width;x++) for(int z=0;z<s.height;z++) {
                var c=new Cell(x,z); var t=s.At(c); var p=Position(s,c);
                if(t.coins>0) for(int i=0;i<t.coins;i++) markers.Add(Cube("Moeda",p+Vector3.up*(.06f+.07f*i),new Vector3(.32f,.05f,.32f),coinMaterial,world));
                if(t.trap!=0&&(viewer==0||s.Ally(t.trap,viewer))) markers.Add(Cube("Armadilha",p+Vector3.up*.02f,new Vector3(.55f,.03f,.55f),trapMaterial,world));
            }
        }
        public void FitCamera()
        {
            if(!BoardCamera) return;
            bool portrait=Screen.width<Screen.height;
            BoardCamera.rect=portrait?new Rect(.02f,.22f,.96f,.55f):new Rect(.24f,.11f,.74f,.77f);
            float aspect=BoardCamera.pixelRect.width/Mathf.Max(1,BoardCamera.pixelRect.height);
            float halfW=0,halfH=0;
            for(int x=-1;x<=1;x+=2) for(int z=-1;z<=1;z+=2) for(int y=0;y<=1;y++) {
                var p=BoardCamera.transform.InverseTransformPoint(new Vector3(x*(boardSize*.5f+.4f),y*3.2f,z*(boardSize*.5f+.4f))); halfW=Mathf.Max(halfW,Mathf.Abs(p.x)); halfH=Mathf.Max(halfH,Mathf.Abs(p.y));
            }
            BoardCamera.orthographicSize=Mathf.Max(halfH,halfW/aspect)*1.06f;
            screenWidth=Screen.width; screenHeight=Screen.height;
        }

        // Estado final aplicado sem animação (snapshot, reset, fim da fila).
        public void Sync(MatchState state,bool immediate)
        {
            if(!Ready||state==null) return;
            if(immediate) CancelAnimations();
            BuildBoard(state); RefreshMarkers(state);
            foreach(var f in state.fighters) {
                if(!units.TryGetValue(f.id,out var root)) { if(!f.Alive) continue; root=units[f.id]=CreateUnit(f); }
                root.gameObject.SetActive(f.Alive);
                root.position=Position(state,f.cell);
            }
            foreach(var id in units.Keys.Where(id=>state.F(id)==null).ToList()) { Destroy(units[id].gameObject); units.Remove(id); }
            shownKey=null;
        }
        public void Play(List<GameEvent> events,MatchState state)
        {
            queue.Enqueue((events,state));
            if(!running) StartCoroutine(RunQueue());
        }
        IEnumerator RunQueue()
        {
            running=true;
            while(queue.Count>0) {
                var (events,state)=queue.Dequeue();
                foreach(var e in events) yield return Animate(e,state);
                Sync(state,false);
            }
            running=false; shownKey=null;
        }
        Transform CreateUnit(Fighter f)
        {
            var root=new GameObject("Unidade_"+f.id).transform; root.SetParent(unitRoot);
            float height=f.def=="ciantia"?1.6f:f.def=="dragao-espectral"?1.3f:.75f;
            string model=f.def=="ciantia"?"CiantiaChibi3D":f.def=="dragao-espectral"?"DragaoTransplanar3D":null;
            var prefab=model==null?null:Resources.Load<GameObject>("Models/"+model);
            Transform visual;
            if(prefab) visual=Instantiate(prefab,root).transform;
            else {
                if(model!=null) Debug.LogWarning("Modelo não importado: "+model);
                visual=GameObject.CreatePrimitive(PrimitiveType.Capsule).transform; Destroy(visual.GetComponent<Collider>()); visual.SetParent(root);
                visual.GetComponent<Renderer>().sharedMaterial=Material(new Color(.55f,.52f,.5f));
            }
            visual.name="Modelo"; visual.localPosition=Vector3.zero;
            var renderers=visual.GetComponentsInChildren<Renderer>();
            if(renderers.Length>0) {
                var bounds=renderers[0].bounds; foreach(var r in renderers) bounds.Encapsulate(r.bounds);
                visual.localScale*=height/Mathf.Max(.01f,bounds.size.y);
                bounds=renderers[0].bounds; foreach(var r in renderers) bounds.Encapsulate(r.bounds);
                visual.position+=new Vector3(root.position.x-bounds.center.x,root.position.y-bounds.min.y,root.position.z-bounds.center.z);
            }
            visual.localRotation=Quaternion.Euler(0,14.3f,0);
            if(Content.Fighters[f.def].flying) visual.localPosition+=Vector3.up*.35f;
            foreach(var t in visual.GetComponentsInChildren<Transform>()) if(t.name.StartsWith("Wing")||t.name.StartsWith("Asa")) { wings.Add(t); wingRest[t]=t.localRotation; }
            var color=TeamColor(f.team); if(f.summoner!=0) color=Color.Lerp(color,Violet,.45f);
            Cube("Marcador",new Vector3(0,.018f,0),new Vector3(.67f,.028f,.67f),Material(color),root);
            root.gameObject.layer=8;
            root.gameObject.AddComponent<ArenaUnitPick>().Id=f.id;
            var collider=root.gameObject.AddComponent<CapsuleCollider>();
            collider.height=height; collider.radius=.38f; collider.center=Vector3.up*height*.5f; collider.isTrigger=true;
            return root;
        }
        string Name(MatchState s,int id) { var f=s.F(id); return f==null?"?":Content.Fighters[f.def].name+" (P"+f.team+")"; }
        // Nome do time: em todos contra todos é o próprio jogador.
        public static string TeamName(MatchState s,int team) => s.TeamPlay?"Time "+team:"P"+team;
        static string Dice(Face[] dice) => dice==null?"":string.Join(" ",dice.Select(d=>d==Face.CoringaEsquivaTrava?"Coringa(E/T)":d.ToString()));

        IEnumerator Animate(GameEvent e,MatchState s)
        {
            units.TryGetValue(e.actor,out var actor);
            units.TryGetValue(e.target,out var target);
            switch(e.kind)
            {
                case "turno": Sound("turn"); break;
                case "mover":
                    if(actor&&e.path!=null) for(int i=1;i<e.path.Length;i++) {
                        var from=Position(s,e.path[i-1]); var to=Position(s,e.path[i]);
                        if(actor.childCount>0) { var flat=to-from; flat.y=0; if(flat!=Vector3.zero) actor.GetChild(0).rotation=Quaternion.LookRotation(flat); }
                        for(float t=0;t<.24f;t+=Time.deltaTime) { float k=.5f-.5f*Mathf.Cos(Mathf.PI*t/.24f); actor.position=Vector3.Lerp(from,to,k)+Vector3.up*Mathf.Sin(k*Mathf.PI)*.12f; yield return null; }
                        actor.position=to;
                    }
                    break;
                case "trava":
                    Float(Position(s,e.cell),e.value==1?"Travado!":"Escapou",e.value==1?Red:Green);
                    Game.Hud?.Log($"{Name(s,e.actor)} tenta sair: {(e.value==1?"travado":"escapou")} [{Dice(e.dice)}]");
                    yield return Wait(.45f); break;
                case "moeda": Float(Position(s,e.cell),"+"+e.value+" moeda",Gold); yield return Wait(.15f); break;
                case "ataque":
                    if(actor&&target) {
                        if(actor.childCount>0) { var d=target.position-actor.position; d.y=0; if(d!=Vector3.zero) actor.GetChild(0).rotation=Quaternion.LookRotation(d); }
                        Sound("attack"); Flash(target.position+Vector3.up,Gold); Float(target.position,"−"+e.value,Gold);
                        yield return Pulse(actor);
                    }
                    Game.Hud?.Log($"{Name(s,e.actor)} → {Name(s,e.target)}: {e.value} de dano [{Dice(e.dice)}]");
                    break;
                case "cura": if(target) { Flash(target.position+Vector3.up,Green); Float(target.position,"+"+e.value,Green); } yield return Wait(.4f); break;
                case "armadilha":
                    if(target) { Flash(target.position+Vector3.up*.3f,Red); Float(target.position,"−"+e.value,Red); }
                    Game.Hud?.Log($"{Name(s,e.target)} pisou numa armadilha: {e.value} de dano");
                    yield return Wait(.45f); break;
                case "armadilha-posta": Float(Position(s,e.cell),"Armadilha",Red); yield return Wait(.2f); break;
                case "invocar":
                    var f=s.F(e.target);
                    if(f!=null&&!units.ContainsKey(f.id)) { var u=units[f.id]=CreateUnit(f); u.position=Position(s,f.cell); yield return Pulse(u); }
                    Game.Hud?.Log($"{Name(s,e.actor)} invocou {Name(s,e.target)}");
                    break;
                case "nocaute":
                    Sound("death"); if(target) { Flash(target.position+Vector3.up,Violet); yield return Wait(.3f); target.gameObject.SetActive(false); }
                    Game.Hud?.Log($"{Name(s,e.target)} foi nocauteado");
                    break;
                case "cataclisma":
                    Game.Hud?.Log("CATACLISMA: "+e.info.Replace('-',' '));
                    Sound("breath"); BuildBoard(s); RefreshMarkers(s); foreach(var x in s.fighters) if(x.Alive&&units.TryGetValue(x.id,out var r)) r.position=Position(s,x.cell);
                    yield return Shake(); break;
                case "compra": Game.Hud?.Log($"{Name(s,e.actor)} comprou {Shop.Def(e.info)?.name} por {e.value}"); break;
                case "rodada": Game.Hud?.Log("Rodada "+e.value); break;
                case "dominio": Game.Hud?.Log($"{TeamName(s,e.actor)} domina o centro ({e.value})"); break;
                case "eliminado": Game.Hud?.Log($"{TeamName(s,e.actor)} está fora da partida"); break;
                case "vitoria": Game.Hud?.Log(e.actor==0?"EMPATE":$"VITÓRIA de {TeamName(s,e.actor)} ({e.info})"); break;
            }
        }
        IEnumerator Wait(float t) { yield return new WaitForSeconds(t); }
        IEnumerator Pulse(Transform unit)
        {
            if(unit.childCount==0) yield break;
            var model=unit.GetChild(0); var scale=model.localScale;
            for(float t=0;t<.35f;t+=Time.deltaTime) { model.localScale=scale*(1+.12f*Mathf.Sin(t/.35f*Mathf.PI)); yield return null; }
            model.localScale=scale;
        }
        IEnumerator Shake()
        {
            var origin=BoardCamera.transform.position;
            for(float t=0;t<.5f;t+=Time.deltaTime) { BoardCamera.transform.position=origin+UnityEngine.Random.insideUnitSphere*.25f*(1-t/.5f); yield return null; }
            BoardCamera.transform.position=origin;
        }
        void Float(Vector3 world,string text,Color color) => Game.Hud?.FloatText(world,text,color);
        void Flash(Vector3 at,Color color)
        {
            if(transient.Count>=64) return;
            var fx=new GameObject("Impacto"); fx.transform.SetParent(effectRoot); fx.transform.position=at; transient.Add(fx);
            var particle=fx.AddComponent<ParticleSystem>(); particle.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=particle.main; main.loop=false; main.duration=.5f; main.startLifetime=.45f; main.startSpeed=2; main.startSize=.10f; main.maxParticles=40; main.startColor=color; main.simulationSpace=ParticleSystemSimulationSpace.World;
            var emission=particle.emission; emission.rateOverTime=0; emission.SetBursts(new[]{new ParticleSystem.Burst(0,20)});
            particle.GetComponent<ParticleSystemRenderer>().sharedMaterial=flashMaterial;
            particle.Play(); StartCoroutine(DisposeEffect(fx,1));
        }
        IEnumerator DisposeEffect(GameObject effect,float duration) { yield return new WaitForSeconds(duration); transient.Remove(effect); Destroy(effect); }
        public void Sound(string name) { if(!sounds.TryGetValue(name,out var clip)||!clip) return; foreach(var source in voices) if(!source.isPlaying) { source.clip=clip; source.Play(); break; } }
        public void CancelAnimations()
        {
            StopAllCoroutines(); queue.Clear(); running=false; foreach(var item in transient) if(item) Destroy(item); transient.Clear();
            if(voices!=null) foreach(var source in voices) source.Stop();
        }
        public void ResetView(MatchState state)
        {
            CancelAnimations(); foreach(var u in units.Values) if(u) Destroy(u.gameObject); units.Clear(); wings.Clear(); wingRest.Clear(); boardKey=null; Sync(state,true);
        }
        // Unidade por raycast; senão a casa mais alta sob o cursor (testa do relevo mais alto para o mais baixo).
        public bool Pick(Vector2 screenPoint,out Cell cell,out int unitId)
        {
            cell=default; unitId=0;
            var s=Game.State; if(s==null) return false;
            var ray=BoardCamera.ScreenPointToRay(screenPoint);
            if(Physics.Raycast(ray,out var hit,200,1<<8,QueryTriggerInteraction.Collide)) {
                var unit=hit.collider.GetComponent<ArenaUnitPick>(); var f=unit?s.F(unit.Id):null;
                if(f?.Alive==true) { unitId=f.id; cell=f.cell; return true; }
            }
            for(int h=Board.MaxHeight;h>=0;h--) {
                if(!new Plane(Vector3.up,Vector3.up*h*HStep).Raycast(ray,out float distance)) continue;
                var p=ray.GetPoint(distance); var c=new Cell(Mathf.FloorToInt(p.x+s.width*.5f),Mathf.FloorToInt(p.z+s.height*.5f));
                if(Board.Contains(s,c)&&s.At(c).height==h) { cell=c; return true; }
            }
            return false;
        }
        void Update()
        {
            var s=Game.State;
            if(!Ready||s==null) return;
            if(Screen.width!=screenWidth||Screen.height!=screenHeight) FitCamera();
            for(int i=wings.Count-1;i>=0;i--) { var wing=wings[i]; if(!wing) { wings.RemoveAt(i); continue; } wing.localRotation=wingRest[wing]*Quaternion.Euler(0,0,Mathf.Sin(Time.time*1.8f)*3.15f); }
            string key=s.revision+"|"+Game.Mode+"|"+Game.CanAct;
            if(key!=shownKey) UpdateHighlights(key);
            Vector2 point=default; bool click=false,hasPointer=false;
            if(Touchscreen.current?.primaryTouch.press.isPressed==true) { point=Touchscreen.current.primaryTouch.position.ReadValue(); click=Touchscreen.current.primaryTouch.press.wasPressedThisFrame; hasPointer=true; }
            else if(Mouse.current!=null) { point=Mouse.current.position.ReadValue(); click=Mouse.current.leftButton.wasPressedThisFrame; hasPointer=true; }
            bool blocked=false;
            if(EventSystem.current&&hasPointer) { var results=new List<RaycastResult>(); EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=point},results); blocked=results.Count>0; }
            if(hasPointer&&!blocked&&BoardCamera.pixelRect.Contains(point)) {
                bool valid=Pick(point,out var c,out _); hover.SetActive(valid); if(valid) hover.transform.position=Position(s,c)+Vector3.up*.03f;
                if(valid&&click) { if(Game.Mode=="") Game.Submit("mover",c); else Game.Submit("skill",c,Game.Mode); }
            } else hover.SetActive(false);
            bool typing=EventSystem.current?.currentSelectedGameObject?.GetComponent<UnityEngine.UI.InputField>()!=null;
            if(!typing&&Keyboard.current?.spaceKey.wasPressedThisFrame==true) Game.Submit("passar");
            if(!typing&&Keyboard.current?.escapeKey.wasPressedThisFrame==true) Game.Mode="";
        }
        // Mover: casas ao alcance dos PM. Skill: área de alcance (clara) e alvos válidos (forte).
        void UpdateHighlights(string key)
        {
            shownKey=key;
            foreach(var go in highlights) Destroy(go); highlights.Clear();
            var f=Game.Active; var s=Game.State;
            if(!Game.CanAct||f==null) return;
            void Mark(Cell c,Material m) => highlights.Add(Cube("Alcance",Position(s,c)+Vector3.up*.012f,new Vector3(.8f,.012f,.8f),m,world));
            if(Game.Mode=="") {
                var costs=Board.Costs(s,f);
                for(int i=0;i<costs.Length;i++) if(costs[i]>0&&costs[i]<=f.pm) Mark(new Cell(i%s.width,i/s.width),moveMaterial);
                return;
            }
            var sk=Match.Skills(f).FirstOrDefault(x=>x.id==Game.Mode); if(sk==null) { Game.Mode=""; return; }
            var m=Game.Preview; int r=sk.maxRange+2;
            for(int x=-r;x<=r;x++) for(int z=-r;z<=r;z++) {
                var c=f.cell+new Cell(x,z);
                if(m.CanTarget(f,sk,c,out _)) Mark(c,targetMaterial);
                else if(m.InRange(f,sk,c,out _)) Mark(c,rangeMaterial);
            }
        }
        void OnDestroy() { CancelAnimations(); if(world) Destroy(world.gameObject); foreach(var m in materials) Destroy(m); foreach(var m in generatedMeshes) Destroy(m); }
    }
}
