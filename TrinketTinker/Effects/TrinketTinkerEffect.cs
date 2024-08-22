using Microsoft.Xna.Framework;
using StardewValley.Objects.Trinkets;
using StardewValley;
using StardewValley.Monsters;
using TrinketTinker.Models;
using TrinketTinker.Companions;
using TrinketTinker.Effects.Abilities;
using StardewModdingAPI;

namespace TrinketTinker.Effects
{
    /// <summary>Base class for TrinketTinker trinkets, allows extensible companions with extensible abilities.</summary>
    public class TrinketTinkerEffect : TrinketEffect
    {
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

        /// <summary>Constructor</summary>
        /// <param name="trinket"></param>
        public TrinketTinkerEffect(Trinket trinket)
            : base(trinket)
        {
            ModEntry.CompanionData.TryGetValue(trinket.ItemId, out Data);
            Abilities = new();
            SortedAbilities = new();
            if (Data != null)
            {
                int lvl = 0;
                // Abilities
                foreach (List<AbilityData> abList in Data.Abilities)
                {
                    List<Ability> lvlAbility = new();
                    Dictionary<ProcOn, List<Ability>> lvlSorted = new();
                    foreach (ProcOn actv in Enum.GetValues(typeof(ProcOn)))
                    {
                        lvlSorted[actv] = new List<Ability>();
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

            // Companion
            if (ModEntry.TryGetType(Data.CompanionClass, out Type? companionCls))
                Companion = (TrinketTinkerCompanion?)Activator.CreateInstance(companionCls, Trinket.ItemId);
            else
                Companion = new TrinketTinkerCompanion(Trinket.ItemId);
            farmer.AddCompanion(Companion);

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

            foreach (var ability in Abilities[Level])
            {
                ability.Deactivate(farmer);
            }
        }

        /// <summary>When item is used (???).</summary>
        /// <param name="farmer"></param>
        public override void OnUse(Farmer farmer)
        {
            // foreach (var ability in SortedAbilities[Level][ProcOn.Use])
            // {
            //     ability.Proc(farmer);
            // }
        }

        /// <summary>When player takes a step (moves).</summary>
        /// <param name="farmer"></param>
        public override void OnFootstep(Farmer farmer)
        {
            if (farmer != Game1.player)
                return;
            foreach (var ability in SortedAbilities[Level][ProcOn.Footstep])
            {
                ability.Proc(farmer);
            }
        }

        /// <summary>When player takes damage.</summary>
        /// <param name="farmer"></param>
        /// <param name="damageAmount"></param>
        public override void OnReceiveDamage(Farmer farmer, int damageAmount)
        {
            if (farmer != Game1.player)
                return;
            foreach (var ability in SortedAbilities[Level][ProcOn.ReceiveDamage])
            {
                ability.Proc(farmer, damageAmount);
            }
        }

        /// <summary>When player deals damage to a monster.</summary>
        /// <param name="farmer"></param>
        /// <param name="monster"></param>
        /// <param name="damageAmount"></param>
        public override void OnDamageMonster(Farmer farmer, Monster monster, int damageAmount)
        {
            if (farmer != Game1.player || monster == null)
                return;
            foreach (var ability in SortedAbilities[Level][ProcOn.DamageMonster])
            {
                ability.Proc(farmer, monster, damageAmount);
            }
            if (monster.Health <= 0)
            {
                foreach (var ability in SortedAbilities[Level][ProcOn.SlayMonster])
                {
                    ability.Proc(farmer, monster, damageAmount);
                }
            }
        }

        /// <summary>When the trinket action should run.</summary>
        /// <param name="farmer"></param>
        /// <param name="monster"></param>
        /// <param name="damageAmount"></param>
        public virtual void OnTrigger(Farmer farmer)
        {
            foreach (var ability in SortedAbilities[Level][ProcOn.Trigger])
            {
                ability.Proc(farmer);
            }
        }

        /// <summary>Update tick</summary>
        /// <param name="farmer"></param>
        /// <param name="time"></param>
        /// <param name="location"></param>
        public override void Update(Farmer farmer, GameTime time, GameLocation location)
        {
            if (farmer != Game1.player)
                return;
            foreach (var ability in Abilities[Level])
            {
                ability.Update(farmer, time, location);
            }
        }

        /// <summary>Randomize this trinket's stats through anvil.</summary>
        /// <param name="trinket"></param>
        public override bool GenerateRandomStats(Trinket trinket)
        {
            if (Data == null)
                return false;
            if (Data.MinLevel == Data.MaxLevel)
                GeneralStat = Data.MinLevel;
            Random r = Utility.CreateRandom(trinket.generationSeed.Value);
            GeneralStat = r.Next(Data.MinLevel, Data.MaxLevel + 1);
            trinket.descriptionSubstitutionTemplates.Add(GeneralStat.ToString());
            return true;
        }
    }
}
