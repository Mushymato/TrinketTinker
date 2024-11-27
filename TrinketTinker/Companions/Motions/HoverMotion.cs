using Microsoft.Xna.Framework;
using StardewValley;
using TrinketTinker.Models;
using TrinketTinker.Models.MotionArgs;

namespace TrinketTinker.Companions.Motions;

/// <summary>Companion follows the player and bobs up and down</summary>
/// <inheritdoc/>
public sealed class HoverMotion(TrinketTinkerCompanion companion, MotionData data, VariantData vdata)
    : BaseLerpMotion<HoverArgs>(companion, data, vdata)
{
    /// <summary>Perching anim clip name.</summary>
    private const string PERCHING = "Perching";

    /// <summary>Default Y offset</summary>
    private const float DEFAULT_HEIGHT = 96f;

    /// <summary>trig function input</summary>
    private double theta = 0f;

    /// <summary>Timer until perch</summary>
    private double perchingTimer = 0;

    /// <summary>Track perching state for local. Global must check c.OverrideKey == PERCHING instead.</summary>
    private bool perching = false;

    public override void UpdateLocal(GameTime time, GameLocation location)
    {
        if (args.PerchingTimeout != null && currAnchorTarget == AnchorTarget.Owner)
        {
            if (perching)
            {
                if (c.OwnerMoving)
                {
                    perching = false;
                    perchingTimer = 0f;
                    Lerp = -1f;
                    c.OverrideKey = null;
                }
            }
            else
            {
                perchingTimer += time.ElapsedGameTime.TotalMilliseconds;
                if (perchingTimer > args.PerchingTimeout)
                {
                    perching = true;
                    c.startPosition = c.Position;
                    c.endPosition = c.OwnerPosition + args.PerchingOffset;
                    Lerp = 0f;
                }
            }
        }
        base.UpdateLocal(time, location);
        if (perching && Lerp < 0f)
        {
            c.OverrideKey = PERCHING;
        }
    }

    /// <inheritdoc/>
    public override void UpdateGlobal(GameTime time, GameLocation location)
    {
        if (c.OverrideKey == PERCHING)
        {
            theta = 0f;
        }
        else
        {
            theta += time.ElapsedGameTime.TotalMilliseconds / args.Period;
            if (theta >= 1f)
                theta = 0f;
        }
        base.UpdateGlobal(time, location);
    }

    /// <inheritdoc/>
    protected override float GetPositionalLayerDepth(Vector2 offset)
    {
        if (c.OverrideKey == PERCHING)
        {
            return c.Owner.getDrawLayer() + 0.2f;
        }
        return c.Position.Y / 10000f;
    }

    /// <inheritdoc/>
    public override Vector2 GetOffset()
    {
        return new Vector2(0, args.Magnitude * (float)Math.Sin(Math.PI * theta) - DEFAULT_HEIGHT) + base.GetOffset();
    }
}
