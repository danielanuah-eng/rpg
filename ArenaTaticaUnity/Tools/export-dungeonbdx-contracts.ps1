param()
$ErrorActionPreference = 'Stop'
if ($PSVersionTable.PSVersion.Major -lt 7) { throw 'Execute com pwsh 7 ou superior.' }
$dungeonProject = Split-Path $PSScriptRoot -Parent
$dungeonRepo = Split-Path $dungeonProject -Parent
$dungeonAssemblies = Join-Path $dungeonProject 'Library/ScriptAssemblies'
$dungeonCombatDll = Join-Path $dungeonAssemblies 'DungeonBDX.Combat.dll'
$dungeonSkillsDll = Join-Path $dungeonAssemblies 'DungeonBDX.Skills.dll'
foreach ($dungeonDll in @($dungeonCombatDll, $dungeonSkillsDll)) {
    if (-not (Test-Path -LiteralPath $dungeonDll)) { throw 'Compile e teste o projeto Unity antes de exportar.' }
    [Reflection.Assembly]::LoadFrom($dungeonDll) | Out-Null
}

function Get-DungeonFactorial([int]$number) {
    [long]$value = 1
    for ($factor = 2; $factor -le $number; $factor++) { $value *= $factor }
    return $value
}
function Get-DungeonFraction([long]$numerator, [long]$denominator) {
    $left = $numerator; $right = $denominator
    while ($right -ne 0) { $remainder = $left % $right; $left = $right; $right = $remainder }
    return ('{0}/{1}' -f ($numerator / $left), ($denominator / $left))
}

$dungeonEmptyMods = [DungeonBDX.Combat.DamageModifiers]::Combine($null)
$dungeonMaxMods = [DungeonBDX.Combat.DamageModifiers]::Combine(
    [DungeonBDX.Combat.DamageModifier[]]@([DungeonBDX.Combat.DamageModifier]::new([int]::MaxValue, [int]::MaxValue)))
$dungeonOutcomes = @{}
foreach ($dungeonDice in @([DungeonBDX.Skills.DungeonSkillCatalog]::All.Values | Where-Object Dice -gt 0 | Select-Object -ExpandProperty Dice -Unique)) {
    $states = @{}; $needed = ($dungeonDice + 1) * ($dungeonDice + 2) / 2
    # Find a real immutable roll for each (hits,critical) state, then weight states exactly.
    # This is enumeration, not a Monte Carlo expected-value estimate. Guard missing states.
    for ([ulong]$seed = 0; $seed -lt 100000 -and $states.Count -lt $needed; $seed++) {
        $roll = [DungeonBDX.Combat.CombatMath]::RollAttack($dungeonDice, 0, [DungeonBDX.Combat.DungeonRandom]::new($seed))
        $states[('{0}:{1}' -f $roll.Hits, $roll.Criticals)] = $roll
    }
    if ($states.Count -ne $needed) { throw "Nao foi possivel enumerar todos os estados de $dungeonDice dados." }
    $dungeonOutcomes[$dungeonDice] = $states
}

