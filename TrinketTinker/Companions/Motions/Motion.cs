using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using TrinketTinker.Models;

namespace TrinketTinker.Companions.Motions
{
    public abstract class Motion
    {
        protected readonly TrinketTinkerCompanion c;
        protected readonly MotionData d;
        protected Vector2 MotionOffset;
        protected float frameRate;
        public Motion(TrinketTinkerCompanion companion, MotionData data)
        {
            c = companion;
            d = data;
            MotionOffset = new(d.OffsetX, d.OffsetY);
            c.Offset = MotionOffset;
            frameRate = 1000f / d.Interval;
        }
        public abstract void UpdateLocal(GameTime time, GameLocation location);
        public virtual void UpdateGlobal(GameTime time, GameLocation location)
        {
            if (d.AlwaysMoving || c.Moving)
            {
                c.Sprite.Animate(time, DirectionFrameStart(), d.AnimationFrameLength, d.Interval);
            }
            else
            {
                c.Sprite.currentFrame = DirectionFrameStart();
                c.Sprite.UpdateSourceRect();
            }
        }
        public abstract void Draw(SpriteBatch b);
        protected virtual void DrawWithShadow(SpriteBatch b, float rotation, float layerDepth, Vector2 textureScale, Vector2 shadowScale)
        {
            b.Draw(
                c.Sprite.Texture,
                Game1.GlobalToLocal(c.Position + c.Offset + c.Owner.drawOffset),
                c.Sprite.SourceRect,
                Color.White,
                rotation,
                c.SpriteOrigin,
                textureScale,
                (c.direction.Value < 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                layerDepth
            );
            b.Draw(
                Game1.shadowTexture,
                Game1.GlobalToLocal(c.Position + new Vector2(c.Offset.X, 0) + c.Owner.drawOffset),
                Game1.shadowTexture.Bounds,
                Color.White, 0f,
                new Vector2(
                    Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y
                ),
                shadowScale,
                SpriteEffects.None,
                layerDepth - 2E-06f
            );
        }
        protected virtual void UpdateDirection()
        {
            UpdateDirection(c.Position);
        }
        protected virtual void UpdateDirection(Vector2 position)
        {
            Vector2 posDelta;
            switch (d.DirectionMode)
            {
                case DirectionMode.DRUL:
                    posDelta = c.Anchor - position;
                    if (Math.Abs(posDelta.X) > Math.Abs(posDelta.Y))
                    {
                        c.direction.Value = (c.Anchor.X > position.X) ? 2 : 4;
                    }
                    else
                    {
                        c.direction.Value = (c.Anchor.Y > position.Y) ? 1 : 3;
                    }
                    break;
                case DirectionMode.DRU:
                    posDelta = c.Anchor - position;
                    if (Math.Abs(posDelta.X) > Math.Abs(posDelta.Y))
                    {
                        c.direction.Value = (c.Anchor.X > position.X) ? 2 : -2;
                    }
                    else
                    {
                        c.direction.Value = (c.Anchor.Y > position.Y) ? 1 : 3;
                    }
                    break;
                case DirectionMode.R:
                    posDelta = c.Anchor - position;
                    if (Math.Abs(posDelta.X) > 8f)
                        c.direction.Value = (c.Anchor.X > position.X) ? 1 : -1;
                    break;
                case DirectionMode.None:
                    c.direction.Value = 0;
                    break;
            }
        }
        protected virtual int DirectionFrameStart()
        {
            if (d.DirectionMode == DirectionMode.None)
                return d.AnimationFrameStart;
            return (Math.Abs(c.direction.Value) - 1) * d.AnimationFrameLength + d.AnimationFrameStart;
        }
    }
}