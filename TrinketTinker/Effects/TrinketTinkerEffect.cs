using Microsoft.Xna.Framework;
using StardewValley.Objects;
using StardewValley;
using StardewValley.Monsters;
using TrinketTinker.Models;
using TrinketTinker.Companions;
using TrinketTinker.Effects.Abilities;
using StardewModdingAPI;

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
        protected Dictionary<string, Ability> Abilities;
        protected Dictionary<ProcOn, List<Ability>> SortedAbilities;
        public Vector2 CompanionPosition
        {
            get
            {
                if (_companion is TrinketTinkerCompanion cmp)
                {
                    return cmp.Position + cmp.Offset;
                }
                return _companion.Position;
            }
        }

        public TrinketTinkerEffect(Trinket trinket)
            : base(trinket)
        {
            ModEntry.CompanionData.TryGetValue(trinket.ItemId, out Data);
            Abilities = new();
            SortedAbilities = new();
            foreach (ProcOn actv in Enum.GetValues(typeof(ProcOn)))
            {
                SortedAbilities[actv] = new List<Ability>();
            }
            if (Data != null)
            {
                // Abilities
                foreach (KeyValuePair<string, AbilityData> kv in Data.Abilities)
                {
                    if (ModEntry.TryGetType(kv.Value.AbilityClass, out Type? abilityType, ABILITY_FMT))
                    {
                        Ability? ability = (Ability?)Activator.CreateInstance(abilityType, this, kv.Value);
                        if (ability != null && ability.Valid)
                        {
                            Abilities[kv.Key] = ability;
                            SortedAbilities[kv.Value.ProcOn].Add(ability);
                        }
                        else
                        {
                            ModEntry.Log($"Skip invalid ability ({kv.Value.AbilityClass} from {trinket.QualifiedItemId})", LogLevel.Warn);
                        }
                    }
                    else
                    {
                        ModEntry.Log($"Failed to get type for ability ({kv.Value.AbilityClass} from {trinket.QualifiedItemId})", LogLevel.Warn);
                    }
                }
            }
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

            // Apply Abilities
            foreach (var ability in Abilities.Values)
            {
                ability.Activate(farmer);
            }
        }

        public override void Unapply(Farmer farmer)
        {
            farmer.RemoveCompanion(_companion);

            foreach (var ability in Abilities.Values)
            {
                ability.Deactivate(farmer);
            }
        }
        public override void OnUse(Farmer farmer)
        {
            foreach (var ability in SortedAbilities[ProcOn.Use])
            {
                ability.Proc(farmer);
            }
        }
        public override void OnFootstep(Farmer farmer)
        {
            foreach (var ability in SortedAbilities[ProcOn.Footstep])
            {
                ability.Proc(farmer);
            }
        }
        public override void OnReceiveDamage(Farmer farmer, int damageAmount)
        {
            foreach (var ability in SortedAbilities[ProcOn.ReceiveDamage])
            {
                ability.Proc(farmer, damageAmount);
            }
        }
        public override void OnDamageMonster(Farmer farmer, Monster monster, int damageAmount)
        {
            foreach (var ability in SortedAbilities[ProcOn.DamageMonster])
            {
                ability.Proc(farmer, monster, damageAmount);
            }
        }
        public override void Update(Farmer farmer, GameTime time, GameLocation location)
        {
            foreach (var ability in Abilities.Values)
            {
                ability.Update(farmer, time, location);
            }
        }
        public override void GenerateRandomStats(Trinket trinket)
        {
            // foreach (var ability in Abilities.Values)
            // {
            //     ability.GenerateRandomStats(trinket);
            // }
        }
    }
}
