using StardewModdingAPI;
using StardewValley.Triggers;
using TrinketTinker.Effects.Proc;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;

namespace TrinketTinker.Effects.Abilities
{
    /// <summary>
    /// Call a registered (trigger) action.
    /// </summary>
    public sealed class ActionAbility(TrinketTinkerEffect effect, AbilityData data, int lvl) : Ability<ActionArgs>(effect, data, lvl)
    {
        /// <summary>Parse and call the action</summary>
        /// <param name="proc"></param>
        /// <returns></returns>
        protected override bool ApplyEffect(ProcEventArgs proc)
        {
            if (!TriggerActionManager.TryRunAction(args.Action, out string error, out Exception _))
                ModEntry.LogOnce("Couldn't apply action '" + args.Action + "': " + error, LogLevel.Error);
            return base.ApplyEffect(proc);
        }
    }
}
