using Microsoft.Xna.Framework;
using StardewValley;

namespace TrinketTinker
{
    internal static class Extensions
    {
        /// <summary>
        /// Reverse the animation from last frame, e.g. 1 2 3 4 3 2 1 2 3 4.
        /// Ignores <see cref="AnimatedSprite.loop"/>.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="gameTime"></param>
        /// <param name="startFrame"></param>
        /// <param name="numberOfFrames"></param>
        /// <param name="interval"></param>
        /// <param name="isReverse">True if animation is going backwards</param>
        /// <returns>Updated isReverse flag</returns>
        public static bool AnimatePingPong(this AnimatedSprite s, GameTime gameTime, int startFrame, int numberOfFrames, float interval, bool isReverse)
        {
            int lastFrame;
            int step;
            if (isReverse)
            {
                lastFrame = startFrame;
                step = -1;
            }
            else
            {
                lastFrame = startFrame + numberOfFrames - 1;
                step = 1;
            }

            if (s.currentFrame >= startFrame + numberOfFrames || s.currentFrame < startFrame)
                s.currentFrame = startFrame + s.currentFrame % numberOfFrames;

            s.timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (s.timer > interval)
            {
                s.currentFrame += step;
                s.timer = 0f;
                if (s.currentFrame == lastFrame)
                {
                    isReverse = !isReverse;
                }
            }
            s.UpdateSourceRect();
            return isReverse;
        }
    }
}
