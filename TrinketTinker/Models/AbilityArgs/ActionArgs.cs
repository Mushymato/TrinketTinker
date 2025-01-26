using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Models.AbilityArgs;

/// <summary>(trigger) action arguments</summary>
public sealed class ActionArgs : IArgs
{
    /// <summary>String action for TriggerAction</summary>
    public string? Action
    {
        get => Actions.FirstOrDefault();
        set
        {
            if (value != null)
            {
                Actions.Insert(0, value);
            }
        }
    }

    /// <summary>List of actions for TriggerAction</summary>
    public List<string> Actions { get; set; } = [];

    /// <summary>String action for TriggerAction, runs at the end</summary>
    public string? ActionEnd
    {
        get => ActionsEnd.FirstOrDefault();
        set
        {
            if (value != null)
            {
                ActionsEnd.Insert(0, value);
            }
        }
    }

    /// <summary>List of actions for TriggerAction, fires at the end</summary>
    public List<string> ActionsEnd { get; set; } = [];

    /// <inheritdoc/>
    public bool Validate()
    {
        return Actions.Any();
    }
}
