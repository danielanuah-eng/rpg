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
        // Ritmo por clipe (tarefa 005 com o Codex): ações a ~7 FPS; a caminhada dá meio ciclo (um passo) por casa andada.
        static readonly Dictionary<string,float> ClipFps=new Dictionary<string,float>{ ["idle"]=5, ["walk"]=10, ["hit"]=8, ["death"]=6 };
        const float DefaultFps=7;
        static float Fps(string clip) => clip!=null&&ClipFps.TryGetValue(clip,out float f)?f:DefaultFps;
        // Altura em casas de cada lutador no tabuleiro (os sprites têm o mesmo quadro de 128 px).
        static readonly Dictionary<string,float> Heights=new Dictionary<string,float>{
            ["ciantia"]=1.6f, ["talyssa"]=1.6f, ["teantha"]=1.65f, ["devon"]=1.7f, ["diantus"]=1.6f, ["damian"]=1.6f,
            ["ezequiela"]=1.3f, ["ilusao"]=1.3f, ["tchu-tcha"]=1.85f, ["arqueiro-jack"]=1.6f, ["perry"]=1.2f, ["dragao-espectral"]=2f, ["lobo-de-cinzas"]=.8f };
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
        Material moveMaterial,rangeMaterial,targetMaterial,hoverMaterial,coinMaterial,trapMaterial,flashMaterial,glowMaterial,baseMaterial;
        Camera backdrop;
        Transform sceneryRoot;
        int shownTheme=-1,sceneryWidth;
        Material[] floorMaterials;
        Mesh cubeMesh;
        GameObject hover,boardBase;
        int boardSize=Board.Size;
        string boardKey,shownKey;
        bool running;
        int screenWidth,screenHeight;
        // Câmera: pinça ou roda do mouse aproximam; arrastar (dedo, botão esquerdo, direito ou do meio) move.
        // Toque ou clique sem arrastar é a ação na casa. fitSize é o enquadramento do tabuleiro inteiro.
        static readonly Vector3 CameraHome=new Vector3(36,36,36);
        float fitSize=1,pinchDistance; bool pressing,pressOnBoard,dragged,pinching; Vector2 pressStart,lastDrag,pinchCenter;
        public static Color Gold=new Color(.93f,.72f,.35f), Blue=new Color(.3f,.73f,.95f), Violet=new Color(.64f,.5f,.98f), Red=new Color(.9f,.32f,.28f), Green=new Color(.45f,.85f,.5f);
        // Cor de cada jogador (P1..P6); invocações puxam para o violeta.
        static readonly Color[] PlayerColors={ Gold, Blue, Red, Green, new Color(.95f,.5f,.8f), new Color(.88f,.9f,.95f) };
        public static Color TeamColor(int player) => PlayerColors[Mathf.Clamp(player-1,0,PlayerColors.Length-1)];
        // Cenários (MapGen.Themes): piso xadrez a/b, zona, arbusto, tronco, copa, caixa, base, céu, luz ambiente e enfeites fora do tabuleiro.
        sealed class Palette { public Color a,b,zone,bush,trunk,crown,box,ground,sky,ambient; public string props; }
        // Piso de cada cenário (Textures/tile_<nome>, tarefa 007); "pedra" é o piso cinza antigo, só tingido.
        static readonly string[] Tiles={"pedreira","grama","ruinas","cristal","pedra"};
        static Color C(float r,float g,float b) => new Color(r,g,b);
        static readonly Palette[] Palettes={
            new Palette{ a=C(.67f,.7f,.77f), b=C(.36f,.41f,.49f), zone=C(.78f,.66f,.42f), bush=C(.32f,.58f,.3f), trunk=C(.42f,.3f,.2f), crown=C(.18f,.42f,.24f), box=C(.6f,.43f,.25f), ground=C(.16f,.2f,.27f), sky=C(.045f,.06f,.095f), ambient=C(.55f,.6f,.72f), props="pedras" },
            new Palette{ a=C(.47f,.63f,.37f), b=C(.34f,.5f,.29f), zone=C(.78f,.67f,.4f), bush=C(.24f,.52f,.22f), trunk=C(.4f,.28f,.18f), crown=C(.15f,.38f,.19f), box=C(.55f,.4f,.24f), ground=C(.2f,.16f,.11f), sky=C(.04f,.08f,.06f), ambient=C(.56f,.66f,.55f), props="arvores" },
            new Palette{ a=C(.86f,.74f,.5f), b=C(.74f,.6f,.38f), zone=C(.9f,.55f,.3f), bush=C(.5f,.6f,.3f), trunk=C(.5f,.36f,.22f), crown=C(.33f,.53f,.27f), box=C(.62f,.45f,.3f), ground=C(.42f,.29f,.17f), sky=C(.12f,.08f,.06f), ambient=C(.8f,.69f,.55f), props="cactos" },
            new Palette{ a=C(.9f,.93f,.97f), b=C(.74f,.8f,.88f), zone=C(.6f,.76f,.9f), bush=C(.84f,.9f,.93f), trunk=C(.38f,.3f,.25f), crown=C(.27f,.45f,.4f), box=C(.5f,.42f,.36f), ground=C(.3f,.36f,.46f), sky=C(.07f,.09f,.13f), ambient=C(.72f,.77f,.88f), props="pinheiros" },
            new Palette{ a=C(.32f,.28f,.28f), b=C(.2f,.18f,.19f), zone=C(.85f,.4f,.15f), bush=C(.45f,.3f,.22f), trunk=C(.25f,.2f,.18f), crown=C(.55f,.22f,.12f), box=C(.4f,.3f,.25f), ground=C(.12f,.06f,.05f), sky=C(.09f,.03f,.03f), ambient=C(.72f,.52f,.46f), props="lava" } };
        // Disparo à distância (tarefa 006): nasce em quem ataca e o impacto coincide com o dano. Efeitos/<skill>.png do Codex, se existir,
        // vira a cabeça do projétil (Bala, Flecha, Orbe, Lamina, Bomba) ou a animação de impacto (Raio, Sopro).
        enum ShotKind { Bala, Flecha, Orbe, Lamina, Bomba, Raio, Sopro }
        static readonly Dictionary<string,(ShotKind kind,Color color)> Shots=new Dictionary<string,(ShotKind,Color)>{
            ["dragao-dourado"]=(ShotKind.Raio,Gold), ["foice-verde"]=(ShotKind.Lamina,Green), ["tiro-de-pistola"]=(ShotKind.Bala,C(1,.9f,.55f)),
            ["canhao-de-mao"]=(ShotKind.Bomba,C(1,.55f,.2f)), ["ilusao-confusa"]=(ShotKind.Orbe,Violet), ["tiro-carregado"]=(ShotKind.Flecha,C(.85f,.95f,1)),
            ["flecha-abissal"]=(ShotKind.Raio,C(.55f,.25f,.95f)), ["baforada"]=(ShotKind.Sopro,C(.6f,.85f,1)), ["raio-arcano"]=(ShotKind.Raio,C(.4f,.9f,1)) };
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
            backdrop=new GameObject("FundoDaTela",typeof(Camera)).GetComponent<Camera>();
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
            // Sem luz: partículas, rastros e raios pegam a cor do vértice.
            glowMaterial=spriteMaterial?new Material(spriteMaterial):flashMaterial; if(glowMaterial!=flashMaterial) materials.Add(glowMaterial);
            sceneryRoot=new GameObject("Cenario").transform; sceneryRoot.SetParent(world);
            hover=Cube("Casa",Vector3.zero,new Vector3(.97f,.015f,.97f),hoverMaterial,world); hover.SetActive(false);
            baseMaterial=Material(new Color(.16f,.2f,.27f));
            boardBase=Cube("BaseDoTabuleiro",new Vector3(0,-.4f,0),new Vector3(Board.Size+.4f,.3f,Board.Size+.4f),baseMaterial,world);
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
            ApplyTheme(s);
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
        // Cores do cenário e enfeites ao redor do tabuleiro; os enfeites saem de um sorteio próprio (não mexe no rng da partida).
        void ApplyTheme(MatchState s)
        {
            int theme=Mathf.Clamp(s.theme,0,Palettes.Length-1);
            if(theme==shownTheme&&s.width==sceneryWidth) return;
            shownTheme=theme; sceneryWidth=s.width; var p=Palettes[theme];
            var colors=new[]{p.a,p.b,p.zone,p.bush,p.trunk,p.crown,p.box};
            var tile=Resources.Load<Texture2D>("Textures/tile_"+Tiles[theme]); bool art=tile&&Tiles[theme]!="pedra";
            for(int i=0;i<floorMaterials.Length;i++) { floorMaterials[i].color=art&&i<3?Color.Lerp(Color.white,colors[i],.4f):colors[i]; if(i<3&&tile) floorMaterials[i].mainTexture=tile; }
            baseMaterial.color=p.ground; BoardCamera.backgroundColor=backdrop.backgroundColor=p.sky; RenderSettings.ambientLight=p.ambient;
            foreach(Transform child in sceneryRoot) Destroy(child.gameObject);
            var rng=new System.Random(theme*977+s.width);
            float half=s.width*.5f;
            Cube("Chao",new Vector3(0,-.62f,0),new Vector3(s.width+7,.12f,s.width+7),baseMaterial,sceneryRoot);
            int count=Mathf.RoundToInt(s.width*1.6f);
            for(int i=0;i<count;i++) {
                // Anel de 1 a 3 casas fora da borda.
                float along=(float)rng.NextDouble()*2-1, off=half+.9f+(float)rng.NextDouble()*2.2f; int side=rng.Next(4);
                var at=side==0?new Vector3(along*(half+2),0,-off):side==1?new Vector3(along*(half+2),0,off):side==2?new Vector3(-off,0,along*(half+2)):new Vector3(off,0,along*(half+2));
                at.y=-.56f; float size=.35f+(float)rng.NextDouble()*.5f;
                switch(p.props) {
                    case "arvores": case "pinheiros":
                        Cube("Tronco",at+Vector3.up*.4f,new Vector3(.22f,.8f,.22f),floorMaterials[4],sceneryRoot);
                        Cube("Copa",at+Vector3.up*(.95f+size*.4f),new Vector3(.5f+size,.6f+size,.5f+size)*(p.props=="pinheiros"?.8f:1),floorMaterials[5],sceneryRoot);
                        if(p.props=="pinheiros") Cube("Neve",at+Vector3.up*(1.3f+size*.75f),new Vector3(.4f,.12f,.4f)*(1+size),floorMaterials[0],sceneryRoot);
                        break;
                    case "cactos":
                        if(rng.Next(3)==0) { Cube("Cacto",at+Vector3.up*.45f,new Vector3(.2f,.9f+size,.2f),floorMaterials[5],sceneryRoot); Cube("Braco",at+new Vector3(.16f,.6f,0),new Vector3(.12f,.35f,.12f),floorMaterials[5],sceneryRoot); }
                        else Cube("Pedra",at+Vector3.up*size*.3f,new Vector3(size,size*.6f,size*.8f),floorMaterials[1],sceneryRoot);
                        break;
                    case "lava":
                        Cube(rng.Next(3)==0?"Lava":"Pedra",at+Vector3.up*size*.3f,new Vector3(size,size*.6f,size),rng.Next(3)==0?floorMaterials[2]:floorMaterials[1],sceneryRoot);
                        break;
                    default:
                        Cube("Pedra",at+Vector3.up*size*.3f,new Vector3(size,size*.6f,size*.85f),floorMaterials[rng.Next(2)],sceneryRoot);
                        break;
                }
            }
        }
        // Moedas e armadilhas visíveis para quem olha esta tela.
        void RefreshMarkers(MatchState s)
        {
            foreach(var m in markers) Destroy(m); markers.Clear();
            int viewer=Game.ViewerTeam;
            for(int x=0;x<s.width;x++) for(int z=0;z<s.height;z++) {
                var c=new Cell(x,z); var t=s.At(c); var p=Position(s,c);
                if(t.coins>0) for(int i=0;i<t.coins;i++) markers.Add(Cube(CoinName(c),p+Vector3.up*(.06f+.07f*i),new Vector3(.32f,.05f,.32f),coinMaterial,world));
                if(t.trap!=0&&(viewer==0||s.Ally(t.trap,viewer))) markers.Add(Cube("Armadilha",p+Vector3.up*.02f,new Vector3(.55f,.03f,.55f),trapMaterial,world));
            }
        }
        static string CoinName(Cell c) => "Moeda_"+c.x+"_"+c.z;
        void RemoveCoins(Cell c) { string name=CoinName(c); for(int i=markers.Count-1;i>=0;i--) if(markers[i]&&markers[i].name==name) { Destroy(markers[i]); markers.RemoveAt(i); } }
        public void FitCamera()
        {
            if(!BoardCamera) return;
            BoardCamera.pixelRect=ArenaHud.BoardArea();
            float aspect=BoardCamera.pixelRect.width/Mathf.Max(1,BoardCamera.pixelRect.height);
            float halfW=0,halfH=0;
            for(int x=-1;x<=1;x+=2) for(int z=-1;z<=1;z+=2) for(int y=0;y<=1;y++) {
                var p=BoardCamera.transform.InverseTransformPoint(new Vector3(x*(boardSize*.5f+.4f),y*3.2f,z*(boardSize*.5f+.4f))); halfW=Mathf.Max(halfW,Mathf.Abs(p.x)); halfH=Mathf.Max(halfH,Mathf.Abs(p.y));
            }
            BoardCamera.orthographicSize=fitSize=Mathf.Max(halfH,halfW/aspect)*1.06f; BoardCamera.transform.position=CameraHome;
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
            else if(!prefab&&Game.Hud?.ShopIcon(f.def) is Sprite icon) {
                // Invocação sem sprites próprios (lobo): a figura da loja, sempre de frente.
                var renderer=new GameObject("Sprite",typeof(SpriteRenderer)).GetComponent<SpriteRenderer>(); renderer.sprite=icon; if(spriteMaterial) renderer.sharedMaterial=spriteMaterial;
                visual=renderer.transform; visual.SetParent(root,false); sprite=true;
                rigs[visual]=new SpriteRig{ renderer=renderer, still=Enumerable.Repeat(icon,8).ToArray() };
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
            // De pé sob a câmera inclinada, a figura sairia achatada (cos 35° ≈ 82% da altura); estica de volta.
            if(sprite) visual.localScale=Vector3.Scale(visual.localScale,new Vector3(1,1/Mathf.Cos(BoardCamera.transform.eulerAngles.x*Mathf.Deg2Rad),1));
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
                case "mover": if(actor&&e.path!=null&&e.path.Length>1) yield return Walk(actor,s,e.path); break;
                case "trava":
                    if(e.dice!=null) {
                        // Primeiro os dados dos inimigos vizinhos; depois uma Esquiva para cada Trava tirada.
                        int locks=Enumerable.Range(1,e.dice.Length).FirstOrDefault(k=>e.dice.Length-k==e.dice.Take(k).Count(ArenaTatica.Core.Dice.Lock));
                        if(locks==0) locks=e.dice.Length;
                        Game.Hud?.RollDice(e.dice,e.dice.Select((_,i)=>i<locks?"Trava":"Esquiva").ToArray()); yield return Wait(ArenaHud.DiceLand);
                    }
                    Float(Position(s,e.cell),e.value==1?"Travado!":"Escapou",e.value==1?Red:Green);
                    Game.Hud?.Log($"{Name(s,e.actor)} tenta sair: {(e.value==1?"travado":"escapou")} [{Dice(e.dice)}]");
                    yield return Wait(.45f); break;
                case "moeda":
                    // As moedas da casa sobem girando e voam até quem pegou.
                    RemoveCoins(e.cell);
                    for(int i=0;i<Mathf.Min(e.value,5);i++) StartCoroutine(Coin(Position(s,e.cell)+Vector3.up*.1f,actor?actor.position+Vector3.up*1.3f:Position(s,e.cell)+Vector3.up*1.5f,i*.09f,.6f));
                    Float(Position(s,e.cell),"+"+e.value+" moeda",Gold); yield return Wait(.35f); break;
                case "ataque":
                    if(actor&&target) {
                        // Golpe principal anima quem ataca (clipe da skill); alvos extras da área só reagem.
                        bool main=e.dice!=null; float swing=0;
                        if(main) { Game.Hud?.RollDice(e.dice,new[]{"Ataque","Defesa"}); yield return Wait(ArenaHud.DiceLand); }
                        if(main) { Turn(actor,target.position-actor.position); swing=PlayClip(actor,Content.AnySkill(e.info)?.anim??"attack"); if(swing==0) swing=PlayClip(actor,"attack"); }
                        if(swing>0) yield return Wait(swing*.5f);
                        // À distância: o projétil/raio sai no quadro do disparo e o dano aparece quando ele chega.
                        var dealt=Content.AnySkill(e.info);
                        if(main&&Vector3.Distance(Flat(actor.position),Flat(target.position))>1.5f) yield return Shot(e.info,actor.position,target.position,dealt?.area??0);
                        Sound("attack");
                        if(e.value<0) Float(target.position,"Errou!",Blue);
                        else { Hit(target,actor.position,e.value,Shots.TryGetValue(e.info??"",out var hue)?hue.color:C(1,.85f,.6f)); Float(target.position,"−"+e.value,Gold); Turn(target,actor.position-target.position); PlayClip(target,"hit"); }
                        if(swing>0) yield return Wait(swing*.4f); else if(main) yield return Pulse(actor);
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
                    Game.Hud?.Log($"{Name(s,e.actor)} refletiu {Content.AnySkill(e.info)?.name} em {Name(s,e.target)}");
                    break;
                case "desvio":
                    if(target) Float(target.position,"No caminho!",Blue);
                    Game.Hud?.Log($"{Name(s,e.actor)} acertou {Name(s,e.target)}, que estava no caminho");
                    break;
                case "loja": Game.Hud?.Log($"Loja de P{e.actor} renovada"); break;
                case "guarda": if(actor) { Float(actor.position,"−1 (guarda)",Gold); PlayClip(actor,"guard"); } break;
                case "roubo": if(actor) { Float(actor.position,"+1 moeda roubada",Gold); if(target) StartCoroutine(Coin(target.position+Vector3.up*1.1f,actor.position+Vector3.up*1.3f,0,.5f)); } Game.Hud?.Log($"{Name(s,e.actor)} roubou 1 moeda de {Name(s,e.target)}"); yield return Wait(.2f); break;
                case "renascer":
                    if(target) { target.gameObject.SetActive(true); Flash(target.position+Vector3.up,Green); float rise=PlayClip(target,"revive"); if(rise==0) rise=PlayClip(target,"death",true); Float(target.position,"Renasceu!",Green); yield return Wait(Mathf.Max(.5f,rise)); PlayClip(target,"idle"); }
                    Game.Hud?.Log($"{Name(s,e.target)} renasceu com {e.value} PV");
                    break;
                case "cura": if(target) { Flash(target.position+Vector3.up,Green); Float(target.position,"+"+e.value,Green); } yield return Wait(.4f); break;
                case "armadilha":
                    if(target) { Hit(target,target.position+Vector3.back,e.value,Red); Float(target.position,"−"+e.value,Red); PlayClip(target,"hit"); }
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
                    var before=new HashSet<string>(markers.Where(g=>g).Select(g=>g.name));
                    Sound("breath"); BuildBoard(s); RefreshMarkers(s);
                    // Chuva de moedas: as novas caem do céu.
                    foreach(var m in markers) if(m&&m.name.StartsWith("Moeda")&&!before.Contains(m.name)) StartCoroutine(Drop(m.transform,UnityEngine.Random.Range(0,.4f))); foreach(var x in s.fighters) if(x.Alive&&units.TryGetValue(x.id,out var r)) r.position=Position(s,x.cell);
                    yield return Shake(); break;
                case "compra":
                    // Moedas saem de quem comprou e a figura do produto aparece sobre a cabeça.
                    if(actor) {
                        for(int i=0;i<Mathf.Min(e.value,5);i++) { var up=actor.position+Vector3.up*1.3f; StartCoroutine(Coin(up,up+Vector3.up*1.1f+new Vector3(UnityEngine.Random.Range(-.5f,.5f),0,UnityEngine.Random.Range(-.5f,.5f)),i*.07f,.5f)); }
                        Float(actor.position,"−"+e.value+" moedas",Gold);
                        var icon=Game.Hud?.ShopIcon(e.info); if(icon) StartCoroutine(ShowItem(icon,actor.position+Vector3.up*1.9f));
                        yield return Wait(.6f);
                    }
                    Game.Hud?.Log($"{Name(s,e.actor)} comprou {Shop.Def(e.info)?.name} por {e.value}"); break;
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
            int i=(int)(rig.time*Fps(rig.clip)); i=Loops(rig.clip)?i%frames.Length:Mathf.Min(i,frames.Length-1); if(rig.reverse) i=frames.Length-1-i;
            rig.renderer.sprite=frames[i]??rig.still[rig.dir]; rig.renderer.flipX=flip&&frames[i];
        }
        // Começa um clipe e devolve sua duração (0 se o lutador não tem sprite ou não tem esse clipe).
        float PlayClip(Transform unit,string clip,bool reverse=false)
        {
            if(!unit||unit.childCount==0||!rigs.TryGetValue(unit.GetChild(0),out var rig)) return 0;
            // A reação de dano não corta um golpe/skill no meio; a morte sempre entra.
            if(clip=="hit"&&!Loops(rig.clip)&&rig.clip!="hit"&&rig.clip!="death"&&ClipFrames(rig,out _) is Sprite[] busy&&rig.time*Fps(rig.clip)<busy.Length) return 0;
            rig.clip=clip; rig.time=0; rig.reverse=reverse; var frames=ClipFrames(rig,out _); ShowFrame(rig);
            return frames==null?0:frames.Length/Fps(clip);
        }
        void AdvanceRigs()
        {
            foreach(var rig in rigs.Values) {
                if(!rig.renderer) continue;
                rig.time+=Time.deltaTime;
                var frames=ClipFrames(rig,out _);
                if(frames!=null&&!Loops(rig.clip)&&rig.clip!="death"&&rig.time*Fps(rig.clip)>=frames.Length) { rig.clip="idle"; rig.time=0; }
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
        IEnumerator Shake(float strength=.25f,float time=.5f)
        {
            var origin=BoardCamera.transform.position;
            for(float t=0;t<time;t+=Time.deltaTime) { BoardCamera.transform.position=origin+UnityEngine.Random.insideUnitSphere*strength*(1-t/time); yield return null; }
            BoardCamera.transform.position=origin;
        }
        void Float(Vector3 world,string text,Color color) => Game.Hud?.FloatText(world,text,color);
        // Impacto de dano: faíscas esticadas na cor da skill, lascas que caem, anel de choque, alvo piscando vermelho
        // e recuando para longe de quem bateu. Tudo cresce com o dano; golpe de 4+ treme a câmera.
        Texture2D softDot,ringTexture; Material sparkMaterial,ringMaterial;
        static Texture2D Radial(Func<float,float> alpha)
        {
            const int n=64; var tex=new Texture2D(n,n,TextureFormat.RGBA32,false){ wrapMode=TextureWrapMode.Clamp };
            for(int y=0;y<n;y++) for(int x=0;x<n;x++) { float r=Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(n*.5f,n*.5f))/(n*.5f); tex.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(alpha(r)))); }
            tex.Apply(); return tex;
        }
        Material FxMaterial(ref Material cached,Texture2D texture)
        {
            if(!cached) { cached=new Material(glowMaterial){ mainTexture=texture }; materials.Add(cached); }
            return cached;
        }
        Material Spark => FxMaterial(ref sparkMaterial,softDot??=Radial(r=>(1-r)*(1-r)));
        void Hit(Transform target,Vector3 from,int damage,Color color)
        {
            var at=target.position+Vector3.up*.8f; int power=Mathf.Clamp(damage,1,6);
            Burst(at,Spark,10+power*5,(2.5f,5.5f),(.04f,.09f),(.18f,.42f),1.5f,color,stretch:true);
            Burst(at,Spark,4+power*2,(1f,2.6f),(.07f,.14f),(.35f,.6f),3f,Color.Lerp(color,Color.black,.45f));
            Burst(at,FxMaterial(ref ringMaterial,ringTexture??=Radial(r=>1-Mathf.Abs(r-.8f)/.18f)),1,(0,0),(.35f,.35f),(.24f,.24f),0,Color.white,grow:2f+.3f*power);
            StartCoroutine(Recoil(target,Flat(target.position)-Flat(from),power));
            if(power>=4) StartCoroutine(Shake(.06f+.02f*power,.25f));
        }
        void Burst(Vector3 at,Material material,int count,(float min,float max) speed,(float min,float max) size,(float min,float max) life,float gravity,Color color,bool stretch=false,float grow=0)
        {
            if(transient.Count>=64) return;
            var fx=new GameObject("Impacto"); fx.transform.SetParent(effectRoot); fx.transform.position=at; transient.Add(fx);
            var particle=fx.AddComponent<ParticleSystem>(); particle.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=particle.main; main.loop=false; main.duration=.5f; main.maxParticles=count; main.simulationSpace=ParticleSystemSimulationSpace.World;
            main.startLifetime=new ParticleSystem.MinMaxCurve(life.min,life.max); main.startSpeed=new ParticleSystem.MinMaxCurve(speed.min,speed.max);
            main.startSize=new ParticleSystem.MinMaxCurve(size.min,size.max); main.startColor=color; main.gravityModifier=gravity;
            var emission=particle.emission; emission.rateOverTime=0; emission.SetBursts(new[]{new ParticleSystem.Burst(0,(short)count)});
            var shape=particle.shape; shape.shapeType=ParticleSystemShapeType.Sphere; shape.radius=.08f;
            // Começa branco (o clarão do golpe), passa para a cor e some.
            var fade=particle.colorOverLifetime; fade.enabled=true; var g=new Gradient();
            g.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(color,.25f),new GradientColorKey(color,1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(1,.55f),new GradientAlphaKey(0,1)});
            fade.color=g;
            if(grow>0) { var sizes=particle.sizeOverLifetime; sizes.enabled=true; sizes.size=new ParticleSystem.MinMaxCurve(grow,AnimationCurve.EaseInOut(0,.25f,1,1)); }
            var renderer=particle.GetComponent<ParticleSystemRenderer>(); renderer.sharedMaterial=material;
            if(stretch) { renderer.renderMode=ParticleSystemRenderMode.Stretch; renderer.velocityScale=.05f; renderer.lengthScale=1.6f; }
            particle.Play(); StartCoroutine(DisposeEffect(fx,1.2f));
        }
        IEnumerator Recoil(Transform unit,Vector3 away,int power)
        {
            if(unit.childCount==0) yield break;
            var model=unit.GetChild(0); var rest=model.localPosition; var sprite=model.GetComponent<SpriteRenderer>();
            var push=unit.InverseTransformDirection(away.sqrMagnitude>0?away.normalized:Vector3.back)*(.08f+.03f*power);
            for(float t=0;t<.22f;t+=Time.deltaTime) {
                float k=t/.22f; model.localPosition=rest+push*Mathf.Sin(k*Mathf.PI);
                if(sprite) { float a=sprite.color.a; sprite.color=Color.Lerp(new Color(1,.3f,.3f,a),new Color(1,1,1,a),k); }
                yield return null;
            }
            model.localPosition=rest; if(sprite) sprite.color=new Color(1,1,1,sprite.color.a);
        }
        void Flash(Vector3 at,Color color,int burst=20)
        {
            if(transient.Count>=64) return;
            var fx=new GameObject("Impacto"); fx.transform.SetParent(effectRoot); fx.transform.position=at; transient.Add(fx);
            var particle=fx.AddComponent<ParticleSystem>(); particle.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=particle.main; main.loop=false; main.duration=.5f; main.startLifetime=.45f; main.startSpeed=2; main.startSize=.10f; main.maxParticles=40; main.startColor=color; main.simulationSpace=ParticleSystemSimulationSpace.World;
            var emission=particle.emission; emission.rateOverTime=0; emission.SetBursts(new[]{new ParticleSystem.Burst(0,(short)burst)});
            particle.GetComponent<ParticleSystemRenderer>().sharedMaterial=Spark;
            particle.Play(); StartCoroutine(DisposeEffect(fx,1));
        }
        static Vector3 Flat(Vector3 v) { v.y=0; return v; }
        // Caminho inteiro numa curva só: acelera na primeira meia casa, anda constante e freia na última (independe do FPS).
        // Com clipe de caminhada cada casa dura meio ciclo do walk (um passo); sem ele, saltinhos.
        IEnumerator Walk(Transform actor,MatchState s,Cell[] path)
        {
            float cycle=PlayClip(actor,"walk"); bool walks=cycle>0;
            float step=walks?Mathf.Clamp(cycle*.5f,.3f,.6f):.3f;
            var points=path.Select(c=>Position(s,c)).ToArray(); int n=points.Length-1, seg=-1;
            float total=step*n, a=.5f/n, vmax=1/(1-a);
            for(float t=0;t<total;t+=Time.deltaTime) {
                float x=t/total, u=x<a?vmax*x*x/(2*a):x<1-a?vmax*(x-a/2):1-vmax*(1-x)*(1-x)/(2*a);
                float along=Mathf.Clamp(u*n,0,n); int i=Mathf.Min((int)along,n-1); float k=along-i;
                if(i!=seg) { seg=i; Turn(actor,points[i+1]-points[i]); }
                actor.position=Vector3.Lerp(points[i],points[i+1],k)+Vector3.up*Mathf.Sin(k*Mathf.PI)*(walks?.02f:.12f);
                yield return null;
            }
            actor.position=points[n]; PlayClip(actor,"idle");
        }
        static readonly Dictionary<string,Sprite[]> effectCache=new Dictionary<string,Sprite[]>();
        static Sprite[] EffectFrames(string id)
        {
            if(string.IsNullOrEmpty(id)) return null;
            if(!effectCache.TryGetValue(id,out var frames)) {
                frames=Resources.LoadAll<Sprite>("Sprites/Efeitos/"+id).OrderBy(x=>int.TryParse(x.name.Substring(x.name.LastIndexOf('_')+1),out int n)?n:0).ToArray();
                effectCache[id]=frames=frames.Length>0?frames:null;
            }
            return frames;
        }
        static readonly Dictionary<string,Sprite> projectileCache=new Dictionary<string,Sprite>();
        static Sprite ProjectileImage(string id)
        {
            if(string.IsNullOrEmpty(id)) return null;
            if(!projectileCache.TryGetValue(id,out var sp)) projectileCache[id]=sp=Resources.Load<Sprite>("Sprites/Projeteis/"+id);
            return sp;
        }
        // Vira o sprite para a câmera e aponta sua direita (ponta da flecha) para a direção na tela.
        void Orient(Transform t,Vector3 dir)
        {
            var cam=BoardCamera.transform; float ang=Mathf.Atan2(Vector3.Dot(dir,cam.up),Vector3.Dot(dir,cam.right))*Mathf.Rad2Deg;
            t.rotation=cam.rotation*Quaternion.Euler(0,0,ang);
        }
        LineRenderer Line(Transform parent,Color color,float width)
        {
            var l=new GameObject("Linha",typeof(LineRenderer)).GetComponent<LineRenderer>(); l.transform.SetParent(parent,false);
            l.sharedMaterial=glowMaterial; l.useWorldSpace=true; l.positionCount=2; l.widthMultiplier=width; l.startColor=l.endColor=color;
            l.numCapVertices=6; l.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off; l.receiveShadows=false; return l;
        }
        GameObject Effect(string name) { var go=new GameObject(name); go.transform.SetParent(effectRoot); transient.Add(go); return go; }
        IEnumerator Shot(string skill,Vector3 from,Vector3 to,int area)
        {
            (ShotKind kind,Color color) style=skill!=null&&Shots.TryGetValue(skill,out var st)?st:(ShotKind.Orbe,Gold);
            from+=Vector3.up*.9f; var hit=to+Vector3.up*.8f;
            var frames=EffectFrames(skill); var image=ProjectileImage(skill);
            // Projeteis/<skill>.png do Codex: imagem larga (128 px) estica como raio; as menores voam como projétil.
            bool beam=style.kind==ShotKind.Raio||style.kind==ShotKind.Sopro; if(image&&image.rect.width<128) beam=false;
            if(beam) { yield return Beam(from,hit,style.color,style.kind==ShotKind.Sopro,image); if(frames!=null) StartCoroutine(PlayEffect(frames,hit,14)); }
            else yield return Missile(from,hit,style.kind,style.color,frames,image);
            Flash(hit,style.color,area>0?40:20);
            if(area>0) StartCoroutine(Ring(to+Vector3.up*.05f,area,style.color));
        }
        IEnumerator Missile(Vector3 from,Vector3 to,ShotKind kind,Color color,Sprite[] frames,Sprite image)
        {
            var go=Effect("Projetil"); go.transform.position=from;
            float dist=Vector3.Distance(from,to), speed=kind==ShotKind.Bala?18:kind==ShotKind.Flecha?14:kind==ShotKind.Bomba?7:9;
            float dur=Mathf.Clamp(dist/speed,.12f,.6f), arc=kind==ShotKind.Bomba?.6f+.25f*dist:kind==ShotKind.Flecha?.08f*dist:0;
            float width=kind==ShotKind.Bala?.07f:kind==ShotKind.Flecha?.05f:kind==ShotKind.Bomba?.24f:.17f;
            float length=kind==ShotKind.Flecha?.45f:kind==ShotKind.Bala?.22f:kind==ShotKind.Lamina?.4f:.01f;
            var trail=go.AddComponent<TrailRenderer>(); trail.sharedMaterial=glowMaterial; trail.time=kind==ShotKind.Bomba?.25f:.16f;
            trail.widthMultiplier=width*(kind==ShotKind.Bala||kind==ShotKind.Flecha?.8f:.7f); trail.widthCurve=AnimationCurve.Linear(0,1,1,0);
            trail.startColor=color; trail.endColor=new Color(color.r,color.g,color.b,0); trail.numCapVertices=2; trail.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            LineRenderer head=null; SpriteRenderer sprite=null;
            if(frames!=null) { sprite=new GameObject("Imagem",typeof(SpriteRenderer)).GetComponent<SpriteRenderer>(); sprite.transform.SetParent(go.transform,false); sprite.sharedMaterial=spriteMaterial; sprite.sprite=frames[0]; sprite.transform.rotation=BoardCamera.transform.rotation; }
            else if(image) { sprite=new GameObject("Imagem",typeof(SpriteRenderer)).GetComponent<SpriteRenderer>(); sprite.transform.SetParent(go.transform,false); sprite.sharedMaterial=spriteMaterial; sprite.sprite=image; }
            else head=Line(go.transform,Color.Lerp(color,Color.white,.35f),width);
            var prev=from;
            for(float t=0;t<dur;t+=Time.deltaTime) {
                float k=t/dur; var p=Vector3.Lerp(from,to,k)+Vector3.up*Mathf.Sin(k*Mathf.PI)*arc;
                var d=(p-prev).sqrMagnitude>1e-6f?(p-prev).normalized:(to-from).normalized; prev=p; go.transform.position=p;
                if(kind==ShotKind.Lamina) d=Quaternion.AngleAxis(t*1440,Vector3.up)*d;
                if(head) { head.SetPosition(0,p-d*length*.5f); head.SetPosition(1,p+d*length*.5f); }
                if(sprite&&frames!=null) sprite.sprite=frames[(int)(t*12)%frames.Length]; else if(sprite) Orient(sprite.transform,d);
                yield return null;
            }
            go.transform.position=to; if(head) head.enabled=false; if(sprite) sprite.enabled=false;
            StartCoroutine(DisposeEffect(go,trail.time));
        }
        IEnumerator Beam(Vector3 from,Vector3 to,Color color,bool breath,Sprite image=null)
        {
            var go=Effect("Raio");
            float width=breath?.5f:.13f;
            SpriteRenderer art=null; if(image) { art=new GameObject("Imagem",typeof(SpriteRenderer)).GetComponent<SpriteRenderer>(); art.transform.SetParent(go.transform,false); art.sharedMaterial=spriteMaterial; art.sprite=image; width*=.5f; }
            void Stretch(Vector3 tip,float alpha) { if(!art) return; art.transform.position=(from+tip)*.5f; Orient(art.transform,tip-from); float len=Vector3.ProjectOnPlane(tip-from,BoardCamera.transform.forward).magnitude; art.transform.localScale=new Vector3(len/image.bounds.size.x,breath?2:1.3f,1); art.color=new Color(1,1,1,alpha); }
            var glow=Line(go.transform,new Color(color.r,color.g,color.b,.35f),width*2.2f); var core=Line(go.transform,Color.Lerp(color,Color.white,.4f),width);
            if(breath) glow.widthCurve=core.widthCurve=AnimationCurve.Linear(0,.2f,1,1);
            Flash(from,color,10);
            for(float t=0;t<.14f;t+=Time.deltaTime) { var tip=Vector3.Lerp(from,to,t/.14f); foreach(var l in new[]{glow,core}) { l.SetPosition(0,from); l.SetPosition(1,tip); } Stretch(tip,1); yield return null; }
            foreach(var l in new[]{glow,core}) { l.SetPosition(0,from); l.SetPosition(1,to); }
            for(float t=0;t<.3f;t+=Time.deltaTime) {
                float fade=t<.12f?1:1-(t-.12f)/.18f, flicker=1+.15f*Mathf.Sin(t*80);
                core.widthMultiplier=width*flicker*fade; glow.widthMultiplier=width*2.2f*flicker;
                glow.startColor=glow.endColor=new Color(color.r,color.g,color.b,.35f*fade); Stretch(to,fade); yield return null;
            }
            transient.Remove(go); Destroy(go);
        }
        // Área: anel que se abre até o alcance da explosão.
        IEnumerator Ring(Vector3 center,int area,Color color)
        {
            var go=Effect("Area"); var l=Line(go.transform,color,.06f); l.loop=true; l.positionCount=40;
            for(float t=0;t<.45f;t+=Time.deltaTime) {
                float r=(area+.5f)*Mathf.Sin(Mathf.Min(1,t/.25f)*Mathf.PI*.5f), alpha=1-Mathf.Max(0,t-.25f)/.2f;
                for(int i=0;i<40;i++) { float ang=i/40f*Mathf.PI*2; l.SetPosition(i,center+new Vector3(Mathf.Cos(ang)*r,0,Mathf.Sin(ang)*r)); }
                l.startColor=l.endColor=new Color(color.r,color.g,color.b,alpha); yield return null;
            }
            transient.Remove(go); Destroy(go);
        }
        IEnumerator PlayEffect(Sprite[] frames,Vector3 at,float fps)
        {
            var go=Effect("Impacto"); var r=go.AddComponent<SpriteRenderer>(); r.sharedMaterial=spriteMaterial; go.transform.position=at; go.transform.rotation=BoardCamera.transform.rotation;
            for(float t=0;t<frames.Length/fps;t+=Time.deltaTime) { r.sprite=frames[Mathf.Min((int)(t*fps),frames.Length-1)]; yield return null; }
            transient.Remove(go); Destroy(go);
        }
        // Moeda voando (ganho, roubo, gasto): sobe girando e vai ao destino; Codex pode trocar pela tira Efeitos/moeda.
        IEnumerator Coin(Vector3 from,Vector3 to,float delay,float duration)
        {
            if(delay>0) yield return Wait(delay);
            var coin=Cube("MoedaVoando",from,new Vector3(.26f,.04f,.26f),coinMaterial,effectRoot); transient.Add(coin);
            var frames=EffectFrames("moeda"); SpriteRenderer sprite=null;
            if(frames!=null) { coin.GetComponent<Renderer>().enabled=false; sprite=new GameObject("Imagem",typeof(SpriteRenderer)).GetComponent<SpriteRenderer>(); sprite.transform.SetParent(coin.transform,false); sprite.sharedMaterial=spriteMaterial; }
            var mid=Vector3.Lerp(from,to,.5f)+Vector3.up*.7f;
            for(float t=0;t<duration;t+=Time.deltaTime) {
                float k=t/duration, e=k*k*(3-2*k);
                var p=Vector3.Lerp(Vector3.Lerp(from,mid,e),Vector3.Lerp(mid,to,e),e); coin.transform.position=p;
                coin.transform.rotation=Quaternion.Euler(t*720,t*240,0); coin.transform.localScale=new Vector3(.26f,.04f,.26f)*(1-.5f*Mathf.Max(0,k-.6f)/.4f);
                if(sprite) { sprite.transform.rotation=BoardCamera.transform.rotation; sprite.sprite=frames[(int)(t*12)%frames.Length]; }
                yield return null;
            }
            transient.Remove(coin); Destroy(coin);
        }
        IEnumerator Drop(Transform coin,float delay)
        {
            var rest=coin.position; coin.position=rest+Vector3.up*6;
            yield return Wait(delay);
            for(float t=0;t<.45f&&coin;t+=Time.deltaTime) { float k=t/.45f; coin.position=rest+Vector3.up*6*(1-k*k); coin.rotation=Quaternion.Euler(0,t*600,0); yield return null; }
            if(coin) { coin.position=rest; coin.rotation=Quaternion.identity; }
        }
        // Figura do produto comprado sobe sobre o lutador e some.
        IEnumerator ShowItem(Sprite icon,Vector3 at)
        {
            var go=Effect("ProdutoComprado"); var r=go.AddComponent<SpriteRenderer>(); r.sprite=icon; if(spriteMaterial) r.sharedMaterial=spriteMaterial;
            go.transform.rotation=BoardCamera.transform.rotation;
            for(float t=0;t<.9f;t+=Time.deltaTime) {
                float pop=Mathf.Min(1,t/.15f); go.transform.position=at+Vector3.up*t*.4f; go.transform.localScale=Vector3.one*1.6f*pop*(1+.1f*Mathf.Sin(t*12));
                r.color=new Color(1,1,1,t<.6f?1:1-(t-.6f)/.3f); yield return null;
            }
            transient.Remove(go); Destroy(go);
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
            Vector2 point=default; bool tap=false,hasPointer=false,down=false;
            var touches=Touchscreen.current?.touches.Where(t=>t.press.isPressed).ToArray();
            var mouse=Mouse.current;
            if(touches!=null&&touches.Length>=2) { Pinch(touches[0].position.ReadValue(),touches[1].position.ReadValue()); point=lastDrag; hasPointer=down=true; }
            else {
                if(touches!=null&&touches.Length==1) { point=touches[0].position.ReadValue(); hasPointer=down=true; }
                else if(mouse!=null) { point=mouse.position.ReadValue(); hasPointer=true; down=mouse.leftButton.isPressed||mouse.rightButton.isPressed||mouse.middleButton.isPressed; }
                if(pinching) { pinching=false; lastDrag=point; } // sobrou um dedo da pinça: segue arrastando dali, sem salto
            }
            bool overBoard=hasPointer&&BoardCamera.pixelRect.Contains(point)&&!OverUi(point);
            if(down) {
                if(!pressing) { pressing=true; pressOnBoard=overBoard; dragged=pinching; pressStart=lastDrag=point; }
                else if(!pinching) {
                    if((point-pressStart).magnitude>DragThreshold) dragged=true;
                    if(dragged&&pressOnBoard) { BoardCamera.transform.position+=World(lastDrag)-World(point); ClampCamera(); }
                    lastDrag=point;
                }
            } else if(pressing) {
                pressing=false;
                // Só o toque/botão esquerdo age; o direito e o do meio servem apenas para arrastar.
                tap=pressOnBoard&&!dragged&&(mouse==null||!mouse.rightButton.wasReleasedThisFrame&&!mouse.middleButton.wasReleasedThisFrame);
            }
            float wheel=mouse!=null?mouse.scroll.ReadValue().y:0;
            if(overBoard&&Mathf.Abs(wheel)>.01f) ZoomAt(point,wheel>0?.87f:1/.87f);
            if(overBoard||pressing&&pressOnBoard) {
                bool valid=Pick(point,out var c,out _); hover.SetActive(valid&&!dragged); if(valid) hover.transform.position=Position(s,c)+Vector3.up*.03f;
                if(valid&&tap) { if(Game.Mode=="") Game.Submit("mover",c); else Game.Submit("skill",c,Game.Mode); }
            } else hover.SetActive(false);
            bool typing=EventSystem.current?.currentSelectedGameObject?.GetComponent<UnityEngine.UI.InputField>()!=null;
            if(!typing&&Game.Hud?.PickerOpen!=true&&Keyboard.current?.spaceKey.wasPressedThisFrame==true) Game.Submit("passar");
            if(!typing&&Keyboard.current?.escapeKey.wasPressedThisFrame==true) Game.Mode="";
        }
        float DragThreshold => Mathf.Max(8,Screen.dpi*.06f);
        Vector3 World(Vector2 p) => BoardCamera.ScreenToWorldPoint(new Vector3(p.x,p.y,10));
        bool OverUi(Vector2 p)
        {
            if(!EventSystem.current) return false;
            var results=new List<RaycastResult>(); EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=p},results); return results.Count>0;
        }
        // Aproxima mantendo o ponto sob o cursor (ou o meio da pinça) parado na tela.
        void ZoomAt(Vector2 p,float factor)
        {
            var before=World(p);
            BoardCamera.orthographicSize=Mathf.Clamp(BoardCamera.orthographicSize*factor,fitSize*.3f,fitSize);
            BoardCamera.transform.position+=before-World(p); ClampCamera();
        }
        void Pinch(Vector2 a,Vector2 b)
        {
            var center=(a+b)*.5f; float distance=Mathf.Max(1,Vector2.Distance(a,b));
            if(!pinching) { pinching=dragged=true; pressOnBoard=BoardCamera.pixelRect.Contains(center)&&!OverUi(center); }
            else if(pressOnBoard) { BoardCamera.transform.position+=World(pinchCenter)-World(center); ZoomAt(center,pinchDistance/distance); }
            pinchCenter=lastDrag=center; pinchDistance=distance;
        }
        // O tabuleiro nunca sai de vista; afastado de todo, a câmera volta ao centro.
        void ClampCamera() { var cam=BoardCamera.transform; cam.position=CameraHome+Vector3.ClampMagnitude(cam.position-CameraHome,(fitSize-BoardCamera.orthographicSize)*1.5f); }
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
