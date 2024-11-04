using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using TrinketTinker.Effects.Proc;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;

namespace TrinketTinker.Effects.Abilities;

/// <summary>
/// Deal damage to monster within range, optionally stun them for a period.
/// </summary>
/// <param name="effect"></param>
/// <param name="data"></param>
/// <param name="lvl"></param>
public sealed class HitscanAbility(TrinketTinkerEffect effect, AbilityData data, int lvl) : Ability<DamageArgs>(effect, data, lvl)
{
    /// <inheritdoc/>
    protected override bool ApplyEffect(ProcEventArgs proc)
    {
        Monster? target = proc.Monster ?? Utility.findClosestMonsterWithinRange(
            proc.LocationOrCurrent,
            e.CompanionPosition ?? proc.Farmer.Position,
            base.args.Range,
            ignoreUntargetables: true
        );
        if (target == null)
            return false;
        proc.Monster = target;
        base.args.DamageMonster(proc, target);
        return base.ApplyEffect(proc);
    }

    /// <inheritdoc/>
    protected override Vector2 GetTASPosition(ProcEventArgs proc)
    {
        return proc.Monster!.getStandingPosition();
    }
}
