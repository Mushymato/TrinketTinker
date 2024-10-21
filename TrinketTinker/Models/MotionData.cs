using Microsoft.Xna.Framework;

namespace TrinketTinker.Models
{
    /// <summary>Determine how the sprites are interpreted.</summary>
    public enum DirectionMode
    {
        /// <summary>Direction never changes, animate the.</summary>
        Single,
        /// <summary>Has right animations, flips sprite if going left</summary>
        R,
        /// <summary>Has right/left animations, no down/up</summary>
        RL,
        /// <summary>Has down/right/up animations, flips the right animation to go left.</summary>
        DRU,
        /// <summary>Has down/right/up/left animations</summary>
        DRUL,
    }

    /// <summary>Determine how sprites loop.</summary>
    public enum LoopMode
    {
        /// <summary>Loops to start from last frame, e.g. 1 2 3 4 1 2 3 4 1 2 3 4</summary>
        Standard,
        /// <summary>Reverse the animation from last frame, e.g. 1 2 3 4 3 2 1 2 3 4</summary>
        PingPong,
    }

    /// <summary>Which target to anchor (follow/attach) to</summary>
    public enum AnchorTarget
    {
        /// <summary>Anchor to the trinket owner</summary>
        Owner,
        /// <summary>Anchor to the nearest monster</summary>
        Monster,
    }

    /// <summary>Determine the layer depth to use when drawing the companion</summary>
    public enum LayerDepth
    {
        /// <summary>Draw just behind the farmer.</summary>
        Behind,
        /// <summary>Draw according to current Y position</summary>
        Position,
        /// <summary>Draw just in front of the farmer</summary>
        InFront,
    }

    /// <summary>Model defining how companions pick anchor target</summary>
    public class AnchorTargetData
    {
        /// <summary>Targeting mode, see <see cref="AnchorTarget"/>.</summary>
        public AnchorTarget Mode = AnchorTarget.Owner;
        /// <summary>Search range, applicable to <see cref="AnchorTarget.Monster"/>.</summary>
        public int Range = 640;
    }

    /// <summary>Model for additional animation</summary>
    public class AnimClipData
    {
        /// <summary>Anim clip frame start</summary>
        public int FrameStart { get; set; } = 0;
        /// <summary>Anim clip frame length</summary>
        public int FrameLength { get; set; } = 1;
        /// <summary>Anim clip loop mode</summary>
        public LoopMode LoopMode { get; set; } = LoopMode.Standard;
        /// <summary>Anim clip interval, fall back to <see cref="MotionData.Interval"/> if not set.</summary>
        public float? Interval { get; set; } = null;
    }

    /// <summary>Data for <see cref="Companions.Motions"/>, defines how a companion moves.</summary>
    public sealed class MotionData : Mixin.IHaveArgs
    {
        /// <summary>Type name of the motion, can use short form like "Hover" for hover motion.</summary>
        public string? MotionClass { get; set; } = null;
        /// <summary>Direction mode, determines how sprites should be arranged.</summary>
        public DirectionMode DirectionMode { get; set; } = DirectionMode.Single;
        /// <summary>Apply sprite rotation depending on direction.</summary>
        public bool DirectionRotate { get; set; } = false;
        /// <summary>Animation looping mode.</summary>
        public LoopMode LoopMode { get; set; } = LoopMode.Standard;
        /// <summary>
        /// Prefer <see cref="AnchorTargetData"/> that comes earlier in the list.
        /// Defaults to <see cref="AnchorTarget.Owner"/>.
        /// </summary>
        public List<AnchorTargetData> Anchors { get; set; } = [];
        /// <summary>If true, continue the moving animation when not owner is not moving.</summary>
        public bool AlwaysMoving { get; set; } = false;
        /// <summary>First frame of the animation.</summary>
        public int AnimationFrameStart { get; set; } = 0;
        /// <summary>Length of 1 set of movement animation.</summary>
        public int AnimationFrameLength { get; set; } = 4;
        /// <summary>Miliseconds between frames.</summary>
        public float Interval { get; set; } = 100f;
        /// <summary>Position offset.</summary>
        public Vector2 Offset { get; set; } = Vector2.Zero;
        /// <summary>Layer depth mode.</summary>
        public LayerDepth LayerDepth { get; set; } = LayerDepth.Position;
        /// <summary>
        /// Repository of anim clips that can be shown in place of the default movement anim.
        /// Must live on the same sprite sheet specified by variant data.
        /// </summary>
        public Dictionary<string, AnimClipData> AnimClips = [];
        /// <summary>Special idle animation, for when always moving is false and player is still.</summary>
        public AnimClipData? IdleAnim => AnimClips.GetValueOrDefault("Idle");
    }
}