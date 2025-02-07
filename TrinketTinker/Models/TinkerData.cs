namespace TrinketTinker.Models;

/// <summary>Tinker inventory definition</summary>
public sealed class TinkerInventoryData
{
    /// <summary>Inventory size</summary>
    public int Capacity = 9;

    /// <summary>Game state query condition, if false the inventory cannot be opened</summary>
    public string? OpenCondition = null;

    /// <summary>Item must have these context tags (OR), can use "tag1 tag2" for AND</summary>
    public List<string>? RequiredTags = null;

    /// <summary>Game state query condition, if false the item cannot be put inside</summary>
    public string? RequiredItemCondition = null;
}

public sealed class ChatterLinesData
{
    /// <summary>Ordered dialogue lines, one will be picked at random. Supports translation keys.</summary>
    public List<string> Lines = [];

    /// <summary>Response dialogue lines, used for $q and other cross dialogue key things. Supports translation keys.</summary>
    public Dictionary<string, string>? Responses;

    /// <summary>Game state query condition</summary>
    public string? Condition { get; set; } = null;

    /// <summary>Priority of this chatter line, higher is earlier</summary>
    public int Priority { get; set; } = 0;
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

    /// <summary>List of variants</summary>
    public List<VariantData> Variants { get; set; } = [];

    /// <summary>List of abilities</summary>
    public List<List<AbilityData>> Abilities { get; set; } = [];

    /// <summary>GSQ conditions for locking variants.</summary>
    public List<string?> VariantUnlockConditions = [];

    /// <summary>GSQ conditions for locking abilities.</summary>
    public List<string?> AbilityUnlockConditions = [];

    /// <summary>Definition for inventory, tied to level</summary>
    public TinkerInventoryData? Inventory = null;

    /// <summary>Data for <see cref="Effects.Abilities.ChatterAbility"/></summary>
    public Dictionary<string, ChatterLinesData>? Chatter = null;
}
