using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Models.MotionArgs
{
    /// <summary>Minimum and maximum float</summary>
    public class LerpArgs : IArgs
    {
        /// <summary>Min distance from anchor, the companion does not move until they are this far from the anchor.</summary>
        public float Min { get; set; } = 80f;
        /// <summary>Max distance from anchor, if the companion is farther away than this, teleport.</summary>
        public float Max { get; set; } = 768f;
        /// <inheritdoc/>
        public bool Validate() => Min < Max;
    }
}