using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Models;

/// <summary>Determine how the sprites are interpreted.</summary>
public enum DirectionMode
{
    /// <summary>Direction never changes.</summary>
    Single,

    /// <summary>Has right animations, flips sprite if going left</summary>
    R,

    /// <summary>Has right/left animations, no down/up</summary>
    RL,

    /// <summary>Has down/right/up animations, flips the right animation to go left.</summary>
    DRU,

    /// <summary>Has down/right/up/left animations</summary>
    DRUL,
}

/// <summary>Determine how sprites loop.</summary>
public enum LoopMode
{
    /// <summary>Loops to start from last frame, e.g. 1 2 3 4 1 2 3 4 1 2 3 4</summary>
    Standard,

    /// <summary>Reverse the animation from last frame, e.g. 1 2 3 4 3 2 1 2 3 4</summary>
    PingPong,
}

/// <summary>Which target to anchor (follow/attach) to</summary>
public enum AnchorTarget
{
    /// <summary>Anchor to the trinket owner</summary>
    Owner,

    /// <summary>Anchor to the nearest monster</summary>
    Monster,

    /// <summary>Anchor to the nearest farm animal</summary>
    FarmAnimal,

    /// <summary>Anchor to the nearest NPC</summary>
    NPC,

    /// <summary>Anchor to the nearest placed object</summary>
    Object,

    /// <summary>Anchor to the nearest forage object</summary>
    Forage,

    /// <summary>Anchor to the nearest breakable stone</summary>
    Stone,

    /// <summary>Anchor to the nearest breakable twig</summary>
    Twig,

    /// <summary>Anchor to the nearest breakable weed</summary>
    Weed,

    /// <summary>Anchor to the nearest dig spot ((O)590 or (O)SeedSpot)</summary>
    DigSpot,

    /// <summary>Anchor to the nearest crop</summary>
    Crop,

    /// <summary>Anchor to the nearest terrain feature</summary>
    TerrainFeature,

    /// <summary>Anchor to the nearest "shakeable" terrain feature</summary>
    Shakeable,
}

/// <summary>Determine the layer depth to use when drawing the companion</summary>
public enum LayerDepth
{
    /// <summary>Draw just behind the farmer.</summary>
    Behind,

    /// <summary>Draw according to current Y position</summary>
    Position,

    /// <summary>Draw just in front of the farmer</summary>
    InFront,
}

/// <summary>Whether to enable</summary>
public enum CollisionMode
{
    None,
    Line,
}

/// <summary>Model defining how companions pick anchor target</summary>
public sealed class AnchorTargetData
{
    /// <summary>Backing field for <see cref="Id"/>.</summary>
    private string? IdImpl = null;

    /// <summary>Id for this anchor target, defaults to Mode.</summary>
    public string Id
    {
        get => IdImpl ??= Mode.ToString();
        set => IdImpl = value;
    }

    /// <summary>Targeting mode, see <see cref="AnchorTarget"/>.</summary>
    public AnchorTarget Mode { get; set; } = AnchorTarget.Owner;

    /// <summary>Search range.</summary>
    public int Range { get; set; } = Game1.tileSize * 10;

    /// <summary>Stop moving once companion is this distance away.</summary>
    public int StopRange { get; set; } = 0;

    /// <summary>
    /// Additional filters to apply, specific behavior depends on the anchor mode.
    /// <list type="bullet">
    /// <item><see cref="AnchorTarget.Monster"/></item>
    /// </list>
    /// </summary>
    public List<string>? Filters { get; set; } = null;

    /// <summary>
    ///
    /// </summary>
    private List<string>? requiredAbilities = null;

    /// <summary>Enable this anchor only if the companion has matching ability type.</summary>
    public List<string>? RequiredAbilities
    {
        get
        {
            if (requiredAbilities != null)
                return requiredAbilities;
            return Mode switch
            {
                AnchorTarget.Monster => ["Hitscan", "Projectile"],
                AnchorTarget.FarmAnimal => ["PetFarmAnimal"],
                AnchorTarget.Forage
                or AnchorTarget.Stone
                or AnchorTarget.Twig
                or AnchorTarget.Weed
                or AnchorTarget.DigSpot
                or AnchorTarget.Crop
                or AnchorTarget.Shakeable => [$"Harvest{Mode}"],
                _ => null,
            };
        }
        set => requiredAbilities = value;
    }
}

public sealed class FrameOverrideData
{
    /// <summary>Non-standard explicitly provided source rect to use in place of normal frame logic</summary>
    public Rectangle? SourceRect { get; set; } = null;

