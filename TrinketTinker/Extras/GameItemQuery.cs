using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Internal;
using StardewValley.Inventories;
using StardewValley.Objects.Trinkets;
using StardewValley.TokenizableStrings;
using TrinketTinker.Companions;
using TrinketTinker.Effects;

namespace TrinketTinker.Extras;

public class HiredTrinket(string itemId, int generationSeed) : Trinket(itemId, generationSeed)
{
    public override bool actionWhenPurchased(string shopId)
    {
        Game1.exitActiveMenu();
        Game1.playSound("purchaseClick", null);
        return true;
    }

    protected override string loadDisplayName()
    {
        return TokenParser.ParseText(displayNameFormat) ?? base.loadDisplayName();
    }
}

public static class GameItemQuery
{
    public static string ItemQuery_CREATE_TRINKET => $"{ModEntry.ModId}_CREATE_TRINKET";
    public static string ItemQuery_CREATE_TRINKET_ALL_VARIANTS => $"{ModEntry.ModId}_CREATE_TRINKET_ALL_VARIANTS";
    public static string ItemQuery_HIRE_TRINKET => $"{ModEntry.ModId}_HIRE_TRINKET";
    public static string GameStateQuery_IS_TINKER => $"{ModEntry.ModId}_IS_TINKER";
    public static string GameStateQuery_HAS_LEVELS => $"{ModEntry.ModId}_HAS_LEVELS";
    public static string GameStateQuery_HAS_VARIANTS => $"{ModEntry.ModId}_HAS_VARIANTS";
    public static string GameStateQuery_ENABLED_TRINKET_COUNT => $"{ModEntry.ModId}_ENABLED_TRINKET_COUNT";
    public static string GameStateQuery_IN_ALT_VARIANT => $"{ModEntry.ModId}_IN_ALT_VARIANT";
    public static string GameStateQuery_TRINKET_HAS_ITEM => $"{ModEntry.ModId}_TRINKET_HAS_ITEM";

    private const string RANDOM = "R";
    private const string MAX = "M";
    private const string ANY = "?";
    private static readonly Regex compareInt = new("([>=<!]=?)(\\d+|M)");

