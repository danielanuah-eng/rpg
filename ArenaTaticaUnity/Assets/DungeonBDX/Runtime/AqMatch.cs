using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DungeonBDX.Arcadia;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

namespace DungeonBDX
{
    public sealed class AqMatch : MonoBehaviour
    {
        AqMatchMenu menu;AqJournal journal;AqLocalSave store;GameObject world;AqBoardView view;Canvas canvas;RectTransform safe;
        readonly DungeonCameraInput input=new DungeonCameraInput();
        readonly List<RaycastResult> hits=new List<RaycastResult>();
        int selected,card,lastW,lastH;string mode="move",pendingOperation;AqResult pendingResult;
        AudioSource music,sfx;bool muted;
        public AqSession Session=>journal?.Session;
        public AqBoardView View=>view;
        public Canvas HudCanvas=>canvas;
        public string Message {get;private set;}="";
        public bool IsOpen=>world&&world.activeSelf;
        public bool SavePaused=>pendingOperation!=null;
        public bool Open(AqMatchMenu owner,string scenario,string[][] parties,bool autoMonsters,bool resume,string storageRoot=null)
        {
            try
            {
                var nextStore=new AqLocalSave(storageRoot??AqUi.Root,scenario);var stored=nextStore.Load();
                if(resume&&stored==null){Message="Nenhuma partida salva neste cenário.";return false;}
                var next=resume?stored:new AqJournal(scenario,parties,Environment.GetCommandLineArgs().Contains("-dungeonbdx-qa")?29u:unchecked((uint)DateTime.UtcNow.Ticks),autoMonsters);
                if(!resume)nextStore.Save(next,Guid.NewGuid().ToString("N"));
                Cleanup();menu=owner;journal=next;store=nextStore;AqHeroImport.Apply(Session);input.Reset();
                world=new GameObject("Arcadia adventure");world.transform.SetParent(transform,false);view=world.AddComponent<AqBoardView>();view.Build(Session.Scenario,Session.Units);
                canvas=AqUi.Canvas(world.transform,"AQ bottom HUD");
                selected=Session.Units.First(u=>!u.Monster&&u.Guild==Session.CurrentGuild&&!u.Huge).Id;
                card=0;mode="move";Message=resume?"Partida restaurada.":Session.Scenario.Story;pendingOperation=null;pendingResult=null;
                SetupAudio();enabled=true;Draw();return true;
            }
            catch(Exception e){Message="Não foi possível abrir: "+e.Message;return false;}
        }
        void SetupAudio()
        {
            music=world.AddComponent<AudioSource>();music.loop=true;music.volume=.14f;
            string musicId=Session.Scenario.Id==AqScenarios.Ids[0]?"biblioteca-do-crepusculo":Session.Scenario.Id==AqScenarios.Ids[1]?"cripta-de-bronze":"ninho-do-eclipse";
            music.clip=Resources.Load<AudioClip>("DungeonBDX/Audio/Music/"+musicId);sfx=world.AddComponent<AudioSource>();sfx.volume=.3f;
            if(!FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Any(l=>l.enabled))view.Camera.gameObject.AddComponent<AudioListener>();
            if(Application.isPlaying&&music.clip)music.Play();
        }
        void Update()
        {
            if(!IsOpen)return;
            if(lastW!=Screen.width||lastH!=Screen.height){Draw();view.ResetCamera();}
            if(input.Read(view.ZoomCamera,view.RotateCamera,view.PanCamera,HitsHud,out var point)&&!SavePaused&&view.Pick(point,out var cell,out int unit))Tap(cell,unit);
        }
        // Compact footer: camera controls live in a slim column beside the table, so the board gets the room.
        static float FooterHeight=>Screen.width<Screen.height?.33f:.2f;
        bool HitsHud(Vector2 p)
        {
            Rect area=Screen.safeArea;float y=(p.y-area.y)/Mathf.Max(1,area.height);bool portrait=Screen.width<Screen.height;
            if(!area.Contains(p)||y<FooterHeight||y>.88f)return true;
            if(!EventSystem.current)return false;
            var data=new PointerEventData(EventSystem.current){position=p};hits.Clear();canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>().Raycast(data,hits);return hits.Count>0;
        }
        public void Tap(AqCell cell,int unitId=-1)
        {
            if(SavePaused||Session.Winner>=0)return;
            if(Session.Reaction!=null)
            {Do(new AqCommand(mode=="door"?"react-door":"react-move",x:cell.X,y:cell.Y));return;}
            var clicked=Session.Unit(unitId);var hero=Session.Unit(selected);
            if(mode=="transfer"&&clicked!=null){Do(new AqCommand("transfer",selected,unitId,card));mode="move";return;}
            if(mode=="revive"){Do(new AqCommand("revive",selected,x:cell.X,y:cell.Y));return;}
            if(mode=="attack"&&clicked!=null&&hero!=null&&Session.Enemy(hero,clicked)){Do(new AqCommand("attack",selected,unitId,card));return;}
            if(clicked!=null&&!clicked.Monster&&clicked.Guild==Session.CurrentGuild)
            {selected=unitId;card=0;mode=Session.Resting&&!clicked.Alive?"revive":"move";Draw();return;}
            if(mode=="door"||mode=="phase")Do(new AqCommand(mode,selected,x:cell.X,y:cell.Y));
            else if(mode=="move")Do(new AqCommand("move",selected,x:cell.X,y:cell.Y));
            else {Message="Selecione uma miniatura inimiga para atacar.";Draw();}
        }
        public AqResult Do(AqCommand command)
        {
            if(SavePaused)return new AqResult(false,"Salvamento pendente; tente salvar antes de continuar.");
            var result=Session.Apply(command);Message=result.Message;
            if(!result.Accepted){Draw();return result;}
            pendingOperation=Guid.NewGuid().ToString("N");pendingResult=result;SavePending();return result;
        }
        void SavePending()
        {
            if(pendingOperation==null)return;
            try
            {
                store.Save(journal,pendingOperation);pendingOperation=null;
                // Accepted domain commands are durable before audiovisual playback.
                AqHeroImport.Apply(Session);view.Apply(pendingResult);view.Sync(Session.Units,Session.Reaction?.Dying==true?Session.Reaction.Monster:-1);
                if(!muted&&pendingResult.Effects.Any(e=>e.Kind=="attack"))
                {var clip=Resources.Load<AudioClip>("DungeonBDX/Audio/Sfx/basico-golpe");if(clip&&Application.isPlaying)sfx.PlayOneShot(clip);}
                pendingResult=null;
                if(Session.Unit(selected)?.Guild!=Session.CurrentGuild){selected=Session.Units.First(u=>u.Guild==Session.CurrentGuild&&!u.Monster).Id;card=0;mode="move";}
                if(Session.Resting&&!Session.Unit(selected).Alive)mode="revive";
            }
            catch(Exception ex){Message="SALVAMENTO PENDENTE: "+ex.Message;}
            Draw();
        }
        void Select(int id)
        {selected=id;card=0;mode=Session.Resting&&!Session.Unit(id).Alive?"revive":"move";Draw();}
        void Draw()
        {
            if(!canvas||Session==null)return;lastW=Screen.width;lastH=Screen.height;bool portrait=Screen.width<Screen.height;
            var scaler=canvas.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.referenceResolution=portrait?new Vector2(720,1100):new Vector2(1440,900);
            AqUi.Clear(canvas.transform);safe=AqUi.Box(canvas.transform,"Safe area",0,0,1,1,Color.clear);DungeonShell.ApplySafeArea(safe,Screen.safeArea,Screen.width,Screen.height);
            var header=AqUi.Box(safe,"Header",0,.925f,1,.075f,AqUi.Panel);
            AqUi.Text(header,Session.Scenario.Name,.02f,.37f,.62f,.58f,portrait?21:28,AqUi.Gold);
            // Status markers with PixelLab icons; the icon width is estimated from the reference resolution.
            float hi=portrait?.045f:.02f;
            AqUi.Chip(header,"emblem",$"GUILDA {Session.CurrentGuild+1}",.02f,.02f,.15f,.36f,hi,14,AqUi.GuildColors[Session.CurrentGuild%AqUi.GuildColors.Length]);
            AqUi.Chip(header,"die",$"TURNO {Session.Turn}",.17f,.02f,.12f,.36f,hi,14,AqUi.Muted);
            AqUi.Chip(header,"coin",$"{Session.Guilds[Session.CurrentGuild].Coins} MOEDAS",.29f,.02f,.15f,.36f,hi,14,AqUi.Gold);
            AqUi.Chip(header,"save",SavePaused?"SAVE PENDENTE":"SALVO",.44f,.02f,.11f,.36f,hi,14,SavePaused?new Color(1,.48f,.35f):AqUi.Muted);
            AqUi.Icon(AqUi.Button(header,"MENU",.86f,.12f,.13f,.76f,Close),"menu");
            AqUi.Icon(AqUi.Button(header,muted?"SOM OFF":"SOM",.735f,.12f,.12f,.76f,()=>{muted=!muted;music.mute=sfx.mute=muted;Draw();}),"sound");
            AqUi.Icon(AqUi.Button(header,AqBoardView.ShowLife?"VIDA":"VIDA OFF",.645f,.2f,.08f,.6f,()=>{AqBoardView.ShowLife=!AqBoardView.ShowLife;view?.RefreshLife();Draw();}),"life");
            var quests=AqUi.Box(safe,"Quest strip",0,.88f,1,.045f,new Color(.12f,.14f,.15f,.97f));
            // Quest progress per guild (trophy) and the scenario quests (quest scroll).
            float qi=portrait?.065f:.026f,gw=portrait?.13f:.075f;int goal=Session.Guilds.Count==2?2:3;
            foreach(var g in Session.Guilds)
                AqUi.Chip(quests,"trophy",$"G{g.Index+1} {g.Completed.Count}/{goal}",.01f+g.Index*gw,.03f,gw,.94f,qi,15,AqUi.GuildColors[g.Index%AqUi.GuildColors.Length]);
            float qx=.015f+Session.Guilds.Count*gw;
            float qw=(.985f-qx)/Session.Scenario.Quests.Length;
            for(int i=0;i<Session.Scenario.Quests.Length;i++)
                AqUi.Chip(quests,AqScenarios.QuestIcon(Session.Scenario.Quests[i]),Session.Scenario.Quests[i],qx+i*qw,.03f,qw-.005f,.94f,qi,15);
            float footerHeight=FooterHeight;var foot=AqUi.Panelled(safe,"Bottom guild dashboard",0,0,1,footerHeight,AqUi.Panel);
            var party=Session.Units.Where(u=>!u.Monster&&u.Guild==Session.CurrentGuild).ToArray();
            // PC: three columns (heroes | cards | actions). Portrait keeps stacked rows.
            Rect heroArea=portrait?new Rect(.01f,.755f,.98f,.235f):new Rect(.005f,.03f,.25f,.94f);
            Rect cardArea=portrait?new Rect(.01f,.36f,.98f,.245f):new Rect(.265f,.03f,.43f,.72f);
            Rect stateArea=portrait?new Rect(.02f,.62f,.96f,.12f):new Rect(.27f,.78f,.42f,.2f);
            Rect actionArea=portrait?new Rect(.01f,.04f,.98f,.3f):new Rect(.705f,.03f,.29f,.94f);
            for(int i=0;i<party.Length;i++)
            {
                var u=party[i];
                var b=portrait?AqUi.Button(foot,"",heroArea.x+i*heroArea.width/3,heroArea.y,heroArea.width/3,heroArea.height,()=>Select(u.Id),selected==u.Id)
                    :AqUi.Button(foot,"",heroArea.x,heroArea.y+(2-i)*heroArea.height/3,heroArea.width,heroArea.height/3-.02f,()=>Select(u.Id),selected==u.Id);
                AqUi.Portrait(b.transform,u.HeroId,.02f,.03f,portrait?.21f:.16f,.94f);
                AqUi.Text(b.transform,portrait?u.Name+"\n"+(u.Alive?$"VIDA {u.Health}/{u.MaxHealth} · DEF {u.Defense}":"CAÍDO · DESCANSE"):u.Name+"   "+(u.Alive?$"{u.Health}/{u.MaxHealth} · DEF {u.Defense}":"CAÍDO"),
                    portrait?.25f:.2f,.36f,portrait?.72f:.78f,.6f,portrait?16:15,u.Alive?AqUi.Ink:AqUi.Muted);
                if(u.Alive)AqUi.LifeBar(b.transform,portrait?.25f:.2f,.1f,portrait?.6f:.5f,.24f,u.Health,u.MaxHealth);
            }
            var hero=Session.Unit(selected);string ability=hero.AbilityText.Split(':')[0];
            string state=Session.Reaction!=null?$"GUILDA {Session.Controller+1}: REAÇÃO DE {Session.Unit(Session.Reaction.Monster).Name.ToUpperInvariant()} · MOV {Session.Reaction.Movement}"+(string.IsNullOrEmpty(Session.Unit(Session.Reaction.Monster).AbilityText)?"":" · "+Session.Unit(Session.Reaction.Monster).AbilityText.Split(':')[0]):
                Session.Resting?"DESCANSO · carta + herói transfere; caído ressuscita":$"{hero.Name} · MOV {Session.MovesLeft}/3 · ATAQUE {(Session.Attacked?"USADO":"PRONTO")} · {ability}";
            AqUi.Text(foot,state,stateArea.x,stateArea.y,stateArea.width,stateArea.height,15,AqUi.Ink);
            for(int i=0;i<4;i++)
            {
                int index=i;var g=i<hero.Gear.Count?hero.Gear[i]:null;
                var b=AqUi.Button(foot,g==null?"":g.Name+"\n<size=80%>"+(g.Exhausted?"EXAUSTA":$"{g.Dice} DADOS · {(g.Ranged?"VISÃO":"PRÓXIMO")}")+"</size>",cardArea.x+i*cardArea.width/4,cardArea.y,cardArea.width/4-.004f,cardArea.height,
                    ()=>{card=index;mode=Session.Resting?"transfer":"attack";Draw();},mode=="attack"&&card==i);
                b.interactable=g!=null&&Session.Reaction==null&&(!g.Exhausted||Session.Resting);
            }
            // Actions: one row in portrait; a 3 + 2 grid on PC.
            void Act(int slot,int count,int row,string text,Action click,bool on=false,bool enabled=true,string icon=null)
            {
                float w=actionArea.width/count,h=portrait?actionArea.height:actionArea.height/2;
                float y=portrait?actionArea.y:actionArea.y+(1-row)*h;
                var act=AqUi.Button(foot,text,actionArea.x+slot*w,y,w-.004f,h-.02f,click,on);act.interactable=enabled;if(icon!=null)AqUi.Icon(act,icon);
            }
            if(Session.Reaction!=null)
            {
                int n=portrait?4:2;
                Act(0,n,0,"MOVER MONSTRO",()=>{mode="move";Draw();},icon:"claw");
                Act(1,n,0,"PORTA",()=>{mode="door";Draw();},icon:"door");
                Act(portrait?2:0,n,portrait?0:1,"ATACAR HERÓI",()=>Do(new AqCommand("react-attack")),icon:"sword");
                Act(portrait?3:1,n,portrait?0:1,"ENCERRAR REAÇÃO",()=>Do(new AqCommand("react-skip")),icon:"skip");
            }
            else
            {
                int top=portrait?5:3,low=portrait?5:2;
                Act(0,top,0,"MOVER",()=>{mode="move";Draw();},mode=="move",icon:"move");
                Act(1,top,0,"PORTA",()=>{mode="door";Draw();},icon:"door");
                Act(2,top,0,hero.Ability=="arsenal"?"FORMA":hero.Ability=="phase"?"PASSO SOMBRIO":"HABILIDADE",()=>{if(hero.Ability=="arsenal")Do(new AqCommand("stance",selected));else if(hero.Ability=="phase"){mode="phase";Draw();}else {Message=hero.AbilityText;Draw();}},icon:"ability");
                Act(portrait?3:0,low,portrait?0:1,Session.Resting?"DESCANSANDO":"DESCANSAR",()=>Do(new AqCommand("rest")),false,!Session.Resting&&Session.ActiveId<0,"rest");
                Act(portrait?4:1,low,portrait?0:1,"ENCERRAR TURNO",()=>Do(new AqCommand("end")),icon:"endturn");
            }
            // Camera column at the right edge of the table area.
            float colX=portrait?.89f:.935f,colW=portrait?.1f:.055f,step=portrait?.055f:.08f,colY=footerHeight+.07f;
            string[] camLabels={"GIRAR","GIRAR","ZOOM","ZOOM","REPOR"},camIcons={"rotate-left","rotate-right","zoom-in","zoom-out","reset"};
            Action[] camActions={()=>view.RotateCamera(-90),()=>view.RotateCamera(90),()=>view.ZoomCamera(1.2f),()=>view.ZoomCamera(1/1.2f),view.ResetCamera};
            for(int i=0;i<camLabels.Length;i++)AqUi.Icon(AqUi.Button(safe,camLabels[i],colX,colY+(camLabels.Length-1-i)*step,colW,step-.008f,camActions[i]),camIcons[i]);
            if(SavePaused)AqUi.Button(header,"TENTAR SALVAR",.55f,.12f,.17f,.76f,SavePending);
            var hint=AqUi.Box(safe,"Board hint",portrait?.5f:.6f,footerHeight+.005f,portrait?.48f:.32f,.035f,Color.clear);
            AqUi.Text(hint,mode=="attack"?"DISTÂNCIA NAS CASAS · CLIQUE EM UM ALVO":mode=="door"?"CLIQUE NO ESPAÇO DO OUTRO LADO DA PORTA":"CLIQUE NO TABULEIRO PARA AGIR",0,0,1,1,13,AqUi.Muted,TextAlignmentOptions.Right);
            var notice=AqUi.Box(safe,"Action result",.02f,footerHeight+.005f,portrait?.47f:.34f,portrait?.05f:.06f,new Color(.055f,.06f,.065f,.92f));
            AqUi.Text(notice,Message,.01f,.03f,.98f,.94f,15,SavePaused?new Color(1,.48f,.35f):AqUi.Ink);
            if(Session.Winner>=0)
            {
                var win=AqUi.Box(safe,"Victory",.18f,.43f,.64f,.32f,AqUi.Panel);
                AqUi.Text(win,$"GUILDA {Session.Winner+1} VITORIOSA\n\n"+Session.Scenario.Ending,.07f,.28f,.86f,.65f,25,AqUi.Gold,TextAlignmentOptions.Center);
                AqUi.Button(win,"VOLTAR ÀS AVENTURAS",.15f,.07f,.7f,.19f,Close);
            }
            Preview();
        }
        void Preview()
        {
            var u=Session.Unit(selected);var cells=new Dictionary<AqCell,int>();int selectedId=selected;
            if(Session.Reaction!=null)
            {
                var rx=Session.Reaction;u=Session.Unit(rx.Monster);selectedId=u.Id;
                foreach(var s in Session.Board.Spaces){var path=Session.Path(u,s.Cell);if(path.Count>1&&path.Count-1<=rx.Movement&&Session.CanStop(u,s.Cell))cells[s.Cell]=path.Count-1;}
            }
            else if(mode=="revive")foreach(var c in Session.ResurrectionCells(u))cells[c]=0;
            else if(mode=="door"||mode=="phase")
                foreach(var e in Session.Board.Edges.Where(e=>e.Door&&(e.A.Equals(u.Cell)||e.B.Equals(u.Cell))))cells[e.A.Equals(u.Cell)?e.B:e.A]=1;
            else if(!Session.Resting)cells=Session.Range(selected,mode=="move",card);
            view.Select(selectedId);view.ShowCells(cells,mode!="attack");
        }
        void Close()
        {
            if(SavePaused){Message="Tente salvar a ação pendente antes de sair.";Draw();return;}
            Cleanup();enabled=false;if(menu)menu.Return();
        }
        void Cleanup()
        {if(world){world.SetActive(false);if(Application.isPlaying)Destroy(world);else DestroyImmediate(world);}world=null;view=null;canvas=null;}
        void OnDestroy()=>Cleanup();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [Serializable] public sealed class Probe
        {public bool passed,sharedSpace,colossusLarger,uniqueAbilities,saveReplay,camera,animation,moved,combat,idleAnimated;public int heroes,monsters,tiles,commands,width,height;public float colossusHeight,heroHeight;public string error,idleBefore,idleAfter,attackClip,attackFrame,facingBefore,facingAfter;}
        public IEnumerator CaptureProbe(string path)
        {
            var report=new Probe();Exception failure=null;DungeonActorAnimation idle=null;
            for(int i=0;i<30;i++)yield return null;
            try
            {
                idle=view.Animations.Values.First(a=>a.AnimationFrameCount>1);report.idleBefore=idle.CurrentSprite?.name;
            }catch(Exception e){failure=e;}
            yield return new WaitForSecondsRealtime(.3f);
            try
            {
                report.width=Screen.width;report.height=Screen.height;report.idleAfter=idle?.CurrentSprite?.name;report.idleAnimated=report.idleBefore!=report.idleAfter;
                report.heroes=Session.Units.Count(u=>!u.Monster);report.monsters=Session.Units.Count(u=>u.Monster);report.tiles=Session.Board.Spaces.Select(s=>s.Tile).Distinct().Count();
                report.sharedSpace=Session.Units.Where(u=>!u.Monster).GroupBy(u=>u.Cell).Any(g=>g.Count()==2);
                report.colossusHeight=view.RenderedSpriteHeight(Session.Units.First(u=>u.HeroId=="colosso").Id);report.heroHeight=view.RenderedSpriteHeight(Session.Units.First(u=>u.HeroId=="talyssa").Id);
                report.colossusLarger=report.colossusHeight>report.heroHeight*1.4f;
                report.uniqueAbilities=AqContent.Heroes.Values.Select(h=>h.Ability).Distinct().Count()==AqContent.Heroes.Count;
                ProbeCommands(report);
                if(SavePaused)throw new IOException(Message);
                report.saveReplay=store.Load().Session.Fingerprint()==Session.Fingerprint();
                // Isometric orbit: a 90 degree turn must re-pick the camera-relative sprite direction.
                report.facingBefore=idle.FacingDirection;view.RotateCamera(90);view.SettleCamera();report.facingAfter=idle.FacingDirection;
                bool turned=Math.Abs(Mathf.DeltaAngle(view.Yaw,AqBoardView.IsoYaw+90))<.1f&&report.facingBefore!=report.facingAfter;
                view.RotateCamera(-90);view.SettleCamera();view.ZoomCamera(1.08f);
                report.camera=turned&&Math.Abs(Mathf.DeltaAngle(view.Yaw,AqBoardView.IsoYaw))<.1f&&view.Zoom>1;
                mode="move";Draw();
            }
            catch(Exception e){failure=e;}
            yield return new WaitForSecondsRealtime(.25f);
            try{Capture(path,view.Camera,canvas);}catch(Exception e){failure=e;}
            report.error=failure?.ToString();report.passed=failure==null&&report.sharedSpace&&report.colossusLarger&&report.uniqueAbilities&&report.saveReplay&&report.camera&&report.animation&&report.idleAnimated&&report.moved&&report.combat;
            File.WriteAllText(Path.ChangeExtension(path,".json"),JsonUtility.ToJson(report,true));Application.Quit(report.passed?0:1);
        }
        void ProbeCommands(Probe report)
        {
            for(int turn=0;turn<8&&!report.combat;turn++)
            {
                var hero=Session.Units.FirstOrDefault(u=>u.Guild==Session.CurrentGuild&&u.Alive&&!u.Monster&&!u.Huge&&u.Gear.Any(g=>!g.Exhausted&&g.Ranged));
                if(hero==null){Do(new AqCommand("rest"));Do(new AqCommand("end"));continue;}
                selected=hero.Id;card=hero.Gear.FindIndex(g=>g.Ranged&&!g.Exhausted);
                for(int step=0;step<4&&hero.Alive&&!report.combat;step++)
                {
                    var target=Session.Units.FirstOrDefault(u=>Session.CanTarget(hero,u,hero.Gear[card]));
                    if(target!=null)
                    {
                        var attack=Do(new AqCommand("attack",hero.Id,target.Id,card));
                        report.combat=attack.Accepted&&attack.Effects.Any(e=>e.Kind=="attack");
                        if(report.combat){var anim=view.Animations[hero.Id];report.animation=anim.AnimationFrameCount>1;report.attackClip=anim.CurrentClip;report.attackFrame=anim.CurrentSprite?.name;}
                        ProbeReactions();break;
                    }
                    var door=Session.Board.Edges.FirstOrDefault(e=>e.Door&&!e.Open&&(e.A.Equals(hero.Cell)||e.B.Equals(hero.Cell)));
                    if(door!=null&&Session.MovesLeft>0)
                    {var to=door.A.Equals(hero.Cell)?door.B:door.A;Do(new AqCommand("door",hero.Id,x:to.X,y:to.Y));}
                    var destinations=Session.Range(hero.Id,true).Keys.OrderBy(c=>Session.Units.Where(u=>u.Monster&&u.Alive).Select(u=>c.Distance(u.Cell)).DefaultIfEmpty(99).Min()).ToArray();
                    if(destinations.Length==0)break;
                    var cell=destinations[0];var moved=Do(new AqCommand("move",hero.Id,x:cell.X,y:cell.Y));report.moved|=moved.Effects.Any(e=>e.Kind=="move");ProbeReactions();
                }
                if(!report.combat)Do(new AqCommand("end"));
            }
            report.commands=Session.Commands.Count;
        }
        void ProbeReactions()
        {for(int i=0;Session.Reaction!=null&&i<50;i++)Do(new AqCommand("react-skip"));}
        public static void Capture(string path,Camera camera,Canvas captureCanvas)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
            var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24,RenderTextureFormat.ARGB32);var oldTarget=camera.targetTexture;var active=RenderTexture.active;
            var oldMode=captureCanvas.renderMode;var oldCamera=captureCanvas.worldCamera;Texture2D pixels=null;
            try
            {
                camera.targetTexture=target;captureCanvas.renderMode=RenderMode.ScreenSpaceCamera;captureCanvas.worldCamera=camera;captureCanvas.planeDistance=1;Canvas.ForceUpdateCanvases();
                if(GraphicsSettings.currentRenderPipeline){var request=new RenderPipeline.StandardRequest{destination=target};RenderPipeline.SubmitRenderRequest(camera,request);}else camera.Render();
                RenderTexture.active=target;pixels=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);pixels.Apply();File.WriteAllBytes(path,pixels.EncodeToPNG());
            }
            finally{camera.targetTexture=oldTarget;RenderTexture.active=active;captureCanvas.renderMode=oldMode;captureCanvas.worldCamera=oldCamera;if(pixels)Destroy(pixels);RenderTexture.ReleaseTemporary(target);}
        }
#endif
    }
}
