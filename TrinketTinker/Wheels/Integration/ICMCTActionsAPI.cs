using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace TrinketTinker.Wheels.Integration;

public partial interface ICrossModCompatibilityToolsAPI
{
    /// <summary>
    /// Register an Action with Cross-Mod Compatibility Tools.
    /// </summary>
    /// <param name="manifest">The manifest of the mod you want to register the Action for.</param>
    /// <param name="actionId">The ID you want to give to the Action you are registering.</param>
    /// <param name="category">Optional. A user-defined category that this action belongs to. Arbitrary and up to you, but if you expect a certain mod to use your action, you may want to check if they expect any particular category. Defaults to <c>Default</c></param>
    /// <param name="name">Optional. A user-facing name for this action. Defaults to the qualified name of the method.</param>
    /// <param name="description">Optional. A user-facing description explaining what this action is meant to do or how it is meant to be used. Defaults to <c>(No description provided.)</c></param>
    /// <param name="action">The Action you want to register.</param>
    /// <param name="customFields">Optional. A dictionary of custom fields you want to attach to the Action.</param>
    /// <param name="customData">Optional. A class object of custom data you want to attach to the Action. You should explain your class structure in your documentation if you want others to use this custom data.</param>
    void RegisterAction(
        IManifest manifest,
        string actionId,
        string? category,
        Func<string>? name,
        Func<string>? description,
        Action action,
        Dictionary<string, string>? customFields,
        object? customData
    );
}
