using System.Collections.Immutable;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Monsters;
using StardewValley.Objects.Trinkets;
using TrinketTinker.Companions;
using TrinketTinker.Effects.Abilities;
using TrinketTinker.Effects.Proc;
using TrinketTinker.Models;
using TrinketTinker.Wheels;

namespace TrinketTinker.Effects;

/// <summary>Base class for TrinketTinker trinkets, allows extensible companions with extensible abilities.</summary>
public class TrinketTinkerEffect : TrinketEffect
{
    /// <summary></summary>
    public static readonly string ModData_Variant = $"{ModEntry.ModId}/Variant";
    public static readonly string ModData_Level = $"{ModEntry.ModId}/Level";

    /// <summary>Companion data with matching ID</summary>
    protected TinkerData? Data;
    private readonly Lazy<ImmutableList<IAbility>> abilities;

    /// <summary>Abilities for this trinket.</summary>
    internal ImmutableList<IAbility> Abilities => abilities.Value;

    /// <summary>Position of companion, including offset if applicable.</summary>
    public Vector2? CompanionPosition
    {
        get
        {
            if (Companion is TrinketTinkerCompanion cmp)
                return cmp.Position + cmp.Offset;
            return null;
        }
    }

    /// <summary>Anchor position of companion.</summary>
    public Vector2? CompanionAnchor
    {
        get
        {
            if (Companion is TrinketTinkerCompanion cmp)
                return cmp.Anchor;
            return null;
        }
    }

    /// <summary>Draw layer of owner.</summary>
    public float CompanionOwnerDrawLayer => Companion.Owner.getDrawLayer();

    /// <summary>Number of ability levels</summary>
    public int MaxLevel => Data?.Abilities.Count ?? 0;

    /// <summary>Number of variant levels</summary>
    public int MaxVariant => Data?.Variants.Count ?? 0;

    internal event EventHandler<ProcEventArgs>? EventFootstep;
    internal event EventHandler<ProcEventArgs>? EventReceiveDamage;
    internal event EventHandler<ProcEventArgs>? EventDamageMonster;
    internal event EventHandler<ProcEventArgs>? EventSlayMonster;
    internal event EventHandler<ProcEventArgs>? EventTrigger;
    internal event EventHandler<ProcEventArgs>? EventPlayerWarped;

    /// <summary>Constructor</summary>
    /// <param name="trinket"></param>
    public TrinketTinkerEffect(Trinket trinket)
        : base(trinket)
    {
        AssetManager.TinkerData.TryGetValue(trinket.ItemId, out Data);
        abilities = new(InitAbilities, false);
    }

    /// <summary>
    /// Lazy init of abilities, which depend on <see cref="GeneralStat"/> being set
    /// </summary>
    /// <returns></returns>
    private ImmutableList<IAbility> InitAbilities()
    {
        List<IAbility> initAblities = [];
        if (Data != null && Data.Abilities.Count != 0)
        {
            List<AbilityData> levelAbilities;
            if (GeneralStat > Data.Abilities.Count)
            {
                ModEntry.Log(
                    $"No abilities defined for level {GeneralStat}, default to highest level ({Data.Abilities.Count - 1})",
                    LogLevel.Warn
                );
                levelAbilities = Data.Abilities.Last();
            }
            else
            {
                levelAbilities = Data.Abilities[GeneralStat];
            }
            foreach (AbilityData ab in levelAbilities)
            {
                if (Reflect.TryGetType(ab.AbilityClass, out Type? abilityType, TinkerConst.ABILITY_CLS))
                {
                    IAbility? ability = (IAbility?)Activator.CreateInstance(abilityType, this, ab, GeneralStat);
                    if (ability != null && ability.Valid)
                        initAblities.Add(ability);
                    else
                        ModEntry.Log(
                            $"Skip invalid ability ({ab.AbilityClass} from {Trinket.QualifiedItemId})",
                            LogLevel.Warn
                        );
                }
                else
                {
                    ModEntry.Log(
                        $"Failed to get type for ability ({ab.AbilityClass} from {Trinket.QualifiedItemId})",
                        LogLevel.Warn
                    );
                }
            }
        }
        return initAblities.ToImmutableList();
    }