    /// <summary>Non-standard explicitly provided origin to use in place of normal width/2 height/2</summary>
    public Vector2? Origin { get; set; } = null;

    /// <summary>Frame interval, falls back to <see cref="AnimClipData.Interval"/> and then fall back to <see cref="MotionData.Interval"/> if not set.</summary>
    public double? Interval { get; set; } = null;
}

/// <summary>Model for additional animation</summary>
public sealed class AnimClipData : WeightedRandData
{
    /// <summary>Anim clip frame start</summary>
    public int FrameStart { get; set; } = 0;

    /// <summary>Anim clip frame length</summary>
    public int FrameLength { get; set; } = 1;

    /// <summary>Anim clip loop mode</summary>
    public LoopMode LoopMode { get; set; } = LoopMode.Standard;

    /// <summary>Sprite flip to apply to this anim clip, falls back to <see cref="MotionData.Flip"/> if not set.</summary>
    public SpriteEffects? Flip { get; set; } = null;

    /// <summary>Anim clip interval, fall back to <see cref="MotionData.Interval"/> if not set.</summary>
    public double? Interval { get; set; } = null;

    /// <summary>If set, the companion won't move while clip plays.</summary>
    public bool PauseMovement { get; set; } = false;

    /// <summary>If set, use <see cref="VariantData.TextureExtra"/> instead of <see cref="VariantData.Texture"/>.</summary>
    public bool UseExtra { get; set; } = false;

    /// <summary>Non-standard frame overrides</summary>
    public List<FrameOverrideData>? FrameOverrides { get; set; } = null;

    /// <summary>Get the frame override data for a particular frame, if it exists</summary>
    internal FrameOverrideData? GetFrameOverride(int currentFrame)
    {
        if (FrameOverrides == null)
            return null;
        int idx = currentFrame - FrameStart;
        if (idx >= 0 && idx < FrameOverrides.Count)
            return FrameOverrides[idx];
        return null;
    }

    /// <summary>Additional clips that may randomly be called, only valid for the top level clip.</summary>
    public IReadOnlyList<AnimClipData>? RandomClips
    {
        get => randomExtra?.Select((clip) => (AnimClipData)clip).ToList();
        set => randomExtra = value?.Select((clip) => (WeightedRandData)clip).ToList();
    }

    internal AnimClipData Selected => randSelected == null ? this : (AnimClipData)randSelected;

    /// <summary>Choose a random clip</summary>
    /// <param name="rand"></param>
    /// <returns></returns>
    public bool TryPickRand(Random rand, Farmer owner, [NotNullWhen(true)] out AnimClipData? clip)
    {
        clip = null;
        WeightedRandData? result = PickRandBase(rand, owner);
        if (result == null)
            return false;
        clip = (AnimClipData)result;
        return true;
    }
}

public sealed class SpeechBubbleData : WeightedRandData
{
    /// <summary>Text for speech bubble</summary>
    public string Text { get; set; } = "Hey, Listen!";

    /// <summary>Timer to show speech bubble for, miliseconds</summary>
    public double Timer { get; set; } = 3000;

    /// <summary>Speech bubble draw offset</summary>
    public Vector2 Offset { get; set; } = Vector2.Zero;

    /// <summary>Text color</summary>
    public string? Color { get; set; } = null;

    /// <summary>Scroll BG type, see</summary>
    public int ScrollType { get; set; } = 1;

    /// <summary>Draw layer depth, relative to the companion's layer depth</summary>
    public float LayerDepth { get; set; } = 2E-05f;

    /// <summary>Draw using the junimo text font</summary>
    public bool JunimoText { get; set; } = false;

    /// <summary>Percent of timer to spend on fade in</summary>
    public float FadeIn { get; set; } = 0.1f;

    /// <summary>Percent of timer to spend on fade out</summary>
    public float FadeOut { get; set; } = 0.1f;

    /// <summary>Random shake to apply to the speech bubble</summary>
    public int Shake { get; set; } = 0;

    /// <summary>Additional speech that may randomly be called, only valid for the top level speech.</summary>
    public IReadOnlyList<SpeechBubbleData>? RandomSpeech
    {
        get => randomExtra?.Select((speech) => (SpeechBubbleData)speech).ToList();
        set => randomExtra = value?.Select((speech) => (WeightedRandData)speech).ToList();
    }

    internal SpeechBubbleData Selected => randSelected == null ? this : (SpeechBubbleData)randSelected;

    public bool TryPickRand(Random rand, Farmer owner, [NotNullWhen(true)] out SpeechBubbleData? speech)
    {
        speech = null;
        WeightedRandData? result = PickRandBase(rand, owner);
        if (result == null)
            return false;
        speech = (SpeechBubbleData)result;
        return true;
    }
}

