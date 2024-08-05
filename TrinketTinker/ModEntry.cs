using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using TrinketTinker.Companions;
using TrinketTinker.Companions.Motions;
using TrinketTinker.Effects;
using TrinketTinker.Effects.Abilities;
using TrinketTinker.Models;

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
                "tt_reload_trinkets",
                "Invalidate trinkets and companions (but not textures).",
                ConsoleReloadTrinkets
            );
            helper.ConsoleCommands.Add(
                "tt_print_types",
                "Print valid Effect, Companion, Motion, and Ability types.",
                ConsolePrintTypenames
            );
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

        private void ConsolePrintTypenames(string command, string[] args)
        {
            Log("=== TrinketTinkerEffect ===", LogLevel.Info);
            foreach (TypeInfo typeInfo in typeof(TrinketTinkerEffect).Assembly.DefinedTypes)
            {
                if (typeInfo.IsAssignableTo(typeof(TrinketTinkerEffect)) && typeInfo.AssemblyQualifiedName != null)
                    Log(typeInfo.AssemblyQualifiedName, LogLevel.Info);
            }

            Log("=== TrinketTinkerCompanion ===", LogLevel.Info);
            foreach (TypeInfo typeInfo in typeof(TrinketTinkerCompanion).Assembly.DefinedTypes)
            {
                if (typeInfo.IsAssignableTo(typeof(TrinketTinkerCompanion)) && typeInfo.AssemblyQualifiedName != null)
                    Log(typeInfo.AssemblyQualifiedName, LogLevel.Info);
            }

            Log("=== Motion ===", LogLevel.Info);
            foreach (TypeInfo typeInfo in typeof(Motion).Assembly.DefinedTypes)
            {
                if (typeInfo.IsAssignableTo(typeof(Motion)) && typeInfo.AssemblyQualifiedName != null)
                    Log(typeInfo.AssemblyQualifiedName, LogLevel.Info);
            }

            Log("=== Ability ===", LogLevel.Info);
            foreach (TypeInfo typeInfo in typeof(Ability).Assembly.DefinedTypes)
            {
                if (typeInfo.IsAssignableTo(typeof(Ability)) && typeInfo.AssemblyQualifiedName != null)
                    Log(typeInfo.AssemblyQualifiedName, LogLevel.Info);
            }
        }

        public static void Log(string msg, LogLevel level = LogLevel.Debug)
        {
            mon!.Log(msg, level);
        }

        public static void LogOnce(string msg, LogLevel level = LogLevel.Debug)
        {
            mon!.LogOnce(msg, level);
        }

        public static bool TryGetType(string? className, [NotNullWhen(true)] out Type? typ)
        {
            typ = null;
            if (className == null)
                return false;
            typ = Type.GetType(className);
            if (typ != null)
                return true;
            return false;
        }

        public static bool TryGetType(string? className, [NotNullWhen(true)] out Type? typ, string longFormat)
        {
            typ = null;
            if (className == null)
                return false;
            string longClassName = string.Format(longFormat, className);
            typ = Type.GetType(longClassName);
            if (typ != null)
                return true;
            typ = Type.GetType(className);
            if (typ != null)
                return true;
            return false;
        }
    }
}
