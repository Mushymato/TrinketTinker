using Microsoft.Xna.Framework;
using StardewValley;

namespace TrinketTinker.Models
{
    public class VariantData
    {
        public string Texture { get; set; } = "";
        public int Width { get; set; } = 16;
        public int Height { get; set; } = 16;
        public Color ColorMask { get; set; } = Color.White;

        public AnimatedSprite MakeAnimatedSprite()
        {
            return new AnimatedSprite(Texture, 0, Width, Height);
        }
    }
}