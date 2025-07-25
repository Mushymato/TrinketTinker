using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Monsters;
using StardewValley.Objects.Trinkets;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using TrinketTinker.Companions.Anim;
using TrinketTinker.Effects.Abilities;
using TrinketTinker.Models;
using TrinketTinker.Models.AbilityArgs;
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
    protected MotionData md;

    /// <inheritdoc/>
    public string MotionClass => md.MotionClass;

    /// <summary>Data for this motion.</summary>
    protected VariantData vd;

    /// <summary>Companion animation controller</summary>
    protected TinkerAnimSprite cs;

    public TinkerAnimSprite CompanionAnimSprite => cs;

    /// <summary>Light source ID, generated if LightRadius is set in <see cref="MotionData"/>.</summary>
    protected string lightId = "";

    /// <summary>Class dependent arguments for subclasses</summary>
    internal readonly TArgs args = default!;

    /// <summary>Anchors update every 50ms</summary>
    private const double ANCHOR_UPDATE_RATE = 50;

    /// <summary>Anchor timer</summary>
    private double anchorTimer = 0;

    /// <summary>Idle is "has not moved for at least 100ms"</summary>
    private const double IDLE_CD = 100;

    /// <summary>Anchor timer</summary>
    private double idleTimer = IDLE_CD;

    /// <summary>The previous anchor target</summary>
    protected AnchorTarget prevAnchorTarget = AnchorTarget.Owner;

    /// <summary>The current anchor target, is sync'd by <see cref="TrinketTinkerCompanion.CurrAnchorTarget"/></summary>
    protected AnchorTarget currAnchorTarget = AnchorTarget.Owner;

    /// <summary>Anchor changed during this tick</summary>s
    protected bool AnchorChanged => prevAnchorTarget != currAnchorTarget;

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
    protected int framesetLength = 1;

    /// <summary>Actual total frame used for Repeat, equal to frame length</summary>
    protected virtual int TotalFrames => framesetLength;

    /// <summary>Clip random</summary>
    public Random NetRand { get; set; } = Random.Shared;

    /// <summary>Currently valid anchor targets, based on abilities</summary>
    private readonly List<AnchorTargetData> activeAnchors = [];

    /// <summary>Bounding box of trinket</summary>
    public Rectangle BoundingBox { get; private set; } = Rectangle.Empty;

    public ChatterSpeaker Speaker => cs.Speaker;

    /// <summary>Basic constructor, tries to parse arguments as the generic <see cref="IArgs"/> type.</summary>
    /// <param name="companion"></param>
    /// <param name="mdata"></param>
    public Motion(TrinketTinkerCompanion companion, MotionData mdata, VariantData vdata)
    {
        if (typeof(TArgs) != typeof(NoArgs))
        {
            if (mdata.Args != null && mdata.Args.Parse<TArgs>() is TArgs parsed && parsed.Validate())
                args = parsed;
            else
                args = (TArgs)Activator.CreateInstance(typeof(TArgs))!;
        }
        c = companion;

        // this is just to prevent IDE from yelling
        md = mdata;
        vd = vdata;
        cs = new TinkerAnimSprite(vdata);
        // actual setter to use
        SetMotionVariantData(mdata, vdata);
    }

    public virtual void SetMotionVariantData(MotionData mdata, VariantData vdata)
    {
        md = mdata;
        vd = vdata;
        cs.SetFullVariant(vdata, c._altVariantKey.Value);

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
        if (speechBubbleKey == null || !md.SpeechBubbles.TryGetValue(speechBubbleKey, out speechBubble))
            speechBubble = null;
        else if (speechBubbleTimer > speechBubble.FadeOut * speechBubble.Timer)
            return;
        if (speechBubble != null)
        {
            if (!speechBubble.TryPickRand(NetRand, c.Owner, out speechBubble))
                return;
            if (!speechBubble.Nop)
            {
                speechBubbleTimer = speechBubble.Timer;
                return;
            }
        }
        speechBubble = null;
        speechBubbleTimer = 0;
    }

    /// <summary>Do the thing where I recheck me alt variants</summary>
    private void DoRecheckAltVariant(Trinket? trinketItem)
    {
        if (vd.TryRecheckAltVariant(c.Owner, c._altVariantKey.Value, trinketItem, out string? newAltVariantKey))
        {
            c._altVariantKey.Value = newAltVariantKey;
        }
    }

    /// <inheritdoc/>
    public void SetAltVariant(string? altVariantKey, Trinket? trinketItem)
    {
        if (altVariantKey == "RECHECK")
        {
            DoRecheckAltVariant(trinketItem);
        }
        else
        {
            cs.SetAltVariant(altVariantKey);
        }
    }

    /// <inheritdoc/>
    public void SetActiveAnchors(IEnumerable<string> abilityTypes)
    {
        activeAnchors.Clear();
        if (md.Anchors == null)
            return;
        foreach (AnchorTargetData anchor in md.Anchors)
        {
            if (anchor.RequiredAbilities == null || abilityTypes.Intersect(anchor.RequiredAbilities).Any())
            {
                activeAnchors.Add(anchor);
                continue;
            }
        }
    }

    /// <inheritdoc/>
    public void SetCurrAnchorTarget(int val)
    {
        currAnchorTarget = (AnchorTarget)val;
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
        DoRecheckAltVariant(c.trinketItem);
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
        anchorTimer = 0;
        if (vd.LightSource is LightSourceData ldata)
        {
            Game1.currentLightSources.Add(lightId, new TinkerLightSource(lightId, c.Position + GetOffset(), ldata));
        }
        DoRecheckAltVariant(c.trinketItem);
    }

    /// <summary>does a simple straight line collision check</summary>
    /// <param name="location"></param>
    /// <param name="farmer"></param>
    /// <param name="current"></param>
    /// <param name="target"></param>
    /// <param name="collisionStep"></param>
    /// <param name="collidingOn"></param>
    /// <returns></returns>
    internal static bool CanReachTarget(GameLocation location, Vector2 current, Vector2 target, int collisionStep = 64)
    {
        if (collisionStep < 1 || target == current)
            return true;
        Vector2 travel = target - current;
        float distance = travel.Length();
        if (distance <= collisionStep)
            return true;
        travel.Normalize();
        int idx = 0;
        do
        {
            Vector2 probe = current + idx * collisionStep * travel;
            if (!location.isTilePassable(Vector2.Round(probe / Game1.tileSize)))
                return false;
            idx++;
        } while (idx * collisionStep <= distance);
        return true;
    }

    /// <summary>Changes the position of the anchor that the companion moves relative to, based on <see cref="MotionData.Anchors"/>.</summary>
    /// <param name="time"></param>
    /// <param name="location"></param>
    public virtual AnchorTarget UpdateAnchor(GameTime time, GameLocation location)
    {
        if ((anchorTimer -= time.ElapsedGameTime.TotalMilliseconds) > 0)
        {
            if (currAnchorTarget == AnchorTarget.Owner)
                c.Anchor = Utility.PointToVector2(c.Owner.GetBoundingBox().Center);
            return currAnchorTarget;
        }
        anchorTimer = ANCHOR_UPDATE_RATE;
        return UpdateAnchor(location);
    }

    /// <summary>Set the current anchor position</summary>
    /// <param name="anchor"></param>
    /// <param name="location"></param>
    /// <param name="anchorPos"></param>
    /// <returns></returns>
    private bool SetAnchor(AnchorTargetData anchor, GameLocation location, Vector2 anchorPos)
    {
        if (
            md.Collision != CollisionMode.None
            && anchor.Mode != AnchorTarget.Owner
            && !CanReachTarget(location, c.Position, anchorPos)
        )
            return false;
        currAnchorTarget = anchor.Mode;
        if ((anchorPos - c.Position).Length() > anchor.StopRange)
            c.Anchor = anchorPos;
        return true;
    }

    /// <summary>Changes the position of the anchor that the companion moves relative to, based on <see cref="MotionData.Anchors"/>.</summary>
    /// <param name="time"></param>
    /// <param name="location"></param>
    protected virtual AnchorTarget UpdateAnchor(GameLocation location)
    {
        prevAnchorTarget = currAnchorTarget;
        var originPoint = c.Owner.getStandingPosition();
        foreach (AnchorTargetData anchor in activeAnchors)
        {
            Func<SObject, bool>? objMatch = null;
            Func<TerrainFeature, bool>? terrainMatch = null;
            bool includeLarge = false;
            switch (anchor.Mode)
            {
                case AnchorTarget.Monster:
                    {
                        // continue to use the base game version of this method for monsters, even tho I wrote a similar thing
                        Monster closest = Utility.findClosestMonsterWithinRange(
                            location,
                            originPoint,
                            anchor.Range,
                            ignoreUntargetables: true,
                            match: anchor.Filters != null ? (m) => Places.FilterStringId(anchor.Filters, m.Name) : null
                        );
                        if (
                            closest != null
                            && SetAnchor(anchor, location, Utility.PointToVector2(closest.GetBoundingBox().Center))
                        )
                            return anchor.Mode;
                    }
                    break;
                case AnchorTarget.FarmAnimal:
                    {
                        if (
                            Places.ClosestMatchingFarmAnimal(
                                location,
                                originPoint,
                                anchor.Range,
                                PetFarmAnimalAbility.IsFarmAnimalInNeedOfPetting
                            )
                                is Character closest
                            && SetAnchor(anchor, location, Utility.PointToVector2(closest.GetBoundingBox().Center))
                        )
                            return anchor.Mode;
                    }
                    break;
                case AnchorTarget.NPC:
                    {
                        if (
                            Places.ClosestMatchingCharacter(
                                location,
                                originPoint,
                                anchor.Range,
                                anchor.Filters != null ? (npc) => Places.FilterStringId(anchor.Filters, npc.Name) : null
                            )
                                is Character closest
                            && SetAnchor(anchor, location, Utility.PointToVector2(closest.GetBoundingBox().Center))
                        )
                            return anchor.Mode;
                    }
                    break;
                case AnchorTarget.Forage:
                    objMatch = (obj) => HarvestForageAbility.IsSpawnedItem(obj, anchor.Filters);
                    goto case AnchorTarget.Object;
                case AnchorTarget.Stone:
                    objMatch = (obj) => obj.IsBreakableStone();
                    goto case AnchorTarget.Object;
                case AnchorTarget.Twig:
                    objMatch = (obj) => obj.IsTwig();
                    goto case AnchorTarget.Object;
                case AnchorTarget.Weed:
                    objMatch = (obj) => obj.IsWeeds();
                    goto case AnchorTarget.Object;
                case AnchorTarget.DigSpot:
                    objMatch = (obj) => obj.IsDigSpot();
                    goto case AnchorTarget.Object;
                case AnchorTarget.Object:
                    {
                        if (
                            Places.ClosestMatchingObject(location, originPoint, anchor.Range, objMatch)
                                is SObject closest
                            && SetAnchor(
                                anchor,
                                location,
                                closest.TileLocation * Game1.tileSize
                                    + new Vector2(Game1.tileSize / 2, Game1.tileSize / 2)
                                    - Vector2.One
                            )
                        )
                        {
                            return anchor.Mode;
                        }
                    }
                    break;
                case AnchorTarget.Crop:
                    terrainMatch = (terrain) => HarvestCropAbility.CheckCrop(terrain, anchor.Filters);
                    goto case AnchorTarget.TerrainFeature;
                case AnchorTarget.Shakeable:
                    terrainMatch = (terrain) => HarvestShakeableAbility.CheckShakeable(terrain, anchor.Filters);
                    includeLarge = true;
                    goto case AnchorTarget.TerrainFeature;
                case AnchorTarget.TerrainFeature:
                    {
                        if (
                            Places.ClosestMatchingTerrainFeature(
                                location,
                                originPoint,
                                anchor.Range,
                                includeLarge,
                                terrainMatch
                            )
                                is TerrainFeature closest
                            && SetAnchor(
                                anchor,
                                location,
                                closest.Tile * Game1.tileSize
                                    + new Vector2(Game1.tileSize / 2, Game1.tileSize / 2)
                                    - Vector2.One
                            )
                        )
                        {
                            return anchor.Mode;
                        }
                    }
                    break;
                case AnchorTarget.Owner:
                    if (SetAnchor(anchor, location, Utility.PointToVector2(c.Owner.GetBoundingBox().Center)))
                        return anchor.Mode;
                    break;
            }
        }
        // base case is Owner
        currAnchorTarget = AnchorTarget.Owner;
        c.Anchor = Utility.PointToVector2(c.Owner.GetBoundingBox().Center);
        return currAnchorTarget;
    }

    /// <inheritdoc/>
    public abstract void UpdateLocal(GameTime time, GameLocation location);

    /// <summary>Helper, animate a particular clip</summary>
    /// <param name="time"></param>
    /// <param name="key"></param>
    /// <returns>
    /// 0: clip not found
    /// 1: clip is animating
    /// 2: clip reached last frame
    /// </returns>
    private TinkerAnimState AnimateClip(GameTime time, string? key, out AnimClipData? clip)
    {
        clip = null;
        if (key == null)
            return TinkerAnimState.None;
        int direction = c.direction.Value;
        if (key == HoverMotion.PERCHING)
            direction = StaticMotion.GetDirectionFromOwner(md, c.Owner.FacingDirection);
        if (!md.AnimClips.TryGetDirectional(key, direction, out clip))
        {
            if (key != null)
                currentClipKey = null;
            return TinkerAnimState.None;
        }
        if (currentClipKey != key)
        {
            if (!clip.TryPickRand(NetRand, c.Owner, out clip))
                return TinkerAnimState.None;
            cs.SetCurrentFrame(clip.FrameStart);
        }
        else
        {
            clip = clip.Selected;
        }
        currentClipKey = key;
        var animState = cs.AnimateClip(time, clip, md.Interval, md.Flip);
        // reset currentClipKey, allow the clip to be rerolled next time it is chosen
        if (animState == TinkerAnimState.Complete)
            currentClipKey = null;
        return animState;
    }

    /// <summary>Helper, animate a particular clip, discard selected clip</summary>
    /// <param name="time"></param>
    /// <param name="key"></param>
    /// <returns>
    /// 0: clip not found
    /// 1: clip is animating
    /// 2: clip reached last frame
    /// </returns>
    private TinkerAnimState AnimateClip(GameTime time, string? key)
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

        // Try each kind of anim in order, stop whenever one kind is playing

        // Oneshot Clip: play once and unset.
        if (
            AnimateClip(time, oneshotClipKey, out AnimClipData? clip) is TinkerAnimState res
            && res != TinkerAnimState.None
        )
        {
            if (res == TinkerAnimState.Complete)
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
        if (overrideClipKey != null && TinkerAnimState.Playing.HasFlag(AnimateClip(time, overrideClipKey)))
        {
            PauseMovementByAnimClip = false;
            return;
        }
        // Swiming: play while player is in the water,
        if (c.Owner.swimming.Value && TinkerAnimState.Playing.HasFlag(AnimateClip(time, AnimClipDictionary.SWIM)))
        {
            return;
        }
        // Moving: play while moving
        if (IsMoving())
        {
            idleTimer = IDLE_CD;
            // first, try anchor target based clip
            if (
                currAnchorTarget == AnchorTarget.Owner
                || AnimateClip(time, $"Anchor.{currAnchorTarget}") == TinkerAnimState.None
            )
            {
                // then play the default directional clip
                cs.UseExtra = false;
                cs.Animate(
                    md.LoopMode,
                    time,
                    DirectionFrameStart(),
                    md.FrameLength,
                    md.Interval,
                    spriteEffects: md.Flip
                );
            }
            return;
        }
        else if (c.OwnerMoving)
        {
            idleTimer = IDLE_CD;
            return;
        }
        if ((idleTimer -= time.ElapsedGameTime.Milliseconds) > 0)
        {
            return;
        }
        // Idle: play while companion is not moving
        if (!TinkerAnimState.Playing.HasFlag(AnimateClip(time, AnimClipDictionary.IDLE)))
        {
            // cs.CurrentClip = null;
            cs.UseExtra = false;
            if (md.FrameLength > 0)
                // Default: Use first frame of the current direction as fallback
                cs.SetCurrentFrame(DirectionFrameStart());
            else
                cs.SetCurrentFrame(-1);
        }
    }

    /// <summary>Moving flag used for basis of anim</summary>
    /// <returns></returns>
    protected abstract bool IsMoving();

    /// <summary>Whether the companion should attempt to move this frame</summary>
    /// <returns></returns>
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
        return new(cs.TextureScale, cs.TextureScale);
    }

    /// <summary>Get shadow draw scale.</summary>
    /// <returns></returns>
    protected virtual Vector2 GetShadowScale()
    {
        return new(cs.ShadowScale, cs.ShadowScale);
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

    public virtual float GetDrawLayer()
    {
        return md.LayerDepth switch
        {
            LayerDepth.Behind => c.Owner.getDrawLayer() - 2 * Visuals.LAYER_OFFSET,
            LayerDepth.InFront => c.Owner.getDrawLayer() + 2 * Visuals.LAYER_OFFSET,
            _ => GetPositionalLayerDepth(GetOffset()),
        };
    }

    /// <inheritdoc/>
    public void Draw(SpriteBatch b)
    {
        if (
            Game1.eventUp && (ModEntry.Config.HideAllCompanionsDuringEvents || md.HideDuringEvents)
            || (Game1.CurrentEvent != null && Game1.CurrentEvent.id == "MovieTheaterScreening")
        )
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
        float layerDepth = GetDrawLayer();

        Vector2 drawPos = c.Position + c.Owner.drawOffset + offset;
        Vector2 scale = GetTextureScale();
        DrawSnapshot snapshot =
            new(
                cs.Texture,
                drawPos,
                cs.SourceRect,
                cs.DrawColor,
                GetRotation(),
                cs.Origin,
                scale,
                cs.Flip ^ ((c.direction.Value < 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None),
                layerDepth,
                CurrentFrame: cs.currentFrame
            );
        DrawCompanion(b, snapshot);
        if (cs.Breather != null)
        {
            float breathing = Math.Max(
                0f,
                (float)
                    Math.Ceiling(
                        Math.Sin(
                            Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 600.0 + (double)(c.Position.X * 20f)
                        )
                    ) / 4f
            );
            DrawSnapshot breatherSnapshot =
                new(
                    cs.Texture,
                    drawPos + cs.Breather.Pos,
                    new(
                        cs.SourceRect.X + cs.Breather.Rect.X,
                        cs.SourceRect.Y + cs.Breather.Rect.Y,
                        cs.Breather.Rect.Width,
                        cs.Breather.Rect.Height
                    ),
                    cs.DrawColor,
                    GetRotation(),
                    new(cs.Breather.Rect.Width / 2, cs.Breather.Rect.Height / 2 + 1),
                    new(scale.X + breathing, scale.Y + breathing),
                    cs.Flip ^ ((c.direction.Value < 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None),
                    layerDepth + Visuals.LAYER_OFFSET / 2
                );
            DrawCompanionBreathing(b, breatherSnapshot);
        }

        Vector2 shadowScale = GetShadowScale();
        Vector2 shadowDrawPos = Vector2.Zero;
        if (shadowScale.X > 0 || shadowScale.Y > 0)
        {
            shadowDrawPos = c.Position + c.Owner.drawOffset + GetShadowOffset(offset);
            DrawSnapshot shadowSnapshot =
                new(
                    Game1.shadowTexture,
                    shadowDrawPos,
                    Game1.shadowTexture.Bounds,
                    Color.White,
                    0f,
                    new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y),
                    shadowScale,
                    SpriteEffects.None,
                    layerDepth - Visuals.LAYER_OFFSET
                );
            DrawShadow(b, shadowSnapshot);
        }

        // bounding box
        BoundingBox = cs.GetBoundingBox(drawPos, scale, shadowDrawPos, shadowScale);
        if (ModEntry.Config.DrawDebugMode)
        {
            Utility.DrawSquare(
                b,
                Game1.GlobalToLocal(Game1.viewport, BoundingBox),
                0,
                backgroundColor: cs.UseExtra ? Color.Maroon : Color.Magenta
            );
            Utility.DrawSquare(
                b,
                Game1.GlobalToLocal(Game1.viewport, c.Owner.GetBoundingBox()),
                0,
                backgroundColor: Color.Cyan
            );
        }

        // speech bubble
        if (speechBubble != null)
        {
            Vector2 worldDrawPos =
                Game1.GlobalToLocal(new(drawPos.X, drawPos.Y - cs.Height * 4 - Game1.tileSize)) + speechBubble.Offset;
            float alphaD = 0f;
            double fadeInThreshold = speechBubble.Timer * (1 - speechBubble.FadeIn);
            double fadeOutThreshold = speechBubble.Timer * speechBubble.FadeOut;
            if (speechBubbleTimer >= fadeInThreshold)
                alphaD = (float)((speechBubbleTimer - fadeInThreshold) / (speechBubble.FadeIn * speechBubble.Timer));
            else if (speechBubbleTimer <= fadeOutThreshold)
                alphaD = (float)((fadeOutThreshold - speechBubbleTimer) / (speechBubble.FadeOut * speechBubble.Timer));
            SpriteText.drawStringWithScrollCenteredAt(
                b,
                TokenParser.ParseText(speechBubble.Text),
                (int)worldDrawPos.X + Game1.random.Next(-1 * speechBubble.Shake, 2 * speechBubble.Shake),
                (int)worldDrawPos.Y + Game1.random.Next(-1 * speechBubble.Shake, 2 * speechBubble.Shake),
                "",
                1f - alphaD,
                speechBubble.Color == null ? null : Visuals.GetSDVColor(speechBubble.Color, out bool _),
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

    /// <summary>Draw companion breathing from snapshot, enqueue repeat as required</summary>
    /// <param name="b"></param>
    /// <param name="snapshot"></param>
    protected virtual void DrawCompanionBreathing(SpriteBatch b, DrawSnapshot snapshot)
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
                    int newFrame = cs.currentFrame + frameset * TotalFrames;
                    framesetSnapshot = snapshot.CloneWithChanges(
                        sourceRect: cs.GetSourceRect(newFrame),
                        currentFrame: newFrame
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
        Vector2 posDelta = c.Anchor - c.Position;
        switch (md.DirectionMode)
        {
            case DirectionMode.DRUL:
                if (Math.Abs(posDelta.X) - Math.Abs(posDelta.Y) > TinkerConst.TURN_LEEWAY)
                    c.direction.Value = posDelta.X > 0 ? 2 : 4;
                else
                    c.direction.Value = posDelta.Y > 0 ? 1 : 3;
                break;
            case DirectionMode.DRU:
                if (Math.Abs(posDelta.X) - Math.Abs(posDelta.Y) > TinkerConst.TURN_LEEWAY)
                    c.direction.Value = posDelta.X > 0 ? 2 : -2;
                else
                    c.direction.Value = posDelta.Y > 0 ? 1 : 3;
                break;
            case DirectionMode.RL:
                if (Math.Abs(posDelta.X) > TinkerConst.TURN_LEEWAY)
                    c.direction.Value = posDelta.X > 0 ? 1 : 2;
                break;
            case DirectionMode.R:
                if (Math.Abs(posDelta.X) > TinkerConst.TURN_LEEWAY)
                    c.direction.Value = posDelta.X > 0 ? 1 : -1;
                break;
            default:
                c.direction.Value = 1;
                break;
        }
    }

    public virtual bool IsFacing(Vector2 target)
    {
        Vector2 position = c.Position;
        Vector2 posDirection = target - position;
        int direction = c.direction.Value;
        switch (md.DirectionMode)
        {
            case DirectionMode.DRUL:
                switch (direction)
                {
                    case 1:
                        return posDirection.Y > 0;
                    case 2:
                        return posDirection.X > 0;
                    case 3:
                        return posDirection.Y < 0;
                    case 4:
                        return posDirection.X < 0;
                }
                break;
            case DirectionMode.DRU:
                switch (direction)
                {
                    case 1:
                        return posDirection.Y > 0;
                    case 2:
                        return posDirection.X > 0;
                    case 3:
                        return posDirection.Y < 0;
                    case -2:
                        return posDirection.X < 0;
                }
                break;
            case DirectionMode.RL:
                switch (direction)
                {
                    case 1:
                        return posDirection.X > 0;
                    case 2:
                        return posDirection.X < 0;
                }
                break;
            case DirectionMode.R:
                switch (direction)
                {
                    case 1:
                        return posDirection.X > 0;
                    case -1:
                        return posDirection.X < 0;
                }
                break;
            default:
                return false;
        }
        return false;
    }

    /// <summary>First frame of animation, depending on direction.</summary>
    /// <returns>Frame number</returns>
    protected virtual int DirectionFrameStart()
    {
        if (md.DirectionMode == DirectionMode.Single)
            return md.FrameStart;
        return (Math.Abs(c.direction.Value) - 1) * md.FrameLength + md.FrameStart;
    }
}
