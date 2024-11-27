using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using TrinketTinker.Effects.Proc;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;
using TrinketTinker.Wheels;

namespace TrinketTinker.Effects.Abilities;

/// <summary>Harvest terrain features</summary>
public abstract class BaseHarvestAbility<TArgs>(TrinketTinkerEffect effect, AbilityData data, int lvl)
    : Ability<TArgs>(effect, data, lvl)
    where TArgs : TileArgs
{
    /// <summary>Check that tile has object</summary>
    /// <param name="location"></param>
    /// <param name="tile"></param>
    /// <returns></returns>
    protected abstract bool ProbeTile(GameLocation location, Vector2 tile);

    /// <summary>Harvest given object</summary>
    /// <param name="location"></param>
    /// <param name="farmer"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    protected abstract bool DoHarvest(GameLocation location, Farmer farmer, Vector2 tile);

    /// <summary>Harvest forage or crops within range</summary>
    /// <param name="proc"></param>
    /// <returns></returns>
    protected override bool ApplyEffect(ProcEventArgs proc)
    {
        bool harvested = false;
        foreach (
            Vector2 tile in args.IterateRandomTiles(
                proc.LocationOrCurrent,
                e.CompanionPosition ?? proc.Farmer.Position,
                ProbeTile
            )
        )
        {
            harvested = DoHarvest(proc.LocationOrCurrent, proc.Farmer, tile) || harvested;
        }
        return harvested && base.ApplyEffect(proc);
    }
}

/// <summary>Harvest forage</summary>
public sealed class HarvestForageAbility(TrinketTinkerEffect effect, AbilityData data, int level)
    : BaseHarvestAbility<TileArgs>(effect, data, level)
{
    /// <inheritdocs/>
    protected override bool ProbeTile(GameLocation location, Vector2 tile)
    {
        return location.objects.TryGetValue(tile, out SObject obj) && obj.isForage();
    }

    /// <inheritdocs/>
    protected override bool DoHarvest(GameLocation location, Farmer farmer, Vector2 tile)
    {
        if (
            location.objects.TryGetValue(tile, out SObject obj)
            && obj.isForage()
            && !obj.questItem.Value
            && farmer.couldInventoryAcceptThisItem(obj)
        )
        {
            obj.Quality = location.GetHarvestSpawnedObjectQuality(
                farmer,
                obj.isForage(),
                obj.TileLocation,
                Random.Shared
            );
            farmer.addItemToInventoryBool(obj.getOne());
            if (!location.isFarmBuildingInterior())
            {
                if (
                    farmer.professions.Contains(Farmer.gatherer)
                    && Random.Shared.NextDouble() < 0.2
                    && !obj.questItem.Value
                    && farmer.couldInventoryAcceptThisItem(obj)
                )
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
            location.objects.Remove(tile);
            return true;
        }
        return false;
    }
}

/// <summary>Harvest (destroy) stone</summary>
public sealed class HarvestStoneAbility(TrinketTinkerEffect effect, AbilityData data, int level)
    : BaseHarvestAbility<TileArgs>(effect, data, level)
{
    /// <inheritdocs/>
    protected override bool ProbeTile(GameLocation location, Vector2 tile)
    {
        return location.objects.TryGetValue(tile, out var obj) && obj.IsBreakableStone();
    }

    /// <inheritdocs/>
    protected override bool DoHarvest(GameLocation location, Farmer farmer, Vector2 tile)
    {
        if (!location.objects.TryGetValue(tile, out SObject obj))
            return false;
        void OnDebrisAdded(Debris debris)
        {
            if (
                debris.debrisType.Value == Debris.DebrisType.OBJECT
                || debris.debrisType.Value == Debris.DebrisType.ARCHAEOLOGY
            )
            {
                location.debris.Remove(debris);
                debris.collect(farmer);
            }
        }
        ;
        location.debris.OnValueAdded += OnDebrisAdded;
        location.destroyObject(tile, farmer);
        location.debris.OnValueAdded -= OnDebrisAdded;
        // stone break TAS, from Pickaxe.cs
        TemporaryAnimatedSprite temporaryAnimatedSprite =
            (
                ItemRegistry.GetDataOrErrorItem(obj.QualifiedItemId).TextureName == "Maps\\springobjects"
                && obj.ParentSheetIndex < 200
                && !Game1.objectData.ContainsKey((obj.ParentSheetIndex + 1).ToString())
                && obj.QualifiedItemId != "(O)25"
            )
                ? new TemporaryAnimatedSprite(
                    obj.ParentSheetIndex + 1,
                    300f,
                    1,
                    2,
                    new Vector2(tile.X - tile.X % Game1.tileSize, tile.Y - tile.Y % Game1.tileSize),
                    flicker: true,
                    obj.Flipped
                )
                {
                    alphaFade = 0.01f,
                }
                : new TemporaryAnimatedSprite(47, tile * Game1.tileSize, Color.Gray, 10, flipped: false, 80f);
        Game1.Multiplayer.broadcastSprites(location, temporaryAnimatedSprite);

        return true;
    }
}

/// <summary>Harvest (destroy) stone</summary>
public sealed class HarvestCropAbility(TrinketTinkerEffect effect, AbilityData data, int level)
    : BaseHarvestAbility<TileArgs>(effect, data, level)
{
    /// <inheritdocs/>
    protected override bool ProbeTile(GameLocation location, Vector2 tile)
    {
        return location.terrainFeatures.TryGetValue(tile, out TerrainFeature feature)
            && feature is HoeDirt dirt
            && dirt.crop != null
            && dirt.crop.CanHarvest();
    }

    /// <inheritdocs/>
    protected override bool DoHarvest(GameLocation location, Farmer farmer, Vector2 tile)
    {
        if (!(location.terrainFeatures.TryGetValue(tile, out TerrainFeature feature) && feature is HoeDirt dirt))
            return false;
        bool harvested = false;
        void OnDebrisAdded(Debris debris)
        {
            if (
                debris.debrisType.Value == Debris.DebrisType.OBJECT
                || debris.debrisType.Value == Debris.DebrisType.ARCHAEOLOGY
            )
            {
                location.debris.Remove(debris);
                debris.collect(farmer);
                harvested = true;
            }
        }
        ;
        location.debris.OnValueAdded += OnDebrisAdded;
        if (dirt.crop.harvest((int)tile.X, (int)tile.Y, dirt, null, true))
        {
            dirt.destroyCrop(true);
        }
        location.debris.OnValueAdded -= OnDebrisAdded;

        return harvested;
    }
}