    public void SetOneshotClip(string? clipKey)
    {
        if (Companion is TrinketTinkerCompanion cmp)
            cmp.OneshotKey = clipKey;
    }

    /// <summary>Spawn the companion, and activate all abilities</summary>
    /// <param name="farmer"></param>
    public override void Apply(Farmer farmer)
    {
        if (Data == null || Game1.gameMode != 3)
            return;

        int variant = 0;
        if (Trinket.modData.TryGetValue(ModData_Variant, out string variantStr))
            variant = int.Parse(variantStr);

        // Companion
        if (Data.Variants.Count > 0 && Data.Motions.Count > 0)
        {
            Companion = new TrinketTinkerCompanion(Trinket.ItemId, variant);
            farmer.AddCompanion(Companion);
        }
        else
        {
            Companion = null;
        }

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
        if (Companion != null)
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
        // EventUse?.Invoke(this, new(ProcOn.Use, farmer));
    }

    public override void OnFootstep(Farmer farmer)
    {
        EventFootstep?.Invoke(this, new(ProcOn.Footstep, farmer));
    }

    public override void OnReceiveDamage(Farmer farmer, int damageAmount)
    {
        EventReceiveDamage?.Invoke(this, new(ProcOn.ReceiveDamage, farmer) { DamageAmount = damageAmount });
    }

    public override void OnDamageMonster(
        Farmer farmer,
        Monster monster,
        int damageAmount,
        bool isBomb,
        bool isCriticalHit
    )
    {
        EventDamageMonster?.Invoke(
            this,
            new(ProcOn.DamageMonster, farmer)
            {
                Monster = monster,
                DamageAmount = damageAmount,
                IsBomb = isBomb,
                IsCriticalHit = isCriticalHit,
            }
        );
        if (monster.Health <= 0)
            EventSlayMonster?.Invoke(
                this,
                new(ProcOn.SlayMonster, farmer)
                {
                    Monster = monster,
                    DamageAmount = damageAmount,
                    IsBomb = isBomb,
                    IsCriticalHit = isCriticalHit,
                }
            );
    }

    /// <summary>Handle the trigger.</summary>
    /// <param name="farmer"></param>
    /// <param name="monster"></param>
    /// <param name="damageAmount"></param>
    public virtual void OnTrigger(Farmer farmer, string[] args, TriggerActionContext context)
    {
        EventTrigger?.Invoke(this, new(ProcOn.Trigger, farmer) { TriggerArgs = args, TriggerContext = context });
    }

    public virtual void OnPlayerWarped(Farmer farmer, GameLocation oldLocation, GameLocation newLocation)
    {
        EventPlayerWarped?.Invoke(this, new(ProcOn.Warped, farmer));
    }

    /// <summary>Update every tick. Not an event because this happens for every ability regardless of <see cref="Proc"/>.</summary>
    /// <param name="farmer"></param>
    /// <param name="time"></param>
    /// <param name="location"></param>
    public override void Update(Farmer farmer, GameTime time, GameLocation location)
    {
        foreach (IAbility ability in Abilities)
            ability.Update(farmer, time, location);
    }

    /// <summary>
    /// For some reason random stats here get rolled again whenever the sprite is reloaded, prefer not to do that.
    /// </summary>
    /// <param name="trinket"></param>
    /// <returns></returns>
    public override bool GenerateRandomStats(Trinket trinket)
    {
        if (trinket.modData.TryGetValue(ModData_Level, out string levelStr) && int.TryParse(levelStr, out int level))
            SetLevel(trinket, level);
        else
            SetLevel(trinket, 0);
        return false;
    }

