using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.GameData.Characters;
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

/// <summary>Breather information for a NPC style trinket</summary>
/// <param name="Pos"></param>
/// <param name="Rect"></param>
public sealed record BreatherPositionAndRectangle(Vector2 Pos, Rectangle Rect);

/// <summary>
/// Simplified animated sprite controller.
/// Not a net object and must be built independently in each game instance.
/// </summary>
public sealed class TinkerAnimSprite
{
    /// <summary>Selected variant data</summary>
    private IVariantData vd;

    /// <summary>full variant data</summary>
    private VariantData fullVd;

    /// <summary>full variant data</summary>
    internal VariantData FullVd => fullVd;

    /// <summary>Draw origin of the sprite.</summary>
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
    internal Texture2D Texture => UseExtra ? TextureExtra ?? TextureBase : TextureBase;
    internal Texture2D TextureBase;
    internal Texture2D? TextureExtra = null;
    private Rectangle TextureSourceRect = Rectangle.Empty;
    private Point BoundingBoxSize = Point.Zero;
    private bool useExtra = false;
    internal bool UseExtra
    {
        get => useExtra;
        set => useExtra = value;
    }
    internal int Width;
    internal int Height;
    internal float TextureScale;
    internal float ShadowScale;
    internal Rectangle Bounding;
    internal BreatherPositionAndRectangle? Breather;
    internal Rectangle SourceRect { get; private set; } = Rectangle.Empty;
    internal bool Hidden => currentFrame == -1;
    internal ChatterSpeaker Speaker =>
        new(vd.Portrait ?? fullVd.Portrait, vd.Name ?? fullVd.Name, vd.NPC ?? fullVd.NPC);
    public SpriteEffects Flip { get; internal set; }
    private AnimClipData? currentClip = null;
    internal AnimClipData? CurrentClip
    {
        get => currentClip;
        set
        {
            if (value != currentClip)
            {
                UseExtra = value?.UseExtra ?? false;
                currentClip = value;
                UpdateSourceRect();
            }
        }
    }

    private double timer = 0f;
    internal int currentFrame = 0;
    private bool isReverse = false;

    public TinkerAnimSprite(VariantData vdata)
    {
        fullVd = vdata;
        vd = fullVd;
        TextureBase = UpdateVariantFields();
        UpdateSourceRect();
    }

    public void SetFullVariant(VariantData vdata, string? altVariantKey = null)
    {
        fullVd = vdata;
        vd = fullVd;
        SetAltVariant(altVariantKey);
    }

    public void SetAltVariant(string? altVariantKey)
    {
        if (
            altVariantKey != null
            && (fullVd.AltVariants?.TryGetValue(altVariantKey, out AltVariantData? subVd) ?? false)
        )
            vd = subVd;
        else
            vd = fullVd;
        UpdateVariantFields();
        UpdateSourceRect();
    }

    /// <summary>Load the texture.</summary>
    internal static Texture2D? LoadTexture(string? texture)
    {
        return
            string.IsNullOrEmpty(texture)
            || !ModEntry.Help.GameContent.DoesAssetExist<Texture2D>(ModEntry.Help.GameContent.ParseAssetName(texture))
            ? null
            : ModEntry.Help.GameContent.Load<Texture2D>(texture);
    }

    /// <summary>Get breathing data for an npc.</summary>
    internal static BreatherPositionAndRectangle? GetBreatherPositionAndRectangle(
        int width,
        int height,
        string? npcName
    )
    {
        if (width > 16 || height > 32)
            return null;
        if (npcName == null || !Game1.characterData.TryGetValue(npcName, out CharacterData? data) || !data.Breather)
            return null;
        Rectangle breathingRect;
        if (data.BreathChestRect != null)
        {
            breathingRect = data.BreathChestRect.Value;
        }
        else
        {
            breathingRect = new Rectangle(width / 4, height / 2 + height / 32, height / 4, width / 2);
            if (data.Age == NpcAge.Child)
            {
                breathingRect.Y += height / 6 + 1;
                breathingRect.Height /= 2;
            }
            else if (data.Gender == Gender.Female)
            {
                breathingRect.Y++;
                breathingRect.Height /= 2;
            }
        }
        Vector2 breathingPos;
        if (data.BreathChestPosition != null)
        {
            breathingPos = Utility.PointToVector2(data.BreathChestPosition.Value);
        }
        else
        {
            breathingPos = new Vector2(width * 4 / 2, 8f);
            if (data.Age == NpcAge.Child)
            {
                breathingPos.Y += height / 8 * 4;
            }
            else if (data.Gender == Gender.Female)
            {
                breathingPos.Y -= 4f;
            }
        }
        breathingPos = new(breathingPos.X - 32, breathingPos.Y + height / 2);
        return new(breathingPos, breathingRect);
    }

