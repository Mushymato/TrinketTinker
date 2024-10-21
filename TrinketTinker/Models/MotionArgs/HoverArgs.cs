using Microsoft.Xna.Framework;

namespace TrinketTinker.Models.MotionArgs
{
    /// <summary>Hover magnitude (wiggleness)</summary>
    public sealed class HoverArgs : LerpArgs
    {
        /// <summary>Amount of up and down bobbing motion to add to hover</summary>
        public float Magnitude { get; set; } = 0;
        /// <summary>Period of bobbing motion, in ms</summary>
        public float Period { get; set; } = 400f;
        /// <summary>If set, the companion will perch on the player's head after the player stays still for this many miliseconds</summary>
        public float? PerchingTimeout { get; set; } = null;
        public Vector2 PerchingOffset { get; set; } = Vector2.Zero;
    }
}