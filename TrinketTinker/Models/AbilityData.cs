using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;

namespace TrinketTinker.Models
{
    /// <summary>Defines how an ability can activate.</summary>
    public enum ProcOn
    {
        /// <summary>Proc on equip, ignores all conditions.</summary>
        Always,
        /// <summary>Proc on item use.</summary>
        Use,
        /// <summary>Proc on walk.</summary>
        Footstep,
        /// <summary>Proc on player damaged.</summary>
        ReceiveDamage,
        /// <summary>Proc on monster damaged.</summary>
        DamageMonster,
        /// <summary>Proc on timer elapsed.</summary>
        Timed,
    }

    /// <summary>Data for <see cref="Effects.Abilities"/></summary>
    public class AbilityData
    {
        /// <summary>Name of this ability.</summary>
        public string Name = "";
        /// <summary>Class name, need to be fully qualified to use an ability not provided by this mod.</summary>
        public string? AbilityClass { get; set; } = null;
        /// <summary>Proc on rule</summary>
        public ProcOn ProcOn = new();
        /// <summary>Timeout for ability procs.</summary>
        public double ProcTimer { get; set; } = -1;
        /// <summary>Sound to play on proc</summary>
        public string? ProcSound { get; set; } = null;
        /// <summary>Condition, see <see cref="StardewValley.GameStateQuery"/></summary>
        public string? Condition { get; set; } = null;
        /// <summary>Minimum damage dealt or received before proc.</summary>
        public int DamageThreshold { get; set; } = -1;
        /// <summary>Ability specific arguments.</summary>
        public Dictionary<string, string> Args { get; set; } = new();

        /// <summary>
        /// Parse a string argument to expected type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public bool TryGetParsed<T>(string key, [NotNullWhen(true)] out T? ret)
        {
            ret = default;
            if (Args.TryGetValue(key, out string? valueStr) && valueStr != null)
            {
                ret = default;
                TypeConverter con = TypeDescriptor.GetConverter(typeof(T));
                if (con != null)
                {
                    try
                    {
                        ret = (T?)con.ConvertFromString(valueStr);
                        if (ret != null)
                            return true;
                    }
                    catch (NotSupportedException)
                    {
                        return false;
                    }
                }
                return false;
            }
            return false;
        }

        /// <summary>
        /// Get a string, check that it is not null
        /// </summary>
        /// <typeparam name="String"></typeparam>
        /// <param name="key"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public bool TryGetParsed<String>(string key, [NotNullWhen(true)] out string? ret)
        {
            ret = default;
            return Args.TryGetValue(key, out string? valueStr) && valueStr != null;
        }

        /// <summary>
        /// Check that <see cref="Args"/> has key, without checking the value.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            return Args.ContainsKey(key);
        }

    }
}