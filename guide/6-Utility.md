# Utility

Extra non-trinket features provided by this mod.

## Trinket Colorizer ![Trinket Colorizer](~/images/favicon.png)

The trinket colorizer is a big craftable [machine](https://stardewvalleywiki.com/Modding:Machines) that can be purchased from [the Blacksmith](https://stardewvalleywiki.com/Blacksmith) for 50 [Gold Bars](https://stardewvalleywiki.com/Gold_Bar) once the [Anvil](https://stardewvalleywiki.com/Anvil) recipe is obtained. Trinkets must have at least 2 [variants](2-Variant.md) and `CanBeReforged` set to true in order to use the colorizer.

Consumes 15 [Omni Geode](https://stardewvalleywiki.com/Omni_Geode) to reroll the variant on a TrinketTinker trinket, no effect on vanilla trinkets.

The colorizer will never roll the same variant twice in a row. Anvils have similar rule applied to them for TrinketTinker trinkets.

Implementation wise, the colorizer is a standard machine with qualified ID `(BC)mushymato.TrinketTinker_TrinketColorizer` and a single complex output machine rule.Mods are can add more ways to obtain this machine and prepend the colorizer with special upgrade rules for their trinkets.

## [Item Queries](https://stardewvalleywiki.com/Modding:Item_queries)

These are useful for populating shops and machine output rules.

### mushymato.TrinketTinker_CREATE_TRINKET

```
mushymato.TrinketTinker_CREATE_TRINKET <trinket id> [level] [variant]
```

Create a new trinket item. If the trinket has tinker data, set level and variant. Useful for producing a trinket at specified level/variant.

Setting level or variant to "R" will randomize the level or variant.

### mushymato.TrinketTinker_CREATE_TRINKET_ALL_VARIANTS

```
mushymato.TrinketTinker_CREATE_TRINKET_ALL_VARIANTS <trinket id> [level]
```

Create all variants of a trinket (with tinker data). Useful for making a shop sell all variants of a trinket instead of making the player roll for them.

Setting level to "R" will randomize the level once, and then set trinkets created by this query to that level.

## [Data/Location](https://stardewvalleywiki.com/Modding:Location_data) CustomFields

Aside from conditions defined on a particular trinket, it's also possible to disable trinket features for a whole location using CustomFields.

```
"mushymato.TrinketTinker/disableAbilities": true|false
```
Disable trinket abilities while owner is in the location (except for always active abilities).

```
"mushymato.TrinketTinker/disableCompanions": true|false
```
Disable companion display while owner is in the location. Their position updates continue.
