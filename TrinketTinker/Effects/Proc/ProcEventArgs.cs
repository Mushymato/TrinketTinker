using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Monsters;
using TrinketTinker.Models;

namespace TrinketTinker.Effects.Proc
{
    /// <summary>
    /// Proc event data.
    /// Most properties are only set for specific kinds of proc.
    /// </summary>
    public class ProcEventArgs(ProcOn procOn, Farmer farmer) : EventArgs
    {
        /// <summary>Kind of proc triggering this event</summary>
        public ProcOn Proc => procOn;
        /// <summary>Player who triggered the event</summary>
        public Farmer Farmer => farmer;
        /// <summary>Game time</summary>
        public GameTime? Time { get; set; } = null;
        /// <summary>Proc location, one of the backing props of <see cref="LocationOrCurrent"/></summary>
        public GameLocation? Location { get; set; } = null;
        /// <summary>Target monster</summary>
        public Monster? Monster { get; set; } = null;
        /// <summary>Damage amount, to monster or to player</summary>
        public int? DamageAmount { get; set; } = null;
        /// <summary>Whether damage (to monster) was a bomb</summary>
        public bool? IsBomb { get; set; } = null;
        /// <summary>Whether damage (to monster) was a critical hit</summary>
        public bool? IsCriticalHit { get; set; } = null;
        /// <summary>Arguments given to trigger action handler.</summary>
        public string[]? TriggerArgs { get; set; } = null;
        /// <summary>Trigger action context</summary>
        public TriggerActionContext? TriggerContext { get; set; } = null;

        /// <summary>Get the most valid location of this proc, either the event location or the player's current location</summary>
        public GameLocation LocationOrCurrent => Location ?? Farmer.currentLocation;

        /// <summary>
        /// Validate whether this proc should trigger ability of given data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal bool Check(AbilityData data)
        {
            if (Farmer == null)
                return false;
            if (DamageAmount != null && DamageAmount < data.DamageThreshold)
                return false;
            if (Proc == ProcOn.SlayMonster || Proc == ProcOn.DamageMonster)
            {
                if (Monster == null || (data.IsBomb ?? IsBomb) != IsBomb || (data.IsCriticalHit ?? IsCriticalHit) != IsCriticalHit)
                    return false;
            }
            if (data.Condition != null)
                return GameStateQuery.CheckConditions(data.Condition, LocationOrCurrent, Farmer);
            return true;
        }
    }
}
