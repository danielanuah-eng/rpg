using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DungeonBDX.Combat;
using DungeonBDX.Services;
using DungeonBDX.Skills;
using DungeonBDX.World;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace DungeonBDX
{
    // Offline controller. Every mutation goes through the same replayable command journal.
    public sealed class DungeonExpedition : MonoBehaviour
    {
        static readonly Color Panel = new Color(.10f,.085f,.13f,.96f), Card = new Color(.19f,.15f,.21f),
            Gold = new Color(.79f,.63f,.4f), Ink = new Color(.95f,.9f,.83f), Wine = new Color(.40f,.23f,.31f);
        DungeonShell shell;
        GameObject root;
        DungeonWorldView world;
        DungeonAudio audioBus;
        DungeonMap map;
        CampaignJournal journal;
        LocalCampaignStore store;
        long revision;
        string operation, selectedSkill, status, saveStatus, storageDirectory;
        bool moving, saveBlocked, saveConflict, dirty, enemyBusy, confirmClose;
        int width, height;
        Rect safeArea;
        RectTransform safe, actions, header, footer;
        TextMeshProUGUI title, stats, hint, history, targetInfo, saveLabel;
        string inspected;
        readonly List<(UnityEngine.UI.Button button,string skill)> actionButtons = new List<(UnityEngine.UI.Button,string)>();
        readonly DungeonCameraInput cameraInput = new DungeonCameraInput();
        string rangeSummary, rangeFeedback;
        public bool IsOpen => root;
        public Canvas HudCanvas { get; private set; }
        public Camera ViewCamera => world ? world.ViewCamera : null;
        public DungeonSkillSession Session => journal?.Session;
        public string SaveMessage => saveStatus;
        public DungeonWorldView WorldView => world;
        public string RangeSummary => rangeSummary;

        public bool Open(DungeonShell owner, string scenario, DungeonSetup setup, string storageRoot = null, ulong? fixedSeed = null)
        {
            if (root) return false;
            shell = owner;
            moving = saveBlocked = saveConflict = dirty = enemyBusy = confirmClose = false; selectedSkill = operation = null;
            try
            {
                bool qa = Environment.GetCommandLineArgs().Contains("-dungeonbdx-qa");
                string path = storageRoot ?? Path.Combine(Application.persistentDataPath,"DungeonBDX",qa ? "QA" : "Campaigns");
                storageDirectory=path;
                store = new LocalCampaignStore(path,"local");
                var saved = store.Load(DungeonProfiles.Resolve,(seed,id)=>DungeonMap.Generate(seed,id));
                revision = saved?.Revision ?? 0;
                if (scenario==null)
                {
                    if (saved==null) { saveStatus="Nenhuma expedição salva neste dispositivo."; return false; }
                    journal = saved.Journal;
                    saveStatus = saved.RecoveredBackup ? "Backup recuperado" : "Expedição retomada";
                }
                else
                {
                    ulong seed = fixedSeed ?? (qa ? 42UL : unchecked((ulong)DateTime.UtcNow.Ticks));
                    map = DungeonMap.Generate(seed,scenario);
                    var mechanics = SkillMechanics.Default;
                    if (setup.HasMechanic(CampaignMechanics.FriendlyFire)) mechanics |= SkillMechanics.FriendlyFire;
                    var ids = setup.Party.Count==0 ? DungeonHeroProfiles.NewHeroes.Select(h=>h.Id).ToArray() : setup.Party.ToArray();
                    journal = CampaignJournal.Create(seed,scenario,mechanics,CreateSpawns(map,ids,setup.HasMechanic(CampaignMechanics.DragonEncounters)),map,(int)setup.Mechanics);
                    dirty = true;
                    if (!Persist()) return false;
                }
                if(scenario==null) map = DungeonMap.Generate(journal.Seed,journal.Scenario);
                root = new GameObject("Dungeon Expedition"); root.transform.SetParent(transform,false);
                world = root.AddComponent<DungeonWorldView>(); world.Build(map,journal.Session);
                root.AddComponent<AudioListener>();
                audioBus = root.AddComponent<DungeonAudio>(); audioBus.Begin(journal.Scenario);
                var go = new GameObject("Expedition HUD",typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster));
                go.transform.SetParent(root.transform,false); HudCanvas = go.GetComponent<Canvas>(); HudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = go.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
                status = "Escolha uma ação e toque no alvo. Mover custa 1 PA."; inspected = Session.ActiveActorId;
                BuildHud(); return true;
            }
            catch (Exception error) { Debug.LogException(error); saveStatus = "Não foi possível abrir: "+error.Message; Cleanup(); return false; }
        }

        public static IReadOnlyList<HeroSpawn> CreateSpawns(DungeonMap map,IReadOnlyList<string> party,bool dragons)
        {
            if (party==null || party.Count<1 || party.Count>3 || party.Distinct().Count()!=party.Count) throw new ArgumentException("Equipe inválida.");
            var initial = new List<HeroSpawn>();
            for (int i=0;i<party.Count;i++) initial.Add(new HeroSpawn("hero"+i,0,DungeonProfiles.Resolve(party[i]),map.HeroSpawns[i]));
            // Enemies are NPCs, never playable heroes (author, 17/10).
            string[] enemies = {"raizor","nixira",dragons ? "vaelis-vinculo" : "domador-de-correntes"};
            for (int i=0;i<enemies.Length;i++) initial.Add(new HeroSpawn("enemy"+i,1,DungeonProfiles.Resolve(enemies[i]),map.EnemySpawns[i]));
            return initial.AsReadOnly();
        }

        bool Persist()
        {
            if (!dirty) return !saveBlocked;
            try
            {
                operation ??= Guid.NewGuid().ToString("N");
                revision = store.Save(journal,revision,operation); dirty = false; saveBlocked = false; operation = null;
                saveConflict = false; saveStatus = "Salvo · revisão "+revision; return true;
            }
            catch (Exception error)
            {
                saveBlocked = true; saveConflict = error is SaveConflictException;
                saveStatus = saveConflict ? "Conflito: use RETOMAR DISCO." : "Falha ao salvar. Use TENTAR SALVAR.";
                Debug.LogWarning("DungeonBDX save: "+error.Message); return false;
            }
        }

        void Changed()
        { dirty = true; Persist(); world.Sync(); UpdateHud(); }

        void BuildHud()
        {
            width = Screen.width; height = Screen.height; safeArea = Screen.safeArea;
            bool portrait = width<height;
            HudCanvas.GetComponent<UnityEngine.UI.CanvasScaler>().referenceResolution = portrait ? new Vector2(720,1280) : new Vector2(1280,800);
            for (int i=HudCanvas.transform.childCount-1;i>=0;i--)
            { var old = HudCanvas.transform.GetChild(i).gameObject; old.SetActive(false); Dispose(old); }
            safe = Box("Safe area",HudCanvas.transform,Vector2.zero,Vector2.one,Color.clear);
            DungeonShell.ApplySafeArea(safe,safeArea,width,height);
            var top = Box("Expedition header",safe,new Vector2(0,.9f),Vector2.one,Panel);
            header=top;
            top.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            title = Text(top,"",new Vector2(.025f,.42f),new Vector2(.74f,.96f),25,Gold);
            hint = Text(top,"",new Vector2(.025f,.05f),new Vector2(.74f,.42f),14,Ink);
            Button(top,"ÁUDIO",new Vector2(.76f,.06f),new Vector2(.87f,.94f),()=>audioBus.ToggleMute());
            Button(top,"MENU",new Vector2(.88f,.06f),new Vector2(.99f,.94f),Close);
            var bottom = Box("Combat bottom HUD",safe,Vector2.zero,new Vector2(1,portrait ? .37f : .29f),Panel);
            footer=bottom;
            bottom.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            stats = Text(bottom,"",new Vector2(.025f,.76f),new Vector2(.78f,.98f),19,Gold);
            targetInfo = Text(bottom,"",new Vector2(.025f,.62f),new Vector2(.975f,.76f),14,Ink);
            actions = Box("Skill action buttons",bottom,new Vector2(.015f,.38f),new Vector2(.985f,.62f),Color.clear);
            var cameraControls=Box("Camera controls",bottom,new Vector2(.015f,.225f),new Vector2(.985f,.37f),Color.clear);
            string[] cameraNames={"GIRAR ‹","GIRAR ›","ZOOM +","ZOOM −","RESET"};
            Action[] cameraActions={() => world.RotateCamera(-45),() => world.RotateCamera(45),() => world.ZoomCamera(1.2f),() => world.ZoomCamera(1/1.2f),() => world.FitCamera()};
            for(int i=0;i<cameraNames.Length;i++)
                Button(cameraControls,cameraNames[i],new Vector2(i/5f,0),new Vector2((i+1)/5f,1),cameraActions[i]);
            actionButtons.Clear();
            Button(bottom,"MOVER · 1 PA",new Vector2(.01f,.02f),new Vector2(.23f,.22f),()=>SelectForPreview(null,true));
            var saveButton=Button(bottom,"TENTAR SALVAR",new Vector2(.24f,.02f),new Vector2(.49f,.22f),RetrySave);
            saveLabel=saveButton.GetComponentInChildren<TextMeshProUGUI>();
            Button(bottom,"ENCERRAR TURNO",new Vector2(.68f,.02f),new Vector2(.99f,.22f),EndPlayerTurn);
            history = Text(safe,"",new Vector2(.02f,.87f),new Vector2(.98f,.9f),15,Ink);
            world.FitCamera(); BuildActions(); UpdateHud();
        }

        void BuildActions()
        {
            for (int i=actions.childCount-1;i>=0;i--) { var old=actions.GetChild(i).gameObject;old.SetActive(false);Dispose(old); }
            actionButtons.Clear();
            var unit = Session.Unit(Session.ActiveActorId); if (unit==null || unit.Team!=0) return;
            var profile = DungeonProfiles.Resolve(unit.ProfileId); int count = profile.SkillIds.Count;
            bool portrait = width<height; int columns = portrait ? 3 : count; int rows = Mathf.CeilToInt(count/(float)columns);
            for (int i=0;i<count;i++)
            {
                string id = profile.SkillIds[i]; var skill = DungeonSkillCatalog.All[id]; int x=i%columns,y=i/columns;
                var button = Button(actions,$"{skill.Name}\n<size=80%>{skill.Cost} PA · ALC {skill.MinRange}–{skill.Range}</size>",
                    new Vector2(x/(float)columns,1-(y+1)/(float)rows),new Vector2((x+1)/(float)columns,1-y/(float)rows),()=>SelectSkill(id));
                actionButtons.Add((button,id));
            }
        }

        void SelectSkill(string id)
        {
            if (saveBlocked || Session.Finished || Session.Unit(Session.ActiveActorId)?.Team!=0) return;
            rangeFeedback=null;selectedSkill=id; moving=false; var skill=DungeonSkillCatalog.All[id];
            if (skill.Target==SkillTarget.Self) Execute(id,null,null);
            else { status = skill.Target==SkillTarget.Destination ? "Toque numa casa livre." : "Toque no alvo da habilidade."; UpdateHud(); }
        }

        // QA/presentation selection never casts, including self-targeted skills.
        public bool SelectForPreview(string skillId=null,bool movement=false)
        {
            if(!IsOpen || saveBlocked || Session.Finished || Session.Unit(Session.ActiveActorId)?.Team!=0)return false;
            if(!movement && (!DungeonSkillCatalog.TryGet(skillId,out _) || !DungeonProfiles.Resolve(Session.Unit(Session.ActiveActorId).ProfileId).SkillIds.Contains(skillId)))return false;
            rangeFeedback=null;moving=movement;selectedSkill=movement?null:skillId;
            status=movement?"MOVER · 1 PA · toque numa casa destacada.":DungeonSkillCatalog.All[skillId].Name+" · toque no alvo.";
            UpdateHud();return true;
        }

        public void RotateCamera(float degrees)=>world?.RotateCamera(degrees);
        public void ZoomCamera(float factor)=>world?.ZoomCamera(factor);
        public void ResetCamera()=>world?.FitCamera();

        public void ClickCell(GridCell cell)
        {
            if (!IsOpen || saveBlocked || Session.Finished || !CanCommand()) return;
            var actor = Session.Unit(Session.ActiveActorId); if (actor==null || actor.Team!=0) return;
            var target = Session.Units.FirstOrDefault(u=>u.Alive && u.Cell.Equals(cell));
            inspected = target?.Id;
            if (moving)
            {
                var result = journal.Move(actor.Id,cell); status = result==SkillFailure.None ? "Movimento realizado · 1 PA." : Explain(result);
                rangeFeedback=result==SkillFailure.None?null:status;
                if (result==SkillFailure.None) Changed(); else UpdateHud(); return;
            }
            if (selectedSkill==null) { status = "Escolha MOVER ou uma habilidade no painel inferior.";UpdateHud();return; }
            var skill = DungeonSkillCatalog.All[selectedSkill];
            if (skill.Target==SkillTarget.Destination) Execute(selectedSkill,null,cell);
            else if (target!=null)
            {
                var targets = new List<string>{target.Id};
                if (skill.MaxTargets>1)
                    targets.AddRange(Session.Units.Where(u=>u.Alive && u.Id!=target.Id &&
                        GridCell.Distance(cell,u.Cell)<=skill.AreaRadius &&
                        DungeonActionRange.EligibleTarget(Session,map,actor,skill,u,journal.Mechanics))
                        .OrderBy(u=>u.Id,StringComparer.Ordinal).Take(skill.MaxTargets-1).Select(u=>u.Id));
                Execute(selectedSkill,targets,null);
            }
            else { status="Toque em um herói ou adversário.";UpdateHud(); }
        }

        void Execute(string id,IReadOnlyList<string> targets,GridCell? destination)
        {
            if (!CanCommand()) return;
            try
            {
                var result = journal.Cast(Session.ActiveActorId,id,targets,destination);
                if (!result.Success) { status=Explain(result.Failure);rangeFeedback=status;UpdateHud();return; }
                rangeFeedback=null;selectedSkill=null; moving=false;
                string impacts=string.Join("  ",result.Impacts.Select(i=>$"{Name(Session.Unit(i.UnitId))}: {i.HealthDelta:+0;-0;0} vida · {i.MitigationBasisPoints/100f:0}% mitigação"));
                status=DungeonSkillCatalog.All[id].Name+"  "+impacts; Changed();
                world.PlayResult(result);audioBus.Play(result);
            }
            catch (InvalidOperationException) { status="Não foi possível aplicar a ação. Use RETOMAR DISCO para recuperar o último save.";saveBlocked=saveConflict=true;UpdateHud(); }
        }

        void EndPlayerTurn()
        {
            if (saveBlocked || Session.Finished || Session.Unit(Session.ActiveActorId)?.Team!=0 || !CanCommand()) return;
            if (journal.EndTurn(Session.ActiveActorId)) { selectedSkill=null;moving=false;inspected=Session.ActiveActorId;Changed();BuildActions(); }
        }

        void Update()
        {
            if (!IsOpen) return;
            if (width!=Screen.width || height!=Screen.height || safeArea!=Screen.safeArea) BuildHud();
            if (!saveBlocked && !Session.Finished && journal.CommandCount<CampaignJournal.MaxCommands && Session.Unit(Session.ActiveActorId)?.Team==1 && !enemyBusy) StartCoroutine(EnemyTurn());
            if(cameraInput.Read(world,HitsHud,out var tap) && world.Pick(tap,out var cell))ClickCell(cell);
            if(world.Preview!=null && Mouse.current!=null && !cameraInput.GestureBusy)
            {
                var position=Mouse.current.position.ReadValue();
                var hovered=!HitsHud(position)&&world.Pick(position,out var hover)?world.Preview.Cells.FirstOrDefault(c=>c.Cell.Equals(hover)):null;
                if(history)history.text=RangeCaption()+(hovered==null?"":$" · DISTÂNCIA {hovered.Distance} CASAS" );
            }
        }

        public static bool PointerHitsUi(Vector2 position)
        {
            if(!EventSystem.current)return false;
            var data=new PointerEventData(EventSystem.current){position=position};var results=new List<RaycastResult>();
            EventSystem.current.RaycastAll(data,results);
            return results.Any(hit=>hit.module is UnityEngine.UI.GraphicRaycaster);
        }

        bool HitsHud(Vector2 position)
        {
            var camera=HudCanvas.renderMode==RenderMode.ScreenSpaceOverlay?null:HudCanvas.worldCamera;
            // Also blocks on the first frame, before GraphicRaycaster has rendered depths.
            return header && RectTransformUtility.RectangleContainsScreenPoint(header,position,camera) ||
                footer && RectTransformUtility.RectangleContainsScreenPoint(footer,position,camera) || PointerHitsUi(position);
        }

        IEnumerator EnemyTurn()
        {
            enemyBusy=true; string actorId=Session.ActiveActorId;
            try
            {
            for (int count=0;count<3 && IsOpen && !saveBlocked && !Session.Finished && Session.ActiveActorId==actorId && Session.RemainingActions>0;count++)
            {
                yield return new WaitForSecondsRealtime(.65f);
                if (!IsOpen || saveBlocked || Session.Finished || Session.ActiveActorId!=actorId || !CanCommand()) break;
                var actor=Session.Unit(actorId); var enemies=Session.Units.Where(u=>u.Alive && u.Team!=actor.Team).OrderBy(u=>GridCell.Distance(actor.Cell,u.Cell)).ThenBy(u=>u.Id,StringComparer.Ordinal).ToArray();
                bool acted=false;
                if(enemies.Length>0 && GridCell.Distance(actor.Cell,enemies[0].Cell)<=4 && Session.PetId(actorId)==null)
                {
                    var summon=DungeonProfiles.Resolve(actor.ProfileId).SkillIds.Select(id=>DungeonSkillCatalog.All[id]).FirstOrDefault(s=>s.Has(SkillEffect.Summon));
                    if(summon!=null)
                        foreach(var cell in map.Floors.Where(c=>GridCell.Distance(c,actor.Cell)==1).OrderBy(c=>GridCell.Distance(c,enemies[0].Cell)))
                        {
                            var result=journal.Cast(actorId,summon.Id,destination:cell);if(!result.Success)continue;
                            status=Name(actor)+" invocou um dragão.";Changed();world.PlayResult(result);audioBus.Play(result);acted=true;break;
                        }
                }
                foreach (var id in DungeonProfiles.Resolve(actor.ProfileId).SkillIds)
                {
                    if(acted)break;
                    var skill=DungeonSkillCatalog.All[id];
                    if (skill.Target!=SkillTarget.Enemy || !skill.Has(SkillEffect.Attack)) continue;
                    foreach (var target in enemies)
                    {
                        var result=journal.Cast(actorId,id,new[]{target.Id});
                        if (!result.Success) continue;
                        status=Name(actor)+" usou "+skill.Name;Changed();world.PlayResult(result);audioBus.Play(result);acted=true;break;
                    }
                    if (acted) break;
                }
                if (!acted && enemies.Length>0)
                {
                    var next = DungeonTactics.Advance(Session,map);
                    if (next.HasValue && journal.Move(actorId,next.Value)==SkillFailure.None)
                    {
                        status=Name(actor)+" avançou.";Changed();acted=true;
                    }
                }
                if (!acted) break;
            }
            if (IsOpen && !saveBlocked && !Session.Finished && Session.ActiveActorId==actorId && CanCommand())
            { journal.EndTurn(actorId);inspected=Session.ActiveActorId;Changed();BuildActions(); }
            }
            finally {enemyBusy=false;}
        }

        void UpdateHud()
        {
            if (!title) return;
            var unit=Session.Unit(Session.ActiveActorId);
            title.text="DUNGEONBDX  /  "+ScenarioName(journal.Scenario);
            hint.text=$"TURNO {Session.TurnIndex} · {saveStatus}";
            stats.text=Session.Finished ? (Session.Units.Any(u=>u.Alive && u.Team==0) ? "VITÓRIA · MASMORRA CONQUISTADA" : "EXPEDIÇÃO DERROTADA") :
                $"{Name(unit)}  ·  VIDA {unit.Health}/{unit.MaxHealth}  ·  ESCUDO {unit.Shield}  ·  {Session.RemainingActions}/3 PA";
            var target=Session.Unit(inspected) ?? unit;
            targetInfo.text=target==null ? "Crítico linear · Mitigação máxima 60%" :
                $"{Name(target)} · VIDA {target.Health}/{target.MaxHealth} · ESCUDO {target.Shield} · ARM {target.Armor} · MITIGAÇÃO {CombatMath.Mitigation(target.Armor)/100f:0}% · BRUTO {Session.DamageSpentThisTurn}/60";
            history.text=status;
            if(saveLabel) saveLabel.text=saveConflict ? "RETOMAR DISCO" : "TENTAR SALVAR";
            foreach (var entry in actionButtons)
            {
                entry.button.interactable=!saveBlocked && unit!=null && unit.Team==0 && Session.RemainingActions>=DungeonSkillCatalog.All[entry.skill].Cost;
                entry.button.GetComponent<DungeonMotion>().Chosen=entry.skill==selectedSkill;
            }
            RefreshRange();
        }

        void RefreshRange()
        {
            if(!world)return;
            var active=Session.Unit(Session.ActiveActorId);
            if(saveBlocked || Session.Finished || active==null || active.Team!=0 || !moving && selectedSkill==null)
            {world.ShowRange(null);rangeSummary=rangeFeedback=null;return;}
            var preview=DungeonActionRange.Compute(Session,map,moving,selectedSkill,journal.Mechanics);
            world.ShowRange(preview);
            if(moving)
                rangeSummary=$"MOVER · até {Math.Max(1,DungeonProfiles.Resolve(active.ProfileId).Movement-active.Slow)} CASAS · {preview.Cells.Count} destinos livres";
            else
            {
                var skill=DungeonSkillCatalog.All[selectedSkill];
                rangeSummary=$"{skill.Name} · ALC {skill.MinRange}–{skill.Range} CASAS"+
                    (skill.RequiresPet&&skill.Has(SkillEffect.Attack)?" · ORIGEM: DRAGÃO":"")+
                    (skill.Target==SkillTarget.Enemy||skill.Target==SkillTarget.Ally?$" · {preview.TargetIds.Count} alvos":"");
            }
            if(history)history.text=RangeCaption();
        }

        string RangeCaption()=>string.IsNullOrEmpty(rangeFeedback)?rangeSummary:rangeFeedback+" · "+rangeSummary;

        public void Close()
        {
            if (dirty && !Persist() && !confirmClose)
            { confirmClose=true;status="Falha ao salvar. Toque MENU novamente para sair; as últimas ações podem não persistir.";UpdateHud();return; }
            StopAllCoroutines(); enemyBusy=false;Cleanup();shell.ReturnToSetup();
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [Serializable] sealed class SmokeReport
        { public bool passed; public string error,scenario,state; public long revision; public int commands,units; }
        public bool RunSmoke(string path)
        {
            var report=new SmokeReport{scenario=journal.Scenario};
            try
            {
                void Check(bool value,string message){if(!value)throw new InvalidOperationException(message);}
                GridCell FreeNeighbour(string id,int range=1)
                {
                    var unit=Session.Unit(id);
                    return map.Floors.First(c=>GridCell.Distance(c,unit.Cell)>=1 && GridCell.Distance(c,unit.Cell)<=range &&
                        !Session.Units.Any(u=>u.Alive && u.Cell.Equals(c)));
                }
                moving=true;ClickCell(FreeNeighbour("hero0"));
                Check(Session.RemainingActions==2,"Move did not cost exactly 1 AP");
                SelectSkill("dbdx.pistola.mira");Check(Session.RemainingActions==1,"Aim action failed");
                EndPlayerTurn();Check(Session.ActiveActorId=="hero1","Hero turn order diverged");
                SelectSkill("dbdx.arcano.egide");ClickCell(Session.Unit("hero1").Cell);
                Check(Session.Unit("hero1").Shield==8,"Shield binding failed");
                EndPlayerTurn();Check(Session.ActiveActorId=="hero2","Dragon binder turn missing");
                SelectSkill("dbdx.draco.invocar");ClickCell(FreeNeighbour("hero2"));
                Check(Session.PetId("hero2")!=null,"Companion binding failed");
                Check(!saveBlocked && !dirty,"Continuous save did not complete");
                var restored=store.Load(DungeonProfiles.Resolve,(seed,id)=>DungeonMap.Generate(seed,id));
                Check(restored.Journal.Session.PersistenceState==Session.PersistenceState,"Reload changed clocks, budgets, units or RNG");
                var cell=Session.Unit("hero2").Cell;
                Check(world.Pick(ViewCamera.WorldToScreenPoint(DungeonWorldView.Position(cell)),out var picked) && picked.Equals(cell),"World input projection mismatch");
                Canvas.ForceUpdateCanvases();
                var hudButton=actions.GetComponentInChildren<UnityEngine.UI.Button>();
                if(hudButton)
                {
                    var rect=(RectTransform)hudButton.transform;
                    Check(HitsHud(RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center))),"HUD does not block pointer input");
                }
                report.passed=true;report.revision=revision;report.commands=journal.CommandCount;report.units=Session.Units.Count;
                report.state=restored.Journal.Export();
                Debug.Log("DUNGEONBDX_SMOKE_OK commands="+report.commands+" revision="+revision);
            }
            catch(Exception error){report.error=error.Message;Debug.LogError("DUNGEONBDX_SMOKE_FAILED "+error.Message);}
            File.WriteAllText(path,JsonUtility.ToJson(report,true));return report.passed;
        }
