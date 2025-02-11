global using SObject = StardewValley.Object;
using System.Reflection;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
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

    public static ModConfig Config { get; set; } = null!;
    internal static IModHelper Help { get; set; } = null!;

    internal static bool HasWearMoreRings = false;

    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        mon = Monitor;
        Help = Helper;
        ModId = ModManifest.UniqueID;

        // Config is not player facing atm, just holds whether draw debug mode is on.
        Config = Helper.ReadConfig<ModConfig>();

        // Events for game launch and custom asset
        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.Content.AssetRequested += OnAssetRequested;
        helper.Events.Content.AssetsInvalidated += OnAssetInvalidated;
        // Events for abilities
        helper.Events.Player.Warped += OnPlayerWarped;
        helper.Events.Input.ButtonsChanged += OnButtonsChanged;
        helper.Events.GameLoop.Saving += OnSaving;
        helper.Events.GameLoop.DayStarted += OnDayStarted;
        helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;

        helper.ConsoleCommands.Add(
            "tt_draw_debug",
            "Toggle drawing of the sprite index when drawing companions.",
            ConsoleDrawDebugToggle
        );
        helper.ConsoleCommands.Add(
            "tt_unequip_trinket",
            "Debug unequip all trinkets of current player and send the trinkets to lost and found.",
            ConsoleUnequipTrinkets
        );

#if DEBUG
        // Print all types
        helper.ConsoleCommands.Add(
            "tt_print_types",
            "Print valid Effect, Companion, Motion, and Ability types.",
            ConsolePrintTypenames
        );
        // Spawn a bunch of forage around the player
        helper.ConsoleCommands.Add("tt_spawn_forage", "Spawn forage for testing.", ConsoleSpawnForage);
        // Print all global inventories that exist
        helper.ConsoleCommands.Add("tt_global_inv", "Check all global inventories.", ConsoleGlobalInv);
        helper.ConsoleCommands.Add(
            "tt_print_trinkets",
            "Debug unequip all trinkets of current player and send the trinkets to lost and found.",
            ConsolePrintTrinkets
        );
#endif
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        // Add trigger & action
        TriggerActionManager.RegisterTrigger(RaiseTriggerAbility.TriggerEventName);
        TriggerActionManager.RegisterAction(ProcTrinket.TriggerActionNameOld, ProcTrinket.Action);
        TriggerActionManager.RegisterAction(ProcTrinket.TriggerActionName, ProcTrinket.Action);
        TriggerActionManager.RegisterAction(EquipTrinket.Action_EquipHiddenTrinket, EquipTrinket.EquipHiddenTrinket);
        TriggerActionManager.RegisterAction(
            EquipTrinket.Action_UnequipHiddenTrinket,
            EquipTrinket.UnequipHiddenTrinket
        );
        // Add item queries
        GameItemQuery.Register();
        // Check for WearMoreRings, which adds a 2nd trinket slot
        HasWearMoreRings = Helper.ModRegistry.IsLoaded("bcmpinc.WearMoreRings");
        Config.Register(Helper, ModManifest);
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

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (Game1.activeClickableMenu == null && Config.OpenTinkerInventoryKey.JustPressed())
        {
            GlobalInventoryHandler pagedInvHandler = new(Game1.player);
            if (pagedInvHandler.pagedInfo.Count > 0)
            {
                Game1.activeClickableMenu = pagedInvHandler.GetMenu();
            }
            return;
        }
        if (Game1.activeClickableMenu is TinkerInventoryMenu menu && menu.pageMethod != null)
        {
            if (Config.TinkerInventoryNextKey.JustPressed())
                menu.pageMethod(1);
            else if (Config.TinkerInventoryPrevKey.JustPressed())
                menu.pageMethod(-1);
            return;
        }
        foreach (Trinket trinketItem in Game1.player.trinketItems)
        {
            if (trinketItem != null && trinketItem.GetEffect() is TrinketTinkerEffect effect)
            {
                effect.OnButtonsChanged(Game1.player, e);
            }
        }
    }

    private void OnSaving(object? sender, SavingEventArgs e)
    {
        EquipTrinket.UnequipHiddenTrinkets();
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        EquipTrinket.ReequipHiddenTrinkets();
    }

    private void OnDayEnding(object? sender, DayEndingEventArgs e)
    {
        GlobalInventoryHandler.DayEndingCleanup();
    }

    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        EquipTrinket.ReequipHiddenTrinkets();
        EquipTrinket.FixVanillaDupeCompanions();
    }

    private void OnModMessageReceived(object? sender, ModMessageReceivedEventArgs e)
    {
        if (e.FromModID != ModId)
        {
            if (e.FromModID.StartsWith("Spiderbuttons."))
                LogOnce("There be spooders in here ::::)", LogLevel.Debug);
            return;
        }
        ProcTrinket.BroadcastedAction(e);
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

    private void ConsolePrintTrinkets(string arg1, string[] arg2)
    {
        if (!Context.IsWorldReady)
            return;
        var trinketItems = Game1.player.trinketItems;
        for (int i = 0; i < trinketItems.Count; i++)
        {
            Log($"trinketItems[{i}] is {trinketItems[i]?.QualifiedItemId ?? "NULL"}");
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

    private void ConsoleGlobalInv(string arg1, string[] arg2)
    {
        if (!Context.IsWorldReady)
            return;

        foreach (var key in Game1.player.team.globalInventories.Keys)
        {
            var value = Game1.player.team.globalInventories[key];
            if (value == null)
                continue;
            Log($"{key}: {value.Count}");
            foreach (var item in value)
            {
                Log($"- {item.QualifiedItemId} {item.Stack}");
            }
        }
    }
#endif

    private void ConsoleDrawDebugToggle(string arg1, string[] arg2)
    {
        if (Config != null)
        {
            Config.DrawDebugMode = !Config.DrawDebugMode;
            Log($"DrawDebugMode: {Config.DrawDebugMode}", LogLevel.Info);
            Helper.WriteConfig(Config);
        }
    }

    private void ConsoleUnequipTrinkets(string arg1, string[] arg2)
    {
        if (!Context.IsWorldReady)
            return;
        foreach (Trinket trinketItem in Game1.player.trinketItems)
        {
            if (trinketItem == null)
                continue;
            if (trinketItem.GetEffect() is TrinketTinkerEffect effect && effect.HasEquipTrinketAbility)
                Log($"UnequipTrinket: {trinketItem.QualifiedItemId}", LogLevel.Info);
            if (!trinketItem.modData.ContainsKey(TinkerConst.ModData_IndirectEquip))
                Game1.player.team.returnedDonations.Add(trinketItem);
            Game1.player.team.newLostAndFoundItems.Value = true;
        }
        Game1.player.trinketItems.Clear();
        Game1.player.companions.Clear();
    }

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
        mon!.LogOnce(msg, level);
    }
}
