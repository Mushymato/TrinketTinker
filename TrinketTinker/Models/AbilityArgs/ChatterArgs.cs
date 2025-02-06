using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TokenizableStrings;
using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Models.AbilityArgs;

public record ChatterSpeaker(string? Name, string? PortraitPath)
{
    internal string? DisplayName => TokenParser.ParseText(Name) ?? "???";
    internal Lazy<Texture2D?> Portrait =
        new(() =>
        {
            if (!Game1.content.DoesAssetExist<Texture2D>(PortraitPath))
            {
                ModEntry.LogOnce($"Can't load custom portrait '{PortraitPath}', it does not exist.", LogLevel.Warn);
                return null;
            }
            return Game1.content.Load<Texture2D>(PortraitPath);
        });
}

/// <summary>Buff arguments</summary>
public sealed class ChatterArgs : IArgs
{
    /// <summary>String chatter prefix used to filter</summary>
    public string? ChatterPrefix { get; set; } = null;

    /// <inheritdoc/>
    public bool Validate() => true;
}
