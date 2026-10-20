using System;
using System.IO;
using System.Linq;
using DungeonBDX.Combat;
using DungeonBDX.Services;
using DungeonBDX.Skills;
using DungeonBDX.World;
using NUnit.Framework;
using UnityEngine;

namespace DungeonBDX.Tests
{
    public sealed class DungeonExpeditionTests
    {
        [Test]
        public void ClickingAnImportedBillboardSelectsItsOwnerCellRatherThanFloorBehindIt()
        {
            var plan=DungeonMap.Generate(42,DungeonMap.Crypt);
            var session=new DungeonSkillSession(DungeonExpedition.CreateSpawns(plan,DungeonHeroProfiles.NewHeroes.Select(h=>h.Id).ToArray(),false),plan);
            var go=new GameObject("World picking fixture");
            try
            {
                var world=go.AddComponent<DungeonWorldView>();world.Build(plan,session);
                foreach(var unit in session.Units)
                {
                    var foot=world.ViewCamera.WorldToScreenPoint(DungeonWorldView.Position(unit.Cell));
                    Assert.That(world.Pick(foot,out var picked),Is.True);Assert.That(picked,Is.EqualTo(unit.Cell));
                    var actor=go.GetComponentsInChildren<Transform>().First(t=>t.name=="Unit "+unit.Id);
                    var sprite=actor.GetComponentInChildren<SpriteRenderer>();if(!sprite)continue;
                    Assert.That(world.Pick(world.ViewCamera.WorldToScreenPoint(sprite.bounds.center),out picked),Is.True);
                    Assert.That(picked,Is.EqualTo(unit.Cell),unit.Id);
                }
            }
            finally{UnityEngine.Object.DestroyImmediate(go);}
        }
        [Test]
        public void AllNativeProfilesAndImportedProfilesResolveToRealLoadouts()
        {
            var all=DungeonHeroCatalog.ImportAll();
            Assert.That(all.Count,Is.EqualTo(43));
            foreach(var hero in all)
            {
                var profile=DungeonProfiles.Resolve(hero.Id);
                Assert.That(profile.SkillIds.All(DungeonSkillCatalog.All.ContainsKey),Is.True,hero.Id);
                Assert.That(profile.Health,Is.InRange(40,60));
            }
        }

        [Test]
        public void ClassicModeOffersOnlyPlayableHeroesAndFightsNpcs()
        {
            var playable=DungeonHeroCatalog.ImportPlayable().Select(h=>h.Id).ToArray();
            Assert.That(playable,Has.Length.EqualTo(DungeonBDX.Arcadia.AqContent.Playable.Length+DungeonHeroProfiles.NewHeroes.Count));
            Assert.That(playable,Has.No.Member("brumak").And.No.Member("nixira"));
            var map=DungeonMap.Generate(42,DungeonMap.Crypt);
            foreach(bool dragons in new[]{false,true})
                foreach(var enemy in DungeonExpedition.CreateSpawns(map,new[]{"orion-prisma"},dragons).Where(s=>s.Team==1))
                    Assert.That(DungeonBDX.Arcadia.AqContent.Playable,Has.No.Member(enemy.Profile.Id),enemy.Profile.Id);
        }

        [Test]
        public void AssetBindingsResolveForEachNativeHeroAndEverySkillAndScenario()
        {
            foreach(var name in new[]{"WorldLit","SkillUnlit"})
            {
                var material=Resources.Load<Material>("DungeonBDX/Materials/"+name);
                Assert.That(material,Is.Not.Null,name);Assert.That(material.shader,Is.Not.Null,name);
            }
            foreach(var hero in DungeonHeroProfiles.NewHeroes)
            {
                var model=Resources.Load<GameObject>("DungeonBDX/Models/"+hero.Id);
                Assert.That(model,Is.Not.Null,hero.Id);
                Assert.That(model.GetComponentsInChildren<MeshFilter>().Sum(m=>m.sharedMesh.triangles.Length/3),Is.GreaterThan(0));
            }
            foreach(var skill in DungeonSkillCatalog.All.Values)
                Assert.That(Resources.Load<AudioClip>(DungeonAudio.SoundResource(skill.Id)),Is.Not.Null,skill.SoundId);
            foreach(string scenario in new[]{DungeonMap.Crypt,DungeonMap.Library,DungeonMap.Nest})
                Assert.That(Resources.Load<AudioClip>("DungeonBDX/Audio/Music/"+scenario),Is.Not.Null,scenario);
        }

