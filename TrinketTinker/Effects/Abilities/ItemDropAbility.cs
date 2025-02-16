using StardewValley;
using StardewValley.Internal;
using TrinketTinker.Effects.Support;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;

namespace TrinketTinker.Effects.Abilities;

/// <summary>
/// Create item debris on proc.
/// Accepts spawn item arguments, like those used in shop data.
/// </summary>
public sealed class ItemDropAbility(TrinketTinkerEffect effect, AbilityData data, int lvl)
    : Ability<SpawnItemArgs>(effect, data, lvl)
{
    /// <summary>
    /// Do item query, and spawn all items found as debris.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private bool SpawnItem(ItemQueryContext context)
    {
        if (args.HarvestTo == HarvestDestination.None)
            return false;
        IList<ItemQueryResult> itemQueryResults = ItemQueryResolver.TryResolve(args, context, args.SearchMode);
        bool spawned = false;
        foreach (ItemQueryResult res in itemQueryResults)
        {
            if (res.Item is Item item)
            {
                switch (args.HarvestTo)
                {
                    case HarvestDestination.Player:
                        if (context.Player.addItemToInventoryBool(item))
                        {
                            spawned = true;
                            break;
                        }
                        goto case HarvestDestination.Debris;
                    case HarvestDestination.TinkerInventory:
                        if ((item = e.AddItemToInventory(item)!) == null)
                        {
                            spawned = true;
                            break;
                        }
                        // can have case where inventory is full and item is returned, only partially added
                        goto case HarvestDestination.Debris;
                    case HarvestDestination.Debris:
                        Game1.createItemDebris(item, e.CompanionPosOff ?? context.Player.position.Value, -1);
                        spawned = true;
                        break;
                    default:
                        break;
                }
            }
        }
        return spawned;
    }

    /// <summary>
    /// Perform item query and try to spawn items.
    /// </summary>
    /// <param name="proc"></param>
    /// <returns></returns>
    protected override bool ApplyEffect(ProcEventArgs proc)
    {
        return SpawnItem(new ItemQueryContext(proc.Location, proc.Farmer, Random.Shared, Name))
            && base.ApplyEffect(proc);
    }
}
