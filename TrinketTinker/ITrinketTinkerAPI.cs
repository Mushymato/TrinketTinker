using System.Diagnostics.CodeAnalysis;
using StardewValley.Objects.Trinkets;

namespace TrinketTinker;

/// <summary>
// C# facing API for Trinket Tinker
// NOTE: for most usecases it is better to interact with trinket tinker via trigger actions/GSQ/item queries as this is primarily a data facing framework
// This API mainly serves niche cases and I will list out equivalent non API code when available
// </summary>
public interface ITrinketTinkerAPI
{
    /// <summary>
    /// Attempts to equip a trinket through means outside of the slot.
    /// This analogous to trigger action mushymato.TrinketTinker_EquipHiddenTrinket, but accepts a real and existing trinket instance.
    /// </summary>
    /// <param name="trinket">trinket to equip, does not have to be a trinket tinker trinket</param>
    /// <param name="guid">a generated string ID that can be used to unequip the trinket, retain this for TryUnequipHiddenTrinket</param>
    /// <returns></returns>
    public bool TryEquipHiddenTrinket(Trinket trinket, [NotNullWhen(true)] out string? guid);

    /// <summary>
    /// Attempts to unequip a trinket equipped via TryEquipHiddenTrinket
    /// This analogous to trigger action mushymato.TrinketTinker_UnequipHiddenTrinket, but requires the guid given by TryEquipHiddenTrinket
    /// </summary>
    /// <param name="guid">the guid from TryEquipHiddenTrinket that you should manage for unequip</param>
    /// <param name="trinket">trinket that was equipped, returned if successfully unequipped</param>
    /// <returns></returns>
    public bool TryUnequipHiddenTrinket(string guid, [NotNullWhen(true)] out Trinket? trinket);
}
