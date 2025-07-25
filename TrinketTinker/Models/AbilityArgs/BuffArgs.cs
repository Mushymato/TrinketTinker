using StardewValley;
using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Models.AbilityArgs;

public enum BuffApplyMode
{
    All,
    Random,
}

/// <summary>Buff arguments</summary>
public sealed class BuffArgs : IArgs
{
    /// <summary>Buff Id, should match something in Data/Buffs</summary>
    public string BuffId { get; set; } = null!;
    public BuffApplyMode Mode { get; set; } = BuffApplyMode.All;
    internal List<string>? BuffIdList = null;

    /// <inheritdoc/>
    public bool Validate()
    {
        if (BuffId == null)
            return false;
        if (DataLoader.Buffs(Game1.content).ContainsKey(BuffId))
            return true;
        BuffIdList = [];
        foreach (string buffId in BuffId.Split(","))
        {
            if (DataLoader.Buffs(Game1.content).ContainsKey(buffId))
            {
                BuffIdList.Add(buffId);
            }
        }
        if (BuffIdList.Count == 0)
        {
            BuffIdList = null;
            return false;
        }
        return true;
    }
}
