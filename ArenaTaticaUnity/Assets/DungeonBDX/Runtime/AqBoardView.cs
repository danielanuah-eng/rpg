using System;
using System.Collections.Generic;
using System.Linq;
using DungeonBDX.Arcadia;
using TMPro;
using UnityEngine;

namespace DungeonBDX
{
    /// <summary>
    /// Presentation-only view for the AQ board mode: a warm physical 9x9 table with a rich
    /// stone/wood/mosaic floor, low walls, bronze/teal doors, tokens, guild bases and Arena
    /// miniatures. It never touches units, cells or rules; the unit list is only read.
    /// </summary>
    public sealed class AqBoardView : MonoBehaviour
    {
        static readonly Color[] GuildColors =
        {
            new Color(.95f,.74f,.28f), new Color(.70f,.76f,.90f),
            new Color(.88f,.44f,.62f), new Color(.20f,.64f,.66f)
        };
        static readonly string[] SmallIds = { "teantha", "perry", "asamyh", "vespa-cinzenta", "viuva-da-agulha", "lyra-faisca" };
        static readonly Color StoneDark = new Color(.20f,.17f,.14f);
        static readonly Color StoneMid = new Color(.42f,.37f,.30f);
        static readonly Color GoldTrim = new Color(.70f,.53f,.22f);
        static readonly Color RingGold = new Color(1f,.84f,.35f);
        static readonly Color MoveGreen = new Color(.25f,.92f,.52f);
        static readonly Color TargetGold = new Color(1f,.70f,.22f);

        sealed class AqUnitView
        {
            public int Id;
            public AqUnit Unit;
            public Transform Root;
            public Transform Body;
            public Transform Visual;
            public GameObject Base;
            public GameObject Ring;
            public Sprite Fallback;
            public TextMeshPro Label;
            public string LabelText = "";
            public float Height = 1.45f;
            public AqCell Logical;
            public readonly Queue<Vector3> Route = new Queue<Vector3>();
        }

        sealed class DoorView
        {
            public Transform Panel;
            public Renderer PanelRenderer;
            public GameObject[] Frame;
            public Material Bronze;
            public Material Teal;
        }

        sealed class Floating
        {
            public GameObject Go;
            public Vector3 Origin;
            public float Expires;
        }

        sealed class PortalMotion
        {
            public Transform[] Orbs;
            public Vector3 Center;
            public float Radius;
            public float Speed;
        }

        readonly Dictionary<int,AqUnitView> views = new Dictionary<int,AqUnitView>();
        readonly Dictionary<int,DungeonActorAnimation> animations = new Dictionary<int,DungeonActorAnimation>();
        readonly Dictionary<string,DoorView> doors = new Dictionary<string,DoorView>();
        readonly Dictionary<string,GameObject> tokens = new Dictionary<string,GameObject>();
        readonly Dictionary<AqCell,GameObject> cellMarkers = new Dictionary<AqCell,GameObject>();
        readonly Dictionary<string,Material> materials = new Dictionary<string,Material>();
        readonly List<Transform> billboards = new List<Transform>();
        readonly List<Texture2D> textures = new List<Texture2D>();
        readonly List<Floating> floats = new List<Floating>();
        readonly List<PortalMotion> portals = new List<PortalMotion>();
        readonly List<PropSprite> propSprites = new List<PropSprite>();
        readonly Dictionary<string, Sprite> artSprites = new Dictionary<string, Sprite>();

        sealed class PropSprite
        {
            public SpriteRenderer Renderer;
            public Vector3 Facing;
            public Sprite[] ByDirection; // facing index 1,3,5,7 on screen -> sw, ne, nw, se art
        }
        readonly Dictionary<int,float> deathAfterAttack = new Dictionary<int,float>();

        AqScenario scenario;
        AqBoard board;
        Transform boardRoot, liveRoot, actorRoot, markerRoot;
        UnityEngine.Camera viewCamera;
        GameObject cameraObject;
        Light lamp;
        Texture2D streetTexture, cryptTexture, woodTexture, terracottaTexture, mosaicTexture, marbleTexture, gardenTexture;
        int boardWidth, boardHeight;
        int selected = -1;
        bool built, snapPending;
        Vector2 pan;
        float shownYaw = IsoYaw, yawTarget = IsoYaw, lastRotate;

        // Isometric 2D contract (coordenacao/dungeonbdx/contratos/ISOMETRICO-2D.md): orthographic camera,
        // 30 degree pitch turns one square cell into a 2:1 diamond; resting views are 45+90k degrees.
        public const float IsoPitch = 30f, IsoYaw = 45f;
        // Upright sprites are foreshortened by cos(pitch); this restores their authored proportions.
        public static readonly float IsoStretch = 1f/Mathf.Cos(IsoPitch*Mathf.Deg2Rad);
        // Feet sit on the frame pivot (15%); the drawn head ends near 70% of the frame above it.
        const float HeadLine = .68f;
        public const float FigureScale = .74f;

        public UnityEngine.Camera Camera => viewCamera;
        /// <summary>Requested view angle; the camera eases toward it and rests on 45+90k after a gesture.</summary>
        public float Yaw => yawTarget;

