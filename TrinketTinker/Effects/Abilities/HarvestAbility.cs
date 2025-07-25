using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
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
    internal const int UPGRADE_LEVEL = 32;

    protected void HarvestTAS(Item? item, GameLocation location, Farmer farmer)
    {
        if (args.ShowHarvestedItem && item != null)
        {
            Vector2 tasPos;
            Vector2 tasOff;
            if (e.CompanionPosition is Vector2 cPos)
            {
                tasPos = cPos;
                tasOff = new Vector2(0, e.CompanionBoundingBox.Top - tasPos.Y);
            }
            else
            {
                tasPos = farmer.Position;
                tasOff = new Vector2(0, -128);
            }
            Visuals.BroadcastItemGetTAS(item, location, tasPos, tasOff);
        }
    }

    private void CollectDebrisToNone(Debris debris, GameLocation location)
    {
        location.debris.Remove(debris);
    }

    private void CollectDebrisToPlayer(Debris debris, GameLocation location, Farmer farmer)
    {
        Item? item = Places.GetDebrisItem(debris);
        if (debris.collect(farmer))
        {
            location.debris.Remove(debris);
            HarvestTAS(item, location, farmer);
        }
    }

    private void CollectDebrisToTinkerInventory(Debris debris, GameLocation location, Farmer farmer)
    {
        if (Places.GetDebrisItem(debris) is Item item)
        {
            debris.item = e.AddItemToInventory(item);
            if (debris.item == null)
            {
                location.debris.Remove(debris);
                HarvestTAS(item, location, farmer);
            }
        }
    }

    internal bool CollectDebris(HarvestDestination harvestTo, GameLocation location, Farmer farmer, Action harvestFunc)
    {
        if (harvestTo == HarvestDestination.Debris)
        {
            harvestFunc();
            return true;
        }

        bool harvested = false;
        Netcode.NetCollection<Debris>.ContentsChangeEvent? OnDebrisAdded = harvestTo switch
        {
            HarvestDestination.None => debris =>
            {
                if (Places.ShouldCollectDebris(debris))
                {
                    CollectDebrisToNone(debris, location);
                    harvested = true;
                }
            },
            HarvestDestination.Player => debris =>
            {
                if (Places.ShouldCollectDebris(debris))
                {
                    CollectDebrisToPlayer(debris, location, farmer);
                    harvested = true;
                }
            },
            HarvestDestination.TinkerInventory => debris =>
            {
                if (Places.ShouldCollectDebris(debris))
                {
                    CollectDebrisToTinkerInventory(debris, location, farmer);
                    harvested = true;
                }
            },
            _ => throw new NotImplementedException(),
        };
        location.debris.OnValueAdded += OnDebrisAdded;
        harvestFunc();
        location.debris.OnValueAdded -= OnDebrisAdded;
        return harvested;
    }

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
            harvested = DoHarvest(proc.LocationOrCurrent, proc.Farmer, tile) || harvested;
        return harvested && base.ApplyEffect(proc);
    }
}

/// <summary>Harvest stone, by using a fake pickaxe</summary>
public sealed class HarvestStoneAbility(TrinketTinkerEffect effect, AbilityData data, int level)
    : BaseHarvestAbility<HarvestArgs>(effect, data, level)
{
    private static readonly Lazy<Pickaxe> fakeTool =
        new(() =>
        {
            Pickaxe tool = new() { UpgradeLevel = UPGRADE_LEVEL };
            tool.isEfficient.Value = true;
            return tool;
        });

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

        return CollectDebris(
            args.HarvestTo,
            location,
            farmer,
            () =>
            {
                obj.MinutesUntilReady = 0;
                fakeTool.Value.lastUser = farmer;
                fakeTool.Value.DoFunction(location, (int)(tile.X * 64), (int)(tile.Y * 64), 1, farmer);
            }
        );
    }
}

/// <summary>Harvest twig</summary>
public sealed class HarvestTwigAbility(TrinketTinkerEffect effect, AbilityData data, int level)
    : BaseHarvestAbility<HarvestArgs>(effect, data, level)
{
    private static readonly Lazy<Axe> fakeTool =
        new(() =>
        {
            Axe tool = new() { UpgradeLevel = UPGRADE_LEVEL };
            tool.isEfficient.Value = true;
            return tool;
        });

    /// <inheritdocs/>
    protected override bool ProbeTile(GameLocation location, Vector2 tile)
    {
        return location.objects.TryGetValue(tile, out var obj) && obj.IsTwig();
    }

    /// <inheritdocs/>
    protected override bool DoHarvest(GameLocation location, Farmer farmer, Vector2 tile)
    {
        if (!location.objects.TryGetValue(tile, out SObject obj))
            return false;

        return CollectDebris(
            args.HarvestTo,
            location,
            farmer,
            () =>
            {
                fakeTool.Value.lastUser = farmer;
                if (obj.performToolAction(fakeTool.Value))
                {
                    obj.performRemoveAction();
                    location.Objects.Remove(tile);
                }
            }
        );
    }
}

