using StardewValley;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects.Trinkets;
using TrinketTinker.Effects;

namespace TrinketTinker.Extras
{
    public static class ItemQuery
    {
        public static readonly string CreateTrinketQuery = $"{ModEntry.ModId}_CREATE_TRINKET";

        /// <summary>
        /// mushymato.TrinketTinker_CREATE_TRINKET UnqualifiedId Level? Variant?
        /// Creates a new trinket item. If the trinket has tinker data, set level and variant.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="arguments"></param>
        /// <param name="context"></param>
        /// <param name="avoidRepeat"></param>
        /// <param name="avoidItemIds"></param>
        /// <param name="logError"></param>
        /// <returns></returns>
        public static IEnumerable<ItemQueryResult> CreateTrinket(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
        {
            string[] array = ItemQueryResolver.Helpers.SplitArguments(arguments);
            if (!ArgUtility.TryGet(array, 0, out string trinketId, out string error1, allowBlank: false, "string trinketId"))
                return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, error1);
            if (!ArgUtility.TryGetOptionalInt(array, 1, out var level, out var error2, 0, "int level"))
                return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, error1);
            if (!ArgUtility.TryGetOptionalInt(array, 2, out var variant, out var error3, 0, "int variant"))
                return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, error1);

            TrinketDataDefinition trinketDataDefinition = (TrinketDataDefinition)ItemRegistry.GetTypeDefinition(ItemRegistry.type_trinket);
            if (trinketDataDefinition.GetData(trinketId) is ParsedItemData trinketData)
            {
                Trinket trinket = (Trinket)trinketDataDefinition.CreateItem(trinketData);
                if (trinket.GetEffect() is TrinketTinkerEffect effect)
                {
                    effect.SetLevel(trinket, level);
                    effect.SetVariant(trinket, variant);
                }
                return [new ItemQueryResult(trinket)];
            }
            return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, $"No trinket with id {trinketId}.");
        }
    }
}