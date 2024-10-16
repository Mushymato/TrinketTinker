using Microsoft.Xna.Framework;
using StardewValley;
using TrinketTinker.Models;
using TrinketTinker.Models.MotionArgs;

namespace TrinketTinker.Companions.Motions
{
    /// <summary>Companion follows the player and bobs up and down</summary>
    /// <inheritdoc/>
    public sealed class HoverMotion(TrinketTinkerCompanion companion, MotionData data, VariantData vdata)
        : BaseLerpMotion<HoverArgs>(companion, data, vdata)
    {
        private const float DEFAULT_HEIGHT = 96f;
        /// <summary>trig function input</summary>
        private double theta = 0f;

        /// <inheritdoc/>
        public override void UpdateGlobal(GameTime time, GameLocation location)
        {
            theta += time.ElapsedGameTime.TotalMilliseconds / (md.Interval * md.AnimationFrameLength);
            if (theta >= 1f)
                theta = 0f;
            base.UpdateGlobal(time, location);
        }

        /// <summary>Get offset</summary>
        /// <returns></returns>
        public override Vector2 GetOffset()
        {
            return new Vector2(0, args.Magnitude * (float)Math.Sin(Math.PI * theta) - DEFAULT_HEIGHT) + base.GetOffset();
        }
    }
}