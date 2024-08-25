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
    public class AbilityData : HaveArgs
    {
        /// <summary>Name of this ability.</summary>
        public string Name = "";
        /// <summary>Class name, need to be fully qualified to use an ability not provided by this mod.</summary>
        public string? AbilityClass { get; set; } = null;
        /// <summary>Proc on rule</summary>
        public ProcOn ProcOn = new();
        /// <summary>Timeout for ability procs, all types of procs respect this, not just <see cref="ProcOn.Timer"/>.</summary>
        public double ProcTimer { get; set; } = -1;
        /// <summary>Trigger action to listen to</summary>
        public string? ProcSound { get; set; } = null;
        /// <summary>Condition, see <see cref="StardewValley.GameStateQuery"/></summary>
        public string? Condition { get; set; } = null;
        /// <summary>Minimum damage dealt or received before proc.</summary>
        public int DamageThreshold { get; set; } = -1;
    }
}