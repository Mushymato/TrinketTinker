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
        if (trinket.GetEffect() is TrinketTinkerEffect effect && !effect.CheckEnabled(farmer))
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
            !ArgUtility.TryGet(args, 1, out string trinketId, out error, allowBlank: false, name: "string trinketId")
            || !ArgUtility.TryGetOptionalInt(args, 2, out int level, out error, name: "int level")
            || !ArgUtility.TryGetOptionalInt(args, 3, out int variant, out error, name: "int variant")
            || !ArgUtility.TryGetOptionalInt(
                args,
                4,
                out int daysDuration,
                out error,
                defaultValue: 1,
                name: "int daysDuration"
            )
        )
        {
            ModEntry.Log(error, LogLevel.Error);
            return false;
        }
        if (daysDuration < 1)
            daysDuration = -1;

        if (ItemRegistry.Create(trinketId, allowNull: false) is Trinket trinket)
        {
            if (trinket.GetEffect() is TrinketTinkerEffect effect)
            {
                effect.SetLevel(trinket, level);
                effect.SetVariant(trinket, variant);
            }
            if (Equip(Game1.player, trinket))
            {
                trinket.modData[TinkerConst.ModData_HiddenEquip] = daysDuration.ToString();
                hiddenTrinketsInv.Value.Add(trinket);
            }
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
        if (
            !ArgUtility.TryGet(args, 1, out string trinketId, out error, allowBlank: false, "string trinketId")
            || !ArgUtility.TryGetOptionalInt(args, 2, out int level, out error, name: "int level")
            || !ArgUtility.TryGetOptionalInt(args, 3, out int variant, out error, name: "int variant")
        )
        {
            ModEntry.Log(error, LogLevel.Error);
            return false;
        }

        foreach (Item item in hiddenTrinketsInv.Value.Reverse())
        {
            if (item is Trinket trinket && (trinket.QualifiedItemId == trinketId || trinket.ItemId == trinketId))
            {
                if (
                    trinket.GetEffect() is TrinketTinkerEffect effect
                    && (effect.Level != level || effect.Variant != variant)
                )
                    continue;
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
                    if (decrement && daysDuration != -1)
                        daysDuration--;
                    if (Unequip(Game1.player, trinket))
                    {
                        if (daysDuration == 0)
                        {
                            trinket.modData.Remove(TinkerConst.ModData_HiddenEquip);
                            hiddenTrinketsInv.Value.Remove(trinket);
                            ModEntry.Log($"{trinket.QualifiedItemId} expired");
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
