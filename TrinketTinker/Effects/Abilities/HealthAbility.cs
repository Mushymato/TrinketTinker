using Microsoft.Xna.Framework;
using StardewValley;
using TrinketTinker.Effects.Proc;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;

namespace TrinketTinker.Effects.Abilities
{
    /// <summary>
    /// Recover some percent of HP.
    /// For <see cref="ProcOn.DamageMonster"/> and <see cref="ProcOn.ReceiveDamage"/>,
    /// healing is based on damage recieved or dealt instead of percent HP.
    /// </summary>
    public sealed class HealthAbility(TrinketTinkerEffect effect, AbilityData data, int lvl) : Ability<PercentArgs>(effect, data, lvl)
    {
        /// <summary>Healing formula</summary>
        /// <param name="maximum"></param>
        /// <param name="current"></param>
        /// <param name="relative"></param>
        /// <returns></returns>
        private int HealthFormula(int maximum, int current, int relative)
        {
            return (int)Math.Ceiling(Math.Min(maximum, current + relative * args.Rand));
        }

        /// <summary>
        /// Heal the player.
        /// If a damage amount is given, heal % of that value, otherwise heal % of max health.
        /// </summary>
        /// <param name="proc"></param>
        /// <returns></returns>
        protected override bool ApplyEffect(ProcEventArgs proc)
        {
            if (proc.Farmer is not Farmer farmer)
                return false;
            int previousHealth = farmer.health;
            farmer.health = HealthFormula(farmer.maxHealth, farmer.health, proc.DamageAmount ?? farmer.maxHealth);
            int healed = farmer.health - previousHealth;
            if (healed > 0)
                farmer.currentLocation.debris.Add(new Debris(healed, farmer.getStandingPosition(), Color.Lime, 1f, farmer));
            return healed > 0 && base.ApplyEffect(proc);
        }
    }
}