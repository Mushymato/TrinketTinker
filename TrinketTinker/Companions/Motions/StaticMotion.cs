using Microsoft.Xna.Framework;
using StardewValley;
using TrinketTinker.Models;
using TrinketTinker.Models.MotionArgs;

namespace TrinketTinker.Companions.Motions
{
    public class BaseStaticMotion<IArgs>(TrinketTinkerCompanion companion, MotionData mdata, VariantData vdata)
        : Motion<IArgs>(companion, mdata, vdata) where IArgs : StaticArgs
    {
        /// <inheritdoc/>
        public override void UpdateLocal(GameTime time, GameLocation location)
        {
            float distance = (c.Anchor - c.Position).Length();
            if (distance > 64f)
            {
                Utility.addRainbowStarExplosion(location, c.Position, 1);
            }
            c.Position = c.Anchor;
            UpdateDirection();
        }

        /// <inheritdoc/>
        protected override float GetPositionalLayerDepth(Vector2 offset)
        {
            return (c.direction.Value == 2) ? (c.Position.Y / 10000f) : 1f;
        }

        /// <inheritdoc/>
        protected override float GetRotation()
        {
            if (md.DirectionRotate)
            {
                return c.Owner.FacingDirection switch
                {
                    0 => -MathF.PI / 2,
                    1 => 0,
                    2 => MathF.PI / 2,
                    3 => MathF.PI,
                    _ => 0
                };
            }
            return 0f;
        }

        /// <summary>Update companion facing direction using player facing direction.</summary>
        protected override void UpdateDirection()
        {
            int prevDirection = c.direction.Value;
            int facingDirection = c.Owner.FacingDirection;
            c.direction.Value = md.DirectionMode switch
            {
                DirectionMode.DRUL => facingDirection switch
                {
                    0 => 3,
                    2 => 1,
                    _ => facingDirection + 1,
                },
                DirectionMode.DRU => facingDirection switch
                {
                    0 => 3,
                    1 => 2,
                    2 => 1,
                    3 => -2,
                    _ => facingDirection + 1,
                },
                DirectionMode.RL => facingDirection switch
                {
                    1 => 1,
                    3 => 2,
                    _ => prevDirection,
                },
                DirectionMode.R => facingDirection switch
                {
                    1 => 1,
                    3 => -1,
                    _ => prevDirection,
                },
                _ => 1,
            };
        }
    }

    /// <summary>Companion stays at some </summary>
    /// <param name="companion"></param>
    /// <param name="data"></param>
    public sealed class StaticMotion(TrinketTinkerCompanion companion, MotionData mdata, VariantData vdata) : BaseStaticMotion<StaticArgs>(companion, mdata, vdata)
    {
    }
}