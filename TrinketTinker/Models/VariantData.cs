using StardewValley;

namespace TrinketTinker.Models
{
    /// <summary>Data for <see cref="Effects.Abilities"/>, holds sprite variations.</summary>
    public class VariantData
    {
        /// <summary>Variant texture name.</summary>
        public string Texture { get; set; } = "";

        /// <summary>Sprite width</summary>
        public int Width { get; set; } = 16;

        /// <summary>Sprite height</summary>
        public int Height { get; set; } = 16;

        /// <summary>Draw color mask</summary>
        public string? ColorMask { get; set; }

        /// <summary>Create a new <see cref="AnimatedSprite"/></summary>
        /// <returns></returns>
        public AnimatedSprite MakeAnimatedSprite()
        {
            return new AnimatedSprite(Texture, 0, Width, Height);
        }
    }
}