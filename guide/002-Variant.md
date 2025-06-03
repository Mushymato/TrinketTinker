# Variant

In the base game certain companions such as the frog have different color variants. With this model, you can explicitly define a sprite sheet, texture size, and optionally a color for mask. The [trinket colorizer](007.3-Trinket%20Colorizer%20and%20Anvil.md) can be used to reroll the trinket variable, but you can also add other machine rules or shop entries for purpose of obtaining trinket in a specific variant.

Variants can have alternate variants, which are automatically rechecked whenever the player changes locations, or when an [ability](004-Ability.md) has `ProcAltVariant` set.

## Shared Fields

These fields are valid for both variant and alt variant.

| Property | Type | Default | Notes |
| -------- | ---- | ------- | ----- |
| `Texture` | string | **required** | Asset target of the loaded texture, should be a sprite sheet. |
| `TextureExtra` | string | _null_ | Additional sprites of the same dimension as the original, for use in [AnimClip](003.2-Animation%20Clips.md). |
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

### TrinketSpriteIndex and TrinketNameArguments

These two fields are used to allow variants to have different name and icons.

_TrinketSpriteIndex_: Changes the sprite index along with variant, meaning that if your trinket's texture is a sprite sheet with multiple item icon sprites, this can be used to point to a specific one. To make this work, put every icon associated with the variants of 1 trinket on 1 texture. You cannot change to a completely different texture, or use color masks.

_TrinketNameArguments_: Adds substitute strings for use with `{0}` in `DisplayName`, for example:
```json
// Data/Trinkets
"DisplayName": "My Trinket {0} {1}",
// mushymato.TrinketTinker/Tinker
"Variants": [
  {
    "Texture": "{{ModId}}/trinkets/red/water",
    "TrinketNameArguments": ["Red", "Water"],
  },
  {
    "Texture": "{{ModId}}/trinkets/blue/fire",
    "TrinketNameArguments": ["Blue", "Fire"],
  }
]
```

The resulting trinket can have these names:

- `"My Trinket Red Water"`, for the first Variant
- `"My Trinket Blue Fire"`, for the second Variant


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

- There's no need to have the same width and height in all variants, or the same scale. What does matter is having the right number of sprites for all your animation.
