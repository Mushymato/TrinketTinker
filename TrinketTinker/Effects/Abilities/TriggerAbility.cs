using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Triggers;
using TrinketTinker.Models;

namespace TrinketTinker.Effects.Abilities
{
    /// <summary>Raises a trigger (<see cref="TriggerEventName"/>) on proc.</summary>
    public class TriggerAbility : Ability
    {
        public static readonly string TriggerEventName = $"{ModEntry.ModId}_TrinketProc";
        public TriggerAbility(TrinketTinkerEffect effect, AbilityData data) : base(effect, data)
        {
            Valid = true;
        }

        protected override bool ApplyEffect(Farmer farmer)
        {
            TriggerActionManager.Raise(TriggerEventName, player: farmer, targetItem: e.Trinket);
            return base.ApplyEffect(farmer);
        }

        protected override bool ApplyEffect(Farmer farmer, GameTime time, GameLocation location)
        {
            TriggerActionManager.Raise(TriggerEventName, location: location, player: farmer, targetItem: e.Trinket);
            return ApplyEffect(farmer, time, location);
        }

    }
}
