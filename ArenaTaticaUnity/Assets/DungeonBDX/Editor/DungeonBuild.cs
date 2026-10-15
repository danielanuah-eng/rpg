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
