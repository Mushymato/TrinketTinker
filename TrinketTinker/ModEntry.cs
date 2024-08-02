using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using TrinketTinker.Model;

namespace TrinketTinker
{
    public class ModEntry : Mod
    {
        private static IMonitor? mon;
        public static string ModId { get; set; } = "";
        public static string CompanionAsset => $"Mods/{ModId}/Companion";

        private static Dictionary<string, CompanionData>? _companionData = null;
        public static Dictionary<string, CompanionData> CompanionData
        {
            get
            {
                if (_companionData == null)
                {
                    _companionData = Game1.content.Load<Dictionary<string, CompanionData>>(CompanionAsset);
                    LogOnce($"Load {CompanionAsset}, got {_companionData.Count} entries");
                }
                return _companionData;
            }
        }

        public override void Entry(IModHelper helper)
        {
            // I18n.Init(helper.Translation);
            mon = Monitor;
            ModId = ModManifest.UniqueID;

            // helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.Content.AssetRequested += OnAssetRequested;
            helper.Events.Content.AssetsInvalidated += OnAssetInvalidated;

            helper.ConsoleCommands.Add(
                "reload_trinkets",
                "Invalidate trinkets and companions (but not textures).",
                ConsoleReloadTrinkets
            );
        }

        public static void Log(string msg, LogLevel level = LogLevel.Debug)
        {
            mon!.Log(msg, level);
        }

        public static void LogOnce(string msg, LogLevel level = LogLevel.Debug)
        {
            mon!.LogOnce(msg, level);
        }

        private static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.Name.IsEquivalentTo(CompanionAsset))
            {
                _companionData = null;
                e.LoadFrom(() => new Dictionary<string, CompanionData>(), AssetLoadPriority.Exclusive);
            }
        }

        private static void OnAssetInvalidated(object? sender, AssetsInvalidatedEventArgs e)
        {
            if (e.NamesWithoutLocale.Any(an => an.IsEquivalentTo(CompanionAsset)))
            {
                _companionData = null;
            }
        }

        private void ConsoleReloadTrinkets(string command, string[] args)
        {
            Helper.GameContent.InvalidateCache("Data/Trinkets");
            Helper.GameContent.InvalidateCache(CompanionAsset);
        }
    }
}
