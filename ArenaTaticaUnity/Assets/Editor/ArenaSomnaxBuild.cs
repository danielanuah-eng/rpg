using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
public static class ArenaSomnaxBuild {
 [Serializable] sealed class Delivery {public Entry[] entries;public string[] audio;}
 [Serializable] sealed class Entry {public string resource;public int frames,frameSize;}
 public static void Windows(){
  ArenaSomnaxGuardaImport.Apply();var delivery=JsonUtility.FromJson<Delivery>(File.ReadAllText("Assets/ArenaTatica/Resources/Manifestos/somnax-guarda-vozes.json"));int count=0;
  foreach(var e in delivery.entries){var frames=Resources.LoadAll<Sprite>(e.resource);if(frames.Length!=e.frames||frames.Any(f=>f.rect.width!=e.frameSize||f.rect.height!=e.frameSize))throw new Exception("Corte ou quadros inválidos: "+e.resource);count+=frames.Length;}
  foreach(var path in delivery.audio){var clip=Resources.Load<AudioClip>(path);if(!clip||clip.channels!=1||clip.frequency!=44100)throw new Exception("Áudio técnico inválido: "+path);}
  Debug.Log("SOMNAX_BUILD_ASSETS_OK entries="+delivery.entries.Length+" frames="+count+" audio="+delivery.audio.Length+" audioApprovalPending=8");
  if(Environment.GetCommandLineArgs().Contains("--somnax-incremental-qa")) {
   typeof(ArenaBuild).GetMethod("CheckPackages",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static).Invoke(null,null);
   var scenes=EditorBuildSettings.scenes.Where(scene=>scene.enabled).Select(scene=>scene.path).ToArray();if(scenes.Length==0)throw new Exception("Cena preparada ausente.");
   var built=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName="Builds/Windows/ArenaTatica.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
   Debug.Log("SOMNAX_INCREMENTAL_BUILD_"+built.summary.result+" errors="+built.summary.totalErrors);if(built.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Build incremental falhou.");
  }else ArenaBuild.Windows();RestoreDefaultResources();var report=UnityEditor.Build.Reporting.BuildReport.GetLatestReport();var args=Environment.GetCommandLineArgs();int n=Array.IndexOf(args,"--somnax-report");
  if(report!=null&&n>=0&&n+1<args.Length)File.WriteAllText(args[n+1],JsonUtility.ToJson(new Result{result=report.summary.result.ToString(),errors=report.summary.totalErrors,warnings=report.summary.totalWarnings},true));
 }
 static void RestoreDefaultResources(){
  const string target="Builds/Windows/ArenaTatica_Data/Resources/unity default resources";if(File.Exists(target))return;
  foreach(var variant in new[]{"win64_player_nondevelopment_mono","win64_player_development_mono"}){
   string source=Path.Combine(EditorApplication.applicationContentsPath,"PlaybackEngines/windowsstandalonesupport/Variations",variant,"Data/Resources/unity default resources");
   if(!File.Exists(source))continue;Directory.CreateDirectory(Path.GetDirectoryName(target));File.Copy(source,target);Debug.Log("SOMNAX_DEFAULT_RESOURCES_RESTORED sameEditorVersion=true");return;
  }
  throw new Exception("Recurso padrão do player ausente na instalação da mesma versão do Unity.");
 }
 [Serializable] sealed class Result{public string result;public int errors,warnings;}
}
