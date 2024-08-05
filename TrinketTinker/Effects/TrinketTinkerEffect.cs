using StardewValley.Objects;
using StardewValley;
using StardewValley.Monsters;
using TrinketTinker.Models;
using TrinketTinker.Companions;
using TrinketTinker.Effects.Abilities;

namespace TrinketTinker.Effects
{
    /// <summary>
    /// Base class for TrinketTinker trinkets
    /// Support spawning arbiturary companions
    /// </summary>
    public class TrinketTinkerEffect : TrinketEffect
    {
        private const string ABILITY_FMT = "TrinketTinker.Effects.Abilities.{0}Ability, TrinketTinker";
        protected CompanionData? Data;
        protected Dictionary<string, Ability> Abilities = new();

        public TrinketTinkerEffect(Trinket trinket)
            : base(trinket)
        {
            ModEntry.CompanionData.TryGetValue(trinket.ItemId, out Data);
        }

        public override void Apply(Farmer farmer)
        {
            if (Data == null || Game1.gameMode != 3)
                return;

            // Companion
            if (ModEntry.TryGetType(Data.CompanionClass, out Type? companionCls))
                _companion = (TrinketTinkerCompanion?)Activator.CreateInstance(companionCls, _trinket.ItemId);
            else
                _companion = new TrinketTinkerCompanion(_trinket.ItemId);
            farmer.AddCompanion(_companion);
            // Abilities
            foreach (KeyValuePair<string, AbilityData> kv in Data.Abilities)
            {
                if (ModEntry.TryGetType(kv.Value.AbilityClass, out Type? abilityType, ABILITY_FMT))
                {
                    ModEntry.Log($"abiltyType: {abilityType}");
                    Ability? ability = (Ability?)Activator.CreateInstance(abilityType, this, kv.Value);
                    if (ability != null)
                    {
                        Abilities[kv.Key] = ability;
                    }
                }
            }

            // Apply Abilities
            foreach (var ability in Abilities.Values)
            {
                ability.Apply(farmer);
            }
        }

        public override void Unapply(Farmer farmer)
        {
            farmer.RemoveCompanion(_companion);

            foreach (var ability in Abilities.Values)
            {
                ability.Unapply(farmer);
            }
        }

        public override void OnUse(Farmer farmer)
        {
            foreach (var ability in Abilities.Values)
            {
                ability.OnUse(farmer);
            }
        }
        public override void OnFootstep(Farmer farmer)
        {
            foreach (var ability in Abilities.Values)
            {
                ability.OnFootstep(farmer);
            }
        }
        public override void OnReceiveDamage(Farmer farmer, int damageAmount)
        {
            foreach (var ability in Abilities.Values)
            {
                ability.OnReceiveDamage(farmer, damageAmount);
            }
        }
        public override void OnDamageMonster(Farmer farmer, Monster monster, int damageAmount)
        {
            foreach (var ability in Abilities.Values)
            {
                ability.OnDamageMonster(farmer, monster, damageAmount);
            }
        }
        public override void GenerateRandomStats(Trinket trinket)
        {
            foreach (var ability in Abilities.Values)
            {
                ability.GenerateRandomStats(trinket);
            }
        }
    }
}
