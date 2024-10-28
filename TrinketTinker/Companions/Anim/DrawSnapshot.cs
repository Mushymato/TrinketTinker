using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace TrinketTinker.Companions.Anim
{
    /// <summary>
    /// Record holding all info needed to draw with <see cref="SpriteBatch"/>.
    /// The position given is the map X,Y position, to be translated to the viewport position on draw.
    /// </summary>
    /// <param name="Texture"></param>
    /// <param name="Position"></param>
    /// <param name="SourceRect"></param>
    /// <param name="DrawColor"></param>
    /// <param name="Rotation"></param>
    /// <param name="Origin"></param>
    /// <param name="TextureScale"></param>
    /// <param name="Effects"></param>
    /// <param name="LayerDepth"></param>
    public sealed record DrawSnapshot(
        Texture2D Texture,
        Vector2 Position,
        Rectangle SourceRect,
        Color DrawColor,
        float Rotation,
        Vector2 Origin,
        Vector2 TextureScale,
        SpriteEffects Effects,
        float LayerDepth
    )
    {
        /// <summary>Make a shallow copy with changes in some fields.</summary>
        /// <param name="position"></param>
        /// <param name="sourceRect"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        internal DrawSnapshot CloneWithChanges(
            Vector2? position = null,
            Rectangle? sourceRect = null,
            float? rotation = null
        )
        {
            return new(
                Texture,
                position ?? Position,
                sourceRect ?? SourceRect,
                DrawColor,
                rotation ?? Rotation,
                Origin,
                TextureScale,
                Effects,
                LayerDepth
            );
        }

        /// <summary>Do a draw to <see cref="SpriteBatch"/></summary>
        /// <param name="b"></param>
        internal void Draw(SpriteBatch b)
        {
            b.Draw(
                Texture,
                Game1.GlobalToLocal(Position),
                SourceRect,
                DrawColor,
                Rotation,
                Origin,
                TextureScale,
                Effects,
                LayerDepth
            );
        }
    }
}