using System;
using System.IO;
using System.Linq;
using ArenaTatica;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class ArenaBuild
{
    [MenuItem("Arena Tática/Preparar cena")]
    public static void Prepare()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        EnsureMaterials();
        foreach(string model in new[]{"CiantiaChibi3D","DragaoDourado3D","DragaoTransplanar3D"}) {
            string path="Assets/ArenaTatica/Resources/Models/"+model+".glb";
            if(!AssetDatabase.LoadAssetAtPath<GameObject>(path)) throw new Exception("GLB não importado: "+path);
        }
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        new GameObject("ArenaTatica",typeof(ArenaGame));
        Directory.CreateDirectory("Assets/ArenaTatica/Scenes");
        EditorSceneManager.SaveScene(scene,"Assets/ArenaTatica/Scenes/Arena.unity");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/ArenaTatica/Scenes/Arena.unity",true)};
        PlayerSettings.companyName="Arcanoclisma"; PlayerSettings.productName="Arena Tática"; PlayerSettings.bundleVersion="1.0.0";
        PlayerSettings.defaultScreenWidth=1280; PlayerSettings.defaultScreenHeight=720; PlayerSettings.fullScreenMode=FullScreenMode.Windowed; PlayerSettings.resizableWindow=true; PlayerSettings.runInBackground=true;
        PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android,"org.arcanoclisma.arenatatica");
        PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.template="APPLICATION:Default";
        AssetDatabase.SaveAssets(); Debug.Log("ARENA_PREPARE_OK");
    }
    static void EnsureMaterials()
    {
        EnsureMaterial("Assets/ArenaTatica/Resources/Materials/ArenaLit.mat","Universal Render Pipeline/Lit");
        EnsureMaterial("Assets/ArenaTatica/Resources/Materials/ArenaSprite.mat","Universal Render Pipeline/2D/Sprite-Unlit-Default");
        EnsureSprites(); EnsureTiles();
    }
    static void EnsureMaterial(string path,string shaderName)
    {
        if(AssetDatabase.LoadAssetAtPath<Material>(path)) return;
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        var shader=Shader.Find(shaderName);
        if(!shader) throw new Exception("Shader indisponível: "+shaderName);
        AssetDatabase.CreateAsset(new Material(shader),path);
        AssetDatabase.SaveAssets();
    }
    // Sprites do PixelLab: sem compressão, pivô no pé (pixel opaco mais baixo). Na mesa aparecem bem menores que
    // os 168 px do quadro; filtro Point descartava pixels ao reduzir, então trilinear com mipmaps (Kaiser) mantém o detalhe.
    // <direção>.png é um quadro parado; <clipe>_<direção>.png é uma tira de quadros quadrados (sprites <clipe>_<direção>_<i>).
    static void EnsureSprites()
    {
        if(!Directory.Exists("Assets/ArenaTatica/Resources/Sprites")) return;
        var factory=new SpriteDataProviderFactories(); factory.Init();
        foreach(string file in Directory.GetFiles("Assets/ArenaTatica/Resources/Sprites","*.png",SearchOption.AllDirectories)) {
            string path=file.Replace('\\','/');
            var importer=(TextureImporter)AssetImporter.GetAtPath(path); if(importer==null) continue;
            var pixels=new Texture2D(2,2); pixels.LoadImage(File.ReadAllBytes(path));
            // Projeteis/<skill>.png é uma imagem única (pode ser larga); o resto, tira de quadros quadrados.
            bool single=path.Contains("/Projeteis/");
            int w=pixels.width,h=pixels.height,frames=w>h&&!single?w/h:1,size=frames>1?h:w;
            int foot=h; var data=pixels.GetPixels32();
            for(int i=0;i<data.Length;i++) if(data[i].a>40) { foot=i/w; break; }
            if(foot==h) foot=0;
            UnityEngine.Object.DestroyImmediate(pixels);
            // Efeitos (projéteis, raios, moedas) giram em torno do centro; lutadores e ícones apoiam no pé.
            // Dados da HUD (tarefa 013): pixel duro, sem mipmaps, centro; aparecem perto do tamanho original.
            bool hud=path.Contains("/Dados/"); var filter=hud?FilterMode.Point:FilterMode.Trilinear;
            var pivot=path.Contains("/Efeitos/")||single||hud?new Vector2(.5f,.5f):new Vector2(.5f,foot/(float)h);
            string name=Path.GetFileNameWithoutExtension(path);
            var mode=frames>1?SpriteImportMode.Multiple:SpriteImportMode.Single;
            var settings=new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            bool same=importer.textureType==TextureImporterType.Sprite&&importer.spriteImportMode==mode&&importer.filterMode==filter&&importer.mipmapEnabled==!hud&&(hud||importer.mipmapFilter==TextureImporterMipFilter.KaiserFilter)&&importer.alphaIsTransparency&&importer.textureCompression==TextureImporterCompression.Uncompressed&&importer.spritePixelsPerUnit==128&&settings.spriteMeshType==SpriteMeshType.Tight;
            if(same&&mode==SpriteImportMode.Single) same=settings.spriteAlignment==(int)SpriteAlignment.Custom&&importer.spritePivot==pivot;
            var provider=factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            if(same&&mode==SpriteImportMode.Multiple) { var old=provider.GetSpriteRects(); same=old.Length==frames&&old.All(r=>r.pivot==pivot&&r.rect.height==size)&&old.Select(r=>r.name).SequenceEqual(Enumerable.Range(0,frames).Select(i=>name+"_"+i)); }
            if(same) continue;
            importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=mode;
            importer.filterMode=filter; importer.mipmapEnabled=!hud; importer.mipmapFilter=TextureImporterMipFilter.KaiserFilter; importer.alphaIsTransparency=true; importer.textureCompression=TextureImporterCompression.Uncompressed; importer.spritePixelsPerUnit=128;
            importer.ReadTextureSettings(settings); settings.spriteAlignment=(int)SpriteAlignment.Custom; settings.spriteMeshType=SpriteMeshType.Tight; importer.SetTextureSettings(settings);
            importer.spritePivot=pivot;
            if(mode==SpriteImportMode.Multiple) {
                provider=factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
                provider.SetSpriteRects(Enumerable.Range(0,frames).Select(i=>new SpriteRect{ name=name+"_"+i, rect=new Rect(i*size,0,size,size), alignment=SpriteAlignment.Custom, pivot=pivot, spriteID=GUID.Generate() }).ToArray());
                provider.Apply();
            }
            importer.SaveAndReimport();
        }
    }
    // Pisos em pixel art do Codex (Textures/tile_<cenário>): ponto, sem compressão, repetidos.
    static void EnsureTiles()
    {
        foreach(string file in Directory.GetFiles("Assets/ArenaTatica/Resources/Textures","tile_*.png")) {
            string path=file.Replace('\\','/'); if(path.EndsWith("tile_pedra.png")) continue;
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            if(importer==null||importer.filterMode==FilterMode.Point&&importer.textureCompression==TextureImporterCompression.Uncompressed&&importer.wrapMode==TextureWrapMode.Repeat) continue;
            importer.filterMode=FilterMode.Point; importer.textureCompression=TextureImporterCompression.Uncompressed; importer.wrapMode=TextureWrapMode.Repeat; importer.SaveAndReimport();
        }
    }
    [MenuItem("Arena Tática/Exportar Windows")]
    public static void Windows() { Build(BuildTarget.StandaloneWindows64,"Builds/Windows/ArenaTatica.exe"); }
    [MenuItem("Arena Tática/Exportar Web")]
    public static void Web() { Build(BuildTarget.WebGL,"Builds/Web"); }
    [MenuItem("Arena Tática/Exportar Android")]
    public static void Android()
    {
        PlayerSettings.Android.forceInternetPermission=true;
        PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
        EditorUserBuildSettings.buildAppBundle=false;
        Build(BuildTarget.Android,"Builds/Android/ArenaTatica.apk");
    }
    static void Build(BuildTarget target,string destination)
    {
        EnsureMaterials();
        if(!BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(target),target)) throw new Exception("Módulo não instalado: "+target);
        Directory.CreateDirectory(Path.GetDirectoryName(destination));
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(), locationPathName=destination, target=target, options=BuildOptions.None });
        Debug.Log($"ARENA_BUILD_{report.summary.result}: {target}; {report.summary.totalErrors} erros");
        if(report.summary.result!=BuildResult.Succeeded) throw new Exception("Build falhou: "+target);
    }
}
// O Play do editor sempre começa pela Arena, mesmo com outra cena (ou uma vazia) aberta.
[InitializeOnLoad]
static class ArenaPlayStart
{
    static ArenaPlayStart() { UnityEditor.SceneManagement.EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/ArenaTatica/Scenes/Arena.unity"); }
}
