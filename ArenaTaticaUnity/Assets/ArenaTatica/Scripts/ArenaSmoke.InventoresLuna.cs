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
 bool lunaCapturesValid=true;
 void CaptureLuna(string path){
  RenderTexture render=null;Texture2D pixels=null;var previous=RenderTexture.active;
  try{
   render=RenderTexture.GetTemporary(1280,720,24,RenderTextureFormat.ARGB32);
   var request=new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest{destination=render};
   if(!UnityEngine.Rendering.RenderPipeline.SupportsRenderRequest(game.View.BoardCamera,request))throw new InvalidOperationException("URP não aceitou captura fora da tela.");
   UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(game.View.BoardCamera,request);
   RenderTexture.active=render;pixels=new Texture2D(1280,720,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,1280,720),0,0);pixels.Apply();
   bool diverse=pixels.GetPixels32().Select(c=>((uint)c.r<<16)|((uint)c.g<<8)|c.b).Distinct().Take(16).Count()>=16;
   lunaCapturesValid&=diverse;File.WriteAllBytes(path,pixels.EncodeToPNG());Debug.Log("LUNA_CAPTURE: "+Path.GetFileName(path)+" pixelsVariados="+diverse);
  }catch(Exception e){lunaCapturesValid=false;Debug.LogError("LUNA_CAPTURE_FAIL: "+e.Message);}
  finally{RenderTexture.active=previous;if(pixels)Destroy(pixels);if(render)RenderTexture.ReleaseTemporary(render);}
 }
 const BindingFlags PrivateInstance=BindingFlags.Instance|BindingFlags.NonPublic;
 object LunaInvoke(object target,string method,params object[] args)=>target.GetType().GetMethod(method,PrivateInstance).Invoke(target,args);
 IEnumerator InventoresLunaQa(){
  var args=Environment.GetCommandLineArgs();int oi=Array.IndexOf(args,"--arena-luna-out");string output=oi>=0&&oi+1<args.Length?args[oi+1]:"Artifacts/inventores-luna-qa";Directory.CreateDirectory(output);
  string[] folders={"Nikola","Bifia","Desland","Nepturius","Blip","Blipware","Celinos","EsferaNepturius","XadrezPeao","XadrezTorre","XadrezCavalo","XadrezBispo","XadrezRainha","Tubarao"};
  string[] dirs={"south","south-east","east","north-east","north","north-west","west","south-west"};
  foreach(var folder in folders){var frames=Resources.LoadAll<Sprite>("Sprites/"+folder);if(dirs.Any(d=>!frames.Any(f=>f.name==d))||!frames.Any(f=>f.name.StartsWith("walk_"))){Finish(false,"Rig incompleto: "+folder);yield break;}}
  var m=Match.New(new MatchSetup{theme=MapGen.DocksTheme,seed=142,flatMap=false,players=new[]{new PlayerSetup{fighters=new[]{"nikola","bifia"}},new PlayerSetup{fighters=new[]{"desland","nepturius"}}}});
  var owner=m.S.fighters.First(x=>x.def=="nikola");var desland=m.S.fighters.First(x=>x.def=="desland");
  string[] extra={"blipware","peca-peao","peca-torre","peca-cavalo","peca-bispo","peca-rainha","tubarao"};
  foreach(var id in extra)LunaInvoke(m,"Spawn",id,id=="tubarao"?MatchState.Neutral:1,new Cell(1,1),id.StartsWith("peca-")?desland.id:owner.id);
  int index=0;foreach(var f in m.S.fighters){f.cell=new Cell(2+(index%4)*2,2+(index/4)*2);var tile=m.S.At(f.cell);if(f.def=="tubarao")tile.terrain=Terrain.Agua;else if(!Board.Walkable(m.S,f.cell,Content.Def(f).flying))tile.terrain=Terrain.Livre;tile.height=0;tile.obj=0;index++;}
  m.S.Luna.gears.Clear();m.S.Luna.families.Clear();m.S.Luna.families.Add(new LunaFamily{owner=owner.id,gears=6});
  m.S.Luna.gears.Add(new Gear{owner=owner.id,cell=Board.Index(m.S,new Cell(5,5)),hits=3,value=1});m.S.Luna.gears.Add(new Gear{owner=owner.id,cell=Board.Index(m.S,new Cell(7,5)),hits=2,value=2});
  foreach(var machine in m.S.Luna.machines)if(machine.by==owner.id)machine.box=new LunaBox{by=owner.id,hp=6,turns=2,cell=Board.Index(m.S,owner.cell)};
  m.S.active=m.S.timeline.IndexOf(owner.id);m.S.revision++;
  typeof(ArenaGame).GetProperty("Match").SetValue(game,m);typeof(ArenaGame).GetProperty("State").SetValue(game,m.S);typeof(ArenaGame).GetProperty("InMenu").SetValue(game,false);game.VsBots=false;game.Menu?.Close();
  game.View.ResetView(m.S);yield return new WaitForSeconds(.7f);
  CaptureLuna(Path.Combine(output,"docas-e-personagens.png"));yield return new WaitForSeconds(.4f);
  var chess=m.S.Luna.machines.FirstOrDefault(x=>x.by==desland.id);if(chess==null){chess=new LunaMachine{by=desland.id};m.S.Luna.machines.Add(chess);}chess.chessTurns=2;m.S.revision++;game.View.Sync(m.S,true);yield return new WaitForSeconds(.3f);CaptureLuna(Path.Combine(output,"dominio-xadrez.png"));chess.chessTurns=0;m.S.revision++;game.View.Sync(m.S,true);yield return new WaitForSeconds(.2f);
  var cameras=FindObjectsByType<Canvas>(FindObjectsSortMode.None);foreach(var canvas in cameras)canvas.enabled=false;
  var view=game.View;var units=(Dictionary<int,Transform>)typeof(ArenaPresentation).GetField("units",PrivateInstance).GetValue(view);var rigs=(IDictionary)typeof(ArenaPresentation).GetField("rigs",PrivateInstance).GetValue(view);
  var nept=m.S.fighters.First(x=>x.def=="nepturius");var unit=units[nept.id];var rig=rigs[unit.GetChild(0)];rig.GetType().GetField("dir").SetValue(rig,5);LunaInvoke(view,"PlayClip",unit,"idle",false);
  var cam=view.BoardCamera;var focus=unit.position+Vector3.up*.8f;cam.transform.position=focus-cam.transform.forward*20;cam.orthographicSize=1.85f;
  yield return new WaitForSeconds(.2f);CaptureLuna(Path.Combine(output,"nepturius-nw-inicio.png"));yield return new WaitForSeconds(.2f);
  float duration=(float)LunaInvoke(view,"PlayClip",unit,"hit",false);yield return new WaitForSeconds(duration*.45f);CaptureLuna(Path.Combine(output,"nepturius-nw-hit.png"));yield return new WaitForSeconds(duration*.6f);CaptureLuna(Path.Combine(output,"nepturius-nw-fim.png"));yield return new WaitForSeconds(.3f);
  // Reprodução do clique relatado pelo autor: piso plano e o corpo grande sobre casas alcançáveis.
  var primalMatch=Match.New(new MatchSetup{seed=142,flatMap=true,players=new[]{new PlayerSetup{fighters=new[]{"colosso","pele-negra"}},new PlayerSetup{fighters=new[]{"espelhos"}}}});
  var beast=primalMatch.S.fighters.First(f=>f.def=="colosso");int otherIndex=0;foreach(var f in primalMatch.S.fighters)if(f!=beast)f.cell=new Cell(10-otherIndex++,10);
  foreach(var tile in primalMatch.S.tiles){tile.terrain=Terrain.Livre;tile.height=0;tile.frozenBy=0;tile.trap=0;tile.obj=0;}
  beast.cell=new Cell(5,5);beast.pm=5;primalMatch.S.active=primalMatch.S.timeline.IndexOf(beast.id);primalMatch.S.revision++;
  typeof(ArenaGame).GetProperty("Match").SetValue(game,primalMatch);typeof(ArenaGame).GetProperty("State").SetValue(game,primalMatch.S);game.Mode="";
  view.ResetView(primalMatch.S);yield return new WaitForSeconds(.6f);var primalFocus=ArenaPresentation.Spot(primalMatch.S,beast,beast.cell)+Vector3.up*.5f;cam.transform.position=primalFocus-cam.transform.forward*20;cam.orthographicSize=3.8f;Physics.SyncTransforms();
  bool primalPickOk=game.CanAct;int primalPickCount=0;
  foreach(var offset in new[]{new Cell(1,0),new Cell(0,1),new Cell(1,1),new Cell(2,0),new Cell(-1,0),new Cell(0,-1)}){
   var expected=beast.cell+offset;var screen=cam.WorldToScreenPoint(ArenaPresentation.Position(primalMatch.S,expected));bool pickedOk=view.Pick(new Vector2(screen.x,screen.y),out var picked,out _)&&picked==expected;bool info=ArenaPresentation.InspectOnMoveTap(beast,Board.At(primalMatch.S,picked),picked,game.CanAct);primalPickOk&=pickedOk&&!info;if(pickedOk&&!info)primalPickCount++;
   Debug.Log("PRIMAL_PICK expected="+expected.x+","+expected.z+" picked="+picked.x+","+picked.z+" correct="+(pickedOk&&!info));
  }
  CaptureLuna(Path.Combine(output,"primal-antes.png"));var primalDestination=beast.cell+new Cell(1,0);game.Submit("mover",primalDestination);yield return new WaitForSeconds(1.5f);
  bool primalMoveOk=beast.cell==primalDestination;bool namesOk=Content.Fighters["pele-negra"].name=="Pele D. Ark"&&Content.Fighters["espelhos"].name=="Mirhor";lunaCapturesValid&=primalPickOk&&primalMoveOk&&namesOk;
  CaptureLuna(Path.Combine(output,"primal-depois.png"));Debug.Log("PRIMAL_MOVEMENT verified="+(primalPickOk&&primalMoveOk)+" picks="+primalPickCount+" names="+namesOk);
  File.WriteAllText(Path.Combine(output,"resultado.json"),JsonUtility.ToJson(new LunaQaResult{passed=lunaCapturesValid,primalPickCases=primalPickCount,primalMovementVerified=primalPickOk&&primalMoveOk,displayNamesVerified=namesOk,rigs=folders.Length,waterTiles=m.S.tiles.Count(t=>t.terrain==Terrain.Agua),theme=m.S.theme,hitNorthWestFrames=Resources.LoadAll<Sprite>("Sprites/Nepturius").Count(x=>x.name.StartsWith("hit_north-west_")),note="Conferir capturas: renderização real, sem substituir a inspeção visual."},true));
  foreach(var canvas in cameras)canvas.enabled=true;Finish(lunaCapturesValid,"Inventores/lua: 14 rigs completos, docas e hit NW capturados; requer revisão das imagens.");
 }
 [Serializable] sealed class LunaQaResult{public bool passed;public int rigs,hitNorthWestFrames,waterTiles,theme,primalPickCases;public bool primalMovementVerified,displayNamesVerified;public string note;}
}
}
