using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Inventories;
using StardewValley.Objects.Trinkets;
using TrinketTinker.Wheels;

namespace TrinketTinker.Effects.Support;

public static class EquipTrinket
{
    private static readonly Lazy<Inventory> hiddenTrinketsInv =
        new(
            Game1.player.team.GetOrCreateGlobalInventory(
                $"{ModEntry.ModId}+{Game1.player.UniqueMultiplayerID}/HiddenTrinkets"
            )
        );
    public static string Action_EquipHiddenTrinket => $"{ModEntry.ModId}_EquipHiddenTrinket";
    public static string Action_UnequipHiddenTrinket => $"{ModEntry.ModId}_UnequipHiddenTrinket";

    internal static bool Equip(Farmer farmer, Trinket trinket)
    {
        if (
            (
                trinket
                    .GetTrinketData()
                    ?.CustomFields?.TryGetValue(TinkerConst.CustomFields_DirectEquipOnly, out string? directOnly)
                ?? false
            )
            && directOnly != null
        )
            return false;
        var trinketItems = farmer.trinketItems;
        if (trinketItems.Contains(trinket))
            return false;
        // wear more rings compat
        if (ModEntry.HasWearMoreRings)
        {
            while (trinketItems.Count < 2)
                trinketItems.Add(null);
            farmer.trinketItems.Insert(2, trinket);
        }
        else
        {
            farmer.trinketItems.Add(trinket);
        }
        trinket.modData[TinkerConst.ModData_IndirectEquip] = "T";
        return true;
    }

    internal static bool Unequip(Farmer farmer, Trinket trinket)
    {
        if (farmer.trinketItems.Remove(trinket))
        {
            trinket.modData.Remove(TinkerConst.ModData_IndirectEquip);
            return true;
        }
        return false;
    }

    public static bool EquipHiddenTrinket(string[] args, TriggerActionContext context, out string error)
    {
        if (
            !ArgUtility.TryGet(args, 1, out string trinketId, out error, allowBlank: false, "string trinketId")
            || !ArgUtility.TryGetOptionalInt(
                args,
                2,
                out int daysDuration,
                out error,
                defaultValue: -1,
                name: "int daysDuration"
            )
        )
        {
            ModEntry.Log(error, LogLevel.Error);
            return false;
        }

        if (ItemRegistry.Create(trinketId, allowNull: false) is Trinket trinket && Equip(Game1.player, trinket))
        {
            trinket.modData[TinkerConst.ModData_HiddenEquip] = daysDuration.ToString();
            hiddenTrinketsInv.Value.Add(trinket);
        }
        hiddenTrinketsInv.Value.RemoveEmptySlots();

        return true;
    }

    public static bool UnequipHiddenTrinket(string[] args, TriggerActionContext context, out string error)
    {
        if (hiddenTrinketsInv.Value.Count == 0)
        {
            error = "No equipped temporary trinkets.";
            return true;
        }
        if (!ArgUtility.TryGet(args, 1, out string trinketId, out error, allowBlank: false, "string trinketId"))
        {
            ModEntry.Log(error, LogLevel.Error);
            return false;
        }

        foreach (Item item in hiddenTrinketsInv.Value.Reverse())
        {
            if (item is Trinket trinket && (trinket.QualifiedItemId == trinketId || trinket.ItemId == trinketId))
            {
                if (Unequip(Game1.player, trinket))
                {
                    trinket.modData.Remove(TinkerConst.ModData_HiddenEquip);
                    hiddenTrinketsInv.Value.Remove(trinket);
                    hiddenTrinketsInv.Value.RemoveEmptySlots();
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Remove all hidden trinkets before save. This is because the trinket list can get reordered on reload (and expose the hidden trinket)
    /// </summary>
    /// <param name="decrement">indicates that this is called from day ending, decrement count by 1</param>
    internal static void DayEndingRemove(bool decrement = true)
    {
        foreach (Item item in hiddenTrinketsInv.Value.Reverse())
        {
            if (item is Trinket trinket)
            {
                if (
                    trinket.modData.TryGetValue(TinkerConst.ModData_HiddenEquip, out string daysDurationStr)
                    && int.TryParse(daysDurationStr, out int daysDuration)
                )
                {
                    ModEntry.Log($"{trinket.QualifiedItemId}: {daysDurationStr}");
                    if (decrement && daysDuration != -1)
                        daysDuration--;
                    if (Unequip(Game1.player, trinket))
                    {
                        if (daysDuration == 0)
                        {
                            trinket.modData.Remove(TinkerConst.ModData_HiddenEquip);
                            hiddenTrinketsInv.Value.Remove(trinket);
                        }
                        else
                        {
                            trinket.modData[TinkerConst.ModData_HiddenEquip] = daysDuration.ToString();
                        }
                    }
                }
                else
                {
                    hiddenTrinketsInv.Value.Remove(trinket);
                }
            }
        }
        hiddenTrinketsInv.Value.RemoveEmptySlots();
    }

    internal static void DayStartedEquip()
    {
        foreach (Item item in hiddenTrinketsInv.Value)
        {
            if (item is Trinket trinket)
            {
                Equip(Game1.player, trinket);
            }
        }
    }
}
