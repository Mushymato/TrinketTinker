using Microsoft.Xna.Framework;
using StardewValley.Objects.Trinkets;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Inventories;
using TrinketTinker.Models;
using TrinketTinker.Companions;
using TrinketTinker.Effects.Abilities;
using StardewModdingAPI;
using TrinketTinker.Models.Mixin;
using TrinketTinker.Effects.Proc;
using System.Collections.Immutable;
using StardewValley.Delegates;

namespace TrinketTinker.Effects
{
    /// <summary>Base class for TrinketTinker trinkets, allows extensible companions with extensible abilities.</summary>
    public class TrinketTinkerEffect : TrinketEffect
    {
        public readonly string ModData_WhichVariant = $"{ModEntry.ModId}/WhichVariant";
        /// <summary>Companion data with matching ID</summary>
        protected TinkerData? Data;
        private readonly Lazy<ImmutableList<IAbility>> abilities;
        /// <summary>Abilities for this trinket.</summary>
        protected ImmutableList<IAbility> Abilities => abilities.Value;

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
        internal event EventHandler<ProcEventArgs>? EventUse;
        internal event EventHandler<ProcEventArgs>? EventFootstep;
        internal event EventHandler<ProcEventArgs>? EventReceiveDamage;
        internal event EventHandler<ProcEventArgs>? EventDamageMonster;
        internal event EventHandler<ProcEventArgs>? EventSlayMonster;
        internal event EventHandler<ProcEventArgs>? EventTrigger;

        /// <summary>Constructor</summary>
        /// <param name="trinket"></param>
        public TrinketTinkerEffect(Trinket trinket)
            : base(trinket)
        {
            ModEntry.CompanionData.TryGetValue(trinket.ItemId, out Data);
            abilities = new(InitAbilities, false);
        }

        /// <summary>
        /// Lazy init of abilities, which depend on <see cref="GeneralStat"/> being set
        /// </summary>
        /// <returns></returns>
        private ImmutableList<IAbility> InitAbilities()
        {
            List<IAbility> initAblities = [];
            if (Data != null)
            {
                List<AbilityData> levelAbilities;
                if (GeneralStat > Data.Abilities.Count)
                {
                    ModEntry.Log($"No abilities defined for level {GeneralStat}, default to highest level ({Data.Abilities.Count - 1})", LogLevel.Warn);
                    levelAbilities = Data.Abilities.Last();
                }
                else
                {
                    levelAbilities = Data.Abilities[GeneralStat];
                }
                foreach (AbilityData ab in levelAbilities)
                {
                    if (ModEntry.TryGetType(ab.AbilityClass, out Type? abilityType, Constants.ABILITY_CLS))
                    {
                        IAbility? ability = (IAbility?)Activator.CreateInstance(abilityType, this, ab, GeneralStat);
                        if (ability != null && ability.Valid)
                            initAblities.Add(ability);
                        else
                            ModEntry.Log($"Skip invalid ability ({ab.AbilityClass} from {Trinket.QualifiedItemId})", LogLevel.Warn);
                    }
                    else
                    {
                        ModEntry.Log($"Failed to get type for ability ({ab.AbilityClass} from {Trinket.QualifiedItemId})", LogLevel.Warn);
                    }
                }
            }
            return initAblities.ToImmutableList();
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

            // Only activate ability for local player
            if (Game1.player != farmer)
                return;

            foreach (IAbility ability in Abilities)
            {
                ability.Activate(farmer);
            }
        }

        /// <summary>Remove the companion, and deactivate all abilities</summary>
        /// <param name="farmer"></param>
        public override void Unapply(Farmer farmer)
        {
            farmer.RemoveCompanion(Companion);

            if (farmer != Game1.player || Abilities.Count <= GeneralStat)
                return;

            foreach (IAbility ability in Abilities)
            {
                ability.Deactivate(farmer);
            }
        }

        public override void OnUse(Farmer farmer)
        {
            EventUse?.Invoke(this, new(ProcOn.Use)
            {
                Farmer = farmer
            });
        }

        public override void OnFootstep(Farmer farmer)
        {
            EventFootstep?.Invoke(this, new(ProcOn.Footstep)
            {
                Farmer = farmer
            });
        }

        public override void OnReceiveDamage(Farmer farmer, int damageAmount)
        {
            EventReceiveDamage?.Invoke(this, new(ProcOn.ReceiveDamage)
            {
                Farmer = farmer,
                DamageAmount = damageAmount
            });
        }

        public override void OnDamageMonster(Farmer farmer, Monster monster, int damageAmount, bool isBomb, bool isCriticalHit)
        {
            EventDamageMonster?.Invoke(this, new(ProcOn.DamageMonster)
            {
                Farmer = farmer,
                Monster = monster,
                DamageAmount = damageAmount,
                IsBomb = isBomb,
                IsCriticalHit = isCriticalHit
            });
            if (monster.Health <= 0)
                EventSlayMonster?.Invoke(this, new(ProcOn.SlayMonster)
                {
                    Farmer = farmer,
                    Monster = monster,
                    DamageAmount = damageAmount,
                    IsBomb = isBomb,
                    IsCriticalHit = isCriticalHit
                });
        }

        /// <summary>Handle the trigger.</summary>
        /// <param name="farmer"></param>
        /// <param name="monster"></param>
        /// <param name="damageAmount"></param>
        public virtual void OnTrigger(Farmer farmer, string[] args, TriggerActionContext context)
        {
            EventTrigger?.Invoke(this, new(ProcOn.Trigger)
            {
                Farmer = farmer,
                TriggerArgs = args,
                TriggerContext = context
            });
        }

        /// <summary>Update every tick. Not an event because this happens for every ability regardless of <see cref="ProcOn"/>.</summary>
        /// <param name="farmer"></param>
        /// <param name="time"></param>
        /// <param name="location"></param>
        public override void Update(Farmer farmer, GameTime time, GameLocation location)
        {
            foreach (IAbility ability in Abilities)
                ability.Update(farmer, time, location);
        }

        public override bool GenerateRandomStats(Trinket trinket)
        {
            if (Data == null)
                return false;
            if (Data.Abilities.Count <= 1)
            {
                GeneralStat = 0;
            }
            else
            {
                Random r = Utility.CreateRandom(trinket.generationSeed.Value);
                GeneralStat = r.Next(Data.Abilities.Count);
            }
            trinket.descriptionSubstitutionTemplates.Clear();
            trinket.descriptionSubstitutionTemplates.Add((Data.MinLevel + GeneralStat).ToString());
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
