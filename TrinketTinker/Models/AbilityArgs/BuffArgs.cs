using StardewValley;
using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Models.AbilityArgs;

/// <summary>Buff arguments</summary>
public sealed class BuffArgs : IArgs
{
    /// <summary>Buff Id, should match something in Data/Buffs</summary>
    public string BuffId { get; set; } = null!;
    /// <inheritdoc/>
    public bool Validate() => BuffId != null && DataLoader.Buffs(Game1.content).ContainsKey(BuffId);
}
