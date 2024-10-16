
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using TrinketTinker.Models;

namespace TrinketTinker.Companions.Anim
{
    internal sealed class TinkerLightSource : LightSource
    {
        /// <summary>Light source data, not netsync'd</summary>
        private readonly LightSourceData? ld = null;
        /// <summary>If draw color need to update after init.</summary>
        private bool drawColorIsConstant = false;

        /// <inheritdoc/>
        internal TinkerLightSource() : base()
        {
        }

        /// <summary>
        /// Init light source from <see cref="LightSourceData"/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="position"></param>
        /// <param name="ldata"></param>
        internal TinkerLightSource(string id, Vector2 position, LightSourceData ldata) : base(id, ldata.Index, position, ldata.Radius)
        {
            ld = ldata;
            color.Value = VariantData.GetSDVColor(ld.Color, out drawColorIsConstant, defaultColor: Color.Black);
            if (ldata.Texture != null)
                lightTexture = Game1.content.Load<Texture2D>(ldata.Texture);
        }

        /// <inheritdoc/>
        public override void Draw(SpriteBatch spriteBatch, GameLocation location, float lightMultiplier)
        {
            if (ld != null && !drawColorIsConstant)
            {
                color.Value = VariantData.GetSDVColor(ld.Color, out drawColorIsConstant, defaultColor: Color.Black);
            }
            base.Draw(spriteBatch, location, lightMultiplier);
        }
    }
}