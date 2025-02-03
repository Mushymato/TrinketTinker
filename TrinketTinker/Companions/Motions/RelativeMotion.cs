using Microsoft.Xna.Framework;
using TrinketTinker.Models;
using TrinketTinker.Models.MotionArgs;

namespace TrinketTinker.Companions.Motions;

/// <summary>Companion's offset is adjusted depending on player facing direction</summary>
/// <inheritdoc/>
public sealed class RelativeMotion(TrinketTinkerCompanion companion, MotionData mdata, VariantData vdata)
    : BaseStaticMotion<RelativeArgs>(companion, mdata, vdata)
{
    /// <inheritdoc/>
    protected override float GetPositionalLayerDepth(Vector2 offset)
    {
        // // up
        // 0 => 1f,
        // // down
        // 1 => 0f,
        // // right & left
        // _ => c.Position.Y / 10000f,
        return (
                c.Owner.FacingDirection switch
                {
                    // up
                    0 => args.LayerU,
                    // right
                    1 => args.LayerR,
                    // down
                    2 => args.LayerD,
                    // left
                    3 => args.LayerL,
                    _ => null,
                } ?? 0
            ) + base.GetPositionalLayerDepth(offset);
    }

    /// <inheritdoc/>
    public override Vector2 GetOffset()
    {
        return (
                c.Owner.FacingDirection switch
                {
                    // up
                    0 => args.OffsetU,
                    // right
                    1 => args.OffsetR,
                    // down
                    2 => args.OffsetD,
                    // left
                    3 => args.OffsetL,
                    _ => null,
                } ?? Vector2.Zero
            ) + base.GetOffset();
    }
}
