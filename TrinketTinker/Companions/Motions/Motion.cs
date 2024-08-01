using Microsoft.Xna.Framework;
using StardewValley;

namespace TrinketTinker.Companions.Motions
{
    public abstract class Motion
    {
        protected TrinketTinkerCompanion c;
        public Motion(TrinketTinkerCompanion companion)
        {
            c = companion;
        }
        public abstract void Update(GameTime time, GameLocation location);
    }
}