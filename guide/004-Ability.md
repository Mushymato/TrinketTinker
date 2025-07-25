# Ability

Ability describes the effects that occur while the trinket is equipped. Such the healing the player, harvesting a crop, attacking an enemy, and so on.

An ability is primarily defined by `AbilityClass` (what it does) and `Proc` (when does it activate). Following the successful proc of an ability, a number of proc effects can happen.

```json
{
  "Action": "EditData",
  "Target": "mushymato.TrinketTinker/Tinker",
  "TargetField": [
    "{{ModId}}_Sample",
  ],
  "Entries": {
    "Abilities": [
      // lvl 1 abilities
      [
        // 1st ability for lvl 1
        {
          "Id": "<string ability id>",
          "AbilityClass": "<string ability class>",
          "Description": null|"<string description class>",
          "Proc": "<emu proc on>",
          "ProcTimer": <double timer miliseconds>,
          "ProcSyncId": null|"<string proc sync ability id>",
          "ProcSyncIndex": <int proc sync index in list>,
          "ProcSyncDelay": 0,
          "ProcFuel": {
            "RequiredItemId": "<string qualified item id>",
            "RequiredTags": [/* string context tags */],
            "Condition": "<string game state query>",
            "RequiredCount" <int required item count>
          },
          "ProcSound": {
            "CueName": "<string sound cue>",
            "Pitch": [/* int pitch values */],
          }, // OR just "<string sound cue>"
          "ProcTAS": [/* TemporaryAnimatedSprite ids */],
          "ProcOneshotAnim": "<string anim clip key>",
          "ProcSpeechBubble": "<string speech bubble key>",
          "ProcAltVariant": "<string alt variant key>",
          "ProcChatterKey": "<string next chatter key>",
          "Condition": "<string game state query>",
          "DamageThreshold": <int damage threshold (for ReceiveDamage|DamageMonster only)>,
          "IsBomb": <bool damage came from explosion (for DamageMonster|SlayMonster only)>,
          "IsCriticalHit": <bool if damage is a crit>,
          "InCombat": <bool if player is in combat>,
          "Args": {
              /* AbilityClass dependent arguments */
          }
        },
        { /* another lvl 1 ability */}
      ],
      [ /* lvl 2 abilities */ ],
      //and so on...
    ]
  }
}
```

## Structure

| Property | Type | Default | Notes |
| -------- | ---- | ------- | ----- |
| `Id` | string | _same as `AbilityClass`_ | The Id of this ability, used for `ProcSyncId` and content patcher EditData. |
| `AbilityClass` | string | `"Nop"` | Type name of the motion class to use, can use short name like `"Buff"`. <br>Refer to docs under "Ability Classes" in the table of contents for details. |
| `Description` | string | _null_ | String of the ability, will be used to substitute `"{1}"` in a [trinket's](000-Trinket.md) description. |
| `Proc` | [ProcOn](004.0-Proc.md) | Footstep | Make ability activate (proc) when something happens. |
| `ProcTimer` | double | -1 | After an ability proc, prevent further activations for this amount of time. |
| `ProcSyncId`| string | _null_ | For use with [Proc.Sync](004.0-Proc.md), makes this ability proc after another ability in the same level, by their Id. If set, this field takes precedence over `ProcSyncIndex`. |
| `ProcSyncIndex`| int | 0 | For use with [Proc.Sync](004.0-Proc.md), makes this ability proc after another ability in the same level, by their order in the list of abilities. |
| `ProcSyncDelay`| int | 0 | For use with other abilities with [Proc.Sync](004.0-Proc.md), add a delay between the proc of this ability and any sync ability listening to this one. |
| `ProcFuel` | RequiredFuelData | _null_ | A model defining what items will be consumed by each activation of this ability. The fule item comes from a [trinket inventory](005.0-Inventory.md). |
| `ProcSound` | string | _null_ | Play a sound cue when ability procs ([details](https://stardewvalleywiki.com/Modding:Audio)) |
| `ProcTAS` | List\<string\> | _null_ | String Ids of [temporary animated sprites](006-Temporary%20Animated%20Sprite.md) to show when the ability activates. For most abilities, this TAS is drawn shown relative to the farmer. For Hitscan/Projectile, this TAS is shown on the targeted monster instead. |
| `ProcOneshotAnim` | string | _null_ | Play the matching [anim clip](003.2-Animation%20Clips.md) on proc, return to normal animation after 1 cycle. |
| `ProcSpeechBubble` | string | _null_ | Show the matching [speech bubble](003.3-Speech%20Bubbles.md) on proc. |
| `ProcAltVariant` | string | _null_ | Switch the companion to the matching [alt variant](002-Variant.md) on proc. Use `"RECHECK"` to switch |
| `ProcChatterKey` | string | _null_ | On the next activation of a [Chatter ability](005.1-Chatter.md), use this key instead of the normal conditional key. |
| `Condition` | string | _null_ | A [game state query](https://stardewvalleywiki.com/Modding:Game_state_queries) that must pass before proc. |
| `DamageThreshold` | int | -1 | Must receive or deal this much damage before proc.<br>For ReceiveDamage & DamageMonster |
| `IsBomb` | bool? | _null_ | Must deal damage with(true)/not with(false) a bomb.<br>For DamageMonster & SlayMonster |
| `IsCriticalHit` | bool? | _null_ | Must (true)/must not(false) deal a critical hit.<br>For DamageMonster & SlayMonster |
| `InCombat` | bool? | _null_ | Must (true)/must not(false) be in combat.<br>"In combat" is defined as there being monsters in the same location, and the player has or taken damage in the last 10 seconds. |
| `Args` | Dictionary | _varies_ | Arguments specific to an ability class, see respective page for details. |

## Ability Descriptions

Abilities are internally 0-indexed, but the displayed minimum ability can be changed in [TinkerData](001-Tinker.md)

Then there are multiple abilities per level and a description for each, they will be joined with new line before passed to description template.

It's not neccesary to provide descriptions to all abilities in a level, often it is sufficient to describe the entire ability in 1 description on the first ability, while leaving the others blank.

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

## Fuel Item

| Property | Type | Default | Notes |
| -------- | ---- | ------- | ----- |
| `RequiredItemId` | string | _null_ | Required item id, to target a specific item. |
| `RequiredTags` | List<string> | _null_ | Required item context tag, target all items that have all of these tags. |
| `Condition` | string | _null_ | Required item [game state queries](https://stardewvalleywiki.com/Modding:Game_state_queries) condition. |
| `RequiredCount` | int | Total required fuel item count. |

An item must qualify for at least one of `RequiredItemId`, `RequiredTags`, and `Condition` to be considered fuel to be consumed.
If not enough fuel items, nothing is consumed and the ability does not activate.
