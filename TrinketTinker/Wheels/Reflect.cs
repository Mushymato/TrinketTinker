using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

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

    /// Android Compat Nonsense
    /// Broken code in TrinketTinker.dll:
    //      reference to StardewValley.ISoundBank.Exists (no such method)
    //      reference to StardewValley.Menus.InventoryMenu.SetPosition (no such method)
    //      reference to StardewValley.Menus.ItemGrabMenu.storageSpaceTopBorderOffset (no such field).

    internal static readonly MethodInfo? SoundBank_Exists = typeof(ISoundBank).GetMethod(
        "Exists",
        BindingFlags.Public | BindingFlags.Instance
    );

    internal static bool Try_SoundBank_Exists(ISoundBank soundBank, string CueName)
    {
        return (bool?)SoundBank_Exists?.Invoke(soundBank, [CueName]) ?? true;
    }

    internal static readonly MethodInfo? InventoryMenu_SetPosition = typeof(InventoryMenu).GetMethod(
        "SetPosition",
        BindingFlags.Public | BindingFlags.Instance
    );

    internal static void Try_InventoryMenu_SetPosition(
        InventoryMenu inventoryMenu,
        int xPositionOnScreen,
        int yPositionOnScreen
    )
    {
        InventoryMenu_SetPosition?.Invoke(inventoryMenu, [xPositionOnScreen, yPositionOnScreen]);
    }

    internal static readonly FieldInfo? ItemGrabMenu_storageSpaceTopBorderOffset = typeof(ItemGrabMenu).GetField(
        "storageSpaceTopBorderOffset",
        BindingFlags.Public | BindingFlags.Instance
    );

    internal static int Try_ItemGrabMenu_storageSpaceTopBorderOffset_Get(ItemGrabMenu itemGrabMenu)
    {
        return (int?)ItemGrabMenu_storageSpaceTopBorderOffset?.GetValue(itemGrabMenu) ?? 0;
    }

    internal static void Try_ItemGrabMenu_storageSpaceTopBorderOffset_Set(ItemGrabMenu itemGrabMenu, int newValue)
    {
        ItemGrabMenu_storageSpaceTopBorderOffset?.SetValue(itemGrabMenu, newValue);
    }
}
