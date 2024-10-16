using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using TrinketTinker.Models;
using TrinketTinker.Models.MotionArgs;

namespace TrinketTinker.Companions.Motions
{
    /// <summary>Companion orbits around a point</summary>
    /// <inheritdoc/>
    public class OrbitMotion(TrinketTinkerCompanion companion, MotionData mdata, VariantData vdata)
        : BaseStaticMotion<OrbitArgs>(companion, mdata, vdata)
    {
        /// <summary>trig function input</summary>
        private double theta = 0f;

        /// <summary>
        /// Calculates circular motion using cos for x and sin for y
        /// </summary>
        /// <param name="time"></param>
        /// <param name="location"></param>
        public override void UpdateGlobal(GameTime time, GameLocation location)
        {
            theta += time.ElapsedGameTime.TotalSeconds;
            // c.NetOffset.X = motionOffset.X + args.RadiusX * (float)Math.Cos(Math.PI * theta);
            // c.NetOffset.Y = motionOffset.Y + args.RadiusY * (float)Math.Sin(Math.PI * theta);
            if (theta >= 2f)
                theta = 0f;
            base.UpdateGlobal(time, location);
        }

        /// <inheritdoc/>
        public override Vector2 GetOffset()
        {
            return new Vector2(
                args.RadiusX * (float)Math.Cos(Math.PI * theta),
                args.RadiusY * (float)Math.Sin(Math.PI * theta) - 64f
            ) + base.GetOffset();
        }

        /// <inheritdoc/>
        protected override float GetPositionalLayerDepth(Vector2 offset)
        {
            return (c.Position.Y + offset.Y + 64f - md.Offset.Y) / 10000f;
        }

        /// <inheritdoc/>
        protected override Vector2 GetTextureScale()
        {
            return base.GetTextureScale() * Utility.Lerp(0.96f, 1f, (float)Math.Sin(Math.PI * theta));
        }
    }
}