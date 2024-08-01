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
        protected readonly NetString _motionClass = new("");

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

        public TrinketTinkerCompanion() : base()
        {
        }

        public TrinketTinkerCompanion(CompanionModel data)
        {
            _sprite.Value = new AnimatedSprite(data.Texture, 0, data.Size.X, data.Size.Y);
            _interval.Value = data.FrameInterval;
            _framesPerAnimation.Value = data.FramesPerAnimation;
            _motionClass.Value = typeof(LinearMotion).AssemblyQualifiedName;
        }

        public override void InitializeCompanion(Farmer farmer)
        {
            base.InitializeCompanion(farmer);
            Type? motionCls = Type.GetType(_motionClass.Value);
            if (motionCls != null)
            {
                Motion = (Motion?)Activator.CreateInstance(motionCls, this);
            }
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
            ;
        }

        public override void Draw(SpriteBatch b)
        {
            if (Owner == null || Owner.currentLocation == null || (Owner.currentLocation.DisplayName == "Temp" && !Game1.isFestival()))
            {
                return;
            }
            b.Draw(
                Sprite.Texture,
                Game1.GlobalToLocal(Position + Owner.drawOffset),
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
                3f * Utility.Lerp(1f, 0.8f, 1f),
                SpriteEffects.None, (_position.Y - 8f) / 10000f - 2E-06f
            );
        }

        // public void UpdateLerpPosition(GameTime time, GameLocation location)
        // {
        //     // Copied from Companion.Update's IsLocal block
        //     if (lerp < 0f)
        //     {
        //         if ((Anchor - Position).Length() > 768f)
        //         {
        //             Utility.addRainbowStarExplosion(location, Position + new Vector2(0f, 0f - height), 1);
        //             Position = Owner.Position;
        //             lerp = -1f;
        //         }
        //         if ((Anchor - Position).Length() > 80f)
        //         {
        //             startPosition = Position;
        //             float radius = 0.33f;
        //             endPosition = Anchor + new Vector2(
        //                 Utility.RandomFloat(-64f, 64f) * radius,
        //                 Utility.RandomFloat(-64f, 64f) * radius
        //             );
        //             if (location.isCollidingPosition(
        //                     new Rectangle((int)endPosition.X - 8,
        //                     (int)endPosition.Y - 8, 16, 16),
        //                     Game1.viewport,
        //                     isFarmer: false,
        //                     0,
        //                     glider: false,
        //                     null,
        //                     pathfinding: true,
        //                     projectile: false,
        //                     ignoreCharacterRequirement: true
        //                 ))
        //             {
        //                 endPosition = Anchor;
        //             }
        //             lerp = 0f;
        //             // hopEvent.Fire(1f);
        //             if (Math.Abs(Anchor.X - Position.X) > 8f)
        //             {
        //                 _orientation.Value = (Anchor.X > Position.X) ? 1 : 3;
        //             }
        //         }
        //     }
        //     if (lerp >= 0f)
        //     {
        //         lerp += (float)time.ElapsedGameTime.TotalSeconds / 0.4f;
        //         if (lerp > 1f)
        //         {
        //             lerp = 1f;
        //         }
        //         float x = Utility.Lerp(startPosition.X, endPosition.X, lerp);
        //         float y = Utility.Lerp(startPosition.Y, endPosition.Y, lerp);
        //         Position = new Vector2(x, y);
        //         if (lerp == 1f)
        //         {
        //             lerp = -1f;
        //         }
        //     }
        // }

        public override void Update(GameTime time, GameLocation location)
        {
            // if (IsLocal)
            //     UpdateLerpPosition(time, location);
            if (IsLocal)
                Motion!.Update(time, location);
            Sprite.Animate(time, 0, FramesPerAnimation, Interval);
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
