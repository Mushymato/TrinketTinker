using StardewValley.Monsters;
using TrinketTinker.Effects.Proc;
using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Models.AbilityArgs
{
    /// <summary>Damage to monster argument</summary>
    public class DamageArgs : IArgs
    {
        /// <summary>Min damage</summary>
        public int Min { get; set; } = 0;
        /// <summary>Max damage, out of 1</summary>
        public int Max { get; set; } = 0;
        /// <summary>Pixel range for finding monsters</summary>
        public int Range { get; set; } = 64;
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
        /// <summary>Number of hits to perform</summary>
        public int Hits { get; set; } = 1;

        /// <summary>Random percent between min and max</summary>
        public int Rand => Random.Shared.Next(Min, Max);
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
        public void DamageMonster(ProcEventArgs proc, Monster target)
        {
            if (Min > 0)
            {
                for (int i = 1; i < Hits; i++)
                {
                    proc.LocationOrCurrent.damageMonster(
                        areaOfEffect: target.GetBoundingBox(),
                        minDamage: Min,
                        maxDamage: Max,
                        isBomb: false,
                        knockBackModifier: Knockback,
                        addedPrecision: Precision,
                        critChance: CritChance,
                        critMultiplier: CritDamage,
                        triggerMonsterInvincibleTimer: false,
                        who: proc.Farmer,
                        isProjectile: false
                    );
                }
                proc.LocationOrCurrent.damageMonster(
                    areaOfEffect: target.GetBoundingBox(),
                    minDamage: Min,
                    maxDamage: Max,
                    isBomb: false,
                    knockBackModifier: Knockback,
                    addedPrecision: Precision,
                    critChance: CritChance,
                    critMultiplier: CritDamage,
                    triggerMonsterInvincibleTimer: true,
                    who: proc.Farmer,
                    isProjectile: false
                );
            }
            if (StunTime > 0)
            {
                target.stunTime.Value = StunTime;
            }
        }
    }
}