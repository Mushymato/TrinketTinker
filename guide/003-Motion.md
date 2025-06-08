# Motion

Motion describes how the companion moves and animates.

There are 2 main kinds of motion implemented in by this mod.
- Lerp: the companion moves to follow whenever the anchor moves far enough. If the anchor is too far, teleport over.
- Static: the companion stays at a fixed position relative to the anchor.

Each of these have sub types like Hover for Lerp motion that hovers, see the individual pages on the sidebar for details.

The term "anchor" refers to a position that the companion derives it's own position from. By default this is the player, but it can also be other entities like a monster.

## Sample

```json
{
  "Action": "EditData",
  "Target": "mushymato.TrinketTinker/Tinker",
  "TargetField": [
    "{{ModId}}_Sample"
  ],
  "Entries": {
    "Motion": {
      // movement logic
      "MotionClass": "<string motion class>",
      "DirectionMode": "<enum direction mode>",
      "DirectionRotate": true|false,
      "LoopMode": "<enum loop mode>",
      "Anchors": [
        { /* anchor target data */ },
        { /* anchor target data */ },
        //...
      ],
      // animation and display
      "AlwaysMoving": <boolean moving>,
      "FrameStart": <int start frame>,
      "FrameLength": <int frame length per direction>,
      "Interval": <double interval in miliseconds>,
      "Offset": "<x>,<y>",
      "LayerDepth": "<layer depth enum>",
      "HideDuringEvents": true|false,
      // advanced: repeat draw settings
      "RepeatCount": <int how many times to repeat>,
      "RepeatInterval": <int miliseconds between repeat draws>,
      "RepeatFrameSets": <int frameset offset>,
      "AnimClips": {
        "<anim clip key>.1": {
          /* anim clip data 1 */
        },
        "<anim clip key>.2": {
          /* anim clip data 2 */
        },
        //...
      },
      "SpeechBubbles": {
        "<speech bubble 1>": {
          /* speech bubble data 1 */
        },
        "<speech bubble 2>": {
          /* speech bubble data 2 */
        },
        //...
      },
      "Args": {
        /* MotionClass dependent arguments */
      }
    }
  }
}
```

## Structure

| Property | Type | Default | Notes |
| -------- | ---- | ------- | ----- |
| `MotionClass` | string | `"Lerp"` | Type name of the motion class to use, can use short name like `"Hover"`.<br>Refer to pages under Motion Classes in the table of contents for details. |
| `DirectionMode` | [DirectionMode](003.0-Direction.md) | Single | Determines how the trinket behaves when changing directions and controls what sprites are required. |
| `DirectionRotate` | bool | false | When true, the sprite will rotate depending on direction, the exact behavior depends the motion class. |
| `LoopMode` | [LoopMode](~/api/TrinketTinker.Models.LoopMode.yml) | Standard | Control animation playback. <ul><li>Standard: 1 2 3 4 1 2 3 4</li><li>PingPong:  1 2 3 4 3 2 1</li><ul> |
| `Anchors` | List\<[AnchorTargetData](003.1-Anchors.md)\> | _null_ | Ordered list of anchors to follow, if not set, fall back to following the player |
| `AlwaysMoving` | bool | false | By default the companion only animates while the player is moving, setting this to true makes the companion continue their animation. |
| `FrameStart` | int | 0 | First frame/sprite index of the directional animations, set this if you want to put multiple companions on 1 file. |
| `FrameLength` | int | 4 | Length of each cycle for directional animations. |
| `Interval` | float | 100 | Milisecond Interval between animation frames. |
| `Offset` | Vector2 | 0, 0 | Constant offset to apply to the companion, on top of the motion. |
| `LayerDepth` | [LayerDepth](~/api/TrinketTinker.Models.LayerDepth.yml) | Position | Changes draw layer relative to player. <ul><li>Position: Calculate layer based on Y position</li><li>Behind: Always behind the player.</li><li>InFront: Always infront of the player</li></ul> |
| `HideDuringEvents` | bool | false | Hide the companion during events. |
| `RepeatCount` | int | 0 | Number of additional repeat draws to do, to make 1 companion appear to be multiple. |
| `RepeatInterval` | int | 1000 | Miliseconds between repeat draws. |
| `RepeatFrameSets` | int | 0 | If set, offset the sprite on repeat draw. |
| `AnimClips` | Dictionary\<string, [AnimClipData](003.2-Animation%20Clips.md) \> | _null_ | Named anim clips that can play over the movement animation. |
| `SpeechBubbles` | Dictionary\<string, [SpeechBubbles](003.3-Speech%20Bubbles.md) \> | _null_ | Named speech bubbles that appear over the companion. |
| `Args` | Dictionary | _varies_ | Arguments specific to a motion class, see respective page for details. |

### Animation

The default animation behavior for a companion is to animate while the player is moving according to [direction mode](003.0-Direction.md), and display static first frame when player is still.

To use a special animation while player is still, add an `AnimClip` named `"Idle"`. If the idle animation should change according to direction, add `"Idle.X"` where `"X"` is a number between 1 to 4, depending on how many directions are available for the chosen `DirectionMode`. See [animation clips](003.2-Animation%20Clips.md) for details.

To have the directional animation continue regardless of player motion, set `AlwaysMoving` to true.

### Repeat Draws

One trinket can only have one companion, but it's possible to make this companion appear as multiple entities by repeating draws with a delay, creating "shadow clones". Abilities only happen relative to the first, original copy of the companion.

To give these clones alternate appearances, extend the spritesheet downward with a modified duplicate of the companion's animations containing the same number of frames as the original. Then, set `RepeatFrameSets` to the number of these duplicate frames.

#### Example

* `RepeatCount`: 2
* `RepeatFrameSets`: 3
* Spritesheet:

![3 frame sets](~/images/sheets/rl-repeat-2.png)

![Repeat Demo](~/images/demo/repeat.png)
