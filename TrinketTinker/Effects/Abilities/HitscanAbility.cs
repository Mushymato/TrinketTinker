using StardewValley.Monsters;
using TrinketTinker.Effects.Support;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;
using TrinketTinker.Wheels;

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
        List<Monster> targets = Places.SelectMatchingMonsters(
            proc.LocationOrCurrent,
            e.CompanionPosOff ?? proc.Farmer.Position,
            args.Range,
            args.TargetCount,
            args.TargetMode,
            proc.Monster,
            FilterMonster
        );

        if (targets.Count == 0)
            return false;

        proc.Monster = targets[0];

        foreach (Monster target in targets)
        {
            args.DamageMonster(proc.GSQContext, target, false);
        }

        return base.ApplyEffect(proc);
    }
}
