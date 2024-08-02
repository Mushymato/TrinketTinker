using Microsoft.Xna.Framework;
using StardewValley;
using TrinketTinker.Model;

namespace TrinketTinker.Companions.Motions
{
    public abstract class Motion
    {
        protected readonly TrinketTinkerCompanion c;
        protected readonly MotionData d;
        public readonly Vector2 DrawOffset;
        public Motion(TrinketTinkerCompanion companion, MotionData data)
        {
            c = companion;
            d = data;
            DrawOffset = new Vector2(d.DrawOffsetX, d.DrawOffsetY);
        }
        protected virtual void UpdateDirection()
        {
            Vector2 posDelta;
            switch (d.DirectionMode)
            {
                case DirectionMode.UDLR:
                    posDelta = c.Anchor - c.Position;
                    if (Math.Abs(posDelta.X) > Math.Abs(posDelta.Y))
                    {
                        if (Math.Abs(posDelta.X) > 8f)
                            c.direction.Value = (c.Anchor.X > c.Position.X) ? 1 : 3;
                    }
                    else
                    {
                        if (Math.Abs(posDelta.Y) > 8f)
                            c.direction.Value = (c.Anchor.Y > c.Position.Y) ? 2 : 0;
                    }
                    break;
                case DirectionMode.LR:
                    posDelta = c.Anchor - c.Position;
                    if (Math.Abs(posDelta.X) > 8f)
                        c.direction.Value = (c.Anchor.X > c.Position.X) ? 10 : 30;
                    break;
                case DirectionMode.None:
                    c.direction.Value = 0;
                    break;
            }
        }
        public abstract void UpdateLocal(GameTime time, GameLocation location);
        public virtual void UpdateGlobal(GameTime time, GameLocation location)
        {
            if (d.AlwaysMoving || c.Moving)
            {
                int frameStart = (d.DirectionMode == DirectionMode.UDLR) ? c.direction.Value * d.AnimationFrameLength : 0;
                frameStart += d.AnimationFrameStart;
                c.Sprite.Animate(time, frameStart, d.AnimationFrameLength, d.Interval);
            }
            else
            {
                c.Sprite.faceDirection((d.DirectionMode == DirectionMode.UDLR) ? c.direction.Value : 0);
            }
        }
    }
}