using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
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
            // ghost: silhueta na cor do time, visível só atrás de props e relevo (shader Arena/Silhueta).
            public SpriteRenderer ghost;
            public string folder;
            public Sprite[] still=new Sprite[8];
            public Dictionary<string,Sprite[][]> clips=new Dictionary<string,Sprite[][]>();
            public int dir; public string clip="idle"; public float time; public bool reverse;
            // Para onde o lutador olha no mundo; a câmera girada escolhe de novo o quadro (dir) a partir daqui.
            public Vector3 facing;
            // Pose da forma (FormDef.pose): parado, mostra o último quadro desse clipe no lugar do idle.
            public string pose;
        }
        // Ritmo por clipe (tarefa 005 com o Codex): ações a ~7 FPS; a caminhada dá meio ciclo (um passo) por casa andada.
        static readonly Dictionary<string,float> ClipFps=new Dictionary<string,float>{ ["idle"]=5, ["walk"]=10, ["hit"]=8, ["death"]=6 };
        const float DefaultFps=7;
        static float Fps(string clip) => clip!=null&&ClipFps.TryGetValue(clip,out float f)?f:DefaultFps;
        // Altura em casas de cada lutador no tabuleiro (os sprites têm o mesmo quadro de 128 px).
        static readonly Dictionary<string,float> Heights=new Dictionary<string,float>{
            ["ciantia"]=1.6f, ["talyssa"]=1.6f, ["teantha"]=1.65f, ["devon"]=1.7f, ["diantus"]=1.6f, ["damian"]=1.6f,
            ["ezequiela"]=1.3f, ["ilusao"]=1.3f, ["tchu-tcha"]=1.85f, ["arqueiro-jack"]=1.6f, ["perry"]=1.2f, ["dragao-espectral"]=2f, ["lobo-de-cinzas"]=.8f,
            ["asamyh"]=1.5f, ["gato-laranja"]=.55f, ["gato-branco"]=.55f, ["gato-malhado"]=.55f, ["gato-manchado"]=.55f, ["colosso"]=3.1f, ["cheiro-pneu"]=2.1f, ["edgar"]=1.65f, ["navio-fantasma"]=2.1f,
            ["rei-bobett"]=1.5f, ["principe-kellen"]=1f, ["vespa-cinzenta"]=.8f, ["viuva-da-agulha"]=1.55f, ["guardiao-de-musgo"]=1.8f, ["domador-de-correntes"]=1.65f,
            ["ogro-de-pedra"]=2f, ["baltazar"]=1.8f, ["totem-de-cura"]=.9f, ["papa-moedas"]=.6f, ["brutamontes-errante"]=2.5f,
            ["chefe-dragao"]=2.6f, ["chefe-necromante"]=2.1f, ["chefe-rei-goblin"]=2.3f };
        // Clone/ilusão (Fighter.disguise, preenchido pelo núcleo): o modelo, o sprite e o nome saem da identidade
        // de quem conjurou, nunca da etiqueta da ilusão. Sem disfarce, é o próprio lutador.
        static string ShownId(Fighter f) => f!=null&&!string.IsNullOrEmpty(f.disguise)&&Content.Fighters.ContainsKey(f.disguise)?f.disguise:f?.def;
        public static FighterDef Shown(Fighter f) => f==null?null:ShownId(f)==f.def?Content.Def(f):Content.Fighters.TryGetValue(ShownId(f),out var d)?d:Content.Def(f);
        static bool Veiled(Fighter f) => f!=null&&ShownId(f)!=f.def;
        // Pasta de sprites: a da forma atual (Berserk) ou a da identidade mostrada. Forma sem pasta com quadros (sem sprites, ou ""
        // depois das edições, que passam por JsonUtility) cai na forma inicial e depois na identidade, nunca na cápsula cinza (3.2).
        public static string SpriteFolder(Fighter f)
        {
            var d=Content.Def(f);
            foreach(var form in new[]{Content.Form(f),d.forms?.FirstOrDefault(x=>x.id==d.startForm)}) if(HasRig(form?.sprites)) return form.sprites;
            return Shown(f).sprites;
        }
        readonly Dictionary<Transform,SpriteRig> rigs=new Dictionary<Transform,SpriteRig>();
        Material spriteMaterial;
        readonly List<GameObject> transient=new List<GameObject>();
        readonly List<Mesh> generatedMeshes=new List<Mesh>();
        readonly List<Material> materials=new List<Material>();
        readonly Dictionary<string,AudioClip> sounds=new Dictionary<string,AudioClip>();
        readonly Queue<(List<GameEvent> events,MatchState state)> queue=new Queue<(List<GameEvent>,MatchState)>();
        AudioSource[] voices;
        Transform world,boardRoot,unitRoot,effectRoot;
        Material moveMaterial,rangeMaterial,targetMaterial,hoverMaterial,coinMaterial,trapMaterial,flashMaterial,glowMaterial,baseMaterial,lavaMaterial,iceMaterial,markMaterial,cageMaterial;
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
        // Vista: 4 lados (90° cada), só neste aparelho; girar não muda casas, alcance nem turno (tarefa 012/013).
        // Autor (13/10): tabuleiro reto, como um quadrado, com a inclinação só na vertical (CameraSide 0); o giro é contínuo e
        // suave (Yaw desliza até YawGoal), sem ficar preso aos 4 lados. View = lado mais próximo (0..3), para quem precisa dele.
        public int View => ((Mathf.RoundToInt(Yaw/90f)%4)+4)%4;
        public float Yaw { get; private set; }
        float yawGoal;
        public const float CameraPitch=60,CameraSide=0,CameraDistance=62,YawSpeed=10;
        static Vector3 SideDirection(float yaw) => Quaternion.Euler(0,yaw+CameraSide,0)*Vector3.back;
        Vector3 CameraHome => (SideDirection(Yaw)*Mathf.Cos(CameraPitch*Mathf.Deg2Rad)+Vector3.up*Mathf.Sin(CameraPitch*Mathf.Deg2Rad))*CameraDistance;
        // Posição de repouso: CameraHome deslocada para o tabuleiro ficar no centro da parte livre da tela (FitCamera).
        Vector3 homeShift; Vector3 Home => CameraHome+homeShift;
        float fitSize=1,pinchDistance,pinchStart,pinchScale=1; bool pressing,pressOnBoard,pressOnUi,pinched,dragged,pinching,touchPress; int pressFingers=1; Vector2 pressStart,lastDrag,pinchCenter;
        // Girar a vista sem teclado: botão direito arrastado ou segurar parado ~0,45 s e arrastar para o lado.
        // Dois dedos dão SÓ zoom (ArenaTouchGesture): sem giro e sem deslocar o tabuleiro.
        bool rotating,rotateButton,pinchZoomed; float rotateAcc,pressTime;
        int multiTouchReleased=-1;
        public bool TouchGestureBusy => touchPress&&(pinching||(pinched&&(pressing||multiTouchReleased==Time.frameCount)));
        Material silhouetteMaterial;
        public static Color Gold=new Color(.93f,.72f,.35f), Blue=new Color(.3f,.73f,.95f), Violet=new Color(.64f,.5f,.98f), Red=new Color(.9f,.32f,.28f), Green=new Color(.45f,.85f,.5f);
        // Cor de cada jogador (P1..P6); invocações puxam para o violeta.
        static readonly Color[] PlayerColors={ Gold, Blue, Red, Green, new Color(.95f,.5f,.8f), new Color(.88f,.9f,.95f) };
        public static Color TeamColor(int player) => player==MatchState.Neutral?new Color(.55f,.55f,.5f):PlayerColors[Mathf.Clamp(player-1,0,PlayerColors.Length-1)];
        // Céu (tarefa 015 do Codex): Sprites/Ceus/<id>/fundo.png atrás do tabuleiro; o evento ativo troca o do cenário.
        static readonly string[] ThemeSkies={"ceu-aberto","ceu-aberto","deserto-poente","nuvens-densas","cinzas-vulcanicas","masmorra","masmorra","ceu-aberto"};
        static readonly Dictionary<string,(string sky,Color tint)> EventSkies=new Dictionary<string,(string,Color)>{
            ["chuva-de-moedas"]=("ceu-dourado",new Color(.2f,.15f,.05f)), ["nevoeiro"]=("nuvens-densas",new Color(.16f,.17f,.2f)), ["mare-de-lava"]=("cinzas-vulcanicas",new Color(.16f,.05f,.03f)),
            ["errante"]=("tempestade",new Color(.06f,.06f,.1f)), ["festival-de-fogos"]=("noite-festiva",new Color(.05f,.03f,.1f)) };
        public static string EventName(string id) => id switch {
            "chuva-de-moedas"=>"Chuva de moedas", "queda-de-pedras"=>"Queda de pedras", "terremoto"=>"Terremoto", "mare-de-lava"=>"Maré de lava",
            "portal-caotico"=>"Portal caótico", "eco-ancestral"=>"Eco ancestral (ultimates de volta)", "mare-de-energia"=>"Maré de energia (+1 PA e +1 PM)",
            "festival-de-fogos"=>"Festival de fogos", "nevoeiro"=>"Nevoeiro (alcance −2)", "vendaval"=>"Vendaval", "papa-moedas"=>"Papa-moedas", "errante"=>"Brutamontes Errante",
            _=>id?.Replace('-',' ') };
        SpriteRenderer sky; string shownSky;
        // Cenários (MapGen.Themes): piso xadrez a/b, zona, arbusto, tronco, copa, caixa, base, céu, luz ambiente e enfeites fora do tabuleiro.
        sealed class Palette { public Color a,b,zone,bush,trunk,crown,box,ground,sky,ambient; public string props; }
        // Piso de cada cenário (Textures/tile_<nome>, tarefa 007); "pedra" é o piso cinza antigo, só tingido.
        static readonly string[] Tiles={"pedreira","grama","ruinas","cristal","pedra","pedra","pedra","grama"};
        static Color C(float r,float g,float b) => new Color(r,g,b);
        static readonly Palette[] Palettes={
            new Palette{ a=C(.67f,.7f,.77f), b=C(.36f,.41f,.49f), zone=C(.78f,.66f,.42f), bush=C(.32f,.58f,.3f), trunk=C(.42f,.3f,.2f), crown=C(.18f,.42f,.24f), box=C(.6f,.43f,.25f), ground=C(.16f,.2f,.27f), sky=C(.045f,.06f,.095f), ambient=C(.55f,.6f,.72f), props="pedras" },
            new Palette{ a=C(.47f,.63f,.37f), b=C(.34f,.5f,.29f), zone=C(.78f,.67f,.4f), bush=C(.24f,.52f,.22f), trunk=C(.4f,.28f,.18f), crown=C(.15f,.38f,.19f), box=C(.55f,.4f,.24f), ground=C(.2f,.16f,.11f), sky=C(.04f,.08f,.06f), ambient=C(.56f,.66f,.55f), props="arvores" },
            new Palette{ a=C(.86f,.74f,.5f), b=C(.74f,.6f,.38f), zone=C(.9f,.55f,.3f), bush=C(.5f,.6f,.3f), trunk=C(.5f,.36f,.22f), crown=C(.33f,.53f,.27f), box=C(.62f,.45f,.3f), ground=C(.42f,.29f,.17f), sky=C(.12f,.08f,.06f), ambient=C(.8f,.69f,.55f), props="cactos" },
            new Palette{ a=C(.9f,.93f,.97f), b=C(.74f,.8f,.88f), zone=C(.6f,.76f,.9f), bush=C(.84f,.9f,.93f), trunk=C(.38f,.3f,.25f), crown=C(.27f,.45f,.4f), box=C(.5f,.42f,.36f), ground=C(.3f,.36f,.46f), sky=C(.07f,.09f,.13f), ambient=C(.72f,.77f,.88f), props="pinheiros" },
            new Palette{ a=C(.32f,.28f,.28f), b=C(.2f,.18f,.19f), zone=C(.85f,.4f,.15f), bush=C(.45f,.3f,.22f), trunk=C(.25f,.2f,.18f), crown=C(.55f,.22f,.12f), box=C(.4f,.3f,.25f), ground=C(.12f,.06f,.05f), sky=C(.09f,.03f,.03f), ambient=C(.72f,.52f,.46f), props="lava" },
            // Fechados (18/10): Cripta, Biblioteca e Jardim; o piso e as paredes vêm do DungeonBDX (BuildDungeon).
            new Palette{ a=C(.6f,.64f,.7f), b=C(.45f,.5f,.56f), zone=C(.8f,.75f,.62f), bush=C(.3f,.45f,.3f), trunk=C(.3f,.28f,.27f), crown=C(.25f,.35f,.3f), box=C(.4f,.4f,.42f), ground=C(.08f,.09f,.12f), sky=C(.03f,.04f,.06f), ambient=C(.5f,.56f,.7f), props="pedras" },
            new Palette{ a=C(.7f,.55f,.4f), b=C(.55f,.42f,.3f), zone=C(.85f,.7f,.45f), bush=C(.35f,.45f,.3f), trunk=C(.35f,.25f,.18f), crown=C(.3f,.4f,.28f), box=C(.5f,.36f,.22f), ground=C(.12f,.08f,.06f), sky=C(.05f,.03f,.03f), ambient=C(.72f,.62f,.52f), props="pedras" },
            new Palette{ a=C(.5f,.66f,.4f), b=C(.38f,.54f,.32f), zone=C(.82f,.68f,.48f), bush=C(.26f,.5f,.24f), trunk=C(.4f,.28f,.18f), crown=C(.16f,.4f,.2f), box=C(.55f,.42f,.3f), ground=C(.16f,.2f,.12f), sky=C(.04f,.08f,.06f), ambient=C(.6f,.68f,.58f), props="arvores" } };
        // Disparo à distância (tarefa 006): nasce em quem ataca e o impacto coincide com o dano. Efeitos/<skill>.png do Codex, se existir,
        // vira a cabeça do projétil (Bala, Flecha, Orbe, Lamina, Bomba) ou a animação de impacto (Raio, Sopro).
        enum ShotKind { Bala, Flecha, Orbe, Lamina, Bomba, Raio, Sopro }
        static readonly Dictionary<string,(ShotKind kind,Color color)> Shots=new Dictionary<string,(ShotKind,Color)>{
            ["dragao-dourado"]=(ShotKind.Raio,Gold), ["foice-verde"]=(ShotKind.Lamina,Green), ["tiro-de-pistola"]=(ShotKind.Bala,C(1,.9f,.55f)),
            ["canhao-de-mao"]=(ShotKind.Bomba,C(1,.55f,.2f)), ["ilusao-confusa"]=(ShotKind.Orbe,Violet), ["tiro-carregado"]=(ShotKind.Flecha,C(.85f,.95f,1)),
            ["flecha-abissal"]=(ShotKind.Raio,C(.55f,.25f,.95f)), ["baforada"]=(ShotKind.Sopro,C(.6f,.85f,1)), ["raio-arcano"]=(ShotKind.Raio,C(.4f,.9f,1)),
            ["bola-de-fogo"]=(ShotKind.Bomba,C(1,.45f,.15f)), ["faisca-laranja"]=(ShotKind.Orbe,C(1,.6f,.2f)), ["faisca-branca"]=(ShotKind.Orbe,C(.95f,.95f,1)),
            ["faisca-malhada"]=(ShotKind.Orbe,C(.75f,.75f,.8f)), ["faisca-manchada"]=(ShotKind.Orbe,C(.6f,.4f,.25f)), ["canhao-de-peito"]=(ShotKind.Bomba,C(.35f,.3f,.3f)),
            ["corrente"]=(ShotKind.Lamina,C(.7f,.72f,.75f)), ["corrente-de-ferro"]=(ShotKind.Lamina,C(.7f,.72f,.75f)), ["agulha"]=(ShotKind.Bala,C(.8f,.95f,.9f)),
            ["prisao-dos-pesadelos"]=(ShotKind.Orbe,C(.5f,.2f,.7f)), ["raizes"]=(ShotKind.Orbe,C(.4f,.7f,.3f)), ["arremessar-pedra"]=(ShotKind.Bomba,C(.55f,.45f,.35f)) };
        public static Vector3 Position(MatchState s,Cell c,int height) => new Vector3(c.x-(s.width-1)*.5f,height*HStep,c.z-(s.height-1)*.5f);
        public static Vector3 Position(MatchState s,Cell c) => Board.Contains(s,c)?Position(s,c,s.At(c).height):Vector3.zero;
        // Onde fica o lutador com o canto em c: o grande (Primal Beast, 2×2) no meio das casas dele, na mais alta.
        public static Vector3 Spot(MatchState s,Fighter f,Cell c)
        {
            int n=f==null?1:Board.SizeOf(f); if(n==1||!Board.Contains(s,c)) return Position(s,c);
            var cells=Board.Footprint(f,c).Where(x=>Board.Contains(s,x)).ToList();
            var p=cells.Aggregate(Vector3.zero,(a,x)=>a+Position(s,x))/cells.Count; p.y=cells.Max(x=>s.At(x).height)*HStep; return p;
        }

        void Awake()
        {
            // O Play do editor não recarrega o domínio: sem isto, uma pasta lida antes da importação fica em cache como vazia.
            rigCache.Clear(); angelFrames=demonFrames=null; effectCache.Clear(); projectileCache.Clear(); flightCache.Clear(); ArenaEfeitos.Reset();
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
            BoardCamera.transform.position=CameraHome; BoardCamera.transform.LookAt(Vector3.zero);
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
            silhouetteMaterial=Resources.Load<Material>("Materials/ArenaSilhueta");
            if(!silhouetteMaterial&&Shader.Find("Arena/Silhueta") is Shader silhouette) { silhouetteMaterial=new Material(silhouette); materials.Add(silhouetteMaterial); }
            hoverMaterial=Material(new Color(.7f,.78f,.85f)); coinMaterial=Material(Gold); trapMaterial=Material(Red); flashMaterial=Material(Gold);
            lavaMaterial=Material(new Color(1,.42f,.1f)); iceMaterial=Material(new Color(.7f,.9f,1)); markMaterial=Material(new Color(1,.4f,.75f)); cageMaterial=Material(new Color(.5f,.3f,.75f));
            sky=new GameObject("Ceu",typeof(SpriteRenderer)).GetComponent<SpriteRenderer>(); sky.transform.SetParent(BoardCamera.transform,false); sky.transform.localPosition=new Vector3(0,0,140); sky.enabled=false;
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
            string key=s.theme+":"+string.Join(",",s.tiles.Select(t=>t.height*10+(int)t.terrain+(t.zone?100:0)+(t.Obj==TileObj.Bau?0:t.obj*1000)))+"|"+string.Join(";",s.decor.Select(d=>d.piece+d.x+","+d.z+","+d.rot));
            if(key==boardKey) return;
            // Tabuleiro de outro tamanho (2, 4 ou 6 jogadores): base e câmera acompanham.
            if(s.width!=boardSize) { boardSize=s.width; boardBase.transform.localScale=new Vector3(boardSize+.4f,.3f,boardSize+.4f); FitCamera(); }
            boardKey=key;
            foreach(Transform child in boardRoot) Destroy(child.gameObject);
            foreach(var m in generatedMeshes) Destroy(m); generatedMeshes.Clear();
            BuildHeights(s);
            if(MapGen.Closed(s.theme)) { boardBase.SetActive(!HasKit(s.theme)); BuildDungeon(s); return; }
            int kitTheme=Mathf.Clamp(s.theme,0,KitFolders.Length-1);
            boardBase.SetActive(!HasKit(kitTheme));
            if(HasKit(kitTheme)) { BuildKit(s,kitTheme); return; }
            var groups=floorMaterials.Select(_=>new List<CombineInstance>()).ToArray();
            void Add(int group,Vector3 center,Vector3 size) => groups[group].Add(new CombineInstance{mesh=cubeMesh,transform=Matrix4x4.TRS(center,Quaternion.identity,size)});
            for(int x=0;x<s.width;x++) for(int z=0;z<s.height;z++) {
                var c=new Cell(x,z); var t=s.At(c); var top=Position(s,c);
                float h=top.y+.25f;
                Add(t.zone?2:(x+z)%2,new Vector3(top.x,top.y-h/2,top.z),new Vector3(.96f,h,.96f));
                if(t.terrain==Terrain.Arbusto) Add(3,top+Vector3.up*.22f,new Vector3(.78f,.44f,.78f));
                else if(t.terrain==Terrain.Arvore) { Add(4,top+Vector3.up*.45f,new Vector3(.28f,.9f,.28f)); Add(5,top+Vector3.up*1.2f,new Vector3(.86f,.8f,.86f)); }
                else if(t.terrain==Terrain.Caixa) Add(6,top+Vector3.up*.42f,new Vector3(.84f,.84f,.84f));
                else if(t.terrain==Terrain.Parede||t.terrain==Terrain.Porta) Add(6,top+Vector3.up*WallHeight*.5f,new Vector3(1,WallHeight,1));
            }
            for(int g=0;g<groups.Length;g++) {
                if(groups[g].Count==0) continue;
                var mesh=new Mesh{name="Tabuleiro"+g,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32}; mesh.CombineMeshes(groups[g].ToArray()); generatedMeshes.Add(mesh);
                var go=new GameObject("Tabuleiro"+g,typeof(MeshFilter),typeof(MeshRenderer)); go.transform.SetParent(boardRoot,false);
                go.GetComponent<MeshFilter>().sharedMesh=mesh; go.GetComponent<MeshRenderer>().sharedMaterial=floorMaterials[g];
            }
        }
        // Sprint 3.5: altura (0–3) escrita no topo de cada casa livre; liga/desliga só neste aparelho (botão Alturas ou H).
        public static bool ShowHeights;
        Transform heightRoot; Font heightFont;
        public void ToggleHeights() { ShowHeights=!ShowHeights; if(heightRoot) heightRoot.gameObject.SetActive(ShowHeights); }
        void BuildHeights(MatchState s)
        {
            heightRoot=new GameObject("Alturas").transform; heightRoot.SetParent(boardRoot,false);
            if(!heightFont) heightFont=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            for(int x=0;x<s.width;x++) for(int z=0;z<s.height;z++) {
                var c=new Cell(x,z); if(s.At(c).terrain!=Terrain.Livre) continue;
                var label=new GameObject($"Altura_{x}_{z}").transform; label.SetParent(heightRoot,false); label.localPosition=Position(s,c)+Vector3.up*.04f;
                // Sombra escura um pouco abaixo e atrás: o número fica legível em casa clara e escura.
                foreach(var (name,color,offset) in new[]{("Sombra",new Color(0,0,0,.75f),new Vector3(.02f,-.02f,.005f)),("Numero",new Color(1,.95f,.75f),Vector3.zero)}) {
                    var go=new GameObject(name,typeof(TextMesh)); go.transform.SetParent(label,false); go.transform.localPosition=offset;
                    var t=go.GetComponent<TextMesh>(); t.text=s.At(c).height.ToString(); t.font=heightFont; t.fontSize=64; t.characterSize=.045f; t.fontStyle=FontStyle.Bold;
                    t.anchor=TextAnchor.MiddleCenter; t.alignment=TextAlignment.Center; t.color=color; go.GetComponent<MeshRenderer>().sharedMaterial=heightFont.material;
                }
            }
            heightRoot.gameObject.SetActive(ShowHeights);
        }
        // Números deitados no topo da casa, sempre de pé para a câmera (a vista gira com Q/E).
        void FaceHeights()
        {
            if(!ShowHeights||!heightRoot) return;
            var rot=Quaternion.Euler(90,BoardCamera.transform.eulerAngles.y,0);
            foreach(Transform label in heightRoot) label.rotation=rot;
        }
        // ── Kit Blender do Codex (tarefa 011): Models/Mapa/<cenário>/<peça>.glb; 1 casa = 1 m, nível = 0,45 m, pivô no centro da base ──
        static readonly string[] KitFolders={"pedreira","bosque","deserto","neve","vulcao"};
        readonly Dictionary<string,GameObject> kitCache=new Dictionary<string,GameObject>();
        // Peça do cenário; se o cenário não tiver (palmeira, relicário), procura em comum/ e nos outros cenários.
        GameObject Kit(int theme,string piece)
        {
            string key=theme+"/"+piece;
            if(kitCache.TryGetValue(key,out var prefab)) return prefab;
            prefab=Resources.Load<GameObject>("Models/Mapa/"+KitFolders[KitIndex(theme)]+"/"+piece)??Resources.Load<GameObject>("Models/Mapa/comum/"+piece);
            for(int i=0;!prefab&&i<KitFolders.Length;i++) prefab=Resources.Load<GameObject>("Models/Mapa/"+KitFolders[i]+"/"+piece);
            return kitCache[key]=prefab;
        }
        bool HasKit(int theme) => Kit(theme,"topo_a")!=null;
        // Labirintos usam o kit 3D de um cenário aberto: Cripta → pedreira, Biblioteca → deserto, Jardim → bosque.
        static readonly int[] ClosedKits={0,2,1};
        static int KitIndex(int theme) => theme>=0&&theme<KitFolders.Length?theme:MapGen.Closed(theme)?ClosedKits[(theme-MapGen.OpenThemes)%ClosedKits.Length]:0;
        GameObject PlaceKit(int theme,string piece,Vector3 at,float yaw,Transform parent,float scale=1)
        {
            if(piece!=null&&piece.StartsWith(MapGen.FloorPrefix)) return null;
            if(piece!=null&&piece.StartsWith("dg_")) return DungeonProp(piece.Substring(3),at,yaw,scale,parent);
            var prefab=Kit(theme,piece); if(!prefab) return null;
            var go=Instantiate(prefab,parent); go.transform.localPosition=at; go.transform.localRotation=Quaternion.Euler(0,yaw,0);
            if(scale!=1) go.transform.localScale*=scale;
            return go;
        }
        static int Hash(int x,int z) => ((x*73856093)^(z*19349663))&0x7fffffff;
        static string DefaultSkin(Terrain t) => t==Terrain.Arvore?"arvore":t==Terrain.Arbusto?"arbusto":"caixa";
        // Tampa de cada casa na altura do relevo (o topo da tampa é a altura de Position), colunas empilhadas embaixo,
        // beira nas bordas, árvores/arbustos/caixas (com o modelo escolhido no mapa), a loja no meio, os enfeites e o entorno.
        // Pisos do DungeonBDX também nos cenários abertos (autor, 18/10): Pedreira, Bosque, Deserto, Neve e Vulcão; zona em mosaico.
        // Piso (autor, 18/10): as tampas do kit 3D do Blender são a base; nas manchas de cenário (MapGen.FloorPatches) a casa
        // vira um bloco com a textura do DungeonBDX (grama, pedra…). A zona central fica sempre com a tampa da zona do kit.
        void BuildFloor(MatchState s,int kit)
        {
            var patches=MapGen.FloorPatches(s); var groups=new Dictionary<Material,List<CombineInstance>>();
            for(int x=0;x<s.width;x++) for(int z=0;z<s.height;z++) {
                var c=new Cell(x,z); var t=s.At(c); var top=Position(s,c); int hash=Hash(x,z);
                if(t.terrain!=Terrain.Parede) {
                    if(!t.zone&&patches.TryGetValue(c,out var tile)) {
                        var m=DungeonMaterial("Tiles",tile,(x+z)%2==0?Color.white:C(.88f,.88f,.9f)); float h=top.y+.25f;
                        if(!groups.TryGetValue(m,out var list)) groups[m]=list=new List<CombineInstance>();
                        list.Add(new CombineInstance{mesh=cubeMesh,transform=Matrix4x4.TRS(new Vector3(top.x,top.y-h/2,top.z),Quaternion.identity,new Vector3(1,h,1))});
                    } else {
                        PlaceKit(kit,t.zone?"topo_zona":(x+z)%2==0?"topo_a":"topo_b",top+Vector3.down*.05f,90*(hash%4),boardRoot);
                        for(int k=0;k<t.height;k++) PlaceKit(kit,(hash>>(k+2)&1)==0?"coluna_a":"coluna_b",new Vector3(top.x,-.05f+k*HStep,top.z),90*((hash>>(k+4))%4),boardRoot);
                    }
                }
                // Beira: reta em cada lado de fora; a peça corre ao longo de x, então os lados ±x giram 90°.
                if(z==0) PlaceKit(kit,"beira_reta",top+new Vector3(0,0,-.5f),180,boardRoot);
                if(z==s.height-1) PlaceKit(kit,"beira_reta",top+new Vector3(0,0,.5f),0,boardRoot);
                if(x==0) PlaceKit(kit,"beira_reta",top+new Vector3(-.5f,0,0),270,boardRoot);
                if(x==s.width-1) PlaceKit(kit,"beira_reta",top+new Vector3(.5f,0,0),90,boardRoot);
            }
            int n=0;
            foreach(var kv in groups) {
                var mesh=new Mesh{name="Mancha"+n,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32}; mesh.CombineMeshes(kv.Value.ToArray()); generatedMeshes.Add(mesh);
                var go=new GameObject("Mancha"+n++,typeof(MeshFilter),typeof(MeshRenderer)); go.transform.SetParent(boardRoot,false);
                go.GetComponent<MeshFilter>().sharedMesh=mesh; go.GetComponent<MeshRenderer>().sharedMaterial=kv.Key;
            }
            PlaceKit(kit,s.width==12?"entorno":"entorno_"+s.width,new Vector3(0,-.002f,0),0,boardRoot);
        }
        void BuildKit(MatchState s,int theme)
        {
            BuildFloor(s,theme);
            var skins=new Dictionary<Cell,Decor>();
            foreach(var d in s.decor) { var at=new Cell(Mathf.RoundToInt(d.x),Mathf.RoundToInt(d.z)); var t=MapGen.SkinTerrain(d.piece); if(t!=Terrain.Livre&&Board.Contains(s,at)&&s.At(at).terrain==t) skins[at]=d; }
            for(int x=0;x<s.width;x++) for(int z=0;z<s.height;z++) {
                var c=new Cell(x,z); var t=s.At(c); var top=Position(s,c); int hash=Hash(x,z);
                if(t.terrain!=Terrain.Livre&&t.terrain!=Terrain.Loja&&t.terrain!=Terrain.Parede&&t.Obj!=TileObj.Braseiro&&t.Obj!=TileObj.Vela) {
                    if(skins.TryGetValue(c,out var d)) PlaceKit(theme,d.piece,top+new Vector3(d.x-x,0,d.z-z),d.rot,boardRoot,d.scale);
                    else PlaceKit(theme,DefaultSkin(t.terrain),top,t.terrain==Terrain.Caixa?90*(hash%4):hash%360,boardRoot);
                }
            }
            // Loja: a barraca do mascate nas 4 casas do meio, de frente para +z, no chão mais alto delas.
            var shop=MapGen.ShopCells(s);
            if(shop.All(c=>s.At(c).terrain==Terrain.Loja)) {
                var center=shop.Aggregate(Vector3.zero,(a,c)=>a+Position(s,c))/shop.Length; center.y=shop.Max(c=>s.At(c).height)*HStep;
                PlaceKit(theme,"loja_mascate",center,180,boardRoot);
            }
            foreach(var d in s.decor) {
                if(MapGen.SkinTerrain(d.piece)!=Terrain.Livre) continue;
                var at=new Cell(Mathf.Clamp(Mathf.RoundToInt(d.x),0,s.width-1),Mathf.Clamp(Mathf.RoundToInt(d.z),0,s.height-1));
                var p=Position(s,at); PlaceKit(theme,d.piece,p+new Vector3(d.x-at.x,0,d.z-at.z),d.rot,boardRoot,d.scale);
            }
            BuildWalls(s,"wall"); BuildObjects(s,null);
            // Props animados do Codex (fechamento 11/10): um par espelhado em 180° fora do tabuleiro, só enfeite (sem regra), em loop.
            if(AmbientProps.TryGetValue(theme,out var ambient)) foreach(int side in new[]{1,-1}) {
                var go=PlaceKit(theme,ambient,new Vector3(side*(s.width/2f+.85f),0,side*s.height/4f),side>0?270:90,boardRoot);
                if(go&&go.GetComponentInChildren<Animation>() is Animation anim&&anim.clip) { anim.wrapMode=WrapMode.Loop; anim.clip.wrapMode=WrapMode.Loop; anim.Play(); }
            }
        }
        // ── Cenários fechados (MapGen.Closed, 18/10): piso, zona e paredes com as texturas do DungeonBDX (Resources/DungeonBDX/Iso);
        // obstáculos com modelo 3D quando ele existe (caixote, coluna, estante, braseiro, cristal, espetos) e o resto em sprite de pé,
        // virado para a câmera como os lutadores.
        public const float WallHeight=.95f;
        const string DungeonArt="DungeonBDX/Iso/";
        static readonly (string floor,string zone,string wall,string objective,string[] blocks,string tree)[] Dungeons={
            ("crypt","marble","wall-crypt","braseiro",new[]{"sarcophagus","statue","pillar","crates"},"deadtree"),
            ("wood","mosaic","wall-books","cristal",new[]{"table","bookshelf","barrels","crates"},"pillar"),
            ("garden","street","hedge","braseiro",new[]{"fountain","statue","crates","deadtree"},"tree") };
        static readonly Dictionary<string,string> PropModels=new Dictionary<string,string>{
            ["crates"]="crate", ["pillar"]="column-bronze", ["bookshelf"]="bookcase", ["braseiro"]="brazier", ["cristal"]="objective-crystal", ["espetos"]="trap-spikes", ["entulho"]="rubble", ["arco"]="door-arch" };
        // Tamanho (lado no chão, altura máxima) de cada prop, em casas.
        static (float side,float tall) PropSize(string id) => id switch {
            "tree"=>(.95f,1.45f), "pine"=>(.85f,1.45f), "deadtree"=>(.9f,1.35f), "statue"=>(.55f,1.25f), "pillar"=>(.55f,1.3f), "bookshelf"=>(.9f,1.25f),
            "fountain"=>(.9f,.85f), "sarcophagus"=>(.85f,.75f), "table"=>(.8f,.7f), "crates"=>(.72f,.72f), "barrels"=>(.7f,.75f), "chest"=>(.6f,.6f),
            "braseiro"=>(.55f,.85f), "cristal"=>(.5f,.95f), "espetos"=>(.86f,.3f), "candles"=>(.5f,.95f), "arco"=>(1f,1.15f), _=>(.6f,.6f) };
        readonly Dictionary<string,Material> dungeonMaterials=new Dictionary<string,Material>();
        Material DungeonMaterial(string folder,string id,Color tint)
        {
            string key=folder+"/"+id+"/"+ColorUtility.ToHtmlStringRGB(tint);
            if(!dungeonMaterials.TryGetValue(key,out var m)) dungeonMaterials[key]=m=Material(tint,Resources.Load<Texture2D>(DungeonArt+folder+"/"+id));
            return m;
        }
        void BuildDungeon(MatchState s)
        {
            var look=Dungeons[(s.theme-MapGen.OpenThemes)%Dungeons.Length];
            BuildFloor(s,s.theme);
            for(int x=0;x<s.width;x++) for(int z=0;z<s.height;z++) {
                var c=new Cell(x,z); var t=s.At(c); if(t.terrain==Terrain.Parede) continue;
                var top=Position(s,c); int hash=Hash(x,z);
                if(t.terrain==Terrain.Caixa&&t.Obj!=TileObj.Braseiro&&t.Obj!=TileObj.Vela) {
                    string piece=s.decor.Where(d=>Mathf.RoundToInt(d.x)==x&&Mathf.RoundToInt(d.z)==z&&d.piece.StartsWith("dg_")).Select(d=>d.piece.Substring(3)).FirstOrDefault()??look.blocks[hash%look.blocks.Length];
                    DungeonProp(piece,top,90*(hash%4),1,boardRoot);
                }
                else if(t.terrain==Terrain.Arvore) DungeonProp(look.tree,top,hash%360,1,boardRoot);
                else if(t.terrain==Terrain.Arbusto) DungeonProp("bush",top,0,1.3f,boardRoot);
            }
            BuildWalls(s,look.wall); BuildObjects(s,look.objective);
            // Loja: a barraca do mascate do kit, como nos abertos.
            var shop=MapGen.ShopCells(s);
            if(shop.All(c=>s.At(c).terrain==Terrain.Loja)) {
                var center=shop.Aggregate(Vector3.zero,(a,c)=>a+Position(s,c))/shop.Length; center.y=shop.Max(c=>s.At(c).height)*HStep;
                PlaceKit(0,"loja_mascate",center,180,boardRoot);
            }
            foreach(var d in s.decor) {
                if(MapGen.SkinTerrain(d.piece)!=Terrain.Livre) continue;
                var at=new Cell(Mathf.Clamp(Mathf.RoundToInt(d.x),0,s.width-1),Mathf.Clamp(Mathf.RoundToInt(d.z),0,s.height-1));
                if(s.At(at).terrain!=Terrain.Livre) continue;
                PlaceKit(0,d.piece,Position(s,at)+new Vector3(d.x-at.x,0,d.z-at.z),d.rot,boardRoot,d.scale);
            }
        }
        // Paredes: só as faces que dão para fora (casa vizinha sem parede ou borda) e a tampa, num mesh por material.
        // A tira de estantes (1024×448) cobre 2 casas; as texturas quadradas, 1 casa por face.
        void BuildWalls(MatchState s,string wall)
        {
            var sides=new MeshParts(); var tops=new MeshParts(); var doors=new MeshParts(); bool strip=wall=="wall-books";
            for(int x=0;x<s.width;x++) for(int z=0;z<s.height;z++) {
                var c=new Cell(x,z); bool door=s.At(c).terrain==Terrain.Porta; if(s.At(c).terrain!=Terrain.Parede&&!door) continue;
                var top=Position(s,c); float y0=-.25f, y1=top.y+WallHeight;
                tops.Quad(new Vector3(top.x-.5f,y1,top.z-.5f),new Vector3(top.x-.5f,y1,top.z+.5f),new Vector3(top.x+.5f,y1,top.z+.5f),new Vector3(top.x+.5f,y1,top.z-.5f),0,1);
                foreach(var d in Board.Dirs) {
                    var n=c+d; if(Board.Contains(s,n)&&s.At(n).terrain==Terrain.Parede) continue;
                    var dir=new Vector3(d.x,0,d.z); var right=Vector3.Cross(Vector3.up,-dir)*.5f; var p=new Vector3(top.x,0,top.z)+dir*.5f;
                    if(door) { (n.x>=0&&n.z>=0&&Board.Contains(s,n)&&s.At(n).terrain==Terrain.Livre?doors:sides).Quad(p-right+Vector3.up*y0,p-right+Vector3.up*y1,p+right+Vector3.up*y1,p+right+Vector3.up*y0,0,1); continue; }
                    float u0=strip?((x+z)&1)*.5f:0, u1=strip?u0+.5f:1;
                    sides.Quad(p-right+Vector3.up*y0,p-right+Vector3.up*y1,p+right+Vector3.up*y1,p+right+Vector3.up*y0,u0,u1);
                }
            }
            sides.Build("Paredes",boardRoot,DungeonMaterial("Walls",wall,Color.white),generatedMeshes);
            tops.Build("TopoDasParedes",boardRoot,DungeonMaterial("Walls",strip?"wall":wall,C(.55f,.55f,.6f)),generatedMeshes);
            doors.Build("Portas",boardRoot,DungeonMaterial("Doors","door",Color.white),generatedMeshes);
        }
        // Objetos fixos que não mudam na partida: espetos e braseiros (o baú some ao abrir e fica nos marcadores).
        void BuildObjects(MatchState s,string objective)
        {
            for(int x=0;x<s.width;x++) for(int z=0;z<s.height;z++) {
                var t=s.At(new Cell(x,z)); var top=Position(s,new Cell(x,z));
                if(t.Obj==TileObj.Espetos) DungeonProp("espetos",top,90*(Hash(x,z)%4),1,boardRoot);
                else if(t.Obj==TileObj.Braseiro) DungeonProp(objective??"braseiro",top,0,1,boardRoot);
                else if(t.Obj==TileObj.Vela) DungeonProp("candles",top,0,1,boardRoot);
                // Porta aberta: o arco de pedra, de frente para o corredor.
                else if(t.Obj==TileObj.PortaAberta) { bool alongX=Board.Contains(s,new Cell(x,z+1))&&s.At(new Cell(x,z+1)).terrain==Terrain.Parede; DungeonProp("arco",top,alongX?0:90,1,boardRoot); }
            }
        }
        sealed class MeshParts
        {
            readonly List<Vector3> v=new List<Vector3>(); readonly List<Vector2> uv=new List<Vector2>(); readonly List<int> tri=new List<int>();
            // a..d: embaixo à esquerda, em cima à esquerda, em cima à direita, embaixo à direita, vistos de frente (sentido horário).
            public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,float u0,float u1)
            {
                int i=v.Count; v.AddRange(new[]{a,b,c,d}); uv.AddRange(new[]{new Vector2(u0,0),new Vector2(u0,1),new Vector2(u1,1),new Vector2(u1,0)});
                tri.AddRange(new[]{i,i+1,i+2,i,i+2,i+3});
            }
            public void Build(string name,Transform parent,Material material,List<Mesh> owned)
            {
                if(v.Count==0) return;
                var mesh=new Mesh{name=name,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32}; mesh.SetVertices(v); mesh.SetUVs(0,uv); mesh.SetTriangles(tri,0);
                mesh.RecalculateNormals(); mesh.RecalculateBounds(); owned.Add(mesh);
                var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer)); go.transform.SetParent(parent,false);
                go.GetComponent<MeshFilter>().sharedMesh=mesh; go.GetComponent<MeshRenderer>().sharedMaterial=material;
            }
        }
        // Prop do DungeonBDX no chão de uma casa (at = topo da casa): modelo 3D (cores do FBX no material da Arena) ou sprite em pé.
        readonly Dictionary<string,Sprite> propSprites=new Dictionary<string,Sprite>();
        readonly Dictionary<Color,Material> modelMaterials=new Dictionary<Color,Material>();
        // Espetos ocultos do labirinto saem do chão quando alguém pisa (depois ficam no tabuleiro, MatchState.sprung).
        IEnumerator RiseSpikes(Vector3 top)
        {
            var go=DungeonProp("espetos",top,0,1,world); if(!go) yield break;
            markers.Add(go); var up=go.transform.position; var down=up+Vector3.down*.35f;
            for(float t=0;t<.16f&&go;t+=Time.deltaTime) { go.transform.position=Vector3.Lerp(down,up,t/.16f); yield return null; }
            if(go) go.transform.position=up;
        }
        GameObject DungeonProp(string id,Vector3 at,float yaw,float scale,Transform parent)
        {
            var (side,tall)=PropSize(id); side*=scale; tall*=scale;
            if(PropModels.TryGetValue(id,out var model)&&Resources.Load<GameObject>("DungeonBDX/Models/"+model) is GameObject prefab) {
                var go=Instantiate(prefab,parent); go.name=id; go.transform.localPosition=Vector3.zero; go.transform.localRotation=Quaternion.Euler(0,yaw,0);
                foreach(var col in go.GetComponentsInChildren<Collider>()) Destroy(col);
                var renderers=go.GetComponentsInChildren<Renderer>();
                foreach(var r in renderers) r.sharedMaterials=r.sharedMaterials.Select(m=>{
                    var color=m&&m.HasProperty("_Color")?m.color:C(.55f,.46f,.34f);
                    if(!modelMaterials.TryGetValue(color,out var mm)) modelMaterials[color]=mm=Material(color);
                    return mm; }).ToArray();
                if(renderers.Length==0) return go;
                Bounds Measure() { var b=renderers[0].bounds; foreach(var r in renderers) b.Encapsulate(r.bounds); return b; }
                var size=Measure().size; go.transform.localScale*=Mathf.Min(side/Mathf.Max(.01f,Mathf.Max(size.x,size.z)),tall/Mathf.Max(.01f,size.y));
                var bounds=Measure(); go.transform.position+=parent.TransformPoint(at)-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
                return go;
            }
            if(!propSprites.TryGetValue(id,out var sprite)) {
                var tex=Resources.Load<Texture2D>(DungeonArt+"Props/"+id)??Resources.Load<Texture2D>(DungeonArt+"Props/"+id+"_sw");
                sprite=propSprites[id]=tex?Sprite.Create(tex,new Rect(0,0,tex.width,tex.height),new Vector2(.5f,0),Mathf.Max(tex.width/PropSize(id).side,tex.height/PropSize(id).tall)):null;
            }
            if(!sprite) return null;
            var spriteObject=new GameObject("Prop_"+id,typeof(SpriteRenderer)); spriteObject.transform.SetParent(parent,false);
            spriteObject.transform.localPosition=at+Vector3.up*.01f; spriteObject.transform.localScale=Vector3.one*scale;
            var sr=spriteObject.GetComponent<SpriteRenderer>(); sr.sprite=sprite; if(spriteMaterial) sr.sharedMaterial=spriteMaterial;
            var loop=spriteObject.AddComponent<SpriteLoop>(); loop.frames=new[]{sprite}; loop.billboard=true;
            return spriteObject;
        }
        static readonly Dictionary<int,string> AmbientProps=new Dictionary<int,string>{
            [0]="ampulheta_das_ruinas", [1]="estandarte_da_clareira", [2]="cata_vento_solar", [3]="fonte_de_geada", [4]="braseiro_de_obsidiana" };
        // Cores do cenário e enfeites ao redor do tabuleiro; os enfeites saem de um sorteio próprio (não mexe no rng da partida).
        void ApplyTheme(MatchState s)
        {
            int theme=Mathf.Clamp(s.theme,0,Palettes.Length-1);
            if(theme==shownTheme&&s.width==sceneryWidth) return;
            shownTheme=theme; sceneryWidth=s.width; var p=Palettes[theme];
            var colors=new[]{p.a,p.b,p.zone,p.bush,p.trunk,p.crown,p.box};
            var tile=Resources.Load<Texture2D>("Textures/tile_"+Tiles[theme]); bool art=tile&&Tiles[theme]!="pedra";
            for(int i=0;i<floorMaterials.Length;i++) { floorMaterials[i].color=art&&i<3?Color.Lerp(Color.white,colors[i],.4f):colors[i]; if(i<3&&tile) floorMaterials[i].mainTexture=tile; }
            baseMaterial.color=p.ground; shownSky=null; ApplySky(s); RenderSettings.ambientLight=p.ambient;
            foreach(Transform child in sceneryRoot) Destroy(child.gameObject);
            if(HasKit(theme)) return; // o entorno do kit já faz o chão e a borda
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
        // Céu do evento ativo ou do cenário: sprite do Codex se existir; a cor de fundo acompanha.
        void ApplySky(MatchState s)
        {
            int theme=Mathf.Clamp(s.theme,0,Palettes.Length-1);
            bool ev=!string.IsNullOrEmpty(s.ev)&&EventSkies.ContainsKey(s.ev);
            string id=ev?EventSkies[s.ev].sky:ThemeSkies[theme];
            BoardCamera.backgroundColor=backdrop.backgroundColor=ev?EventSkies[s.ev].tint:Palettes[theme].sky;
            if(id==shownSky) return; shownSky=id;
            var art=Resources.Load<Sprite>("Sprites/Ceus/"+id+"/fundo");
            // Sem arte do céu: degradê do tema (antes era a cor lisa do fundo, marrom no deserto).
            skyGradient=!art; if(!art) art=GradientSky(ev?EventSkies[s.ev].tint*3.2f:SkyTops[theme],ev?EventSkies[s.ev].tint:SkyBottoms[theme]);
            sky.sprite=art; sky.enabled=art; if(art&&spriteMaterial) sky.sharedMaterial=spriteMaterial; FitSky();
        }
        // Céu de dia por tema (topo → horizonte): montanha, bosque, deserto ao poente, neve e cinzas do vulcão.
        static readonly Color[] SkyTops={C(.36f,.5f,.7f),C(.4f,.62f,.82f),C(.3f,.45f,.72f),C(.52f,.66f,.84f),C(.16f,.07f,.09f),C(.08f,.1f,.16f),C(.14f,.09f,.07f),C(.4f,.62f,.82f)};
        static readonly Color[] SkyBottoms={C(.72f,.8f,.88f),C(.78f,.88f,.86f),C(.98f,.74f,.5f),C(.88f,.92f,.97f),C(.55f,.2f,.09f),C(.2f,.26f,.34f),C(.42f,.3f,.2f),C(.78f,.88f,.86f)};
        bool skyGradient; readonly Dictionary<string,Sprite> gradients=new Dictionary<string,Sprite>();
        Sprite GradientSky(Color top,Color bottom)
        {
            string key=ColorUtility.ToHtmlStringRGB(top)+ColorUtility.ToHtmlStringRGB(bottom);
            if(gradients.TryGetValue(key,out var cached)) return cached;
            var tex=new Texture2D(4,64,TextureFormat.RGBA32,false){ wrapMode=TextureWrapMode.Clamp, filterMode=FilterMode.Bilinear };
            for(int y=0;y<64;y++) { var c=Color.Lerp(bottom,top,Mathf.SmoothStep(0,1,y/63f)); c.a=1; for(int x=0;x<4;x++) tex.SetPixel(x,y,c); }
            tex.Apply(); return gradients[key]=Sprite.Create(tex,new Rect(0,0,4,64),new Vector2(.5f,.5f),64);
        }
        void FitSky()
        {
            if(!sky||!sky.enabled||!sky.sprite) return;
            float h=BoardCamera.orthographicSize*2, w=h*BoardCamera.aspect; var size=sky.sprite.bounds.size;
            // Degradê estica para a tela toda; arte do Codex mantém a proporção e cobre a tela.
            if(skyGradient) { sky.transform.localScale=new Vector3(w/size.x*1.02f,h/size.y*1.02f,1); return; }
            float k=Mathf.Max(w/size.x,h/size.y)*1.02f; sky.transform.localScale=new Vector3(k,k,1);
        }
        // Moedas, armadilhas visíveis para quem olha esta tela, lava, gelo, marcas dos fogos e a prisão dos pesadelos.
        // Evento de campo ativo: a fase loop fica em cena enquanto s.ev durar (inicio e fim tocam nos eventos cataclisma e evento-fim).
        GameObject eventLoop; string eventLoopId;
        void SyncEventLoop(string ev)
        {
            if(eventLoopId==ev||!BoardCamera||!effectRoot) return;
            if(eventLoop) Destroy(eventLoop); eventLoop=null; eventLoopId=ev;
            var frames=string.IsNullOrEmpty(ev)?null:EffectFrames(ev+"/loop"); if(frames==null) return;
            eventLoop=new GameObject("EventoLoop",typeof(SpriteRenderer)); eventLoop.transform.SetParent(effectRoot,false); eventLoop.transform.position=Vector3.up*1.5f;
            eventLoop.transform.localScale=Vector3.one*(boardSize*.9f/Mathf.Max(.01f,frames[0].bounds.size.x));
            var r=eventLoop.GetComponent<SpriteRenderer>(); r.sprite=frames[0]; if(spriteMaterial) r.sharedMaterial=spriteMaterial;
            var loop=eventLoop.AddComponent<SpriteLoop>(); loop.frames=frames; loop.fps=ArenaEfeitos.Fps("Sprites/Efeitos/"+ev+"/loop",10); loop.billboard=true;
        }
        void RefreshMarkers(MatchState s)
        {
            SyncEventLoop(s.ev);
            foreach(var m in markers) Destroy(m); markers.Clear();
            int viewer=Game.ViewerTeam;
            for(int x=0;x<s.width;x++) for(int z=0;z<s.height;z++) {
                var c=new Cell(x,z); var t=s.At(c); var p=Position(s,c);
                if(t.coins>0) for(int i=0;i<t.coins;i++) {
                    var coin=PlaceKit(Mathf.Clamp(s.theme,0,KitFolders.Length-1),"moeda_chao",p+Vector3.up*.045f*i,Hash(x,z+i)%360,world,1.6f);
                    if(coin) coin.name=CoinName(c);
                    markers.Add(coin?coin:Cube(CoinName(c),p+Vector3.up*(.06f+.07f*i),new Vector3(.32f,.05f,.32f),coinMaterial,world));
                }
                // Só quem é aliado do dono vê a armadilha (ownerOnly); a mina de gelo usa a tira de objeto em laço.
                if(t.trap!=0&&(viewer==0||s.Ally(t.trap,viewer))) markers.Add((t.trapPa>0?ObjectLoop("mina-de-gelo-armada",p):null)??Cube("Armadilha",p+Vector3.up*.02f,new Vector3(.55f,.03f,.55f),t.trapPa>0?iceMaterial:trapMaterial,world));
                if(t.Obj==TileObj.Bau&&DungeonProp("chest",p,0,1,world) is GameObject chest) { chest.name=ChestName(c); markers.Add(chest); }
                if(t.lava) markers.Add(Overlay("lava",p,lavaMaterial,.94f));
                if(t.frozenBy!=0) markers.Add(Overlay("gelo",p,iceMaterial,.9f));
                if(t.mark) markers.Add(Overlay("fogos",p,markMaterial,.5f));
            }
            // Portais (sprint 1): o brilho da invocação dos gatos da Asamyh em laço, deitado na entrada e na saída.
            // Na névoa, só a ponta que o time de quem olha já conhece.
            foreach(var g in s.portals??new List<Portal>()) foreach(int i in new[]{g.a,g.b})
                if(!Fog.On(s)||Game.LocalTeam<=0||Fog.Explored(s,Game.LocalTeam,i)) markers.Add(PortalMark(Position(s,new Cell(i%s.width,i/s.width)),i==g.a));
            // Minas terrestres do time: placa menor que a armadilha.
            foreach(int i in s.mines??new List<int>()) if(i>=0&&i<s.tiles.Length&&s.tiles[i].trap!=0&&(viewer==0||s.Ally(s.tiles[i].trap,viewer)))
                markers.Add(Cube("Mina",Position(s,new Cell(i%s.width,i/s.width))+Vector3.up*.05f,new Vector3(.3f,.06f,.3f),markMaterial,world));
            foreach(var f in s.fighters.Where(f=>f.Alive&&f.prisoned>0&&Board.Contains(s,f.cell))) {
                var p=Spot(s,f,f.cell); float h=Heights.TryGetValue(f.def,out float hh)?hh:.75f;
                foreach(var corner in new[]{new Vector3(-.4f,0,-.4f),new Vector3(.4f,0,-.4f),new Vector3(-.4f,0,.4f),new Vector3(.4f,0,.4f)})
                    markers.Add(Cube("Prisao",p+corner+Vector3.up*h*.5f,new Vector3(.08f,h,.08f),cageMaterial,world));
                markers.Add(Cube("Prisao",p+Vector3.up*h,new Vector3(.9f,.06f,.9f),cageMaterial,world));
            }
        }
        GameObject ObjectLoop(string id,Vector3 p)
        {
            var frames=ArenaEfeitos.Frames("Sprites/Objetos/"+id+"/idle"); if(frames==null) return null;
            var go=new GameObject("Armadilha",typeof(SpriteRenderer)); go.transform.SetParent(world,false); go.transform.position=p+Vector3.up*.02f;
            var r=go.GetComponent<SpriteRenderer>(); r.sprite=frames[0]; if(spriteMaterial) r.sharedMaterial=spriteMaterial;
            var loop=go.AddComponent<SpriteLoop>(); loop.frames=frames; loop.fps=ArenaEfeitos.Fps("Sprites/Objetos/"+id+"/idle",8); loop.billboard=true; return go;
        }
        GameObject PortalMark(Vector3 p,bool entrance)
        {
            var frames=EffectFrames("chamar-gato/conjuracao")??EffectFrames("chamar-gato/impacto");
            if(frames==null) return Cube(entrance?"Portal_entrada":"Portal_saida",p+Vector3.up*.02f,new Vector3(.8f,.02f,.8f),entrance?markMaterial:iceMaterial,world);
            var go=new GameObject(entrance?"Portal_entrada":"Portal_saida",typeof(SpriteRenderer)); go.transform.SetParent(world,false); go.transform.position=p+Vector3.up*.03f; go.transform.rotation=Quaternion.Euler(90,0,0);
            var r=go.GetComponent<SpriteRenderer>(); r.sprite=frames[0]; if(spriteMaterial) r.sharedMaterial=spriteMaterial; r.color=entrance?C(1,.75f,1):C(.7f,.9f,1);
            go.transform.localScale=Vector3.one*(.95f/Mathf.Max(.01f,frames[0].bounds.size.x));
            if(frames.Length>1) { var loop=go.AddComponent<SpriteLoop>(); loop.frames=frames; loop.fps=10; }
            return go;
        }
        // Casa coberta: arte do Codex em Sprites/Efeitos/<id>/loop (deitada no chão) ou uma placa colorida.
        GameObject Overlay(string id,Vector3 p,Material fallback,float size)
        {
            var frames=EffectFrames(id+"/loop");
            if(frames==null) return Cube("Casa_"+id,p+Vector3.up*.015f,new Vector3(size,.02f,size),fallback,world);
            var go=new GameObject("Casa_"+id,typeof(SpriteRenderer)); go.transform.SetParent(world,false); go.transform.position=p+Vector3.up*.02f; go.transform.rotation=Quaternion.Euler(90,0,0);
            var r=go.GetComponent<SpriteRenderer>(); r.sprite=frames[0]; if(spriteMaterial) r.sharedMaterial=spriteMaterial;
            go.transform.localScale=Vector3.one*(size/Mathf.Max(.01f,frames[0].bounds.size.x));
            // Laço enquanto a casa estiver coberta (lava, gelo, fogos): antes só o primeiro quadro aparecia.
            if(frames.Length>1) { var loop=go.AddComponent<SpriteLoop>(); loop.frames=frames; loop.fps=ArenaEfeitos.Fps("Sprites/Efeitos/"+id+"/loop",10); }
            return go;
        }
        // Efeito do Codex (Sprites/Efeitos/<id>) ou um clarão na cor dada.
        // phase: fase preferida quando <id> é uma pasta (conjuracao no lançador, impacto no destino); cai em uso.
        IEnumerator Fx(string id,Vector3 at,Color fallback,float wait=.3f,string phase="impacto")
        {
            Sprite[] frames=null; string res=null;
            foreach(var p in new[]{id,id+"/"+phase,id+"/uso"}) { frames=EffectFrames(p); if(frames!=null) { res="Sprites/Efeitos/"+p; break; } }
            if(frames!=null) yield return PlayEffect(frames,at,ArenaEfeitos.Fps(res,10));
            else { Flash(at,fallback); yield return Wait(wait); }
        }
        IEnumerator Slide(Transform unit,MatchState s,Cell[] path)
        {
            if(!unit||path==null||path.Length<2) yield break;
            for(int i=1;i<path.Length;i++) {
                Vector3 a=Position(s,path[i-1]), b=Position(s,path[i]);
                for(float t=0;t<.09f;t+=Time.deltaTime) { unit.position=Vector3.Lerp(a,b,t/.09f); yield return null; }
            }
            unit.position=Position(s,path[path.Length-1]);
        }
        static string CoinName(Cell c) => "Moeda_"+c.x+"_"+c.z;
        static string ChestName(Cell c) => "Bau_"+c.x+"_"+c.z;
        void RemoveCoins(Cell c) { string name=CoinName(c); for(int i=markers.Count-1;i>=0;i--) if(markers[i]&&markers[i].name==name) { Destroy(markers[i]); markers.RemoveAt(i); } }
        public void FitCamera()
        {
            if(!BoardCamera) return;
            BoardCamera.transform.position=CameraHome; BoardCamera.transform.LookAt(Vector3.zero);
            // A câmera desenha a tela inteira (o céu vai até as bordas); o tabuleiro cabe na parte livre e é deslocado
            // para o centro dela, já que painel e faixas ficam por cima.
            BoardCamera.rect=new Rect(0,0,1,1);
            var area=ArenaHud.BoardArea();
            float aspect=area.width/Mathf.Max(1,area.height);
            float halfW=0,halfH=0;
            for(int x=-1;x<=1;x+=2) for(int z=-1;z<=1;z+=2) for(int y=0;y<=1;y++) {
                var p=BoardCamera.transform.InverseTransformPoint(new Vector3(x*(boardSize*.5f+.4f),y*3.2f,z*(boardSize*.5f+.4f))); halfW=Mathf.Max(halfW,Mathf.Abs(p.x)); halfH=Mathf.Max(halfH,Mathf.Abs(p.y));
            }
            // orthographicSize mede a tela inteira; reserve também a fração coberta pela HUD.
            BoardCamera.orthographicSize=fitSize=Mathf.Max(halfH,halfW/aspect)*Screen.height/Mathf.Max(1,area.height)*1.06f;
            var cam=BoardCamera.transform; Vector2 d=area.center-new Vector2(Screen.width,Screen.height)*.5f;
            homeShift=-(cam.right*d.x+cam.up*d.y)*(2*fitSize/Mathf.Max(1,Screen.height)); cam.position=Home;
            screenWidth=Screen.width; screenHeight=Screen.height;
        }

        // Labirinto (névoa) no Host ou jogo local: a tela desenha o que o time de quem joga aqui conhece (Match.ViewFor), igual
        // ao que o cliente recebe. O cliente e as vistas (rng 0) já chegam filtrados.
        MatchState seenFrom, seenView; int seenRevision=-1, seenTeam=-1;
        public MatchState Seen(MatchState s)
        {
            if(s==null||!s.fog||s.rng==0||Game.Client||Game.EditingMap) return s;
            int team=Game.LocalTeam; if(team<=0) return s;
            if(seenFrom==s&&seenRevision==s.revision&&seenTeam==team&&seenView!=null) return seenView;
            seenFrom=s; seenRevision=s.revision; seenTeam=team; return seenView=Match.ViewFor(s,team);
        }
        // Estado final aplicado sem animação (snapshot, reset, fim da fila).
        public void Sync(MatchState state,bool immediate)
        {
            state=Seen(state);
            if(!Ready||state==null) return;
            if(immediate) CancelAnimations();
            BuildBoard(state); RefreshMarkers(state);
            foreach(var f in state.fighters) {
                if(!Board.Contains(state,f.cell)) { if(units.TryGetValue(f.id,out var concealed)) concealed.gameObject.SetActive(false); continue; }
                bool fresh=false;
                if(!units.TryGetValue(f.id,out var root)) { if(!f.Alive) continue; root=units[f.id]=CreateUnit(f); fresh=true; }
                root.gameObject.SetActive(f.Alive);
                root.position=Spot(state,f,f.cell);
                if(fresh) Turn(root,-root.position);
                if(!fresh&&f.Alive&&root.childCount>0&&rigs.TryGetValue(root.GetChild(0),out var shown)&&shown.folder!=SpriteFolder(f)&&LoadRig(SpriteFolder(f))!=null) { rigs.Remove(root.GetChild(0)); Destroy(root.gameObject); root=units[f.id]=CreateUnit(f); root.position=Spot(state,f,f.cell); Turn(root,-root.position); }
                else if(root.childCount>0&&rigs.TryGetValue(root.GetChild(0),out var rig)) rig.pose=Content.Form(f)?.pose;
            }
            foreach(var id in units.Keys.Where(id=>state.F(id)==null).ToList()) { if(units[id].childCount>0) rigs.Remove(units[id].GetChild(0)); Destroy(units[id].gameObject); units.Remove(id); }
            RefreshHidden(state); shownKey=null;
        }
        // Fila de animações a 50% (muitas ações seguidas chegando): pula as que faltam e mostra direto o estado mais novo.
        public const int QueueCapacity=8;
        public void Play(List<GameEvent> events,MatchState state)
        {
            if(Ready&&queue.Count>=QueueCapacity/2) { Sync(state,true); return; }
            queue.Enqueue((events,Seen(state)));
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
            // Identidade mostrada: o clone ilusório usa o modelo/altura/figura do disfarce (quem conjurou).
            string shown=ShownId(f);
            float height=Heights.TryGetValue(shown,out float h)?h:.75f;
            string model=shown=="ciantia"?"CiantiaChibi3D":shown=="dragao-espectral"?"DragaoTransplanar3D":null;
            var prefab=model==null?null:Resources.Load<GameObject>("Models/"+model);
            string folder=SpriteFolder(f);
            var rig=folder==null?null:LoadRig(folder);
            bool sprite=rig!=null;
            Transform visual;
            if(sprite) {
                var renderer=rig.renderer=new GameObject("Sprite",typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
                renderer.sprite=rig.still[0]; if(spriteMaterial) renderer.sharedMaterial=spriteMaterial; visual=renderer.transform; visual.SetParent(root,false);
                rig.pose=Content.Form(f)?.pose; rigs[visual]=rig;
                if(silhouetteMaterial) { rig.ghost=new GameObject("Silhueta",typeof(SpriteRenderer)).GetComponent<SpriteRenderer>(); rig.ghost.sharedMaterial=silhouetteMaterial; rig.ghost.color=new Color(TeamColor(f.team).r,TeamColor(f.team).g,TeamColor(f.team).b,.75f); rig.ghost.transform.SetParent(visual,false); }
            }
            else if(!prefab&&Game.Hud?.ShopIcon(shown) is Sprite icon) {
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
            // Sprite: encara a câmera de frente (inclinado com ela: não achata nem atravessa blocos atrás); a direção do lutador escolhe o quadro.
            visual.localRotation=sprite?BoardCamera.transform.rotation:Quaternion.Euler(0,14.3f,0);
            if(Shown(f).flying) visual.localPosition+=Vector3.up*.35f;
            if(sprite) { lifts[visual]=Shown(f).flying?.35f:0; Plant(visual); }
            foreach(var t in visual.GetComponentsInChildren<Transform>()) if(t.name.StartsWith("Wing")||t.name.StartsWith("Asa")) { wings.Add(t); wingRest[t]=t.localRotation; }
            // Indicador do pé: cor exata da facção. O clone ilusório não se tinge de violeta (isso o denunciaria);
            // só as demais invocações continuam puxando para o violeta.
            var color=TeamColor(f.team); if(f.summoner!=0&&!Veiled(f)) color=Color.Lerp(color,Violet,.45f);
            if(rigs.TryGetValue(visual,out var made)&&made.ghost) made.ghost.color=new Color(color.r,color.g,color.b,.62f);
            float side=Board.SizeOf(f)-.33f; Cube("Marcador",new Vector3(0,.018f,0),new Vector3(side,.028f,side),Material(color),root);
            root.gameObject.layer=8;
            root.gameObject.AddComponent<ArenaUnitPick>().Id=f.id;
            var collider=root.gameObject.AddComponent<CapsuleCollider>();
            collider.height=height; collider.radius=.38f; collider.center=Vector3.up*height*.5f; collider.isTrigger=true;
            return root;
        }
        string Name(MatchState s,int id) { var f=s.F(id); return f==null?"?":Shown(f).name+(f.team==MatchState.Neutral?" (neutro)":" (P"+f.team+")"); }
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
                    // Comando de andar (Mover gato): conjuração em quem manda, impacto onde a invocação chega.
                    if(e.info!=null&&s.F(e.actor) is Fighter walker&&units.TryGetValue(walker.summoner,out var boss)&&boss) PhaseFx(e.info+"/conjuracao",boss.position+Vector3.up*.9f);
                    if(actor&&e.path!=null&&e.path.Length>1) yield return Walk(actor,s,e.path,s.F(e.actor));
                    if(actor&&e.info!=null) PhaseFx(e.info+"/impacto",actor.position+Vector3.up*.3f);
                    break;
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
                        // Skill devorada pelo Colosso usa o projétil e as partículas da original.
                        string look=Content.BaseSkill(e.info);
                        bool burstStart=main||burst!=(e.actor,e.info); burst=(e.actor,e.info);
                        if(main&&Vector3.Distance(Flat(actor.position),Flat(target.position))>1.5f) yield return Shot(look,actor.position,target.position,dealt?.area??0);
                        // Corpo a corpo e área: a fase de impacto toca no alvo e o dano entra no quadro da colisão (impactFrame0).
                        else if(e.value>=0&&ImpactPhase(look,out var blowRes) is Sprite[] blow) {
                            StartCoroutine(PlayEffect(blow,target.position+Vector3.up*.7f,ArenaEfeitos.Fps(blowRes,12)));
                            if(burstStart&&ArenaEfeitos.ImpactTime(blowRes) is float lag&&lag>0) yield return Wait(lag);
                        }
                        Sound("attack");
                        if(e.value<0) Float(target.position,"Errou!",Blue);
                        else if(e.value>0||(dealt?.damage??1)>0) { Hit(target,actor.position,e.value,Shots.TryGetValue(look??"",out var hue)?hue.color:C(1,.85f,.6f)); Float(target.position,"−"+e.value,Gold); Turn(target,actor.position-target.position); PlayClip(target,"hit"); }
                        if(swing>0) yield return Wait(swing*.4f); else if(main) yield return Pulse(actor);
                    }
                    else if(actor&&e.target==0&&Board.Contains(s,e.cell)) {
                        var ground=Position(s,e.cell); var miss=Content.AnySkill(e.info);
                        if(e.dice!=null) { Game.Hud?.RollDice(e.dice,new[]{"Ataque","Defesa"}); yield return Wait(ArenaHud.DiceLand); }
                        Turn(actor,ground-actor.position); float cast=PlayClip(actor,miss?.anim??"attack");
                        if(cast==0) cast=PlayClip(actor,"attack"); if(cast>0) yield return Wait(cast*.5f);
                        if(Vector3.Distance(Flat(actor.position),Flat(ground))>1.5f) yield return Shot(Content.BaseSkill(e.info),actor.position,ground,miss?.area??0);
                        else { PhaseFx(Content.BaseSkill(e.info)+"/impacto",ground+Vector3.up*.3f); Flash(ground+Vector3.up*.3f,Blue); }
                        Sound("attack"); Float(ground,miss?.kind==SkillKind.Dano?(miss.maxRange>1?"Disparo":"Golpe"):"Conjuração",Blue); if(cast>0) yield return Wait(cast*.4f);
                    }
                    Game.Hud?.Log(e.target==0?$"{Name(s,e.actor)}: {Content.AnySkill(e.info)?.name} no chão":e.value<0?$"{Name(s,e.actor)} errou {Name(s,e.target)} [{Dice(e.dice)}]":$"{Name(s,e.actor)} → {Name(s,e.target)}: {e.value} de dano{(e.dice!=null?$" [{Dice(e.dice)}]":"")}");
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
                case "troca":
                {
                    // Shambles: os dois somem e reaparecem um no lugar do outro.
                    units.TryGetValue(e.target,out var a); units.TryGetValue(e.value,out var b);
                    if(e.path!=null&&e.path.Length==2) {
                        var pa=Position(s,e.path[0]); var pb=Position(s,e.path[1]);
                        if(actor) PlayClip(actor,Content.AnySkill(e.info)?.anim??"attack");
                        Flash(pa+Vector3.up,Blue); Flash(pb+Vector3.up,Blue); yield return Wait(.25f);
                        if(a) a.position=pb; if(b) b.position=pa; Flash(pa+Vector3.up,Blue); Flash(pb+Vector3.up,Blue); yield return Wait(.25f);
                    }
                    Game.Hud?.Log($"{Name(s,e.actor)}: {Content.AnySkill(e.info)?.name} ({Name(s,e.target)} ⇄ {Name(s,e.value)})");
                    break;
                }
                case "portal-aberto":
                    if(actor) PlayClip(actor,Content.AnySkill(e.info)?.anim??"attack");
                    if(e.path!=null) foreach(var c in e.path) PhaseFx("chamar-gato/conjuracao",Position(s,c)+Vector3.up*.3f);
                    Game.Hud?.Log($"{Name(s,e.actor)}: portal aberto por {e.value} turnos"); yield return Wait(.35f);
                    break;
                case "portal":
                    if(actor&&e.path!=null&&e.path.Length==2) {
                        var from=Position(s,e.path[0]); var to=Position(s,e.path[1]);
                        PhaseFx("chamar-gato/impacto",from+Vector3.up*.5f); yield return Wait(.2f);
                        actor.position=to; PhaseFx("chamar-gato/impacto",to+Vector3.up*.5f); yield return Wait(.2f);
                    }
                    Game.Hud?.Log($"{Name(s,e.actor)} atravessou o portal");
                    break;
                case "portal-fechado": Game.Hud?.Log($"Portal de {Name(s,e.actor)} fechou"); break;
                case "forma":
                    if(actor) {
                        // info é a forma de destino: o clipe vem da skill que leva a ela (Damian: morph); a volta usa transform.
                        var who=s.F(e.actor); var formSkill=who==null||e.info==null?null:Content.Def(who).skills.FirstOrDefault(k=>k.kind==SkillKind.Forma&&k.toForm==e.info);
                        string anim=formSkill?.anim;
                        PhaseFx(ArenaEfeitos.SkillPhase(formSkill?.id,"conjuracao"),actor.position+Vector3.up*.9f);
                        float morph=PlayClip(actor,anim??"transform");
                        if(morph==0) morph=PlayClip(actor,"transform");
                        // A volta do Berserk usa a transformação normal ao contrário quando a forma atual não tem clipe.
                        if(morph==0&&who!=null&&actor.childCount>0&&rigs.TryGetValue(actor.GetChild(0),out var previous)&&previous.folder!=SpriteFolder(who)&&LoadRig(SpriteFolder(who))!=null) {
                            var position=actor.position; rigs.Remove(actor.GetChild(0)); Destroy(actor.gameObject);
                            actor=units[who.id]=CreateUnit(who); actor.position=position; Turn(actor,-position);
                            morph=PlayClip(actor,"transform",reverse:true);
                        }
                        Flash(actor.position+Vector3.up,e.info=="berserk"?Red:Violet);
                        if(morph>0) yield return Wait(morph); else yield return Pulse(actor);
                        PhaseFx(ArenaEfeitos.SkillPhase(formSkill?.id,"impacto"),actor.position+Vector3.up*.7f);
                        // Forma com pasta própria de sprites (Berserk): troca o boneco.
                        if(who!=null&&units[who.id].childCount>0&&rigs.TryGetValue(units[who.id].GetChild(0),out var old)&&old.folder!=SpriteFolder(who)&&LoadRig(SpriteFolder(who))!=null) {
                            rigs.Remove(units[who.id].GetChild(0)); Destroy(units[who.id].gameObject);
                            var fresh=units[who.id]=CreateUnit(who); fresh.position=Spot(s,who,who.cell); Turn(fresh,-fresh.position);
                        }
                        // Sem pasta própria (Damian): a pose parada passa a segurar a arma da forma.
                        else if(who!=null&&units[who.id].childCount>0&&rigs.TryGetValue(units[who.id].GetChild(0),out var same)) same.pose=Content.Form(who)?.pose;
                    }
                    Game.Hud?.Log($"{Name(s,e.actor)}: {(e.info==null?"forma normal":Content.Def(s.F(e.actor)).forms?.FirstOrDefault(x=>x.id==e.info)?.name??e.info)}");
                    break;
                case "esconder": if(actor) { PlayClip(actor,"hide"); PhaseFx(ArenaEfeitos.UsePhase(e.kind,e.info),actor.position+Vector3.up*.5f); Flash(actor.position+Vector3.up*.5f,Violet); yield return Wait(.4f); } RefreshHidden(s); Game.Hud?.Log($"{Name(s,e.actor)} se escondeu"); break;
                case "revelar":
                    if(!target&&s.F(e.target) is Fighter revealed&&Board.Contains(s,revealed.cell)) target=units[e.target]=CreateUnit(revealed);
                    if(target) { target.gameObject.SetActive(true); target.position=Position(s,e.cell); }
                    RefreshHidden(s); if(target) { Flash(target.position+Vector3.up,Violet); Float(target.position,"Revelado!",Violet); } yield return Wait(.25f); break;
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
                case "aura":
                    // Dualidade guardiã: anel do alcance ao ligar/manter (o anjo e o demônio orbitam em UpdateCompanions).
                    if(actor&&e.info!="desligar") { StartCoroutine(Ring(actor.position+Vector3.up*.05f,e.value,C(1,.85f,.45f))); Flash(actor.position+Vector3.up,Gold); yield return Wait(.35f); }
                    else if(actor) Flash(actor.position+Vector3.up,Violet,10);
                    Game.Hud?.Log($"{Name(s,e.actor)}: Dualidade guardiã {(e.info=="ativar"?"ligada":e.info=="manter"?"mantida (1 PA)":"desligada")}");
                    break;
                case "cura":
                    if(actor&&e.info=="totem-de-cura"&&!Playing(actor,"heal")) PlayClip(actor,"heal"); // um pulso do totem cura vários aliados
                    // Aura de gatinho e curas com arte: conjuração em quem cura, impacto no alvo; o número entra na colisão.
                    if(target) {
                        PhaseFx(ArenaEfeitos.UsePhase(e.kind,e.info),target.position+Vector3.up*.6f);
                        // Dreno (cura vinda de golpe) não repete a arte do golpe.
                        bool mendArt=e.info!=null&&Content.AnySkill(e.info) is SkillDef mendSkill&&mendSkill.kind!=SkillKind.Dano;
                        if(mendArt&&actor&&actor!=target&&e.info!="totem-de-cura") PhaseFx(e.info+"/conjuracao",actor.position+Vector3.up*.9f);
                        if(mendArt&&PhaseFx(e.info+"/impacto",target.position+Vector3.up*.6f)>0&&ArenaEfeitos.ImpactTime("Sprites/Efeitos/"+e.info+"/impacto") is float mend&&mend>0) yield return Wait(mend);
                        else Flash(target.position+Vector3.up,Green);
                        Float(target.position,"+"+e.value,Green);
                    }
                    else if(Board.Contains(s,e.cell)) { if(actor) PlayClip(actor,"heal"); PhaseFx(Content.BaseSkill(e.info)+"/impacto",Position(s,e.cell)+Vector3.up*.3f); Flash(Position(s,e.cell)+Vector3.up*.3f,Green); }
                    yield return Wait(.4f); break;
                case "armadilha":
                    if(target) { Hit(target,target.position+Vector3.back,e.value,Red); Float(target.position,"−"+e.value,Red); PlayClip(target,"hit"); }
                    Game.Hud?.Log($"{Name(s,e.target)} pisou numa armadilha: {e.value} de dano");
                    yield return Wait(.45f); break;
                case "armadilha-posta": PhaseFx(ArenaEfeitos.UsePhase(e.kind,e.info),Position(s,e.cell)+Vector3.up*.05f); Float(Position(s,e.cell),"Armadilha",Red); yield return Wait(.2f); break;
                case "invocar":
                    var f=s.F(e.target);
                    if(f!=null&&!units.ContainsKey(f.id)) {
                        float cast=0; if(actor) { Turn(actor,Position(s,f.cell)-actor.position); cast=PlayClip(actor,"summon"); }
                        // Skill que invocou (Chamar gato, Noite dos Gatos): conjuração no lançador e impacto na casa, antes de surgir.
                        string by=s.F(e.actor)?.lastSkill; float call=0;
                        PhaseFx(ArenaEfeitos.UsePhase(e.kind,by),Position(s,f.cell)+Vector3.up*.4f);
                        if(actor&&by!=null) { PhaseFx(by+"/conjuracao",actor.position+Vector3.up*.9f); if(EffectFrames(by+"/impacto")!=null) { PhaseFx(by+"/impacto",Position(s,f.cell)+Vector3.up*.4f); call=ArenaEfeitos.ImpactTime("Sprites/Efeitos/"+by+"/impacto"); } }
                        cast=Mathf.Max(cast*.6f,call); if(cast>0) yield return Wait(cast);
                        var u=units[f.id]=CreateUnit(f); u.position=Spot(s,f,f.cell); if(actor) Turn(u,u.position-actor.position);
                        float rise=PlayClip(u,"spawn"); // quem tem arte de surgimento usa a arte; os outros pulsam
                        if(rise>0) yield return Wait(rise); else yield return Pulse(u);
                    }
                    Game.Hud?.Log(e.actor==0?$"Surgiu {Name(s,e.target)}":$"{Name(s,e.actor)} invocou {Name(s,e.target)}");
                    break;
                case "nocaute":
                    Sound("death"); if(target) { Flash(target.position+Vector3.up,Violet); float fall=PlayClip(target,"death"); yield return Wait(Mathf.Max(.3f,fall+.25f)); target.gameObject.SetActive(false); }
                    Game.Hud?.Log(e.info=="renasce"?$"{Name(s,e.target)} caiu…":$"{Name(s,e.target)} foi nocauteado");
                    break;
                case "cataclisma":
                    Game.Hud?.Log("CATACLISMA: "+EventName(e.info)+(e.value>0?$" ({e.value} rodada{(e.value>1?"s":"")})":""));
                    Float(Vector3.up*2,EventName(e.info),Violet);
                    var before=new HashSet<string>(markers.Where(g=>g).Select(g=>g.name));
                    Sound("breath"); BuildBoard(s); RefreshMarkers(s); ApplySky(s);
                    if(EffectFrames(e.info+"/inicio") is Sprite[] intro) StartCoroutine(PlayEffect(intro,Vector3.up*1.5f,ArenaEfeitos.Fps("Sprites/Efeitos/"+e.info+"/inicio",10),boardSize*.9f));
                    Game.Hud?.ShowWarning(e.info);
                    // Chuva de moedas: as novas caem do céu.
                    foreach(var m in markers) if(m&&m.name.StartsWith("Moeda")&&!before.Contains(m.name)) StartCoroutine(Drop(m.transform,UnityEngine.Random.Range(0,.4f))); foreach(var x in s.fighters) if(x.Alive&&units.TryGetValue(x.id,out var r)) r.position=Spot(s,x,x.cell);
                    yield return Shake(); break;
                case "compra":
                    // Moedas saem de quem comprou e a figura do produto aparece sobre a cabeça.
                    if(actor) {
                        for(int i=0;i<Mathf.Min(e.value,5);i++) { var up=actor.position+Vector3.up*1.3f; StartCoroutine(Coin(up,up+Vector3.up*1.1f+new Vector3(UnityEngine.Random.Range(-.5f,.5f),0,UnityEngine.Random.Range(-.5f,.5f)),i*.07f,.5f)); }
                        Float(actor.position,"−"+e.value+" moedas",Gold);
                        var icon=Game.Hud?.ShopIcon(e.info); if(icon) StartCoroutine(ShowItem(icon,actor.position+Vector3.up*1.9f));
                        PhaseFx(ArenaEfeitos.UsePhase(e.kind,e.info),actor.position+Vector3.up*.9f);
                        yield return Wait(.6f);
                    }
                    Game.Hud?.Log($"{Name(s,e.actor)} comprou {Shop.Def(e.info)?.name} por {e.value}"); break;
                // ── Eventos de 10/10: área em volta de si, zonas, comandos, controle, suporte, Desejo, neutros e eventos de mapa ──
                case "area":
                    burst=default;
                    if(actor) {
                        if(e.dice!=null) { Game.Hud?.RollDice(e.dice,new[]{"Ataque","Defesa"}); yield return Wait(ArenaHud.DiceLand); }
                        var sk=Content.AnySkill(e.info); float sw=PlayClip(actor,sk?.anim??"attack"); if(sw==0) sw=PlayClip(actor,"attack");
                        StartCoroutine(Ring(actor.position+Vector3.up*.05f,e.value,Shots.TryGetValue(Content.BaseSkill(e.info),out var ring)?ring.color:Gold));
                        StartCoroutine(Fx(Content.BaseSkill(e.info),actor.position+Vector3.up*.6f,Gold,.3f,"conjuracao")); Sound("attack");
                        yield return Wait(sw>0?sw*.55f:.35f);
                    }
                    Game.Hud?.Log($"{Name(s,e.actor)}: {Content.AnySkill(e.info)?.name}");
                    break;
                case "zona":
                    if(actor) { Turn(actor,Position(s,e.cell)-actor.position); PlayClip(actor,Content.AnySkill(e.info)?.anim??"attack"); PhaseFx(ArenaEfeitos.SkillPhase(e.info,"conjuracao"),actor.position+Vector3.up*.9f); }
                    if(e.path!=null) foreach(var c in e.path) StartCoroutine(Fx(e.info,Position(s,c)+Vector3.up*.1f,C(.7f,.9f,1),.1f));
                    RefreshMarkers(s); yield return Wait(.45f);
                    Game.Hud?.Log($"{Name(s,e.actor)}: {Content.AnySkill(e.info)?.name}");
                    break;
                case "comando":
                {
                    // Faísca da varinha: conjuração em quem manda e impacto no gato; o disparo do gato sai no quadro da colisão.
                    string order=s.F(e.actor)?.lastSkill; float spark=0;
                    if(target) {
                        if(actor) { PlayClip(actor,"summon"); if(order!=null) PhaseFx(order+"/conjuracao",actor.position+Vector3.up*.9f); }
                        if(order!=null&&PhaseFx(order+"/impacto",target.position+Vector3.up*.5f)>0) spark=ArenaEfeitos.ImpactTime("Sprites/Efeitos/"+order+"/impacto");
                        else Flash(target.position+Vector3.up*.4f,Violet,10);
                    }
                    yield return Wait(Mathf.Max(.2f,spark)); break;
                }
                case "sumir":
                    if(target) { Flash(target.position+Vector3.up*.5f,Violet,12); yield return Wait(.2f); target.gameObject.SetActive(false); }
                    break;
                case "empurrao": case "puxao":
                    if(target) yield return Slide(target,s,e.path);
                    if(e.kind=="puxao"&&target) Float(target.position,"Puxado!",Blue);
                    break;
                case "colisao": if(target) { Float(target.position,"−1 batida",Red); PlayClip(target,"hit"); } yield return Shake(.12f,.25f); break;
                case "firme": if(target) Float(target.position,"Firme!",Blue); yield return Wait(.25f); break;
                case "prisao":
                    if(target) { yield return Fx(e.info,target.position+Vector3.up*.7f,Violet); Float(target.position,"Preso!",Violet); }
                    RefreshMarkers(s); Game.Hud?.Log($"{Name(s,e.target)} ficou preso por {e.value} turno"); break;
                case "preso": if(target) Float(target.position,"Preso: perde o turno",Violet); Game.Hud?.Log($"{Name(s,e.target)} perdeu o turno na prisão"); yield return Wait(.4f); break;
                case "escapou": if(target) Float(target.position,"Escapou!",Green); yield return Wait(.3f); break;
                case "buff":
                    // Efeito de uso sobre o alvo acompanha a visibilidade dele: unidade escondida do inimigo não é revelada pela sobreposição.
                    if(target) { var tf=s.F(e.target); if(tf==null||ArenaEfeitos.Visible("Sprites/Efeitos/"+e.info+"/uso",true,tf.hidden,s.Ally(tf.team,Game.LocalTeam))) StartCoroutine(Fx(e.info+"/uso",target.position+Vector3.up*.7f,Green,.1f)); Float(target.position,e.value>0?$"+{e.value} armadura":"Fortalecido",Green); }
                    yield return Wait(.3f); break;
                case "suporte":
                    if(actor) {
                        float sp=PlayClip(actor,Content.AnySkill(e.info)?.anim??"summon"); if(e.value>0) StartCoroutine(Ring(Position(s,e.cell)+Vector3.up*.05f,e.value,Green));
                        // Noite dos Gatos e outros suportes com arte: conjuração em si, impacto no centro da área.
                        PhaseFx(e.info+"/conjuracao",actor.position+Vector3.up*.9f);
                        float land=PhaseFx(e.info+"/impacto",Position(s,e.cell)+Vector3.up*.3f)>0?ArenaEfeitos.ImpactTime("Sprites/Efeitos/"+e.info+"/impacto"):0;
                        yield return Wait(Mathf.Max(sp>0?sp*.5f:.3f,land));
                    }
                    Game.Hud?.Log($"{Name(s,e.actor)}: {Content.AnySkill(e.info)?.name}");
                    break;
                case "reviver":
                {
                    var who=s.F(e.target);
                    if(who!=null) {
                        if(!units.TryGetValue(who.id,out var u)) u=units[who.id]=CreateUnit(who);
                        u.gameObject.SetActive(true); u.position=Position(s,e.cell);
                        yield return Fx(e.info,u.position+Vector3.up*.8f,Green); Float(u.position,"Reviveu!",Green); PlayClip(u,"idle");
                    }
                    Game.Hud?.Log($"{Name(s,e.target)} voltou com {e.value} PV");
                    break;
                }
                case "desejo":
                    if(actor) {
                        bool start=string.IsNullOrEmpty(e.info); float w=PlayClip(actor,start?"wishchannel":"wish"); if(w==0) w=PlayClip(actor,"summon");
                        yield return Fx(start?"desejo/inicio":"desejo-"+e.info,actor.position+Vector3.up*1.2f,Gold);
                        Float(actor.position,start?"Desejo: canalizando…":Content.AnySkill("desejo-"+e.info)?.name??"Desejo",Gold);
                    }
                    Game.Hud?.Log(string.IsNullOrEmpty(e.info)?$"{Name(s,e.actor)} começou o Desejo (2 turnos)":$"{Name(s,e.actor)}: {Content.AnySkill("desejo-"+e.info)?.name}");
                    break;
                case "canalizar": if(actor) { PlayClip(actor,"wishchannel"); Float(actor.position,"Canalizando…",Gold); } yield return Wait(.3f); break;
                case "desejo-pronto": if(actor) { Flash(actor.position+Vector3.up,Gold); Float(actor.position,"Desejo pronto!",Gold); } Game.Hud?.Log($"{Name(s,e.actor)}: escolha o Desejo"); yield return Wait(.4f); break;
                case "aprender": if(actor) Float(actor.position,"Copiou "+Content.AnySkill(e.info)?.name,Violet); Game.Hud?.Log($"{Name(s,e.actor)} copiou {Content.AnySkill(e.info)?.name}"); yield return Wait(.4f); break;
                case "devorar":
                    if(actor) { if(target) StartCoroutine(Coin(target.position+Vector3.up,actor.position+Vector3.up*1.5f,0,.5f)); yield return Fx("devorar",actor.position+Vector3.up*1.2f,Violet); Float(actor.position,"Devorou "+Content.AnySkill(e.info)?.name,Violet); }
                    Game.Hud?.Log($"{Name(s,e.actor)} devorou {Content.AnySkill(e.info)?.name}"); break;
                case "recompensa":
                    if(actor) {
                        for(int i=0;i<Mathf.Min(e.value,6);i++) StartCoroutine(Coin(Position(s,e.cell)+Vector3.up*.6f,actor.position+Vector3.up*1.3f,i*.08f,.6f));
                        Float(actor.position,$"+{e.value} moedas",Gold);
                        if(Game.Hud?.ShopIcon(e.info) is Sprite prize) StartCoroutine(ShowItem(prize,actor.position+Vector3.up*1.9f));
                        yield return Wait(.6f);
                    }
                    Game.Hud?.Log($"{Name(s,e.actor)} ganhou {e.value} moedas{(e.info!=null?" e "+Shop.Def(e.info)?.name:"")}"); break;
                case "lava": if(target) { Hit(target,target.position+Vector3.down,e.value,C(1,.45f,.1f)); Float(target.position,$"−{e.value} lava",Red); PlayClip(target,"hit"); } yield return Wait(.3f); break;
                case "espetos": if(Board.Contains(s,e.cell)) StartCoroutine(RiseSpikes(Position(s,e.cell)));
                    if(target) { Hit(target,target.position+Vector3.down,e.value,C(.8f,.8f,.85f)); Float(target.position,$"−{e.value} espetos",Red); PlayClip(target,"hit"); } yield return Wait(.25f); break;
                case "bau":
                {
                    string name=ChestName(e.cell); for(int i=markers.Count-1;i>=0;i--) if(markers[i]&&markers[i].name==name) { Destroy(markers[i]); markers.RemoveAt(i); }
                    for(int i=0;i<Mathf.Min(e.value,5);i++) StartCoroutine(Coin(Position(s,e.cell)+Vector3.up*.3f,actor?actor.position+Vector3.up*1.3f:Position(s,e.cell)+Vector3.up*1.5f,i*.09f,.6f));
                    Float(Position(s,e.cell),$"Baú! +{e.value} moedas",Gold); Game.Hud?.Log($"{Name(s,e.actor)} abriu um baú: +{e.value} moedas"); yield return Wait(.45f); break;
                }
                case "porta": BuildBoard(s); RefreshHidden(s); Float(Position(s,e.cell)+Vector3.up*.6f,"Porta aberta",Gold); Game.Hud?.Log($"{Name(s,e.actor)} abriu uma porta"); yield return Wait(.3f); break;
                case "vela":
                    Flash(Position(s,e.cell)+Vector3.up*.8f,Gold,24); RefreshHidden(s);
                    Float(Position(s,e.cell)+Vector3.up*.8f,$"Vela acesa! +{e.value} moedas",Gold); Game.Hud?.Log($"{Name(s,e.actor)} acendeu a vela: o mapa todo à vista (+{e.value} moedas)"); yield return Wait(.5f); break;
                case "braseiro": if(actor) Float(actor.position,"+1 PA (braseiro)",Gold); Game.Hud?.Log($"{Name(s,e.actor)} ganha +1 PA do braseiro"); break;
                case "espinhos": if(target) { Float(target.position,$"−{e.value} espinhos",Red); PlayClip(target,"hit"); } yield return Wait(.25f); break;
                case "explosao":
                {
                    var at=Position(s,e.cell)+Vector3.up*.3f; Sound("attack");
                    if(EffectFrames("festival-de-fogos/explosao") is Sprite[] boom) StartCoroutine(PlayEffect(boom,at,ArenaEfeitos.Fps("Sprites/Efeitos/festival-de-fogos/explosao",12),1.6f));
                    else { Flash(at,C(1,.4f,.75f),24); Flash(at+Vector3.up*.3f,Gold,14); }
                    yield return Shake(.1f,.2f); break;
                }
                case "dano-evento": if(target) { Float(target.position,"−"+e.value,Red); if(e.value>0) PlayClip(target,"hit"); } yield return Wait(.15f); break;
                case "comer": RemoveCoins(e.cell); if(actor) Float(actor.position,"Nhac! +"+e.value,Gold); yield return Wait(.15f); break;
                case "evento-tick":
                    RefreshMarkers(s);
                    if(e.path!=null&&e.info=="mare-de-lava") foreach(var c in e.path) StartCoroutine(Fx("mare-de-lava/inicio",Position(s,c)+Vector3.up*.1f,C(1,.45f,.1f),.05f));
                    if(e.value>0) Game.Hud?.Log($"{EventName(e.info)}: falta{(e.value>1?"m":"")} {e.value} rodada{(e.value>1?"s":"")}");
                    break;
                case "evento-fim":
                    if(EffectFrames(e.info+"/fim") is Sprite[] outro) StartCoroutine(PlayEffect(outro,Vector3.up*1.5f,ArenaEfeitos.Fps("Sprites/Efeitos/"+e.info+"/fim",10),boardSize*.9f));
                    RefreshMarkers(s); ApplySky(s); Game.Hud?.Log($"Fim do evento: {EventName(e.info)}"); break;
                case "rodada": Game.Hud?.Log("Rodada "+e.value); break;
                case "dominio": Game.Hud?.Log($"{TeamName(s,e.actor)} domina {(e.info=="braseiro"?"um braseiro":"o centro")} ({e.value})"); break;
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
            rig.facing=direction; rig.dir=((Mathf.RoundToInt(angle/45f)%8)+8)%8; ShowFrame(rig);
        }
        // Gira a vista até o próximo lado reto (delta +1/−1), deslizando.
        public void RotateView(int delta)
        {
            yawGoal=Mathf.Round(yawGoal/90f+delta)*90f;
            Game.Notify("Girando a vista (Q/E segurados ou arrastar com o botão direito).");
        }
        // Giro livre em graus (arraste com o botão direito, segurar e arrastar, Q/E segurados, botões ◄ ►).
        // Dois dedos não giram: a pinça é só zoom (020-ux).
        public void RotateBy(float degrees) { yawGoal+=degrees; }
        // Endireita no lado reto mais próximo.
        public void StraightenView() { yawGoal=Mathf.Round(yawGoal/90f)*90f; }
        // Vista instantânea (partida nova, captura).
        void SetYaw(float yaw) { Yaw=yawGoal=yaw; ApplyYaw(); }
        // Pés no chão (autor, 18/10): o sprite gira pelo centro para encarar a câmera e, sem isto, quem é alto (Primal Beast)
        // fica com os pés acima da casa. Desce o sprite até o desenho tocar o meio da casa (ou das casas, no 2×2).
        readonly Dictionary<Transform,float> lifts=new Dictionary<Transform,float>();
        void Plant(Transform visual)
        {
            if(!visual||!visual.parent||!(visual.GetComponent<SpriteRenderer>() is SpriteRenderer r)||!r.sprite) return;
            // Quadros com folga transparente embaixo: o pé é o vértice mais baixo da malha justa do sprite.
            var b=r.sprite.bounds; float low=b.min.y; var v=r.sprite.vertices; if(v!=null&&v.Length>0) low=v.Min(x=>x.y);
            var feet=visual.TransformPoint(new Vector3(b.center.x,low,0));
            lifts.TryGetValue(visual,out float lift);
            visual.position+=visual.parent.position+Vector3.up*lift-feet;
        }
        void ApplyYaw()
        {
            FitCamera(); FitSky();
            // Os sprites voltam a encarar a câmera e escolhem o quadro pela direção em que já olhavam.
            foreach(var kv in rigs.ToList()) {
                if(!kv.Key) { rigs.Remove(kv.Key); continue; }
                kv.Key.localRotation=BoardCamera.transform.rotation; Plant(kv.Key);
                if(kv.Value.facing!=Vector3.zero&&kv.Key.parent) Turn(kv.Key.parent,kv.Value.facing);
            }
        }
        void SlideYaw()
        {
            if(Mathf.Abs(Yaw-yawGoal)<.01f) { if(Yaw!=yawGoal) { Yaw=yawGoal; ApplyYaw(); } return; }
            Yaw=Mathf.Lerp(Yaw,yawGoal,1-Mathf.Exp(-YawSpeed*Time.unscaledDeltaTime)); ApplyYaw();
        }
        // Sala: começa com o próprio time perto da câmera (embaixo na tela).
        void AutoView(MatchState s)
        {
            var own=s.fighters.Where(f=>f.Alive&&s.Ally(f.team,Game.ViewerTeam)).Select(f=>Position(s,f.cell)).ToList();
            if(own.Count==0) return;
            var center=own.Aggregate(Vector3.zero,(a,p)=>a+p)/own.Count;
            int best=Enumerable.Range(0,4).OrderByDescending(k=>Vector3.Dot(SideDirection(k*90),center)).First();
            SetYaw(best*90);
        }
        // Quadros por pasta lidos uma vez; cada unidade ganha seu próprio rig (estado do clipe) apontando para eles.
        static readonly Dictionary<string,SpriteRig> rigCache=new Dictionary<string,SpriteRig>();
        static bool HasRig(string folder)
        {
            if(string.IsNullOrEmpty(folder)) return false;
            if(!rigCache.TryGetValue(folder,out var shared)) rigCache[folder]=shared=ReadRig(folder);
            return shared!=null;
        }
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
        // Clipes que terminam fora de cena (morte, gato que some no zap) seguram o último quadro em vez de voltar ao idle.
        static bool Holds(string clip) => clip=="death"||clip=="zap";
        // Escondido: some para os inimigos de quem olha; para os aliados fica translúcido.
        // Névoa do labirinto (Fog): casa nunca vista fica preta (a vista já chega sem nada nela); a lembrada, escurecida; só as
        // coladas nos lutadores do time de quem olha ficam claras. Some quando a vela é acesa.
        GameObject fogObject; Mesh fogMesh;
        void RefreshFog(MatchState s)
        {
            if(fogObject) Destroy(fogObject); if(fogMesh) Destroy(fogMesh); fogObject=null; fogMesh=null;
            int team=Game.LocalTeam; if(!Fog.On(s)||team<=0||!spriteMaterial) return;
            var seen=Fog.Vision(s,team); var mem=Fog.Memory(s,team);
            var v=new List<Vector3>(); var tri=new List<int>(); var col=new List<Color>();
            for(int i=0;i<s.tiles.Length;i++) {
                var t=s.tiles[i]; if(seen[i]) continue;
                bool known=mem[i]; if(known&&(t.terrain==Terrain.Parede||t.terrain==Terrain.Porta)) continue;
                var p=Position(s,new Cell(i%s.width,i/s.width))+Vector3.up*(known?.04f:.06f); int k=v.Count;
                v.AddRange(new[]{p+new Vector3(-.5f,0,-.5f),p+new Vector3(-.5f,0,.5f),p+new Vector3(.5f,0,.5f),p+new Vector3(.5f,0,-.5f)});
                for(int j=0;j<4;j++) col.Add(known?new Color(0,0,.02f,.62f):new Color(0,0,0,1)); tri.AddRange(new[]{k,k+1,k+2,k,k+2,k+3});
            }
            if(v.Count==0) return;
            fogMesh=new Mesh{name="Nevoa",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32}; fogMesh.SetVertices(v); fogMesh.SetColors(col); fogMesh.SetTriangles(tri,0); fogMesh.RecalculateBounds();
            fogObject=new GameObject("Nevoa",typeof(MeshFilter),typeof(MeshRenderer)); fogObject.transform.SetParent(world,false);
            fogObject.GetComponent<MeshFilter>().sharedMesh=fogMesh; fogObject.GetComponent<MeshRenderer>().sharedMaterial=spriteMaterial;
        }
        void RefreshHidden(MatchState s)
        {
            RefreshFog(s);
            foreach(var f in s.fighters) {
                if(!units.TryGetValue(f.id,out var unit)||!unit) continue;
                bool hidden=f.Alive&&Match.Concealed(s,Game.LocalTeam,f), mine=s.Ally(f.team,Game.LocalTeam);
                foreach(var r in unit.GetComponentsInChildren<Renderer>(true)) {
                    r.enabled=!hidden||mine;
                    if(r is SpriteRenderer sr&&r.name!="Silhueta") sr.color=new Color(1,1,1,hidden?.45f:1);
                }
                var pick=unit.GetComponent<Collider>(); if(pick) pick.enabled=!hidden||mine;
            }
        }
        static void ShowFrame(SpriteRig rig)
        {
            // Forma com idle próprio (idle<pose>, ex.: idlehammer): respira em loop segurando a arma.
            if(rig.clip=="idle"&&rig.pose!=null&&rig.clips.ContainsKey("idle"+rig.pose)) {
                rig.clip="idle"+rig.pose; var loop=ClipFrames(rig,out bool mirrored); rig.clip="idle";
                if(loop?.Length>0) { int k=(int)(rig.time*Fps("idle"))%loop.Length; rig.renderer.sprite=loop[k]??rig.still[rig.dir]; rig.renderer.flipX=mirrored&&loop[k]; return; }
            }
            if(rig.clip=="idle"&&rig.pose!=null&&rig.clips.ContainsKey(rig.pose)) {
                rig.clip=rig.pose; var held=ClipFrames(rig,out bool turned); rig.clip="idle";
                if(held?.Length>0) { rig.renderer.sprite=held[held.Length-1]??rig.still[rig.dir]; rig.renderer.flipX=turned; return; }
            }
            var frames=ClipFrames(rig,out bool flip);
            if(frames==null) { rig.renderer.sprite=rig.still[rig.dir]; rig.renderer.flipX=false; return; }
            int i=(int)(rig.time*Fps(rig.clip)); i=Loops(rig.clip)?i%frames.Length:Mathf.Min(i,frames.Length-1); if(rig.reverse) i=frames.Length-1-i;
            rig.renderer.sprite=frames[i]??rig.still[rig.dir]; rig.renderer.flipX=flip&&frames[i];
        }
        bool Playing(Transform unit,string clip) =>
            unit&&unit.childCount>0&&rigs.TryGetValue(unit.GetChild(0),out var rig)&&rig.clip==clip&&ClipFrames(rig,out _) is Sprite[] fr&&rig.time*Fps(clip)<fr.Length;
        // Começa um clipe e devolve sua duração (0 se o lutador não tem sprite ou não tem esse clipe).
        float PlayClip(Transform unit,string clip,bool reverse=false)
        {
            if(!unit||unit.childCount==0||!rigs.TryGetValue(unit.GetChild(0),out var rig)) return 0;
            // A reação de dano não corta um golpe/skill no meio; a morte sempre entra.
            if(clip=="hit"&&!Loops(rig.clip)&&rig.clip!="hit"&&rig.clip!="death"&&ClipFrames(rig,out _) is Sprite[] busy&&rig.time*Fps(rig.clip)<busy.Length) return 0;
            rig.clip=clip; rig.time=0; rig.reverse=reverse; var frames=ClipFrames(rig,out _); ShowFrame(rig);
            return frames==null?0:frames.Length/Fps(clip);
        }
        // Anjo e demônio do Baltazar: só enquanto a aura está ligada e ele de pé. Órbita defasada 180°, tiras de 5 quadros em
        // vaivém (0,1,2,3,4,3,2,1) a 6 FPS. Puro visual: a regra é resolvida no motor (AuraPulse), não pelo contato da órbita.
        readonly Dictionary<int,(SpriteRenderer angel,SpriteRenderer demon)> companions=new Dictionary<int,(SpriteRenderer,SpriteRenderer)>();
        static Sprite[] angelFrames,demonFrames;
        static readonly int[] Hover={0,1,2,3,4,3,2,1};
        void UpdateCompanions(MatchState s)
        {
            foreach(var id in companions.Keys.ToList()) {
                var f=s.F(id);
                if(f!=null&&f.Alive&&f.auraOn&&units.TryGetValue(id,out var u)&&u&&u.gameObject.activeSelf&&companions[id].angel) continue;
                if(companions[id].angel) Destroy(companions[id].angel.gameObject); if(companions[id].demon) Destroy(companions[id].demon.gameObject); companions.Remove(id);
            }
            foreach(var f in s.fighters) {
                if(!f.Alive||!f.auraOn||!units.TryGetValue(f.id,out var unit)||!unit||!unit.gameObject.activeSelf) continue;
                if(!companions.TryGetValue(f.id,out var pair)) {
                    angelFrames??=Resources.LoadAll<Sprite>("Sprites/Efeitos/baltazar-anjo").OrderBy(x=>x.name.Length).ThenBy(x=>x.name).ToArray();
                    demonFrames??=Resources.LoadAll<Sprite>("Sprites/Efeitos/baltazar-demonio").OrderBy(x=>x.name.Length).ThenBy(x=>x.name).ToArray();
                    SpriteRenderer Make(string name) { var r=new GameObject(name,typeof(SpriteRenderer)).GetComponent<SpriteRenderer>(); r.transform.SetParent(effectRoot,false); if(spriteMaterial) r.sharedMaterial=spriteMaterial; r.transform.localScale=Vector3.one*2.4f; return r; }
                    companions[f.id]=pair=(Make("Anjo"),Make("Demonio"));
                }
                float t=Time.time*1.1f, radius=1.25f; int frame=Hover[(int)(Time.time*6)%Hover.Length];
                var center=unit.position+Vector3.up*1.15f;
                void Place(SpriteRenderer r,Sprite[] frames,float angle) {
                    if(!r) return;
                    r.transform.position=center+new Vector3(Mathf.Cos(angle),Mathf.Sin(t*2.3f+angle)*.12f,Mathf.Sin(angle))*radius;
                    r.transform.rotation=BoardCamera.transform.rotation;
                    if(frames!=null&&frames.Length>0) r.sprite=frames[Mathf.Min(frame,frames.Length-1)];
                }
                Place(pair.angel,angelFrames,t); Place(pair.demon,demonFrames,t+Mathf.PI);
            }
        }
        // Estados contínuos (entrega 017): laço sobre o lutador enquanto o estado durar, retirado quando acaba. Acompanha a
        // visibilidade do alvo: o inimigo não vê o laço de quem está escondido. Vários estados se empilham acima da cabeça.
        readonly Dictionary<(int id,string fx),SpriteRenderer> stateLoops=new Dictionary<(int,string),SpriteRenderer>();
        void UpdateStates(MatchState s)
        {
            var want=new List<(int id,string fx)>();
            foreach(var f in s.fighters) {
                if(!f.Alive||Match.Concealed(s,Game.LocalTeam,f)||!units.TryGetValue(f.id,out var unit)||!unit||!unit.gameObject.activeSelf) continue;
                foreach(var fx in ArenaEfeitos.StateLoops(f)) if(EffectFrames(fx)!=null) want.Add((f.id,fx));
            }
            foreach(var key in stateLoops.Keys.ToList()) if(!want.Contains(key)||!stateLoops[key]) { if(stateLoops[key]) Destroy(stateLoops[key].gameObject); stateLoops.Remove(key); }
            var stack=new Dictionary<int,int>();
            foreach(var key in want) {
                if(!stateLoops.TryGetValue(key,out var r)) {
                    var frames=EffectFrames(key.fx);
                    r=new GameObject("Estado_"+key.fx,typeof(SpriteRenderer)).GetComponent<SpriteRenderer>(); r.transform.SetParent(effectRoot,false);
                    if(spriteMaterial) r.sharedMaterial=spriteMaterial; r.sprite=frames[0];
                    // Do tamanho da casa: a tira não é reamostrada, só a escala do objeto.
                    r.transform.localScale=Vector3.one*(1.1f/Mathf.Max(.01f,frames[0].bounds.size.x));
                    var loop=r.gameObject.AddComponent<SpriteLoop>(); loop.frames=frames; loop.fps=ArenaEfeitos.Fps("Sprites/Efeitos/"+key.fx,12); loop.billboard=true;
                    stateLoops[key]=r;
                }
                var f=s.F(key.id); float h=f!=null&&Heights.TryGetValue(f.def,out float hh)?hh:.75f;
                // Aura e escondido envolvem o corpo; os demais ficam em fila acima da cabeça.
                bool body=key.fx.EndsWith("/aura")||key.fx.StartsWith("aura")||key.fx.Contains("escondido")||key.fx.Contains("preso");
                int n=body?0:(stack.TryGetValue(key.id,out int k)?k:0); if(!body) stack[key.id]=n+1;
                r.transform.position=units[key.id].position+Vector3.up*(body?h*.55f:h+.35f+.38f*n);
            }
        }
        // Fase de efeito sem bloquear (conjuração no lançador, impacto no destino): devolve a duração, ou 0 sem arte.
        float PhaseFx(string fx,Vector3 at)
        {
            var frames=EffectFrames(fx); if(frames==null) return 0;
            float fps=ArenaEfeitos.Fps("Sprites/Efeitos/"+fx,12); StartCoroutine(PlayEffect(frames,at,fps)); return frames.Length/fps;
        }
        // Fase que leva o golpe ao alvo (impacto, uso ou arquivo único) e seu caminho no manifesto.
        static Sprite[] ImpactPhase(string skill,out string res)
        {
            res=null; if(string.IsNullOrEmpty(skill)) return null;
            foreach(var p in new[]{skill+"/impacto",skill+"/uso",skill}) if(EffectFrames(p) is Sprite[] f) { res="Sprites/Efeitos/"+p; return f; }
            return null;
        }
        // Golpe de área: o dano espera a colisão só no primeiro alvo; os outros aparecem junto.
        (int actor,string skill) burst;
        void AdvanceRigs()
        {
            foreach(var rig in rigs.Values) {
                if(!rig.renderer) continue;
                rig.time+=Time.deltaTime;
                var frames=ClipFrames(rig,out _);
                if(frames!=null&&!Loops(rig.clip)&&!Holds(rig.clip)&&rig.time*Fps(rig.clip)>=frames.Length) { rig.clip="idle"; rig.time=0; }
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
        IEnumerator Walk(Transform actor,MatchState s,Cell[] path,Fighter who=null)
        {
            float cycle=PlayClip(actor,"walk"); bool walks=cycle>0;
            float step=walks?Mathf.Clamp(cycle*.5f,.3f,.6f):.3f;
            var points=path.Select(c=>Spot(s,who,c)).ToArray(); int n=points.Length-1, seg=-1;
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
            id=Content.BaseSkill(id);
            if(!effectCache.TryGetValue(id,out var frames)) {
                // Uma fase = um arquivo (<id>/inicio, <id>/impacto…): só os quadros dele, nunca os de outras fases da mesma pasta.
                effectCache[id]=frames=ArenaEfeitos.Frames("Sprites/Efeitos/"+id);
            }
            return frames;
        }
        // Tira animada do projétil (ProjeteisAnimados/<skill>), voltada para a direita; null se não houver.
        static readonly Dictionary<string,Sprite[]> flightCache=new Dictionary<string,Sprite[]>();
        static Sprite[] ProjectileFrames(string id)
        {
            if(string.IsNullOrEmpty(id)) return null;
            id=Content.BaseSkill(id);
            if(!flightCache.TryGetValue(id,out var frames)) flightCache[id]=frames=ArenaEfeitos.Frames("Sprites/ProjeteisAnimados/"+id);
            return frames;
        }
        static readonly Dictionary<string,Sprite> projectileCache=new Dictionary<string,Sprite>();
        static Sprite ProjectileImage(string id)
        {
            if(string.IsNullOrEmpty(id)) return null;
            id=Content.BaseSkill(id);
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
            // Fases do disparo: conjuração no lançador, projétil (tira animada ou imagem estática) pela trajetória, impacto no destino.
            var cast=EffectFrames(skill+"/conjuracao"); var frames=ImpactPhase(skill,out var impactRes); var image=ProjectileImage(skill);
            var flight=ProjectileFrames(skill);
            if(cast!=null) StartCoroutine(PlayEffect(cast,from,ArenaEfeitos.Fps("Sprites/Efeitos/"+skill+"/conjuracao",12)));
            // Projeteis/<skill>.png do Codex: imagem larga (128 px) estica como raio; as menores voam como projétil.
            bool beam=style.kind==ShotKind.Raio||style.kind==ShotKind.Sopro; if(image&&image.rect.width<128) beam=false;
            if(beam) yield return Beam(from,hit,style.color,style.kind==ShotKind.Sopro,image);
            else yield return Missile(from,hit,style.kind,style.color,flight,image);
            // O dano (depois do Shot) entra no quadro de colisão da fase de impacto, não no fim do voo.
            if(frames!=null) { StartCoroutine(PlayEffect(frames,hit,ArenaEfeitos.Fps(impactRes,14))); float lag=ArenaEfeitos.ImpactTime(impactRes,14); if(lag>0) yield return Wait(lag); }
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
            if(frames!=null) { sprite=new GameObject("Imagem",typeof(SpriteRenderer)).GetComponent<SpriteRenderer>(); sprite.transform.SetParent(go.transform,false); sprite.sharedMaterial=spriteMaterial; sprite.sprite=frames[0]; sprite.transform.rotation=BoardCamera.transform.rotation; sprite.transform.localScale=Vector3.one; }
            else if(image) { sprite=new GameObject("Imagem",typeof(SpriteRenderer)).GetComponent<SpriteRenderer>(); sprite.transform.SetParent(go.transform,false); sprite.sharedMaterial=spriteMaterial; sprite.sprite=image; }
            else head=Line(go.transform,Color.Lerp(color,Color.white,.35f),width);
            var prev=from;
            for(float t=0;t<dur;t+=Time.deltaTime) {
                float k=t/dur; var p=Vector3.Lerp(from,to,k)+Vector3.up*Mathf.Sin(k*Mathf.PI)*arc;
                var d=(p-prev).sqrMagnitude>1e-6f?(p-prev).normalized:(to-from).normalized; prev=p; go.transform.position=p;
                if(kind==ShotKind.Lamina) d=Quaternion.AngleAxis(t*1440,Vector3.up)*d;
                if(head) { head.SetPosition(0,p-d*length*.5f); head.SetPosition(1,p+d*length*.5f); }
                if(sprite&&frames!=null) sprite.sprite=frames[(int)(t*12)%frames.Length]; if(sprite) Orient(sprite.transform,d);
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
        IEnumerator PlayEffect(Sprite[] frames,Vector3 at,float fps,float width=0)
        {
            var go=Effect("Impacto"); var r=go.AddComponent<SpriteRenderer>(); r.sharedMaterial=spriteMaterial; go.transform.position=at; go.transform.rotation=BoardCamera.transform.rotation;
            if(width>0) go.transform.localScale=Vector3.one*(width/Mathf.Max(.01f,frames[0].bounds.size.x));
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
            CancelAnimations(); foreach(var u in units.Values) if(u) Destroy(u.gameObject); units.Clear(); wings.Clear(); wingRest.Clear(); rigs.Clear(); boardKey=null;
            if(Game.ViewerTeam>0&&state!=null) AutoView(state);
            Sync(state,true); FitCamera(); // partida nova começa com o tabuleiro inteiro na tela
        }
        // Unidade por raycast; senão a casa mais alta sob o cursor (testa do relevo mais alto para o mais baixo).
        public bool Pick(Vector2 screenPoint,out Cell cell,out int unitId)
        {
            cell=default; unitId=0;
            var s=Seen(Game.State); if(s==null) return false;
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
        // Silhueta acompanha o quadro do sprite (e some junto quando o lutador está escondido para quem olha).
        void SyncGhosts()
        {
            foreach(var rig in rigs.Values) {
                if(!rig.ghost||!rig.renderer) continue;
                rig.ghost.sprite=rig.renderer.sprite; rig.ghost.flipX=rig.renderer.flipX; rig.ghost.enabled=rig.renderer.enabled&&rig.renderer.color.a>.9f;
            }
        }
        // Barras de vida sobre cada lutador (Canvas próprio, abaixo do HUD). Chefes neutros: barra larga com nome e PV.
        Canvas barCanvas; Font barFont;
        readonly Dictionary<int,(RectTransform root,RectTransform fill,Text label)> bars=new Dictionary<int,(RectTransform,RectTransform,Text)>();
        static bool Boss(Fighter f) => f.team==MatchState.Neutral&&Content.MaxHp(f)>=12;
        void UpdateBars(MatchState s)
        {
            if(!barCanvas) {
                var go=new GameObject("BarrasDeVida",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));
                barCanvas=go.GetComponent<Canvas>(); barCanvas.renderMode=RenderMode.ScreenSpaceOverlay; barCanvas.sortingOrder=-10;
                barFont=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            float k=ArenaHud.UiScale; barCanvas.GetComponent<CanvasScaler>().scaleFactor=k;
            foreach(var id in bars.Keys.Where(id=>!units.ContainsKey(id)).ToList()) { if(bars[id].root) Destroy(bars[id].root.gameObject); bars.Remove(id); }
            foreach(var kv in units) {
                var f=s.F(kv.Key); var unit=kv.Value;
                bool shown=f!=null&&f.Alive&&unit&&unit.gameObject.activeInHierarchy&&unit.GetComponentsInChildren<Renderer>().Any(r=>r.enabled&&r.name!="Silhueta");
                if(!bars.TryGetValue(kv.Key,out var bar)) {
                    if(!shown) continue;
                    bool boss=Boss(f); float w=boss?120:46, h=boss?11:6;
                    var root=new GameObject("Barra_"+kv.Key,typeof(RectTransform),typeof(Image)).GetComponent<RectTransform>(); root.SetParent(barCanvas.transform,false);
                    root.anchorMin=root.anchorMax=Vector2.zero; root.pivot=new Vector2(.5f,0); root.sizeDelta=new Vector2(w,h);
                    var back=root.GetComponent<Image>(); back.color=new Color(.05f,.05f,.08f,.85f); back.raycastTarget=false;
                    var fill=new GameObject("Vida",typeof(RectTransform),typeof(Image)).GetComponent<RectTransform>(); fill.SetParent(root,false);
                    fill.anchorMin=Vector2.zero; fill.anchorMax=Vector2.one; fill.offsetMin=new Vector2(1,1); fill.offsetMax=new Vector2(-1,-1);
                    var img=fill.GetComponent<Image>(); img.color=f.team==MatchState.Neutral?Red:TeamColor(f.team); img.raycastTarget=false;
                    Text label=null;
                    if(boss) {
                        label=new GameObject("Nome",typeof(RectTransform),typeof(Text)).GetComponent<Text>(); label.transform.SetParent(root,false);
                        label.font=barFont; label.fontSize=13; label.fontStyle=FontStyle.Bold; label.alignment=TextAnchor.LowerCenter; label.color=Color.white; label.raycastTarget=false;
                        label.horizontalOverflow=HorizontalWrapMode.Overflow; var lr=label.rectTransform; lr.anchorMin=new Vector2(0,1); lr.anchorMax=new Vector2(1,1); lr.pivot=new Vector2(.5f,0); lr.sizeDelta=new Vector2(0,18); lr.anchoredPosition=new Vector2(0,1);
                        label.gameObject.AddComponent<Outline>().effectColor=new Color(0,0,0,.9f);
                    }
                    bar=bars[kv.Key]=(root,fill,label);
                }
                bar.root.gameObject.SetActive(shown); if(!shown) continue;
                float height=(Heights.TryGetValue(ShownId(f),out float hh)?hh:.75f)+(Shown(f).flying?.35f:0)+.18f;
                var p=BoardCamera.WorldToScreenPoint(unit.position+Vector3.up*height);
                if(p.z<0) { bar.root.gameObject.SetActive(false); continue; }
                bar.root.anchoredPosition=new Vector2(p.x,p.y)/k;
                bar.fill.anchorMax=new Vector2(Mathf.Clamp01(f.hp/(float)Mathf.Max(1,Content.MaxHp(f))),1);
                if(bar.label) bar.label.text=$"{Shown(f).name}  {f.hp}/{Content.MaxHp(f)}";
            }
        }
        void Update()
        {
            var s=Seen(Game.State);
            if(!Ready||s==null) return;
            if(Screen.width!=screenWidth||Screen.height!=screenHeight) FitCamera();
            SlideYaw();
            FitSky();
            AdvanceRigs(); SyncGhosts(); UpdateBars(s); UpdateCompanions(s); UpdateStates(s);
            for(int i=wings.Count-1;i>=0;i--) { var wing=wings[i]; if(!wing) { wings.RemoveAt(i); continue; } wing.localRotation=wingRest[wing]*Quaternion.Euler(0,0,Mathf.Sin(Time.time*1.8f)*3.15f); }
            string key=s.revision+"|"+Game.Mode+"|"+Game.CanAct+"|"+Game.CanPlace+"|"+Game.Hud?.HasPendingConfirmation;
            if(key!=shownKey) UpdateHighlights(key);
            Vector2 point=default; bool tap=false,hasPointer=false,down=false;
            var touches=Touchscreen.current?.touches.Where(t=>t.press.isPressed).ToArray();
            var mouse=Mouse.current;
            if(touches!=null&&touches.Length>=2) { Pinch(touches[0].position.ReadValue(),touches[1].position.ReadValue()); point=lastDrag; hasPointer=down=true; }
            else {
                if(touches!=null&&touches.Length==1) { point=touches[0].position.ReadValue(); hasPointer=down=true; }
                // Dedo levantado: o toque não tem mais posição (no celular nem há mouse); o toque vale onde o dedo estava.
                else if(pressing&&touchPress) { point=lastDrag; hasPointer=true; }
                else if(mouse!=null) { point=mouse.position.ReadValue(); hasPointer=true; down=mouse.leftButton.isPressed||mouse.rightButton.isPressed||mouse.middleButton.isPressed; }
                if(pinching) { pinching=false; lastDrag=point; } // sobrou um dedo da pinça: segue arrastando dali, sem salto
            }
            bool overBoard=hasPointer&&BoardCamera.pixelRect.Contains(point)&&!OverUi(point);
            if(down) {
                if(!pressing) {
                    // Toque que nasce sobre a interface não controla a câmera nem o tabuleiro (pressOnUi).
                    pressOnUi=(pinching&&pressOnUi)||(hasPointer&&OverUi(point));
                    pressing=true; pressOnBoard=overBoard&&!pressOnUi; dragged=pinching; pinched=pinching;
                    pressStart=lastDrag=point; pressTime=Time.unscaledTime; rotating=false; rotateAcc=0;
                    touchPress=touches!=null&&touches.Length>0;
                    pressFingers=touchPress?touches.Length:1; // começar com dois dedos nunca acaba em clique
                    rotateButton=mouse!=null&&mouse.rightButton.isPressed&&(touches==null||touches.Length==0);
                }
                else if(!pinching&&!pinched) {
                    bool moved=(point-pressStart).magnitude>DragThreshold;
                    // Botão direito arrastado gira; dedo/botão parado ~0,45 s também passa a girar (arrastar para o lado).
                    if(pressOnBoard&&!rotating&&!dragged&&(rotateButton&&moved||!rotateButton&&!moved&&Time.unscaledTime-pressTime>ArenaTouchGesture.LongPressSeconds)) {
                        rotating=dragged=true; Game.Hud?.ClearConfirmation(); if(!rotateButton) Game.Notify("Arraste para o lado para girar a vista.");
                    }
                    if(rotating) {
                        rotateAcc+=point.x-lastDrag.x;
                        // Giro contínuo: ~1 volta ao arrastar a largura da tela.
                        RotateBy((point.x-lastDrag.x)*360f/Mathf.Max(400,Screen.width)); rotateAcc=0;
                    } else {
                        if(moved&&!dragged) { dragged=true; if(pressOnBoard) Game.Hud?.ClearConfirmation(); } // arrastou: a mira armada cai
                        if(dragged&&pressOnBoard) { BoardCamera.transform.position+=World(lastDrag)-World(point); ClampCamera(); }
                    }
                    lastDrag=point;
                }
            } else if(pressing) {
                pressing=false;
                if(touchPress&&pinched) multiTouchReleased=Time.frameCount;
                // Só o toque/botão esquerdo age; o direito e o do meio servem apenas para arrastar.
                // E só age o toque que começou com um dedo e nunca virou pinça nem gesto de câmera (1->2->1 não é clique).
                tap=pressOnBoard&&ArenaTouchGesture.TapAfterGesture(pressFingers,pinched,dragged||rotating,pressOnUi)
                    &&(touchPress||mouse==null||!mouse.rightButton.wasReleasedThisFrame&&!mouse.middleButton.wasReleasedThisFrame);
                pressOnUi=false;
            }
            float wheel=mouse!=null?mouse.scroll.ReadValue().y:0;
            if(overBoard&&Mathf.Abs(wheel)>.01f) ZoomAt(point,wheel>0?.87f:1/.87f);
            if(overBoard||pressing&&pressOnBoard) {
                bool valid=Pick(point,out var c,out _); hover.SetActive(valid&&!dragged); if(valid) hover.transform.position=Position(s,c)+Vector3.up*.03f;
                if(valid&&tap&&Game.EditingMap) Game.MapEditor.Tap(c);
                else if(valid&&tap&&Game.Placing) Game.PlaceTap(c);
                else if(valid&&tap&&Game.Hud?.HasPendingConfirmation!=true) {
                    // Mover numa casa com lutador visível: abre a ficha dele (skills e o que fazem) em vez de tentar andar.
                    var who=Board.At(s,c); bool seen=who!=null&&!Match.Concealed(s,Game.LocalTeam,who);
                    if(Game.Mode==""&&seen) Game.Hud?.Inspect(who);
                    else if(Game.Mode=="") Game.Submit(Match.Interaction(s,c)??"mover",c); else if(Game.Mode=="desejo-d") Game.Hud?.ChooseCopy(c); else if(Game.Hud?.ChooseRevive(c)!=true&&Game.Hud?.ChooseCat(c)!=true) SkillTap(c);
                }
            } else hover.SetActive(false);
            var selected=EventSystem.current?EventSystem.current.currentSelectedGameObject:null; // "?." não enxerga objeto Unity destruído
            bool typing=selected&&selected.GetComponent<UnityEngine.UI.InputField>()!=null;
            // Espaço passa o turno pelo mesmo caminho do botão: o HUD confirma em dois toques (Hud.ConfirmPass).
            if(!typing&&Game.Hud?.PickerOpen!=true&&Keyboard.current?.spaceKey.wasPressedThisFrame==true) {
                if(Game.Hud!=null) Game.Hud.ConfirmPass(); else Game.Submit("passar");
            }
            if(!typing&&Keyboard.current?.escapeKey.wasPressedThisFrame==true) Game.Mode="";
            // Q/E segurados giram aos poucos; ao soltar, a vista fica onde parou.
            if(!typing&&Keyboard.current?.qKey.isPressed==true) RotateBy(-90*Time.unscaledDeltaTime);
            if(!typing&&Keyboard.current?.eKey.isPressed==true) RotateBy(90*Time.unscaledDeltaTime);
            if(!typing&&Keyboard.current?.hKey.wasPressedThisFrame==true) ToggleHeights();
            FaceHeights();
        }
        // Limiar de toque/arrasto (~10 px) escalado por DPI e escala da interface, separado do limiar da pinça.
        float TouchScale => ArenaTouchGesture.Scale(Screen.dpi,ArenaHud.UiScale);
        float DragThreshold => ArenaTouchGesture.Threshold(TouchScale);
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
        // Dois dedos: SÓ zoom. Não gira a vista e não desloca o tabuleiro (o deslocamento aqui é só o que o
        // ZoomAt faz para manter o meio da pinça parado na tela). Só vale se o gesto começou fora da interface.
        void Pinch(Vector2 a,Vector2 b)
        {
            var center=(a+b)*.5f; float distance=Mathf.Max(1,Vector2.Distance(a,b));
            if(!pinching) {
                touchPress=true; Game.Hud?.ClearConfirmation();
                pressOnUi=pressOnUi||OverUi(a)||OverUi(b);
                pinching=pinched=dragged=true; pinchZoomed=false; pinchStart=pinchDistance=distance; pinchScale=TouchScale;
                pressOnBoard=!pressOnUi&&BoardCamera.pixelRect.Contains(center)&&!OverUi(center);
                if(pressOnBoard) { yawGoal=Yaw; rotating=false; } // a pinça interrompe um giro anterior
                pinchCenter=lastDrag=center; return; // no primeiro quadro da pinça só se mede a distância entre os dedos
            }
            // Abaixo do limiar (8 px escalados ou 2% da distância inicial) a pinça ainda não move a câmera.
            if(!pinchZoomed&&ArenaTouchGesture.Zoom(pinchStart,distance,pinchScale)) pinchZoomed=true;
            if(pressOnBoard&&pinchZoomed) {
                ZoomAt(pinchCenter,pinchDistance/distance); pinchDistance=distance;
            }
            lastDrag=center;
        }
        // O tabuleiro nunca sai de vista; afastado de todo, a câmera volta ao centro.
        void ClampCamera() { var cam=BoardCamera.transform; cam.position=Home+Vector3.ClampMagnitude(cam.position-Home,(fitSize-BoardCamera.orthographicSize)*1.5f); }
        // Toque com skill armada. Skill de 2 casas: o 1º toque guarda a casa (quem vai / entrada / 1º alvo) e o 2º envia.
        void SkillTap(Cell c)
        {
            var f=Game.Active; var sk=f==null?null:Match.Skills(f).FirstOrDefault(x=>x.id==Game.Mode);
            if(sk==null||!sk.TwoCells) { Game.Submit("skill",c,Game.Mode); return; }
            if(!Game.HasFirst) {
                if(!Game.Preview.CanTarget(f,sk,c,out string reason)) { Game.Notify(reason); return; }
                Game.FirstCell=c; Game.FirstFor=Game.Mode; shownKey=null;
                Game.Notify(sk.kind==SkillKind.Portal?"Agora escolha a saída do portal.":sk.kind==SkillKind.Troca?"Agora escolha o segundo lutador.":"Agora escolha o destino.");
                return;
            }
            if(c==Game.FirstCell) { Game.FirstFor=""; shownKey=null; return; }
            Game.Submit("skill",Game.FirstCell,Game.Mode,0,c);
        }
        // Mover: casas ao alcance dos PM. Skill: área de alcance (clara) e alvos válidos (forte).
        void UpdateHighlights(string key)
        {
            shownKey=key;
            foreach(var go in highlights) Destroy(go); highlights.Clear();
            var f=Game.Active; var s=Seen(Game.State);
            // Na névoa, acima dela (RefreshFog fica a .04–.06): dá para ver aonde andar mesmo no escuro.
            float lift=s!=null&&Fog.On(s)?.08f:.012f;
            void Mark(Cell c,Material m) => highlights.Add(Cube("Alcance",Position(s,c)+Vector3.up*lift,new Vector3(.8f,.012f,.8f),m,world));
            // Editor de mapas: casas de início (sempre livres) e a peça escolhida para mover.
            if(Game.EditingMap) { foreach(var c in Game.MapEditor.SpawnCells) Mark(c,moveMaterial); if(Game.MapEditor.Picked is Cell p) Mark(p,targetMaterial); return; }
            // Posicionamento: casas da borda que o herói escolhido (ou qualquer um, sem escolha) pode ocupar (verde) e ele (forte).
            if(Game.Placing) { if(!Game.CanPlace) return; foreach(var c in Game.Preview.PlaceCells(s.placing,s.F(Game.PlacePick))) Mark(c,moveMaterial); if(s.F(Game.PlacePick) is Fighter pick) Mark(pick.cell,targetMaterial); return; }
            if(!Game.CanAct||f==null||Game.Hud?.HasPendingConfirmation==true) return;
            if(Game.Mode=="") {
                var costs=Board.Costs(s,f);
                for(int i=0;i<costs.Length;i++) if(costs[i]>0&&costs[i]<=f.pm) Mark(new Cell(i%s.width,i/s.width),moveMaterial);
                return;
            }
            var sk=Match.Skills(f).FirstOrDefault(x=>x.id==Game.Mode); if(sk==null) { Game.Mode=""; return; }
            var m=Game.Preview; int r=Mathf.Max(s.width,s.height); bool second=sk.TwoCells&&Game.HasFirst;
            if(second) Mark(Game.FirstCell,moveMaterial);
            for(int x=-r;x<=r;x++) for(int z=-r;z<=r;z++) {
                var c=f.cell+new Cell(x,z);
                if(second?c!=Game.FirstCell&&m.CanTarget(f,sk,Game.FirstCell,c,out _):m.CanTarget(f,sk,c,out _)) Mark(c,targetMaterial);
                else if(m.InRange(f,sk,c,out _)) Mark(c,rangeMaterial);
            }
        }
        void OnDestroy() { CancelAnimations(); if(barCanvas) Destroy(barCanvas.gameObject); if(world) Destroy(world.gameObject); foreach(var m in materials) Destroy(m); foreach(var m in generatedMeshes) Destroy(m); }
    }
}
