using Microsoft.Xna.Framework;
using StardewValley;
using TrinketTinker.Models;
using TrinketTinker.Models.MotionArgs;

namespace TrinketTinker.Companions.Motions
{
    /// <summary>Companion follows the player and bobs up and down</summary>
    public sealed class HoverMotion : BaseLerpMotion<HoverArgs>
    {
        /// <summary>trig function input</summary>
        private double theta = 0f;

        public HoverMotion(TrinketTinkerCompanion companion, MotionData data) : base(companion, data)
        {
            motionOffset.Y -= 128f;
            c.Offset = motionOffset;
        }

        public override void UpdateLocal(GameTime time, GameLocation location)
        {
            base.UpdateLocal(time, location);
            theta += time.ElapsedGameTime.TotalMilliseconds / (d.Interval * d.AnimationFrameLength);
            c.Offset = motionOffset + new Vector2(0, args.Magnitude * (float)Math.Sin(Math.PI * theta));
            if (theta >= 1f)
                theta = 0f;
        }
    }
}