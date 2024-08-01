using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Companions;
using StardewValley;


namespace TrinketTinker.Model
{
    public class CompanionModel
    {
        public string ID { get; set; } = "";
        public string CompanionClass { get; set; } = "";
        public string Texture { get; set; } = "";
        public Point Size { get; set; } = new Point(16, 16);
        public int FramesPerAnimation { get; set; } = 0;
        public int FrameRate { get; set; } = 1;
        public float FrameInterval { get; set; } = 0;
        public Point HoverOffset { get; set; } = Point.Zero;
        public int Variant { get; set; } = 0;
        public float LightRadius = 0f;

        public bool TryGetCompanion([NotNullWhen(true)] out Companion? companion)
        {
            if (FramesPerAnimation == 0)
            {
                Texture2D texture = Game1.content.Load<Texture2D>(Texture);
                FramesPerAnimation = texture.Width / Size.X;
            }
            if (FrameInterval == 0)
            {
                FrameInterval = 1000f / FrameRate;
            }
            companion = null;
            Type? companionCls = Type.GetType(CompanionClass);
            if (companionCls == null)
                return false;
            companion = (Companion?)Activator.CreateInstance(companionCls, this);
            return companion != null;
        }

        public override string ToString()
        {
            return $"{CompanionClass}: {Texture} Variant {Variant}";
        }
    }
}