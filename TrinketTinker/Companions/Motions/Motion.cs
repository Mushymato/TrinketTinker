using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;

namespace TrinketTinker.Companions.Motions
{
    public abstract class Motion
    {
        protected TrinketTinkerCompanion c;
        protected Vector2 _drawOffset = Vector2.Zero;
        public Vector2 DrawOffset => _drawOffset;
        public Motion(TrinketTinkerCompanion companion)
        {
            c = companion;
        }
        public abstract void UpdateLocal(GameTime time, GameLocation location);
        public virtual void UpdateGlobal(GameTime time, GameLocation location)
        {
            if (c.Moving)
                c.Sprite.Animate(time, c.direction.Value * c.FramesPerAnimation, c.FramesPerAnimation, c.Interval);
            else
                c.Sprite.faceDirection(c.direction.Value);
        }
    }
}