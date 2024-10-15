using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TrinketTinker.Models;
using TrinketTinker.Models.MotionArgs;

namespace TrinketTinker.Companions.Motions
{
    public sealed class RelativeMotion(TrinketTinkerCompanion companion, MotionData mdata, VariantData vdata)
        : BaseStaticMotion<StaticArgs>(companion, mdata, vdata)
    {
        /// <inheritdoc/>
        public override void Draw(SpriteBatch b)
        {
            DrawWithShadow(
                b, c.Owner.FacingDirection == 0 ? 1f : (c.Position.Y / 10000f),
                vd.VecTextureScale,
                vd.VecShadowScale
            );
        }

        /// <summary>Update companion facing direction using player facing direction, adjust the offset to give illusions of perspective.</summary>
        protected override void UpdateDirection()
        {
            base.UpdateDirection();
            switch (c.Owner.FacingDirection)
            {
                case 0: // up
                    motionOffset = new(0, -md.Offset.Y / 2);
                    break;
                case 1: // right
                    motionOffset = new(md.Offset.X, md.Offset.Y);
                    break;
                case 2: // down
                    motionOffset = new(0, md.Offset.Y * 1.5f);
                    break;
                case 3: // left
                    motionOffset = new(-md.Offset.X, md.Offset.Y);
                    break;
            }
        }
    }
}