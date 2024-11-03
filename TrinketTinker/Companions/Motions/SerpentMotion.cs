using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using TrinketTinker.Companions.Anim;
using TrinketTinker.Models;
using TrinketTinker.Models.MotionArgs;
using TrinketTinker.Wheels;

namespace TrinketTinker.Companions.Motions
{
    /// <summary>
    /// Companion has trailing segments and tail, like the royal serpent.
    /// Needs additional sets of sprites for segment(s) and tail.
    /// </summary>
    public sealed class SerpentMotion : BaseLerpMotion<SerpentArgs>
    {
        /// <summary>Position and rotation of segments</summary>
        private readonly List<Vector3> segments = [];
        /// <summary>Number of segments</summary>
        private int SegmentCount => args.SegmentCount;
        /// <summary>Total frame, accounting for number of segments</summary>
        protected override int TotalFrames => framesetLength * SegmentCount;
        /// <summary>Length of a segment, based on width of sprite.</summary>
        private readonly int segUnit;

        public SerpentMotion(TrinketTinkerCompanion companion, MotionData mdata, VariantData vdata) : base(companion, mdata, vdata)
        {
            segUnit = (int)(vdata.Width * args.Sparcity);
        }

        /// <inheritdoc/>
        public override void UpdateGlobal(GameTime time, GameLocation location)
        {
            Vector2 pos = c.Position;

            // populate or truncate segments
            if (segments.Count > SegmentCount)
                segments.RemoveRange(SegmentCount, segments.Count - SegmentCount);
            else
                while (segments.Count < SegmentCount)
                    segments.Add(new Vector3(pos.X, pos.Y, 0f));

            // propangate position and rotation
            for (int i = 0; i < segments.Count; i++)
            {
                Vector2 segPos = segments[i].AsVec2();
                Vector2 segDelta = segPos - pos;
                float segDeltaLength = segDelta.Length();
                segDelta.Normalize();
                if (segDeltaLength > segUnit)
                    segPos = segDelta * segUnit + pos;
                // segments[i] = segPos.WithRot(MathF.Atan2(segDelta.Y, segDelta.X) - MathF.PI / 2f);
                segments[i] = segPos.WithRot(MathF.Atan2(segDelta.Y, segDelta.X) + MathF.PI);
                pos = segPos;
            }

            base.UpdateGlobal(time, location);
        }

        /// <inheritdoc/>
        protected override void DrawCompanion(SpriteBatch b, DrawSnapshot snapshot)
        {
            base.DrawCompanion(b, snapshot);
            Vector2 offset = GetOffset();
            DrawSnapshot segSnapshot;
            // segments
            int altIdx = 1;
            for (int i = 0; i < segments.Count - 1; i++)
            {
                segSnapshot = snapshot.CloneWithChanges(
                    position: segments[i].AsVec2() + c.Owner.drawOffset + offset,
                    sourceRect: cs.GetSourceRect(cs.currentFrame + framesetLength * altIdx),
                    rotation: segments[i].Z
                );
                segSnapshot.Draw(b);
                EnqueueRepeatDraws(segSnapshot, false);
                altIdx++;
                if (altIdx > args.SegmentAlts)
                    altIdx = 1;
            }
            if (args.HasTail)
            {
                // tail
                segSnapshot = snapshot.CloneWithChanges(
                    position: segments.Last().AsVec2() + c.Owner.drawOffset + offset,
                    sourceRect: cs.GetSourceRect(cs.currentFrame + framesetLength * (args.SegmentAlts + 1)),
                    rotation: segments.Last().Z
                );
                segSnapshot.Draw(b);
                EnqueueRepeatDraws(segSnapshot, false);
            }
        }

        /// <summary>Do not draw shadow for this motion type it looks bad.</summary>
        /// <param name="b"></param>
        /// <param name="snapshot"></param>
        protected override void DrawShadow(SpriteBatch b, DrawSnapshot snapshot)
        {
            // base.DrawShadow(b, snapshot);
            // Vector2 offset = GetOffset();
            // DrawSnapshot segSnapshot;
            // // shadows
            // for (int i = 0; i < segments.Count - 1; i++)
            // {
            //     segSnapshot = snapshot.CloneWithChanges(
            //         position: segments[i].AsVec2() + c.Owner.drawOffset + new Vector2(offset.X, 0)
            //     );
            //     segSnapshot.Draw(b);
            //     EnqueueRepeatDraws(segSnapshot, false);
            // }
        }
    }
}