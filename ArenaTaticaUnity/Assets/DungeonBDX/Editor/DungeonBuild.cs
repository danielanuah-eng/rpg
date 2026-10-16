using System;
using System.IO;
using DungeonBDX;
using TMPro;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class DungeonBuild
{
    public const string ScenePath = "Assets/DungeonBDX/Scenes/DungeonBDX.unity";

    [MenuItem("DungeonBDX/Preparar cena de seleção")]
    public static void Prepare()
    {
        EnsureMaterial("WorldLit","Universal Render Pipeline/Lit");
        EnsureMaterial("SkillUnlit","Universal Render Pipeline/Unlit");
        foreach(var guid in AssetDatabase.FindAssets("t:Model",new[]{"Assets/DungeonBDX/Art"}))
        {
            var path=AssetDatabase.GUIDToAssetPath(guid);
            if(AssetImporter.GetAtPath(path) is ModelImporter model && !model.isReadable)
            { model.isReadable=true;model.importAnimation=false;model.SaveAndReimport(); }
        }
        if (!Resources.Load<TMP_Settings>("TMP Settings"))
        {
            throw new Exception("Restaure os recursos TMP oficiais incluídos em Assets/DungeonBDX/TextMesh Pro antes de preparar.");
        }
        Directory.CreateDirectory("Assets/DungeonBDX/Scenes");
        // Batch starts with an untitled scene, where Unity forbids additive creation.
        // In an interactive Editor preserve its existing scenes.
        bool additive = !Application.isBatchMode;
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, additive ? NewSceneMode.Additive : NewSceneMode.Single);
        try
        {
            var shell = new GameObject("DungeonBDX", typeof(DungeonShell));
            SceneManager.MoveGameObjectToScene(shell, scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new Exception("Falha ao salvar cena DungeonBDX.");
            Debug.Log("DUNGEONBDX_PREPARE_OK heroes=" + DungeonHeroCatalog.ImportArena().Count);
        }
        finally { if (additive) EditorSceneManager.CloseScene(scene, true); }
        AssetDatabase.SaveAssets();
    }

    static void EnsureMaterial(string name,string shaderName)
    {
        const string folder="Assets/DungeonBDX/Art/Resources/DungeonBDX/Materials";
        if(!Directory.Exists(folder)){Directory.CreateDirectory(folder);AssetDatabase.Refresh();}
        string path=folder+"/"+name+".mat";
        if(AssetDatabase.LoadAssetAtPath<Material>(path))return;
        var shader=Shader.Find(shaderName);if(!shader)throw new Exception("Shader obrigatório indisponível: "+shaderName);
        AssetDatabase.CreateAsset(new Material(shader){name=name,color=Color.white,enableInstancing=true},path);
    }

    [MenuItem("DungeonBDX/Build Windows da prévia")]
    public static void Windows() => BuildWindows(false);

    public static void WindowsScriptsOnly() => BuildWindows(true);

    static void BuildWindows(bool scriptsOnly)
    {
        if (!scriptsOnly) Prepare();
        else if (!File.Exists(ScenePath) || !File.Exists("Builds/DungeonBDX/DungeonBDX.exe"))
            throw new Exception("Build completo necessário antes de atualizar somente scripts.");
        string productName = PlayerSettings.productName;
        try
        {
            PlayerSettings.productName = "DungeonBDX";
            Directory.CreateDirectory("Builds/DungeonBDX");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath }, locationPathName = "Builds/DungeonBDX/DungeonBDX.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | (scriptsOnly ? BuildOptions.BuildScriptsOnly : BuildOptions.None)
            });
            if (report.summary.result != BuildResult.Succeeded) throw new Exception("DungeonBDX build: " + report.summary.result);
            Debug.Log("DUNGEONBDX_WINDOWS_OK bytes=" + report.summary.totalSize);
        }
        finally { PlayerSettings.productName = productName; }
    }
}
