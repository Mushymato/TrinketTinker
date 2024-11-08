using Microsoft.Xna.Framework;
using StardewValley;
using TrinketTinker.Models;
using TrinketTinker.Models.MotionArgs;
using TrinketTinker.Wheels;

namespace TrinketTinker.Companions.Motions;

/// <summary>Companion follows the player and bobs up and down</summary>
public sealed class HopMotion(TrinketTinkerCompanion companion, MotionData mdata, VariantData vdata)
    : BaseLerpMotion<HopArgs>(companion, mdata, vdata)
{
    /// <summary>Jump anim clip key</summary>
    private const string JUMP = "Jump";
    float height = 0f;

    /// <inheritdoc/>
    public override void UpdateGlobal(GameTime time, GameLocation location)
    {
        if (Lerp > 0f)
        {
            height = Visuals.EaseOut(0f, args.MaxHeight, 2 * ((Lerp > 0.5f) ? 1f - Lerp : Lerp));
            c.OverrideKey = JUMP;
        }
        else
        {
            height = 0f;
            c.OverrideKey = null;
        }
        base.UpdateGlobal(time, location);
    }

    /// <inheritdoc/>
    public override Vector2 GetOffset()
    {
        Vector2 baseOffset = base.GetOffset();
        return new Vector2(baseOffset.X, baseOffset.Y - height);
    }

    /// <inheritdoc/>
    protected override Vector2 GetShadowScale()
    {
        return MathF.Max(0f, Visuals.EaseOut(1.0f, 0.8f, height / args.MaxHeight))
            * base.GetShadowScale();
    }
}
