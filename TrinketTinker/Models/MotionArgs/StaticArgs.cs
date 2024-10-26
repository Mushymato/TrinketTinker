using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Models.MotionArgs
{
    /// <summary>Static args, a placeholder class for motion that inherit StaticMotion</summary>
    public class StaticArgs : IArgs
    {
        /// <inheritdoc/>
        public bool Validate() => true;
    }
}