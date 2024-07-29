using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using TrinketTinker.Effects;
using TrinketTinker.Model;

namespace TrinketTinker
{
    public class ModEntry : Mod
    {
        private static IMonitor? mon;
        public static string ModId { get; set; } = "";
        public static string CompanionAsset => $"Mods/{ModId}/Companion";

        private static Dictionary<string, CompanionModel>? _companionData = null;
        public static Dictionary<string, CompanionModel> CompanionData
        {
            get
            {
                if (_companionData == null)
                {
                    _companionData = Game1.content.Load<Dictionary<string, CompanionModel>>(CompanionAsset);
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
            // helper.Events.Content.AssetReady += OnAssetReady;
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
                e.LoadFrom(() => new Dictionary<string, CompanionModel>(), AssetLoadPriority.Low);
            }
        }
    }
}
