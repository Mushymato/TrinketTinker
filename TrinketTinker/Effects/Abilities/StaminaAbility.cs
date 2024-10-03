using Microsoft.Xna.Framework;
using StardewValley;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;

namespace TrinketTinker.Effects.Abilities
{
    /// <summary>Recover some percent of stamina.</summary>
    public class StaminaAbility(TrinketTinkerEffect effect, AbilityData data, int lvl) : Ability<PercentArgs>(effect, data, lvl)
    {
        private float StaminaFormula(float maximum, float current)
        {
            return (float)Math.Ceiling(Math.Min(maximum, current + maximum * args.Percent));
        }

        /// <summary>Heal % based on max Stamina</summary>
        /// <param name="farmer"></param>
        /// <returns></returns>
        protected override bool ApplyEffect(Farmer farmer)
        {
            float previous = farmer.Stamina;
            farmer.Stamina = StaminaFormula(farmer.MaxStamina, farmer.Stamina);
            float healed = farmer.Stamina - previous;
            if (healed > 0)
                farmer.currentLocation.debris.Add(new Debris((int)healed, farmer.getStandingPosition(), Color.ForestGreen, 1f, farmer));
            return healed > 0 && base.ApplyEffect(farmer);
        }
    }
}