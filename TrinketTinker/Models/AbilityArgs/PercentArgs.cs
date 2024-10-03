using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Models.AbilityArgs
{
    /// <summary>Percent argument out of 1000, clamped to 1 to 1000</summary>
    public class PercentArgs : IArgs
    {
        private int min = 0;
        /// <summary>Min percent, out of 1000</summary>
        public int Min
        {
            get => min;
            set
            {
                min = value;
            }
        }
        private int max = 0;
        /// <summary>Max percent, out of 1000</summary>
        public int Max
        {
            get => max;
            set
            {
                max = value;
            }
        }

        /// <summary>Float min percent</summary>
        internal double MinPercent => min / 1000.0;
        /// <summary>Float max percent</summary>
        internal double MaxPercent => max / 1000.0;

        /// <summary>Random percent between min and max</summary>
        internal double Percent => Random.Shared.Next(min, max) / 1000.0;

        /// <inheritdoc/>
        public bool Validate()
        {
            if (min > max)
            {
                if (min == 0)
                    return false;
                max = min;
            }
            return true;
        }
    }
}