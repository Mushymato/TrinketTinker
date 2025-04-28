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
    public bool FilterMonster(Monster m) =>
        (!args.FacingDirectionOnly || e.CompanionIsFacing(m.Position)) && !(args.Filters?.Contains(m.Name) ?? false);

    /// <inheritdoc/>
    protected override bool ApplyEffect(ProcEventArgs proc)
    {
        Monster? target =
            proc.Monster
            ?? Utility.findClosestMonsterWithinRange(
                proc.LocationOrCurrent,
                e.CompanionPosOff ?? proc.Farmer.Position,
                args.Range,
                ignoreUntargetables: true,
                match: FilterMonster
            );
        if (target == null)
            return false;
        proc.Monster = target;
        args.DamageMonster(proc.GSQContext, target);
        return base.ApplyEffect(proc);
    }
}
