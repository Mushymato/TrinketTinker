using Microsoft.Xna.Framework;
using StardewValley;
using TrinketTinker.Wheels;

namespace TrinketTinker.Models
{
    /// <summary>Data for light source on the companion.</summary>
    public class LightSourceData
    {
        /// <summary>Light radius</summary>
        public float Radius = 2f;
        /// <summary>Use a vanilla texture</summary>
        public int Index = 1;
        /// <summary>Optional, use a custom light texture.</summary>
        public string? Texture { get; set; } = null;
        /// <summary>Light source color</summary>
        public string? Color { get; set; } = null;
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

        /// <summary>
        /// Get a monogame color from string.
        /// Supports <see cref="TinkerConst.COLOR_PRISMATIC"/> for animated color.
        /// The default color is White (#FFFFFF).
        /// </summary>
        /// <param name="color">Color string</param>
        /// <param name="isConstant">Indicates that this is not animated, no need to update.</param>
        /// <param name="invert">Invert the RGB components.</param>
        /// <returns></returns>
        public static Color GetSDVColor(string? colorStr, out bool isConstant, bool invert = false)
        {
            Color result;
            if (colorStr == TinkerConst.COLOR_PRISMATIC)
            {
                isConstant = false;
                result = Utility.GetPrismaticColor();
            }
            else
            {
                isConstant = true;
                result = Utility.StringToColor(colorStr) ?? Color.White;
            }
            if (invert)
                return new Color(result.PackedValue ^ 0x00FFFFFF);
            return result;
        }
    }
}