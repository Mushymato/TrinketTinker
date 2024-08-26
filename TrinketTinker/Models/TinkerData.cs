using Microsoft.Xna.Framework;

namespace TrinketTinker.Models
{
    /// <summary>Top level data class for <see cref="ModEntry.TinkerAsset"/> entries.</summary>
    public class TinkerData
    {
        /// <summary>Companion name.</summary>
        public string Name { get; set; } = "";
        /// <summary>Class name, need to be fully qualified. Will use <see cref="Companions.TrinketTinkerCompanion"/> if not set.</summary>
        public string? CompanionClass { get; set; } = null;
        /// <summary>Trinket stat minimum level, this added to the internal level value which is based on number of items in <see cref="Abilities"/></summary>
        public int MinLevel { get; set; } = 1;
        /// <summary>List of variants</summary>
        public List<VariantData> Variants { get; set; } = new();
        /// <summary>List of motions</summary>
        public List<MotionData> Motions { get; set; } = new();
        /// <summary>List of abilities</summary>
        public List<List<AbilityData>> Abilities { get; set; } = new();

        // TODO: state controllers?
    }
}