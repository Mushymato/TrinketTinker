using StardewValley;
using StardewValley.Extensions;
using TrinketTinker.Models.Mixin;
using TrinketTinker.Wheels;

namespace TrinketTinker.Models;

/// <summary>Defines how an ability can proc (activate).</summary>
public enum ProcOn
{
    /// <summary>Proc on equip, ignore conditions.</summary>
    Always,

    /// <summary>Proc when the ability denoted by ProcSyncIndex on the same level procs</summary>
    Sync,

    /// <summary>Proc on walk.</summary>
    Footstep,

    /// <summary>Proc on player damaged.</summary>
    ReceiveDamage,

    /// <summary>Proc on monster damaged.</summary>
    DamageMonster,

    /// <summary>Proc on monster slayed.</summary>
    SlayMonster,

    /// <summary>Proc on timer elapsed.</summary>
    Timer,

    /// <summary>Proc on trigger action.</summary>
    Trigger,

    /// <summary>Proc on when player changes map.</summary>
    Warped,

    /// <summary>Proc on when player interacts.</summary>
    Interact,
}

/// <summary>Proc sound data</summary>
public sealed class ProcSoundData
{
    public string? CueName = null;
    public List<int>? Pitch = null;

    public static implicit operator ProcSoundData(string cueName)
    {
        return new() { CueName = cueName };
    }

    public void PlaySound(string? logName = null)
    {
        if (!string.IsNullOrEmpty(CueName))
        {
            if (Reflect.Try_SoundBank_Exists(Game1.soundBank, CueName))
            {
                Game1.playSound(CueName, 0, out ICue sound);
                // weird Pitch nonsense
                if (Pitch != null)
                    sound.Pitch = Random.Shared.ChooseFrom(Pitch) / 2400f;
            }
            else
            {
                if (logName != null)
                {
                    ModEntry.LogOnce(
                        $"{logName}: ProcSound '{CueName}' does not exist",
                        StardewModdingAPI.LogLevel.Warn
                    );
                }
                CueName = null;
            }
        }
    }
}

/// <summary>Data for fuel required to activate an ability.</summary>
public sealed class RequiredFuelData
{
    /// <summary>Specific item id required</summary>
    public string? RequiredItemId { get; set; } = null;

    /// <summary>Tags required (all tags must be on item)</summary>
    public List<string>? RequiredTags { get; set; } = null;

    /// <summary>GSQ check on item</summary>
    public string? Condition { get; set; } = null;

    /// <summary>Amount to consume on proc</summary>
    public int RequiredCount { get; set; } = 1;

    /// <summary>Amount to consume on proc</summary>
    internal bool CheckItem(Item item)
    {
        if (item.QualifiedItemId == RequiredItemId)
            return true;
        if (RequiredTags?.All(item.HasContextTag) ?? false)
            return true;
        if (GameStateQuery.CheckConditions(Condition, new(Game1.currentLocation, Game1.player, item, item, null)))
            return true;
        return false;
    }
}

/// <summary>Data for <see cref="Effects.Abilities"/>, defines game effect that a trinket can provide.</summary>
public sealed class AbilityData : IHaveArgs
{
    /// <summary>Backing Id value</summary>
    private string? id = null;

    /// <summary>Id</summary>
    public string Id
    {
        get => id ?? AbilityClass;
        set => id = value;
    }

    /// <summary>Type name of the ability, can use short form like "Buff" for buff ability. Default "Nop" for <see cref="Effects.Abilities.NopAbility"/></summary>
    public string AbilityClass { get; set; } = "Nop";

    /// <summary>String description of what this ability does, will be passed to trinket item description and replace {1}</summary>
    public string? Description { get; set; } = null;

    /// <summary>Determine when this ability activates.</summary>
    public ProcOn Proc { get; set; } = ProcOn.Footstep;

    /// <summary>Minimum cooldown time between ability activation, all <see cref="ProcOn"/> values respect this, not just <see cref="ProcOn.Timer"/>.</summary>
    public double ProcTimer { get; set; } = -1;

    /// <summary>
    /// For <see cref="ProcOn.Sync"/>, this is the ability id (on same level) to listen to.
    /// If this is not null or empty, it takes preceedence over ProcSyncIndex and will simply give up if the Id is not found.
    /// </summary>
    public string? ProcSyncId { get; set; } = null;

    /// <summary>For <see cref="ProcOn.Sync"/>, this is the ability index (on same level) to listen to.</summary>
    public int ProcSyncIndex { get; set; } = 0;

    /// <summary>For <see cref="ProcOn.Sync"/>, this is the delay between this ability's proc and the proc on any sync abilities.</summary>
    public int ProcSyncDelay { get; set; } = 0;

    /// <summary>Fuel that needs to be consumed from the tinker inventory.</summary>
    public RequiredFuelData? ProcFuel { get; set; } = null;

    /// <summary>Sound cue to play on proc.</summary>
    public ProcSoundData? ProcSound { get; set; } = null;

    /// <summary>Temporary animated sprite to spawn on proc, each item is the id of an entry in the mushymato.TrinketTinker/TAS asset.</summary>
    public List<string>? ProcTAS { get; set; } = null;

    /// <summary>Have companion switch to an <see cref="MotionData.AnimClips"/> on proc, switch back to normal anim after it is done.</summary>
    public string? ProcOneshotAnim { get; set; } = null;

    /// <summary>Have companion display a <see cref="MotionData.SpeechBubbles"/> on proc, switch back to normal anim after it is done.</summary>
    public string? ProcSpeechBubble { get; set; } = null;

    /// <summary>Switch the companion to a specific sub variant on proc, or "RECHECK" to return to normal conditional base variants.</summary>
    public string? ProcAltVariant { get; set; } = null;

    /// <summary>Make the next invocation of "Chatter" use this key.</summary>
    public string? ProcChatterKey { get; set; } = null;

    /// <summary>Condition, see <see cref="GameStateQuery"/></summary>
    public string? Condition { get; set; } = null;

    /// <summary>
    /// Minimum damage dealt or received before proc. <br/>
    /// Applies to <see cref="ProcOn.ReceiveDamage"/>, <see cref="ProcOn.DamageMonster"/>, and <see cref="ProcOn.SlayMonster"/>.
    /// </summary>
    public int DamageThreshold { get; set; } = -1;

    /// <summary>
    /// Requires the damage be caused by a bomb (true), or not caused by a bomb (false). <br/>
    /// Applies to <see cref="ProcOn.DamageMonster"/> and <see cref="ProcOn.SlayMonster"/>.
    /// </summary>
    public bool? IsBomb { get; set; } = null;

    /// <summary>
    /// Requires the damage to be critical hit (true), or not a critical hit (false). <br/>
    /// Applies to <see cref="ProcOn.DamageMonster"/> and <see cref="ProcOn.SlayMonster"/>.
    /// </summary>
    public bool? IsCriticalHit { get; set; } = null;

    /// <summary>
    /// Check if the player is in-combat, which is defined in this mod as "location has monster" and have dealt damage/taken damage in the last 10 seconds
    /// </summary>
    public bool? InCombat { get; set; } = null;

    /// <inheritdocs/>
    public Dictionary<string, object>? Args { get; set; } = null;
}
