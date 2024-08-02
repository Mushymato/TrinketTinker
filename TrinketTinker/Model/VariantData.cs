using StardewValley;

namespace TrinketTinker.Model
{
    public class VariantData
    {
        public string Texture { get; set; } = "";
        public int Width { get; set; } = 16;
        public int Height { get; set; } = 16;

        public AnimatedSprite MakeAnimatedSprite()
        {
            return new AnimatedSprite(Texture, 0, Width, Height);
        }
    }
}