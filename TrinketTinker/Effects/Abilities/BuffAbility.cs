using StardewValley;
using TrinketTinker.Models;

namespace TrinketTinker.Effects.Abilities
{
    /// <summary>Applies a buff on proc.</summary>
    public class BuffAbility : Ability
    {
        /// <summary>Valid buff ID, must be found in Data/Buffs</summary>
        protected readonly string buffId = "";
        public BuffAbility(TrinketTinkerEffect effect, AbilityData data, int lvl) : base(effect, data, lvl)
        {
            if (d.TryGetParsed("BuffId", out string? argsBuffId) &&
                DataLoader.Buffs(Game1.content).ContainsKey(argsBuffId))
            {
                buffId = argsBuffId;
                Valid = true;
            }
        }

        /// <summary>Apply or refreshes the buff.</summary>
        /// <param name="farmer"></param>
        /// <returns></returns>
        protected override bool ApplyEffect(Farmer farmer)
        {
            farmer.applyBuff(buffId);
            return base.ApplyEffect(farmer);
        }

        /// <summary>Removes the buff.</summary>
        /// <param name="farmer"></param>
        /// <returns></returns>
        protected override void UnProc(Farmer farmer)
        {
            farmer.buffs.Remove(buffId);
        }

    }
}
