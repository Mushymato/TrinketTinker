using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Companions;
using Netcode;
using StardewValley;
using TrinketTinker.Model;
using TrinketTinker.Companions.Motions;


namespace TrinketTinker.Companions
{
    public class TrinketTinkerCompanion : Companion
    {
        // NetFields + Getters
        protected readonly NetRef<AnimatedSprite> _sprite = new();
        protected readonly NetFloat _interval = new(100f);
        protected readonly NetInt _framesPerAnimation = new();
        protected readonly NetString _currentMotion = new("");
        protected readonly NetVector2 _drawOffset = new(Vector2.Zero);

        public AnimatedSprite Sprite => _sprite.Value;
        public SpriteEffects Orientation
        {
            get
            {
                return (SpriteEffects)direction.Value;
            }
            set
            {
                direction.Value = (int)value;
            }
        }

        public float Interval => _interval.Value;
        public int FramesPerAnimation => _framesPerAnimation.Value;
        // State
        public Vector2 Anchor
        {
            get
            {
                return Utility.PointToVector2(this.Owner.GetBoundingBox().Center);
            }
        }
        public Motion? Motion { get; set; }
        public float motionTimer = 0;

        public TrinketTinkerCompanion() : base()
        {
        }

        public TrinketTinkerCompanion(CompanionModel data)
        {
            _sprite.Value = new AnimatedSprite(data.Texture, 0, data.Size.X, data.Size.Y);
            _interval.Value = data.FrameInterval;
            _framesPerAnimation.Value = data.FramesPerAnimation;
            _currentMotion.Value = typeof(LerpMotion).AssemblyQualifiedName;
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
                .AddField(_sprite, "_sprite")
                .AddField(_interval, "_interval")
                .AddField(_framesPerAnimation, "_framesPerAnimation")
                .AddField(_currentMotion, "_motionClass")
            ;
            _currentMotion.fieldChangeEvent += InitMotion;
        }

        public virtual void InitMotion(NetString field, string oldValue, string newValue)
        {
            _currentMotion.Value = newValue;
            Type? motionCls = Type.GetType(_currentMotion.Value);
            if (motionCls != null)
            {
                Motion = (Motion?)Activator.CreateInstance(motionCls, this);
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
                Game1.GlobalToLocal(Position + Motion!.DrawOffset),
                Sprite.SourceRect,
                Color.White, 0f, new Vector2(8f, 8f), 4f,
                Orientation, _position.Y / 10000f
            );

            b.Draw(
                Game1.shadowTexture,
                Game1.GlobalToLocal(Position + Owner.drawOffset),
                Game1.shadowTexture.Bounds,
                Color.White, 0f,
                new Vector2(
                    Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y
                ),
                3f,
                SpriteEffects.None, (_position.Y - 8f) / 10000f - 2E-06f
            );
        }

        public override void Update(GameTime time, GameLocation location)
        {
            // if (IsLocal)
            //     UpdateLerpPosition(time, location);
            motionTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
            if (motionTimer >= 5000f)
            {
                motionTimer = 0f;
                if (Motion is LerpMotion)
                    _currentMotion.Value = typeof(StaticMotion).AssemblyQualifiedName;
                else
                    _currentMotion.Value = typeof(LerpMotion).AssemblyQualifiedName;
            }

            if (IsLocal)
                Motion!.UpdateLocal(time, location);
            Motion!.UpdateGlobal(time, location);
            // if (LightRadius.Value != 0f && location.Equals(Game1.currentLocation))
            // {
            //     Utility.repositionLightSource(lightID, Position);
            // }
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
