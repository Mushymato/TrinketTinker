using Microsoft.Xna.Framework;
using StardewValley;

namespace TrinketTinker.Effects.Abilities
{
    public interface IAbility
    {
        bool Valid { get; }

        bool Activate(Farmer farmer);
        bool Deactivate(Farmer farmer);
        void Update(Farmer farmer, GameTime time, GameLocation location);
    }
}
