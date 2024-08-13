using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;

namespace TrinketTinker.Models
{
    public enum ProcOn
    {
        Always = 0, // this is Apply + Unapply
        Use = 1,
        Footstep = 2,
        ReceiveDamage = 3,
        DamageMonster = 4,
        Timed = 5
    }
    public class AbilityData
    {
        public string Name = "";
        public string? AbilityClass { get; set; } = null;
        public ProcOn ProcOn = new();
        public double ProcTimer { get; set; } = -1;
        public string? Condition { get; set; } = null;
        public int DamageThreshold { get; set; } = -1;
        public Dictionary<string, string> Args { get; set; } = new();

        public bool TryGetParsed<T>(string key, [NotNullWhen(true)] out T? ret)
        {
            ret = default;
            if (Args.TryGetValue(key, out string? valueStr) && valueStr != null)
            {
                return TryParse(valueStr, out ret);
            }
            return false;
        }

        public bool TryGetParsed<String>(string key, [NotNullWhen(true)] out string? ret)
        {
            ret = default;
            return Args.TryGetValue(key, out string? valueStr) && valueStr != null;
        }

        public bool ContainsKey(string key)
        {
            return Args.ContainsKey(key);
        }

        private static bool TryParse<T>(string valueStr, out T? ret)
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

    }
}