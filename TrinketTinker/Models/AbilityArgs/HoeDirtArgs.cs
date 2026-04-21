namespace TrinketTinker.Models.AbilityArgs;

/// <summary>Args for picking a random tile within range</summary>
public class HoeDirtArgs : TileArgs
{
    /// <summary>Whether we should make new dirt as needed.</summary>
    public bool NewDirt { get; set; } = true;

    /// <summary>Water the new hoedirt</summary>
    public bool Watering { get; set; } = true;
}
