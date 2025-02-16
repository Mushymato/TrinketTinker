using StardewModdingAPI;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Objects.Trinkets;
using TrinketTinker.Effects.Support;
using TrinketTinker.Extras;
using TrinketTinker.Models;
using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Effects.Abilities;

/// <summary>Equips trinkets held in the inventory.</summary>
public sealed class EquipTrinketAbility : Ability<NoArgs>
{
    public EquipTrinketAbility(TrinketTinkerEffect effect, AbilityData data, int lvl)
        : base(effect, data, lvl)
    {
        if (data.Proc != ProcOn.Always)
        {
            ModEntry.LogOnce(
                $"EquipTrinket can only be used with Proc=Always (from {e.Trinket.QualifiedItemId})",
                LogLevel.Warn
            );
            Valid = false;
        }
        else if (e.InventoryId == null)
        {
            ModEntry.LogOnce(
                $"EquipTrinketAbility requires Inventory to use, ({Name}, from {e.Trinket.QualifiedItemId})",
                LogLevel.Warn
            );
            Valid = false;
        }
    }

    /// <summary>Apply or refreshes the buff.</summary>
    /// <param name="proc"></param>
    /// <returns></returns>
    protected override bool ApplyEffect(ProcEventArgs proc)
    {
        if (EquipTrinket.Resizing)
            return false;
        if (e.GetInventory() is not Inventory trinketInv)
            return false;
        foreach (Item item in trinketInv)
        {
            if (item == null || item is not Trinket trinket)
                continue;
            EquipTrinket.Equip(proc.Farmer, trinket);
        }
        return base.ApplyEffect(proc);
    }

    /// <summary>Removes the buff.</summary>
    /// <param name="farmer"></param>
    /// <returns></returns>
    protected override void CleanupEffect(Farmer farmer)
    {
        if (EquipTrinket.Resizing)
            return;
        if (e.GetInventory() is not Inventory trinketInv)
            return;
        foreach (Item item in trinketInv.Reverse())
        {
            if (item == null || item is not Trinket trinket)
                continue;
            EquipTrinket.Unequip(farmer, trinket);
        }
        base.CleanupEffect(farmer);
    }
}
