using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Companions;

namespace TrinketTinker.Framework.Fren
{
    public class DustSpiritCompanion : Companion
    {
        private readonly AnimatedSprite sprite;
        private readonly float scale;
        public byte voice;
        private ICue? meep;

        public DustSpiritCompanion()
        {
            sprite = new("Characters\\Monsters\\Dust Spirit", 0, 16, 24)
            {
                interval = 45f
            };
            scale = 0.75f;
            voice = (byte)Random.Shared.Next(1, 24);
        }

        public override void Update(GameTime time, GameLocation location)
        {
            if (IsLocal)
            {
                if (lerp < 0f)
                {
                    sprite.AnimateDown(time);
                    if ((OwnerPosition - Position).Length() > 768f)
                    {
                        Utility.addRainbowStarExplosion(location, Position + new Vector2(0f, 0f - height), 1);
                        Position = Owner.Position;
                        lerp = -1f;
                    }
                    else
                    {
                        startPosition = Position;
                        float radius = 0.33f;
                        endPosition = OwnerPosition + new Vector2(Utility.RandomFloat(-64f, 64f) * radius, Utility.RandomFloat(-64f, 64f) * radius);
                        if (location.isCollidingPosition(new Rectangle((int)endPosition.X - 8, (int)endPosition.Y - 8, 16, 16), Game1.viewport, isFarmer: false, 0, glider: false, null, pathfinding: true, projectile: false, ignoreCharacterRequirement: true))
                        {
                            endPosition = OwnerPosition;
                        }
                        lerp = 0f;
                        hopEvent.Fire(1f);
                        if (Math.Abs(OwnerPosition.X - Position.X) > 8f)
                        {
                            if (OwnerPosition.X > Position.X)
                            {
                                direction.Value = 1;
                            }
                            else
                            {
                                direction.Value = 3;
                            }
                        }
                    }
                    // if ((OwnerPosition - Position).Length() > 80f)
                    // {
                    // }
                }
                if (lerp >= 0f)
                {
                    lerp += (float)time.ElapsedGameTime.TotalSeconds / 0.4f;
                    if (lerp > 1f)
                    {
                        lerp = 1f;
                    }
                    float x = Utility.Lerp(startPosition.X, endPosition.X, lerp);
                    float y = Utility.Lerp(startPosition.Y, endPosition.Y, lerp);
                    Position = new Vector2(x, y);
                    if (lerp == 1f)
                    {
                        lerp = -1f;
                    }
                }
            }
            hopEvent.Poll();
            if (gravity != 0f || height != 0f)
            {
                height += gravity * 3;
                gravity -= (float)time.ElapsedGameTime.TotalSeconds * 6f;
                if (height <= 0f)
                {
                    height = 0f;
                    gravity = 0f;
                }
            }

            if (Random.Shared.NextDouble() < 0.1 && (meep == null || !meep.IsPlaying))
            {
                Game1.playSound("dustMeep", voice * 100 + Random.Shared.Next(-100, 100), out meep);
            }
        }

        public override void Draw(SpriteBatch b)
        {
            if (base.Owner != null && base.Owner.currentLocation != null && (!(base.Owner.currentLocation.DisplayName == "Temp") || Game1.isFestival()))
            {
                _draw(b, sprite);
            }
        }

        protected void _draw(SpriteBatch b, AnimatedSprite sprite)
        {
            SpriteEffects effect = direction.Value == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            // sprite
            // b.Draw(
            //     sprite.Texture,
            //     Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - height) * 4f)),
            //     sprite.SourceRect,
            //     Color.White, 0f, new Vector2(8f, 16f), 4f, effect, _position.Y / 10000f
            // );
            b.Draw(
                sprite.Texture,
                Game1.GlobalToLocal(Position + Owner.drawOffset + new Vector2(0f, (0f - height) * 4f)),
                // getLocalPosition(Game1.viewport) + new Vector2(32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), 64 + yJumpOffset),
                sprite.SourceRect,
                Color.White, 0f, new Vector2(8f, 16f),
                (scale - Math.Max(-0.1f, height / 256f)) * 4f,
                effect,
                Math.Max(0f, Position.Y / 10000f));

            // shadow
            b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f * Utility.Lerp(1f, 0.8f, Math.Min(height, 1f)), SpriteEffects.None, 0f);
        }

        // protected void _draw(SpriteBatch b, Texture2D texture, Rectangle startingSourceRect)
        // {
        //     SpriteEffects effect = SpriteEffects.None;
        //     if (direction.Value == 3)
        //     {
        //         effect = SpriteEffects.FlipHorizontally;
        //     }
        //     if (height > 0f)
        //     {
        //         b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - height) * 4f)), startingSourceRect, Color.White, 0f, new Vector2(8f, 16f), 4f, effect, _position.Y / 10000f);
        //     }
        //     else
        //     {
        //         b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - height) * 4f)), startingSourceRect, Color.White, 0f, new Vector2(8f, 16f), 4f, effect, _position.Y / 10000f);
        //     }
        //     b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f * Utility.Lerp(1f, 0.8f, Math.Min(height, 1f)), SpriteEffects.None, 0f);
        // }
    }
}