        // Life numbers over the figures: a player option, remembered between sessions.
        const string ShowLifeKey = "dungeonbdx.showLife";
        public static bool ShowLife
        {
            get => PlayerPrefs.GetInt(ShowLifeKey, 1) == 1;
            set { PlayerPrefs.SetInt(ShowLifeKey, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public void RefreshLife()
        {
            foreach (var view in views.Values) RefreshLabel(view);
        }
        public float Zoom { get; private set; } = 1f;
        public IReadOnlyDictionary<int,DungeonActorAnimation> Animations => animations;
        public float RenderedSpriteHeight(int id)
        {
            if(!views.TryGetValue(id,out var actor)||!actor.Visual)return 0;
            var sprite=actor.Visual.GetComponentInChildren<SpriteRenderer>();
            return sprite&&sprite.sprite?sprite.sprite.bounds.size.y*Mathf.Abs(sprite.transform.lossyScale.y):0;
        }

        // ---------------------------------------------------------------- build

        public void Build(AqScenario scenario, IReadOnlyList<AqUnit> units)
        {
            if (built) return;
            this.scenario = scenario;
            board = scenario != null ? scenario.Board : null;
            boardWidth = board != null ? board.Width : 9;
            boardHeight = board != null ? board.Height : 9;
            boardRoot = new GameObject("AQ table").transform; boardRoot.SetParent(transform,false);
            // Doors, tokens and their labels stay outside the static batch: they move or swap material.
            liveRoot = new GameObject("AQ living").transform; liveRoot.SetParent(transform,false);
            actorRoot = new GameObject("AQ actors").transform; actorRoot.SetParent(transform,false);
            markerRoot = new GameObject("AQ markers").transform; markerRoot.SetParent(transform,false);
            CreateCamera();
            CreateLamp();
            BuildTable();
            BuildStarts();
            BuildTokens();
            if (Application.isPlaying) StaticBatchingUtility.Combine(boardRoot.gameObject);
            built = true;
            Sync(units);
            ResetCamera();
        }

        void CreateCamera()
        {
            cameraObject = new GameObject("AQ camera", typeof(UnityEngine.Camera));
            cameraObject.transform.SetParent(transform,false);
            viewCamera = cameraObject.GetComponent<UnityEngine.Camera>();
            viewCamera.orthographic = true;
            viewCamera.clearFlags = CameraClearFlags.SolidColor;
            viewCamera.backgroundColor = new Color(.05f,.04f,.035f);
            viewCamera.nearClipPlane = .05f;
            viewCamera.farClipPlane = 120f;
        }

        void CreateLamp()
        {
            var go = new GameObject("AQ light", typeof(Light));
            go.transform.SetParent(transform,false);
            lamp = go.GetComponent<Light>();
            lamp.type = LightType.Directional;
            lamp.color = new Color(1f,.93f,.81f);
            lamp.intensity = 1.35f;
            lamp.shadows = LightShadows.Soft;
            go.transform.rotation = Quaternion.Euler(52f,-35f,0f);
        }

        static void DisableDuplicateListeners(GameObject own)
        {
            var listeners = Resources.FindObjectsOfTypeAll<AudioListener>();
            if (listeners == null || listeners.Length <= 1) return;
            bool kept = false;
            foreach (var listener in listeners)
            {
                if (!listener) continue;
                // Scene objects only: never touch prefab assets loaded by the editor.
                if (!listener.gameObject.scene.IsValid()) continue;
                if (own && listener.gameObject == own) { listener.enabled = false; continue; }
                if (!kept) { kept = true; continue; }
                listener.enabled = false;
            }
        }

        void BuildTable()
        {
            if (board == null) return;
            Slab("Base", new Vector3((boardWidth-1)*.5f,-.22f,(boardHeight-1)*.5f),
                new Vector3(boardWidth+.5f,.16f,boardHeight+.5f), Solid(StoneDark), boardRoot);
            foreach (var space in board.Spaces)
            {
                if (space == null) continue;
                Slab("Plate "+space.Cell.X+","+space.Cell.Y, CellPosition(space.Cell) + Vector3.down*.07f,
                    new Vector3(.94f,.14f,.94f), Surface(space.Surface), boardRoot);
                if (!string.IsNullOrEmpty(space.Prop)) BuildProp(space.Prop, space.Cell);
            }
            // Nine 3x3 plates keep discreet stone grids between the tiles.
            var grid = MaterialLook("grid", new Color(.26f,.22f,.18f), null, false);
            for (int ty=0; ty<boardHeight/3; ty++) for (int tx=0; tx<boardWidth/3; tx++)
            {
                float x0 = tx*3f-.5f, z0 = ty*3f-.5f;
                Slab("Grid", new Vector3(x0+1.5f,.02f,z0), new Vector3(3f,.04f,.09f), grid, boardRoot);
                Slab("Grid", new Vector3(x0+1.5f,.02f,z0+3f), new Vector3(3f,.04f,.09f), grid, boardRoot);
                Slab("Grid", new Vector3(x0,.02f,z0+1.5f), new Vector3(.09f,.04f,3f), grid, boardRoot);
                Slab("Grid", new Vector3(x0+3f,.02f,z0+1.5f), new Vector3(.09f,.04f,3f), grid, boardRoot);
            }
            // Heavy frame around the whole table: delivered stone/bronze art (repeated, never stretched) or gold.
            var frameArt = Resources.Load<Texture2D>("DungeonBDX/Iso/Frame/frame");
            var gold = frameArt ? MaterialLook("frame-cap", new Color(.36f,.31f,.26f), null, false) : MaterialLook("frame-gold", GoldTrim, null, false);
            float minX = -.5f, maxX = boardWidth-.5f, minZ = -.5f, maxZ = boardHeight-.5f;
            var frameX = frameArt ? FrameLook("x", frameArt, new Vector2((boardWidth+.78f)/.38f,1f)) : gold;
            var frameZ = frameArt ? FrameLook("z", frameArt, new Vector2(1f,(boardHeight+.78f)/.38f)) : gold;
            Slab("Frame", new Vector3((minX+maxX)*.5f,-.13f,minZ-.19f), new Vector3(boardWidth+.78f,.32f,.38f), frameX, boardRoot);
            Slab("Frame", new Vector3((minX+maxX)*.5f,-.13f,maxZ+.19f), new Vector3(boardWidth+.78f,.32f,.38f), frameX, boardRoot);
            Slab("Frame", new Vector3(minX-.19f,-.13f,(minZ+maxZ)*.5f), new Vector3(.38f,.32f,boardHeight+.78f), frameZ, boardRoot);
            Slab("Frame", new Vector3(maxX+.19f,-.13f,(minZ+maxZ)*.5f), new Vector3(.38f,.32f,boardHeight+.78f), frameZ, boardRoot);
            foreach (var edge in board.Edges)
            {
                if (edge == null) continue;
                bool horizontal = edge.A.X != edge.B.X;
                var mid = EdgeMid(edge.A, edge.B);
                if (edge.Door) BuildDoor(edge, mid, horizontal);
                else
                {
                    // Thin PixelLab wall (texture shown at its real proportion), so props keep the cell.
                    var wall = WallLook(WallArtFor(edge));
                    Slab("Wall", mid + Vector3.up*WallHeight*.5f, horizontal ? new Vector3(WallThickness,WallHeight,1.02f) : new Vector3(1.02f,WallHeight,WallThickness), wall, boardRoot);
                    Slab("Cap", mid + Vector3.up*(WallHeight+.012f), horizontal ? new Vector3(WallThickness+.02f,.024f,1.04f) : new Vector3(1.04f,.024f,WallThickness+.02f), gold, boardRoot);
                }
            }
        }

        void BuildStarts()
        {
            if (scenario == null || scenario.Starts == null) return;
            for (int guild=0; guild<scenario.Starts.Length; guild++)
            {
                var cells = scenario.Starts[guild];
                if (cells == null) continue;
                var color = GuildColors[((guild%GuildColors.Length)+GuildColors.Length)%GuildColors.Length];
                var rim = MaterialLook("start-rim-" + guild, color, null, true);
                foreach (var cell in cells)
                {
                    // Four small corner marks: the guild colour reads without a big disc covering the floor art.
                    for (int corner=0; corner<4; corner++)
                    {
                        var dir = new Vector3(corner%2==0 ? -.36f : .36f, 0f, corner<2 ? -.36f : .36f);
                        Slab("Start mark", CellPosition(cell) + dir + Vector3.up*.006f, new Vector3(.1f,.012f,.1f), rim, boardRoot);
                    }
                }
            }
        }

        public const float WallThickness = .06f, WallHeight = .42f;

        // Crypt rooms get the mossy crypt wall, gardens a hedge; everything else the stone wall.
        string WallArtFor(AqEdge edge)
        {
            string a = board.Contains(edge.A) ? board.Space(edge.A).Surface : null, b = board.Contains(edge.B) ? board.Space(edge.B).Surface : null;
            if (board.Contains(edge.A) && board.Space(edge.A).Wall == "books" || board.Contains(edge.B) && board.Space(edge.B).Wall == "books") return "wall-books";
            if (a == "crypt" || b == "crypt") return "wall-crypt";
            if (a == "garden" || b == "garden") return "hedge";
            return "wall";
        }

        Material WallLook(string id)
        {
            var art = Resources.Load<Texture2D>("DungeonBDX/Iso/Walls/" + id) ?? Resources.Load<Texture2D>("DungeonBDX/Iso/Walls/wall");
            if (!art) return MaterialLook("wall", StoneMid, null, false);
            string key = "wall-art-" + id;
            bool fresh = !materials.ContainsKey(key);
            var material = MaterialLook(key, Color.white, art, false);
            if (fresh)
            {
                // The bookshelf strip is drawn at the wall's own proportions; the square tiles show their lower band.
                var tiling = id == "wall-books" ? Vector2.one : new Vector2(1f, WallHeight);
                material.mainTextureScale = tiling;
                if (material.HasProperty("_BaseMap")) material.SetTextureScale("_BaseMap", tiling);
            }
            return material;
        }

        void BuildDoor(AqEdge edge, Vector3 mid, bool horizontal)
        {
            var along = horizontal ? Vector3.forward : Vector3.right;
            var key = EdgeKey(edge.A, edge.B);
            var root = new GameObject("Door " + key);
            root.transform.SetParent(liveRoot,false);
            var frame = MaterialLook("door-frame", new Color(.44f,.34f,.21f), null, false);
            var doorArt = Resources.Load<Texture2D>("DungeonBDX/Iso/Doors/door");
            var bronze = doorArt ? MaterialLook("door-art", Color.white, doorArt, false) : MaterialLook("door-bronze", new Color(.63f,.44f,.21f), null, false);
            var teal = MaterialLook("door-teal", new Color(.17f,.40f,.38f), null, false);
            var postA = Slab("Post", mid + Vector3.up*.23f - along*.46f, new Vector3(.08f,.46f,.08f), frame, root.transform);
            var postB = Slab("Post", mid + Vector3.up*.23f + along*.46f, new Vector3(.08f,.46f,.08f), frame, root.transform);
            var lintel = Slab("Lintel", mid + Vector3.up*.48f, horizontal ? new Vector3(.1f,.06f,1.02f) : new Vector3(1.02f,.06f,.1f),
                MaterialLook("door-lintel", GoldTrim, null, false), root.transform);
            Slab("Threshold", mid + Vector3.up*.012f, horizontal ? new Vector3(.7f,.024f,1.02f) : new Vector3(1.02f,.024f,.7f),
                MaterialLook("threshold", new Color(.30f,.27f,.23f), null, false), root.transform);
            var panel = Slab("Panel", mid + Vector3.up*.22f, horizontal ? new Vector3(WallThickness,.44f,.86f) : new Vector3(.86f,.44f,WallThickness),
                bronze, root.transform);
            doors[key] = new DoorView {Panel=panel.transform, PanelRenderer=panel.GetComponent<Renderer>(), Bronze=bronze, Teal=teal, Frame=new[]{postA,postB,lintel}};
        }

        void BuildTokens()
        {
            if (scenario == null || scenario.Tokens == null) return;
            foreach (var token in scenario.Tokens)
            {
                if (token == null || string.IsNullOrEmpty(token.Id) || tokens.ContainsKey(token.Id)) continue;
                var root = new GameObject("Token " + token.Kind + " " + token.Id);
                root.transform.SetParent(liveRoot,false);
                int slot=scenario.Tokens.Where(t=>t.Cell.Equals(token.Cell)&&t.Kind!="portal"&&t.Kind!="spawn").TakeWhile(t=>t!=token).Count();
                var offset=token.Kind=="portal"?Vector3.zero:token.Kind=="spawn"?new Vector3(-.3f,0f,.3f):new Vector3(.23f-(slot%2)*.4f,.035f+(slot/4)*.05f,-.23f+(slot/2%2)*.4f);
                root.transform.position = CellPosition(token.Cell) + offset;
                var color = TokenColor(token);
                if (token.Kind == "portal")
                {
                    var portalArt = ArtSprite("Tokens/portal", new Vector2(.5f,.5f), 256f/.78f);
                    if (portalArt) FlatSprite("Portal art", root.transform.position + Vector3.up*.014f, portalArt, root.transform);
                    // Thin glowing ring around a dark well instead of a full-cell coloured disc.
                    else Cylinder("Portal rim", root.transform.position + Vector3.up*.008f, new Vector3(.7f,.016f,.7f),
                        MaterialLook("portal-rim-"+token.Color, color, null, true), root.transform);
                    if (!portalArt) Cylinder("Portal", root.transform.position + Vector3.up*.012f, new Vector3(.6f,.016f,.6f),
                        MaterialLook("portal-"+token.Color, Color.Lerp(color,Color.black,.78f), null, true), root.transform);
                    // The spiral art already moves the eye; the orbiting orbs stay only on the flat fallback.
                    var orbs = new Transform[portalArt ? 0 : 3];
                    for (int i=0;i<orbs.Length;i++)
                        orbs[i] = Sphere("Orb", root.transform.position + Vector3.up*.32f, Vector3.one*.12f,
                            MaterialLook("portal-orb", Color.Lerp(color,Color.white,.45f), null, true), root.transform).transform;
                    portals.Add(new PortalMotion {Orbs=orbs, Center=root.transform.position, Radius=.32f, Speed=1.4f + portals.Count*.2f});
                }
                else
                {
                    var chipArt = ArtSprite("Tokens/" + token.Kind, new Vector2(.5f,.5f), 256f/.4f);
                    if (chipArt) FlatSprite("Chip art", root.transform.position + Vector3.up*.012f, chipArt, root.transform);
                    else Cylinder("Chip", root.transform.position + Vector3.up*.02f, new Vector3(.34f,.04f,.34f),
                        MaterialLook("chip-"+token.Kind, color, null, true), root.transform);
                    // Colour only on the table: letters made it read like a board-game print. Icons come from the art lot.
                }
                tokens[token.Id] = root;
            }
        }

        static Color TokenColor(AqToken token)
        {
            string kind = (token.Kind ?? "").Trim().ToLowerInvariant();
            switch (kind)
            {
                case "portal":
                    if (token.Color == "violet") return new Color(.68f,.36f,.92f);
                    if (token.Color == "red") return new Color(.88f,.28f,.32f);
                    if (token.Color == "gold") return new Color(.94f,.76f,.28f);
                    return new Color(.24f,.70f,.95f);
                case "clue": return new Color(.94f,.76f,.28f);
                case "explore": return new Color(.28f,.74f,.58f);
                case "emblem": return new Color(.72f,.46f,.92f);
                case "captive": return new Color(.92f,.52f,.72f);
                case "ritual": return new Color(.86f,.30f,.52f);
                case "hazard": return new Color(.92f,.30f,.24f);
                default: return new Color(.78f,.72f,.60f);
            }
        }

        // Half footprint of each prop on the floor (world units); billboards use half their drawn width.
        static float PropHalf(string prop)
        {
            switch (prop)
            {
                case "table": case "fountain": case "sarcophagus": case "tree": case "rune": return .32f;
                case "barrels": return .25f;
                default: return .24f;
            }
        }

        // PixelLab props (Iso/Props/<id>.png, trimmed, feet at the bottom): drawn at their world height,
        // but never wider than .72 of a cell so the anchor can keep them off the walls.
        static readonly Dictionary<string,float> PropHeights = new Dictionary<string,float> {
            ["tree"]=1.3f, ["pine"]=1.3f, ["deadtree"]=1.2f, ["statue"]=1f, ["pillar"]=.85f, ["candles"]=.75f,
            ["weapons"]=.7f, ["fountain"]=.6f, ["crates"]=.6f, ["barrels"]=.55f, ["table"]=.5f, ["sarcophagus"]=.5f,
            ["chest"]=.45f, ["bush"]=.45f, ["mushrooms"]=.4f, ["bones"]=.3f };
        public const float PropMaxWidth = .72f;

        public static float PropPixelsPerUnit(string prop, Texture2D art)
            => Mathf.Max(art.height/(PropHeights.TryGetValue(prop, out var h) ? h : .6f), art.width/PropMaxWidth);

        void BuildPropArt(string prop, Texture2D art, AqCell cell)
        {
            float ppu = PropPixelsPerUnit(prop, art);
            var sprite = ArtSprite("Props/" + prop, new Vector2(.5f,0f), ppu);
            var go = new GameObject("Prop " + prop, typeof(SpriteRenderer));
            go.transform.SetParent(boardRoot, false);
            go.transform.position = PropAnchor(board, cell, art.width*.5f/ppu);
            go.transform.localScale = new Vector3(1f, IsoStretch, 1f);
            var view = new PropSprite {Renderer=go.GetComponent<SpriteRenderer>(), Facing=Vector3.back, ByDirection=new[]{sprite,sprite,sprite,sprite}};
            propSprites.Add(view);
            RefreshPropSprite(view);
        }

        // Walls reach half their thickness into each cell; the prop leans toward the open side so it never crosses them.
        public static Vector3 PropAnchor(AqBoard board, AqCell cell, float half)
        {
            float Axis(AqCell plus, AqCell minus, float preferred)
            {
                bool blockedPlus = !board.Contains(plus) || board.Edge(cell, plus) != null;
                bool blockedMinus = !board.Contains(minus) || board.Edge(cell, minus) != null;
                float sign = preferred;
                if (sign > 0 ? blockedPlus && !blockedMinus : blockedMinus && !blockedPlus) sign = -sign;
                bool blocked = sign > 0 ? blockedPlus : blockedMinus;
                return sign*Mathf.Clamp((blocked ? .48f - WallThickness*.5f : .48f) - half, 0f, .30f);
            }
            return new Vector3(cell.X, 0f, cell.Y) + new Vector3(Axis(new AqCell(cell.X+1,cell.Y), new AqCell(cell.X-1,cell.Y), 1f),
                0f, Axis(new AqCell(cell.X,cell.Y+1), new AqCell(cell.X,cell.Y-1), -1f));
        }

        void BuildProp(string prop, AqCell cell)
        {
            prop = (prop ?? "").Trim().ToLowerInvariant();
            var art = Resources.Load<Texture2D>("DungeonBDX/Iso/Props/" + prop);
            if (art) { BuildPropArt(prop, art, cell); return; }
            var center = PropAnchor(board, cell, PropHalf(prop));
            var wood = MaterialLook("prop-wood", new Color(.55f,.36f,.20f), null, false);
            var woodDark = MaterialLook("prop-wood-dark", new Color(.36f,.24f,.14f), null, false);
            var stone = MaterialLook("prop-stone", new Color(.60f,.58f,.53f), null, false);
            switch ((prop ?? "").Trim().ToLowerInvariant())
            {
                case "table":
                    Slab("Table", center + Vector3.up*.36f, new Vector3(.64f,.07f,.54f), wood, boardRoot);
                    Slab("Table leg", center + Vector3.up*.17f, new Vector3(.14f,.34f,.14f), woodDark, boardRoot);
                    break;
                case "barrels":
                    Cylinder("Barrel", center + new Vector3(-.10f,.20f,.06f), new Vector3(.30f,.40f,.30f), wood, boardRoot);
                    Cylinder("Barrel", center + new Vector3(.14f,.17f,.08f), new Vector3(.26f,.34f,.26f), woodDark, boardRoot);
                    break;
                case "fountain":
                    Cylinder("Basin", center + Vector3.up*.11f, new Vector3(.68f,.22f,.68f), stone, boardRoot);
                    Cylinder("Water", center + Vector3.up*.215f, new Vector3(.52f,.03f,.52f),
                        MaterialLook("water", new Color(.24f,.60f,.66f), null, true), boardRoot);
                    Cylinder("Pillar", center + Vector3.up*.36f, new Vector3(.16f,.38f,.16f), stone, boardRoot);
                    Cylinder("Bowl", center + Vector3.up*.57f, new Vector3(.34f,.07f,.34f), stone, boardRoot);
                    break;
                case "statue":
                    {
                        var model = Resources.Load<GameObject>("DungeonBDX/Models/column-bronze");
                        if (model)
                        {
                            var instance = Instantiate(model, boardRoot);
                            instance.name = "Statue";
                            FixMaterials(instance);
                            StripColliders(instance);
                            instance.transform.position = center;
                            instance.transform.localScale = Vector3.one*.44f;
                        }
                        else
                        {
                            Slab("Pedestal", center + Vector3.up*.12f, new Vector3(.36f,.24f,.36f), stone, boardRoot);
                            Cylinder("Column", center + Vector3.up*.42f, new Vector3(.22f,.42f,.22f), stone, boardRoot);
                            Sphere("Head", center + Vector3.up*.72f, Vector3.one*.2f, stone, boardRoot);
                        }
                    }
                    break;
                case "sarcophagus":
                    Slab("Sarcophagus", center + Vector3.up*.14f, new Vector3(.66f,.26f,.42f), stone, boardRoot);
                    Slab("Lid", center + Vector3.up*.30f, new Vector3(.62f,.08f,.38f),
                        MaterialLook("prop-stone-dark", StoneDark, null, false), boardRoot);
                    break;
                case "rune" when ArtSprite("Tokens/ritual", new Vector2(.5f,.5f), 256f/.7f):
                    FlatSprite("Rune art", center + Vector3.up*.01f, ArtSprite("Tokens/ritual", new Vector2(.5f,.5f), 256f/.7f), boardRoot);
                    break;
                case "rune":
                    Cylinder("Rune", center + Vector3.up*.02f, new Vector3(.62f,.04f,.62f),
                        MaterialLook("rune", new Color(.62f,.36f,.90f), null, true), boardRoot);
                    Slab("Shard", center + Vector3.up*.24f, new Vector3(.12f,.44f,.12f),
                        MaterialLook("shard", new Color(.78f,.62f,.95f), null, true), boardRoot);
                    break;
                case "tree":
                    Cylinder("Trunk", center + Vector3.up*.22f, new Vector3(.13f,.44f,.13f), woodDark, boardRoot);
                    Sphere("Canopy", center + Vector3.up*.64f, new Vector3(.64f,.56f,.64f),
                        MaterialLook("leaf", new Color(.26f,.42f,.20f), null, false), boardRoot);
                    Sphere("Canopy", center + new Vector3(.14f,.52f,.10f), new Vector3(.42f,.38f,.42f),
                        MaterialLook("leaf-light", new Color(.34f,.52f,.24f), null, false), boardRoot);
                    break;
                case "chest":
                    Slab("Chest", center + Vector3.up*.13f, new Vector3(.42f,.26f,.30f), woodDark, boardRoot);
                    Slab("Chest lid", center + Vector3.up*.29f, new Vector3(.44f,.08f,.32f), wood, boardRoot);
                    Slab("Chest band", center + Vector3.up*.30f, new Vector3(.46f,.10f,.08f),
                        MaterialLook("chest-gold", GoldTrim, null, false), boardRoot);
                    break;
            }
        }

        // ---------------------------------------------------------------- sync

        public void Sync(IReadOnlyList<AqUnit> units,int dyingReaction=-1)
        {
            if (!built) return;
            if (units != null)
            {
                foreach (var unit in units)
                {
                    if (unit == null) continue;
                    if (!views.TryGetValue(unit.Id, out var view)) view = CreateUnit(unit);
                    view.Unit = unit;
                    if (!view.Logical.Equals(unit.Cell)) { view.Route.Clear();view.Root.position=CellPosition(unit.Cell);view.Logical=unit.Cell; }
                    if (animations.TryGetValue(unit.Id, out var animation) && animation)
                    {
                        if (!unit.Alive && unit.Id!=dyingReaction && !deathAfterAttack.ContainsKey(unit.Id) && animation.State != "death") animation.PlayAction(Vector3.zero,dead:true);
                        else if (unit.Alive && animation.State == "death") animation.Revive();
                    }
                    if (view.Root) view.Root.gameObject.SetActive(unit.Alive||!animations[unit.Id].DeathComplete);
                    RefreshLabel(view);
                }
            }
            UpdateDoors();
            RefreshTokens();
            RefreshBillboards();
        }

        AqUnitView CreateUnit(AqUnit unit)
        {
            var root = new GameObject("Unit " + unit.Id);
            root.transform.SetParent(actorRoot,false);
            root.transform.position = CellPosition(unit.Cell);
            // The anchor sits on the feet; its sorting group draws the whole figure by foot depth.
            var body = new GameObject("Anchor", typeof(UnityEngine.Rendering.SortingGroup));
            body.transform.SetParent(root.transform,false);
            var view = new AqUnitView {Id=unit.Id, Unit=unit, Root=root.transform, Body=body.transform, Logical=unit.Cell};
            view.Height = TargetHeight(unit);
            // Player heroes only: slim guild ring around a darker tinted centre. Monsters stand bare on the floor.
            if (!unit.Monster)
            {
                view.Base = Primitive(PrimitiveType.Cylinder,"Base", body.transform.position + Vector3.up*.008f,
                    new Vector3(.4f,.012f,.4f), Solid(BaseColor(unit)), body.transform);
                Primitive(PrimitiveType.Cylinder,"Foot shadow", body.transform.position + Vector3.up*.011f,
                    new Vector3(.32f,.012f,.32f), Solid(Color.Lerp(BaseColor(unit),Color.black,.6f)), body.transform);
            }
            view.Ring = Primitive(PrimitiveType.Cylinder,"Selection", body.transform.position + Vector3.up*.004f,
                new Vector3(.52f,.008f,.52f), Solid(RingGold,true), body.transform);
            view.Ring.SetActive(false);
            view.Visual = CreateVisual(view, body.transform);
            var animation = body.AddComponent<DungeonActorAnimation>();
            animation.Upright = true;
            // Foot anchor is handled inside DungeonActorAnimation; the root stays on the cell plane.
            animation.Initialize(SpriteFolderOf(unit), view.Fallback, view.Visual, viewCamera);
            animations[unit.Id] = animation;
            view.Label = CreateLabel("Unit label", body.transform, view.Height*IsoStretch*HeadLine + .08f, .13f);
            var labelRenderer = view.Label.GetComponent<Renderer>();
            if (labelRenderer) labelRenderer.sortingOrder = 10;
            views[unit.Id] = view;
            RefreshLabel(view);
            return view;
        }

        Transform CreateVisual(AqUnitView view, Transform parent)
        {
            var unit = view.Unit;
            var sprite = FallbackSprite(unit);
            if (sprite)
            {
                var go = new GameObject("Arena sprite", typeof(SpriteRenderer));
                go.transform.SetParent(parent,false);
                var renderer = go.GetComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                var reference = DungeonActorAnimation.ReferenceFrame(SpriteFolderOf(unit)) ?? sprite;
                float height = Mathf.Max(.02f, reference.bounds.size.y);
                float scale = view.Height/height;
                float width = Mathf.Max(.02f, reference.bounds.size.x)*scale;
                float limit = IsWide(unit) ? 3.4f : 1.45f;
                if (width > limit) scale *= limit/width;
                go.transform.localScale = new Vector3(scale, scale*IsoStretch, scale);
                view.Fallback = sprite;
                return go.transform;
            }
            var imported = string.IsNullOrEmpty(unit.HeroId) ? null : Resources.Load<GameObject>("DungeonBDX/Models/"+unit.HeroId);
            if (imported)
            {
                var instance = Instantiate(imported, parent);
                instance.name = "Arena model";
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                FixMaterials(instance);
                StripColliders(instance);
                float modelHeight = 1.6f, floor = 0f;
                var renderers = instance.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    var bounds = renderers[0].bounds;
                    for (int i=1;i<renderers.Length;i++) bounds.Encapsulate(renderers[i].bounds);
                    modelHeight = Mathf.Max(.05f, bounds.size.y);
                    floor = bounds.min.y;
                }
                float scale = view.Height/Mathf.Max(.05f, modelHeight);
                instance.transform.localScale = Vector3.one*scale;
                instance.transform.localPosition = Vector3.up*(-floor*scale);
                view.Fallback = null;
                return instance.transform;
            }
            var capsule = Primitive(PrimitiveType.Capsule,"Fallback figure", parent.position + Vector3.up*(view.Height*.5f),
                new Vector3(view.Height*.42f, view.Height*.5f, view.Height*.42f), Solid(BaseColor(unit)), parent);
            return capsule.transform;
        }

        void RefreshLabel(AqUnitView view)
        {
            if (view == null || view.Label == null || view.Unit == null) return;
            // Names live in the HUD cards; the table keeps only life, so crowded rooms stay readable.
            string text = view.Unit.Monster
                ? view.Unit.Health.ToString()
                : view.Unit.Health + "/" + Mathf.Max(1, view.Unit.MaxHealth);
            view.Label.gameObject.SetActive(ShowLife);
            if (view.LabelText == text) return;
            view.LabelText = text;
            view.Label.text = text;
            view.Label.color = !view.Unit.Alive ? new Color(.6f,.6f,.6f) : view.Unit.Monster ? new Color(1f,.62f,.55f) : Color.white;
            view.Label.ForceMeshUpdate();
        }

        void UpdateDoors()
        {
            if (board == null) return;
            foreach (var edge in board.Edges)
            {
                if (edge == null || !edge.Door) continue;
                if (!doors.TryGetValue(EdgeKey(edge.A,edge.B), out var view) || view.Panel == null) continue;
                bool horizontal = edge.A.X != edge.B.X;
                var mid = EdgeMid(edge.A, edge.B);
                if (view.Frame != null) foreach (var piece in view.Frame) if (piece) piece.SetActive(!edge.Open);
                if (edge.Open)
                {
                    // An open passage is just a gap with a faint sill; the frame only marks closed doors.
                    view.Panel.position = mid + Vector3.up*.015f;
                    view.Panel.localScale = horizontal ? new Vector3(.08f,.02f,.7f) : new Vector3(.7f,.02f,.08f);
                    if (view.PanelRenderer) view.PanelRenderer.sharedMaterial = view.Teal;
                }
                else
                {
                    view.Panel.position = mid + Vector3.up*.22f;
                    view.Panel.localScale = horizontal ? new Vector3(WallThickness,.44f,.86f) : new Vector3(.86f,.44f,WallThickness);
                    if (view.PanelRenderer) view.PanelRenderer.sharedMaterial = view.Bronze;
                }
            }
        }

        void RefreshTokens()
        {
            if (scenario == null || scenario.Tokens == null) return;
            foreach (var token in scenario.Tokens)
            {
                if (token == null || string.IsNullOrEmpty(token.Id)) continue;
                if (tokens.TryGetValue(token.Id, out var root) && root) root.SetActive(!token.Taken);
            }
        }

        // ---------------------------------------------------------------- effects

        public void Apply(AqResult result)
        {
            if (!built || result == null) return;
            foreach (var effect in result.Effects)
            {
                if (effect == null) continue;
                switch ((effect.Kind ?? "").Trim().ToLowerInvariant())
                {
                    case "move": QueueMove(effect.Actor, effect.From, effect.To); break;
                    case "attack": PlayActorAction(effect.Actor, effect.Target, false, false, effect.Clip); break;
                    case "hit":
                        {
                            var target = ViewOf(effect.Target);
                            PlayActorAction(effect.Target, effect.Actor, true, false, effect.Clip);
                            SpawnFloat(effect.Target, Mathf.Abs(effect.Amount), false);
                        }
                        break;
                    case "death":
                        int deadId=effect.Target>=0?effect.Target:effect.Actor;
                        if(animations.TryGetValue(deadId,out var dying)&&(dying.State=="attack"||dying.State=="cast"))deathAfterAttack[deadId]=Time.unscaledTime+.65f;
                        else PlayActorAction(deadId,deadId,false,true,effect.Clip);
                        break;
                    case "heal": SpawnFloat(effect.Target, Mathf.Abs(effect.Amount), true); break;
                    case "door": UpdateDoors(); break;
                    case "token": RefreshTokens(); break;
                }
            }
            UpdateDoors();
            RefreshTokens();
            RefreshBillboards();
        }

        void PlayActorAction(int actorId, int targetId, bool hurt, bool dead, string clip)
        {
            if (!views.TryGetValue(actorId, out var view) || view.Root == null) return;
            var direction = Vector3.zero;
            if (views.TryGetValue(targetId, out var target) && target.Root) direction = target.Root.position - view.Root.position;
            if (animations.TryGetValue(actorId, out var animation) && animation)
            {
                string name = (clip ?? "").Trim().ToLowerInvariant();
                bool cast = name == "cast" || name == "zap" || name == "summon";
                if(!hurt&&!dead)animation.PlayClip(name,direction);else animation.PlayAction(direction, hurt, dead, cast);
            }
        }

        void QueueMove(int unitId, AqCell from, AqCell to)
        {
            if (!views.TryGetValue(unitId, out var view) || view.Root == null) return;
            // The domain supplies every committed step. Never invent a second route or
            // erase the previous step when several movement events arrive together.
            if(from.Distance(to)>1){view.Route.Clear();view.Root.position=CellPosition(to);}
            else view.Route.Enqueue(CellPosition(to));
            view.Logical = to;
        }

        void SpawnFloat(int unitId, int amount, bool heal)
        {
            if (!views.TryGetValue(unitId, out var view) || view.Body == null) return;
            var go = new GameObject("Feedback", typeof(TextMeshPro));
            go.transform.SetParent(actorRoot,false);
            go.transform.position = view.Body.position + Vector3.up*(view.Height*IsoStretch*HeadLine + .26f);
            go.transform.localScale = Vector3.one*.22f;
            var label = go.GetComponent<TextMeshPro>();
            label.font = TMP_Settings.defaultFontAsset;
            label.fontSize = 16;
            label.alignment = TextAlignmentOptions.Center;
            label.outlineWidth = .25f;
            label.outlineColor = Color.black;
            label.rectTransform.sizeDelta = new Vector2(4f,2f);
            label.color = heal ? new Color(.45f,1f,.58f) : new Color(1f,.44f,.36f);
            label.text = (heal ? "+" : "-") + Mathf.Max(1, amount);
            label.ForceMeshUpdate();
            var renderer = go.GetComponent<Renderer>();
            if (renderer) renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            billboards.Add(go.transform);
            floats.Add(new Floating {Go=go, Origin=go.transform.position, Expires=Time.unscaledTime+1.1f});
        }

        public void Select(int unitId)
        {
            selected = unitId;
            foreach (var view in views.Values) if (view.Ring) view.Ring.SetActive(view.Id == selected);
        }

        public void ShowCells(IReadOnlyDictionary<AqCell,int> cells, bool movement)
        {
            foreach (var marker in cellMarkers.Values) if (marker) marker.SetActive(false);
            if (!built || cells == null) return;
            var material = Solid(movement ? MoveGreen : TargetGold, true);
            foreach (var entry in cells)
            {
                if (!cellMarkers.TryGetValue(entry.Key, out var marker))
                {
                    marker = new GameObject("Cell " + entry.Key.X + "," + entry.Key.Y, typeof(LineRenderer));
                    marker.transform.SetParent(markerRoot,false);
                    var line = marker.GetComponent<LineRenderer>();
                    line.positionCount = 5;
                    line.startWidth = line.endWidth = .03f;
                    line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    line.useWorldSpace = true;
                    var point = CellPosition(entry.Key) + Vector3.up*.05f;
                    line.SetPositions(new[]
                    {
                        point+new Vector3(-.42f,0f,-.42f), point+new Vector3(.42f,0f,-.42f),
                        point+new Vector3(.42f,0f,.42f), point+new Vector3(-.42f,0f,.42f), point+new Vector3(-.42f,0f,-.42f)
                    });
                    var label = CreateLabel("Distance", marker.transform, 0f, .16f);
                    label.transform.position = point + Vector3.up*.12f;
                    label.fontSize = 14;
                    label.color = new Color(1f,1f,1f,.7f);
                    cellMarkers[entry.Key] = marker;
                }
                marker.SetActive(true);
                var markerLine = marker.GetComponent<LineRenderer>();
                if (markerLine) markerLine.sharedMaterial = material;
                var distance = marker.GetComponentInChildren<TextMeshPro>();
                if (distance)
                {
                    distance.text = entry.Value.ToString();
                    distance.ForceMeshUpdate();
                }
            }
            RefreshBillboards();
        }

        // ---------------------------------------------------------------- picking

        public bool Pick(Vector2 screen, out AqCell cell, out int unitId)
        {
            cell = default;
            unitId = -1;
            if (!built || !viewCamera || board == null) return false;
            if (Bad(screen.x) || Bad(screen.y)) return false;
            // Bases first: two occupants of one cell keep their own screen anchor.
            AqUnitView chosen = null;
            float nearest = float.MaxValue;
            foreach (var view in views.Values)
            {
                if (view.Root == null || view.Unit == null || !view.Unit.Alive) continue;
                var foot = (Vector2)viewCamera.WorldToScreenPoint(view.Root.position);
                var edge = (Vector2)viewCamera.WorldToScreenPoint(view.Root.position + viewCamera.transform.right*.30f);
                if (Bad(foot.x) || Bad(foot.y)) continue;
                float radius = Mathf.Max(8f, Vector2.Distance(foot,edge));
                float distance = Vector2.Distance(screen, foot);
                if (distance <= radius && distance < nearest) { nearest = distance; chosen = view; }
            }
            if (chosen != null)
            {
                cell = chosen.Unit.Cell;
                unitId = chosen.Id;
                return true;
            }
            foreach (var view in views.Values)
            {
                if (view.Visual == null || view.Unit == null || !view.Unit.Alive) continue;
                var renderers = view.Visual.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0) continue;
                var bounds = renderers[0].bounds;
                for (int i=1;i<renderers.Length;i++) bounds.Encapsulate(renderers[i].bounds);
                Vector2 min = new Vector2(float.MaxValue,float.MaxValue), max = new Vector2(float.MinValue,float.MinValue);
                for (int corner=0;corner<8;corner++)
                {
                    var point = viewCamera.WorldToScreenPoint(bounds.center + Vector3.Scale(bounds.extents,
                        new Vector3((corner&1)==0?-1f:1f,(corner&2)==0?-1f:1f,(corner&4)==0?-1f:1f)));
                    min = Vector2.Min(min,(Vector2)point);
                    max = Vector2.Max(max,(Vector2)point);
                }
                var rect = Rect.MinMaxRect(min.x,min.y,max.x,max.y);
                float distance = (screen-rect.center).sqrMagnitude;
                if (rect.Contains(screen) && distance < nearest) { nearest = distance; chosen = view; }
            }
            if (chosen != null)
            {
                cell = chosen.Unit.Cell;
                unitId = chosen.Id;
                return true;
            }
            var ray = viewCamera.ScreenPointToRay(screen);
            if (new Plane(Vector3.up,Vector3.zero).Raycast(ray,out float hit))
            {
                var point = ray.GetPoint(hit);
                var target = new AqCell(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.z));
                if (board.Contains(target))
                {
                    cell = target;
                    unitId = -1;
                    return true;
                }
            }
            return false;
        }