    public static void Register()
    {
        // Add item queries
        ItemQueryResolver.Register(ItemQuery_CREATE_TRINKET, CREATE_TRINKET);
        ItemQueryResolver.Register(ItemQuery_CREATE_TRINKET_ALL_VARIANTS, CREATE_TRINKET_ALL_VARIANTS);
        ItemQueryResolver.Register(ItemQuery_HIRE_TRINKET, HIRE_TRINKET);

        // Add GSQs
        GameStateQuery.Register(GameStateQuery_IS_TINKER, IS_TINKER);
        GameStateQuery.Register(GameStateQuery_HAS_LEVELS, HAS_LEVELS);
        GameStateQuery.Register(GameStateQuery_HAS_VARIANTS, HAS_VARIANTS);
        GameStateQuery.Register(GameStateQuery_ENABLED_TRINKET_COUNT, ENABLED_TRINKET_COUNT);
        GameStateQuery.Register(GameStateQuery_IN_ALT_VARIANT, IN_ALT_VARIANT);
        GameStateQuery.Register(GameStateQuery_TRINKET_HAS_ITEM, TRINKET_HAS_ITEM);
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

    /// <summary>Compare integers with this funny syntax: [operator][compareVal], value is implied to be left of the operator</summary>
    /// <param name="query"></param>
    /// <param name="index"></param>
    /// <param name="value"></param>
    /// <param name="maxValue"></param>
    /// <returns></returns>
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

    /// <summary>Check for a input or target item, assert that it is a trinket tinker trinket</summary>
    /// <param name="query"></param>
    /// <param name="context"></param>
    /// <param name="idx"></param>
    /// <param name="item"></param>
    /// <param name="effect"></param>
    /// <returns></returns>
    private static bool TryGetTinkerTrinket(
        string[] query,
        GameStateQueryContext context,
        int idx,
        [NotNullWhen(true)] out Trinket? trinket,
        [NotNullWhen(true)] out TrinketTinkerEffect? effect,
        bool createFromId = true
    )
    {
        trinket = null;
        effect = null;
        if (
            !GameStateQuery.Helpers.TryGetItemArg(
                query,
                idx,
                context.TargetItem,
                context.InputItem,
                out Item item,
                out var error
            )
        )
        {
            if (!createFromId)
                return false;
            if (!ArgUtility.TryGet(query, idx, out string itemId, out string _, allowBlank: true, "string itemId"))
            {
                ModEntry.Log(
                    $"Failed parsing condition '{string.Join(" ", query)}': {error}.",
                    StardewModdingAPI.LogLevel.Warn
                );
                return false;
            }
            if (ItemRegistry.Create(itemId, allowNull: false) is not Item item2)
            {
                ModEntry.Log($"Invalid item ID '{itemId}': {error}.", StardewModdingAPI.LogLevel.Warn);
                return false;
            }
            item = item2;
        }

        if (item is not Trinket trinketItem || trinketItem.GetEffect() is not TrinketTinkerEffect effectTT)
        {
            return false;
        }
        trinket = trinketItem;
        effect = effectTT;
        return true;
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

    public static IEnumerable<ItemQueryResult> HIRE_TRINKET(
        string key,
        string arguments,
        ItemQueryContext context,
        bool avoidRepeat,
        HashSet<string> avoidItemIds,
        Action<string, string> logError
    )
    {
        string[] array = ItemQueryResolver.Helpers.SplitArguments(arguments);
        if (!ArgUtility.TryGet(array, 0, out string trinketId, out string error1, allowBlank: false, "string itemId"))
            return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, error1);

        return [new ItemQueryResult(new HiredTrinket(trinketId, context.Random.Next()))];
    }

    /// <summary>
    /// Check that the input item is a trinket using TrinketTinkerEffect, then check current level/variant against int compare
    /// </summary>
    /// <param name="query"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static bool IS_TINKER(string[] query, GameStateQueryContext context)
    {
        if (TryGetTinkerTrinket(query, context, 1, out Trinket? trinket, out TrinketTinkerEffect? effect))
        {
            // check for level
            return CompareIntegerQ(query, 2, effect.Level, maxValue: effect.MaxLevel)
                && CompareIntegerQ(query, 3, effect.Variant, maxValue: effect.MaxVariant);
        }
        return false;
    }

    /// <summary>
    /// Check that the input item is a trinket using TrinketTinkerEffect and has more than 1 unlocked level
    /// </summary>
    /// <param name="query"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static bool HAS_LEVELS(string[] query, GameStateQueryContext context)
    {
        if (TryGetTinkerTrinket(query, context, 1, out Trinket? trinket, out TrinketTinkerEffect? effect))
        {
            // check for level
            return effect.GetMaxUnlockedLevel(trinket) > 1;
        }
        return false;
    }

    /// <summary>
    /// Check that the input item is a trinket using TrinketTinkerEffect and has more than 1 unlocked variant
    /// </summary>
    /// <param name="query"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static bool HAS_VARIANTS(string[] query, GameStateQueryContext context)
    {
        if (TryGetTinkerTrinket(query, context, 1, out Trinket? trinket, out TrinketTinkerEffect? effect))
        {
            // check for variant
            return effect.GetMaxUnlockedVariant(trinket) > 1;
        }
        return false;
    }

    /// <summary>Count the number of trinkets equipped, compare to a particular number</summary>
    /// <param name="query"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private static bool ENABLED_TRINKET_COUNT(string[] query, GameStateQueryContext context)
    {
        if (!TryGetTinkerTrinket(query, context, 1, out Trinket? trinket, out TrinketTinkerEffect? effect))
            return false;
        if (!ArgUtility.TryGet(query, 2, out var playerKey, out string error, allowBlank: true, "string playerKey"))
        {
            ModEntry.Log($"Failed parsing condition '{string.Join(" ", query)}': {error}.", LogLevel.Warn);
            return false;
        }
        string tId = trinket.ItemId;
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
                        trinketItem.ItemId == tId
                        && trinketItem.GetEffect() is TrinketTinkerEffect effect
                        && effect.Enabled
                    )
                        count++;
                }
                return true;
            }
        );
        return CompareIntegerQ(query, 3, count);
    }

    private static bool IN_ALT_VARIANT(string[] query, GameStateQueryContext context)
    {
        if (
            TryGetTinkerTrinket(query, context, 1, out Trinket? _, out TrinketTinkerEffect? effect, createFromId: false)
            && ArgUtility.TryGetOptional(query, 2, out string altVariantKey, out string _, "string altVariantKey")
        )
        {
            if (effect.Companion is TrinketTinkerCompanion cmp)
            {
                if (altVariantKey == "BASE")
                    return cmp._altVariantKey.Value == null;
                else
                    return cmp._altVariantKey.Value == altVariantKey;
            }
        }
        return false;
    }

    private static bool TRINKET_HAS_ITEM(string[] query, GameStateQueryContext context)
    {
        if (
            TryGetTinkerTrinket(query, context, 1, out Trinket? _, out TrinketTinkerEffect? effect, createFromId: false)
            && ArgUtility.TryGetOptional(query, 2, out string itemId, out string _, "string itemId")
            && effect.GetInventory() is Inventory trinketInv
        )
        {
            return CompareIntegerQ(query, 3, trinketInv.CountId(itemId));
        }
        return false;
    }
}
