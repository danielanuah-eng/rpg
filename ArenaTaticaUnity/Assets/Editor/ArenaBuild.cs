using System;
using System.IO;
using System.Linq;
using ArenaTatica;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
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
        const string path="Assets/ArenaTatica/Resources/Materials/ArenaLit.mat";
        if(AssetDatabase.LoadAssetAtPath<Material>(path)) return;
        Directory.CreateDirectory("Assets/ArenaTatica/Resources/Materials");
        var shader=Shader.Find("Universal Render Pipeline/Lit");
        if(!shader) throw new Exception("Shader URP Lit indisponível.");
        AssetDatabase.CreateAsset(new Material(shader),path);
        AssetDatabase.SaveAssets();
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
