using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using TrinketTinker.Models;
using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Effects.Abilities
{
    /// <summary>Prints many logs, doesn't do anything else.</summary>
    public class DebugDummyAbility(TrinketTinkerEffect effect, AbilityData data, int lvl) : Ability<NoArgs>(effect, data, lvl)
    {
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
        protected override bool ApplyEffect(Farmer farmer, Monster monster, int damageAmount, bool isBomb, bool isCriticalHit)
        {
            ModEntry.Log($"{Name}.ApplyEffect({farmer}, {monster}, {damageAmount})");
            return ApplyEffect(farmer, monster, damageAmount, isBomb, isCriticalHit);
        }
        protected override bool ApplyEffect(Farmer farmer, GameTime time, GameLocation location)
        {
            ModEntry.Log($"{Name}.ApplyEffect({farmer}, {time}, {location})");
            return ApplyEffect(farmer, time, location);
        }
    }
}
