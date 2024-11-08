using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using StardewValley.TerrainFeatures;
using TrinketTinker.Effects.Proc;
using TrinketTinker.Models.AbilityArgs;
using TrinketTinker.Wheels;

namespace TrinketTinker.Effects.Pewpew;

/// <summary>
/// Custom projectile class, can utilize custom texture and deal damage with optional knockback crit/crit damage and stun.
/// </summary>
public sealed class TinkerProjectile : Projectile
{
    internal readonly NetString projectileTexture = new("");
    private Texture2D? loadedProjectileTexture = null;
    internal readonly NetInt projectileSpriteWidth = new(16);
    internal readonly NetInt projectileSpriteHeight = new(16);
    internal readonly NetInt minDamage = new(0);
    internal readonly NetInt maxDamage = new(0);
    internal readonly NetFloat knockBackModifier = new(0f);
    internal readonly NetInt addedPrecision = new(0);
    internal readonly NetFloat critChance = new(0f);
    internal readonly NetFloat critMultiplier = new(0f);
    internal readonly NetInt stunTime = new(0);
    internal readonly NetInt hits = new(0);
    internal readonly NetInt explodeRadius = new(0);
    internal readonly NetString stunTAS = new(null);

    /// <summary>Construct an empty instance.</summary>
    public TinkerProjectile()
        : base() { }

    public TinkerProjectile(
        ProjectileArgs args,
        ProcEventArgs proc,
        Monster target,
        Vector2 sourcePosition
    )
        : this()
    {
        if (args.Texture != null)
            projectileTexture.Value = args.Texture;
        currentTileSheetIndex.Value = args.SpriteIndex;
        projectileSpriteWidth.Value = args.SpriteWidth;
        projectileSpriteHeight.Value = args.SpriteHeight;

        Vector2 velocity = Utility.getVelocityTowardPoint(
            sourcePosition,
            target.getStandingPosition(),
            args.MinVelocity
        );
        xVelocity.Value = velocity.X;
        yVelocity.Value = velocity.Y;
        startingRotation.Value = args.RotateToTarget
            ? (float)Math.Atan2(velocity.Y, velocity.X)
            : 0f;
        acceleration.Value = Utility.getVelocityTowardPoint(
            sourcePosition,
            target.getStandingPosition(),
            args.Acceleration
        );
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
        stunTAS.Value = args.StunTAS;
        hits.Value = args.Hits;
        explodeRadius.Value = args.ExplodeRadius;

        damagesMonsters.Value = true;
    }

    /// <inheritdoc />
    protected override void InitNetFields()
    {
        base.InitNetFields();
        NetFields
            .AddField(projectileTexture, "projectileTexture")
            .AddField(projectileSpriteWidth, "projectileSpriteWidth")
            .AddField(projectileSpriteHeight, "projectileSpriteHeight")
            .AddField(minDamage, "minDamage")
            .AddField(maxDamage, "maxDamage")
            .AddField(knockBackModifier, "knockBackModifier")
            .AddField(addedPrecision, "addedPrecision")
            .AddField(critChance, "critChance")
            .AddField(critMultiplier, "critMultiplier")
            .AddField(stunTime, "stunTime")
            .AddField(stunTAS, "stunTAS")
            .AddField(hits, "hits")
            .AddField(explodeRadius, "explodeRadius");
    }

    /// <summary>Get the texture to draw for the projectile.</summary>
    private Texture2D GetCustomTexture()
    {
        if (loadedProjectileTexture != null)
            return loadedProjectileTexture;
        if (projectileTexture.Value != "")
        {
            loadedProjectileTexture ??= Game1.content.Load<Texture2D>(projectileTexture.Value);
            return loadedProjectileTexture;
        }
        return projectileSheet;
    }

    /// <summary>Get the texture name being used.</summary>
    private string GetCustomTexturePath()
    {
        if (loadedProjectileTexture != null)
            return projectileTexture.Value;
        return projectileSheetName;
    }

    /// <summary>Get the texture to draw for the projectile.</summary>
    private Rectangle GetCustomSourceRect(Texture2D texture)
    {
        return Game1.getSourceRectForStandardTileSheet(
            texture,
            currentTileSheetIndex.Value,
            projectileSpriteWidth.Value,
            projectileSpriteHeight.Value
        );
    }

