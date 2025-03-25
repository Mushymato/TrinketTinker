using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace TrinketTinker.Wheels;

internal sealed class ModConfig
{
    public bool HideAllCompanionsDuringEvents = false;
    public KeybindList DoInteractKey = KeybindList.Parse("LeftAlt+MouseRight, LeftStick+ControllerA");
    public KeybindList OpenTinkerInventoryKey = KeybindList.Parse("RightAlt+OemPeriod, LeftStick+ControllerX");
    public KeybindList TinkerInventoryNextKey = KeybindList.Parse("PageUp, RightShoulder");
    public KeybindList TinkerInventoryPrevKey = KeybindList.Parse("PageDown, LeftShoulder");

    /// <summary>Enable draw debug mode</summary>
    public bool DrawDebugMode { get; set; } = false;

    /// <summary>Restore default config values</summary>
    private void Reset()
    {
        OpenTinkerInventoryKey = KeybindList.Parse("RightAlt+OemPeriod, LeftStick+ControllerX");
        DoInteractKey = KeybindList.Parse("LeftAlt+MouseRight, LeftStick+ControllerA");
        TinkerInventoryNextKey = KeybindList.Parse("PageUp, RightShoulder");
        TinkerInventoryPrevKey = KeybindList.Parse("PageDown, LeftShoulder");
        DrawDebugMode = false;
    }

    /// <summary>Add mod config to GMCM if available</summary>
    /// <param name="helper"></param>
    /// <param name="mod"></param>
    public void Register(IModHelper helper, IManifest mod)
    {
        Integration.IGenericModConfigMenuApi? GMCM = helper.ModRegistry.GetApi<Integration.IGenericModConfigMenuApi>(
            "spacechase0.GenericModConfigMenu"
        );
        if (GMCM == null)
        {
            helper.WriteConfig(this);
            return;
        }
        GMCM.Register(
            mod: mod,
            reset: () =>
            {
                Reset();
                helper.WriteConfig(this);
            },
            save: () =>
            {
                helper.WriteConfig(this);
            },
            titleScreenOnly: false
        );

        GMCM.AddSectionTitle(mod, I18n.Config_Section_General);
        GMCM.AddBoolOption(
            mod,
            getValue: () => HideAllCompanionsDuringEvents,
            setValue: (value) => HideAllCompanionsDuringEvents = value,
            name: I18n.Config_HideAllCompanionsDuringEvents_Name,
            tooltip: I18n.Config_HideAllCompanionsDuringEvents_Description
        );

        GMCM.AddSectionTitle(mod, I18n.Config_Section_Keybindings);
        GMCM.AddKeybindList(
            mod,
            getValue: () => DoInteractKey,
            setValue: (value) => DoInteractKey = value,
            name: I18n.Config_DoInteractKey_Name,
            tooltip: I18n.Config_DoInteractKey_Description
        );

        GMCM.AddKeybindList(
            mod,
            getValue: () => OpenTinkerInventoryKey,
            setValue: (value) => OpenTinkerInventoryKey = value,
            name: I18n.Config_OpenTinkerInventoryKey_Name,
            tooltip: I18n.Config_OpenTinkerInventoryKey_Description
        );
        GMCM.AddKeybindList(
            mod,
            getValue: () => TinkerInventoryNextKey,
            setValue: (value) => TinkerInventoryNextKey = value,
            name: I18n.Config_TinkerInventoryNextKey_Name,
            tooltip: I18n.Config_TinkerInventoryNextKey_Description
        );
        GMCM.AddKeybindList(
            mod,
            getValue: () => TinkerInventoryPrevKey,
            setValue: (value) => TinkerInventoryPrevKey = value,
            name: I18n.Config_TinkerInventoryPrevKey_Name,
            tooltip: I18n.Config_TinkerInventoryPrevKey_Description
        );

        GMCM.AddSectionTitle(mod, I18n.Config_Section_Debug);
        GMCM.AddBoolOption(
            mod,
            getValue: () => DrawDebugMode,
            setValue: (value) => DrawDebugMode = value,
            name: I18n.Config_DrawDebugMode_Name,
            tooltip: I18n.Config_DrawDebugMode_Description
        );
    }
}
