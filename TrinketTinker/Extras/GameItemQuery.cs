using System.Text.RegularExpressions;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects.Trinkets;
using StardewValley.TokenizableStrings;
using TrinketTinker.Effects;

namespace TrinketTinker.Extras;

public static class GameItemQuery
{
    public static string ItemQuery_CREATE_TRINKET => $"{ModEntry.ModId}_CREATE_TRINKET";
    public static string ItemQuery_CREATE_TRINKET_ALL_VARIANTS => $"{ModEntry.ModId}_CREATE_TRINKET_ALL_VARIANTS";
    public static string GameStateQuery_IS_TINKER => $"{ModEntry.ModId}_IS_TINKER";
    public static string GameStateQuery_HAS_LEVELS => $"{ModEntry.ModId}_HAS_LEVELS";
    public static string GameStateQuery_HAS_VARIANTS => $"{ModEntry.ModId}_HAS_VARIANTS";

    private const string RANDOM = "R";
    private const string MAX = "M";
    private const string PLUS = "+";
    private const string ANY = "?";
    private static readonly Regex compareInt = new("([>=<!]=?)(\\d+|M)");

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
    private static bool TryGetOptionalOrRandomOrPlus(
        string[] array,
        int index,
        int maxValue,
        int currentValue,
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
                case PLUS:
                    value = currentValue + 1;
                    return true;
                case ANY:
                    value = currentValue;
                    return false;
                default:
                    break;
            }
        }
        return ArgUtility.TryGetOptionalInt(array, index, out value, out string? _);
    }

    private static bool CompareIntegerQ(string[] query, int index, int value, int maxValue)
    {
        if (query.Length <= index)
            return true;
        string qStr = query[index];
        if (qStr == ANY)
            return true;
        int compValue;
        if (compareInt.Match(qStr) is Match match)
        {
            compValue = match.Groups[2].Value == "M" ? maxValue : int.Parse(match.Groups[2].Value);
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
                if (TryGetOptionalOrRandomOrPlus(array, 1, effect.MaxLevel, effect.Level, context, out int level))
                    effect.SetLevel(trinket, level);
                if (TryGetOptionalOrRandomOrPlus(array, 2, effect.MaxVariant, effect.Variant, context, out int variant))
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

        TryGetOptionalOrRandomOrPlus(array, 1, effect.MaxLevel, effect.Level, context, out int level);

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
        return CompareIntegerQ(query, 1, effect.Level, effect.MaxLevel)
            && CompareIntegerQ(query, 2, effect.Variant, effect.MaxVariant);
    }

    /// <summary>
    /// Check that the input item is a trinket using TrinketTinkerEffect and has more than 1 level
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
        return effect.MaxLevel > 1;
    }

    /// <summary>
    /// Check that the input item is a trinket using TrinketTinkerEffect and has more than 1 variant
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
        return effect.MaxVariant > 1;
    }
}