    /// <summary>Needed to override this to get custom texture weh</summary>
    /// <param name="b"></param>
    public override void draw(SpriteBatch b)
    {
        float scale = 4f * localScale;
        Texture2D texture = GetCustomTexture();
        Rectangle sourceRect = GetCustomSourceRect(texture);
        Vector2 value = position.Value;
        b.Draw(
            texture,
            Game1.GlobalToLocal(
                Game1.viewport,
                value + new Vector2(0f, 0f - height.Value) + new Vector2(32f, 32f)
            ),
            sourceRect,
            color.Value * alpha.Value,
            rotation,
            new Vector2(8f, 8f),
            scale,
            SpriteEffects.None,
            (value.Y + 96f) / 10000f
        );
        if (height.Value > 0f)
        {
            b.Draw(
                Game1.shadowTexture,
                Game1.GlobalToLocal(Game1.viewport, value + new Vector2(32f, 32f)),
                Game1.shadowTexture.Bounds,
                Color.White * alpha.Value * 0.75f,
                0f,
                new Vector2(
                    Game1.shadowTexture.Bounds.Center.X,
                    Game1.shadowTexture.Bounds.Center.Y
                ),
                2f,
                SpriteEffects.None,
                (value.Y - 1f) / 10000f
            );
        }
        float num = alpha.Value;
        for (int num2 = tail.Count - 1; num2 >= 0; num2--)
        {
            b.Draw(
                texture,
                Game1.GlobalToLocal(
                    Game1.viewport,
                    Vector2.Lerp(
                        (num2 == tail.Count - 1) ? value : tail.ElementAt(num2 + 1),
                        tail.ElementAt(num2),
                        (float)tailCounter / 50f
                    )
                        + new Vector2(0f, 0f - height.Value)
                        + new Vector2(32f, 32f)
                ),
                sourceRect,
                color.Value * num,
                rotation,
                new Vector2(8f, 8f),
                scale,
                SpriteEffects.None,
                (value.Y - (float)(tail.Count - num2) + 96f) / 10000f
            );
            num -= 1f / (float)tail.Count;
            scale = 0.8f * (float)(4 - 4 / (num2 + 4));
        }
    }

    /// <summary>Deal damage to monster.</summary>
    /// <param name="n"></param>
    /// <param name="location"></param>
    public override void behaviorOnCollisionWithMonster(NPC n, GameLocation location)
    {
        Farmer playerWhoFiredMe = (theOneWhoFiredMe.Get(location) as Farmer) ?? Game1.player;

        if (n is Monster monster)
        {
            if (minDamage.Value > 0)
            {
                for (int i = 1; i < hits.Value; i++)
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
                        triggerMonsterInvincibleTimer: false,
                        who: playerWhoFiredMe,
                        isProjectile: true
                    );
                }
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
            if (stunTime.Value > 0)
            {
                monster.stunTime.Value = stunTime.Value;
                if (stunTAS.Value != null)
                {
                    Vector2 pos = monster.getStandingPosition();
                    Visuals.BroadcastTAS(
                        stunTAS.Value,
                        pos,
                        (pos.Y + 96f) / 10000f,
                        location,
                        duration: stunTime.Value,
                        rotation: rotation
                    );
                }
            }
            if (explodeRadius.Value > 0)
            {
                location.explode(
                    monster.TilePoint.ToVector2(),
                    explodeRadius.Value,
                    playerWhoFiredMe,
                    damage_amount: minDamage.Value
                );
            }
            if (!monster.IsInvisible)
            {
                UpdatePiercesLeft(location);
            }
        }
    }

    public override void behaviorOnCollisionWithOther(GameLocation location)
    {
        if (!ignoreObjectCollisions.Value)
            UpdatePiercesLeft(location);
    }

    public override void behaviorOnCollisionWithPlayer(GameLocation location, Farmer player) { }

    public override void behaviorOnCollisionWithTerrainFeature(
        TerrainFeature t,
        Vector2 tileLocation,
        GameLocation location
    )
    {
        t.performUseAction(tileLocation);
        if (!ignoreObjectCollisions.Value)
            UpdatePiercesLeft(location);
    }

    /// <summary>Same as basic projectile</summary>
    /// <param name="time"></param>
    public override void updatePosition(GameTime time)
    {
        xVelocity.Value += acceleration.X;
        yVelocity.Value += acceleration.Y;
        if (
            maxVelocity.Value != -1f
            && Math.Sqrt(xVelocity.Value * xVelocity.Value + yVelocity.Value * yVelocity.Value)
                >= (double)maxVelocity.Value
        )
        {
            xVelocity.Value -= acceleration.X;
            yVelocity.Value -= acceleration.Y;
        }
        position.X += xVelocity.Value;
        position.Y += yVelocity.Value;
    }

    public void UpdatePiercesLeft(GameLocation location)
    {
        piercesLeft.Value--;
        if (piercesLeft.Value == 0)
        {
            Rectangle sourceRect = GetSourceRect();
            sourceRect.X += 4;
            sourceRect.Y += 4;
            sourceRect.Width = 8;
            sourceRect.Height = 8;
            Game1.createRadialDebris_MoreNatural(
                location,
                GetCustomTexturePath(),
                sourceRect,
                1,
                (int)position.X + 32,
                (int)position.Y + 32,
                6,
                (int)(position.Y / Game1.tileSize) + 1
            );
        }
    }
}
