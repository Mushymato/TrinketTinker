using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mushymato.ExtendedTAS;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Companions;
using StardewValley.Delegates;
using StardewValley.Network;
using TrinketTinker.Companions.Motions;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;
using TrinketTinker.Wheels;

namespace TrinketTinker.Companions;

/// <summary>Main companion class for Trinket Tinker.</summary>
public class TrinketTinkerCompanion : Companion
{
    // NetFields + Getters
    /// <summary>NetField for <see cref="ID"/></summary>
    protected readonly NetString _id = new("");

    /// <summary>Companion ID. Companion is (re)loaded when this is changed.</summary>
    public string ID => _id.Value;

    /// <summary>Owner position in prev tick, for detecting moving</summary>
    private Vector2? prevOwnerPosition;

    /// <summary>Whether owner is moving</summary>
    private bool ownerMoving = false;

    /// <summary>Whether owner is moving and not using a tool</summary>
    public bool OwnerMoving => ownerMoving && !Owner.UsingTool && !Owner.usingSlingshot;

    /// <summary>Companion position in prev tick, for detecting moving</summary>
    private Vector2? prevPosition;

    /// <summary>Whether companion is moving</summary>
    public bool CompanionMoving { get; private set; } = false;

    /// <summary>Net position, exposed internally</summary>
    internal NetPosition NetPosition => _position;

    /// <summary>Net lerp, for niche cases as normally the net position is sufficient</summary>
    private readonly NetFloat _netLerp = new NetFloat(-1f).Interpolated(true, false);

    /// <summary>Lerp motion variable</summary>
    internal float Lerp
    {
        get => _netLerp.Value;
        set => _netLerp.Value = value;
    }

    /// <summary>Seed for anim clip random, to ensure some level of sync without need to update net field</summary>
    private readonly NetInt _netRandSeed = new(Random.Shared.Next());

    /// <summary>Seed for speech bubble random, to ensure some level of sync without need to update net field</summary>
    private readonly NetInt _speechSeed = new(Random.Shared.Next());

    /// <summary>Speech bubble key</summary>
    private readonly NetString _speechBubbleKey = new(null);

    /// <summary>Sub variant key</summary>
    internal readonly NetString _altVariantKey = new(null);

    // Derived

    /// <summary>Marks data associated with this instance as dirty, must reload motion</summary>
    internal PerScreen<bool> IsDirty { get; set; } = new();

    /// <summary>Motion class that controls how the companion moves.</summary>
    public IMotion? Motion { get; private set; }

    /// <summary>Position the companion should follow.</summary>
    public Vector2 Anchor { get; set; }

    /// <summary>Current motion offset</summary>
    public Vector2 Offset => Motion?.GetOffset() ?? Vector2.Zero;

    /// <summary>NetString key of oneshot clip</summary>
    private readonly NetString _oneshotKey = new(null);

    /// <summary>Getter and setter for oneshot key</summary>
    public string? OneshotKey
    {
        get => _oneshotKey.Value;
        set
        {
            if (Motion != null && value != _oneshotKey.Value)
                _oneshotKey.Value = value;
        }
    }

    /// <summary>String key of override clip</summary>
    private readonly NetString _overrideKey = new(null);

    /// <summary>Getter and setter for override key</summary>
    public string? OverrideKey
    {
        get => _overrideKey.Value;
        set
        {
            if (Motion != null && value != _overrideKey.Value)
                _overrideKey.Value = value;
        }
    }

    /// <summary>Should draw in current location, rechecked on warp</summary>
    private readonly NetBool _disableCompanion = new(false);

    /// <summary>Net sync'd currAnchorTarget, for use in anim clip</summary>
    private readonly NetInt _currAnchorTarget = new(0);

    /// <summary>Getter and setter for override key</summary>
    public AnchorTarget CurrAnchorTarget
    {
        get => (AnchorTarget)_currAnchorTarget.Value;
        set => _currAnchorTarget.Value = (int)value;
    }

    /// <summary>Bounding box of companion</summary>
    public Rectangle BoundingBox => Motion?.BoundingBox ?? Rectangle.Empty;

    /// <summary>Speaker data for Chatter ability</summary>
    public ChatterSpeaker? Speaker => Motion?.Speaker;

    /// <summary>A TAS associated with the companion and will move alongside it.</summary>
    internal List<TASContext>? AttachedTAS { get; set; } = null;

    /// <summary>Argumentless constructor for netcode deserialization.</summary>
    public TrinketTinkerCompanion()
        : base() { }

    /// <summary>Construct new companion using companion ID.</summary>
    public TrinketTinkerCompanion(string companionId, int variant)
    {
        // _moving.Value = false;
        whichVariant.Value = variant;
        _id.Value = companionId;
    }

