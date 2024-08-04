using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using TrinketTinker.Model;

namespace TrinketTinker.Companions.Motions
{
    public class HoverMotion : LerpMotion
    {
        private double theta = 0f;

        public HoverMotion(TrinketTinkerCompanion companion, MotionData data) : base(companion, data)
        {
        }

        public override void UpdateLocal(GameTime time, GameLocation location)
        {
            base.UpdateLocal(time, location);
            theta += time.ElapsedGameTime.TotalMilliseconds / (d.Interval * d.AnimationFrameLength);
            c.Offset = MotionOffset + new Vector2(0, 16 * (float)Math.Sin(Math.PI * theta));
            if (theta >= 1f)
                theta = 0f;
        }

        public override void Draw(SpriteBatch b)
        {
            float layerDepth = c.Position.Y / 10000f;
            b.Draw(
                c.Sprite.Texture,
                Game1.GlobalToLocal(c.Position + c.Offset + c.Owner.drawOffset),
                c.Sprite.SourceRect,
                Color.White, 0f, new Vector2(8f, 8f), 4f,
                (c.direction.Value < 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                layerDepth
            );
            b.Draw(
                Game1.shadowTexture,
                Game1.GlobalToLocal(c.Position + c.Owner.drawOffset),
                Game1.shadowTexture.Bounds,
                Color.White, 0f,
                new Vector2(
                    Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y
                ),
                3f * Utility.Lerp(1f, 0.8f, Math.Max(1f, -c.Offset.Y / 12)),
                SpriteEffects.None,
                layerDepth - 2E-06f
            );
        }
    }
}