using System.Text.RegularExpressions;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Internal;
using StardewValley.Objects.Trinkets;
using TrinketTinker.Effects;

namespace TrinketTinker.Extras;

public static class GameItemQuery
{
    public static string ItemQuery_CREATE_TRINKET => $"{ModEntry.ModId}_CREATE_TRINKET";
    public static string ItemQuery_CREATE_TRINKET_ALL_VARIANTS => $"{ModEntry.ModId}_CREATE_TRINKET_ALL_VARIANTS";
    public static string GameStateQuery_IS_TINKER => $"{ModEntry.ModId}_IS_TINKER";
    public static string GameStateQuery_HAS_LEVELS => $"{ModEntry.ModId}_HAS_LEVELS";
    public static string GameStateQuery_HAS_VARIANTS => $"{ModEntry.ModId}_HAS_VARIANTS";
    public static string GameStateQuery_ENABLED_TRINKET_COUNT => $"{ModEntry.ModId}_ENABLED_TRINKET_COUNT";

    private const string RANDOM = "R";
    private const string MAX = "M";
    private const string ANY = "?";
    private static readonly Regex compareInt = new("([>=<!]=?)(\\d+|M)");

    public static void Register()
    {
        // Add item queries
        ItemQueryResolver.Register(ItemQuery_CREATE_TRINKET, CREATE_TRINKET);
        ItemQueryResolver.Register(ItemQuery_CREATE_TRINKET_ALL_VARIANTS, CREATE_TRINKET_ALL_VARIANTS);

        // Add GSQs
        GameStateQuery.Register(GameStateQuery_IS_TINKER, IS_TINKER);
        GameStateQuery.Register(GameStateQuery_HAS_LEVELS, HAS_LEVELS);
        GameStateQuery.Register(GameStateQuery_HAS_VARIANTS, HAS_VARIANTS);
        GameStateQuery.Register(GameStateQuery_ENABLED_TRINKET_COUNT, ENABLED_TRINKET_COUNT);
    }

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
        if (array.Length > index)
        {
            switch (array[index])
            {
                case RANDOM:
                    value = (context.Random ?? Random.Shared).Next(maxValue);
                    return true;
                default:
                    break;
            }
        }
        return ArgUtility.TryGetOptionalInt(array, index, out value, out string? _);
    }

    private static bool CompareIntegerQ(string[] query, int index, int value, int? maxValue = null)
    {
        if (query.Length <= index)
            return true;
        string qStr = query[index];
        if (qStr == ANY)
            return true;
        int compValue;
        if (compareInt.Match(qStr) is Match match && match.Success)
        {
            compValue =
                maxValue != null && match.Groups[2].Value == MAX ? (int)maxValue : int.Parse(match.Groups[2].Value);
            switch (match.Groups[1].Value)
            {
                case ">":
                    return value > compValue;
                case ">=":
                    return value >= compValue;
                case "<":
                    return value < compValue;
                case "<=":
                    return value <= compValue;
                case "!=":
                    return value != compValue;
                case "=":
                case "==":
                    return value == compValue;
            }
        }
        return int.TryParse(qStr, out compValue) && value == compValue;
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
            !ArgUtility.TryGet(array, 0, out string trinketId, out string error1, allowBlank: false, "string trinketId")
        )
            return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, error1);

        if (ItemRegistry.Create(trinketId, allowNull: false) is Trinket trinket)
        {
            if (trinket.GetEffect() is TrinketTinkerEffect effect)
            {
                if (TryGetOptionalOrRandom(array, 1, effect.MaxLevel, context, out int level))
                    effect.SetLevel(trinket, level);
                if (TryGetOptionalOrRandom(array, 2, effect.MaxVariant, context, out int variant))
                    effect.SetVariant(trinket, variant);
            }
            return [new ItemQueryResult(trinket)];
        }
        else if (trinketId == "0" || trinketId == "mushymato.MachineControlPanel_DefaultItem")
        {
            // special handling for lookup anything and machine control panel
            Trinket placeholder = (Trinket)ItemRegistry.Create("(TR)MagicHairDye");
            placeholder.displayNameOverrideTemplate.Value =
                $"[LocalizedText Strings\\1_6_Strings:Trinket] {string.Join(' ', array.Skip(1))}";
            return [new ItemQueryResult(placeholder)];
        }

        return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, $"No trinket with id {trinketId}.");
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
            !ArgUtility.TryGet(array, 0, out string trinketId, out string error1, allowBlank: false, "string trinketId")
        )
            return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, error1);

        if (ItemRegistry.Create(trinketId, allowNull: false) is not Trinket trinket)
            return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, $"No trinket with id {trinketId}.");
        if (trinket.GetEffect() is not TrinketTinkerEffect effect)
            return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, $"Not a TrinketTinker trinket.");
        if (effect.MaxVariant == 0)
            return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, $"Trinket has no variants.");

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
    public static bool IS_TINKER(string[] query, GameStateQueryContext context)
    {
        if (
            context.InputItem == null
            || context.InputItem is not Trinket trinket
            || trinket.GetEffect() is not TrinketTinkerEffect effect
        )
            return false;
        // check for level
        return CompareIntegerQ(query, 1, effect.Level, maxValue: effect.MaxLevel)
            && CompareIntegerQ(query, 2, effect.Variant, maxValue: effect.MaxVariant);
    }

    /// <summary>
    /// Check that the input item is a trinket using TrinketTinkerEffect and has more than 1 unlocked level
    /// </summary>
    /// <param name="query"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static bool HAS_LEVELS(string[] query, GameStateQueryContext context)
    {
        if (
            context.InputItem == null
            || context.InputItem is not Trinket trinket
            || trinket.GetEffect() is not TrinketTinkerEffect effect
        )
            return false;
        // check for level
        return effect.GetMaxUnlockedLevel(trinket) > 1;
    }

    /// <summary>
    /// Check that the input item is a trinket using TrinketTinkerEffect and has more than 1 unlocked variant
    /// </summary>
    /// <param name="query"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static bool HAS_VARIANTS(string[] query, GameStateQueryContext context)
    {
        if (
            context.InputItem == null
            || context.InputItem is not Trinket trinket
            || trinket.GetEffect() is not TrinketTinkerEffect effect
        )
            return false;
        // check for variant
        return effect.GetMaxUnlockedVariant(trinket) > 1;
    }

    /// <summary>Count the number of trinkets equipped, compare to a particular number</summary>
    /// <param name="query"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private static bool ENABLED_TRINKET_COUNT(string[] query, GameStateQueryContext context)
    {
        if (
            !ArgUtility.TryGet(query, 1, out var playerKey, out var error, allowBlank: true, "string playerKey")
            || !ArgUtility.TryGetOptional(
                query,
                3,
                out string tId,
                out error,
                defaultValue: context.InputItem?.QualifiedItemId ?? context.TargetItem?.QualifiedItemId ?? "",
                allowBlank: false,
                name: "string trinketId"
            )
        )
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }
        int count = 0;
        GameStateQuery.Helpers.WithPlayer(
            context.Player,
            playerKey,
            (Farmer target) =>
            {
                foreach (Trinket trinketItem in target.trinketItems)
                {
                    if (trinketItem == null)
                        continue;
                    if (
                        (trinketItem.QualifiedItemId == tId || trinketItem.ItemId == tId)
                        && trinketItem.GetEffect() is TrinketTinkerEffect effect
                        && effect.Enabled
                    )
                        count++;
                }
                return true;
            }
        );
        return CompareIntegerQ(query, 2, count);
    }
}
