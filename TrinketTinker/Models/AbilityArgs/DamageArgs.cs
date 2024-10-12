using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Models.AbilityArgs
{
    /// <summary>Range arguments</summary>
    public class HitscanArgs : IArgs
    {
        /// <summary>Min damage</summary>
        public int Min { get; set; } = 0;
        /// <summary>Max damage, out of 1</summary>
        public int Max { get; set; } = 0;
        /// <summary>Pixel range for finding monsters</summary>
        public int Range { get; set; } = 64;
        /// <summary>Knockback modifier</summary>
        public float Knockback { get; set; } = 0f;
        /// <summary>Precision modifier</summary>
        public int Precision { get; set; } = 0;
        /// <summary>Critical chance</summary>
        public float CriticalChance { get; set; } = 0f;
        /// <summary>Critical damage</summary>
        public float CriticalDamage { get; set; } = 0f;

        /// <summary>Random percent between min and max</summary>
        public int Rand => Random.Shared.Next(Min, Max);
        /// <inheritdoc/>
        public bool Validate()
        {
            if (Range < 1)
                return false;
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