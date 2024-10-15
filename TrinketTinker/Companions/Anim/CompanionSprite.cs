
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
    public sealed class CompanionSprite
    {
        private readonly VariantData vd;
        /// <summary>Middle point of the sprite, based on width and height.</summary>
        internal readonly Vector2 Origin;
        /// <summary>Backing sprite color field.</summary>
        private Color? drawColor = null;
        private bool drawColorIsConstant = false;
        internal Color DrawColor
        {
            get
            {
                if (drawColor != null)
                    return (Color)drawColor;
                if (!drawColorIsConstant) // cannot keep prismatic color
                    return GetSDVColor(vd.ColorMask, out drawColorIsConstant, defaultColor: Color.White);
                drawColor = GetSDVColor(vd.ColorMask, out drawColorIsConstant, defaultColor: Color.White);
                return (Color)drawColor;
            }
        }
        internal readonly Texture2D Texture;
        internal Rectangle SourceRect { get; private set; } = Rectangle.Empty;
        private float timer = 0f;
        private int currentFrame = 0;


        /// <summary>Get a monogame color from string. Supports <see cref="TinkerConst.COLOR_PRISMATIC"/> for animated color.</summary>
        /// <param name="color">Color string</param>
        /// <param name="isConstant">Indicates that this is not animated, no need to update.</param>
        /// <param name="defaultColor">Fallback color, if not set the fallback is <see cref="Color.White"/>.</param>
        /// <returns></returns>
        public static Color GetSDVColor(string? colorStr, out bool isConstant, Color? defaultColor = null)
        {
            isConstant = false;
            if (colorStr == TinkerConst.COLOR_PRISMATIC)
                return Utility.GetPrismaticColor();
            isConstant = true;
            return Utility.StringToColor(colorStr) ?? defaultColor ?? Color.White;
        }

        /// <summary>Calculate the source rectangle for a sprite in an NPC spritesheet.</summary>
        /// <param name="textureWidth">The pixel width of the full spritesheet texture.</param>
        /// <param name="spriteWidth">The pixel width of each sprite.</param>
        /// <param name="spriteHeight">The pixel height of each sprite.</param>
        /// <param name="frame">The frame index, starting at 0 for the top-left corner.</param>
        public static Rectangle GetSourceRect(int textureWidth, int spriteWidth, int spriteHeight, int frame)
        {
            return new Rectangle(frame * spriteWidth % textureWidth, frame * spriteWidth / textureWidth * spriteHeight, spriteWidth, spriteHeight);
        }

        public CompanionSprite(VariantData vdata)
        {
            vd = vdata;
            Origin = new Vector2(vd.Width / 2, vd.Height / 2);
            Texture = Game1.content.Load<Texture2D>(vd.Texture);
            UpdateSourceRect();
        }

        private void UpdateSourceRect()
        {
            int s_w = vd.Width;
            int s_h = vd.Height;
            int t_w = Texture.Width;
            // int t_h = Texture.Height;
            SourceRect = GetSourceRect(t_w, s_w, s_h, currentFrame);
            // if (SourceRect.Right > t_w || SourceRect.Bottom > t_h)
            // {
            //     currentFrame = 0;
            //     SourceRect = GetSourceRect(t_w, s_w, s_h, currentFrame);
            // }
        }

        internal void SetCurrentFrame(int frame)
        {
            if (frame != currentFrame)
            {
                currentFrame = frame;
                UpdateSourceRect();
            }
        }

        internal void AnimateStandard(GameTime gameTime, int startFrame, int numberOfFrames, float interval)
        {
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
                }
            }
            UpdateSourceRect();
        }

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
        public void AnimatePingPong(GameTime gameTime, int startFrame, int numberOfFrames, float interval, ref bool isReverse)
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

            if (currentFrame >= startFrame + numberOfFrames || currentFrame < startFrame)
                currentFrame = startFrame + currentFrame % numberOfFrames;

            timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (timer > interval)
            {
                currentFrame += step;
                timer = 0f;
                if (currentFrame == lastFrame)
                {
                    isReverse = !isReverse;
                }
            }
            UpdateSourceRect();
        }
    }
}