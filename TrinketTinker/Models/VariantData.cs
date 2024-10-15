using Microsoft.Xna.Framework;
using StardewValley;
using TrinketTinker.Wheels;

namespace TrinketTinker.Models
{
    /// <summary>Data for light source on the companion.</summary>
    public class LightSourceData
    {
        /// <summary>Light radius</summary>
        public float Radius = 0f;
        /// <summary>Which vanilla light texture to use</summary>
        public int TextureIndex = 1;
        /// <summary>Light source color</summary>
        public string? Color { get; set; }
    }

    /// <summary>Data for <see cref="Effects.Abilities"/>, holds sprite variations.</summary>
    public sealed class VariantData
    {
        /// <summary>Variant texture content path.</summary>
        public string Texture { get; set; } = "";
        /// <summary>Sprite width</summary>
        public int Width { get; set; } = 16;
        /// <summary>Sprite height</summary>
        public int Height { get; set; } = 16;
        /// <summary>Draw color mask, can use color name from <see cref="Color"/>, hex value, or <see cref="TinkerConst.COLOR_PRISMATIC"/> for animated prismatic effect.</summary>
        public string? ColorMask { get; set; }
        /// <summary>Base scale to draw texture at.</summary>
        public float TextureScale { get; set; } = 4f;
        /// <summary>Base scale to draw shadow texture.</summary>
        public float ShadowScale { get; set; } = 3f;
        /// <summary>If set, add a light with given radius. Note that the light is only visible to local player.</summary>
        public LightSourceData? LightSource { get; set; } = null;

        /// <summary>Get to vector2 texture scale for draw</summary>
        internal Vector2 VecShadowScale => new(TextureScale, TextureScale);
        /// <summary>Get to vector2 shadow scale for draw</summary>
        internal Vector2 VecTextureScale => new(TextureScale, TextureScale);
        /// <summary>Create a new <see cref="AnimatedSprite"/></summary>
        internal AnimatedSprite MakeAnimatedSprite()
        {
            return new AnimatedSprite(Texture, 0, Width, Height);
        }
    }
}