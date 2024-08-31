using Microsoft.Xna.Framework;
using StardewValley.Objects.Trinkets;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Inventories;
using TrinketTinker.Models;
using TrinketTinker.Companions;
using TrinketTinker.Effects.Abilities;
using StardewModdingAPI;

namespace TrinketTinker.Effects
{
    /// <summary>Base class for TrinketTinker trinkets, allows extensible companions with extensible abilities.</summary>
    public class TrinketTinkerEffect : TrinketEffect
    {
        public readonly string ModData_WhichVariant = $"{ModEntry.ModId}/WhichVariant";
        /// <summary>Companion data with matching ID</summary>
        protected TinkerData? Data;

        /// <summary>Abilities for this trinket.</summary>
        protected List<List<Ability>> Abilities;

        /// <summary>Abilities, key'd by <see cref="AbilityData.ProcOn"/></summary>
        protected List<Dictionary<ProcOn, List<Ability>>> SortedAbilities;

        /// <summary>Get adjusted index for <see cref="Abilities"/> and <see cref="SortedAbilities"/></summary>
        protected int Level => GeneralStat - (Data?.MinLevel ?? 0);

        /// <summary>Position of companion, including offset if applicable.</summary>
        public Vector2 CompanionPosition
        {
            get
            {
                if (Companion is TrinketTinkerCompanion cmp)
                {
                    return cmp.Position + cmp.Offset;
                }
                return Companion.Position;
            }
        }

        public Inventory Items { get; } = [];

        /// <summary>Constructor</summary>
        /// <param name="trinket"></param>
        public TrinketTinkerEffect(Trinket trinket)
            : base(trinket)
        {
            ModEntry.CompanionData.TryGetValue(trinket.ItemId, out Data);
            Abilities = [];
            SortedAbilities = [];
            if (Data != null)
            {
                int lvl = 0;
                // Abilities
                foreach (List<AbilityData> abList in Data.Abilities)
                {
                    List<Ability> lvlAbility = [];
                    Dictionary<ProcOn, List<Ability>> lvlSorted = new();
                    foreach (ProcOn actv in Enum.GetValues(typeof(ProcOn)))
                    {
                        lvlSorted[actv] = [];
                    }
                    foreach (AbilityData ab in abList)
                    {
                        if (ModEntry.TryGetType(ab.AbilityClass, out Type? abilityType, Constants.ABILITY_CLS))
                        {
                            Ability? ability = (Ability?)Activator.CreateInstance(abilityType, this, ab, lvl);
                            if (ability != null && ability.Valid)
                            {
                                lvlAbility.Add(ability);
                                lvlSorted[ab.ProcOn].Add(ability);
                            }
                            else
                            {
                                ModEntry.Log($"Skip invalid ability ({ab.AbilityClass} from {trinket.QualifiedItemId})", LogLevel.Warn);
                            }
                        }
                        else
                        {
                            ModEntry.Log($"Failed to get type for ability ({ab.AbilityClass} from {trinket.QualifiedItemId})", LogLevel.Warn);
                        }
                    }
                    Abilities.Add(lvlAbility);
                    SortedAbilities.Add(lvlSorted);
                    lvl++;
                }
            }
        }

        /// <summary>Spawn the companion, and activate all abilities</summary>
        /// <param name="farmer"></param>
        public override void Apply(Farmer farmer)
        {
            if (Data == null || Game1.gameMode != 3)
                return;

            int variant = 0;
            if (Trinket.modData.TryGetValue(ModData_WhichVariant, out string variantStr))
                variant = int.Parse(variantStr);

            // Companion
            if (ModEntry.TryGetType(Data.CompanionClass, out Type? companionCls))
                Companion = (TrinketTinkerCompanion?)Activator.CreateInstance(companionCls, Trinket.ItemId, variant);
            else
                Companion = new TrinketTinkerCompanion(Trinket.ItemId, variant);
            farmer.AddCompanion(Companion);

            if (farmer != Game1.player || Abilities.Count <= Level)
                return;

            // Apply Abilities
            foreach (var ability in Abilities[Level])
            {
                ability.Activate(farmer);
            }
        }

        /// <summary>Remove the companion, and deactivate all abilities</summary>
        /// <param name="farmer"></param>
        public override void Unapply(Farmer farmer)
        {
            farmer.RemoveCompanion(Companion);

            if (farmer != Game1.player || Abilities.Count <= Level)
                return;

            foreach (var ability in Abilities[Level])
            {
                ability.Deactivate(farmer);
            }
        }

        public override void OnUse(Farmer farmer)
        {
            // foreach (var ability in SortedAbilities[Level][ProcOn.Use])
            // {
            //     ability.Proc(farmer);
            // }
            ModEntry.Log($"OnUse {farmer}");
        }

        public override void OnFootstep(Farmer farmer)
        {
            if (farmer != Game1.player || Abilities.Count <= Level)
                return;
            foreach (var ability in SortedAbilities[Level][ProcOn.Footstep])
            {
                ability.Proc(farmer);
            }
        }

        public override void OnReceiveDamage(Farmer farmer, int damageAmount)
        {
            if (farmer != Game1.player || Abilities.Count <= Level)
                return;
            foreach (var ability in SortedAbilities[Level][ProcOn.ReceiveDamage])
            {
                ability.Proc(farmer, damageAmount);
            }
        }

        public override void OnDamageMonster(Farmer farmer, Monster monster, int damageAmount, bool isBomb, bool isCriticalHit)
        {
            if (farmer != Game1.player || Abilities.Count <= Level || monster == null)
                return;
            foreach (var ability in SortedAbilities[Level][ProcOn.DamageMonster])
            {
                ability.Proc(farmer, monster, damageAmount, isBomb, isCriticalHit);
            }
            if (monster.Health <= 0)
            {
                foreach (var ability in SortedAbilities[Level][ProcOn.SlayMonster])
                {
                    ability.Proc(farmer, monster, damageAmount, isBomb, isCriticalHit);
                }
            }
        }

        /// <summary>Handle the trigger (<see cref="ProcTrinket.TriggerActionName"/>).</summary>
        /// <param name="farmer"></param>
        /// <param name="monster"></param>
        /// <param name="damageAmount"></param>
        public virtual void OnTrigger(Farmer farmer)
        {
            if (farmer != Game1.player || Abilities.Count <= Level)
                return;
            foreach (var ability in SortedAbilities[Level][ProcOn.Trigger])
            {
                ability.Proc(farmer);
            }
        }

        public override void Update(Farmer farmer, GameTime time, GameLocation location)
        {
            if (farmer != Game1.player || Abilities.Count <= Level)
                return;
            foreach (var ability in Abilities[Level])
            {
                ability.Update(farmer, time, location);
            }
        }

        public override bool GenerateRandomStats(Trinket trinket)
        {
            if (Data == null)
                return false;
            if (Abilities.Count <= 1)
                GeneralStat = Data.MinLevel;
            Random r = Utility.CreateRandom(trinket.generationSeed.Value);
            GeneralStat = r.Next(Data.MinLevel, Data.MinLevel + Abilities.Count);
            trinket.descriptionSubstitutionTemplates.Add(GeneralStat.ToString());
            return true;
        }

        /// <summary>Randomize this trinket's variant through trinket colorizer, return true of the variant is rerolled.</summary>
        /// <param name="trinket"></param>
        public virtual bool RerollVariant(Trinket trinket)
        {
            if (Data == null || Data.Variants.Count <= 1)
                return false;
            int nextVariant = Random.Shared.Next(Data.Variants.Count);
            trinket.modData[ModData_WhichVariant] = nextVariant.ToString();
            return true;
        }
    }
}
