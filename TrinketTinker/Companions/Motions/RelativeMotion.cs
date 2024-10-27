using Microsoft.Xna.Framework;
using TrinketTinker.Models;
using TrinketTinker.Models.MotionArgs;

namespace TrinketTinker.Companions.Motions
{
    /// <summary>Companion's offset is adjusted depending on player facing direction</summary>
    /// <inheritdoc/>
    public sealed class RelativeMotion(TrinketTinkerCompanion companion, MotionData mdata, VariantData vdata)
        : BaseStaticMotion<StaticArgs>(companion, mdata, vdata)
    {
        /// <inheritdoc/>
        protected override float GetPositionalLayerDepth(Vector2 offset)
        {
            // return (c.Owner.FacingDirection == 0) ? (c.Position.Y / 10000f) : 1f;
            return c.Owner.FacingDirection switch
            {
                // up
                0 => 1f,
                // down
                1 => 0f,
                // right & left
                _ => c.Position.Y / 10000f
            };
        }

        /// <inheritdoc/>
        public override Vector2 GetOffset()
        {
            return c.Owner.FacingDirection switch
            {
                // up
                0 => new(0, -md.Offset.Y / 2),
                // right
                1 => new(md.Offset.X, md.Offset.Y),
                // down
                2 => new(0, md.Offset.Y * 1.5f),
                // left
                3 => new(-md.Offset.X, md.Offset.Y),
                _ => md.Offset,
            };
        }
    }
}