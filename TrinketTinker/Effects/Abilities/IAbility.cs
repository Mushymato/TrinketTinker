using Microsoft.Xna.Framework;
using StardewValley;
using TrinketTinker.Effects.Proc;

namespace TrinketTinker.Effects.Abilities
{
    public interface IAbility
    {
        /// <summary>Mark the new ability as valid, if this is false after constructor, the ability is discarded</summary>
        bool Valid { get; }
        event EventHandler<ProcEventArgs>? EventAbilityProc;

        /// <summary>Activate the ability by registering events.</summary>
        /// <param name="farmer"></param>
        /// <returns></returns>
        bool Activate(Farmer farmer);

        /// <summary>Deactivate the ability by unregistering events.</summary>
        /// <param name="farmer"></param>
        /// <returns></returns>
        bool Deactivate(Farmer farmer);

        /// <summary>Perform update every tick.</summary>
        /// <param name="farmer"></param>
        /// <param name="time"></param>
        /// <param name="location"></param>
        void Update(Farmer farmer, GameTime time, GameLocation location);
    }
}
