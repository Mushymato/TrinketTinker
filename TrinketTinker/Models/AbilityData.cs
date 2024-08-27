using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;

namespace TrinketTinker.Models
{
    /// <summary>Defines how an ability can proc (activate).</summary>
    public enum ProcOn
    {
        /// <summary>Proc on equip, ignores all conditions.</summary>
        Always,
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
    public class AbilityData : Mixin.HaveArgs
    {
        /// <summary>Name of this ability. If unset, a name is generatde from class name and trinket ID.</summary>
        public string Name = "";
        /// <summary>Type name of the ability, can use short form that matches <see cref="Constants.ABILITY_CLS"/></summary>
        public string? AbilityClass { get; set; } = null;
        /// <summary>Determine when this ability activates.</summary>
        public ProcOn ProcOn = new();
        /// <summary>Minimum cooldown time between ability activation, all <see cref="Models.ProcOn"/> values respect this, not just <see cref="ProcOn.Timer"/>.</summary>
        public double ProcTimer { get; set; } = -1;
        /// <summary>Sound cue to play on proc.</summary>
        public string? ProcSound { get; set; } = null;
        /// <summary>Condition, see <see cref="StardewValley.GameStateQuery"/></summary>
        public string? Condition { get; set; } = null;
        /// <summary>Minimum damage dealt or received before proc, applicable to <see cref="ProcOn.ReceiveDamage"/> and <see cref="ProcOn.DamageMonster"/>.</summary>
        public int DamageThreshold { get; set; } = -1;
    }
}