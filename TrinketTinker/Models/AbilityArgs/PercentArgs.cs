using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Models.AbilityArgs
{
    /// <summary>Percent arguments</summary>
    public sealed class PercentArgs : IArgs
    {
        /// <summary>Min percent, out of 1</summary>
        public double Min { get; set; } = 0;
        /// <summary>Max percent, out of 1</summary>
        public double Max { get; set; } = 0;

        /// <summary>Random percent between min and max</summary>
        public double Rand => Random.Shared.NextDouble() * (Max - Min);

        /// <inheritdoc/>
        public bool Validate()
        {
            if (Min > Max)
            {
                if (Min == 0)
                    return false;
                Max = Min;
            }
            return true;
        }
    }
}