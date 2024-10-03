using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Models.MotionArgs
{
    /// <summary>Args for static motion</summary>
    public class StaticArgs : IArgs
    {
        /// <inheritdoc/>
        public bool Validate() => true;
    }
}