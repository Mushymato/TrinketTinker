using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using TrinketTinker.Models;

namespace TrinketTinker.Effects.Abilities
{
    /// <summary>Prints many logs, doesn't do anything else.</summary>
    public class DebugDummyAbility : Ability
    {
        public DebugDummyAbility(TrinketTinkerEffect effect, AbilityData data, int lvl) : base(effect, data, lvl)
        {
            Valid = true;
        }

        protected override bool ApplyEffect(Farmer farmer)
        {
            ModEntry.Log($"{Name}.ApplyEffect({farmer})");
            return base.ApplyEffect(farmer);
        }
        protected override bool ApplyEffect(Farmer farmer, int damageAmount)
        {
            ModEntry.Log($"{Name}.ApplyEffect({farmer}, {damageAmount})");
            return base.ApplyEffect(farmer, damageAmount);
        }
        protected override bool ApplyEffect(Farmer farmer, Monster monster, int damageAmount)
        {
            ModEntry.Log($"{Name}.ApplyEffect({farmer}, {monster}, {damageAmount})");
            return ApplyEffect(farmer, monster, damageAmount);
        }
        protected override bool ApplyEffect(Farmer farmer, GameTime time, GameLocation location)
        {
            ModEntry.Log($"{Name}.ApplyEffect({farmer}, {time}, {location})");
            return ApplyEffect(farmer, time, location);
        }

    }
}
