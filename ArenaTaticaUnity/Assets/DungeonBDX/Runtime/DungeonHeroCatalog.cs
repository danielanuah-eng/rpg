using System;
using System.Collections.Generic;
using System.Linq;
using ArenaTatica.Core;
using DungeonBDX.Skills;
using UnityEngine;

namespace DungeonBDX
{
    // Presentation snapshots: source combat definitions are never changed here.
    public sealed class DungeonHero
    {
        public string Id { get; }
        public string Name { get; }
        public string SpriteFolder { get; }
        public int Health { get; }
        public int Actions { get; }
        public int Movement { get; }
        public int Armor { get; }
        public IReadOnlyList<DungeonAction> Skills { get; }

        internal DungeonHero(FighterDef source)
        {
            Id = source.id; Name = source.name; SpriteFolder = source.sprites;
            Health = source.hp; Actions = source.pa; Movement = source.pm; Armor = source.armor;
            Skills = Array.AsReadOnly((source.skills ?? Array.Empty<SkillDef>())
                .Select(s => new DungeonAction(s)).ToArray());
        }

        internal DungeonHero(DungeonHeroProfile profile)
        {
            Id = profile.Id; Name = profile.Name; Health = profile.Health; Armor = profile.Armor;
            Movement = profile.Movement; Actions = profile.Actions;
            Skills = Array.AsReadOnly(profile.SkillIds.Select(id => new DungeonAction(DungeonSkillCatalog.All[id])).ToArray());
        }

        public Sprite LoadPortrait()
        {
            if (string.IsNullOrEmpty(SpriteFolder)) return null;
            return Resources.Load<Sprite>("Sprites/" + SpriteFolder + "/south")
                ?? Resources.Load<Sprite>("Sprites/Loja/" + Id);
        }
    }

    public sealed class DungeonAction
    {
        public string Id { get; }
        public string Name { get; }
        public int Cost { get; }
        public string Description { get; }

        internal DungeonAction(SkillDef skill)
        {
            Id = skill.id; Name = skill.name; Cost = skill.pa;
            Description = $"{skill.name}  •  {skill.pa} PA\nAlcance {skill.minRange}–{skill.maxRange}  •  {skill.kind}";
            if (skill.damage != 0) Description += $"\nDano base: {skill.damage}";
            if (!string.IsNullOrEmpty(skill.form)) Description += "\nForma: " + skill.form;
        }

        internal DungeonAction(DungeonSkill skill)
        {
            Id = skill.Id; Name = skill.Name; Cost = skill.Cost;
            Description = $"{skill.Name}  •  {skill.Cost} PA\nAlcance {skill.MinRange}–{skill.Range}  •  Recarga {skill.Cooldown}";
            if(skill.Has(SkillEffect.Attack))Description+=$"\n{skill.Dice} dados · Poder {skill.Power}";
            if(skill.Healing>0)Description+=$"\nCura {skill.Healing}";
            if(skill.Shield>0)Description+=$"\nEscudo {skill.Shield}";
            if(skill.Has(SkillEffect.Slow))Description+="\nAplica lentidão";
            if(skill.Has(SkillEffect.Aim))Description+="\nMira: +15% de dano e uma rerrolagem";
            if(skill.Has(SkillEffect.Summon))Description+="\nInvoca um dragão companheiro";
            if(skill.Has(SkillEffect.Recall))Description+="\nRecolhe o companheiro";
            if(skill.Has(SkillEffect.Armor))Description+="\n+10 armadura temporária";
            if(skill.Has(SkillEffect.Move))Description+=$"\n{(skill.Teleport?"Transposição":"Movimento")} até {skill.Steps} casas";
        }
    }

    public static class DungeonHeroCatalog
    {
        public static IReadOnlyList<DungeonHero> ImportAll() => Array.AsReadOnly(ImportArena()
            .Concat(DungeonHeroProfiles.NewHeroes.Select(p => new DungeonHero(p))).ToArray());

        public static IReadOnlyList<DungeonHero> ImportArena()
        {
            return Array.AsReadOnly(Content.Fighters.Values.Where(f => f.picker && f.level > 0)
                .OrderBy(f => f.name, StringComparer.Ordinal).Select(f => new DungeonHero(f)).ToArray());
        }
    }
}
