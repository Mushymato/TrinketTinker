using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using TrinketTinker.Models;

namespace TrinketTinker.Companions.Motions
{
    public class OrbitMotion : StaticMotion
    {
        private double theta = 0f;

        public OrbitMotion(TrinketTinkerCompanion companion, MotionData data) : base(companion, data)
        {
            MotionOffset.Y -= 64f;
        }

        public override void UpdateLocal(GameTime time, GameLocation location)
        {
            base.UpdateLocal(time, location);
            theta += time.ElapsedGameTime.TotalSeconds;
            c.Offset = MotionOffset + new Vector2(
                96 * (float)Math.Cos(Math.PI * theta),
                40 * (float)Math.Sin(Math.PI * theta)
            );
            if (theta >= 2f)
                theta = 0f;
        }
        public override void Draw(SpriteBatch b)
        {
            DrawWithShadow(
                b, (c.Position.Y + c.Offset.Y - MotionOffset.Y) / 10000f,
                new Vector2(d.TextureScale, d.TextureScale) * Utility.Lerp(0.96f, 1f, (float)Math.Sin(Math.PI * theta)),
                new Vector2(d.ShadowScale, d.ShadowScale)
            );
        }
    }
}