using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using ArenaTatica;

// Importa uma entrega visual (assets.visual.json): copia os PNGs para Resources e aplica o slicing/pivô/FPS do manifesto.
// Reaproveitável nas próximas entregas: ajuste a pasta em Pasta, ou chame Importar(manifestoJson, pastaDaEntrega).
// Os .meta existentes são preservados (só o PNG é sobrescrito), então GUIDs e referências não mudam.
public static class ArenaEfeitosImport
{
    const string Pasta = "coordenacao/arena-pixelai/entregas/polimento-ui-efeitos-2026-10-11";
    const string ResDir = "Assets/ArenaTatica/Resources/";
    const string ManifestoCopia = "Assets/ArenaTatica/Resources/Efeitos/manifesto.json";

    [System.Serializable] class Raiz { public EntradaEntrega[] entries; }
    [System.Serializable] class EntradaEntrega { public string resource, file; public int frameSize, frames; public float fps; public float[] unityPivot; public float pixelsPerUnit; }

    // Caminhos (Assets/…png) governados pelo manifesto: ArenaBuild.EnsureSprites não deve reimportá-los com outro pivô/filtro.
    static HashSet<string> managed;
    public static bool IsManaged(string assetPath)
    {
        if(managed==null) {
            managed=new HashSet<string>();
            if(File.Exists(ManifestoCopia)) foreach(var e in JsonUtility.FromJson<ArenaManifestoEditor>(File.ReadAllText(ManifestoCopia)).entries) managed.Add(ResDir+e.resource+".png");
        }
        return managed.Contains(assetPath);
    }
    [System.Serializable] class ArenaManifestoEditor { public EfeitoEntrada[] entries; }

    // Depois de copiar os PNGs, a primeira recompilação configura sozinha o que ainda estiver com a importação padrão.
    [InitializeOnLoadMethod]
    static void AutoImportar()
    {
        EditorApplication.delayCall+=()=>{
            if(Application.isPlaying) return;
            MenuArt();
            if(!File.Exists(ManifestoCopia)) return;
            var primeiro=JsonUtility.FromJson<ArenaManifestoEditor>(File.ReadAllText(ManifestoCopia)).entries.FirstOrDefault(); if(primeiro==null) return;
            var imp=AssetImporter.GetAtPath(ResDir+primeiro.resource+".png") as TextureImporter;
            if(imp!=null&&(imp.textureType!=TextureImporterType.Sprite||imp.filterMode!=FilterMode.Point)) Importar();
        };
    }

    // Arte do menu (Sprites/Menu/*.png): imagem única, sem recorte em tira.
    static void MenuArt()
    {
        if(!Directory.Exists(ResDir+"Sprites/Menu")) return;
        foreach(string f in Directory.GetFiles(ResDir+"Sprites/Menu","*.png")) {
            string path=f.Replace("\\","/"); var imp=AssetImporter.GetAtPath(path) as TextureImporter;
            if(imp==null||(imp.textureType==TextureImporterType.Sprite&&imp.spriteImportMode==SpriteImportMode.Single)) continue;
            imp.textureType=TextureImporterType.Sprite; imp.spriteImportMode=SpriteImportMode.Single; imp.mipmapEnabled=false; imp.alphaIsTransparency=true; imp.textureCompression=TextureImporterCompression.Uncompressed; imp.spritePixelsPerUnit=100; imp.SaveAndReimport();
        }
    }

    [MenuItem("Arena Tática/Importar entrega visual (efeitos, objetos, reparos)")]
    public static void Importar()
    {
        string raiz=Path.GetFullPath(Path.Combine(Application.dataPath,"..","..")), entrega=Path.Combine(raiz,Pasta);
        Importar(Path.Combine(entrega,"assets.visual.json"),entrega);
    }

