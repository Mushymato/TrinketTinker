using StardewValley;
using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Models.AbilityArgs;

/// <summary>Generic range argument</summary>
public sealed class PosRangeArgs : IArgs
{
    /// <summary>Pixel range for finding some kind of entity in map</summary>
    public int Range { get; set; } = 96;

    /// <inheritdoc/>
    public bool Validate()
    {
        return Range > 0;
    }
}
