using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using Netcode;
using StardewValley;
using StardewValley.Companions;
using TrinketTinker.Models;
using TrinketTinker.Companions.Motions;


namespace TrinketTinker.Companions
{
    public class TrinketTinkerCompanion : Companion
    {
        private const string MOTION_FMT = "TrinketTinker.Companions.Motions.{0}Motion, TrinketTinker";
        // NetFields + Getters
        protected readonly NetString _id = new("");
        public string ID => _id.Value;
        public AnimatedSprite Sprite { get; set; } = new();
        protected readonly NetBool _moving = new(false);
        public bool Moving
        {
            get
            {
                return _moving.Value;
            }
            set
            {
                _moving.Value = value;
            }
        }
        protected readonly NetVector2 _offset = new();
        public Vector2 Offset
        {
            get
            {
                return _offset.Value;
            }
            set
            {
                _offset.Value = value;
            }
        }
        public readonly NetFloat rotation = new(0f);
        // Derived
        public CompanionData? Data;
        public Motion? Motion
        { get; set; }
        public Vector2 Anchor
        {
            get
            {
                return Utility.PointToVector2(this.Owner.GetBoundingBox().Center);
            }
        }
        public Vector2 SpriteOrigin { get; set; } = Vector2.Zero;
        public Color SpriteColor => Utility.StringToColor(Data?.Variants[whichVariant.Value].ColorMask) ?? Color.White;

        public TrinketTinkerCompanion() : base()
        {
        }

        public TrinketTinkerCompanion(string companionId)
        {
            // _sprite.Value = new AnimatedSprite(data.Texture, 0, data.Size.X, data.Size.Y);
            // _interval.Value = data.FrameInterval;
            // _framesPerAnimation.Value = data.FramesPerAnimation;
            _id.Value = companionId;
            _moving.Value = false;
            whichVariant.Value = 0;
        }

        public override void InitializeCompanion(Farmer farmer)
        {
            base.InitializeCompanion(farmer);
        }

        public override void CleanupCompanion()
        {
            base.CleanupCompanion();
            Motion = null;
        }

        public override void InitNetFields()
        {
            base.InitNetFields();
            NetFields
                .AddField(_id, "_id")
                .AddField(_moving, "_moving")
                .AddField(_offset, "_offset")
                .AddField(rotation, "rotation")
            ;
            _id.fieldChangeEvent += InitCompanionData;
            _moving.fieldChangeEvent += (NetBool field, bool oldValue, bool newValue) => { _moving.Value = newValue; };
            _offset.fieldChangeEvent += (NetVector2 field, Vector2 oldValue, Vector2 newValue) => { _offset.Value = newValue; };
        }

        private void InitCompanionData(NetString field, string oldValue, string newValue)
        {
            _id.Value = newValue;
            if (!ModEntry.CompanionData.TryGetValue(_id.Value, out Data))
            {
                ModEntry.Log($"Failed to get companion data for ${_id.Value}", LogLevel.Error);
                return;
            }
            VariantData vdata = Data.Variants[whichVariant.Value];
            Sprite = new AnimatedSprite(vdata.Texture, 0, vdata.Width, vdata.Height);
            SpriteOrigin = new Vector2(vdata.Width / 2, vdata.Height / 2);

            // Interval = Data.FrameInterval;
            // FramesPerAnimation = Data.FramesPerAnimation;
            MotionData mdata = Data.Motions["default"];
            if (ModEntry.TryGetType(mdata.MotionClass, out Type? motionCls, MOTION_FMT))
                Motion = (Motion?)Activator.CreateInstance(motionCls, this, mdata);
            else
                Motion = new LerpMotion(this, mdata);
        }


        public override void Draw(SpriteBatch b)
        {
            if (Owner == null || Owner.currentLocation == null || (Owner.currentLocation.DisplayName == "Temp" && !Game1.isFestival()))
            {
                return;
            }
            Motion!.Draw(b);
        }

        public override void Update(GameTime time, GameLocation location)
        {
            if (IsLocal)
                Motion!.UpdateLocal(time, location);
            Motion!.UpdateGlobal(time, location);
        }

        public override void OnOwnerWarp()
        {
            base.OnOwnerWarp();
            _position.Value = _owner.Value.Position;
        }

        public override void Hop(float amount)
        {
        }

        // private void ApplyLight()
        // {
        //     lightID = Game1.random.Next();
        //     Game1.currentLightSources.Add(new LightSource(1, Position, LightRadius.Value, Color.Black, lightID));
        // }

        // public override void InitializeCompanion(Farmer farmer)
        // {
        //     InitializeCompanion(farmer);
        //     if (LightRadius.Value != 0f)
        //         ApplyLight();
        // }

        // public override void CleanupCompanion()
        // {
        //     CleanupCompanion();
        //     if (LightRadius.Value != 0f)
        //     {
        //         Utility.removeLightSource(lightID);
        //     }
        // }
    }
}
