using StardewValley;
using TrinketTinker.Models;
using StardewValley.Internal;
using TrinketTinker.Models.AbilityArgs;
using Microsoft.Xna.Framework;

namespace TrinketTinker.Effects.Abilities
{
    /// <summary>
    /// Create item debris on proc.
    /// Accepts spawn item arguments, like those used in shop data, never submits any item to Input.
    /// </summary>
    public class ItemDropAbility(TrinketTinkerEffect effect, AbilityData data, int lvl) : Ability<SpawnItemArgs>(effect, data, lvl)
    {
        private bool SpawnItem(ItemQueryContext context)
        {
            IList<ItemQueryResult> itemQueryResults = ItemQueryResolver.TryResolve(
                args, context, ItemQuerySearchMode.RandomOfTypeItem
            );
            bool spawned = false;
            foreach (ItemQueryResult res in itemQueryResults)
            {
                if (res.Item is Item item)
                {
                    Game1.createItemDebris(item, e.CompanionPosition, -1);
                    spawned = true;
                }
            }
            return spawned;
        }
        /// <summary>Create item drop based on item query results.</summary>
        /// <param name="farmer"></param>
        /// <returns></returns>
        protected override bool ApplyEffect(Farmer farmer)
        {
            return SpawnItem(new ItemQueryContext(farmer.currentLocation, farmer, Random.Shared)) && base.ApplyEffect(farmer);
        }

        /// <summary>Apply effect for <see cref="ProcOn.Timer"/> abilities, via <see cref="Update"/>.</summary>
        /// <param name="farmer"></param>
        /// <param name="time"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        protected override bool ApplyEffect(Farmer farmer, GameTime time, GameLocation location)
        {
            return SpawnItem(new ItemQueryContext(location, farmer, Random.Shared)) && base.ApplyEffect(farmer, time, location);
        }
    }
}
