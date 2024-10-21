using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Models.AbilityArgs
{
    public enum ValueMode
    {
        True,
        Percent,
    }
    /// <summary>Generic range argument</summary>
    public sealed class RangeArgs : IArgs
    {
        /// <summary>Determine how to interpret the value</summary>
        public ValueMode ValueMode { get; set; } = ValueMode.Percent;
        /// <summary>Min, out of 1</summary>
        public double Min { get; set; } = 0;
        /// <summary>Max, out of 1</summary>
        public double Max { get; set; } = 0;

        /// <summary>Random value between min and max</summary>
        public double Rand(double maxValue)
        {
            double randValue = Min + (Random.Shared.NextDouble() * (Max - Min));
            return ValueMode switch
            {
                ValueMode.True => randValue,
                ValueMode.Percent => maxValue * randValue,
                _ => throw new NotImplementedException()
            };
        }

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