using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace TrinketTinker.Models
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    /// <summary>Provides the <see cref="Args"/> property, whose values can be parsed to different types depending on the consumer.</summary>
    public abstract class HaveArgs
    {
        /// <summary>Arbiturary arguments.</summary>
        public Dictionary<string, string> Args { get; set; } = [];

        /// <summary>
        /// Get value from <see cref="Args"/> and try to parse to the expected type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public virtual bool TryGetParsed<T>(string key, [NotNullWhen(true)] out T? ret)
        {
            ret = default;
            if (Args.TryGetValue(key, out string? valueStr) && valueStr != null)
            {
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
        /// Get parsed value, or a specified non null default.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public virtual T GetParsedOrDefault<T>(string key, T defaultValue)
        {
            T ret = defaultValue;
            if (Args.TryGetValue(key, out string? valueStr) && valueStr != null)
            {
                TypeConverter con = TypeDescriptor.GetConverter(typeof(T));
                if (con != null)
                {
                    try
                    {
                        T? parsed = (T?)con.ConvertFromString(valueStr);
                        if (parsed != null)
                            return parsed;
                    }
                    catch (NotSupportedException)
                    {
                        return ret;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Get value from <see cref="Args"/>.
        /// </summary>
        /// <typeparam name="String"></typeparam>
        /// <param name="key"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public virtual bool TryGetParsed<String>(string key, [NotNullWhen(true)] out string? ret)
        {
            ret = default;
            return Args.TryGetValue(key, out string? valueStr) && valueStr != null;
        }

        /// <summary>
        /// Check that <see cref="Args"/> has key, without checking the value.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual bool ContainsKey(string key)
        {
            return Args.ContainsKey(key);
        }
    }
}