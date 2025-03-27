using Newtonsoft.Json;

namespace TrinketTinker.Models.Mixin;

/// <summary>Arbitrary arguments to be deserialized later.</summary>
public class ArgsDict : Dictionary<string, object>
{
    /// <summary>Tries to parse this dict to target model of type <see cref="IArgs"/></summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    internal T? Parse<T>()
        where T : IArgs
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(this));
        }
        catch (Exception ex)
        {
            ModEntry.LogOnce($"Failed to convert args to {typeof(T)}, this is caused by invalid data in a content pack using TrinketTinker:\n{ex}");
            return default;
        }
    }
}

public interface IHaveArgs
{
    /// <summary>Arbitrary arguments to be deserialized later.</summary>
    public ArgsDict? Args { get; set; }
}
