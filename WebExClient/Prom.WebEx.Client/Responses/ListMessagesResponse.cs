using Newtonsoft.Json;

namespace Prom.WebEx.Client.Responses
{
    internal class ItemsResponse<T>
    {
        [JsonProperty("items")]
        public List<T> Items { get; set; }
    }
}