/// <summary>Harvest weedses</summary>
public sealed class HarvestWeedAbility(TrinketTinkerEffect effect, AbilityData data, int level)
    : BaseHarvestAbility<HarvestArgs>(effect, data, level)
{
    /// <inheritdocs/>
    protected override bool ProbeTile(GameLocation location, Vector2 tile)
    {
        return location.objects.TryGetValue(tile, out var obj) && obj.IsWeeds();
    }

    /// <inheritdocs/>
    protected override bool DoHarvest(GameLocation location, Farmer farmer, Vector2 tile)
    {
        if (!location.objects.TryGetValue(tile, out SObject obj))
            return false;

        return CollectDebris(
            args.HarvestTo,
            location,
            farmer,
            () =>
            {
                obj.cutWeed(farmer);
                obj.performRemoveAction();
                location.Objects.Remove(tile);
            }
        );
    }
}

/// <summary>Harvest dig spot</summary>
public sealed class HarvestDigSpotAbility(TrinketTinkerEffect effect, AbilityData data, int level)
    : BaseHarvestAbility<HarvestArgs>(effect, data, level)
{
    private static readonly Lazy<Hoe> fakeTool =
        new(() =>
        {
            Hoe tool = new() { UpgradeLevel = UPGRADE_LEVEL };
            return tool;
        });

    /// <inheritdocs/>
    protected override bool ProbeTile(GameLocation location, Vector2 tile)
    {
        return location.objects.TryGetValue(tile, out var obj) && obj.IsDigSpot();
    }

    /// <inheritdocs/>
    protected override bool DoHarvest(GameLocation location, Farmer farmer, Vector2 tile)
    {
        if (!location.objects.TryGetValue(tile, out SObject obj))
            return false;

        return CollectDebris(
            args.HarvestTo,
            location,
            farmer,
            () =>
            {
                fakeTool.Value.lastUser = farmer;
                obj.performToolAction(fakeTool.Value);
            }
        );
    }
}

/// <summary>Harvest forage (and other kinds of spawn item)</summary>
public sealed class HarvestForageAbility(TrinketTinkerEffect effect, AbilityData data, int level)
    : BaseHarvestAbility<HarvestArgs>(effect, data, level)
{
    /// <inheritdocs/>
    protected override bool ProbeTile(GameLocation location, Vector2 tile)
    {
        if (Name.Equals("LewisBasement"))
            return false;
        return location.objects.TryGetValue(tile, out SObject obj) && IsSpawnedItem(obj, args.Filters);
    }

    /// <summary>Check if an object is forage</summary>
    /// <param name="obj"></param>
    /// <param name="filters"></param>
    /// <returns></returns>
    public static bool IsSpawnedItem(SObject obj, IList<string>? filters)
    {
        return obj.IsSpawnedObject && (filters == null || Places.CheckContextTagFilter(obj, filters));
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
            HarvestDestination.TinkerInventory => e.CanAcceptThisItem(obj),
            _ => true,
        };
    }

    /// <inheritdocs/>
    protected override bool DoHarvest(GameLocation location, Farmer farmer, Vector2 tile)
    {
        if (
            location.objects.TryGetValue(tile, out SObject obj)
            && obj.IsSpawnedObject
            && !obj.questItem.Value
            && CanTargetAccept(farmer, obj)
        )
        {
            bool isForage = obj.isForage();
            obj.Quality = location.GetHarvestSpawnedObjectQuality(farmer, isForage, obj.TileLocation, Random.Shared);

            if (args.HarvestTo != HarvestDestination.None)
            {
                Action harvestMethod;
                if (args.HarvestTo == HarvestDestination.Debris)
                    harvestMethod = () =>
                        Game1.createItemDebris(obj.getOne(), new Vector2(tile.X * 64f + 32f, tile.Y * 64f + 32f), -1);
                else if (args.HarvestTo == HarvestDestination.TinkerInventory)
                    harvestMethod = () =>
                    {
                        if (e.AddItemToInventory(obj.getOne()) is Item item)
                        {
                            Game1.createItemDebris(item, new Vector2(tile.X * 64f + 32f, tile.Y * 64f + 32f), -1);
                        }
                        else
                        {
                            HarvestTAS(obj, location, farmer);
                        }
                    };
                else
                    harvestMethod = () =>
                    {
                        if (farmer.addItemToInventory(obj.getOne()) is Item item)
                        {
                            Game1.createItemDebris(item, new Vector2(tile.X * 64f + 32f, tile.Y * 64f + 32f), -1);
                        }
                        else
                        {
                            HarvestTAS(obj, location, farmer);
                        }
                    };
                harvestMethod();
                if (!location.isFarmBuildingInterior() && isForage)
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
                    if (isForage)
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

        return CollectDebris(
            args.HarvestTo,
            location,
            farmer,
            () =>
            {
                if (dirt.crop.harvest((int)tile.X, (int)tile.Y, dirt, null, true))
                    dirt.destroyCrop(true);
            }
        );
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
                && bush.inBloom()
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

        return CollectDebris(args.HarvestTo, location, farmer, () => shakeFunc(tile, false));
    }
}
