using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using TrinketTinker.Effects.Support;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;
using TrinketTinker.Wheels;

namespace TrinketTinker.Effects.Abilities;

/// <summary>Harvest terrain features</summary>
public abstract class BaseHarvestAbility<TArgs>(TrinketTinkerEffect effect, AbilityData data, int lvl)
    : Ability<TArgs>(effect, data, lvl)
    where TArgs : HarvestArgs
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
    : BaseHarvestAbility<HarvestArgs>(effect, data, level)
{
    /// <inheritdocs/>
    protected override bool ProbeTile(GameLocation location, Vector2 tile)
    {
        return location.objects.TryGetValue(tile, out SObject obj) && IsForage(obj, args.Filters);
    }

    /// <summary>Check if an object is forage</summary>
    /// <param name="obj"></param>
    /// <param name="filters"></param>
    /// <returns></returns>
    public static bool IsForage(SObject obj, IList<string>? filters)
    {
        return obj.isForage() && (filters == null || Places.CheckContextTagFilter(obj, filters));
    }

    /// <summary>Check if farmer has room in inventory</summary>
    /// <param name="farmer"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    private bool CanTargetAccept(Farmer farmer, SObject obj)
    {
        return args.HarvestTo switch
        {
            HarvestDestination.Player => farmer.couldInventoryAcceptThisItem(obj),
            HarvestDestination.TinkerInventory => e.InvHandler.Value?.CanAcceptThisItem(obj) ?? false,
            _ => true,
        };
    }

    /// <inheritdocs/>
    protected override bool DoHarvest(GameLocation location, Farmer farmer, Vector2 tile)
    {
        if (
            location.objects.TryGetValue(tile, out SObject obj)
            && obj.isForage()
            && !obj.questItem.Value
            && CanTargetAccept(farmer, obj)
        )
        {
            obj.Quality = location.GetHarvestSpawnedObjectQuality(
                farmer,
                obj.isForage(),
                obj.TileLocation,
                Random.Shared
            );

            if (args.HarvestTo != HarvestDestination.None)
            {
                Action harvestMethod;
                if (args.HarvestTo == HarvestDestination.Debris)
                    harvestMethod = () =>
                        Game1.createItemDebris(obj.getOne(), new Vector2(tile.X * 64f + 32f, tile.Y * 64f + 32f), -1);
                else if (args.HarvestTo == HarvestDestination.TinkerInventory)
                    harvestMethod = () => e.InvHandler.Value?.AddItem(obj.getOne());
                else
                    harvestMethod = () => farmer.addItemToInventoryBool(obj.getOne());
                harvestMethod();
                if (!location.isFarmBuildingInterior())
                {
                    if (
                        farmer.professions.Contains(Farmer.gatherer)
                        && Random.Shared.NextDouble() < 0.2
                        && !obj.questItem.Value
                        && CanTargetAccept(farmer, obj)
                    )
                    {
                        harvestMethod();
                        farmer.gainExperience(2, 7);
                    }
                    location.OnHarvestedForage(farmer, obj);
                }
                else
                {
                    farmer.gainExperience(0, 5);
                }
                Game1.stats.ItemsForaged++;
            }
            location.objects.Remove(tile);

            return true;
        }
        return false;
    }
}

/// <summary>Harvest (destroy) stone</summary>
public sealed class HarvestStoneAbility(TrinketTinkerEffect effect, AbilityData data, int level)
    : BaseHarvestAbility<HarvestArgs>(effect, data, level)
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

        switch (args.HarvestTo)
        {
            case HarvestDestination.None:
                location.objects.Remove(tile);
                break;
            case HarvestDestination.Debris:
                location.destroyObject(tile, farmer);
                break;
            case HarvestDestination.Player:
            case HarvestDestination.TinkerInventory:
                void OnDebrisAdded(Debris debris)
                {
                    if (
                        debris.debrisType.Value == Debris.DebrisType.OBJECT
                        || debris.debrisType.Value == Debris.DebrisType.ARCHAEOLOGY
                    )
                    {
                        if (args.HarvestTo == HarvestDestination.Player)
                        {
                            if (debris.collect(farmer))
                                location.debris.Remove(debris);
                        }
                        else
                        {
                            debris.item = e.InvHandler.Value?.AddItem(debris.item);
                            if (debris.item == null)
                                location.debris.Remove(debris);
                        }
                    }
                }
                ;
                location.debris.OnValueAdded += OnDebrisAdded;
                location.destroyObject(tile, farmer);
                location.debris.OnValueAdded -= OnDebrisAdded;
                break;
        }

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

