using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using TrinketTinker.Models;

namespace TrinketTinker.Effects.Abilities
{
    public class ProjectileAbility : Ability
    {
        public ProjectileAbility(TrinketTinkerEffect effect, AbilityData data) : base(effect, data)
        {
            Valid = true;
        }

        protected override bool ApplyEffect(Farmer farmer)
        {
            Monster closest_monster = Utility.findClosestMonsterWithinRange(
                farmer.currentLocation, e.CompanionPosition, 500, ignoreUntargetables: true
            );
            if (closest_monster != null)
            {
                Vector2 motion = Utility.getVelocityTowardPoint(e.CompanionPosition, closest_monster.getStandingPosition(), 2f);
                float projectile_rotation = (float)Math.Atan2(motion.Y, motion.X) + (float)Math.PI / 2f;
                BasicProjectile p = new(Game1.random.Next(10, 100), 16, 0, 0, 0f, motion.X, motion.Y, e.CompanionPosition, null, null, null, explode: false, damagesMonsters: true, farmer.currentLocation, farmer)
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
                p.collisionSound.Value = "magic_arrow_hit";

                farmer.currentLocation.projectiles.Add(p);
                farmer.currentLocation.playSound("magic_arrow");
            }
            return base.ApplyEffect(farmer);
        }
    }
}
