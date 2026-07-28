using StardewValley;

namespace TrinketTinker.Models.Mixin;

/// <summary>Data for fuel required to activate an ability.</summary>
public class RequiredItemData
{
    /// <summary>Specific item id required</summary>
    public string? RequiredItemId { get; set; } = null;

    /// <summary>Tags required (all tags must be on item)</summary>
    public List<string>? RequiredTags { get; set; } = null;

    /// <summary>GSQ check on item</summary>
    public string? Condition { get; set; } = null;

    /// <summary>Check if a given item is valid</summary>
    internal bool CheckItem(Item? item)
    {
        if (item == null)
            return false;
        if (item.QualifiedItemId == RequiredItemId)
            return true;
        if (RequiredTags?.All(item.HasContextTag) ?? false)
            return true;
        if (GameStateQuery.CheckConditions(Condition, new(Game1.currentLocation, Game1.player, item, item, null)))
            return true;
        return false;
    }
}
