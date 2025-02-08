using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;
using TrinketTinker.Wheels;

namespace TrinketTinker.Companions.Anim;

[Flags]
public enum TinkerAnimState
{
    None = 1 << 0,
    InProgress = 1 << 1,
    Complete = 1 << 2,
    InNop = 1 << 3,
    Playing = InProgress | Complete,
}

/// <summary>
/// Simplified animated sprite controller.
/// Not a net object and must be built independently in each game instance.
/// </summary>
public sealed class TinkerAnimSprite
{
    /// <summary>Selected variant data</summary>
    private IVariantData vd;

    /// <summary>full variant data</summary>
    private readonly VariantData fullVd;

    /// <summary>Middle point of the sprite, based on width and height.</summary>
    internal Vector2 Origin;

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
            if (!drawColorIsConstant) // must update draw color every call
                return Visuals.GetSDVColor(vd.ColorMask, out drawColorIsConstant);
            drawColor = Visuals.GetSDVColor(vd.ColorMask, out drawColorIsConstant);
            return (Color)drawColor;
        }
    }

    /// <summary>Current texture</summary>
    internal Texture2D Texture;
    internal int Width;
    internal int Height;
    internal float TextureScale;
    internal float ShadowScale;
    internal Rectangle SourceRect { get; private set; } = Rectangle.Empty;
    internal bool Hidden => currentFrame == -1;
    internal ChatterSpeaker Speaker =>
        new(vd.Portrait ?? fullVd.Portrait, vd.Name ?? fullVd.Name, vd.NPC ?? fullVd.NPC);

    private double timer = 0f;
    internal int currentFrame = 0;
    private bool isReverse = false;

    public TinkerAnimSprite(VariantData vdata)
    {
        fullVd = vdata;
        vd = fullVd;
        Texture = UpdateVariantFields();
        UpdateSourceRect();
    }

    public void SetAltVariant(string? altVariantKey)
    {
        if (altVariantKey == null)
        {
            vd = fullVd;
        }
        else if (fullVd.AltVariants?.TryGetValue(altVariantKey, out AltVariantData? subVd) ?? false)
        {
            vd = subVd;
        }
        else
        {
            vd = fullVd;
            return;
        }
        Texture = UpdateVariantFields();
        UpdateSourceRect();
    }

    /// <summary>Load the texture.</summary>
    internal static Texture2D? LoadTexture(string? texture)
    {
        return string.IsNullOrEmpty(texture) ? null : Game1.content.Load<Texture2D>(texture);
    }

    /// <summary>Update fields according to selected variant</summary>
    /// <returns></returns>
    private Texture2D UpdateVariantFields()
    {
        Width = vd.Width >= 0 ? vd.Width : fullVd.Width;
        Height = vd.Height >= 0 ? vd.Height : fullVd.Height;
        TextureScale = vd.TextureScale >= 0 ? vd.TextureScale : fullVd.TextureScale;
        ShadowScale = vd.ShadowScale >= 0 ? vd.ShadowScale : fullVd.ShadowScale;
        Origin = new Vector2(Width / 2, Height / 2);
        drawColor = null;
        drawColorIsConstant = false;
        return LoadTexture(vd.Texture) ?? LoadTexture(fullVd.Texture) ?? LoadTexture("Animals/Error")!;
    }

    /// <summary>Get source rect corresponding to a particular frame.</summary>
    /// <param name="frame">Frame, or sprite index</param>
    /// <returns></returns>
    public Rectangle GetSourceRect(int frame)
    {
        return new Rectangle(frame * Width % Texture.Width, frame * Width / Texture.Width * Height, Width, Height);
    }

    /// <summary>
    /// Bounding box of the sprite, calculated at draw time because thats the most convienant way.
    /// Extends down to their shadow if they have one.
    /// </summary>
    /// <param name="drawPos"></param>
    /// <param name="drawScale"></param>
    /// <param name="shadowDrawPos"></param>
    /// <param name="shadowScale"></param>
    /// <returns></returns>
    public Rectangle GetBoundingBox(Vector2 drawPos, Vector2 drawScale, Vector2 shadowDrawPos, Vector2 shadowScale)
    {
        Rectangle textureBox =
            new(
                (int)(drawPos.X - Origin.X * drawScale.X),
                (int)(drawPos.Y - Origin.Y * drawScale.Y),
                (int)(Width * drawScale.X),
                (int)(Height * drawScale.Y)
            );
        if (shadowDrawPos == Vector2.Zero)
            return textureBox;
        Rectangle shadowBox =
            new(
                (int)(shadowDrawPos.X - Game1.shadowTexture.Bounds.Center.X * shadowScale.X),
                (int)(shadowDrawPos.Y - Game1.shadowTexture.Bounds.Center.Y * shadowScale.Y),
                (int)(Game1.shadowTexture.Width * shadowScale.X),
                (int)(Game1.shadowTexture.Height * shadowScale.Y)
            );
        return Rectangle.Union(textureBox, shadowBox);
    }

    /// <summary>Move source rect to current frame</summary>
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
            if (currentFrame > -1)
                UpdateSourceRect();
        }
    }

    /// <summary>
    /// Convenience method for calling AnimateStandard or AnimatePingPong with <see cref="AnimClipData"/>
    /// </summary>
    /// <param name="time">current game time</param>
    /// <param name="clip">animation clip object</param>
    /// <param name="interval">default miliseconds between frames, if the clip did not set one</param>
    internal TinkerAnimState AnimateClip(GameTime time, AnimClipData clip, double interval)
    {
        interval = clip.Interval ?? interval;
        if (clip.Nop)
        {
            timer += time.ElapsedGameTime.TotalMilliseconds;
            if (clip.FrameLength * interval <= timer)
            {
                timer = 0f;
                return TinkerAnimState.Complete;
            }
            return TinkerAnimState.InNop;
        }
        return Animate(clip.LoopMode, time, clip.FrameStart, clip.FrameLength, interval);
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
    internal TinkerAnimState Animate(
        LoopMode loopMode,
        GameTime time,
        int startFrame,
        int numberOfFrames,
        double interval
    )
    {
        if (numberOfFrames == 0)
        {
            SetCurrentFrame(-1);
            return TinkerAnimState.Complete;
        }
        return loopMode switch
        {
            LoopMode.PingPong => AnimatePingPong(time, startFrame, numberOfFrames, interval),
            LoopMode.Standard => AnimateStandard(time, startFrame, numberOfFrames, interval),
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Standard looping animation, e.g. 1 2 3 4 1 2 3 4.
    /// Return true whenever animation reaches last frame.
    /// </summary>
    /// <param name="time">game time object from update</param>
    /// <param name="startFrame">initial frame</param>
    /// <param name="numberOfFrames">length of animation</param>
    /// <param name="interval">miliseconds between frames</param>
    /// <returns>True if animation reached last frame</returns>
    internal TinkerAnimState AnimateStandard(GameTime time, int startFrame, int numberOfFrames, double interval)
    {
        if (currentFrame >= startFrame + numberOfFrames || currentFrame < startFrame)
            currentFrame = startFrame + currentFrame % numberOfFrames;
        timer += time.ElapsedGameTime.TotalMilliseconds;
        if (timer > interval)
        {
            currentFrame++;
            timer = 0f;
            if (currentFrame >= startFrame + numberOfFrames)
            {
                currentFrame = startFrame;
                UpdateSourceRect();
                return TinkerAnimState.Complete;
            }
        }
        UpdateSourceRect();
        return TinkerAnimState.InProgress;
    }

    /// <summary>
    /// Reverse the animation from last frame, e.g. 1 2 3 4 3 2 1 2 3 4.
    /// Return true when animation return to first frame.
    /// </summary>
    /// <param name="time">game time object from update</param>
    /// <param name="startFrame">initial frame</param>
    /// <param name="numberOfFrames">length of animation</param>
    /// <param name="interval">miliseconds between frames</param>
    /// <returns>True if animation reached last frame</returns>
    public TinkerAnimState AnimatePingPong(GameTime time, int startFrame, int numberOfFrames, double interval)
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

        timer += time.ElapsedGameTime.TotalMilliseconds;
        if (timer > interval)
        {
            currentFrame += step;
            timer = 0f;
            if (currentFrame == lastFrame)
            {
                UpdateSourceRect();
                isReverse = !isReverse;
                return isReverse ? TinkerAnimState.InProgress : TinkerAnimState.Complete;
            }
        }
        UpdateSourceRect();
        return TinkerAnimState.InProgress;
    }
}
