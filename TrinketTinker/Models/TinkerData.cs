namespace TrinketTinker.Models
{
    /// <summary>Top level data class for Tinker.</summary>
    public sealed class TinkerData
    {
        /// <summary>Trinket stat minimum level, this added to the internal level value that is based on size of <see cref="Abilities"/></summary>
        public int MinLevel { get; set; } = 1;
        /// <summary>List of variants</summary>
        public List<VariantData> Variants { get; set; } = [];
        /// <summary>List of motions</summary>
        public List<MotionData> Motions { get; set; } = [];
        /// <summary>List of abilities</summary>
        public List<List<AbilityData>> Abilities { get; set; } = [];

        // TODO: state controllers?
    }
}