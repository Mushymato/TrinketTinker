using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using TrinketTinker.Models;

namespace TrinketTinker.Effects.Abilities
{
    /// <summary>Fires a projectile that either deals damage, or apply a debuff</summary>
    public class ProjectileAbility : Ability
    {
        /// <summary>projectile sprite</summary>
        protected readonly int spriteIndex = 16;
        /// <summary>projectile range</summary>
        protected readonly int range = 500;
        /// <summary>projectile max velocity</summary>
        protected readonly float speed = 2f;
        /// <summary>projectile should apply debuff</summary>
        protected readonly string? debuffId = null;
        /// <summary>min projectile damage</summary>
        protected readonly int minDamage = 1;
        /// <summary>max projectile damage</summary>
        protected readonly int maxDamage = 10;
        /// <summary>projectile should explode</summary>
        protected readonly bool isExplosive = false;

        /// <summary>the projectile make func to use</summary>
        private readonly Func<Farmer, Monster, Projectile> MakeProjectile;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public ProjectileAbility(TrinketTinkerEffect effect, AbilityData data, int lvl) : base(effect, data, lvl)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            if (d.TryGetParsed("MinDamage", out int? minDamageVal) && d.TryGetParsed("MaxDamage", out int? maxDamageVal))
            {
                minDamage = (int)minDamageVal;
                maxDamage = (int)maxDamageVal;
                isExplosive = d.GetParsedOrDefault("IsExplosive", isExplosive);
                Valid = true;
                MakeProjectile = MakeProjectile_Basic;
            }
            else if (d.TryGetParsed("DebuffId", out string? argsDebuffId))
            {
                debuffId = argsDebuffId;
                Valid = true;
                MakeProjectile = MakeProjectile_Debuffing;
            }
            else
            {
                return;
            }

            spriteIndex = d.GetParsedOrDefault("SpriteIndex", spriteIndex);
            range = d.GetParsedOrDefault("Range", range);
            speed = d.GetParsedOrDefault("Speed", speed);
        }

        private BasicProjectile MakeProjectile_Basic(Farmer farmer, Monster monster)
        {
            Vector2 motion = Utility.getVelocityTowardPoint(e.CompanionPosition, monster.getStandingPosition(), speed);
            float projectile_rotation = (float)Math.Atan2(motion.Y, motion.X) + (float)Math.PI / 2f;
            ;

            BasicProjectile p = new(
                Game1.random.Next(minDamage, maxDamage),
                spriteIndex,
                0, 0, 0f,
                motion.X, motion.Y,
                e.CompanionPosition,
                explode: isExplosive,
                damagesMonsters: true,
                location: farmer.currentLocation,
                firer: farmer
            );

            p.ignoreObjectCollisions.Value = true;
            p.IgnoreLocationCollision = true;
            // p.acceleration.Value = motion;
            p.maxVelocity.Value = speed;
            // p.projectileID.Value = 14;
            p.startingRotation.Value = projectile_rotation;
            p.alpha.Value = 0.001f;
            p.alphaChange.Value = 0.05f;
            p.light.Value = true;

            return p;
        }

        private DebuffingProjectile MakeProjectile_Debuffing(Farmer farmer, Monster monster)
        {
            Vector2 motion = Utility.getVelocityTowardPoint(farmer.getStandingPosition(), monster.getStandingPosition(), speed);
            DebuffingProjectile p = new(
                debuffId,
                spriteIndex,
                0, 0, 0f,
                motion.X, motion.Y,
                e.CompanionPosition,
                location: farmer.currentLocation,
                owner: farmer,
                hitsMonsters: true,
                playDefaultSoundOnFire: false
            );

            p.wavyMotion.Value = false;
            p.piercesLeft.Value = 99999;
            p.maxTravelDistance.Value = 3000;
            p.IgnoreLocationCollision = true;
            p.ignoreObjectCollisions.Value = true;
            p.maxVelocity.Value = speed;
            // p.projectileID.Value = 15;
            p.alpha.Value = 0.001f;
            p.alphaChange.Value = 0.05f;
            p.light.Value = true;
            p.boundingBoxWidth.Value = 32;

            if (debuffId == "frozen")
                p.debuffIntensity.Value = 4000;

            return p;
        }

        protected override bool ApplyEffect(Farmer farmer)
        {
            Monster closest_monster = Utility.findClosestMonsterWithinRange(
                farmer.currentLocation, e.CompanionPosition, range, ignoreUntargetables: true
            );
            if (closest_monster != null)
            {
                farmer.currentLocation.projectiles.Add(MakeProjectile(farmer, closest_monster));
                return base.ApplyEffect(farmer);
            }
            return false;
        }
    }
}
