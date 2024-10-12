using Microsoft.Xna.Framework;
using StardewValley;
using TrinketTinker.Models;
using TrinketTinker.Models.MotionArgs;

namespace TrinketTinker.Companions.Motions
{
    /// <summary>Base version of LerpMotion, for use with inheritance</summary>
    public class BaseLerpMotion<IArgs> : Motion<IArgs> where IArgs : LerpArgs
    {
        /// <summary>Variable for how much interpolation happened so far.</summary>
        private float lerp = -1f;

        /// <summary>If the companion is farther than this, start pulling them to the anchor</summary>
        protected readonly float minDistance = 80f;
        /// <summary>If the companion is farther than this, teleport instead of lerp</summary>
        protected readonly float maxDistance = 768f;

        public BaseLerpMotion(TrinketTinkerCompanion companion, MotionData data) : base(companion, data)
        {
            motionOffset.Y -= c.Sprite.SpriteHeight * 4 / 2;
            c.Offset = motionOffset;
            minDistance = args?.Min ?? minDistance;
            maxDistance = args?.Max ?? maxDistance;
        }

        public override void UpdateLocal(GameTime time, GameLocation location)
        {
            if (lerp < 0f || AnchorChanged)
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
                    if (CheckSpriteCollsion(location, c.endPosition + motionOffset))
                    {
                        c.endPosition = c.Anchor;
                    }
                    lerp = 0f;
                }
            }
            if (lerp >= 0f)
            {
                lerp += (float)time.ElapsedGameTime.TotalSeconds / 0.4f;
                if (lerp > 1f)
                {
                    lerp = 1f;
                }
                c.NetPosition.X = Utility.Lerp(c.startPosition.X, c.endPosition.X, lerp);
                c.NetPosition.Y = Utility.Lerp(c.startPosition.Y, c.endPosition.Y, lerp);
                UpdateDirection();
                if (lerp == 1f)
                {
                    lerp = -1f;
                }
            }
            c.Moving = lerp >= 0;
        }
    }

    /// <summary>Companion closely follows the anchor, at a distance</summary>
    /// <param name="companion"></param>
    /// <param name="data"></param>
    public class LerpMotion(TrinketTinkerCompanion companion, MotionData data) : BaseLerpMotion<BounceArgs>(companion, data)
    {
    }
}