        // ---------------------------------------------------------------- camera

        public Vector3 CellPosition(AqCell cell) => new Vector3(cell.X, 0f, cell.Y);

        public void ResetCamera()
        {
            shownYaw = yawTarget = IsoYaw;
            snapPending = false;
            Zoom = 1f;
            pan = Vector2.zero;
            ApplyCamera();
        }

        /// <summary>Full 360 degree orbit. Gestures turn freely; the view eases to the nearest isometric rest.</summary>
        public void RotateCamera(float degrees)
        {
            if (Bad(degrees)) return;
            yawTarget = Mathf.Repeat(yawTarget + degrees, 360f);
            snapPending = true;
            lastRotate = Time.unscaledTime;
        }

        /// <summary>Jumps to the requested isometric rest (captures and tests).</summary>
        public void SettleCamera()
        {
            yawTarget = RestYaw(yawTarget);
            shownYaw = yawTarget;
            snapPending = false;
            ApplyCamera();
        }

        static float RestYaw(float yaw) => Mathf.Repeat(IsoYaw + 90f*Mathf.Round((yaw - IsoYaw)/90f), 360f);

        void EaseCamera()
        {
            if (snapPending && Time.unscaledTime - lastRotate > .3f) { yawTarget = RestYaw(yawTarget); snapPending = false; }
            float gap = Mathf.DeltaAngle(shownYaw, yawTarget);
            if (Mathf.Abs(gap) < .01f) return;
            shownYaw = Mathf.Abs(gap) < .05f ? yawTarget
                : Mathf.Repeat(shownYaw + gap*(1f - Mathf.Exp(-14f*Mathf.Max(0f,Time.unscaledDeltaTime))), 360f);
            ApplyCamera();
        }

