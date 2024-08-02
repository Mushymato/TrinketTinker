using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace TrinketTinker.Companions.Motions
{
    public class StaticMotion : Motion
    {
        public StaticMotion(TrinketTinkerCompanion companion) : base(companion) { }
        public override void UpdateLocal(GameTime time, GameLocation location)
        {
            c.Position = c.Owner.Position;
        }

        public override void UpdateGlobal(GameTime time, GameLocation location)
        {
            base.UpdateGlobal(time, location);
            _drawOffset = new Vector2(0f, -64f);
        }
    }
}