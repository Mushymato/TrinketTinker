# Variant

In the base game certain companions such as the frog have different color variants. With this model, you can explicitly define a sprite sheet, texture size, and optionally a color for mask.

## Structure

| Property | Type | Default | Notes |
| -------- | ---- | ------- | ----- |
| `Texture` | string | **required** | Asset target of the loaded texture, should be a sprite sheet. |
| `Width` | int | 16 | Width of 1 sprite on the sprite sheet. |
| `Height` | int | 16 | Height of 1 sprite on the sprite sheet. |
| `ColorMask` | Color | _empty_ | The color to apply on draw, for use with grayscale sprites.<br>Aside from RGB and hex values, monogame provides [named colors](https://docs.monogame.net/api/Microsoft.Xna.Framework.Color.html) and this mod provides special value `Prismatic` for an animated color cycle. |

## Notes

- There's no need to have the same width and height in all variants, as long as you have the same number of sprites required by the motion.
