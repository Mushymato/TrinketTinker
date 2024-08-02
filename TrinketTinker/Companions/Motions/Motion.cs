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
            c.Sprite.Animate(time, 0, c.FramesPerAnimation, c.Interval);
        }
    }
}