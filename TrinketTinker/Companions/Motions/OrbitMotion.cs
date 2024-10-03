using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using TrinketTinker.Models;
using TrinketTinker.Models.MotionArgs;

namespace TrinketTinker.Companions.Motions
{
    /// <summary>Companion orbits around a point</summary>
    public class OrbitMotion : StaticMotion<OrbitArgs>
    {
        /// <summary>trig function input</summary>
        private double theta = 0f;
        /// <summary>trig function input</summary>
        protected readonly float radiusX = 96f;
        /// <summary>trig function input</summary>
        protected readonly float radiusY = 40f;

        public OrbitMotion(TrinketTinkerCompanion companion, MotionData data) : base(companion, data)
        {
            motionOffset.Y -= 64f;
            c.Offset = motionOffset;
            radiusX = args?.RadiusX ?? radiusX;
            radiusY = args?.RadiusY ?? radiusY;
        }

        public override void UpdateLocal(GameTime time, GameLocation location)
        {
            base.UpdateLocal(time, location);
            theta += time.ElapsedGameTime.TotalSeconds;
            c.NetOffset.X = motionOffset.X + radiusX * (float)Math.Cos(Math.PI * theta);
            c.NetOffset.Y = motionOffset.Y + radiusY * (float)Math.Sin(Math.PI * theta);
            if (theta >= 2f)
                theta = 0f;
        }
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