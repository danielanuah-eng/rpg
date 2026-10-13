using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArenaTatica.Core;
using UnityEngine;
using UnityEngine.UI;
using Terrain=ArenaTatica.Core.Terrain;

namespace ArenaTatica
{
    // Mapas feitos no editor, salvos neste aparelho (persistentDataPath/arena-mapas.json). Na sala, o mapa do Host vai no estado.
    public static class ArenaMaps
    {
        [Serializable] class Library { public List<MapDef> maps=new List<MapDef>(); }
        static string FilePath => Path.Combine(Application.persistentDataPath,"arena-mapas.json");
        static Library lib;
        static Library Lib
        {
            get
            {
                if(lib!=null) return lib;
                try { lib=File.Exists(FilePath)?JsonUtility.FromJson<Library>(File.ReadAllText(FilePath)):null; } catch(Exception) { lib=null; }
                return lib??=new Library();
            }
        }
        public static List<MapDef> All => Lib.maps;
        // Mapas do Codex (Resources/Mapas/codex.json, gerado por Tools/mapas-codex.mjs). Os que não passam em MapGen.Problem ficam de fora.
        public const string Generated="@gerado";
        static List<MapDef> builtin;
        public static List<MapDef> Builtin
        {
            get
            {
                if(builtin!=null) return builtin;
                var asset=Resources.Load<TextAsset>("Mapas/codex");
                builtin=asset?JsonUtility.FromJson<Library>(asset.text).maps:new List<MapDef>();
                builtin.RemoveAll(m=>{ var p=MapGen.Problem(m); if(p!=null) Debug.LogWarning($"Mapa do Codex fora da lista ({m.name}): {p}"); return p!=null; });
                return builtin;
            }
        }
        public static bool IsBuiltin(MapDef m) => Builtin.Contains(m);
        // O Play do editor não recarrega o domínio: limpa os caches lidos antes.
        public static void Reset() { lib=null; builtin=null; }
        // Mapa sorteado: um dos mapas do Codex do tamanho da partida, escolhido pela seed (null = gerador do MapGen).
        public static MapDef Pick(int size,uint seed) { var list=Builtin.FindAll(m=>m.width==size); return list.Count==0?null:list[(int)(seed%(uint)list.Count)]; }
        public static MapDef Find(string name) => Lib.maps.Find(m=>m.name==name)??Builtin.Find(m=>m.name==name);
        public static MapDef Copy(MapDef m) => JsonUtility.FromJson<MapDef>(JsonUtility.ToJson(m));
        public static void Save(MapDef map) { int i=Lib.maps.FindIndex(m=>m.name==map.name); var copy=Copy(map); if(i>=0) Lib.maps[i]=copy; else Lib.maps.Add(copy); Write(); }
        public static void Delete(string name) { Lib.maps.RemoveAll(m=>m.name==name); Write(); }
        static void Write() { try { File.WriteAllText(FilePath,JsonUtility.ToJson(Lib)); } catch(Exception e) { Debug.LogWarning("Mapas: "+e.Message); } }
    }

    // Editor de mapas dentro do jogo: relevo, peças (árvore, arbusto, caixa), zona central e mover peças, com espelho opcional
    // (o mapa sorteado é simétrico pelo centro; desligar o espelho permite mapas assimétricos). As casas de início ficam livres.
    public sealed class ArenaMapEditor : MonoBehaviour
    {
        public ArenaGame Game;
        public bool Active { get; private set; }
        public Cell? Picked { get; private set; }
        public List<Cell> SpawnCells { get; } = new List<Cell>();
        MapDef map; MatchState board; string tool="subir"; bool mirror=true;
        Canvas canvas; RectTransform panel; Text title, info; InputField nameField; Button mirrorButton, themeButton;
        readonly Dictionary<string,Image> toolImages=new Dictionary<string,Image>();
        Font font;
        static readonly Color Bg=new Color(.065f,.085f,.13f,1), Idle=new Color(.14f,.19f,.27f), Chosen=new Color(.45f,.29f,.16f), Ink=new Color(.91f,.93f,.98f), Muted=new Color(.65f,.7f,.8f);
        static readonly (string id,string label)[] Tools={("subir","Subir relevo"),("descer","Descer relevo"),("arvore","Árvore"),("arbusto","Arbusto"),
            ("caixa","Caixa"),("modelo","Trocar modelo da peça"),("enfeite","Enfeite"),("limpar","Limpar casa"),("zona","Zona central"),("mover","Mover peça")};
        // Modelos do kit para cada terreno (o primeiro é o padrão) e enfeites só visuais (Models/Mapa/<cenário> ou comum/).
        static readonly Dictionary<Terrain,string[]> Skins=new Dictionary<Terrain,string[]>{
            [Terrain.Arvore]=new[]{"arvore","arvore_jovem","arvore_palmeira"}, [Terrain.Arbusto]=new[]{"arbusto"}, [Terrain.Caixa]=new[]{"caixa","caixa_rachada","pedra"} };
        public static readonly string[] Ornaments={"enfeite_capim","enfeite_flores","enfeite_seixos","enfeite_pedra_baixa","enfeite_cogumelos","enfeite_raiz_chao",
            "enfeite_cristal","enfeite_fogueira","enfeite_tocha","enfeite_placa","enfeite_cerca","enfeite_lago","relicario_solar","barricada_partida"};
        int ornament;
        Button ornamentButton;

