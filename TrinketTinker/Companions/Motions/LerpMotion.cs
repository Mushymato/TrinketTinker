using Microsoft.Xna.Framework;
using StardewValley;
using TrinketTinker.Models;

namespace TrinketTinker.Companions.Motions
{
    /// <summary>Companion follows the player, at a distance</summary>
    public class LerpMotion : Motion
    {
        /// <summary>Variable for how much interpolation happened so far.</summary>
        private float lerp = -1f;

        /// <summary>If the companion is farther than this, start pulling them to the anchor</summary>
        protected readonly float minDistance = 80f;
        /// <summary>If the companion is farther than this, teleport instead of lerp</summary>
        protected readonly float maxDistance = 768f;

        public LerpMotion(TrinketTinkerCompanion companion, MotionData data) : base(companion, data)
        {
            motionOffset.Y -= c.Sprite.SpriteHeight * 4 / 2;
            c.Offset = motionOffset;
            minDistance = d.GetParsedOrDefault("MinDistance", minDistance);
            maxDistance = d.GetParsedOrDefault("MaxDistance", maxDistance);
        }

        public override void UpdateLocal(GameTime time, GameLocation location)
        {
            // Copied from Companion.Update's IsLocal block
            if (lerp < 0f)
            {
                float distance = (c.Anchor - c.Position).Length();
                if (distance > maxDistance)
                {
                    Utility.addRainbowStarExplosion(location, c.Position, 1);
                    c.Position = c.Anchor;
                    lerp = -1f;
                }
                if (distance > minDistance)
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
    }
}