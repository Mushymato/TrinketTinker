﻿using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Triggers;
using StardewValley;
using TrinketTinker.Companions;
using TrinketTinker.Companions.Motions;
using TrinketTinker.Effects;
using TrinketTinker.Effects.Abilities;
using TrinketTinker.Models;
using TrinketTinker.Extra;

namespace TrinketTinker
{
    public class ModEntry : Mod
    {
        private static IMonitor? mon;
        public static string ModId { get; set; } = "";
        public static string TinkerAsset => $"Mods/{ModId}/Tinker";

        private static Dictionary<string, TinkerData>? _companionData = null;
        public static Dictionary<string, TinkerData> CompanionData
        {
            get
            {
                if (_companionData == null)
                {
                    _companionData = Game1.content.Load<Dictionary<string, TinkerData>>(TinkerAsset);
                    LogOnce($"Load {TinkerAsset}, got {_companionData.Count} entries");
                }
                return _companionData;
            }
        }

        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            mon = Monitor;
            ModId = ModManifest.UniqueID;

            // Add trigger & action
            TriggerActionManager.RegisterAction(ProcTrinket.TriggerActionName, ProcTrinket.Action);
            TriggerActionManager.RegisterTrigger(TriggerAbility.TriggerEventName);

            // Events for game launch and custom asset
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.Content.AssetRequested += OnAssetRequested;
            helper.Events.Content.AssetsInvalidated += OnAssetInvalidated;

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
            // Add content patcher tokens for various constants
            Integration.IContentPatcherAPI? CP = Helper.ModRegistry.GetApi<Integration.IContentPatcherAPI>("Pathoschild.ContentPatcher") ??
                throw new ContentLoadException("Failed to get Content Patcher API");
            CP.RegisterToken(
                ModManifest, "EffectClass",
                () => { return [typeof(TrinketTinkerEffect).AssemblyQualifiedName!]; }
            );
            CP.RegisterToken(
                ModManifest, "Target",
                () => { return [TinkerAsset]; }
            );
            CP.RegisterToken(
                ModManifest, "TrinketProc",
                () => { return [TriggerAbility.TriggerEventName]; }
            );
            CP.RegisterToken(
                ModManifest, "ProcTrinket",
                () => { return [ProcTrinket.TriggerActionName]; }
            );

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
            if (e.Name.IsEquivalentTo(TinkerAsset))
            {
                e.LoadFrom(() => new Dictionary<string, TinkerData>(), AssetLoadPriority.Exclusive);
            }
            // add a big craftable for recoloring stuff
            TrinketColorizer.OnAssetRequested(e);
        }

        private static void OnAssetInvalidated(object? sender, AssetsInvalidatedEventArgs e)
        {
            if (e.NamesWithoutLocale.Any(an => an.IsEquivalentTo(TinkerAsset)))
            {
                _companionData = null;
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
            Log(Constants.MOTION_CLS);
            foreach (TypeInfo typeInfo in typeof(IMotion).Assembly.DefinedTypes)
            {
                if (typeInfo.IsAssignableTo(typeof(IMotion)) && typeInfo.AssemblyQualifiedName != null)
                    Log(typeInfo.AssemblyQualifiedName);
            }

            Log("=== Ability ===", LogLevel.Info);
            Log(Constants.ABILITY_CLS);
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

        /// <summary>Get type from a string class name</summary>
        /// <param name="className"></param>
        /// <param name="typ"></param>
        /// <returns></returns>
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

        /// <summary>Get type from a string class name that is possibly in short form.</summary>
        /// <param name="className"></param>
        /// <param name="typ"></param>
        /// <param name="longFormat">Full class name format</param>
        /// <returns></returns>
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
