using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using TrinketTinker.Companions.Anim;
using TrinketTinker.Models;
using TrinketTinker.Models.Mixin;
using TrinketTinker.Wheels;


namespace TrinketTinker.Companions.Motions
{
    /// <summary>Abstract class, controls drawing and movement of companion</summary>
    public abstract class Motion<TArgs> : IMotion where TArgs : IArgs
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
        /// <summary>NetString, key of next oneshotClip to set</summary>
        private AnimClipData? oneshotClip = null;
        /// <summary>NetString, key of next oneshotClip to set</summary>
        private AnimClipData? overrideClip = null;
        /// <summary>Heap of frames to draw, after the initial one.</summary>
        private readonly PriorityQueue<DrawSnapshot, long> drawSnapshotQueue = new();

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
        }

        /// <inheritdoc/>
        public void SetOneshotClip(string? clipKey)
        {
            if (clipKey == null)
                overrideClip = null;
            else
                md.AnimClips.TryGetValue(clipKey, out oneshotClip);
        }

        /// <inheritdoc/>
        public void SetOverrideClip(string? clipKey)
        {
            if (clipKey == null)
                overrideClip = null;
            else
                md.AnimClips.TryGetValue(clipKey, out overrideClip);
        }

        /// <inheritdoc/>
        public virtual void Initialize(Farmer farmer)
        {
            if (vd.LightSource is LightSourceData ldata)
            {
                lightId = $"{farmer.userID}/{c.ID}";
                Game1.currentLightSources.Add(lightId, new TinkerLightSource(lightId, c.Position + GetOffset(), ldata));
            }
        }

        /// <inheritdoc/>
        public virtual void Cleanup()
        {
            drawSnapshotQueue.Clear();
            if (vd.LightSource != null)
                Utility.removeLightSource(lightId);
        }

        /// <inheritdoc/>
        public virtual void OnOwnerWarp()
        {
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
                                location, originPoint, anchor.Range, ignoreUntargetables: true
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
                            if (Seek.ClosestMatchingObject(location, originPoint, anchor.Range, objMatch) is SObject closest)
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
                            if (Seek.ClosestMatchingTerrainFeature(location, originPoint, anchor.Range, terrainMatch) is TerrainFeature closest)
                            {
                                currAnchorTarget = anchor.Mode;
                                c.Anchor = closest.Tile * Game1.tileSize + new Vector2(Game1.tileSize / 2, Game1.tileSize / 2) - Vector2.One;
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

        /// <inheritdoc/>
        public virtual void UpdateGlobal(GameTime time, GameLocation location)
        {
            if (oneshotClip != null)
            {
                if (cs.AnimateClip(time, oneshotClip, md.Interval))
                {
                    oneshotClip = null;
                }
            }
            else if (overrideClip != null)
            {
                cs.AnimateClip(time, overrideClip, md.Interval);
            }
            else if (md.AlwaysMoving || GetMoving())
            {
                cs.Animate(md.LoopMode, time, DirectionFrameStart(), md.FrameLength, md.Interval);
            }
            else
            {
                if (md.GetIdleAnim(c.direction.Value) is AnimClipData idleAnim)
                {
                    cs.AnimateClip(time, idleAnim, md.Interval);
                }
                else
                {
                    cs.SetCurrentFrame(DirectionFrameStart());
                }
            }
        }

        /// <summary>Moving flag used for basis of anim</summary>
        /// <returns></returns>
        protected virtual bool GetMoving()
        {
            return c.OwnerMoving;
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

        /// <inheritdoc/>
        public virtual void Draw(SpriteBatch b)
        {
            while (drawSnapshotQueue.TryPeek(out DrawSnapshot? _, out long priority) &&
                   Game1.currentGameTime.TotalGameTime.Ticks >= priority)
            {
                drawSnapshotQueue.Dequeue().DoDraw(b);
            }

            Vector2 offset = GetOffset();
            float layerDepth = md.LayerDepth switch
            {
                LayerDepth.Behind => c.Owner.getDrawLayer() - 2 * 2E-06f,
                LayerDepth.InFront => c.Owner.getDrawLayer() + 2 * 2E-06f,
                _ => GetPositionalLayerDepth(offset),
            };
            b.Draw(
                cs.Texture,
                Game1.GlobalToLocal(c.Position + offset + c.Owner.drawOffset),
                cs.SourceRect,
                cs.DrawColor,
                GetRotation(),
                cs.Origin,
                GetTextureScale(),
                (c.direction.Value < 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                layerDepth
            );

            DrawSnapshot snapshot = new()
            {
                texture = cs.Texture,
                position = Game1.GlobalToLocal(c.Position + offset + c.Owner.drawOffset),
                sourceRect = cs.SourceRect,
                drawColor = cs.DrawColor,
                rotation = GetRotation(),
                origin = cs.Origin,
                textureScale = GetTextureScale(),
                effects = (c.direction.Value < 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                layerDepth = layerDepth
            };
            snapshot.DoDraw(b);
            EnqueueDrawSnapshots(snapshot);

            Vector2 shadowScale = GetShadowScale();
            if (shadowScale.X > 0 || shadowScale.Y > 0)
            {
                DrawSnapshot shadowSnapshot = new()
                {
                    texture = Game1.shadowTexture,
                    position = Game1.GlobalToLocal(c.Position + new Vector2(offset.X, 0) + c.Owner.drawOffset),
                    sourceRect = Game1.shadowTexture.Bounds,
                    drawColor = Color.White,
                    rotation = 0f,
                    origin = new Vector2(
                        Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y
                    ),
                    textureScale = shadowScale,
                    effects = SpriteEffects.None,
                    layerDepth = layerDepth - 2E-06f
                };
                shadowSnapshot.DoDraw(b);
                EnqueueDrawSnapshots(shadowSnapshot);
            }
        }

        private void EnqueueDrawSnapshots(DrawSnapshot snapshot)
        {
            for (int i = 1; i <= md.RepeatCount; i++)
            {
                drawSnapshotQueue.Enqueue(
                    snapshot,
                    Game1.currentGameTime.TotalGameTime.Ticks +
                    TimeSpan.FromMilliseconds(md.RepeatInternval * i).Ticks
                );
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
}