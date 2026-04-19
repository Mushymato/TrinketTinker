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

    /// <summary>Change if explosion should damange farmer</summary>
    public bool ExplodeDamagesFarmer { get; set; } = true;

    /// <summary>Pixel radius for splash damage around the impacted target.</summary>
    public int AoERadius { get; set; } = 0;

    /// <summary>
    /// When true, AoE is centered on the companion position instead of an impact location.
    /// Intended for hitscan pulse / aura style attacks.
    /// </summary>
    public bool AoEFromCompanion { get; set; } = false;

    /// <summary>
    /// If this is set, override whether the damage incurred by this is projectile and bypasses barriers.
    /// Otherwise, default to the damage type dependent value (false for HitscanAbility, true for ProjectileAbility)
    /// </summary>
    public bool? TreatAsProjectile { get; set; } = null;

    /// <summary>List of monster types to avoid targeting.</summary>
    public List<string>? Filters = null;

    /// <inheritdoc/>
    public bool Validate()
    {
        if (Range < 1)
            return false;
        if (AoERadius < 0) //no negative values
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
    public void DamageMonster(GameStateQueryContext context, Monster target, bool isProjectile)
    {
        isProjectile = TreatAsProjectile ?? isProjectile;
        Vector2 impactCenter = target.GetBoundingBox().Center.ToVector2();

        void DoHit() //turning impact point damage into impact point AoE
        {
            foreach (Monster hitTarget in GetAoETargets(context, target, impactCenter))
            {
                WrappedDamageMonster(context, hitTarget, isProjectile, false);
            }
        }

        if (Min > 0)
        {
            if (HitsDelay == 0)
            {
                for (int i = 1; i < Hits; i++)
                    DoHit();
            }
            else
            {
                for (int i = 1; i < Hits; i++)
                    DelayedAction.functionAfterDelay(DoHit, i * HitsDelay);
            }

            DoHit();

            if (HitTAS != null)
            {
                foreach (Monster hitTarget in GetAoETargets(context, target, impactCenter))
                    BroadcastHitTAS(context, hitTarget);
            }
        }

        if (StunTime > 0)
        {
            foreach (Monster hitTarget in GetAoETargets(context, target, impactCenter))
            {
                hitTarget.stunTime.Value = StunTime;
                BroadcastStunTAS(context, hitTarget);
            }
        }

        if (ExplodeRadius > 0)
        {
            context.Location?.explode(
                target.TilePoint.ToVector2(),
                ExplodeRadius,
                context.Player,
                damageFarmers: ExplodeDamagesFarmer,
                damage_amount: Min
            );
        }
    }

    /// <summary>
    /// Do one AoE pulse around a point.
    /// Take a point in space, find monsters in radius around that point, apply damage/stun etc.
    /// </summary>
    public bool DamageAtPoint(
        GameStateQueryContext context,
        Vector2 center,
        bool isProjectile,
        Func<Monster, bool>? match = null
    )
    {
        isProjectile = TreatAsProjectile ?? isProjectile;
        List<Monster> targets = GetAoETargetsAtPoint(context, center, match).ToList();

        if (targets.Count == 0)
            return false;

        if (Min > 0)
        {
            foreach (Monster hitTarget in targets)
            {
                WrappedDamageMonster(context, hitTarget, isProjectile, false);
            }

            if (HitTAS != null)
            {
                foreach (Monster hitTarget in targets)
                    BroadcastHitTAS(context, hitTarget);
            }
        }

        if (StunTime > 0)
        {
            foreach (Monster hitTarget in targets)
            {
                hitTarget.stunTime.Value = StunTime;
                BroadcastStunTAS(context, hitTarget);
            }
        }

        return true;
    }

    private IEnumerable<Monster> GetAoETargets(GameStateQueryContext context, Monster primaryTarget, Vector2 impactCenter)
    {
        if (context.Location == null || AoERadius <= 0)
        {
            yield return primaryTarget;
            yield break;
        }

        bool foundPrimary = false;

        foreach (Monster monster in context.Location.characters.OfType<Monster>())
        {
            if (monster.IsInvisible)
                continue;

            // Keep existing behavior for the primary target, but let Filters exclude secondary splash targets.
            if (monster != primaryTarget && (Filters?.Contains(monster.Name) ?? false))
                continue;

            if (Vector2.Distance(monster.GetBoundingBox().Center.ToVector2(), impactCenter) > AoERadius)
                continue;

            if (monster == primaryTarget)
                foundPrimary = true;

            yield return monster;
        }

        if (!foundPrimary)
            yield return primaryTarget;
    }

    /// <summary>Needed helper method to find monsters around an arbitrary point.</summary>
    private IEnumerable<Monster> GetAoETargetsAtPoint( 
        GameStateQueryContext context,
        Vector2 center,
        Func<Monster, bool>? match = null
    )
    {
        if (context.Location == null || AoERadius <= 0)
            yield break;

        foreach (Monster monster in context.Location.characters.OfType<Monster>())
        {
            if (monster.IsInvisible)
                continue;

            if ((Filters?.Contains(monster.Name) ?? false))
                continue;

            if (match != null && !match(monster))
                continue;

            if (Vector2.Distance(monster.GetBoundingBox().Center.ToVector2(), center) > AoERadius)
                continue;

            yield return monster;
        }
    }

    private void BroadcastHitTAS(GameStateQueryContext context, Monster target)
    {
        if (HitTAS == null)
            return;

        Vector2 pos = target.GetBoundingBox().Center.ToVector2();
        float drawLayer = pos.Y / 10000f + Visuals.LAYER_OFFSET;
        Visuals.BroadcastTASList(HitTAS.Split('|'), pos, drawLayer, context);
    }

    private void BroadcastStunTAS(GameStateQueryContext context, Monster target)
    {
        if (StunTAS == null)
            return;

        Vector2 pos = target.GetBoundingBox().Center.ToVector2();
        float drawLayer = pos.Y / 10000f + Visuals.LAYER_OFFSET;

        if (!Visuals.BroadcastTAS(StunTAS, pos, drawLayer, context, duration: StunTime))
            StunTAS = null;
    }

    private void WrappedDamageMonster(
        GameStateQueryContext context,
        Monster target,
        bool isProjectile,
        bool triggerMonsterInvincibleTimer
    )
    {
        context.Location?.damageMonster(
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