using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using TrinketTinker.Models;
using TrinketTinker.Models.MotionArgs;

namespace TrinketTinker.Companions.Motions;

/// <summary>Base version of LerpMotion, for use with inheritance</summary>
/// <inheritdoc/>
public class BaseLerpMotion<IArgs>(TrinketTinkerCompanion companion, MotionData mdata, VariantData vdata)
    : Motion<IArgs>(companion, mdata, vdata)
    where IArgs : LerpArgs
{
    /// <summary>Variable for how much interpolation happened so far.</summary>
    protected float Lerp
    {
        get => c.Lerp;
        set => c.Lerp = value;
    }
    private double pauseTimer = 0;

    /// <inheritdoc/>
    public override void UpdateLocal(GameTime time, GameLocation location)
    {
        if (!ShouldMove())
            return;

        if (args.MoveSync && !c.OwnerMoving)
            return;

        if (Lerp < 0f || AnchorChanged)
        {
            pauseTimer += time.ElapsedGameTime.TotalMilliseconds;
            if (pauseTimer < args.Pause)
                return;
            pauseTimer = 0f;
            float distance = (c.Anchor - c.Position).Length();
            if (distance > args.Max)
            {
                Utility.addRainbowStarExplosion(location, c.Position, 1);
                c.Position = c.Anchor;
                Lerp = -1f;
            }
            else if (distance > args.Min)
            {
                c.startPosition = c.Position;
                c.endPosition = c.Anchor;
                // float radius = 0.5f;
                c.endPosition =
                    c.Anchor
                    + 0.5f
                        * new Vector2(
                            Utility.RandomFloat(-args.Jitter, args.Jitter),
                            Utility.RandomFloat(-args.Jitter, args.Jitter)
                        );
                // if (CheckSpriteCollision(location, c.endPosition + GetOffset()))
                // {
                //     c.endPosition = c.Anchor;
                // }
                Lerp = 0f;
            }
            else if (md.AlwaysMoving && args.Jitter > 0f)
            {
                c.startPosition = c.Position;
                c.endPosition =
                    c.Anchor
                    + new Vector2(
                        Utility.RandomFloat(-args.Jitter, args.Jitter),
                        Utility.RandomFloat(-args.Jitter, args.Jitter)
                    );
                Lerp = 0f;
            }
        }
        if (Lerp >= 0f)
        {
            Lerp += (float)(time.ElapsedGameTime.TotalMilliseconds / args.Rate);
            Lerp = MathF.Min(1f, Lerp);
            c.NetPosition.X = Utility.Lerp(c.startPosition.X, c.endPosition.X, Lerp);
            c.NetPosition.Y = Utility.Lerp(c.startPosition.Y, c.endPosition.Y, Lerp);
            UpdateDirection();
            if (Lerp == 1f)
            {
                Lerp = -1f;
            }
        }
    }

    /// <inheritdoc/>
    public override Vector2 GetOffset()
    {
        return new Vector2(0, -vd.Height * 4 / 2) + base.GetOffset();
    }

    /// <inheritdoc/>
    protected override float GetRotation()
    {
        if (md.DirectionRotate)
        {
            Vector2 posDelta = c.Anchor - c.Position;
            return (float)Math.Atan2(posDelta.Y, posDelta.X);
        }
        return 0f;
    }

    // #if DEBUG
    //         public override void Draw(SpriteBatch b)
    //         {
    //             Vector2 localStartPos = Game1.GlobalToLocal(c.startPosition);
    //             b.Draw(
    //                 Game1.staminaRect,
    //                 new Rectangle((int)localStartPos.X, (int)localStartPos.Y, 16, 16),
    //                 Game1.staminaRect.Bounds,
    //                 Color.Red,
    //                 0f, Vector2.Zero,
    //                 SpriteEffects.None,
    //                 1f
    //             );
    //             Vector2 localEndPos = Game1.GlobalToLocal(c.endPosition);
    //             b.Draw(
    //                 Game1.staminaRect,
    //                 new Rectangle((int)localEndPos.X, (int)localEndPos.Y, 16, 16),
    //                 Game1.staminaRect.Bounds,
    //                 Color.Blue,
    //                 0f, Vector2.Zero,
    //                 SpriteEffects.None,
    //                 1f
    //             );
    //             base.Draw(b);
    //         }
    // #endif

    /// <inheritdoc/>
    public override void OnOwnerWarp()
    {
        Lerp = -1f;
        base.OnOwnerWarp();
    }
}

/// <summary>Companion closely follows the anchor, at a distance</summary>
/// <param name="companion"></param>
/// <param name="data"></param>
public class LerpMotion(TrinketTinkerCompanion companion, MotionData data, VariantData vdata)
    : BaseLerpMotion<LerpArgs>(companion, data, vdata) { }
