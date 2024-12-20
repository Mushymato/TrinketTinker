using TrinketTinker.Companions.Motions;
using TrinketTinker.Effects.Abilities;

namespace TrinketTinker.Wheels;

/// <summary>Constants</summary>
public static class TinkerConst
{
    /// <summary>Float second of 1 frame (60fps), not perfectly accurate since it is float, only use for bounds.</summary>
    public const float ONE_FRAME = 1000f / 60;

    /// <summary>Special color name for the animated prismatic color mask.</summary>
    public const string COLOR_PRISMATIC = "Prismatic";

    /// <summary>String pattern for trinket tinker motion classes.</summary>
    internal static readonly string MOTION_CLS = GetClsPattern(typeof(LerpMotion), "Motion");

    /// <summary>String pattern for trinket tinker ability classes</summary>
    internal static readonly string ABILITY_CLS = GetClsPattern(typeof(BuffAbility), "Ability");

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
