using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Objects.Trinkets;
using StardewValley.Triggers;
using TrinketTinker.Effects;
using TrinketTinker.Models;

namespace TrinketTinker.Extras;

/// <summary>Payload of a broadcasted action</summary>
/// <param name="Condition"></param>
/// <param name="Actions"></param>
public sealed record BroadcastAction
{
    public string? Condition { get; set; } = null;
    public IEnumerable<string>? Actions { get; set; } = null;
};

public static class ProcTrinket
{
    internal const string BROADCAST_ACTION = "tt-broadcast-action";
    public static string TriggerActionNameOld => $"{ModEntry.ModId}/ProcTrinket";
    public static string TriggerActionName => $"{ModEntry.ModId}_ProcTrinket";

    /// <summary>Trigger action, proc trinkets that use <see cref="ProcOn.Trigger"/>.</summary>
    public static bool Action(string[] args, TriggerActionContext context, out string error)
    {
        if (!ArgUtility.TryGetOptional(args, 1, out string trinketId, out error))
            return false;

        foreach (Trinket trinketItem in Game1.player.trinketItems)
        {
            if (trinketItem == null)
                continue;
            if (
                (trinketId == null || trinketItem.ItemId == trinketId)
                && trinketItem.GetEffect() is TrinketTinkerEffect effect
            )
                effect.OnTrigger(Game1.player, args, context);
        }
        return true;
    }

    internal static void BroadcastAction(string? Condition, IEnumerable<string> Actions, long[] PlayerIds)
    {
        BroadcastAction toBroadcast = new() { Condition = Condition, Actions = Actions };
        ModEntry.Help.Multiplayer.SendMessage(
            toBroadcast,
            BROADCAST_ACTION,
            modIDs: [ModEntry.ModId],
            playerIDs: PlayerIds
        );
    }

    internal static void BroadcastedAction(ModMessageReceivedEventArgs e)
    {
        BroadcastAction broadcasted = e.ReadAs<BroadcastAction>();
        if (
            (broadcasted.Actions?.Any() ?? false)
            && GameStateQuery.CheckConditions(broadcasted.Condition, player: Game1.player)
        )
        {
            foreach (string actionStr in broadcasted.Actions)
            {
                if (!TriggerActionManager.TryRunAction(actionStr, out string error, out Exception _))
                {
                    ModEntry.LogOnce($"Couldn't apply action '{actionStr}': {error}", LogLevel.Error);
                }
            }
        }
    }
}
