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

public enum HatSourceMode
{
    Hatless = 0,
    Given = 1,
    Owner = 2,
}

/// <summary>Data for defining the position of the companion's head, for hat purposes.</summary>
public class HatPositionData
{
    /// <summary>The default hat offset.</summary>
    public Vector2? OffsetDefault { get; set; } = null;

    /// <summary>Offset on hat position for particular frames on the base sprite sheet.</summary>
    public Dictionary<int, Vector2?>? OffsetOnFrame { get; set; } = null;

    /// <summary>Offset on hat position for particular frames on the extra sprite sheet.</summary>
    public Dictionary<int, Vector2?>? OffsetOnFrameExtra { get; set; } = null;

    /// <summary>Offset on hat position for particular companion direction.</summary>
    public Dictionary<int, Vector2?>? OffsetOnDirection { get; set; } = null;

    /// <summary>Set which hat frame (0 1 2 3) should match the trinket companion direction.</summary>
    public Dictionary<int, int>? DirectionToHatFrame { get; set; } = null;

    /// <summary>Modifies the hat's draw scale</summary>
    public float ModifyScale { get; set; } = 1f;

    /// <summary>Where does the hat come from?</summary>
    public HatSourceMode Source { get; set; } = HatSourceMode.Given;
}

public interface IVariantData
{
    /// <summary>Variant texture content path.</summary>
    public string? Texture { get; set; }

    /// <summary>Which section of <see cref="Texture"/> to use, defaults to entire texture</summary>
    public Rectangle TextureSourceRect { get; set; }

    /// <summary>Additional textures used in anim clips only, this should generally have the same layout as <see cref="Texture"/>.</summary>
    public string? TextureExtra { get; set; }

    /// <summary>Which section of <see cref="TextureExtra"/> to use, defaults to entire texture</summary>
    public Rectangle TextureExtraSourceRect { get; set; }

    /// <summary>Draw color mask, can use color name from <see cref="Color"/>, hex value, or <see cref="TinkerConst.COLOR_PRISMATIC"/> for animated prismatic effect.</summary>
    public string? ColorMask { get; set; }

    /// <summary>Sprite width</summary>
    public int Width { get; set; }

    /// <summary>Sprite height</summary>
    public int Height { get; set; }

    /// <summary>Adjusts the bounding box</summary>
    public Rectangle Bounding { get; set; }

    /// <summary>Base scale to draw texture at.</summary>
    public float TextureScale { get; set; }

    /// <summary>Base scale to draw shadow texture.</summary>
    public float ShadowScale { get; set; }

    /// <summary>Variant speaker NPC, for chatter ability. Required for Portraiture compatibility, can omit <see cref="Name"/> if set. </summary>
    public string? NPC { get; set; }

    /// <summary>Variant speaker name, for chatter ability.</summary>
    public string? Name { get; set; }

    /// <summary>Variant portrait content path, for chatter ability.</summary>
    public string? Portrait { get; set; }

    /// <summary>Show NPC breathing, only usable if NPC is a real NPC with standard 16x32 or smaller sprite.</summary>
    public bool? ShowBreathing { get; set; }

    /// <summary>Hat position data, for giving the companion hats.</summary>
    public HatPositionData? HatPosition { get; set; }
}

/// <summary>Additional variant data, kind of like NPC appearance</summary>
public class AltVariantData : IVariantData
{
    /// <inheritdoc/>
    public string? Texture { get; set; } = null;

    /// <inheritdoc/>
    public Rectangle TextureSourceRect { get; set; } = Rectangle.Empty;

    /// <inheritdoc/>
    public string? TextureExtra { get; set; } = null;

    /// <inheritdoc/>
    public Rectangle TextureExtraSourceRect { get; set; } = Rectangle.Empty;

    /// <inheritdoc/>
    public string? ColorMask { get; set; } = null;

    /// <inheritdoc/>
    public int Width { get; set; } = -1;

    /// <inheritdoc/>
    public int Height { get; set; } = -1;

    /// <inheritdoc/>
    public Rectangle Bounding { get; set; } = Rectangle.Empty;

