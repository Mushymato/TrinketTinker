using Microsoft.Xna.Framework;
using StardewValley;

namespace TrinketTinker.Wheels
{
    internal static class Seek
    {
        public static SObject? ClosestMatchingObject(GameLocation location, Vector2 originPoint, int range, Func<SObject, bool>? match)
        {
            SObject? result = null;
            float minDistance = range + 1;
            foreach (Vector2 tile in location.Objects.Keys)
            {
                SObject obj = location.Objects[tile];
                if (match == null || match(obj))
                {
                    float distance = Vector2.Distance(originPoint, tile * Game1.tileSize);
                    if (distance <= range && distance < minDistance)
                    {
                        result = obj;
                    }
                }
            }
            return result;
        }

    }
}