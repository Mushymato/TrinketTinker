 Ability

Ability describes the effects that occur when you equip the trinket, like getting healed, or attacking an enemy.

An ability is primarily defined by `AbilityClass` (what it does) and `Proc` (when does it activate).

## Structure

| Property | Type | Default | Notes |
| -------- | ---- | ------- | ----- |
| `AbilityClass` | string | `"Nop"` | Type name of the motion class to use, can use short name like `"Buff"`. <br>Refer to docs under "Ability Classes" in the table of contents for details. |
| `Description` | string | _null_ | String of the ability, will be used to substitute `"{1}"` in a [trinket's](0-Trinket.md) description. |
| `Proc` | [ProcOn](4.0-Proc.md) | Footstep | Make ability activate (proc) when something happens. |
| `ProcTimer` | double | -1 | After an ability proc, prevent further activations for this amount of time. |
| `ProcSyncIndex`| int | 0 | For use with [Proc.Sync](4.0-Proc.md), makes this ability proc after another ability in the same level. |
| `ProcSyncDelay`| int | 0 | For use for other abilities with [Proc.Sync](4.0-Proc.md), add a delay between the proc of this ability and any sync ability listening to this one. |
| `ProcSound` | string | _null_ | Play a sound cue when ability procs ([details](https://stardewvalleywiki.com/Modding:Audio)) |
| `ProcTAS` | `List<string>` | _null_ | String Ids of [temporary animated sprites](5-Temporary%20Animated%20Sprite.md) to show when the ability activates. |
| `ProcOneshotAnim` | string | _null_ | Play the matching [anim clip](3.2-Animation%20Clips.md) on proc, return to normal animation after 1 cycle. |
| `ProcSpeechBubble` | string | _null_ | Show the matching [speech bubble](3.3-Speech%20Bubbles.md) on proc. |
| `Condition` | string | _null_ | A [game state query](https://stardewvalleywiki.com/Modding:Game_state_queries) that must pass before proc. |
| `DamageThreshold` | int | -1 | Must receive or deal this much damage before proc.<br>For ReceiveDamage & DamageMonster |
| `IsBomb` | bool? | _null_ | Must deal damage with(true)/not with(false) a bomb.<br>For DamageMonster & SlayMonster, |
| `IsCriticalHit` | bool? | _null_ | Must (true)/must not(false) deal a critical hit.<br> |
| `Args` | Dictionary | _varies_ | Arguments specific to an ability class, see respective page for details. |

## Ability Descriptions

Abilities are internally 0-indexed, but the displayed minimum ability can be changed in [TinkerData](1-Tinker.md)

-hen there are multiple abilities per level and a description for each, they will be joined with new line before passed to description template.

It's not neccesary to provide descriptions to all abilities in a level, often it is easier to describe the entire ability in 1 description on the first ability, while leaving the others blank.

Example of a description template for a trinket with 2 ability levels:
```json
// Trinket
"Description": "My trinket {0}:\n====\n{1}"
// Tinker
"MinLevel": 2
// Tinker Abilities
[
    [   // rest of ability omitted
        {"Description": "first ability A", ...},
        {"Description": "second ability B", ...},
    ],
    [   // rest of ability omitted
        {"Description": "first ability C", ...},
        {"Description": "second ability D", ...}
    ]
]
```

Description when trinket has internal level 0:
```
My trinket 2:
====
first ability A
second ability B
```

Description when trinket has internal level 1:
```
My trinket 3:
====
first ability C
second ability D
```
