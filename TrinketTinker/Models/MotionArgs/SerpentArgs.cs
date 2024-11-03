namespace TrinketTinker.Models.MotionArgs
{
    /// <summary>Hop args</summary>
    public sealed class SerpentArgs : LerpArgs
    {
        /// <summary>Number of segments, not including head</summary>
        public int SegmentCount { get; set; } = 5;
        /// <summary>Number of alternate segment textures.</summary>
        public int SegmentAlts { get; set; } = 1;
        /// <summary>Whether the serpent has a tail</summary>
        public bool HasTail = true;
        /// <summary>How spaced out each segment is</summary>
        public float Sparcity { get; set; } = 3.5f;
    }
}