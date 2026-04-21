using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using TrinketTinker.Effects.Support;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;

namespace TrinketTinker.Effects.Abilities;

/// <summary>Hoe dirt around the companion</summary>
public sealed class HoeDirtAbility(TrinketTinkerEffect effect, AbilityData data, int lvl)
    : Ability<HoeDirtArgs>(effect, data, lvl)
{
    /// <summary>Hoe random amounts of dirt within range</summary>
    /// <param name="proc"></param>
    /// <returns></returns>
    protected override bool ApplyEffect(ProcEventArgs proc)
    {
        int madeDirt = 0;
        Func<GameLocation, Vector2, bool>? match = null;
        if (!args.NewDirt)
        {
            match = (location, pos) =>
            {
                if (location.terrainFeatures.TryGetValue(pos, out TerrainFeature feature) && feature is HoeDirt hoeDirt)
                    return true;
                return false;
            };
        }
        foreach (
            var tile in args.IterateRandomTiles(proc.LocationOrCurrent, e.CompanionPosition ?? proc.Farmer.Position)
        )
        {
            if (
                !proc.LocationOrCurrent.terrainFeatures.TryGetValue(tile, out TerrainFeature feature)
                || feature is not HoeDirt dirt
            )
            {
                if (args.NewDirt && proc.LocationOrCurrent.makeHoeDirt(tile))
                {
                    madeDirt++;
                    dirt = proc.LocationOrCurrent.GetHoeDirtAtTile(tile);
                }
                else
                {
                    continue;
                }
            }
            if (args.Watering && dirt.state.Value == 0)
            {
                dirt.state.Value = 1;
            }
        }
        return madeDirt > 0 && base.ApplyEffect(proc);
    }
}