    public static void Importar(string manifestoJson,string pastaEntrega)
    {
        var data=JsonUtility.FromJson<Raiz>(File.ReadAllText(manifestoJson));
        var entradas=data.entries.Where(e=>e.resource.StartsWith("Sprites/")).ToArray();
        var factory=new SpriteDataProviderFactories(); factory.Init();
        int copiados=0,configurados=0,erros=0;
        AssetDatabase.StartAssetEditing();
        try {
            foreach(var e in entradas) {
                string destino=ResDir+e.resource+".png", origem=Path.Combine(pastaEntrega,e.file);
                if(!File.Exists(origem)) { Debug.LogError("Entrega: arquivo ausente "+origem); erros++; continue; }
                Directory.CreateDirectory(Path.GetDirectoryName(destino));
                if(!File.Exists(destino)||!File.ReadAllBytes(destino).SequenceEqual(File.ReadAllBytes(origem))) { File.Copy(origem,destino,true); copiados++; }
            }
        } finally { AssetDatabase.StopAssetEditing(); }
        AssetDatabase.Refresh();
        foreach(var e in entradas) {
            string destino=ResDir+e.resource+".png";
            if(!File.Exists(destino)) continue;
            if(Configurar(destino,e,factory)) configurados++; else erros++;
        }
        // Cópia enxuta do manifesto lida em tempo de execução (flags ownerOnly, FPS, quadros).
        File.WriteAllText(ManifestoCopia,JsonUtility.ToJson(new ArenaManifestoEditor{ entries=FromManifest(entradas,File.ReadAllText(manifestoJson)) }));
        managed=null;
        AssetDatabase.Refresh();
        Debug.Log($"Entrega importada: {entradas.Length} tiras, {copiados} copiadas, {configurados} configuradas, {erros} erros.");
    }

    // Mantém as flags do manifesto original (ownerOnly etc.) lendo o JSON completo.
    static EfeitoEntrada[] FromManifest(EntradaEntrega[] entradas,string json)
    {
        var full=JsonUtility.FromJson<ArenaManifestoEditor>(json).entries.ToDictionary(x=>x.resource,x=>x);
        return entradas.Where(e=>full.ContainsKey(e.resource)).Select(e=>full[e.resource]).ToArray();
    }

    static bool Configurar(string path,EntradaEntrega e,SpriteDataProviderFactories factory)
    {
        var importer=(TextureImporter)AssetImporter.GetAtPath(path); if(importer==null) { Debug.LogError("Sem importador: "+path); return false; }
        var pixels=new Texture2D(2,2); pixels.LoadImage(File.ReadAllBytes(path)); int w=pixels.width,h=pixels.height; Object.DestroyImmediate(pixels);
        if(h!=e.frameSize||w!=e.frameSize*e.frames) { Debug.LogError($"{path}: {w}x{h} não bate com {e.frames} quadros de {e.frameSize} px"); return false; }
        var pivot=new Vector2(e.unityPivot[0],e.unityPivot[1]); float ppu=e.pixelsPerUnit>0?e.pixelsPerUnit:128;
        var mode=e.frames>1?SpriteImportMode.Multiple:SpriteImportMode.Single;
        string name=Path.GetFileNameWithoutExtension(path);
        importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=mode; importer.filterMode=FilterMode.Point;
        importer.mipmapEnabled=false; importer.alphaIsTransparency=true; importer.textureCompression=TextureImporterCompression.Uncompressed; importer.spritePixelsPerUnit=ppu;
        var settings=new TextureImporterSettings(); importer.ReadTextureSettings(settings);
        settings.spriteAlignment=(int)SpriteAlignment.Custom; settings.spriteMeshType=SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
        importer.spritePivot=pivot;
        if(mode==SpriteImportMode.Multiple) {
            var provider=factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var antigos=provider.GetSpriteRects().ToDictionary(r=>r.name,r=>r.spriteID);
            provider.SetSpriteRects(Enumerable.Range(0,e.frames).Select(i=>{
                string n=name+"_"+i;
                return new SpriteRect{ name=n, rect=new Rect(i*e.frameSize,0,e.frameSize,e.frameSize), alignment=SpriteAlignment.Custom, pivot=pivot, spriteID=antigos.TryGetValue(n,out var id)?id:GUID.Generate() };
            }).ToArray());
            provider.Apply();
        }
        importer.SaveAndReimport();
        return true;
    }
}
