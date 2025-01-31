namespace TrinketTinker.Models;

/// <summary>Defines how an ability can proc (activate).</summary>
public enum ProcOn
{
    /// <summary>Proc on equip, ignore conditions.</summary>
    Always,

    /// <summary>Proc when the first ability on the level procs</summary>
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

/// <summary>Data for <see cref="Effects.Abilities"/>, defines game effect that a trinket can provide.</summary>
public sealed class AbilityData : Mixin.IHaveArgs
{
    /// <summary>Type name of the ability, can use short form like "Buff" for buff ability. Default "Nop" for <see cref="Effects.Abilities.NopAbility"/></summary>
    public string AbilityClass { get; set; } = "Nop";

    /// <summary>String description of what this ability does, will be passed to trinket item description and replace {1}</summary>
    public string? Description { get; set; } = null;

    /// <summary>Determine when this ability activates.</summary>
    public ProcOn Proc { get; set; } = ProcOn.Footstep;

    /// <summary>Minimum cooldown time between ability activation, all <see cref="ProcOn"/> values respect this, not just <see cref="ProcOn.Timer"/>.</summary>
    public double ProcTimer { get; set; } = -1;

    /// <summary>For <see cref="ProcOn.Sync"/>, this is the ability index (on same level) to listen to.</summary>
    public int ProcSyncIndex { get; set; } = 0;

    /// <summary>For <see cref="ProcOn.Sync"/>, this is the delay between this ability's proc and the proc on any sync abilities.</summary>
    public int ProcSyncDelay { get; set; } = 0;

    /// <summary>Sound cue to play on proc.</summary>
    public string? ProcSound { get; set; } = null;

    /// <summary>Temporary animated sprite to spawn on proc, each item is the id of an entry in the mushymato.TrinketTinker/TAS asset.</summary>
    public List<string> ProcTAS { get; set; } = [];

    /// <summary>Have companion switch to an <see cref="MotionData.AnimClips"/> on proc, switch back to normal anim after it is done.</summary>
    public string? ProcOneshotAnim { get; set; } = null;

    /// <summary>Have companion display a <see cref="MotionData.SpeechBubbles"/> on proc, switch back to normal anim after it is done.</summary>
    public string? ProcSpeechBubble { get; set; } = null;

    /// <summary>Condition, see <see cref="StardewValley.GameStateQuery"/></summary>
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
}
