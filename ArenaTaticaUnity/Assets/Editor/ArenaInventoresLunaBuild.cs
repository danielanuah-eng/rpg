using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
public static class ArenaInventoresLunaBuild {
 [Serializable] sealed class Delivery {public Entry[] entries;}
 [Serializable] sealed class Entry {public string resource;public int frames,frameSize;}
 [Serializable] sealed class Result {public bool passed;public int files,frames,rigs;public string note;}
 [Serializable] sealed class BuildResult{public string result;public int errors,warnings;public string[] messages;}
 public static void Windows(){
  ArenaInventoresLunaImport.Apply();
  var delivery=JsonUtility.FromJson<Delivery>(File.ReadAllText("Assets/ArenaTatica/Resources/Manifestos/inventores-luna.json"));int count=0;
  foreach(var e in delivery.entries){var frames=Resources.LoadAll<Sprite>(e.resource);if(frames.Length!=e.frames)throw new Exception(e.resource+": esperado "+e.frames+", importado "+frames.Length);if(frames.Any(f=>f.rect.width!=e.frameSize||f.rect.height!=e.frameSize))throw new Exception("Corte incorreto: "+e.resource);count+=frames.Length;}
  string[] folders={"Nikola","Bifia","Desland","Nepturius","Blip","Blipware","Celinos","EsferaNepturius","XadrezPeao","XadrezTorre","XadrezCavalo","XadrezBispo","XadrezRainha","Tubarao"};string[] directions={"south","south-east","east","north-east","north","north-west","west","south-west"};
  foreach(var folder in folders){var sprites=Resources.LoadAll<Sprite>("Sprites/"+folder);if(directions.Any(d=>!sprites.Any(s=>s.name==d)))throw new Exception("Direção parada ausente: "+folder);}
  var route=typeof(ArenaTatica.ArenaPresentation).GetMethod("EffectFrames",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static);
  foreach(var path in new[]{"tiro-sonus/impacto","comando-blip/conjuracao","comando-celinos/impacto","eco-de-aliado/impacto","dominio-xadrez/impacto","mover-cavalo/impacto","mover-esfera/impacto","tiro-celinos/impacto"})if(!(route.Invoke(null,new object[]{path}) is Sprite[] linked)||linked.Length==0)throw new Exception("Efeito sem ligação: "+path);
  Debug.Log("INVENTORES_LUNA_EFFECT_ROUTES_OK routes=8");
  var args=Environment.GetCommandLineArgs();int n=Array.IndexOf(args,"--luna-report");if(n>=0&&n+1<args.Length)File.WriteAllText(args[n+1],JsonUtility.ToJson(new Result{passed=true,files=delivery.entries.Length,frames=count,rigs=folders.Length,note="Importação e cortes conferidos; revisão visual registrada separadamente."},true));
  Debug.Log("INVENTORES_LUNA_ASSETS_OK files="+delivery.entries.Length+" frames="+count+" rigs="+folders.Length);
  ArenaBuild.Windows();
  var report=UnityEditor.Build.Reporting.BuildReport.GetLatestReport();
  if(n>=0&&n+1<args.Length&&report!=null){var errors=report.steps.SelectMany(step=>step.messages).Where(message=>message.type==LogType.Error||message.type==LogType.Exception).Select(message=>System.Text.RegularExpressions.Regex.Replace(message.content,@"(?i)(Bearer\s+|accessToken[=\s]+)[^\s]+","$1[redacted]")).ToArray();File.WriteAllText(Path.Combine(Path.GetDirectoryName(args[n+1]),"build-report.json"),JsonUtility.ToJson(new BuildResult{result=report.summary.result.ToString(),errors=report.summary.totalErrors,warnings=report.summary.totalWarnings,messages=errors},true));}

 }
}