        public void Open(MapDef def)
        {
            map=ArenaMaps.Copy(def); if(string.IsNullOrEmpty(map.name)) map.name="Meu mapa"; else if(ArenaMaps.IsBuiltin(def)) map.name+=" (meu)";
            map.decor??=new List<Decor>();
            board=new MatchState{ width=map.width, height=map.width, players=MapGen.PlayersFor(map.width), theme=Math.Max(0,map.theme), tiles=map.tiles, decor=map.decor };
            SpawnCells.Clear(); for(int slot=0;slot<board.players;slot++) SpawnCells.AddRange(MapGen.Spawns(board,slot));
            foreach(var c in SpawnCells) board.At(c).terrain=Terrain.Livre;
            MapGen.Center(board);
            if(!canvas) Build();
            Active=true; Picked=null; panel.gameObject.SetActive(true); nameField.text=map.name;
            Game.Menu?.Close(); Game.BeginMapEdit(board); Refresh("Toque nas casas para aplicar a ferramenta.");
        }
        public void Close(bool newMatch=true)
        {
            if(!Active) return;
            Active=false; Picked=null; if(panel) panel.gameObject.SetActive(false);
            if(newMatch) Game.NewLocal();
        }

        public void SetTool(string id) { if(Tools.Any(t=>t.id==id)) { tool=id; Picked=null; if(board!=null) { board.revision++; Refresh(null); } } }
        public void Tap(Cell c)
        {
            if(!Active||!Board.Contains(board,c)) return;
            var t=board.At(c); bool spawn=SpawnCells.Contains(c), shop=t.terrain==Terrain.Loja;
            if(shop&&tool!="subir"&&tool!="descer") { Refresh("A loja fica sempre nas 4 casas do meio; dá só para subir ou descer o chão dela."); return; }
            switch(tool)
            {
                case "subir": Edit(c,x=>x.height=Math.Min(Board.MaxHeight,x.height+1)); break;
                case "descer": Edit(c,x=>x.height=Math.Max(0,x.height-1)); break;
                case "arvore": case "arbusto": case "caixa":
                    if(spawn) { Refresh("Casa de início: fica sempre livre."); return; }
                    var piece=tool=="arvore"?Terrain.Arvore:tool=="arbusto"?Terrain.Arbusto:Terrain.Caixa;
                    Edit(c,x=>{ x.terrain=piece; x.zone=false; },true);
                    break;
                case "modelo":
                    if(!Skins.TryGetValue(t.terrain,out var skins)) { Refresh("Toque numa árvore, arbusto ou caixa para trocar o modelo."); return; }
                    var now=DecorAt(c).Find(d=>MapGen.SkinTerrain(d.piece)==t.terrain);
                    string next=skins[(Array.IndexOf(skins,now?.piece??skins[0])+1)%skins.Length];
                    EditDecor(c,list=>{ list.RemoveAll(d=>MapGen.SkinTerrain(d.piece)!=Terrain.Livre); list.Add(new Decor{ piece=next }); });
                    Refresh($"Modelo: {next.Replace('_',' ')}.");
                    return;
                case "enfeite":
                    if(t.terrain!=Terrain.Livre) { Refresh("O enfeite vai em casa livre (árvore e caixa já têm modelo)."); return; }
                    bool has=DecorAt(c).Exists(d=>MapGen.SkinTerrain(d.piece)==Terrain.Livre); string orn=Ornaments[ornament];
                    EditDecor(c,list=>{ if(has) list.RemoveAll(d=>MapGen.SkinTerrain(d.piece)==Terrain.Livre); else list.Add(new Decor{ piece=orn }); });
                    Refresh(has?"Enfeite tirado.":$"Enfeite: {OrnamentName(orn)} (só visual, não bloqueia).");
                    return;
                case "limpar": Edit(c,x=>{ x.terrain=Terrain.Livre; x.height=0; x.zone=false; },true); EditDecor(c,list=>list.Clear()); break;
                case "zona":
                    if(t.terrain!=Terrain.Livre) { Refresh("A zona central só vai em casa livre."); return; }
                    bool on=!t.zone; Edit(c,x=>x.zone=on);
                    break;
                case "mover":
                    if(Picked==null)
                    {
                        if(t.terrain==Terrain.Livre) { Refresh("Toque numa árvore, arbusto ou caixa para mover."); return; }
                        Picked=c; Refresh("Agora toque na casa de destino."); return;
                    }
                    var from=Picked.Value; Picked=null;
                    if(c==from) { Refresh("Movimento cancelado."); return; }
                    if(t.terrain!=Terrain.Livre||spawn||t.zone) { Refresh("O destino precisa ser uma casa livre, fora do início e da zona."); return; }
                    var kind=board.At(from).terrain; var skin=DecorAt(from).Find(d=>MapGen.SkinTerrain(d.piece)==kind);
                    Edit(from,x=>x.terrain=Terrain.Livre,true);
                    // Com espelho, a peça espelhada vai para o espelho do destino (e leva o modelo junto).
                    Edit(c,x=>x.terrain=kind);
                    if(skin!=null) EditDecor(c,list=>list.Add(new Decor{ piece=skin.piece, rot=skin.rot }));
                    break;
            }
        }
        static string OrnamentName(string piece) => piece.Replace("enfeite_","").Replace('_',' ');
        // Enfeites e modelos cujo centro cai na casa c.
        List<Decor> DecorAt(Cell c) => map.decor.FindAll(d=>(int)Math.Round(d.x)==c.x&&(int)Math.Round(d.z)==c.z);
        // Muda a lista de enfeites da casa (e, com espelho, da casa oposta, girada 180°). Peças novas nascem no centro da casa.
        void EditDecor(Cell c,Action<List<Decor>> change)
        {
            void One(Cell at,bool mirrored)
            {
                var list=DecorAt(at); foreach(var d in list) map.decor.Remove(d);
                var before=new HashSet<Decor>(list); change(list);
                foreach(var d in list) { if(!before.Contains(d)) { d.x=at.x; d.z=at.z; if(mirrored) d.rot+=180; } map.decor.Add(d); }
            }
            One(c,false); var m=board.Mirror(c); if(mirror&&m!=c) One(m,true);
            board.revision++; Game.View.Sync(board,true);
        }
        void Edit(Cell c,Action<Tile> set,bool dropSkin=false)
        {
            set(board.At(c));
            var m=board.Mirror(c);
            if(mirror&&m!=c) set(board.At(m));
            foreach(var s in SpawnCells) board.At(s).terrain=Terrain.Livre;
            // Peça nova ou tirada: o modelo trocado da casa sai junto (volta o padrão do terreno).
            if(dropSkin) foreach(var at in mirror&&m!=c?new[]{c,m}:new[]{c}) map.decor.RemoveAll(d=>(int)Math.Round(d.x)==at.x&&(int)Math.Round(d.z)==at.z&&MapGen.SkinTerrain(d.piece)!=Terrain.Livre);
            board.revision++; Game.View.Sync(board,true); Refresh(null);
        }

