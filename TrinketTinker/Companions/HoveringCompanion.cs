using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Companions;
using StardewValley;


namespace TrinketTinker.Companions
{
    public class HoveringCompanion : Companion
    {
        private Vector2 extraPosition;

        private Vector2 extraPositionMotion;

        private Vector2 extraPositionAcceleration;

        private int lightID = 301579;

        private float frameRate = 0;
        private float frameTimer = 0;

        private int frame = 0;

        public string companionTexture = "";

        public int animLength = 0;

        public bool hasLight = true;

        public int variant = 0;

        public int spriteW = 16;

        public int spriteH = 16;

        public HoveringCompanion(string texturePath)
        {
            companionTexture = texturePath;
            Texture2D texture = Game1.content.Load<Texture2D>(companionTexture);
            animLength = texture.Width / 16;
            frameRate = 200;
            extraPosition = Vector2.Zero;
        }

        // public override void InitNetFields()
        // {
        //     base.InitNetFields();
        // }

        public override void Draw(SpriteBatch b)
        {
            if (base.Owner == null || base.Owner.currentLocation == null || (base.Owner.currentLocation.DisplayName == "Temp" && !Game1.isFestival()))
            {
                return;
            }
            Texture2D texture = Game1.content.Load<Texture2D>(companionTexture);
            SpriteEffects effect = direction.Value == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            b.Draw(
                texture,
                Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - height) * 4f) + extraPosition),
                new Rectangle(
                    frame * spriteW,
                    variant * spriteH,
                    spriteW, spriteH
                ),
                Color.White, 0f, new Vector2(8f, 8f), 4f,
                effect, _position.Y / 10000f
            );

            b.Draw(
                Game1.shadowTexture,
                Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(extraPosition.X, 0f)),
                Game1.shadowTexture.Bounds,
                Color.White, 0f,
                new Vector2(
                    Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y),
                    3f * Utility.Lerp(1f, 0.8f, Math.Min(height, 1f)
                ),
                SpriteEffects.None, (_position.Y - 8f) / 10000f - 2E-06f
            );
        }

        public override void Update(GameTime time, GameLocation location)
        {
            base.Update(time, location);
            height = 32f;
            frameTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
            if (frameTimer > frameRate)
            {
                frameTimer = 0f;
                // extraPositionMotion = new Vector2((Game1.random.NextDouble() < 0.5) ? 0.1f : (-0.1f), -2f);
                // extraPositionAcceleration = new Vector2(0f, 0.14f);
                frame = (frame + 1) % animLength;
            }
            // extraPosition += extraPositionMotion;
            // extraPositionMotion += extraPositionAcceleration;
            if (hasLight && location.Equals(Game1.currentLocation))
            {
                Utility.repositionLightSource(lightID, base.Position - new Vector2(0f, height * 4f) + extraPosition);
            }
        }

        public override void InitializeCompanion(Farmer farmer)
        {
            base.InitializeCompanion(farmer);
            if (hasLight)
            {
                lightID = Game1.random.Next();
                Game1.currentLightSources.Add(new LightSource(1, base.Position, 2f, Color.Black, lightID, LightSource.LightContext.None, 0L));
            }
        }

        public override void CleanupCompanion()
        {
            base.CleanupCompanion();
            if (hasLight)
            {
                Utility.removeLightSource(lightID);
            }
        }

        public override void OnOwnerWarp()
        {
            base.OnOwnerWarp();
            extraPosition = Vector2.Zero;
            extraPositionMotion = Vector2.Zero;
            extraPositionAcceleration = Vector2.Zero;
            if (hasLight)
            {
                lightID = Game1.random.Next();
                Game1.currentLightSources.Add(new LightSource(1, base.Position, 2f, Color.Black, lightID, LightSource.LightContext.None, 0L));
            }
        }

        public override void Hop(float amount)
        {
        }
    }
}
