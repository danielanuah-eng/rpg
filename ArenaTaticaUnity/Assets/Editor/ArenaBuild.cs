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
        PlayerSettings.companyName="Arcanoclisma"; PlayerSettings.productName="Arena Tática"; PlayerSettings.bundleVersion="1.2.2";
        PlayerSettings.Android.bundleVersionCode=5;
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
        EnsureMaterial("Assets/ArenaTatica/Resources/Materials/ArenaSilhueta.mat","Arena/Silhueta");
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
            string path=file.Replace('\\','/'); if(ArenaEfeitosImport.IsManaged(path)) continue;
            var importer=(TextureImporter)AssetImporter.GetAtPath(path); if(importer==null) continue;
            var pixels=new Texture2D(2,2); pixels.LoadImage(File.ReadAllBytes(path));
            // Projeteis/<skill>.png é uma imagem única (pode ser larga); o resto, tira de quadros quadrados.
            // Ceus/<id>/fundo.png e nuvens.png (tarefa 015) também são imagens únicas e largas.
            bool single=path.Contains("/Projeteis/")||path.Contains("/Ceus/")||path.Contains("/Menu/");
            int w=pixels.width,h=pixels.height,frames=w>h&&!single?w/h:1,size=frames>1?h:w;
            int foot=h; var data=pixels.GetPixels32();
            for(int i=0;i<data.Length;i++) if(data[i].a>40) { foot=i/w; break; }
            if(foot==h) foot=0;
            UnityEngine.Object.DestroyImmediate(pixels);
            // Efeitos (projéteis, raios, moedas) giram em torno do centro; lutadores e ícones apoiam no pé.
            // Dados da HUD (tarefa 013): pixel duro, sem mipmaps, centro; aparecem perto do tamanho original.
            bool hud=path.Contains("/Dados/")||path.Contains("/Avisos/"); var filter=hud?FilterMode.Point:FilterMode.Trilinear;
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
    // Com acento no caminho do projeto o Unity recusa o Android: o menu chama Tools/build-android.ps1 (cópia fixa em
    // caminho ASCII, build incremental) numa janela própria; o APK chega em Builds/Android/ArenaTatica.apk.
    [MenuItem("Arena Tática/Exportar Android")]
    static void AndroidMenu()
    {
        string root=Path.GetFullPath(Path.Combine(Application.dataPath,".."));
        if(!root.Any(c=>c>127)) { Android(); return; }
        AssetDatabase.SaveAssets();
        string script=Path.Combine(root,"Tools","build-android.ps1");
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("powershell.exe",$"-NoExit -ExecutionPolicy Bypass -File \"{script}\"") { UseShellExecute=true, WorkingDirectory=root });
        Debug.Log("ARENA_ANDROID: build iniciado numa cópia em caminho ASCII (janela do PowerShell); log em Artifacts/android-cli.log.");
    }
    public static void Android()
    {
        // Celular só deitado (a mesa e a HUD foram feitas para paisagem); gira entre os dois lados.
        PlayerSettings.defaultInterfaceOrientation=UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToPortrait=false; PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
        PlayerSettings.allowedAutorotateToLandscapeLeft=true; PlayerSettings.allowedAutorotateToLandscapeRight=true;
        PlayerSettings.Android.forceInternetPermission=true;
        PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
        EditorUserBuildSettings.buildAppBundle=false;
        Build(BuildTarget.Android,"Builds/Android/ArenaTatica.apk");
    }
    static void Build(BuildTarget target,string destination)
    {
        PlayerSettings.bundleVersion="1.2.2"; PlayerSettings.Android.bundleVersionCode=5;
        EnsureMaterials();
        if(!BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(target),target)) throw new Exception("Módulo não instalado: "+target);
        Directory.CreateDirectory(Path.GetDirectoryName(destination));
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(), locationPathName=destination, target=target, options=BuildOptions.None });
        Debug.Log($"ARENA_BUILD_{report.summary.result}: {target}; {report.summary.totalErrors} erros");
        if(report.summary.result!=BuildResult.Succeeded) throw new Exception("Build falhou: "+target);
    }
}
// Convite da sala no Android: arenatatica://sala/CÓDIGO abre o jogo (ArenaGame trata em Application.deepLinkActivated).
// Injeta o intent-filter na activity de entrada do projeto Gradle gerado, sem manter um AndroidManifest próprio.
class ArenaAndroidDeepLink : UnityEditor.Android.IPostGenerateGradleAndroidProject
{
    public const string Scheme="arenatatica";
    public int callbackOrder => 1;
    public void OnPostGenerateGradleAndroidProject(string unityLibrary)
    {
        const string ns="http://schemas.android.com/apk/res/android";
        foreach(string path in new[]{Path.Combine(unityLibrary,"src/main/AndroidManifest.xml"),Path.Combine(unityLibrary,"../launcher/src/main/AndroidManifest.xml")}) {
            if(!File.Exists(path)) continue;
            var doc=new System.Xml.XmlDocument(); doc.Load(path);
            var nsm=new System.Xml.XmlNamespaceManager(doc.NameTable); nsm.AddNamespace("android",ns);
            var activity=doc.SelectSingleNode("//activity[intent-filter/action[@android:name='android.intent.action.MAIN']]",nsm) as System.Xml.XmlElement;
            if(activity==null||activity.SelectSingleNode($"intent-filter/data[@android:scheme='{Scheme}']",nsm)!=null) continue;
            var filter=doc.CreateElement("intent-filter");
            System.Xml.XmlElement Child(string name,string value) { var e=doc.CreateElement(name); e.SetAttribute("name",ns,value); filter.AppendChild(e); return e; }
            Child("action","android.intent.action.VIEW"); Child("category","android.intent.category.DEFAULT"); Child("category","android.intent.category.BROWSABLE");
            var data=doc.CreateElement("data"); data.SetAttribute("scheme",ns,Scheme); data.SetAttribute("host",ns,"sala"); filter.AppendChild(data);
            activity.AppendChild(filter); doc.Save(path);
            Debug.Log("ARENA_DEEPLINK: "+Scheme+"://sala em "+path);
            return;
        }
        throw new Exception("Activity de entrada não encontrada para registrar "+Scheme+"://sala");
    }
}
// Atalhos de conferência no Play (o eval do MCP falha no Play por causa do Á no caminho): partida montada e ação do lutador ativo.
static class ArenaTestMenu
{
    static ArenaGame Game => UnityEngine.Object.FindFirstObjectByType<ArenaGame>();
    static void Start(bool bots,params string[][] teams) { var g=Game; if(!g) return; g.Menu?.Close(); g.VsBots=bots; g.StartLocal(teams); }
    [MenuItem("Arena Tática/Teste/Damian e Baltazar contra bot")] static void DamianBaltazar() => Start(true,new[]{"damian","baltazar"},new[]{"ogro-de-pedra","perry"});
    [MenuItem("Arena Tática/Teste/Baltazar contra bot")] static void Baltazar() => Start(true,new[]{"baltazar"},new[]{"ogro-de-pedra"});
    [MenuItem("Arena Tática/Teste/Tchu tcha contra bot")] static void TchuTcha() => Start(true,new[]{"tchu-tcha"},new[]{"ciantia"});
    [MenuItem("Arena Tática/Teste/Bots contra bots")] static void BotsOnly() { Start(true,new[]{"baltazar","perry"},new[]{"damian","teantha"}); }
    [MenuItem("Arena Tática/Teste/Painel: abrir ou recolher")] static void Side() => Game?.Hud?.ToggleSide();
    [MenuItem("Arena Tática/Teste/Painel: Ações ou Partida")] static void Page() => Game?.Hud?.TogglePage();
    [MenuItem("Arena Tática/Teste/Loja: espiar")] static void Shop() => Game?.Hud?.ToggleShopPeek();
    static void Self(string skill) { var g=Game; var f=g?.Active; if(f!=null) g.Submit("skill",f.cell,skill); }
    [MenuItem("Arena Tática/Teste/Ativo: martelo (Damian)")] static void Hammer() => Self("camaleao-martelo");
    [MenuItem("Arena Tática/Teste/Ativo: canhão (Damian)")] static void Cannon() => Self("camaleao-canhao");
    [MenuItem("Arena Tática/Teste/Ativo: aura (Baltazar)")] static void Aura() => Self("baltazar-dualidade");
    [MenuItem("Arena Tática/Teste/Ativo: Olho abissal (Tchu tcha)")] static void Berserk() => Self("olho-abissal");
    [MenuItem("Arena Tática/Teste/Editor de mapas (sorteado 12×12)")] static void MapEditor() => Game?.MapEditor.Open(ArenaTatica.Core.MapGen.NewMap(12,7));
    [MenuItem("Arena Tática/Teste/Editor: árvores no meio e caixa movida")] static void MapEdits()
    {
        var e=Game?.MapEditor; if(e==null||!e.Active) return;
        e.SetTool("arvore"); e.Tap(new Cell(5,3)); e.Tap(new Cell(6,3));
        e.SetTool("subir"); e.Tap(new Cell(3,4)); e.Tap(new Cell(3,4));
        e.SetTool("mover"); e.Tap(new Cell(5,3)); e.Tap(new Cell(8,4));
        e.SetTool("modelo"); e.Tap(new Cell(6,3)); e.Tap(new Cell(6,3));
        e.SetTool("enfeite"); e.Tap(new Cell(3,6)); e.Tap(new Cell(2,6));
    }
    [MenuItem("Arena Tática/Teste/Editor: salvar e jogar")] static void MapSavePlay() => Game?.MapEditor.Save(true);
    // Percorre os mapas do Codex (Resources/Mapas/codex.json) numa partida contra bot, e gira a vista.
    static int codexMap=-1;
    [MenuItem("Arena Tática/Teste/Mapa do Codex: próximo")] static void NextCodexMap()
    {
        var g=Game; if(!g) return; var list=ArenaMaps.Builtin; if(list.Count==0) return;
        codexMap=(codexMap+1)%list.Count; var map=list[codexMap]; g.MapName=map.name; g.SetSeats(ArenaTatica.Core.MapGen.PlayersFor(map.width));
        Start(true,Enumerable.Range(0,g.Seats).Select(i=>i==0?new[]{"baltazar","damian"}:new[]{"ogro-de-pedra"}).ToArray()); Debug.Log("Mapa: "+map.name);
    }
    [MenuItem("Arena Tática/Teste/Girar vista")] static void Rotate() => Game?.View.RotateView(1);
    // Toque de celular simulado (Touchscreen virtual): dedo desce e sobe numa casa vizinha do lutador ativo; loga se ele andou.
    [MenuItem("Arena Tática/Teste/Toque: mover para casa vizinha")] static void TouchMove()
    {
        var g=Game; var f=g?.Active; var s=g?.State; if(f==null) return;
        if(!g.CanAct) { Debug.Log("ARENA_TOQUE: não é a vez de um lutador do jogador; passe a vez do bot e tente de novo"); return; }
        var screen=UnityEngine.InputSystem.Touchscreen.current??UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Touchscreen>();
        var from=f.cell; var targets=new[]{new Cell(from.x+1,from.z),new Cell(from.x-1,from.z),new Cell(from.x,from.z+1),new Cell(from.x,from.z-1)}
            .Where(c=>c.x>=0&&c.z>=0&&c.x<s.width&&c.z<s.height).ToArray();
        int step=0,target=0; Vector2 pos=default;
        void Touch(UnityEngine.InputSystem.TouchPhase phase) => UnityEngine.InputSystem.InputSystem.QueueStateEvent(screen,new UnityEngine.InputSystem.LowLevel.TouchState{touchId=1,phase=phase,position=pos});
        void Tick()
        {
            if(!Game||target>=targets.Length) { EditorApplication.update-=Tick; Debug.Log("ARENA_TOQUE: nenhuma casa vizinha aceitou o movimento"); return; }
            if(step==0) { var c=targets[target]; Vector3 p=Game.View.BoardCamera.WorldToScreenPoint(ArenaTatica.ArenaPresentation.Position(Game.State,c)); pos=new Vector2(p.x,p.y); Touch(UnityEngine.InputSystem.TouchPhase.Began); }
            else if(step==4) Touch(UnityEngine.InputSystem.TouchPhase.Ended);
            else if(step==12) {
                var moved=Game.State.fighters.FirstOrDefault(x=>x.id==f.id);
                if(moved!=null&&moved.cell.Equals(targets[target])) { EditorApplication.update-=Tick; Debug.Log($"ARENA_TOQUE: {f.def} andou de {from.x},{from.z} para a casa tocada {moved.cell.x},{moved.cell.z}"); return; }
                target++; step=-1;
            }
            step++;
        }
        EditorApplication.update+=Tick;
    }
    // Props ambientais (GLB com clipe Legacy): confirma no Play que cada Animation está tocando em laço.
    [MenuItem("Arena Tática/Teste/Props animados: estado")] static void AmbientState()
    {
        foreach(var a in UnityEngine.Object.FindObjectsByType<Animation>(FindObjectsSortMode.None)) {
            var s=a.clip?a[a.clip.name]:null;
            Debug.Log($"ARENA_PROP: {a.gameObject.name} tocando={a.isPlaying} laco={s?.wrapMode} t={s?.time:0.00}");
        }
    }
    [MenuItem("Arena Tática/Teste/Ativo: passar")] static void Pass() => Game?.Submit("passar");
}
// O Play do editor sempre começa pela Arena, mesmo com outra cena (ou uma vazia) aberta.
[InitializeOnLoad]
static class ArenaPlayStart
{
    static ArenaPlayStart() { UnityEditor.SceneManagement.EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/ArenaTatica/Scenes/Arena.unity"); }
}
