using StardewValley.Objects;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Companions;
using Microsoft.VisualBasic;
using Netcode;

namespace TrinketTinker.Effects
{
    /// <summary>
    /// Base class for TrinketTinker trinkets, main purpose is to support spawning
    /// arbiturary companions.
    /// </summary>
    public class TrinketTinkerEffect : TrinketEffect
    {
        protected Type? CompanionClass = null;
        protected string? CompanionTexture = null;

        public TrinketTinkerEffect(Trinket trinket)
            : base(trinket)
        {
            if (trinket.GetTrinketMetadata("mushymato.TrinketTinker/CompanionClass") is string companionClassStr)
            {
                CompanionClass = Type.GetType(companionClassStr);
                CompanionTexture = trinket.GetTrinketMetadata("mushymato.TrinketTinker/CompanionTexture");
            }
        }

        public override void Apply(Farmer farmer)
        {
            if (CompanionClass != null)
            {
                if (Game1.gameMode == 3)
                {
                    Companion? newCompanion = (Companion?)Activator.CreateInstance(
                        CompanionClass,
                        new object?[] { CompanionTexture }
                    );
                    if (newCompanion != null)
                    {
                        _companion = newCompanion;
                        farmer.AddCompanion(_companion);
                    }
                }
                base.Apply(farmer);
            }
        }

        public override void Unapply(Farmer farmer)
        {
            farmer.RemoveCompanion(_companion);
        }

        // void out various functions that should not have been impl in a base class
        public override void OnDamageMonster(Farmer farmer, Monster monster, int damageAmount)
        {
        }

        public override void GenerateRandomStats(Trinket trinket)
        {
        }
    }
}
