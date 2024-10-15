using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
            c.Moving = c.Position != c.Anchor;
            float distance = (c.Anchor - c.Position).Length();
            if (distance > 64f)
            {
                Utility.addRainbowStarExplosion(location, c.Position, 1);
            }
            c.Position = c.Anchor;
            UpdateDirection();
            c.Offset = motionOffset;
        }

        /// <inheritdoc/>
        public override void Draw(SpriteBatch b)
        {
            DrawWithShadow(
                b, (c.direction.Value == 2) ? (c.Position.Y / 10000f) : 1f,
                vd.VecTextureScale,
                vd.VecShadowScale
            );
        }

        /// <summary>Update companion facing direction using player facing direction.</summary>
        protected override void UpdateDirection()
        {
            int prevDirection = c.direction.Value;
            int facingDirection = c.Owner.FacingDirection;
            switch (md.DirectionMode)
            {
                case DirectionMode.DRUL:
                    c.direction.Value = facingDirection switch
                    {
                        0 => 3,
                        2 => 1,
                        _ => facingDirection + 1,
                    };
                    break;
                case DirectionMode.DRU:
                    c.direction.Value = facingDirection switch
                    {
                        0 => 3,
                        1 => 2,
                        2 => 1,
                        3 => -2,
                        _ => facingDirection + 1,
                    };
                    break;
                case DirectionMode.RL:
                    c.direction.Value = facingDirection switch
                    {
                        1 => 1,
                        3 => 2,
                        _ => prevDirection,
                    };
                    break;
                case DirectionMode.R:
                    c.direction.Value = facingDirection switch
                    {
                        1 => 1,
                        3 => -1,
                        _ => prevDirection,
                    };
                    break;
                case DirectionMode.Rotate:
                    c.rotation.Value = facingDirection switch
                    {
                        0 => -MathF.PI / 2,
                        1 => 0,
                        2 => MathF.PI / 2,
                        3 => MathF.PI,
                        _ => 0
                    };
                    break;
                case DirectionMode.Single:
                    c.direction.Value = 1;
                    break;
            }
        }
    }

    /// <summary>Companion stays at some </summary>
    /// <param name="companion"></param>
    /// <param name="data"></param>
    public sealed class StaticMotion(TrinketTinkerCompanion companion, MotionData mdata, VariantData vdata) : BaseStaticMotion<StaticArgs>(companion, mdata, vdata)
    {
    }
}