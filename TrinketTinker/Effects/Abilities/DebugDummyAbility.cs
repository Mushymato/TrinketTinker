using StardewValley;
using TrinketTinker.Models;

namespace TrinketTinker.Effects.Abilities
{
    public class DebugDummyAbility : Ability
    {
        public DebugDummyAbility(TrinketTinkerEffect effect, AbilityData data) : base(effect, data)
        {
            ModEntry.Log($"Ability.ctor({effect})", logLvl);
            Valid = true;
        }

        protected override bool ApplyEffect(Farmer farmer)
        {
            ModEntry.Log($"{Name}.ApplyEffect");
            return base.ApplyEffect(farmer);
        }
    }
}