        [Test]
        public void RealProfilesMapJournalAndStorageResumeTheSamePaidMove()
        {
            var map=DungeonMap.Generate(42,DungeonMap.Crypt);
            var spawns=DungeonExpedition.CreateSpawns(map,DungeonHeroProfiles.NewHeroes.Select(h=>h.Id).ToArray(),true);
            var journal=CampaignJournal.Create(42,map.Scenario,SkillMechanics.Default,spawns,map);
            var source=journal.Session.Unit("hero0");
            var destination=map.Floors.First(c=>GridCell.Distance(source.Cell,c)==1 && !spawns.Any(s=>s.Cell.Equals(c)));
            Assert.That(journal.Move("hero0",destination),Is.EqualTo(SkillFailure.None));
            string path=Path.Combine(Path.GetTempPath(),"dungeon-integration-"+Guid.NewGuid().ToString("N"));
            try
            {
                var store=new LocalCampaignStore(path,"fixture"); store.Save(journal,0,"move-one");
                var restored=store.Load(DungeonProfiles.Resolve,(seed,id)=>DungeonMap.Generate(seed,id));
                Assert.That(restored.Journal.Session.Unit("hero0").Cell,Is.EqualTo(destination));
                Assert.That(restored.Journal.Session.RemainingActions,Is.EqualTo(2));
                Assert.That(restored.Journal.Session.RandomState,Is.EqualTo(journal.Session.RandomState));
            }
            finally { if(Directory.Exists(path)) Directory.Delete(path,true); }
        }

        [TestCase(DungeonMap.Crypt)]
        [TestCase(DungeonMap.Library)]
        [TestCase(DungeonMap.Nest)]
        public void FullBattlesFinishWithinCapsAndReplayExactly(string scenario)
        {
            var map=DungeonMap.Generate(813,scenario);
            var journal=CampaignJournal.Create(813,scenario,SkillMechanics.Default,
                DungeonExpedition.CreateSpawns(map,DungeonHeroProfiles.NewHeroes.Select(h=>h.Id).ToArray(),false),map);
            var game=journal.Session;
            for(int command=0;command<2000 && !game.Finished;command++)
            {
                var actor=game.Unit(game.ActiveActorId);bool accepted=false;
                foreach(var id in DungeonProfiles.Resolve(actor.ProfileId).SkillIds)
                {
                    var skill=DungeonSkillCatalog.All[id];if(!skill.Has(SkillEffect.Attack) || skill.Target!=SkillTarget.Enemy)continue;
                    foreach(var target in game.Units.Where(u=>u.Alive && u.Team!=actor.Team))
                        if(journal.Cast(actor.Id,id,new[]{target.Id}).Success){accepted=true;break;}
                    if(accepted)break;
                }
                if(!accepted)
                {
                    var next=DungeonTactics.Advance(game,map);
                    accepted=next.HasValue && journal.Move(actor.Id,next.Value)==SkillFailure.None;
                }
                if(!accepted) Assert.That(journal.EndTurn(actor.Id),Is.True);
                Assert.That(game.DamageSpentThisTurn,Is.LessThanOrEqualTo(CombatCaps.DamagePerTurn));
                Assert.That(game.Units.All(u=>u.Health>=0 && u.Health<=u.MaxHealth && u.Shield<=CombatCaps.ShieldCapacity),Is.True);
            }
            Assert.That(game.Finished,Is.True,"Battle stalled");
            var replay=CampaignJournal.Restore(journal.Export(),DungeonProfiles.Resolve,(seed,id)=>DungeonMap.Generate(seed,id));
            Assert.That(replay.Session.PersistenceState,Is.EqualTo(game.PersistenceState));
        }

        [TestCase(1600,900,0,0,1600,900)]
        [TestCase(900,1600,30,90,840,1430)]
        [TestCase(2400,1080,100,20,2200,1040)]
        public void CameraKeepsAllBoardCornersAboveHudAndWithinSafeArea(int w,int h,int x,int y,int sw,int sh)
        {
            var go=new GameObject("Camera fixture",typeof(Camera));
            try
            {
                var camera=go.GetComponent<Camera>();camera.orthographic=true;var safe=new Rect(x,y,sw,sh);
                DungeonWorldView.Frame(camera,11,11,safe,w,h);
                float bottom=(y+sh*(w<h?.37f:.29f))/(float)h,top=(y+sh*.9f)/h;
                foreach(float cx in new[]{-.5f,10.5f})foreach(float cz in new[]{-.5f,10.5f})foreach(float cy in new[]{0f,1.3f})
                {
                    var point=camera.WorldToViewportPoint(new Vector3(cx,cy,cz));
                    Assert.That(point.x,Is.InRange(x/(float)w,(x+sw)/(float)w));
                    Assert.That(point.y,Is.InRange(bottom,top));
                }
            }
            finally{UnityEngine.Object.DestroyImmediate(go);}
        }
    }
}
