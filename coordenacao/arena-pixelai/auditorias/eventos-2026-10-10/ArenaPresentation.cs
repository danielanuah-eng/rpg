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
        // Quadros do PixelLab na ordem de Directions; ângulo 0 = de frente para a câmera, 90 = para a direita da tela.
        static readonly string[] Directions={"south","south-east","east","north-east","north","north-west","west","south-west"};
        // Sprite do PixelLab: quadros parados por direção e clipes (idle, walk, attack, summon, hit, death...) nomeados <clipe>_<direção>_<quadro>.
        sealed class SpriteRig
        {
            public SpriteRenderer renderer;
            public string folder;
            public Sprite[] still=new Sprite[8];
            public Dictionary<string,Sprite[][]> clips=new Dictionary<string,Sprite[][]>();
            public int dir; public string clip="idle"; public float time; public bool reverse;
        }
        const float Fps=10;
        // Altura em casas de cada lutador no tabuleiro (os sprites têm o mesmo quadro de 128 px).
        static readonly Dictionary<string,float> Heights=new Dictionary<string,float>{
            ["ciantia"]=1.6f, ["talyssa"]=1.6f, ["teantha"]=1.65f, ["devon"]=1.7f, ["diantus"]=1.6f, ["damian"]=1.6f,
            ["ezequiela"]=1.3f, ["ilusao"]=1.3f, ["tchu-tcha"]=1.85f, ["arqueiro-jack"]=1.6f, ["perry"]=1.2f, ["dragao-espectral"]=2f };
        // Pasta de sprites: a da forma atual (Berserk) ou a do lutador.
        static string SpriteFolder(Fighter f) => Content.Form(f)?.sprites??Content.Def(f).sprites;
        readonly Dictionary<Transform,SpriteRig> rigs=new Dictionary<Transform,SpriteRig>();
        Material spriteMaterial;
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
            spriteMaterial=Resources.Load<Material>("Materials/ArenaSprite");
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
                bool fresh=false;
                if(!units.TryGetValue(f.id,out var root)) { if(!f.Alive) continue; root=units[f.id]=CreateUnit(f); fresh=true; }
                root.gameObject.SetActive(f.Alive);
                root.position=Position(state,f.cell);
                if(fresh) Turn(root,-root.position);
                if(!fresh&&f.Alive&&root.childCount>0&&rigs.TryGetValue(root.GetChild(0),out var shown)&&shown.folder!=SpriteFolder(f)&&LoadRig(SpriteFolder(f))!=null) { rigs.Remove(root.GetChild(0)); Destroy(root.gameObject); root=units[f.id]=CreateUnit(f); root.position=Position(state,f.cell); Turn(root,-root.position); }
            }
            foreach(var id in units.Keys.Where(id=>state.F(id)==null).ToList()) { if(units[id].childCount>0) rigs.Remove(units[id].GetChild(0)); Destroy(units[id].gameObject); units.Remove(id); }
            RefreshHidden(state); shownKey=null;
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
            float height=Heights.TryGetValue(f.def,out float h)?h:.75f;
            string model=f.def=="ciantia"?"CiantiaChibi3D":f.def=="dragao-espectral"?"DragaoTransplanar3D":null;
            var prefab=model==null?null:Resources.Load<GameObject>("Models/"+model);
            string folder=SpriteFolder(f);
            var rig=folder==null?null:LoadRig(folder);
            bool sprite=rig!=null;
            Transform visual;
            if(sprite) {
                var renderer=rig.renderer=new GameObject("Sprite",typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
                renderer.sprite=rig.still[0]; if(spriteMaterial) renderer.sharedMaterial=spriteMaterial; visual=renderer.transform; visual.SetParent(root,false);
                rigs[visual]=rig;
            }
            else if(prefab) visual=Instantiate(prefab,root).transform;
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
            // Sprite: de pé, virado para a câmera (que não gira); a direção do lutador escolhe o quadro.
            visual.localRotation=sprite?Quaternion.Euler(0,BoardCamera.transform.eulerAngles.y,0):Quaternion.Euler(0,14.3f,0);
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
                case "turno": Sound("turn"); RefreshHidden(s); break;
                case "mover":
                    if(actor&&e.path!=null) {
                        bool walks=PlayClip(actor,"walk")>0;
                        for(int i=1;i<e.path.Length;i++) {
                            var from=Position(s,e.path[i-1]); var to=Position(s,e.path[i]);
                            Turn(actor,to-from);
                            float hop=walks?.03f:.12f;
                            for(float t=0;t<.24f;t+=Time.deltaTime) { float k=.5f-.5f*Mathf.Cos(Mathf.PI*t/.24f); actor.position=Vector3.Lerp(from,to,k)+Vector3.up*Mathf.Sin(k*Mathf.PI)*hop; yield return null; }
                            actor.position=to;
                        }
                        PlayClip(actor,"idle");
                    }
                    break;
                case "trava":
                    Float(Position(s,e.cell),e.value==1?"Travado!":"Escapou",e.value==1?Red:Green);
                    Game.Hud?.Log($"{Name(s,e.actor)} tenta sair: {(e.value==1?"travado":"escapou")} [{Dice(e.dice)}]");
                    yield return Wait(.45f); break;
                case "moeda": Float(Position(s,e.cell),"+"+e.value+" moeda",Gold); yield return Wait(.15f); break;
                case "ataque":
                    if(actor&&target) {
                        // Golpe principal anima quem ataca (clipe da skill); alvos extras da área só reagem.
                        bool main=e.dice!=null; float swing=0;
                        if(main) { Turn(actor,target.position-actor.position); swing=PlayClip(actor,Content.AnySkill(e.info)?.anim??"attack"); if(swing==0) swing=PlayClip(actor,"attack"); }
                        if(swing>0) yield return Wait(swing*.55f);
                        Sound("attack");
                        if(e.value<0) Float(target.position,"Errou!",Blue);
                        else { Flash(target.position+Vector3.up,Gold); Float(target.position,"−"+e.value,Gold); Turn(target,actor.position-target.position); PlayClip(target,"hit"); }
                        if(swing>0) yield return Wait(swing*.45f); else if(main) yield return Pulse(actor);
                    }
                    Game.Hud?.Log(e.value<0?$"{Name(s,e.actor)} errou {Name(s,e.target)} [{Dice(e.dice)}]":$"{Name(s,e.actor)} → {Name(s,e.target)}: {e.value} de dano{(e.dice!=null?$" [{Dice(e.dice)}]":"")}");
                    break;
                case "teleporte":
                case "salto":
                    if(actor&&e.path!=null&&e.path.Length==2) {
                        var from=Position(s,e.path[0]); var to=Position(s,e.path[1]); Turn(actor,to-from);
                        string clip=e.kind=="salto"?"leap":"teleport"; float t0=PlayClip(actor,clip);
                        if(e.kind=="teleporte") {
                            // Some girando a foice e reaparece do outro lado (o mesmo clipe ao contrário).
                            Flash(from+Vector3.up,Green); yield return Wait(Mathf.Max(.25f,t0));
                            actor.position=to; Flash(to+Vector3.up,Green); PlayClip(actor,clip,true); yield return Wait(Mathf.Max(.25f,t0));
                        } else {
                            float dur=Mathf.Max(.45f,t0);
                            for(float t=0;t<dur;t+=Time.deltaTime) { float k=t/dur; actor.position=Vector3.Lerp(from,to,k)+Vector3.up*Mathf.Sin(k*Mathf.PI)*.9f; yield return null; }
                            actor.position=to;
                        }
                        PlayClip(actor,"idle");
                    }
                    Game.Hud?.Log($"{Name(s,e.actor)}: {Content.AnySkill(e.info)?.name}");
                    break;
                case "forma":
                    if(actor) {
                        var who=s.F(e.actor); float morph=PlayClip(actor,Content.AnySkill(e.info)?.anim??"transform");
                        if(morph==0) morph=PlayClip(actor,"transform");
                        Flash(actor.position+Vector3.up,e.info=="berserk"?Red:Violet);
                        if(morph>0) yield return Wait(morph); else yield return Pulse(actor);
                        // Forma com pasta própria de sprites (Berserk): troca o boneco.
                        if(who!=null&&units[who.id].childCount>0&&rigs.TryGetValue(units[who.id].GetChild(0),out var old)&&old.folder!=SpriteFolder(who)&&LoadRig(SpriteFolder(who))!=null) {
                            rigs.Remove(units[who.id].GetChild(0)); Destroy(units[who.id].gameObject);
                            var fresh=units[who.id]=CreateUnit(who); fresh.position=Position(s,who.cell); Turn(fresh,-fresh.position);
                        }
                    }
                    Game.Hud?.Log($"{Name(s,e.actor)}: {(e.info==null?"forma normal":Content.Def(s.F(e.actor)).forms?.FirstOrDefault(x=>x.id==e.info)?.name??e.info)}");
                    break;
                case "esconder": if(actor) { PlayClip(actor,"hide"); Flash(actor.position+Vector3.up*.5f,Violet); yield return Wait(.4f); } RefreshHidden(s); Game.Hud?.Log($"{Name(s,e.actor)} se escondeu"); break;
                case "revelar": RefreshHidden(s); if(target) { Flash(target.position+Vector3.up,Violet); Float(target.position,"Revelado!",Violet); } yield return Wait(.25f); break;
                case "espelho":
                    if(actor) { PlayClip(actor,"mirror"); Flash(actor.position+Vector3.up,Violet); if(target) Flash(target.position+Vector3.up,Violet); yield return Wait(.5f); }
                    Game.Hud?.Log($"{Name(s,e.actor)} copiou {Content.AnySkill(e.info)?.name} de {Name(s,e.target)}");
                    break;
                case "guarda": if(actor) { Float(actor.position,"−1 (guarda)",Gold); PlayClip(actor,"guard"); } break;
                case "roubo": if(actor) Float(actor.position,"+1 moeda roubada",Gold); Game.Hud?.Log($"{Name(s,e.actor)} roubou 1 moeda de {Name(s,e.target)}"); yield return Wait(.2f); break;
                case "renascer":
                    if(target) { target.gameObject.SetActive(true); Flash(target.position+Vector3.up,Green); float rise=PlayClip(target,"revive"); if(rise==0) rise=PlayClip(target,"death",true); Float(target.position,"Renasceu!",Green); yield return Wait(Mathf.Max(.5f,rise)); PlayClip(target,"idle"); }
                    Game.Hud?.Log($"{Name(s,e.target)} renasceu com {e.value} PV");
                    break;
                case "cura": if(target) { Flash(target.position+Vector3.up,Green); Float(target.position,"+"+e.value,Green); } yield return Wait(.4f); break;
                case "armadilha":
                    if(target) { Flash(target.position+Vector3.up*.3f,Red); Float(target.position,"−"+e.value,Red); PlayClip(target,"hit"); }
                    Game.Hud?.Log($"{Name(s,e.target)} pisou numa armadilha: {e.value} de dano");
                    yield return Wait(.45f); break;
                case "armadilha-posta": Float(Position(s,e.cell),"Armadilha",Red); yield return Wait(.2f); break;
                case "invocar":
                    var f=s.F(e.target);
                    if(f!=null&&!units.ContainsKey(f.id)) {
                        float cast=0; if(actor) { Turn(actor,Position(s,f.cell)-actor.position); cast=PlayClip(actor,"summon"); }
                        if(cast>0) yield return Wait(cast*.6f);
                        var u=units[f.id]=CreateUnit(f); u.position=Position(s,f.cell); if(actor) Turn(u,u.position-actor.position);
                        yield return Pulse(u);
                    }
                    Game.Hud?.Log($"{Name(s,e.actor)} invocou {Name(s,e.target)}");
                    break;
                case "nocaute":
                    Sound("death"); if(target) { Flash(target.position+Vector3.up,Violet); float fall=PlayClip(target,"death"); yield return Wait(Mathf.Max(.3f,fall+.25f)); target.gameObject.SetActive(false); }
                    Game.Hud?.Log(e.info=="renasce"?$"{Name(s,e.target)} caiu…":$"{Name(s,e.target)} foi nocauteado");
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
        void Turn(Transform unit,Vector3 direction)
        {
            direction.y=0; if(!unit||unit.childCount==0||direction==Vector3.zero) return;
            var model=unit.GetChild(0);
            if(!rigs.TryGetValue(model,out var rig)) { model.rotation=Quaternion.LookRotation(direction); return; }
            var toCamera=-BoardCamera.transform.forward; toCamera.y=0; var right=BoardCamera.transform.right; right.y=0;
            float angle=Mathf.Atan2(Vector3.Dot(direction,right.normalized),Vector3.Dot(direction,toCamera.normalized))*Mathf.Rad2Deg;
            rig.dir=((Mathf.RoundToInt(angle/45f)%8)+8)%8; ShowFrame(rig);
        }
        // Quadros por pasta lidos uma vez; cada unidade ganha seu próprio rig (estado do clipe) apontando para eles.
        static readonly Dictionary<string,SpriteRig> rigCache=new Dictionary<string,SpriteRig>();
        static SpriteRig LoadRig(string folder)
        {
            if(string.IsNullOrEmpty(folder)) return null;
            if(!rigCache.TryGetValue(folder,out var shared)) rigCache[folder]=shared=ReadRig(folder);
            return shared==null?null:new SpriteRig{ folder=folder, still=shared.still, clips=shared.clips };
        }
        static SpriteRig ReadRig(string folder)
        {
            var rig=new SpriteRig();
            foreach(var sprite in Resources.LoadAll<Sprite>("Sprites/"+folder)) {
                int d=Array.IndexOf(Directions,sprite.name);
                if(d>=0) { rig.still[d]=sprite; continue; }
                var parts=sprite.name.Split('_');
                if(parts.Length!=3||!int.TryParse(parts[2],out int i)) continue;
                d=Array.IndexOf(Directions,parts[1]); if(d<0) continue;
                if(!rig.clips.TryGetValue(parts[0],out var byDir)) rig.clips[parts[0]]=byDir=new Sprite[8][];
                var list=byDir[d]??new Sprite[0]; if(list.Length<=i) Array.Resize(ref list,i+1); list[i]=sprite; byDir[d]=list;
            }
            return rig.still.All(x=>x)?rig:null;
        }
        // Quadros do clipe na direção pedida; sem ela, o espelho (virando o sprite) ou a direção mais próxima.
        static Sprite[] ClipFrames(SpriteRig rig,out bool flip)
        {
            flip=false;
            if(rig.clip==null||!rig.clips.TryGetValue(rig.clip,out var byDir)) return null;
            if(byDir[rig.dir]?.Length>0) return byDir[rig.dir];
            int mirror=(8-rig.dir)%8; if(byDir[mirror]?.Length>0) { flip=true; return byDir[mirror]; }
            for(int k=1;k<=4;k++) foreach(int d in new[]{(rig.dir+k)%8,(rig.dir+8-k)%8}) if(byDir[d]?.Length>0) return byDir[d];
            return null;
        }
        static bool Loops(string clip) => clip=="idle"||clip=="walk";
        // Escondido: some para os inimigos de quem olha; para os aliados fica translúcido.
        void RefreshHidden(MatchState s)
        {
            foreach(var f in s.fighters) {
                if(!units.TryGetValue(f.id,out var unit)||!unit) continue;
                bool hidden=f.Alive&&f.hidden, mine=s.Ally(f.team,Game.LocalTeam);
                foreach(var r in unit.GetComponentsInChildren<Renderer>(true)) {
                    r.enabled=!hidden||mine;
                    if(r is SpriteRenderer sr) sr.color=new Color(1,1,1,hidden?.45f:1);
                }
                var pick=unit.GetComponent<Collider>(); if(pick) pick.enabled=!hidden||mine;
            }
        }
        static void ShowFrame(SpriteRig rig)
        {
            var frames=ClipFrames(rig,out bool flip);
            if(frames==null) { rig.renderer.sprite=rig.still[rig.dir]; rig.renderer.flipX=false; return; }
            int i=(int)(rig.time*Fps); i=Loops(rig.clip)?i%frames.Length:Mathf.Min(i,frames.Length-1); if(rig.reverse) i=frames.Length-1-i;
            rig.renderer.sprite=frames[i]??rig.still[rig.dir]; rig.renderer.flipX=flip&&frames[i];
        }
        // Começa um clipe e devolve sua duração (0 se o lutador não tem sprite ou não tem esse clipe).
        float PlayClip(Transform unit,string clip,bool reverse=false)
        {
            if(!unit||unit.childCount==0||!rigs.TryGetValue(unit.GetChild(0),out var rig)) return 0;
            rig.clip=clip; rig.time=0; rig.reverse=reverse; var frames=ClipFrames(rig,out _); ShowFrame(rig);
            return frames==null?0:frames.Length/Fps;
        }
        void AdvanceRigs()
        {
            foreach(var rig in rigs.Values) {
                if(!rig.renderer) continue;
                rig.time+=Time.deltaTime;
                var frames=ClipFrames(rig,out _);
                if(frames!=null&&!Loops(rig.clip)&&rig.clip!="death"&&rig.time*Fps>=frames.Length) { rig.clip="idle"; rig.time=0; }
                ShowFrame(rig);
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
            CancelAnimations(); foreach(var u in units.Values) if(u) Destroy(u.gameObject); units.Clear(); wings.Clear(); wingRest.Clear(); rigs.Clear(); boardKey=null; Sync(state,true);
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
            AdvanceRigs();
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
