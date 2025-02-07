# Variant

In the base game certain companions such as the frog have different color variants. With this model, you can explicitly define a sprite sheet, texture size, and optionally a color for mask. The [trinket colorizer](8.2-Trinket%20Colorizer%20and%20Anvil.md) can be used to reroll the trinket variable, but you can also add other machine rules or shop entries for purpose of obtaining trinket in a specific variant.

Variants can have alternate variants, which are automatically rechecked whenever the player changes locations, or when an [ability](4-Ability.md) has `ProcAltVariant` set.

## Shared Fields

These fields are valid for both variant and alt variant.

| Property | Type | Default | Notes |
| -------- | ---- | ------- | ----- |
| `Texture` | string | **required** | Asset target of the loaded texture, should be a sprite sheet. |
| `ColorMask` | string | _null_ | Color to apply on draw, for use with grayscale sprites.<br>Aside from RGB and hex values, monogame accepts [named colors](https://docs.monogame.net/api/Microsoft.Xna.Framework.Color.html) and this mod accepts special value `"Prismatic"` for an animated color cycle. |
| `Width` | int | 16 | Width of 1 sprite on the sprite sheet. |
| `Height` | int | 16 | Height of 1 sprite on the sprite sheet. |
| `TextureScale` | float | 4 | Texture draw scale, default is 4 like most things in the game. |
| `ShadowScale` | float | 3 | Size of the shadow to draw, 0 to disable shadow. |
| `NPC` | string | _null_ | An NPC name (key of `Data/Characters`) to associate this variant with, used for the [Chatter ability](4.z.103-Chatter.md). |
| `Name` | string | _null_ | A display name for the [Chatter ability](4.z.103-Chatter.md), used if there's no real `NPC`. |

### Top Level Variant Only

| Property | Type | Default | Notes |
| -------- | ---- | ------- | ----- |
| `LightSource` | `LightSourceData` | _null_ | If set, display a light source. This light source is only visible to the owner. |
| `TrinketSpriteIndex` | int | -1 | If set, alters the trinket item's sprite index to this. This is used to give the trinket different icon depending on the variant. |
| `TrinketNameArguments` | `List<string>?` | _null_ | If set, use these strings as the argument to the item name. |


### Alt Variant Only 

### LightSourceData

| Property | Type | Default | Notes |
| -------- | ---- | ------- | ----- |
| `Radius` | float | 2 | Size of light source. |
| `Index` | int | 1 | Vanilla light source texture index. |
| `Texture` | string | 1 | Custom light map, must be loaded into game content. |
| `Color` | string | _null_ | Light color name, accepts same values as `ColorMask`. |

## Notes

- There's no need to have the same width and height in all variants, as long as you have the same number of sprites required by the motion.
