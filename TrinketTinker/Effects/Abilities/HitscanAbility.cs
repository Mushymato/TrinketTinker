using StardewValley;
using StardewValley.Monsters;
using TrinketTinker.Effects.Proc;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;

namespace TrinketTinker.Effects.Abilities
{
    public class HitscanAbility(TrinketTinkerEffect effect, AbilityData data, int lvl) : Ability<HitscanArgs>(effect, data, lvl)
    {
        protected override bool ApplyEffect(ProcEventArgs proc)
        {
            Monster? target = proc.Monster ?? Utility.findClosestMonsterWithinRange(
                proc.LocationOrCurrent, e.CompanionPosition, args.Range, ignoreUntargetables: true
            );
            if (target == null)
                return false;
            proc.LocationOrCurrent.damageMonster(
                areaOfEffect: target.GetBoundingBox(),
                minDamage: args.Min,
                maxDamage: args.Max,
                isBomb: false,
                knockBackModifier: args.Knockback,
                addedPrecision: args.Precision,
                critChance: args.CriticalChance,
                critMultiplier: args.CriticalDamage,
                triggerMonsterInvincibleTimer: true,
                who: proc.Farmer,
                isProjectile: false
            );
            return base.ApplyEffect(proc);
        }
    }
}