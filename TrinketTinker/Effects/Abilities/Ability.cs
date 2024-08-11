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
        public double LastProcTime { get; set; } = -1;
        public bool Valid { get; set; } = false;
        public Ability(TrinketTinkerEffect effect, AbilityData data)
        {
            e = effect;
            d = data;
            Name = data.Name == "" ? GetType().Name : data.Name;
        }

        protected virtual bool CheckFarmer(Farmer farmer)
        {
            return Active && (
                d.Condition == null ||
                GameStateQuery.CheckConditions(d.Condition, farmer.currentLocation, farmer, null, null, Random.Shared)
            );
        }
        protected virtual bool CheckMonster(Monster monster)
        {
            return monster != null;
        }

        protected virtual bool ApplyOnFarmer(Farmer farmer)
        {
            LastProcTime = Game1.currentGameTime.TotalGameTime.TotalSeconds;
            return true;
        }
        protected virtual bool ApplyOnFarmerAndMonster(Farmer farmer, Monster monster)
        {
            return ApplyOnFarmer(farmer);
        }

        public virtual bool Activate(Farmer farmer)
        {
            if (!Active)
            {
                ModEntry.Log($"{Name}.Activate({farmer}), Active: {Active}");
                Active = true;
            }
            return Active;
        }
        public virtual bool Deactivate(Farmer farmer)
        {
            if (Active)
            {
                ModEntry.Log($"{Name}.Deactivate({farmer}), Active: {Active}");
                Active = false;
                return true;
            }
            return false;
        }
        public virtual bool Proc(Farmer farmer)
        {
            if (CheckFarmer(farmer))
            {
                ModEntry.Log($"{Name}.Proc({farmer}), ProcTime: {LastProcTime}");
                return ApplyOnFarmer(farmer);
            }
            return false;
        }
        public virtual void Update(Farmer farmer, GameTime time, GameLocation location)
        {
        }
        public virtual bool Proc(Farmer farmer, int damageAmount)
        {
            if (CheckFarmer(farmer) && damageAmount >= d.DamageThreshold)
            {
                ModEntry.Log($"{Name}.Proc({farmer}, {damageAmount}), ProcTime: {LastProcTime}");
                return ApplyOnFarmer(farmer);
            }
            return false;
        }
        public virtual bool Proc(Farmer farmer, Monster monster, int damageAmount)
        {
            if (CheckFarmer(farmer) && CheckMonster(monster) && damageAmount >= d.DamageThreshold)
            {
                ModEntry.Log($"{Name}.Proc({farmer}, {monster}, {damageAmount}), ProcTime: {LastProcTime}");
                return ApplyOnFarmerAndMonster(farmer, monster);
            }
            return false;
        }
        public virtual bool Proc(Farmer farmer, GameTime time, GameLocation location)
        {
            if (CheckFarmer(farmer))
            {
                ModEntry.Log($"{Name}.Proc({farmer}, {time}, {location}), ProcTime: {LastProcTime}");
                return ApplyOnFarmer(farmer);
            }
            return false;
        }
    }
}
