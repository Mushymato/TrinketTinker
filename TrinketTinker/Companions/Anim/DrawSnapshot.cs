using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TrinketTinker.Companions.Anim
{
    internal sealed class DrawSnapshot
    {
        internal Texture2D texture = null!;
        internal Vector2 position;
        internal Rectangle sourceRect;
        internal Color drawColor;
        internal float rotation;
        internal Vector2 origin;
        internal Vector2 textureScale;
        internal SpriteEffects effects;
        internal float layerDepth;

        internal void DoDraw(SpriteBatch b)
        {
            b.Draw(
                texture,
                position,
                sourceRect,
                drawColor,
                rotation,
                origin,
                textureScale,
                effects,
                layerDepth
            );
        }
    }
}