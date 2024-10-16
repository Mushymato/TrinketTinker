using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using Netcode;
using StardewValley;
using StardewValley.Companions;
using TrinketTinker.Models;
using TrinketTinker.Companions.Motions;
using StardewValley.Network;
using TrinketTinker.Wheels;


namespace TrinketTinker.Companions
{
    /// <summary>Main companion class for trinket tinker.</summary>
    public class TrinketTinkerCompanion : Companion
    {
        // NetFields + Getters
        /// <summary>NetField for <see cref="ID"/></summary>
        protected readonly NetString _id = new("");
        /// <summary>Companion ID. Companion is (re)loaded when this is changed.</summary>
        public string ID => _id.Value;
        /// <summary>Whether companion is moving</summary>
        public bool Moving => Owner?.position.moving.Value ?? false;
        internal NetPosition NetPosition => _position;
        // Derived
        /// <summary>Backing companion data from content.</summary>
        public TinkerData? Data;
        /// <summary>Motion class that controls how the companion moves.</summary>
        public IMotion? Motion { get; set; }
        /// <summary>Position the companion should follow.</summary>
        public Vector2 Anchor { get; set; }

        /// <summary>Argumentless constructor for netcode deserialization.</summary>
        public TrinketTinkerCompanion() : base()
        {
        }

        /// <summary>Construct new companion using companion ID.</summary>
        public TrinketTinkerCompanion(string companionId, int variant)
        {
            _id.Value = companionId;
            // _moving.Value = false;
            whichVariant.Value = variant;
        }

        /// <summary>Initialize Motion class.</summary>
        public override void InitializeCompanion(Farmer farmer)
        {
            base.InitializeCompanion(farmer);
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
            // .AddField(_moving, "_moving")
            // .AddField(_offset.NetFields, "_offset.NetFields")
            ;
            _id.fieldChangeVisibleEvent += InitCompanionData;
            // _moving.fieldChangeEvent += (NetBool field, bool oldValue, bool newValue) => { _moving.Value = newValue; };
            // _offset.fieldChangeEvent += (NetVector2 field, Vector2 oldValue, Vector2 newValue) => { _offset.Value = newValue; };
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
            if (Owner == null || Owner.currentLocation == null || (Owner.currentLocation.DisplayName == "Temp" && !Game1.isFestival()))
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
        }

        /// <summary>Reset position on warp</summary>
        public override void OnOwnerWarp()
        {
            base.OnOwnerWarp();
            _position.Value = _owner.Value.Position;
            Motion?.OnOwnerWarp();
        }

        /// <summary>Vanilla hop event handler, not using.</summary>
        public override void Hop(float amount)
        {
        }

    }
}
