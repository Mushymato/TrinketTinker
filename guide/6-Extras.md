# Extras

TrinketTinker provides a few extra supporting features.


## Item Query: mushymato.TrinketTinker_CREATE_TRINKET

```
mushymato.TrinketTinker_CREATE_TRINKET <trinket id> [level] [variant]
```

Creates a new trinket item. If the trinket has tinker data, set level and variant.
Useful for producing a trinket at specified level/variant.

## Machine: Trinket Colorizer ![Trinket Colorizer](~/images/favicon.png)

A new machine that can be purchased from [the Blacksmith](https://stardewvalleywiki.com/Blacksmith) for 50 [Gold Bars](https://stardewvalleywiki.com/Gold_Bar) once the [Anvil](https://stardewvalleywiki.com/Anvil) recipe is obtained. Trinkets must have at least 2 [variants](2-Variant.md) and `CanBeReforged` set to true in order to use the colorizer.

Consumes 15 [Omni Geode](https://stardewvalleywiki.com/Omni_Geode) to reroll the variant on a TrinketTinker trinket, no effect on vanilla trinkets.

The colorizer will never roll the same variant twice in a row. Anvils have similar rule applied to them for TrinketTinker trinkets.

Implementation wise, the colorizer is a standard machine with qualified ID `(BC)mushymato.TrinketTinker_TrinketColorizer` and a single complex output machine rule. Mods are free to add more ways to obtain this machine, and free to prepend the colorizer with special recipes for their trinkets.
