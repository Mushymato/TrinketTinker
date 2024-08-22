using StardewValley;
using StardewValley.Delegates;
using StardewValley.Objects.Trinkets;
using TrinketTinker.Models;

namespace TrinketTinker.Effects
{
    public static class ProcTrinket
    {
        public static readonly string TriggerActionName = $"{ModEntry.ModId}_ProcTrinket";
        /// <summary>Trigger action, proc trinkets that use <see cref="ProcOn.Trigger"/>.</summary>
        public static bool Action(string[] args, TriggerActionContext context, out string error)
        {
            if (!ArgUtility.TryGetOptional(args, 1, out string trinketId, out error))
                return false;

            foreach (Trinket trinketItem in Game1.player.trinketItems)
            {
                if ((trinketId == null || trinketItem.ItemId == trinketId) &&
                     trinketItem?.GetEffect() is TrinketTinkerEffect effect)
                    effect.OnTrigger(Game1.player);
            }
            return true;
        }
    }
}
