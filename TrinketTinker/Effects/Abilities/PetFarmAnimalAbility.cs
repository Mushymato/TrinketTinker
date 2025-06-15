using StardewValley;
using TrinketTinker.Effects.Support;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;
using TrinketTinker.Wheels;

namespace TrinketTinker.Effects.Abilities;

/// <summary>Pets a farm animal :).</summary>
public sealed class PetFarmAnimalAbility(TrinketTinkerEffect effect, AbilityData data, int lvl)
    : Ability<PosRangeArgs>(effect, data, lvl)
{
    /// <summary>
    /// Check that this is a farm animal in need of petting
    /// </summary>
    /// <param name="chara"></param>
    /// <returns></returns>
    internal static bool IsFarmAnimalInNeedOfPetting(Character chara)
    {
        return chara is FarmAnimal farmAnimal && !farmAnimal.wasPet.Value;
    }

    /// <summary>Apply or refreshes the buff.</summary>
    /// <param name="proc"></param>
    /// <returns></returns>
    protected override bool ApplyEffect(ProcEventArgs proc)
    {
        if (
            Places.ClosestMatchingFarmAnimal(
                proc.LocationOrCurrent,
                e.CompanionPosOff ?? proc.Farmer.Position,
                args.Range,
                IsFarmAnimalInNeedOfPetting
            )
            is not FarmAnimal closest
        )
            return false;
        closest.pet(proc.Farmer);
        return base.ApplyEffect(proc);
    }
}
