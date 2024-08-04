using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using TrinketTinker.Model;

namespace TrinketTinker.Companions.Motions
{
    public class StaticMotion : Motion
    {
        public StaticMotion(TrinketTinkerCompanion companion, MotionData data) : base(companion, data) { }
        public override void UpdateLocal(GameTime time, GameLocation location)
        {
            c.Moving = c.Position != c.Anchor;
            c.Position = c.Anchor;
            c.Offset = MotionOffset;
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
            b.Draw(
                c.Sprite.Texture,
                Game1.GlobalToLocal(c.Position + c.Offset + c.Owner.drawOffset),
                c.Sprite.SourceRect,
                Color.White, 0f, new Vector2(8f, 8f), 4f,
                (c.direction.Value < 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                (c.direction.Value == 3) ? (c.Position.Y / 10000f - 2E-06f) : 1f
            );
        }
        protected override void UpdateDirection()
        {
            UpdateDirection(c.Position + c.Offset);
        }
    }
}