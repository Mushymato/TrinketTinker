using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Models.AbilityArgs;

/// <summary>(trigger) action arguments</summary>
public class ActionArgs : IArgs
{
    /// <summary>String action for TriggerAction</summary>
    public string? Action { private get; set; } = null;

    /// <summary>List of actions for TriggerAction</summary>
    public List<string> Actions { private get; set; } = [];

    internal IEnumerable<string> AllActions => Action != null ? [Action, .. Actions] : Actions;

    /// <summary>String action for TriggerAction, runs at the end</summary>
    public string? ActionEnd { private get; set; } = null;

    /// <summary>List of actions for TriggerAction, fires at the end</summary>
    public List<string> ActionsEnd { private get; set; } = [];

    internal IEnumerable<string> AllActionsEnd => ActionEnd != null ? [ActionEnd, .. ActionsEnd] : ActionsEnd;

    /// <inheritdoc/>
    public bool Validate() => AllActions.Any();
}
