<!-- https://wiki.stardewvalley.net/Modding:Machines#Audio_.26_visuals -->
# Temporary Animated Sprite

Temporary animated sprites are ways to show animated effects on screen temporarily. They existed prior to 1.6, but was given a proper data model to use in [Data/Machines](https://wiki.stardewvalley.net/Modding:Machines#Audio_.26_visuals).

For trinket tinker, any field that calls for temporary animated sprites takes string ids corresponding to an entry in `mushymato.TrinketTinker/TAS` rather than the full definition. This let you reuse the same TAS in multiple places.

## TemporaryAnimatedSpriteDefinition

> [!NOTE]
> TemporaryAnimatedSpriteDefinition is a class provided by the game, not defined by this mod.

| Property | Type | Default | Notes |
| -------- | ---- | ------- | ----- |
| `Id` | string | **required** | Unique string id |
| `Condition` | string | _empty_ | Game state query, show TAS if true. |
| `Texture` | string | **required** | Texture asset name. |
| `SourceRect` | Rectangle | **required** | Area of texture to draw. |
| `Interval` | float | 100 | Time between frames, in miliseconds. |
| `Frames` | int | 1 | Length of the animation. |
| `Loops` | int | _empty_ | Number of times to repeat the animation. |
| `PositionOffset` | Vector2 | Vector2.Zero | Offset added to position during draw. |
| `Flicker` | bool | _empty_ | Skips drawing every other frame. |
| `Flip` | bool | false | Horizontally flip the sprite during draw. |
| `SortOffset` | float | _empty_ | Offset on layer depth, for determining whether this sprite appear over or under other sprites. |
| `AlphaFade` | float | _empty_ | Amount of additional transparency every frame. Set this to make the sprite fade away over time. |
| `Scale` | float | 1f | Draw scale, applied on top of the default 4x scale. |
| `ScaleChange` | float | _empty_ | Amount of additional scale every frame. Set this to make sprite enlarge/shrink over time. |
| `Rotation` | float | _empty_ | Amount of rotation on the sprite. |
| `RotationChange` | float | _empty_ | Amount of additional rotation every frame. Set this to make the sprite spin. |
| `Color` | string | _empty_ | Color to apply on draw, for use with grayscale sprites.<br>Aside from RGB and hex values, monogame accepts [named colors](https://docs.monogame.net/api/Microsoft.Xna.Framework.Color.html). |
