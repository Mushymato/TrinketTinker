using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Locations;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using TrinketTinker.Models.AbilityArgs;

namespace TrinketTinker.Wheels;

/// <summary>Helper methods dealing with location</summary>
internal static class Places
{
    /// <summary>Custom</summary>
    public const string Field_DisableTrinketAbilities = $"{ModEntry.ModId}/disableAbilities";
    public const string Field_DisableTrinketCompanions = $"{ModEntry.ModId}/disableCompanions";

    /// <summary>Find closest matching farm animal within range</summary>
    /// <param name="location"></param>
    /// <param name="originPoint"></param>
    /// <param name="range"></param>
    /// <param name="ignoreUntargetables"></param>
    /// <param name="match"></param>
    /// <returns></returns>
    public static FarmAnimal? ClosestMatchingFarmAnimal(
        GameLocation location,
        Vector2 originPoint,
        int range,
        Func<FarmAnimal, bool>? match = null
    )
    {
        FarmAnimal? result = null;
        float num = range + 1;
        foreach (FarmAnimal animal in location.animals.Values)
        {
            if (match == null || match(animal))
            {
                float num2 = Vector2.Distance(originPoint, animal.getStandingPosition());
                if (num2 <= range && num2 < num)
                {
                    result = animal;
                    num = num2;
                }
            }
        }
        return result;
    }

    /// <summary>Find closest matching NPC within range</summary>
    /// <param name="location"></param>
    /// <param name="originPoint"></param>
    /// <param name="range"></param>
    /// <param name="ignoreUntargetables"></param>
    /// <param name="match"></param>
    /// <returns></returns>
    public static NPC? ClosestMatchingCharacter(
        GameLocation location,
        Vector2 originPoint,
        int range,
        Func<NPC, bool>? match
    )
    {
        NPC? result = null;
        float num = range + 1;
        foreach (NPC character in location.characters)
        {
            if (match == null || match(character))
            {
                float num2 = Vector2.Distance(originPoint, character.getStandingPosition());
                if (num2 <= range && num2 < num && !character.IsInvisible)
                {
                    result = character;
                    num = num2;
                }
            }
        }
        return result;
    }

    /// <summary>Get all matching monsters within range.</summary>
    /// <param name="location"></param>
    /// <param name="originPoint"></param>
    /// <param name="range"></param>
    /// <param name="match"></param>
    /// <returns></returns>
    private static List<(float, Monster)> MatchingMonsters(
        GameLocation location,
        Vector2 originPoint,
        int range,
        Monster? guaranteedTarget,
        Func<Monster, bool>? match = null
    )
    {
        List<(float, Monster)> result = [];
        foreach (NPC character in location.characters)
        {
            if (character is not Monster monster || monster.IsInvisible)
                continue;
            if (guaranteedTarget == monster)
                continue;
            float distance = Vector2.Distance(originPoint, monster.getStandingPosition());
            if (distance > range)
                continue;

            if (match == null || match(monster))
                result.Add((distance, monster));
        }
        return result;
    }

    /// <summary>Select matching monsters within range according to target mode and max target count.</summary>
    /// <param name="location"></param>
    /// <param name="originPoint"></param>
    /// <param name="range"></param>
    /// <param name="maxTargetCount"></param>
    /// <param name="targetMode"></param>
    /// <param name="guaranteedTarget"></param>
    /// <param name="match"></param>
    /// <returns></returns>
    public static List<Monster> SelectMatchingMonsters(
        GameLocation location,
        Vector2 originPoint,
        int range,
        int maxTargetCount,
        TargetSelectionMode targetMode,
        Monster? guaranteedTarget = null,
        Func<Monster, bool>? match = null
    )
    {
        List<Monster> result = [];

        bool guaranteedValid = false;
        if (guaranteedTarget != null)
        {
            float guaranteedDistance = Vector2.Distance(originPoint, guaranteedTarget.getStandingPosition());
            guaranteedValid =
                !guaranteedTarget.IsInvisible
                && guaranteedDistance <= range
                && (match == null || match(guaranteedTarget));

            if (guaranteedValid)
                result.Add(guaranteedTarget);
            else
                guaranteedTarget = null;
        }

        List<(float, Monster)> candidates = MatchingMonsters(location, originPoint, range, guaranteedTarget, match);
        if (candidates.Count == 0)
            return result;

        int remainingCount = Math.Max(0, maxTargetCount - result.Count);

        switch (targetMode)
        {
            case TargetSelectionMode.Closest:
                candidates.Sort();
                break;
            case TargetSelectionMode.Random:
                ShuffleInPlace(Random.Shared, candidates);
                break;
        }

        if (remainingCount > 0 && remainingCount <= candidates.Count)
            result.AddRange(candidates.Take(remainingCount).Select(monster => monster.Item2));

        return result;
    }

    /// <summary>Shuffle a list in place using Fisher-Yates shuffle.</summary>
    /// <param name="rand"></param>
    /// <param name="listToShuffle"></param>
    private static void ShuffleInPlace<T>(Random rand, List<T> listToShuffle)
    {
        int n = listToShuffle.Count;
        while (n > 1)
        {
            n--;
            int k = rand.Next(n + 1);
            (listToShuffle[n], listToShuffle[k]) = (listToShuffle[k], listToShuffle[n]);
        }
    }

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

    /// <summary>Check object against filters (list of context tags), return false if matched</summary>
    /// <param name="obj"></param>
    /// <param name="filters"></param>
    /// <returns></returns>
    public static bool CheckContextTagFilter(Item item, IList<string> filters)
    {
        foreach (string tagLst in filters)
        {
            if (tagLst.Split(' ').All(item.HasContextTag))
                return false;
        }
        return true;
    }

    /// <summary>Check crop harvest object against filters (list of context tags), return false if matched</summary>
    /// <param name="obj"></param>
    /// <param name="filters"></param>
    /// <returns></returns>
    public static bool CheckCropFilter(Crop crop, IList<string> filters)
    {
        if (ItemRegistry.Create(crop.indexOfHarvest.Value, allowNull: true) is Item item)
        {
            return CheckContextTagFilter(item, filters);
        }
        return true;
    }

    /// <summary>
    /// Check that this is a digspot
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool IsDigSpot(this SObject obj)
    {
        return obj.QualifiedItemId == "(O)590" || obj.QualifiedItemId == "(O)SeedSpot";
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

    /// <summary>Filter on a string id against a list of string ids</summary>
    /// <param name="Filters"></param>
    /// <param name="checkId"></param>
    /// <returns></returns>
    public static bool FilterStringId(List<string>? Filters, string checkId)
    {
        if (Filters == null)
            return true;
        return Filters.Contains(checkId) && Filters[0] == "!";
    }

    /// <summary>Whether the debris should be redirected</summary>
    /// <param name="debris"></param>
    /// <returns></returns>
    internal static bool ShouldCollectDebris(Debris debris) =>
        debris.debrisType.Value
            is Debris.DebrisType.OBJECT
                or Debris.DebrisType.ARCHAEOLOGY
                or Debris.DebrisType.RESOURCE;

    /// <summary>Get debris item</summary>
    /// <param name="debris"></param>
    /// <returns></returns>
    internal static Item? GetDebrisItem(Debris debris)
    {
        if (debris.item != null)
            return debris.item;
        if (debris.debrisType.Value != 0 || debris.chunkType.Value != 8)
            return ItemRegistry.Create(debris.itemId.Value, 1, debris.itemQuality, allowNull: true);
        return null;
    }
}
