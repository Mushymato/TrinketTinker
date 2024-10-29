namespace TrinketTinker.Models
{
    /// <summary>Top level data class for Tinker.</summary>
    public sealed class TinkerData
    {
        /// <summary>Trinket stat minimum level, this added to the internal level value that is based on size of <see cref="Abilities"/></summary>
        public int MinLevel { get; set; } = 1;
        /// <summary>GSQ conditions for locking variants.</summary>
        public List<string?> VariantUnlockConditions = [];
        /// <summary>List of variants</summary>
        public List<VariantData> Variants { get; set; } = [];
        /// <summary>List of motions</summary>
        public List<MotionData> Motions { get; set; } = [];
        /// <summary>Shim for case of just 1 motion</summary>
        public MotionData? Motion
        {
            get => Motions.FirstOrDefault();
            set
            {
                if (value != null)
                    Motions.Insert(0, value);
            }
        }
        /// <summary>GSQ conditions for locking abilities.</summary>
        public List<string?> AbilityUnlockConditions = [];
        /// <summary>List of abilities</summary>
        public List<List<AbilityData>> Abilities { get; set; } = [];
    }
}