namespace TrinketTinker.Models.MotionArgs
{
    /// <summary>Minimum and maximum float</summary>
    public class BounceArgs : LerpArgs
    {
        /// <summary>Bounce height</summary>
        public float MaxHeight { get; set; }
        /// <summary>Deform when hitting the ground</summary>
        public bool Squash { get; set; }
    }
}