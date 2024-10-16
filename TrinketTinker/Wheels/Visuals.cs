using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;
using TrinketTinker.Effects;
using TrinketTinker.Models;

namespace TrinketTinker.Wheels
{
    /// <summary>Helper methods for visual effects</summary>
    internal static class Visuals
    {

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

        public static void BroadcastTAS(TemporaryAnimatedSpriteDefinition temporarySprite, Vector2 position, float drawLayer, GameLocation location)
        {
            TemporaryAnimatedSprite temporaryAnimatedSprite = new(
                temporarySprite.Texture,
                temporarySprite.SourceRect,
                temporarySprite.Interval,
                temporarySprite.Frames,
                temporarySprite.Loops,
                position + temporarySprite.PositionOffset * 4f,
                temporarySprite.Flicker, temporarySprite.Flip,
                drawLayer + temporarySprite.SortOffset,
                temporarySprite.AlphaFade,
                Utility.StringToColor(temporarySprite.Color) ?? Color.White,
                temporarySprite.Scale * 4f,
                temporarySprite.ScaleChange,
                temporarySprite.Rotation,
                temporarySprite.RotationChange
            );
            Game1.Multiplayer.broadcastSprites(location, temporaryAnimatedSprite);

        }
    }
}