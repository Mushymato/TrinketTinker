using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using StardewValley.TerrainFeatures;
using TrinketTinker.Models.AbilityArgs;

namespace TrinketTinker.Effects.Support;

/// <summary>
/// Custom projectile class, can utilize custom texture and deal damage with optional knockback crit/crit damage and stun.
/// </summary>
public sealed class TinkerProjectile : Projectile
{
    private ProjectileArgs? args = null;
    internal readonly NetString projectileTexture = new("");
    private Texture2D? loadedProjectileTexture = null;
    internal readonly NetInt projectileSpriteWidth = new(16);
    internal readonly NetInt projectileSpriteHeight = new(16);
    internal readonly NetBool rotateToTarget = new(false);
    internal readonly NetInt homingRange = new(0);
    internal readonly NetStringList filters = new();
    internal readonly GameStateQueryContext context;
    private double homingTimer = 0;

    /// <summary>Construct an empty instance.</summary>
    public TinkerProjectile()
        : base() { }

    public TinkerProjectile(ProjectileArgs args, ProcEventArgs proc, Monster target, Vector2 sourcePosition)
        : this()
    {
        this.args = args;
        if (args.Texture != null)
            projectileTexture.Value = args.Texture;
        currentTileSheetIndex.Value = args.SpriteIndex;
        projectileSpriteWidth.Value = args.SpriteWidth;
        projectileSpriteHeight.Value = args.SpriteHeight;
        height.Value = args.Height;

        position.Value = sourcePosition;
        UpdateVelocityAndAcceleration(target.GetBoundingBox().Center.ToVector2(), args.MinVelocity, args.Acceleration);
        rotateToTarget.Value = args.RotateToTarget;
        startingRotation.Value = rotateToTarget.Value ? (float)Math.Atan2(yVelocity.Value, xVelocity.Value) : 0f;

        piercesLeft.Value = args.Pierce;
        ignoreObjectCollisions.Value = args.IgnoreObjectCollisions;
        ignoreLocationCollision.Value = args.IgnoreLocationCollisions;
        theOneWhoFiredMe.Set(proc.Location, proc.Farmer);
        context = proc.GSQContext;

        if (args.Homing)
        {
            homingRange.Value = args.Range;
        }
        if (args.Filters != null)
        {
            filters.AddRange(args.Filters);
        }

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
            .AddField(homingRange, "homingRange")
            .AddField(rotateToTarget, "rotateToTarget")
            .AddField(filters, "filters");
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
        Vector2 offsetByHeight = new(0f, -height.Value);
        Vector2 offsetToCenter = new(projectileSpriteWidth.Value / 2, projectileSpriteHeight.Value / 2);
        b.Draw(
            texture,
            Game1.GlobalToLocal(Game1.viewport, value + offsetByHeight + offsetToCenter),
            sourceRect,
            color.Value * alpha.Value,
            rotation,
            new Vector2(8f, 8f),
            scale,
            SpriteEffects.None,
            (value.Y + 96f) / 10000f
        );
        if (projectileSpriteHeight.Value > 0f)
        {
            b.Draw(
                Game1.shadowTexture,
                Game1.GlobalToLocal(Game1.viewport, value + offsetToCenter),
                Game1.shadowTexture.Bounds,
                Color.White * alpha.Value * 0.75f,
                0f,
                new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y),
                2f * new Vector2(projectileSpriteWidth.Value / 16f, projectileSpriteHeight.Value / 16f),
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
                        tailCounter / 50f
                    )
                        + offsetByHeight
                        + offsetToCenter
                ),
                sourceRect,
                color.Value * num,
                rotation,
                new Vector2(8f, 8f),
                scale,
                SpriteEffects.None,
                (value.Y - (tail.Count - num2) + 96f) / 10000f
            );
            num -= 1f / tail.Count;
            scale = 0.8f * (4 - 4 / (num2 + 4));
        }

        if (ModEntry.Config.DrawDebugMode)
        {
            Rectangle boundingBox = getBoundingBox();
            Utility.DrawSquare(
                b,
                Game1.GlobalToLocal(Game1.viewport, boundingBox),
                0,
                backgroundColor: Color.BlueViolet
            );
        }
    }

    public override Rectangle getBoundingBox()
    {
        Vector2 value = position.Value;
        float scale = localScale;
        float width = projectileSpriteWidth.Value * scale;
        float height = projectileSpriteHeight.Value * scale;
        return new Rectangle((int)(value.X - (width / 2)), (int)(value.Y - (height / 2)), (int)width, (int)height);
    }

    /// <summary>Deal damage to monster.</summary>
    /// <param name="n"></param>
    /// <param name="location"></param>
    public override void behaviorOnCollisionWithMonster(NPC n, GameLocation location)
    {
        Farmer playerWhoFiredMe = (theOneWhoFiredMe.Get(location) as Farmer) ?? Game1.player;
        if (n is Monster monster)
        {
            if (!monster.IsInvisible)
                UpdatePiercesLeft(location);
            if (!playerWhoFiredMe.IsLocalPlayer)
                return;
            args?.DamageMonster(context, monster, true);
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

    private void UpdateVelocityAndAcceleration(Vector2 targetPosition, float velocity, float accel)
    {
        Vector2 velocityVect = Utility.getVelocityTowardPoint(position.Value, targetPosition, velocity);
        xVelocity.Value = velocityVect.X;
        yVelocity.Value = velocityVect.Y;
        acceleration.Value = Utility.getVelocityTowardPoint(position.Value, targetPosition, accel);
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
            DebrisAnimation(location);
        }
    }

    private void DebrisAnimation(GameLocation location)
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

    public override bool update(GameTime time, GameLocation location)
    {
        if (homingRange.Value > 0)
        {
            homingTimer += time.ElapsedGameTime.TotalMilliseconds;
            if (homingTimer > 100f)
            {
                homingTimer = 0;
                Monster homingTarget = Utility.findClosestMonsterWithinRange(
                    location,
                    position.Value,
                    homingRange.Value,
                    ignoreUntargetables: true,
                    match: filters.Any() ? (m) => !filters.Contains(m.Name) : null
                );
                if (homingTarget != null)
                {
                    UpdateVelocityAndAcceleration(
                        homingTarget.GetBoundingBox().Center.ToVector2(),
                        new Vector2(xVelocity.Value, yVelocity.Value).Length(),
                        acceleration.Value.Length()
                    );
                    _rotation = rotateToTarget.Value ? (float)Math.Atan2(yVelocity.Value, xVelocity.Value) : 0f;
                }
                else
                {
                    return true;
                }
            }
        }
        return base.update(time, location);
    }
}
