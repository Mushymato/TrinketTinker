using TrinketTinker.Effects.Support;
using TrinketTinker.Models;
using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Effects.Abilities;

/// <summary>Does absolutely nothing.</summary>
public sealed class NopAbility(TrinketTinkerEffect effect, AbilityData data, int lvl)
    : Ability<NoArgs>(effect, data, lvl)
{
    /// <summary>Does absolutely nothing.</summary>
    /// <param name="proc"></param>
    /// <returns></returns>
    protected override bool ApplyEffect(ProcEventArgs proc)
    {
        return true;
    }
}
