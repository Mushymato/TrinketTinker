using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Monsters;
using TrinketTinker.Models.Mixin;
using TrinketTinker.Wheels;

namespace TrinketTinker.Models.AbilityArgs;

/// <summary>Damage to monster argument</summary>
public class DamageArgs : IArgs
{
    /// <summary>Min damage</summary>
    public int Min { get; set; } = 0;

    /// <summary>Max damage, out of 1</summary>
    public int Max { get; set; } = 0;

    /// <summary>Pixel range for finding monsters</summary>
    public int Range { get; set; } = Game1.tileSize;

    /// <summary>Restrict monster picks to facing direction only</summary>
    public bool FacingDirectionOnly { get; set; } = false;

    /// <summary>Knockback modifier</summary>
    public float Knockback { get; set; } = 0f;

    /// <summary>Precision modifier</summary>
    public int Precision { get; set; } = 0;

    /// <summary>Critical chance</summary>
    public float CritChance { get; set; } = 0f;

    /// <summary>Critical damage</summary>
    public float CritDamage { get; set; } = 0f;

    /// <summary>Stun time in miliseconds</summary>
    public int StunTime { get; set; } = 0;

    /// <summary>
    /// Temporary sprite to display while an enemy is stunned, must be defined in mushymato.TrinketTinker/TAS.
    /// Loop will be overwritten by the stun time.
    /// Rotation will be overwritten if this is used for a projectile.
    /// </summary>
    public string? StunTAS { get; set; } = null;

    /// <summary>
    /// Temporary sprite to display on hit, must be defined in mushymato.TrinketTinker/TAS.
    /// Rotation will be overwritten if this is used for a projectile.
    /// </summary>
    public string? HitTAS { get; set; } = null;

    /// <summary>Number of hits to perform</summary>
    public int Hits { get; set; } = 1;

    /// <summary>Delay between hits</summary>
    public int HitsDelay { get; set; } = 0;

    /// <summary>
    /// If this is non-zero, generate a explosion on hit.
    /// Farmer will take <see cref="Min"/> damage from this.
    /// Might damage another monster, but the monster that got hit would be in iframe at this point.
    /// </summary>
    public int ExplodeRadius { get; set; } = 0;

    /// <summary>List of monster types to avoid targeting.</summary>
    public List<string>? Filters = null;

    /// <inheritdoc/>
    public bool Validate()
    {
        if (Range < 1)
            return false;
        if (Min > Max)
        {
            if (Min == 0 && StunTime == 0)
                return false;
            Max = Min;
        }
        return true;
    }

    /// <summary>Do damage and debuff on monster.</summary>
    /// <param name="proc"></param>
    /// <param name="target"></param>
    public void DamageMonster(GameStateQueryContext context, Monster target, bool isProjectile)
    {
        Vector2 pos = target.GetBoundingBox().Center.ToVector2();
        float drawLayer = pos.Y / 10000f + Visuals.LAYER_OFFSET;
        if (Min > 0)
        {
            if (HitsDelay == 0)
                for (int i = 1; i < Hits; i++)
                {
                    WrappedDamageMonster(context, target, isProjectile, false);
                }
            else
            {
                for (int i = 1; i < Hits; i++)
                {
                    DelayedAction.functionAfterDelay(
                        () =>
                        {
                            WrappedDamageMonster(context, target, isProjectile, false);
                        },
                        i * HitsDelay
                    );
                }
            }
            WrappedDamageMonster(context, target, isProjectile, false);
            if (HitTAS != null)
            {
                Visuals.BroadcastTASList(HitTAS.Split('|'), pos, drawLayer, context);
            }
        }
        if (StunTime > 0)
        {
            target.stunTime.Value = StunTime;
            if (StunTAS != null)
            {
                if (!Visuals.BroadcastTAS(StunTAS, pos, drawLayer, context, duration: StunTime))
                    StunTAS = null;
            }
        }
        if (ExplodeRadius > 0)
        {
            context.Location.explode(target.TilePoint.ToVector2(), ExplodeRadius, context.Player, damage_amount: Min);
        }
    }

    private void WrappedDamageMonster(
        GameStateQueryContext context,
        Monster target,
        bool isProjectile,
        bool triggerMonsterInvincibleTimer
    )
    {
        context.Location.damageMonster(
            areaOfEffect: target.GetBoundingBox(),
            minDamage: Min,
            maxDamage: Max,
            isBomb: false,
            knockBackModifier: Knockback,
            addedPrecision: Precision,
            critChance: CritChance,
            critMultiplier: CritDamage,
            triggerMonsterInvincibleTimer: triggerMonsterInvincibleTimer,
            who: context.Player,
            isProjectile: isProjectile
        );
    }
}
