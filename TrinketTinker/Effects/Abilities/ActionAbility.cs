using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Objects.Trinkets;
using StardewValley.Triggers;
using TrinketTinker.Effects.Support;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;

namespace TrinketTinker.Effects.Abilities;

/// <summary>Call a registered (trigger) action.</summary>
public sealed class ActionAbility(TrinketTinkerEffect effect, AbilityData data, int lvl)
    : Ability<ActionArgs>(effect, data, lvl)
{
    internal static readonly string TriggerContextName = $"{ModEntry.ModId}/Action";
    internal static readonly string CustomFields_Owner = $"{ModEntry.ModId}/Owner";
    internal static readonly string CustomFields_Trinket = $"{ModEntry.ModId}/Trinket";
    internal static readonly string CustomFields_Data = $"{ModEntry.ModId}/Data";
    internal static readonly string CustomFields_Position = $"{ModEntry.ModId}/Position";

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

        context.CustomFields[CustomFields_Trinket] = e.Trinket;
        context.CustomFields[CustomFields_Data] = d;
        context.CustomFields[CustomFields_Owner] = farmer;
        context.CustomFields[CustomFields_Position] = e.CompanionPosition;

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
