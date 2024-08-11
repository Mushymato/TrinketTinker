using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using TrinketTinker.Models;

namespace TrinketTinker.Effects.Abilities
{
    public class BuffAbility : Ability
    {
        private readonly string buffId = "";
        public BuffAbility(TrinketTinkerEffect effect, AbilityData data) : base(effect, data)
        {
            Valid = false;
            if (d.Args.TryGetValue("BuffId", out string? argsBuffId))
            {
                buffId = argsBuffId;
                Valid = true;
            }
        }

        protected override bool ApplyOnFarmer(Farmer farmer)
        {
            ModEntry.Log($"{Name}.ApplyOnFarmer: buffId={buffId}");
            farmer.applyBuff(buffId);
            return base.ApplyOnFarmer(farmer);
        }
    }
}
