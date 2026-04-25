using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Internal;
using TrinketTinker.Effects.Support;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;
using TrinketTinker.Wheels;

namespace TrinketTinker.Effects.Abilities;

/// <summary>
/// Place objects around the companion.
/// This can be used to plant grass and/or seeds.
/// </summary>
public sealed class PlaceObjectAbility(TrinketTinkerEffect effect, AbilityData data, int lvl)
    : Ability<PlaceObjectArgs>(effect, data, lvl)
{
    /// <summary>Hoe random amounts of dirt within range</summary>
    /// <param name="proc"></param>
    /// <returns></returns>
    protected override bool ApplyEffect(ProcEventArgs proc)
    {
        ItemQueryContext context = new(proc.Location, proc.Farmer, Random.Shared, Name);
        IList<Vector2> tileList = args.GetTiles(proc.LocationOrCurrent, e.CompanionPosition ?? proc.Farmer.Position);
        args.MaxItems = tileList.Count;
        IList<ItemQueryResult> itemQueryResults = ItemQueryResolver.TryResolve(
            args,
            context,
            ItemQuerySearchMode.AllOfTypeItem
        );
        int placedCount = 0;
        int iqIdx = 0;
        for (int i = 0; i < tileList.Count; ++i)
        {
            if (iqIdx >= itemQueryResults.Count)
            {
                itemQueryResults = ItemQueryResolver.TryResolve(args, context, ItemQuerySearchMode.AllOfTypeItem);
                iqIdx = 0;
            }
            ItemQueryResult queryResult = itemQueryResults[iqIdx++];
            if (queryResult.Item is not SObject obj)
                continue;
            Vector2 tile = tileList[i];

            int timeout = args.Interval * i;
            if (timeout <= 0)
            {
                if (PlaceObject(proc, obj, tile))
                {
                    placedCount++;
                }
            }
            else
            {
                DelayedAction.functionAfterDelay(() => PlaceObject(proc, obj, tile), timeout);
                placedCount++;
            }
        }
        return placedCount > 0 && base.ApplyEffect(proc);
    }

    private static bool PlaceObject(ProcEventArgs proc, SObject obj, Vector2 tile)
    {
        int hudMessagesLen = Game1.hudMessages.Count;
        bool placed = obj.placementAction(
            proc.LocationOrCurrent,
            (int)(tile.X * Game1.tileSize),
            (int)(tile.Y * Game1.tileSize),
            proc.Farmer
        );
        Game1.hudMessages.RemoveRange(hudMessagesLen, Game1.hudMessages.Count - hudMessagesLen);
        return placed;
    }
}
