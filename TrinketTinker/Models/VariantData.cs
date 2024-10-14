using Microsoft.Xna.Framework;
using StardewValley;

namespace TrinketTinker.Models
{
    /// <summary>Data for <see cref="Effects.Abilities"/>, holds sprite variations.</summary>
    public sealed class VariantData
    {
        /// <summary>Variant texture content path.</summary>
        public string Texture { get; set; } = "";

        /// <summary>Sprite width</summary>
        public int Width { get; set; } = 16;

        /// <summary>Sprite height</summary>
        public int Height { get; set; } = 16;

        /// <summary>Draw color mask, can use color name from <see cref="Color"/>, hex value, or <see cref="Constants.COLOR_PRISMATIC"/> for animated prismatic effect.</summary>
        public string? ColorMask { get; set; }

        /// <summary>Create a new <see cref="AnimatedSprite"/></summary>
        internal AnimatedSprite MakeAnimatedSprite()
        {
            return new AnimatedSprite(Texture, 0, Width, Height);
        }
    }
}