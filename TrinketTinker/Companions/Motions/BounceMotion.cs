using Microsoft.Xna.Framework;
using StardewValley;
using TrinketTinker.Models;
using TrinketTinker.Models.MotionArgs;

namespace TrinketTinker.Companions.Motions
{
    /// <summary>Companion follows the player and bobs up and down</summary>
    public sealed class BounceMotion(TrinketTinkerCompanion companion, MotionData mdata, VariantData vdata) : BaseLerpMotion<BounceArgs>(companion, mdata, vdata)
    {
        private const string BOUNCE = "Bounce";
        /// <summary>trig function input</summary>
        private double theta = 0f;
        private double pauseTimer = 0f;

        /// <inheritdoc/>
        public override void UpdateGlobal(GameTime time, GameLocation location)
        {
            pauseTimer += time.ElapsedGameTime.TotalMilliseconds;
            if (theta != 0f || (GetMoving() && pauseTimer > args.Pause))
            {
                pauseTimer = 0f;
                theta += time.ElapsedGameTime.TotalMilliseconds / args.Period;
                if (theta >= 1f)
                    theta = 0f;
                c.OverrideKey = BOUNCE;
            }
            else
            {
                c.OverrideKey = null;
            }
            base.UpdateGlobal(time, location);
        }

        /// <inheritdoc/>
        public override void UpdateLocal(GameTime time, GameLocation location)
        {
            if (theta == 0f && args.Pause > 0)
                return;
            base.UpdateLocal(time, location);
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
                return new(
                    baseTxScale.X + thetaF,
                    baseTxScale.Y - thetaF
                );
            }
            return base.GetTextureScale();
        }

        /// <inheritdoc/>
        protected override Vector2 GetShadowScale()
        {
            return MathF.Max(0f, Utility.Lerp(1.0f, 0.8f, (float)Math.Sin(Math.PI * theta))) * base.GetShadowScale();
        }
    }
}