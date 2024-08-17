namespace TrinketTinker.Models
{
    /// <summary>Determine how the sprites are interpreted.</summary>
    public enum DirectionMode
    {
        /// <summary>Direction never changes.</summary>
        None,
        /// <summary>Has right animations, flips sprite if going left</summary>
        R,
        /// <summary>Has right/left animations, no down/up</summary>
        RL,
        /// <summary>Has down/right/up animations, flips the right animation to go left.</summary>
        DRU,
        /// <summary>Has down/right/up/left animations</summary>
        DRUL,
        /// <summary>Sprite is rotated to angular direction.</summary>
        Rotate,
    }
    /// <summary>Determine how sprites loop.</summary>
    public enum LoopMode
    {
        /// <summary>Loops to start from last frame, e.g. 1 2 3 4 1 2 3 4 1 2 3 4</summary>
        Standard,
        /// <summary>Reverse the animation from last frame, e.g. 1 2 3 4 3 2 1 2 3 4</summary>
        PingPong,
    }
    /// <summary>Defines how the companion moves and animates.</summary>
    public class MotionData
    {
        /// <summary>Class name, need to be fully qualified to use a motion not provided by this mod.</summary>
        public string? MotionClass { get; set; } = null;
        /// <summary>If true, continue animation when not moving.</summary>
        public bool AlwaysMoving { get; set; } = false;
        /// <summary>Direction mode, determines how sprites should be arranged.</summary>
        public DirectionMode DirectionMode { get; set; } = DirectionMode.None;
        /// <summary>First frame of the animation.</summary>
        public LoopMode LoopMode { get; set; } = LoopMode.Standard;
        public int AnimationFrameStart { get; set; } = 0;
        /// <summary>Length of 1 set of movement animation.</summary>
        public int AnimationFrameLength { get; set; } = 4;
        /// <summary>Miliseconds between frames.</summary>
        public float Interval { get; set; } = 100f;
        /// <summary>Position offset X.</summary>
        public float OffsetX { get; set; } = 0f;
        /// <summary>Position offset Y.</summary>
        public float OffsetY { get; set; } = 0f;
        /// <summary>Base scale to draw texture at.</summary>
        public float TextureScale { get; set; } = 4f;
        /// <summary>Base scale to draw shadow texture.</summary>
        public float ShadowScale { get; set; } = 3f;
    }
}