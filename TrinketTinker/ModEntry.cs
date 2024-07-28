using StardewModdingAPI;
using StardewValley.Objects;
using TrinketTinker.Effects;

namespace TrinketTinker
{
    public class ModEntry : Mod
    {
        private static IMonitor? mon;

        public override void Entry(IModHelper helper)
        {
            // I18n.Init(helper.Translation);
            mon = Monitor;
            Log(typeof(TrinketEffect).AssemblyQualifiedName!.ToString());
            Log(typeof(TrinketTinkerEffect).AssemblyQualifiedName!.ToString());
        }

        public static void Log(string msg, LogLevel level = LogLevel.Debug)
        {
            mon!.Log(msg, level);
        }
    }
}
