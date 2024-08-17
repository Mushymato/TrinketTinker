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
        protected readonly int healPower = 0;
        /// <summary>True if heal should target health</summary>
        protected readonly bool targetHealth = false;
        /// <summary>True if heal should target stamina</summary>
        protected readonly bool targetStamina = false;
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

        /// <summary>Healing formula</summary>
        /// <param name="maximum"></param>
        /// <param name="current"></param>
        /// <param name="relative"></param>
        /// <returns></returns>
        private int HealthFormula(int maximum, int current, int relative)
        {
            return (int)Math.Ceiling(Math.Min(maximum, current + relative * healPower / 1000.0));
        }

        private float StaminaFormula(float maximum, float current)
        {
            return (float)Math.Ceiling(Math.Min(maximum, current + maximum * healPower / 1000.0));
        }

        /// <summary>Heal % based on max HP/Stamina</summary>
        /// <param name="farmer"></param>
        /// <returns></returns>
        protected override bool ApplyEffect(Farmer farmer)
        {
            bool playSound = false;
            if (targetHealth)
            {
                int previousHealth = farmer.health;
                farmer.health = HealthFormula(farmer.maxHealth, farmer.health, farmer.maxHealth);
                int healed = farmer.health - previousHealth;
                if (healed > 0)
                {
                    farmer.currentLocation.debris.Add(new Debris(healed, farmer.getStandingPosition(), Color.Lime, 1f, farmer));
                    playSound = true;
                }
            }
            if (targetStamina)
            {
                farmer.Stamina = StaminaFormula(farmer.MaxStamina, farmer.Stamina);
            }
            return base.ApplyEffect(farmer) && playSound;
        }

        /// <summary>Heal % based on damage taken</summary>
        /// <param name="farmer"></param>
        /// <param name="damageAmount"></param>
        /// <returns></returns>
        protected override bool ApplyEffect(Farmer farmer, int damageAmount)
        {
            bool playSound = false;
            if (targetHealth)
            {
                int previousHealth = farmer.health;
                farmer.health = HealthFormula(farmer.maxHealth, farmer.health, damageAmount);
                int healed = farmer.health - previousHealth;
                if (healed > 0)
                {
                    farmer.currentLocation.debris.Add(new Debris(healed, farmer.getStandingPosition(), Color.Lime, 1f, farmer));
                    playSound = true;
                }
            }
            if (targetStamina)
            {
                farmer.Stamina = StaminaFormula(farmer.MaxStamina, farmer.Stamina);
            }
            return base.ApplyEffect(farmer, damageAmount) && playSound;
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