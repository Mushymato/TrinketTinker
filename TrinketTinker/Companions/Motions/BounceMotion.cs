using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using TrinketTinker.Models;

namespace TrinketTinker.Companions.Motions
{
    /// <summary>Companion follows the player and bobs up and down</summary>
    public class BounceMotion : LerpMotion
    {
        /// <summary>trig function input</summary>
        private double theta = 0f;

        protected readonly float maxHeight = 128f;

        public BounceMotion(TrinketTinkerCompanion companion, MotionData data) : base(companion, data)
        {
            maxHeight = d.GetParsedOrDefault("maxHeight", maxHeight);
            // motionOffset.Y -= maxHeight;
            // c.Offset = motionOffset;
        }

        public override void UpdateLocal(GameTime time, GameLocation location)
        {
            base.UpdateLocal(time, location);
            theta += time.ElapsedGameTime.TotalMilliseconds / (d.Interval * d.AnimationFrameLength);
            c.Offset = motionOffset + new Vector2(0, -maxHeight * (float)Math.Sin(Math.PI * theta));
            if (theta >= 1f)
                theta = 0f;
        }

        public override void Draw(SpriteBatch b)
        {
            float thetaF = (float)(theta / 2.0);
            Vector2 textureScale = new(
                d.TextureScale - thetaF,
                d.TextureScale + thetaF
            );
            float shadowScale = d.ShadowScale * Utility.Lerp(1f, 0.8f, Math.Max(1f, -c.Offset.Y / 12));
            DrawWithShadow(
                b, c.Position.Y / 10000f,
                textureScale,
                new Vector2(shadowScale, shadowScale)
            );
        }
    }
}