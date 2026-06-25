using StardewValley;
using StardewValley.Characters;
using TrinketTinker.Effects.Support;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;
using TrinketTinker.Wheels;

namespace TrinketTinker.Effects.Abilities;

/// <summary>Pets a farm animal :).</summary>
public sealed class PetFarmAnimalAbility(TrinketTinkerEffect effect, AbilityData data, int lvl)
    : Ability<PosRangeArgs>(effect, data, lvl)
{
    /// <summary>Check that this is a farm animal in need of petting</summary>
    /// <param name="chara"></param>
    /// <returns></returns>
    internal static bool IsFarmAnimalInNeedOfPetting(Character chara)
    {
        return chara is FarmAnimal farmAnimal && !farmAnimal.wasPet.Value;
    }

    /// <summary>Pet the farm animal.</summary>
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

/// <summary>Pets a pet :).</summary>
public sealed class PetPetAbility(TrinketTinkerEffect effect, AbilityData data, int lvl)
    : Ability<PosRangeArgs>(effect, data, lvl)
{
    /// <summary>Check that this is a farm animal in need of petting</summary>
    /// <param name="chara"></param>
    /// <returns></returns>
    internal static bool IsPetInNeedOfPetting(Character chara)
    {
        return chara is Pet pet && !pet.grantedFriendshipForPet.Value;
    }

    /// <summary>Pet the farm animal.</summary>
    /// <param name="proc"></param>
    /// <returns></returns>
    protected override bool ApplyEffect(ProcEventArgs proc)
    {
        if (
            Places.ClosestMatchingCharacter(
                proc.LocationOrCurrent,
                e.CompanionPosOff ?? proc.Farmer.Position,
                args.Range,
                IsPetInNeedOfPetting
            )
            is not Pet closest
        )
            return false;
        Farmer who = proc.Farmer;
        int currentToolIndex = who.CurrentToolIndex;
        try
        {
            who.CurrentToolIndex = who.Items.Count;
            closest.checkAction(proc.Farmer, proc.LocationOrCurrent);
        }
        finally
        {
            who.CurrentToolIndex = currentToolIndex;
        }
        return base.ApplyEffect(proc);
    }
}
