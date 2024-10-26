namespace TrinketTinker.Models.MotionArgs
{
    /// <summary>Hop args</summary>
    public sealed class HopArgs : LerpArgs
    {
        /// <summary>Hop height</summary>
        public float MaxHeight { get; set; } = 128f;
    }
}