using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Models.AbilityArgs;

/// <summary>Interface describing related to picking tiles around the companion or player</summary>
public interface ITileArgs
{
    /// <summary>Tile range radius, e.g. range 1 = 3x3, results in a square</summary>
    public int Range { get; set; }

    /// <summary>Number of tiles to affect at a time, -1 means all</summary>
    public int Count { get; set; }

    /// <summary>
    /// An optional collision map for specifying exactly what tiles to pick.
    /// X is a tile to pick, and any other char is skipped.
    /// <code>
    ///  OXX
    ///  XOX
    ///  XXO
    /// </code>
    /// When this is set, count is ignored, but range is still used for the overall area affected.
    /// Tiles within range but not within the collision maps are never picked
    /// </summary>
    public string? CollisionMap { get; set; }

    public bool IsCollide(int tileIdx);
}

/// <summary>Args for picking a random tile within range</summary>
public class TileArgs : IArgs, ITileArgs
{
    /// <inheritdoc/>
    public int Range { get; set; } = 1;

    /// <inheritdoc/>
    public int Count { get; set; } = 1;

    /// <inheritdoc/>
    public string? CollisionMap { get; set; } = null;

    private char[]? collisionMapArray = null;

    public bool IsCollide(int tileIdx)
    {
        if (CollisionMap == null)
            return true;
        collisionMapArray ??= CollisionMap.Where(ch => ch != ' ' && ch != '\n').ToArray();
        return collisionMapArray[tileIdx] == 'X';
    }

    /// <inheritdoc/>
    public bool Validate() => Range >= 0 && Count > 0;
}
