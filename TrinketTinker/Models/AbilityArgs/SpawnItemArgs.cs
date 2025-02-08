using StardewValley.GameData;
using StardewValley.Internal;
using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Models.AbilityArgs;

/// <summary>Item arguments, accepts everything in SDV item spawn data</summary>
public sealed class SpawnItemArgs : GenericSpawnItemData, IArgs
{
    /// <summary>Where to put the item that spawned</summary>
    public HarvestDestination HarvestTo { get; set; } = HarvestDestination.Debris;

    /// <summary>
    /// How to query the item, by default <see cref="ItemQuerySearchMode.RandomOfTypeItem"/>.
    /// Be wary of using <see cref="ItemQuerySearchMode.All"/>, every matching item will be dropped at once.
    /// </summary>
    public ItemQuerySearchMode SearchMode { get; set; } = ItemQuerySearchMode.RandomOfTypeItem;

    /// <inheritdoc/>
    public bool Validate() => Id != "???";
}
