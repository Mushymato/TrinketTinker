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

    public bool TryUnequipHiddenTrinket(string guid)
    {
        if (!Context.IsWorldReady)
            return false;

        if (guid == null)
            return false;
        foreach (Trinket trinket in Game1.player.trinketItems)
        {
            if (
                trinket != null
                && trinket.modData.TryGetValue(TinkerConst.ModData_IndirectEquipFromAPI, out string? existingGuid)
                && guid == existingGuid
            )
            {
                if (EquipTrinket.Unequip(Game1.player, trinket))
                {
                    trinket.modData.Remove(TinkerConst.ModData_IndirectEquipFromAPI);
                    return true;
                }
            }
        }
        return false;
    }
}
