using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Triggers;
using TrinketTinker.Companions;
using TrinketTinker.Companions.Motions;
using TrinketTinker.Effects;
using TrinketTinker.Effects.Abilities;
using TrinketTinker.Extras;
using StardewValley.Internal;
using TrinketTinker.Wheels;
using StardewValley;
using StardewValley.Objects.Trinkets;

namespace TrinketTinker
{
    internal sealed class ModEntry : Mod
    {
        private static IMonitor? mon;
        public static string ModId { get; set; } = "";


        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            mon = Monitor;
            ModId = ModManifest.UniqueID;

            // Events for game launch and custom asset
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.Content.AssetRequested += OnAssetRequested;
            helper.Events.Content.AssetsInvalidated += OnAssetInvalidated;

            helper.Events.Player.Warped += OnPlayerWarped;

#if DEBUG
            // Debug console
            helper.ConsoleCommands.Add(
                "tt_print_types",
                "Print valid Effect, Companion, Motion, and Ability types.",
                ConsolePrintTypenames
            );
#endif
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            // Add trigger & action
            TriggerActionManager.RegisterAction(ProcTrinket.TriggerActionName, ProcTrinket.Action);
            TriggerActionManager.RegisterTrigger(RaiseTriggerAbility.TriggerEventName);

            // Add item query for creating a trinket with specific level and variant
            ItemQueryResolver.Register(ItemQuery.CreateTrinketQuery, ItemQuery.CreateTrinket);

            // FIXME: spacecore doesn't support trinkets atm, perhaps send PR
            // Add extra equipment slots
            // if (Helper.ModRegistry.IsLoaded("spacechase0.SpaceCore") &&
            //     Helper.ModRegistry.GetApi<Integration.ISpaceCoreApi>("spacechase0.SpaceCore") is Integration.ISpaceCoreApi SC)
            // {
            //     foreach (int i in Enumerable.Range(1, 3))
            //     {
            //         SC.RegisterEquipmentSlot(
            //             ModManifest,
            //             $"{ModId}_ExtraTrinketSlot_{i}",
            //             (item) => item == null || item is Trinket,
            //             () => $"Extra Trinket {i}",
            //             Game1.menuTexture,
            //             Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 70)
            //         );
            //     }
            // }
        }

        private static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            // load the custom asset
            AssetManager.OnAssetRequested(e);
            // add a big craftable for recoloring stuff
            TrinketColorizer.OnAssetRequested(e);
        }

        private static void OnAssetInvalidated(object? sender, AssetsInvalidatedEventArgs e)
        {
            AssetManager.OnAssetInvalidated(e);
        }

        private static void OnPlayerWarped(object? sender, WarpedEventArgs e)
        {
            if (!e.IsLocalPlayer)
                return;
            foreach (Trinket trinketItem in e.Player.trinketItems)
            {
                if (trinketItem != null && trinketItem.GetEffect() is TrinketTinkerEffect effect)
                {
                    effect.OnPlayerWarped(e.Player, e.NewLocation);
                }
            }
        }

        private void ConsolePrintTypenames(string command, string[] args)
        {
            Log("=== TrinketTinkerEffect ===", LogLevel.Info);
            foreach (TypeInfo typeInfo in typeof(TrinketTinkerEffect).Assembly.DefinedTypes)
            {
                if (typeInfo.IsAssignableTo(typeof(TrinketTinkerEffect)) && typeInfo.AssemblyQualifiedName != null)
                    Log(typeInfo.AssemblyQualifiedName);
            }

            Log("=== TrinketTinkerCompanion ===", LogLevel.Info);
            foreach (TypeInfo typeInfo in typeof(TrinketTinkerCompanion).Assembly.DefinedTypes)
            {
                if (typeInfo.IsAssignableTo(typeof(TrinketTinkerCompanion)) && typeInfo.AssemblyQualifiedName != null)
                    Log(typeInfo.AssemblyQualifiedName);
            }

            Log("=== Motion ===", LogLevel.Info);
            Log(TinkerConst.MOTION_CLS);
            foreach (TypeInfo typeInfo in typeof(IMotion).Assembly.DefinedTypes)
            {
                if (typeInfo.IsAssignableTo(typeof(IMotion)) && typeInfo.AssemblyQualifiedName != null)
                    Log(typeInfo.AssemblyQualifiedName);
            }

            Log("=== Ability ===", LogLevel.Info);
            Log(TinkerConst.ABILITY_CLS);
            foreach (TypeInfo typeInfo in typeof(IAbility).Assembly.DefinedTypes)
            {
                if (typeInfo.IsAssignableTo(typeof(IAbility)) && typeInfo.AssemblyQualifiedName != null)
                    Log(typeInfo.AssemblyQualifiedName);
            }
        }

        /// Static helper functions

        /// <summary>Static SMAPI logger</summary>
        /// <param name="msg"></param>
        /// <param name="level"></param>
        public static void Log(string msg, LogLevel level = LogLevel.Debug)
        {
            level = (level == LogLevel.Trace) ? LogLevel.Debug : level;
            mon!.Log(msg, level);
        }

        /// <summary>Static SMAPI logger, only logs the same message once</summary>
        /// <param name="msg"></param>
        /// <param name="level"></param>
        public static void LogOnce(string msg, LogLevel level = LogLevel.Debug)
        {
            level = (level == LogLevel.Trace) ? LogLevel.Debug : level;
            mon!.LogOnce(msg, level);
        }
    }
}
