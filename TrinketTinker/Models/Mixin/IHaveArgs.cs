using Newtonsoft.Json;

namespace TrinketTinker.Models.Mixin
{
    public abstract class IHaveArgs
    {
        /// <summary>Arbiturary arguments.</summary>
        public Dictionary<string, string>? Args { get; set; }
        public T? ParseArgs<T>()
        {
            if (Args == null)
            {
                return default;
            }
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(Args));
        }
    }
}