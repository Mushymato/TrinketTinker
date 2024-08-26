using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using TrinketTinker.Models;

namespace TrinketTinker.Companions.Motions
{
    public class StaticMotion : Motion
    {
        public StaticMotion(TrinketTinkerCompanion companion, MotionData data) : base(companion, data) { }
        public override void UpdateLocal(GameTime time, GameLocation location)
        {
            c.Moving = c.Position != c.Anchor;
            c.Position = c.Anchor;
            c.Offset = motionOffset;
            UpdateDirection();
        }
        public override void Draw(SpriteBatch b)
        {
            DrawWithShadow(
                b, (c.direction.Value == 2) ? (c.Position.Y / 10000f) : 1f,
                new Vector2(d.TextureScale, d.TextureScale),
                new Vector2(d.ShadowScale, d.ShadowScale)
            );
        }

        /// <summary>Update companion facing direction using current position and offset.</summary>
        protected override void UpdateDirection()
        {
            int prevDirection = c.direction.Value;
            int facingDirection = c.Owner.FacingDirection;
            switch (d.DirectionMode)
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
                        0 => -1,
                        2 => 1,
                        _ => facingDirection + 1,
                    };
                    break;
                case DirectionMode.RL:
                    c.direction.Value = facingDirection switch
                    {
                        0 => 2,
                        2 => 1,
                        _ => prevDirection,
                    };
                    break;
                case DirectionMode.R:
                    c.direction.Value = facingDirection switch
                    {
                        0 => -1,
                        2 => 1,
                        _ => prevDirection,
                    };
                    break;
                case DirectionMode.Rotate:
                case DirectionMode.None:
                    c.direction.Value = 1;
                    break;
            }
        }
    }
}