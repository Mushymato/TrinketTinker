using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Triggers;
using TrinketTinker.Models;
using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Effects.Abilities
{
    /// <summary>
    /// Raises a trigger (<see cref="TriggerEventName"/>) on proc.
    /// The trinket is given as the target item.
    /// </summary>
    public class TriggerAbility(TrinketTinkerEffect effect, AbilityData data, int lvl) : Ability<NoArgs>(effect, data, lvl)
    {
        public static readonly string TriggerEventName = $"{ModEntry.ModId}_TrinketProc";

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
