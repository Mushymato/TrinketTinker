using Microsoft.Xna.Framework;
using StardewValley;
using TrinketTinker.Models.Mixin;
using TrinketTinker.Wheels;

namespace TrinketTinker.Models.AbilityArgs;

/// <summary>Args for picking a random tile within range</summary>
public class TileArgs : IArgs
{
    /// <summary>Tile range radius, e.g. range 1 = 3x3, results in a square</summary>
    public int Range { get; set; } = 1;

    /// <summary>Number of tiles to affect at a time</summary>
    public int Count { get; set; } = 1;

    /// <summary>Get tiles within range</summary>
    /// <param name="location"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    internal List<Vector2> GetTilesInRange(GameLocation location, Vector2 position)
    {
        int x = (int)MathF.Round(position.X / Game1.tileSize);
        int y = (int)MathF.Round(position.Y / Game1.tileSize);
        if (Range == 0)
            return [new(x, y)];
        List<Vector2> result = [];
        for (int i = -Range; i <= Range; i++)
        for (int j = -Range; j <= Range; j++)
            if (location.hasTileAt(x + i, y + j, "Back"))
                result.Add(new(x + i, y + j));
        return result;
    }

    /// <summary>Get random tiles without replacement</summary>
    /// <param name="location"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    internal IEnumerable<Vector2> IterateRandomTiles(
        GameLocation location,
        Vector2 position,
        Func<GameLocation, Vector2, bool>? match = null
    )
    {
        List<Vector2> tiles = GetTilesInRange(location, position);
        Places.ShuffleInPlace(Random.Shared, tiles);
        if (tiles.Count > 0)
        {
            int count = Count > tiles.Count ? tiles.Count : Count;
            foreach (Vector2 tile in tiles)
            {
                if (match != null && !match(location, tile))
                    continue;
                yield return tile;
                count--;
            }
        }
    }

    /// <inheritdoc/>
    public bool Validate() => Range >= 0 && Count > 0;
}