#endif
        bool CanCommand()
        {
            if (journal.CommandCount<CampaignJournal.MaxCommands) return true;
            status="Limite desta expedição alcançado. Seu progresso está salvo; inicie outra pelo menu.";UpdateHud();return false;
        }
        void RetrySave()
        {
            if (!saveConflict) {Persist();UpdateHud();return;}
            StopAllCoroutines();Cleanup();
            if (!Open(shell,null,shell.Setup,storageDirectory)) shell.ReturnToSetup();
        }
        void Cleanup()
        { if(root){root.SetActive(false);Dispose(root);}root=null;HudCanvas=null;world=null; }
        void OnApplicationPause(bool paused) { if (paused && IsOpen && dirty) Persist(); }
        void OnApplicationQuit() { if (IsOpen && dirty) Persist(); }
        void OnDestroy() { if(root) Cleanup(); }

        static string Name(UnitSnapshot unit) => unit==null ? "" : unit.IsSummon ? "Dragão companheiro" : DungeonProfiles.Resolve(unit.ProfileId).Name;
        static string ScenarioName(string id) => id==DungeonMap.Crypt ? "CRIPTA DE BRONZE" : id==DungeonMap.Library ? "BIBLIOTECA DO CREPÚSCULO" : "NINHO DO ECLIPSE";
        static string Explain(SkillFailure failure)
        {
            switch(failure)
            {
                case SkillFailure.NoActions:return "PA insuficientes. Encerre o turno.";
                case SkillFailure.Cooldown:return "Habilidade em recarga.";
                case SkillFailure.OutOfRange:return "Alvo fora do alcance.";
                case SkillFailure.BlockedSight:return "A parede bloqueia a linha de visão.";
                case SkillFailure.InvalidDestination:return "Destino ocupado, bloqueado ou longe demais.";
                case SkillFailure.MissingPet:return "Invoque seu dragão primeiro.";
                case SkillFailure.PetAlreadyPresent:return "Seu dragão já está na masmorra.";
                case SkillFailure.UsesExhausted:return "Limite desta habilidade no turno alcançado.";
                default:return "Escolha um alvo válido para essa habilidade.";
            }
        }
        UnityEngine.UI.Button Button(Transform parent,string text,Vector2 min,Vector2 max,Action click)
        {
            var hit=Box(text,parent,min,max,Color.clear);hit.offsetMin=new Vector2(3,3);hit.offsetMax=new Vector2(-3,-3);
            var image=hit.GetComponent<UnityEngine.UI.Image>();image.raycastTarget=true;
            var button=hit.gameObject.AddComponent<UnityEngine.UI.Button>();button.targetGraphic=image;button.transition=UnityEngine.UI.Selectable.Transition.None;
            button.onClick.AddListener(()=>click());var visual=Box("Visual",hit,Vector2.zero,Vector2.one,Card);
            hit.gameObject.AddComponent<DungeonMotion>().Configure(visual,Card,Wine);Text(visual,text,new Vector2(.04f,.02f),new Vector2(.96f,.98f),16,Ink,TextAlignmentOptions.Center);return button;
        }
        static RectTransform Box(string name,Transform parent,Vector2 min,Vector2 max,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image));var rect=(RectTransform)go.transform;rect.SetParent(parent,false);
            rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;var image=go.GetComponent<UnityEngine.UI.Image>();image.color=color;image.raycastTarget=false;return rect;
        }
        static TextMeshProUGUI Text(Transform parent,string text,Vector2 min,Vector2 max,float size,Color color,TextAlignmentOptions alignment=TextAlignmentOptions.Left)
        {
            var go=new GameObject("Label",typeof(RectTransform),typeof(TextMeshProUGUI));var rect=(RectTransform)go.transform;rect.SetParent(parent,false);rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;
            var label=go.GetComponent<TextMeshProUGUI>();label.font=TMP_Settings.defaultFontAsset;label.text=text;label.fontSize=size;label.color=color;label.alignment=alignment;
            label.enableAutoSizing=true;label.fontSizeMin=12;label.fontSizeMax=size;label.raycastTarget=false;label.overflowMode=TextOverflowModes.Ellipsis;return label;
        }
        static void Dispose(UnityEngine.Object value) { if(Application.isPlaying)Destroy(value);else DestroyImmediate(value); }
    }
}
