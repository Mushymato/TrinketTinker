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
        /// <summary>Trinket stat minimum level</summary>
        public int MinLevel { get; set; } = 0;
        /// <summary>Trinket stat maximum level</summary>
        public int MaxLevel { get; set; } = 0;
        /// <summary>List of variants</summary>
        public List<VariantData> Variants { get; set; } = new();
        /// <summary>List of motions</summary>
        public Dictionary<string, MotionData> Motions { get; set; } = new();
        /// <summary>List of abilities</summary>
        public List<List<AbilityData>> Abilities { get; set; } = new();

        // TODO: state controllers?
    }
}