        void Refresh(string message)
        {
            foreach(var kv in toolImages) kv.Value.color=kv.Key==tool?Chosen:Idle;
            SetLabel(mirrorButton,mirror?"Espelho: ligado":"Espelho: desligado");
            SetLabel(ornamentButton,"Qual enfeite: "+OrnamentName(Ornaments[ornament])+" ›");
            SetLabel(themeButton,"Cenário: "+(map.theme<0?"sorteado":MapGen.Themes[map.theme]));
            title.text=$"EDITOR DE MAPAS · {map.width}×{map.width} ({MapGen.PlayersFor(map.width)} jog.)";
            string problem=MapGen.Problem(map);
            info.text=(message??"")+(problem!=null?$"\n<color=#ef6f7a>{problem}</color>":"\n<color=#7fd1a0>Mapa jogável.</color>")+"\n<size=11>Azul: casas de início (sempre livres).</size>";
            Game.Notify(message??"Editor de mapas.");
        }

        public void Save(bool use)
        {
            string name=(nameField.text??"").Trim(); if(name=="") { Refresh("Dê um nome ao mapa."); return; }
            string problem=MapGen.Problem(map); if(problem!=null) { Refresh("Não salvei: "+problem); return; }
            map.name=name; ArenaMaps.Save(map);
            if(use) { Game.MapName=name; Game.SetSeats(MapGen.PlayersFor(map.width)); Close(); Game.Notify($"Partida no mapa {name}."); return; }
            Refresh($"Mapa \"{name}\" salvo. Use na aba Mapas do menu.");
        }

