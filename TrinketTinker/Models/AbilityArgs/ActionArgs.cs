using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Models.AbilityArgs;

/// <summary>(trigger) action arguments</summary>
public sealed class ActionArgs : IArgs
{
    /// <summary>String action for TriggerAction</summary>
    public string? Action = null;

    /// <summary>List of actions for TriggerAction</summary>
    public List<string>? Actions = null;

    /// <summary>List of actions for TriggerAction</summary>
    public List<string>? ActionEnd = null;

    /// <inheritdoc/>
    public bool Validate()
    {
        return Action != null;
    }
}
