using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Locations;
using StardewValley.TerrainFeatures;

namespace TrinketTinker.Wheels;

/// <summary>Helper methods dealing with location</summary>
internal static class Places
{
    /// <summary>Custom</summary>
    public static readonly string Field_DisableTrinketAbilities = $"{ModEntry.ModId}/disableAbilities";
    public static readonly string Field_DisableTrinketCompanions = $"{ModEntry.ModId}/disableCompanions";

    /// <summary>Find nearest placed object within range.</summary>
    /// <param name="location">Current location</param>
    /// <param name="originPoint">Position to search from</param>
    /// <param name="range">Pixel range</param>
    /// <param name="match">Filter predicate</param>
    /// <returns></returns>
    public static SObject? ClosestMatchingObject(
        GameLocation location,
        Vector2 originPoint,
        int range,
        Func<SObject, bool>? match
    )
    {
        SObject? result = null;
        float minDistance = range + 1;
        foreach (Vector2 tile in location.Objects.Keys)
        {
            SObject obj = location.Objects[tile];
            if (match == null || match(obj))
            {
                float distance = Vector2.Distance(originPoint, tile * Game1.tileSize);
                if (distance <= range && distance < minDistance)
                {
                    result = obj;
                }
            }
        }
        return result;
    }

    /// <summary>Find nearest terrain feature within range.</summary>
    /// <param name="location">Current location</param>
    /// <param name="originPoint">Position to search from</param>
    /// <param name="range">Pixel range</param>
    /// <param name="match">Filter predicate</param>
    /// <returns></returns>
    public static TerrainFeature? ClosestMatchingTerrainFeature(
        GameLocation location,
        Vector2 originPoint,
        int range,
        bool includeLarge,
        Func<TerrainFeature, bool>? match
    )
    {
        TerrainFeature? result = null;
        float minDistance = range + 1;
        // large terrain features
        if (includeLarge)
        {
            foreach (TerrainFeature terrain in location.largeTerrainFeatures)
            {
                if (match == null || match(terrain))
                {
                    float distance = Vector2.Distance(originPoint, terrain.Tile * Game1.tileSize);
                    if (distance <= range && distance < minDistance)
                    {
                        result = terrain;
                    }
                }
            }
        }
        // terrain features
        foreach (Vector2 tile in location.terrainFeatures.Keys)
        {
            TerrainFeature terrain = location.terrainFeatures[tile];
            if (match == null || match(terrain))
            {
                float distance = Vector2.Distance(originPoint, tile * Game1.tileSize);
                if (distance <= range && distance < minDistance)
                {
                    result = terrain;
                }
            }
        }
        // Special: check the hoedirt inside a garden pot
        return result;
    }

    /// <summary>Check that a crop can be harvested</summary>
    /// <param name="crop"></param>
    /// <returns></returns>
    public static bool CanHarvest(this Crop crop)
    {
        return !crop.dead.Value
            && (crop.currentPhase.Value >= (crop.phaseDays.Count - 1))
            && (!crop.fullyGrown.Value || crop.dayOfCurrentPhase.Value <= 0);
    }

    /// <summary>Check object against filters (list of context tags)</summary>
    /// <param name="obj"></param>
    /// <param name="filters"></param>
    /// <returns></returns>
    public static bool CheckContextTagFilter(Item item, IReadOnlyList<string> filters)
    {
        foreach (string tagLst in filters)
        {
            if (tagLst.Split(' ').All(item.HasContextTag))
                return false;
        }
        return true;
    }

    /// <summary>Check crop harvest object against filters (list of context tags)</summary>
    /// <param name="obj"></param>
    /// <param name="filters"></param>
    /// <returns></returns>
    public static bool CheckCropFilter(Crop crop, IReadOnlyList<string> filters)
    {
        if (ItemRegistry.Create(crop.indexOfHarvest.Value, allowNull: true) is Item item)
        {
            return CheckContextTagFilter(item, filters);
        }
        return true;
    }

    /// <summary>Disable all trinket abilities in certain locations</summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public static bool LocationDisableTrinketAbilities(GameLocation? location)
    {
        if (location?.GetData() is not LocationData data || data.CustomFields == null)
            return false;
        return data.CustomFields.TryGetValue(Field_DisableTrinketAbilities, out string? value)
            && value.EqualsIgnoreCase("true");
    }

    /// <summary>Hide all trinket companions in certain locations</summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public static bool LocationDisableTrinketCompanions(GameLocation? location)
    {
        if (location == null || (location.DisplayName == "Temp" && !Game1.isFestival()))
            return true;
        if (location.GetData() is not LocationData data)
            return false;
        return (data.CustomFields?.TryGetValue(Field_DisableTrinketCompanions, out string? value) ?? false)
            && value.EqualsIgnoreCase("true");
    }
}
