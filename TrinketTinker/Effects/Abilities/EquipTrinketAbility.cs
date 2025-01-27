using StardewValley;
using StardewValley.Inventories;
using StardewValley.Objects.Trinkets;
using TrinketTinker.Effects.Support;
using TrinketTinker.Models;
using TrinketTinker.Models.Mixin;

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
        ModEntry.Log("EquipTrinketAbility:ApplyEffect");
        if (e.GetInventory(proc.Farmer) is not Inventory trinketInv)
            return false;
        foreach (Item item in trinketInv)
        {
            if (item == null || item is not Trinket trinket)
                continue;
            ModEntry.Log($"{trinket.QualifiedItemId} ({trinket.generationSeed})");
            if (proc.Farmer.trinketItems.Contains(trinket))
                continue;
            ModEntry.Log($"ADD {trinket.QualifiedItemId} ({trinket.generationSeed})");
            proc.Farmer.trinketItems.Add(trinket);
        }
        return base.ApplyEffect(proc);
    }

    /// <summary>Removes the buff.</summary>
    /// <param name="farmer"></param>
    /// <returns></returns>
    protected override void CleanupEffect(Farmer farmer)
    {
        ModEntry.Log("EquipTrinketAbility:CleanupEffect");
        if (e.GetInventory(farmer) is not Inventory trinketInv)
            return;
        foreach (Item item in trinketInv)
        {
            if (item == null || item is not Trinket trinket)
                continue;
            ModEntry.Log($"REMOVE {trinket.QualifiedItemId} ({trinket.generationSeed})");
            farmer.trinketItems.Remove(trinket);
        }
        base.CleanupEffect(farmer);
    }
}
