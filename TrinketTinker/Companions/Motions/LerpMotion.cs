using Microsoft.Xna.Framework;
using StardewValley;
using TrinketTinker.Models;
using TrinketTinker.Models.MotionArgs;

namespace TrinketTinker.Companions.Motions
{
    /// <summary>Base version of LerpMotion, for use with inheritance</summary>
    /// <inheritdoc/>
    public class BaseLerpMotion<IArgs>(TrinketTinkerCompanion companion, MotionData mdata, VariantData vdata) : Motion<IArgs>(companion, mdata, vdata) where IArgs : LerpArgs
    {
        /// <summary>Variable for how much interpolation happened so far.</summary>
        private float lerp = -1f;

        /// <inheritdoc/>
        public override void UpdateLocal(GameTime time, GameLocation location)
        {
            if (lerp < 0f || AnchorChanged)
            {
                float distance = (c.Anchor - c.Position).Length();
                if (distance > args.Max)
                {
                    Utility.addRainbowStarExplosion(location, c.Position, 1);
                    c.Position = c.Anchor;
                    lerp = -1f;
                }
                if (distance > args.Min)
                {
                    c.startPosition = c.Position;
                    float radius = 0.33f;
                    c.endPosition = c.Anchor + new Vector2(
                        Utility.RandomFloat(-64f, 64f) * radius,
                        Utility.RandomFloat(-64f, 64f) * radius
                    );
                    if (CheckSpriteCollsion(location, c.endPosition + GetOffset()))
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
        }

        /// <inheritdoc/>
        public override Vector2 GetOffset()
        {
            return new Vector2(0, -vd.Height * 4 / 2) + base.GetOffset();
        }

        /// <inheritdoc/>
        protected override float GetRotation()
        {
            if (md.DirectionRotate)
            {
                Vector2 posDelta = c.Anchor - c.Position;
                return (float)Math.Atan2(posDelta.Y, posDelta.X);
            }
            return 0f;
        }
    }

    /// <summary>Companion closely follows the anchor, at a distance</summary>
    /// <param name="companion"></param>
    /// <param name="data"></param>
    public class LerpMotion(TrinketTinkerCompanion companion, MotionData data, VariantData vdata) : BaseLerpMotion<LerpArgs>(companion, data, vdata)
    {
    }
}