#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using System.Linq;
using DungeonBDX.Skills;
using TMPro;
using UnityEngine;

namespace DungeonBDX
{
    /// <summary>Development player probe. Presentation commands never touch campaign state.</summary>
    public sealed class DungeonPresentationSmoke
    {
        [Serializable]
        public sealed class Report
        {
            public bool passed, cameraRotated, cameraZoomed, cameraReset, actorPicking,
                movementRange, skillRange, petRangeOrigin, rejectedActionFeedback, idleAdvanced, attackPlayed, returnedToIdle, domainUnchanged;
            public int movementCells, skillCells, spriteActors, width, height;
            public string spriteActor, idleBefore, idleAfter, attackSprite, mode, error;
            public float yaw, zoom;
        }

        readonly DungeonExpedition expedition;
        readonly string path;
        readonly Report report;
        DungeonActorAnimation actor;
        string initialState;
        Sprite initialSprite;

        public DungeonPresentationSmoke(DungeonExpedition expedition, string imagePath, string mode)
        {
            this.expedition = expedition;
            path = Path.ChangeExtension(imagePath, ".presentation.json");
            report = new Report { mode = mode, width = Screen.width, height = Screen.height };
        }

        public void Begin()
        {
            initialState = expedition.Session.PersistenceState;
            var world = expedition.WorldView;
            world.FitCamera();
            float baseline = world.ViewCamera.orthographicSize;
            world.RotateCamera(43);
            report.cameraRotated = Mathf.Abs(Mathf.DeltaAngle(0, world.CameraYaw)) > 40;
            world.ZoomCamera(1.25f);
            report.cameraZoomed = world.CameraZoom > 1 && world.ViewCamera.orthographicSize > 0;
            world.PanCamera(new Vector2(24, 0));
            var current = expedition.Session.Unit(expedition.Session.ActiveActorId);
            Vector2 foot = world.ViewCamera.WorldToScreenPoint(new Vector3(current.Cell.X, .05f, current.Cell.Y));
            report.actorPicking = world.Pick(foot, out var picked) && picked.Equals(current.Cell);
            world.FitCamera();
            report.cameraReset = Mathf.Abs(world.CameraYaw) < .001f && Mathf.Abs(world.CameraZoom - 1) < .001f
                && Mathf.Abs(world.ViewCamera.orthographicSize - baseline) < .001f;
            report.movementRange = expedition.SelectForPreview(movement: true)
                && world.Preview != null && world.Preview.Movement && world.Preview.Cells.Count > 0;
            report.movementCells = world.Preview?.Cells.Count ?? 0;
            string petId = expedition.Session.PetId(current.Id);
            string skill = petId != null ? "dbdx.draco.comandar" :
                DungeonProfiles.Resolve(current.ProfileId).SkillIds.FirstOrDefault(id =>
                    DungeonSkillCatalog.All[id].Target == SkillTarget.Enemy &&
                    DungeonSkillCatalog.All[id].Cost <= expedition.Session.RemainingActions);
            report.skillRange = skill != null && expedition.SelectForPreview(skill)
                && world.Preview != null && !world.Preview.Movement && world.Preview.Cells.Count > 0;
            report.skillCells = world.Preview?.Cells.Count ?? 0;
            report.petRangeOrigin = petId == null || world.Preview.Origin.Equals(expedition.Session.Unit(petId).Cell);
            var distant = expedition.Session.Units.FirstOrDefault(u => u.Alive && u.Team != current.Team &&
                GridCell.Distance(world.Preview.Origin, u.Cell) > DungeonSkillCatalog.All[skill].Range);
            if (distant == null) throw new InvalidOperationException("Fixture sem alvo fora de alcance para conferir feedback.");
            expedition.ClickCell(distant.Cell);
            report.rejectedActionFeedback = expedition.HudCanvas.GetComponentsInChildren<TextMeshProUGUI>()
                .Any(label => label.text.Contains("Alvo fora do alcance."));
            expedition.SelectForPreview(skill);
            var sprites = world.GetComponentsInChildren<DungeonActorAnimation>()
                .Where(a => a.CurrentSprite && a.AnimationFrameCount > 1).ToArray();
            report.spriteActors = sprites.Length;
            actor = sprites.FirstOrDefault();
            if (!actor) throw new InvalidOperationException("Nenhum sprite animado real na expedição.");
            report.spriteActor = actor.gameObject.name;
            initialSprite = actor.CurrentSprite;
            report.idleBefore = initialSprite.name;
        }

        public void ObserveIdleAndAttack()
        {
            report.idleAfter = actor.CurrentSprite ? actor.CurrentSprite.name : "";
            report.idleAdvanced = actor.CurrentSprite && actor.CurrentSprite != initialSprite && actor.State == "idle";
            actor.PlayAction(Vector3.right);
        }

        public void ObserveAttackAndPrepareCapture()
        {
            report.attackPlayed = actor.State == "attack" && actor.CurrentSprite && actor.AnimationFrameCount > 1;
            report.attackSprite = actor.CurrentSprite ? actor.CurrentSprite.name : "";
            if (report.mode == "rotate")
            {
                expedition.RotateCamera(43);
                expedition.ZoomCamera(1.2f);
            }
            else if (report.mode == "range") expedition.SelectForPreview(movement: true);
            report.yaw = expedition.WorldView.CameraYaw;
            report.zoom = expedition.WorldView.CameraZoom;
        }

        public bool Complete(Exception failure = null)
        {
            report.returnedToIdle = actor && actor.State == "idle";
            report.domainUnchanged = initialState != null && initialState == expedition.Session.PersistenceState;
            report.error = failure?.Message;
            report.passed = failure == null && report.cameraRotated && report.cameraZoomed && report.cameraReset
                && report.actorPicking && report.movementRange && report.skillRange && report.petRangeOrigin
                && report.rejectedActionFeedback && report.idleAdvanced
                && report.attackPlayed && report.returnedToIdle && report.domainUnchanged;
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
            Debug.Log("DUNGEONBDX_PRESENTATION_" + (report.passed ? "OK" : "FAILED"));
            return report.passed;
        }
    }
}
#endif