    /// <inheritdoc/>
    public float TextureScale { get; set; } = -1;

    /// <inheritdoc/>
    public float ShadowScale { get; set; } = -1;

    /// <inheritdoc/>
    public string? NPC { get; set; } = null;

    /// <inheritdoc/>
    public string? Name { get; set; } = null;

    /// <inheritdoc/>
    public string? Portrait { get; set; } = null;

    /// <inheritdoc/>
    public bool? ShowBreathing { get; set; } = null;

    /// <inheritdoc/>
    public HatPositionData? HatPosition { get; set; } = null;

    /// <summary>Game state query condition</summary>
    public string? Condition { get; set; } = null;

    /// <summary>Priority of this alt variant, higher </summary>
    public int Priority { get; set; } = 0;
}

/// <summary>Data for <see cref="Companions.Anim.TinkerAnimSprite"/>, holds sprite variations.</summary>
public sealed class VariantData : IVariantData
{
    /// <inheritdoc/>
    public string? Texture { get; set; } = null;

    /// <inheritdoc/>
    public Rectangle TextureSourceRect { get; set; } = Rectangle.Empty;

    /// <inheritdoc/>
    public string? TextureExtra { get; set; } = null;

    /// <inheritdoc/>
    public Rectangle TextureExtraSourceRect { get; set; } = Rectangle.Empty;

    /// <inheritdoc/>
    public string? ColorMask { get; set; } = null;

    /// <inheritdoc/>
    public int Width { get; set; } = 16;

    /// <inheritdoc/>
    public int Height { get; set; } = 16;

    /// <inheritdoc/>
    public Rectangle Bounding { get; set; } = Rectangle.Empty;

    /// <inheritdoc/>
    public float TextureScale { get; set; } = 4f;

    /// <inheritdoc/>
    public float ShadowScale { get; set; } = 3f;

    /// <inheritdoc/>
    public string? NPC { get; set; } = null;

    /// <inheritdoc/>
    public string? Name { get; set; } = null;

    /// <inheritdoc/>
    public string? Portrait { get; set; } = null;

    /// <inheritdoc/>
    public bool? ShowBreathing { get; set; } = true;

    /// <inheritdoc/>
    public HatPositionData? HatPosition { get; set; } = null;

    /// <summary>If set, add a light with given radius. Note that the light is only visible to local player.</summary>
    public LightSourceData? LightSource { get; set; } = null;

    /// <summary>Sprite index of the item icon.</summary>
    public int TrinketSpriteIndex { get; set; } = -1;

    /// <summary>Display name override</summary>
    public List<string>? TrinketNameArguments { get; set; } = null;

    /// <summary>Temporary animated sprite to attach to the companion, these will follow them around. DOES NOT SYNC IN MULTIPLAYER.</summary>
    public List<string>? AttachedTAS { get; set; } = null;

    /// <summary>Alternate variants dict</summary>
    public Dictionary<string, AltVariantData>? AltVariants { get; set; } = null;

    /// <summary>Recheck alt variant by conditions</summary>
    /// <param name="farmer"></param>
    /// <param name="prevKey"></param>
    /// <param name="nextKey"></param>
    /// <returns>True if sub variant is different</returns>
    internal bool TryRecheckAltVariant(
        Farmer farmer,
        string? prevKey,
        StardewValley.Objects.Trinkets.Trinket? trinketItem,
        out string? nextKey
    )
    {
        nextKey = null;
        if (
            AltVariants
                ?.OrderByDescending((kv) => kv.Value.Priority)
                .FirstOrDefault(
                    (kv) =>
                        GameStateQuery.CheckConditions(
                            kv.Value.Condition,
                            player: farmer,
                            targetItem: trinketItem,
                            inputItem: trinketItem
                        )
                )
                is KeyValuePair<string, AltVariantData> foundAltVariant
            && !string.IsNullOrEmpty(foundAltVariant.Key)
        )
        {
            nextKey = foundAltVariant.Key;
            return true;
        }
        else if (prevKey != null)
        {
            nextKey = null;
            return true;
        }
        return false;
    }
}