        public void ZoomCamera(float factor)
        {
            if (Bad(factor) || factor <= 0f) return;
            Zoom = Mathf.Clamp(Zoom*factor, 1f, 3f);
            ApplyCamera();
        }

        public void PanCamera(Vector2 pixelDelta)
        {
            if (Bad(pixelDelta.x) || Bad(pixelDelta.y) || !viewCamera) return;
            // Ground-locked drag: a vertical screen step covers 1/sin(pitch) cells on the floor.
            var turn = Quaternion.Euler(0f,shownYaw,0f);
            var delta = -(turn*Vector3.right*pixelDelta.x + turn*Vector3.forward*(pixelDelta.y/Mathf.Sin(IsoPitch*Mathf.Deg2Rad)))
                *(2f*viewCamera.orthographicSize/Mathf.Max(1f,Screen.height));
            pan += new Vector2(delta.x,delta.z);
            ApplyCamera();
        }

        void ApplyCamera()
        {
            if (!built || !viewCamera || board == null) return;
            float factor = 1f - 1f/Mathf.Clamp(Zoom,1f,3f);
            pan = new Vector2(Mathf.Clamp(pan.x,-boardWidth*.45f*factor,boardWidth*.45f*factor),
                Mathf.Clamp(pan.y,-boardHeight*.45f*factor,boardHeight*.45f*factor));
            Frame(viewCamera,boardWidth,boardHeight,Screen.safeArea,Screen.width,Screen.height,shownYaw,Zoom,pan);
            RefreshBillboards();
        }

