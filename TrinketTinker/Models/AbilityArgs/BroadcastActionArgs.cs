using StardewValley.Extensions;

namespace TrinketTinker.Models.AbilityArgs;

/// <summary>Args to broadcast an action, less things are checked with this</summary>
public sealed class BroadcastActionArgs : ActionArgs
{
    /// <summary>An additional condition to check, on the recipient</summary>
    public string? Condition = null;

    /// <summary>Player key</summary>
    public string PlayerKey = "All";

    public new bool Validate() =>
        AllActions.Any() && (PlayerKey.EqualsIgnoreCase("All") || PlayerKey.EqualsIgnoreCase("Host"));
}
