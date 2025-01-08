# Variant

In the base game certain companions such as the frog have different color variants. With this model, you can explicitly define a sprite sheet, texture size, and optionally a color for mask.

## Structure

| Property | Type | Default | Notes |
| -------- | ---- | ------- | ----- |
| `Texture` | string | **required** | Asset target of the loaded texture, should be a sprite sheet. |
| `Width` | int | 16 | Width of 1 sprite on the sprite sheet. |
| `Height` | int | 16 | Height of 1 sprite on the sprite sheet. |
| `ColorMask` | string | _empty_ | Color to apply on draw, for use with grayscale sprites.<br>Aside from RGB and hex values, monogame accepts [named colors](https://docs.monogame.net/api/Microsoft.Xna.Framework.Color.html) and this mod accepts special value `"Prismatic"` for an animated color cycle. |
| `TextureScale` | float | 4 | Texture draw scale, default is 4 like most things in the game. |
| `ShadowScale` | float | 3 | Size of the shadow to draw, 0 to disable shadow. |
| `LightSource` | `LightSourceData` | _empty_ | If set, display a light source. This light source is only visible to the owner. |

### LightSourceData

| Property | Type | Default | Notes |
| -------- | ---- | ------- | ----- |
| `Radius` | float | 2 | Size of light source. |
| `Index` | int | 1 | Vanilla light source texture index. |
| `Texture` | string | 1 | Custom light map, must be loaded into game content. |
| `Color` | string | _empty_ | Light color name, accepts same values as `ColorMask`. |

## Notes

- There's no need to have the same width and height in all variants, as long as you have the same number of sprites required by the motion.
