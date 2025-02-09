using System.Collections.Immutable;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Companions;
using StardewValley.Delegates;
using StardewValley.Inventories;
using StardewValley.Monsters;
using StardewValley.Objects.Trinkets;
using StardewValley.TokenizableStrings;
using TrinketTinker.Companions;
using TrinketTinker.Effects.Abilities;
using TrinketTinker.Effects.Support;
using TrinketTinker.Extras;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;
using TrinketTinker.Wheels;

namespace TrinketTinker.Effects;

/// <summary>Base class for TrinketTinker trinkets, allows extensible companions with extensible abilities.</summary>
public class TrinketTinkerEffect : TrinketEffect
{
    /// <summary>Miliseconds</summary>
    public const double IN_COMBAT_CD = 10000;

    /// <summary>ModData variant key</summary>
    public static readonly string ModData_Variant = $"{ModEntry.ModId}/Variant";

    /// <summary>ModData variant key</summary>
    public static readonly string ModData_Level = $"{ModEntry.ModId}/Level";

    /// <summary>ModData inventory guid</summary>
    public static readonly string ModData_Inventory = $"{ModEntry.ModId}/Inventory";

    /// <summary>ModData inventory guid</summary>
    public static readonly string ModData_Enabled = $"{ModEntry.ModId}/Enabled";

    /// <summary>Companion data with matching ID</summary>
    internal TinkerData? Data;
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

    /// <summary>Companion bounding box.</summary>
    public Rectangle CompanionBoundingBox
    {
        get
        {
            if (Companion is TrinketTinkerCompanion cmp)
                return cmp.BoundingBox;
            return Rectangle.Empty;
        }
    }

    /// <summary>Companion bounding box.</summary>
    public ChatterSpeaker? CompanionSpeaker
    {
        get
        {
            if (Companion is TrinketTinkerCompanion cmp)
                return cmp.Speaker;
            return null;
        }
    }

    /// <summary>Number of ability levels</summary>
    public int MaxLevel => Data?.Abilities.Count ?? 0;

    /// <summary>Level is GeneralStat</summary>
    public int Level => GeneralStat;

    /// <summary>Number of variant levels</summary>
    public int MaxVariant => Data?.Variants.Count ?? 0;

    /// <summary>Variant is tracked by mod data as string, parse it here</summary>
    public int Variant
    {
        get
        {
            if (
                Trinket.modData.TryGetValue(ModData_Variant, out string variantStr)
                && int.TryParse(variantStr, out int variant)
            )
                return variant;
            return 0;
        }
    }

    /// <summary>Backing inventory id string</summary>
    private string? inventoryId;

    /// <summary>Inventory Id, for use in <see cref="GlobalInventoryHandler"/></summary>
    public string? InventoryId
    {
        get
        {
            if (Data?.Inventory == null)
                return null;
            if (inventoryId != null)
                return inventoryId;
            if (Trinket.modData.TryGetValue(ModData_Inventory, out inventoryId))
                return inventoryId;
            inventoryId = Guid.NewGuid().ToString();
            Trinket.modData[ModData_Inventory] = inventoryId;
            return inventoryId;
        }
    }

    /// <summary>Full ID, including mod id item id and guid</summary>
    public string? FullInventoryId => InventoryId == null ? null : $"{ModEntry.ModId}/{Trinket.ItemId}/{InventoryId}";

    internal Lazy<GlobalInventoryHandler?> InvHandler =>
        new(() => Data?.Inventory == null ? null : new(this, Data.Inventory, FullInventoryId!));

    /// <summary>Track if this trinket is enabled (for local player)</summary>
    internal bool? enabledLocal = null;

    /// <summary>Track if this trinket is enabled, backed by modData for coop</summary>
    internal bool Enabled
    {
        get => enabledLocal != null ? (enabledLocal ?? false) : Trinket.modData.ContainsKey(ModData_Enabled);
        set
        {
            enabledLocal = value;
            if (value)
                Trinket.modData[ModData_Enabled] = "T";
            else
                Trinket.modData.Remove(ModData_Enabled);
        }
    }