        // ── Interface ──
        void Build()
        {
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var go=new GameObject("EditorDeMapas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)); go.transform.SetParent(transform);
            canvas=go.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=50;
            go.GetComponent<CanvasScaler>().uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;
            panel=new GameObject("Painel",typeof(RectTransform),typeof(Image)).GetComponent<RectTransform>(); panel.SetParent(go.transform,false);
            panel.GetComponent<Image>().color=Bg; panel.anchorMin=new Vector2(0,0); panel.anchorMax=new Vector2(0,1); panel.offsetMin=new Vector2(0,62); panel.offsetMax=new Vector2(290,-82);
            var v=panel.gameObject.AddComponent<VerticalLayoutGroup>(); v.padding=new RectOffset(14,14,10,10); v.spacing=3; v.childControlWidth=v.childControlHeight=true; v.childForceExpandHeight=false;
            title=Label("",15,new Color(.88f,.74f,.42f),22);
            nameField=Field();
            foreach(var t in Tools) { var id=t.id; var b=Btn(t.label,()=>{ tool=id; Picked=null; board.revision++; Refresh(null); }); toolImages[id]=b.GetComponent<Image>(); }
            ornamentButton=Btn("",()=>{ ornament=(ornament+1)%Ornaments.Length; tool="enfeite"; Refresh($"Enfeite escolhido: {OrnamentName(Ornaments[ornament])}. Toque numa casa livre."); });
            mirrorButton=Btn("",()=>{ mirror=!mirror; Refresh(mirror?"Cada mudança vale também na casa espelhada (mapa justo).":"Espelho desligado: mudanças só na casa tocada."); });
            themeButton=Btn("",()=>{ map.theme=map.theme+1>=MapGen.Themes.Length?-1:map.theme+1; board.theme=Math.Max(0,map.theme); Game.View.Sync(board,true); Refresh(null); });
            Btn("Salvar",()=>Save(false)).GetComponent<Image>().color=new Color(.2f,.36f,.27f);
            Btn("Salvar e jogar neste mapa",()=>Save(true)).GetComponent<Image>().color=new Color(.2f,.36f,.27f);
            Btn("Sair sem salvar",()=>Close());
            info=Label("",12,Muted,74);
            panel.gameObject.SetActive(false);
        }
        Text Label(string text,int size,Color color,float height)
        {
            var t=new GameObject("Texto",typeof(RectTransform),typeof(Text),typeof(LayoutElement)).GetComponent<Text>(); t.transform.SetParent(panel,false);
            t.font=font; t.fontSize=size; t.color=color; t.text=text; t.supportRichText=true; t.horizontalOverflow=HorizontalWrapMode.Wrap;
            t.GetComponent<LayoutElement>().preferredHeight=height; return t;
        }
        Button Btn(string label,Action click)
        {
            var go=new GameObject(label,typeof(RectTransform),typeof(Image),typeof(Button),typeof(LayoutElement)); go.transform.SetParent(panel,false);
            go.GetComponent<Image>().color=Idle; go.GetComponent<LayoutElement>().preferredHeight=26;
            var b=ArenaTema.Lerp(go.GetComponent<Button>()); b.onClick.AddListener(()=>click());
            var t=new GameObject("Texto",typeof(RectTransform),typeof(Text)).GetComponent<Text>(); t.transform.SetParent(go.transform,false);
            var r=t.rectTransform; r.anchorMin=Vector2.zero; r.anchorMax=Vector2.one; r.offsetMin=r.offsetMax=Vector2.zero;
            t.font=font; t.fontSize=13; t.color=Ink; t.alignment=TextAnchor.MiddleCenter; t.text=label;
            return b;
        }
        static void SetLabel(Button b,string text) { if(b) b.GetComponentInChildren<Text>().text=text; }
        InputField Field()
        {
            var go=new GameObject("Nome",typeof(RectTransform),typeof(Image),typeof(InputField),typeof(LayoutElement)); go.transform.SetParent(panel,false);
            go.GetComponent<Image>().color=new Color(.03f,.04f,.07f); go.GetComponent<LayoutElement>().preferredHeight=30;
            var input=go.GetComponent<InputField>();
            Text Inner(string name,Color color) { var t=new GameObject(name,typeof(RectTransform),typeof(Text)).GetComponent<Text>(); t.transform.SetParent(go.transform,false); var r=t.rectTransform; r.anchorMin=Vector2.zero; r.anchorMax=Vector2.one; r.offsetMin=new Vector2(8,2); r.offsetMax=new Vector2(-8,-2); t.font=font; t.fontSize=13; t.color=color; t.alignment=TextAnchor.MiddleLeft; t.supportRichText=false; return t; }
            input.textComponent=Inner("Texto",Ink); var hint=Inner("Dica",Muted); hint.text="Nome do mapa"; input.placeholder=hint; input.characterLimit=32;
            return input;
        }
    }
}
