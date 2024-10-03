using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;

namespace TrinketTinker.Effects.Abilities
{
    public interface IAbility
    {
        bool Valid { get; }

        bool Activate(Farmer farmer);
        bool Deactivate(Farmer farmer);
        void Update(Farmer farmer, GameTime time, GameLocation location);
        bool Proc(Farmer farmer);
        bool Proc(Farmer farmer, int damageAmount);
        bool Proc(Farmer farmer, Monster monster, int damageAmount, bool isBomb, bool isCriticalHit);
    }
}
