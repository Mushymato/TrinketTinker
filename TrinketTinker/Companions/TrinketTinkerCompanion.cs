using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using Netcode;
using StardewValley;
using StardewValley.Companions;
using TrinketTinker.Model;
using TrinketTinker.Companions.Motions;


namespace TrinketTinker.Companions
{
    public class TrinketTinkerCompanion : Companion
    {
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
        public CompanionData? Data;
        public Motion? Motion
        { get; set; }
        // State
        public Vector2 Anchor
        {
            get
            {
                return Utility.PointToVector2(this.Owner.GetBoundingBox().Center);
            }
        }

        public TrinketTinkerCompanion() : base()
        {
            ModEntry.Log($"TrinketTinkerCompanion.ctor(): {ID}");
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
            // .AddField(_currentMotion, "_motionClass")
            ;
            _id.fieldChangeEvent += InitCompanionData;
            _moving.fieldChangeEvent += (NetBool field, bool oldValue, bool newValue) => { _moving.Value = newValue; };
        }

        private void InitCompanionData(NetString field, string oldValue, string newValue)
        {
            ModEntry.LogOnce($"InitCompanionData({newValue})");
            _id.Value = newValue;
            if (!ModEntry.CompanionData.TryGetValue(_id.Value, out Data))
            {
                ModEntry.Log($"Failed to get companion data for ${_id.Value}", LogLevel.Error);
                return;
            }
            VariantData vdata = Data.Variants[whichVariant.Value];
            Sprite = new AnimatedSprite(vdata.Texture, 0, vdata.Width, vdata.Height);

            // Interval = Data.FrameInterval;
            // FramesPerAnimation = Data.FramesPerAnimation;
            MotionData mdata = Data.Motions["default"];
            if (mdata.MotionClass != null &&
                Type.GetType($"TrinketTinker.Companions.Motions.{mdata.MotionClass}Motion, TrinketTinker") is Type motionCls)
            {
                Motion = (Motion?)Activator.CreateInstance(motionCls, this, mdata);
            }
            else
            {
                Motion = new LerpMotion(this, mdata);
            }
        }

        public override void Draw(SpriteBatch b)
        {
            if (Owner == null || Owner.currentLocation == null || (Owner.currentLocation.DisplayName == "Temp" && !Game1.isFestival()))
            {
                return;
            }

            b.Draw(
                Sprite.Texture,
                Game1.GlobalToLocal(Position + Owner.drawOffset + Motion!.DrawOffset),
                Sprite.SourceRect,
                Color.White, 0f, new Vector2(8f, 8f), 4f,
                (direction.Value == 30) ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                _position.Y / 10000f
            );

            b.Draw(
                Game1.shadowTexture,
                Game1.GlobalToLocal(Position + Owner.drawOffset),
                Game1.shadowTexture.Bounds,
                Color.White, 0f,
                new Vector2(
                    Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y
                ),
                3f * Utility.Lerp(1f, 0.8f, Math.Min(height, 1f)),
                SpriteEffects.None, (_position.Y - 8f) / 10000f - 2E-06f
            );
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
