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
            // UpdateDirection();
            int newDirection = c.Owner.FacingDirection;
            switch (newDirection)
            {
                case 0:
                    newDirection = 2;
                    break;
                case 2:
                    newDirection = 0;
                    break;
            }
            c.direction.Value = newDirection + 1;
        }
        public override void Draw(SpriteBatch b)
        {
            DrawWithShadow(
                b, (c.direction.Value == 3) ? (c.Position.Y / 10000f) : 1f,
                new Vector2(d.TextureScale, d.TextureScale),
                new Vector2(d.ShadowScale, d.ShadowScale)
            );
        }

        /// <summary>Update companion facing direction using current position and offset.</summary>
        protected override void UpdateDirection()
        {
            UpdateDirection(c.Position + c.Offset);
        }
    }
}