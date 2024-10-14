using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using TrinketTinker.Models;
using TrinketTinker.Models.MotionArgs;

namespace TrinketTinker.Companions.Motions
{
    /// <summary>Companion follows the player and bobs up and down</summary>
    public sealed class BounceMotion(TrinketTinkerCompanion companion, MotionData data) : BaseLerpMotion<BounceArgs>(companion, data)
    {
        /// <summary>trig function input</summary>
        private double theta = 0f;

        public override void UpdateLocal(GameTime time, GameLocation location)
        {
            base.UpdateLocal(time, location);
            if (theta == 0f && !c.Moving && !d.AlwaysMoving)
                return;
            theta += time.ElapsedGameTime.TotalMilliseconds / (d.Interval * d.AnimationFrameLength);
            c.Offset = motionOffset + new Vector2(0, -args.MaxHeight * (float)Math.Sin(Math.PI * theta));
            if (theta >= 1f)
                theta = 0f;
        }

        public override void Draw(SpriteBatch b)
        {
            float thetaF = args.Squash ? (float)Math.Max(Math.Pow(Math.Cos(2 * Math.PI * theta), 5) / 2, 0) : 0;
            Vector2 textureScale = new(
                d.TextureScale + thetaF,
                d.TextureScale - thetaF
            );
            // float shadowScale = d.ShadowScale * Utility.Lerp(1f, 0.8f, Math.Max(1f, -c.Offset.Y / 6));
            DrawWithShadow(
                b, c.Position.Y / 10000f,
                textureScale,
                new Vector2(d.ShadowScale, d.ShadowScale)
            );
        }
    }
}