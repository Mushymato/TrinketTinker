namespace TrinketTinker.Models
{
    public class CompanionData
    {
        /// <summary>Companion name.</summary>
        public string Name { get; set; } = "";
        /// <summary>Class name, need to be fully qualified. Will use <see cref="Companions.TrinketTinkerCompanion"/> if not set.</summary>
        public string? CompanionClass { get; set; } = null;
        /// <summary>List of variants</summary>
        public List<VariantData> Variants { get; set; } = new();
        /// <summary>List of motions</summary>
        public Dictionary<string, MotionData> Motions { get; set; } = new();
        /// <summary>List of abilities</summary>
        public Dictionary<string, AbilityData> Abilities { get; set; } = new();

        // TODO: state controllers?
    }
}