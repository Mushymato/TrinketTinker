using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace TrinketTinker.Companions.Motions
{
    public interface IMotion
    {
        void Initialize(Farmer farmer);
        void Cleanup();
        void Draw(SpriteBatch b);
        void OnOwnerWarp();
        void UpdateAnchor(GameTime time, GameLocation location);
        void UpdateGlobal(GameTime time, GameLocation location);
        void UpdateLocal(GameTime time, GameLocation location);
    }
}
