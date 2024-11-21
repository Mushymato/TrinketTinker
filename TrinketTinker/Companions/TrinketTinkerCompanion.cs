using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Companions;
using StardewValley.Network;
using TrinketTinker.Companions.Motions;
using TrinketTinker.Models;
using TrinketTinker.Wheels;

namespace TrinketTinker.Companions;

/// <summary>Main companion class for trinket tinker.</summary>
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
    public bool OwnerMoving { get; private set; } = false;

    /// <summary>Companion position in prev tick, for detecting moving</summary>
    private Vector2? prevPosition;

    /// <summary>Whether companion is moving</summary>
    public bool CompanionMoving { get; private set; } = false;
    internal NetPosition NetPosition => _position;
    private readonly NetFloat _netLerp = new NetFloat(-1f).Interpolated(true, false);
    internal float Lerp
    {
        get => _netLerp.Value;
        set => _netLerp.Value = value;
    }

    // Derived
    /// <summary>Backing companion data from content.</summary>
    public TinkerData? Data;

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
    }

    /// <summary>Cleanup Motion class.</summary>
    public override void CleanupCompanion()
    {
        base.CleanupCompanion();
        if (Motion != null)
        {
            Motion.Cleanup();
            Motion = null;
        }
    }

    /// <summary>Setup net fields.</summary>
    public override void InitNetFields()
    {
        base.InitNetFields();
        NetFields
            .AddField(_id, "_id")
            .AddField(_oneshotKey, "_oneshotKey")
            .AddField(_overrideKey, "_overrideKey")
            .AddField(_netLerp, "_netLerp")
            .AddField(_disableCompanion, "_disableCompanion");
        _id.fieldChangeVisibleEvent += InitCompanionData;
        _oneshotKey.fieldChangeVisibleEvent += (NetString field, string oldValue, string newValue) => Motion?.SetOneshotClip(newValue);
        _overrideKey.fieldChangeVisibleEvent += (NetString field, string oldValue, string newValue) => Motion?.SetOverrideClip(newValue);
    }

    /// <summary>When <see cref="Id"/> is changed through net event, fetch companion data and build all fields.</summary>
    private void InitCompanionData(NetString field, string oldValue, string newValue)
    {
        // _id.Value = newValue;
        if (!AssetManager.TinkerData.TryGetValue(_id.Value, out Data))
        {
            ModEntry.Log($"Failed to get companion data for ${_id.Value}", LogLevel.Error);
            return;
        }

        if (Data.Motions.Count > 0)
        {
            VariantData vdata = Data.Variants[whichVariant.Value];
            MotionData mdata = Data.Motions[0];
            if (mdata.MotionClass == null)
            {
                Motion = new LerpMotion(this, mdata, vdata);
            }
            if (Reflect.TryGetType(mdata.MotionClass, out Type? motionCls, TinkerConst.MOTION_CLS))
            {
                Motion = (IMotion?)Activator.CreateInstance(motionCls, this, mdata, vdata);
            }
            else
            {
                ModEntry.LogOnce($"Could not get motion class {mdata.MotionClass}", LogLevel.Error);
            }
        }
    }

    /// <summary>Draw using <see cref="Motion"/>.</summary>
    /// <param name="b">SpriteBatch</param>
    public override void Draw(SpriteBatch b)
    {
        if (Owner == null || _disableCompanion.Value)
            return;
        if (!Visuals.ShouldDraw())
            return;

        Motion?.Draw(b);
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
        OwnerMoving = prevOwnerPosition != OwnerPosition;
        CompanionMoving = prevPosition != Position;
        prevOwnerPosition = OwnerPosition;
        prevPosition = Position;
        Motion?.UpdateAnchor(time, location);
        if (IsLocal)
        {
            if (Motion == null)
            {
                Position = Anchor;
            }
            else
            {
                Motion?.UpdateLocal(time, location);
            }
        }
        Motion?.UpdateGlobal(time, location);
        Motion?.UpdateLightSource(time, location);
    }

    /// <summary>Reset position and display status on warp</summary>
    public override void OnOwnerWarp()
    {
        base.OnOwnerWarp();
        _position.Value = _owner.Value.Position;
        Motion?.OnOwnerWarp();
        _disableCompanion.Value = Places.LocationDisableTrinketCompanions(Owner.currentLocation);
    }

    /// <summary>Vanilla hop event handler, not using.</summary>
    public override void Hop(float amount) { }
}