    /// <summary>Initialize Motion class.</summary>
    public override void InitializeCompanion(Farmer farmer)
    {
        base.InitializeCompanion(farmer);
        _disableCompanion.Value = Places.LocationDisableTrinketCompanions(Owner.currentLocation);
        Anchor = Utility.PointToVector2(farmer.GetBoundingBox().Center);
        Motion?.Initialize(farmer);
        if (
            Motion != null
            && AssetManager.TinkerData.TryGetValue(_id.Value, out TinkerData? Data)
            && Data.Motion != null
        )
        {
            InitializeAttachedTAS(Data.Motion);
        }
    }

    /// <summary>Cleanup Motion class.</summary>
    public override void CleanupCompanion()
    {
        base.CleanupCompanion();
        Motion?.Cleanup();
        Motion = null;
    }

    /// <summary>Setup net fields.</summary>
    public override void InitNetFields()
    {
        base.InitNetFields();
        NetFields
            .AddField(_id, "_id")
            .AddField(_oneshotKey, "_oneshotKey")
            .AddField(_overrideKey, "_overrideKey")
            .AddField(_speechBubbleKey, "_speechBubbleText")
            .AddField(_altVariantKey, "_altVariantKey")
            .AddField(_netLerp, "_netLerp")
            .AddField(_disableCompanion, "_disableCompanion")
            .AddField(_netRandSeed, "_netRandSeed")
            .AddField(_speechSeed, "_speechSeed")
            .AddField(_currAnchorTarget, "_currAnchorTarget");
        _id.fieldChangeVisibleEvent += InitCompanionData;
        _oneshotKey.fieldChangeVisibleEvent += (field, oldValue, newValue) => Motion?.SetOneshotClip(newValue);
        _overrideKey.fieldChangeVisibleEvent += (field, oldValue, newValue) => Motion?.SetOverrideClip(newValue);
        _speechBubbleKey.fieldChangeVisibleEvent += (field, oldValue, newValue) => Motion?.SetSpeechBubble(newValue);
        _altVariantKey.fieldChangeVisibleEvent += (field, oldValue, newValue) => Motion?.SetAltVariant(newValue);
        _netRandSeed.fieldChangeVisibleEvent += (field, oldValue, newValue) =>
        {
            if (Motion != null)
                Motion.NetRand = new Random(newValue);
        };
        _currAnchorTarget.fieldChangeVisibleEvent += (NetInt field, int oldValue, int newValue) =>
        {
            if (Motion != null)
            {
                Motion?.SetCurrAnchorTarget(newValue);
            }
        };
    }

    /// <summary>When <see cref="Id"/> is changed through net event, fetch companion data and build all fields.</summary>
    private void InitCompanionData(NetString field, string oldValue, string newValue)
    {
        // _id.Value = newValue;
        if (!AssetManager.TinkerData.TryGetValue(_id.Value, out TinkerData? Data))
        {
            ModEntry.Log($"Failed to get companion data for ${_id.Value}", LogLevel.Error);
            return;
        }

        if (Data?.Motion == null || whichVariant.Value >= Data.Variants.Count)
            return;

        VariantData vdata = Data.Variants[whichVariant.Value];
        MotionData mdata = Data.Motion;
        Motion = CreateMotionInstance(mdata, vdata);
    }

    private void ReloadCompanionData()
    {
        IsDirty.Value = false;
        Motion?.Cleanup();
        ModEntry.Log($"Reload dirty companion {_id.Value}");
        if (!AssetManager.TinkerData.TryGetValue(_id.Value, out TinkerData? Data))
        {
            ModEntry.Log($"Failed to get companion data for ${_id.Value}", LogLevel.Error);
            Motion = null;
            return;
        }
        if (Data?.Motion == null || whichVariant.Value >= Data.Variants.Count)
            return;

        VariantData vdata = Data.Variants[whichVariant.Value];
        MotionData mdata = Data.Motion;
        if (mdata.MotionClass == Motion?.MotionClass)
        {
            Motion.SetMotionVariantData(mdata, vdata);
        }
        else
        {
            Motion = CreateMotionInstance(mdata, vdata);
        }
        if (Motion != null)
        {
            Motion.Initialize(Owner);
            InitializeAttachedTAS(mdata);
        }
    }

    private void SetTASPosition(TemporaryAnimatedSprite tas) => tas.Position = Position;

    private void SetTASPositionWithOffset(TemporaryAnimatedSprite tas) => tas.Position = Position + Offset;

