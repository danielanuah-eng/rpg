using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using ArenaTatica.Core;

namespace ArenaTatica
{
    // Execução automática opcional num player compilado. Nunca roda no jogo normal.
    public sealed partial class ArenaSmoke : MonoBehaviour
    {
        ArenaGame game;
        // Casa vizinha alcançável com 1 PM (o mapa é sorteado, então não dá para fixar a casa).
        Cell Step(Fighter f)
        {
            var costs=Board.Costs(game.State,f);
            for(int i=0;i<costs.Length;i++) if(costs[i]==1) return new Cell(i%game.State.width,i/game.State.width);
            return f.cell;
        }
        IEnumerator Idle(float limit=10) { float end=Time.realtimeSinceStartup+limit; while(game.View.Busy&&Time.realtimeSinceStartup<end) yield return null; yield return new WaitForSeconds(.3f); }
        IEnumerator Confirmed(System.Action submit)
        {
            int revision=game.State.revision; submit();
            float end=Time.realtimeSinceStartup+5; while(game.State.revision==revision&&Time.realtimeSinceStartup<end) yield return null;
            yield return Idle();
        }
        IEnumerator MoveAndPass()
        {
            var f=game.Active; var to=Step(f);
            // No cliente o estado volta pela rede: espera a resposta do host antes de esperar a animação.
            yield return Confirmed(()=>game.Submit("mover",to));
            yield return Confirmed(()=>game.Submit("passar"));
        }
        IEnumerator Start()
        {
            game=GetComponent<ArenaGame>();
            yield return new WaitForSeconds(2);
            if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--arena-qa-render")>=0) {
                Screen.SetResolution(1280,720,false);
                // O runner deste lote pode terminar o splash para capturar com a janela oculta.
                if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--arena-luna-qa")>=0||System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--arena-somnax-qa")>=0)UnityEngine.Rendering.SplashScreen.Stop(UnityEngine.Rendering.SplashScreen.StopBehavior.StopImmediate);
                float splashDeadline=Time.realtimeSinceStartup+20;
                while(!UnityEngine.Rendering.SplashScreen.isFinished&&Time.realtimeSinceStartup<splashDeadline) yield return null;
                if(!UnityEngine.Rendering.SplashScreen.isFinished) { Finish(false,"Splash não terminou para a captura de interface"); yield break; }
                yield return new WaitForSeconds(.5f);
            }
            if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--arena-somnax-qa")>=0){yield return SomnaxGuardaQa();yield break;}
            if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--arena-luna-qa")>=0){yield return InventoresLunaQa();yield break;}
            Debug.Log($"ARENA_SMOKE_FIXTURE: {game.Network.Role}, {game.Turns}, ativo {game.Active?.def}, time salvo isolado");
            if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--arena-ux-qa")>=0) { yield return InteractionQa(); yield break; }
            if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--arena-effects-qa")>=0) { yield return EffectsQa(); yield break; }
            if((Application.isBatchMode||System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--arena-qa-render")>=0)&&System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--arena-release-qa")>=0) { yield return ReleaseQa(); yield break; }
            if(game.Network.IsHost&&System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--arena-relay-report")>=0) {
                float roomDeadline=Time.realtimeSinceStartup+40;
                while(string.IsNullOrEmpty(game.Network.RoomCode)&&Time.realtimeSinceStartup<roomDeadline) yield return null;
                if(string.IsNullOrEmpty(game.Network.RoomCode)) { Finish(false,"Relay: "+game.Status); yield break; }
                Directory.CreateDirectory("Artifacts"); File.WriteAllText("Artifacts/019-relay-code.txt",game.Network.RoomCode);
            }
            if(game.Network.IsHost) {
                float deadline=Time.realtimeSinceStartup+25;
                while(!game.Network.IsConnected&&Time.realtimeSinceStartup<deadline) yield return null;
                if(!game.Network.IsConnected) { Finish(false,"Cliente não conectou"); yield break; }
                yield return new WaitForSeconds(1);
                var guest=game.State.fighters.Find(x=>x.team==2&&x.summoner==0); var start=guest.cell;
                yield return MoveAndPass();
                deadline=Time.realtimeSinceStartup+40;
                while((game.Active?.team!=1||game.State.round<2)&&Time.realtimeSinceStartup<deadline) yield return null;
                Finish(game.State.round>=2&&game.State.F(guest.id).cell!=start,$"Host: turno próprio, comando do cliente e volta do turno (rodada {game.State.round}, ativo P{game.Active?.team}, convidado {start.x},{start.z}→{game.State.F(guest.id)?.cell.x},{game.State.F(guest.id)?.cell.z}, status {game.Status})");
            } else if(game.Client) {
                float deadline=Time.realtimeSinceStartup+25;
                while((!game.Synced||game.Active?.team!=2)&&Time.realtimeSinceStartup<deadline) yield return null;
                if(!game.Synced||game.Active?.team!=2) { Finish(false,"Sem turno sincronizado"); yield break; }
                yield return new WaitForSeconds(.5f);
                var me=game.Active; var to=Step(me);
                yield return MoveAndPass();
                yield return new WaitForSeconds(2);
                Finish(game.State.F(me.id).cell==to&&game.State.rng==0,"Cliente: comando autoritativo e estado filtrado");
            } else if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--arena-shots")>=0) {
                yield return Shots();
            } else {
                Physics.SyncTransforms();
                var me=game.Active;
                var screen=game.View.BoardCamera.WorldToScreenPoint(ArenaPresentation.Position(game.State,me.cell)+Vector3.up*.8f);
                if(!game.View.Pick(screen,out _,out int picked)||picked!=me.id) { Finish(false,"Seleção da miniatura por raycast"); yield break; }
                // Loja: moedas de teste (o jogo local tem autoridade) e compra do slot 0.
                // Compra só ao lado da loja: uma casa vizinha vira loja durante a compra.
                game.State.coins[me.team]=10; var item=Shop.Def(game.State.shops[me.team].slots[0]);
                Tile next=null; foreach(var d in Board.Dirs) if(next==null&&Board.Contains(game.State,me.cell+d)) next=game.State.At(me.cell+d);
                var ground=next.terrain; next.terrain=ArenaTatica.Core.Terrain.Loja;
                game.Submit("comprar",slot:0); yield return Idle();
                next.terrain=ground; bool bought=game.State.coins[me.team]==10-item.price&&me.bought;
                // Skill: área de alcance desenhada e invocação numa casa válida.
                Directory.CreateDirectory("Artifacts");
                game.Mode="invocacao-dragao"; yield return new WaitForSeconds(.5f);
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/arena-skill.png")); yield return new WaitForSeconds(.5f);
                var sk=Content.Skill(me,"invocacao-dragao"); Cell spot=me.cell;
                foreach(var d in Board.Dirs) if(game.Preview.CanTarget(me,sk,me.cell+d,out _)) { spot=me.cell+d; break; }
                game.Submit("skill",spot,"invocacao-dragao"); yield return Idle();
                bool summoned=game.State.fighters.Exists(x=>x.summoner==me.id&&x.Alive);
                var to=Step(me);
                yield return MoveAndPass();
                bool moved=game.State.F(me.id).cell==to;
                // Depois da Ciantia age o dragão invocado; passa a vez dele.
                if(game.Active?.summoner==me.id) { game.Submit("passar"); yield return Idle(); }
                bool switched=game.Active!=null&&game.Active.team!=me.team;
                yield return MoveAndPass();
                string capture=Path.GetFullPath("Artifacts/arena-unity.png");
                ScreenCapture.CaptureScreenshot(capture); yield return new WaitForSeconds(1);
                if(!bought||!summoned||!moved||!switched) { Finish(false,$"Local: compra {bought}, invocação {summoned}, movimento {moved}, troca de time {switched}"); yield break; }
                Finish(game.State.round==2&&File.Exists(capture),"Local: seleção, compra, skill, movimento, turnos, rodada e captura");
            }
        }
        // Capturas para conferência visual (--arena-shots): um inimigo é posto ao lado do ativo, que ataca;
        // fotos durante a rolagem dos dados, na face final e no impacto (Artifacts/shots-<largura>x<altura>-<n>.png).
        IEnumerator Shots()
        {
            var me=game.Active; var foe=game.State.fighters.Find(x=>x.team!=me.team&&x.Alive&&x.summoner==0);
            var costs=Board.Costs(game.State,me); SkillDef attack=null; Cell spot=me.cell;
            for(int i=0;i<costs.Length&&attack==null;i++) {
                if(costs[i]!=1) continue; var c=new Cell(i%game.State.width,i/game.State.width); var old=foe.cell; foe.cell=c;
                foreach(var sk in Content.Def(me).skills) if(sk.damage>0&&Content.Skill(me,sk.id)!=null&&game.Preview.CanTarget(me,sk,c,out _)) { attack=sk; spot=c; break; }
                if(attack==null) foe.cell=old;
            }
            if(attack==null) { Finish(false,"Capturas: nenhum ataque possível"); yield break; }
            game.View.Sync(game.State,true); yield return new WaitForSeconds(1);
            Directory.CreateDirectory("Artifacts"); string prefix=Path.GetFullPath($"Artifacts/shots-{Screen.width}x{Screen.height}-");
            ScreenCapture.CaptureScreenshot(prefix+"0.png"); yield return new WaitForSeconds(.3f);
            game.Submit("skill",spot,attack.id);
            float start=Time.time; int n=1;
            foreach(float at in new[]{.25f,.5f,.8f,1.1f,1.4f,1.8f,2.4f}) { while(Time.time-start<at) yield return null; ScreenCapture.CaptureScreenshot(prefix+(n++)+".png"); }
            yield return new WaitForSeconds(1);
            Finish(true,$"Capturas: {me.def} ataca {foe.def} com {attack.id}");
        }
        object InvokeGame(string name,params object[] args) => typeof(ArenaGame).GetMethod(name,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(game,args);
        IEnumerator QaWait(System.Func<bool> ready,float seconds=25) { float end=Time.realtimeSinceStartup+seconds; while(!ready()&&Time.realtimeSinceStartup<end) yield return null; }
        IEnumerator ReleaseQa()
        {
            var args=System.Environment.GetCommandLineArgs(); int ix=System.Array.IndexOf(args,"--arena-qa-prefix");
            string prefix=ix>=0&&ix+1<args.Length?args[ix+1]:Path.GetFullPath("Artifacts/019-qa");
            Directory.CreateDirectory(Path.GetDirectoryName(prefix));
            bool cup=System.Array.IndexOf(args,"--arena-qa-cup")>=0; int players=game.Seats;
            game.AutoStart=false; game.Turns=TurnMode.Alternada; game.SetTournament(cup); game.OpenLobby();
            var initial=game.State; game.StartRoom();
            if(!game.Setup||game.Lobby||game.State!=initial) { Finish(false,"Configuração iniciou a partida antes da liberação"); yield break; }
            if(players==2&&!cup) { ScreenCapture.CaptureScreenshot(prefix+"-configurar.png"); yield return new WaitForSeconds(.3f); }
            game.Release(false);
            if(!game.Lobby||!game.Network.IsHost) { Finish(false,"Liberar LAN: "+game.Status); yield break; }
            File.WriteAllText(prefix+".ready","LAN");
            yield return QaWait(()=>game.Filled==players,60);
            if(game.Filled!=players) { Finish(false,"Não chegaram todos os participantes"); yield break; }
            if(players==2&&!cup) { ScreenCapture.CaptureScreenshot(prefix+"-sala.png"); yield return new WaitForSeconds(.3f); }
            game.StartRoom();
            if(cup) {
                int played=0;
                while(game.Torneio.champion==0&&played<players) {
                    float end=Time.realtimeSinceStartup+25;
                    while(game.Torneio.phase!=""&&Time.realtimeSinceStartup<end) {
                        var t=game.Torneio;
                        if(t.a==1||t.b==1) {
                            var choices=t.phase=="ban"?t.bans:t.picks;
                            if(!choices.ContainsKey(1)) InvokeGame("DraftChoice",1,t.phase,t.phase=="ban"?game.DraftOptions(1).Take(2).ToArray():ArenaGame.ValidTeam(game.DraftOptions(1)));
                        }
                        yield return null;
                    }
                    if(game.Torneio.phase!=""||game.Lobby) { Finish(false,"Draft remoto não completou"); yield break; }
                    // Fixture: os combates já têm cobertura própria. Aqui validamos chave/draft/transporte.
                    game.State.over=true; game.State.winner=1; InvokeGame("RecordTournament"); played++; game.NextTournamentMatch();
                    yield return new WaitForSeconds(.2f);
                }
                bool passed=played==players-1&&game.Torneio.champion>0;
                File.WriteAllText(prefix+".result",passed?$"cup:{players};matches:{played};champion:{game.Torneio.champion}":"FAIL");
                Finish(passed,$"Torneio LAN {players}: draft remoto, {played} partidas de fixture, campeão"); yield break;
            }
            if(game.Lobby||game.State.players!=players) { Finish(false,"Iniciar não entregou o estado da sala"); yield break; }
            yield return Idle();
            // Fixture opt-in e somente batch: usa Execute/Match.Apply reais para movimento e contato.
            var s=game.State; var hidden=s.fighters.First(f=>f.team==1&&f.summoner==0); var visitor=s.fighters.First(f=>f.team==2&&f.summoner==0);
            var reserved=new[]{new Cell(2,2),new Cell(2,3),new Cell(4,3),new Cell(3,3)};
            foreach(var f in s.fighters.Where(f=>f!=hidden&&f!=visitor&&reserved.Contains(f.cell))) {
                f.cell=Enumerable.Range(0,s.tiles.Length).Select(i=>new Cell(i%s.width,i/s.width)).First(c=>!reserved.Contains(c)&&Board.At(s,c)==null&&Board.Walkable(s,c,false));
            }
            foreach(var c in reserved) { s.At(c).terrain=ArenaTatica.Core.Terrain.Livre; s.At(c).height=0; s.At(c).coins=0; s.At(c).trap=0; }
            hidden.cell=reserved[0]; hidden.hidden=true; hidden.pm=2; visitor.cell=reserved[2]; visitor.pm=2;
            s.active=s.timeline.IndexOf(hidden.id);
            InvokeGame("Execute",new Command{kind="mover",fighter=hidden.id,revision=s.revision,cell=reserved[1]},1,-1);
            yield return QaWait(()=>File.Exists(prefix+".reconnected"));
            if(!File.Exists(prefix+".reconnected")) { Finish(false,"Cliente não confirmou o snapshot secreto após reconexão"); yield break; }
            yield return Idle(); s.active=s.timeline.IndexOf(visitor.id);
            InvokeGame("Execute",new Command{kind="mover",fighter=visitor.id,revision=s.revision,cell=reserved[3]},2,-1);
            yield return QaWait(()=>File.Exists(prefix+".revealed"));
            if(hidden.hidden||!File.Exists(prefix+".revealed")) { Finish(false,"Contato não revelou pelo protocolo"); yield break; }
            yield return Idle();
            if(players==2) {
                ScreenCapture.CaptureScreenshot(prefix+"-tabuleiro.png"); yield return new WaitForSeconds(.3f);
                game.Hud.Inspect(visitor); ScreenCapture.CaptureScreenshot(prefix+"-ficha.png"); yield return new WaitForSeconds(.3f);
                game.Hud.OpenPicker(); ScreenCapture.CaptureScreenshot(prefix+"-selecao.png"); yield return new WaitForSeconds(.3f);
                Screen.SetResolution(960,540,false); yield return new WaitForSeconds(.5f);
                ScreenCapture.CaptureScreenshot(prefix+"-selecao-960.png"); yield return new WaitForSeconds(.3f);
            }
            game.RestartRoom(); yield return QaWait(()=>File.Exists(prefix+".restarted"));
            game.BackToLobby(); yield return QaWait(()=>File.Exists(prefix+".lobby"));
            game.StartRoom(); yield return QaWait(()=>File.Exists(prefix+".again"));
            bool complete=File.Exists(prefix+".restarted")&&File.Exists(prefix+".lobby")&&File.Exists(prefix+".again")&&game.State.players==players;
            File.WriteAllText(prefix+".result",complete?$"room:{players};invisible:pass;reconnect:pass;restart:pass":"FAIL");
            Finish(complete,$"Sala LAN {players}: configurar/liberar/iniciar, invisibilidade, reconexão, reinício, voltar à sala");
        }
        IEnumerator EffectsQa()
        {
            game.Menu?.Close(); game.VsBots=false;
            game.StartLocal(new[]{new[]{"cheiro-pneu"},new[]{"ciantia"}}); yield return Idle();
            var s=game.State; var f=s.fighters.First(x=>x.team==1);
            var totem=new Fighter { id=s.fighters.Max(x=>x.id)+1,def="totem-de-cura",team=1,hp=4,cell=f.cell };
            s.fighters.Add(totem); f.lastSkill="erguer-totem";
            var events=new System.Collections.Generic.List<GameEvent>{
                new GameEvent {kind="forma",actor=f.id,info="motor-quente",cell=f.cell},
                new GameEvent {kind="esconder",actor=f.id,info="bomba-de-fumaca",cell=f.cell},
                new GameEvent {kind="armadilha-posta",actor=f.id,info="mina-de-gelo",cell=f.cell},
                new GameEvent {kind="invocar",actor=f.id,target=totem.id,cell=totem.cell},
                new GameEvent {kind="cura",actor=totem.id,target=f.id,info="totem-de-cura",value=1,cell=f.cell},
                new GameEvent {kind="zona",actor=f.id,info="chao-de-gelo",cell=f.cell,path=new[]{f.cell}}
            };
            foreach(string id in new[]{"cristal-de-foco","ancora","manto-de-espinhos","bencao-de-pedra"}) events.Add(new GameEvent {kind="compra",actor=f.id,info=id});
            game.View.Play(events,s); yield return Idle(20);
            var cache=(System.Collections.Generic.Dictionary<string,Sprite[]>)typeof(ArenaPresentation).GetField("effectCache",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).GetValue(null);
            foreach(string phase in new[]{"aquecer-motor/conjuracao","aquecer-motor/impacto","bomba-de-fumaca/uso","mina-de-gelo/uso","totem-de-cura/uso","chao-de-gelo/conjuracao","chao-de-gelo/impacto","cristal-de-foco/uso","ancora/uso","manto-de-espinhos/uso","bencao-de-pedra/uso"}) {
                if(!cache.TryGetValue(phase,out var frames)||frames==null||frames.Length==0) { Finish(false,"Fase não tocada: "+phase); yield break; }
                Debug.Log("ARENA_EFFECT_PHASE: "+phase+" quadros="+frames.Length);
            }
            Finish(!game.View.Busy,"Efeitos: 11 fases tocadas pelos eventos no player, sem exceções");
        }
        IEnumerator InteractionQa()
        {
            game.Menu?.Close(); game.VsBots=false; game.NewLocal(); yield return Idle();
            var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
            var choose=typeof(ArenaHud).GetMethod("ChooseSkill",flags);
            var pinch=typeof(ArenaPresentation).GetMethod("Pinch",flags);
            var hud=game.Hud; var f=game.Active; var skills=Match.Skills(f);
            int at=skills.FindIndex(sk=>sk.kind==SkillKind.Dano&&!sk.selfArea); var attack=skills[at];
            int revision=game.State.revision, pa=f.pa;
            choose.Invoke(hud,new object[]{at}); yield return null;
            if(game.State.revision!=revision||f.pa!=pa||game.Mode!=""||!hud.HasPendingConfirmation) { Finish(false,"Primeiro toque de skill agiu"); yield break; }
            choose.Invoke(hud,new object[]{at}); yield return null;
            if(game.State.revision!=revision||game.Mode!=attack.id||hud.HasPendingConfirmation) { Finish(false,"Segundo toque não armou a mira"); yield break; }
            var empty=Enumerable.Range(0,game.State.tiles.Length).Select(i=>new Cell(i%game.State.width,i/game.State.width)).First(c=>Board.At(game.State,c)==null&&game.Preview.CanTarget(f,attack,c,out _));
            game.Match.Forced.Enqueue(Face.Trava); game.Match.Forced.Enqueue(Face.Trava);
            game.Submit("skill",empty,attack.id); yield return Idle();
            if(game.State.revision!=revision+1||f.pa!=pa-attack.pa) { Finish(false,"Ataque no chão não consumiu custo correto"); yield break; }
            revision=game.State.revision; hud.ConfirmPass(); yield return null;
            if(game.State.revision!=revision||!hud.HasPendingConfirmation) { Finish(false,"Primeiro toque de Passar agiu"); yield break; }
            hud.ConfirmPass(); yield return Idle();
            if(game.State.revision!=revision+1) { Finish(false,"Segundo toque não passou"); yield break; }
            // Método de câmera real: variação da distância aproxima; translação/ângulo constantes em distância não giram/panam.
            var rect=ArenaHud.BoardArea(); var center=rect.center; var half=new Vector2(50,0);
            typeof(ArenaPresentation).GetField("pressOnUi",flags).SetValue(game.View,false);
            typeof(ArenaPresentation).GetField("pinching",flags).SetValue(game.View,false);
            pinch.Invoke(game.View,new object[]{center-half,center+half});
            float before=game.View.BoardCamera.orthographicSize;
            pinch.Invoke(game.View,new object[]{center-half*1.4f,center+half*1.4f});
            float zoom=game.View.BoardCamera.orthographicSize; var position=game.View.BoardCamera.transform.position;
            float yaw=(float)typeof(ArenaPresentation).GetField("yawGoal",flags).GetValue(game.View);
            pinch.Invoke(game.View,new object[]{center+new Vector2(10,-70),center+new Vector2(10,70)});
            revision=game.State.revision; hud.ConfirmPass();
            if(hud.HasPendingConfirmation||game.State.revision!=revision) { Finish(false,"Pinça acionou botão de turno"); yield break; }
            if(zoom>=before||Mathf.Abs(game.View.BoardCamera.orthographicSize-zoom)>.001f||Vector3.Distance(position,game.View.BoardCamera.transform.position)>.001f||(float)typeof(ArenaPresentation).GetField("yawGoal",flags).GetValue(game.View)!=yaw) { Finish(false,"Pinça conflitou com giro/pan"); yield break; }
            Directory.CreateDirectory("Artifacts");
            game.View.ResetView(game.State); yield return Idle();
            Canvas.ForceUpdateCanvases();
            var faces=(System.Collections.Generic.List<UnityEngine.UI.Image>)typeof(ArenaHud).GetField("timelineFaces",flags).GetValue(hud);
            var content=(RectTransform)typeof(ArenaHud).GetField("timelineContent",flags).GetValue(hud);
            var faceCorners=new Vector3[4]; var viewportCorners=new Vector3[4];
            if(faces.Count==0||faces[0].sprite==null) { Finish(false,"Iniciativa sem retrato"); yield break; }
            faces[0].rectTransform.GetWorldCorners(faceCorners); ((RectTransform)content.parent).GetWorldCorners(viewportCorners);
            var faceRect=new Rect(faceCorners[0],faceCorners[2]-faceCorners[0]); var visibleRect=new Rect(viewportCorners[0],viewportCorners[2]-viewportCorners[0]);
            if(!visibleRect.Contains(faceRect.min)||!visibleRect.Contains(faceRect.max)) { Finish(false,"Retrato da iniciativa cortado pela lista"); yield break; }
            if(!BoardFits()) { Finish(false,"Tabuleiro cortado em paisagem"); yield break; }
            ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/020-ux-landscape.png")); yield return new WaitForSeconds(.4f);
            Screen.SetResolution(540,960,false); yield return new WaitForSeconds(.7f);
            if(!BoardFits()) { Finish(false,"Tabuleiro cortado em retrato"); yield break; }
            ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/020-ux-portrait.png")); yield return new WaitForSeconds(.4f);
            game.Menu.Open("opcoes"); yield return new WaitForSeconds(.3f);
            ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/020-opcoes-portrait.png")); yield return new WaitForSeconds(.4f);
            game.Menu.Close(); Screen.SetResolution(1280,720,false); yield return new WaitForSeconds(.5f);
            game.SetSeats(6); game.SetRandomizer(true,true,false,true);
            var team=ArenaGame.ValidTeam(Content.Roster.OrderBy(id=>Content.Fighters[id].level));
            game.StartLocal(Enumerable.Range(0,6).Select(_=>team.ToArray()).ToArray()); yield return Idle(); Canvas.ForceUpdateCanvases();
            var cards=(System.Collections.Generic.List<RectTransform>)typeof(ArenaHud).GetField("timelineCards",flags).GetValue(hud);
            var scroll=(UnityEngine.UI.ScrollRect)typeof(ArenaHud).GetField("timelineScroll",flags).GetValue(hud);
            if(game.State.fighters.Count(fighter=>fighter.Alive&&fighter.summoner==0)<24||content.rect.width<=((RectTransform)content.parent).rect.width||cards.Count<24||!BoardFits()) { Finish(false,"Iniciativa completa de seis jogadores não coube na rolagem"); yield break; }
            scroll.horizontalNormalizedPosition=1; yield return new WaitForSeconds(.3f);
            ScreenCapture.CaptureScreenshot(Path.GetFullPath("Artifacts/020-ux-six-players.png")); yield return new WaitForSeconds(.4f);
            game.CloseMatch(); if(!game.InMenu||game.CanAct) { Finish(false,"Fechar Partida deixou ações ativas"); yield break; }
            Finish(true,"UX: dois toques, ataque no vazio, pinça exclusiva, retrato/paisagem, fechar partida");
        }
        bool BoardFits()
        {
            var area=ArenaHud.BoardArea(); float half=game.State.width*.5f+.4f;
            for(int x=-1;x<=1;x+=2) for(int z=-1;z<=1;z+=2) for(int y=0;y<=1;y++) {
                var p=game.View.BoardCamera.WorldToScreenPoint(new Vector3(x*half,y*3.2f,z*half));
                if(!area.Contains(new Vector2(p.x,p.y))) return false;
            }
            return true;
        }
        // Espera 1 s antes de fechar: o último estado ainda precisa sair pela rede.
        void Finish(bool success,string message) { Debug.Log("ARENA_SMOKE_"+(success?"PASS: ":"FAIL: ")+message); StartCoroutine(Quit(success?0:1)); }
        IEnumerator Quit(int code) { yield return new WaitForSecondsRealtime(1); Application.Quit(code); }
    }
}