/// <summary>Harvest crops</summary>
public sealed class HarvestCropAbility(TrinketTinkerEffect effect, AbilityData data, int level)
    : BaseHarvestAbility<HarvestArgs>(effect, data, level)
{
    /// <inheritdocs/>
    protected override bool ProbeTile(GameLocation location, Vector2 tile)
    {
        return location.terrainFeatures.TryGetValue(tile, out TerrainFeature feature)
            && CheckCrop(feature, args.Filters);
    }

    /// <summary>Check if a terrain feature </summary>
    /// <param name="feature"></param>
    /// <param name="filters"></param>
    /// <returns></returns>
    public static bool CheckCrop(TerrainFeature feature, IList<string>? filters)
    {
        return feature is HoeDirt dirt
            && dirt.crop != null
            && dirt.crop.CanHarvest()
            && (filters == null || Places.CheckCropFilter(dirt.crop, filters));
    }

    /// <inheritdocs/>
    protected override bool DoHarvest(GameLocation location, Farmer farmer, Vector2 tile)
    {
        if (!(location.terrainFeatures.TryGetValue(tile, out TerrainFeature feature) && feature is HoeDirt dirt))
            return false;
        bool harvested = false;

        switch (args.HarvestTo)
        {
            case HarvestDestination.Debris:
                if (dirt.crop.harvest((int)tile.X, (int)tile.Y, dirt, null, true))
                {
                    dirt.destroyCrop(true);
                }
                harvested = true;
                break;
            case HarvestDestination.None:
            case HarvestDestination.Player:
            case HarvestDestination.TinkerInventory:
                void OnDebrisAdded(Debris debris)
                {
                    if (
                        debris.debrisType.Value == Debris.DebrisType.OBJECT
                        || debris.debrisType.Value == Debris.DebrisType.ARCHAEOLOGY
                    )
                    {
                        if (debris.collect(farmer))
                            location.debris.Remove(debris);
                        if (args.HarvestTo == HarvestDestination.Player)
                        {
                            if (debris.collect(farmer))
                                location.debris.Remove(debris);
                        }
                        else if (args.HarvestTo == HarvestDestination.TinkerInventory)
                        {
                            debris.item = e.InvHandler.Value?.AddItem(debris.item);
                            if (debris.item == null)
                                location.debris.Remove(debris);
                        }
                        else
                        {
                            location.debris.Remove(debris);
                        }
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
                break;
        }

        return harvested;
    }
}

/// <summary>Harvest shakeable tree/fruittree/bush</summary>
public sealed class HarvestShakeableAbility(TrinketTinkerEffect effect, AbilityData data, int level)
    : BaseHarvestAbility<HarvestArgs>(effect, data, level)
{
    public const string BUSH = "Bush";
    public const string TREE = "Tree";
    public const string FRUIT_TREE = "FruitTree";

    private static bool TryGetFeature(
        GameLocation location,
        Vector2 tile,
        [NotNullWhen(true)] out TerrainFeature? feature
    )
    {
        if (!location.terrainFeatures.TryGetValue(tile, out feature))
            feature = location.getLargeTerrainFeatureAt((int)tile.X, (int)tile.Y);
        return feature != null;
    }

    protected override bool ProbeTile(GameLocation location, Vector2 tile)
    {
        if (!TryGetFeature(location, tile, out TerrainFeature? feature))
            return false;
        return CheckShakeable(feature, args.Filters);
    }

    /// <summary>BBM doesn't patch readyForHarvest :u</summary>
    /// <param name="bush"></param>
    /// <returns></returns>
    // public static bool InBloomBBM(Bush bush) =>
    //     bush.modData.Keys.Any((key) => key.StartsWith("NCarigon.BushBloomMod/"));
    public static bool InBloomBBM(Bush bush) => bush.modData.ContainsKey("NCarigon.BushBloomMod/bush-schedule");

    /// <summary>Check if a terrain feature </summary>
    /// <param name="feature"></param>
    /// <param name="filters"></param>
    /// <returns></returns>
    public static bool CheckShakeable(TerrainFeature feature, IReadOnlyList<string>? filters)
    {
        return feature switch
        {
            // size 4 is walnut bushes, ban
            Bush bush => (filters?.Contains(BUSH) ?? true)
                && bush.size.Value != 4
                && (bush.readyForHarvest() || InBloomBBM(bush)),
            Tree tree => (filters?.Contains(TREE) ?? true)
                && tree.maxShake == 0f
                && tree.growthStage.Value >= 5
                && !tree.stump.Value
                && !tree.wasShakenToday.Value,
            FruitTree fruitTree => (filters?.Contains(FRUIT_TREE) ?? true) && fruitTree.fruit.Count > 0,
            _ => false,
        };
    }

    protected override bool DoHarvest(GameLocation location, Farmer farmer, Vector2 tile)
    {
        if (!TryGetFeature(location, tile, out TerrainFeature? feature))
            return false;

        Action<Vector2, bool>? shakeFunc = feature switch
        {
            Bush bush => bush.shake,
            Tree tree => tree.shake,
            FruitTree fruitTree => fruitTree.shake,
            _ => null,
        };
        if (shakeFunc == null)
            return false;

        bool harvested = false;

        if (args.HarvestTo == HarvestDestination.Debris)
        {
            shakeFunc(tile, false);
            harvested = true;
        }
        else
        {
            void OnDebrisAdded(Debris debris)
            {
                if (
                    debris.debrisType.Value == Debris.DebrisType.OBJECT
                    || debris.debrisType.Value == Debris.DebrisType.ARCHAEOLOGY
                )
                {
                    if (args.HarvestTo == HarvestDestination.Player)
                    {
                        if (debris.collect(farmer))
                            location.debris.Remove(debris);
                    }
                    else if (args.HarvestTo == HarvestDestination.TinkerInventory)
                    {
                        debris.item = e.InvHandler.Value?.AddItem(debris.item);
                        if (debris.item == null)
                            location.debris.Remove(debris);
                    }
                    else
                    {
                        location.debris.Remove(debris);
                    }
                    harvested = true;
                }
            }
            ;
            location.debris.OnValueAdded += OnDebrisAdded;
            shakeFunc(tile, false);
            location.debris.OnValueAdded -= OnDebrisAdded;
        }

        return harvested;
    }
}
