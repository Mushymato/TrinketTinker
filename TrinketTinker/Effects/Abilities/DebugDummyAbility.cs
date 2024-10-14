using System.Text;
using StardewModdingAPI;
using TrinketTinker.Effects.Proc;
using TrinketTinker.Models;
using TrinketTinker.Models.Mixin;

namespace TrinketTinker.Effects.Abilities
{
    /// <summary>Prints many logs, doesn't do anything else.</summary>
    public sealed class DebugDummyAbility(TrinketTinkerEffect effect, AbilityData data, int lvl) : Ability<NoArgs>(effect, data, lvl)
    {
        /// <summary>Print debug log.</summary>
        /// <param name="proc"></param>
        /// <returns></returns>
        protected override bool ApplyEffect(ProcEventArgs proc)
        {
            StringBuilder sbld = new();
            sbld.Append(Name);
            sbld.Append('(');
            sbld.Append(proc.ProcOn);
            sbld.Append(')');
            ModEntry.Log(sbld.ToString(), LogLevel.Debug);
            return true;
        }
    }
}
