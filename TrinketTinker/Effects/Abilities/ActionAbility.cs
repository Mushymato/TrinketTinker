using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Objects.Trinkets;
using StardewValley.Triggers;
using TrinketTinker.Effects.Support;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;
using TrinketTinker.Wheels;

namespace TrinketTinker.Effects.Abilities;

/// <summary>Call a registered (trigger) action.</summary>
public sealed class ActionAbility(TrinketTinkerEffect effect, AbilityData data, int lvl)
    : Ability<ActionArgs>(effect, data, lvl)
{
    internal static readonly string TriggerContextName = $"{ModEntry.ModId}/Action";

    /// <summary>Parse and call the action</summary>
    /// <param name="proc"></param>
    /// <returns></returns>
    private bool ApplyEffectOnActions(
        IEnumerable<CachedAction> actions,
        Farmer farmer,
        TriggerActionContext? TriggerContext = null
    )
    {
        TriggerActionContext context;
        if (TriggerContext != null)
            context = (TriggerActionContext)TriggerContext;
        else
            context = new TriggerActionContext(TriggerContextName, [], null, []);

        context.CustomFields[TinkerConst.CustomFields_Trinket] = e.Trinket;
        context.CustomFields[TinkerConst.CustomFields_Data] = d;
        context.CustomFields[TinkerConst.CustomFields_Owner] = farmer;
        context.CustomFields[TinkerConst.CustomFields_Position] = e.CompanionPosition;

        // if (!TriggerActionManager.TryRunAction(args.Action, out string error, out Exception _))
        //     ModEntry.LogOnce("Couldn't apply action '" + args.Action + "': " + error, LogLevel.Error);
        foreach (CachedAction action in actions)
        {
            if (!TriggerActionManager.TryRunAction(action, context, out string error, out Exception _))
            {
                ModEntry.LogOnce(
                    "Couldn't apply action '" + string.Join(' ', action.Args) + "': " + error,
                    LogLevel.Error
                );
            }
        }
        return true;
    }

    protected override bool ApplyEffect(ProcEventArgs proc)
    {
        return ApplyEffectOnActions(
                args.Actions.Select(TriggerActionManager.ParseAction),
                proc.Farmer,
                proc.TriggerContext
            ) && base.ApplyEffect(proc);
    }

    protected override void CleanupEffect(Farmer farmer)
    {
        ApplyEffectOnActions(args.ActionsEnd.Select(TriggerActionManager.ParseAction), farmer);
        base.CleanupEffect(farmer);
    }
}
