using StardewValley;
using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Models.AbilityArgs
{
    /// <summary>Buff arguments</summary>
    public class BuffArgs : IArgs
    {
        public string BuffId { get; set; } = null!;
        /// <inheritdoc/>
        public bool Validate() => BuffId != null && DataLoader.Buffs(Game1.content).ContainsKey(BuffId);
    }
}