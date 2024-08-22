using StardewValley;
using TrinketTinker.Models;
using StardewValley.Internal;

namespace TrinketTinker.Effects.Abilities
{
    /// <summary>Create item debris on proc.</summary>
    public class ItemDropAbility : Ability
    {
        /// <summary>Valid item ID, must not result in error item</summary>
        protected readonly string? itemId;
        protected readonly string? itemQuery;
        protected readonly string? itemQueryCondition;
        protected readonly int stackSize = 1;
        protected readonly int quality = 0;
        public ItemDropAbility(TrinketTinkerEffect effect, AbilityData data, int lvl) : base(effect, data, lvl)
        {
            if (d.TryGetParsed("ItemId", out itemId) &&
                ItemRegistry.GetData(itemId) != null)
            {
                itemQuery = null;
                Valid = true;
            }
            else if (d.TryGetParsed("ItemQuery", out itemQuery))
            {
                d.TryGetParsed("ItemQueryCondition", out itemQueryCondition);
                itemId = null;
                Valid = true;
            }
            else
                return;

            stackSize = d.GetParsedOrDefault("StackSize", stackSize);
            quality = d.GetParsedOrDefault("Quality", quality);
        }

        /// <summary>Create item drop.</summary>
        /// <param name="farmer"></param>
        /// <returns></returns>
        protected override bool ApplyEffect(Farmer farmer)
        {
            Item dropItem;
            if (itemId != null)
            {
                dropItem = ItemRegistry.Create(itemId, amount: stackSize, quality: quality);
            }
            else if (itemQuery != null)
            {
                if (ItemQueryResolver.TryResolve(
                    itemQuery,
                    new ItemQueryContext(farmer.currentLocation, farmer, Game1.random),
                    ItemQuerySearchMode.RandomOfTypeItem,
                    perItemCondition: itemQueryCondition
                ).FirstOrDefault()?.Item is not Item queryItem)
                    return false;
                dropItem = queryItem;
            }
            else
            {
                ModEntry.Log($"{Name}: Failed to create item drop (itemId={itemId}, itemQuery={itemQuery})");
                return false;
            }

            Game1.createItemDebris(
                dropItem,
                e.CompanionPosition,
                -1
            );
            return base.ApplyEffect(farmer);
        }
    }
}
