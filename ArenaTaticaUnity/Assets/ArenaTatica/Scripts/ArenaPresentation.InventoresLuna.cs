using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ArenaTatica.Core;
using Terrain=ArenaTatica.Core.Terrain;
namespace ArenaTatica {
public sealed partial class ArenaPresentation {
 Transform lunaRoot; Mesh lunaChessMesh; Material lunaChessMaterial; MatchState lunaState; int lunaRevision=-1; static bool lunaStyle;
 static readonly HashSet<string> LunaEvents=new HashSet<string>{"funilaria","teia-some","funilaria-golpe","funilaria-fim","blip-loja","comando-celinos","matriz-protecao","matriz-protecao-fim","matriz-dano","caixa-voo","viva-luna","viva-luna-fim","roque","dominio-xadrez","dominio-xadrez-fim","eco-de-aliado","aura-nepturius","nepturius-esfera","nepturius-cubo","esfera-orfa","agarrao-negro","loucura-negra","explosao-antimateria","mordida-tubarao","tubaroes-inicio","tubaroes-fim"};
 static void EnsureLunaStyle() {
  if(lunaStyle)return;lunaStyle=true;
  foreach(var x in new Dictionary<string,float>{{"nikola",1.7f},{"bifia",1.7f},{"desland",1.75f},{"nepturius",1.75f},{"blip",1.15f},{"blipware",2.35f},{"celinos",.85f},{"esfera-nepturius",.9f},{"peca-peao",.85f},{"peca-torre",1f},{"peca-cavalo",1.1f},{"peca-bispo",1.15f},{"peca-rainha",1.25f},{"tubarao",.85f}})Heights[x.Key]=x.Value;
  Shots["eletaser"]=(ShotKind.Raio,Blue);Shots["flecha-lunar"]=(ShotKind.Flecha,Blue);Shots["tiro-celinos"]=(ShotKind.Bala,Blue);Shots["xeque-mate"]=(ShotKind.Orbe,Violet);
 }
 static Sprite[] LunaEffectFrames(string path) {
  if(string.IsNullOrEmpty(path))return null;string id=path.Split('/')[0];
  switch(id){
   case "comando-blip":case "comando-celinos":case "blipware":id="funilaria-especial";break;
   case "funilaria":id="funilaria-engrenagem";break;
   case "mover-esfera":case "loucura-negra":case "nepturius-esfera":case "esfera-orfa":id="aura-nepturius";break;
   case "nepturius-cubo":id="aura-nepturius";break;
   case "saida-matriz":case "caixa-voo":id="matriz-protecao";break;
   case "tiro-celinos":id="eletaser";break;
   case "mover-peao":case "mover-torre":case "mover-cavalo":case "mover-bispo":case "mover-rainha":id="dominio-xadrez";break;
   case "tubaroes-inicio":case "tubaroes-fim":id="mordida-tubarao";break;
  }
  var frames=Resources.LoadAll<Sprite>("Sprites/Efeitos/InventoresLuna/"+id).OrderBy(x=>x.name,StringComparer.Ordinal).ToArray();return frames.Length>0?frames:null;
 }
 Sprite[] LunaArt(string id) => LunaEffectFrames(id)??Array.Empty<Sprite>();
 GameObject LunaSprite(string id,Vector3 at,float width,Transform parent,bool flat=false) {
  var frames=LunaArt(id);if(frames.Length==0)return null;
  var go=new GameObject(id,typeof(SpriteRenderer));go.transform.SetParent(parent,false);go.transform.position=at;
  var sr=go.GetComponent<SpriteRenderer>();sr.sprite=frames[0];if(spriteMaterial)sr.sharedMaterial=spriteMaterial;sr.sortingOrder=flat?-2:15;
  go.transform.localScale=Vector3.one*(width/Mathf.Max(.01f,sr.sprite.bounds.size.x));
  if(flat)go.transform.rotation=Quaternion.Euler(90,0,0);else go.AddComponent<LunaFacing>().camera=BoardCamera;
  if(frames.Length>1){var a=go.AddComponent<LunaSpriteLoop>();a.frames=frames;a.fps=10;}
  return go;
 }
 void LunaLabel(string text,Vector3 at,Color color,Transform parent,Transform anchor=null,float size=.035f) {
  var go=new GameObject("Marcador "+text,typeof(TextMesh));go.transform.SetParent(parent,false);go.transform.position=at;
  var t=go.GetComponent<TextMesh>();t.text=text;t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=64;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=color;t.fontStyle=FontStyle.Bold;
  go.GetComponent<MeshRenderer>().sharedMaterial=t.font.material;go.AddComponent<LunaFacing>().camera=BoardCamera;
  if(anchor){var a=go.AddComponent<LunaAnchor>();a.target=anchor;a.offset=at-anchor.position;}
 }
 void SyncLuna(MatchState s) {
  if(s==null)return;EnsureLunaStyle();if(lunaState==s&&lunaRevision==s.revision&&lunaRoot)return;lunaState=s;lunaRevision=s.revision;
  if(lunaChessMesh){Destroy(lunaChessMesh);lunaChessMesh=null;}
  if(lunaRoot)Destroy(lunaRoot.gameObject);lunaRoot=new GameObject("Inventores e Lua").transform;lunaRoot.SetParent(world,false);
  foreach(var g in s.Luna.gears) {
   var c=new Cell(g.cell%s.width,g.cell/s.width);if(!Board.Contains(s,c))continue;
   var at=Position(s,c)+Vector3.up*.12f;LunaSprite(g.value>1?"funilaria-especial":"funilaria-engrenagem",at,.7f,lunaRoot,true);
   for(int i=0;i<g.hits;i++){var mark=GameObject.CreatePrimitive(PrimitiveType.Sphere);Destroy(mark.GetComponent<Collider>());mark.name="Marcador de golpe "+(i+1);mark.transform.SetParent(lunaRoot,false);mark.transform.position=at+new Vector3((i-1)*.12f,.55f,0);mark.transform.localScale=Vector3.one*.085f;mark.GetComponent<Renderer>().sharedMaterial=coinMaterial;}
  }
  // Teia da Viúva (autor 24/10): a arte de impacto da Teia deitada em cada casa, translúcida.
  var web=Resources.LoadAll<Sprite>("Sprites/Efeitos/teia/impacto").OrderBy(x=>x.name,StringComparer.Ordinal).FirstOrDefault();
  foreach(var w in s.Luna.webs) foreach(int i in w.cells??new int[0]) {
   var c=new Cell(i%s.width,i/s.width);if(!Board.Contains(s,c))continue;
   var go=new GameObject("Teia",typeof(SpriteRenderer));go.transform.SetParent(lunaRoot,false);go.transform.position=Position(s,c)+Vector3.up*.03f;go.transform.rotation=Quaternion.Euler(90,0,0);
   var sr=go.GetComponent<SpriteRenderer>();sr.sprite=web;sr.color=new Color(.92f,.92f,1,.6f);
   if(web)go.transform.localScale=Vector3.one*(.95f/Mathf.Max(.01f,web.bounds.size.x));
  }
  foreach(var f in s.fighters.Where(x=>x.Alive)) {
   if(!units.TryGetValue(f.id,out var actor)||!actor.gameObject.activeInHierarchy)continue;
   // Contador de engrenagens: fica ao lado da barra de vida (ArenaPresentation.UpdateBars).
   if(f.def=="celinos") {var radar=LunaSprite("celinos-radar",actor.position+Vector3.up*.04f,1.3f,lunaRoot,true);if(radar){radar.GetComponent<SpriteRenderer>().color=new Color(1,1,1,.4f);// autor 23/10: menor e discreto
    var a=radar.AddComponent<LunaAnchor>();a.target=actor;a.offset=Vector3.up*.04f;}}
  }
  foreach(var mark in s.Luna.marks)if(units.TryGetValue(mark.id,out var marked)&&marked.gameObject.activeInHierarchy&&s.F(mark.id)?.Alive==true)LunaLabel("ALVO",marked.position+Vector3.up*2,Red,lunaRoot,marked);
  foreach(var machine in s.Luna.machines){var box=machine.box;if(box.hp<=0||!units.TryGetValue(box.by,out var actor))continue;var go=LunaSprite("matriz-protecao",actor.position+Vector3.up*.8f,1.9f,lunaRoot);if(go){var a=go.AddComponent<LunaAnchor>();a.target=actor;a.offset=Vector3.up*.8f;}LunaLabel("Matriz "+box.hp,actor.position+Vector3.up*2.45f,Blue,lunaRoot,actor);}
  if(s.Luna.machines.Any(x=>x.chessTurns>0))BuildLunaChess(s);
 }
 void BuildLunaChess(MatchState s){
  var art=LunaArt("dominio-xadrez");if(art.Length==0)return;
  if(!lunaChessMaterial)lunaChessMaterial=Material(Color.white,Resources.Load<Texture2D>("Textures/tile_xadrez")??art[0].texture);
  var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
  for(int z=0;z<s.height;z++)for(int x=0;x<s.width;x++){
   var at=Position(s,new Cell(x,z))+Vector3.up*.003f;int i=vertices.Count;
   vertices.AddRange(new[]{at+new Vector3(-.5f,0,-.5f),at+new Vector3(.5f,0,-.5f),at+new Vector3(.5f,0,.5f),at+new Vector3(-.5f,0,.5f)});
   uv.AddRange(new[]{new Vector2(x*.5f,z*.5f),new Vector2((x+1)*.5f,z*.5f),new Vector2((x+1)*.5f,(z+1)*.5f),new Vector2(x*.5f,(z+1)*.5f)});
   triangles.AddRange(new[]{i,i+2,i+1,i,i+3,i+2});
  }
  lunaChessMesh=new Mesh{name="Xadrez sobre o relevo"};lunaChessMesh.SetVertices(vertices);lunaChessMesh.SetUVs(0,uv);lunaChessMesh.SetTriangles(triangles,0);lunaChessMesh.RecalculateNormals();lunaChessMesh.RecalculateBounds();
  var go=new GameObject("Tabuleiro do Desland",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(lunaRoot,false);go.GetComponent<MeshFilter>().sharedMesh=lunaChessMesh;go.GetComponent<MeshRenderer>().sharedMaterial=lunaChessMaterial;
 }

 bool HandlesLuna(GameEvent e) => e.kind=="skill"&&LunaEvents.Contains(e.info??"")||e.kind=="status"&&LunaEvents.Contains(e.info??"")||e.kind=="mover"&&(e.info=="puxao-blipware"||e.info=="saida-matriz");
 IEnumerator AnimateLuna(GameEvent e,MatchState s) {
  units.TryGetValue(e.actor,out var actor);units.TryGetValue(e.target,out var target);var at=Board.Contains(s,e.cell)?Position(s,e.cell)+Vector3.up*.5f:actor?actor.position+Vector3.up*.7f:Vector3.zero;
  if(e.kind=="mover"&&e.path!=null&&e.path.Length>1){
   if(e.info=="puxao-blipware"&&actor&&target){PlayClip(actor,"cast");var hand=LunaSprite("puxao-blipware",target.position+Vector3.up*.6f,.8f,effectRoot);var old=target.position;var to=Spot(s,s.F(e.target),e.cell);float t=0;while(t<.55f){t+=Time.deltaTime;float k=Mathf.Clamp01(t/.55f);target.position=Vector3.Lerp(old,to,k);if(hand)hand.transform.position=target.position+Vector3.up*.6f;yield return null;}if(hand)Destroy(hand);}
   else if(actor){var from=Position(s,e.path[0]);var to=Spot(s,s.F(e.actor),e.cell);float t=0;while(t<.7f){t+=Time.deltaTime;float k=Mathf.Clamp01(t/.7f);actor.position=Vector3.Lerp(from,to,k)+Vector3.up*(Mathf.Sin(k*Mathf.PI)*2f);yield return null;}}
   yield break;
  }
  string id=e.info;
  if(id=="comando-celinos") {lunaRevision=-1;yield break;}
  if(id=="blip-loja") Game.Hud?.OpenRemoteShop(e.actor);
  if(id=="teia-some"||id=="viva-luna-fim"||id=="dominio-xadrez-fim"||id=="matriz-protecao-fim"||id=="funilaria-fim"||id=="funilaria-golpe"||id=="matriz-dano") {lunaRevision=-1;yield break;}
  if(id=="nepturius-esfera"||id=="loucura-negra"||id=="esfera-orfa")id="aura-nepturius";
  // Cubo (dano numa casa): pulso escuro de uma casa; o estilhaço grande saiu (autor 24/10).
  if(id=="nepturius-cubo")id="aura-nepturius";
  if(id=="funilaria")id="funilaria-engrenagem";
  if(actor&&e.kind=="skill")PlayClip(actor,"cast");
  // Agarrão (autor 24/10): as mãos saem da esfera e se esticam até cada alvo.
  if(id=="agarrao-negro"&&target&&actor){var hand=LunaSprite(id,actor.position+Vector3.up*.65f,1.6f,effectRoot);if(hand){var from=hand.transform.position;var to=target.position+Vector3.up*.65f;float t=0;while(t<.3f&&hand){t+=Time.deltaTime;hand.transform.position=Vector3.Lerp(from,to,Mathf.Clamp01(t/.3f));yield return null;}if(hand)Destroy(hand,1.7f);}if(e.value!=0)Float(target.position+Vector3.up*1.8f,e.value.ToString(),Blue);yield return Wait(.2f);yield break;}
  if(id=="agarrao-negro"&&target)at=target.position+Vector3.up*.65f;
  var fx=LunaSprite(id,at,id=="viva-luna"?3.5f:id=="explosao-antimateria"?3f:1.6f,effectRoot);
  if(fx)Destroy(fx,id=="agarrao-negro"?2f:.9f);
  if(e.value!=0&&target)Float(target.position+Vector3.up*1.8f,e.info=="matriz-dano"?"Matriz "+e.value:e.value.ToString(),e.info=="aura-nepturius"?Violet:Blue);
  yield return Wait(.35f);
 }
 void BuildDocksBoard(MatchState s) {
  boardBase.SetActive(true);var water=Resources.Load<Texture2D>("Textures/tile_agua");var wood=Resources.Load<Texture2D>("Textures/tile_docas");
  var wet=Material(new Color(.22f,.7f,.87f),water);var dry=Material(new Color(.62f,.46f,.31f),wood);var stone=Material(new Color(.22f,.3f,.36f));var groups=new[]{new List<CombineInstance>(),new List<CombineInstance>(),new List<CombineInstance>()};
  void Add(int g,Vector3 p,Vector3 size){groups[g].Add(new CombineInstance{mesh=cubeMesh,transform=Matrix4x4.TRS(p,Quaternion.identity,size)});}
  for(int x=0;x<s.width;x++)for(int z=0;z<s.height;z++){var c=new Cell(x,z);var t=s.At(c);var at=Position(s,c);bool flow=t.terrain==Terrain.Agua;float h=at.y+.25f;Add(flow?0:1,new Vector3(at.x,at.y-h*.5f,at.z),new Vector3(.985f,h,.985f));if(t.terrain==Terrain.Caixa)Add(1,at+Vector3.up*.42f,new Vector3(.8f,.84f,.8f));if(t.terrain==Terrain.Arvore||t.terrain==Terrain.Parede)Add(2,at+Vector3.up*.75f,new Vector3(.65f,1.5f,.65f));}
  float edge=s.width*.5f+.25f;
  foreach(int side in new[]{-1,1}){Add(2,new Vector3(0,.5f,side*edge),new Vector3(s.width+1,1.2f,.5f));Add(2,new Vector3(side*edge,.5f,0),new Vector3(.5f,1.2f,s.height+1));}
  var mats=new[]{wet,dry,stone};for(int i=0;i<groups.Length;i++){if(groups[i].Count==0)continue;var mesh=new Mesh{name="Docas "+i,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.CombineMeshes(groups[i].ToArray());generatedMeshes.Add(mesh);var go=new GameObject(mesh.name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(boardRoot,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=mats[i];if(i==0)go.AddComponent<LunaWaterFlow>().material=wet;}
  LunaLabel("Água: movimento pela metade (mín. 1)",new Vector3(0,.2f,-s.height*.5f-1.1f),Blue,boardRoot,null,.035f);
  // Loja no meio das docas também (autor 24/10): a mesma barraca do mascate dos mapas abertos.
  var shop=MapGen.ShopCells(s);
  if(shop.All(c=>s.At(c).terrain==Terrain.Loja)){var center=shop.Aggregate(Vector3.zero,(a,c)=>a+Position(s,c))/shop.Length;center.y=shop.Max(c=>s.At(c).height)*HStep;PlaceKit(0,"loja_mascate",center,180,boardRoot);}
 }
}
sealed class LunaFacing:MonoBehaviour {public Camera camera;void LateUpdate(){if(camera)transform.rotation=camera.transform.rotation;}}
sealed class LunaAnchor:MonoBehaviour {public Transform target;public Vector3 offset;void LateUpdate(){if(target)transform.position=target.position+offset;}}
sealed class LunaSpriteLoop:MonoBehaviour {public Sprite[] frames;public float fps=10;SpriteRenderer sr;void Awake(){sr=GetComponent<SpriteRenderer>();}void Update(){if(sr&&frames!=null&&frames.Length>0)sr.sprite=frames[(int)(Time.time*fps)%frames.Length];}}
sealed class LunaWaterFlow:MonoBehaviour {public Material material;void Update(){if(material)material.mainTextureOffset=new Vector2(Time.time*.08f,Time.time*.19f);}}
}
