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
    internal bool activatedByAlways = false;

    private void BroadcastByPlayerKey(Farmer farmer, IEnumerable<string> Actions)
    {
        if (args.PlayerKey == "Current" || args.PlayerKey == "All" || (args.PlayerKey == "Host" && Game1.IsMasterGame))
        {
            if (Actions.Any() && GameStateQuery.CheckConditions(args.Condition, player: Game1.player))
            {
                foreach (string actionStr in Actions)
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
        if (proc.Proc == ProcOn.Always)
            activatedByAlways = true;
        BroadcastByPlayerKey(proc.Farmer, args.AllActions);
        return base.ApplyEffect(proc);
    }

    protected override void CleanupEffect(Farmer farmer)
    {
        if (activatedByAlways)
            BroadcastByPlayerKey(farmer, args.AllActionsEnd);
        base.CleanupEffect(farmer);
    }
}
