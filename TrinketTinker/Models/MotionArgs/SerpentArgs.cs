namespace TrinketTinker.Models.MotionArgs
{
    /// <summary>Hop args</summary>
    public sealed class SerpentArgs : LerpArgs
    {
        /// <summary>Number of segments, not including head</summary>
        public int SegmentCount { get; set; } = 5;
        /// <summary>How spaced out each segment is</summary>
        public float Sparcity { get; set; } = 3.5f;
    }
}