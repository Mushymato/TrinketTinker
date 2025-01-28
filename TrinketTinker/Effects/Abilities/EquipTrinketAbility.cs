using StardewValley;
using StardewValley.Inventories;
using StardewValley.Objects.Trinkets;
using TrinketTinker.Effects.Support;
using TrinketTinker.Models;
using TrinketTinker.Models.Mixin;
using TrinketTinker.Wheels;

namespace TrinketTinker.Effects.Abilities;

/// <summary>Equips trinkets held in the inventory.</summary>
public sealed class EquipTrinketAbility(TrinketTinkerEffect effect, AbilityData data, int lvl)
    : Ability<NoArgs>(effect, data, lvl)
{
    /// <summary>Apply or refreshes the buff.</summary>
    /// <param name="proc"></param>
    /// <returns></returns>
    protected override bool ApplyEffect(ProcEventArgs proc)
    {
        if (proc.Proc != ProcOn.Always)
        {
            ModEntry.LogOnce(
                $"EquipTrinket can only be used with Proc Always (from {e.Trinket.QualifiedItemId})",
                StardewModdingAPI.LogLevel.Warn
            );
            return false;
        }
        if (e.GetInventory(proc.Farmer) is not Inventory trinketInv)
            return false;
        foreach (Item item in trinketInv)
        {
            if (
                item == null
                || item is not Trinket trinket
                || (
                    (
                        trinket
                            .GetTrinketData()
                            ?.CustomFields?.TryGetValue(
                                TinkerConst.CustomFields_DirectEquipOnly,
                                out string? directOnly
                            ) ?? false
                    )
                    && directOnly != null
                )
            )
                continue;
            var trinketItems = proc.Farmer.trinketItems;
            if (trinketItems.Contains(trinket))
                continue;
            while (trinketItems.Count < 2)
                trinketItems.Add(null);
            proc.Farmer.trinketItems.Insert(2, trinket);
            trinket.modData[TinkerConst.ModData_IndirectEquip] = "T";
        }
        return base.ApplyEffect(proc);
    }

    /// <summary>Removes the buff.</summary>
    /// <param name="farmer"></param>
    /// <returns></returns>
    protected override void CleanupEffect(Farmer farmer)
    {
        if (e.GetInventory(farmer) is not Inventory trinketInv)
            return;
        foreach (Item item in trinketInv)
        {
            if (item == null || item is not Trinket trinket)
                continue;
            farmer.trinketItems.Remove(trinket);
            trinket.modData.Remove(TinkerConst.ModData_IndirectEquip);
        }
        base.CleanupEffect(farmer);
    }
}
