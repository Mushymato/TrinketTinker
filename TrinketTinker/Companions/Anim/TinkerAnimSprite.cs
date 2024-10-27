
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using TrinketTinker.Models;
using TrinketTinker.Wheels;

namespace TrinketTinker.Companions.Anim
{
    /// <summary>
    /// Simplified animated sprite controller.
    /// Not a net object and must be built independently in each game instance.
    /// Always loops.
    /// </summary>
    public sealed class TinkerAnimSprite
    {
        private readonly VariantData vd;
        /// <summary>Middle point of the sprite, based on width and height.</summary>
        internal readonly Vector2 Origin;
        /// <summary>Backing draw color field.</summary>
        private Color? drawColor = null;
        /// <summary>If draw color need to update after init.</summary>
        private bool drawColorIsConstant = false;
        /// <summary>Sprite draw color</summary>
        internal Color DrawColor
        {
            get
            {
                if (drawColor != null)
                    return (Color)drawColor;
                if (!drawColorIsConstant) // cannot keep prismatic color
                    return Visuals.GetSDVColor(vd.ColorMask, out drawColorIsConstant);
                drawColor = Visuals.GetSDVColor(vd.ColorMask, out drawColorIsConstant);
                return (Color)drawColor;
            }
        }
        internal readonly Texture2D Texture;
        internal Rectangle SourceRect { get; private set; } = Rectangle.Empty;
        private float timer = 0f;
        internal int currentFrame = 0;
        private bool isReverse = false;

        public TinkerAnimSprite(VariantData vdata)
        {
            vd = vdata;
            Origin = new Vector2(vd.Width / 2, vd.Height / 2);
            Texture = Game1.content.Load<Texture2D>(vd.Texture);
            UpdateSourceRect();
        }

        /// <summary>Get source rect corresponding to a particular frame.</summary>
        /// <param name="frame">Frame, or sprite index</param>
        /// <returns></returns>
        public Rectangle GetSourceRect(int frame)
        {
            return new Rectangle(
                frame * vd.Width % Texture.Width,
                frame * vd.Width / Texture.Width * vd.Height,
                vd.Width, vd.Height
            );
        }

        private void UpdateSourceRect()
        {
            SourceRect = GetSourceRect(currentFrame);
        }

        /// <summary>Set sprite to specific frame</summary>
        /// <param name="frame"></param>
        internal void SetCurrentFrame(int frame)
        {
            if (frame != currentFrame)
            {
                currentFrame = frame;
                isReverse = false;
                UpdateSourceRect();
            }
        }

        /// <summary>
        /// Convenience method for calling AnimateStandard or AnimatePingPong with <see cref="AnimClipData"/>
        /// </summary>
        /// <param name="time">current game time</param>
        /// <param name="clip">animation clip object</param>
        /// <param name="interval">default miliseconds between frames, if the clip did not set one</param>
        internal bool AnimateClip(GameTime time, AnimClipData clip, float interval)
        {
            return Animate(
                clip.LoopMode, time,
                clip.FrameStart, clip.FrameLength,
                clip.Interval ?? interval
            );
        }

        /// <summary>
        /// Convenience method for calling AnimateStandard or AnimatePingPong
        /// </summary>
        /// <param name="loopMode">which frame pattern to use</param>
        /// <param name="time">current game time</param>
        /// <param name="startFrame">initial frame</param>
        /// <param name="numberOfFrames">length of animation</param>
        /// <param name="interval">miliseconds between frames</param>
        /// <returns>True if animation reached last frame</returns>
        internal bool Animate(LoopMode loopMode, GameTime time, int startFrame, int numberOfFrames, float interval)
        {
            if (numberOfFrames == 1)
            {
                SetCurrentFrame(startFrame);
                return true;
            }
            return loopMode switch
            {
                LoopMode.PingPong => AnimatePingPong(time, startFrame, numberOfFrames, interval),
                LoopMode.Standard => AnimateStandard(time, startFrame, numberOfFrames, interval),
                _ => false,
            };
        }

        /// <summary>
        /// Standard looping animation, e.g. 1 2 3 4 1 2 3 4.
        /// Return true whenever animation reaches last frame.
        /// </summary>
        /// <param name="gameTime">game time object from update</param>
        /// <param name="startFrame">initial frame</param>
        /// <param name="numberOfFrames">length of animation</param>
        /// <param name="interval">miliseconds between frames</param>
        /// <returns>True if animation reached last frame</returns>
        internal bool AnimateStandard(GameTime gameTime, int startFrame, int numberOfFrames, float interval)
        {
            isReverse = false;
            if (currentFrame >= startFrame + numberOfFrames || currentFrame < startFrame)
            {
                currentFrame = startFrame + currentFrame % numberOfFrames;
            }
            timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (timer > interval)
            {
                currentFrame++;
                timer = 0f;
                if (currentFrame >= startFrame + numberOfFrames)
                {
                    currentFrame = startFrame;
                    UpdateSourceRect();
                    return true;
                }
            }
            UpdateSourceRect();
            return false;
        }

        /// <summary>
        /// Reverse the animation from last frame, e.g. 1 2 3 4 3 2 1 2 3 4.
        /// Return true when animation return to first frame.
        /// </summary>
        /// <param name="gameTime">game time object from update</param>
        /// <param name="startFrame">initial frame</param>
        /// <param name="numberOfFrames">length of animation</param>
        /// <param name="interval">miliseconds between frames</param>
        /// <returns>True if animation reached last frame</returns>
        public bool AnimatePingPong(GameTime gameTime, int startFrame, int numberOfFrames, float interval)
        {
            int lastFrame;
            int step;
            if (isReverse)
            {
                lastFrame = startFrame;
                step = -1;
                if (currentFrame < lastFrame || currentFrame > startFrame)
                    currentFrame = startFrame + Math.Abs(currentFrame) % numberOfFrames;
            }
            else
            {
                lastFrame = startFrame + numberOfFrames - 1;
                step = 1;
                if (currentFrame > lastFrame || currentFrame < startFrame)
                    currentFrame = startFrame + Math.Abs(currentFrame) % numberOfFrames;
            }

            timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (timer > interval)
            {
                currentFrame += step;
                timer = 0f;
                if (currentFrame == lastFrame)
                {
                    isReverse = !isReverse;
                    // when frame reach lastFrame and isReverse had been true, 1 cycle is completed
                    // invert isReverse again to obtain true (and false in the case where only half of the cycle is done)
                    return !isReverse;
                }
            }
            UpdateSourceRect();
            return false;
        }
    }
}