using StardewValley.Objects;
using StardewValley;
using StardewValley.Monsters;
using TrinketTinker.Model;
using TrinketTinker.Companions;

namespace TrinketTinker.Effects
{
    /// <summary>
    /// Base class for TrinketTinker trinkets
    /// Support spawning arbiturary companions
    /// </summary>
    public class TrinketTinkerEffect : TrinketEffect
    {
        protected CompanionData? CompanionData;

        public TrinketTinkerEffect(Trinket trinket)
            : base(trinket)
        {
            ModEntry.CompanionData.TryGetValue(trinket.ItemId, out CompanionData);
        }

        public override void Apply(Farmer farmer)
        {
            if (CompanionData != null)
            {
                if (Game1.gameMode == 3)
                {
                    if (CompanionData.CompanionClass != null &&
                        Type.GetType(CompanionData.CompanionClass) is Type companionCls)
                        _companion = (TrinketTinkerCompanion?)Activator.CreateInstance(companionCls, _trinket.ItemId);
                    else
                    {
                        _companion = new TrinketTinkerCompanion(_trinket.ItemId);
                    }
                    farmer.AddCompanion(_companion);
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
