using Microsoft.Xna.Framework;
using StardewValley;
using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Models.AbilityArgs;

public enum HarvestDestination
{
    None,
    Player,
}

/// <summary>Args for harvest abilities</summary>
public class HarvestArgs : TileArgs
{
    /// <summary>Where to deposit the harvested item</summary>
    public HarvestDestination HarvestTo = HarvestDestination.Player;
}
