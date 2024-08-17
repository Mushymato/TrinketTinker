using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using TrinketTinker.Models;

namespace TrinketTinker.Effects.Abilities
{
    public class ProjectileAbility : Ability
    {
        /// <summary>min projectile damage</summary>
        protected readonly int minDamage = 1;
        /// <summary>max projectile damage</summary>
        protected readonly int maxDamage = 10;
        /// <summary>projectile sprite</summary>
        protected readonly int spriteIndex = 16;
        /// <summary>projectile range</summary>
        protected readonly int range = 500;
        public ProjectileAbility(TrinketTinkerEffect effect, AbilityData data) : base(effect, data)
        {
            if (d.TryGetParsed("MinDamage", out int? minDamageVal) && d.TryGetParsed("MaxDamage", out int? maxDamageVal))
            {
                minDamage = (int)minDamageVal;
                maxDamage = (int)maxDamageVal;
                Valid = true;
            }
            if (d.TryGetParsed("SpriteIndex", out int? spriteIndexVal))
            {
                spriteIndex = (int)spriteIndexVal;
            }
            if (d.TryGetParsed("SpriteIndex", out int? rangeVal))
            {
                range = (int)rangeVal;
            }
        }

        protected override bool ApplyEffect(Farmer farmer)
        {
            Monster closest_monster = Utility.findClosestMonsterWithinRange(
                farmer.currentLocation, e.CompanionPosition, range, ignoreUntargetables: true
            );
            if (closest_monster != null)
            {
                Vector2 motion = Utility.getVelocityTowardPoint(e.CompanionPosition, closest_monster.getStandingPosition(), 2f);
                float projectile_rotation = (float)Math.Atan2(motion.Y, motion.X) + (float)Math.PI / 2f;
                BasicProjectile p = new(
                    Game1.random.Next(minDamage, maxDamage),
                    spriteIndex,
                    0, 0, 0f,
                    motion.X, motion.Y,
                    e.CompanionPosition,
                    explode: false,
                    damagesMonsters: true,
                    location: farmer.currentLocation,
                    firer: farmer
                )
                {
                    IgnoreLocationCollision = true
                };
                p.ignoreObjectCollisions.Value = true;
                p.acceleration.Value = motion;
                p.maxVelocity.Value = 24f;
                p.projectileID.Value = 14;
                p.startingRotation.Value = projectile_rotation;
                p.alpha.Value = 0.001f;
                p.alphaChange.Value = 0.05f;
                p.light.Value = true;

                // p.collisionSound.Value = "magic_arrow_hit";
                // farmer.currentLocation.playSound("magic_arrow");

                farmer.currentLocation.projectiles.Add(p);
                return base.ApplyEffect(farmer);
            }
            return false;
        }
    }
}
