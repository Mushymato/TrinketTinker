using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using TrinketTinker.Models;

namespace TrinketTinker.Companions.Motions
{
    public class LerpMotion : Motion
    {
        private float lerp = -1f;
        public LerpMotion(TrinketTinkerCompanion companion, MotionData data) : base(companion, data)
        {
        }

        public override void UpdateLocal(GameTime time, GameLocation location)
        {
            // Copied from Companion.Update's IsLocal block
            if (lerp < 0f)
            {
                if ((c.Anchor - c.Position).Length() > 768f)
                {
                    Utility.addRainbowStarExplosion(location, c.Position, 1);
                    c.Position = c.Anchor;
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
                UpdateDirection();
                if (lerp == 1f)
                {
                    lerp = -1f;
                }
            }
            c.Moving = lerp >= 0;
        }

        public override void Draw(SpriteBatch b)
        {
            float shadowScale = 3f * Utility.Lerp(1f, 0.8f, Math.Max(1f, -c.Offset.Y / 12));
            DrawWithShadow(
                b, c.Position.Y / 10000f,
                new Vector2(d.TextureScale, d.TextureScale),
                new Vector2(shadowScale, shadowScale)
            );
        }
    }
}