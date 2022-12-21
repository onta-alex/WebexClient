using Newtonsoft.Json;

namespace Prom.WebEx.Client.Models
{
    public class Message
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("roomId")]
        public string RoomId { get; set; }

        [JsonProperty("roomType")]
        public string RoomType { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("files")]
        public string[] Files { get; set; }

        [JsonProperty("personId")]
        public string PersonId { get; set; }

        [JsonProperty("personEmail")]
        public string PersonEmail { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }
    }
}