        static void Frame(UnityEngine.Camera camera,int mapWidth,int mapHeight,Rect safe,int width,int height,float yaw,float zoom,Vector2 offset)
        {
            if (!camera || mapWidth <= 0 || mapHeight <= 0) return;
            float w = Mathf.Max(1f,width), h = Mathf.Max(1f,height), aspect = w/h;
            // Header strips (12%) and the compact footer stay free; the camera column takes the right edge.
            bool portrait = width < height;
            float bottom = portrait ? .385f : .215f, across = portrait ? .86f : .9f;
            var usable = new Rect((safe.x + safe.width*.02f)/w,(safe.y + safe.height*bottom)/h,
                Mathf.Max(.05f,safe.width*across/w),Mathf.Max(.08f,safe.height*(.875f-bottom)/h));
            var center = new Vector3((mapWidth-1)*.5f,0f,(mapHeight-1)*.5f);
            camera.aspect = aspect;
            camera.transform.rotation = Quaternion.Euler(IsoPitch,yaw,0f);
            Vector3 right = camera.transform.right, up = camera.transform.up;
            // Fit the framed table plus standing figures (feet to ~1.25 high) in every view angle.
            float minR = float.MaxValue, maxR = float.MinValue, minU = float.MaxValue, maxU = float.MinValue;
            for (int corner=0; corner<8; corner++)
            {
                var point = new Vector3(((corner&1)==0?-1f:1f)*(mapWidth*.5f+.42f),(corner&4)==0?-.3f:1f,((corner&2)==0?-1f:1f)*(mapHeight*.5f+.42f));
                float r = Vector3.Dot(point,right), u = Vector3.Dot(point,up);
                minR = Mathf.Min(minR,r); maxR = Mathf.Max(maxR,r); minU = Mathf.Min(minU,u); maxU = Mathf.Max(maxU,u);
            }
            camera.transform.position = center + right*((minR+maxR)*.5f) + up*((minU+maxU)*.5f) - camera.transform.forward*40f;
            camera.orthographicSize = Mathf.Max((maxU-minU)*.5f/usable.height, (maxR-minR)*.5f/aspect/usable.width);
            camera.orthographicSize /= Mathf.Clamp(zoom,1f,3f);
            camera.transform.position -= camera.transform.right*((usable.center.x-.5f)*2f*camera.orthographicSize*aspect)
                + camera.transform.up*((usable.center.y-.5f)*2f*camera.orthographicSize);
            float factor = 1f - 1f/Mathf.Clamp(zoom,1f,3f);
            camera.transform.position += new Vector3(
                Mathf.Clamp(offset.x,-mapWidth*.45f*factor,mapWidth*.45f*factor),0f,
                Mathf.Clamp(offset.y,-mapHeight*.45f*factor,mapHeight*.45f*factor));
        }

