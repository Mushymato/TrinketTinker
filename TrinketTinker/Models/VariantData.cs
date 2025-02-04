using Microsoft.Xna.Framework;
using StardewValley;
using TrinketTinker.Wheels;

namespace TrinketTinker.Models;

/// <summary>Data for light source on the companion.</summary>
public class LightSourceData
{
    /// <summary>Light radius</summary>
    public float Radius = 2f;

    /// <summary>Use a vanilla texture</summary>
    public int Index = 1;

    /// <summary>Optional, use a custom light texture.</summary>
    public string? Texture { get; set; } = null;

    /// <summary>Light source color</summary>
    public string? Color { get; set; } = null;
}

public interface IVariantData
{
    /// <summary>Variant texture content path.</summary>
    public string Texture { get; set; }

    /// <summary>Variant portrait content path, for dialogue ability.</summary>
    public string? Portrait { get; set; }

    /// <summary>Extra textures only usable by <seealso cref="AnimClipData"/>.</summary>
    public List<string>? AnimClipTextures { get; set; }

    /// <summary>Draw color mask, can use color name from <see cref="Color"/>, hex value, or <see cref="TinkerConst.COLOR_PRISMATIC"/> for animated prismatic effect.</summary>
    public string? ColorMask { get; set; }

    /// <summary>Sprite width</summary>
    public int Width { get; set; }

    /// <summary>Sprite height</summary>
    public int Height { get; set; }

    /// <summary>Base scale to draw texture at.</summary>
    public float TextureScale { get; set; }

    /// <summary>Base scale to draw shadow texture.</summary>
    public float ShadowScale { get; set; }
}

/// <summary>Additional variant data, kind of like NPC appearance</summary>
public class SubVariantData : IVariantData
{
    /// <inheritdoc/>
    public string Texture { get; set; } = "";

    /// <inheritdoc/>
    public string? Portrait { get; set; } = null;

    /// <inheritdoc/>
    public List<string>? AnimClipTextures { get; set; } = null;

    /// <inheritdoc/>
    public string? ColorMask { get; set; } = null;

    /// <inheritdoc/>
    public int Width { get; set; } = 16;

    /// <inheritdoc/>
    public int Height { get; set; } = 16;

    /// <inheritdoc/>
    public float TextureScale { get; set; } = 4f;

    /// <inheritdoc/>
    public float ShadowScale { get; set; } = 3f;

    /// <summary>Game state query condition</summary>
    public string? Condition { get; set; } = null;

    /// <summary>Exclude</summary>
    public bool ProcOnly { get; set; } = false;

    /// <summary>Priority </summary>
    public int Priority { get; set; } = 0;
}

/// <summary>Data for <see cref="Companions.Anim.TinkerAnimSprite"/>, holds sprite variations.</summary>
public sealed class VariantData : IVariantData
{
    /// <inheritdoc/>
    public string Texture { get; set; } = "";

    /// <inheritdoc/>
    public string? Portrait { get; set; } = null;

    /// <inheritdoc/>
    public List<string>? AnimClipTextures { get; set; } = null;

    /// <inheritdoc/>
    public string? ColorMask { get; set; } = null;

    /// <inheritdoc/>
    public int Width { get; set; } = 16;

    /// <inheritdoc/>
    public int Height { get; set; } = 16;

    /// <inheritdoc/>
    public float TextureScale { get; set; } = 4f;

    /// <inheritdoc/>
    public float ShadowScale { get; set; } = 3f;

    /// <summary>If set, add a light with given radius. Note that the light is only visible to local player.</summary>
    public LightSourceData? LightSource { get; set; } = null;

    /// <summary>Sprite index of the item icon.</summary>
    public int TrinketSpriteIndex { get; set; } = -1;

    /// <summary>Display name override</summary>
    public List<string>? TrinketNameArguments { get; set; } = null;

    /// <summary>Subvariants dict</summary>
    public Dictionary<string, SubVariantData>? SubVariants { get; set; } = null;

    /// <summary>Recheck subvariant by conditions</summary>
    /// <param name="farmer"></param>
    /// <param name="prevSubVariantKey"></param>
    /// <param name="nextSubVariantKey"></param>
    /// <returns>True of subvariant is different</returns>
    internal bool TryRecheckSubVariant(Farmer farmer, string? prevSubVariantKey, out string? nextSubVariantKey)
    {
        nextSubVariantKey = null;
        if (
            SubVariants
                ?.OrderByDescending((kv) => kv.Value.Priority)
                .First((kv) => !kv.Value.ProcOnly && GameStateQuery.CheckConditions(kv.Value.Condition, player: farmer))
                is KeyValuePair<string, SubVariantData> foundSubVariant
            && foundSubVariant.Key != prevSubVariantKey
        )
        {
            nextSubVariantKey = foundSubVariant.Key;
            return true;
        }
        else if (prevSubVariantKey != null)
        {
            nextSubVariantKey = null;
            return true;
        }
        return false;
    }
}
