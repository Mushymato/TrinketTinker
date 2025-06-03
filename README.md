# TrinketTinker - Stardew Valley Trinket Framework

This is a framework for creating trinkets that can have advanced abilities using just [Content Patcher](https://github.com/Pathoschild/StardewMods/tree/stable/ContentPatcher).

## What can this framework do?

- Animated companions with various movement patterns.
- A skin system (variants) for companions, and related features to set/change companion variants
- Dynamic event based abilities that work through the companions rather than the player.
- Support for custom effects from other C# mods, through the use of actions.
- Have the player obtain trinket effects without actually having the trinket slot unlocked, through the use of special trigger actions.

If you are looking to make mods using this framework, [start here](https://mushymato.github.io/TrinketTinker/guide/0-Trinket.html).

This mod is licensed under MIT, contributions are welcome.

## Example Mods for TrinketTinker

- [[CP] Sinister Servants](https://github.com/Mushymato/TrinketTinker/tree/main/%5BFullMod%5D/%5BCP%5D%20Sinister%20Servants): Playable mod for TrinketTinker, adds 6 monster trinkets.
- [[CP] Pack Possum and Critter Cages](https://github.com/Mushymato/TrinketTinker/tree/main/%5BFullMod%5D/%5BCP%5D%20Pack%20Possum%20and%20Critter%20Cages): Playable mod for TrinketTinker, adds 3 "box" trinkets that hold more trinkets, and a hireable opossum.
- [[CP] Abigail Axcellent Adventure](https://github.com/Mushymato/TrinketTinker/tree/main/%5BExamples%5D/%5BCP%5D%20Abigail%20Axcellent%20Adventure): Example mod for a NPC style trinket, which are unique across the world and hides the corresponding NPC.
- [[CP] Lockmachete](https://github.com/Mushymato/TrinketTinker/tree/main/%5BExamples%5D/%5BCP%5D%20Lockmachete): Example mod showing how frame set repeat animation works.
- [[CP] Trinket Tinker Examples](https://github.com/Mushymato/TrinketTinker/tree/main/%5BExamples%5D/%5BCP%5D%20Trinket%20Tinker%20Examples): Test mod, a bit messy.

## User Configuration

These keybinds are shared across mods using this framework, for the user to set to their liking.

- `Do Interact Key`: Press this key to interact with your companion, while you are close enough.
- `Open Tinker Inventory Key`: Press this key to open the inventory of your equipped trinket(s).
- `Tinker Inventory Next Key`: While a Tinker Inventory is open and multiple trinkets with inventory are equipped, press this to go to the next inventory.
- `Tinker Inventory Prev Key`: While a Tinker Inventory is open and multiple trinkets with inventory are equipped, press this to go to the previous inventory.

These settings are global across all trinkets created using this framework, but have no effect on other trinkets (base game, added via other C# mods).

- `Hide During Events`: Trinket companions appear in events by default, the user can hide all by unchecking this.
- `Draw Debug Mode`: Enable a draw debug mode that higlights the bounds of the companion and show their current frame number.


## Translations

- English [default.json](https://github.com/Mushymato/TrinketTinker/blob/main/TrinketTinker/i18n/default.json)
- Simplified Chinese [zh.json](https://github.com/Mushymato/TrinketTinker/blob/main/TrinketTinker/i18n/zh.json)
- French [fr.json](https://github.com/Mushymato/TrinketTinker/blob/main/TrinketTinker/i18n/fr.json) (by [Caranud](https://next.nexusmods.com/profile/Caranud))

## Credits

Documentation generated with [Docfx](https://dotnet.github.io/docfx/docs/basic-concepts.html), with [docfx-material](https://ovasquez.github.io/docfx-material) theme.
