using StardewValley.Triggers;
using TrinketTinker.Effects.Support;
using TrinketTinker.Models;
using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Effects.Abilities;

/// <summary>
/// Raises a trigger (<see cref="TriggerEventName"/>) on proc.
/// The trinket is given as the target item.
/// </summary>
public sealed class RaiseTriggerAbility(TrinketTinkerEffect effect, AbilityData data, int lvl)
    : Ability<NoArgs>(effect, data, lvl)
{
    public const string TriggerEventName = $"{ModEntry.ModId}/TrinketProc";

    /// <summary>Raise the trigger <see cref="TriggerEventName"/></summary>
    /// <param name="proc"></param>
    /// <returns></returns>
    protected override bool ApplyEffect(ProcEventArgs proc)
    {
        TriggerActionManager.Raise(
            TriggerEventName,
            player: proc.Farmer,
            location: proc.Location,
            targetItem: e.Trinket,
            inputItem: e.Trinket
        );
        return base.ApplyEffect(proc);
    }
}
