using StardewValley;
using StardewValley.Monsters;
using StardewValley.Objects;
using TrinketTinker.Models;

namespace TrinketTinker.Effects.Abilities
{
    public abstract class Ability
    {
        protected readonly TrinketTinkerEffect e;
        protected readonly AbilityData d;
        public Ability(TrinketTinkerEffect effect, AbilityData data)
        {
            e = effect;
            d = data;
        }
        public virtual void Apply(Farmer farmer) { }
        public virtual void Unapply(Farmer farmer) { }
        public virtual void OnUse(Farmer farmer) { }
        public virtual void OnFootstep(Farmer farmer) { }
        public virtual void OnReceiveDamage(Farmer farmer, int damageAmount) { }
        public virtual void OnDamageMonster(Farmer farmer, Monster monster, int damageAmount) { }
        public virtual void GenerateRandomStats(Trinket trinket) { }
    }
}
