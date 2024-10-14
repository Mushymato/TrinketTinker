namespace TrinketTinker.Models.MotionArgs
{
    /// <summary>Hover magnitude (wiggleness)</summary>
    public sealed class HoverArgs : LerpArgs
    {
        /// <summary>Amount of up and down bobbing motion to add to hover</summary>
        public float Magnitude { get; set; } = 0;
    }
}