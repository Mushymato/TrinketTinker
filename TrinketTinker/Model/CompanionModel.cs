using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Companions;
using StardewValley;

namespace TrinketTinker.Model
{
    public class CompanionModel
    {
        //   "ID": "{{ModId}}_ButterflyTrinket1",
        //   "CompanionClass": "TrinketTinker.Companions.HoveringCompanion, TrinketTinker",
        //   "Texture": "Mod/{{ModId}}/Butterflies",
        //   "FrameRate": 24,
        //   "Variant": 0
        //   // "SpriteSize": { "X": 16, // width "Y": 16 // height }, // default 16x16
        //   // "AnimationLength": 4 // derived from texture and sprite size if not given
        public string ID { get; set; } = "";
        public string CompanionClass { get; set; } = "";
        public string Texture { get; set; } = "";
        public Point Size { get; set; } = new Point(16, 16);
        public int AnimationLength { get; set; } = 0;
        public int FrameRate { get; set; } = 1;
        public float FrameDuration { get; set; } = 0;
        public int Variant { get; set; } = 0;
        public bool IsLightSource = false;

        public bool TryGetCompanion([NotNullWhen(true)] out Companion? companion)
        {
            if (AnimationLength == 0)
            {
                Texture2D texture = Game1.content.Load<Texture2D>(Texture);
                AnimationLength = texture.Width / Size.X;
            }
            if (FrameDuration == 0)
            {
                FrameDuration = 1000f / FrameRate;
            }
            companion = null;
            Type? companionCls = Type.GetType(CompanionClass);
            if (companionCls == null)
                return false;
            companion = (Companion?)Activator.CreateInstance(companionCls, new object[] { this });
            return companion != null;
        }

        public override string ToString()
        {
            return $"{CompanionClass}: {Texture} Variant {Variant}";
        }
    }
}