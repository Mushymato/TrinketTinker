using Force.DeepCloner;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using TrinketTinker.Models;

namespace TrinketTinker.Effects.Abilities
{
    public abstract class Ability
    {
        public static readonly LogLevel logLvl = LogLevel.Debug;
        protected readonly TrinketTinkerEffect e;
        protected readonly AbilityData d;
        public readonly string Name;
        public bool Active { get; set; }
        private bool Allowed { get; set; }
        public double ProcTimer { get; set; } = -1;
        public bool Valid { get; set; } = false;
        public Ability(TrinketTinkerEffect effect, AbilityData data)
        {
            e = effect;
            d = data;
            Name = data.Name == "" ? GetType().Name : data.Name;
            ProcTimer = data.ProcTimer;
        }

        protected virtual bool CheckFarmer(Farmer farmer)
        {
            return Active && Allowed && (
                d.Condition == null ||
                GameStateQuery.CheckConditions(d.Condition, farmer.currentLocation, farmer, null, null, Random.Shared)
            );
        }
        protected virtual bool CheckMonster(Monster monster)
        {
            return monster != null;
        }

        protected virtual bool ApplyEffect(Farmer farmer)
        {
            Allowed = false;
            return true;
        }
        protected virtual bool ApplyEffect(Farmer farmer, int damageAmount)
        {
            return ApplyEffect(farmer);
        }
        protected virtual bool ApplyEffect(Farmer farmer, Monster monster, int damageAmount)
        {
            return ApplyEffect(farmer);
        }
        protected virtual bool ApplyEffect(Farmer farmer, GameTime time, GameLocation location)
        {
            return ApplyEffect(farmer);
        }
        public virtual bool Activate(Farmer farmer)
        {
            if (!Active)
            {
                ModEntry.Log($"{Name}.Activate({farmer})");
                Active = true;
                Allowed = true;
                ProcTimer = d.ProcTimer;
            }
            return Active;
        }
        public virtual bool Deactivate(Farmer farmer)
        {
            if (Active)
            {
                ModEntry.Log($"{Name}.Deactivate({farmer})");
                Active = false;
                Allowed = false;
                return true;
            }
            return false;
        }
        public virtual bool Proc(Farmer farmer)
        {
            if (CheckFarmer(farmer))
            {
                ModEntry.Log($"{Name}.Proc({farmer})");
                return ApplyEffect(farmer);
            }
            return false;
        }
        public virtual void Update(Farmer farmer, GameTime time, GameLocation location)
        {
            if (d.ProcTimer > 0 && !Allowed)
            {
                ProcTimer -= time.ElapsedGameTime.TotalMilliseconds;
                Allowed = ProcTimer <= 0;
                if (Allowed)
                {
                    if (d.ProcOn == ProcOn.Timed)
                        Proc(farmer, time, location);
                    ProcTimer = d.ProcTimer;
                }
            }
            else
            {
                Allowed = true;
            }
        }
        public virtual bool Proc(Farmer farmer, int damageAmount)
        {
            if (CheckFarmer(farmer) && damageAmount >= d.DamageThreshold)
            {
                ModEntry.Log($"{Name}.Proc({farmer}, {damageAmount})");
                return ApplyEffect(farmer, damageAmount);
            }
            return false;
        }
        public virtual bool Proc(Farmer farmer, Monster monster, int damageAmount)
        {
            if (CheckFarmer(farmer) && CheckMonster(monster) && damageAmount >= d.DamageThreshold)
            {
                ModEntry.Log($"{Name}.Proc({farmer}, {monster}, {damageAmount})");
                return ApplyEffect(farmer, monster, damageAmount);
            }
            return false;
        }
        public virtual bool Proc(Farmer farmer, GameTime time, GameLocation location)
        {
            if (CheckFarmer(farmer))
            {
                ModEntry.Log($"{Name}.Proc({farmer}, {time}, {location})");
                return ApplyEffect(farmer, time, location);
            }
            return false;
        }
    }
}
