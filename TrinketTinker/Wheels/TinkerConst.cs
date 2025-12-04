using TrinketTinker.Companions.Motions;
using TrinketTinker.Effects.Abilities;

namespace TrinketTinker.Wheels;

/// <summary>Constants</summary>
public static class TinkerConst
{
    /// <summary>Float second of 1 frame (60fps), not perfectly accurate since it is float, only use for bounds.</summary>
    public const float ONE_FRAME = 1000f / 60;

    public const float TURN_LEEWAY = 8f;

    /// <summary>Special color name for the animated prismatic color mask.</summary>
    public const string COLOR_PRISMATIC = "Prismatic";

    /// <summary>String pattern for Trinket Tinker motion classes.</summary>
    internal static readonly string MOTION_CLS = GetClsPattern(typeof(LerpMotion), "Motion");

    /// <summary>String pattern for Trinket Tinker ability classes</summary>
    internal static readonly string ABILITY_CLS = GetClsPattern(typeof(BuffAbility), "Ability");

    public const string CustomFields_DirectEquipOnly = $"{ModEntry.ModId}/DirectEquipOnly";
    public const string ModData_IndirectEquip = $"{ModEntry.ModId}/IndirectEquip";
    public const string ModData_IndirectEquipFromAPI = $"{ModEntry.ModId}/IndirectEquipFromAPI";
    public const string ModData_HiddenEquip = $"{ModEntry.ModId}/HiddenEquip";
    public const string ModData_NoPersistEquip = $"{ModEntry.ModId}/NoPersistEquip";
    public const string CustomFields_Owner = $"{ModEntry.ModId}/Owner";
    public const string CustomFields_Trinket = $"{ModEntry.ModId}/Trinket";
    public const string CustomFields_Data = $"{ModEntry.ModId}/Data";
    public const string CustomFields_Position = $"{ModEntry.ModId}/Position";
    public const string CustomFields_PosOff = $"{ModEntry.ModId}/PosOff";

    internal static string GetClsPattern(Type cls, string suffix)
    {
        if (cls.AssemblyQualifiedName is not string clsName)
            throw new ArgumentException($"Can't get AssemblyQualifiedName from type {cls}");
        ReadOnlySpan<char> clsSpan = clsName.AsSpan();
        int end = clsSpan.IndexOf(',');
        if (end < 0)
            end = clsSpan.Length - 1;
        int start = end;
        while (start > 0 && clsSpan[--start] != '.')
            ;
        return $"{clsSpan.Slice(0, start)}.{{0}}{suffix}{clsSpan.Slice(end)}";
    }
}
