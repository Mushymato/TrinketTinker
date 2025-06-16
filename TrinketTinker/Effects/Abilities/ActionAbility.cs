using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;
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
    internal const string TriggerContextName = $"{ModEntry.ModId}/Action";
    internal bool activatedByAlways = false;

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

        if (context.CustomFields != null)
        {
            context.CustomFields[TinkerConst.CustomFields_Trinket] = e.Trinket;
            context.CustomFields[TinkerConst.CustomFields_Owner] = farmer;
            context.CustomFields[TinkerConst.CustomFields_Position] = e.CompanionPosition;
            context.CustomFields[TinkerConst.CustomFields_PosOff] = e.CompanionPosOff;
            context.CustomFields[TinkerConst.CustomFields_Data] = d;
        }

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
        if (proc.Proc == ProcOn.Always)
            activatedByAlways = true;
        return ApplyEffectOnActions(
                args.AllActions.Select(TriggerActionManager.ParseAction),
                proc.Farmer,
                proc.TriggerContext
            ) && base.ApplyEffect(proc);
    }

    protected override void CleanupEffect(Farmer farmer)
    {
        if (activatedByAlways)
            ApplyEffectOnActions(args.AllActionsEnd.Select(TriggerActionManager.ParseAction), farmer);
        base.CleanupEffect(farmer);
    }
}
