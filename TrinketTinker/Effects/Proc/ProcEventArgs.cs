using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Monsters;
using TrinketTinker.Models;

namespace TrinketTinker.Effects.Proc
{
    /// <summary>Proc event data</summary>
    public class ProcEventArgs(ProcOn procOn, Farmer farmer) : EventArgs
    {
        /// <summary>Kind of proc triggering this event <see cref="ProcOn"/></summary>
        public ProcOn ProcOn => procOn;
        public Farmer Farmer => farmer;
        public GameTime? Time { get; set; } = null;
        public GameLocation? Location { get; set; } = null;
        public Monster? Monster { get; set; } = null;
        public int? DamageAmount { get; set; } = null;
        public bool? IsBomb { get; set; } = null;
        public bool? IsCriticalHit { get; set; } = null;
        public string[]? TriggerArgs { get; set; } = null;
        public TriggerActionContext? TriggerContext { get; set; } = null;
        public GameLocation LocationOrCurrent => Location ?? Farmer?.currentLocation ?? Game1.currentLocation;

        public bool Check(AbilityData data)
        {
            if (Farmer == null)
                return false;
            if (DamageAmount != null && DamageAmount < data.DamageThreshold)
                return false;
            if (ProcOn == ProcOn.SlayMonster || ProcOn == ProcOn.DamageMonster)
            {
                if (Monster != null || (data.IsBomb ?? IsBomb) == IsBomb || (data.IsCriticalHit ?? IsCriticalHit) == IsCriticalHit)
                    return false;
            }
            if (data.Condition != null)
                return GameStateQuery.CheckConditions(data.Condition, LocationOrCurrent, Farmer, null, null, Random.Shared);
            return true;
        }
    }
}
