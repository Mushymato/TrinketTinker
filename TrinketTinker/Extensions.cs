using Microsoft.Xna.Framework;
using StardewValley;

namespace TrinketTinker
{
    internal static class Extensions
    {
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
