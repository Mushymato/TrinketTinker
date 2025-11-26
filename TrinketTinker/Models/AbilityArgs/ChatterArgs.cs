using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TokenizableStrings;
using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Models.AbilityArgs;

public record ChatterSpeaker(string? Portrait, string? Name, string? NPC)
{
    internal string? DisplayName => TokenParser.ParseText(Name);
    internal Lazy<Texture2D?> PortraitTx2D = new(() =>
    {
        if (string.IsNullOrEmpty(Portrait))
            return null;
        if (!Game1.content.DoesAssetExist<Texture2D>(Portrait))
        {
            ModEntry.LogOnce($"Can't load custom portrait '{Portrait}', it does not exist.", LogLevel.Warn);
            return null;
        }
        return Game1.content.Load<Texture2D>(Portrait);
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
