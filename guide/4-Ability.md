# Ability

Ability describes the effects that occur when you equip the trinket, like getting healed, or attacking an enemy.

An ability is primarily defined by `AbilityClass` (what it does) and `Proc` (when does it activate).

## Structure

| Property | Type | Default | Notes |
| -------- | ---- | ------- | ----- |
| `AbilityClass` | string | DebugDummy | Type name of the motion class to use, can use short name like `Buff`. <br>Refer to pages under Motion Classes in the table of contents for details. |
| `Proc` | [ProcOn](4.0-Proc.md) | Footstep | Make ability activate when something happens. |
| `ProcTimer` | double | -1 | After an ability proc, prevent further activations for this amount of time. |
| `ProcSound` | string | _empty_ | Play a sound cue when ability procs ([details](https://stardewvalleywiki.com/Modding:Audio)) |
| `ProcTemporarySprites` | `List<TemporaryAnimatedSpriteDefinition>` | Temporary animated sprites to show when ability activates ([details](https://wiki.stardewvalley.net/Modding:Machines#Audio_.26_visuals)) |
| `Condition` | string | _empty_ | A [game state query](https://stardewvalleywiki.com/Modding:Game_state_queries) that must pass before proc. |
| `DamageThreshold` | int | Must receive or deal this much before before proc.<br>For ReceiveDamage & DamageMonster |
| `IsBomb` | bool? | _empty_ | Must deal damage with(true)/not with(false) a bomb.<br>For DamageMonster & SlayMonster, |
| `IsCriticalHit` | bool? | _empty_ | Must (true)/must not(false) deal a critical hit.<br> |
| `Args` | Dictionary | _varies_ | Arguments specific to an ability class, see respective page for details. |