    /// <summary>Get the maximum allowed count from list of GSQ.</summary>
    /// <param name="conditions"></param>
    /// <param name="count"></param>
    /// <param name="trinket"></param>
    /// <returns></returns>
    public static int GetMaxUnlockedCount(List<string?> conditions, int count, Trinket trinket)
    {
        if (conditions.Count == count - 1)
            return count;
        for (int result = 0; result < count; result++)
        {
            if (conditions.Count <= result)
                return count;
            if (!GameStateQuery.CheckConditions(conditions[result], null, null, trinket))
                return result + 1;
        }
        return 1;
    }

    /// <summary>
    /// Randomize this trinket's ability level through anvil, return true if the level is rerolled.
    /// Will not roll the same level.
    /// </summary>
    /// <param name="trinket"></param>
    /// <returns></returns>
    public virtual bool RerollLevel(Trinket trinket, int previous)
    {
        if (Data == null)
            return false;
        int maxAbility = GetMaxUnlockedCount(Data.AbilityUnlockConditions, Data.Abilities.Count, trinket);
        if (maxAbility <= 1)
        {
            SetLevel(trinket, 0);
            return false;
        }
        int newStat = Random.Shared.Next(maxAbility - 1);
        if (newStat >= previous)
            newStat++;
        SetLevel(trinket, newStat);
        return true;
    }

    /// <summary>
    /// Randomize this trinket's variant through trinket colorizer, return true if the variant is rerolled.
    /// Will not roll the same variant.
    /// </summary>
    /// <param name="trinket"></param>
    public virtual bool RerollVariant(Trinket trinket, int previous)
    {
        if (Data == null)
            return false;
        int maxVariant = GetMaxUnlockedCount(Data.VariantUnlockConditions, Data.Variants.Count, trinket);
        if (maxVariant <= 1)
        {
            SetVariant(trinket, 0);
            return false;
        }
        int newVariant = Random.Shared.Next(maxVariant - 1);
        if (newVariant >= previous)
            newVariant++;
        SetVariant(trinket, newVariant);
        return true;
    }

    /// <summary>Set level</summary>
    /// <param name="trinket"></param>
    /// <param name="generalStat"></param>
    public void SetLevel(Trinket trinket, int generalStat)
    {
        if (Data == null || Data.Abilities.Count == 0)
            return;
        if (generalStat >= Data.Abilities.Count)
            generalStat = 0;
        GeneralStat = generalStat;
        trinket.modData[ModData_Level] = GeneralStat.ToString();
        trinket.descriptionSubstitutionTemplates.Clear();
        trinket.descriptionSubstitutionTemplates.Add((Data.MinLevel + GeneralStat).ToString());
        trinket.descriptionSubstitutionTemplates.Add(
            string.Join(
                '\n',
                Data.Abilities[GeneralStat].Where((ab) => ab.Description != null).Select((ab) => ab.Description)
            )
        );
        return;
    }

    /// <summary>Set trinket variant</summary>
    /// <param name="trinket"></param>
    /// <param name="variant"></param>
    public void SetVariant(Trinket trinket, int variant)
    {
        if (Data == null || Data.Variants.Count == 0)
            return;
        if (variant >= Data.Variants.Count)
            variant = 0;
        trinket.modData[ModData_Variant] = variant.ToString();
        if (Data.Variants[variant].TrinketSpriteIndex > 0)
            trinket.ParentSheetIndex = Data.Variants[variant].TrinketSpriteIndex;
        return;
    }

    /// <summary>Reset trinket variant icon to modData value</summary>
    /// <param name="trinket"></param>
    /// <param name="variant"></param>
    public void ResetVariant(Trinket trinket)
    {
        if (Data == null || Data.Variants.Count == 0)
            return;
        if (trinket.modData.TryGetValue(ModData_Variant, out string variantStr))
        {
            int variant = int.Parse(variantStr);
            if (variant >= Data.Variants.Count)
                variant = 0;
            if (Data.Variants[variant].TrinketSpriteIndex > 0)
                trinket.ParentSheetIndex = Data.Variants[variant].TrinketSpriteIndex;
        }
    }
}
