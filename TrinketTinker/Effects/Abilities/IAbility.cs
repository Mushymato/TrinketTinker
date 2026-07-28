using Microsoft.Xna.Framework;
using StardewValley;
using TrinketTinker.Effects.Support;
using TrinketTinker.Models;

namespace TrinketTinker.Effects.Abilities;

public interface IAbility
{
    /// <summary>Mark the new ability as valid, if this is false after constructor, the ability is discarded</summary>
    bool Valid { get; }

    /// <summary>The data associated with this ability</summary>
    public AbilityData Data { get; }

    /// <summary>The resolved ProcSyncIndex for <see cref="Models.ProcOn.Sync"/>, only set to >0 if the ability is enabled</summary>
    int ProcSyncIndex { get; }

    /// <summary>Proc event for use with sync procs</summary>
    event EventHandler<ProcEventArgs>? EventAbilityProc;

    /// <summary>Activate the ability by registering events.</summary>
    /// <param name="farmer"></param>
    /// <returns></returns>
    bool Activate(Farmer farmer);

    /// <summary>Deactivate the ability by unregistering events.</summary>
    /// <param name="farmer"></param>
    /// <returns></returns>
    bool Deactivate(Farmer farmer);

    /// <summary>Manually proc this ability for <see cref="ProcOn.Interact"/> which has special handling and does not use events.</summary>
    /// <param name="sender"></param>
    /// <param name="farmer"></param>
    /// <returns></returns>
    bool InteractProc(object? sender, ProcEventArgs proc);

    /// <summary>Perform update every tick.</summary>
    /// <param name="farmer"></param>
    /// <param name="time"></param>
    /// <param name="location"></param>
    void Update(Farmer farmer, GameTime time, GameLocation location);
}
