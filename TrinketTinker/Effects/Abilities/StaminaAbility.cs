using Microsoft.Xna.Framework;
using StardewValley;
using TrinketTinker.Effects.Proc;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;

namespace TrinketTinker.Effects.Abilities;

/// <summary>Recover some percent of stamina.</summary>
public sealed class StaminaAbility(TrinketTinkerEffect effect, AbilityData data, int lvl)
    : Ability<RangeArgs>(effect, data, lvl)
{
    /// <summary>
    /// Recover % stamina.
    /// If a damage amount is given, recover % of that value, otherwise recover % of max stamina.
    /// </summary>
    /// <param name="proc"></param>
    /// <returns></returns>
    protected override bool ApplyEffect(ProcEventArgs proc)
    {
        float healed = (float)
            Math.Ceiling(
                Math.Min(
                    proc.Farmer.MaxStamina - proc.Farmer.Stamina,
                    args.Rand(proc.DamageAmount ?? proc.Farmer.MaxStamina)
                )
            );
        proc.Farmer.Stamina += healed;
        if (healed > 0)
            proc.Farmer.currentLocation.debris.Add(
                new Debris((int)healed, proc.Farmer.getStandingPosition(), Color.SeaGreen, 1f, proc.Farmer)
            );
        return healed > 0 && base.ApplyEffect(proc);
    }
}
