using StardewModdingAPI;
using StardewValley;
using StardewValley.Triggers;
using TrinketTinker.Effects.Support;
using TrinketTinker.Extras;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;

namespace TrinketTinker.Effects.Abilities;

/// <summary>Broadcast a registered trigger action to run on a target player.</summary>
public sealed class BroadcastActionAbility(TrinketTinkerEffect effect, AbilityData data, int lvl)
    : Ability<BroadcastActionArgs>(effect, data, lvl)
{
    private void BroadcastByPlayerKey(Farmer farmer, IEnumerable<string> Actions)
    {
        if (args.PlayerKey == "Current" || args.PlayerKey == "All" || (args.PlayerKey == "Host" && Game1.IsMasterGame))
        {
            if ((args.Actions?.Any() ?? false) && GameStateQuery.CheckConditions(args.Condition, player: Game1.player))
            {
                foreach (string actionStr in args.Actions)
                {
                    if (!TriggerActionManager.TryRunAction(actionStr, out string error, out Exception _))
                    {
                        ModEntry.LogOnce($"Couldn't apply action '{actionStr}': {error}", LogLevel.Error);
                    }
                }
            }
            if (args.PlayerKey != "All")
                return;
        }

        List<long> playerIds = [];
        GameStateQuery.Helpers.WithPlayer(
            farmer,
            args.PlayerKey,
            (Farmer target) =>
            {
                playerIds.Add(target.UniqueMultiplayerID);
                return false;
            }
        );
        ProcTrinket.BroadcastAction(args.Condition, Actions, playerIds.ToArray());
    }

    protected override bool ApplyEffect(ProcEventArgs proc)
    {
        BroadcastByPlayerKey(proc.Farmer, args.Actions);
        return base.ApplyEffect(proc);
    }

    protected override void CleanupEffect(Farmer farmer)
    {
        BroadcastByPlayerKey(farmer, args.ActionsEnd);
        base.CleanupEffect(farmer);
    }
}
