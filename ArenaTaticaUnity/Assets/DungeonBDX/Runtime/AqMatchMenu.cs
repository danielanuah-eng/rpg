using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DungeonBDX.Arcadia;
using TMPro;
using UnityEngine;

namespace DungeonBDX
{
    internal static class AqUi
    {
        public static readonly Color Ink=new Color(.95f,.9f,.8f),Gold=new Color(.83f,.65f,.35f),Muted=new Color(.67f,.65f,.61f),Panel=new Color(.105f,.11f,.13f,.97f),Card=new Color(.17f,.18f,.2f),Teal=new Color(.12f,.34f,.34f);
        public static readonly Color[] GuildColors={new Color(.93f,.7f,.22f),new Color(.5f,.66f,.92f),new Color(.82f,.35f,.51f),new Color(.18f,.68f,.59f)};
        public static Canvas Canvas(Transform parent,string name)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster));go.transform.SetParent(parent,false);
            var c=go.GetComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;c.sortingOrder=25;
            var s=go.GetComponent<UnityEngine.UI.CanvasScaler>();s.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.screenMatchMode=UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;s.referenceResolution=Screen.width<Screen.height?new Vector2(720,1100):new Vector2(1440,900);return c;
        }
        public static RectTransform Box(Transform parent,string name,float x,float y,float w,float h,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image));var r=(RectTransform)go.transform;r.SetParent(parent,false);
            r.anchorMin=new Vector2(x,y);r.anchorMax=new Vector2(x+w,y+h);r.offsetMin=r.offsetMax=Vector2.zero;
            var i=go.GetComponent<UnityEngine.UI.Image>();i.color=color;i.raycastTarget=false;return r;
        }
        public static TextMeshProUGUI Text(Transform parent,string text,float x,float y,float w,float h,float size=18,Color? color=null,TextAlignmentOptions align=TextAlignmentOptions.Left)
        {
            var go=new GameObject("Text",typeof(RectTransform),typeof(TextMeshProUGUI));var r=(RectTransform)go.transform;r.SetParent(parent,false);r.anchorMin=new Vector2(x,y);r.anchorMax=new Vector2(x+w,y+h);r.offsetMin=r.offsetMax=Vector2.zero;
            var t=go.GetComponent<TextMeshProUGUI>();t.font=TMP_Settings.defaultFontAsset;t.text=text;t.fontSize=size;t.color=color??Ink;t.alignment=align;
            t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.Normal;t.enableAutoSizing=true;t.fontSizeMin=Math.Min(size,13);t.fontSizeMax=size;return t;
        }
        public static UnityEngine.UI.Button Button(Transform parent,string text,float x,float y,float w,float h,Action click,bool selected=false)
        {
            var r=Box(parent,text,x,y,w,h,Card);r.offsetMin=new Vector2(3,3);r.offsetMax=new Vector2(-3,-3);
            var img=r.GetComponent<UnityEngine.UI.Image>();img.raycastTarget=true;var b=r.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=img;
            b.transition=UnityEngine.UI.Selectable.Transition.None;b.onClick.AddListener(()=>click());
            var visual=Box(r,"Face",0,0,1,1,selected?Teal:Card);var motion=r.gameObject.AddComponent<DungeonMotion>();motion.Chosen=selected;
            var skin=Skin("button");
            if(skin)
            {
                img.color=Color.clear;Slice(visual.GetComponent<UnityEngine.UI.Image>(),selected?Skin("button-selected")??skin:skin);
                motion.Configure(visual,new Color(.86f,.86f,.86f),Color.white);motion.UseSprites(skin,Skin("button-selected"),Skin("button-disabled"));
            }
            else{motion.Configure(visual,Card,Teal);Box(visual,"Rule",0,0,1,.025f,selected?Gold:new Color(.34f,.31f,.25f));}
            Text(visual,text,.06f,.08f,.88f,.84f,18,Ink,TextAlignmentOptions.Center);return b;
        }
        // PixelLab tab plaque (Iso/UI/tab); the chosen tab keeps full colour, the others are dimmed.
        public static UnityEngine.UI.Button Tab(Transform parent,string text,float x,float y,float w,float h,Action click,bool selected)
        {
            var b=Button(parent,text,x,y,w,h,click,selected);var tab=Skin("tab");if(!tab)return b;
            var face=(RectTransform)b.transform.Find("Face");var motion=b.GetComponent<DungeonMotion>();
            Slice(face.GetComponent<UnityEngine.UI.Image>(),tab);motion.UseSprites(tab,tab,tab);
            var dim=new Color(.6f,.56f,.52f);motion.Configure(face,dim,Color.white);face.GetComponent<UnityEngine.UI.Image>().color=selected?Color.white:dim;
            return b;
        }
        // PixelLab life bar: frame (Iso/UI/bar-frame) with the red fill cut horizontally by health.
        public static RectTransform LifeBar(Transform parent,float x,float y,float w,float h,int health,int max)
        {
            float amount=Mathf.Clamp01(health/(float)Mathf.Max(1,max));var frame=Skin("bar-frame");var fill=Skin("bar-fill");
            var r=Box(parent,"Life bar",x,y,w,h,frame?Color.white:new Color(.17f,.09f,.12f));
            if(frame){var i=r.GetComponent<UnityEngine.UI.Image>();i.sprite=frame;i.type=UnityEngine.UI.Image.Type.Simple;}
            var f=Box(r,"Fill",0,0,1,1,fill?Color.white:new Color(.78f,.16f,.18f)).GetComponent<UnityEngine.UI.Image>();
            if(fill){f.sprite=fill;f.type=UnityEngine.UI.Image.Type.Filled;f.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;f.fillAmount=amount;}
            else{var fr=f.rectTransform;fr.anchorMax=new Vector2(amount,1);}
            return r;
        }
        // PixelLab action icon (Iso/UI/Icons/<id>) on the left of a button; the label moves right. No art: unchanged.
        public static void Icon(UnityEngine.UI.Button button,string id)
        {
            var t=Resources.Load<Texture2D>("DungeonBDX/Iso/UI/Icons/"+id);if(!t||!button)return;
            var face=button.transform.Find("Face");var label=face.GetComponentInChildren<TextMeshProUGUI>();
            var icon=Box(face,"Icon "+id,.04f,.14f,.24f,.72f,Color.white).GetComponent<UnityEngine.UI.Image>();
            icon.sprite=Sprite.Create(t,new Rect(0,0,t.width,t.height),new Vector2(.5f,.5f),100);icon.preserveAspect=true;
            if(label){var lr=label.rectTransform;lr.anchorMin=new Vector2(.28f,lr.anchorMin.y);lr.anchorMax=new Vector2(.97f,lr.anchorMax.y);}
        }
        // Scoreboard marker: square PixelLab icon (height-driven) then the text; iconW is the caller's width estimate. No art: text only.
        public static TextMeshProUGUI Chip(Transform parent,string id,string text,float x,float y,float w,float h,float iconW,float size=14,Color? color=null)
        {
            var t=Resources.Load<Texture2D>("DungeonBDX/Iso/UI/Icons/"+id);
            if(!t)return Text(parent,text,x,y,w,h,size,color);
            var r=Box(parent,"Chip "+id,x,y+h*.1f,0,h*.8f,Color.white);r.pivot=new Vector2(0,.5f);
            var fit=r.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();fit.aspectMode=UnityEngine.UI.AspectRatioFitter.AspectMode.HeightControlsWidth;fit.aspectRatio=1;
            var icon=r.GetComponent<UnityEngine.UI.Image>();icon.sprite=Sprite.Create(t,new Rect(0,0,t.width,t.height),new Vector2(.5f,.5f),100);icon.preserveAspect=true;
            return Text(parent,text,x+iconW,y,Mathf.Max(.01f,w-iconW),h,size,color);
        }
        // Delivered UI art (lote Tripo): 9-slice frames from Resources/DungeonBDX/Iso/UI; null keeps the flat look.
        static readonly System.Collections.Generic.Dictionary<string,Sprite> skins=new System.Collections.Generic.Dictionary<string,Sprite>();
        public static Sprite Skin(string id)
        {
            if(skins.TryGetValue(id,out var cached))return cached;
            var t=Resources.Load<Texture2D>("DungeonBDX/Iso/UI/"+id);Sprite sprite=null;
            if(t){float edge=Mathf.Round(Mathf.Min(t.width,t.height)*.18f);
                sprite=Sprite.Create(t,new Rect(0,0,t.width,t.height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(edge,edge,edge,edge));}
            skins[id]=sprite;return sprite;
        }
        public static void Slice(UnityEngine.UI.Image image,Sprite sprite)
        {
            if(!image||!sprite)return;image.sprite=sprite;image.type=UnityEngine.UI.Image.Type.Sliced;image.pixelsPerUnitMultiplier=3.2f;image.color=Color.white;
        }
        public static RectTransform Panelled(Transform parent,string name,float x,float y,float w,float h,Color fallback)
        {
            var r=Box(parent,name,x,y,w,h,fallback);Slice(r.GetComponent<UnityEngine.UI.Image>(),Skin("panel"));return r;
        }
        public static void Portrait(Transform parent,string id,float x,float y,float w,float h)
        {
            var r=Box(parent,"Portrait "+id,x,y,w,h,Color.clear);var sprite=AqHeroImport.Portrait(id);var ring=Skin("portrait-ring");
            // With the delivered ring (PixelLab kit) the face sits inside it and the band hides the square corners.
            var face=ring?Box(r,"Face",.16f,.16f,.68f,.68f,Color.clear):r;
            if(sprite){var image=face.GetComponent<UnityEngine.UI.Image>();image.sprite=sprite;image.color=Color.white;image.preserveAspect=true;}
            else Text(face,AqContent.Hero(id).Name.Substring(0,1),0,0,1,1,44,Gold,TextAlignmentOptions.Center);
            if(ring){var band=Box(r,"Ring",0,0,1,1,Color.white).GetComponent<UnityEngine.UI.Image>();band.sprite=ring;band.preserveAspect=true;}
        }
        public static void Clear(Transform parent)
        {for(int i=parent.childCount-1;i>=0;i--){var c=parent.GetChild(i).gameObject;c.SetActive(false);if(Application.isPlaying)UnityEngine.Object.Destroy(c);else UnityEngine.Object.DestroyImmediate(c);}}
        public static string Root=>Path.Combine(Application.persistentDataPath,"DungeonBDX",Environment.GetCommandLineArgs().Contains("-dungeonbdx-qa")?"QA":"Campaigns");
    }
    public sealed class AqMatchMenu : MonoBehaviour
    {
        DungeonShell shell;Canvas canvas;RectTransform safe,body;string scenario=AqScenarios.Ids[0],tab="adventures",inspected="colosso",message="";
        readonly List<List<string>> parties=AqHeroImport.DefaultParties(4).Select(p=>p.ToList()).ToList();
        int guildCount=2,guild,page,lastW,lastH;bool autoMonsters;
        public Canvas HudCanvas=>canvas;
        public void Open(DungeonShell owner)
        {shell=owner;if(!canvas)canvas=AqUi.Canvas(transform,"Adventure selection");canvas.gameObject.SetActive(true);enabled=true;shell.HideForArcadia();Draw();}
        void Update(){if(lastW!=Screen.width||lastH!=Screen.height)Draw();}
        void Draw()
        {
            if(!canvas)return;lastW=Screen.width;lastH=Screen.height;bool portrait=Screen.width<Screen.height;
            canvas.GetComponent<UnityEngine.UI.CanvasScaler>().referenceResolution=portrait?new Vector2(720,1100):new Vector2(1440,900);
            AqUi.Clear(canvas.transform);AqUi.Box(canvas.transform,"Backdrop",0,0,1,1,new Color(.055f,.065f,.075f));
            safe=AqUi.Box(canvas.transform,"Safe area",0,0,1,1,Color.clear);DungeonShell.ApplySafeArea(safe,Screen.safeArea,Screen.width,Screen.height);
            AqUi.Text(safe,"DUNGEON<size=75%>BDX</size>",.035f,.92f,.7f,.065f,39,AqUi.Gold);
            AqUi.Text(safe,"GUILDAS  /  AVENTURAS  /  RIVAIS",.04f,.885f,.8f,.04f,14,AqUi.Muted);
            string[] tabs={"adventures","heroes","rules"},labels={"AVENTURAS","GUILDAS & HERÓIS","REGRAS"};
            for(int i=0;i<3;i++){string key=tabs[i];AqUi.Tab(safe,labels[i],.03f+i*.315f,.805f,.31f,.072f,()=>{tab=key;Draw();},tab==key);}
            body=AqUi.Box(safe,"Page",.03f,.17f,.94f,.62f,AqUi.Panel);
            if(tab=="adventures")Adventures(portrait);else if(tab=="heroes")Heroes(portrait);else Rules();
            var foot=AqUi.Box(safe,"Bottom controls",.03f,.025f,.94f,.13f,AqUi.Panel);
            AqUi.Text(foot,message==""?$"{guildCount} GUILDAS · 3 HERÓIS CADA · { (autoMonsters?"REAÇÕES AUTOMÁTICAS":"MONSTROS CONTROLADOS PELO RIVAL")}":message,.02f,.63f,.96f,.31f,15,AqUi.Muted);
            bool exists=new AqLocalSave(AqUi.Root,scenario).Exists;
            AqUi.Button(foot,exists?"RECOMEÇAR CENÁRIO":"INICIAR AVENTURA",.015f,.07f,.48f,.52f,()=>Launch(false));
            AqUi.Button(foot,"CONTINUAR PARTIDA SALVA",.505f,.07f,.48f,.52f,()=>Launch(true)).interactable=exists;
        }
        void Adventures(bool portrait)
        {
            var selected=AqScenarios.Create(scenario);
            for(int i=0;i<AqScenarios.Ids.Length;i++)
            {
                string id=AqScenarios.Ids[i];var definition=AqScenarios.Create(id);float top=.98f-i*.325f;
                var b=AqUi.Button(body,"",.01f,top-.31f,portrait?.42f:.4f,.305f,()=>{scenario=id;message="";Draw();},scenario==id);
                var f=b.transform;MiniMap(f,definition,.04f,.15f,.34f,.72f);
                AqUi.Text(f,definition.Name,.42f,.44f,.54f,.45f,portrait?16:23,AqUi.Gold);
                AqUi.Text(f,$"0{i+1}  ·  9 TILES\n81 espaços",.42f,.13f,.54f,.29f,15,AqUi.Muted);
            }
            var panel=AqUi.Box(body,"Adventure details",.44f,.025f,.54f,.95f,Color.clear);
            AqUi.Text(panel,selected.Subtitle.ToUpperInvariant(),.03f,.87f,.94f,.1f,16,AqUi.Gold);
            AqUi.Text(panel,selected.Story,.03f,.54f,.94f,.31f,portrait?18:24);
            AqUi.Text(panel,"MISSÕES DO CENÁRIO",.03f,.46f,.94f,.08f,17,AqUi.Gold);
            AqUi.Text(panel,"1. "+selected.Quests[0]+"\n2. "+selected.Quests[1]+"\n+ Derrotar um herói de cada guilda rival.",.03f,.22f,.94f,.23f,portrait?16:21);
            AqUi.Text(panel,"9 tiles de 3×3 · portas e portais\n2 personagens por espaço · movimento 3\nVitória por missões · salvamento contínuo",.03f,.025f,.94f,.18f,15,AqUi.Muted);
        }
        static void MiniMap(Transform parent,AqScenario s,float x,float y,float w,float h)
        {
            var r=AqUi.Box(parent,"Map preview",x,y,w,h,new Color(.22f,.19f,.14f));
            foreach(var p in s.Board.Spaces)
            {
                Color c=p.Room==""?new Color(.61f,.56f,.42f):p.Surface=="wood"?new Color(.36f,.26f,.17f):new Color(.46f,.39f,.3f);
                AqUi.Box(r,"Cell",p.Cell.X/9f+.003f,p.Cell.Y/9f+.003f,1/9f-.006f,1/9f-.006f,c);
            }
            foreach(var t in s.Tokens.Where(t=>t.Kind=="portal"))AqUi.Box(r,"Portal",(t.Cell.X+.3f)/9,(t.Cell.Y+.3f)/9,.045f,.045f,new Color(.23f,.75f,.75f));
            for(int i=0;i<s.Starts.Length;i++)foreach(var c in s.Starts[i])AqUi.Box(r,"Guild",c.X/9f,c.Y/9f,1/9f,1/9f,AqUi.GuildColors[i]);
        }
        void Heroes(bool portrait)
        {
            AqUi.Text(body,"ESCOLHA TRÊS HERÓIS PARA CADA GUILDA",.025f,.91f,.95f,.075f,21,AqUi.Gold);
            for(int i=0;i<guildCount;i++){int g=i;AqUi.Button(body,$"GUILDA {i+1}  ·  {parties[i].Count}/3",.02f+i*.96f/guildCount,.81f,.96f/guildCount,.09f,()=>{guild=g;Draw();},guild==g);}
            string[] ids=AqHeroImport.PlayableIds();int per=portrait?6:10,cols=portrait?3:5;int pages=(ids.Length+per-1)/per;page=Math.Min(page,pages-1);
            for(int i=0;i<per&&page*per+i<ids.Length;i++)
            {
                string id=ids[page*per+i];var hero=AqContent.Hero(id);bool chosen=parties[guild].Contains(id);
                var b=AqUi.Button(body,"",.02f+i%cols*.96f/cols,.42f-(i/cols)*.27f,.96f/cols,.265f,()=>
                {inspected=id;if(chosen)parties[guild].Remove(id);else if(parties.Take(guildCount).Any(p=>p.Contains(id)))message="Este herói já pertence a uma guilda.";else if(parties[guild].Count<3){parties[guild].Add(id);message="";}else message="Remova um herói antes de recrutar outro.";Draw();},chosen);
                AqUi.Portrait(b.transform,id,.15f,.22f,.7f,.74f);AqUi.Text(b.transform,hero.Name,.03f,.01f,.94f,.23f,15,chosen?AqUi.Gold:AqUi.Ink,TextAlignmentOptions.Center);
            }
            AqUi.Button(body,"<",.02f,.71f,.09f,.08f,()=>{page=(page+pages-1)%pages;Draw();});
            AqUi.Text(body,$"HERÓIS {page+1}/{pages} · clique para recrutar ou remover",.12f,.71f,.76f,.08f,14,AqUi.Muted,TextAlignmentOptions.Center);
            AqUi.Button(body,">",.89f,.71f,.09f,.08f,()=>{page=(page+1)%pages;Draw();});
            var h=AqContent.Hero(inspected);AqUi.Text(body,$"{h.Name} · VIDA {h.Health} · DEFESA {h.Defense}\n{h.AbilityText}",.03f,.01f,.94f,.13f,17,AqUi.Ink);
        }
        void Rules()
        {
            AqUi.Text(body,"CONFIGURAÇÃO DA PARTIDA",.04f,.84f,.92f,.12f,27,AqUi.Gold);
            for(int i=2;i<=4;i++){int count=i;AqUi.Button(body,i+" GUILDAS",.04f+(i-2)*.31f,.72f,.3f,.1f,()=>{guildCount=count;guild=Math.Min(guild,guildCount-1);Draw();},guildCount==count);}
            AqUi.Button(body,autoMonsters?"REAÇÕES: AUTOMÁTICAS":"REAÇÕES: JOGADOR À DIREITA",.04f,.59f,.92f,.1f,()=>{autoMonsters=!autoMonsters;Draw();});
            AqUi.Text(body,"Em cada turno, ative UM herói ou descanse a guilda.\nMovimento: 3 pontos; ataque antes ou depois, sem interromper o movimento.\nAtaques exaustam cartas; descanso restaura cartas, permite transferir itens e ressuscitar.\nInventário: 4 cartas por herói. Duas miniaturas por espaço; Colosso ocupa o espaço inteiro.\nMonstros reagem por Alerta e Vingança. Extermínio impede a última reação.\nVitória: 2 missões com 2 guildas; 3 com 3–4 guildas, incluindo ao menos uma PvE.",.04f,.17f,.92f,.4f,19);
            AqUi.Text(body,"BALANCEAMENTO CONFIRMADO: até 6 dados, crítico linear (+1 por crítico, bônus máx. 3), mitigação máx. 60%, dano 30/ação e 60/turno. Heróis e aventuras DungeonBDX adaptados às referências AQ.",.04f,.025f,.92f,.13f,15,AqUi.Muted);
        }
        void Launch(bool resume)
        {
            if(!resume&&parties.Take(guildCount).Any(p=>p.Count!=3)){tab="heroes";message="Complete as guildas com três heróis cada.";Draw();return;}
            var match=gameObject.GetComponent<AqMatch>()??gameObject.AddComponent<AqMatch>();
            if(!match.Open(this,scenario,parties.Take(guildCount).Select(p=>p.ToArray()).ToArray(),autoMonsters,resume)){message=match.Message;Draw();return;}
            canvas.gameObject.SetActive(false);enabled=false;
        }
        public void Return(){canvas.gameObject.SetActive(true);enabled=true;Draw();}
    }
}