public sealed class AnimClipDictionary : Dictionary<string, AnimClipData?>
{
    public const string IDLE = "Idle";
    public const string SWIM = "Swim";

    /// <summary>
    /// Obtain the anim clip for key and direction.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="direction"></param>
    /// <param name="clip"></param>
    /// <returns></returns>
    public bool TryGetDirectional(string? key, int direction, [NotNullWhen(true)] out AnimClipData? clip)
    {
        clip = null;
        if (key == null)
            return false;
        string keyDirectional = $"{key}.{MathF.Abs(direction)}";
        if (TryGetValue(keyDirectional, out clip))
        {
            return clip != null;
        }
        if (TryGetValue(key, out clip))
        {
            // short-circuit the next attempt to obtain AnimClipData
            Add(keyDirectional, clip);
            return clip != null;
        }
        // short-circuit the next attempt to obtain AnimClipData, null means this key has no anim
        Add(keyDirectional, null);
        Add(key, null);
        return false;
    }
}

/// <summary>Data for <see cref="Companions.Motions"/>, defines how a companion moves.</summary>
public sealed class MotionData : IHaveArgs
{
    /// <summary>Type name of the motion, can use short form like "Hover" for hover motion.</summary>
    public string MotionClass { get; set; } = "Lerp";

    /// <summary>Direction mode, determines how sprites should be arranged.</summary>
    public DirectionMode DirectionMode { get; set; } = DirectionMode.Single;

    /// <summary>Apply sprite rotation depending on direction.</summary>
    public bool DirectionRotate { get; set; } = false;

    /// <summary>Animation looping mode.</summary>
    public LoopMode LoopMode { get; set; } = LoopMode.Standard;

    /// <summary>
    /// Prefer <see cref="AnchorTargetData"/> that comes earlier in the list.
    /// Defaults to <see cref="AnchorTarget.Owner"/>.
    /// </summary>
    public List<AnchorTargetData>? Anchors { get; set; } = null;

    /// <summary>If true, continue the moving animation when not owner is not moving.</summary>
    public bool AlwaysMoving { get; set; } = false;

    /// <summary>Collision mode</summary>
    public CollisionMode Collision { get; set; } = CollisionMode.None;

    /// <summary>First frame of the animation.</summary>
    public int FrameStart { get; set; } = 0;

    /// <summary>Length of 1 set of movement animation.</summary>
    public int FrameLength { get; set; } = 4;

    /// <summary>Miliseconds between frames.</summary>
    public double Interval { get; set; } = 100.0;

    /// <summary>Position offset.</summary>
    public Vector2 Offset { get; set; } = Vector2.Zero;

    /// <summary>Sprite flip to apply to the directional animation</summary>
    public SpriteEffects Flip { get; set; } = SpriteEffects.None;

    /// <summary>Layer depth mode.</summary>
    public LayerDepth LayerDepth { get; set; } = LayerDepth.Position;

    /// <summary>Hide the companion during all events.</summary>
    public bool HideDuringEvents { get; set; } = false;

    /// <summary>Number of times to repeat the draw.</summary>
    public float RepeatCount { get; set; } = 0;

    /// <summary>Number of miliseconds between repeats.</summary>
    public double RepeatInterval { get; set; } = 500.0;

    /// <summary>
    /// When used with repeat, apply offset of this value times number of frames (per direction mode and frame length).<br/>
    /// Example with <see cref="RepeatFrameSets"/> = 2 <see cref="DirectionMode"/> = <see cref="DirectionMode.R"/> and <see cref="FrameLength"/> = 4.
    /// <list type="bullet">
    /// <item>The initial companion need 4 frames, taking up frames 0 to 3.</item>
    /// <item>First repeat companion need 4 frames, taking up frames 4 to 7.</item>
    /// <item>Second repeat companion need 4 frames, taking up frames 8 to 11.</item>
    /// </list>
    /// Spritesheet needs a total of 12 frames.
    /// </summary>
    public int RepeatFrameSets { get; set; } = 0;

    /// <summary>
    /// Repository of anim clips that can be shown in place of the default movement anim.
    /// Must live on the same sprite sheet specified by variant data.
    /// </summary>
    public AnimClipDictionary AnimClips { get; set; } = [];

    /// <summary>
    /// Repository of speech bubbles (overhead text for companions).
    /// Can be proc'd by ability activation.
    /// </summary>
    public Dictionary<string, SpeechBubbleData> SpeechBubbles { get; set; } = [];

    /// <inheritdocs/>
    public Dictionary<string, object>? Args { get; set; } = null;
}