        void RefreshBillboards()
        {
            for (int i=billboards.Count-1;i>=0;i--) if (!billboards[i]) billboards.RemoveAt(i);
            var rotation = viewCamera ? viewCamera.transform.rotation : Quaternion.identity;
            foreach (var billboard in billboards) if (billboard) billboard.rotation = rotation;
            foreach (var prop in propSprites) RefreshPropSprite(prop);
            ApplyOccupancy();
            foreach (var animation in animations.Values) if (animation) animation.RefreshFacing();
        }

        void ApplyOccupancy()
        {
            if (!viewCamera) return;
            var right = viewCamera.transform.right;
            right.y = 0f;
            if (right.sqrMagnitude < .0001f) right = Vector3.right;
            right.Normalize();
            var away = Vector3.Cross(right, Vector3.up);
            foreach (var group in views.Values.Where(v => v.Unit != null && v.Unit.Alive && v.Body != null).GroupBy(v => v.Unit.Cell))
            {
                var list = group.OrderBy(v => v.Id).ToList();
                for (int i=0;i<list.Count;i++)
                {
                    var view = list[i];
                    var offset = Vector3.zero;
                    // Shared cell: back-left and front-right, so the feet never tie in depth order.
                    float t = list.Count > 1 ? i/(float)(list.Count-1) - .5f : 0f;
                    if (list.Count > 1 && !IsWide(view.Unit))
                        offset = right*(t*.46f) - away*(t*.22f);
                    view.Body.localPosition = offset;
                }
            }
        }

        // ---------------------------------------------------------------- loop

        void Update()
        {
            if (!built) return;
            EaseCamera();
            foreach(var id in deathAfterAttack.Where(p=>Time.unscaledTime>=p.Value).Select(p=>p.Key).ToArray())
            {deathAfterAttack.Remove(id);if(views.TryGetValue(id,out var dead)&&!dead.Unit.Alive)PlayActorAction(id,id,false,true,"death");}
            float blend = 1f - Mathf.Exp(-7f*Mathf.Max(0f,Time.unscaledDeltaTime));
            foreach (var view in views.Values)
            {
                if (view.Root == null) continue;
                if(!view.Unit.Alive&&animations[view.Id].DeathComplete){view.Root.gameObject.SetActive(false);continue;}
                bool moving = view.Route.Count > 0;
                var direction = moving ? view.Route.Peek() - view.Root.position : Vector3.zero;
                if (animations.TryGetValue(view.Id, out var animation) && animation) animation.SetMoving(direction,moving);
                if (!moving) continue;
                view.Root.position = Vector3.Lerp(view.Root.position,view.Route.Peek(),blend);
                if ((view.Root.position-view.Route.Peek()).sqrMagnitude < .0004f)
                {
                    view.Root.position = view.Route.Peek();
                    view.Route.Dequeue();
                }
            }
            AnimatePortals();
            AnimateFloats();
        }

        void AnimatePortals()
        {
            float time = Time.unscaledTime;
            foreach (var portal in portals)
            {
                if (portal.Orbs == null) continue;
                for (int i=0;i<portal.Orbs.Length;i++)
                {
                    var orb = portal.Orbs[i];
                    if (!orb) continue;
                    float angle = time*portal.Speed + i*Mathf.PI*2f/portal.Orbs.Length;
                    orb.position = portal.Center + new Vector3(Mathf.Cos(angle)*portal.Radius,
                        .30f + Mathf.Sin(time*2.2f + i)*.06f, Mathf.Sin(angle)*portal.Radius);
                }
            }
        }

        void AnimateFloats()
        {
            for (int i=floats.Count-1;i>=0;i--)
            {
                var entry = floats[i];
                if (!entry.Go) { floats.RemoveAt(i); continue; }
                if (Time.unscaledTime >= entry.Expires)
                {
                    billboards.Remove(entry.Go.transform);
                    Destroy(entry.Go);
                    floats.RemoveAt(i);
                    continue;
                }
                float travel = 1f - Mathf.Clamp01((entry.Expires-Time.unscaledTime)/1.1f);
                entry.Go.transform.position = entry.Origin + Vector3.up*(.22f + travel*.5f);
                var label = entry.Go.GetComponent<TextMeshPro>();
                if (label)
                {
                    var color = label.color;
                    color.a = 1f - travel*.85f;
                    label.color = color;
                }
            }
        }

