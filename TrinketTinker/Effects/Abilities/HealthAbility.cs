using Microsoft.Xna.Framework;
using StardewValley;
using TrinketTinker.Effects.Support;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;

namespace TrinketTinker.Effects.Abilities;

/// <summary>
/// Recover some percent of HP.
/// For <see cref="ProcOn.DamageMonster"/> and <see cref="ProcOn.ReceiveDamage"/>,
/// healing is based on damage recieved or dealt instead of percent HP.
/// </summary>
public sealed class HealthAbility(TrinketTinkerEffect effect, AbilityData data, int lvl)
    : Ability<ValueRangeArgs>(effect, data, lvl)
{
    /// <summary>
    /// Heal the player.
    /// If a damage amount is given, heal % of that value, otherwise heal % of max health.
    /// </summary>
    /// <param name="proc"></param>
    /// <returns></returns>
    protected override bool ApplyEffect(ProcEventArgs proc)
    {
        int healed = (int)
            Math.Ceiling(
                Math.Min(
                    proc.Farmer.maxHealth - proc.Farmer.health,
                    args.Rand(proc.DamageAmount ?? proc.Farmer.maxHealth)
                )
            );
        proc.Farmer.health += healed;
        if (healed > 0)
            proc.Farmer.currentLocation.debris.Add(
                new Debris(healed, proc.Farmer.getStandingPosition(), Color.Lime, 1f, proc.Farmer)
            );
        return base.ApplyEffect(proc);
    }
}
