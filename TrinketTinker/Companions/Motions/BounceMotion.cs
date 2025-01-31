using Microsoft.Xna.Framework;
using StardewValley;
using TrinketTinker.Models;
using TrinketTinker.Models.MotionArgs;

namespace TrinketTinker.Companions.Motions;

/// <summary>Companion follows the player and bobs up and down</summary>
public sealed class BounceMotion(TrinketTinkerCompanion companion, MotionData mdata, VariantData vdata)
    : BaseLerpMotion<BounceArgs>(companion, mdata, vdata)
{
    /// <summary>Jump anim clip key</summary>
    private const string JUMP = "Jump";

    /// <summary>trig function input</summary>
    private double theta = Random.Shared.NextDouble();

    /// <inheritdoc/>
    public override void UpdateGlobal(GameTime time, GameLocation location)
    {
        if (theta != 0f || IsMoving())
        {
            theta += time.ElapsedGameTime.TotalMilliseconds / args.Period;
            if (theta >= 1f)
                theta = 0f;
            c.OverrideKey = JUMP;
        }
        else
        {
            c.OverrideKey = null;
        }
        base.UpdateGlobal(time, location);
    }

    /// <inheritdoc/>
    public override Vector2 GetOffset()
    {
        Vector2 baseOffset = base.GetOffset();
        return new Vector2(baseOffset.X, -args.MaxHeight * (float)Math.Sin(Math.PI * theta));
    }

    /// <inheritdoc/>
    protected override Vector2 GetTextureScale()
    {
        if (args.Squash > 0f)
        {
            float thetaF = (float)Math.Max(Math.Pow(Math.Cos(2 * Math.PI * theta), 5) / 2, 0) * args.Squash;
            Vector2 baseTxScale = base.GetTextureScale();
            return new(baseTxScale.X + thetaF, baseTxScale.Y - thetaF);
        }
        return base.GetTextureScale();
    }

    /// <inheritdoc/>
    protected override Vector2 GetShadowScale()
    {
        return MathF.Max(0f, Utility.Lerp(1.0f, 0.8f, (float)Math.Sin(Math.PI * theta))) * base.GetShadowScale();
    }
}