    /// <summary>Update fields according to selected variant</summary>
    /// <returns></returns>
    private Texture2D UpdateVariantFields()
    {
        Width = vd.Width >= 0 ? vd.Width : fullVd.Width;
        Height = vd.Height >= 0 ? vd.Height : fullVd.Height;
        Bounding =
            !vd.Bounding.IsEmpty ? vd.Bounding
            : !fullVd.Bounding.IsEmpty ? fullVd.Bounding
            : new(0, 0, Width, Height);
        TextureScale = vd.TextureScale >= 0 ? vd.TextureScale : fullVd.TextureScale;
        ShadowScale = vd.ShadowScale >= 0 ? vd.ShadowScale : fullVd.ShadowScale;
        Origin = new Vector2(Width / 2, Height / 2);
        drawColor = null;
        drawColorIsConstant = false;
        TextureBase = LoadTexture(vd.Texture) ?? LoadTexture(fullVd.Texture) ?? LoadTexture("Animals/Error")!;
        TextureExtra = LoadTexture(vd.TextureExtra) ?? LoadTexture(fullVd.TextureExtra) ?? null;
        TextureSourceRect =
            !vd.TextureSourceRect.IsEmpty ? vd.TextureSourceRect
            : !fullVd.TextureSourceRect.IsEmpty ? fullVd.TextureSourceRect
            : TextureBase.Bounds;
        Breather =
            (vd.ShowBreathing ?? fullVd.ShowBreathing ?? false)
                ? GetBreatherPositionAndRectangle(Width, Height, vd.NPC ?? fullVd.NPC)
                : null;
        return TextureBase;
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
                (int)(drawPos.X - Origin.X * drawScale.X + Bounding.X * drawScale.X),
                (int)(drawPos.Y - Origin.Y * drawScale.Y + Bounding.Y * drawScale.Y),
                (int)(Bounding.Width * drawScale.X),
                (int)(Bounding.Height * drawScale.Y)
            );
        if (shadowScale.X <= 0 && shadowScale.Y <= 0)
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

    /// <summary>
    // Move source rect to current frame.
    // If a frame override is present, potentially pick that over the default source rect.
    // Update origin accordingly
    // </summary>
    private void UpdateSourceRect()
    {
        if (CurrentClip?.GetFrameOverride(currentFrame) is FrameOverrideData overrideData)
        {
            SourceRect = overrideData.SourceRect ?? GetSourceRect(currentFrame);
            Origin = overrideData.Origin ?? new Vector2(SourceRect.Width / 2, SourceRect.Height / 2);
            return;
        }
        SourceRect = GetSourceRect(currentFrame);
        Origin = new Vector2(SourceRect.Width / 2, SourceRect.Height / 2);
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
    internal TinkerAnimState AnimateClip(GameTime time, AnimClipData clip, double interval, SpriteEffects flip)
    {
        if (clip.Nop)
        {
            CurrentClip = null;
            timer += time.ElapsedGameTime.TotalMilliseconds;
            if (clip.FrameLength * interval <= timer)
            {
                timer = 0f;
                return TinkerAnimState.Complete;
            }
            return TinkerAnimState.InNop;
        }
        TinkerAnimState result = Animate(
            clip.LoopMode,
            time,
            clip.FrameStart,
            clip.FrameLength,
            interval,
            clip,
            clip.Flip ?? flip
        );
        return result;
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
        double interval,
        AnimClipData? clip = null,
        SpriteEffects spriteEffects = SpriteEffects.None
    )
    {
        CurrentClip = clip;
        Flip = spriteEffects;
        if (numberOfFrames == 0)
        {
            SetCurrentFrame(-1);
            return TinkerAnimState.Complete;
        }
        interval = clip?.Interval ?? interval;
        if (clip?.GetFrameOverride(currentFrame) is FrameOverrideData overrideData)
            interval = overrideData.Interval ?? interval;
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
