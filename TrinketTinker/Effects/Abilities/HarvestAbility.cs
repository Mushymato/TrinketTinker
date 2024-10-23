using Microsoft.Xna.Framework;
using StardewValley;
using TrinketTinker.Effects.Proc;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;

namespace TrinketTinker.Effects.Abilities
{
    /// <summary>Harvest terrain features</summary>
    public sealed class HarvestAbility(TrinketTinkerEffect effect, AbilityData data, int lvl) : Ability<TileArgs>(effect, data, lvl)
    {
        private static bool DoHarvest(GameLocation location, Farmer farmer, SObject obj)
        {
            if (obj.isForage() && !obj.questItem.Value && farmer.couldInventoryAcceptThisItem(obj))
            {
                location.localSound("pickUpItem");
                DelayedAction.playSoundAfterDelay("coin", 300);
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

        /// <summary>Harvest forage or crops within range</summary>
        /// <param name="proc"></param>
        /// <returns></returns>
        protected override bool ApplyEffect(ProcEventArgs proc)
        {
            bool gotForage = false;
            foreach (Vector2 tile in args.GetTilesInRange(proc.LocationOrCurrent, e.CompanionPosition ?? proc.Farmer.Position))
            {
                // if (proc.LocationOrCurrent.objects.TryGetValue(tile, out SObject obj) && obj.isForage())
                // {
                //     proc.LocationOrCurrent.checkAction(new xTile.Dimensions.Location((int)tile.X, (int)tile.Y), Game1.viewport, proc.Farmer);
                // }
                if (proc.LocationOrCurrent.objects.TryGetValue(tile, out SObject obj) && DoHarvest(proc.LocationOrCurrent, proc.Farmer, obj))
                {
                    proc.LocationOrCurrent.objects.Remove(tile);
                }
            }
            return gotForage && base.ApplyEffect(proc);
        }
    }
}
