namespace TrinketTinker.Models.MotionArgs
{
    /// <summary>Minimum and maximum float</summary>
    public sealed class BounceArgs : LerpArgs
    {
        /// <summary>Bounce height</summary>
        public float MaxHeight { get; set; } = 128f;
        /// <summary>Deform when hitting the ground</summary>
        public bool Squash { get; set; } = false;
    }
}