# Tinker

To make a trinket use TrinketTinker features, add a new entry to the custom asset `mushymato.TrinketTinker/Tinker`.
The key used must match the __unqualified ID__ of the trinket.

When a `mushymato.TrinketTinker/Tinker` entry exists, the `TrinketEffectClass` will be set to `TrinketTinker.Effects.TrinketTinkerEffect` from this mod.

> [!NOTE]
> Trinkets can be reloaded with `patch reload <content mod id>`, however it must be removed and equipped again to do anything.

## Structure

| Property | Type | Default | Notes |
| -------- | ---- | ------- | ----- |
| `MinLevel` | int | 1 | Changes the level value that will replace `{0}` in `DisplayName`. |
| `Variants` | [List\<VariantData\>](2-Variant.md) | _empty_ | Defines the sprites of the companion. |
| `Motion` | [MotionData](3-Motion.md) | _empty_ | Defines how the companion moves, setting this is shortcut for having a single item in `Motions`. |
| `Motions` | [List\<MotionData\>](3-Motion.md) | _empty_ | Defines how the companion moves. |
| `Abilities` | [List\<List\<AbilityData\>\>](4-Ability.md) | _empty_ | Defines what effects are activated, and when. Each list in the list of lists represents 1 ability level. |
| `VariantUnlockConditions` | List<string> | _empty_ | List of [game state queries](https://stardewvalleywiki.com/Modding:Game_state_queries) that determine how many variants are unlocked. |
| `AbilityUnlockConditions` | List<string> | _empty_ | List of [game state queries](https://stardewvalleywiki.com/Modding:Game_state_queries) that determine how many abilities are unlocked. |


### This is a lot of stuff, what do I need to define?

Technically all fields here are optional, but in that case there'd be little point to using this framework at all. To display a companion, at least 1 Variant and 1 Motion must be defined. To have the trinket do things, at least 1 list of abilities must be defined.

At the moment, there's no reason to have more than 1 Motion so just setting `Motion` is sufficient, but this may change in the future.

Trinkets are created with the first variant and at minimum level. The item query [mushymato.TrinketTinker_CREATE_TRINKET](6-Item%20Queries.md) is needed to create trinket at other variants/levels.

### Unlock Conditions

`VariantUnlockConditions` and `AbilityUnlockConditions` can prevent the player from rolling variants or abilities above a certain level using [game state queries](https://stardewvalleywiki.com/Modding:Game_state_queries). This only affects rerolling level and variants on the [anvil](https://stardewvalleywiki.com/Anvil) and [colorizer](7-Trinket%20Colorizer.md).

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
