using StardewValley;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;

namespace TrinketTinker.Effects.Abilities
{
    /// <summary>Applies a buff on proc.</summary>
    public class BuffAbility(TrinketTinkerEffect effect, AbilityData data, int lvl) : Ability<BuffArgs>(effect, data, lvl)
    {
        /// <summary>Apply or refreshes the buff.</summary>
        /// <param name="farmer"></param>
        /// <returns></returns>
        protected override bool ApplyEffect(Farmer farmer)
        {
            // Buff(string id, string source = null, string displaySource = null, int duration = -1, Texture2D iconTexture = null, int iconSheetIndex = -1, BuffEffects effects = null, bool? isDebuff = null, string displayName = null, string description = null)
            farmer.applyBuff(args.BuffId);
            return base.ApplyEffect(farmer);
        }

        /// <summary>Removes the buff.</summary>
        /// <param name="farmer"></param>
        /// <returns></returns>
        protected override void UnProc(Farmer farmer)
        {
            farmer.buffs.Remove(args.BuffId);
        }

    }
}
