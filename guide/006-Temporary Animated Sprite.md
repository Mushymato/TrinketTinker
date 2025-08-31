# Temporary Animated Sprites

Temporary animated sprites are ways to show animated effects on screen temporarily. They existed prior to 1.6, but was given a proper data model to use in [Data/Machines](https://wiki.stardewvalley.net/Modding:Machines#Audio_.26_visuals). Trinket Tinker uses an extended version of the game's TemporaryAnimatedSprites model with more options available. This is the [same temporary animated sprites model used in MMAP](https://github.com/Mushymato/MiscMapActionsProperties/blob/main/docs/temporary-animated-sprites.md), but some fields don't actually come into play when used in trinket tinker.

Any field that calls for temporary animated sprites takes string id(s) corresponding to an entry in `mushymato.TrinketTinker/TAS` rather than the full definition. This let you reuse the same TAS in multiple places.

## Structure

| Property | Type | Default | Notes |
| -------- | ---- | ------- | ----- |
| `Id` | string | **required** | Unique string id |
| `Condition` | string | _null_ | Game state query, show TAS if true (or null). |
| `ConditionInterval` | double | -1 | Number of miliseconds between each GSQ check for TAS, -1 means never recheck. Only useful for respawning TAS (`SpawnInterval` > 0). |
| `Texture` | string | **required** | Texture asset name. |
| `SourceRect` | Rectangle | **required** | Area of texture to draw. |
| `Interval` | float | 100 | Time between frames, in miliseconds. |
| `Frames` | int | 1 | Length of the animation. |
| `Loops` | int | 0 | Number of times to repeat the animation. |
| `PositionOffset` | Vector2 | Vector2.Zero | Offset added to position during draw. |
| `Flicker` | bool | false | Skips drawing every other frame. |
| `Flip` | bool | false | Horizontally flip the sprite during draw. |
| `LayerDepth` | float | 0 | Absolute layer depth for determining whether sprite is above or below other sprites. |
| `SortOffset` | float | 0f | If this is set to a non-zero value, do an offset on layer depth relative to the Y position of the sprite + SortOffset, useful when using this TAS at various different positions. |
| `DrawAboveAlwaysFront` | bool | false | If true, draw sprite above always front layer of map. |
| `Scale` | float | 1f | Draw scale, applied on top of the default 4x scale. |
| `ScaleChange` | float | 0f | Amount of additional scale every frame. Set this to make sprite enlarge/shrink over time. |
| `Rotation` | float | 0f | Amount of rotation on the sprite. |
| `RotationChange` | float | 0f | Amount of additional rotation every frame. Set this to make the sprite spin. |
| `Color` | string | _null_ | Color to apply on draw, for use with grayscale sprites.<br>Aside from RGB and hex values, monogame accepts [named colors](https://docs.monogame.net/api/Microsoft.Xna.Framework.Color.html). |
| `LightRadius` | float | Spawn a light source on this TAS with this radius. |
| `LightColor` | float | Color to apply on the light. |
| `ScaleChangeChange` | float | 0 | A change upon `ScaleChange`, i.e. acceleration but for scale. |
| `Motion` | Vector2 | 0,0 | Amount of movement in pixels to do on X and Y axis each tick. |
| `Acceleration` | Vector2 | 0,0 | Amount of increase in motion on X and Y axis each tick. |
| `AccelerationChange` | Vector2 | 0,0 | Amount of increase in acceleration on X and Y axis each tick. |
| `Alpha` | float | 1f | Multiplier on Color, i.e. 0 is transparent. |
| `AlphaFade` | float | 0f | Change in alpha each tick. |
| `AlphaFadeFade` | float | 0f | Change in alpha fade each tick. |
| `PingPong` | bool | false | Makes animation frames go 0 1 2 3 2 1 0 instead of 0 1 2 3 0 1 2 3. |
| `EndActions` | List\<string\> | _null_ | List of string [trigger actions](https://stardewvalleywiki.com/Modding:Trigger_actions) to run when the TAS finishes. For things with `SpawnInterval` defined, these actions will run multiple every time the TAS spawns and ends. |
| `SpawnInterval` | double | -1 | How many miliseconds before a new copy of this TAS should be spawned, ideally used with some randomization. |
<!-- These don't matter right now in Trinket Tinker -->
<!-- | `ApplyEndActionsOnForceRemove` | bool | false | `EndActions` only fire for natural ending of TAS, setting this to true also makes it fire on manual removals. | -->
<!-- | `SpawnDelay` | double | -1 | How many miliseconds before the first copy of this TAS should be spawned, `SpawnInterval` begins counting after `SpawnDelay` elapses. | -->
| `RandMin` | TASExtRand | _null_ | Model used for randomizing certain fields. |
| `RandMax` | TASExtRand | _null_ | Model used for randomizing certain fields. |

## Randomization

Some of the above fields can be randomized with an offset chosen from a range. These must be defined via `RandMin` and `RandMax`.

For example, this causes the `Motion` of the temporary animated sprite to have a random value between `-3,-3` and `5,5` every time it (re)spawns.
```js
// Content Patcher "Changes" entry
{
    "Action": "EditData",
    "Target": "mushymato.TrinketTinker/TAS",
    "Entries": {
        "{{ModId}}_Example": {
            // Non-random fields omitted for brevity
            ...
            // Random is most useful when the TAS spawns more than once, which SpawnInterval will do
            "SpawnInterval": 100,
            // Have this amount of base motion
            "Motion": "1,1"
            // Randomize amount of motion of this TAS
            "RandMin": {
                "Motion": "-4,-4"
            },
            "RandMax": {
                "Motion": "4,4"
            }
        }
    }
}
```

You can randomize these fields:

| Property | Default |
| -------- | ------- |
| `SortOffset` | 0 |
| `Alpha` | 0 |
| `AlphaFade` | 0 |
| `Scale` | 0 |
| `ScaleChange` | 0 |
| `ScaleChangeChange` | 0 |
| `Rotation` | 0 |
| `RotationChange` | 0 |
| `Motion` | 0,0 |
| `Acceleration` | 0,0 |
| `AccelerationChange` | 0,0 |
| `PositionOffset` | 0,0 |
| `SpawnInterval` | 0 |
| `SpawnDelay` | 0 |
