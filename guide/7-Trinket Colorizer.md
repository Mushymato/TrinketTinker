# Trinket Colorizer ![Trinket Colorizer](~/images/favicon.png)

The trinket colorizer is a big craftable [machine](https://stardewvalleywiki.com/Modding:Machines) that can be purchased from [the Blacksmith](https://stardewvalleywiki.com/Blacksmith) for 50 [Gold Bars](https://stardewvalleywiki.com/Gold_Bar) once the [Anvil](https://stardewvalleywiki.com/Anvil) recipe is obtained. Trinkets must have at least 2 [variants](2-Variant.md) and `CanBeReforged` set to true in order to use the colorizer.

Consumes 15 [Omni Geode](https://stardewvalleywiki.com/Omni_Geode) to reroll the variant on a TrinketTinker trinket, no effect on vanilla trinkets.

The colorizer will never roll the same variant twice in a row. Anvils have similar rule applied to them for TrinketTinker trinkets.

Implementation wise, the colorizer is a standard machine with qualified ID `(BC)mushymato.TrinketTinker_TrinketColorizer` and a single complex output machine rule. Mods are can add more ways to obtain this machine or prepend the colorizer with special upgrade rules for their trinkets.