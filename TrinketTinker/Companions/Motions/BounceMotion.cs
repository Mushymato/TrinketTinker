using Microsoft.Xna.Framework;
using StardewValley;
using TrinketTinker.Models;
using TrinketTinker.Models.MotionArgs;

namespace TrinketTinker.Companions.Motions
{
    /// <summary>Companion follows the player and bobs up and down</summary>
    public sealed class BounceMotion(TrinketTinkerCompanion companion, MotionData mdata, VariantData vdata) : BaseLerpMotion<BounceArgs>(companion, mdata, vdata)
    {
        /// <summary>trig function input</summary>
        private double theta = 0f;

        /// <inheritdoc/>
        public override void UpdateGlobal(GameTime time, GameLocation location)
        {
            if (theta == 0f && !c.Moving && !md.AlwaysMoving)
                return;
            theta += time.ElapsedGameTime.TotalMilliseconds / (md.Interval * md.AnimationFrameLength);
            if (theta >= 1f)
                theta = 0f;
            base.UpdateGlobal(time, location);
        }

        /// <inheritdoc/>
        internal override Vector2 GetOffset()
        {
            return new Vector2(0, -args.MaxHeight * (float)Math.Sin(Math.PI * theta)) + base.GetOffset();
        }

        /// <inheritdoc/>
        protected override Vector2 GetTextureScale()
        {
            if (args.Squash)
            {
                float thetaF = (float)Math.Max(Math.Pow(Math.Cos(2 * Math.PI * theta), 5) / 2, 0);
                return new(
                    vd.TextureScale + thetaF,
                    vd.TextureScale - thetaF
                );
            }
            return base.GetTextureScale();
        }

        /// <inheritdoc/>
        protected override Vector2 GetShadowScale()
        {
            return Utility.Lerp(1f, 0.8f, Math.Max(1f, -GetOffset().Y / 6)) * base.GetShadowScale();
        }
    }
}