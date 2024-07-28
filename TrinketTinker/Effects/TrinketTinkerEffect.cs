using StardewValley.Objects;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Companions;
using TrinketTinker.Model;

namespace TrinketTinker.Effects
{
    /// <summary>
    /// Base class for TrinketTinker trinkets
    /// Support spawning arbiturary companions
    /// </summary>
    public class TrinketTinkerEffect : TrinketEffect
    {
        protected CompanionModel? CompanionModel;

        public TrinketTinkerEffect(Trinket trinket)
            : base(trinket)
        {
            ModEntry.CompanionData.TryGetValue(trinket.ItemId, out CompanionModel);
            ModEntry.Log($"{CompanionModel}");
        }

        public override void Apply(Farmer farmer)
        {
            if (CompanionModel != null)
            {
                if (Game1.gameMode == 3)
                {
                    if (CompanionModel.TryGetCompanion(out Companion? newCompanion))
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

        public override void OnDamageMonster(Farmer farmer, Monster monster, int damageAmount)
        {
        }

        public override void GenerateRandomStats(Trinket trinket)
        {
        }
    }
}
