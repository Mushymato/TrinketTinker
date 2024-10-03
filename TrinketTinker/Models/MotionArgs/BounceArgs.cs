namespace TrinketTinker.Models.MotionArgs
{
    /// <summary>Minimum and maximum float</summary>
    public class BounceArgs : LerpArgs
    {
        public float MaxHeight { get; set; }
        public bool Squash { get; set; }
    }
}