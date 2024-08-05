using StardewValley;
using StardewValley.Monsters;
using StardewValley.Objects;
using TrinketTinker.Models;

namespace TrinketTinker.Effects.Abilities
{
    public class DebugPrintAbility : Ability
    {
        public DebugPrintAbility(TrinketTinkerEffect effect, AbilityData data) : base(effect, data)
        {
            ModEntry.Log($"Ability.ctor");
        }
        public override void Apply(Farmer farmer)
        {
            ModEntry.Log($"Ability.Apply({farmer})");
        }
        public override void Unapply(Farmer farmer)
        {
            ModEntry.Log($"Ability.Unapply({farmer})");
        }
        public override void OnUse(Farmer farmer)
        {
            ModEntry.Log($"Ability.OnUse({farmer})");
        }
        public override void OnFootstep(Farmer farmer)
        {
            ModEntry.Log($"Ability.OnFootstep({farmer})");
        }
        public override void OnReceiveDamage(Farmer farmer, int damageAmount)
        {
            ModEntry.Log($"Ability.OnReceiveDamage({farmer}, {damageAmount})");
        }
        public override void OnDamageMonster(Farmer farmer, Monster monster, int damageAmount)
        {
            ModEntry.Log($"Ability.OnDamageMonster({farmer}, {monster}, {damageAmount})");
        }
        public override void GenerateRandomStats(Trinket trinket)
        {
            ModEntry.Log($"Ability.GenerateRandomStats({trinket})");
        }

    }
}
