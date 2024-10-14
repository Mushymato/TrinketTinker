using Microsoft.Xna.Framework;
using StardewValley;

namespace TrinketTinker.Wheels
{
    public static class Extensions
    {
        /// <summary>
        /// Reverse the animation from last frame, e.g. 1 2 3 4 3 2 1 2 3 4.
        /// Ignores <see cref="AnimatedSprite.loop"/>.
        /// </summary>
        /// <param name="s">animated sprite</param>
        /// <param name="gameTime">game time object from update</param>
        /// <param name="startFrame">initial frame</param>
        /// <param name="numberOfFrames">length of animation</param>
        /// <param name="interval">milisecond interval between frames</param>
        /// <param name="isReverse">flag for whether animation is going forward or backwards, will be updated in this method.</param>
        public static void AnimatePingPong(this AnimatedSprite s, GameTime gameTime, int startFrame, int numberOfFrames, float interval, ref bool isReverse)
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
        }
    }
}
