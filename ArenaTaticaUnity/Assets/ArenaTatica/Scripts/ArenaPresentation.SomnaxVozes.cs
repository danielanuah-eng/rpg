using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArenaTatica.Core;
namespace ArenaTatica
{
    public sealed partial class ArenaPresentation
    {
        static readonly Dictionary<string,AudioClip> recordedLines=new Dictionary<string,AudioClip>();
        readonly Dictionary<int,AudioSource> characterVoices=new Dictionary<int,AudioSource>();
        readonly Dictionary<int,float> voiceTail=new Dictionary<int,float>();
        readonly Dictionary<int,int> voiceEpoch=new Dictionary<int,int>();
        readonly Dictionary<string,float> voiceLast=new Dictionary<string,float>();
        readonly HashSet<string> voicedBatch=new HashSet<string>();
        readonly Dictionary<int,GameObject> guardAreas=new Dictionary<int,GameObject>();
        int batchVoiceActor;float jackVoiceUntil;
        ArenaGlobalVoiceQueue globalVoiceQueue; int globalVoiceRevision=-1;
        readonly HashSet<string> globalVoiceSeen=new HashSet<string>();
        void ResetGlobalVoices() { if(globalVoiceQueue) globalVoiceQueue.Clear();globalVoiceSeen.Clear();globalVoiceRevision=-1; }
        void QueueGlobalVoices(List<GameEvent> events,MatchState state)
        {
            if(events==null||state==null||state.revision<globalVoiceRevision) return;
            if(state.revision!=globalVoiceRevision) { globalVoiceRevision=state.revision;globalVoiceSeen.Clear(); }
            for(int i=0;i<events.Count;i++) {
                var e=events[i];if(e.kind!="voz-global"||e.info!="primal_cacador"||!globalVoiceSeen.Add(e.actor+":"+i)) continue;
                if(!globalVoiceQueue) { var go=new GameObject("AnunciosDeVozGlobais");go.transform.SetParent(transform,false);globalVoiceQueue=go.AddComponent<ArenaGlobalVoiceQueue>(); }
                globalVoiceQueue.Enqueue(RecordedLine(e.info));
            }
        }
        void BeginVoiceBatch(int actor) { batchVoiceActor=actor;jackVoiceUntil=0;voicedBatch.Clear(); }
        public static AudioClip RecordedLine(string id)
        {
            if(!ArenaVoiceCatalog.Valid(id)) return null;
            if(!recordedLines.TryGetValue(id,out var clip)) recordedLines[id]=clip=Resources.Load<AudioClip>("Audio/Vozes/"+id);
            return clip;
        }
        float PlayLine(string id,int actor,bool throttle=false)
        {
            var clip=RecordedLine(id);if(!clip) return Time.unscaledTime;
            string key=actor+":"+id;
            if(voicedBatch.Contains(key)) return voiceTail.TryGetValue(actor,out float tail)?tail:Time.unscaledTime;
            if(throttle&&voiceLast.TryGetValue(key,out float last)&&Time.unscaledTime-last<4) return Time.unscaledTime;
            voicedBatch.Add(key);voiceLast[key]=Time.unscaledTime;
            float at=Mathf.Max(Time.unscaledTime,voiceTail.TryGetValue(actor,out float until)?until:0);
            voiceTail[actor]=at+clip.length;
            if(ArenaSom.Recorded) StartCoroutine(PlayScheduledLine(actor,clip,at,voiceEpoch.TryGetValue(actor,out int epoch)?epoch:0));
            return voiceTail[actor];
        }
        IEnumerator PlayScheduledLine(int actor,AudioClip clip,float at,int epoch)
        {
            float delay=at-Time.unscaledTime;if(delay>0) yield return new WaitForSecondsRealtime(delay);
            if((voiceEpoch.TryGetValue(actor,out int current)?current:0)!=epoch||!ArenaSom.Recorded) yield break;
            if(!characterVoices.TryGetValue(actor,out var source)||!source) {
                source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;source.volume=1;
                characterVoices[actor]=source;
            }
            source.clip=clip;source.Play();
        }
        void InterruptVoice(int actor)
        {
            voiceEpoch[actor]=(voiceEpoch.TryGetValue(actor,out int n)?n:0)+1;
            if(characterVoices.TryGetValue(actor,out var source)&&source) source.Stop();voiceTail[actor]=Time.unscaledTime;
        }
        void StopVoiceLines()
        {
            foreach(var source in characterVoices.Values) if(source) source.Stop();
            voiceTail.Clear();voiceEpoch.Clear();voiceLast.Clear();voicedBatch.Clear();jackVoiceUntil=0;ClearGuardAreas();
        }
        void EventVoice(GameEvent e,MatchState state)
        {
            var id=ArenaVoiceCatalog.ForEvent(e,state);
            if(id!=null) {
                int actor=e.kind=="nocaute"?e.target:e.actor;
                if(e.kind=="nocaute") InterruptVoice(actor);
                PlayLine(id,actor,id=="tchu_proteger_aliado");
            }
            if(e.kind=="cura"&&Content.BaseSkill(e.info)=="doce-e-salgado"&&units.TryGetValue(e.target,out var target)&&target)
                PhaseFx("doce-e-salgado/cura",target.position+Vector3.up*.7f);
        }
        static float RigFps(SpriteRig rig)
        {
            if(rig.folder=="Somnax") return rig.clip switch {"idle"=>8,"walk"=>10,"hit"=>12,"death"=>10,"attack"=>14,"pacto"=>14,"renuncia"=>14,"blinkout"=>12,"blinkin"=>12,"support"=>12,"ult"=>12,_=>Fps(rig.clip)};
            if(rig.folder=="TchuTchaGuarda"&&rig.clip=="idle") return 8;
            return Fps(rig.clip);
        }
        float StretchJackDraw(Transform actor,string skill,float nativeDuration)
        {
            if(skill!="flecha-abissal") return nativeDuration*.5f;
            float wait=ArenaVoiceCatalog.JackWindupSeconds(nativeDuration,jackVoiceUntil-Time.unscaledTime);
            if(actor&&actor.childCount>0&&rigs.TryGetValue(actor.GetChild(0),out var rig)&&wait>0) rig.rate=nativeDuration*ArenaVoiceCatalog.JackReleaseFraction/wait;
            return wait;
        }
        void RestoreClipRate(Transform actor) { if(actor&&actor.childCount>0&&rigs.TryGetValue(actor.GetChild(0),out var rig)) rig.rate=1; }
        static bool HandlesSomnaxVoiceEvent(GameEvent e)=>e!=null&&(e.kind=="voz-global"||e.kind=="voz-personagem"||e.kind=="guarda-direcional"||e.kind=="doce-salgado"||e.kind=="elo-duas-laminas");
        IEnumerator AnimateSomnaxVoiceEvent(GameEvent e,MatchState state)
        {
            if(e.kind=="voz-global") yield break;
            if(e.kind=="voz-personagem") { EventVoice(e,state);yield break; }
            units.TryGetValue(e.actor,out var actor);
            if(e.kind=="guarda-direcional") {
                Sync(state,false);
                if(units.TryGetValue(e.actor,out actor)&&actor) {
                    if(e.value==1) PlayLine("tchu_escudo",e.actor);
                    Float(actor.position,e.value==1?"Guarda direcional":"Guarda desativada",Gold);
                }
                yield return Wait(.2f);yield break;
            }
            if(e.kind=="elo-duas-laminas") {
                if(actor) PhaseFx("elo-duas-laminas/uso",actor.position+Vector3.up*.8f);
                if(units.TryGetValue(e.target,out var target)&&target) Float(target.position,"Elo +1",Gold);
                yield break;
            }
            if(e.kind=="doce-salgado"&&actor) {
                float duration=PlayClip(actor,"support");
                var frames=EffectFrames("doce-e-salgado/sorvete");if(frames!=null) StartCoroutine(PlayEffect(frames,actor.position+BoardCamera.transform.up*1.85f,4,.55f));
                yield return Wait(Mathf.Max(1,duration));
            }
        }
        IEnumerator RupturaGrid(GameEvent e,MatchState state,float cast)
        {
            var center=Board.Contains(state,e.aim)?e.aim:e.cell;
            PhaseFx("ruptura-das-memorias/conjuracao",Position(state,center)+Vector3.up*.02f);
            yield return Wait(Mathf.Max(.3f,cast*.5f));
            var frames=EffectFrames("ruptura-das-memorias/pilar");
            if(frames!=null) for(int x=-1;x<=1;x++) for(int z=-1;z<=1;z++) {
                var cell=new Cell(center.x+x,center.z+z);if(!Board.Contains(state,cell)) continue;
                float widest=.01f;foreach(var frame in frames) if(frame) widest=Mathf.Max(widest,frame.bounds.size.x);
                float initialWidth=.85f*frames[0].bounds.size.x/widest;
                StartCoroutine(PlayRupturaPillar(frames,Position(state,cell)+Vector3.up*.03f,initialWidth));
            }
            yield return Wait(ArenaEfeitos.ImpactTime("Sprites/Efeitos/ruptura-das-memorias/pilar",12));
        }
        IEnumerator PlayRupturaPillar(Sprite[] frames,Vector3 at,float width)
        {
            var go=Effect("PilarMemorias");var renderer=go.AddComponent<SpriteRenderer>();renderer.sharedMaterial=spriteMaterial;
            go.transform.position=at;go.transform.rotation=BoardCamera.transform.rotation;go.transform.localScale=Vector3.one*(width/Mathf.Max(.01f,frames[0].bounds.size.x));
            // Each native frame gets at least one display interval, including during a render hitch.
            for(int i=0;i<frames.Length;i++){renderer.sprite=frames[i];yield return new WaitForSeconds(i==2?.18f:1f/12);}
            transient.Remove(go);Destroy(go);
        }
        void ClearGuardAreas() { foreach(var area in guardAreas.Values) if(area) Destroy(area);guardAreas.Clear(); }
        void SyncDirectionalGuards(MatchState state)
        {
            ClearGuardAreas();
            foreach(var f in state.fighters) {
                if(units.TryGetValue(f.id,out var unit)&&unit&&unit.childCount>0&&rigs.TryGetValue(unit.GetChild(0),out var rig)) {
                    rig.guardLocked=false;
                    if(Match.GuardActive(f)) { var direction=new Vector3(f.guardDirection.x,0,f.guardDirection.z);Turn(unit,direction);rig.guardLocked=true; }
                }
                if(!Match.GuardActive(f)||Match.Concealed(state,Game.LocalTeam,f)||!Board.Contains(state,f.cell)) continue;
                var root=new GameObject("AreaGuarda_"+f.id);root.transform.SetParent(world,false);guardAreas[f.id]=root;
                var skill=Content.Skill(f,f.guardSkill??"guarda-direcional")??Content.AnySkill("guarda-direcional");int radius=Mathf.Max(0,skill?.area??2);
                for(int x=-radius;x<=radius;x++) for(int z=-radius;z<=radius;z++) {
                    var cell=new Cell(f.cell.x+x,f.cell.z+z);if(Math.Abs(x)+Math.Abs(z)>radius||!Board.Contains(state,cell)) continue;
                    Cube("Protecao",Position(state,cell)+Vector3.up*.025f,new Vector3(.86f,.012f,.86f),rangeMaterial,root.transform);
                }
                Cube("LadoDefendido",Position(state,f.cell)+new Vector3(f.guardDirection.x*.42f,.055f,f.guardDirection.z*.42f),new Vector3(.24f,.025f,.24f),targetMaterial,root.transform);
            }
        }
    }
}
