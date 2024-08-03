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
            c.Moving = false;
            c.Position = c.Anchor;
            UpdateDirection();
        }

        public override void UpdateGlobal(GameTime time, GameLocation location)
        {
            int frameStart = (d.DirectionMode == DirectionMode.UDLR) ? c.direction.Value * d.AnimationFrameLength : 0;
            frameStart += d.AnimationFrameStart;
            c.Sprite.Animate(time, frameStart, d.AnimationFrameLength, d.Interval);
        }

        public override void Draw(SpriteBatch b)
        {
            b.Draw(
                c.Sprite.Texture,
                Game1.GlobalToLocal(c.Position + c.Owner.drawOffset + DrawOffset),
                c.Sprite.SourceRect,
                Color.White, 0f, new Vector2(8f, 8f), 4f,
                (c.direction.Value == 30) ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                1f
            );
        }
    }
}