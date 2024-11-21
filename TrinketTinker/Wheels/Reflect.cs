using System.Diagnostics.CodeAnalysis;

namespace TrinketTinker.Wheels;

/// <summary>Helper methods for reflection</summary>
internal static class Reflect
{
    /// <summary>Get type from a string class name</summary>
    /// <param name="className"></param>
    /// <param name="typ"></param>
    /// <returns></returns>
    public static bool TryGetType(string? className, [NotNullWhen(true)] out Type? typ)
    {
        typ = null;
        if (className == null)
            return false;
        typ = Type.GetType(className);
        if (typ != null)
            return true;
        return false;
    }

    /// <summary>Get type from a string class name that is possibly in short form.</summary>
    /// <param name="className"></param>
    /// <param name="typ"></param>
    /// <param name="longFormat">Full class name format</param>
    /// <returns></returns>
    public static bool TryGetType(string? className, [NotNullWhen(true)] out Type? typ, string longFormat)
    {
        typ = null;
        if (className == null)
            return false;
        string longClassName = string.Format(longFormat, className);
        typ = Type.GetType(longClassName);
        if (typ != null)
            return true;
        typ = Type.GetType(className);
        if (typ != null)
            return true;
        return false;
    }
}
