using StardewValley.TerrainFeatures;
using TrinketTinker.Effects.Proc;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;

namespace TrinketTinker.Effects.Abilities;

/// <summary>Hoe dirt around the companion</summary>
public sealed class HoeDirtAbility(TrinketTinkerEffect effect, AbilityData data, int lvl) : Ability<TileArgs>(effect, data, lvl)
{
    /// <summary>Hoe random amounts of dirt within range</summary>
    /// <param name="proc"></param>
    /// <returns></returns>
    protected override bool ApplyEffect(ProcEventArgs proc)
    {
        int madeDirt = 0;
        foreach (var tile in args.IterateRandomTiles(proc.LocationOrCurrent, e.CompanionPosition ?? proc.Farmer.Position))
        {
            if (proc.LocationOrCurrent.makeHoeDirt(tile))
            {
                madeDirt++;
                HoeDirt dirt = proc.LocationOrCurrent.GetHoeDirtAtTile(tile);
                if (dirt.state.Value == 0)
                {
                    proc.LocationOrCurrent.GetHoeDirtAtTile(tile).state.Value = 1;
                }
            }
        }
        return madeDirt > 0 && base.ApplyEffect(proc);
    }
}
