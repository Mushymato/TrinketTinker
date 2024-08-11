using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
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
    }
}
