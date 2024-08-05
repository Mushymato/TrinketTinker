using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using TrinketTinker.Models;

namespace TrinketTinker.Companions.Motions
{
    public class HoverMotion : LerpMotion
    {
        private double theta = 0f;

        public HoverMotion(TrinketTinkerCompanion companion, MotionData data) : base(companion, data)
        {
        }

        public override void UpdateLocal(GameTime time, GameLocation location)
        {
            base.UpdateLocal(time, location);
            theta += time.ElapsedGameTime.TotalMilliseconds / (d.Interval * d.AnimationFrameLength);
            c.Offset = MotionOffset + new Vector2(0, 16 * (float)Math.Sin(Math.PI * theta));
            if (theta >= 1f)
                theta = 0f;
        }
    }
}