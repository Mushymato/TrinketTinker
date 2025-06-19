namespace TrinketTinker.Models.AbilityArgs;

/// <summary>Where the harvested item go</summary>
public enum HarvestDestination
{
    None,
    Debris,
    Player,
    TinkerInventory,
}

/// <summary>Args for harvest abilities</summary>
public class HarvestArgs : TileArgs
{
    /// <summary>Where to deposit the harvested item</summary>
    public HarvestDestination HarvestTo { get; set; } = HarvestDestination.Player;

    /// <summary>Context tags to exclude from harvest</summary>
    public List<string>? Filters { get; set; } = null;

    /// <summary>
    /// Show the item that was harvested as a temporary animated sprite above the companion or the player
    /// Only applies when <seealso cref="HarvestTo"/> is <see cref="HarvestDestination.Player"/> or <see cref="HarvestDestination.TinkerInventory"/>
    /// </summary>
    public bool ShowHarvestedItem { get; set; } = true;
}
