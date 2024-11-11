using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;
using TrinketTinker.Effects;
using TrinketTinker.Models;

namespace TrinketTinker.Wheels;

/// <summary>Handles caching of custom asset.</summary>
internal static class AssetManager
{
    /// <summary>Vanilla trinket asset target</summary>
    internal const string TRINKET_TARGET = "Data/Trinkets";

    /// <summary>Tinker asset target</summary>
    internal static string TinkerAsset => $"{ModEntry.ModId}/Tinker";

    /// <summary>TAS (TemporaryAnimatedSprite) asset target</summary>
    internal static string TASAsset => $"{ModEntry.ModId}/TAS";

    /// <summary>Backing field for tinker data</summary>
    private static Dictionary<string, TinkerData>? _tinkerData = null;

    /// <summary>Tinker data lazy loader</summary>
    internal static Dictionary<string, TinkerData> TinkerData
    {
        get
        {
            _tinkerData ??= Game1.content.Load<Dictionary<string, TinkerData>>(TinkerAsset);
            return _tinkerData;
        }
    }

    /// <summary>Backing field for tinker data</summary>
    private static Dictionary<string, TemporaryAnimatedSpriteDefinition>? _tasData = null;
    internal static Dictionary<string, TemporaryAnimatedSpriteDefinition> TASData
    {
        get
        {
            _tasData ??= Game1.content.Load<Dictionary<string, TemporaryAnimatedSpriteDefinition>>(
                TASAsset
            );
            return _tasData;
        }
    }

    internal static void OnAssetRequested(AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(TinkerAsset))
            e.LoadFrom(() => new Dictionary<string, TinkerData>(), AssetLoadPriority.Exclusive);
        if (e.Name.IsEquivalentTo(TASAsset))
            e.LoadFrom(
                () => new Dictionary<string, TemporaryAnimatedSpriteDefinition>(),
                AssetLoadPriority.Exclusive
            );
        if (e.Name.IsEquivalentTo(TRINKET_TARGET))
            e.Edit(Edit_Trinkets_EffectClass, AssetEditPriority.Late + 100);
    }

    internal static bool OnAssetInvalidated(AssetsInvalidatedEventArgs e)
    {
        if (e.NamesWithoutLocale.Any(an => an.IsEquivalentTo(TASAsset)))
            _tasData = null;
        if (e.NamesWithoutLocale.Any(an => an.IsEquivalentTo(TinkerAsset)))
        {
            _tinkerData = null;
            return true;
        }
        return false;
    }

    /// <summary>Ensure all trinkets that have a Tinker entry also have <see cref="EffectClass"/> </summary>
    /// <param name="asset"></param>
    public static void Edit_Trinkets_EffectClass(IAssetData asset)
    {
        // this fails sometimes(?)
        string? effectClass = typeof(TrinketTinkerEffect).AssemblyQualifiedName;
        if (effectClass == null)
        {
            ModEntry.LogOnce(
                $"Could not get qualified name for TrinketTinkerEffect({typeof(TrinketTinkerEffect)}), will use hardcoded value."
            );
            effectClass = "TrinketTinker.Effects.TrinketTinkerEffect, TrinketTinker";
        }

        IDictionary<string, TrinketData> trinkets = asset.AsDictionary<string, TrinketData>().Data;
        foreach ((string key, TrinketData data) in trinkets)
        {
            if (TinkerData.ContainsKey(key))
                data.TrinketEffectClass = effectClass;
        }
    }
}
