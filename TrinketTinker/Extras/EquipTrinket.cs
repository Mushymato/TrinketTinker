using System.Reflection;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Companions;
using StardewValley.Delegates;
using StardewValley.Inventories;
using StardewValley.Objects.Trinkets;
using TrinketTinker.Effects;
using TrinketTinker.Wheels;
using TrinketList = Netcode.NetList<
    StardewValley.Objects.Trinkets.Trinket,
    Netcode.NetRef<StardewValley.Objects.Trinkets.Trinket>
>;

namespace TrinketTinker.Extras;

public static class EquipTrinket
{
    /// <summary>Global inventory for holding all hidden trinkets</summary>
    private static readonly Lazy<Inventory> hiddenTrinketsInv =
        new(
            Game1.player.team.GetOrCreateGlobalInventory(
                $"{ModEntry.ModId}+{Game1.player.UniqueMultiplayerID}/HiddenTrinkets"
            )
        );

    /// <summary>Equip hidden trinket action name</summary>
    public static string Action_EquipHiddenTrinket => $"{ModEntry.ModId}_EquipHiddenTrinket";

    /// <summary>Unequip trinket action name</summary>
    public static string Action_UnequipHiddenTrinket => $"{ModEntry.ModId}_UnequipHiddenTrinket";

    private static readonly MethodInfo ResizeMethod = typeof(TrinketList).GetMethod(
        "Resize",
        BindingFlags.NonPublic | BindingFlags.Instance
    )!;

    /// <summary>When resizing trinketItems, do not reapply <seealso cref="Effects.Abilities.EquipTrinketAbility"/> </summary>
    internal static bool Resizing = false;

    private static void ResizeTrinketItems(TrinketList trinketItems, int capacity)
    {
        ModEntry.Log($"ResizeTrinketItems {trinketItems.Capacity} => {capacity}");
        Resizing = true;
        ResizeMethod.Invoke(trinketItems, [capacity]);
        Resizing = false;
    }

    private static void AddTrinket(TrinketList trinketItems, Trinket trinket)
    {
        int skipTo = ModEntry.HasWearMoreRings ? 2 : 1;
        while (trinketItems.Count < skipTo)
            trinketItems.Add(null!);
        if (trinketItems.Capacity <= trinketItems.Count)
        {
            ResizeTrinketItems(trinketItems, trinketItems.Capacity * 2);
        }
        trinketItems.Add(trinket);
    }

    internal static bool Equip(Farmer owner, Trinket trinket)
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
        var trinketItems = owner.trinketItems;
        if (trinketItems.Contains(trinket))
            return false;
        else if (trinket.GetEffect() is TrinketEffect effect2 && effect2.Companion != null)
        {
            effect2.Companion.CleanupCompanion();
            effect2.Companion = null;
        }

        AddTrinket(trinketItems, trinket);
        trinket.modData[TinkerConst.ModData_IndirectEquip] = "T";
        return true;
    }

    internal static bool Unequip(Farmer owner, Trinket trinket)
    {
        if (owner.trinketItems.Remove(trinket))
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
            || !ArgUtility.TryGetOptionalInt(args, 2, out int level, out error, defaultValue: -1, name: "int level")
            || !ArgUtility.TryGetOptionalInt(args, 3, out int variant, out error, defaultValue: -1, name: "int variant")
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
                    && ((level != -1 && effect.Level != level) || (variant != -1 && effect.Variant != variant))
                )
                    continue;
                if (Unequip(Game1.player, trinket))
                {
                    trinket.modData.Remove(TinkerConst.ModData_HiddenEquip);
                    hiddenTrinketsInv.Value.Remove(trinket);
                    hiddenTrinketsInv.Value.RemoveEmptySlots();
                    var team = Game1.player.team;
                    if (
                        trinket.GetEffect() is TrinketTinkerEffect effect2
                        && effect2.GetInventory(Game1.player) is Inventory trinketInv
                    )
                    {
                        trinketInv.RemoveEmptySlots();
                        foreach (var item2 in trinketInv)
                            team.returnedDonations.Add(item2);
                        team.globalInventories.Remove(effect2.FullInventoryId);
                    }
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
    internal static void UnequipHiddenTrinkets(bool decrement = true)
    {
        // hidden trinkets
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

    internal static void ReequipHiddenTrinkets()
    {
        foreach (Item item in hiddenTrinketsInv.Value)
        {
            if (item is Trinket trinket)
            {
                Equip(Game1.player, trinket);
            }
        }
    }

    internal static void FixVanillaDupeCompanions()
    {
        // deal with vanilla dupe companions
        List<Companion> validCompanions = [];
        foreach (Trinket trinket in Game1.player.trinketItems)
        {
            if (trinket?.GetEffect()?.Companion is Companion cmp)
            {
                validCompanions.Add(cmp);
            }
        }
        foreach (Companion cmp in Game1.player.companions.Reverse())
        {
            if (!validCompanions.Contains(cmp))
            {
                Game1.player.RemoveCompanion(cmp);
            }
        }
    }
}