    private void InitializeAttachedTAS(MotionData mdata)
    {
        if (mdata.AttachedTAS != null)
        {
            AttachedTAS = [];
            foreach (string tasId in mdata.AttachedTAS)
            {
                GameStateQueryContext queryContext = new();
                if (AssetManager.TAS.TryGetTASContext(tasId, out TASContext? tasCtx))
                {
                    if (tasCtx.Def.SpawnInterval <= 0)
                    {
                        if (!tasCtx.TryCreate(queryContext, SetTASPosition))
                            continue;
                    }
                    else
                    {
                        tasCtx.TryCreateRespawning(Game1.currentGameTime, queryContext, SetTASPositionWithOffset);
                    }
                    AttachedTAS.Add(tasCtx);
                }
            }
        }
        else
        {
            ModEntry.Log($"AttachedTAS: NULL");
            AttachedTAS = null;
        }
    }

    private IMotion? CreateMotionInstance(MotionData mdata, VariantData vdata)
    {
        if (mdata.MotionClass == null)
        {
            return new LerpMotion(this, mdata, vdata);
        }
        if (Reflect.TryGetType(mdata.MotionClass, out Type? motionCls, TinkerConst.MOTION_CLS))
        {
            return (IMotion?)Activator.CreateInstance(motionCls, this, mdata, vdata);
        }
        ModEntry.LogOnce($"Could not get motion class {mdata.MotionClass}", LogLevel.Error);
        return null;
    }

    /// <summary>Draw using <see cref="Motion"/>.</summary>
    /// <param name="b">SpriteBatch</param>
    public override void Draw(SpriteBatch b)
    {
        if (Owner == null || _disableCompanion.Value)
            return;
        if (!Visuals.ShouldDraw())
            return;

        if (Motion == null)
            return;

        Motion.Draw(b);
        if (AttachedTAS != null)
        {
            Vector2 tasOffset = Offset - Motion.CompanionAnimSprite.Origin * Motion.CompanionAnimSprite.TextureScale;
            foreach (TASContext tasCtx in AttachedTAS)
            {
                foreach (TemporaryAnimatedSprite spawned in tasCtx.Spawned)
                {
                    if (tasCtx.Def.SpawnInterval <= 0)
                    {
                        spawned.draw(b, xOffset: (int)tasOffset.X, yOffset: (int)tasOffset.Y);
                    }
                    else
                    {
                        spawned.draw(b);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Do updates in <see cref="Motion"/>.
    /// The client of the player with the trinket is responsible for calculating position direction rotation.
    /// All clients must update animation frame.
    /// </summary>
    /// <param name="time">Game time</param>
    /// <param name="location">Current map location</param>
    public override void Update(GameTime time, GameLocation location)
    {
        if (IsDirty.Value)
            ReloadCompanionData();

        ownerMoving = prevOwnerPosition != OwnerPosition;
        CompanionMoving = prevPosition != Position;
        prevOwnerPosition = OwnerPosition;
        prevPosition = Position;
        if (IsLocal)
        {
            if (Motion == null)
            {
                Position = Anchor;
            }
            else
            {
                CurrAnchorTarget = Motion.UpdateAnchor(time, location);
                Motion.UpdateLocal(time, location);
            }
        }
        Motion?.UpdateGlobal(time, location);
        Motion?.UpdateLightSource(time, location);
        if (AttachedTAS != null)
        {
            GameStateQueryContext queryContext = new();
            foreach (TASContext tasCtx in AttachedTAS)
            {
                if (tasCtx.Def.SpawnInterval <= 0)
                {
                    foreach (TemporaryAnimatedSprite spawned in tasCtx.Spawned)
                        spawned.Position += Position - (prevPosition ?? Position);
                }
                else
                {
                    tasCtx.TryCreateRespawning(Game1.currentGameTime, queryContext, SetTASPositionWithOffset);
                }
                foreach (TemporaryAnimatedSprite spawned in tasCtx.Spawned)
                {
                    spawned.update(Game1.currentGameTime);
                }
            }
        }
    }

    /// <summary>Reset position and display status on warp</summary>
    public override void OnOwnerWarp()
    {
        base.OnOwnerWarp();
        _position.Value = _owner.Value.Position;
        Motion?.OnOwnerWarp();
        _disableCompanion.Value = Places.LocationDisableTrinketCompanions(Owner.currentLocation);
    }

    /// <summary>Set speech bubble key</summary>
    public void SetSpeechBubble(string? speechBubbleKey)
    {
        _speechBubbleKey.Value = speechBubbleKey;
    }

    /// <summary>Set active anchors list, based on ability types</summary>
    public void SetActiveAnchors(IEnumerable<string> abilityTypes)
    {
        Motion?.SetActiveAnchors(abilityTypes);
    }

    /// <summary>Set alt variant, or "RECHECK" to reroll</summary>
    internal void SetAltVariant(string altVariantKey)
    {
        Motion?.SetAltVariant(altVariantKey);
    }

    /// <summary>Vanilla hop event handler, not using.</summary>
    public override void Hop(float amount) { }
}
