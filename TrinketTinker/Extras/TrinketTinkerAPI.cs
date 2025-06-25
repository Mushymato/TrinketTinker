using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects.Trinkets;
using TrinketTinker.Wheels;

namespace TrinketTinker.Extras;

public sealed class TrinketTinkerAPI : ITrinketTinkerAPI
{
    public bool TryEquipHiddenTrinket(Trinket trinket, [NotNullWhen(true)] out string? guid)
    {
        guid = null;
        if (!Context.IsWorldReady)
            return false;

        if (trinket == null)
            return false;
        guid = $"{ModEntry.ModId}+API+{Game1.player.UniqueMultiplayerID}/{Guid.NewGuid()}";
        trinket.modData[TinkerConst.ModData_IndirectEquipFromAPI] = guid;
        if (EquipTrinket.Equip(Game1.player, trinket))
        {
            return true;
        }
        guid = null;
        trinket.modData.Remove(guid);
        return false;
    }

    public bool TryUnequipHiddenTrinket(string guid, [NotNullWhen(true)] out Trinket? trinket)
    {
        trinket = null;
        if (!Context.IsWorldReady)
            return false;

        if (guid == null)
            return false;
        foreach (Trinket trinketItem in Game1.player.trinketItems)
        {
            if (
                trinketItem != null
                && trinketItem.modData.TryGetValue(TinkerConst.ModData_IndirectEquipFromAPI, out string? existingGuid)
                && guid == existingGuid
            )
            {
                if (EquipTrinket.Unequip(Game1.player, trinketItem))
                {
                    trinketItem.modData.Remove(TinkerConst.ModData_IndirectEquipFromAPI);
                    trinket = trinketItem;
                    return true;
                }
            }
        }
        return false;
    }
}
