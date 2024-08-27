using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Delegates;
using TrinketTinker.Models;

namespace TrinketTinker.Effects.Abilities
{
    /// <summary>Abstract class, proc various effects while trinket is equipped.</summary>
    public abstract class Ability
    {
        /// <summary>Companion that owns this ability.</summary>
        protected readonly TrinketTinkerEffect e;
        /// <summary>Data for this ability.</summary>
        protected readonly AbilityData d;
        /// <summary>Ability name, default to type name.</summary>
        public readonly string Name;
        /// <summary>True if trinket data produces a valid ability.</summary>
        public bool Valid { get; set; } = false;
        /// <summary>True if trinket equiped.</summary>
        protected bool Active { get; set; }
        /// <summary>True if trinket proc timeout is not set, or elapsed.</summary>
        protected bool Allowed { get; set; }
        /// <summary>Tracks trinket proc timeout, counts down to 0 and resets to ProcTimer value is set in <see cref="AbilityData"/>.</summary>
        protected double ProcTimer { get; set; } = -1;

        /// <summary>Constructor</summary>
        /// <param name="effect"></param>
        /// <param name="data"></param>
        public Ability(TrinketTinkerEffect effect, AbilityData data, int lvl)
        {
            e = effect;
            d = data;
            string clsName = data.Name == "" ? GetType().Name : data.Name;
            Name = $"{effect.Trinket.ItemId}:{clsName}[{lvl}]";
            ProcTimer = data.ProcTimer;
        }

        /// <summary>Check condition is valid for farmer's location.</summary>
        /// <param name="farmer"></param>
        /// <returns>True if ability should activate.</returns>
        protected virtual bool CheckFarmer(Farmer farmer, GameLocation? location = null)
        {
            return Active && Allowed && (
                d.Condition == null ||
                GameStateQuery.CheckConditions(
                    d.Condition, location ?? farmer.currentLocation,
                    farmer, null, null, Random.Shared
                )
            );
        }

        /// <summary>Check monster is valid.</summary>
        /// <param name="monster"></param>
        /// <returns>True if ability should activate.</returns>
        protected virtual bool CheckMonster(Monster monster)
        {
            return monster != null;
        }

        /// <summary>Applies ability effect, plays sound if ProcSound value is set in <see cref="AbilityData"/>.</summary>
        /// <param name="farmer"></param>
        /// <returns></returns>
        protected virtual bool ApplyEffect(Farmer farmer)
        {
            Allowed = false;
            return true;
        }

        /// <summary>Apply effect for <see cref="TrinketTinkerEffect.OnReceiveDamage"/></summary>
        /// <param name="farmer"></param>
        /// <param name="damageAmount"></param>
        /// <returns></returns>
        protected virtual bool ApplyEffect(Farmer farmer, int damageAmount)
        {
            return ApplyEffect(farmer);
        }

        /// <summary>Apply effect for <see cref="TrinketTinkerEffect.OnDamageMonster"/></summary>
        /// <param name="farmer"></param>
        /// <param name="monster"></param>
        /// <param name="damageAmount"></param>
        /// <returns></returns>
        protected virtual bool ApplyEffect(Farmer farmer, Monster monster, int damageAmount)
        {
            return ApplyEffect(farmer);
        }

        /// <summary>Apply effect for <see cref="ProcOn.Timer"/> abilities, via <see cref="Update"/>.</summary>
        /// <param name="farmer"></param>
        /// <param name="time"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        protected virtual bool ApplyEffect(Farmer farmer, GameTime time, GameLocation location)
        {
            return ApplyEffect(farmer);
        }

        /// <summary>Setup the ability, when trinket is equipped.</summary>
        /// <param name="farmer"></param>
        /// <returns></returns>
        public virtual bool Activate(Farmer farmer)
        {
            if (!Active)
            {
                if (d.ProcOn == ProcOn.Always)
                    Proc(farmer);
                Active = true;
                Allowed = d.ProcOn != ProcOn.Timer;
                ProcTimer = d.ProcTimer;
            }
            return Active;
        }

        /// <summary>Teardown the ability, when trinket is removed.</summary>
        /// <param name="farmer"></param>
        /// <returns></returns>
        public virtual bool Deactivate(Farmer farmer)
        {
            if (Active)
            {
                if (d.ProcOn == ProcOn.Always)
                    UnProc(farmer);
                Active = false;
                Allowed = false;
                return true;
            }
            return false;
        }

        /// <summary>Update on game tick, handles the proc timer.</summary>
        /// <param name="farmer"></param>
        /// <param name="time"></param>
        /// <param name="location"></param>
        public virtual void Update(Farmer farmer, GameTime time, GameLocation location)
        {
            if (!Game1.shouldTimePass())
            {
                return;
            }
            if (d.ProcTimer >= Constants.ONE_FRAME && !Allowed)
            {
                ProcTimer -= time.ElapsedGameTime.TotalMilliseconds;
                Allowed = ProcTimer <= 0;
                if (Allowed)
                {
                    if (d.ProcOn == ProcOn.Timer)
                        Proc(farmer, time, location);
                    ProcTimer = d.ProcTimer;
                }
            }
            else
            {
                Allowed = true;
            }
        }

        /// <summary>Check conditions then apply effect.</summary>
        /// <param name="farmer"></param>
        /// <returns></returns>
        public virtual bool Proc(Farmer farmer)
        {
            if (CheckFarmer(farmer))
            {
                if (ApplyEffect(farmer))
                {
                    if (d.ProcSound != null)
                        Game1.playSound(d.ProcSound);
                    return true;
                }
            }
            return false;
        }

        /// <summary>Remove the ability, used if ProcOn is <see cref="ProcOn.Always"/></summary>
        /// <param name="farmer"></param>
        protected virtual void UnProc(Farmer farmer)
        {
        }

        /// <summary>Proc for <see cref="TrinketTinkerEffect.OnReceiveDamage"/>.</summary>
        /// <param name="farmer"></param>
        /// <param name="damageAmount"></param>
        /// <returns></returns>
        public virtual bool Proc(Farmer farmer, int damageAmount)
        {
            if (CheckFarmer(farmer) && damageAmount >= d.DamageThreshold)
            {
                if (ApplyEffect(farmer, damageAmount))
                {
                    if (d.ProcSound != null)
                        Game1.playSound(d.ProcSound);
                    return true;
                }
            }
            return false;
        }

        /// <summary>Proc for <see cref="TrinketTinkerEffect.OnDamageMonster"/>.</summary>
        /// <param name="farmer"></param>
        /// <param name="damageAmount"></param>
        /// <returns></returns>
        public virtual bool Proc(Farmer farmer, Monster monster, int damageAmount)
        {
            if (CheckFarmer(farmer) && CheckMonster(monster) && damageAmount >= d.DamageThreshold)
            {
                if (ApplyEffect(farmer, monster, damageAmount))
                {
                    if (d.ProcSound != null)
                        Game1.playSound(d.ProcSound);
                    return true;
                }
            }
            return false;
        }

        /// <summary>Proc for <see cref="ProcOn.Timer"/> abilities, via <see cref="Update"/>.</summary>
        /// <param name="farmer"></param>
        /// <param name="time"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public virtual bool Proc(Farmer farmer, GameTime time, GameLocation location)
        {
            if (CheckFarmer(farmer, location))
            {
                if (ApplyEffect(farmer, time, location))
                {
                    if (d.ProcSound != null)
                        Game1.playSound(d.ProcSound);
                    return true;
                }
            }
            return false;
        }
    }
}
