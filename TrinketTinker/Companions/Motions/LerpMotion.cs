using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace TrinketTinker.Companions.Motions
{
    public class LerpMotion : Motion
    {
        public LerpMotion(TrinketTinkerCompanion companion) : base(companion) { }
        public override void UpdateLocal(GameTime time, GameLocation location)
        {
            // Copied from Companion.Update's IsLocal block
            if (c.lerp < 0f)
            {
                if ((c.Anchor - c.Position).Length() > 768f)
                {
                    Utility.addRainbowStarExplosion(location, c.Position, 1);
                    c.Position = c.Owner.Position;
                    c.lerp = -1f;
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
                    c.lerp = 0f;
                    // hopEvent.Fire(1f);
                    Vector2 posDelta = c.Anchor - c.Position;
                    if (Math.Abs(posDelta.X) > Math.Abs(posDelta.Y))
                    {
                        if (Math.Abs(posDelta.X) > 8f)
                        {
                            c.direction.Value = (c.Anchor.X > c.Position.X) ? 1 : 3;
                        }
                    }
                    else
                    {
                        if (Math.Abs(posDelta.Y) > 8f)
                        {
                            c.direction.Value = (c.Anchor.Y > c.Position.Y) ? 0 : 2;
                        }
                    }
                }
            }
            if (c.lerp >= 0f)
            {
                c.lerp += (float)time.ElapsedGameTime.TotalSeconds / 0.4f;
                if (c.lerp > 1f)
                {
                    c.lerp = 1f;
                }
                float x = Utility.Lerp(c.startPosition.X, c.endPosition.X, c.lerp);
                float y = Utility.Lerp(c.startPosition.Y, c.endPosition.Y, c.lerp);
                c.Position = new Vector2(x, y);
                if (c.lerp == 1f)
                {
                    c.lerp = -1f;
                }
            }
            c.Moving = c.lerp >= 0;
        }

        public override void UpdateGlobal(GameTime time, GameLocation location)
        {
            base.UpdateGlobal(time, location);
            _drawOffset = c.Owner.drawOffset;
            _drawOffset.Y -= 128;
        }
    }
}