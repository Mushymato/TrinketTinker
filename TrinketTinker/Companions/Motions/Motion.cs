using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using TrinketTinker.Companions.Anim;
using TrinketTinker.Models;
using TrinketTinker.Models.Mixin;
using TrinketTinker.Wheels;

namespace TrinketTinker.Companions.Motions;

/// <summary>Abstract class, controls drawing and movement of companion</summary>
public abstract class Motion<TArgs> : IMotion
    where TArgs : IArgs
{
    /// <summary>Companion that owns this motion.</summary>
    protected readonly TrinketTinkerCompanion c;

    /// <summary>Data for this motion.</summary>
    protected readonly MotionData md;

    /// <summary>Data for this motion.</summary>
    protected readonly VariantData vd;

    /// <summary>Light source ID, generated if LightRadius is set in <see cref="MotionData"/>.</summary>
    protected string lightId = "";

    /// <summary>Class dependent arguments for subclasses</summary>
    protected readonly TArgs args = default!;

    /// <summary>The previous anchor target</summary>
    protected AnchorTarget prevAnchorTarget = AnchorTarget.Owner;

    /// <summary>The current anchor target</summary>
    protected AnchorTarget currAnchorTarget = AnchorTarget.Owner;

    /// <summary>Anchor changed during this tick</summary>s
    protected bool AnchorChanged => prevAnchorTarget != currAnchorTarget;

    /// <summary>Companion animation controller</summary>
    protected readonly TinkerAnimSprite cs;

    /// <summary>Oneshot anim clip key</summary>
    private string? oneshotClipKey = null;

    /// <summary>Override anim clip key</summary>
    private string? overrideClipKey = null;

    /// <summary>The current clip key</summary>
    private string? currentClipKey = null;

    /// <summary>An anim clip that pauses movement is currently playing.</summary>
    public bool PauseMovementByAnimClip = false;

    /// <summary>Speech bubble data</summary>
    private SpeechBubbleData? speechBubble = null;

    /// <summary>Speech bubble timer</summary>
    private double speechBubbleTimer = 0;

    /// <summary>Heap of frames to draw, after the initial one.</summary>
    private readonly PriorityQueue<DrawSnapshot, long> drawSnapshotQueue = new();

    /// <summary>Number of frames in 1 set, used for Repeat and <see cref="SerpentMotion"/></summary>
    protected readonly int framesetLength = 1;

    /// <summary>Actual total frame used for Repeat, equal to frame length</summary>
    protected virtual int TotalFrames => framesetLength;

    /// <summary>Clip random</summary>
    public Random ClipRand { get; set; } = Random.Shared;

    /// <summary>Speech random</summary>
    public Random SpeechRand { get; set; } = Random.Shared;

    /// <summary>Basic constructor, tries to parse arguments as the generic <see cref="IArgs"/> type.</summary>
    /// <param name="companion"></param>
    /// <param name="mdata"></param>
    public Motion(TrinketTinkerCompanion companion, MotionData mdata, VariantData vdata)
    {
        if (typeof(TArgs) != typeof(NoArgs))
        {
            if (mdata.ParseArgs<TArgs>() is TArgs parsed && parsed.Validate())
                args = parsed;
            else
                args = (TArgs)Activator.CreateInstance(typeof(TArgs))!;
        }
        c = companion;
        md = mdata;
        vd = vdata;
        cs = new TinkerAnimSprite(vdata);

        framesetLength = md.DirectionMode switch
        {
            DirectionMode.Single => md.FrameLength,
            DirectionMode.R => md.FrameLength,
            DirectionMode.RL => md.FrameLength * 2,
            DirectionMode.DRU => md.FrameLength * 3,
            DirectionMode.DRUL => md.FrameLength * 4,
            _ => throw new NotImplementedException(),
        };

        PauseMovementByAnimClip = false;
    }

    /// <inheritdoc/>
    public void SetOneshotClip(string? clipKey)
    {
        oneshotClipKey = clipKey;
        if (clipKey == null)
            PauseMovementByAnimClip = false;
    }

    /// <inheritdoc/>
    public void SetOverrideClip(string? clipKey)
    {
        overrideClipKey = clipKey;
        if (clipKey == null)
            PauseMovementByAnimClip = false;
    }

    /// <inheritdoc/>
    public void SetSpeechBubble(string? speechBubbleKey)
    {
        if (speechBubbleTimer > 0)
            return;
        if (speechBubbleKey == null || !md.SpeechBubbles.TryGetValue(speechBubbleKey, out speechBubble))
            speechBubble = null;
        if (speechBubble != null)
        {
            speechBubble = speechBubble.PickRand(SpeechRand);
            if (!speechBubble.Nop)
            {
                speechBubbleTimer = speechBubble.Timer;
                return;
            }
        }
        speechBubble = null;
        speechBubbleTimer = 0;
    }

    /// <inheritdoc/>
    public virtual void Initialize(Farmer farmer)
    {
        if (vd.LightSource is LightSourceData ldata)
        {
            lightId = $"{farmer.UniqueMultiplayerID}/{c.ID}";
            if (!Game1.currentLightSources.ContainsKey(lightId))
                Game1.currentLightSources.Add(lightId, new TinkerLightSource(lightId, c.Position + GetOffset(), ldata));
        }
    }

    /// <inheritdoc/>
    public virtual void Cleanup()
    {
        PauseMovementByAnimClip = false;
        drawSnapshotQueue.Clear();
        if (vd.LightSource != null)
            Utility.removeLightSource(lightId);
    }

    /// <inheritdoc/>
    public virtual void OnOwnerWarp()
    {
        PauseMovementByAnimClip = false;
        drawSnapshotQueue.Clear();
        if (vd.LightSource is LightSourceData ldata)
        {
            Game1.currentLightSources.Add(lightId, new TinkerLightSource(lightId, c.Position + GetOffset(), ldata));
        }
    }

    /// <summary>Changes the position of the anchor that the companion moves relative to, based on <see cref="MotionData.Anchors"/>.</summary>
    /// <param name="time"></param>
    /// <param name="location"></param>
    public virtual void UpdateAnchor(GameTime time, GameLocation location)
    {
        prevAnchorTarget = currAnchorTarget;
        var originPoint = c.Owner.getStandingPosition();
        foreach (AnchorTargetData anchor in md.Anchors)
        {
            Func<SObject, bool>? objMatch = null;
            Func<TerrainFeature, bool>? terrainMatch = null;
            switch (anchor.Mode)
            {
                case AnchorTarget.Monster:
                    {
                        Monster closest = Utility.findClosestMonsterWithinRange(
                            location,
                            originPoint,
                            anchor.Range,
                            ignoreUntargetables: true,
                            match: anchor.Filters != null ? (m) => !anchor.Filters.Contains(m.Name) : null
                        );
                        if (closest != null)
                        {
                            currAnchorTarget = AnchorTarget.Monster;
                            c.Anchor = Utility.PointToVector2(closest.GetBoundingBox().Center);
                            return;
                        }
                    }
                    break;
                case AnchorTarget.Forage:
                    objMatch = (obj) => obj.isForage();
                    goto case AnchorTarget.Object;
                case AnchorTarget.Stone:
                    objMatch = (obj) => obj.IsBreakableStone();
                    goto case AnchorTarget.Object;
                case AnchorTarget.Object:
                    {
                        if (
                            Places.ClosestMatchingObject(location, originPoint, anchor.Range, objMatch)
                            is SObject closest
                        )
                        {
                            currAnchorTarget = anchor.Mode;
                            c.Anchor = Utility.PointToVector2(closest.GetBoundingBox().Center) - Vector2.One;
                            return;
                        }
                    }
                    break;
                case AnchorTarget.Crop:
                    terrainMatch = (terrain) => terrain is HoeDirt dirt && dirt.crop != null && dirt.crop.CanHarvest();
                    goto case AnchorTarget.TerrainFeature;
                case AnchorTarget.TerrainFeature:
                    {
                        if (
                            Places.ClosestMatchingTerrainFeature(location, originPoint, anchor.Range, terrainMatch)
                            is TerrainFeature closest
                        )
                        {
                            currAnchorTarget = anchor.Mode;
                            c.Anchor =
                                closest.Tile * Game1.tileSize
                                + new Vector2(Game1.tileSize / 2, Game1.tileSize / 2)
                                - Vector2.One;
                            return;
                        }
                    }
                    break;
                case AnchorTarget.Owner:
                    currAnchorTarget = AnchorTarget.Owner;
                    c.Anchor = Utility.PointToVector2(c.Owner.GetBoundingBox().Center);
                    return;
            }
        }
        // base case is Owner
        currAnchorTarget = AnchorTarget.Owner;
        c.Anchor = Utility.PointToVector2(c.Owner.GetBoundingBox().Center);
        return;
    }

    /// <inheritdoc/>
    public abstract void UpdateLocal(GameTime time, GameLocation location);

    /// <summary>Helper, animate a particular clip</summary>
    /// <param name="time"></param>
    /// <param name="key"></param>
    /// <returns>
    /// 0: clip not found
    /// 1: clip is animating
    /// 1: clip reached last frame
    /// </returns>
    private int AnimateClip(GameTime time, string? key, out AnimClipData? clip)
    {
        clip = null;
        if (key == null)
            return 0;
        int direction = c.direction.Value;
        if (key == HoverMotion.PERCHING)
            direction = StaticMotion.GetDirectionFromOwner(md, c.Owner.FacingDirection);
        if (!md.AnimClips.TryGetDirectional(key, direction, out clip))
        {
            if (key != null)
                currentClipKey = null;
            return 0;
        }
        if (currentClipKey != key)
        {
            currentClipKey = key;
            clip = clip.PickRand(ClipRand);
        }
        else
        {
            clip = clip.Selected;
        }
        if (clip.Nop)
        {
            currentClipKey = null;
            return 0;
        }
        if (cs.AnimateClip(time, clip, md.Interval))
        {
            currentClipKey = null;
            return 2;
        }
        return 1;
    }

    /// <summary>Helper, animate a particular clip, discard selected clip</summary>
    /// <param name="time"></param>
    /// <param name="key"></param>
    /// <returns>
    /// 0: clip not found
    /// 1: clip is animating
    /// 1: clip reached last frame
    /// </returns>
    private int AnimateClip(GameTime time, string? key)
    {
        return AnimateClip(time, key, out _);
    }

    /// <inheritdoc/>
    public virtual void UpdateGlobal(GameTime time, GameLocation location)
    {
        // Speech Bubble timer
        if (speechBubbleTimer > 0)
        {
            speechBubbleTimer -= time.ElapsedGameTime.TotalMilliseconds;
            if (speechBubbleTimer <= 0)
            {
                c.SetSpeechBubble(null);
            }
        }

        // Try each kind of anim in order, stop whenever one kind succeeds

        // Oneshot Clip: play once and unset.
        if (AnimateClip(time, oneshotClipKey, out AnimClipData? clip) is int res && res != 0)
        {
            if (res == 2)
            {
                PauseMovementByAnimClip = false;
                c.OneshotKey = null;
            }
            else
            {
                PauseMovementByAnimClip = clip!.PauseMovement;
            }
            return;
        }
        // Override Clip: play until override is unset externally
        if (overrideClipKey != null && AnimateClip(time, overrideClipKey) != 0)
        {
            PauseMovementByAnimClip = false;
            return;
        }
        // Swiming: play while player is in the water,
        if (c.Owner.swimming.Value && AnimateClip(time, AnimClipDictionary.SWIM) != 0)
        {
            return;
        }
        // Moving: play while player is moving, or if always moving is true
        if (IsMoving())
        {
            // first, try anchor target based clip
            if (currAnchorTarget == AnchorTarget.Owner || AnimateClip(time, $"Anchor.{currAnchorTarget}") == 0)
            {
                // then play the default directional clip
                cs.Animate(md.LoopMode, time, DirectionFrameStart(), md.FrameLength, md.Interval);
            }
            cs.Animate(md.LoopMode, time, DirectionFrameStart(), md.FrameLength, md.Interval);
            return;
        }
        // Idle: play while player is not moving
        if (AnimateClip(time, AnimClipDictionary.IDLE) != 0)
        {
            return;
        }
        if (md.FrameLength > 0)
            // Default: Use first frame of the current direction as fallback
            cs.SetCurrentFrame(DirectionFrameStart());
        else
            cs.SetCurrentFrame(-1);
    }

    /// <summary>Moving flag used for basis of anim</summary>
    /// <returns></returns>
    protected virtual bool IsMoving()
    {
        return md.AlwaysMoving || c.OwnerMoving;
    }

    protected virtual bool ShouldMove()
    {
        return !PauseMovementByAnimClip;
    }

    /// <inheritdoc/>
    public virtual void UpdateLightSource(GameTime time, GameLocation location)
    {
        // doesn't work in multiplayer right now
        if (vd.LightSource != null && location.Equals(Game1.currentLocation))
            Utility.repositionLightSource(lightId, c.Position + GetOffset());
    }

    /// <summary>Get layer depth based on position</summary>
    /// <returns></returns>
    protected virtual float GetPositionalLayerDepth(Vector2 offset)
    {
        return c.Position.Y / 10000f;
    }

    /// <summary>Get sprite rotation</summary>
    /// <returns>Rotation in radians</returns>
    protected virtual float GetRotation()
    {
        return 0f;
    }

    /// <summary>Get texture draw scale.</summary>
    /// <returns></returns>
    protected virtual Vector2 GetTextureScale()
    {
        return new(vd.TextureScale, vd.TextureScale);
    }

    /// <summary>Get shadow draw scale.</summary>
    /// <returns></returns>
    protected virtual Vector2 GetShadowScale()
    {
        return new(vd.ShadowScale, vd.ShadowScale);
    }

    /// <summary>Get offset</summary>
    /// <returns></returns>
    public virtual Vector2 GetOffset()
    {
        return md.Offset;
    }

    /// <summary>Get shadow offset, default same as offset but with no Y</summary>
    /// <returns></returns>
    public virtual Vector2 GetShadowOffset(Vector2 offset)
    {
        return new Vector2(offset.X, 0);
    }

    /// <inheritdoc/>
    public void Draw(SpriteBatch b)
    {
        if (Game1.eventUp && (md.HideDuringEvents && Game1.eventUp || Game1.CurrentEvent.id == "MovieTheaterScreening"))
            return;

        if (cs.Hidden)
            return;

        while (
            drawSnapshotQueue.TryPeek(out DrawSnapshot? _, out long priority)
            && Game1.currentGameTime.TotalGameTime.Ticks >= priority
        )
        {
            drawSnapshotQueue.Dequeue().Draw(b);
        }

        Vector2 offset = GetOffset();
        float layerDepth = md.LayerDepth switch
        {
            LayerDepth.Behind => c.Owner.getDrawLayer() - 2 * 2E-06f,
            LayerDepth.InFront => c.Owner.getDrawLayer() + 2 * 2E-06f,
            _ => GetPositionalLayerDepth(offset),
        };

        Vector2 drawPos = c.Position + c.Owner.drawOffset + offset;
        DrawSnapshot snapshot =
            new(
                cs.Texture,
                drawPos,
                cs.SourceRect,
                cs.DrawColor,
                GetRotation(),
                cs.Origin,
                GetTextureScale(),
                (c.direction.Value < 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                layerDepth
            );
        DrawCompanion(b, snapshot);

        Vector2 shadowScale = GetShadowScale();
        if (shadowScale.X > 0 || shadowScale.Y > 0)
        {
            Vector2 shadowOffset = GetShadowOffset(offset);
            DrawSnapshot shadowSnapshot =
                new(
                    Game1.shadowTexture,
                    c.Position + c.Owner.drawOffset + shadowOffset,
                    Game1.shadowTexture.Bounds,
                    Color.White,
                    0f,
                    new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y),
                    shadowScale,
                    SpriteEffects.None,
                    layerDepth - 2E-06f
                );
            DrawShadow(b, shadowSnapshot);
        }

        // speech bubble
        if (speechBubble != null)
        {
            Vector2 worldDrawPos =
                Game1.GlobalToLocal(new(drawPos.X, drawPos.Y - vd.Height * 4 - Game1.tileSize)) + speechBubble.Offset;
            float alpha = 1;
            double fadeInThreshold = speechBubble.Timer * (1 - speechBubble.FadeIn);
            double fadeOutThreshold = speechBubble.Timer * speechBubble.FadeOut;
            if (speechBubbleTimer >= fadeInThreshold)
                alpha =
                    1f - (float)((speechBubbleTimer - fadeInThreshold) / (speechBubble.FadeIn * speechBubble.Timer));
            else if (speechBubbleTimer <= fadeOutThreshold)
                alpha =
                    1f - (float)((fadeOutThreshold - speechBubbleTimer) / (speechBubble.FadeOut * speechBubble.Timer));
            SpriteText.drawStringWithScrollCenteredAt(
                b,
                TokenParser.ParseText(speechBubble.Text),
                (int)worldDrawPos.X + Game1.random.Next(-1 * speechBubble.Shake, 2 * speechBubble.Shake),
                (int)worldDrawPos.Y + Game1.random.Next(-1 * speechBubble.Shake, 2 * speechBubble.Shake),
                "",
                alpha,
                Visuals.GetSDVColor(speechBubble.Color, out bool _),
                speechBubble.ScrollType,
                layerDepth + speechBubble.LayerDepth,
                speechBubble.JunimoText
            );
        }
    }

    /// <summary>Draw companion from snapshot, enqueue repeat as required</summary>
    /// <param name="b"></param>
    /// <param name="snapshot"></param>
    protected virtual void DrawCompanion(SpriteBatch b, DrawSnapshot snapshot)
    {
        snapshot.Draw(b);
        EnqueueRepeatDraws(snapshot, false);
    }

    /// <summary>Draw shadow from snapshot, enqueue repeat as required</summary>
    /// <param name="b"></param>
    /// <param name="snapshot"></param>
    protected virtual void DrawShadow(SpriteBatch b, DrawSnapshot snapshot)
    {
        snapshot.Draw(b);
        EnqueueRepeatDraws(snapshot, true);
    }

    /// <summary>Queue up repeats of the current draw.</summary>
    /// <param name="snapshot"></param>
    protected void EnqueueRepeatDraws(DrawSnapshot snapshot, bool isShadow)
    {
        int totalFrameSets = md.RepeatFrameSets + 1;
        // repeat for the base frame set
        for (int repeat = 1; repeat <= md.RepeatCount; repeat++)
        {
            drawSnapshotQueue.Enqueue(
                snapshot,
                Game1.currentGameTime.TotalGameTime.Ticks
                    + TimeSpan.FromMilliseconds(md.RepeatInterval * totalFrameSets * repeat).Ticks
            );
        }

        // repeat for additional frame sets
        DrawSnapshot framesetSnapshot;
        for (int repeat = 0; repeat <= md.RepeatCount; repeat++)
        {
            for (int frameset = 1; frameset < totalFrameSets; frameset++)
            {
                if (isShadow)
                {
                    framesetSnapshot = snapshot;
                }
                else
                {
                    framesetSnapshot = snapshot.CloneWithChanges(
                        sourceRect: cs.GetSourceRect(cs.currentFrame + frameset * TotalFrames)
                    );
                }
                drawSnapshotQueue.Enqueue(
                    framesetSnapshot,
                    Game1.currentGameTime.TotalGameTime.Ticks
                        + TimeSpan.FromMilliseconds(md.RepeatInterval * (totalFrameSets * repeat + frameset)).Ticks
                );
            }
        }
    }

    /// <summary>Update companion facing direction using a direction.</summary>
    /// <param name="position"></param>
    protected virtual void UpdateDirection()
    {
        Vector2 position = c.Position;
        Vector2 posDelta = c.Anchor - position;
        switch (md.DirectionMode)
        {
            case DirectionMode.DRUL:
                if (Math.Abs(posDelta.X) > Math.Abs(posDelta.Y))
                    c.direction.Value = (c.Anchor.X > position.X) ? 2 : 4;
                else
                    c.direction.Value = (c.Anchor.Y > position.Y) ? 1 : 3;
                break;
            case DirectionMode.DRU:
                if (Math.Abs(posDelta.X) > Math.Abs(posDelta.Y))
                    c.direction.Value = (c.Anchor.X > position.X) ? 2 : -2;
                else
                    c.direction.Value = (c.Anchor.Y > position.Y) ? 1 : 3;
                break;
            case DirectionMode.RL:
                if (Math.Abs(posDelta.X) > 8f)
                    c.direction.Value = (c.Anchor.X > position.X) ? 1 : 2;
                break;
            case DirectionMode.R:
                if (Math.Abs(posDelta.X) > 8f)
                    c.direction.Value = (c.Anchor.X > position.X) ? 1 : -1;
                break;
            default:
                c.direction.Value = 1;
                break;
        }
    }

    /// <summary>First frame of animation, depending on direction.</summary>
    /// <returns>Frame number</returns>
    protected virtual int DirectionFrameStart()
    {
        if (md.DirectionMode == DirectionMode.Single)
            return md.FrameStart;
        return (Math.Abs(c.direction.Value) - 1) * md.FrameLength + md.FrameStart;
    }

    /// <summary>Helper function, check if the sprite collides with anything.</summary>
    /// <param name="location"></param>
    /// <param name="spritePosition"></param>
    /// <returns></returns>
    protected virtual bool CheckSpriteCollision(GameLocation location, Vector2 spritePosition)
    {
        return location.isCollidingPosition(
            new Rectangle(
                (int)spritePosition.X - vd.Width / 2,
                (int)spritePosition.Y - vd.Height / 2,
                vd.Width,
                vd.Height
            ),
            Game1.viewport,
            isFarmer: false,
            0,
            glider: false,
            null,
            pathfinding: true,
            projectile: false,
            ignoreCharacterRequirement: true
        );
    }
}
