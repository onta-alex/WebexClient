using Newtonsoft.Json;

namespace Prom.WebEx.Client.Models
{
    public class RoomDetails
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("isLocked")]
        public bool IsLocked { get; set; }

        [JsonProperty("lastActivity")]
        public DateTime LastActivity { get; set; }

        [JsonProperty("creatorId")]
        public string CreatorId { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("ownerId")]
        public string OwnerId { get; set; }

        [JsonProperty("isPublic")]
        public bool IsPublic { get; set; }

        [JsonProperty("isReadOnly")]
        public bool IsReadOnly { get; set; }
    }
}
