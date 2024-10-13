using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using TrinketTinker.Models;
using TrinketTinker.Models.MotionArgs;

namespace TrinketTinker.Companions.Motions
{
    public sealed class RelativeMotion(TrinketTinkerCompanion companion, MotionData data) : BaseStaticMotion<StaticArgs>(companion, data)
    {
        /// <summary>Update companion facing direction using player facing direction, adjust Offset.</summary>
        protected override void UpdateDirection()
        {
            base.UpdateDirection();
        }
    }
}