namespace TrinketTinker.Models;

public sealed class TinkerInventoryData
{
    public int Capacity = 9;
    public string? OpenCondition = null;
    public List<string>? RequiredTags = null;
    public string? RequiredItemCondition = null;
}

/// <summary>Top level data class for Tinker.</summary>
public sealed class TinkerData
{
    /// <summary>If this is false, does not actually do anything on equip</summary>
    public string? EnableCondition { get; set; } = null;

    /// <summary>Show this message when the trinket is not allowed</summary>
    public string? EnableFailMessage { get; set; } = null;

    /// <summary>Trinket stat minimum level, this added to the internal level value that is based on size of <see cref="Abilities"/></summary>
    public int MinLevel { get; set; } = 1;

    /// <summary>Motion of the companion</summary>
    public MotionData? Motion { get; set; } = null;

    /// <summary>Definition for inventory, tied to level</summary>
    public TinkerInventoryData? Inventory = null;

    /// <summary>List of variants</summary>
    public List<VariantData> Variants { get; set; } = [];

    /// <summary>List of abilities</summary>
    public List<List<AbilityData>> Abilities { get; set; } = [];

    /// <summary>GSQ conditions for locking variants.</summary>
    public List<string?> VariantUnlockConditions = [];

    /// <summary>GSQ conditions for locking abilities.</summary>
    public List<string?> AbilityUnlockConditions = [];
}