$dungeonSkills = @(foreach ($skill in [DungeonBDX.Skills.DungeonSkillCatalog]::All.Values | Sort-Object Id) {
    $maxBase = 0; $maxCapped = 0; $maxTurn = 0; $expected = '0/1'; $expectedArea = '0/1'
    if ($skill.Dice -gt 0) {
        [long]$sum = 0; [long]$areaSum = 0; [long]$weightSum = 0
        foreach ($roll in $dungeonOutcomes[$skill.Dice].Values) {
            $misses = $skill.Dice - $roll.Hits; $normal = $roll.Hits - $roll.Criticals
            [long]$ways = (Get-DungeonFactorial $skill.Dice) / ((Get-DungeonFactorial $misses) * (Get-DungeonFactorial $normal) * (Get-DungeonFactorial $roll.Criticals))
            $ways *= [long][Math]::Pow(2, $misses) * [long][Math]::Pow(3, $normal)
            $raw = [DungeonBDX.Combat.CombatMath]::RawDamage($roll, $skill.Power, $dungeonEmptyMods)
            $sum += $ways * $raw
            $areaSum += $ways * [Math]::Min([DungeonBDX.Combat.CombatCaps]::DamagePerAction, $raw * $skill.MaxTargets)
            $weightSum += $ways
        }
        [long]$denominator = [Math]::Pow(6, $skill.Dice)
        if ($weightSum -ne $denominator) { throw 'Peso da enumeracao de dados inconsistente.' }
        $expected = Get-DungeonFraction $sum $denominator
        $expectedArea = Get-DungeonFraction $areaSum $denominator
        $critical = $dungeonOutcomes[$skill.Dice][('{0}:{0}' -f $skill.Dice)]
        $baseRaw = [DungeonBDX.Combat.CombatMath]::RawDamage($critical, $skill.Power, $dungeonEmptyMods)
        $maxRaw = [DungeonBDX.Combat.CombatMath]::RawDamage($critical, $skill.Power, $dungeonMaxMods)
        $maxBase = [Math]::Min([DungeonBDX.Combat.CombatCaps]::DamagePerAction, $baseRaw * $skill.MaxTargets)
        $maxCapped = [Math]::Min([DungeonBDX.Combat.CombatCaps]::DamagePerAction, $maxRaw * $skill.MaxTargets)
        $turn = [DungeonBDX.Combat.TurnBudget]::new()
        for ($use = 0; $use -lt $skill.UsesPerTurn; $use++) {
            $action = $null
            if (-not $turn.TryBeginAction($skill.Cost, [ref]$action)) { break }
            for ($target = 0; $target -lt $skill.MaxTargets; $target++) {
                $maxTurn += $action.Spend([DungeonBDX.Combat.CombatResource]::Damage, $maxRaw)
            }
            $action.Dispose()
        }
    }
    [ordered]@{
        id=$skill.Id; name=$skill.Name; family=$skill.Id.Split('.')[1]; effects=@($skill.Effects.ToString().Split(',') | ForEach-Object Trim)
        target=$skill.Target.ToString(); cost=$skill.Cost; cooldownOwnerTurns=$skill.Cooldown; minRange=$skill.MinRange; range=$skill.Range
        maxTargets=$skill.MaxTargets; areaRadius=$skill.AreaRadius; dice=$skill.Dice; power=$skill.Power; penetration=$skill.Penetration
        healing=$skill.Healing; shield=$skill.Shield; steps=$skill.Steps; durationFutureOwnerTurns=$skill.Duration; usesPerTurn=$skill.UsesPerTurn
        requiresPet=$skill.RequiresPet; teleport=$skill.Teleport
        animationId=$skill.AnimationId; soundId=$skill.SoundId; iconId=$skill.IconId
        budget=[ordered]@{ noArmorNoBuffExpectedPerTarget=$expected; noArmorNoBuffExpectedMaxTargetSet=$expectedArea
            allCriticalBaseMaxRawPerAction=$maxBase; allCriticalWithCappedModifiersMaxRawPerAction=$maxCapped
            repeatedSkillWithCappedModifiersMaxRawPerOwnerTurn=$maxTurn; maxAcceptedFaceRollsWithAim=2*$skill.Dice
            healingPerAction=[Math]::Min(12,$skill.Healing*$skill.MaxTargets); shieldPerAction=[Math]::Min(8,$skill.Shield*$skill.MaxTargets) }
    }
})
$dungeonSourceHeroes = @(foreach ($entry in [DungeonBDX.Skills.DungeonHeroProfiles]::ArenaSources.GetEnumerator() | Sort-Object Key) {
    # Only ID/class/loadout are exported for imports: actual source stats are runtime inputs.
    $profile = [DungeonBDX.Skills.DungeonHeroProfiles]::AdaptArena($entry.Key, $entry.Key, 10, 0, 3, $null)
    [ordered]@{ id=$profile.Id; archetype=$profile.Archetype.ToString(); skillIds=@($profile.SkillIds); cardId='dbdx.card.'+$profile.Id }
})
$dungeonNewHeroes = @(foreach ($profile in [DungeonBDX.Skills.DungeonHeroProfiles]::NewHeroes) {
    [ordered]@{ id=$profile.Id; name=$profile.Name; archetype=$profile.Archetype.ToString(); health=$profile.Health; armor=$profile.Armor
        movement=$profile.Movement; actions=$profile.Actions; skillIds=@($profile.SkillIds); cardId='dbdx.card.'+$profile.Id
        modelId='dbdx.hero.'+$profile.Id; assetStatus='requested-stage-4' }
})
$dungeonContract = [ordered]@{
    version=[DungeonBDX.Skills.DungeonSkillCatalog]::Version; balanceVersion=[DungeonBDX.Combat.CombatCaps]::Version
    executor='Codex; reassignment explicitly authorized by author'; schema='dungeonbdx-skills-1'
    skills=$dungeonSkills; arenaHeroes=$dungeonSourceHeroes; newHeroes=$dungeonNewHeroes
    conversion=[ordered]@{ version=[DungeonBDX.Skills.DungeonHeroProfiles]::ConversionVersion
        health='clamp(24+2*sourceHP,40,60)'; armor='clamp(10+20*sourceArmor+(sourceHP>=14?10:0),0,50)'; movement='clamp(sourcePM,2,5)'; actions=3
        source='Explicit adapter inputs, snapshot source skill IDs; no mutation, no 1:1 port of old effects. Base/forms/passives retain their Arena definitions.' }
    execution=[ordered]@{ oneAttackRollPerCast=$true; targetOrder='targetIds[0] is the selected area center; budget distribution uses Ordinal unit ID independently'
        cooldown='C>0 skips the next C owner turns; C=0 still honors per-turn use and AP caps'
        statuses='Duration 1: expires at start of target owner tick current+2; refreshing does not stack potency; pets use owner clock'
        aim='Next attack gains at most one miss reroll per die and additive 1500 BP; consumed even on miss'
        pet='One 12HP/5armor companion per owner; paid commands share owner AP, RNG, cooldown and raw budgets; no independent turn; death/recall never refunds or resets cooldown'
        modules='Companions, Control, Teleport and FriendlyFire; mandatory combat caps cannot be disabled'
        authority='Domain engine only. Caller must authenticate/serialize commands and validate turn ownership in stage 5.'
        integration='No combat binding to preview scene/HUD yet; stages 5-6 consume immutable outcomes and animation/SFX IDs'
        assets='Bindings are requests; no models, VFX, SFX or music delivered in stage 3'
        budget='Exact no-reroll expectations enumerate hit/critical outcome weights through compiled core. Capped modifier bounds are mathematical ceilings, not all reachable by every loadout. Mixed skills still share the same owner budget.' }
    compiledAssemblies=[ordered]@{ combat=(Get-FileHash -LiteralPath $dungeonCombatDll).Hash.ToLowerInvariant(); skills=(Get-FileHash -LiteralPath $dungeonSkillsDll).Hash.ToLowerInvariant() }
}
$dungeonOutput = Join-Path $dungeonRepo 'coordenacao/dungeonbdx/contratos/skills.json'
$dungeonJson = $dungeonContract | ConvertTo-Json -Depth 10
[IO.File]::WriteAllText($dungeonOutput, $dungeonJson.Replace("`r",'')+"`n", [Text.UTF8Encoding]::new($false))
Write-Output ('Exportado: {0} skills, {1} adaptacoes Arena, {2} novos perfis.' -f $dungeonSkills.Count,$dungeonSourceHeroes.Count,$dungeonNewHeroes.Count)
