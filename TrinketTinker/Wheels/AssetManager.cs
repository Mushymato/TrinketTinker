using Mushymato.ExtendedTAS;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Companions;
using StardewValley.GameData;
using StardewValley.Objects.Trinkets;
using TrinketTinker.Companions;
using TrinketTinker.Effects;
using TrinketTinker.Models;

namespace TrinketTinker.Wheels;

/// <summary>Handles caching of custom asset.</summary>
internal static class AssetManager
{
    /// <summary>Vanilla trinket asset target</summary>
    internal const string TRINKET_TARGET = "Data/Trinkets";

    /// <summary>Tinker asset target</summary>
    internal const string TinkerAsset = $"{ModEntry.ModId}/Tinker";

    /// <summary>Backing field for tinker data</summary>
    private static Dictionary<string, TinkerData>? _tinkerData = null;

    /// <summary>Tinker data lazy loader</summary>
    internal static Dictionary<string, TinkerData> TinkerData
    {
        get
        {
            _tinkerData ??= ModEntry.Help.GameContent.Load<Dictionary<string, TinkerData>>(TinkerAsset);
            return _tinkerData;
        }
    }

    internal static TASAssetManager TAS = null!;

    internal static void OnAssetRequested(AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(TinkerAsset))
            e.LoadFrom(() => new Dictionary<string, TinkerData>(), AssetLoadPriority.Exclusive);
        if (e.Name.IsEquivalentTo(TRINKET_TARGET))
            e.Edit(Edit_Trinkets_EffectClass, AssetEditPriority.Late + 100);
    }

    internal static bool OnAssetInvalidated(AssetsInvalidatedEventArgs e)
    {
        if (e.NamesWithoutLocale.Any(an => an.IsEquivalentTo(TinkerAsset)))
        {
            ModEntry.Log($"Invalidate {TinkerAsset} on screen {Context.ScreenId}");
            _tinkerData = null;
            GameRunner.instance.ExecuteForInstances(
                (instance) =>
                {
                    foreach (Farmer onlineFarmer in Game1.getOnlineFarmers())
                    {
                        foreach (Trinket trinket in onlineFarmer.trinketItems)
                        {
                            if (trinket?.GetEffect() is TrinketTinkerEffect effect)
                            {
                                ModEntry.Log(
                                    $"Mark {trinket.QualifiedItemId} as dirty for player {onlineFarmer.UniqueMultiplayerID} ({onlineFarmer.IsLocalPlayer})"
                                );
                                effect.IsDirty.Value = true;
                            }
                        }
                        foreach (Companion companion in onlineFarmer.companions)
                        {
                            if (companion is TrinketTinkerCompanion ttCmp)
                            {
                                ModEntry.Log(
                                    $"Mark {ttCmp} as dirty for player {onlineFarmer.UniqueMultiplayerID} ({onlineFarmer.IsLocalPlayer})"
                                );

                                ttCmp.IsDirty.Value = true;
                            }
                        }
                    }
                }
            );

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
