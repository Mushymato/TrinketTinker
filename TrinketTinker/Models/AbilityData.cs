using StardewValley.GameData;

namespace TrinketTinker.Models
{
    /// <summary>Defines how an ability can proc (activate).</summary>
    public enum ProcOn
    {
        /// <summary>Proc on equip, ignores all conditions.</summary>
        Always,
        /// <summary>Proc on use (right click while holding).</summary>
        Use,
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
    }

    /// <summary>Data for <see cref="Effects.Abilities"/>, defines game effect that a trinket can provide.</summary>
    public sealed class AbilityData : Mixin.IHaveArgs
    {
        /// <summary>Type name of the ability, can use short form like "Buff" for buff ability.</summary>
        public string? AbilityClass { get; set; } = null;
        /// <summary>Determine when this ability activates.</summary>
        public ProcOn Proc { get; set; } = ProcOn.Footstep;
        /// <summary>Minimum cooldown time between ability activation, all <see cref="Models.ProcOn"/> values respect this, not just <see cref="ProcOn.Timer"/>.</summary>
        public double ProcTimer { get; set; } = -1;
        /// <summary>Sound cue to play on proc.</summary>
        public string? ProcSound { get; set; } = null;
        /// <summary>Temporary animated sprites to spawn on proc.</summary>
        public List<TemporaryAnimatedSpriteDefinition> ProcTemporarySprites { get; set; } = [];
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
}