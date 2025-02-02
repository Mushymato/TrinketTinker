namespace TrinketTinker.Models.AbilityArgs;

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
    public HarvestDestination HarvestTo = HarvestDestination.Player;

    /// <summary>Context tags to exclude from harvest</summary>
    public List<string>? Filters = null;
}