        // ---------------------------------------------------------------- helpers

        static float TargetHeight(AqUnit unit)
        {
            // Figures stay smaller than a space (the table reads as a map, not as oversized miniatures).
            return FigureScale*(unit.Monster&&!unit.Huge&&unit.Reward<2?Mathf.Min(1.35f,AqHeroImport.Height(unit.HeroId)):AqHeroImport.Height(unit.HeroId));
        }

        static bool IsWide(AqUnit unit) => unit != null && (unit.Huge || unit.HeroId == "colosso");

        static Color BaseColor(AqUnit unit)
        {
            int guild = unit.Monster ? 3 : unit.Guild;
            return GuildColors[((guild % GuildColors.Length)+GuildColors.Length)%GuildColors.Length];
        }

        static string SpriteFolderOf(AqUnit unit)
        {
            if (unit == null) return null;
            if (!string.IsNullOrEmpty(unit.SpriteFolder)) return unit.SpriteFolder;
            if (string.IsNullOrEmpty(unit.HeroId)) return null;
            try
            {
                return ArenaTatica.Core.Content.Fighters[unit.HeroId].sprites;
            }
            catch (Exception)
            {
                return null;
            }
        }

        static Sprite FallbackSprite(AqUnit unit)
        {
            var folder = SpriteFolderOf(unit);
            if (string.IsNullOrEmpty(folder)) return null;
            folder = folder.Trim().TrimStart('/').Replace("Sprites/","");
            var south = Resources.Load<Sprite>("Sprites/" + folder + "/south");
            if (south) return south;
            var all = Resources.LoadAll<Sprite>("Sprites/" + folder);
            return all != null && all.Length > 0 ? all[0] : null;
        }

        AqUnitView ViewOf(int unitId) => views.TryGetValue(unitId, out var view) ? view : null;

        static string EdgeKey(AqCell a, AqCell b)
        {
            int minX = Mathf.Min(a.X,b.X), minY = Mathf.Min(a.Y,b.Y), maxX = Mathf.Max(a.X,b.X), maxY = Mathf.Max(a.Y,b.Y);
            return minX+","+minY+":"+maxX+","+maxY;
        }

        static Vector3 EdgeMid(AqCell a, AqCell b)
            => new Vector3((a.X+b.X)*.5f,0f,(a.Y+b.Y)*.5f);

        static bool Bad(float value) => float.IsNaN(value) || float.IsInfinity(value);

        TextMeshPro CreateLabel(string name, Transform parent, float height, float size)
        {
            var go = new GameObject(name, typeof(TextMeshPro));
            go.transform.SetParent(parent,false);
            go.transform.position = parent.position + Vector3.up*height;
            go.transform.localScale = Vector3.one*size;
            var label = go.GetComponent<TextMeshPro>();
            label.font = TMP_Settings.defaultFontAsset;
            label.fontSize = 14;
            label.alignment = TextAlignmentOptions.Center;
            label.outlineWidth = .2f;
            label.outlineColor = Color.black;
            label.rectTransform.sizeDelta = new Vector2(5f,2f);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.color = Color.white;
            var renderer = go.GetComponent<Renderer>();
            if (renderer) renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            billboards.Add(go.transform);
            return label;
        }

        GameObject Slab(string name, Vector3 position, Vector3 size, Material material, Transform parent)
            => Primitive(PrimitiveType.Cube,name,position,size,material,parent);

        GameObject Cylinder(string name, Vector3 position, Vector3 size, Material material, Transform parent)
            => Primitive(PrimitiveType.Cylinder,name,position,size,material,parent);

        GameObject Sphere(string name, Vector3 position, Vector3 size, Material material, Transform parent)
            => Primitive(PrimitiveType.Sphere,name,position,size,material,parent);

