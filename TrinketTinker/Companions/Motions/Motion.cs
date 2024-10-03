using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Monsters;
using TrinketTinker.Models;
using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Companions.Motions
{
    /// <summary>Abstract class, controls drawing and movement of companion</summary>
    public abstract class Motion<TArgs> : IMotion where TArgs : IArgs
    {
        /// <summary>Companion that owns this motion.</summary>
        protected readonly TrinketTinkerCompanion c;
        /// <summary>Data for this motion.</summary>
        protected readonly MotionData d;
        /// <summary>Constant offset, derived from data</summary>
        protected Vector2 motionOffset;
        /// <summary>Should reverse the animation, used for <see cref="LoopMode.PingPong"/></summary>
        protected bool isReverse = false;
        /// <summary>Light source ID, generated if LightRadius is set in <see cref="MotionData"/>.</summary>
        protected string lightId = "";
        /// <summary>Class dependent arguments for subclasses</summary>
        protected readonly TArgs? args;

        /// <summary>Constructor</summary>
        /// <param name="companion"></param>
        /// <param name="data"></param>
        public Motion(TrinketTinkerCompanion companion, MotionData data)
        {
            args = data.ParseArgs<TArgs>();
            if (args != null && !args.Validate())
                args = default;
            c = companion;
            d = data;
            motionOffset = new(d.OffsetX, d.OffsetY);
            c.Offset = motionOffset;
        }

        /// <summary>Initialize motion, setup lightsource if needed.</summary>
        /// <param name="farmer"></param>
        public virtual void Initialize(Farmer farmer)
        {
            if (d.LightRadius > 0)
            {
                lightId = $"{c.ID}_{Game1.random.Next()}";
                Game1.currentLightSources.Add(lightId, new LightSource(lightId, 1, c.Position + c.Offset, d.LightRadius, Color.Black, LightSource.LightContext.None, 0L));
            }
        }

        /// <summary>Initialize motion, remove lightsource.</summary>
        /// <param name="farmer"></param>
        public virtual void Cleanup()
        {
            if (d.LightRadius > 0)
            {
                Utility.removeLightSource(lightId);
            }
        }

        /// <summary>Make a new light source on warp.</summary>
        /// <param name="farmer"></param>
        /// 
        public virtual void OnOwnerWarp()
        {
            if (d.LightRadius > 0)
            {
                Game1.currentLightSources.Add(lightId, new LightSource(lightId, 1, c.Position + c.Offset, d.LightRadius, Color.Black, LightSource.LightContext.None, 0L));
            }
        }

        public virtual void UpdateAnchor(GameTime time, GameLocation location)
        {
            foreach (AnchorTarget target in d.AnchorTargetPriority)
            {
                switch (target)
                {
                    case AnchorTarget.Monster:
                        Monster closest = Utility.findClosestMonsterWithinRange(
                            location, c.Owner.getStandingPosition(), 384, ignoreUntargetables: true
                        );
                        if (closest != null)
                        {
                            c.Anchor = Utility.PointToVector2(closest.GetBoundingBox().Center);
                            return;
                        }
                        break;
                    case AnchorTarget.Owner:
                        c.Anchor = Utility.PointToVector2(c.Owner.GetBoundingBox().Center);
                        return;
                }
            }
            c.Anchor = Utility.PointToVector2(c.Owner.GetBoundingBox().Center);
        }

        /// <summary>Update for the client of the player that equipped this trinket, responsible for netfields.</summary>
        /// <param name="time">Game time</param>
        /// <param name="location">Current map location</param>
        public abstract void UpdateLocal(GameTime time, GameLocation location);

        /// <summary>Update for all clients, responsible for animation frame.</summary>
        /// <param name="time">Game time</param>
        /// <param name="location">Current map location</param>
        public virtual void UpdateGlobal(GameTime time, GameLocation location)
        {
            if (d.AlwaysMoving || c.Moving)
            {
                int frameStart = DirectionFrameStart();
                switch (d.LoopMode)
                {
                    case LoopMode.PingPong:
                        isReverse = c.Sprite.AnimatePingPong(time, frameStart, d.AnimationFrameLength, d.Interval, isReverse);
                        break;
                    case LoopMode.Standard:
                        c.Sprite.Animate(time, frameStart, d.AnimationFrameLength, d.Interval);
                        break;
                }
            }
            else
            {
                c.Sprite.currentFrame = DirectionFrameStart();
                c.Sprite.UpdateSourceRect();
            }
            if (d.LightRadius > 0 && location.Equals(Game1.currentLocation))
                Utility.repositionLightSource(lightId, c.Position + c.Offset);
        }

        /// <summary>Draw the companion.</summary>
        /// <param name="b">SpriteBatch</param>
        public virtual void Draw(SpriteBatch b)
        {
            // float shadowScale = 3f * Utility.Lerp(1f, 0.8f, Math.Max(1f, -c.Offset.Y / 12));
            DrawWithShadow(
                b, c.Position.Y / 10000f,
                new Vector2(d.TextureScale, d.TextureScale),
                new Vector2(d.ShadowScale, d.ShadowScale)
            );
        }

        /// <summary>Default draw implementation, draws the companion plus a shadow.</summary>
        /// <param name="b"></param>
        /// <param name="layerDepth"></param>
        /// <param name="textureScale"></param>
        /// <param name="shadowScale"></param>
        protected virtual void DrawWithShadow(SpriteBatch b, float layerDepth, Vector2 textureScale, Vector2 shadowScale)
        {
            layerDepth = d.LayerDepth switch
            {
                LayerDepth.Behind => c.Owner.getDrawLayer() - 2 * 2E-06f,
                LayerDepth.InFront => c.Owner.getDrawLayer() + 2 * 2E-06f,
                _ => layerDepth,
            };
            b.Draw(
                c.Sprite.Texture,
                Game1.GlobalToLocal(c.Position + c.Offset + c.Owner.drawOffset),
                c.Sprite.SourceRect,
                c.SpriteColor,
                c.rotation.Value,
                c.SpriteOrigin,
                textureScale,
                (c.direction.Value < 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                layerDepth
            );
            if (shadowScale.X > 0 || shadowScale.Y > 0)
            {
                b.Draw(
                    Game1.shadowTexture,
                    Game1.GlobalToLocal(c.Position + new Vector2(c.Offset.X, 0) + c.Owner.drawOffset),
                    Game1.shadowTexture.Bounds,
                    Color.White,
                    0f,
                    new Vector2(
                        Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y
                    ),
                    shadowScale,
                    SpriteEffects.None,
                    layerDepth - 2E-06f
                );
            }
        }

        /// <summary>Update companion facing direction using current position</summary>
        protected virtual void UpdateDirection()
        {
            UpdateDirection(c.Position);
        }

        /// <summary>Update companion facing direction using a direction.</summary>
        /// <param name="position"></param>
        protected virtual void UpdateDirection(Vector2 position)
        {
            Vector2 posDelta = c.Anchor - position;
            switch (d.DirectionMode)
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
                case DirectionMode.Rotate:
                    c.rotation.Value = (float)Math.Atan2(posDelta.Y, posDelta.X);
                    break;
                case DirectionMode.None:
                    c.direction.Value = 1;
                    break;
            }
        }

        /// <summary>First frame of animation, depending on direction.</summary>
        /// <returns>Frame number</returns>
        protected virtual int DirectionFrameStart()
        {
            if (d.DirectionMode == DirectionMode.None)
                return d.AnimationFrameStart;
            return (Math.Abs(c.direction.Value) - 1) * d.AnimationFrameLength + d.AnimationFrameStart;
        }

        protected virtual bool CheckSpriteCollsion(GameLocation location, Vector2 spritePosition)
        {
            return location.isCollidingPosition(
                new Rectangle(
                    (int)spritePosition.X - c.Sprite.SpriteWidth / 2,
                    (int)spritePosition.Y - c.Sprite.SpriteHeight / 2,
                    c.Sprite.SpriteWidth,
                    c.Sprite.SpriteHeight
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