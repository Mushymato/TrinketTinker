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
        if (args.AoEFromCompanion)
        {
            // Only use Range as a trigger/start condition
            Monster? triggerTarget =
                proc.Monster
                ?? Utility.findClosestMonsterWithinRange(
                    proc.LocationOrCurrent,
                    e.CompanionPosOff ?? proc.Farmer.Position,
                    args.Range,
                    ignoreUntargetables: true,
                    match: FilterMonster
                );

            if (triggerTarget == null)
                return false;

            proc.Monster = triggerTarget;

            void DoPulse() //Damage monsters around the trinket's current position
            {
                Vector2 center = e.CompanionPosOff ?? proc.Farmer.Position;
                args.DamageAtPoint(proc.GSQContext, center, false, FilterMonster);
            }

            DoPulse(); //Can be repeated AoE pulses from the trinket's moving position

            if (args.HitsDelay == 0)
            {
                for (int i = 1; i < args.Hits; i++)
                    DoPulse();
            }
            else
            {
                for (int i = 1; i < args.Hits; i++)
                    DelayedAction.functionAfterDelay(DoPulse, i * args.HitsDelay);
            }

            return base.ApplyEffect(proc);
        }

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
        args.DamageMonster(proc.GSQContext, target, false);
        return base.ApplyEffect(proc);
    }
}