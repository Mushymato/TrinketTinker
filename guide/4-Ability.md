# Ability

Ability describes the effects that occur when you equip the trinket, like getting healed, or attacking an enemy.

An ability is primarily defined by `AbilityClass` (what it does) and `Proc` (when does it activate).

## Structure

| Property | Type | Default | Notes |
| -------- | ---- | ------- | ----- |
| `AbilityClass` | string | DebugDummy | Type name of the motion class to use, can use short name like `Buff`. <br>Refer to page titles under "Ability Classes" in the table of contents for details. |
| `Description` | string | _empty_ | String of the ability, will be used to substitute `"{1}"` in a [trinket's](0-Trinket.md) description. |
| `Proc` | [ProcOn](4.0-Proc.md) | Footstep | Make ability activate when something happens. |
| `ProcTimer` | double | -1 | After an ability proc, prevent further activations for this amount of time. |
| `ProcSound` | string | _empty_ | Play a sound cue when ability procs ([details](https://stardewvalleywiki.com/Modding:Audio)) |
| `ProcTAS` | `List<string>` | _empty_ | String Id of [temporary animated sprites](5-TAS.md) to show when the ability activates. |
| `Condition` | string | _empty_ | A [game state query](https://stardewvalleywiki.com/Modding:Game_state_queries) that must pass before proc. |
| `DamageThreshold` | int | -1 | Must receive or deal this much damage before proc.<br>For ReceiveDamage & DamageMonster |
| `IsBomb` | bool? | _empty_ | Must deal damage with(true)/not with(false) a bomb.<br>For DamageMonster & SlayMonster, |
| `IsCriticalHit` | bool? | _empty_ | Must (true)/must not(false) deal a critical hit.<br> |
| `Args` | Dictionary | _varies_ | Arguments specific to an ability class, see respective page for details. |

## Note

- Abilities are internally 0-indexed, but the displayed minimum ability can be changed in [TinkerData](1-Tinker.md)
- When there are multiple abilities per level and a description for each, they will be joined with new line before passed to description template.
- Example of how description template works:
    ```json
    // Trinket
    "Description": "My trinket {0}:\n{1}"
    // Tinker
    "MinLevel": 2
    // Tinker Abilities
    [
        [   // rest of ability omitted
            {"Description": "first ability A", ...},
            {"Description": "second ability B", ...}
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
    first ability A
    second ability B
    ```
    Description when trinket has internal level 1:
    ```
    My trinket 3:
    first ability C
    second ability D
    ```
