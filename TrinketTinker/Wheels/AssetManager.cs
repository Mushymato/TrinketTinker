using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;
using TrinketTinker.Effects;
using TrinketTinker.Models;

namespace TrinketTinker.Wheels
{
    /// <summary>
    /// Handles caching of custom asset.
    /// </summary>
    internal static class AssetManager
    {
        internal static string TinkerAsset => $"{ModEntry.ModId}/Tinker";
        internal static string EffectClass => typeof(TrinketTinkerEffect).AssemblyQualifiedName!;
        private static Dictionary<string, TinkerData>? _companionData = null;
        internal static Dictionary<string, TinkerData> TinkerData
        {
            get
            {
                if (_companionData == null)
                {
                    _companionData = Game1.content.Load<Dictionary<string, TinkerData>>(TinkerAsset);
#if DEBUG
                    ModEntry.Log($"Loaded {TinkerAsset}, got {_companionData.Count} entries");
#else
                    ModEntry.LogOnce($"Loaded {TinkerAsset}, got {_companionData.Count} entries");
#endif
                }
                return _companionData;
            }
        }

        internal static void OnAssetRequested(AssetRequestedEventArgs e)
        {
            if (e.Name.IsEquivalentTo(TinkerAsset))
            {
                e.LoadFrom(() => new Dictionary<string, TinkerData>(), AssetLoadPriority.Exclusive);
            }
            if (e.Name.IsEquivalentTo("Data/Trinkets"))
            {
                e.Edit(Edit_Trinkets_EffectClass, AssetEditPriority.Late + 100);
            }
        }

        internal static void OnAssetInvalidated(AssetsInvalidatedEventArgs e)
        {
            if (e.NamesWithoutLocale.Any(an => an.IsEquivalentTo(TinkerAsset)))
            {
                _companionData = null;
            }
        }

        /// <summary>Ensure all trinkets that have a Tinker entry also have</summary>
        /// <param name="asset"></param>
        public static void Edit_Trinkets_EffectClass(IAssetData asset)
        {
            IDictionary<string, TrinketData> trinkets = asset.AsDictionary<string, TrinketData>().Data;
            foreach ((string key, TrinketData data) in trinkets)
            {
                if (TinkerData.ContainsKey(key))
                    data.TrinketEffectClass = EffectClass;
            }
        }
    }
}