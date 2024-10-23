using Microsoft.Xna.Framework;
using StardewValley;
using TrinketTinker.Effects.Proc;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;

namespace TrinketTinker.Effects.Abilities
{
    /// <summary>Harvest terrain features</summary>
    public abstract class BaseHarvestAbility<TArgs>(TrinketTinkerEffect effect, AbilityData data, int lvl) : Ability<TArgs>(effect, data, lvl) where TArgs : TileArgs
    {
        /// <summary>Check that tile has object</summary>
        /// <param name="location"></param>
        /// <param name="tile"></param>
        /// <returns></returns>
        protected virtual bool ProbeTile(GameLocation location, Vector2 tile)
        {
            return location.objects.ContainsKey(tile);
        }

        /// <summary>Harvest given object</summary>
        /// <param name="location"></param>
        /// <param name="farmer"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected abstract bool DoHarvest(GameLocation location, Farmer farmer, SObject obj);

        /// <summary>Harvest forage or crops within range</summary>
        /// <param name="proc"></param>
        /// <returns></returns>
        protected override bool ApplyEffect(ProcEventArgs proc)
        {
            bool gotForage = false;
            foreach (Vector2 tile in args.IterateRandomTiles(proc.LocationOrCurrent, e.CompanionPosition ?? proc.Farmer.Position, ProbeTile))
            {
                if (proc.LocationOrCurrent.objects.TryGetValue(tile, out SObject obj) && DoHarvest(proc.LocationOrCurrent, proc.Farmer, obj))
                {
                    proc.LocationOrCurrent.objects.Remove(tile);
                    gotForage = true;
                }
            }
            return gotForage && base.ApplyEffect(proc);
        }
    }

    /// <summary>Harvest forage</summary>
    public sealed class HarvestForageAbility(TrinketTinkerEffect effect, AbilityData data, int level) : BaseHarvestAbility<TileArgs>(effect, data, level)
    {
        /// <inheritdocs/>
        protected override bool ProbeTile(GameLocation location, Vector2 tile)
        {
            return base.ProbeTile(location, tile) && location.objects[tile].isForage();
        }

        protected override bool DoHarvest(GameLocation location, Farmer farmer, SObject obj)
        {
            if (obj.isForage() && !obj.questItem.Value && farmer.couldInventoryAcceptThisItem(obj))
            {
                obj.Quality = location.GetHarvestSpawnedObjectQuality(
                    farmer, obj.isForage(), obj.TileLocation,
                    Random.Shared
                );
                farmer.addItemToInventoryBool(obj.getOne());
                if (!location.isFarmBuildingInterior())
                {
                    if (farmer.professions.Contains(Farmer.gatherer) &&
                        Random.Shared.NextDouble() < 0.2 &&
                        !obj.questItem.Value && farmer.couldInventoryAcceptThisItem(obj))
                    {
                        farmer.addItemToInventoryBool(obj.getOne());
                        farmer.gainExperience(2, 7);
                    }
                    location.OnHarvestedForage(farmer, obj);
                }
                else
                {
                    farmer.gainExperience(0, 5);
                }
                Game1.stats.ItemsForaged++;
                return true;
            }
            return false;
        }
    }
}