        GameObject Primitive(PrimitiveType type, string name, Vector3 position, Vector3 scale, Material material, Transform parent)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent,false);
            go.transform.position = position;
            go.transform.localScale = scale;
            var renderer = go.GetComponent<Renderer>();
            if (renderer) renderer.sharedMaterial = material;
            StripColliders(go);
            return go;
        }

        static void StripColliders(GameObject go)
        {
            foreach (var collider in go.GetComponentsInChildren<Collider>())
            {
                if (Application.isPlaying) Destroy(collider);
                else DestroyImmediate(collider);
            }
        }

        void FixMaterials(GameObject root)
        {
            foreach (var renderer in root.GetComponentsInChildren<Renderer>())
            {
                var source = renderer.sharedMaterials;
                var replaced = new Material[source.Length];
                for (int i=0;i<source.Length;i++)
                {
                    var color = source[i] && source[i].HasProperty("_Color") ? source[i].color : new Color(.55f,.46f,.34f);
                    replaced[i] = Solid(color);
                }
                renderer.sharedMaterials = replaced;
            }
        }

        Material Solid(Color color, bool unlit = false)
            => MaterialLook((unlit ? "unlit-" : "lit-") + ColorUtility.ToHtmlStringRGBA(color), color, null, unlit);

        Material MaterialLook(string key, Color color, Texture2D texture, bool unlit)
        {
            if (materials.TryGetValue(key, out var cached)) return cached;
            var template = Resources.Load<Material>("DungeonBDX/Materials/" + (unlit ? "SkillUnlit" : "WorldLit"));
            var shader = Shader.Find(unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit")
                ?? Shader.Find("Universal Render Pipeline/Lit");
            if (!template && !shader) throw new InvalidOperationException("Execute DungeonBuild.Prepare para incluir os materiais DungeonBDX.");
            var material = template ? new Material(template) : new Material(shader);
            material.name = key;
            material.color = color;
            material.enableInstancing = true;
            if (texture)
            {
                material.mainTexture = texture;
                if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap",texture);
            }
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness",texture ? .16f : .22f);
            materials[key] = material;
            return material;
        }

        // ---------------------------------------------------------------- delivered sprites (lote Tripo)

        Sprite ArtSprite(string path, Vector2 pivot, float pixelsPerUnit)
        {
            string key = path + "@" + pixelsPerUnit.ToString("0.0");
            if (artSprites.TryGetValue(key, out var cached)) return cached;
            var texture = Resources.Load<Texture2D>("DungeonBDX/Iso/" + path);
            var sprite = texture ? Sprite.Create(texture, new Rect(0,0,texture.width,texture.height), pivot, pixelsPerUnit) : null;
            artSprites[key] = sprite;
            return sprite;
        }

        // Flat on the floor and drawn under every figure, so feet never hide behind a token.
        void FlatSprite(string name, Vector3 position, Sprite sprite, Transform parent)
        {
            var go = new GameObject(name, typeof(SpriteRenderer));
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = -20;
        }

        void RefreshPropSprite(PropSprite prop)
        {
            if (prop == null || !prop.Renderer || !viewCamera) return;
            var north = viewCamera.transform.forward; north.y = 0f;
            if (north.sqrMagnitude < .0001f) north = Vector3.forward;
            north.Normalize();
            var right = Vector3.Cross(Vector3.up, north);
            float angle = Mathf.Atan2(Vector3.Dot(prop.Facing, right), Vector3.Dot(prop.Facing, -north))*Mathf.Rad2Deg;
            int index = ((Mathf.FloorToInt(angle/90f) % 4) + 4) % 4; // quadrant: 0..90 = index 1, 90..180 = 3, ...
            prop.Renderer.sprite = prop.ByDirection[index];
            prop.Renderer.transform.rotation = Quaternion.Euler(0f, viewCamera.transform.eulerAngles.y, 0f);
        }

        Material FrameLook(string axis, Texture2D art, Vector2 tiling)
        {
            string key = "frame-art-" + axis + "-" + tiling.x.ToString("0.00") + "x" + tiling.y.ToString("0.00");
            bool fresh = !materials.ContainsKey(key);
            var material = MaterialLook(key, Color.white, art, false);
            if (fresh)
            {
                material.mainTextureScale = tiling;
                if (material.HasProperty("_BaseMap")) material.SetTextureScale("_BaseMap", tiling);
            }
            return material;
        }

        Material Surface(string surface)
        {
            string key = (surface ?? "street").Trim().ToLowerInvariant();
            // Delivered tile art (contract ISOMETRICO-2D) replaces the procedural floor of that surface.
            var art = Resources.Load<Texture2D>("DungeonBDX/Iso/Tiles/" + key);
            if (art) return MaterialLook("surface-art-" + key, Color.white, art, false);
            switch (key)
            {
                case "wood": return MaterialLook("surface-wood",Color.white,Wood(),false);
                case "terracotta": return MaterialLook("surface-terracotta",Color.white,Terracotta(),false);
                case "mosaic": return MaterialLook("surface-mosaic",Color.white,Mosaic(),false);
                case "marble": return MaterialLook("surface-marble",Color.white,Marble(),false);
                case "garden": return MaterialLook("surface-garden",Color.white,Garden(),false);
                case "crypt": return MaterialLook("surface-crypt",Color.white,Crypt(),false);
                default: return MaterialLook("surface-street",Color.white,Street(),false);
            }
        }

        // ---------------------------------------------------------------- floor art

        Texture2D NewTexture(string name, int size)
        {
            var texture = new Texture2D(size,size,TextureFormat.RGBA32,true);
            texture.name = "AQ " + name;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            texture.anisoLevel = 2;
            textures.Add(texture);
            return texture;
        }

        Texture2D Street()
        {
            if (!streetTexture) streetTexture = Cobble("street",new Color(.70f,.60f,.44f),new Color(.84f,.70f,.45f),
                new Color(.20f,.16f,.13f),5,256);
            return streetTexture;
        }

        Texture2D Crypt()
        {
            if (!cryptTexture) cryptTexture = Cobble("crypt",new Color(.44f,.43f,.43f),new Color(.58f,.55f,.50f),
                new Color(.11f,.11f,.13f),5,256);
            return cryptTexture;
        }

        Texture2D Wood()
        {
            if (!woodTexture) woodTexture = BuildWood("wood",new Color(.62f,.42f,.24f),new Color(.34f,.22f,.12f),
                new Color(.20f,.13f,.08f),6,256);
            return woodTexture;
        }

        Texture2D Terracotta()
        {
            if (!terracottaTexture) terracottaTexture = Tiles("terracotta",new Color(.42f,.31f,.24f),
                new[]{new Color(.78f,.45f,.28f),new Color(.70f,.38f,.26f),new Color(.82f,.52f,.32f)},4,256,2.5f);
            return terracottaTexture;
        }

        Texture2D Mosaic()
        {
            if (!mosaicTexture) mosaicTexture = Tiles("mosaic",new Color(.72f,.67f,.56f),
                new[]{new Color(.84f,.68f,.32f),new Color(.78f,.44f,.28f),new Color(.62f,.60f,.30f),
                    new Color(.84f,.78f,.58f),new Color(.70f,.52f,.30f)},8,256,2f);
            return mosaicTexture;
        }

        Texture2D Marble()
        {
            if (!marbleTexture)
            {
                var texture = NewTexture("marble",256);
                var pixels = new Color[256*256];
                for (int y=0;y<256;y++) for (int x=0;x<256;x++)
                {
                    float detail = Mathf.PerlinNoise(x*.05f,y*.05f);
                    var color = Color.Lerp(new Color(.88f,.86f,.80f),new Color(.74f,.72f,.68f),detail*.5f);
                    float vein = Mathf.Abs(Mathf.Sin(x*.022f + y*.016f + Mathf.PerlinNoise(y*.012f,3f)*3.4f));
                    if (vein < .09f) color = Color.Lerp(color,new Color(.58f,.56f,.54f),.6f);
                    if (vein > .995f) color = Color.Lerp(color,new Color(.60f,.56f,.52f),.35f);
                    pixels[y*256+x] = color;
                }
                texture.SetPixels(pixels);
                texture.Apply(true,false);
                marbleTexture = texture;
            }
            return marbleTexture;
        }

        Texture2D Garden()
        {
            if (!gardenTexture)
            {
                var texture = NewTexture("garden",256);
                var pixels = new Color[256*256];
                for (int y=0;y<256;y++) for (int x=0;x<256;x++)
                {
                    float patch = Mathf.PerlinNoise(x*.045f,y*.045f);
                    var color = Color.Lerp(new Color(.24f,.36f,.18f),new Color(.50f,.60f,.30f),patch);
                    float gravel = ((x*13+y*7)%11==0) ? .82f : 1f;
                    pixels[y*256+x] = new Color(color.r*gravel,color.g*gravel,color.b*gravel,1f);
                }
                texture.SetPixels(pixels);
                texture.Apply(true,false);
                gardenTexture = texture;
            }
            return gardenTexture;
        }

        Texture2D Cobble(string name, Color light, Color dark, Color joint, int grid, int size)
        {
            var texture = NewTexture(name,size);
            var random = new System.Random(name.GetHashCode() ^ 7919);
            var sites = new Vector2[grid*grid];
            var tint = new float[grid*grid];
            float spacing = size/(float)grid;
            for (int index=0;index<sites.Length;index++)
            {
                int gx = index%grid, gy = index/grid;
                sites[index] = new Vector2((gx+.5f+((float)random.NextDouble()-.5f)*.55f)*spacing,
                    (gy+.5f+((float)random.NextDouble()-.5f)*.55f)*spacing);
                tint[index] = (float)random.NextDouble();
            }
            var pixels = new Color[size*size];
            for (int y=0;y<size;y++) for (int x=0;x<size;x++)
            {
                float nearest = float.MaxValue, second = float.MaxValue;
                int closest = 0;
                int baseX = Mathf.FloorToInt(x/spacing), baseY = Mathf.FloorToInt(y/spacing);
                for (int oy=-1;oy<=1;oy++) for (int ox=-1;ox<=1;ox++)
                {
                    int ix = baseX+ox, iy = baseY+oy;
                    if (ix<0||iy<0||ix>=grid||iy>=grid) continue;
                    int index = iy*grid+ix;
                    float squared = (new Vector2(x+.5f,y+.5f)-sites[index]).sqrMagnitude;
                    if (squared < nearest) { second = nearest; nearest = squared; closest = index; }
                    else if (squared < second) second = squared;
                }
                float edge = second >= float.MaxValue ? 99f : Mathf.Sqrt(second)-Mathf.Sqrt(Mathf.Max(0f,nearest));
                float blend = Mathf.Clamp01(edge/spacing*.7f);
                var stone = Color.Lerp(dark,light,tint[closest]);
                float grain = (x*5+y*3)%7==0 ? .93f : 1f;
                pixels[y*size+x] = Color.Lerp(joint,new Color(stone.r*grain,stone.g*grain,stone.b*grain,1f),blend);
            }
            texture.SetPixels(pixels);
            texture.Apply(true,false);
            return texture;
        }

        Texture2D BuildWood(string name, Color light, Color dark, Color seam, int planks, int size)
        {
            var texture = NewTexture(name,size);
            var random = new System.Random(name.GetHashCode() ^ 3571);
            var tone = new float[planks];
            var shift = new float[planks];
            for (int i=0;i<planks;i++)
            {
                tone[i] = (float)random.NextDouble();
                shift[i] = (float)random.NextDouble()*80f;
            }
            var pixels = new Color[size*size];
            float height = size/(float)planks;
            for (int y=0;y<size;y++) for (int x=0;x<size;x++)
            {
                int row = Mathf.Clamp(Mathf.FloorToInt(y/height),0,planks-1);
                float grain = Mathf.PerlinNoise(shift[row] + x*.07f,shift[row]*.5f + y*.9f);
                float wear = Mathf.PerlinNoise(shift[row]*.3f + x*.02f,y*.02f + 5f);
                var color = Color.Lerp(dark,light,Mathf.Clamp01(tone[row]*.5f + grain*.45f + wear*.2f));
                float edge = Mathf.Min(y%height,height - y%height);
                int seamX = Mathf.Abs((x + (int)(shift[row]*37f))%size);
                if (edge < 1.2f || seamX < 1.5f) color = Color.Lerp(color,seam,.8f);
                pixels[y*size+x] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply(true,false);
            return texture;
        }

        Texture2D Tiles(string name, Color grout, Color[] palette, int grid, int size, float seam)
        {
            var texture = NewTexture(name,size);
            var random = new System.Random(name.GetHashCode() ^ 6263);
            var tiles = new Color[grid*grid];
            for (int i=0;i<tiles.Length;i++)
            {
                var pick = palette[random.Next(palette.Length)];
                float shade = .88f + (float)random.NextDouble()*.24f;
                tiles[i] = new Color(pick.r*shade,pick.g*shade,pick.b*shade,1f);
            }
            var pixels = new Color[size*size];
            float spacing = size/(float)grid;
            for (int y=0;y<size;y++) for (int x=0;x<size;x++)
            {
                int tx = Mathf.Clamp(Mathf.FloorToInt(x/spacing),0,grid-1);
                int ty = Mathf.Clamp(Mathf.FloorToInt(y/spacing),0,grid-1);
                float dx = x%spacing, dy = y%spacing;
                dx = Mathf.Min(dx,spacing-dx);
                dy = Mathf.Min(dy,spacing-dy);
                var color = dx < seam || dy < seam ? grout : tiles[ty*grid+tx];
                if (dx >= seam && dy >= seam)
                {
                    float speck = Mathf.PerlinNoise(x*.11f,y*.11f);
                    color = Color.Lerp(color,grout,Mathf.Clamp01((speck-.5f)*.3f));
                }
                pixels[y*size+x] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply(true,false);
            return texture;
        }

        // ---------------------------------------------------------------- teardown

        void OnDestroy()
        {
            foreach (var material in materials.Values) DestroyObject(material);
            materials.Clear();
            foreach (var texture in textures) DestroyObject(texture);
            textures.Clear();
            portals.Clear();
            propSprites.Clear();
            foreach (var sprite in artSprites.Values) if (sprite) DestroyObject(sprite);
            artSprites.Clear();
            floats.Clear();
            views.Clear();
            animations.Clear();
            doors.Clear();
            tokens.Clear();
            cellMarkers.Clear();
            billboards.Clear();
            if (cameraObject) DestroyObject(cameraObject);
            cameraObject = null;
            viewCamera = null;
        }

        static void DestroyObject(UnityEngine.Object target)
        {
            if (!target) return;
            if (Application.isPlaying) Destroy(target);
            else DestroyImmediate(target);
        }
    }
}
