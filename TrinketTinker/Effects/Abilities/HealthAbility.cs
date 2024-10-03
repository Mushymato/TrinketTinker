using Microsoft.Xna.Framework;
using StardewValley;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;

namespace TrinketTinker.Effects.Abilities
{
    /// <summary>
    /// Recover some percent of HP.
    /// For <see cref="ProcOn.DamageMonster"/> and <see cref="ProcOn.ReceiveDamage"/>,
    /// healing is based on damage recieved or dealt instead of percent HP.
    /// </summary>
    public class HealthAbility(TrinketTinkerEffect effect, AbilityData data, int lvl) : Ability<PercentArgs>(effect, data, lvl)
    {
        /// <summary>Healing formula</summary>
        /// <param name="maximum"></param>
        /// <param name="current"></param>
        /// <param name="relative"></param>
        /// <returns></returns>
        private int HealthFormula(int maximum, int current, int relative)
        {
            return (int)Math.Ceiling(Math.Min(maximum, current + relative * args.Percent));
        }

        /// <summary>Heal % based on max HP</summary>
        /// <param name="farmer"></param>
        /// <returns></returns>
        protected override bool ApplyEffect(Farmer farmer)
        {
            int previousHealth = farmer.health;
            farmer.health = HealthFormula(farmer.maxHealth, farmer.health, farmer.maxHealth);
            int healed = farmer.health - previousHealth;
            if (healed > 0)
                farmer.currentLocation.debris.Add(new Debris(healed, farmer.getStandingPosition(), Color.Lime, 1f, farmer));
            return healed > 0 && base.ApplyEffect(farmer);
        }

        /// <summary>Heal % based on damage taken</summary>
        /// <param name="farmer"></param>
        /// <param name="damageAmount"></param>
        /// <returns></returns>
        protected override bool ApplyEffect(Farmer farmer, int damageAmount)
        {
            int previousHealth = farmer.health;
            farmer.health = HealthFormula(farmer.maxHealth, farmer.health, damageAmount);
            int healed = farmer.health - previousHealth;
            if (healed > 0)
                farmer.currentLocation.debris.Add(new Debris(healed, farmer.getStandingPosition(), Color.Lime, 1f, farmer));
            return healed > 0 && base.ApplyEffect(farmer, damageAmount);
        }
    }
}