using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Models.AbilityArgs;

/// <summary>(trigger) action arguments</summary>
public sealed class ActionArgs : IArgs
{
    /// <summary>String action for TriggerAction</summary>
    public string? Action = null;
    /// <inheritdoc/>
    public bool Validate()
    {
        return Action != null;
    }
}

