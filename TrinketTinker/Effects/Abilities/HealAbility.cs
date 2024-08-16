using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using TrinketTinker.Models;

namespace TrinketTinker.Effects.Abilities
{
    /// <summary>Recover some HP or stamina.</summary>
    public class HealAbility : Ability
    {
        /// <summary>Healing power, will get divided by 1000 for percent</summary>
        private readonly int healPower = 0;
        /// <summary>True if heal should target health</summary>
        private readonly bool targetHealth = false;
        /// <summary>True if heal should target stamina</summary>
        private readonly bool targetStamina = false;
        public HealAbility(TrinketTinkerEffect effect, AbilityData data) : base(effect, data)
        {
            Valid = false;
            if (d.TryGetParsed("HealPower", out int? healPowerVal))
            {
                healPower = (int)healPowerVal;
                targetHealth = d.ContainsKey("TargetHealth");
                targetStamina = d.ContainsKey("TargetStamina");
                Valid = healPowerVal != 0 && (targetHealth || targetStamina);
            }
        }

        /// <summary>Heal % based on max HP/Stamina</summary>
        /// <param name="farmer"></param>
        /// <returns></returns>
        protected override bool ApplyEffect(Farmer farmer)
        {
            if (targetHealth)
            {
                int previousHealth = farmer.health;
                farmer.health = Math.Min(farmer.maxHealth, farmer.health + farmer.maxHealth * healPower / 1000);
                int healed = farmer.health - previousHealth;
                if (healed > 0)
                {
                    farmer.currentLocation.debris.Add(new Debris(healed, farmer.getStandingPosition(), Color.Lime, 1f, farmer));
                    Game1.playSound("fairy_heal");
                }
            }
            if (targetStamina)
            {
                farmer.Stamina = Math.Min(farmer.MaxStamina, farmer.Stamina + farmer.MaxStamina * healPower / 1000);
            }
            return base.ApplyEffect(farmer);
        }

        /// <summary>Heal % based on damage taken</summary>
        /// <param name="farmer"></param>
        /// <param name="damageAmount"></param>
        /// <returns></returns>
        protected override bool ApplyEffect(Farmer farmer, int damageAmount)
        {
            if (targetHealth)
            {
                int previousHealth = farmer.health;
                farmer.health = Math.Min(farmer.maxHealth, farmer.health + damageAmount * healPower / 1000);
                int healed = farmer.health - previousHealth;
                if (healed > 0)
                {
                    farmer.currentLocation.debris.Add(new Debris(healed, farmer.getStandingPosition(), Color.Lime, 1f, farmer));
                }
            }
            if (targetStamina)
            {
                farmer.Stamina = Math.Min(farmer.MaxStamina, farmer.Stamina + farmer.MaxStamina * healPower / 1000);
            }
            return base.ApplyEffect(farmer, damageAmount);
        }

        /// <summary>Heal % based on damage dealt</summary>
        /// <param name="farmer"></param>
        /// <param name="damageAmount"></param>
        /// <returns></returns>
        protected override bool ApplyEffect(Farmer farmer, Monster monster, int damageAmount)
        {
            return base.ApplyEffect(farmer, monster, damageAmount);
        }

    }
}