    internal bool CheckEnabled(Farmer owner) =>
        GameStateQuery.CheckConditions(Data?.EnableCondition, player: owner, inputItem: Trinket, targetItem: Trinket);

    internal bool CheckCanOpenInventory(Farmer owner) =>
        Data?.Inventory != null
        && GameStateQuery.CheckConditions(
            Data.Inventory.OpenCondition,
            player: owner,
            inputItem: Trinket,
            targetItem: Trinket
        );

    /// <summary>Check if this trinket has an equip ability</summary>
    internal bool HasEquipTrinketAbility => Abilities.Any((ab) => ab is EquipTrinketAbility);

    /// <summary>timer for checking combat status</summary>
    private double inCombatTimer = -1;

    /// <summary>Is in combat (have dealt/taken damage in past <see cref="IN_COMBAT_CD"/> miliseconds</summary>
    internal bool InCombat => inCombatTimer > 0;

    /// <summary>Queue'd next chatter key</summary>
    internal string? NextChatterKey { get; set; } = null;

    internal event EventHandler<ProcEventArgs>? EventFootstep;
    internal event EventHandler<ProcEventArgs>? EventReceiveDamage;
    internal event EventHandler<ProcEventArgs>? EventDamageMonster;
    internal event EventHandler<ProcEventArgs>? EventSlayMonster;
    internal event EventHandler<ProcEventArgs>? EventTrigger;
    internal event EventHandler<ProcEventArgs>? EventPlayerWarped;
    internal event EventHandler<ProcEventArgs>? EventInteract;

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
                ModEntry.LogOnce(
                    $"No abilities defined for level {GeneralStat}, default to highest level ({Data.Abilities.Count - 1})",
                    LogLevel.Warn
                );
                levelAbilities = Data.Abilities[Data.Abilities.Count - 1];
            }
            else
            {
                levelAbilities = Data.Abilities[GeneralStat];
            }
            int idx = 0;
            bool foundEquipTrinketAbility = false;
            foreach (AbilityData ab in levelAbilities)
            {
                if (Reflect.TryGetType(ab.AbilityClass, out Type? abilityType, TinkerConst.ABILITY_CLS))
                {
                    if (abilityType == typeof(EquipTrinketAbility))
                    {
                        if (foundEquipTrinketAbility)
                            continue;
                        foundEquipTrinketAbility = true;
                    }
                    IAbility? ability = (IAbility?)Activator.CreateInstance(abilityType, this, ab, GeneralStat);
                    if (ability != null && ability.Valid)
                        initAblities.Add(ability);
                    else
                        ModEntry.LogOnce(
                            $"Skip invalid ability ({ab.AbilityClass}-{GeneralStat}:{idx}, from {Trinket.QualifiedItemId})",
                            LogLevel.Warn
                        );
                }
                else
                {
                    ModEntry.LogOnce(
                        $"Failed to get type for ability ({ab.AbilityClass}-{GeneralStat}:{idx}, from {Trinket.QualifiedItemId})",
                        LogLevel.Warn
                    );
                }
                idx++;
            }
        }
        return initAblities.ToImmutableList();
    }

    public void SetOneshotClip(string? clipKey)
    {
        if (Companion is TrinketTinkerCompanion cmp)
            cmp.OneshotKey = clipKey;
    }

    public void SetSpeechBubble(string speechBubbleKey)
    {
        if (Companion is TrinketTinkerCompanion cmp)
            cmp.SetSpeechBubble(speechBubbleKey);
    }

    public void SetAltVariant(string altVariantKey)
    {
        if (Companion is TrinketTinkerCompanion cmp)
            cmp._altVariantKey.Value = altVariantKey;
    }

    /// <summary>Spawn the companion, and activate all abilities</summary>
    /// <param name="farmer"></param>
    public override void Apply(Farmer farmer)
    {
        if (Data == null || Game1.gameMode != 3)
            return;

        Enabled = CheckEnabled(farmer);
        if (!Enabled)
        {
            string failMessage =
                Data.EnableFailMessage != null
                    ? string.Format(TokenParser.ParseText(Data.EnableFailMessage), Trinket.DisplayName)
                    : I18n.Effect_NotAllowed(trinketName: Trinket.DisplayName);
            Game1.addHUDMessage(new HUDMessage(failMessage) { messageSubject = Trinket });
            return;
        }

        int variant = 0;
        if (Trinket.modData.TryGetValue(ModData_Variant, out string variantStr))
            variant = int.Parse(variantStr);

        // Companion
        if (Data.Variants.Count > 0 && Data.Motion != null)
        {
            Companion ??= new TrinketTinkerCompanion(Trinket.ItemId, variant);
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
        if (Companion is TrinketTinkerCompanion ttCompanion)
        {
            ttCompanion.SetActiveAnchors(Abilities.Select((ab) => ab.AbilityClass));
        }
    }

    /// <summary>Remove the companion, and deactivate all abilities</summary>
    /// <param name="farmer"></param>
    public override void Unapply(Farmer farmer)
    {
        if (!Enabled)
            return;
        Enabled = false;

        if (Companion is TrinketTinkerCompanion myTTCmp)
        {
            farmer.RemoveCompanion(myTTCmp);
            Companion = null;
        }

        if (farmer != Game1.player || MaxLevel <= GeneralStat)
            return;

        foreach (IAbility ability in Abilities)
        {
            ability.Deactivate(farmer);
        }
    }

    public override void OnUse(Farmer farmer)
    {
        if (
            Game1.activeClickableMenu == null
            && Data?.Inventory != null
            && InvHandler.Value != null
            && GameStateQuery.CheckConditions(Data.Inventory.OpenCondition, player: farmer, inputItem: Trinket)
        )
        {
            Game1.activeClickableMenu = InvHandler.Value.GetMenu();
        }
    }

    public override void OnFootstep(Farmer farmer)
    {
        if (!Enabled)
            return;

        EventFootstep?.Invoke(this, new(ProcOn.Footstep, farmer));
    }

    public override void OnReceiveDamage(Farmer farmer, int damageAmount)
    {
        if (!Enabled)
            return;

        EventReceiveDamage?.Invoke(this, new(ProcOn.ReceiveDamage, farmer) { DamageAmount = damageAmount });
        inCombatTimer = IN_COMBAT_CD;
    }

    public override void OnDamageMonster(
        Farmer farmer,
        Monster monster,
        int damageAmount,
        bool isBomb,
        bool isCriticalHit
    )
    {
        if (!Enabled)
            return;

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
        {
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
            if (farmer.currentLocation?.characters.Any((chara) => chara is Monster m && m != monster) ?? false)
                return;
        }
        inCombatTimer = IN_COMBAT_CD;
    }

    /// <summary>Handle the trigger.</summary>
    /// <param name="farmer"></param>
    /// <param name="monster"></param>
    /// <param name="damageAmount"></param>
    public virtual void OnTrigger(Farmer farmer, string[] args, TriggerActionContext context)
    {
        if (!Enabled)
            return;

        EventTrigger?.Invoke(this, new(ProcOn.Trigger, farmer) { TriggerArgs = args, TriggerContext = context });
    }

    public virtual void OnPlayerWarped(Farmer farmer, GameLocation oldLocation, GameLocation newLocation)
    {
        if (!Enabled)
            return;

        EventPlayerWarped?.Invoke(this, new(ProcOn.Warped, farmer));
        inCombatTimer = 0;
    }

    public virtual void OnButtonsChanged(Farmer farmer, ButtonsChangedEventArgs e)
    {
        if (
            !Enabled
            || Companion is not TrinketTinkerCompanion
            || Game1.activeClickableMenu != null
            || farmer.UsingTool
            || farmer.usingSlingshot
        )
            return;
        if (ModEntry.Config.DoInteractKey.JustPressed() && farmer.GetBoundingBox().Intersects(CompanionBoundingBox))
        {
            EventInteract?.Invoke(this, new(ProcOn.Interact, farmer));
        }
    }

    /// <summary>Update every tick. Not an event because this happens for every ability regardless of <see cref="ProcOn"/>.</summary>
    /// <param name="farmer"></param>
    /// <param name="time"></param>
    /// <param name="location"></param>
    public override void Update(Farmer farmer, GameTime time, GameLocation location)
    {
        if (!Enabled)
            return;

        foreach (IAbility ability in Abilities)
            ability.Update(farmer, time, location);
        if (inCombatTimer > 0)
            inCombatTimer -= time.ElapsedGameTime.TotalMilliseconds;
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
        if (
            trinket.modData.TryGetValue(ModData_Variant, out string variantStr)
            && int.TryParse(variantStr, out int variant)
        )
            SetVariant(trinket, variant);
        else
            SetVariant(trinket, 0);
        return false;
    }

    /// <summary>Get the maximum allowed count from list of GSQ.</summary>
    /// <param name="conditions"></param>
    /// <param name="count"></param>
    /// <param name="trinket"></param>
    /// <returns></returns>
    private static int GetMaxUnlockedCount(IList<string?> conditions, int count, Trinket trinket)
    {
        if (conditions.Count == count - 1)
            return count;
        for (int result = 0; result < count; result++)
        {
            if (conditions.Count <= result)
                return count;
            if (
                !GameStateQuery.CheckConditions(conditions[result], null, null, inputItem: trinket, targetItem: trinket)
            )
                return result + 1;
        }
        return 1;
    }

    /// <summary>Get max unlocked level by GSQ</summary>
    /// <param name="trinket"></param>
    /// <returns></returns>
    public int GetMaxUnlockedLevel(Trinket trinket)
    {
        if (Data == null)
            return 0;
        return GetMaxUnlockedCount(Data.AbilityUnlockConditions, Data.Abilities.Count, trinket);
    }

    /// <summary>Get max unlocked variant by GSQ</summary>
    /// <param name="trinket"></param>
    /// <returns></returns>
    public int GetMaxUnlockedVariant(Trinket trinket)
    {
        if (Data == null)
            return 0;
        return GetMaxUnlockedCount(Data.VariantUnlockConditions, Data.Variants.Count, trinket);
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
        int maxAbility = GetMaxUnlockedLevel(trinket);
        if (maxAbility <= 1)
        {
            return SetLevel(trinket, 0);
        }
        int newStat = Random.Shared.Next(maxAbility - 1);
        if (newStat >= previous)
            newStat++;
        return SetLevel(trinket, newStat);
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
        int maxVariant = GetMaxUnlockedVariant(trinket);
        if (maxVariant <= 1)
            return SetVariant(trinket, 0);
        int newVariant = Random.Shared.Next(maxVariant - 1);
        if (newVariant >= previous)
            newVariant++;
        return SetVariant(trinket, newVariant);
    }

    /// <summary>Set level</summary>
    /// <param name="trinket"></param>
    /// <param name="generalStat"></param>
    public bool SetLevel(Trinket trinket, int generalStat)
    {
        if (Data == null || Data.Abilities.Count == 0)
            return false;
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
        return true;
    }

    /// <summary>Set trinket variant</summary>
    /// <param name="trinket"></param>
    /// <param name="variant"></param>
    public bool SetVariant(Trinket trinket, int variant)
    {
        if (Data == null || Data.Variants.Count == 0)
            return false;
        if (variant >= Data.Variants.Count)
            variant = 0;
        trinket.modData[ModData_Variant] = variant.ToString();
        VariantData variantData = Data.Variants[variant];
        if (variantData.TrinketSpriteIndex > 0)
            trinket.ParentSheetIndex = variantData.TrinketSpriteIndex;
        if (variantData.TrinketNameArguments != null)
        {
            trinket.displayNameOverrideTemplate.Value = string.Format(
                TokenParser.ParseText(trinket.GetTrinketData()?.DisplayName) ?? trinket.DisplayName,
                variantData.TrinketNameArguments.Select((txt) => TokenParser.ParseText(txt) ?? txt).ToArray()
            );
        }

        return true;
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
            SetVariant(trinket, int.Parse(variantStr));
        }
    }

    /// <summary>Get inventory of trinket, if there is one</summary>
    /// <param name="farmer"></param>
    /// <returns></returns>
    public Inventory? GetInventory(Farmer farmer)
    {
        if (
            InventoryId != null
            && farmer.team.globalInventories.TryGetValue(FullInventoryId, out Inventory? trinketInv)
        )
            return trinketInv;
        return null;
    }
}
