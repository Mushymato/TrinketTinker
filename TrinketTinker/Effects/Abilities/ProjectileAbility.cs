using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using TrinketTinker.Effects.Pewpew;
using TrinketTinker.Effects.Proc;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;

namespace TrinketTinker.Effects.Abilities
{
    public class ProjectileAbility(TrinketTinkerEffect effect, AbilityData data, int lvl) : Ability<ProjectileArgs>(effect, data, lvl)
    {
        protected override bool ApplyEffect(ProcEventArgs proc)
        {
            Vector2 sourcePosition = e.CompanionPosition ?? proc.Farmer.Position;
            Monster? target = proc.Monster ?? Utility.findClosestMonsterWithinRange(
                proc.LocationOrCurrent, sourcePosition, args.Range, ignoreUntargetables: true
            );
            if (target == null)
                return false;

            proc.LocationOrCurrent.projectiles.Add(new TinkerProjectile(args, proc, target, sourcePosition));
            return base.ApplyEffect(proc);
        }
    }
}