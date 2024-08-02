using Microsoft.Xna.Framework;
using StardewValley;
using TrinketTinker.Model;

namespace TrinketTinker.Companions.Motions
{
    public class StaticMotion : Motion
    {
        public StaticMotion(TrinketTinkerCompanion companion, MotionData data) : base(companion, data) { }
        public override void UpdateLocal(GameTime time, GameLocation location)
        {
            c.lerp = -1;
            c.Moving = false;
            c.Position = c.Owner.Position;
        }
    }
}