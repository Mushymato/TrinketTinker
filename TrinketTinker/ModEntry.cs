global using SObject = StardewValley.Object;
using System.Reflection;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Internal;
using StardewValley.Objects.Trinkets;
using StardewValley.Triggers;
using TrinketTinker.Companions;
using TrinketTinker.Companions.Motions;
using TrinketTinker.Effects;
using TrinketTinker.Effects.Abilities;
using TrinketTinker.Extras;
using TrinketTinker.Wheels;

namespace TrinketTinker;

internal sealed class ModEntry : Mod
{
#if DEBUG
    private const LogLevel DEFAULT_LOG_LEVEL = LogLevel.Debug;
#else
    private const LogLevel DEFAULT_LOG_LEVEL = LogLevel.Trace;
#endif

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
        // Events for abilities
        helper.Events.Player.Warped += OnPlayerWarped;

#if DEBUG
        // Print all types
        helper.ConsoleCommands.Add(
            "tt_print_types",
            "Print valid Effect, Companion, Motion, and Ability types.",
            ConsolePrintTypenames
        );
        // Spawn a bunch of forage around the player
        helper.ConsoleCommands.Add("tt_spawn_forage", "Spawn forage for testing.", ConsoleSpawnForage);
#endif
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        // Add trigger & action
        TriggerActionManager.RegisterAction(ProcTrinket.TriggerActionName, ProcTrinket.Action);
        TriggerActionManager.RegisterTrigger(RaiseTriggerAbility.TriggerEventName);

        // Add item query for creating a trinket with specific level and variant
        ItemQueryResolver.Register(ItemQuery.ItemQuery_CREATE_TRINKET, ItemQuery.CREATE_TRINKET);
        // Add item query for creating all variants of a trinket
        ItemQueryResolver.Register(
            ItemQuery.ItemQuery_CREATE_TRINKET_ALL_VARIANTS,
            ItemQuery.CREATE_TRINKET_ALL_VARIANTS
        );

        // Add GSQ for checking trinket is for this mod
        GameStateQuery.Register(ItemQuery.GameStateQuery_INPUT_IS_TINKER, ItemQuery.INPUT_IS_TINKER);
    }

    private static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        // load the custom asset
        AssetManager.OnAssetRequested(e);
        // add a big craftable for recoloring stuff
        TrinketColorizer.OnAssetRequested(e);
    }

    private void OnAssetInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        if (AssetManager.OnAssetInvalidated(e))
        {
            // need to invalidate this as well to ensure proper updates on Data/Trinkets
            Helper.GameContent.InvalidateCache(AssetManager.TRINKET_TARGET);
        }
    }

    private static void OnPlayerWarped(object? sender, WarpedEventArgs e)
    {
        if (!e.IsLocalPlayer)
            return;
        foreach (Trinket trinketItem in e.Player.trinketItems)
        {
            if (trinketItem != null && trinketItem.GetEffect() is TrinketTinkerEffect effect)
            {
                effect.OnPlayerWarped(e.Player, e.OldLocation, e.NewLocation);
            }
        }
    }

#if DEBUG
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

    private void ConsoleSpawnForage(string command, string[] args)
    {
        if (!Context.IsWorldReady)
            return;

        for (int i = 0; i < 30; i++)
        {
            Vector2 tilePos =
                new(
                    Random.Shared.Next(Game1.currentLocation.map.DisplayWidth / 64),
                    Random.Shared.Next(Game1.currentLocation.map.DisplayHeight / 64)
                );
            Log($"Spawn? {tilePos}");
            SObject forage = (SObject)ItemRegistry.Create("(O)16");
            if (Game1.currentLocation.dropObject(forage, tilePos * 64f, Game1.viewport, initialPlacement: true))
                Log("Yes");
        }
    }
#endif

    /// Static helper functions

    /// <summary>Static SMAPI logger</summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    public static void Log(string msg, LogLevel level = DEFAULT_LOG_LEVEL)
    {
        mon!.Log(msg, level);
    }

    /// <summary>Static SMAPI logger, only logs the same message once</summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    public static void LogOnce(string msg, LogLevel level = DEFAULT_LOG_LEVEL)
    {
        level = (level == LogLevel.Trace) ? LogLevel.Debug : level;
        mon!.LogOnce(msg, level);
    }
}
