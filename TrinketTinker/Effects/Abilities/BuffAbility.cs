using StardewValley;
using TrinketTinker.Effects.Support;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;

namespace TrinketTinker.Effects.Abilities;

/// <summary>Applies a buff on proc.</summary>
public sealed class BuffAbility(TrinketTinkerEffect effect, AbilityData data, int lvl)
    : Ability<BuffArgs>(effect, data, lvl)
{
    private string? randAppliedBuff = null;

    /// <summary>Apply or refreshes the buff.</summary>
    /// <param name="proc"></param>
    /// <returns></returns>
    protected override bool ApplyEffect(ProcEventArgs proc)
    {
        // Buff(string id, string source = null, string displaySource = null, int duration = -1, Texture2D iconTexture = null, int iconSheetIndex = -1, BuffEffects effects = null, bool? isDebuff = null, string displayName = null, string description = null)
        if (args.BuffIdList != null)
        {
            if (args.Mode == BuffApplyMode.All)
            {
                foreach (string buffId in args.BuffIdList)
                {
                    proc.Farmer.applyBuff(buffId);
                }
            }
            else if (args.Mode == BuffApplyMode.Random)
            {
                int idx = Random.Shared.Next(args.BuffIdList.Count);
                randAppliedBuff = args.BuffIdList[idx];
                proc.Farmer.applyBuff(randAppliedBuff);
            }
        }
        else
        {
            proc.Farmer.applyBuff(args.BuffId);
        }
        return base.ApplyEffect(proc);
    }

    /// <summary>Removes the buff.</summary>
    /// <param name="farmer"></param>
    /// <returns></returns>
    protected override void CleanupEffect(Farmer farmer)
    {
        if (args.BuffIdList != null)
        {
            if (args.Mode == BuffApplyMode.All)
            {
                foreach (string buffId in args.BuffIdList)
                {
                    farmer.buffs.Remove(buffId);
                }
            }
            else if (args.Mode == BuffApplyMode.Random)
            {
                farmer.buffs.Remove(randAppliedBuff);
                randAppliedBuff = null;
            }
        }
        else
        {
            farmer.buffs.Remove(args.BuffId);
        }
        base.CleanupEffect(farmer);
    }
}
