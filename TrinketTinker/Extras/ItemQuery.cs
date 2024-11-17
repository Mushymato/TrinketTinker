using StardewValley;
using StardewValley.Delegates;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects.Trinkets;
using TrinketTinker.Effects;

namespace TrinketTinker.Extras;

public static class ItemQuery
{
    public static string ItemQuery_CREATE_TRINKET => $"{ModEntry.ModId}_CREATE_TRINKET";
    public static string ItemQuery_CREATE_TRINKET_ALL_VARIANTS =
        $"{ModEntry.ModId}_CREATE_TRINKET_ALL_VARIANTS";
    public static string GameStateQuery_INPUT_IS_TINKER => $"{ModEntry.ModId}_INPUT_IS_TINKER";
    private const string RANDOM = "R";
    private const string ALL = "A";

    /// <summary>
    /// Get a int value from argument at index.
    /// If argument at index is <see cref="RANDOM"/>, get a random value between 0 and <see cref="maxValue"/>
    /// </summary>
    /// <param name="array"></param>
    /// <param name="index"></param>
    /// <param name="maxValue"></param>
    /// <param name="context"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    private static bool TryGetOptionalOrRandom(
        string[] array,
        int index,
        int maxValue,
        ItemQueryContext context,
        out int value
    )
    {
        if (array.Length > index && array[index] == RANDOM)
        {
            value = (context.Random ?? Random.Shared).Next(maxValue);
            return true;
        }
        return ArgUtility.TryGetOptionalInt(array, index, out value, out string? _);
    }

    /// <summary>
    /// mushymato.TrinketTinker_CREATE_TRINKET UnqualifiedId [Level] [Variant]<br/>
    /// Creates a new trinket item. If the trinket has tinker data, set level and variant.
    /// If level="R", roll a random level
    /// If variant="R", roll a random variant
    /// </summary>
    /// <param name="key"></param>
    /// <param name="arguments"></param>
    /// <param name="context"></param>
    /// <param name="avoidRepeat"></param>
    /// <param name="avoidItemIds">ignored</param>
    /// <param name="logError"></param>
    /// <returns></returns>
    public static IEnumerable<ItemQueryResult> CREATE_TRINKET(
        string key,
        string arguments,
        ItemQueryContext context,
        bool avoidRepeat,
        HashSet<string> avoidItemIds,
        Action<string, string> logError
    )
    {
        string[] array = ItemQueryResolver.Helpers.SplitArguments(arguments);
        if (
            !ArgUtility.TryGet(
                array,
                0,
                out string trinketId,
                out string error1,
                allowBlank: false,
                "string trinketId"
            )
        )
            return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, error1);

        TrinketDataDefinition trinketDataDefinition = (TrinketDataDefinition)
            ItemRegistry.GetTypeDefinition(ItemRegistry.type_trinket);
        if (trinketDataDefinition.GetData(trinketId) is ParsedItemData trinketData)
        {
            Trinket trinket = (Trinket)trinketDataDefinition.CreateItem(trinketData);
            if (trinket.GetEffect() is TrinketTinkerEffect effect)
            {
                if (TryGetOptionalOrRandom(array, 1, effect.MaxLevel, context, out int level))
                    effect.SetLevel(trinket, level);
                if (TryGetOptionalOrRandom(array, 2, effect.MaxVariant, context, out int variant))
                    effect.SetVariant(trinket, variant);
            }
            return [new ItemQueryResult(trinket)];
        }
        return ItemQueryResolver.Helpers.ErrorResult(
            key,
            arguments,
            logError,
            $"No trinket with id {trinketId}."
        );
    }

    /// <summary>
    /// mushymato.TrinketTinker_CREATE_TRINKET_ALL_VARIANTS UnqualifiedId [Level]<br/>
    /// If the trinket has trinket tinker data, create one of each variant.
    /// If level="R", roll a random level and apply it to every created trinket.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="arguments"></param>
    /// <param name="context"></param>
    /// <param name="avoidRepeat"></param>
    /// <param name="avoidItemIds">ignored</param>
    /// <param name="logError"></param>
    /// <returns></returns>
    public static IEnumerable<ItemQueryResult> CREATE_TRINKET_ALL_VARIANTS(
        string key,
        string arguments,
        ItemQueryContext context,
        bool avoidRepeat,
        HashSet<string> avoidItemIds,
        Action<string, string> logError
    )
    {
        string[] array = ItemQueryResolver.Helpers.SplitArguments(arguments);
        if (
            !ArgUtility.TryGet(
                array,
                0,
                out string trinketId,
                out string error1,
                allowBlank: false,
                "string trinketId"
            )
        )
            return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, error1);

        TrinketDataDefinition trinketDataDefinition = (TrinketDataDefinition)
            ItemRegistry.GetTypeDefinition(ItemRegistry.type_trinket);
        if (trinketDataDefinition.GetData(trinketId) is not ParsedItemData trinketData)
            return ItemQueryResolver.Helpers.ErrorResult(
                key,
                arguments,
                logError,
                $"No trinket with id {trinketId}."
            );
        Trinket trinket = (Trinket)trinketDataDefinition.CreateItem(trinketData);
        if (trinket.GetEffect() is not TrinketTinkerEffect effect)
            return ItemQueryResolver.Helpers.ErrorResult(
                key,
                arguments,
                logError,
                $"Not a TrinketTinker trinket."
            );
        if (effect.MaxVariant == 0)
            return ItemQueryResolver.Helpers.ErrorResult(
                key,
                arguments,
                logError,
                $"Trinket has no variants."
            );

        TryGetOptionalOrRandom(array, 1, effect.MaxLevel, context, out int level);

        List<ItemQueryResult> createdTrinkets = [];
        for (int variant = 0; variant < effect.MaxVariant; variant++)
        {
            effect = (TrinketTinkerEffect)trinket.GetEffect();
            effect.SetLevel(trinket, level);
            effect.SetVariant(trinket, variant);
            createdTrinkets.Add(new ItemQueryResult(trinket));
            trinket = (Trinket)trinket.getOne();
        }
        return createdTrinkets;
    }

    /// <summary>
    /// Check that the input item is a trinket using TrinketTinkerEffect
    /// </summary>
    /// <param name="query"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static bool INPUT_IS_TINKER(string[] query, GameStateQueryContext context)
    {
        if (context.InputItem == null)
            return false;
        return context.InputItem is Trinket trinket && trinket.GetEffect() is TrinketTinkerEffect;
    }
}
