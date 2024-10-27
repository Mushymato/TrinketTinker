namespace TrinketTinker.Models.AbilityArgs
{
    public sealed class ProjectileArgs : DamageArgs
    {
        /// <summary>Projectile texture, need to be 16x16</summary>
        public string? Texture { get; set; } = null;
        /// <summary>Projectile texture sprite index</summary>
        public int SpriteIndex { get; set; } = 0;
        /// <summary>Projectile texture sprite width</summary>
        public int SpriteWidth { get; set; } = 16;
        /// <summary>Projectile texture sprite height</summary>
        public int SpriteHeight { get; set; } = 16;
        /// <summary>Number of trailing sprites to draw</summary>
        public int TailCount { get; set; } = 0;
        /// <summary>Initial velocity for projectile</summary>
        public float MinVelocity { get; set; } = 12;
        /// <summary>Maximum velocity or -1 if not capped</summary>
        public float MaxVelocity { get; set; } = -1;
        /// <summary>Acceleration per tick</summary>
        public float Acceleration { get; set; } = 1;
        /// <summary>Rotate the rightside of sprite towards target</summary>
        public bool RotateToTarget { get; set; } = true;
        /// <summary>Number of enemies the projectile can pass through before it is destroyed</summary>
        public int Pierce { get; set; } = 1;
        /// <summary>Let projectile pass through objects/terrain feature</summary>
        public bool IgnoreObjectCollisions { get; set; } = false;
        /// <summary>Let projectile pass through location walls</summary>
        public bool IgnoreLocationCollisions { get; set; } = false;
    }
}