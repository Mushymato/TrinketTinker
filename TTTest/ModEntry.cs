using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects.Trinkets;

namespace TTTest;

public sealed class ModEntry : Mod
{
#if DEBUG
    private const LogLevel DEFAULT_LOG_LEVEL = LogLevel.Debug;
#else
    private const LogLevel DEFAULT_LOG_LEVEL = LogLevel.Trace;
#endif

    public const string ModId = "mushymato.TTTest";
    private static IMonitor? mon;
    private static Integration.ITrinketTinkerAPI? tt;
    private static Trinket? trinket = null;

    public override void Entry(IModHelper helper)
    {
        mon = Monitor;
        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.ConsoleCommands.Add("tttest", "Test toggling trinket", ConsoleCommandTTTest);
    }

    private void ConsoleCommandTTTest(string arg1, string[] arg2)
    {
        if (!Context.IsWorldReady)
            return;
        trinket = (Trinket)ItemRegistry.Create("(TR)MagicHairDye");
        if (tt?.TryEquipHiddenTrinket(trinket, out string? guid) ?? false)
        {
            Log($"Equipped {trinket.QualifiedItemId}");
            DelayedAction.functionAfterDelay(
                () =>
                {
                    if (tt.TryUnequipHiddenTrinket(guid))
                    {
                        Log($"Unequipped {trinket.QualifiedItemId}");
                    }
                    else
                    {
                        Log($"Failed to unequip {trinket.QualifiedItemId}");
                    }
                },
                5000
            );
        }
        else
        {
            Log($"Failed to equip {trinket.QualifiedItemId}");
        }
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        tt = Helper.ModRegistry.GetApi<Integration.ITrinketTinkerAPI>("mushymato.TrinketTinker");
    }

    /// <summary>SMAPI static monitor Log wrapper</summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    internal static void Log(string msg, LogLevel level = DEFAULT_LOG_LEVEL)
    {
        mon!.Log(msg, level);
    }

    /// <summary>SMAPI static monitor LogOnce wrapper</summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    internal static void LogOnce(string msg, LogLevel level = DEFAULT_LOG_LEVEL)
    {
        mon!.LogOnce(msg, level);
    }
}
