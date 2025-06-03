# Trinkets

> [!NOTE]
> The following page covers how trinkets are added in the base game, regardless of whether TrinketTinker is being used.
> You must add trinket items before you can extend it with Trinket Tinker data.

Trinkets can be added with by editing `Data/Trinkets`, generally with [Content Patcher](https://github.com/Pathoschild/StardewMods/tree/stable/ContentPatcher). See wiki for [full description of` Data/Trinket`](https://stardewvalleywiki.com/Modding:Trinkets).

## Sample

```json
{
  "Changes": [
    // Load texture
    {
      "Action": "Load",
      "Target": "Mods/{{ModId}}/MyTrinket",
      "FromFile": "sprites/{{TargetWithoutPath}}.png"
    },
    // Edit Data/Trinkets 
    {
      "Action": "EditData",
      "Target": "Data/Trinkets",
      "Entries": {
        "{{ModId}}_MyTrinket": {
          // Trinket ID, gives qualified ID of (TR){{ModId}}_MyTrinket
          "Id": "{{ModId}}_MyTrinket",
          // Display name (with i18n)
          "DisplayName": "{{i18n:MyTrinket.DisplayName}}",
          // Description, can include {0} token for the trinket level and {1} for ability descriptions
          "Description": "{{i18n:MyTrinket.Description}}",
          // Path to asset texture load target
          "Texture": "Mods/{{ModId}}/MyTrinket",
          // Sheet index (with 16x16 sprite size)
          "SheetIndex": 0,
          // Type that controls behavior of trinket, changing this alters what the trinket does, but several effects are hardcoded.
          "TrinketEffectClass": "StardewValley.Objects.Trinkets.TrinketEffect",
          // Add trinket to random drop pool once player attains combat mastery
          // Can still add other ways to acquire (e.g. shops, machine outputs)
          "DropsNaturally": true,
          // Allow trinket to reroll stats on the anvil (and reroll appearance on the colorizer, for trinkets with tinker data).
          "CanBeReforged": true,
          // Mod specific data, shared across trinkets
          // "CustomFields": null,
          // Mod specific data, per instance of trinket
          // "ModData": null
        },
      }
    }
  ]
}
```

> [!TIP]
> Refer to content patcher docs for more details about [EditData](https://github.com/Pathoschild/StardewMods/blob/stable/ContentPatcher/docs/author-guide/action-load.md) and [Load](https://github.com/Pathoschild/StardewMods/blob/stable/ContentPatcher/docs/author-guide/action-load.md).
> Samples in this guide are usually examples of a single content patcher [EditData](https://github.com/Pathoschild/StardewMods/blob/stable/ContentPatcher/docs/author-guide/action-load.md) patch, or the contents of a content patcher [Include](https://github.com/Pathoschild/StardewMods/blob/stable/ContentPatcher/docs/author-guide/action-include.md) patch.

## TrinketEffectClass

The base game provide these trinket effect classes:

| Type Name | Notes |
| --------- | ----- |
| StardewValley.Objects.Trinkets.TrinketEffect | Base class, drops coins if the id is ParrotEgg |
| StardewValley.Objects.Trinkets.RainbowHairTrinketEffect | Makes your hair prismatic |
| StardewValley.Objects.Trinkets.CompanionTrinketEffect | Spawns the hungry frog companion |
| StardewValley.Objects.Trinkets.MagicQuiverTrinketEffect | Shoot an arrow every few seconds |
| StardewValley.Objects.Trinkets.FairyBoxTrinketEffect | Heal the player every few seconds while in combat |
| StardewValley.Objects.Trinkets.IceOrbTrinketEffect | Shoot an icy orb that freezes the enemy every few seconds |

The Golden Spur and Basilisk Paw effects are not implemented through an effect class, instead certain parts of game simply checks if the player has a specific trinket equipped.

Trinket Tinker uses a specific TrinketEffectClass, it is automatically set if there is a matching [`mushymato.TrinketTinker/Tinker`](1-Tinker.md) entry so there is no need to explictly provide this when editing `Data/Trinkets`.
