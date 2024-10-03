using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Models.MotionArgs
{
    /// <summary>Minimum and maximum float</summary>
    public class LerpArgs : IArgs
    {
        public float Min { get; set; } = -1;
        public float Max { get; set; } = -1;
        public bool Validate() => Min < Max && Min > -1 && Max > -1;
    }
}