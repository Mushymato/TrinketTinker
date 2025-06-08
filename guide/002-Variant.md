# Variant

In the base game certain companions such as the frog have different color variants. With this model, you can explicitly define a sprite sheet, texture size, and optionally a color for mask. The [trinket colorizer](007.3-Trinket%20Colorizer%20and%20Anvil.md) can be used to reroll the trinket variable, but you can also add other machine rules or shop entries for purpose of obtaining trinket in a specific variant.

Variants can have alternate variants, which are automatically rechecked whenever the player changes locations, or when an [ability](004-Ability.md) has `ProcAltVariant` set.

## Sample

```json
{
  "Action": "EditData",
  "Target": "mushymato.TrinketTinker/Tinker",
  "TargetField": [
    "{{ModId}}_Sample"
  ],
  "Entries": {
    "Variants": [
      // This block is the top level Variant
      {
        "Texture": "<texture asset name>",
        "TextureExtra": "<additional texture asset name>",
        "ColorMask": "<hex color or monogame color name>",
        "Width": <int width>,
        "Height": <int height>,
        "TextureScale": <int scale>,
        "ShadowScale": <int sclae>,
        "NPC": "{{ModId}}_SampleNPC",
        "Name": "[LocalizedText Strings/NPCNames:{{ModId}}_SampleNPC]",
        "Portrait": "Portrait/{{ModId}}_SampleNPC",
        "ShowBreathing": true|false,
        "LightSource": {
          // This block is LightSourceData
          "Radius": <float radius(size)>,
          "Index": <int base game light map texture index>,
          "Texture": "<light map texture>",
          "Color": "<hex color or monogame color name>",
        },
        "TrinketSpriteIndex": <int sprite index for trinket item when in this variant>,
        "TrinketNameArguments": [
          "<trinket name substitution 1>",
          "<trinket name substitution 2>",
          //...
        ],
        "AltVariants": {
          // These blocks are AltVariant
          "<alt variant key>": {
            "Texture": "<texture asset name>",
            "TextureExtra": "<additional texture asset name>",
            "ColorMask": "<hex color or monogame color name>",
            "Width": <int width>,
            "Height": <int height>,
            "TextureScale": <float scale>,
            "ShadowScale": <float scale>,
            "NPC": "{{ModId}}_SampleNPC",
            "Name": "[LocalizedText Strings/NPCNames:{{ModId}}_SampleNPC]",
            "Portrait": "Portrait/{{ModId}}_SampleNPC",
            "ShowBreathing": true|false,
            "Condition": "<game state query>",
            "Priority": <int priority>
          },
          // more alt variants...
        }
      }
    ]
  }
}
```

## Shared Fields

These fields are valid for both variant and alt variant.

| Property | Type | Default | Notes |
| -------- | ---- | ------- | ----- |
| `Texture` | string | **required** | Asset target of the loaded texture, should be a sprite sheet and needs to contain the relevant [directional animation frames](003.0-Direction.md) for the motion. |
| `TextureExtra` | string | _null_ | Texture holding additional sprites for use in [AnimClip](003.2-Animation%20Clips.md). This allows you to keep trinket specific sprites on a separate asset, but conversely you cannot put basic directional animation sprites on this sheet, only anim clips. |
| `ColorMask` | string | _null_ | Color to apply on draw, for use with grayscale sprites.<br>Aside from RGB and hex values, monogame accepts [named colors](https://docs.monogame.net/api/Microsoft.Xna.Framework.Color.html) and this mod accepts special value `"Prismatic"` for an animated color cycle. |
| `Width` | int | 16 | Width of 1 sprite on the sprite sheet. |
| `Height` | int | 16 | Height of 1 sprite on the sprite sheet. |
| `TextureScale` | float | 4 | Texture draw scale, default is 4 like most things in the game. |
| `ShadowScale` | float | 3 | Size of the shadow to draw, 0 to disable shadow. |
| `Portrait` | string | _null_ | A portrait texture for the [Chatter ability](004.z.201-Chatter.md), required to display a portrait and a name. |
| `NPC` | string | _null_ | An NPC name (key of `Data/Characters`) to associate this variant with, used for the [Chatter ability](004.z.201-Chatter.md). |
| `Name` | string | _null_ | A display name for the [Chatter ability](004.z.201-Chatter.md), used if there's no real `NPC`. |
| `ShowBreathing` | bool | _null_ | If the NPC name is set and they have `Breather=true` along with a sprite size less than 16x32, apply the NPC "breathing" effect on this trinket companion. |

### Top Level Variant

The top level variant can have all shared fields, as well as:

| Property | Type | Default | Notes |
| -------- | ---- | ------- | ----- |
| `LightSource` | `LightSourceData` | _null_ | If set, display a light source. This light source is only visible to the owner. |
| `TrinketSpriteIndex` | int | -1 | If set, alters the trinket item's sprite index to this. This is used to give the trinket different icon depending on the variant. |
| `TrinketNameArguments` | List\<string\> | _null_ | If set, use these strings as the argument to the item name. |
| `AltVariants` | Dictionary\<string, AltVariantData\> | _null_ | A dictionary of alternate variants. |

### Alt Variant Only

The alt variant in `AltVariants` can have all shared fields, as well as:

| Property | Type | Default | Notes |
| -------- | ---- | ------- | ----- |
| `Condition` | string | `"FALSE"` | A [game state query](https://stardewvalleywiki.com/Modding:Game_state_queries) used to check if this alt variant should be selected. If you want to have an alt variant exclusively activate through [ability](004-Ability.md) with `ProcAltVariant`, use `"FALSE"`. |
| `Priority` | int | 0 | Sort priority of this variant, higher number have their conditions checked first. |

Note that not all shared fields are required in alt variant, and any not set field will simply fall back to the value found in top level.

An example: Change the companion's appearance during winter.
```json
// assuming "{{ModId}}/Companion" and "{{ModId}}/Companion_Winter" are loaded
"Variants": [
  {
    "Texture": "{{ModId}}/Companion",
    "Width": 16,
    "Height": 32,
    "AltVariants": {
      "WINTER": {
        // Since Width and Height is not set,
        // The alt variant inherits 16x36 from the base variant.
        "Texture": "{{ModId}}/Companion_Winter",
        "Condition": "SEASON Winter"
      }
    }
  }
],
```

[Abilities](004-Ability.md) can explicitly set a specific variant, which bypasses `Condition`. If you want to make a special variant that only activates by ability proc, set the `Condition` to `"FALSE"` to exclude it from standard checks.

### LightSourceData

| Property | Type | Default | Notes |
| -------- | ---- | ------- | ----- |
| `Radius` | float | 2 | Size of light source. |
| `Index` | int | 1 | Base game light source texture index. |
| `Texture` | string | 1 | Custom light map, must be loaded into game content. |
| `Color` | string | _null_ | Light color name, accepts same values as `ColorMask`. |

## Notes

- Usually, there's no need to have the same width and height in all variants, or the same scale. What does matter is having the right number of sprites for all your animation. That said Width and Height do come up when using [FrameOverrides on AnimClip, which is discussed on that page](003.2-Animation%20Clips.md).
