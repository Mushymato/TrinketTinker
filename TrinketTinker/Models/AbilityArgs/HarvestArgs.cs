namespace TrinketTinker.Models.AbilityArgs;

public enum HarvestDestination
{
    None,
    Debris,
    Player,
}

/// <summary>Args for harvest abilities</summary>
public class HarvestArgs : TileArgs
{
    /// <summary>Where to deposit the harvested item</summary>
    public HarvestDestination HarvestTo = HarvestDestination.Player;

    /// <summary>Context tags to exclude from harvest</summary>
    public IReadOnlyList<string>? Filters = null;
}
