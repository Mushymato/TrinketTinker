using StardewValley.GameData;
using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Models.AbilityArgs;

/// <summary>Args for placing random items derived from item spawn rule</summary>
public class PlaceObjectArgs : GenericSpawnItemData, ITileArgs, IArgs
{
    /// <inheritdoc/>
    public int Range { get; set; } = 1;

    /// <inheritdoc/>
    public int Count { get; set; } = 1;

    /// <inheritdoc/>
    public string? CollisionMap { get; set; } = null;

    /// <summary>A delay between each object placement</summary>
    public int Interval { get; set; } = 0;

    private char[]? collisionMapArray = null;

    public bool IsCollide(int tileIdx)
    {
        if (CollisionMap == null)
            return true;
        collisionMapArray ??= CollisionMap.Where(ch => ch != ' ' && ch != '\n').ToArray();
        if (collisionMapArray.Length <= tileIdx)
            return false;
        return collisionMapArray[tileIdx] == 'X';
    }

    public bool Validate() => Range >= 0 && Count > 0 && Id != "???";
}
