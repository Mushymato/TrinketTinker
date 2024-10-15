using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using TrinketTinker.Models;
using TrinketTinker.Models.MotionArgs;

namespace TrinketTinker.Companions.Motions
{
    /// <summary>Companion orbits around a point</summary>
    public class OrbitMotion : BaseStaticMotion<OrbitArgs>
    {
        /// <summary>trig function input</summary>
        private double theta = 0f;

        /// <inheritdoc/>
        public OrbitMotion(TrinketTinkerCompanion companion, MotionData data) : base(companion, data)
        {
            motionOffset.Y -= 64f;
            c.Offset = motionOffset;
        }

        /// <summary>
        /// Calculates circular motion using cos for x and sin for y
        /// </summary>
        /// <param name="time"></param>
        /// <param name="location"></param>
        public override void UpdateLocal(GameTime time, GameLocation location)
        {
            base.UpdateLocal(time, location);
            theta += time.ElapsedGameTime.TotalSeconds;
            c.NetOffset.X = motionOffset.X + args.RadiusX * (float)Math.Cos(Math.PI * theta);
            c.NetOffset.Y = motionOffset.Y + args.RadiusY * (float)Math.Sin(Math.PI * theta);
            if (theta >= 2f)
                theta = 0f;
        }

        /// <inheritdoc/>
        public override void Draw(SpriteBatch b)
        {
            DrawWithShadow(
                b, (c.Position.Y + c.Offset.Y - motionOffset.Y) / 10000f,
                new Vector2(d.TextureScale, d.TextureScale) * Utility.Lerp(0.96f, 1f, (float)Math.Sin(Math.PI * theta)),
                new Vector2(d.ShadowScale, d.ShadowScale)
            );
        }
    }
}