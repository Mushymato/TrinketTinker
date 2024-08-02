using Microsoft.Xna.Framework;

namespace TrinketTinker.Model
{
    public enum DirectionMode
    {
        None = 0,
        LR = 1,
        UDLR = 2
    }
    public class MotionData
    {
        public string? MotionClass { get; set; } = null;
        public bool AlwaysMoving { get; set; } = false;
        public DirectionMode DirectionMode { get; set; } = DirectionMode.UDLR;
        public int AnimationFrameStart { get; set; } = 0;
        public int AnimationFrameLength { get; set; } = 4;
        public float Interval { get; set; } = 100f;
        public float DrawOffsetX { get; set; } = 0f;
        public float DrawOffsetY { get; set; } = 0f;
    }
}