# Tinker

To make a trinket use TrinketTinker features, add a new entry to the custom asset `mushymato.TrinketTinker/Tinker`.
The key used must match the __unqualified ID__ of the trinket, e.g. `{{ModId}}_Trinket` instead of `(TR){{ModId}}_Trinket`.

When a `mushymato.TrinketTinker/Tinker` entry exists, the `TrinketEffectClass` field on `Data/Trinkets` will be set to `TrinketTinker.Effects.TrinketTinkerEffect` from this mod.

> [!NOTE]
> Trinkets can be reloaded with `patch reload <your content mod id>`, however the trinket must be unequipped and reequipped to get updates.
> Content Patcher tokens might not updated even through a patch reload, in those case you must either spawn a new trinket or reload the save file.

## Structure

| Property | Type | Default | Notes |
| -------- | ---- | ------- | ----- |
| `EnableCondition` | string | _null_ | A [game state query](https://stardewvalleywiki.com/Modding:Game_state_queries) used to check if the trinket should be enabled. This is checked on equip, it can only be rechecked by reequipping the trinket. The check also happens every night, when the trinket is unequipped/reequipped by the game. |
| `EnableFailMessage` | string | _null_ | When `EnableCondition` is false, this message will be displayed upon equipping the trinket.<br/>Default message: ` "You are not worthy of {{trinketName}}..."` |
| `MinLevel` | int | 1 | Changes the level value that will replace `{0}` in `DisplayName`. |
| `Variants` | [List\<VariantData\>](2-Variant.md) | _null_ | Defines the sprites of the companion. |
| `Motion` | [MotionData](3-Motion.md) | _null_ | Defines how the companion moves. |
| `Abilities` | [List\<List\<AbilityData\>\>](4-Ability.md) | _null_ | Defines what effects are activated, and when. Each list in the list of lists represents 1 ability level. |
| `VariantUnlockConditions` | List\<string\> | _null_ | List of [game state queries](https://stardewvalleywiki.com/Modding:Game_state_queries) that determine how many variants are unlocked. |
| `AbilityUnlockConditions` | List\<string\> | _null_ | List of [game state queries](https://stardewvalleywiki.com/Modding:Game_state_queries) that determine how many abilities are unlocked. |
| `Inventory` | [TinkerInventoryData](5-Inventory.md) | _null_ | Gives the trinket an inventory that can be opened by the "use" button (RightClick/X) over the trinket item. |
| `Chatter` | [Dictionary\<string, ChatterLinesData\>](4.z.201-Chatter.md) | _null_ | Gives the trinket dialogue for use with the [Chatter ability](4.z.201-Chatter.md). |

### This is a lot of stuff, what do I need to define?

Technically all fields here are optional, but in that case there'd be little point to using this framework at all. To display a companion, at least Motion and 1 Variant must be defined. To have the trinket do things after equippping, at least 1 list of abilities must be defined. For `Inventory` and `Chatter` usage, refer to their subpages.

Trinkets are created with the first variant and at minimum level. The item query [mushymato.TrinketTinker_CREATE_TRINKET](7-Utility.md) is needed to create trinket at other variants/levels.

### Unlock Conditions

`VariantUnlockConditions` and `AbilityUnlockConditions` can prevent the player from rolling variants or abilities above a certain level using [game state queries](https://stardewvalleywiki.com/Modding:Game_state_queries). This only affects rerolling level and variants on the [anvil](https://stardewvalleywiki.com/Anvil) and [colorizer](7-Utility.md).

Example usage with 4 abilities (lv1 to lv4):

```json
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
```

### Deprecated

- `Motions`, previously unused but exists, is deprecated as of 1.5.0
