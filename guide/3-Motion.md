# Motion

Motion describes how the companion moves and animates.


There are 2 main kinds of motion implemented in by this mod.
- Lerp: the companion moves to follow whenever the anchor moves far enough. If the anchor is too far, teleport over.
- Static: the companion stays at a fixed position relative to the anchor.

Each of these have sub types like Hover for Lerp motion that hovers, see the individual pages on the sidebar for details.

The term "anchor" refers to a position that the companion derives it's own position from. By default this is the player, but it can also be other entities like a monster.


## Structure

| Property | Type | Default | Notes |
| -------- | ---- | ------- | ----- |
| `MotionClass` | string | Lerp | Type name of the motion class to use, can use short name like `"Lerp"`.<br>Refer to pages under Motion Classes in the table of contents for details. |
| `DirectionMode` | [DirectionMode](3.0-Direction.md) | Single | Determines how the trinket behaves when changing directions and controls what sprites are required. |
| `DirectionRotation` | bool | false | When true, the sprite will rotate depending on direction, the exact behavior depends the motion class. |
| `LoopMode` | [LoopMode](~/api/TrinketTinker.Models.LoopMode.yml) | Standard | Control animation playback. <ul><li>Standard: 1 2 3 4 1 2 3 4</li><li>PingPong:  1 2 3 4 3 2 1</li><ul> |
| `Anchors` | List\<[AnchorTargetData](3.1-Anchors.md)\> | _empty_ | Ordered list of anchors to follow, if not set, fall back to following the player |
| `AlwaysMoving` | bool | false | By default the companion only animates while the player is moving, setting this to true makes the companion continue their animation. |
| `FrameStart` | int | 0 | First frame/sprite index of the directional animations, set this if you want to put multiple companions on 1 file. |
| `FrameLength` | int | 4 | Length of each cycle for directional animations. |
| `Interval` | float | 100 | Milisecond Interval between animation frames. |
| `Offset` | Vector2 | 0, 0 | Constant offset to apply to the companion, on top of the motion. |
| `LayerDepth` | [LayerDepth](~/api/TrinketTinker.Models.LayerDepth.yml) | Position | Changes draw layer relative to player. <ul><li>Position: Calculate layer based on Y position</li><li>Behind: Always behind the player.</li><li>InFront: Always infront of the player</li></ul> |
| `RepeatCount` | int | 0 | Number of additional repeat draws to do, to make 1 companion appear to be multiple. Putting a big number here may cause performance issues. |
| `RepeatInterval` | int | 1000 | Miliseconds between repeat draws. |
| `RepeatFrameSets` | int | 0 | If set, offset the sprite on repeat draw. |
| `AnimClips` | Dictionary\<string, [AnimClipData](3.2-Animation Clips.md)\> | _empty_ | Named anim clips that can play over the movement animation. |
| `Args` | Dictionary | _varies_ | Arguments specific to a motion class, see respective page for details. |

### Animation

The default animation behavior for a companion is to animate while the player is moving according to [direction mode](3.0-Direction.md), and display static first frame when player is still.

To use a special animation while player is still, add an `AnimClip` named `"Idle.0"`. If the idle animation should change according to direction, add `"Idle.X"` where `"X"` is a number between 1 to 4, depending on how many directions are available for the chosen `DirectionMode`. See [animation clips](3.2-Animation%20Clips.md) for details.

To have the directional animation continue regardless of player motion, set `AlwaysMoving` to true.

### Repeat Draws

One trinket can only have one companion, but it's possible to make this companion appear as multiple entities by repeating draws with a delay, creating "shadow clones". Abilities only happen relative to the first, original copy of the companion.

To give these clones alternate appearances, extend the spritesheet downward with a modified duplicate of the companion's animations containing the same number of frames as the original. Then, set `RepeatFrameSets` to the number of these duplicate frames.

#### Example

* `RepeatCount`: 2
* `RepeatFrameSets`: 3
* Spritesheet
![3 frame sets](~/images/sheets/rl-repeat-2.png)

![Repeat Demo](~/images/demo/repeat.png)