using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using TrinketTinker.Effects.Support;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;

namespace TrinketTinker.Effects.Abilities;

/// <summary>
/// Deal damage to monster within range, optionally stun them for a period.
/// </summary>
/// <param name="effect"></param>
/// <param name="data"></param>
/// <param name="lvl"></param>
public sealed class HitscanAbility(TrinketTinkerEffect effect, AbilityData data, int lvl)
    : Ability<DamageArgs>(effect, data, lvl)
{
    /// <inheritdoc/>
    protected override bool ApplyEffect(ProcEventArgs proc)
    {
        Monster? target =
            proc.Monster
            ?? Utility.findClosestMonsterWithinRange(
                proc.LocationOrCurrent,
                e.CompanionPosOff ?? proc.Farmer.Position,
                base.args.Range,
                ignoreUntargetables: true,
                match: args.Filters != null ? (m) => !args.Filters.Contains(m.Name) : null
            );
        if (target == null)
            return false;
        proc.Monster = target;
        base.args.DamageMonster(proc.LocationOrCurrent, proc.Farmer, target);
        return base.ApplyEffect(proc);
    }
}
