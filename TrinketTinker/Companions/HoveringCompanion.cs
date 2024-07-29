using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Companions;
using StardewValley;
using TrinketTinker.Model;
using Netcode;
using StardewValley.Network;


namespace TrinketTinker.Companions
{
    public class HoveringCompanion : Companion
    {
        // TrinketMetadata
        private readonly NetString Texture = new();
        private readonly NetPoint Size = new();
        private readonly NetInt AnimationLength = new();
        private readonly NetFloat FrameDuration = new();
        private readonly NetPoint HoverOffset = new();
        public Vector2 HoverVec => new(HoverOffset.Value.X, -HoverOffset.Value.Y);
        private readonly NetFloat LightRadius = new();
        // State
        private int frame = 0;
        private float frameTimer = 0;
        private int lightID = 0;

        public HoveringCompanion()
        {
        }

        public HoveringCompanion(CompanionModel data)
        {
            Texture.Value = data.Texture;
            Size.Value = data.Size;
            AnimationLength.Value = data.AnimationLength;
            FrameDuration.Value = data.FrameDuration;
            HoverOffset.Value = data.HoverOffset;
            LightRadius.Value = data.LightRadius;

            whichVariant.Value = data.Variant;
        }

        public override void InitNetFields()
        {
            base.InitNetFields();
            base.NetFields
                .AddField(Texture, "Texture")
                .AddField(Size, "Size")
                .AddField(AnimationLength, "AnimationLength")
                .AddField(FrameDuration, "FrameDuration")
                .AddField(HoverOffset, "HoverOffset")
                .AddField(LightRadius, "LightRadius")
                ;
        }

        public override void Draw(SpriteBatch b)
        {
            if (base.Owner == null || base.Owner.currentLocation == null || (base.Owner.currentLocation.DisplayName == "Temp" && !Game1.isFestival()))
            {
                return;
            }
            Texture2D texture = Game1.content.Load<Texture2D>(Texture.Value);
            SpriteEffects effect = direction.Value == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            b.Draw(
                texture,
                Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + HoverVec),
                new Rectangle(
                    frame * Size.Value.X, whichVariant.Value * Size.Value.Y,
                    Size.Value.X, Size.Value.Y
                ),
                Color.White, 0f, new Vector2(8f, 8f), 4f,
                effect, _position.Y / 10000f
            );

            b.Draw(
                Game1.shadowTexture,
                Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(HoverVec.X, 0f)),
                Game1.shadowTexture.Bounds,
                Color.White, 0f,
                new Vector2(
                    Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y
                ),
                3f * Utility.Lerp(1f, 0.8f, Math.Min(Math.Abs(HoverVec.Y), 1f)),
                SpriteEffects.None, (_position.Y - 8f) / 10000f - 2E-06f
            );
        }

        public override void Update(GameTime time, GameLocation location)
        {
            base.Update(time, location);
            frameTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
            if (frameTimer > FrameDuration.Value)
            {
                frameTimer = 0f;
                frame = (frame + 1) % AnimationLength.Value;
            }
            if (LightRadius.Value != 0f && location.Equals(Game1.currentLocation))
            {
                Utility.repositionLightSource(lightID, base.Position + HoverVec);
            }
        }

        private void ApplyLight()
        {
            lightID = Game1.random.Next();
            Game1.currentLightSources.Add(new LightSource(1, base.Position, LightRadius.Value, Color.Black, lightID));
        }

        public override void InitializeCompanion(Farmer farmer)
        {
            base.InitializeCompanion(farmer);
            if (LightRadius.Value != 0f)
                ApplyLight();
        }

        public override void CleanupCompanion()
        {
            base.CleanupCompanion();
            if (LightRadius.Value != 0f)
            {
                Utility.removeLightSource(lightID);
            }
        }

        public override void OnOwnerWarp()
        {
            base.OnOwnerWarp();
            if (LightRadius.Value != 0f)
                ApplyLight();
        }

        public override void Hop(float amount)
        {
        }
    }
}
