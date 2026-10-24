using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using ArenaTatica.Core;
using Terrain=ArenaTatica.Core.Terrain;
namespace ArenaTatica {
public sealed partial class ArenaSmoke {
 [Serializable] sealed class SomnaxQaResult {public bool passed,regularSize,guardLocked,healingVerified,ruptureVerified;public int guardDirections,pillarsVisible;public string note;}
 void SomnaxFixture(Match m,Fighter active) {
  m.S.active=m.S.timeline.IndexOf(active.id);m.S.placing=0;m.S.revision++;active.pa=30;active.pm=6;
  typeof(ArenaGame).GetProperty("Match").SetValue(game,m);typeof(ArenaGame).GetProperty("State").SetValue(game,m.S);typeof(ArenaGame).GetProperty("InMenu").SetValue(game,false);game.VsBots=false;game.Menu?.Close();game.Mode="";
  game.View.ResetView(m.S);
 }
 bool SomnaxUse(Match m,Fighter f,string skill,Cell cell,Cell cell2=default) {
  var ok=m.Apply(new Command{kind="skill",fighter=f.id,revision=m.S.revision,skill=skill,cell=cell,cell2=cell2},f.team,out var events,out var reason);
  Debug.Log("SOMNAX_QA_COMMAND "+skill+" accepted="+ok+" reason="+reason);
  if(ok) game.View.Play(events,m.S);else lunaCapturesValid=false;return ok;
 }
 float somnaxQaZoom=4;
 void CaptureSomnax(Match m,string path) {
  var cam=game.View.BoardCamera;var hero=m.S.fighters.First(f=>f.def=="somnax");
  var center=ArenaPresentation.Position(m.S,somnaxQaZoom<3?hero.cell:new Cell(5,5))+Vector3.up*.7f;
  cam.transform.position=center-cam.transform.forward*20;cam.orthographicSize=somnaxQaZoom;CaptureLuna(path);
 }
 IEnumerator WaitSomnaxEffect(string prefix,float timeout=8) {
  float end=Time.realtimeSinceStartup+timeout;
  while(Time.realtimeSinceStartup<end&&!FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Any(r=>r.sprite&&r.sprite.name.StartsWith(prefix)))yield return null;
 }
 IEnumerator SomnaxGuardaQa() {
  AudioListener.volume=0; // Runtime only: this fixture never changes the user's sound preferences.
  var args=Environment.GetCommandLineArgs();int oi=Array.IndexOf(args,"--arena-somnax-out");string output=oi>=0&&oi+1<args.Length?args[oi+1]:"Artifacts/somnax-guarda-qa";Directory.CreateDirectory(output);
  var m=Match.New(new MatchSetup{flatMap=true,seed=142,team1=new[]{"somnax","tchu-tcha","ciantia"},team2=new[]{"perry"}});
  foreach(var tile in m.S.tiles){tile.terrain=Terrain.Livre;tile.height=0;tile.obj=0;tile.trap=0;tile.coins=0;}
  var hero=m.S.fighters.First(x=>x.def=="somnax");var guard=m.S.fighters.First(x=>x.def=="tchu-tcha");var ally=m.S.fighters.First(x=>x.def=="ciantia");var enemy=m.S.fighters.First(x=>x.def=="perry");
  hero.cell=new Cell(4,4);guard.cell=new Cell(6,4);ally.cell=new Cell(5,5);enemy.cell=new Cell(7,5);SomnaxFixture(m,hero);yield return new WaitForSeconds(.7f);
  var view=game.View;var cam=view.BoardCamera;var focus=ArenaPresentation.Position(m.S,new Cell(5,4))+Vector3.up*.6f;cam.transform.position=focus-cam.transform.forward*20;cam.orthographicSize=4;
  foreach(var canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))canvas.enabled=false;
  yield return new WaitForSeconds(.2f);CaptureSomnax(m,Path.Combine(output,"somnax-escala-regular.png"));
  var units=(Dictionary<int,Transform>)typeof(ArenaPresentation).GetField("units",PrivateInstance).GetValue(view);var rigs=(IDictionary)typeof(ArenaPresentation).GetField("rigs",PrivateInstance).GetValue(view);
  var result=new SomnaxQaResult{regularSize=Content.Def(hero).size==1,guardLocked=true};
  // Four genuine authoritative activations; toggling off is required between orientations.
  foreach(var direction in new[]{new Cell(1,0),new Cell(0,1),new Cell(-1,0),new Cell(0,-1)}) {
   m.S.active=m.S.timeline.IndexOf(guard.id);m.S.revision++;guard.pa=30;
   if(!SomnaxUse(m,guard,"guarda-direcional",guard.cell+direction))break;
   yield return new WaitForSeconds(.45f);
   units=(Dictionary<int,Transform>)typeof(ArenaPresentation).GetField("units",PrivateInstance).GetValue(view);rigs=(IDictionary)typeof(ArenaPresentation).GetField("rigs",PrivateInstance).GetValue(view);
   var rig=rigs[units[guard.id].GetChild(0)];result.guardLocked&=(bool)rig.GetType().GetField("guardLocked").GetValue(rig)&&Match.GuardActive(guard);result.guardDirections++;
   CaptureSomnax(m,Path.Combine(output,"guarda-"+direction.x+"-"+direction.z+".png"));
   SomnaxUse(m,guard,"guarda-direcional",guard.cell);yield return new WaitForSeconds(.3f);
  }
  SomnaxFixture(m,hero);hero.hp=1;guard.hp=1;ally.hp=1;yield return new WaitForSeconds(.3f);
  SomnaxUse(m,hero,"doce-e-salgado",guard.cell,ally.cell);yield return WaitSomnaxEffect("sorvete_");yield return new WaitForSeconds(.15f);CaptureSomnax(m,Path.Combine(output,"doce-e-salgado-gesto.png"));yield return WaitSomnaxEffect("cura_");yield return new WaitForSeconds(.12f);CaptureSomnax(m,Path.Combine(output,"doce-e-salgado-cura.png"));
  while(view.Busy)yield return null;result.healingVerified=hero.hp==3&&guard.hp==3&&ally.hp==3;
  enemy.cell=new Cell(6,6);hero.pa=30;m.S.revision++;view.Sync(m.S,true);int hp=enemy.hp;yield return new WaitForSeconds(.25f);
  m.Forced.Clear();m.Forced.Enqueue(Face.Trava);m.Forced.Enqueue(Face.Trava);
  SomnaxUse(m,hero,"ruptura-das-memorias",new Cell(5,6));yield return new WaitForSeconds(.55f);CaptureSomnax(m,Path.Combine(output,"ruptura-conjuracao.png"));yield return WaitSomnaxEffect("pilar_2");result.pillarsVisible=FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Count(r=>r.sprite&&r.sprite.name.StartsWith("pilar_"));CaptureSomnax(m,Path.Combine(output,"ruptura-pilares.png"));
  while(view.Busy)yield return null;result.ruptureVerified=enemy.hp==hp-5&&guard.hp==3&&ally.hp==3;
  // Native clip sampling in the actual player, same camera and regular character scale.
  focus=units[hero.id].position+Vector3.up*.8f;cam.transform.position=focus-cam.transform.forward*20;cam.orthographicSize=1.8f;somnaxQaZoom=1.8f;
  foreach(var clip in new[]{"pacto","renuncia","blinkout","blinkin","support","ult","hit","death"}) {
   float duration=(float)LunaInvoke(view,"PlayClip",units[hero.id],clip,false);yield return new WaitForSeconds(Mathf.Max(.1f,duration*.45f));CaptureSomnax(m,Path.Combine(output,"somnax-"+clip+".png"));yield return new WaitForSeconds(Mathf.Max(.1f,duration*.6f));
  }
  result.passed=lunaCapturesValid&&result.regularSize&&result.guardLocked&&result.guardDirections==4&&result.healingVerified&&result.ruptureVerified&&result.pillarsVisible==9;
  result.note="Capturas reais requerem inspeção visual. Áudio provisório silenciado em runtime; oito falas aguardam gravações de entonação. Não valida host/cliente nem reconexão.";
  File.WriteAllText(Path.Combine(output,"resultado.json"),JsonUtility.ToJson(result,true));Finish(result.passed,"Somnax e guarda: comandos, quatro direções e efeitos capturados; inspecionar imagens.");
 }
}
}
