namespace TrinketTinker.Models;

/// <summary>Top level data class for Tinker.</summary>
public sealed class TinkerData
{
    /// <summary>Trinket stat minimum level, this added to the internal level value that is based on size of <see cref="Abilities"/></summary>
    public int MinLevel { get; set; } = 1;

    /// <summary>List of variants</summary>
    public IReadOnlyList<VariantData> Variants { get; set; } = [];

    /// <summary>Shim for case of just 1 motion</summary>
    public MotionData? Motion
    {
        get => Motions.FirstOrDefault();
        set
        {
            if (value != null)
            {
                if (Motions.Any())
                    Motions[0] = value;
                else
                    Motions.Add(value);
            }
        }
    }

    /// <summary>List of motions</summary>
    public List<MotionData> Motions { get; set; } = [];

    /// <summary>List of abilities</summary>
    public IReadOnlyList<List<AbilityData>> Abilities { get; set; } = [];

    /// <summary>GSQ conditions for locking variants.</summary>
    public IReadOnlyList<string?> VariantUnlockConditions = [];

    /// <summary>GSQ conditions for locking abilities.</summary>
    public IReadOnlyList<string?> AbilityUnlockConditions = [];
}
