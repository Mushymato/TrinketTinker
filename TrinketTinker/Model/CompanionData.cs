using System.Diagnostics.CodeAnalysis;

namespace TrinketTinker.Model
{
    public class CompanionData
    {
        public string ID { get; set; } = "";
        public string? CompanionClass { get; set; } = null;
        public List<VariantData> Variants { get; set; } = new();
        public Dictionary<string, MotionData> Motions { get; set; } = new();
    }
}