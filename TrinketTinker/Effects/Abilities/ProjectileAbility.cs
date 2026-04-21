using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using TrinketTinker.Effects.Support;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;
using TrinketTinker.Wheels;

namespace TrinketTinker.Effects.Abilities;

/// <summary>
/// Shoot a projectile that deals damage to monster within range, optionally stun them for a period.
/// </summary>
/// <param name="effect"></param>
/// <param name="data"></param>
/// <param name="lvl"></param>
public sealed class ProjectileAbility(TrinketTinkerEffect effect, AbilityData data, int lvl)
    : Ability<ProjectileArgs>(effect, data, lvl)
{
    public bool FilterMonster(Monster m) =>
        (!args.FacingDirectionOnly || e.CompanionIsFacing(m.Position)) && !(args.Filters?.Contains(m.Name) ?? false);

    protected override bool ApplyEffect(ProcEventArgs proc)
    {
        Vector2 sourcePosition = e.CompanionPosition ?? proc.Farmer.Position;

        List<Monster> targets = Places.SelectMatchingMonsters(
            proc.LocationOrCurrent,
            sourcePosition,
            args.Range,
            args.MaxTargetCount,
            args.TargetMode,
            proc.Monster,
            FilterMonster
        );

        if (targets.Count == 0)
            return false;

        proc.Monster = targets[0];

        foreach (Monster target in targets)
        {
            proc.LocationOrCurrent.projectiles.Add(new TinkerProjectile(args, proc, target, sourcePosition));
        }

        return base.ApplyEffect(proc);
    }
}