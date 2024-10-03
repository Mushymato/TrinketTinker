using Newtonsoft.Json;

namespace TrinketTinker.Models.Mixin
{
    public abstract class IHaveArgs
    {
        /// <summary>Arbiturary arguments in form of a dict.</summary>
        public Dictionary<string, string>? Args { get; set; }
        /// <summary>Tries to parse <see cref="Args"/> to target model of type <see cref="IArgs"/></summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal T? ParseArgs<T>() where T : IArgs
        {
            if (Args == null)
                return default;
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(Args));
        }
    }
}