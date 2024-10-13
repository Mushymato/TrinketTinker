using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using StardewValley.TerrainFeatures;
using TrinketTinker.Effects.Proc;
using TrinketTinker.Models.AbilityArgs;

namespace TrinketTinker.Effects.Pewpew
{
    public class TinkerProjectile : Projectile
    {

        public readonly NetString projectileTexture = new("");
        private Texture2D? loadedProjectileTexture = null;
        public readonly NetInt minDamage = new(0);
        public readonly NetInt maxDamage = new(0);
        public readonly NetFloat knockBackModifier = new(0f);
        public readonly NetInt addedPrecision = new(0);
        public readonly NetFloat critChance = new(0f);
        public readonly NetFloat critMultiplier = new(0f);
        public readonly NetInt stunTime = new(0);

        /// <summary>Construct an empty instance.</summary>
        public TinkerProjectile() : base()
        {
        }

        public TinkerProjectile(ProjectileArgs args, ProcEventArgs proc, Monster target, Vector2 sourcePosition) : this()
        {
            if (args.Texture != null)
                projectileTexture.Value = args.Texture;
            currentTileSheetIndex.Value = args.SpriteIndex;

            Vector2 velocity = Utility.getVelocityTowardPoint(sourcePosition, target.getStandingPosition(), args.MinVelocity);
            xVelocity.Value = velocity.X;
            yVelocity.Value = velocity.Y;
            startingRotation.Value = (float)Math.Atan2(velocity.Y, velocity.X);
            acceleration.Value = Utility.getVelocityTowardPoint(sourcePosition, target.getStandingPosition(), args.Acceleration);
            maxVelocity.Value = args.MaxVelocity;
            position.Value = sourcePosition;

            piercesLeft.Value = args.Pierce;
            ignoreObjectCollisions.Value = args.IgnoreObjectCollisions;
            ignoreLocationCollision.Value = args.IgnoreLocationCollisions;
            theOneWhoFiredMe.Set(proc.Location, proc.Farmer);

            minDamage.Value = args.Min;
            maxDamage.Value = args.Max;
            knockBackModifier.Value = args.Knockback;
            addedPrecision.Value = args.Precision;
            critChance.Value = args.CritChance;
            critMultiplier.Value = args.CritDamage;
            stunTime.Value = args.StunTime;

            damagesMonsters.Value = true;
        }

        /// <inheritdoc />
        protected override void InitNetFields()
        {
            base.InitNetFields();
            NetFields
            .AddField(projectileTexture, "projectileTexture")
            .AddField(minDamage, "minDamage")
            .AddField(maxDamage, "maxDamage")
            .AddField(knockBackModifier, "knockBackModifier")
            .AddField(addedPrecision, "addedPrecision")
            .AddField(critChance, "critChance")
            .AddField(critMultiplier, "critMultiplier")
            .AddField(stunTime, "stunTime")
            ;
        }

        /// <summary>Get the texture to draw for the projectile.</summary>
        public Texture2D GetCustomTexture()
        {
            if (projectileTexture.Value != null)
            {
                loadedProjectileTexture ??= Game1.content.Load<Texture2D>(projectileTexture.Value);
                return loadedProjectileTexture;
            }
            return projectileSheet;
        }

        /// <summary>Needed to override this to get custom texture weh</summary>
        /// <param name="b"></param>
        public override void draw(SpriteBatch b)
        {
            float scale = 4f * localScale;
            Texture2D texture = GetCustomTexture();
            Rectangle sourceRect = GetSourceRect();
            Vector2 value = position.Value;
            b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, value + new Vector2(0f, 0f - height.Value) + new Vector2(32f, 32f)), sourceRect, color.Value * alpha.Value, rotation, new Vector2(8f, 8f), scale, SpriteEffects.None, (value.Y + 96f) / 10000f);
            if (height.Value > 0f)
            {
                b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, value + new Vector2(32f, 32f)), Game1.shadowTexture.Bounds, Color.White * alpha.Value * 0.75f, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 2f, SpriteEffects.None, (value.Y - 1f) / 10000f);
            }
            float num = alpha.Value;
            for (int num2 = tail.Count - 1; num2 >= 0; num2--)
            {
                b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, Vector2.Lerp((num2 == tail.Count - 1) ? value : tail.ElementAt(num2 + 1), tail.ElementAt(num2), (float)tailCounter / 50f) + new Vector2(0f, 0f - height.Value) + new Vector2(32f, 32f)), sourceRect, color.Value * num, rotation, new Vector2(8f, 8f), scale, SpriteEffects.None, (value.Y - (float)(tail.Count - num2) + 96f) / 10000f);
                num -= 1f / (float)tail.Count;
                scale = 0.8f * (float)(4 - 4 / (num2 + 4));
            }
        }

        public override void behaviorOnCollisionWithMonster(NPC n, GameLocation location)
        {
            Farmer playerWhoFiredMe = (theOneWhoFiredMe.Get(location) as Farmer) ?? Game1.player;

            if (n is Monster monster)
            {
                if (minDamage.Value > 0)
                {
                    location.damageMonster(
                        areaOfEffect: monster.GetBoundingBox(),
                        minDamage: minDamage.Value,
                        maxDamage: maxDamage.Value,
                        isBomb: false,
                        knockBackModifier: knockBackModifier.Value,
                        addedPrecision: addedPrecision.Value,
                        critChance: critChance.Value,
                        critMultiplier: critMultiplier.Value,
                        triggerMonsterInvincibleTimer: true,
                        who: playerWhoFiredMe,
                        isProjectile: true
                    );
                }
                monster.stunTime.Value = stunTime.Value;
                if (!monster.IsInvisible)
                {
                    piercesLeft.Value--;
                }
            }
        }

        public override void behaviorOnCollisionWithOther(GameLocation location)
        {
            if (!ignoreObjectCollisions.Value)
                piercesLeft.Value--;
        }

        public override void behaviorOnCollisionWithPlayer(GameLocation location, Farmer player)
        {
        }

        public override void behaviorOnCollisionWithTerrainFeature(TerrainFeature t, Vector2 tileLocation, GameLocation location)
        {
            t.performUseAction(tileLocation);
            if (!ignoreObjectCollisions.Value)
                piercesLeft.Value--;
        }

        /// <summary>same as basic projectile</summary>
        /// <param name="time"></param>
        public override void updatePosition(GameTime time)
        {
            xVelocity.Value += acceleration.X;
            yVelocity.Value += acceleration.Y;
            if (maxVelocity.Value != -1f && Math.Sqrt(xVelocity.Value * xVelocity.Value + yVelocity.Value * yVelocity.Value) >= (double)maxVelocity.Value)
            {
                xVelocity.Value -= acceleration.X;
                yVelocity.Value -= acceleration.Y;
            }
            position.X += xVelocity.Value;
            position.Y += yVelocity.Value;
        }
    }
}