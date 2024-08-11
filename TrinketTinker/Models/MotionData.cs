namespace TrinketTinker.Models
{
    public enum DirectionMode
    {
        None = 0,
        R = 1,
        RL = 2,
        DRU = 3,
        DRUL = 4,
        Rotate = 5,
    }
    public class MotionData
    {
        public string? MotionClass { get; set; } = null;
        public bool AlwaysMoving { get; set; } = false;
        public DirectionMode DirectionMode { get; set; } = DirectionMode.None;
        public int AnimationFrameStart { get; set; } = 0;
        public int AnimationFrameLength { get; set; } = 4;
        public float Interval { get; set; } = 100f;
        public float OffsetX { get; set; } = 0f;
        public float OffsetY { get; set; } = 0f;
        public float TextureScale { get; set; } = 4f;
        public float ShadowScale { get; set; } = 3f;
    }
}