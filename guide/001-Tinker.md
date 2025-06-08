# Tinker

To make a trinket use TrinketTinker features, add a new entry to the custom asset `mushymato.TrinketTinker/Tinker`.
The key used must match the __unqualified ID__ of the trinket, e.g. `{{ModId}}_Trinket` instead of `(TR){{ModId}}_Trinket`.

When a `Data/Trinkets` entry has a matching `mushymato.TrinketTinker/Tinker` entry, the `TrinketEffectClass` field on `Data/Trinkets` will be set to `TrinketTinker.Effects.TrinketTinkerEffect` from this mod.

> [!NOTE]
> Trinkets can be reloaded with `patch reload <your content mod id>`.

## Sample

```json
{
  "Action": "EditData",
  "Target": "mushymato.TrinketTinker/Tinker",
  "Entries": {
    "{{ModId}}_Sample": {
      "EnableCondition": "<game state query>",
      "EnableFailMessage": "<message>",
      "MinLevel": <number level>,
      "Variants": [
        { /* variant data */ },
        { /* more variant data */ },
        //...
      ],
      "Motion": { /* motion data */ },
      "Abilities": [
        [
          // first level abilities
          { /* ability data */ },
          { /* more ability data */ }
        ],
        [
          // second level abilities
          { /* ability data */ },
          { /* more ability data */ }
        ],
        //...
      ],
      "VariantUnlockConditions": [
        null,
        "<game state query>",
        // ...
      ],
      "AbilityUnlockConditions": [
        null,
        "<game state query>",
        // ...
      ],
      "Inventory": { /* inventory data */ },
      "Chatter": {
        "<chatter key 1>": { /* chatter data */ },
        "<chatter key 2>": { /* chatter data */ },
        //...
      },
    }
  }
}
```

### This is a lot of stuff, what do I actually need to have?

Trinket Tinker data fields are optional unless explictly marked as **required** in the tables that describe what each field does. This applies even to the top level TinkerData, but skipping every field means there's little point to using this framework at all.

To display a companion, you need a `Motion` and at least 1 entry in `Variants`.

To have the trinket do things after being equipped, at least 1 list of `Abilities` must be defined. `Inventory` and `Chatter` abilities also require the top level `Inventory` and `Chatter` data, refer to their subpages for details.

Unlike base game trinkets, TrinketTinker trinkets always spawn with the first variant and at minimum level. The item query [mushymato.TrinketTinker_CREATE_TRINKET](007-Utility.md) is needed to create trinket at other variants/levels.

## Structure

| Property | Type | Default | Notes |
| -------- | ---- | ------- | ----- |
| `EnableCondition` | string | _null_ | A [game state query](https://stardewvalleywiki.com/Modding:Game_state_queries) used to check if the trinket should be enabled. This is checked on equip, it can only be rechecked by reequipping the trinket. The check also happens every night, when the trinket is unequipped/reequipped by the game. |
| `EnableFailMessage` | string | _null_ | When `EnableCondition` is false, this message will be displayed upon equipping the trinket. Supports tokenized text.<br/>Default message: ` "You are not worthy of {{trinketName}}..."` |
| `MinLevel` | int | 1 | Changes the level value that will replace `{0}` in `DisplayName`. |
| `Variants` | [List\<VariantData\>](002-Variant.md) | _null_ | Defines the sprites of the companion. |
| `Motion` | [MotionData](003-Motion.md) | _null_ | Defines how the companion moves. |
| `Abilities` | [List\<List\<AbilityData\>\>](004-Ability.md) | _null_ | Defines what effects are activated, and when. Each list in the list of lists represents 1 ability level. |
| `VariantUnlockConditions` | List\<string\> | _null_ | List of [game state queries](https://stardewvalleywiki.com/Modding:Game_state_queries) that determine how many variants are unlocked. |
| `AbilityUnlockConditions` | List\<string\> | _null_ | List of [game state queries](https://stardewvalleywiki.com/Modding:Game_state_queries) that determine how many abilities are unlocked. 
| `Inventory` | [TinkerInventoryData](005-Inventory.md) | _null_ | Gives the trinket an inventory that can be opened by the "use" button (RightClick/X) over the trinket item. |
| `Chatter` | [Dictionary\<string, ChatterLinesData\>](004.z.201-Chatter.md) | _null_ | Gives the trinket dialogue for use with the [Chatter ability](004.z.201-Chatter.md). |

### DEPRECATED
- `Motions`, previously a list of `MotionData` that is unused except for the first element. It has been removed since 1.5.0, please use only `Motion` from now on

### Unlock Conditions

`VariantUnlockConditions` and `AbilityUnlockConditions` can prevent the player from rolling variants or abilities above a certain level using [game state queries](https://stardewvalleywiki.com/Modding:Game_state_queries). This only affects rerolling level and variants on the [anvil](https://stardewvalleywiki.com/Anvil) and [colorizer](007-Utility.md).

Example usage with 4 abilities (lv1 to lv4):

```json
{
  "Action": "EditData",
  "Target": "mushymato.TrinketTinker/Tinker",
  "Fields": {
    "{{ModId}}_Sample": {
      "AbilityUnlockConditions": [
          // level 1 is always unlocked
          // level 2 is unconditionally unlocked
          null,
          // level 3 unlocked if player has a gold ore in inventory
          "PLAYER_HAS_ITEM Current (O)384",
          // level 4 is also unconditionally unlocked once 3 is unlocked
          null,
          // there is no level 5, so this value is meaningless
          "FALSE",
      ],
    }
  }
}
```

## Integrating Trinket Tinker

As a custom asset based framework, you can put all trinket related edits in a json separate from the rest of the mod like so. This helps turn trinkets into an optional feature for your mod.

```json
{
  "Action": "Include",
  "FromFile": "data/trinkets.json",
  "When": {
    "HasMod": "mushymato.TrinketTinker"
  }
}
```

Special behavior as an equipment aside, trinkets are items with qualified item id like `(TR){{ModId}}_Sample`. Besides the base game random drops after combat mastery (if `DropsNaturally` is set to true), you can grant a trinket to the player via any usual means of granting an item via their qualified item id. This includes shops, events, machine output, Farm Type Manager/Spacecore monster drops, and so on. There are also [means of using a trinket without having a real item the player must obtain](007.2-Actions.md) which allows you to utilize Trinket Tinker's system to create followers.
