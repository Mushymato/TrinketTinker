using Newtonsoft.Json.Linq;

namespace TrinketTinker.Models.Mixin;

/// <summary>Arbitrary arguments to be deserialized later.</summary>
public static class ArgsDictExtension
{
    /// <summary>Tries to parse this dict to target model of type <see cref="IArgs"/></summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    internal static T? Parse<T>(this Dictionary<string, object>? args)
        where T : IArgs
    {
        try
        {
            if (args == null)
                return default;
            return JToken.FromObject(args).ToObject<T>();
        }
        catch (Exception ex)
        {
            ModEntry.LogOnce(
                $"Failed to convert args to {typeof(T)}, this is caused by invalid data in a content pack using TrinketTinker:\n{ex}"
            );
            return default;
        }
    }
}

public interface IHaveArgs
{
    /// <summary>Arbitrary arguments to be deserialized later.</summary>
    public Dictionary<string, object>? Args { get; set; }
}
