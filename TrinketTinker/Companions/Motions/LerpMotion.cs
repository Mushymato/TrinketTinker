using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using TrinketTinker.Model;

namespace TrinketTinker.Companions.Motions
{
    public class LerpMotion : Motion
    {
        private float lerp = -1f;
        public LerpMotion(TrinketTinkerCompanion companion, MotionData data) : base(companion, data) { }
        public override void UpdateLocal(GameTime time, GameLocation location)
        {
            // Copied from Companion.Update's IsLocal block
            if (lerp < 0f)
            {
                if ((c.Anchor - c.Position).Length() > 768f)
                {
                    Utility.addRainbowStarExplosion(location, c.Position, 1);
                    c.Position = c.Owner.Position;
                    lerp = -1f;
                }
                if ((c.Anchor - c.Position).Length() > 80f)
                {
                    c.startPosition = c.Position;
                    float radius = 0.33f;
                    c.endPosition = c.Anchor + new Vector2(
                        Utility.RandomFloat(-64f, 64f) * radius,
                        Utility.RandomFloat(-64f, 64f) * radius
                    );
                    if (location.isCollidingPosition(
                            new Rectangle((int)c.endPosition.X - 8,
                            (int)c.endPosition.Y - 8, 16, 16),
                            Game1.viewport,
                            isFarmer: false,
                            0,
                            glider: false,
                            null,
                            pathfinding: true,
                            projectile: false,
                            ignoreCharacterRequirement: true
                        ))
                    {
                        c.endPosition = c.Anchor;
                    }
                    lerp = 0f;
                    // hopEvent.Fire(1f);
                    UpdateDirection();
                }
            }
            if (lerp >= 0f)
            {
                lerp += (float)time.ElapsedGameTime.TotalSeconds / 0.4f;
                if (lerp > 1f)
                {
                    lerp = 1f;
                }
                float x = Utility.Lerp(c.startPosition.X, c.endPosition.X, lerp);
                float y = Utility.Lerp(c.startPosition.Y, c.endPosition.Y, lerp);
                c.Position = new Vector2(x, y);
                if (lerp == 1f)
                {
                    lerp = -1f;
                }
            }
            c.Moving = lerp >= 0;
        }

        public override void UpdateGlobal(GameTime time, GameLocation location)
        {
            if (d.AlwaysMoving || c.Moving)
            {
                int frameStart = (d.DirectionMode == DirectionMode.UDLR) ? c.direction.Value * d.AnimationFrameLength : 0;
                frameStart += d.AnimationFrameStart;
                c.Sprite.Animate(time, frameStart, d.AnimationFrameLength, d.Interval);
            }
            else
            {
                c.Sprite.faceDirection((d.DirectionMode == DirectionMode.UDLR) ? c.direction.Value : 0);
            }
        }

        public override void Draw(SpriteBatch b)
        {
            b.Draw(
                c.Sprite.Texture,
                Game1.GlobalToLocal(c.Position + c.Owner.drawOffset + DrawOffset),
                c.Sprite.SourceRect,
                Color.White, 0f, new Vector2(8f, 8f), 4f,
                (c.direction.Value == 30) ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                c.Position.Y / 10000f
            );

            b.Draw(
                Game1.shadowTexture,
                Game1.GlobalToLocal(c.Position + c.Owner.drawOffset),
                Game1.shadowTexture.Bounds,
                Color.White, 0f,
                new Vector2(
                    Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y
                ),
                3f * Utility.Lerp(1f, 0.8f, Math.Min(Math.Abs(DrawOffset.Y), 1f)),
                SpriteEffects.None, (c.Position.Y - 8f) / 10000f - 2E-06f
            );
        }
    }
}