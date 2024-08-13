using StardewValley;
using TrinketTinker.Models;

namespace TrinketTinker.Effects.Abilities
{
    public class BuffAbility : Ability
    {
        private readonly string buffId = "";
        public BuffAbility(TrinketTinkerEffect effect, AbilityData data) : base(effect, data)
        {
            Valid = false;
            if (d.TryGetParsed("BuffId", out string? argsBuffId))
            {
                buffId = argsBuffId;
                Valid = true;
            }
        }

        protected override bool ApplyEffect(Farmer farmer)
        {
            ModEntry.Log($"{Name}.ApplyEffect: buffId={buffId}");
            farmer.applyBuff(buffId);
            return base.ApplyEffect(farmer);
        }
    }
}
