namespace TrinketTinker.Models.AbilityArgs;

/// <summary>Args for making hoe dirt</summary>
public class HoeDirtArgs : TileArgs
{
    /// <summary>Whether we should make new dirt as needed.</summary>
    public bool NewDirt { get; set; } = true;

    /// <summary>Water the new hoedirt</summary>
    public bool Watering { get; set; } = true;

    /// <summary>A delay between each hoe dirt</summary>
    public int Interval { get; set; } = 0;
}
